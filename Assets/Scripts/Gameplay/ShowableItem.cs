using System;
using Dialog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Yarn.Unity;

namespace Gameplay
{
    /// <summary>
    /// Common component for objects that can be shown in the scene through the YarnCommand <<showItem>>.
    /// </summary>
    public class ShowableItem : MonoBehaviour
    {
        public UnityStringEvent instructionListeners;
        public bool delayDestroy = false;
        
        private ShowableItemData _data;
        private const string DestroyKeyword = "Destroy";

        public void SetData(ShowableItemData data)
        {
            _data = data;
        }

        public bool Match(string name)
        {
            return _data.Match(name);
        }

        public void Instruct(string instruction)
        {
            instructionListeners.Invoke(instruction);
        }

        public void SelfDestroy()
        {
            if (delayDestroy)
            {
                instructionListeners.Invoke(DestroyKeyword);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    [Serializable]
    public class UnityStringEvent : UnityEvent<string>
    {
    }
}