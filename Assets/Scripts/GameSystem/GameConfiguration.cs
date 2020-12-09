using System;
using GameSystem.Save;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameSystem
{
    /// <summary>
    /// This class holds variables that may affect overall gameplay and debugging features.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfiguration",
        menuName = "ScriptableObjects/Data/GameConfiguration")]
    public class GameConfiguration : ScriptableObject
    {
        [Header("Constants")] [Tooltip("Number of max save slots")]
        public int maxSaveSlots = 3;

        [Tooltip("This will be the game configuration used on new game")]
        public GameConfiguration baseConfiguration;

        public FontItem[] fontList;

        public GameObject mainCharacterPrefab;
        public GameObject narratingCharacterPrefab;
        public GameObject[] characterPrefabList;

        #region Option variables

        [Header("Option variables")]
        [Tooltip("The delay in seconds that each character shows up; If less than 0, show instantly")]
        public float textRate = 0.025f;

        public float TextRate
        {
            get => _playerPref.textRate;
            set => _playerPref.textRate = value;
        }

        public bool ShowVisualEffects
        {
            get => _playerPref.showVisualEffects;
            set => _playerPref.showVisualEffects = value;
        }

        public int FontIndex
        {
            get => _playerPref.fontIndex;
            set => _playerPref.fontIndex = value;
        }

        public TMP_FontAsset FontAsset
        {
            get
            {
                if (_playerPref.fontIndex < 0 || _playerPref.fontIndex > fontList.Length)
                {
                    _playerPref.fontIndex = 0;
                }

                return fontList[_playerPref.fontIndex].fontAsset;
            }
            set
            {
                var index = 0;

                for (int i = 0; i < fontList.Length; i++)
                {
                    if (fontList[i].fontAsset == value)
                    {
                        index = i;
                        break;
                    }
                }

                _playerPref.fontIndex = index;
            }
        }

        public float fontSize = 18f;

        public float FontSize
        {
            get => _playerPref.fontSize;
            set => _playerPref.fontSize = value;
        }

        [Range(0.6f, 1f)] public float textOpacity = 0.97f;

        public float TextOpacity
        {
            get => textOpacity;
            set => textOpacity = value;
        }

        public bool EnableTextFormatting
        {
            get => _playerPref.enableTextFormatting;
            set => _playerPref.enableTextFormatting = value;
        }

        public float Volume
        {
            get => _playerPref.volume;
            set => _playerPref.volume = value;
        }

        public Color fontColor = Color.black;

        #endregion Option variables

        [FormerlySerializedAs("saveData")] [Header("Save data")] [SerializeField]
        private SaveData currentSave;

        [Header("Auto save (Do not touch)")] [SerializeField]
        private SaveData autoSave = new SaveData();

        [Header("Other global stuff")] private SaveIO saveIo;
        public GameInstance gameInstance;

        public static int AutoSaveIndex = 0;
        private PlayerPreferences _playerPref = new PlayerPreferences();
        private const float _shakeStrength = 1f;
        private const string PlayerPreferenceFilename = "PengiPlayePref";

        private void Awake()
        {
            saveIo = new SaveIO(this);
            SyncWithPlayerPref();
        }

        public float ShakeStrength => _playerPref.showVisualEffects ? _shakeStrength : 0f;

        public SaveIO SaveIo => saveIo ?? (saveIo = new SaveIO(this));

        /// <summary>
        /// When the game starts, load the player preferences.
        /// </summary>
        /// Q: Why aren't you using player preferences?
        /// I have been trying to use player preferences, but I don't understand
        /// why it's not working. I might as well use the save system I had already made.
        public void SyncWithPlayerPref()
        {
            var doesPlayerPrefExist = SaveIo.RequestJsonExecutor()
                .UsingFilename(PlayerPreferenceFilename)
                .DoesExist();
            bool isSuccessful = false;
            if (doesPlayerPrefExist)
            {
                var jsonString = SaveIo.RequestJsonExecutor()
                    .UsingFilename(PlayerPreferenceFilename)
                    .LoadJsonString();
                if (jsonString != null)
                {
                    _playerPref = JsonUtility.FromJson<PlayerPreferences>(jsonString);
                    isSuccessful = true;
                }
            }

            if (!isSuccessful)
            {
                ResetOptions();
            }
        }

        public void SavePlayerPreference()
        {
            SaveIo.RequestJsonExecutor()
                .UsingFilename(PlayerPreferenceFilename)
                .UsingJsonData(JsonUtility.ToJson(_playerPref))
                .OverwriteJsonFile();
        }

        public void ResetSaveData()
        {
            currentSave.Overwrite(baseConfiguration.currentSave);
            autoSave.Overwrite(baseConfiguration.currentSave);
        }

        public void ResetOptions()
        {
            _playerPref = new PlayerPreferences();
            SavePlayerPreference();
        }

        public SaveClient RequestSaveAccess(SaveClientCallback saveClientCallback)
        {
            var saveClient = gameInstance.RequestSaveAccess();
            saveClient.currentSave = currentSave;
            saveClient.autoSave = autoSave;
            saveClient.saveClientCallback = saveClientCallback;
            return saveClient;
        }

        public void ReleaseSaveAccess(SaveClient saveClient)
        {
            gameInstance.RemoveSaveClient(saveClient);
        }

        public SaveData GetAutoSave()
        {
            return autoSave;
        }

        public void LoadData(int slotIndex)
        {
            var tmpSave = SaveIo.RequestSlotExecutor()
                .AtSlotIndex(slotIndex)
                .LoadSlot();
            if (tmpSave != null)
            {
                currentSave = tmpSave;
            }
            else
            {
                Debug.LogError($"Failed to load slot index: {slotIndex}");
            }
        }

        /// <summary>
        /// Load default sprite for save slot thumbnails
        /// </summary>
        /// <param name="image"></param>
        /// <param name="currentSpeaker"></param>
        public void LoadDefaultSprite(Image image, string currentSpeaker)
        {
            foreach (var character in characterPrefabList)
            {
                var characterScript = character.GetComponent<UnifiedCharacterScript>();
                if (characterScript.IsSimilar(currentSpeaker))
                {
                    image.sprite = characterScript.defaultSprite.defaultState.sprite;
                    break;
                }
            }

            // if no one else is speaking, go with pengi
            image.sprite = mainCharacterPrefab.GetComponent<UnifiedCharacterScript>()
                .defaultSprite.defaultState.sprite;
        }
    }


    [Serializable]
    public class FontItem
    {
        public TMP_FontAsset fontAsset;
        public string fontName;
    }
}