using System;
using UnityEngine;

namespace Others
{
    public class SelfDestructingObject : MonoBehaviour
    {
        private void Start()
        {
            Destroy(gameObject);
        }
    }
}