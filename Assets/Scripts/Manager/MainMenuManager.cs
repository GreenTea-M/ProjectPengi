using System;
using GameSystem;
using GameSystem.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    public class MainMenuManager : MonoBehaviour, SaveClientCallback
    {
        public GameConfiguration gameConfiguration;
        public Button continueButton;
        public SaveClient saveClient;

        private void Awake()
        {
            continueButton.enabled = gameConfiguration.isSaveDirty;
            continueButton.interactable = gameConfiguration.isSaveDirty;
        }

        private void OnDestroy()
        {
            gameConfiguration.ReleaseSaveAccess(saveClient);
            saveClient = null;
        }

        private void Start()
        {
            saveClient = gameConfiguration.RequestSaveAccess(this);
            
            // disable continue when auto save does not exist
            continueButton.interactable = gameConfiguration.SaveIo.RequestExecutor()
                .AtSlotIndex(GameConfiguration.AutoSaveIndex)
                .DoesExist();
        }

        public void OnClickNewGame()
        {
            gameConfiguration.ResetSaveData();
            SceneManager.LoadScene("DialogScene");
        }

        public void OnClickContinue()
        {
            // todo: allow multiple save files in the future
            gameConfiguration.LoadData(GameConfiguration.AutoSaveIndex);
            SceneManager.LoadScene("DialogScene");
            // SceneManager.LoadScene("SaveSelect");
        }

        public void OnClickOptions()
        {
            SceneManager.LoadScene("OptionScene");
        }

        public void WriteAutoSave()
        {
            // do nothing
        }
    }
}