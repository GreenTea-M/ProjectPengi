using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UnifiedCharacterScript : MonoBehaviour
    {
        [Header("Locations")] public Transform stageLocation;
        public Transform exitLocation;
        public Transform alternativeTextLocation;

        [Header("Child components")] public SpriteRenderer sprite;
        public TextMeshProUGUI dialogueText;
        public Transform layoutGroup;
        public GameObject uiButton;
        public Canvas textCanvas;
        public Canvas buttonCanvas;
        public float transitionSpeed = 10f;
        public float textTransitionSpeed = 20f;

        private CharacterData _data;
        private State _state = State.Hidden;
        private TextState _textState = TextState.Default;
        private CharacterType _characterType = CharacterType.Narrator;
        private bool _isSpeaking = false;
        private int _currentTextMax;
        private IconManager _iconManager;
        private List<Button> _buttons = new List<Button>();
        private Vector3 _defaultTextLocation;
        private IEnumerator textMovementCoroutine;

        private enum State
        {
            Hidden,
            Appearing,
            Idling,
            Speaking,
            Disappearing
        }

        private enum TextState
        {
            Default,
            ToAlternative,
            Alternative,
            ToDefault
        }

        private void Awake()
        {
            Debug.Assert(stageLocation != null);
            Debug.Assert(exitLocation != null);
            Debug.Assert(sprite != null);
        }

        private void Start()
        {
            _defaultTextLocation = dialogueText.transform.position;

            dialogueText.text = "";

            if (buttonCanvas != null)
            {
                Camera _camera = Camera.main;
                buttonCanvas.worldCamera = _camera;
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Hidden:
                    // do nothing
                    break;
                case State.Appearing:
                    Appear();
                    break;
                case State.Idling:
                    if (_isSpeaking)
                    {
                        _state = State.Speaking;
                        _iconManager.informSpeakerReturnValue.dialogueBlocker.Unblock();
                    }

                    break;
                case State.Speaking:
                    break;
                case State.Disappearing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (_textState)
            {
                case TextState.Default:
                    break;
                case TextState.ToAlternative:
                    if (dialogueText.transform.position != alternativeTextLocation.position)
                    {
                        dialogueText.transform.position = Vector3.MoveTowards(dialogueText.transform.position,
                            alternativeTextLocation.position, textTransitionSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _textState = TextState.Alternative;
                    }
                    break;
                case TextState.Alternative:
                    break;
                case TextState.ToDefault:
                    if (dialogueText.transform.position != _defaultTextLocation)
                    {
                        dialogueText.transform.position = Vector3.MoveTowards(dialogueText.transform.position,
                            _defaultTextLocation, textTransitionSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _textState = TextState.Default;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Appear()
        {
            switch (_characterType)
            {
                case CharacterType.Narrator:
                    // do nothing
                    break;
                case CharacterType.Main:
                case CharacterType.Side:
                    sprite.transform.position = Vector3.MoveTowards(sprite.transform.position,
                        stageLocation.position,
                        transitionSpeed * Time.deltaTime);
                    if (sprite.transform.position == stageLocation.position)
                    {
                        _state = State.Idling;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TextMeshProUGUI Speak()
        {
            throw new NotImplementedException();
        }

        public void SetData(CharacterData characterData, IconManager iconManager)
        {
            _iconManager = iconManager;
            _data = characterData;
            _characterType = _data.characterType;
            // todo: set sprite???
        }

        public bool IsSimilar(string candidateSpeaker)
        {
            return _data.IsSimilar(candidateSpeaker);
        }

        public void UpdateStatus()
        {
            if (_iconManager.currentSpeaking != this && _state != State.Hidden)
            {
                _state = State.Idling;
                _isSpeaking = false;
            }
            else if (_iconManager.currentSpeaking == this && _characterType != CharacterType.Narrator)
            {
                _isSpeaking = true;

                switch (_state)
                {
                    case State.Hidden:
                        // show up
                        _state = State.Appearing;
                        _iconManager.informSpeakerReturnValue.dialogueBlocker.Block();
                        break;
                    case State.Appearing:
                        _iconManager.informSpeakerReturnValue.dialogueBlocker.Block();
                        // just keep it up
                        break;
                    case State.Idling:
                        // go to speak immediately
                        _state = State.Speaking;
                        break;
                    case State.Speaking:
                        // keep the state
                        break;
                    case State.Disappearing:
                        Debug.LogWarning("UpdateStatus: This should not happen");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (_characterType == CharacterType.Side && !_isSpeaking)
            {
                // offset the queue???
            }

            textCanvas.gameObject.SetActive(false);
            dialogueText.text = "";
        }


        public void SetInitialText(string text)
        {
            textCanvas.gameObject.SetActive(true);
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.text = text;
            _currentTextMax = text.Length;
        }

        public void ShowCharacters(int count)
        {
            dialogueText.maxVisibleCharacters = count;
        }

        public void CreateButtons(int optionsLength)
        {
            Debug.Assert(_characterType == CharacterType.Main);
            buttonCanvas.gameObject.SetActive(true);

            while (_buttons.Count < optionsLength)
            {
                Button newButton = Instantiate(uiButton, layoutGroup).GetComponent<Button>();
                newButton.gameObject.SetActive(false);
                _buttons.Add(newButton);
            }

            // move text to alternative location
            _textState = TextState.ToAlternative;
        }

        public void ActivateButtons(int i, UnityAction call)
        {
            Debug.Assert(_characterType == CharacterType.Main);
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

            buttonCanvas.gameObject.SetActive(false);

            // move text to old location
            _textState = TextState.ToDefault;
        }
    }

    public enum CharacterType
    {
        Narrator,
        Main,
        Side
    }
}