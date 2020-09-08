using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

namespace Look
{
    public class MouseLook : MonoBehaviour
    {

        public Transform _myCamera;
        private Transform _myPlayer;

        private Vector2 _mouseAxisChange;
        private Vector2 _desiredRotation;

        public float mouseSesitivity = 100f;

        private void Start()
        {
            _myPlayer = GetComponent<Transform>();

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            _mouseAxisChange = mouseSesitivity * Time.deltaTime * Mouse.current.delta.ReadValue();
            

            _desiredRotation.y = _myCamera.rotation.eulerAngles.y + _mouseAxisChange.x;
            _desiredRotation.x = Mathf.Clamp( _desiredRotation.x - _mouseAxisChange.y , -90f , 90f );


            _myCamera.localRotation = Quaternion.Euler( _desiredRotation.x , _desiredRotation.y , 0f );
            _myPlayer.localRotation = Quaternion.Euler( 0f , _desiredRotation.y , 0f );

        }
    }
}
