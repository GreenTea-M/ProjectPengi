using System;
using UnityEngine;
using UnityEngine.Assertions;
using Yarn.Unity;

namespace Dialog
{
    // todo: play current song being played??? on load
    public class CustomCommands : MonoBehaviour
    {
        [Header("Assets")]
        public AudioItem[] audioList;

        [Header("Scene objects")]
        // Drag and drop your Dialogue Runner into this variable.
        public DialogueRunner dialogueRunner;

        private AudioSource _audioSource;
        
        private void Awake()
        {
            Debug.Assert(dialogueRunner != null);
            
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
                if (audioItem.name.ToUpper().Equals(searchTerm))
                {
                    audioItem.audioClip = audioItem.audioClip;
                    return;
                }
            }
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