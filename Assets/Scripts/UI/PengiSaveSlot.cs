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

        public void LoadSaveData(SaveSelectManager saveSelectManager, SaveData saveData, int index)
        {
            _saveSelectManager = saveSelectManager;
            _index = index;
            
            // load appropriate sprite
            gameConfiguration.LoadDefaultSprite(image, saveData.currentSpeaker);
            
            // load description
            var autoSaveText = index == GameConfiguration.AutoSaveIndex ? 
                "[Auto save] " : $"[Save {index}]";
            textMesh.text = $"{autoSaveText}{saveData.lastDialog}";
        }

        public void OnClick()
        {
            if (_saveSelectManager != null)
            {
                _saveSelectManager.LoadSaveData(_index);
            }
        }
    }
}