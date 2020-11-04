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

        private void Awake()
        {
            Debug.Assert(Event != null);
            Debug.Assert(gameConfiguration != null);
            
            Response.AddListener(UpdateFontSize);
            
            _text = GetComponent<TextMeshProUGUI>();
            UpdateFontSize();
        }

        public void UpdateFontSize()
        {
            _text.fontSize = gameConfiguration.FontSize;
        }
    }
}