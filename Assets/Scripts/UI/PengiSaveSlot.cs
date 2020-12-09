using GameSystem;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PengiSaveSlot : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        public Image image;
        public TextMeshProUGUI textMesh;

        private SaveSelectManager _saveSelectManager;
        private int _index;
        private PauseManager _pauseManager;
        private SaveData _saveData;

        private void LoadSaveData(SaveData saveData, int index)
        {
            _index = index;
            _saveData = saveData;
            
            if (saveData != null)
            {
                // load appropriate sprite
                gameConfiguration.LoadDefaultSprite(image, saveData.currentSpeaker);

                // load description
                var autoSaveText = index == GameConfiguration.AutoSaveIndex ? "[Auto save] " : $"[Save {index}] ";
                textMesh.text = $"{autoSaveText}{saveData.lastDialog}";
                image.enabled = true;
            }
            else
            {
                image.enabled = false;
                textMesh.text = "Empty save data";
            }
        }

        public void LoadSaveData(SaveSelectManager saveSelectManager, SaveData saveData, int index)
        {
            _saveSelectManager = saveSelectManager;
            LoadSaveData(saveData, index);
        }

        public void LoadSaveData(PauseManager pauseManager, SaveData saveData, int index)
        {
            _pauseManager = pauseManager;
            LoadSaveData(saveData, index);
        }

        public void OnClick()
        {
            if (_saveSelectManager != null)
            {
                _saveSelectManager.LoadSaveData(_index);
            }
            else if (_pauseManager != null)
            {
                _pauseManager.TrySaveData(_saveData, _index);
            }
        }
    }
}