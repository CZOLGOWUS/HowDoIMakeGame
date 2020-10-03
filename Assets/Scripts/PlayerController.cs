using UnityEngine;
using UnityEngine.InputSystem;


namespace Movement
{
    public class PlayerController : MonoBehaviour
    {
        //Components 
        [Header( "Layer Mask" )]
        public LayerMask layerMaskGround;
        public Transform cameraTransform;


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
        public float fallingStartVelocity;
        public float fowardJumpFroce = 5f;

        private bool _isSpacePressed = false;
        private bool _isGrounded;
        [Space]

        [Header("WallRunJumping")]
        


        [Space]


        [Header( "Wall Running" )]
        public LayerMask wallMask;
        public float wallRunSpeed;
        [Range( 0 , 1 )]
        public float wallRunStartAngle;
        public float wallrunFallingAngleChange;
        [Range( 0 , 15 )]
        public int wallRunTimeToStartFalling;
        public float wallRunTimeLength;



        private float _wallRunTime = 0f;
        private bool _isPlayerWallRuning = false;
        private RaycastHit _hitRight;
        private RaycastHit _hitLeft;
        private RaycastHit _hitWall;
        private Vector3 _directionOfWallRun;



        private bool _didPlayerWallRunJump = false;
        private float _timeSinceWallJump;
        private float _timeToEnableNextWallRunJump = 0.2f;
        private bool _isAbleToWallJump = true;


        // Start is called before the first frame update
        void Start()
        {
            _myRigidBody = GetComponent<Rigidbody>();
            _myCollider = GetComponent<CapsuleCollider>();
        }



        private void FixedUpdate()
        {
            _isGrounded = IsGrounded();


            PlayerMove( _isGrounded , _myRigidBody.velocity );
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
            if( isGrounded)
            {
                float _smoothTime;


                #region commented addForce Method
                /*
                if( Mathf.Abs(velocity.x) > maxMoveSpeed ) velocity.x = Mathf.Clamp(velocity.x,-maxMoveSpeed,maxMoveSpeed);
                if(Mathf.Abs(velocity.z) > maxMoveSpeed ) velocity.z = Mathf.Clamp(velocity.z,-maxMoveSpeed,maxMoveSpeed);
                _myRigidBody.velocity = velocity;

                _myRigidBody.AddForce( transform.forward * _inputDirection.z * moveSpeed * Time.deltaTime );
                _myRigidBody.AddForce( transform.right * _inputDirection.x * moveSpeed * Time.deltaTime );
                */
                #endregion

                //the one im using
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
            else
            {
                _myRigidBody.velocity +=
                moveSpeed * 0.05f * Time.fixedDeltaTime *
                (transform.forward * _inputDirection.normalized.z + transform.right * _inputDirection.normalized.x);
               
            }

        }



        public void OnJump( InputValue value )
        {
            _isSpacePressed = value.isPressed;
        }




        private void PlayerJump()
        {

            

            if( _isSpacePressed && _isGrounded )
            {
                //_myRigidBody.AddForce( Vector3.up * jumpHeight + transform.forward * fowardJumpFroce);
                _myRigidBody.AddForce( Vector3.up * Mathf.Sqrt( -2 * jumpHeight * Physics.gravity.y ) , ForceMode.Impulse );

                if( _inputDirection.z > 0f )
                    _myRigidBody.AddForce( transform.forward * fowardJumpFroce , ForceMode.Impulse );
            }
            else if(!_isPlayerWallRuning)
            {
                _myRigidBody.AddForce( Vector3.up * -9.81f , ForceMode.Force );
                
            }


            if( _myRigidBody.velocity.y < fallingStartVelocity && !_isSpacePressed )
                _myRigidBody.velocity += Vector3.up * jumpGravity;

        }



        void PlayerWallRun()
        {
            _timeSinceWallJump += Time.deltaTime;


            if( CanStartWallRuning() )
            {

                
                
                    if( _timeSinceWallJump > _timeToEnableNextWallRunJump )
                        if( _isSpacePressed )
                        {
                        
                                _timeSinceWallJump = 0f;

                                
                            #region directional walljumping
                            
                            if( _inputDirection.x > 0 )
                                {
                                    Vector3 walljumpvector = (transform.forward + transform.right);
                                    walljumpvector.y += 0.5f;
                                    _myRigidBody.velocity = ( walljumpvector.normalized * 9f );
                                }
                                else if( _inputDirection.x < 0 )
                                {
                                    Vector3 walljumpvector = (transform.forward - transform.right);
                                    walljumpvector.y += 0.5f;
                                    _myRigidBody.velocity = ( walljumpvector.normalized * 9f );
                                }
                            
                            #endregion





                            return;

                        }else
                        {
                            print( "iswalruning" );
                            _wallRunTime += Time.deltaTime;
                            RaycastHit hitWall = GetWallToWallRun( _hitRight , _hitLeft );  // <- chosese betwen hitLeft/HitRight
                            WallRun( hitWall );

                            return;
                        }


                

            } 
            else
            {

                _wallRunTime = 0f;
            }

        }



        private void WallRun( RaycastHit hitWall )
        {

            if( _wallRunTime < wallRunTimeLength )
            {
                _directionOfWallRun = GetTheDirectionOfWallRun( _hitWall );


                //rotate the direction of wall run vector Y angle and then multiplie the change in angle by _wallRunTime soo it will cahnge faster at the end
                _directionOfWallRun = new Vector3( _directionOfWallRun.x , _directionOfWallRun.y - Mathf.Pow( _wallRunTime , wallRunTimeToStartFalling ) * wallrunFallingAngleChange , _directionOfWallRun.z );


                //wallrun motion
                if( !(_inputDirection.z < 0) )
                    _myRigidBody.velocity = _directionOfWallRun * wallRunSpeed;
                else
                {
                    _myRigidBody.velocity = new Vector3( Mathf.Lerp(_myRigidBody.velocity.x , 0 , 0.1f) , _myRigidBody.velocity.y , Mathf.Lerp( _myRigidBody.velocity.z , 0 , 0.1f )  );
                    
                }

                //stick to wall
                    _myRigidBody.AddForce( -_hitWall.normal * 2f );

                if( _inputDirection.z < 0 )
                    _myRigidBody.AddForce( -transform.forward * wallRunSpeed );


            }

        }





        private bool CanStartWallRuning()
        {

            if( !_isGrounded && !_didPlayerWallRunJump && IsNearWall( ref _hitRight , ref _hitLeft ) /*&& Mathf.Sqrt(_myRigidBody.velocity.x * _myRigidBody.velocity.x + _myRigidBody.velocity.z * _myRigidBody.velocity.z ) > moveSpeed/3 && Mathf.Abs(_myRigidBody.velocity.y)  < 300f*/ )
            {
                return _isPlayerWallRuning = true;
            }
            else
            {
                //_wallRunTime = 0f; // dzieki temu Player nie leci w dół po paru wallrunach , trza gdzie s indzie to dać
                return _isPlayerWallRuning = false;
            }

        }



        private bool IsNearWall( ref RaycastHit hitRight , ref RaycastHit hitLeft )
        {
            return 
                Physics.Raycast(
                transform.position + Vector3.up * transform.localScale.y * -0.25f ,
                transform.right * transform.localScale.x ,
                out _hitRight ,
                transform.localScale.x * 0.7f ,
                wallMask ) 

                || 

                Physics.Raycast(
                transform.position + Vector3.up * transform.localScale.y * -0.25f ,
                -transform.right * transform.localScale.x ,
                out _hitLeft , 
                transform.localScale.x * 0.7f ,
                wallMask );

        }


        RaycastHit GetWallToWallRun( RaycastHit hitRight , RaycastHit hitLeft )
        {

            
                if( hitRight.collider != null && hitLeft.collider == null )
                {
                    _hitWall = hitRight;

                }
                else if( hitRight.collider == null && hitLeft.collider != null )
                {
                    _hitWall = hitLeft;

                }
                else if( hitLeft.collider != null && hitRight.collider != null )
                {
                    _hitWall = hitLeft.distance > hitRight.distance ? hitRight : hitLeft;

                }
                //else ThrowExpetion( "no wall found" );

            return _hitWall;

        }


        private Vector3 GetTheDirectionOfWallRun(RaycastHit hitWall)
        {
            
            if( transform.right.x * hitWall.normal.normalized.x + transform.right.y * hitWall.normal.normalized.y + transform.right.z * hitWall.normal.normalized.z < 0 )
                _directionOfWallRun = Vector3.Cross( Vector3.up, hitWall.normal ).normalized;
            else
                _directionOfWallRun = -Vector3.Cross( Vector3.up , hitWall.normal ).normalized;


            return new Vector3( _directionOfWallRun.x , _directionOfWallRun.y + wallRunStartAngle , _directionOfWallRun.z ).normalized;

        }



        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay( transform.position + Vector3.up * -0.25f , transform.right *  transform.localScale.x  );
            Gizmos.DrawRay( transform.position + Vector3.up * -0.25f , -transform.right *  transform.localScale.x  );

            Gizmos.color = Color.green;
            Gizmos.DrawRay( transform.position , new Vector3(_directionOfWallRun.x , _directionOfWallRun.y , _directionOfWallRun.z  ) * 2f );


        }
    }
}