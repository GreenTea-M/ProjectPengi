using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    public class MainMenuManager : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        public Button continueButton;

        private void Awake()
        {
            continueButton.enabled = gameConfiguration.isSaveDirty;
            continueButton.interactable = gameConfiguration.isSaveDirty;
        }

        public void OnClickNewGame()
        {
            gameConfiguration.ResetSaveData();
            SceneManager.LoadScene("DialogScene");
        }

        public void OnClickContinue()
        {
            SceneManager.LoadScene("SaveSelect");
        }

        public void OnClickOptions()
        {
            // todo: options
        }
    }
}