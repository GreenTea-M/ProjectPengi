using System;
using System.Collections;
using Dialog;
using UnityEngine;
using Yarn;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class ShelfItem : MonoBehaviour
    {
        public float forceMultiplier = 40f;
        public Vector3 displayPosition = new Vector3(11f, 6f);
        public float translateDelta = 1f;
        public float rotateDelta = 1f;
        
        private Value _value;
        private ShelfItemData _shelfItemData;
        private CustomCommands _customCommands;
        private bool _isDone = false;
        private Rigidbody2D _rigidbody2DLoc;
        private bool isDisplaying = false;

        private void Update()
        {
            if (!isDisplaying)
            {
                return;
            }

            if (transform.position != displayPosition 
                || transform.rotation != Quaternion.identity)
            {
                transform.position = Vector3.MoveTowards(transform.position, displayPosition, 
                    translateDelta * Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, 
                    rotateDelta * Time.deltaTime);
            }
            else
            {
                isDisplaying = false;
            }
        }

        public void Initialize(ShelfItemData shelfItemData, CustomCommands customCommands)
        {
            _rigidbody2DLoc = GetComponent<Rigidbody2D>();
            Debug.Assert(_rigidbody2DLoc != null);
            Debug.Assert(GetComponent<Collider2D>() != null);
            
            _customCommands = customCommands;
            _value = customCommands.memoryStorage.GetValue("$" + shelfItemData.variableName);
            _shelfItemData = shelfItemData;
            
            switch (_value.type)
            {
                case Value.Type.Null:
                    // not yet visited, explode
                    _rigidbody2DLoc.bodyType = RigidbodyType2D.Dynamic;
                    _rigidbody2DLoc.AddForce(Random.insideUnitCircle * forceMultiplier, ForceMode2D.Impulse);
                    break;
                case Value.Type.Number:
                    _isDone = true;
                    break;
                case Value.Type.String:
                case Value.Type.Bool:
                case Value.Type.Variable:
                    Debug.LogWarning("Value of unknown type: " + _value.AsString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnClick()
        {
            if (_isDone)
            {
                return;
            }
            
            _isDone = true; // for the sake of inform shelf knowing this is done
            
            _customCommands.memoryStorage.SetValue("$" + _shelfItemData.variableName, 1f);
            _customCommands.InformShelfItemTouched(this);
        }

        public bool IsDone()
        {
            return _isDone;
        }

        public void Display()
        {
            _rigidbody2DLoc.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2DLoc.simulated = false;
            isDisplaying = true;
        }
    }
}