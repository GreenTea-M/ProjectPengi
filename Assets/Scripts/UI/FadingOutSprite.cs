using UnityEngine;

namespace UI
{
    /// <summary>
    /// On start, the sprite fades and destroys itself.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FadingOutSprite : MonoBehaviour
    {
        public float fadeRate = 1f;
        
        private SpriteRenderer _spriteRenderer;
        private float _alpha = 1f;
        private Color _color;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _color = _spriteRenderer.color;
        }

        private void Update()
        {
            _alpha -= fadeRate * Time.deltaTime;
            _color.a = _alpha;
            _spriteRenderer.color = _color;
            if (_alpha < 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}