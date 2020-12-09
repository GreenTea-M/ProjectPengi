using System.Collections.Generic;
using GameSystem.Save;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// <c>GameInstance</c> is a Unity object that's always present in the game.
    /// It is currently used to save game data.
    /// </summary>
    public class GameInstance : MonoBehaviour
    {
        private static GameInstance _instance = null;

        [Header("Variables")] [Tooltip("Used to access configurations that may be relevant for gameplay and debugging")]
        public GameConfiguration gameConfiguration;

        private List<SaveClient> saveClientList = new List<SaveClient>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            gameConfiguration.gameInstance = this;
            gameConfiguration.SyncWithPlayerPref();
        }

        public SaveClient RequestSaveAccess()
        {
            var client = new SaveClient();
            saveClientList.Add(client);
            return client;
        }

        public void RemoveSaveClient(SaveClient saveClient)
        {
            saveClientList.Remove(saveClient);
        }

        public void WriteOnAutoSave()
        {
            // make all writers write
            foreach (var saveClient in saveClientList)
            {
                saveClient.TryAutoSaveWrite();
            }

            gameConfiguration.SaveIo.RequestSlotExecutor()
                .AtSlotIndex(0)
                .UsingSaveData(gameConfiguration.GetAutoSave())
                .OverwriteSlot();
        }
    }
}