using System;
using Dialog;
using Others;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class BackgroundScript : MonoBehaviour
    {
        [FormerlySerializedAs("allParts")] 
        public SpriteRenderer[] allSpriteRenderers;
        public float transitionDuration = 1f;
        public float maxAlpha = 1f;

        private float _allAlpha = 0f;

        enum State
        {
            None,
            Appearing,
            Disappearing,
            Shown
        }

        private State _state = State.None;
        private BackgroundItem _backgroundItem;
        public string DisplayName => _backgroundItem.displayName;
        public string CodeName => _backgroundItem.name;

        private void Update()
        {
            foreach (var bg in allSpriteRenderers)
            {
                var bgColor = bg.color;
                switch (_state)
                {
                    case State.None:
                        _allAlpha = 0f;
                        bgColor.a = _allAlpha;
                        bg.color = bgColor;
                        break;
                    case State.Appearing:
                        _allAlpha += Time.deltaTime * transitionDuration;
                        bgColor.a = _allAlpha;
                        bg.color = bgColor;
                        if (_allAlpha >= maxAlpha)
                        {
                            _state = State.Shown;
                        }
                        break;
                    case State.Disappearing:
                        // todo: improve fade
                        _allAlpha -= Time.deltaTime * transitionDuration;
                        bgColor.a = _allAlpha;
                        bg.color = bgColor;
                        if (_allAlpha <= 0f)
                        {
                            _state = State.None;
                            gameObject.SetActive(false);
                        }
                        break;
                    case State.Shown:
                        // do nothing
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Appear()
        {
            _state = State.Appearing;
        }

        public void Disappear()
        {
            _state = State.Disappearing;
        }

        public void SetData(BackgroundItem backgroundItem)
        {
            _backgroundItem = backgroundItem;
        }

        public bool IsSimilar(string searchTerm)
        {
            return _backgroundItem.IsSimilar(searchTerm);
        }
    }

    [Serializable]
    public class BackgroundItem : DataItem
    {
        public GameObject prefab;
        public string displayName;
    }
}