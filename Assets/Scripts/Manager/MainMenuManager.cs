using System;
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

        private void OnEnable()
        {
            saveClient = gameConfiguration.RequestSaveAccess(this);
        }

        private void OnDisable()
        {
            gameConfiguration.ReleaseSaveAccess(saveClient);
            saveClient = null;
        }

        public void OnClickNewGame()
        {
            gameConfiguration.ResetSaveData();
            SceneManager.LoadScene("DialogScene");
        }

        public void OnClickContinue()
        {
            // todo: allow multiple save files in the future
            throw new NotImplementedException();
            saveClient.currentSave.Overwrite(saveClient.autoSave);
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