using System;
using Dialog;
using GameSystem;
using RoboRyanTron.Unite2017.Events;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextUnifier : GameEventListener
    {
        public GameConfiguration gameConfiguration;
        public float textFontMultiplier = 1f;
        [FormerlySerializedAs("shouldIgnoreColor")] public bool shouldIgnoreAlpha = false;
        public bool shouldIgnoreSize = false;
        public bool shouldIgnoreColor = false;
        
        private TextMeshProUGUI _text;
        private Color _color;

        protected new void OnEnable()
        {
            base.OnEnable();
            
            Response.AddListener(UpdateFontSize);
            _text = GetComponent<TextMeshProUGUI>();
            _color = _text.color;
            UpdateFontSize();
        }

        private void Awake()
        {
            Debug.Assert(Event != null);
            Debug.Assert(gameConfiguration != null);
        }

        public void UpdateFontSize()
        {
            _text.font = gameConfiguration.FontAsset;
            
            if (!shouldIgnoreSize)
            {
                _text.fontSize = gameConfiguration.FontSize * textFontMultiplier;
            }

            if (!shouldIgnoreColor)
            {
                var newColor = gameConfiguration.fontColor;
                newColor.a = shouldIgnoreAlpha ? _color.a : gameConfiguration.TextOpacity;
                _text.color = newColor;
            }
        }
    }
}