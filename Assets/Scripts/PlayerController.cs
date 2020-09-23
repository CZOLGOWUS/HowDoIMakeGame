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
        public float fowardJumpFroce = 5f;

        private bool isSpacePressed = false;
        private bool isGrounded;


        [Header( "Wall Running" )]
        public LayerMask wallMask;
        public float scaleMultiplayer;
        public float wallrunHight;
        public float wallrunLenght;
        public float wallRunSpeed;


        private float _wallRunTime;
        private bool _isWallRunning = false;
        private RaycastHit _hitRight;
        private RaycastHit _hitLeft;
        private Vector3 _directionOfWallRun;
        private Vector3 _WallRunAimPoint;


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


            PlayerMove( isGrounded , _myRigidBody.velocity );
            PlayerJump();
            PlayerWallRun();

        }




        private bool IsGrounded()
        {
            return Physics.CheckSphere( transform.position + Vector3.up * sphereCheckPosition , _myCollider.radius , layerMaskGround );
        }




        public void OnMovement( InputValue value )
        {
            _inputDirection = new Vector3( value.Get<Vector2>().x , 0 , value.Get<Vector2>().y );
        }



        private void PlayerMove( bool isGrounded , Vector3 velocity )
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
            (transform.forward * _smoothInputMagnitude.z + transform.right * _smoothInputMagnitude.x) +
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
            {
                //_myRigidBody.AddForce( Vector3.up * jumpHeight + transform.forward * fowardJumpFroce);
                _myRigidBody.AddForce( Vector3.up * Mathf.Sqrt( -2 * jumpHeight * Physics.gravity.y ) , ForceMode.Impulse );

                if( _inputDirection.z > 0f )
                    _myRigidBody.AddForce( transform.forward * fowardJumpFroce , ForceMode.Impulse );
            }
            else
            {
                _myRigidBody.AddForce( Vector3.up * -9.81f , ForceMode.Force );
            }
            if( velocity.y < fallingStartVelocity && !isSpacePressed )
                _myRigidBody.velocity += Vector3.up * jumpGravity;

        }


        private WallRunData CalculateWallRunData(Vector3 _directionOfWallRun)
        {
            
            _WallRunAimPoint = new Vector3( _directionOfWallRun.x , _directionOfWallRun.y * wallrunHight , _directionOfWallRun.z ) * wallrunLenght;

            float _displacementY = _WallRunAimPoint.y - transform.position.y;
            Vector3 _displacementXZ = new Vector3( _WallRunAimPoint.x - transform.position.x , 0f , _WallRunAimPoint.z - transform.position.z );

            float time = Mathf.Sqrt( -2 * wallrunHight / Physics.gravity.y ) + Mathf.Sqrt( 2 * (_displacementY - wallrunHight) / Physics.gravity.y );

            Vector3 velocityY = Vector3.up * Mathf.Sqrt( -2 * Physics.gravity.y * wallrunHight );
            Vector3 velocityXZ = _displacementXZ / time;

            return new WallRunData( velocityXZ + velocityY * -Mathf.Sign( Physics.gravity.y ) , time );
        }



        private void PlayerWallRun()
        {
            _directionOfWallRun = GetTheDirectionOfWallRun();

            //do the wall run
            if(_isWallRunning)
            {
                _wallRunTime += Time.deltaTime;

                if(_wallRunTime < 1.5f)
                {
                    _myRigidBody.velocity = _directionOfWallRun * wallRunSpeed;
                    _myRigidBody.velocity += Vector3.Cross( Vector3.up , _directionOfWallRun ).normalized * 2f;
                }

                
                //_myRigidBody.velocity = CalculateWallRunData(_directionOfWallRun).initialVelocity;
                /*
                _myRigidBody.velocity = new Vector3(
                    _directionOfWallRun.x + _myRigidBody.velocity.x,
                    _myRigidBody.velocity.y + wallrunAngle - Mathf.Pow(_wallRunTime,3/2),
                    _directionOfWallRun.z + _myRigidBody.velocity.z );
                */

            }
            else
            {
                _wallRunTime = 0;
                //_myRigidBody.velocity += Vector3.up * -9.81f;
            }
            
        }




        private bool IsNearRightWall()
        {

            return Physics.Raycast( transform.position + Vector3.up * -0.25f , transform.right * transform.localScale.x , out _hitRight , transform.localScale.x , wallMask );

        }
        private bool IsNearLeftWall()
        {
            return Physics.Raycast( transform.position + Vector3.up * -0.25f , -transform.right * transform.localScale.x , out _hitLeft , transform.localScale.x , wallMask );

        }


        private Vector3 GetTheDirectionOfWallRun()
        {
            //if(!isGrounded && (IsNearRightWall() || IsNearLeftWall()) )
            //{
            //    _directionOfWallRun = _hitLeft.distance > _hitRight.distance ? Vector3.Cross( _hitRight.normal , Vector3.up ) : Vector3.Cross( _hitLeft.normal , Vector3.up );
            //}

            Vector3 _directionOfWallRun = Vector3.zero;

            if( !isGrounded && IsNearLeftWall() )
            {
                _isWallRunning = true;
                _directionOfWallRun = Vector3.Cross( _hitLeft.normal , Vector3.up );
            }

            if( !isGrounded && IsNearRightWall() )
            {
                _isWallRunning = true;
                _directionOfWallRun = Vector3.Cross( Vector3.up , _hitRight.normal );
            }
            
            if( _hitLeft.collider != null && _hitRight.collider != null )
                return _directionOfWallRun = _hitLeft.distance > _hitRight.distance ? Vector3.Cross( _hitRight.normal , Vector3.up ).normalized : Vector3.Cross( _hitLeft.normal , Vector3.up ).normalized;
            else if(isGrounded)
                _isWallRunning = false;


            return _directionOfWallRun.normalized;

        }



        private struct WallRunData
        {
            public readonly Vector3 initialVelocity;
            public readonly float timeToTarget;

            public WallRunData( Vector3 initialVelocity , float timeToTarget )
            {
                this.initialVelocity = initialVelocity;
                this.timeToTarget = timeToTarget;
            }
        }



        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay( transform.position + Vector3.up * -0.25f , transform.right * transform.localScale.x );
            Gizmos.DrawRay( transform.position + Vector3.up * -0.25f , -transform.right * transform.localScale.x );

            Gizmos.color = Color.green;
            Gizmos.DrawRay( transform.position , new Vector3(_directionOfWallRun.x , _directionOfWallRun.y + wallrunHight , _directionOfWallRun.z  ) * 2f );


        }
    }
}