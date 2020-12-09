using System.Collections;
using Gameplay;
using GameSystem;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Component for the text that informs you that we are auto saving
    /// </summary>
    public class AutoSaveText : MonoBehaviour
    {
        public InputManager inputManager;
        public GameConfiguration gameConfiguration;
        public float alphaRate = 1 / 60f;

        private TextMeshProUGUI _textMesh;
        private int _maxLength = 0;
        private Color _color;

        private void Start()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
            _maxLength = _textMesh.text.Length;
            _textMesh.maxVisibleCharacters = 0;
        }

        public void AnimateText()
        {
            StopAllCoroutines();
            StartCoroutine(DoAnimateText());
        }

        IEnumerator DoAnimateText()
        {
            var color = _textMesh.color;
            color.a = gameConfiguration.TextOpacity;
            _textMesh.color = color;

            for (int i = 0; i < _maxLength; i++)
            {
                while (inputManager.inputState == InputState.Pause)
                {
                    yield return new WaitForSeconds(1f / 60f);
                }

                _textMesh.maxVisibleCharacters = i;
                yield return new WaitForSeconds(gameConfiguration.TextRate);
            }

            _textMesh.maxVisibleCharacters = _maxLength;

            yield return new WaitForSeconds(1f);

            var alpha = _textMesh.color.a;
            while (alpha > 0f)
            {
                alpha -= alphaRate;
                color.a = alpha;
                _textMesh.color = color;
                yield return new WaitForSeconds(1 / 60f);
            }

            _textMesh.maxVisibleCharacters = 0;
        }
    }
}