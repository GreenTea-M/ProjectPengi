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

        private Vector3 _defaultPosition = new Vector3(18.75f, 8.49f, 0f);
        private IconItem _iconItem;
        private bool _isLeft;
        private string _speaker;
        private Animator _animator;

        private int HashIsActive = Animator.StringToHash("IsActive");
        private int HashIsSpeaking = Animator.StringToHash("IsSpeaking");
        private int HashIsLeft = Animator.StringToHash("IsLeft");
        public string Speaker => GetRealName();
        public bool IsLeft => _isLeft;

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
            _speaker = candidateSpeaker;
            _isLeft = isLeft;
            
            spriteRenderer.sprite = iconItem?.mainSprite;
            
            _animator.SetBool(HashIsLeft, isLeft);
            _animator.SetBool(HashIsActive, true);
            // todo: delete old code
            // _animator.SetTrigger(isLeft ? HashAnimGoLeft : HashAnimGoRight);
        }

        public bool IsSameSpeaker(string candidateSpeaker)
        {
            return _iconItem.IsSpeaker(candidateSpeaker);
        }

        public void Leave()
        {
            _animator.SetBool(HashIsActive, false);
            spriteRenderer.sprite = _iconItem?.mainSprite;
        }

        public void Idle()
        {
            _animator.SetBool(HashIsActive, true);
            _animator.SetBool(HashIsSpeaking, false);
            spriteRenderer.sprite = _iconItem?.mainSprite;
        }

        public void Speak()
        {
            _animator.SetBool(HashIsActive, true);
            _animator.SetBool(HashIsSpeaking, true);
            spriteRenderer.sprite = _iconItem?.outlineSprite;
        }

        public string GetRealName()
        {
            return _iconItem.name;
        }
    }
}