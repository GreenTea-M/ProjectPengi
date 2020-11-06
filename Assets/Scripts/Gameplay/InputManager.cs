using System;
using Dialog;
using Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class InputManager : MonoBehaviour
    {
        public DialogueUIManager dialogueUiManager;
        
        private void Awake()
        {
            Debug.Assert(dialogueUiManager != null);
        }

        public void ContinueText(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                dialogueUiManager.MarkLineComplete();
            }
        }
    }
}