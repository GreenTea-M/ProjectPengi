using System;
using Cinemachine;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UnifiedCharacterScript : MonoBehaviour
    {
        public Transform mainLocation;
        public Transform sideLocation;
        public SpriteRenderer sprite;
        public TextMeshProUGUI dialogueText;
        
        private CharacterData _data;
        private State _state = State.Hidden;
        private CharacterType _characterType = CharacterType.Narrator;
        
        private static readonly InformSpeakerReturn Null = new InformSpeakerReturn { isNull = true };

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
            Debug.Assert(mainLocation != null);
            Debug.Assert(sideLocation != null);
            Debug.Assert(sprite != null);
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

        public InformSpeakerReturn IsSimilar(string candidateSpeaker)
        {
            if (_data.IsSimilar(candidateSpeaker))
            {
                InformSpeakerReturn ret = new InformSpeakerReturn();
                
                // todo: change sprite
                // todo: change state??
                
                return ret;
            }
            else
            {
                dialogueText.text = "";
                return Null;
            }
        }
    }
    
    public enum CharacterType
    {
        Narrator,
        Main,
        Side
    }
}