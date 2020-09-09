using UnityEngine;
using UnityEngine.InputSystem;


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
        public float smoothTimeGrounded = 0.1f;
        public float maxMoveSpeed = 10f;


        private float _currentVelocityX; // for ref
        private float _currentVelocityZ; // for ref
        private Vector3 _smoothInputMagnitude = Vector3.zero;
        [Space]


        [Header( "Ground Check" )]
        public float sphereCheckPosition = -0.51f;
        [Space]


        [Header( "Jumping" )]
        public float jumpGravity = -9.81f;
        public float jumpHeight = 2f;
        public float smoothTimeAirborn = 0.8f;
        public float fallingStartVelocity = 0f;


        private bool isSpacePressed = false;
        private bool isGrounded;
        //private Vector3 _smoothInputMagnitudeWhileAirborn = Vector3.zero;







        // Start is called before the first frame update
        void Start()
        {
            _myRigidBody = GetComponent<Rigidbody>();
            _myCollider = GetComponent<CapsuleCollider>();
        }


        private void FixedUpdate()
        {
            isGrounded = IsGrounded();


            PlayerMove( isGrounded ,_myRigidBody.velocity);
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



        private void PlayerMove( bool isGrounded ,Vector3 velocity)
        {
            float _smoothTime;



            #region addForce Method
            /*
            if( Mathf.Abs(velocity.x) > maxMoveSpeed ) velocity.x = Mathf.Clamp(velocity.x,-maxMoveSpeed,maxMoveSpeed);
            if(Mathf.Abs(velocity.z) > maxMoveSpeed ) velocity.z = Mathf.Clamp(velocity.z,-maxMoveSpeed,maxMoveSpeed);
            _myRigidBody.velocity = velocity;

            _myRigidBody.AddForce( transform.forward * _inputDirection.z * moveSpeed * Time.deltaTime );
            _myRigidBody.AddForce( transform.right * _inputDirection.x * moveSpeed * Time.deltaTime );
            */
            #endregion

            #region velocity change method


            if( isGrounded )
                _smoothTime = smoothTimeGrounded;
            else
                _smoothTime = smoothTimeAirborn;
                

                _smoothInputMagnitude = new Vector3(
                    Mathf.SmoothDamp( _smoothInputMagnitude.x , _inputDirection.x , ref _currentVelocityX , _smoothTime ) ,
                    0f ,//doesnt matter
                    Mathf.SmoothDamp( _smoothInputMagnitude.z , _inputDirection.z , ref _currentVelocityZ , _smoothTime )
                );

                _myRigidBody.velocity = 
                moveSpeed * Time.fixedDeltaTime * 
                ( transform.forward  * _smoothInputMagnitude.z + transform.right* _smoothInputMagnitude.x) + 
                Vector3.up * _myRigidBody.velocity.y;


            #endregion
            #region test
            /*
            _myRigidBody.velocity = new Vector3
                (
                Mathf.Clamp( _myRigidBody.velocity.x , -maxVelocity , maxVelocity ) ,
                _myRigidBody.velocity.y ,
                Mathf.Clamp( _myRigidBody.velocity.z , -maxVelocity , maxVelocity )
                );


            if( _inputDirection.magnitude > 0f )
                _inputDirectionLastPressed = _inputDirection;

            _myRigidBody.AddForce( new Vector3 (
                Mathf.Clamp(moveSpeed * Time.deltaTime * _inputDirection.x ),
                0f,
                moveSpeed * Time.deltaTime * _inputDirection.z
                );
            */

            #endregion

        }



        public void OnJump( InputValue value )
        {
            isSpacePressed = value.isPressed;
        }




        private void PlayerJump()
        {
            Vector3 velocity = _myRigidBody.velocity;

            if( isSpacePressed && isGrounded )
                _myRigidBody.AddForce( Vector3.up * jumpHeight );
            //_myRigidBody.velocity = Vector3.up * Mathf.Sqrt( -2 * jumpHeight * Physics.gravity.y ) * Time.deltaTime;
            if( velocity.y < fallingStartVelocity && !isSpacePressed )
                _myRigidBody.velocity += Vector3.up * jumpGravity;

        }



    }
}