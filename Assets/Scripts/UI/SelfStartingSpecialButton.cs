using TMPro;

namespace UI
{
    /// <summary>
    /// This button does not require external scripts to start.
    /// Its base class requires an external script to activate it.
    /// </summary>
    public class SelfStartingSpecialButton : SpecialButton
    {
        private string _originalText = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_originalText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
                _originalText = buttonText.text;
            }

            SetText(_originalText);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_originalText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = _originalText;
            }
        }
    }
}