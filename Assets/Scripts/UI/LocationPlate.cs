using System;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(Animator))]
    public class LocationPlate : MonoBehaviour
    {
        public TextMeshProUGUI text;
        private Animator _animator;

        private int HashHidden = Animator.StringToHash("LocationPlateHidden");
        private int HashStartLocationPlate = Animator.StringToHash("StartLocationPlate");

        public void SetLocation(string locationName)
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (locationName.Equals(""))
            {
                Debug.Log("Disappear plate now");
                text.text = "";
                _animator.Play(HashHidden);
            }
            else
            {
                Debug.Log(locationName);
                text.text = locationName;
                _animator.Play(HashStartLocationPlate);
            }
        }
    }
}