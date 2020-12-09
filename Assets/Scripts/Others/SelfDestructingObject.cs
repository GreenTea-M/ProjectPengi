using UnityEngine;

namespace Others
{
    /// <summary>
    /// Object that disappears once the game starts.
    /// This is useful for positioning objects in the scene relative to this object.
    /// For example, a drawing mocked up by an artist.
    /// </summary>
    public class SelfDestructingObject : MonoBehaviour
    {
        private void Start()
        {
            Destroy(gameObject);
        }
    }
}