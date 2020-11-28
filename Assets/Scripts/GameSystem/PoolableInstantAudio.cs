using System;
using UnityEngine;

namespace GameSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class PoolableInstantAudio : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        
        private AudioSource _audioSource;
        private State _state = State.Uninitialized;
        private IPooler _pooler;

        private enum State
        {
            Uninitialized,
            Unused,
            Playing
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Uninitialized:
                    break;
                case State.Unused:
                    break;
                case State.Playing:
                    if (!_audioSource.isPlaying)
                    {
                        _state = State.Unused;
                        _pooler.ReturnInstantAudio(this);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public interface IPooler
        {
            void ReturnInstantAudio(PoolableInstantAudio finished);
        }

        public void Initialize()
        {
            _audioSource = GetComponent<AudioSource>();
            _state = State.Unused;
        }

        public void Play(IPooler pooler, AudioClip audioClip)
        {
            Debug.Assert(_state == State.Unused);
            
            _pooler = pooler;
            _audioSource.volume = gameConfiguration.Volume;
            _audioSource.clip = audioClip;
            _audioSource.Play();
            _state = State.Playing;
        }
    }
}