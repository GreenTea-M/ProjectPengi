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
    ///
    /// Reference(s):
    /// Brackeys. SAVE & LOAD SYSTEM in Unity. 2 Dec. 2018. youtu.be/XOjd_qU2Ido.
    ///     Accessed on 8 July 2020.
    /// </remarks>
    public class SaveIO
    {
        public GameConfiguration gameConfiguration;

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void SyncFiles();

        [DllImport("__Internal")]
        private static extern void Hello();
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

            private SaveIO _saveIo;

            internal SlotExecutor(SaveIO saveIo)
            {
                this._saveIo = saveIo;
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

        public SlotExecutor RequestExecutor()
        {
            return new SlotExecutor(this);
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

        private bool OverwriteSlot(SlotExecutor slotExecutor)
        {
            if (slotExecutor.saveData == null)
            {
                return false;
            }

            bool result = true;


            string path = GetPath(slotExecutor);
            Debug.Log(path);
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

        private bool DoesExist(SlotExecutor slotExecutor)
        {
            Debug.Log(GetPath(slotExecutor));
            return File.Exists(GetPath(slotExecutor));
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
    }
}