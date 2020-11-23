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
        
        private CharacterData _data;
        private State _state = State.Hidden;
        private CharacterType _characterType = CharacterType.Narrator;
        private bool _isSpeaking = false;

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
                    break;
                case CharacterType.Main:
                    break;
                case CharacterType.Side:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TextMeshProUGUI Speak()
        {
            throw new NotImplementedException();
        }

        public void SetData(CharacterData characterData)
        {
            _data = characterData;
            _characterType = _data.characterType;
            // todo: set sprite???
        }

        public bool IsSimilar(string candidateSpeaker)
        {
            return _data.IsSimilar(candidateSpeaker);
        }

        public void UpdateStatus(IconManager iconManager)
        {
            if (iconManager.currentSpeaking != this && _state != State.Hidden)
            {
                _state = State.Idling;
                _isSpeaking = false;
            }
            else
            {
                _isSpeaking = true;

                switch (_state)
                {
                    case State.Hidden:
                        // show up
                        _state = State.Appearing;
                        break;
                    case State.Appearing:
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
    }
    
    public enum CharacterType
    {
        Narrator,
        Main,
        Side
    }
}