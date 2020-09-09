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
        public float cameraSmoothTime = 0.01f;
        public float maxCameraRotationSpeed = 10f;

        private float _currentVelocity;

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

            #region SmoothDamp test to resolve jittering camera(made it worse)
            /*
            _myCamera.localRotation = Quaternion.Euler( Mathf.SmoothDamp( _myCamera.rotation.eulerAngles.x , Mathf.Clamp( _myCamera.rotation.eulerAngles.x - _mouseAxisChange.y , -90f , 90f ) , ref _currentVelocity , cameraSmoothTime ) ,
                Mathf.SmoothDampAngle( _myCamera.rotation.eulerAngles.y , _myCamera.rotation.eulerAngles.y + _mouseAxisChange.x , ref _currentVelocity , cameraSmoothTime ) , 
                0f 
                );
                _myPlayer.localRotation = Quaternion.Euler( 0f , Mathf.SmoothDamp( _myCamera.rotation.eulerAngles.y , _myCamera.rotation.eulerAngles.y + _mouseAxisChange.x , ref _currentVelocity , cameraSmoothTime ) , 0f );
            */
            #endregion

            _myCamera.localRotation = Quaternion.Euler( _desiredRotation.x , _desiredRotation.y , 0f );
            _myPlayer.localRotation = Quaternion.Euler( 0f , _desiredRotation.y , 0f );
            
        }
    }
}
