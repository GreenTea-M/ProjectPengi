using System;
using System.Collections.Generic;
using System.Text;
using Dialog;
using GameSystem;
using RoboRyanTron.Unite2017.Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Manager
{
    public class OptionsSceneManager : MonoBehaviour
    {
        public float breakTime = 1f;

        public GameConfiguration gameConfiguration;
        public GameEvent onFontChangedEvent;
        public TextMeshProUGUI textRateSample;

        public Slider sliderTextRate;
        public Slider sliderTextSize;
        public Slider sliderTextOpacity;
        public Slider sliderVolume;
        public TMP_Dropdown dropdownFont;
        public TMP_Dropdown dropdownShake;
        public TMP_Dropdown dropdownTextFormatting;

        private string fullText;
        private StringBuilder currentText = new StringBuilder();
        private float startTime = 0f;
        private int index = 0;
        private int fullTextSize = 0;
        private float breakTimeEnd = -1f;

        private void Start()
        {
            Debug.Assert(gameConfiguration != null);
            Debug.Assert(onFontChangedEvent != null);
            Debug.Assert(sliderTextRate != null);
            Debug.Assert(sliderTextRate != null);
            Debug.Assert(sliderTextOpacity != null);
            Debug.Assert(sliderVolume != null);
            Debug.Assert(dropdownFont != null);
            Debug.Assert(dropdownShake != null);
            Debug.Assert(dropdownTextFormatting != null);
            Debug.Assert(textRateSample != null);
            
            // set values for dropdown
            dropdownFont.options.Clear();
            foreach (var fontItem in gameConfiguration.fontList)
            {
                dropdownFont.options.Add(new TMP_Dropdown.OptionData(fontItem.fontName));
            }

            UpdateValues();

            fullText = textRateSample.text;
            fullTextSize = fullText.Length;

            // listeners
            sliderTextRate.onValueChanged.AddListener(OnTextRateChanged);
            sliderTextSize.onValueChanged.AddListener(OnTextSizeChanged);
            sliderTextOpacity.onValueChanged.AddListener(OnTextOpacityChanged);
            dropdownFont.onValueChanged.AddListener(OnFontChanged);
            dropdownShake.onValueChanged.AddListener(OnShakeChanged);
            dropdownTextFormatting.onValueChanged.AddListener(OnTextFormattingChanged);
            sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
        }

        private void UpdateValues()
        {
            // default values
            sliderTextRate.value = gameConfiguration.TextRate;
            sliderTextSize.value = gameConfiguration.FontSize;
            sliderTextOpacity.value = gameConfiguration.TextOpacity;
            dropdownFont.value = gameConfiguration.FontIndex;
            dropdownShake.value = gameConfiguration.ShouldShake ? 0 : 1;
            dropdownTextFormatting.value = gameConfiguration.EnableTextFormatting ? 0 : 1;
            sliderVolume.value = gameConfiguration.volume;
            PlayerPrefs.Save();
        }

        private void TextReset()
        {
            currentText.Clear();
            textRateSample.text = "";
            index = 0;
            breakTimeEnd = Time.time + breakTime;
        }

        private void Update()
        {
            if (breakTimeEnd > 0f)
            {
                if (Time.time > breakTimeEnd)
                {
                    breakTimeEnd = -1f;
                }
                else
                {
                    return;
                }
            }

            if (index >= fullTextSize)
            {
                TextReset();
            }
            else if (startTime + gameConfiguration.TextRate < Time.time)
            {
                startTime = Time.time;
                currentText.Append(fullText[index]);
                textRateSample.text = currentText.ToString();
                index++;
            }
        }

        private int GetTextAssetIndex(TMP_Asset textFont)
        {
            for (int i = 0; i < gameConfiguration.fontList.Length; i++)
            {
                if (gameConfiguration.fontList[i].fontAsset == textFont)
                {
                    // found
                    return i;
                }
            }

            // font not found
            Debug.LogWarning($"Unknown text font: ${textFont.name}");
            return 0; // fallback
        }

        private void OnTextFormattingChanged(int value)
        {
            gameConfiguration.EnableTextFormatting = value == 0;
            onFontChangedEvent.Raise();
        }

        private void OnVolumeChanged(float value)
        {
            gameConfiguration.Volume = value;
            onFontChangedEvent.Raise();
        }

        private void OnShakeChanged(int value)
        {
            gameConfiguration.ShouldShake = value == 0;
            onFontChangedEvent.Raise();
        }

        private void OnFontChanged(int value)
        {
            Debug.Assert(value < gameConfiguration.fontList.Length);
            gameConfiguration.FontAsset = gameConfiguration.fontList[value].fontAsset;
            onFontChangedEvent.Raise();
        }

        private void OnTextOpacityChanged(float value)
        {
            gameConfiguration.TextOpacity = value;
            onFontChangedEvent.Raise();
        }

        private void OnTextRateChanged(float value)
        {
            gameConfiguration.TextRate = value;
            onFontChangedEvent.Raise();
            Debug.Log("Reset");
            TextReset();
        }

        private void OnTextSizeChanged(float value)
        {
            gameConfiguration.FontSize = value;
            onFontChangedEvent.Raise();
        }

        public void Reset()
        {
            gameConfiguration.ResetOptions();
            onFontChangedEvent.Raise();
            UpdateValues();
        }

        public void Back()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}