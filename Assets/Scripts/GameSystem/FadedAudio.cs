using System;
using Dialog;
using UnityEngine;
using UnityEngine.Audio;

namespace GameSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class FadedAudio : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        public float changeRate = 1f;
        public float maxVolume = 0.35f;
        public float fadeOutRatio = 0.5f;
        
        // for other things
        
        public bool playOnStart = false;
        
        private AudioSource _audioSource;
        private AudioMixerGroup _audioMixerGroup;
        private CustomCommands _parent;
        private State _state = State.Nothing;

        private float _fadeOutRate = 1f; 

        private enum State
        {
            Nothing,
            FadeIn,
            FadeOut
        }
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (playOnStart)
            {
                FadeIn(GetComponent<AudioSource>().clip, null);
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Nothing:
                    break;
                
                case State.FadeIn:
                    if (_audioSource.volume >= maxVolume * gameConfiguration.Volume)
                    {
                        _audioSource.volume = maxVolume * gameConfiguration.Volume;
                        _state = State.Nothing;
                    }
                    else
                    {
                        _audioSource.volume += changeRate * Time.deltaTime;
                    }
                    break;
                
                case State.FadeOut:
                    if (_audioSource.volume <= 0f)
                    {
                        _audioSource.volume = 0f;
                        _state = State.Nothing;
                        _audioSource.Stop();
                        if (_parent != null)
                        {
                            _parent.ReturnAudio(this);
                        }
                    }
                    else
                    {
                        _audioSource.volume -= _fadeOutRate * Time.deltaTime;
                    }
                    break;
                
                default:
                    Debug.LogWarning("Case not made");
                    break;
            }
        }

        public void OnOptionsChanged()
        {
            _state = State.FadeIn;
        }

        public void FadeIn(AudioClip clip, CustomCommands commands)
        {
            _parent = commands;
            _audioSource.clip = clip;
            _audioSource.volume = 0f;
            _audioSource.Play();
            _state = State.FadeIn;
        }

        public void FadeOut()
        {
            _fadeOutRate = changeRate * fadeOutRatio;
            _state = State.FadeOut;
        }
    }
}