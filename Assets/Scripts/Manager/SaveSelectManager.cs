using System;
using GameSystem;
using GameSystem.Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class SaveSelectManager : MonoBehaviour
    {
        public RectTransform panelParent;
        public GameObject saveSlotPrefab;
        public GameConfiguration gameConfiguration;

        private void Awake()
        {
            Debug.Assert(panelParent != null);
            Debug.Assert(saveSlotPrefab != null);
            Debug.Assert(gameConfiguration != null);
        }

        private void Start()
        {
            // assumption: you cannot go here with auto save checked in main menu
            for (int i = 0; i < gameConfiguration.maxSaveSlots; i++)
            {
                CreateSaveSlot(i);
            }
        }

        private void CreateSaveSlot(int index)
        {
            SaveData saveData = null;
            
            if (gameConfiguration.SaveIo.RequestSlotExecutor()
                .AtSlotIndex(index)
                .DoesExist())
            {
                saveData = gameConfiguration.SaveIo.RequestSlotExecutor()
                    .AtSlotIndex(index)
                    .LoadSlot();
            }

            var saveSlotScript = Instantiate(saveSlotPrefab, panelParent)
                .GetComponent<PengiSaveSlot>();
            Debug.Assert(saveSlotScript != null);
            saveSlotScript.LoadSaveData(this, saveData, index);
        }

        public void LoadSaveData(int index)
        {
            gameConfiguration.LoadData(index);
            SceneManager.LoadScene("DialogScene");
        }

        public void GoBack()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}