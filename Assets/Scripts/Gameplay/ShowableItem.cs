using Dialog;
using UnityEngine;

namespace Gameplay
{
    public class ShowableItem : MonoBehaviour
    {
        private ShowableItemData _data;

        public void SetData(ShowableItemData data)
        {
            _data = data;
        }

        public bool Match(string name)
        {
            return _data.Match(name);
        }

        public void SelfDestroy()
        {
            Destroy(gameObject);
        }
    }
}