using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameSystem.Save
{
    /// <summary>
    /// This class gives access to read write operations.
    /// </summary>
    /// Reference(s):
    /// Brackeys. SAVE & LOAD SYSTEM in Unity. 2 Dec. 2018. youtu.be/XOjd_qU2Ido.
    ///     Accessed on 8 July 2020.
    public class SaveIO
    {
        private readonly GameConfiguration gameConfiguration;

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void SyncFiles();
#endif

        public SaveIO(GameConfiguration gameConfiguration)
        {
            this.gameConfiguration = gameConfiguration;
        }

        /// <summary>
        /// Saves a save data into a save slot.
        /// </summary>
        public class SlotExecutor
        {
            private int _slotIndex = 0;
            private SaveData _saveData = null;
            private readonly JsonExecutor _jsonExecutor;
            private SaveIO _saveIo;

            internal SlotExecutor(SaveIO saveIo)
            {
                _saveIo = saveIo;
                _jsonExecutor = saveIo.RequestJsonExecutor();
            }

            public SlotExecutor AtSlotIndex(int slotIndex)
            {
                Assert.IsTrue(slotIndex < _saveIo.gameConfiguration.maxSaveSlots);
                _slotIndex = slotIndex;
                return this;
            }

            public SlotExecutor UsingSaveData(SaveData saveData)
            {
                _saveData = saveData;
                return this;
            }

            public SaveData GetSaveData()
            {
                return _saveData;
            }

            private string GetFilename()
            {
                return $"savedata{_slotIndex}";
            }

            public bool OverwriteSlot()
            {
                return _jsonExecutor.UsingFilename(GetFilename())
                    .UsingJsonData(JsonUtility.ToJson(_saveData))
                    .OverwriteJsonFile();
            }

            /// <summary>
            /// Loads the slot onto the saveData argument.
            /// </summary>
            /// <returns>Returns whether the loading was successful</returns>
            public SaveData LoadSlot()
            {
                var jsonString = _jsonExecutor.UsingFilename(GetFilename()).LoadJsonString();
                return jsonString != null ? JsonUtility.FromJson<SaveData>(jsonString) : null;
            }

            public bool DoesExist()
            {
                return _jsonExecutor.UsingFilename(GetFilename()).DoesExist();
            }
        }

        /// <summary>
        /// Saves data into a persistent json file
        /// </summary>
        public class JsonExecutor
        {
            private string _filename = null;
            private string _jsonString = "";
            private readonly SaveIO _saveIo;

            public JsonExecutor(SaveIO saveIo)
            {
                _saveIo = saveIo;
            }

            public JsonExecutor UsingFilename(string filename)
            {
                this._filename = filename;
                return this;
            }

            public JsonExecutor UsingJsonData(string jsonString)
            {
                this._jsonString = jsonString;
                return this;
            }

            public string GetJsonString()
            {
                Debug.Assert(_jsonString != null);
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

        /// <summary>
        /// Request for a save data writer
        /// </summary>
        /// <returns></returns>
        public SlotExecutor RequestSlotExecutor()
        {
            return new SlotExecutor(this);
        }

        /// <summary>
        /// Request for a json file writer
        /// </summary>
        /// <returns></returns>
        public JsonExecutor RequestJsonExecutor()
        {
            return new JsonExecutor(this);
        }

        private string GetPath(JsonExecutor jsonExecutor)
        {
            return $"{Application.persistentDataPath}/{jsonExecutor.GetFilename()}.json";
        }

        private bool OverwriteJson(JsonExecutor jsonExecutor)
        {
            if (jsonExecutor.GetFilename() == null)
            {
                return false;
            }

            string path = GetPath(jsonExecutor);
            File.WriteAllText(path, jsonExecutor.GetJsonString());

#if UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SyncFiles();
            }
#endif

            return true;
        }

        private bool DoesExist(JsonExecutor jsonExecutor)
        {
            return File.Exists(GetPath(jsonExecutor));
        }

        private string LoadJsonString(JsonExecutor jsonExecutor)
        {
            var path = GetPath(jsonExecutor);

            if (File.Exists(path))
            {
                jsonExecutor.UsingJsonData(File.ReadAllText(path));
                return jsonExecutor.GetJsonString();
            }

            Debug.LogError("Save file not found in path: " + path);
            return null;
        }
    }
}