using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace Movement
{
    public class PlayerController : MonoBehaviour
    {
        //Components 
        [Header( "Layer Mask" )]
        public LayerMask layerMaskGround;
        [Space]

        private Rigidbody _myRigidBody;
        private CapsuleCollider _myCollider;



        //input
        private Vector3 _inputDirection = Vector3.zero;



        [Header( "Movement" )]
        public float moveSpeed = 10f;
        public float smoothTime = 0.1f;

        private float _currentVelocityX; // for ref
        private float _currentVelocityZ; // for ref
        private Vector3 _smoothInputMagnitude = Vector3.zero;
        private Vector3 _playerCoordinatesToMove;
        [Space]


        [Header( "Ground Check" )]
        public float sphereCheckPosition = -0.51f;
        [Space]


        [Header( "Jumping" )]
        public float jumpGravity = -9.81f;
        public float jumpHeight = 2f;
        public float moveSpeedWhileAirborn = 5f;

        private bool isSpacePressed = false;
        private bool isGrounded;
        private float fallingVelocity = 0f;







        // Start is called before the first frame update
        void Start()
        {
            _myRigidBody = GetComponent<Rigidbody>();
            _myCollider = GetComponent<CapsuleCollider>();
        }


        private void FixedUpdate()
        {
            isGrounded = IsGrounded();

            PlayerMove();
            PlayerJump();
        }





        private bool IsGrounded()
        {
            return Physics.CheckSphere( transform.position + Vector3.up * sphereCheckPosition , _myCollider.radius , layerMaskGround );
        }



        public void OnMovement( InputValue value )
        {
            _inputDirection = new Vector3( value.Get<Vector2>().x , 0 , value.Get<Vector2>().y );
        }




        private void PlayerMove()
        {

            _smoothInputMagnitude = new Vector3(
                Mathf.SmoothDamp( _smoothInputMagnitude.x , _inputDirection.x , ref _currentVelocityX , smoothTime ) ,
                0f ,
                Mathf.SmoothDamp( _smoothInputMagnitude.z , _inputDirection.z , ref _currentVelocityZ , smoothTime )
            );



            if(!isGrounded)
                _myRigidBody.MovePosition( transform.position + moveSpeedWhileAirborn * Time.deltaTime * _smoothInputMagnitude );

            _myRigidBody.MovePosition( transform.position + moveSpeed * Time.deltaTime * _smoothInputMagnitude );

        }



        public void OnJump( InputValue value )
        {
            print( "on jump" );
            isSpacePressed = value.isPressed;
        }


        private void PlayerJump()
        {
            Vector3 velocity = _myRigidBody.velocity;

            if( isSpacePressed && isGrounded )
                _myRigidBody.AddForce( Vector3.up *jumpHeight);
                //_myRigidBody.velocity = Vector3.up * Mathf.Sqrt( -2 * jumpHeight * Physics.gravity.y ) * Time.deltaTime;
            if( velocity.y < fallingVelocity )
                _myRigidBody.velocity += Vector3.up * jumpGravity; 

        }



    }
}