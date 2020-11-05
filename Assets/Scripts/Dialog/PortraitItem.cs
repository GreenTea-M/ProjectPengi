using System;
using System.Collections;
using UnityEngine;

namespace Dialog
{
    [RequireComponent(typeof(Animator))]
    public class PortraitItem : MonoBehaviour
    {
        public float scrollDistance = 10f;
        public float scrollDuration = 0.25f;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer outlineRenderer;

        private Vector3 _defaultPosition = new Vector3(18.75f, 8.49f, 0f);
        private IconItem _iconItem;
        private bool _isLeft;
        private string _candidateSpeaker;
        private Animator _animator;

        private int HashAnimGoLeft = Animator.StringToHash("GoLeft");
        private int HashAnimGoRight = Animator.StringToHash("GoRight");
        private int HashAnimIdle = Animator.StringToHash("Idle");
        private int HashAnimSpeak = Animator.StringToHash("Speak");
        private int HashAnimLeave = Animator.StringToHash("Leave");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _defaultPosition = transform.position;
        }

        public void PushUpwards()
        {
            spriteRenderer.enabled = false;
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
            // while (Time.time < endTime)
            // {
            //     transform.position = Vector3.Lerp(startPosition, targetPosition,
            //         (Time.time - startTime) / scrollDuration);
            //     yield return new WaitForSeconds(delay);
            // }

            transform.position = targetPosition;
            yield return null;
        }

        public void SetToCenter(Sprite newSprite)
        {
            spriteRenderer.enabled = true;
            StopAllCoroutines();
            transform.position = _defaultPosition;
            spriteRenderer.sprite = newSprite;
        }

        public void Activate()
        {
            // todo: improve magic numbers
            transform.position = new Vector3(-20f, 20f, 0f);
        }

        public void Appear(IconItem iconItem, string candidateSpeaker, bool isLeft)
        {
            _iconItem = iconItem;
            _candidateSpeaker = candidateSpeaker;
            _isLeft = isLeft;
            
            spriteRenderer.sprite = iconItem?.mainSprite;
            outlineRenderer.sprite = iconItem?.outlineSprite;
            
            _animator.SetTrigger(isLeft ? HashAnimGoLeft : HashAnimGoRight);
        }

        public bool IsSameSpeaker(string candidateSpeaker)
        {
            Debug.LogWarning($"{candidateSpeaker} vs {_candidateSpeaker}: ${_candidateSpeaker.Equals(candidateSpeaker)}");
            return _candidateSpeaker.Equals(candidateSpeaker);
        }

        public void Leave()
        {
            _animator.SetTrigger(HashAnimLeave);
        }

        public void Idle()
        {
            _animator.SetTrigger(HashAnimIdle);
        }

        public void Speak()
        {
            _animator.SetTrigger(HashAnimSpeak);
        }
    }
}