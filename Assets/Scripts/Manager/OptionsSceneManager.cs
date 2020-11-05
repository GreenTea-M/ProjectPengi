using System;
using System.Collections.Generic;
using System.Text;
using Dialog;
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
        public FontItem[] fontList;

        public GameConfiguration gameConfiguration;
        public GameEvent onFontChangedEvent;
        public TextMeshProUGUI textRateSample;

        public Slider sliderTextRate;
        public Slider sliderTextSize;
        public Slider sliderTextOpacity;
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
            Debug.Assert(dropdownFont != null);
            Debug.Assert(dropdownShake != null);
            Debug.Assert(dropdownTextFormatting != null);
            Debug.Assert(textRateSample != null);

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
        }

        private void UpdateValues()
        {
            // default values
            sliderTextRate.value = gameConfiguration.textRate;
            sliderTextSize.value = gameConfiguration.fontSize;
            sliderTextOpacity.value = gameConfiguration.textOpacity;
            dropdownFont.value = GetTextAssetIndex(gameConfiguration.fontAsset);
            dropdownShake.value = gameConfiguration.shouldShake ? 0 : 1;
            dropdownTextFormatting.value = gameConfiguration.enableTextFormatting ? 0 : 1;
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
            else if (startTime + gameConfiguration.textRate < Time.time)
            {
                startTime = Time.time;
                currentText.Append(fullText[index]);
                textRateSample.text = currentText.ToString();
                index++;
            }
        }

        private int GetTextAssetIndex(TMP_Asset textFont)
        {
            for (int i = 0; i < fontList.Length; i++)
            {
                if (fontList[i].fontAsset == textFont)
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
            gameConfiguration.enableTextFormatting = value == 0;
            onFontChangedEvent.Raise();
        }

        private void OnShakeChanged(int value)
        {
            gameConfiguration.shouldShake = value == 0;
            onFontChangedEvent.Raise();
        }

        private void OnFontChanged(int value)
        {
            Debug.Assert(value < fontList.Length);
            gameConfiguration.fontAsset = fontList[value].fontAsset;
            onFontChangedEvent.Raise();
        }

        private void OnTextOpacityChanged(float value)
        {
            gameConfiguration.textOpacity = value;
            onFontChangedEvent.Raise();
        }

        private void OnTextRateChanged(float value)
        {
            gameConfiguration.textRate = value;
            onFontChangedEvent.Raise();
            Debug.Log("Reset");
            TextReset();
        }

        private void OnTextSizeChanged(float value)
        {
            gameConfiguration.fontSize = value;
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

    [Serializable]
    public class FontItem
    {
        public TMP_FontAsset fontAsset;
        public string fontName;
    }
}