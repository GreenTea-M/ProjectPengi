using System;
using System.Collections;
using Gameplay;
using GameSystem;
using TMPro;
using UI;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Manager
{
    public class PauseManager : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        public InputManager inputManager;

        public GameObject blockingBackground;
        public GameObject pauseUI;

        public GameObject saveSlotPrefab;
        public Transform panelParent;

        public GameObject popupObject;
        public TextMeshProUGUI popupMessage;

        private bool _isPaused = false;
        private SaveData _currentSaveData;
        private int _currentIndex;
        private PengiSaveSlot[] _saveSlotScriptList;

        private void Awake()
        {
            Debug.Assert(gameConfiguration != null);
            Debug.Assert(inputManager != null);
            Debug.Assert(blockingBackground != null);
            Debug.Assert(pauseUI != null);
            Debug.Assert(saveSlotPrefab != null);
            Debug.Assert(panelParent != null);
            Debug.Assert(popupObject != null);
            Debug.Assert(popupMessage != null);
        }

        private void Start()
        {
            blockingBackground.SetActive(false);
            pauseUI.SetActive(false);
            popupObject.SetActive(false);
            StartCoroutine(LoadSaveData());
        }

        public void TogglePause()
        {
            if (_isPaused)
            {
                // Unpause
                inputManager.SetInputState(InputState.Normal);
                popupObject.SetActive(false);
            }
            else
            {
                // Pause
                inputManager.SetInputState(InputState.Pause);
                panelParent.gameObject.SetActive(true);
            }

            _isPaused = !_isPaused;

            blockingBackground.SetActive(_isPaused);
            pauseUI.SetActive(_isPaused);
        }

        public IEnumerator LoadSaveData()
        {
            _saveSlotScriptList = new PengiSaveSlot[gameConfiguration.maxSaveSlots - 1];

            for (int i = 1; i < gameConfiguration.maxSaveSlots; i++)
            {
                _saveSlotScriptList[i - 1] = CreateSaveSlot(i);
                yield return new WaitForSeconds(1f / 60f);
            }
        }

        private PengiSaveSlot CreateSaveSlot(int index)
        {
            SaveData saveData = null;

            if (gameConfiguration.SaveIo.RequestExecutor()
                .AtSlotIndex(index)
                .DoesExist())
            {
                saveData = gameConfiguration.SaveIo.RequestExecutor()
                    .AtSlotIndex(index)
                    .LoadSlot();
            }

            var saveSlotScript = Instantiate(saveSlotPrefab, panelParent)
                .GetComponent<PengiSaveSlot>();
            Debug.Assert(saveSlotScript != null);
            saveSlotScript.LoadSaveData(this, saveData, index);

            return saveSlotScript;
        }

        public void TrySaveData(SaveData saveData, int index)
        {
            _currentSaveData = saveData;
            _currentIndex = index;
            popupMessage.text = $"The last save point will be saved on slot {index}.\nDo you want to proceed?";
            popupObject.SetActive(true);
            panelParent.gameObject.SetActive(false);
        }

        public void SaveApproval(bool isApproved)
        {
            if (isApproved)
            {
                var newData = gameConfiguration.GetAutoSave();
                gameConfiguration.SaveIo.RequestExecutor()
                    .AtSlotIndex(_currentIndex)
                    .UsingSaveData(newData)
                    .OverwriteSlot();
                _saveSlotScriptList[_currentIndex - 1].LoadSaveData(
                    this, newData, _currentIndex);
            }

            popupObject.SetActive(false);
            panelParent.gameObject.SetActive(true);
        }
    }
}