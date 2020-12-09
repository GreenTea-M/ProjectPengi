using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// <c>LocationPlate</c> animates the swinging sign which displays the current location's name.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class LocationPlate : MonoBehaviour
    {
        public TextMeshProUGUI text;
        private Animator _animator;

        private int HashHidden = Animator.StringToHash("LocationPlateHidden");
        private int HashStartLocationPlate = Animator.StringToHash("StartLocationPlate");
        private string _previousLocationName = "";

        private void OnEnable()
        {
            StartCoroutine(OnEnablePlate());
        }

        public void SetLocation(string locationName)
        {
            StopAllCoroutines();
            
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (locationName.Equals(""))
            {
                text.text = "";
                _animator.Play(HashHidden);
            }
            else
            {
                if (_previousLocationName.Equals(locationName))
                {
                    return;
                }

                _previousLocationName = locationName;
                
                text.text = locationName;
                _animator.Play(HashStartLocationPlate);
            }
        }

        IEnumerator OnEnablePlate()
        {
            yield return new WaitForSeconds(0.25f);
            SetLocation(text.text);
        }
    }
}