using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dialog
{
    public class TextItem : MonoBehaviour
    {
        public TextMeshProUGUI uiText;
        public GameObject uiButton;
        public Transform layoutGroup;
        public float scrollDistance = 1f;
        public float scrollDuration = 1f;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        
        private readonly List<Button> _buttons = new List<Button>();
        private Vector3 _defaultPosition = new Vector3(9.044f, 9.358f, 0f);
        private int _currentTextMax;

        private void Awake()
        {
            _defaultPosition = transform.position;
            Debug.Assert(uiText != null);
            Debug.Assert(uiButton != null);
            Debug.Assert(uiButton.GetComponent<Button>() != null);
            Debug.Assert(layoutGroup != null);
        }

        public void UpdateLine(string text)
        {
            uiText.text = text;
        }

        public void PushUpwards()
        {
            uiText.text = "";
            layoutGroup.gameObject.SetActive(false);
            StopAllCoroutines();
            StartCoroutine(DoPushUpwards(scrollDistance));
        }

        private IEnumerator DoPushUpwards(float distance)
        {
            const float delay = 1f / 60f;
            float startTime = Time.time;
            float endTime = startTime + scrollDuration;
            Vector3 startPosition = _rectTransform.position;
            Vector3 targetPosition = startPosition + Vector3.up * distance;
            // while (Time.time < endTime)
            // {
            //     _rectTransform.position = Vector3.Lerp(startPosition, targetPosition,
            //         (Time.time - startTime) / scrollDuration);
            //     yield return new WaitForSeconds(delay);
            // }

            _rectTransform.position = targetPosition;
            yield return null;
        }

        public void SetToCenter()
        {
            layoutGroup.gameObject.SetActive(true);
            StopAllCoroutines();
            _rectTransform.position = _defaultPosition;
        }

        public void Activate()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponent<Canvas>();
            // todo: improve magic numbers
            _rectTransform.position = new Vector3(-20f, 20f, 0f);
            _canvas.worldCamera = Camera.main;
        }

        public void CreateButtons(int optionsLength)
        {
            while (_buttons.Count < optionsLength)
            {
                Button newButton = Instantiate(uiButton, layoutGroup).GetComponent<Button>();
                newButton.gameObject.SetActive(false);
                _buttons.Add(newButton);
            }
        }

        public void ActivateButtons(int i, UnityAction call)
        {
            Debug.Assert(i < _buttons.Count);
            _buttons[i].gameObject.SetActive(true);
            _buttons[i].onClick.RemoveAllListeners();
            _buttons[i].onClick.AddListener(call);
        }

        public void SetButtonText(int i, string optionText)
        {
            // todo: improve performance
            var uiText = _buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            Debug.Assert(uiText != null);
            uiText.text = optionText;
        }

        public void HideAllButtons()
        {
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(false);
            }
        }

        public void SetInitialText(string text)
        {
            uiText.maxVisibleCharacters = 0;
            uiText.text = text;
            _currentTextMax = text.Length;
        }

        public void ShowCharacters(int count)
        {
            uiText.maxVisibleCharacters = count;
        }
    }
}