using System;
using Tomato.Core.GameSystem.Save;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameSystem.Save
{
    /// <summary>
    /// Script attached to object that loads a save file.
    /// </summary>
    /// <remarks>
    /// <see cref="SetGameInstance"/> should be the first thing to call. To make full use of this script, call
    /// <see cref="SetSlotIndex"/>. This script also handles scene loading if given a SceneAsset
    /// through <see cref="SetDestination"/>.
    /// </remarks>
    public class UiSaveSlot : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        
        private int _index = 0;
        private Button _button;
        private SaveData _saveData;

        private void Awake()
        {
            _button = GetComponent<Button>();
            Debug.Assert(_button != null);
            _button.onClick.AddListener(OnClick);
        }

        public void SetSlotIndex(int index)
        {
            Assert.IsTrue(gameConfiguration != null);
            Assert.IsTrue(index < gameConfiguration.maxSaveSlots);
            _index = index;

            /*
         * Note to self:
         * I had issues about understanding when Start is called. I assumed that Start is always
         * called first such that when the object is instantiated, it calls Start. Apparently,
         * I'm calling this method before Start here is called. This causes a button null
         * exception error
         */
            _button = GetComponent<Button>();
            var saveIo = gameConfiguration.SaveIo;
            Assert.IsTrue(saveIo != null);
            _button.interactable = saveIo.RequestExecutor()
                .AtSlotIndex(_index)
                .DoesExist();
            _saveData = saveIo.RequestExecutor()
                .AtSlotIndex(_index)
                .LoadSlot();
        }

        public void SetSaveData(SaveData saveData)
        {
            _button = GetComponent<Button>();
            _saveData = saveData;
        }

        private void OnClick()
        {
            throw new NotImplementedException();
            
            // todo: load save slot and go to destination
            if (_saveData != null)
            {
                // gameConfiguration.saveData = _saveData;
                // gameConfiguration.autoSave = _saveData;

                SceneManager.LoadScene("DialogScene");
            }
            else
            {
                Debug.LogWarning("Save data not found. Cannot proceed!");
            }
        }
    }
}