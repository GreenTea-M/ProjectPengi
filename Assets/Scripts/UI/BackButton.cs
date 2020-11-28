using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class BackButton : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenuScene";
        
        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(GoBack);
        }

        private void GoBack()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.Equals(MainMenuScene))
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(MainMenuScene);
            }
        }
    }
}