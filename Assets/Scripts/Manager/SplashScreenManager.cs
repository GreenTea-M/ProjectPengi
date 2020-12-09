using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Manager
{
    [RequireComponent(typeof(VideoPlayer))]
    public class SplashScreenManager : MonoBehaviour
    {
        public SpriteRenderer blackScreen;
        public float fadeDuration = 2f;
        public string videoPath = "Videos/Tea_Logo.mp4";
        public float maxDuration = 10f;
        
        private VideoPlayer _videoPlayer;
        private State _state = State.Playing;
        private float _startAlpha = 0f;
        private float _targetAlpha = 1f;
        private Color _color;
        private float _startTime;

        private enum State
        {
            Playing,
            Fading
        }
        
        private void Start()
        {
            Debug.Assert(blackScreen != null);
            
            _videoPlayer = GetComponent<VideoPlayer>();
            _videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, videoPath);
            _videoPlayer.Play();
            StartCoroutine(LoopAttachmentDelay());
            
            _color = blackScreen.color;
            _color.a = _startAlpha;
            blackScreen.color = _color;
            _startTime = Time.time;
        }


        private IEnumerator LoopAttachmentDelay()
        {
            yield return new WaitForSeconds(0.5f);
            _videoPlayer.loopPointReached += LoopPointReached;
        }
            
            
        private void Update()
        {
            switch (_state)
            {
                case State.Playing:
                    break;
                case State.Fading:
                    _startAlpha += Time.deltaTime * fadeDuration;
                    _color.a = _startAlpha;
                    blackScreen.color = _color;

                    if (_startAlpha >= _targetAlpha)
                    {
                        SceneManager.LoadScene("MainMenuScene");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Time.time > maxDuration)
            {
                _state = State.Fading;
            }
        }

        private void LoopPointReached(VideoPlayer source)
        {
            _state = State.Fading;
        }
    }
}