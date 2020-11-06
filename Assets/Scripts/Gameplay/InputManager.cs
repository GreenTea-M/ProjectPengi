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
        private Camera _camera;
        private InputState _inputState = InputState.Normal;

        private void Awake()
        {
            Debug.Assert(dialogueUiManager != null);

            _camera = Camera.main;
        }

        public void ContinueText(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                dialogueUiManager.MarkLineComplete();
            }
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            switch (_inputState)
            {
                case InputState.Normal:
                    break;
                case InputState.Shelving:
                    var position = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    var hit = Physics2D.Raycast(position, Vector2.zero);
                    if (hit)
                    {
                        var shelfItem = hit.transform.gameObject.GetComponent<ShelfItem>();
                        if (shelfItem != null)
                        {
                            shelfItem.OnClick();
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetInputState(InputState shelving)
        {
            _inputState = shelving;
        }
    }
    

    public enum InputState
    {
        Normal,
        Shelving
    }
}