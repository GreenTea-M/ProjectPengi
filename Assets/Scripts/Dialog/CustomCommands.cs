using System;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using Yarn.Unity;

namespace Dialog
{
    // todo: play current song being played??? on load
    public class CustomCommands : MonoBehaviour
    {
        [Header("Assets")]
        public AudioItem[] audioList;

        [Header("Prefabs")] 
        public GameObject prefabFadedAudio;

        [Header("Scene objects")]
        // Drag and drop your Dialogue Runner into this variable.
        public DialogueRunner dialogueRunner;

        private FadedAudio _lastAudio = null;
        
        private static readonly Stack<FadedAudio> _pool = new Stack<FadedAudio>();
        
        private void Awake()
        {
            Debug.Assert(dialogueRunner != null);
            Debug.Assert(prefabFadedAudio != null);
            
            dialogueRunner.AddCommandHandler(
                "playAudio", // the name of the command
                PlayAudio // the method to run
            );
        }

        private void PlayAudio(string[] parameters)
        {
            // todo: implement play audio
            if (parameters.Length != 1)
            {
                return;
            }

            string searchTerm = parameters[0].ToUpper();
            foreach (var audioItem in audioList)
            {
                if (!audioItem.name.ToUpper().Equals(searchTerm)) continue;
                
                if (_lastAudio != null)
                {
                    _lastAudio.FadeOut();
                }

                _lastAudio = GetNewAudio();
                _lastAudio.FadeIn(audioItem.audioClip, this);
                    
                return;
            }
        }

        private FadedAudio GetNewAudio()
        {
            if (_pool.Count == 0)
            {
                var fadedAudio = Instantiate(prefabFadedAudio).GetComponent<FadedAudio>();
                Debug.Assert(fadedAudio != null);
                return fadedAudio;
            }
            else
            {
                return _pool.Pop();
            }
        }
        
        public void ReturnAudio(FadedAudio fadedAudio)
        {
            _pool.Push(fadedAudio);
        }

        private void PlayPuzzle(string[] parameters)
        {
            // todo: play puzzle
        }

        private void ChangeIcon(string[] paremeters)
        {
            // todo: implement change icon
        }

        /// <summary>
        /// todo: document
        /// </summary>
        /// <example>
        /// <<changeHeader fishMarket>>
        /// </example>
        /// <param name="parameters">1 name for the header</param>
        private void ChangeHeader(string[] parameters)
        {
            // todo implement change header
        }

        /* Example:
         public void Awake() {
        
            // Create a new command called 'camera_look', which looks at a target.
            dialogueRunner.AddCommandHandler(
                "camera_look",     // the name of the command
                CameraLookAtTarget // the method to run
            );
        }
        
        // The method that gets called when '<<camera_look>>' is run.
        private void CameraLookAtTarget(string[] parameters) {
        
            // Take the first parameter, and use it to find the object
            string targetName = parameters[0];
            GameObject target = GameObject.Find(targetName);
        
            // Log an error if we can't find it
            if (target == null) {
                Debug.LogError($"Cannot make camera look at {targetName}:" + 
                               "cannot find target");
                return;
            }
        
            // Make the main camera look at this target
            Camera.main.transform.LookAt(target.transform);
        } */
    }
    

    [Serializable]
    public class AudioItem
    {
        public AudioClip audioClip;
        public string name;
    }
}