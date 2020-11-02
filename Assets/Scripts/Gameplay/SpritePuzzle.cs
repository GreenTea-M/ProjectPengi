using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class SpritePuzzle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public float sqrTolerance = 0.5f;
        public float forceMultiplier = 10f;

        private Vector3 targetPosition;
        private bool isSolved = false;
        private Rigidbody2D _rigidbody2D;
        private bool isDragging = false;
        private Camera _camera;

        private void Awake()
        {
            targetPosition = transform.position;
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _camera = Camera.main;
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody2D.AddForce(Random.insideUnitCircle * forceMultiplier, ForceMode2D.Impulse);
        }

        private void Update()
        {
            if (isDragging)
            {
                var position = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                position.z = 0;
                transform.position = position;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isSolved)
            {
                return;
            }

            isDragging = true;
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isSolved)
            {
                return;
            }

            if ((targetPosition - transform.position).sqrMagnitude < sqrTolerance)
            {
                Debug.Log("We win: " + name);
                isSolved = true;
                isDragging = false;
                // todo: move to position
                return;
            }

            // todo: start rigidbody
            // todo: stick and cancel anything

            isDragging = false;
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}