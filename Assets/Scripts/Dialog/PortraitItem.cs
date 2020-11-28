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
        private Animator _animator;

        private int HashIsActive = Animator.StringToHash("IsActive");
        private int HashIsSpeaking = Animator.StringToHash("IsSpeaking");
        private int HashIsLeft = Animator.StringToHash("IsLeft");
        private bool _isActive = false;
        
        public string Speaker => GetRealName();
        public bool IsActive => _isActive;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _defaultPosition = transform.position;
        }

        private void OnDisable()
        {
            _isActive = false;
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

        public void Setup(IconItem iconItem, string candidateSpeaker)
        {
            Debug.Log($"Speaking: {candidateSpeaker}");
            _iconItem = iconItem;
            spriteRenderer.sprite = iconItem?.mainSprite;
        }

        public void Appear(bool isLeft)
        {
            this._isActive = true;
            _animator.SetBool(HashIsLeft, IsSameSpeaker(IconManager.mainSpeakerName));
            _animator.SetBool(HashIsActive, true);
        }

        public bool IsSameSpeaker(string candidateSpeaker)
        {
            return _iconItem.IsSimilar(candidateSpeaker);
        }

        public void Leave()
        {
            _isActive = false;
            _animator.SetBool(HashIsLeft, IsSameSpeaker(IconManager.mainSpeakerName));
            _animator.SetBool(HashIsActive, false);
            spriteRenderer.sprite = _iconItem?.mainSprite;
        }

        public void Idle()
        {
            _isActive = true;
            _animator.SetBool(HashIsLeft, IsSameSpeaker(IconManager.mainSpeakerName));
            _animator.SetBool(HashIsActive, true);
            _animator.SetBool(HashIsSpeaking, false);
            spriteRenderer.sprite = _iconItem?.mainSprite;
        }

        public void Speak()
        {
            _isActive = true;
            _animator.SetBool(HashIsLeft, IsSameSpeaker(IconManager.mainSpeakerName));
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