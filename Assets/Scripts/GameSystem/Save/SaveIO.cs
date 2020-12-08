using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameSystem.Save
{
    /// <summary>
    /// This class gives access to read write operations.
    /// </summary>
    /// <remarks>
    /// This class was separated from SaveLocator in the case where
    /// we may have a different save source (debugging) or different operating
    /// system.
    ///</remarks>
    /// Reference(s):
    /// Brackeys. SAVE & LOAD SYSTEM in Unity. 2 Dec. 2018. youtu.be/XOjd_qU2Ido.
    ///     Accessed on 8 July 2020.
    /// todo: document
    public class SaveIO
    {
        private GameConfiguration gameConfiguration;

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void SyncFiles();
#endif

        public SaveIO(GameConfiguration gameConfiguration)
        {
            this.gameConfiguration = gameConfiguration;
        }

        /// <summary>
        /// This class works like a Builder class but it nominalizes the actions and let's the users
        /// configure how the actions are made.
        /// </summary>
        /// <remarks>
        /// I'm anticipating for UserIndex in the future or additional arguments required
        /// to make a save data be saved. The reason why this is an inner class is due
        /// to the lack of friend class features in C#. I also need to configure SaveIO so that's
        /// another separate thing from Executor.
        /// </remarks>
        public class SlotExecutor
        {
            internal int slotIndex = 0;
            internal SaveData saveData = null;

            private readonly SaveIO _saveIo;

            internal SlotExecutor(SaveIO saveIo)
            {
                _saveIo = saveIo;
            }

            public SlotExecutor AtSlotIndex(int slotIndex)
            {
                Assert.IsTrue(slotIndex < _saveIo.gameConfiguration.maxSaveSlots);
                this.slotIndex = slotIndex;
                return this;
            }

            public SlotExecutor UsingSaveData(SaveData saveData)
            {
                this.saveData = saveData;
                return this;
            }

            public SaveData GetSaveData()
            {
                return saveData;
            }

            public bool OverwriteSlot()
            {
                return this._saveIo.OverwriteSlot(this);
            }

            /// <summary>
            /// Loads the slot onto the saveData argument.
            /// </summary>
            /// <returns>Returns whether the loading was successful</returns>
            public SaveData LoadSlot()
            {
                return this._saveIo.LoadSlot(this);
            }

            public bool DoesExist()
            {
                return this._saveIo.DoesExist(this);
            }
        }

        /// <summary>
        /// Initially assumes that this is a json file
        ///  todo: improve documentation
        /// </summary>
        public class JSONExecutor
        {
            private string _filename = null;
            private string _jsonString = "";

            private readonly SaveIO _saveIo;

            internal JSONExecutor(SaveIO saveIo)
            {
                _saveIo = saveIo;
            }

            public JSONExecutor UsingFilename(string filename)
            {
                this._filename = filename;
                return this;
            }

            public JSONExecutor UsingJsonData(string jsonString)
            {
                this._jsonString = jsonString;
                return this;
            }

            public string GetJsonString()
            {
                return _jsonString;
            }

            public string GetFilename()
            {
                return _filename;
            }

            public bool OverwriteJsonFile()
            {
                return _saveIo.OverwriteJson(this);
            }

            public string LoadJsonString()
            {
                return _saveIo.LoadJsonString(this);
            }

            public bool DoesExist()
            {
                return _saveIo.DoesExist(this);
            }
        }

        public SlotExecutor RequestSlotExecutor()
        {
            return new SlotExecutor(this);
        }

        public JSONExecutor RequestJsonExecutor()
        {
            return new JSONExecutor(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotExecutor"></param>
        /// <returns></returns>
        /// <remarks>
        /// todo(Turnip): extract constants
        /// </remarks>
        private string GetPath(SlotExecutor slotExecutor)
        {
            return Application.persistentDataPath + "/savedata"
                                                  + slotExecutor.slotIndex + ".json";
        }
        
        private string GetPath(JSONExecutor jsonExecutor)
        {
            return Application.persistentDataPath + "/" + jsonExecutor.GetFilename() + ".json";
        }

        private bool OverwriteSlot(SlotExecutor slotExecutor)
        {
            if (slotExecutor.saveData == null)
            {
                return false;
            }

            bool result = true;


            string path = GetPath(slotExecutor);
            // Debug.Log(path);
            string jsonString = JsonUtility.ToJson(slotExecutor.saveData);
            File.WriteAllText(path, jsonString);

#if UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SyncFiles();
            }
#elif UNITY_STANDALONE_WIN
            /*BinaryFormatter formatter = new BinaryFormatter();
            string path = GetPath(slotExecutor);
            FileStream stream = new FileStream(path, FileMode.Create);

            try
            {
                formatter.Serialize(stream, slotExecutor.saveData);
            }
            catch (Exception)
            {
                // todo: specify the three exceptions here
                result = false;
            }
            finally
            {
                stream.Close();
            }*/
#else
            Debug.LogError("Saving not supported on current platform");
#endif

            return result;
        }
        
        private bool OverwriteJson(JSONExecutor jsonExecutor)
        {
            if (jsonExecutor.GetFilename() == null)
            {
                return false;
            }

            bool result = true;

            string path = GetPath(jsonExecutor);
            File.WriteAllText(path, jsonExecutor.GetJsonString());

#if UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SyncFiles();
            }
#elif UNITY_STANDALONE_WIN
            /*BinaryFormatter formatter = new BinaryFormatter();
            string path = GetPath(slotExecutor);
            FileStream stream = new FileStream(path, FileMode.Create);

            try
            {
                formatter.Serialize(stream, slotExecutor.saveData);
            }
            catch (Exception)
            {
                // todo: specify the three exceptions here
                result = false;
            }
            finally
            {
                stream.Close();
            }*/
#else
            Debug.LogError("Saving not supported on current platform");
#endif

            return result;
        }

        private bool DoesExist(SlotExecutor slotExecutor)
        {
            return File.Exists(GetPath(slotExecutor));
        }
        

        private bool DoesExist(JSONExecutor jsonExecutor)
        {
            return File.Exists(GetPath(jsonExecutor));
        }

        private SaveData LoadSlot(SlotExecutor slotExecutor)
        {
            SaveData result = null;
            string path = GetPath(slotExecutor);

            if (File.Exists(path))
            {
                var jsonString = File.ReadAllText(path);
                result = JsonUtility.FromJson<SaveData>(jsonString);

#if UNITY_WEBGL
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    SyncFiles();
                }
#elif UNITY_STANDALONE_WIN
                /*BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                try
                {
                    result = (SaveData) formatter.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    // todo: specify the three exceptions here
                    Debug.LogError(ex.Message);
                }
                finally
                {
                    stream.Close();
                }*/
#else
            Debug.LogError("Saving not supported on current platform");
#endif
            }
            else
            {
                Debug.LogError("Save file not found in path: " + path);
            }

            return result;
        }

        private string LoadJsonString(JSONExecutor jsonExecutor)
        {
            var path = GetPath(jsonExecutor);

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            Debug.LogError("Save file not found in path: " + path);
            return null;
        }
    }
}