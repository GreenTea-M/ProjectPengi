using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class ClickableItem : MonoBehaviour, IClickable
    {
        public UnityEvent onClickEvent;
        
        public void OnClick()
        {
            onClickEvent.Invoke();
        }
    }
}