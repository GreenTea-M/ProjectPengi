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
        public float clickDelay = 0.25f;
        
        [HideInInspector]
        public InputState inputState = InputState.Normal;
        
        private Camera _camera;
        private float _toleratedTime = 0f;

        private void Awake()
        {
            Debug.Assert(dialogueUiManager != null);

            _camera = Camera.main;
        }

        public void ContinueText(InputAction.CallbackContext context)
        {
            if (context.started && Time.time > _toleratedTime)
            {
                switch (inputState)
                {
                    case InputState.Normal:
                        dialogueUiManager.MarkLineComplete();
                        break;
                    case InputState.Shelving:
                        break;
                    case InputState.Pause:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                var position = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var hit = Physics2D.Raycast(position, Vector2.zero);

                if (hit)
                {
                    var clickableItem = hit.transform.gameObject.GetComponent<IClickable>();

                    switch (inputState)
                    {
                        case InputState.Normal:
                        case InputState.Shelving:
                            clickableItem?.OnClick();
                            break;
                        case InputState.Pause:
                            // if we can cast it to ClickableItem, go activate
                            (clickableItem as ClickableItem)?.OnClick();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            // for mouse clicks:
            ContinueText(context);
        }

        public void SetInputState(InputState state)
        {
            _toleratedTime = Time.time + clickDelay;
            inputState = state;
        }
    }
    

    public enum InputState
    {
        Normal,
        Shelving,
        Pause
    }
}