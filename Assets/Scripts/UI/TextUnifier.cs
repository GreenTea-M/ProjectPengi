using System;
using Dialog;
using RoboRyanTron.Unite2017.Events;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextUnifier : GameEventListener
    {
        public GameConfiguration gameConfiguration;
        private TextMeshProUGUI _text;

        protected new void OnEnable()
        {
            base.OnEnable();
            
            Response.AddListener(UpdateFontSize);
            _text = GetComponent<TextMeshProUGUI>();
            UpdateFontSize();
        }

        private void Awake()
        {
            Debug.Assert(Event != null);
            Debug.Assert(gameConfiguration != null);
        }

        public void UpdateFontSize()
        {
            _text.font = gameConfiguration.fontAsset;
            _text.fontSize = gameConfiguration.FontSize;
            var newColor = gameConfiguration.fontColor;
            newColor.a = gameConfiguration.textOpacity;
            _text.color = newColor;
        }
    }
}