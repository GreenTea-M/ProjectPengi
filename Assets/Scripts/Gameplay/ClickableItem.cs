using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    /// <summary>
    /// Fires an onClickEvent when the attached object is clicked.
    /// Attached object should have a collider.
    /// </summary>
    public class ClickableItem : MonoBehaviour, IClickable
    {
        public UnityEvent onClickEvent;
        
        public void OnClick()
        {
            onClickEvent.Invoke();
        }
    }
}