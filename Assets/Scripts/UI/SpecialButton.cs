using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpecialButton : Button
    {
        protected TextMeshProUGUI buttonText;
        private string _optionText;
        private SelectionState _state;
        private const float Delay = 1f/4f;
        private int _count = 0;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (state == SelectionState.Selected)
            {
                state = SelectionState.Normal;
            }
            base.DoStateTransition(state, instant);
            _state = state;
        }

        IEnumerator AnimateText()
        {
            while (true)
            {
                switch (_state)
                {
                    case SelectionState.Normal:
                        NormalText();
                        break;
                    case SelectionState.Highlighted:
                        HighlightText();
                        break;
                    case SelectionState.Pressed:
                        buttonText.text = $"<b>>>></b> <u>{_optionText}</u>";
                        break;
                    case SelectionState.Selected:
                        NormalText();
                        break;
                    case SelectionState.Disabled:
                        buttonText.text = $"<alpha=#66>{_optionText}";
                        break;
                    default:
                        break;
                }

                _count = (_count + 1) % 6;
                
                yield return new WaitForSeconds(Delay);
            }
        }

        private void NormalText()
        {
            if (_count % 2 == 0)
            {
                buttonText.text = $">  <u>{_optionText}</u>";
            }
            else
            {
                buttonText.text = $"> <u>{_optionText}</u>";
            }
        }

        private void HighlightText()
        {
            var remainder = _count % 3;
            switch (remainder)
            {
                case 0:
                    buttonText.text = $"<b>></b>>> <b><u>{_optionText}</u>";
                    break;
                case 1:
                    buttonText.text = $"><b>></b>> <b><u>{_optionText}</u>";
                    break;
                case 2:
                    buttonText.text = $">><b>></b> <b><u>{_optionText}</u>";
                    break;
                default:
                    break;
            }
        }

        public void SetText(string optionText)
        {
            // from http://digitalnativestudios.com/forum/index.php?topic=1199.0
            optionText = optionText.Replace("\\n","\n");
            
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _optionText = optionText;
            buttonText.text = $"<u>{optionText}";
            StopAllCoroutines();
            StartCoroutine(AnimateText());
        }
    }
}