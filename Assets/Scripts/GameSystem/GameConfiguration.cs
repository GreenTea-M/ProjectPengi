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
        public GameObject[] characterPrefabList;

        #region Option variables
        [Header("Option variables")] 
        [Tooltip("The delay in seconds that each character shows up; If less than 0, show instantly")]
        public float textRate = 0.025f;
        public float TextRate
        {
            get => textRate;
            set
            {
                textRate = value;
                PlayerPrefs.SetFloat(KeyTextRate, textRate);
            }
        }

        public bool shouldShake = true;
        public bool ShouldShake
        {
            get => shouldShake;
            set
            {
                shouldShake = value;
                PlayerPrefs.SetInt(KeyTextRate, shouldShake ? 1 : 0);
            }
        }

        public int fontIndex = 0;
        public int FontIndex
        {
            get => fontIndex;
            set
            {
                fontIndex = value;
                PlayerPrefs.SetInt(KeyFontIndex, value);
            }
        }
        public TMP_FontAsset FontAsset
        {
            get
            {
                if (fontIndex < 0 || fontIndex > fontList.Length)
                {
                    fontIndex = 0;
                }

                return fontList[fontIndex].fontAsset;
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

                fontIndex = index;
                PlayerPrefs.SetInt(KeyFontIndex, index);
            }
        }

        public float fontSize = 18f;
        public float FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                PlayerPrefs.SetFloat(KeyFontSize, fontSize);
            }
        }
        
        [Range(0.6f,1f)]
        public float textOpacity = 0.97f;
        public float TextOpacity
        {
            get => textOpacity;
            set
            {
                textOpacity = value;
                PlayerPrefs.SetFloat(KeyTextOpacity, textOpacity);
            }
        }
        
        public bool enableTextFormatting = true;
        public bool EnableTextFormatting
        {
            get => enableTextFormatting;
            set
            {
                enableTextFormatting = value;
                PlayerPrefs.SetInt(KeyTextFormatting, enableTextFormatting ? 1 : 0);
            }
        }

        public float volume = 0.75f;
        public float Volume { get => volume;
            set
            {
                volume = value;
                PlayerPrefs.SetFloat(KeyVolume, value);
            } 
        }
        
        public Color fontColor = Color.black;
    
        private readonly string KeyTextRate = "TextRate";
        private readonly string KeyShouldShake = "ShakeStrength";
        private readonly string KeyFontIndex = "FontIndex";
        private readonly string KeyFontSize = "FontSize";
        private readonly string KeyTextOpacity = "TextOpacity";
        private readonly string KeyTextFormatting = "TextFormatting";
        private readonly string KeyVolume = "Volume";
        #endregion Option variables
    
        [FormerlySerializedAs("saveData")]
        [Header("Save data")]
        [SerializeField]
        private SaveData currentSave;

        [Header("Auto save (Do not touch)")] 
        public bool isSaveDirty = false;
        [SerializeField]
        private SaveData autoSave;

        [Header("Other global stuff")] 
        private SaveIO saveIo;
        public GameInstance gameInstance;

        public static int AutoSaveIndex = 0;
        private const float _shakeStrength = 1f;

        private void Awake()
        {
            saveIo = new SaveIO(this);
            SyncWithPlayerPref();
        }

        public float ShakeStrength => shouldShake ? _shakeStrength : 0f;

        public SaveIO SaveIo => saveIo ?? (saveIo = new SaveIO(this));

        private void SyncWithPlayerPref()
        {
            if (PlayerPrefs.HasKey(KeyTextRate))
            {
                // assume all has
                textRate = PlayerPrefs.GetFloat(KeyTextRate);
                shouldShake = PlayerPrefs.GetInt(KeyShouldShake) == 1;
            
                // font
                fontIndex = PlayerPrefs.GetInt(KeyFontIndex);
                if (fontIndex < 0 || fontIndex > fontList.Length)
                {
                    fontIndex = 0;
                }

                fontSize = PlayerPrefs.GetFloat(KeyFontSize);
                textOpacity = PlayerPrefs.GetFloat(KeyTextOpacity);
                enableTextFormatting = PlayerPrefs.GetInt(KeyTextFormatting) == 1;
                volume = PlayerPrefs.GetFloat(KeyVolume);
            }
            else
            {
                TextRate = textRate;
                ShouldShake = shouldShake;
                fontIndex = FontIndex;
                FontSize = fontSize;
                TextOpacity = textOpacity;
                EnableTextFormatting = enableTextFormatting;
                Volume = volume;
                PlayerPrefs.Save();
            }
        }

        public void ResetSaveData()
        {
            currentSave.Overwrite(baseConfiguration.currentSave);
            autoSave.Overwrite(baseConfiguration.currentSave);
        }

        public void ResetOptions()
        {
            textRate = baseConfiguration.textRate;
            shouldShake = baseConfiguration.shouldShake;
            fontIndex = baseConfiguration.fontIndex;
            fontSize = baseConfiguration.fontSize;
            textOpacity = baseConfiguration.textOpacity;
            fontColor = baseConfiguration.fontColor;
            enableTextFormatting = baseConfiguration.enableTextFormatting;
            volume = baseConfiguration.volume;
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
            var tmpSave = SaveIo.RequestExecutor()
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
                Debug.Log($"{characterScript.RealName} vs {currentSpeaker}");
                if (characterScript.IsSimilar(currentSpeaker))
                {
                    image.sprite = characterScript.defaultSprite.defaultState.sprite;
                    break;
                }
            }
            
            Debug.Log("Not found...");
        }
    }
    

    [Serializable]
    public class FontItem
    {
        public TMP_FontAsset fontAsset;
        public string fontName;
    }
}