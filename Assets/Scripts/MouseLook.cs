using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Look
{
    public class MouseLook : MonoBehaviour
    {
        private Mouse _mosePosition;
        private Rigidbody _myRigidBody;

        private Vector2 _mosusePositionWorldSpace;

        private void Start()
        {
            _myRigidBody = GetComponent<Rigidbody>();   
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _mosePosition = Mouse.current;
            _mosusePositionWorldSpace = Vector2.zero;
        }
    }
}
