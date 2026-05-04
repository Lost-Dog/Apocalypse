// ================================================================================================
// File: UnityControllerWrapper.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-07-2019: Created this file.
// 
//  All rights for the unmodified portions (original UnityControllerWrapper.cs from Motion Matching for Unity asset)
// are reserved by their owner (Kenneth Claassen).
//
//     Contains a part of the 'MxMGameplay' namespace for 'Unity Engine'.
// ================================================================================================
using MxM;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace MxMGameplay
{
    //============================================================================================
    /**
    *  @brief Wrapper for unity's capsule controller. Allows generic interface with MxM
    *         
    *********************************************************************************************/
    /// <summary>
    /// MMLC component responsible for making all direct modifications to the character's kinematics (position, rotation, velocity, acceleration).
    /// </summary>
    [DefaultExecutionOrder(-1)]
    //[RequireComponent(typeof(CharacterController))]
    public class NGCharacterControllerWrapper : GenericControllerWrapper
    {

        private bool currentlyGrounded = true;

        //[Tooltip("This event will be fired whenever the player lands.")]
        public class LandingEvent : UnityEvent<float> { }

        [SerializeField]
        private bool m_applyGravityWhenGrounded = false;

        [SerializeField]
        private LayerMask groundLayers;
        [SerializeField]
        private float groundOffset = 0.2f;
        [SerializeField]
        private float groundCheckDistance = 0.2f;

        private CharacterController m_characterController;

        [Tooltip("Whether gravity is enabled.  Not typically used directly.")]
        [SerializeField]
        public bool gravityEnabled = true;

        public bool debugMode = false;

        public bool zeroizeMoveAndRotateWhenMxMPaused = true;

        [Tooltip("Whether to automatically force character grounded state to true if character is not moving in Y for the supplied time duration.")]
        public bool automaticForceGroundedWhenCharacterYUnchanged = false;
        public float automaticForceGroundedDuration = 1.0f;

        [Tooltip("Whether jump force (really, this is vertical speed change that propels the character upward) is to be applied by this controller wrapper (false) or is handled external to MMLC (true).")]
        public bool jumpForceHandledExternally = false;

        public bool isJumpingOrFalling = false;

        // MBU: added flag to indicate rigidbody should be used
        public bool useRigidbody;
        // MBU: added flag to indicate external ground check should be used
        public bool useExternalGroundCheck;
        // MBU: added reference to the rigidbody and capsule collider
        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;
        
        public LayerMask GroundLayers
        {
            get { return groundLayers; }
            set { groundLayers = value;  }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void Reset()
        {
            if (useRigidbody)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                CapsuleCollider caps = GetComponent<CapsuleCollider>();

                if (rb == null)
                {
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }
                if (caps == null) 
                { 
                    _capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                }
            }
            else
            {
                CharacterController cctrl = GetComponent<CharacterController>();

                if (cctrl == null)
                {
                    m_characterController = gameObject.AddComponent<CharacterController>();
                }
            }
        }


        public void ConfigureObject(bool cfgApplyGravWhenGrounded, float cfgGroundOffset, float cfgGroundCheckDist, bool cfgGravityEnabled, LayerMask cfgGroundLayers)
        {
            m_applyGravityWhenGrounded = cfgApplyGravWhenGrounded;
            groundOffset = cfgGroundOffset;
            groundCheckDistance = cfgGroundCheckDist;
            gravityEnabled = cfgGravityEnabled;
            groundLayers = cfgGroundLayers;
        }

        private Vector3 wantedMoveDelta = Vector3.zero;
        private Quaternion wantedRotDelta = Quaternion.identity;

        private MxMAnimator mxmAnimator;

        private bool m_enableCollision = true;

        public bool EnabledCollision
        {
            get { return m_enableCollision; }
            set { m_enableCollision = value; }
        }

        private float verticalSpeed = 0f;
        private float blockNonJumpFlyupDuration = 0.5f;
        private float blockNonJumpFlyupTime = -1f;

        private float forcedGroundLastYChangeTime = -1;
        private float forcedGroundLastY = -1;


        public override bool IsGrounded { get { return currentlyGrounded; } }
        public override bool ApplyGravityWhenGrounded { get { return m_applyGravityWhenGrounded; } }

        public override Vector3 Velocity
        {
            get
            {
                return useRigidbody ? _rigidbody.linearVelocity : m_characterController.velocity;
            }
        }

        public override float MaxStepHeight
        {
            get
            {
                if (useRigidbody || m_characterController == null)
                {
                    return 0;
                }
                return m_characterController.stepOffset;
            }
            set
            {
                if (!useRigidbody &&  m_characterController == null)
                {
                     return;
                }

                m_characterController.stepOffset = value;
            }
        }
        public override float Height
        {
            get
            {
                if (useRigidbody && _capsuleCollider != null)
                {
                    return _capsuleCollider.height;
                }
                return m_characterController.height;;
            }
            set
            {
                if (useRigidbody)
                {
                    if (_capsuleCollider != null)
                    {
                        _capsuleCollider.height = value;
                        return;
                    }
                }
                m_characterController.height = value;
            }
        }

        public override Vector3 Center
        {
            get
            {
                if (useRigidbody && _capsuleCollider != null)
                {
                    return _capsuleCollider.center;
                }
                return m_characterController.center;;
            }
            set
            {
                if (useRigidbody)
                {
                    if (_capsuleCollider != null)
                    {
                        _capsuleCollider.center = value;
                        return;
                    }
                }
                m_characterController.center = value;
            }
        }
        public override float Radius
        {
            get
            {
                if (useRigidbody && _capsuleCollider != null)
                {
                    return _capsuleCollider.radius;
                }
                return m_characterController.radius;;
            }
            set
            {
                if (useRigidbody)
                {
                    if (_capsuleCollider != null)
                    {
                        _capsuleCollider.radius = value;
                        return;
                    }
                }
                m_characterController.radius = value;
            }
        }

        private void Start()
        {
            //mxmAnimator = m_characterController.GetComponentInChildren<MxMAnimator>();
            Initialize();
        }
        public override void Initialize()
        {
            MxMAnimator[] anims = GetComponentsInChildren<MxMAnimator>();
            foreach (MxMAnimator anim in anims)
            {
                if (anim.isActiveAndEnabled)
                {
                    mxmAnimator = anim;
                    break;
                }
            }
            
        }

        public override Vector3 GetCachedMoveDelta() { return Vector3.zero; }
        public override Quaternion GetCachedRotDelta() { return Quaternion.identity; }

        //Gets or sets whether collision is enabled (GenericControllerWrapper override)
        public override bool CollisionEnabled
        {
            get { return m_enableCollision; }
            set
            {
                if (!useRigidbody)
                {
                    if (m_enableCollision != value)
                    {
                        if (m_enableCollision)
                            m_characterController.enabled = false;
                        else
                            m_characterController.enabled = true;
                    }
                }

                m_enableCollision = value;
            }
        }

        public void Jump(float jumpHeight)
        {
            // height to speed equation from Unity TPC:
            if (!jumpForceHandledExternally)
            {
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
            blockNonJumpFlyupTime = Time.time; // -1f;
        }

        private void LateUpdate()
        {
            if (currentlyGrounded && (verticalSpeed < 0f))
            {
                verticalSpeed = -2f;
            }

            if (gravityEnabled)
            {
                float fallMult = verticalSpeed < 0 ? 1.5f : 1.0f;

                verticalSpeed += Physics.gravity.y * fallMult * Time.deltaTime;
            }
            else
            {
                verticalSpeed = 0;
                if (!currentlyGrounded && isJumpingOrFalling)
                {
                    wantedMoveDelta.y = 0;
                }
            }
            
            // MBU: option to use external ground check
            if (!useExternalGroundCheck)
            {
                currentlyGrounded = GroundedCheck();
            }

            // MBU: only add downwards velocity if not using rigidbody
            if (!useRigidbody)
            {
                wantedMoveDelta.y += verticalSpeed * Time.deltaTime;
            }

            this.ActualMoveAndRotate(wantedMoveDelta, wantedRotDelta);
            wantedMoveDelta = Vector3.zero;
            wantedRotDelta = Quaternion.identity;

        }

        private bool GroundedCheck()
        {
            RaycastHit[] hits;

            if (automaticForceGroundedWhenCharacterYUnchanged)
            {
                if ((Time.time - forcedGroundLastYChangeTime) > automaticForceGroundedDuration)
                {
                    return true;
                }
            }

            //TODO: change to a simple CheckSphere
            hits = Physics.SphereCastAll(new Vector3(transform.position.x, transform.position.y + groundOffset, transform.position.z), /*groundOffset*/ 0.1f, Vector3.down, groundCheckDistance + 0.1f, groundLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
                {
                    if (debugMode)
                    {
                        Debug.LogFormat("DEBUG: GroundCheck cast hit: currentlyGrounded( {0} ): {1} vs me {2}", currentlyGrounded, hit.collider.name, name);
                    }
                    // hit something other than player
                    return true;
                }
            }
            if (debugMode)
            {
                Debug.Log("DEBUG: GroundCheck cast miss");
            }
            return false;
        }

        //============================================================================================
        /**
        *  @brief Sets up the wrapper, getting a reference to an attached character controller
        *         
        *********************************************************************************************/
        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();

            if (!useRigidbody)
            {
                Assert.IsNotNull(m_characterController, "Error (UnityControllerWrapper): Could not find" +
                                                        "CharacterController component");

                m_characterController.enableOverlapRecovery = true;
            }

            if (useRigidbody)
            {
                _rigidbody = GetComponent<Rigidbody>();
                _capsuleCollider = GetComponent<CapsuleCollider>();
                if (m_characterController != null)
                {
                    m_characterController.enabled = false;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Moves the controller by an absolute Vector3 tranlation. Delta time must be applied
        *  before passing a_move to this function.
        *  
        *  @param [Vector3] a_move - the movement vector
        *         
        *********************************************************************************************/
        public override void Move(Vector3 a_move)
        {
            //Debug.LogFormat("Move called( {0} )", a_move.y);
            wantedMoveDelta += a_move;
        }

        public bool CurrentlyAgainstAPlatform(out Vector3 platformNormal)
        {
            if (mxmAnimator.IsEventPlaying)
            {
                platformNormal = Vector3.zero;
                return false;
            }

            RaycastHit hit;

            // check collision
            if (Physics.Raycast(transform.position, transform.forward, out hit, m_characterController.radius + 0.1f, groundLayers))
            {
                // hit
                platformNormal = new Vector3(hit.normal.x, 0f, hit.normal.z);
                return true;
            }
            platformNormal = Vector3.zero;
            return false;
        }

        //============================================================================================
        /**
        *  @brief Moves the controller by an absolute Vector3 tranlation and sets the new rotation
        *  of the controller. Delta time must be applied before passing a_move to this function.
        *  
        *  @param [Vector3] a_move - the movement vector
        *  @param [Quaternion] a_newRotation - the new rotation for the character
        *         
        *********************************************************************************************/
        public override void MoveAndRotate(Vector3 a_move, Quaternion a_rotDelta)
        {
            //Debug.LogFormat("MoveAndRotate called( {0} )", a_move.y);
            /*m_characterController.transform.rotation *= a_rotDelta;

            if (m_enableCollision)
            {
                m_characterController.Move(a_move);
            }
            else
            {
                m_characterController.transform.Translate(a_move, Space.World);
            }*/
            wantedRotDelta *= a_rotDelta;
            wantedMoveDelta += a_move;
        }

        public void ActualMoveAndRotate(Vector3 a_move, Quaternion a_rotDelta)
        {

            if (zeroizeMoveAndRotateWhenMxMPaused && mxmAnimator.IsPaused)
            {
                //Debug.Log("bailing on isPaused");
                return;
            }

            if (debugMode)
            {
                if (a_move.sqrMagnitude > 0f)
                {
                    Debug.Log("AMR called with nonzero a_move");
                }
            }

            transform.rotation *= a_rotDelta;

            if (useRigidbody)
            {
                if (a_move.magnitude > 0.001f) // prevents jittery physics
                {
                    _rigidbody.MovePosition(transform.position + a_move);
                }
            }
            else
            {
                if (m_enableCollision)
                {
                    m_characterController.Move(a_move);
                }
                else
                {
                    //Debug.LogFormat("Attempting to translate( {0})", )
                    m_characterController.transform.Translate(a_move, Space.World);
                }
                
                if (automaticForceGroundedWhenCharacterYUnchanged)
                {
                    if (forcedGroundLastY != m_characterController.transform.position.y)
                    {
                        forcedGroundLastYChangeTime = Time.time;
                        forcedGroundLastY = m_characterController.transform.position.y;
                    }
                }
            }
        }


        //============================================================================================
        /**
        *  @brief Sets the rotation of the character contorller with interpolation if supported
        *  
        *  @param [Quaternion] a_newRotation - the new rotation for the character
        *         
        *********************************************************************************************/
        public override void Rotate(Quaternion a_rotDelta)
        {
            //m_characterController.transform.rotation *= a_rotDelta;
            wantedRotDelta *= a_rotDelta;
        }
        public void ActualRotate(Quaternion a_rotDelta)
        {
            m_characterController.transform.rotation *= a_rotDelta;
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnEnable function which simply passes the enabling status onto the
        *  controller compopnnet
        *         
        *********************************************************************************************/
        private void OnEnable()
        {
            // MBU: only enable character controller if not using rigidbody
            if (!useRigidbody)
            {
                m_characterController.enabled = true;
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the position of the character controller (teleport)
        *  
        *  @param [Vector3] a_position - the new position
        *         
        *********************************************************************************************/
        public override void SetPosition(Vector3 a_position)
        {
            transform.position = a_position;
        }

        //============================================================================================
        /**
        *  @brief Sets the rotation of the character controller (teleport)
        *         
        *  @param [Quaternion] a_rotation - the new rotation        
        *         
        *********************************************************************************************/
        public override void SetRotation(Quaternion a_rotation)
        {
            transform.rotation = a_rotation;
        }

        //============================================================================================
        /**
        *  @brief Sets the position and rotation of the character controller (teleport)
        *  
        *  @param [Vector3] a_position - the new position
        *  @param [Quaternion] a_rotation - the new rotation    
        *         
        *********************************************************************************************/
        public override void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation)
        {
            transform.SetPositionAndRotation(a_position, a_rotation);
        }

        public float GetCurrentGroundSpeed()
        {
            Vector3 horizontalVelocity = new Vector3(Velocity.x, 0, Velocity.z);

            return horizontalVelocity.magnitude;

        }

        public float GetHeightAboveGround(float maxDistanceToCheck)
        {
            RaycastHit hit;
            if (Physics.SphereCast(new Vector3(transform.position.x, transform.position.y + groundOffset, transform.position.z), groundOffset, Vector3.down, out hit, maxDistanceToCheck, groundLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.distance;
            }

            return -1;
        }

        internal void BlockNonJumpFlyup(float timeDuration)
        {
            blockNonJumpFlyupDuration = timeDuration;
            blockNonJumpFlyupTime = Time.time;
        }
        
        // MBU: added this method to allow external ground check (currentlyGrounded has no setter and is in code from MxM
        public void SetGrounded(bool value)
        {
            currentlyGrounded = value;
        }

    }//End of class: UnityControllerWrapper
}//End of namespace: MxMGameplay