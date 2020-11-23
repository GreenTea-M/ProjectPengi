using System;
using Cinemachine;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UnifiedCharacterScript : MonoBehaviour
    {
        public Transform stageLocation;
        public Transform exitLocation;
        public SpriteRenderer sprite;
        public TextMeshProUGUI dialogueText;
        public float transitionSpeed = 10f;

        private CharacterData _data;
        private State _state = State.Hidden;
        private CharacterType _characterType = CharacterType.Narrator;
        private bool _isSpeaking = false;
        private int _currentTextMax;
        private IconManager _iconManager;

        private enum State
        {
            Hidden,
            Appearing,
            Idling,
            Speaking,
            Disappearing
        }

        private void Awake()
        {
            Debug.Assert(stageLocation != null);
            Debug.Assert(exitLocation != null);
            Debug.Assert(sprite != null);
        }

        private void Start()
        {
            dialogueText.text = "";
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

            dialogueText.text = "";
        }


        public void SetInitialText(string text)
        {
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.text = text;
            _currentTextMax = text.Length;
        }

        public void ShowCharacters(int count)
        {
            dialogueText.maxVisibleCharacters = count;
        }
    }

    public enum CharacterType
    {
        Narrator,
        Main,
        Side
    }
}