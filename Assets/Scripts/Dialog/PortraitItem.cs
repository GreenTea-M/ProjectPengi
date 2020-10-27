using System.Collections;
using UnityEngine;

namespace Dialog
{
    public class PortraitItem : MonoBehaviour
    {
        public float scrollDistance = 10f;
        public float scrollDuration = 0.25f;
        public SpriteRenderer spriteRenderer;

        // todo: improve magic numbers
        private readonly Vector3 _defaultPosition = new Vector3(18.75f, 8.49f, 0f);

        public void PushUpwards()
        {
            StopAllCoroutines();
            StartCoroutine(DoPushUpwards(scrollDistance));
        }

        private IEnumerator DoPushUpwards(float distance)
        {
            const float delay = 1f / 60f;
            float startTime = Time.time;
            float endTime = startTime + scrollDuration;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + Vector3.up * distance;
            while (Time.time < endTime)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition,
                    (Time.time - startTime) / scrollDuration);
                yield return new WaitForSeconds(delay);
            }

            transform.position = targetPosition;
            yield return null;
        }

        public void SetToCenter(Sprite newSprite)
        {
            StopAllCoroutines();
            transform.position = _defaultPosition;
            spriteRenderer.sprite = newSprite;
        }

        public void Activate()
        {
            // todo: improve magic numbers
            transform.position = new Vector3(-20f, 20f, 0f);
        }
    }
}