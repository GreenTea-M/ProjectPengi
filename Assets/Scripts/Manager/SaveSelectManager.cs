using System;
using GameSystem;
using GameSystem.Save;
using UnityEngine;

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
            throw new NotImplementedException();
            // do auto save first
            var autoSaveButton = Instantiate(saveSlotPrefab).GetComponent<UiSaveSlot>();
            // autoSaveButton.SetSaveData(gameConfiguration.autoSave);
            var autoRt = autoSaveButton.GetComponent<RectTransform>();
            autoRt.SetParent(panelParent);
            autoRt.localScale = Vector3.one;

            // todo: add the rest of the save data???
        }
    }
}