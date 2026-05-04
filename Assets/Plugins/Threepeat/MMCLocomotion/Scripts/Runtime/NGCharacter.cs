using MxM;
using MxMGameplay;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace Threepeat {

    /// <summary>
    /// The primary Player Controller for MMLC
    /// </summary>
    [AddComponentMenu("Threepeat/NG Character")]
    public class NGCharacter : MonoBehaviour
    {
        public bool IsInitialized
        {
            get => this.isInitialized;
            protected set => isInitialized = value;
        }

        private bool isInitialized = false;

        [Header("General Configuration")]
        [Tooltip("Specifies how character should receive motion/behavior stimuli (e.g. InputManager, InputSystem, NavMesh, Spline)  If left blank, character will remain idle.  Can be modified at Runtime via SetInputScheme.")]
        [SerializeField] protected NGInputSchemeBase inputScheme;
        public NGInputSchemeBase InputScheme { get { return inputScheme; } set { SetInputScheme(value); } }

        public NGCharacterBaseConfig config;

        [Tooltip("Externally handle calling InitializeBehaviors to initialize this character.  This is useful if you're delaying the initialization of components MMLC depends on, e.g. if MxMAnimator is in diyPlayableGraph mode, it must be independently initialized and configured prior to this Character's InitializeBehaviors being called.")]
        public bool manuallyInitialize = false;

        [Tooltip("Wanted Animator IK layer.  Set to -1 (default) to use any layer, or match this to your Animator Controller's IK-pass-enabled layer index.")]
        public int IKLayer = -1;

        [Header("Component Refs")]

        public MxMAnimator mxmAnimator;

        public MxMTrajectoryGenerator mxmTrajectoryGenerator;

        //public LocomotionSpeedRamp mxmSpeedRamp;

        [Header("Input Profiles")]
        public MxMInputProfile inputProfileLocomotion;
        public MxMInputProfile inputProfileSprint;

        [HideInInspector]public MxMAnimationDecoupler decoupler = null;

        public MxMInputProfile inputProfileStrafe;

        public delegate void CustomUpdateMethod();

        protected List<CustomUpdateMethod> customUpdateMethods = new List<CustomUpdateMethod>();
        protected List<CustomUpdateMethod> customLateUpdateMethods = new List<CustomUpdateMethod>();

        public NGCharacter_AIHelper AI = new NGCharacter_AIHelper(null);
        public NGCharacter_AnimationHelper anim = new NGCharacter_AnimationHelper(null);
        public NGCharacter_MovementHelper movement = new NGCharacter_MovementHelper(null);

        public bool Sprinting
        {
            get => movement.Sprinting;
            set => movement.Sprinting = value;
        }

        public bool Strafing
        {
            get => movement.Strafing;
            set => movement.Strafing = value;
        }

        public bool Crouching
        {
            get => movement.Crouching;
            set => movement.Crouching = value;
        }

        public bool canRun
        {
            get => movement.canRun;
            set => movement.SetCanRun(value);
        }

        public bool canJump
        {
            get => movement.canJump;
            set => movement.canJump = value;
        }

        public float maxFallSpeed
        {
            get => movement.maxFallSpeed;
            set => movement.maxFallSpeed = value;
        }

        public bool lastLandingWasFromJump
        {
            get => movement.lastLandingWasFromJump;
            set => movement.lastLandingWasFromJump = value;
        }

        public void RegisterCustomUpdateMethod(CustomUpdateMethod meth)
        {
            customUpdateMethods.Add(meth);
        }

        public bool UnregisterCustomUpdateMethod(CustomUpdateMethod meth)
        {
            return customUpdateMethods.Remove(meth);
        }

        public void RegisterCustomLateUpdateMethod(CustomUpdateMethod meth)
        {
            customLateUpdateMethods.Add(meth);
        }

        public bool UnregisterCustomLateUpdateMethod(CustomUpdateMethod meth)
        {
            return customLateUpdateMethods.Remove(meth);
        }



        public void DoJump(bool jumpIsBigJump = false)
        {
            movement.DoJump(jumpIsBigJump);
        }

        public bool forceHardLandingOneShot
        {
            get => movement.forceHardLandingOneShot;
            set => movement.forceHardLandingOneShot = value;
        }


        public enum CharacterMode
        {
            None,
            Locomotion,
            Sprint,
            CombatRifleUp
        }


        /* A note regarding DedicatedEvent and CharacterState:  if your event or behavior is AvatarMasked (e.g. upper-body only), 
         * CharacterState should be set to the primary state for the rest of the body (e.g. Locomotion)
         */
        public enum CharacterState
        {
            Locomotion,
            Parkour,
            DedicatedEvent,         
            JumpingFalling,
            Landing,
            Shooter,
            Melee,
            Climbing
        }

        [Tooltip("")]
        public CharacterState currentState = CharacterState.Locomotion;

        [HideInInspector] public bool wasFaceDirectionOnIdleEnabled = false;


        [Tooltip("UnityEvent(bool) called whenever InputScheme is changed.  This will also be fired on initial assignment of InputScheme at startup or character creation.  The boolean parameter is true for initialAssignment and false for a change event.")]
        public UnityEvent<bool> onInputSchemeChanged = new UnityEvent<bool>();

        [HideInInspector] public MxMRootMotionApplicator rootMotionApplicator = null;
        protected MMCStriderWrapper striderWrapper = new MMCStriderWrapper();

        //private GenericControllerWrapper controllerWrapper;
        public NGCharacterControllerWrapper controllerWrapper;
        
        public bool debugMode = false;

        // users should never iterate this list more than is absolutely necessary, but should instead, find the
        // actions of interest and cache them for later use.
        //TODO: switch all strings and compares to string hashes.  Add int actionNameHash to ContextualAction class.
        public List<ContextualAction> registeredContextualActions = new List<ContextualAction>();

        public Dictionary<int, UnityEvent> namedEventHandlers = new Dictionary<int, UnityEvent>();

        public NGTagManager tagManager = new NGTagManager();

        public delegate void VoidCompletionCallback();

        public void RegisterNamedEventHandler(int nameStringHash, UnityAction action)
        {
            UnityEvent eventHandler;
            if (namedEventHandlers.TryGetValue(nameStringHash, out eventHandler))
            {
                eventHandler.AddListener(action);
            }
            else
            {
                eventHandler = new UnityEvent();
                eventHandler.AddListener(action);
                namedEventHandlers[nameStringHash] = eventHandler;
            }
        }

        public void UnregisterNamedEventHandler(int nameStringHash, UnityAction action)
        {
            UnityEvent eventHandler;
            if (namedEventHandlers.TryGetValue(nameStringHash, out eventHandler))
            {
                eventHandler.RemoveListener(action);
            }
        }

        public void FireNamedEvent(int nameStringHash)
        {
            UnityEvent eventHandler;
            if (namedEventHandlers.TryGetValue(nameStringHash, out eventHandler))
            {
                eventHandler.Invoke();
            }
        }

        // convenience functions (cache your string hash and use the above methods instead!):
        public void RegisterNamedEventHandler(string nameString, UnityAction action)
        {
            RegisterNamedEventHandler(Animator.StringToHash(nameString), action);
        }
        public void UnregisterNamedEventHandler(string nameString, UnityAction action)
        {
            UnregisterNamedEventHandler(Animator.StringToHash(nameString), action);
        }
        public void FireNamedEvent(string nameString)
        {
            FireNamedEvent(Animator.StringToHash(nameString));
        }
        // end convenience functions


        // Action names are not case sensitive and will be toLower()'d and hashed before any checks are done.
        public void RegisterContextualAction(string actionName, ContextualAction.CanPerformActionDelegate actionCanPerform, ContextualAction.PerformActionDelegate actionDoPerform)
        {
            //TODO: switch all strings and compares to string hashes.
            registeredContextualActions.Add(new ContextualAction(actionName.ToLower(), actionCanPerform, actionDoPerform));
        }

        public void UnregisterContextualAction(string actionName)
        {
            //TODO: switch all strings and compares to string hashes.
            string aname = actionName.ToLower();

            for (int ii=0; ii < registeredContextualActions.Count; ii++) 
            {
                if (registeredContextualActions[ii].actionName.Equals(aname))
                {
                    registeredContextualActions.RemoveAt(ii);
                    break;
                }
            }
        }

        public static bool ActionNamesMatch(string a1, string a2)
        {
            return a1.ToLower().Equals(a2.ToLower());
        }

        public bool FireContextualAction(string actionName)
        {
            ContextualAction act = GetContextualAction(actionName, debugMode);

            if ((act == null) || !act.canPerformAction(ContextualActionTrigger.TriggeringCondition.Other))
            {
                if (debugMode)
                {
                    Debug.LogFormat("act( {0} )", (act == null) ? "is null" : "can't perform action");
                }
                return false;
            }
            //Debug.Log("attempting to perform action");
            act.performAction(ContextualActionTrigger.TriggeringCondition.Other);
            return true;
        }

        public ContextualAction GetContextualAction(string actionName, bool doDebugLog = false)
        {
            //TODO: switch all strings and compares to string hashes.
            string aname = actionName.ToLower();

            foreach (ContextualAction act in registeredContextualActions)
            {
                if (doDebugLog)
                {
                    Debug.LogFormat("registered( {0} ) comparing to ( {1} )", act.actionName, aname);
                }
                if (aname.Equals(act.actionName))
                {
                    return act;
                }
             }
            return null;
        }

        private void OnDestroy()
        {
        }

        public void SetConfig(NGCharacterBaseConfig newConfig)
        {
            config = newConfig;

            if (config == null)
            {
                Debug.LogErrorFormat("config (NGCharacterConfigurationBase) not set in character( {0} ), this must be set for proper function.", gameObject.name);
            }

            if (!movement.Strafing)
            {
                mxmTrajectoryGenerator.FaceDirectionOnIdle = config.faceAwayFromCameraOnIdleInNonStrafeModes;
            }

            if (movement.Strafing)
            {
                if (movement.Crouching)
                {
                    movement.SetTrajectoryData(config.crouchStrafeSettings, inputProfileStrafe);
                }
                else
                {
                    movement.SetTrajectoryData(config.strafeSettings, inputProfileStrafe);
                }
            }
            else
            {
                if (movement.Crouching)
                {
                    movement.SetTrajectoryData(config.crouchSettings, inputProfileLocomotion);
                }
                else if (movement.Sprinting)
                {
                    movement.SetTrajectoryData(config.sprintSettings, inputProfileSprint);
                }
                else if (movement.canRun)
                {
                    movement.SetTrajectoryData(config.runSettings, inputProfileLocomotion);
                }
                else
                {
                    movement.SetTrajectoryData(config.walkSettings, inputProfileLocomotion);
                }
            }
        }


        public void ChangeModel(GameObject newModelObject)
        {
            Debug.Log($"ChangeModel called! - {newModelObject.name}");
            InitializeBehaviors(true);

            MxMRootMotionApplicator rma = newModelObject.GetComponent<MxMRootMotionApplicator>();

            controllerWrapper.Initialize();

            if (rma != null)
            {
                rma.ControllerWrapper = controllerWrapper;
            }

            striderWrapper.SetObjectContainingStrider(newModelObject);
        }

        public MxMAnimator FindMxMAnimator()
        {
            MxMAnimator[] anims = GetComponentsInChildren<MxMAnimator>();
            foreach (MxMAnimator anim in anims)
            {
                if (anim.isActiveAndEnabled)
                {
                    return anim;
                }
            }
            return null;
        }

        public void InitializeBehaviors(bool forceReassign = false)
        {
            if (AI == null)
            {
                AI = new NGCharacter_AIHelper(this);
            }

            if (anim == null)
            {
                anim = new NGCharacter_AnimationHelper(this);
            }

            if (movement == null)
            {
                movement = new NGCharacter_MovementHelper(this);
            }
            AI.character = anim.character = movement.character = this;
            AI.Initialize();
            anim.Initialize();
            movement.Initialize();

            if (forceReassign || (mxmAnimator == null))
            {
                mxmAnimator = FindMxMAnimator(); //transform.parent.GetComponentInChildren<MxMAnimator>();
            }
            if (forceReassign || (mxmTrajectoryGenerator == null))
            {
                mxmTrajectoryGenerator = mxmAnimator.GetComponent<MxMTrajectoryGenerator>();
            }

            decoupler = GetComponent<MxMAnimationDecoupler>();

            controllerWrapper = this.GetComponentInChildren<NGCharacterControllerWrapper>();
            if (controllerWrapper == null)
            {
                controllerWrapper = GetComponent<NGCharacterControllerWrapper>();
            }

            rootMotionApplicator = controllerWrapper.GetComponentInChildren<MxMRootMotionApplicator>();
            //controllerWrapper = charController.GetComponentInChildren<GenericControllerWrapper>();
            striderWrapper.SetObjectContainingStrider(mxmAnimator.gameObject);

            if (inputScheme != null)
            {
                /*if (inputScheme.GetType() == typeof(NGInputScheme_InputSystem))
                {
                    inputScheme = ScriptableObject.Instantiate<NGInputScheme_InputSystem>((NGInputScheme_InputSystem)inputScheme);
                }
                else {*/
                inputScheme = Instantiate(inputScheme);  
                //}
                
                inputScheme.Initialize(this, mxmTrajectoryGenerator);
                onInputSchemeChanged.Invoke(true);
            }

            if (config == null)
            {
                Debug.LogErrorFormat("config (NGCharacterConfigurationBase) not set in character( {0} ), this must be set for proper function.", gameObject.name);
            }

            mxmTrajectoryGenerator.FaceDirectionOnIdle = config.faceAwayFromCameraOnIdleInNonStrafeModes;

            tagManager.Initialize(mxmAnimator);

            if (!tagManager.initialized)
            {
                Debug.LogErrorFormat("User Tags not correct for character {0}.  Execution won't stop, but MMLC behavior is undefined in this situation.  This is due to either a) AnimData not being processed with correct MxMTagModule. b) NGCharacter.InitializeBehaviors being called prior to MxMAnimator setup completion.", this.name);
            }

            anim.InitializeBlendspaceLayers();

            isInitialized = true;
        }

        protected virtual void Start()
        {
            if (!manuallyInitialize)
            {
                InitializeBehaviors();
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            foreach (CustomUpdateMethod meth in customUpdateMethods)
            {
                meth();
            }

        }

        private void LateUpdate()
        {
            foreach (CustomUpdateMethod meth in customLateUpdateMethods)
            {
                meth();
            }
        }

        public  void UpdateInput()
        {
            if (inputScheme != null)
            {
                mxmTrajectoryGenerator.InputVector = inputScheme.GetUpdatedInputVector();
            }
        }

        public enum LocoState
        {
            Crouch,
            Walk,
            Run,
            Sprint
        }

        public void SetCharacterState(CharacterState newState)
        {
            currentState = newState;
            controllerWrapper.isJumpingOrFalling = newState == CharacterState.JumpingFalling;
        }

        public enum LandingType
        {
            Normal,
            HeavyForwardPathClear,      // This will cause a landing roll.
            HeavyForwardPathBlocked     // FUTURE-FEATURE: Use a responsive animation so character presses hands into wall or onto low platform when landing based on what the obstacle is.
        }

        protected SendMessageOptions smo = SendMessageOptions.DontRequireReceiver;
        
        public void EnableFootPlacement(float transitionTime = 0)
        {
            mxmAnimator.gameObject.SendMessage("EnableFootPlacement", transitionTime, smo);
        }

        public void DisableFootPlacement(float transitionTime)
        {
            
            mxmAnimator.gameObject.SendMessage("DisableFootPlacement", transitionTime, smo);
        }


        private float lastStriderToggleTime = -10f;
        private const float STRIDER_ENABLE_KEEPOUT_TIME = 2f;

        public void EnableStriderIfInUse()
        {
            if ((striderWrapper != null) && !striderWrapper.Enabled && ((Time.time - lastStriderToggleTime) >= STRIDER_ENABLE_KEEPOUT_TIME)) //&& (controllerWrapper.GetCurrentGroundSpeed() >= config.minSpeedForStrider))
            {
                //Debug.Log("calling enable");
                striderWrapper.EnableSmooth(0.5f);
                lastStriderToggleTime = Time.time;
            }
        }

        public void DisableStriderIfInUse()
        {
            if ((striderWrapper != null) && striderWrapper.Enabled && (striderWrapper.Weight >= 1f))
            {
                //Debug.Log("calling disable");
                striderWrapper.DisableSmooth(0.2f);
                lastStriderToggleTime = Time.time;
            }
        }

        /* STRAFESTRAFE (see above, search for STRAFESTRAFE to read about this capability)
        public void ToggleStrafing()
        {
            if (isStrafing)
            {
                Debug.Log("Strafe-STOP");
                //stop strafe
                mxmAnimator.RemoveRequiredTag("Strafe");
                mxmAnimator.SetCalibrationData("General");
                mxmAnimator.AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
                SetTrajectoryData(locoSpeed, locoMoveBias, locoDirBias, inputProfileLocomotion);
                mxmAnimator.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
                isStrafing = false;
                mxmTrajectoryGenerator.Strafing = false;
            }
            else
            {
                Debug.Log("Strafe-START");
                mxmAnimator.AddRequiredTag("Strafe");
                mxmAnimator.SetCalibrationData("Strafe");
                mxmAnimator.AngularErrorWarpMethod = EAngularErrorWarpMethod.TrajectoryFacing;
                SetTrajectoryData(strafeSpeed, strafeMoveBias, strafeDirBias, inputProfileStrafe);
                isStrafing = true;
                mxmAnimator.PastTrajectoryMode = EPastTrajectoryMode.CopyFromCurrentPose;
                mxmTrajectoryGenerator.Strafing = true;
            }
        }*/

        // Only set pCameraTransform for Player controller, otherwise motion will be relative to the camera (which isn't good).
        public void SetInputScheme(NGInputSchemeBase value, Transform pCameraTransform = null, bool destroyOldObject = true)
        {
            if (!Application.isPlaying)
            {
                inputScheme = value;
                return;
            }

            //TODO: all InputScheme change logic
            SetCameraTransform(pCameraTransform);

            if (inputScheme != null)
            {
                inputScheme.Deactivate();
                if (destroyOldObject)
                {
                    Destroy(inputScheme);
                }
                inputScheme = null;
            }

            if (value != null)
            {
                inputScheme = Instantiate(value);
                inputScheme.Initialize(this, mxmTrajectoryGenerator);
            }
            else
            {
                inputScheme = null;
            }

            onInputSchemeChanged.Invoke(false);
        }

        public void SetCameraTransform(Transform pCameraTransform)
        {
            mxmTrajectoryGenerator.RelativeCameraTransform = pCameraTransform;
        }

    }

}