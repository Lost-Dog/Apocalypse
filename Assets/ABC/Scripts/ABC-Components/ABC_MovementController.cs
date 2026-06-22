using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ABCToolkit {
    [System.Serializable]
    [RequireComponent(typeof(CharacterController))]
    public class ABC_MovementController : MonoBehaviour {


        // ********************* Settings ********************
        #region Settings


        /// <summary>
        /// What toolbar tab is selected in the StateManager inspector
        /// </summary>
        public int toolbarMovementManagerSelection;

        /// <summary>
        /// What tab is selected in the MovementManager general settings inspector
        /// </summary>
        public int toolbarMovementManagerGeneralSettingsSelection;


        /// <summary>
        /// What tab is selected in the MovementManager animation settings inspector
        /// </summary>
        public int toolbarMovementManagerAnimationSettingsSelection;

        /// <summary>
        /// What tab is selected in the MovementManager audio settings inspector
        /// </summary>
        public int toolbarMovementManagerAudioSettingsSelection;



        //[Header("Component Settings")]

        /// <summary>
        /// Determines if movement and rotation is allowed
        /// </summary>
        public bool allowMovement = true;

        /// <summary>
        /// Allows for a small delay when turning back on movement
        /// </summary>
        public float enableMovementDelay = 0f;

        /// <summary>
        /// Allows for a small delay when turning off movement
        /// </summary>
        public float disableMovementDelay = 0f;

        /// <summary>
        /// If true then the script will integrate with ABC receiving events when movement is prevented
        /// </summary>
        public bool ABCIntegration = true;


        /// <summary>
        /// If true then script will perform in FPS mode
        /// </summary>
        public bool FPSMode = false;

        //[Header("Movement")]

        /// <summary>
        /// Determines how fast the entity will move
        /// </summary>
        public float moveForce = 5f;

        /// <summary>
        /// If enabled then movement will use acceleration
        /// </summary>
        public bool useAcceleratedMovement = true;

        /// <summary>
        /// Amount of acceleration applied to movement
        /// </summary>
        public float moveAcceleration = 4f;

        /// <summary>
        /// If true then once max acceleration has been hit a new animation will be triggered
        /// </summary>
        public bool enableMaxAcceleratedAnimation = true;

        /// <summary>
        /// How long after max acceleration has been hit the accelerated animation will start 
        /// </summary>
        public float intervalFromMaxAccelerationToAnimation = 2.5f;

 
        /// <summary>
        /// If key is pressed then the entity will be switched to walking mode
        /// </summary>
        public KeyCode walkToggleKey = KeyCode.Comma;

        /// <summary>
        /// If button is pressed then the entity will be switched to walking mode
        /// </summary>
        public string walkToggleButton = "Walk";

        /// <summary>
        /// If true then walking can start from light input
        /// </summary>
        public bool toggleWalkFromLightInput = true;

        /// <summary>
        /// If true then walking can only be enabled in idle mode
        /// </summary>
        public bool walkInIdleOnly = true;

        /// <summary>
        /// Name of the fall animation to play
        /// </summary>
        public string walkAniParameter = "Walking";


#if ENABLE_INPUT_SYSTEM == false
        [HideInInspector]
#endif
        /// <summary>
        /// Axis name of move button
        /// </summary>
        public string moveButton = "Move";

        /// <summary>
        /// Determines how fast the entity will move when sprinting
        /// </summary>
        public float sprintForce = 7f;

        /// <summary>
        /// Axis name of sprint button
        /// </summary>
        public string sprintButton = "Sprint";

        /// <summary>
        /// Key input for sprinting
        /// </summary>
        public KeyCode sprintKey = KeyCode.LeftShift;

        /// <summary>
        /// If true then a button press will enable sprinting, which will disable when movement stops
        /// </summary>
        public bool enableSprintToggle = true; 

        /// <summary>
        /// If enabled then a seperate animation will play when jumping from a sprint
        /// </summary>
        public bool enableSprintJumping = false; 

        /// <summary>
        /// If true then an animation will play when going into a sprint
        /// </summary>
        public bool animateIntoSprint = false; 

        /// <summary>
        /// The animation clip to play when animating into sprint 
        /// </summary>
        public ABC_AnimationClipReference animateIntoSprintClip;

        /// <summary>
        /// Speed of the animate into sprint clip
        /// </summary>
        public float animateIntoSprintClipSpeed = 1f;

        /// <summary>
        /// Will play an animation after sprinting
        /// </summary>
        public bool animateAfterSprint = false;

        /// <summary>
        /// Animation to play after sprinting
        /// </summary>
        public ABC_AnimationClipReference animateAfterSprintClip;

        /// <summary>
        /// Speed of the animate after sprint clip
        /// </summary>
        public float animateAfterSprintClipSpeed = 1.5f;

        //S[Header("LockOn Movement")]

        /// <summary>
        /// The lock on target - once locked on the entity will always face the target
        /// and move sideways/backwards/forwards. If null the entity will as normal
        /// turn to face the direction of the movement button pressed in relation
        /// to the way the camera is facing. 
        /// </summary>
        public GameObject lockOnTarget;


        /// <summary>
        /// If true then lock on movement will be used 
        /// </summary>
        public bool useLockOnMovement = true;

        /// <summary>
        /// If configured the entity will temporarily not be locked on to the entity when the button is pressed
        /// </summary>
        public string tempLockOffButton = "Dodge";


        /// <summary>
        /// If configured the entity will temporarily not be locked on to the entity 
        /// </summary>
        public KeyCode tempLockOffKey = KeyCode.Mouse1;

        /// <summary>
        /// The amount of time to not be locked on to the entity before being locked back on
        /// </summary>
        public float tempLockOffDuration = 0.75f;

        //[Header("Crosshair Movement")]


        /// <summary>
        /// If true then crosshair movement can be enabled when the crosshair appears
        /// </summary>
        public bool enableCrosshairMovement = false; 

        /// <summary>
        /// Key to hold to activate crosshair movement
        /// </summary>
        public KeyCode crosshairModeKey = KeyCode.None;

        /// <summary>
        /// button to hold to activate crosshair movement
        /// </summary>
        public string crosshairModeButton = "Aim";

        /// <summary>
        /// Determines if crosshair movement can be used
        /// </summary>
        public bool useCrosshairAnimation = true;

        /// <summary>
        /// If configured the entity will temporarily not be in crosshair mode
        /// </summary>
        public KeyCode tempCrossOffKey = KeyCode.None;

        /// <summary>
        /// If configured the entity will temporarily not be in crosshair mode
        /// </summary>
        public string tempCrossOffButton = "Sprint";

        /// <summary>
        /// The amount of time to not be be in crosshair mode before it being turned back on
        /// </summary>
        public float tempCrossOffDuration = 0.75f;



        //[Header("Jump and Gravity")]

        /// <summary>
        /// If enabled then the entity can jump
        /// </summary>
        public bool allowJumping = false;

        /// <summary>
        /// If enabled then the entity can double jump 
        /// </summary>
        public bool allowDoubleJump = true;

        /// <summary>
        /// Axis name of jump button
        /// </summary>
        public string jumpButton = "Jump";

        /// <summary>
        /// Determines how far the entity will jump
        /// </summary>
        public float jumpForce = 12f;

        /// <summary>
        /// If enable then gravity will be applied to the entity
        /// </summary>
        public bool allowGravity = true;


        //[Header("Rotation")]
        /// <summary>
        /// If enabled then the entity is allowed to rotate
        /// </summary>
        public bool allowRotation = true;

        /// <summary>
        /// If true then rotation will be enabled/disabled along with movement toggle
        /// </summary>
        public bool rotationToggleWithMovement = false;

        /// <summary>
        /// Determiens the speed of rotation, the lower the number the longer it takes
        /// </summary>
        public float rotationMoveSpeed = 30f;

        /// <summary>
        /// Determines how much input is needed by the user before the rotation starts
        /// </summary>
        public float rotationDrag = 0f;

        /// <summary>
        /// Will determine how long after stopmovement was triggered that rotation can reoccur
        /// </summary>
        public float rotationStopMovementLeeway = 0.5f;

        /// <summary>
        /// If enabled then the entity will apply advanced rotation animations
        /// </summary>
        public bool enableAdvancedRotation = true; 

        /// <summary>
        /// AcceleratedRotationClip to play
        /// </summary>
        public ABC_AnimationClipReference acceleratedRotationClip;

        /// <summary>
        /// Mask to add to the animation
        /// </summary>
        public ABC_AvatarMaskReference acceleratedRotationClipMask;

        /// <summary>
        /// Speed of the rotation clip
        /// </summary>
        public float acceleratedRotationClipSpeed = 2f;

        /// <summary>
        /// The percentage at which the rotation animation will stop
        /// </summary>
        public float acceleratedRotationStopPercentage = 70f;

        /// <summary>
        /// advancedRotation180Clip to play
        /// </summary>
        public ABC_AnimationClipReference advancedRotation180Clip;

        /// <summary>
        /// Mask to add to the animation
        /// </summary>
        public ABC_AvatarMaskReference advancedRotation180ClipMask;


        /// <summary>
        /// Speed of the rotation clip
        /// </summary>
        public float advancedRotation180ClipSpeed = 2f;

        /// <summary>
        /// The percentage at which the rotation animation will stop
        /// </summary>
        public float advancedRotation180StopPercentage = 70f;

        /// <summary>
        /// advancedRotationRightClip to play
        /// </summary>
        public ABC_AnimationClipReference advancedRotationRightClip;


        /// <summary>
        /// Mask to add to the animation
        /// </summary>
        public ABC_AvatarMaskReference advancedRotationRightClipMask;


        /// <summary>
        /// Speed of the rotation clip
        /// </summary>
        public float advancedRotationRightClipSpeed = 2f;

        /// <summary>
        /// The percentage at which the rotation animation will stop
        /// </summary>
        public float advancedRotationRightStopPercentage = 70f;

        /// <summary>
        /// advancedRotationLeftClip to play
        /// </summary>
        public ABC_AnimationClipReference advancedRotationLeftClip;

        /// <summary>
        /// Mask to add to the animation
        /// </summary>
        public ABC_AvatarMaskReference advancedRotationLeftClipMask;


        /// <summary>
        /// Speed of the rotation clip
        /// </summary>
        public float advancedRotationLeftClipSpeed = 2f;

        /// <summary>
        /// The percentage at which the rotation animation will stop
        /// </summary>
        public float advancedRotationLeftStopPercentage = 70f;

        //[Header("Landing Animation")]

        /// <summary>
        /// If true then landing animation will play at different situations
        /// </summary>
        public bool enableLandingAnimations = true; 

        /// <summary>
        /// Animation clip to play when entity lands from a jump
        /// </summary>
        public ABC_AnimationClipReference landingClip;

        /// <summary>
        /// Speed of the landing clip
        /// </summary>
        public float landingClipSpeed = 1f;

        /// <summary>
        /// Animation clip to play when entity lands from a jump but movement input is being triggered
        /// </summary>
        public ABC_AnimationClipReference landingRunClip;

        /// <summary>
        /// Speed of the landing run clip
        /// </summary>
        public float landingRunClipSpeed = 1f;

        /// <summary>
        /// Animation clip to play when entity lands from falling for a long while
        /// </summary>
        public ABC_AnimationClipReference landingImpactClip;

        /// <summary>
        /// Animation clip to play when entity lands from falling for a long while
        /// </summary>
        public float landingImpactClipSpeed = 1f;

        /// <summary>
        /// If enabled then camera will shake on landing impact
        /// </summary>
        public bool landingImpactShakeCamera = true; 

        /// <summary>
        /// Delay before shake camera on initiation starts
        /// </summary>
        [Tooltip("Delay before shake camera on impact starts")]
        public float landingImpactShakeCameraDelay = 0.1f;

        /// <summary>
        /// Duration to shake camera for on initiation
        /// </summary>
        [Tooltip("Duration to shake camera for on impact")]
        public float landingImpactShakeCameraDuration = 0.2f;

        /// <summary>
        /// Amount to shake camera for on initiation
        /// </summary>
        [Tooltip("Amount to shake camera for on impact")]
        public float landingImpactShakeCameraAmount = 0.2f;

        /// <summary>
        /// The shake speed for the camera on initiation
        /// </summary>
        [Tooltip("The shake speed for the camera on impact")]
        public float landingImpactShakeCameraSpeed = 15;


        //[Header("Animation")]
        /// <summary>
        /// If enabled then motion is turned off allowing movement to come from animation and only rotation will occur
        /// </summary>
        public bool rootMotionMode = false;

        /// <summary>
        /// Name of the normal movement animation to play
        /// </summary>
        public string animationParameter = "Move";

        /// <summary>
        /// Name of the accelerated movement animation to play
        /// </summary>
        public string animationAcceleratedParameter = "MoveAccelerated";

        /// <summary>
        /// Name of the jump animation to play
        /// </summary>
        public string jumpAniParameter = "Jump";

        /// <summary>
        /// Name of the double jump animation to play
        /// </summary>
        public string doubleJumpAniParameter = "DoubleJump";

        /// <summary>
        /// Name of the fall animation to play
        /// </summary>
        public string fallAniParameter = "Fall";    


        /// <summary>
        /// Name of the side movement animation to play when in lockon movement mode
        /// </summary>
        public string lockOnSideAniParameter = "SideStep";

        /// <summary>
        /// Name of the front animation to play when in lockon movement mode
        /// </summary>
        public string lockOnForwardAniParameter = "ForwardStep";

        /// <summary>
        /// Name of the side movement animation to play when in crosshair movement mode
        /// </summary>
        public string crossHairSideAniParameter = "SideStep";

        /// <summary>
        /// Name of the front animation to play when in crosshair movement mode
        /// </summary>
        public string crossHairForwardAniParameter = "ForwardStep";

        //[Header("Graphic Settings")]

        /// <summary>
        /// Graphic which displays when animating into sprint
        /// </summary>
        public ABC_GameObjectReference animateIntoSprintGraphic;


        //[Header("Audio Settings")]


        /// <summary>
        /// If enabled then landing audio will play
        /// </summary>
        public bool enableJumpingAudio = true;

        /// <summary>
        /// The landing audio volume (0, 1)
        /// </summary>
        [Range(0, 1)]
        public float jumpingAudioVolume = 0.6f;

        /// <summary>
        /// landing clip to play
        /// </summary>
        public ABC_AudioClipReference jumpingAudio;


        /// <summary>
        /// If enabled then landing audio will play
        /// </summary>
        public bool enableLandingAudio = true;

        /// <summary>
        /// The landing audio volume (0, 1)
        /// </summary>
        [Range(0, 1)]
        public float landingAudioVolume = 0.2f;

        /// <summary>
        /// landing clip to play
        /// </summary>
        public ABC_AudioClipReference landingAudio;


        /// <summary>
        /// If enabled then movement audio will play
        /// </summary>
        public bool enableMovementAudio = true;

        /// <summary>
        /// The movement audio volume (0, 1)
        /// </summary>
        [Range(0, 1)]
        public float movementAudioVolume = 1f;

        /// <summary>
        /// Default movement clip to play when running
        /// </summary>
        public List<ABC_AudioClipReference> movementAudioDefault;

        /// <summary>
        /// Defines a list of audio to play when stepping on certain tags
        /// </summary>
        public List<MovementAudioTagMap> movementAudioTagMaps = new List<MovementAudioTagMap>();

        /// <summary>
        /// Structure which defines what audio to play depending on tag stepped on
        /// </summary>
        [System.Serializable]
        public struct MovementAudioTagMap {

            /// <summary>
            /// The tag stepped on 
            /// </summary>
            public string StepTag;

            /// <summary>
            /// The audio to play for the step tag
            /// </summary>
            public List<ABC_AudioClipReference> StepAudio;

        }

        /// <summary>
        /// Determines if Parkour is enabled
        /// </summary>
        public bool enableParkour = true;

        /// <summary>
        /// If enabled then debug lines will show when doing parkour
        /// </summary>
        public bool enableParkourDebug = false;

        /// <summary>
        /// If true then a trigger is required to activate parkour
        /// </summary>
        public bool parkourRequireInputTrigger = true; 

        /// <summary>
        /// type of input to activate parkour
        /// </summary>
        [Tooltip("type of input for jumping to scroll ability")]
        public InputType parkourInputType = InputType.Key;

        /// <summary>
        /// The Button Name to activate parkour
        /// </summary>
        [Tooltip("The Button Name to jump to the ability")]
        public string parkourInputButton;

        /// <summary>
        /// Key to activate parkour
        /// </summary>
        [Tooltip("Key to quickly jump to the ability")]
        public KeyCode parkourInputKey = KeyCode.LeftShift;

        /// <summary>
        /// List of vault parkour animations
        /// </summary>
        public List<ParkourMovement> parkourVaultAnimations;

        /// <summary>
        /// List of vault parkour animations
        /// </summary>
        public List<ParkourMovement> parkourSlideAnimations;


        /// <summary>
        /// List of vault parkour animations
        /// </summary>
        public List<ParkourMovement> parkourLiftUpAnimations;

        /// <summary>
        /// Structure which defines what parkour animation to play
        /// </summary>
        [System.Serializable]
        public class ParkourMovement {

            /// <summary>
            /// Tag for activating specific Tag
            /// </summary>
            public string parkourTag = ""; 

            /// <summary>
            /// Determines type of parkour
            /// </summary>
            public ParkourType parkourType;

            /// <summary>
            /// Parkour Animation to Run
            /// </summary>
            public ABC_AnimationClipReference parkourAnimation;

            /// <summary>
            /// Speed of the animation
            /// </summary>
            public float parkourAnimationSpeed;

            /// <summary>
            /// Determines what % the animation should be at the vital parkour point
            /// </summary>
            /// <remarks>For climbing up it's when the entity grabs the ledge or for vaulting over its when the entity hand hits the middle of the fence etc</remarks>
            [Range(0, 100)]
            public float parkourAnimationVitalPercentagePoint;

            /// <summary>
            /// Offset to apply to the vital point
            /// </summary>
            public float parkourVitalPointHeightOffset;

            /// <summary>
            /// Will enable root motion at x %
            /// </summary>
            public bool enableRootMotion;

            /// <summary>
            /// Percentage to enable root motion
            /// </summary>
            public float enableRootMotionPercentage;

        }


        #endregion


        // ********************* Private Properties ********************

        #region Private Properties


        /// <summary>
        /// Value which indicates how much vertical velocity is applied to the motion
        /// </summary>
        private float _verticalVelocity;

        /// <summary>
        /// Property which will work out the vertical velocity value to apply to the motion depending on if the user is grounded, jumping or falling
        /// </summary>
        private float VerticalVelocity {
            get {


                //If the entity is grounded or doing a double jump

                if (this.doubleJumpState == DoubleJumpState.IsDoubleJumping && this.allowJumping == true && this.allowMovement == true) {

                    //If vertical velocity is still over 80% of the jump, then do a double jump using jump force + 40% of jump force, else do jump force + 120% of jump force
                    _verticalVelocity = _verticalVelocity >= (_verticalVelocity * 0.80f) ? this.jumpForce + (this.jumpForce * 0.40f) : this.jumpForce + (this.jumpForce * 1.20f);
                    this.doubleJumpState = DoubleJumpState.DoubleJumped;

                } else if (this.IsGrounded()) {

                    if (this.isJumping && this.allowJumping == true && this.allowMovement == true) {  // If jump key has been pressed to change vertical velocity to the jump force defined                 
                        _verticalVelocity = this.jumpForce;
                    } else {
                        _verticalVelocity = 0; // else nothing is happening and the entity is grounded so no vertical velocity is applied
                    }

                } else {

                    //If gravity is disabled then don't apply a gravity force
                    if (this.allowGravity == true)
                        _verticalVelocity += Physics.gravity.y * Time.deltaTime; // else If the entity is not grounded and allow gravity is enabled then apply the gravity value to the vertical velocity
                    else
                        _verticalVelocity = 0;
                }

                return _verticalVelocity;
            }
        }

        /// <summary>
        /// Records the current input X amount (Horizontal Axis)
        /// </summary>
        float inputX;

        /// <summary>
        /// Records the current Z amount (Vertical Axis)
        /// </summary>
        float inputZ;

        /// <summary>
        /// Records when movement was last disabled
        /// </summary>
        float timeMovementLastDisabled = 0f;

        /// <summary>
        /// adjustment to the move speed
        /// </summary>
        [Tooltip("adjustment to the move speed")]
        private float moveForceAdjustment = 0f;

        /// <summary>
        /// Adjustment to the move speed from the weapon equipped
        /// </summary>
        [Tooltip("Adjustment to the move speed from the weapon equipped")]
        private float moveForceWeaponAdjustment = 0f;

        /// <summary>
        /// Records what tag the entity is currently interacting with 
        /// </summary>
        private string CurrentStepTag = "";


        /// <summary>
        /// Pool that holds graphics for movement controller
        /// </summary>
        private Dictionary<string, List<GameObject>> graphicPools = new Dictionary<string, List<GameObject>>();


        #endregion



        // ********************* Variables ********************
        #region Variables

        /// <summary>
        ///Global variable which indicates how much input force was applied by the user
        /// </summary>
        private float inputForce;

        /// <summary>
        /// Variable which tracks the current speed
        /// </summary>
        private float speedForce;

        /// <summary>
        /// Tracks if acceleration rotation occured
        /// </summary>
        private bool acceleratedRotated = false;

        /// <summary>
        /// Records when entity last advanced a rotation
        /// </summary>
        private float lastAdvanceRotatation = 0f;

        /// <summary>
        /// the time when max acceleration was reached
        /// </summary>
        private float maxAccelerationTimeReached = 0;

        /// <summary>
        /// Will track if the entity is walking
        /// </summary>
        private bool isWalking = false;

        /// <summary>
        /// Will track if walking is toggled
        /// </summary>
        private bool walkingToggled = false;

        /// <summary>
        /// Will track if entity is sprinting
        /// </summary>
        private bool isSprinting = true;

        /// <summary>
        /// Will track time when sprint was last disabled
        /// </summary>
        private float lastSprintDisabledTime = 0f;

        /// <summary>
        /// Tracks if the entity is jumping
        /// </summary>
        private bool isJumping = false;

        /// <summary>
        /// Will track the double jump state 
        /// </summary>
        private DoubleJumpState doubleJumpState = DoubleJumpState.DoubleJumpReady;

        /// <summary>
        /// Records when a jump last happened 
        /// </summary>
        private float lastJumpTime = 0f;

        /// <summary>
        /// Records when entity reaches land after a jump 
        /// </summary>
        private float lastLandTime = 0f;

        /// <summary>
        /// Records when entity last sprinted
        /// </summary>
        private float lastSprintDashTime = 0f;

        /// <summary>
        /// records when entity last stopped moving
        /// </summary>
        private float LastMovementTime = 0f;

        /// <summary>
        /// Determines if entity is parkouring
        /// </summary>
        private bool isParkouring = false;

        /// <summary>
        /// Will track the latest parkour obstacle object ray hit
        /// </summary>
        private RaycastHit parkourObjectRayHit;

        /// <summary>
        /// Will track the latest parkour obstacle object which the entity collided with
        /// </summary>
        private ABC_ParkourObstacle parkourObstacle = null;

        /// <summary>
        /// Global variable which tracks which way the entity needs to move calculated from user input
        /// </summary>
        private Vector3 moveDirection;

        /// <summary>
        /// Main Camera
        /// </summary>
        private Camera Cam;

        /// <summary>
        /// Character Controller component attached to entity
        /// </summary>
        private CharacterController charController;

        /// <summary>
        /// Entities transform
        /// </summary>
        private Transform meTransform;

        /// <summary>
        /// Animator attached to entity
        /// </summary>
        private Animator Ani;

        /// <summary>
        /// Tracks if the script is currently locked temporarily (used for dodging)
        /// </summary>
        private bool tempLocked = false;

        /// <summary>
        /// Records the ABC SM Component if used for events
        /// </summary>
        private ABC_StateManager ABCEventsSM;

        /// <summary>
        /// Records the ABC controller component for events
        /// </summary>
        private ABC_Controller ABCEventsCont;

        /// <summary>
        /// Records the ABC Entity
        /// </summary>
        private ABC_IEntity ABCEntity;

        #endregion


        // ********************* Private Methods ********************
        #region Private Methods

        /// <summary>
        /// Will clear all graphic objects for the component
        /// </summary>
        public void ClearObjectPools() {

            //Look through each weapon graphic pooling the object
            foreach (List<GameObject> graphicList in this.graphicPools.Values) {

                foreach (GameObject graphic in graphicList)
                    Destroy(graphic);
            }


            this.graphicPools.Clear();

        }


        /// <summary>
        /// Will create a graphic pool depending on parameter given
        /// </summary>
        /// <param name="GraphicType">The type of pool to create</param>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateObjectPool(GraphicType GraphicType, bool CreateOne = false) {

            GameObject graphicObj = null;

            switch (GraphicType) {
                case GraphicType.AnimateIntoSprint:

                    if (this.animateIntoSprint == false || this.animateIntoSprintGraphic.GameObject == null)
                        return null;

                    //how many objects to make
                    int objCount = CreateOne ? 1 : 3;

                    for (int i = 0; i < objCount; i++) {
                        // create object particle 
                        graphicObj = (GameObject)(GameObject.Instantiate(this.animateIntoSprintGraphic.GameObject));
                        graphicObj.name = this.animateIntoSprintGraphic.GameObject.name;

                        //disable and pool the object 
                        ABC_Utilities.PoolObject(graphicObj);

                        if (this.graphicPools.ContainsKey(GraphicType.ToString())) {
                            this.graphicPools[GraphicType.ToString()].Add(graphicObj);
                        } else {

                            List<GameObject> newObjList = new List<GameObject>();
                            newObjList.Add(graphicObj);

                            this.graphicPools.Add(GraphicType.ToString(), newObjList);

                        }

                    }

                    break; 

            }

            return graphicObj;

        }

        /// <summary>
        /// Will activate graphic setup for the movement controller. Graphic which is activated depends on the type passed.
        /// </summary>
        /// <param name="GraphicType">Type of graphic to activate</param>
        /// <returns>Will return the graphic gameobject which has been created</returns>
        private IEnumerator ActivateGraphic(GraphicType GraphicType) {

            GameObject graphicObj = null;
            float duration = 2f;
            Vector3 startingPosition = new Vector3(0, 0, 0);


            switch (GraphicType) {
                case GraphicType.AnimateIntoSprint:

                    if (this.animateIntoSprint == false)
                        yield break;

                    // Wait small delay
                    yield return new WaitForSeconds(0.1f);

                    //If acceleration rotated since then end here
                    if (Time.time - this.lastAdvanceRotatation < 1f || this.acceleratedRotated)
                        yield break; 

                    graphicObj = this.graphicPools[GraphicType.ToString()].Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreateObjectPool(GraphicType, true);

                    duration = 2;
                    startingPosition = this.meTransform.position + this.meTransform.forward * 1 + new Vector3(0, 1.3f, 0);

                    break;               
                default:

                    break;

            }

            if (graphicObj == null)
                yield break;



            //take from pool 
            graphicObj.transform.SetParent(null);

            //Position
            graphicObj.transform.position = startingPosition;
            graphicObj.transform.localPosition = startingPosition;
            graphicObj.transform.rotation = this.meTransform.rotation; 

            //Set active 
            graphicObj.SetActive(true);


            yield return new WaitForSeconds(duration);

            // disable and pool the object 
            ABC_Utilities.PoolObject(graphicObj);


        }


        /// <summary>
        /// Will intergate with ABC by retriving the component from the current follow target and then subscribing to it's movement and gravity events
        /// </summary>
        private void IntegrateWithABC() {

            this.ABCEntity = ABC_Utilities.GetStaticABCEntity(this.gameObject);
            this.ABCEventsSM = meTransform.GetComponentInChildren<ABC_StateManager>();
            this.ABCEventsCont = meTransform.GetComponentInChildren<ABC_Controller>();

            //subscribe to the events
            if (this.ABCEventsSM != null && this.ABCIntegration == true) {
                this.ABCEventsSM.onEnableMovement += this.EnableMovement;
                this.ABCEventsSM.onDisableMovement += this.DisableMovement;

                this.ABCEventsSM.onEnableGravity += this.EnableGravity;
                this.ABCEventsSM.onDisableGravity += this.DisableGravity;
            }

            if (this.ABCEventsCont != null) {
                this.ABCEventsCont.onTargetSet += this.LockOnTarget;
            }

        }

        /// <summary>
        /// Will check if the Button provided has been setup in the input manager. 
        /// </summary>
        /// <param name="InputName">Name of </param>
        /// <returns>True if input exists, else false.</returns>
        bool IsInputAvailable(string InputName) {
            try {

#if ENABLE_INPUT_SYSTEM
      return ABCEntity.inputManager.IsButtonSupported(InputName);
#endif

                ABCEntity.inputManager.GetXAxis(InputName);
                ABCEntity.inputManager.GetYAxis(InputName);

                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Will determine if sprint is triggered and allowed to occur
        /// </summary>
        /// <returns>True if sprinting triggered and allowed, else false</returns>
        private bool SprintTriggered() {

            //If not locking on, activating an ability (or just activated one) and sprint trigger is pressed down then return true
            if (this.isJumping == false && this.lockOnTarget == null && ABCEntity.activatingAbility == false && Time.time - ABCEntity.timeOfLastAbilityActivation > 1.5f && (this.isSprinting == true  && this.inputForce != 0 || this.inputForce == 0 && Time.time - this.lastSprintDisabledTime < 0.2f || this.sprintButton != string.Empty && this.IsInputAvailable(this.sprintButton) && ABCEntity.inputManager.GetButton(this.sprintButton) || ABCEntity.inputManager.GetKey(this.sprintKey)) && (ABCEntity.inputManager.GetXAxis("Horizontal") != 0 || ABCEntity.inputManager.GetYAxis("Vertical") != 0)) {
                
                //If enabled then pressing the button will keep the entity sprinting whilst moving
                if (this.enableSprintToggle == true)
                    this.isSprinting = true;
                
                return true;

            } else {

                //Record when disabled if sprinting was toggled so we can add lee way to advanced rotation
                if (this.isSprinting == true)
                    this.lastSprintDisabledTime = Time.time;

                this.isSprinting = false;

                return false; //else return false
            }
        }

        /// <summary>
        /// Will determine if walk is triggered and allowed to occur
        /// </summary>
        /// <returns>True if walking triggered and allowed, else false</returns>
        private bool WalkTriggered() {

            //If only walking in idle mode and not in idle mode return false
            if (this.walkInIdleOnly == true && this.ABCEntity.inIdleMode == false)
                return false; 

            //If not locking on, activating an ability (or just activated one) and walk trigger is pressed down then return true
            if (this.isJumping == false && this.lockOnTarget == null && ABCEntity.activatingAbility == false && (this.walkToggleButton != string.Empty && this.IsInputAvailable(this.walkToggleButton) && ABCEntity.inputManager.GetButton(this.walkToggleButton) || this.walkToggleKey != KeyCode.None && ABCEntity.inputManager.GetKey(this.walkToggleKey)))
                return true;
            else
                return false; //else return false

        }

        /// <summary>
        /// Will determine if the entity is in crosshair mode by checking for the crosshair key presses
        /// </summary>
        /// <returns>True if in crosshair mode, else false </returns>
        private bool InCrossHairMode() {

            //Switched off 
            if (this.enableCrosshairMovement == false)
                return false; 

            if (this.crosshairModeKey != KeyCode.None && ABCEntity.inputManager.GetKey(crosshairModeKey) == true)
                return true;


            if (string.IsNullOrEmpty(this.crosshairModeButton) == false && ABCEntity.inputManager.GetButton(crosshairModeButton) == true)
                return true;


            // if this far nothing pressed so return false 
            return false;

        }

        /// <summary>
        /// Returns if the entity is grounded or not
        /// </summary>
        /// <returns>True if the entity is grounded, else false</returns>
        private bool IsGrounded() {

            //If we on ground then return true
            if (ABCEntity.isInTheAir == false)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Will determine if this entity is standing on another entity (something with a bone structure)
        /// </summary>
        /// <returns>True if standing on an entity, else false</returns>
        private bool IsOnTopOfEntity() {

            //Get the object below us
            GameObject objectBelow = ABC_Utilities.GetObjectBelowEntity(ABC_Utilities.GetStaticABCEntity(meTransform.gameObject));

            //If the object below us has a head then return true
            if (objectBelow != null && objectBelow.transform.GetComponent<Animator>() != null && objectBelow.transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head) != null)
                return true;
            else // else if nothing below or no head return false
                return false;

        }

        /// <summary>
        /// Will work out if advanced rotation is allowed
        /// </summary>
        /// <returns>True if advanced rotation is allowed, else false</returns>
        private bool AdvancedRotationAllowed() {

            if (this.enableAdvancedRotation == true && this.InCrossHairMode() == false && (lockOnTarget == null || this.useLockOnMovement == false) && this.IsGrounded() && this.isJumping == false &&  allowMovement == true && this.FPSMode == false &&  ABCEntity.activatingAbility == false  && ABCEntity.IsWeaponBlocking() == false && (ABCEntity.timeOfLastAbilityActivation == 0 || Time.time - ABCEntity.timeOfLastAbilityActivation > 2f) && this.tempLocked == false) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns how long the entity has been in falling/jumping for
        /// </summary>
        /// <returns>Time in seconds since the entity started falling/jumping</returns>
        private float TimeSpentJumping() {

            return Time.time - this.lastJumpTime;

        }

        /// <summary>
        /// Will wait for a duration before enabling or disabling movement
        /// </summary>
        /// <param name="AllowMovement">True if movement is enabled, else false</param>
        /// <param name="Duration">Duration to wait before movement is enabled or disabled</param>
        private IEnumerator ToggleMovement(bool AllowMovement, float Duration = 0f) {

            //if already enabled or disabled then return
            if (this.allowMovement == AllowMovement)
                yield break;


            if (Duration > 0f)
                yield return new WaitForSeconds(Duration);

            this.allowMovement = AllowMovement;

            //If set to toggle rotation also
            if (this.rotationToggleWithMovement == true)
                this.allowRotation = AllowMovement;



            //If movement was disabled record when this occured
            if (AllowMovement == false)
                this.timeMovementLastDisabled = Time.time;


        }

        /// <summary>
        /// Will enable/disable gravity
        /// </summary>
        /// <param name="AllowGravity">True if gravity is enabled, else false</param>
        private IEnumerator ToggleGravity(bool AllowGravity) {

            //if already enabled or disabled then return
            if (this.allowGravity == AllowGravity)
                yield break;


            //Disable root motion as we are not using gravity (animations can have weights)
            if (AllowGravity == false)
                this.Ani.applyRootMotion = false;
            else
                this.Ani.applyRootMotion = true;


            //toggle gravity
            this.allowGravity = AllowGravity;

        }

        /// <summary>
        /// Method to manage double jumping
        /// </summary>
        private void DoubleJumpHandler() {

            if (this.allowDoubleJump == false || this.doubleJumpState != DoubleJumpState.DoubleJumpReady || this.allowJumping == false || this.isJumping == false || this.IsGrounded() || Time.time - lastJumpTime < 0.3f || this.allowMovement == false)
                return;


            if (this.jumpButton != string.Empty && this.IsInputAvailable(this.jumpButton) && ABCEntity.inputManager.GetButton(this.jumpButton) && ABCEntity.inputManager.ShoulderTriggersPressed() == false && this.isJumping == true) {
                this.lastJumpTime = Time.time;
                this.doubleJumpState = DoubleJumpState.IsDoubleJumping;
                StartCoroutine(ABCEntity.ToggleIK(Time.time, false));


                //Start jumping                 
                this.Ani.SetTrigger(this.doubleJumpAniParameter);

                //Play jumping audio
                if (this.enableJumpingAudio == true && this.jumpingAudio.AudioClip != null)
                    AudioSource.PlayClipAtPoint(this.jumpingAudio.AudioClip, this.ABCEntity.Camera.transform.position, jumpingAudioVolume);

            }


        }

        /// <summary>
        /// Method to manage jumping
        /// </summary>
        private void JumpHandler() {

            //If we have landed on top of an entity then move us back a small amount
            if (this.IsOnTopOfEntity() == true && this.charController != null)
                charController.Move(-meTransform.forward * 0.2f);

            //If entity reaches the ground then stop jumping 
            if (this.IsGrounded() && this.isJumping && Time.time - this.lastJumpTime > 0.3f) {

                //if we was accelerating before jumping then go quicker back into acceleration
                if (this.maxAccelerationTimeReached != 0) {
                    this.maxAccelerationTimeReached = Time.time - (this.intervalFromMaxAccelerationToAnimation * 0.65f);
                } else {
                    this.speedForce = 0;
                }

                this.Ani.SetBool(this.jumpAniParameter, false);

                //Play landing audio
                if (this.enableLandingAudio == true && this.landingAudio.AudioClip != null)
                    AudioSource.PlayClipAtPoint(this.landingAudio.AudioClip, this.meTransform.position, landingAudioVolume);

                //Landing animation
                if (this.enableLandingAnimations && ABCEntity.activatingAbility == false) {
                    //Play landing clip
                    if (this.landingImpactClip.AnimationClip != null && this.TimeSpentJumping() > 2f && Time.time - ABCEntity.timeOfLastAbilityActivation > 2f) {
                        this.ABCEntity.animationRunner.PlayAnimation(this.landingImpactClip.AnimationClip, 0.1f, this.landingImpactClipSpeed);

                        //If set to shake camera and we have been falling for over 2.5 seconds
                        if (this.landingImpactShakeCamera == true && this.TimeSpentJumping() >= 2.5f)
                            StartCoroutine(ABCEntity.ShakeCamera(this.landingImpactShakeCameraDuration, this.landingImpactShakeCameraAmount, this.landingImpactShakeCameraSpeed, this.landingImpactShakeCameraDelay));

                    } else if (this.landingClip.AnimationClip != null && this.inputForce == 0) {
                        this.ABCEntity.animationRunner.PlayAnimation(this.landingClip.AnimationClip, 0.1f, this.landingClipSpeed);
                    } else if (this.landingRunClip.AnimationClip != null && this.inputForce > 0 && (this.doubleJumpState == DoubleJumpState.DoubleJumpReady && this.TimeSpentJumping() > 1f || this.doubleJumpState != DoubleJumpState.DoubleJumpReady && this.TimeSpentJumping() > 1.2f)) {

                        //If input given then play landing run clip if time spent jumping is over x value (different for normal jump or double jump)
                        this.ABCEntity.animationRunner.PlayAnimation(this.landingRunClip.AnimationClip, 0.1f, this.landingRunClipSpeed);
                    }

                }




                this.isJumping = false;
                this.doubleJumpState = DoubleJumpState.DoubleJumpReady;
                this.lastLandTime = Time.time;
                StartCoroutine(ABCEntity.ToggleIK(Time.time, true, 0.5f));                
            }

            //If not allowed to jump then end here (includes a small breather from when the entity lands)
            if (this.allowJumping == false || this.IsGrounded() == false || Time.time - this.lastLandTime < 0.3f || this.allowMovement == false || ABCEntity.activatingAbility == true)
                return;


            if (this.jumpButton != string.Empty && this.IsInputAvailable(this.jumpButton) && ABCEntity.inputManager.GetButton(this.jumpButton) && ABCEntity.inputManager.ShoulderTriggersPressed() == false && this.isJumping == false) {
                 
                //if just activated ability or landed then immediatly interrupt animation and quickly go into jump animation
                if (this.ABCEntity.animationRunner.animationClip != null && (Time.time - this.ABCEntity.timeOfLastAbilityActivation < 2f || Time.time - this.lastLandTime < 0.5f)) {
                    if (this.ABCEntity.animationRunner.IsAnimationClipOverrideRunning() == true)
                        this.ABCEntity.animationRunner.InterruptCurrentAnimation(true);
                    else
                        this.ABCEntity.animationRunner.InterruptCurrentAnimation(false);
                }

                //Start jumping 
                this.Ani.SetBool(this.jumpAniParameter, true);

                //Play jumping audio
                if (this.enableJumpingAudio == true && this.jumpingAudio.AudioClip != null)
                    AudioSource.PlayClipAtPoint(this.jumpingAudio.AudioClip, this.ABCEntity.Camera.transform.position, jumpingAudioVolume);

                //If sprint jumping
                if (this.enableSprintJumping == true && this.SprintTriggered())
                    this.Ani.SetTrigger("Sprint"+this.jumpAniParameter + "Trigger");
                else // normal jump
                    this.Ani.SetTrigger(this.jumpAniParameter + "Trigger");


                this.lastJumpTime = Time.time;
                this.isJumping = true;
                StartCoroutine(ABCEntity.ToggleIK(Time.time, false));


            }


        }

        /// <summary>
        /// Will reset the jump settings
        /// </summary>
        private void ResetJump() {

            //Stop jumping 
            this.isJumping = false;
            this.lastJumpTime = Time.time;
            this.doubleJumpState = DoubleJumpState.DoubleJumpReady;

            if (string.IsNullOrEmpty(this.jumpAniParameter) == false)
                this.Ani.SetBool(this.jumpAniParameter, false);
        }

        /// <summary>
        /// Will return the speed force depending on circumstances i.e if sprinting
        /// </summary>
        /// <remarks>Will also play some animation runner animations depending on those circumstances like animating into sprint</remarks>
        /// <returns>Float of the speed force</returns>
        private float GetSpeedForce() {

            //If jumping return double the move force
            if (this.isJumping)
                return this.moveForce * 2;


            //If sprinting
            if (this.SprintTriggered()) {

                //If not been sprinting then start with an animation if enabled 
                if (Time.time - this.lastSprintDashTime > 1f && Time.time - this.lastLandTime > 0.8f && this.animateIntoSprint == true && this.animateIntoSprintClip.AnimationClip != null && this.speedForce < this.sprintForce && this.acceleratedRotated == false) {
                    this.ABCEntity.animationRunner.PlayAnimation(this.animateIntoSprintClip.AnimationClip, 0, this.animateIntoSprintClipSpeed);
                    StartCoroutine(this.ActivateGraphic(GraphicType.AnimateIntoSprint));
                    this.lastSprintDashTime = Time.time;
                }

                //start sprinting 
                return this.sprintForce;

            } else if (this.animateAfterSprint && this.animateAfterSprintClip.AnimationClip != null && this.IsGrounded() && this.doubleJumpState == DoubleJumpState.DoubleJumpReady && this.speedForce == this.sprintForce && this.ABCEntity.activatingAbility == false && Time.time - this.ABCEntity.timeOfLastAbilityActivation > 1f) { // else if was sprinting

                //if enabled then play animate after sprint and stop movement for this update
                this.ABCEntity.animationRunner.PlayAnimation(this.animateAfterSprintClip.AnimationClip, 0, this.animateAfterSprintClipSpeed);

                return 0;

            }

            //If using accelerated movement
            if (this.useAcceleratedMovement == true) {

                //If 0 give some heads up to start animating
                if (this.speedForce == 0) {
                    return this.moveForce / 6;
                }

                //If we have done an accelerated rotation and max acceleration isn't reached then set max acceleration to reached and return max move force
                if (this.acceleratedRotated == true && this.maxAccelerationTimeReached != 0) {
                    this.maxAccelerationTimeReached = Time.time - (this.intervalFromMaxAccelerationToAnimation * 1); // lower the * 1 (100%) if you don't want max acceleration to start instantly after an accelerated rotation
                    this.acceleratedRotated = false;
                    return this.moveForce;
                }

                //If the acceleration has reached the max move force then set acceleration to reached and return max move force
                if (this.speedForce + this.moveAcceleration * Time.deltaTime >= this.moveForce) {

                    //If max acceleration has only just been reached, or we activating ability or just activated an ability then set the time reached ready for the next accelerated animation to start in x seconds
                    if (this.maxAccelerationTimeReached == 0 || this.ABCEntity.activatingAbility == true || Time.time - this.ABCEntity.timeOfLastAbilityActivation < 1.5f) {
                        this.maxAccelerationTimeReached = Time.time;
                    }

                    //Return move force as we reached max acceleration
                    return this.moveForce;
                }

                //If this far then just add some acceleration to the current speed force
                return this.speedForce + this.moveAcceleration * Time.deltaTime;

            }


            //else no special conditions so just return the move force
            return this.moveForce;

        }

        /// <summary>
        /// Determines the move direction depending on the axis input made by the user and the direction of the camera
        /// </summary>
        private void DetermineMoveDirection() {


            //Get speed Force
            this.speedForce = this.GetSpeedForce();

            //Get previous magnitude 
            float previousInputMagnitude = new Vector2(inputX, inputZ).sqrMagnitude;

            //Get input from user
            this.inputX = ABCEntity.inputManager.GetXAxis("Horizontal");
            this.inputZ = ABCEntity.inputManager.GetYAxis("Vertical");




#if ENABLE_INPUT_SYSTEM
        if (this.moveButton != string.Empty && this.IsInputAvailable(this.moveButton)) {
                this.inputX = ABCEntity.inputManager.GetXAxis(this.moveButton);
                this.inputZ = ABCEntity.inputManager.GetYAxis(this.moveButton);
          }
#endif



            //If both axis are being used then clamp the magnitude to avoid the extra speed from moving in a diagonal direction
            if (this.inputX != 0 && this.inputZ != 0) {
                Vector3 inputMod = Vector3.ClampMagnitude(new Vector3(this.inputX, 0, this.inputZ), 1f);
                this.inputX = inputMod.x;
                this.inputZ = inputMod.z; 
            }


            //Check if walk toggle key is pressed
            if (this.WalkTriggered()) {

                if (this.walkingToggled == true) {
                    this.walkingToggled = false;
                    this.isWalking = false; 
                } else {
                    this.walkingToggled = true;
                }
            }


            //If using walk input and force is below the threshold then set is walking to true
            float currentInputMagnitude = new Vector2(inputX, inputZ).sqrMagnitude;


            //If we have input and toggle walk from input is enabled and the current input is slightly pushed and the difference from previous input is not massive (input stopped and joystick resets) then start walking
            if ((this.walkInIdleOnly == false || this.walkInIdleOnly == true && this.ABCEntity.inIdleMode == true) && (this.walkingToggled == true || this.toggleWalkFromLightInput == true && currentInputMagnitude > 0f && currentInputMagnitude < (this.isWalking == true ? 0.8f : 0.05f) && (isWalking == false && previousInputMagnitude > 0f && Mathf.Abs(previousInputMagnitude - currentInputMagnitude) < 0.5f || isWalking == true))) {               
                
                this.isWalking = true;
                this.maxAccelerationTimeReached = 0;
                this.speedForce /= 2; 
            } else {

                this.isWalking = false;
            }


            //Apply speed
            this.inputX *= (this.speedForce + this.moveForceAdjustment + this.moveForceWeaponAdjustment);
            this.inputZ *= (this.speedForce + this.moveForceAdjustment + this.moveForceWeaponAdjustment);

  
            //reset any speed generated as no input
            if (inputX == 0 && inputZ == 0) {
                this.speedForce = 0;
                this.maxAccelerationTimeReached = 0;
            }

            //Retrieve input force to be used by animations and rotation checks later
            if ((this.inputX >= 0.1 || this.inputX <= 0.1) && (this.inputZ >= 0.1 || this.inputZ <= 0.1))
                this.inputForce = new Vector2(inputX, inputZ).sqrMagnitude;
            else if (this.inputX != 0)
                this.inputForce = Mathf.Max(0.1f, new Vector2(inputX, 0).sqrMagnitude);
            else if (this.inputZ != 0)
                this.inputForce = Mathf.Max(0.1f, new Vector2(inputZ, 0).sqrMagnitude);


            //record the direction of the camera
            Vector3 camRight = Cam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 camForward = Cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            //Work out move direction using the camera direction and the input from user
            this.moveDirection = camRight * inputX + camForward * inputZ;

        }

        public ABC_AvatarMaskReference mask;

        /// <summary>
        /// Will apply an accelerated rotation on the entity
        /// </summary>
        /// <param name="AnimationClip">Advanced rotation clip to play</param>
        /// <param name="Speed">Speed of clip</param>
        /// <param name="StopPercentage">Rotation animation will stop once the percentage has been hit</param>
        /// <param name="AvatarMask">Avatar Mask to apply during the rotation</param>
        private IEnumerator ActivateAdvancedRotation(AnimationClip AnimationClip, float Speed = 1, float StopPercentage = 95f, AvatarMask AvatarMask = null) {

            //If the rotation clip is already running end here
            if (this.ABCEntity.animationRunner.IsAnimationRunning(AnimationClip) == true || this.isWalking == true)
                yield break;

            //Track when we last acceleration rotated
            this.lastAdvanceRotatation = Time.time;

            //Disable movement for time being
            this.ToggleMovement(false);
            this.timeMovementLastDisabled = Time.time;



            //Play rotation clip
            this.ABCEntity.animationRunner.StartAnimation(AnimationClip, 0, Speed, AvatarMask);

            while (this.ABCEntity.animationRunner.IsAnimationRunning(AnimationClip) == true) {

                yield return new WaitForEndOfFrame();

                //Start moving when near end of the clip
                if (this.ABCEntity.animationRunner.clipProgress > StopPercentage) {
                    this.ABCEntity.animationRunner.EndAnimation(AnimationClip);
                    break;
                }
               

            }

            //Turn movement back on
            this.ToggleMovement(true);

            //Track if acceleration rotation occurs
            if (AnimationClip == this.acceleratedRotationClip.AnimationClip)
                this.acceleratedRotated = true;

        }

        /// <summary>
        /// Will rotate the entity by the direction and speed provided, as long as the input force is greater then the rotation drag setup
        /// </summary>
        /// <param name="Direction">Vector3 to rotate the entity in</param>
        /// <param name="RotationSpeed">Float indicating the speed of rotation</param>
        /// <param name="InputForce">The input force applied by the user, needs to be greater then the rotation drag for the rotation to happen</param>
        private void Rotate(Vector3 Direction, float RotationSpeed, float InputForce) {

            //If we are not allowed to rotate or the input force is not greater then the setup drag then return here
            if (InputForce <= this.rotationDrag || this.allowRotation == false || Direction == Vector3.zero)
                return;

            // else rotate 
            meTransform.rotation = Quaternion.Slerp(meTransform.rotation, Quaternion.LookRotation(Direction), Time.deltaTime * RotationSpeed);
        }

        /// <summary>
        /// Will move the entity using the character controller component. 
        /// </summary>
        /// <param name="Motion">Vector3 of the movement to apply</param>
        private void Move(Vector3 Motion) {

            //If root motion mode is on then reset any X and Y coords as this is done by the animation
            if (this.rootMotionMode == true) {
                Motion.x = 0f;
                Motion.z = 0f;
            }


            if (this.charController != null && this.charController.enabled == true)
                charController.Move(Motion * Time.deltaTime);


        }


        /// <summary>
        /// Main method to rotate and move the entity
        /// </summary>
        private void RotateAndMoveEntity() {

            //Call lock off handler 
            StartCoroutine(this.TempLockOffHandler());
     

            // If crosshair movement key is held down prioritise the rotate to camera direction
            // else rotate the entity to face the direction we want to travel if we have not got a lock on target
            // else turn the entity to face lock on target
            if (this.AdvancedRotationAllowed() == true && allowMovement == true) {

                //If entity does a sharp 180 turn then play rotation clip (gets current forward and dot of the next move direction (-1 is 180 degree turn)
                if (Vector3.Dot(this.transform.forward.normalized, this.moveDirection.normalized) <= -0.8f) {

                    //If moving do accelerated 180 (unless already doing advanced 180)
                    if (Time.time - this.LastMovementTime < 0.2f && ABCEntity.animationRunner.IsAnimationRunning(this.advancedRotation180Clip.AnimationClip) == false) {

                        //Turned accelerated 180
                        if (this.acceleratedRotationClip.AnimationClip != null) {
                            StartCoroutine(ActivateAdvancedRotation(this.acceleratedRotationClip.AnimationClip, this.acceleratedRotationClipSpeed, this.acceleratedRotationStopPercentage, this.acceleratedRotationClipMask.AvatarMask));
                        }

                    } else { // else normal 180

                        //Turned 180
                        if (this.advancedRotation180Clip.AnimationClip != null) {
                            StartCoroutine(ActivateAdvancedRotation(this.advancedRotation180Clip.AnimationClip, this.advancedRotation180ClipSpeed, this.advancedRotation180StopPercentage, this.advancedRotation180ClipMask.AvatarMask));
                        }

                    }

                } else if (Vector3.Dot(this.transform.right.normalized, this.moveDirection.normalized) >= 0.85f && Time.time - this.LastMovementTime > 0.1f) { // else if turning right

                    //Turned Right
                    if (this.advancedRotationRightClip.AnimationClip != null) {
                        StartCoroutine(ActivateAdvancedRotation(this.advancedRotationRightClip.AnimationClip, this.advancedRotationRightClipSpeed, advancedRotationRightStopPercentage, this.advancedRotationRightClipMask.AvatarMask));
                    }

                } else if (Vector3.Dot((-this.transform.right).normalized, this.moveDirection.normalized) >= 0.85f && Time.time - this.LastMovementTime > 0.1f) {// else if turning left 

                    //Turned Left
                    if (this.advancedRotationLeftClip.AnimationClip != null) {
                        StartCoroutine(ActivateAdvancedRotation(this.advancedRotationLeftClip.AnimationClip, this.advancedRotationLeftClipSpeed, this.advancedRotationLeftStopPercentage, this.advancedRotationLeftClipMask.AvatarMask));
                    }
                }

          
               
            }


            if (this.ABCEntity.inIdleMode == false && (this.FPSMode == true || this.InCrossHairMode() == true)) {
                this.meTransform.rotation = Quaternion.Euler(this.meTransform.eulerAngles.x, Camera.main.transform.eulerAngles.y, this.meTransform.eulerAngles.z);
            } else if (this.ABCEntity.inIdleMode == false && (this.lockOnTarget != null && this.useLockOnMovement == true)) {
                this.meTransform.LookAt(lockOnTarget.transform.position);
            } else if (Time.time - timeMovementLastDisabled > this.rotationStopMovementLeeway) {
                this.Rotate(this.moveDirection, this.rotationMoveSpeed, this.inputForce);
            }



                //If movement is not allowed then end here
                if (allowMovement == false)
                return;

            //Apply veritcal velocity
            this.moveDirection.y = this.VerticalVelocity;

            //Track time we last moved
            if (this.moveDirection != Vector3.zero)
                this.LastMovementTime = Time.time;

            //Move entity
            this.Move(this.moveDirection);



        }

        /// <summary>
        /// Will animate the entity using settings and the input force
        /// </summary>
        private void Animate() {

            //If movement is not allowed then end here making sure animations not playing
            if (allowMovement == false) {

                //Reset normal animation            
                Ani.SetFloat(this.animationParameter, 0, 0.0f, Time.deltaTime * 2f);
                Ani.SetBool(this.animationAcceleratedParameter, false);

                //Reset Lock On Movement
                if (this.useLockOnMovement) {
                    Ani.SetFloat(this.lockOnSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.lockOnForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f); 
                }

                //Reset Crosshair Movement
                if (this.useCrosshairAnimation) {
                    Ani.SetFloat(this.crossHairSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.crossHairForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                }


                return; 
            }

            //If no animator has been setup or jumping is happening (Animations done it it's own method) then return here
            if (this.Ani == null || this.tempLocked == true)
                return;

            //If we are falling then start animation
            if (this.IsGrounded() == false && this.isJumping == false && ABC_Utilities.EntityDistanceFromGround(meTransform) > 2f) {

                //Start falling 
                this.Ani.SetBool(this.jumpAniParameter, true);
                this.Ani.SetTrigger(this.fallAniParameter + "Trigger");
                
                this.isJumping = true;
                this.lastJumpTime = Time.time - 1.5f;
            }


            //If jumping then make sure no other animations playing
            if (this.isJumping || Time.time - this.lastLandTime < 0.2f) {
       
                //Reset normal animation            
                Ani.SetFloat(this.animationParameter, 0, 0.0f, Time.deltaTime * 2f);
                Ani.SetBool(this.animationAcceleratedParameter, false);

                this.isWalking = false;
                Ani.SetBool(this.walkAniParameter, false);

                //Reset Lock On Movement
                if (this.useLockOnMovement) {
                    Ani.SetFloat(this.lockOnSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.lockOnForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                }

                //Reset Crosshair Movement
                if (this.useCrosshairAnimation) {
                    Ani.SetFloat(this.crossHairSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.crossHairForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                }

                //End here as jumping
                return;
            }


            if (this.ABCEntity.inIdleMode == false && (this.FPSMode == true || this.InCrossHairMode() == true && this.useCrosshairAnimation == true)) {

                //Reset normal animation            
                Ani.SetFloat(this.animationParameter, 0, 0.0f, Time.deltaTime * 2f);
                Ani.SetBool(this.animationAcceleratedParameter, false);

                this.isWalking = false;
                this.walkingToggled = false;
                Ani.SetBool(this.walkAniParameter, false);

                //Reset Lock On Movement
                if (this.useLockOnMovement) {
                    Ani.SetFloat(this.lockOnSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.lockOnForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                }

                //Animate crosshair movement
                // if forward (Z axis) is being pressed then stop side animations and use forward animation
                if (Mathf.Abs(this.inputZ) > Mathf.Abs(this.inputX)) {
                    Ani.SetFloat(this.crossHairSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.crossHairForwardAniParameter, this.inputForce * this.inputZ, 0.0f, Time.deltaTime * 2f);
                } else { // else side (X axis) is being pressed so stop forward animation and use sideways animation
                    Ani.SetFloat(this.crossHairForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.crossHairSideAniParameter, this.inputForce * this.inputX, 0.0f, Time.deltaTime * 2f);
                }

                //End here as in crosshair mode
                return;

            }



            if (this.ABCEntity.inIdleMode == false && lockOnTarget != null && this.useLockOnMovement) {

                //Reset normal animation            
                Ani.SetFloat(this.animationParameter, 0, 0.0f, Time.deltaTime * 2f);
                Ani.SetBool(this.animationAcceleratedParameter, false);

                this.isWalking = false;
                this.walkingToggled = false;
                Ani.SetBool(this.walkAniParameter, false);

                //Reset Crosshair Movement
                if (this.useCrosshairAnimation) {
                    Ani.SetFloat(this.crossHairSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.crossHairForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                }

                //Animate lock on movement
                // if forward (Z axis) is being pressed then stop side animations and use forward animation
                if (Mathf.Abs(this.inputZ) > Mathf.Abs(this.inputX)) {
                    Ani.SetFloat(this.lockOnSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.lockOnForwardAniParameter, this.inputForce * this.inputZ, 0.0f, Time.deltaTime * 2f);
                } else { // else side (X axis) is being pressed so stop forward animation and use sideways animation
                    Ani.SetFloat(this.lockOnForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                    Ani.SetFloat(this.lockOnSideAniParameter, this.inputForce * this.inputX, 0.0f, Time.deltaTime * 2f);
                }


                //End here as in lock on mode
                return;

            }


            //If this far just animate normally: 

            //Reset Lock On Movement
            if (this.useLockOnMovement) {
                Ani.SetFloat(this.lockOnSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                Ani.SetFloat(this.lockOnForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
            }

            //Reset Crosshair Movement
            if (this.useCrosshairAnimation) {
                Ani.SetFloat(this.crossHairSideAniParameter, 0, 0.0f, Time.deltaTime * 2f);
                Ani.SetFloat(this.crossHairForwardAniParameter, 0, 0.0f, Time.deltaTime * 2f);
            }

            //animate normally if not in air
            if (ABC_Utilities.EntityDistanceFromGround(this.meTransform) < 2) {

                //If sprinting then make sure the minimum number will sprint animate
                if (this.SprintTriggered()) {
                    Ani.SetFloat(this.animationParameter, 1000, 0.0f, Time.deltaTime * 2f);
                } else {
                    Ani.SetFloat(this.animationParameter, this.inputForce != 0 ? Mathf.Max(0.1f, this.inputForce) : this.inputForce, 0.0f, Time.deltaTime * 2f);


                    //If walking then set the boolean
                    if (this.isWalking)
                        Ani.SetBool(this.walkAniParameter, true);
                    else
                        Ani.SetBool(this.walkAniParameter, false);

                    if (this.isWalking == false && this.enableMaxAcceleratedAnimation == true && this.maxAccelerationTimeReached != 0 && Time.time - this.maxAccelerationTimeReached > this.intervalFromMaxAccelerationToAnimation)
                        Ani.SetBool(this.animationAcceleratedParameter, true);
                    else
                        Ani.SetBool(this.animationAcceleratedParameter, false);

                }


            }


        }

        /// <summary>
        /// Will handle all movement audio 
        /// </summary>
        /// 
        private void MovementAudioHandler() {

            if (enableMovementAudio == false  || this.ABCEntity.animationRunner.IsAnimationRunning(this.animateIntoSprintClip.AnimationClip) == true)
                return;

            //If we have an audio matching the current tag we standing on 
            if (this.movementAudioTagMaps.Where(sm => sm.StepTag == this.CurrentStepTag).Count() > 0) {

                List<ABC_AudioClipReference> audioClips = this.movementAudioTagMaps.Where(sm => sm.StepTag == this.CurrentStepTag).FirstOrDefault().StepAudio;
                AudioSource.PlayClipAtPoint(audioClips[Random.Range(0, audioClips.Count())].AudioClip, this.ABCEntity.Camera.transform.position, movementAudioVolume);

            } else { // else play default 

                List<ABC_AudioClipReference> audioClips = this.movementAudioTagMaps.Where(sm => sm.StepTag == this.CurrentStepTag).FirstOrDefault().StepAudio;
                AudioSource.PlayClipAtPoint(this.movementAudioDefault[Random.Range(0, this.movementAudioDefault.Count())].AudioClip, this.ABCEntity.Camera.transform.position, movementAudioVolume);
                
            }

        }




        /// <summary>
        /// Will handle the temporarily lock off so if a configured button is pressed
        /// the entity will for a duration not be locked on to the current target 
        /// after the duration the entity will lock on to the target again
        /// </summary>
        private IEnumerator TempLockOffHandler() {

            // if temp lock off button is pressed then temporarily unlock on the target 
            if (this.InCrossHairMode() == false && this.lockOnTarget != null && (this.tempLockOffButton != string.Empty && this.IsInputAvailable(this.tempLockOffButton) && ABCEntity.inputManager.GetButton(this.tempLockOffButton) || ABCEntity.inputManager.GetKey(this.tempLockOffKey))) {

                //Let script not temporarily lock is on 
                this.tempLocked = true;

                //store current target 
                GameObject currentLockOnTarget = this.lockOnTarget;

                //turn off lock on
                this.lockOnTarget = null;

                //wait the duration
                yield return new WaitForSeconds(this.tempLockOffDuration);

                //If a new target hasn't been selected then lock back on
                if (this.lockOnTarget == null)
                    this.lockOnTarget = currentLockOnTarget;

                //Turn temp lock off
                this.tempLocked = false;

            }


            // if crosshair is on and temp off button is pressed then temporarily stop crosshair mode
            if (this.InCrossHairMode() == true && (ABCEntity.inputManager.GetKey(this.tempCrossOffKey) || ABCEntity.inputManager.GetButton(this.tempCrossOffButton))) {

                //Let script not temporarily lock is on
                this.tempLocked = true;

                //store current crosshair key 
                KeyCode currentCrosshairKey = this.crosshairModeKey;
                string currentCrosshairButton = this.crosshairModeButton;

                //turn crosshair mode key to none to turn it off
                this.crosshairModeKey = KeyCode.None;
                this.crosshairModeButton = string.Empty;

                //wait the duration
                yield return new WaitForSeconds(this.tempCrossOffDuration);

                //turn back on
                this.crosshairModeKey = currentCrosshairKey;
                this.crosshairModeButton = currentCrosshairButton;

                //Turn temp lock off
                this.tempLocked = false;
            }

        }


        /// <summary>
        /// Will manage if a Parkour event should activate
        /// </summary>
        /// <returns>True if a parkour event activated, else false</returns>
        private bool ParkourHandler() {

            //Tell system we still parkouring
            if (this.isParkouring)
                return true; 

            //If parkour not enabled then end here
            if (this.enableParkour == false)
                return false;

            //if in lock on mode mode
            if (this.lockOnTarget != null && this.useLockOnMovement == true)
                return false; 

            //If activating ability don't parkour
            if (ABCEntity.activatingAbility == true || Time.time - ABCEntity.timeOfLastAbilityActivation < 1)
                return false;

            //If no parkour object has been hit end here
            if (this.parkourObstacle == null)
                return false;

            //Get parkour obstacle
            ABC_ParkourObstacle parkourObstacle = this.parkourObstacle;

            //Don't parkour if just jumped or landed
            if (parkourObstacle.parkourType != ParkourType.LiftUp && (Time.time - this.lastLandTime < 0.6f || Time.time - this.lastJumpTime < 0.6f))
                return false; 

            //If trigger required and not pressed then end here 
            if (parkourObstacle.activateWithoutInput == false && this.parkourRequireInputTrigger == true && (this.parkourInputType == InputType.Key && ABCEntity.inputManager.GetKey(this.parkourInputKey) == false || this.parkourInputType == InputType.Button && ABCEntity.inputManager.GetButton(this.parkourInputButton) == false))
                return false;


            if (parkourObstacle != null) {

                //Define end movement to use
                ABC_MovementController.ParkourMovement parkourMovement = null;

                //If obstacle has a dynamic parkour movement enabled then use that 
                if (parkourObstacle.enableDynamicParkour == true) {

                    parkourMovement = parkourObstacle.dynamicParkourMovement;

                } else { 
                    
                    // else use a movement defined on entity depending on type

                    switch (parkourObstacle.parkourType) {
                        case ParkourType.Vault:

                            if (string.IsNullOrEmpty(parkourObstacle.specificParkourTag) == false && parkourObstacle.activateSpecificParkour == true) {
                                parkourMovement = this.parkourVaultAnimations.Where(p => p.parkourTag == parkourObstacle.specificParkourTag).FirstOrDefault();
                            }


                            if (parkourMovement == null)
                                parkourMovement = this.parkourVaultAnimations[Random.Range(0, this.parkourVaultAnimations.Count)];


                            break;
                        case ParkourType.LiftUp:

                            if (string.IsNullOrEmpty(parkourObstacle.specificParkourTag) == false && parkourObstacle.activateSpecificParkour == true) {
                                parkourMovement = this.parkourLiftUpAnimations.Where(p => p.parkourTag == parkourObstacle.specificParkourTag).FirstOrDefault();
                            }


                            if (parkourMovement == null)
                                parkourMovement = this.parkourLiftUpAnimations[Random.Range(0, this.parkourLiftUpAnimations.Count)];


                            break;
                        case ParkourType.Slide:

                            if (string.IsNullOrEmpty(parkourObstacle.specificParkourTag) == false && parkourObstacle.activateSpecificParkour == true) {
                                parkourMovement = this.parkourSlideAnimations.Where(p => p.parkourTag == parkourObstacle.specificParkourTag).FirstOrDefault();
                            }


                            if (parkourMovement == null)
                                parkourMovement = this.parkourSlideAnimations[Random.Range(0, this.parkourSlideAnimations.Count)];


                            break;
                    }
                }


                //Make sure we have one else end here
                if (parkourMovement == null)
                    return false; 

                StartCoroutine(ActivateParkour(parkourMovement, this.parkourObjectRayHit));

                return true;

            } else {
                return false;
            }

        }

        /// <summary>
        /// Will handle any parkour events and movement
        /// </summary>
        /// <param name="ParkourMovement">The Parkour Movement to use</param>
        private IEnumerator ActivateParkour(ParkourMovement ParkourMovement, RaycastHit ObstacleHit) {

            if (this.enableParkour == false || this.isParkouring || this.ABCEntity.animationRunner.IsAnimationRunning(ParkourMovement.parkourAnimation.AnimationClip) == true)
                yield break;


            //If found find the center top point ready for the animation jump over half point
            Vector3 obstacleCenterPoint = ObstacleHit.collider.bounds.center + new Vector3(0, ObstacleHit.collider.bounds.extents.y + ParkourMovement.parkourVitalPointHeightOffset, 0);
                
            if (this.enableParkourDebug)
                Debug.DrawLine(obstacleCenterPoint, obstacleCenterPoint + Vector3.up * 10, Color.red, 10f);

            //Find other side of the obstacle entity is jumping over by adding the distance between edge to center on again + some lee way on characters front
            Vector3 obstacleOtherSidePoint = obstacleCenterPoint + (ABCEntity.transform.forward * Vector3.Distance(ObstacleHit.point, obstacleCenterPoint)) + (ABCEntity.transform.forward * 3);
            obstacleOtherSidePoint.y = transform.position.y;

            if (this.enableParkourDebug)
                Debug.DrawLine(obstacleOtherSidePoint, obstacleOtherSidePoint + Vector3.up * 10, Color.green, 10f);

            //track the vital grab/height of the obstacle
            Vector3 obstacleVitalHeightPoint = new Vector3(ObstacleHit.point.x, obstacleCenterPoint.y + ParkourMovement.parkourVitalPointHeightOffset, ObstacleHit.point.z);

            if (this.enableParkourDebug)
                Debug.DrawLine(obstacleVitalHeightPoint, obstacleVitalHeightPoint + Vector3.up * 10, Color.blue, 10f);

            //track the near height of the obstacle
            Vector3 obstacleHeightPoint = new Vector3(ObstacleHit.point.x, obstacleCenterPoint.y, ObstacleHit.point.z);

            if (this.enableParkourDebug)
                Debug.DrawLine(obstacleHeightPoint, obstacleHeightPoint + Vector3.up * 10, Color.yellow, 10f);



            //Double check not already running
            if (this.isParkouring || this.ABCEntity.animationRunner.IsAnimationRunning(ParkourMovement.parkourAnimation.AnimationClip) == true)
                yield break;

         
            //Stop max acceleration 
            this.maxAccelerationTimeReached = 0;
            if (string.IsNullOrEmpty(this.animationAcceleratedParameter) == false && this.enableMaxAcceleratedAnimation == true)
                Ani.SetBool(this.animationAcceleratedParameter, false);

            //Stop jumping 
            this.ResetJump();

            //Stop ability activating
            this.ABCEntity.ToggleAbilityActivation(false);

            //Block crosshair override
            this.ABCEntity.ToggleCrossHairOverrideBlock(true);

            //In parkour mode
            this.isParkouring = true;

    

            //Track if animator was in root motion or not then turn it off
            bool animatorWasInRootMotion = this.ABCEntity.animator.applyRootMotion;
            if (animatorWasInRootMotion == true)
                this.ABCEntity.animator.applyRootMotion = false;

            //Start animation
            this.ABCEntity.animationRunner.PlayAnimation(ParkourMovement.parkourAnimation.AnimationClip, 0, ParkourMovement.parkourAnimationSpeed, 0, null, true, false);
            this.ABCEntity.UseGravity(false);

            //create timer and start position
            float timer = 0f;
            Vector3 startPosition = transform.position /*+ -transform.forward * 1f*/;

            //Move entity
            while (timer <= (ParkourMovement.parkourAnimation.AnimationClip.length / ParkourMovement.parkourAnimationSpeed)) {
                timer += Time.deltaTime;



                switch (ParkourMovement.parkourType) {
                    case ParkourType.Vault:

                        if (this.ABCEntity.animationRunner.GetCurrentAnimationProgress() < ParkourMovement.parkourAnimationVitalPercentagePoint) {

                            transform.position = Vector3.Lerp(startPosition, obstacleCenterPoint, Mathf.InverseLerp(0, ParkourMovement.parkourAnimationVitalPercentagePoint, this.ABCEntity.animationRunner.GetCurrentAnimationProgress()));

                        } else {

                            if (ParkourMovement.enableRootMotion == true && this.ABCEntity.animationRunner.GetCurrentAnimationProgress() >= ParkourMovement.enableRootMotionPercentage)
                                this.ABCEntity.animator.applyRootMotion = true;
                            else
                                transform.position = Vector3.Lerp(obstacleCenterPoint, obstacleOtherSidePoint, Mathf.InverseLerp(ParkourMovement.parkourAnimationVitalPercentagePoint, ParkourMovement.enableRootMotion ? ParkourMovement.enableRootMotionPercentage : 100, this.ABCEntity.animationRunner.GetCurrentAnimationProgress()));
                        }

                        break;
                    case ParkourType.LiftUp:

                        if (this.ABCEntity.animationRunner.GetCurrentAnimationProgress() < ParkourMovement.parkourAnimationVitalPercentagePoint) {

                            transform.position = Vector3.Lerp(startPosition, obstacleVitalHeightPoint, Mathf.InverseLerp(0, ParkourMovement.parkourAnimationVitalPercentagePoint, this.ABCEntity.animationRunner.GetCurrentAnimationProgress()));

                        } else {

                            if (ParkourMovement.enableRootMotion == true && this.ABCEntity.animationRunner.GetCurrentAnimationProgress() >= ParkourMovement.enableRootMotionPercentage)
                                this.ABCEntity.animator.applyRootMotion = true;
                            else
                                transform.position = Vector3.Lerp(obstacleVitalHeightPoint, obstacleHeightPoint, Mathf.InverseLerp(ParkourMovement.parkourAnimationVitalPercentagePoint, ParkourMovement.enableRootMotion ? ParkourMovement.enableRootMotionPercentage : 100, this.ABCEntity.animationRunner.GetCurrentAnimationProgress()));

                        }

                        break;
                    case ParkourType.Slide:

                        if (this.ABCEntity.animationRunner.GetCurrentAnimationProgress() < ParkourMovement.parkourAnimationVitalPercentagePoint) {

                            transform.position = Vector3.Lerp(startPosition, new Vector3(obstacleCenterPoint.x, transform.position.y, obstacleCenterPoint.z), Mathf.InverseLerp(0, ParkourMovement.parkourAnimationVitalPercentagePoint, this.ABCEntity.animationRunner.GetCurrentAnimationProgress()));

                        } else {

                            if (ParkourMovement.enableRootMotion == true && this.ABCEntity.animationRunner.GetCurrentAnimationProgress() >= ParkourMovement.enableRootMotionPercentage)
                                this.ABCEntity.animator.applyRootMotion = true;
                            else
                                transform.position = Vector3.Lerp(new Vector3(obstacleCenterPoint.x, transform.position.y, obstacleCenterPoint.z), obstacleOtherSidePoint, Mathf.InverseLerp(ParkourMovement.parkourAnimationVitalPercentagePoint, ParkourMovement.enableRootMotion ? ParkourMovement.enableRootMotionPercentage : 100, this.ABCEntity.animationRunner.GetCurrentAnimationProgress()));

                        }


                        break;
                }



                yield return null;

            }



            //Stop jumping 
            this.ResetJump();

            //Revert root motion to what it was before
            this.ABCEntity.animator.applyRootMotion = animatorWasInRootMotion;

            //Not using gravity and reset dash time
            this.ABCEntity.UseGravity(true); 
            this.lastSprintDashTime = Time.time;

            //Reset parkour values
            this.isParkouring = false; 
            this.parkourObstacle = null;

            //Turn back on ability activating
            this.ABCEntity.ToggleAbilityActivation(true);

            //turn off Block crosshair override
            this.ABCEntity.ToggleCrossHairOverrideBlock(false);



        }


#endregion


        // ********************* Animation Events ********************
#region Animation Events

        /// <summary>
        /// Animation event which will trigger a footstep 
        /// </summary>
        public void ABCFootStep() {
            this.MovementAudioHandler();
        }

#endregion

        // ********************* Public Methods ********************
#region Public Methods

        public void ConvertToGameType(ABC_GlobalPortal.GameType GameType) {

            switch (GameType) {

                case ABC_GlobalPortal.GameType.Action:

                    this.useLockOnMovement = false;
                    this.enableCrosshairMovement = false;
                    this.useCrosshairAnimation = false;
                    this.FPSMode = false;

                    this.allowDoubleJump = true;
                    this.enableLandingAnimations = true;
                    this.enableAdvancedRotation = true;
                    this.animateIntoSprint = true;
                    this.animateAfterSprint = true;
                    this.enableMaxAcceleratedAnimation = true;
                    this.enableSprintJumping = true;

                    break;
                case ABC_GlobalPortal.GameType.TopDownAction:
                case ABC_GlobalPortal.GameType.MOBA:
                case ABC_GlobalPortal.GameType.RPGMMO:

                    this.useLockOnMovement = false;
                    this.enableCrosshairMovement = false;
                    this.useCrosshairAnimation = false;
                    this.FPSMode = false;

                    this.allowDoubleJump = true;
                    this.enableLandingAnimations = true;
                    this.enableAdvancedRotation = true;
                    this.animateIntoSprint = true;
                    this.animateAfterSprint = true;
                    this.enableMaxAcceleratedAnimation = true;
                    this.enableSprintJumping = true;

                    break;
                case ABC_GlobalPortal.GameType.TPS:

                     
                    this.useLockOnMovement = false;                   

                    this.enableCrosshairMovement = true;
                    this.useCrosshairAnimation = true;
                    this.FPSMode = false;

                    this.allowDoubleJump = true;
                    this.enableLandingAnimations = true;
                    this.enableAdvancedRotation = true;
                    this.animateIntoSprint = true;
                    this.animateAfterSprint = true;
                    this.enableMaxAcceleratedAnimation = true;
                    this.enableSprintJumping = true;

                    break;
                case ABC_GlobalPortal.GameType.FPS:

                    this.useLockOnMovement = false;
                    this.enableCrosshairMovement = true;
                    this.useCrosshairAnimation = true;
                    this.FPSMode = true;

                    this.allowDoubleJump = false;
                    this.enableLandingAnimations = false;
                    this.enableAdvancedRotation = false;
                    this.animateIntoSprint = false;
                    this.animateAfterSprint = false;
                    this.enableMaxAcceleratedAnimation = false;
                    this.enableSprintJumping = false;


                    break;

            }
        }

        /// <summary>
        /// Will determine if the entity is currently moving
        /// </summary>
        /// <returns>True if entity is moving, else false</returns>
        public bool IsMoving() {

            if (this.allowMovement == false)
                return false; 

            if (this.inputX != 0 || this.inputZ != 0)
                return true;
            else
                return false; 


        }

        /// <summary>
        /// Will enable movement and rotation
        /// </summary>
        public void EnableMovement() {


            //Stop any current toggle movement calls incase one is mid delay
            StopCoroutine("ToggleMovement");

            //Enable movement after delay
            StartCoroutine(this.ToggleMovement(true, this.enableMovementDelay));

        }


        /// <summary>
        /// Will stop movement and rotation
        /// </summary>
        public void DisableMovement() {

            //Stop any current toggle movement calls incase one is mid delay
            StopCoroutine("ToggleMovement");

            //Disable movement without delay
            StartCoroutine(this.ToggleMovement(false, this.disableMovementDelay));
        }


        /// <summary>
        /// Will enable Gravity
        /// </summary>
        public void EnableGravity() {

            //Stop any current toggle movement calls incase one is mid delay
            StopCoroutine("ToggleGravity");

            //Enable movement after delay
            StartCoroutine(this.ToggleGravity(true));

        }


        /// <summary>
        /// Will disable gravity
        /// </summary>
        public void DisableGravity() {

            //Stop any current toggle movement calls incase one is mid delay
            StopCoroutine("ToggleGravity");

            //Enable movement after delay
            StartCoroutine(this.ToggleGravity(false));
        }


        /// <summary>
        /// Will lock on to the target gameobject
        /// </summary>
        /// <param name="Target">Object to lock on too</param>
        public void LockOnTarget(GameObject Target) {

            if (Target != this.gameObject)
                this.lockOnTarget = Target;
        }


        /// <summary>
        /// Will adjust move force (speed)
        /// </summary>
        /// <param name="SpeedAdjustment">Amount to adjust the speed by</param>
        public void AdjustMoveForce(float SpeedAdjustment) {

            //Add adjustment
            this.moveForceAdjustment += SpeedAdjustment;

        }

        /// <summary>
        /// Will set the weapon move force (speed) 
        /// </summary>
        /// <param name="SpeedAdjustment">Amount to adjust the speed by</param>
        public void SetWeaponMoveForce(float SpeedAdjustment) {

            //Add adjustment
            this.moveForceWeaponAdjustment = SpeedAdjustment;

        }

#endregion


        // ********************** Game ******************

#region Game

        void Start() {


            //Delcare animator
            Ani = this.GetComponentInChildren<Animator>();

            //Reset time recorder
            this.timeMovementLastDisabled = 0f;


            //Retrieve main camera
            Cam = Camera.main;

            //Determine character controller
            charController = this.GetComponentInChildren<CharacterController>();

            //If another collider added turn off collisions on character controller (checks if higher then 1 as character controller counts as a collider)
            if (meTransform.GetComponentsInChildren<Collider>().Where(col => col.enabled == true).Count() > 1) {
                this.charController.detectCollisions = false;
            } else {
                this.charController.detectCollisions = true;
            }


        }



        private void OnEnable() {

            this.ClearObjectPools();
            this.CreateObjectPool(GraphicType.AnimateIntoSprint);

            //Record entity transform
            this.meTransform = this.transform;

            //reset parkouring
            this.isParkouring = false;

            //reset jumping
            this.doubleJumpState = DoubleJumpState.DoubleJumpReady; 
            this.isJumping = false;
            this.lastJumpTime = 0;
            this.LastMovementTime = 0f;
            this.lastAdvanceRotatation = 0; 
            this.lastSprintDashTime = 0;
            this.acceleratedRotated = false;
            this.maxAccelerationTimeReached = 0;
            this.CurrentStepTag = "";
            this.walkingToggled = false;
            this.isSprinting = false;
            this.lastSprintDisabledTime = 0f;


            this.IntegrateWithABC();

            //reset speed adjustment
            this.moveForceAdjustment = 0f;
            this.moveForceWeaponAdjustment = 0f;


        }


        void Update() {

            //Check parkour handler, if parkour activates end update here
            if (this.ParkourHandler())
                return; 

            //Execute double jump handler
            this.DoubleJumpHandler();

            //Execute the jump handler
            this.JumpHandler();

            //Determine input and direction
            this.DetermineMoveDirection();

            //Rotate and move entity
            this.RotateAndMoveEntity();

            //Run animations
            this.Animate();

        }


        private void LateUpdate() {

            if (this.isParkouring)
                return;

            //If in FPS mode make sure rotation is updated in late update to fix any camera jitter
            if (this.ABCEntity.inIdleMode == false && (this.FPSMode == true || this.InCrossHairMode() == true))
                this.meTransform.rotation = Quaternion.Euler(this.meTransform.eulerAngles.x, Camera.main.transform.eulerAngles.y, this.meTransform.eulerAngles.z);

        }



            private void OnDisable() {

            if (this.ABCEventsSM != null) {
                this.ABCEventsSM.onEnableMovement -= this.EnableMovement;
                this.ABCEventsSM.onDisableMovement -= this.DisableMovement;

                this.ABCEventsSM.onEnableGravity -= this.EnableGravity;
                this.ABCEventsSM.onDisableGravity -= this.DisableGravity;
            }

            if (this.ABCEventsCont != null) {
                this.ABCEventsCont.onTargetSet -= this.LockOnTarget;
            }
        }


        void OnControllerColliderHit(ControllerColliderHit hit) {


            if (this.IsGrounded() && this.isJumping == false)
                this.CurrentStepTag = hit.gameObject.tag;


            //Check for parkour
            if (this.enableParkour == true && (this.parkourObstacle == null || hit.gameObject != this.parkourObstacle.gameObject) && hit.gameObject.GetComponent<ABC_ParkourObstacle>() != null) {


                //Ray from entity to see what obstacle infront
                Vector3 rayOrigin = ABCEntity.transform.position + new Vector3(0, 0.5f, 0);

                //Track what obstacle hit 
                RaycastHit obstacleHit;
                bool rayHit = Physics.Raycast(rayOrigin, transform.forward, out obstacleHit, 2f);

                //Try bit higher if false
                if (rayHit == false) {
                    rayHit = Physics.Raycast(rayOrigin + new Vector3(0, 1.5f, 0), transform.forward, out obstacleHit, 2f);
                }

                //Track what has been hit to potentially parkour
                if (rayHit == true) {

                    //Track what the parkour obstacle was that we found
                    this.parkourObjectRayHit = obstacleHit;      
                    this.parkourObstacle = hit.gameObject.GetComponent<ABC_ParkourObstacle>();
                } else {
                    //Nothing hit to null the tracker
                    this.parkourObstacle = null;
                }

      
            }

        }


#endregion


        // ********************** ENUMs ******************

#region Enums


        private enum GraphicType {
            AnimateIntoSprint
        }
        private enum DoubleJumpState {
            DoubleJumpReady = 0,
            IsDoubleJumping = 1,
            DoubleJumped = 2,
        }

        public enum ParkourType {
            Vault = 0, 
            LiftUp = 1, 
            Slide = 2
        }

#endregion
    }
}