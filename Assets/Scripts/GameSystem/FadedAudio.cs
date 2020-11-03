using System;
using Dialog;
using UnityEngine;
using UnityEngine.Audio;

namespace GameSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class FadedAudio : MonoBehaviour
    {
        public float changeRate = 1f;
        public float maxVolume = 0.35f;
        public float fadeOutRatio = 0.5f;
        
        private AudioSource _audioSource;
        private AudioMixerGroup _audioMixerGroup;
        private const string VolumeParam = "Volume";
        private CustomCommands parent;
        private State state = State.Nothing;

        private float fadeOutRate = 1f; 

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

        private void Update()
        {
            switch (state)
            {
                case State.Nothing:
                    break;
                
                case State.FadeIn:
                    if (_audioSource.volume >= maxVolume)
                    {
                        _audioSource.volume = maxVolume;
                        state = State.Nothing;
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
                        state = State.Nothing;
                        _audioSource.Stop();
                        parent.ReturnAudio(this);
                    }
                    else
                    {
                        _audioSource.volume -= fadeOutRate * Time.deltaTime;
                    }
                    break;
                
                default:
                    Debug.LogWarning("Case not made");
                    break;
            }
        }

        public void FadeIn(AudioClip clip, CustomCommands commands)
        {
            parent = commands;
            _audioSource.clip = clip;
            _audioSource.volume = 0f;
            _audioSource.Play();
            state = State.FadeIn;
        }

        public void FadeOut()
        {
            fadeOutRate = changeRate * fadeOutRatio;
            state = State.FadeOut;
        }
    }
}