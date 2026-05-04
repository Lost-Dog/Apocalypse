// Copyright (c) 2022, Threepeat, LLC.
using MxM;
using MxMGameplay;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threepeat
{
    public class NGAbilityParkour : MonoBehaviour
    {
        public enum PlayerRotationMethod
        {
            UsePlayerTransform,
            UseFrontAndBackEdges
        }

        public const string ACTIONNAME_PARKOUR = "Parkour";

        [Header("Config")]
        public NGParkourSettings config = null;

        public PlayerRotationMethod playerRotation = PlayerRotationMethod.UsePlayerTransform;

        public LayerMask layerMask = -1;

        public bool forceDesiredPlaybackSpeedToOne = true;
        public bool forceGravityEnableOnComplete = true;

        public bool debugEventsInSlowMotion = false;
        public float slowMotionTimeScale = 0.2f;

        public bool newDetectionMethod = true;

        public AutoParkourModeEnum AutoParkourMode
        {
            get { return autoParkourMode; }
            set
            {
                if (autoParkourMode != AutoParkourModeEnum.Disabled)
                {
                    if (value == AutoParkourModeEnum.Disabled)
                    {
                        character.UnregisterCustomUpdateMethod(CustomUpdate);
                    }
                }
                else if (value != AutoParkourModeEnum.Disabled)
                {
                    character.RegisterCustomUpdateMethod(CustomUpdate);
                }
                autoParkourMode = value;
            }
        }

        public float autoParkourCheckInterval = 0.25f;
        protected float autoParkourLastCheckTime = 0f;
        public enum AutoParkourModeEnum
        {
            Disabled,
            //AnyMovement,
            Sprint
        }

        public InputType inputType = InputType.InjectIntoJumpKey;

        public enum InputType
        {
            InjectIntoJumpKey,
            Manual
        }

        public bool debugMode = false;

        [Header("Real-time Debugging Information")]
        [Tooltip("Shown in Inspector for debugging purposes")]
        public string parkourCurrentVaultType = "";
        [Tooltip("Shown in Inspector for debugging purposes")]
        public GameObject parkourCurrentVaultObject = null;

        [Header("Events")]
        public UnityEvent onStart = new UnityEvent();
        public UnityEvent onComplete = new UnityEvent();

        // Private references and state
        //private CharacterController charController = null;
        [Header("Experimental Features")]
        [SerializeField, Tooltip("Very Experimental early-stage feature")] protected AutoParkourModeEnum autoParkourMode = AutoParkourModeEnum.Disabled;
        public string AutoParkourIsExtremelyExperimental = "Expect issues, all feedback appreciated!";

        private MxMTrajectoryGenerator mxmTrajectoryGenerator = null;

        // All of these will be found in children of the object containing the CharacterController
        private MxMAnimator mxmAnimator;
        private NGCharacter character;
        private NGCharacterControllerWrapper controllerWrapper;
        private MxMRootMotionApplicator rootMotionApplicator;
        protected MMCStriderWrapper striderWrapper = new MMCStriderWrapper();
        protected MMCEventIKManager eventIKMgr;

        // Internal:  Available Parkour Cababilities (From NGParkourSettings config)
        private bool canVault = false;          // vault over obstacle
        private bool canVaultOn = false;        // vault onto platform
        private bool canHighVault = false;      // fence clear
        private bool canHighVaultOn = false;    // mantle


        private bool handleEventUserTags = true;
        private float delayGravityTime = 0.1f;

        public Vector3 frontEdge; // contact 0 if vault/mantle or *-on
        public Vector3 frontEdgeNormal;
        public Vector3 backEdge; // contact 1 for long vaults, and contact 0 for any kind of drop
        public Vector3 groundPoint; // contact 1 for vault/mantle (not -on) 2-contact events
        public float vaultHeight;
        private bool gravityWasEnabled = false;
        private bool sprintWasEnabled = false;

        public MovementState movementState = MovementState.Running;

        public Vector3 playerFeetOffsetFromTransform = Vector3.zero;
        public Vector3 playerFeetWorldSpace = Vector3.zero;
        public MMCEventCheckCache eventCheckCache = new MMCEventCheckCache();

        private Coroutine executionCoroutine = null;

        public Vector3 FrontEdge { get { return frontEdge; } }
        public Vector3 BackEdge { get { return backEdge; } }
        public Vector3 FrontEdgeNormal { get { return frontEdgeNormal; } }

        public string[] activeBehaviors;

        protected Dictionary<string, MMCEventBehaviorParkour> availableBehaviors = new Dictionary<string, MMCEventBehaviorParkour>();

        public class ParkourEvent
        {
            public MMCEventBehaviorParkour behavior;
            public NGParkourSettings.ParkourCapability capability;
        }

        protected ParkourEvent[] activeEvents;


        public enum MovementState
        {
            Standing,
            Running
        }

        private void Start()
        {
            character = GetComponent<NGCharacter>();
            controllerWrapper = GetComponent<NGCharacterControllerWrapper>();
            mxmAnimator = controllerWrapper.GetComponentInChildren<MxMAnimator>();
            mxmTrajectoryGenerator = mxmAnimator.GetComponent<MxMTrajectoryGenerator>();
            if (mxmAnimator != null)
            {
                striderWrapper.SetObjectContainingStrider(mxmAnimator.gameObject);
                eventIKMgr = mxmAnimator.GetComponent<MMCEventIKManager>();
            }
            //controllerWrapper = charController.GetComponentInChildren<NGCharacterControllerWrapper>();
            rootMotionApplicator = mxmAnimator.GetComponent<MxMRootMotionApplicator>();

            //TODO-customer: How do you report/throw errors?  I'm just logging them, below:
            if (mxmAnimator == null)
            {
                Debug.LogErrorFormat("Can't find MxMAnimator component in children of {0}", controllerWrapper.name);
            }

            if (controllerWrapper == null)
            {
                Debug.LogErrorFormat("Can't find GenericControllerWrapper component in children of {0}", controllerWrapper.name);
            }

            if (rootMotionApplicator == null)
            {
                Debug.LogErrorFormat("Can't find MxMRootMotionApplicator component in children of {0}", controllerWrapper.name);
            }

            character.onInputSchemeChanged.AddListener(OnInputSchemeChangedListener);

            //OnInputSchemeChangedListener(false);

            Initialize();
        }

        private void OnInputSchemeChangedListener(bool isInitial)
        {
            /*if (isInitial)
            {
                return;
            }*/
            if (debugMode) Debug.LogFormat("OnInputSchemeChangeListener called( {0} )", isInitial);
            switch (inputType)
            {
                case InputType.InjectIntoJumpKey:
                    InjectActionIntoJumpKey();
                    break;

                case InputType.Manual:
                default:
                    break;
            }
        }

        private void InjectActionIntoJumpKey()
        {
            if (!character.InputScheme.IsInputDriven())
            {
                // NPC, nothing to do here.
                return;
            }
            NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)character.InputScheme;

            if (scheme.keyProcessorJumpParkour == null)
            {
                return;
            }

            List<ContextualActionTrigger> triggers = new List<ContextualActionTrigger>(scheme.keyProcessorJumpParkour.Triggers);

            if (!NGCharacter.ActionNamesMatch(triggers[0].actionName, ACTIONNAME_PARKOUR))
            {
                // parkour's not at top of this trigger, let's add it:
                triggers.Insert(0, new ContextualActionTrigger(ACTIONNAME_PARKOUR));
                scheme.keyProcessorJumpParkour.Triggers = triggers.ToArray();
            }
        }

        protected bool IsMatch(MMCEventBehaviorParkour behavior, NGParkourSettings.ParkourCapability cap)
        {
            string behaviorEventName = behavior.GetBehaviorUniqueName();
            return IsMatch(behaviorEventName, cap);
        }

        protected bool IsMatch(string behaviorEventName, NGParkourSettings.ParkourCapability cap)
        {
            if (cap.eventType == NGMxMEventDef.NGAnimationEventType.ByStringName)
            {
                return cap.eventTypeName.Equals(behaviorEventName);
            }
            return cap.eventType.ToString().Equals(behaviorEventName);
        }

        public MMCEventBehaviorParkour FindMatchingBehavior(NGParkourSettings.ParkourCapability cap)
        {
            // iterate over availableBehaviors and return entry whose GetBehaviorUniqueName matches cap.eventType string representation
            foreach (var behavior in availableBehaviors.Values)
            {
                if (IsMatch(behavior, cap))
                {
                    return behavior;
                }
            }

            // If no matching behavior is found, return null or throw an exception
            return null;
        }

        public NGParkourSettings.ParkourCapability FindMatchingCapability(MMCEventBehaviorParkour behavior)
        {
            string behaviorEventName = behavior.GetBehaviorUniqueName();
            foreach (NGParkourSettings.ParkourCapability cap in config.capabilities)
            {
                if (IsMatch(behaviorEventName, cap))
                {
                    return cap;
                }
            }
            return null;
        }

        private void Initialize()
        {
            List<System.Type> classes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => typeof(Threepeat.MMCEventBehaviorParkour).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .ToList(); // .Select(x => x.Name).ToList();

            List<MMCEventBehaviorParkour> tmpBehaviors = new List<MMCEventBehaviorParkour>();
            foreach (System.Type typeName in classes)
            {
                //tmpBehaviors.Add((MMCEventBehaviorParkour)System.Activator.CreateInstance(typeName));
                MMCEventBehaviorParkour bvr = (MMCEventBehaviorParkour)System.Activator.CreateInstance(typeName);
                availableBehaviors.Add(bvr.GetBehaviorUniqueName(), bvr);
            }

            //availableBehaviors = tmpBehaviors.OrderBy(o => o.GetDefaultPriority()).ToArray();

            /*foreach (MMCEventBehaviorParkour bvr in availableBehaviors)
            {
                Debug.LogFormat("Found available behavior: {0}", bvr.GetBehaviorUniqueName().ToLower());
            }*/

            List<string> tmpActive = new List<string>();

            List<ParkourEvent> tmpEvents = new List<ParkourEvent>();

            foreach (NGParkourSettings.ParkourCapability cap in config.capabilities)
            {
                MMCEventBehaviorParkour matchBehavior = FindMatchingBehavior(cap);

                if (matchBehavior != null)
                {
                    string behaviorName = matchBehavior.GetBehaviorUniqueName();
                    if (debugMode) Debug.LogFormat("parkour-add: {0}", behaviorName);
                    tmpActive.Add(behaviorName);
                    ParkourEvent evt = new ParkourEvent();
                    evt.behavior = matchBehavior;
                    SetupBehaviorLinks(evt.behavior);
                    evt.capability = cap;
                    tmpEvents.Add(evt);
                }
                else
                {
                    Debug.LogErrorFormat("parkour-add couldn't find matching MMCEventBehaviorParkour for {0}", cap.GetCapabilityName());
                }

                switch (cap.eventType)
                {
                    case NGMxMEventDef.NGAnimationEventType.parkour_vaultlow:
                        if (debugMode) Debug.Log("PARKOUR-CAPABILITY-ADDED: Vault");
                        canVault = true;
                        break;
                    case NGMxMEventDef.NGAnimationEventType.parkour_mantlelow:
                        if (debugMode) Debug.Log("PARKOUR-CAPABILITY-ADDED: Vault-On");
                        canVaultOn = true;
                        break;
                    case NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear:
                        if (debugMode) Debug.Log("PARKOUR-CAPABILITY-ADDED: Fence Clear");
                        canHighVault = true;
                        break;
                    case NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear_reallyhigh:
                        if (debugMode) Debug.Log("PARKOUR-CAPABILITY-ADDED: Fence Clear");
                        break;
                    case NGMxMEventDef.NGAnimationEventType.parkour_mantle:
                        if (debugMode) Debug.Log("PARKOUR-CAPABILITY-ADDED: Mantle");
                        canHighVaultOn = true;
                        break;
                    case NGMxMEventDef.NGAnimationEventType.parkour_mantle_reallyhigh:
                        if (debugMode) Debug.Log("PARKOUR-CAPABILITY-ADDED: MantleReallyHigh");
                        break;
                }
            }

            if (debugMode) Debug.LogFormat("Added {0} behaviors", tmpActive.Count);
            activeBehaviors = tmpActive.ToArray();

            activeEvents = tmpEvents.ToArray();

            character.RegisterContextualAction(ACTIONNAME_PARKOUR, ParkourAction_CanPerformAction, ParkourAction_PerformAction);

            if (autoParkourMode == AutoParkourModeEnum.Sprint)
            {
                character.RegisterCustomUpdateMethod(CustomUpdate);
            }
            OnInputSchemeChangedListener(true);

            if (eventIKMgr != null)
            {
                LoadIKEvents();
            }

            //Debug.LogError("Prior to release, remove: DEBUGONLY_MUST_REMOVE_newDetectionMethod ");
        }

        private void SetupBehaviorLinks(MMCEventBehaviorParkour behavior)
        {
            behavior.eventCheckCache = eventCheckCache;
            behavior.character = character;
            behavior.controllerWrapper = controllerWrapper;
            behavior.parkour = this;
            behavior.config = config;
        }

        private void LoadIKEvents()
        {
            int count = 0;
            foreach (NGParkourSettings.ParkourCapability cape in config.capabilities)
            {
                foreach (MMCAnimationClipInfo info in cape.runningEvent.animationClipInfos)
                {
                    eventIKMgr.AddClipEvents(info.clip, info);
                    count++;
                }
                foreach (MMCAnimationClipInfo info in cape.standingEvent.animationClipInfos)
                {
                    eventIKMgr.AddClipEvents(info.clip, info);
                    count++;
                }
            }
            if (debugMode) Debug.LogFormat("{0} IK events added.", count);
        }

        private void CustomUpdate()
        {
            if ((autoParkourMode == AutoParkourModeEnum.Sprint) && !character.mxmAnimator.IsEventPlaying &&
                (character.currentState == NGCharacter.CharacterState.Locomotion) &&
                character.Sprinting &&
                ((Time.time - autoParkourLastCheckTime) >= autoParkourCheckInterval))
            {
                autoParkourLastCheckTime = Time.time;
                DoParkourCheck(true);
            }


        }

        private void OnDestroy()
        {
            character.UnregisterContextualAction(ACTIONNAME_PARKOUR);
            character.UnregisterCustomUpdateMethod(CustomUpdate);
        }

        public bool ParkourAction_CanPerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            return DoParkourCheck(false);
        }

        public ContextualAction.Propagation ParkourAction_PerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            if ((passingEvent != null) && newDetectionMethod)
            {
                ExecuteParkourAction2(passingEvent);
            }
            else
            {
                ExecuteParkourAction();
            }
            return ContextualAction.Propagation.StopPropagationForAllSuccessiveEvents;
        }

        public void ExecuteParkourIfPossible()
        {
            DoParkourCheck(true);
        }


        private void CachePlayerFeetLocation()
        {
            playerFeetOffsetFromTransform.y = controllerWrapper.Center.y - (controllerWrapper.Height / 2.0f);
            playerFeetWorldSpace = controllerWrapper.transform.position + playerFeetOffsetFromTransform;
        }

        private void ClearEventCheckCache()
        {
            eventCheckCache.Clear();
        }

        protected ParkourEvent passingEvent;

        public bool DoParkourCheck2(bool executeParkourActionIfPossible = true)
        {
            ClearEventCheckCache();
            CachePlayerFeetLocation();

            MMCEventBehaviorParkour bhvr;

            //TODO-CURRENT
            /*
            foreach (string bhvrString in activeBehaviors)
            {
                if (availableBehaviors.TryGetValue(bhvrString, out bhvr))
                {
                    if (bhvr.CanPerformEvent())
                    {
                        if (executeParkourActionIfPossible)
                        {
                            ExecuteParkourAction2(bhvr);
                        }
                        return true;
                    }
                }

            }*/

            passingEvent = null;

            // iterate activeEvents and if CanPerformEvent returns true then return true
            foreach (var activeEvent in activeEvents)
            {
                if (activeEvent.behavior.CanPerformEvent())
                {
                    if (debugMode)
                    {
                        Debug.LogFormat("CAN-PARKOUR(true) - {0}", activeEvent.behavior.GetBehaviorUniqueName());
                    }
                    passingEvent = activeEvent;
                    if (executeParkourActionIfPossible)
                    {
                        ExecuteParkourAction2(activeEvent);
                    }
                    return true;
                }
                else if (debugMode)
                {
                    Debug.LogFormat("CAN-PARKOUR(false) - {0}", activeEvent.behavior.GetBehaviorUniqueName());
                }
            }

            return false;
        }

        private void ExecuteParkourAction2(ParkourEvent evt)
        {
            character.currentState = NGCharacter.CharacterState.Parkour;
            if (onStart != null)
            {
                onStart.Invoke();
            }
            character.FireNamedEvent(PARKOUR_START_HASH);

            gravityWasEnabled = controllerWrapper.gravityEnabled; //rootMotionApplicator.EnableGravity;

            
            NGMxMEventDef eventToRun = GetEvent(evt.capability, movementState); //SetupContactsAndPickSpecificEventToExecute();

            if (debugMode)
            {
                Debug.LogFormat("Running event: {0}", eventToRun.EventName);
            }

            evt.behavior.SetEventContacts(ref eventToRun);

            if ((striderWrapper != null) && striderWrapper.Enabled)
            {
                striderWrapper.DisableSmooth(20);
            }

            character.DisableFootPlacement(0);

            //Debug.LogFormat("PARKOUR:  executing event {0}", eventToRun.name);
            /*if (debugContactMode == DebugContactMode.UseForFrontEdgeAndBackEdge)
            {
                ShowFrontAndBackEdgeDebugContacts();
            }*/

            // check obstacle for FireSpecificParkour component
            /*FireSpecificParkour fsp = parkourCurrentVaultObject.GetComponent<FireSpecificParkour>();

            if (fsp == null)
            {*/
                evt.behavior.ExecuteEvent(ref eventToRun);
            /*}
            else
            {
                evt.behavior.ExecuteEvent(ref eventToRun, fsp.GetAnimationClip(eventToRun, mxmAnimator.FavourTags));
            }*/

            executionCoroutine = mxmAnimator.StartCoroutine(DoTheWork());
        }

        // DoParkourCheck is the actual entrypoint function for detecting/executing a parkour move.
        public bool DoParkourCheck(bool executeParkourActionIfPossible = true)
        {
            if (newDetectionMethod)
            {
                return DoParkourCheck2(executeParkourActionIfPossible);
            }

            if (debugMode) Debug.Log("Old Parkour check!");
            bool parkourable = false;

            ClearEventCheckCache();
            CachePlayerFeetLocation();

            // check if parkour is possible
            var motion = mxmTrajectoryGenerator.ExtractMotion(0.333f);

            if (!controllerWrapper.IsGrounded)
            {
                if (debugMode)
                {
                    Debug.Log("ParkourCheck early-out, because not grounded.");
                }
                return false;
            }

            Vector3 moveDir = motion.moveDelta / 0.333f; //player.characterLocomotion.GetMovementDirection();
            float moveAngle = 0f;
            float moveMag = moveDir.magnitude;

            if (moveMag >= 0.25)
            {
                moveAngle = Vector3.SignedAngle(
                    moveDir,
                    controllerWrapper.transform.TransformDirection(Vector3.forward), Vector3.up);
            }

            /*Debug.Log(string.Format("MOVEDIR: mag {0}, angToFwd {1}",
                moveDir.magnitude,
                Vector3.SignedAngle(
                    moveDir, 
                    player.transform.TransformDirection(Vector3.forward), Vector3.up)));*/

            // moveAngle:
            // -90 right
            // 90 left
            // 0 forward

            if ((moveMag < 0.25) || (Mathf.Abs(moveAngle) <= 50))
            {
                parkourable = CheckForward();
            }
            /*else
            {
                // Available option: I have a side-vault capability that I haven't put into MxM yet, but can be ported in (especially useful to vault low obstacles while aiming or strafing)
                //return CheckSides(charController, moveAngle);
            }*/

            if (parkourable && executeParkourActionIfPossible)
            {
                // execute the parkour action
                //Debug.LogFormat("Do parkour type({0}) over/onto {1}!!!", parkourCurrentVaultType, parkourCurrentVaultObject.name);
                ExecuteParkourAction();
            }

            return parkourable;
        }

        protected static int PARKOUR_START_HASH = Animator.StringToHash("PARKOUR_START");
        protected static int PARKOUR_COMPLETE_HASH = Animator.StringToHash("PARKOUR_COMPLETE");


        private void ExecuteParkourAction()
        {
            character.currentState = NGCharacter.CharacterState.Parkour;
            if (onStart != null)
            {
                onStart.Invoke();
            }
            character.FireNamedEvent(PARKOUR_START_HASH);

            CachePlayerFeetLocation();



            gravityWasEnabled = controllerWrapper.gravityEnabled; //rootMotionApplicator.EnableGravity;
            NGMxMEventDef eventToRun = SetupContactsAndPickSpecificEventToExecute();

            if ((striderWrapper != null) && striderWrapper.Enabled)
            {
                striderWrapper.DisableSmooth(20);
            }

            character.DisableFootPlacement(0);

            //Debug.LogFormat("PARKOUR:  executing event {0}", eventToRun.name);
            /*if (debugContactMode == DebugContactMode.UseForFrontEdgeAndBackEdge)
            {
                ShowFrontAndBackEdgeDebugContacts();
            }*/

            mxmAnimator.BeginEvent(eventToRun);

            executionCoroutine = mxmAnimator.StartCoroutine(DoTheWork());

        }

        public void ExecuteSpecificParkourEvent(NGMxMEventDef eventToRun)
        {
            character.currentState = NGCharacter.CharacterState.Parkour;
            if (onStart != null)
            {
                onStart.Invoke();
            }
            character.FireNamedEvent(PARKOUR_START_HASH);
            CachePlayerFeetLocation();

            gravityWasEnabled = controllerWrapper.gravityEnabled; //rootMotionApplicator.EnableGravity;
                                                                  //NGMxMEventDef eventToRun = SetupContactsAndPickSpecificEventToExecute();
            if ((striderWrapper != null) && striderWrapper.Enabled)
            {
                striderWrapper.DisableSmooth(20);
            }

            character.DisableFootPlacement(0);

            //Debug.LogFormat("PARKOUR:  executing event {0}", eventToRun.name);
            /*if (debugContactMode == DebugContactMode.UseForFrontEdgeAndBackEdge)
            {
                ShowFrontAndBackEdgeDebugContacts();
            }*/
            if (debugMode) Debug.LogFormat("favour multiplier: {0}", mxmAnimator.FavourMultiplier);
            //mxmAnimator.SetFavourMultiplier(0.2f);
            mxmAnimator.BeginEvent(eventToRun);

            executionCoroutine = mxmAnimator.StartCoroutine(DoTheWork());

        }


        /***********************************************************************
         * 
         *  PARKOUR CHECK HELPER FUNCTIONS
         * 
         ***********************************************************************/

        private bool CheckForward()
        {
            string vaultType = "vault";
            RaycastHit hitForward, hitForward2, hitForward3;
            float FORWARD_DISTANCE = GetForwardDistance();


            //float maxDist = FORWARD_DISTANCE / 0.7071f; // ray is cast at 45 degree angle (MB-TODO: change to use cc max slope)

            bool kneeLevelForwardClear =
                //!Physics.Raycast(
                !Physics.SphereCast(
                        playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * config.kneeLevelObstacleCheckHeightFactor,
                        0.2f,
                        controllerWrapper.transform.TransformDirection(Vector3.forward),  //(charController.transform.forward + Vector3.up).normalized, 
                        out hitForward,
                        FORWARD_DISTANCE, //maxDist,
                        layerMask,
                        QueryTriggerInteraction.Ignore);

            //Debug.LogFormat("CHECKFORWARD: kneeLevelFwdClear {0}", kneeLevelForwardClear ? "CLEAR" : "BLOCKED");

            if (!kneeLevelForwardClear)
            {
                // check for ramp:
                if (!CheckForObstacle_RampCheck(hitForward.distance))
                {
                    if (debugMode)
                    {
                        Debug.LogFormat("Parkour Check: Skipping (incline): distToKneeHit( {0} )", hitForward.distance);
                    }
                    return false;
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.LogFormat("Parkour Check: Incline check OK: distToKneeHit( {0} )", hitForward.distance);
                    }
                }


                bool chestLevelForwardClear =
                        //!Physics.Raycast(
                        !Physics.SphereCast(
                                playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * 0.7f,
                                0.1f,
                                controllerWrapper.transform.TransformDirection(Vector3.forward),
                                out hitForward2,
                                FORWARD_DISTANCE,
                                layerMask,
                                QueryTriggerInteraction.Ignore);

                if (chestLevelForwardClear)
                {
                    parkourCurrentVaultType = vaultType;
                    parkourCurrentVaultObject = hitForward.collider.gameObject;
                    return true;
                }
                Vector3 mantleOrigin = playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * config.mantleCheckCharacterHeightFactor;
                bool canMantle =
                    !Physics.Raycast(
                            mantleOrigin,
                            controllerWrapper.transform.TransformDirection(Vector3.forward),
                            out hitForward3,
                            FORWARD_DISTANCE,
                            layerMask,
                            QueryTriggerInteraction.Ignore);

                if (debugMode)
                {
                    Debug.DrawLine(mantleOrigin, mantleOrigin + controllerWrapper.transform.TransformDirection(Vector3.forward) * 2f, Color.red, 5.0f);
                }

                if (canMantle)
                {
                    //Debug.Log("setting high mantle");
                    vaultType = "highMantle";

                    parkourCurrentVaultType = vaultType;
                    parkourCurrentVaultObject = hitForward2.collider.gameObject;
                    return true;
                } /*
                else
                {
                    Debug.LogFormat("no-mantle, collision: {0}, factor: {1}",
                            hitForward2.collider.name,
                            config.mantleCheckCharacterHeightFactor);
                }*/
            }
            return false;
        }


        public float GetForwardDistance()
        {
            float currSpeed = controllerWrapper.GetCurrentGroundSpeed();

            if (currSpeed > 4)
            {
                return 2.5f;
            }
            else if (currSpeed > 2)
            {
                return 2f;
            }
            return 1.5f;
        }


        /***********************************************************************
         * 
         *  PARKOUR EXECUTION FUNCTIONS
         * 
         ***********************************************************************/

        private NGMxMEventDef SetupContactsAndPickSpecificEventToExecute()
        {
            NGMxMEventDef eventToRun;

            NGMxMEventDef.NGAnimationEventType parkourType = NGMxMEventDef.NGAnimationEventType.parkour_vaultlow;

            //TODO: handle strafe vaults
            //TODO: handle drops
            //TODO: handle windows


            NGMxMEventDef.NGAnimationEventType properOverType = NGMxMEventDef.NGAnimationEventType.parkour_vaultlow;
            NGMxMEventDef.NGAnimationEventType properOnType = NGMxMEventDef.NGAnimationEventType.parkour_mantlelow;
            float heightMultiplier = config.kneeLevelObstacleCheckHeightFactor; //0.25f;
            float downRayMultiplier = config.mantleCheckCharacterHeightFactor;
            switch (parkourCurrentVaultType)
            {
                case "highMantle":
                    //Debug.LogFormat("HIGH-MANTLE-CASE");
                    properOverType = NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear;
                    properOnType = NGMxMEventDef.NGAnimationEventType.parkour_mantle;
                    heightMultiplier = 0.6f;
                    break;
                case "vault":
                    downRayMultiplier = 1.0f;
                    break;
                default:
                    //already set by default, above
                    break;
            }

            //bool isOverNotOn = true; // over: {vault, windowVault, fenceClear} on: {vaultOn, mantle}

            float distToObject = 0f;
            float FORWARD_DISTANCE = GetForwardDistanceAndSetMovementState();
            RaycastHit hitForward;
            Vector3 rayDirection = Vector3.forward;

            bool kneeHit = false;
            bool headHit = false;

            // get the frontEdge location x/z
            Vector3 playerKneecaps = playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * heightMultiplier;
            if (Physics.SphereCast(//Physics.Raycast(
                            playerKneecaps,
                            0.2f,
                            controllerWrapper.transform.TransformDirection(rayDirection), //Vector3.forward),
                            out hitForward,
                            FORWARD_DISTANCE,
                            layerMask,
                            QueryTriggerInteraction.Ignore))
            {
                kneeHit = true;
                distToObject = hitForward.distance;
            }

            Vector3 downVecToFindTopOrigin = hitForward.point + controllerWrapper.transform.forward * 0.15f;
            downVecToFindTopOrigin.y = playerFeetWorldSpace.y + controllerWrapper.Height * downRayMultiplier * 1.5f;

            //TODO: do head-level forward spherecast to see if this is a gap that can be vaulted/mantled
            //TODO: if we end up mantling into a space we can't stand, should automatically crouch and adjust 
            // character controller height

            RaycastHit hitForward2;

            Vector3 playerHead = playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * config.headLevelObstacleCheckHeightFactor;
            if (!Physics.SphereCast(//Physics.Raycast(
                playerHead,
                0.15f,
                controllerWrapper.transform.TransformDirection(rayDirection),
                out hitForward2,
                FORWARD_DISTANCE * 1.25f,
                layerMask,
                QueryTriggerInteraction.Ignore))
            {
                // this is a low vault/mantle for sure.
                downVecToFindTopOrigin.y = playerHead.y + 0.2f;
            }
            else
            {
                headHit = true;
            }

            if (debugMode)
            {
                Debug.LogFormat("DEBUG - RAY HIT: KNEE( {0} ) HEAD( {1} )", kneeHit, headHit);
            }


            RaycastHit hitDown;
            bool downRayHitSomething = false;
            if (Physics.SphereCast(
                    downVecToFindTopOrigin,
                    0.15f,
                    Vector3.down,
                    out hitDown,
                    controllerWrapper.Height * downRayMultiplier * 0.8f,
                    layerMask,
                    QueryTriggerInteraction.Ignore))
            {
                downRayHitSomething = true;
                if (debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: Object hit by down ray {0} (prior hit object was {1}", hitDown.collider.name, parkourCurrentVaultObject.name);
                }
                if ((parkourCurrentVaultObject.GetInstanceID() != hitDown.collider.gameObject.GetInstanceID()) &&
                    parkourCurrentVaultType.Equals("highMantle"))
                {
                    // need to make sure backEdge gets set properly
                }
                float hitHeightAbovePlayerFeet = hitDown.point.y - playerFeetWorldSpace.y;

                if (hitHeightAbovePlayerFeet >= config.mantleReallyHighThreshold)
                {
                    if (debugMode)
                    {
                        Debug.Log("Really High Mantle");
                    }
                    parkourCurrentVaultType = "reallyHighMantle";
                    properOverType = NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear_reallyhigh;
                    properOnType = NGMxMEventDef.NGAnimationEventType.parkour_mantle_reallyhigh;
                }

                parkourCurrentVaultObject = hitDown.collider.gameObject;
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: DOWNHIT MISSED: mult( {0} )", downRayMultiplier);
                }
            }

            //Debug.LogFormat("object hit by forward ray {0}", hitForward.collider.name);
            frontEdge = hitForward.point; // x and z values are final, still need y-val for top of the object
            frontEdgeNormal = hitForward.normal;
            Vector3 vecPlayerToCover = hitForward.point - playerKneecaps;
            bool success = PopulateFrontAndBackEdges(
                    hitForward,
                    downRayHitSomething ? hitDown : null,
                    vecPlayerToCover);

            float actualPlatformDepth = (frontEdge - backEdge).magnitude;
            float maxPlatformDepth = 1f;

            List<NGParkourSettings.ParkourCapability> potentialOvers = new List<NGParkourSettings.ParkourCapability>();

            bool canBeOver = false;
            float tmpPlatDepth;

            //TODO: add standing/running check and only use appropriate one

            //TODO: do this in Start and pre-compute all the max depths
            // figure-out over vs on:
            // 1. checking length and if length is > max for any of the over events in config then it's "on"
            foreach (NGParkourSettings.ParkourCapability cap in config.capabilities)
            {
                if (cap.eventType == properOverType)
                {
                    tmpPlatDepth = GetPlatformDepthFromCapability(cap, movementState);
                    maxPlatformDepth = Mathf.Max(tmpPlatDepth, maxPlatformDepth);
                    if (tmpPlatDepth >= actualPlatformDepth)
                    {
                        potentialOvers.Add(cap);
                        canBeOver = true;
                    }
                }
            }

            // 2. if still possibly an over, cast ray
            if (canBeOver)
            {
                RaycastHit hitClearCheck;
                Vector3 ONE_FOOT_UP = Vector3.up * 0.3f;
                //TODO: cast ray 1ft above 
                Vector3 direction = backEdge - frontEdge;
                if (Physics.Raycast(
                            frontEdge + ONE_FOOT_UP,
                            direction,
                            out hitClearCheck,
                            direction.magnitude,
                            layerMask,
                            QueryTriggerInteraction.Ignore))
                {
                    // ray 1 ft above platform from front to back hit something, so this can't be an over.
                    //Debug.Log("CANT-BE-OVER-REASON: 1ft-ray hit something");
                    canBeOver = false;
                }
            }/*
            else
            {
                Debug.LogFormat("CANT-BE-OVER-REASON: no potentials in config: actualPlatDepth( {0} ) frontEdge( {1} ), backEdge( {2} )",
                    actualPlatformDepth,
                    frontEdge,
                    backEdge);
            }*/

            // Get specific event

            if (canBeOver)
            {
                // This is actually an Over
                parkourType = properOverType;
                //Debug.LogFormat("PARKOURTYPE(Over): {0}", parkourType);
                eventToRun = GetEvent(parkourType, movementState);
            }
            else
            {
                // This is an On.
                parkourType = properOnType;
                //Debug.LogFormat("PARKOURTYPE(On): {0}", parkourType);
                eventToRun = GetEvent(parkourType, movementState);
            }

            // setup contacts:
            //TODO: support window vault and drops
            SetupContacts(ref eventToRun);
            /*TODO:
            if (vaultTypeString.Contains("highMantle"))
            {
                SetupHighMantle(hitForward);
                clipList = config.idleMantleClips;
                vaultLength = VaultLength.HighMantle;
            }
            else*/
            {

            }

            return eventToRun;
        }

        public float GetForwardDistanceAndSetMovementState(bool setState = true)
        {
            float currSpeed = controllerWrapper.GetCurrentGroundSpeed(); // player.characterState.forwardSpeed.magnitude;


            if (setState) { movementState = MovementState.Standing; }

            if (currSpeed >= 4.5)
            {

                if (setState) { movementState = MovementState.Running; }
                return 3.25f;
            }
            if (currSpeed >= 3)
            {
                if (setState) { movementState = MovementState.Running; }
                return 2.5f;
            }
            else if (currSpeed > 2)
            {
                movementState = MovementState.Standing;
                return 2f;
            }
            movementState = MovementState.Standing;
            return 1.5f;
        }

        // GetOppositeSideOfObject: sets frontEdge.y, backEdge (XYZ), vaultHeight
        // whether this is Over-or-On will be determined after calling this function by:
        // 1. checking length and if length is > max for any of the over events in config then it's "on"
        // 2. if still possibly an over, cast ray
        //		from frontEdge + Vector3.up*0.333f to backEdge + Vector3.up*0.333f and if there's a hit, then it's "on"
        //		and it's "over" otherwise.
        //
        public bool PopulateFrontAndBackEdges(
                RaycastHit kneecapForwardHit,
                RaycastHit? downwardHit,
                Vector3 vecPlayerToCover)
        {
            // I need this function to find the back-edge of the object as opposed to the landing point.
            // landing point will be found by another down-Ray beyond the end a certain distance based on ContactInfo's
            // Get opposite side of cover bounds
            //Collider bc = vaultObj.GetComponent<Collider>();
            Collider bc = kneecapForwardHit.collider; //.GetComponent<Collider>();
            bool useDownwardHit = downwardHit.HasValue;


            Vector3 vecPlayerToBCCenter = bc.bounds.center - playerFeetWorldSpace;
            Vector3 vecPlayerToBC = Vector3.Project(vecPlayerToBCCenter, vecPlayerToCover);

            Ray ray = new Ray();
            float vp2cMultiplier = 20f * Mathf.Max(5f, vecPlayerToCover.magnitude);

            ray.origin = /*bc.transform.position*/ kneecapForwardHit.point + vecPlayerToCover * vp2cMultiplier; //vecPlayerToBC*3f;
                                                                                                                //ray.origin = new Vector3(ray.origin.x, bc.bounds.min.y + bc.bounds.extents.y / 2.0f, ray.origin.z);
            /*if (useDownwardHit)
            {
                Vector3 offset = new Vector3(0f, downwardHit.Value.point.y - ray.origin.y - 0.1f, 0f);
                ray.origin += offset;
            }*/
            ray.direction = -vecPlayerToCover;
            RaycastHit hit;

            if (!bc.Raycast(ray, out hit, vp2cMultiplier * vecPlayerToCover.magnitude)) //vecPlayerToBC.magnitude*3f))
            {
                return false;
            }

            backEdge = hit.point;


            bool simpleOldBoundsWayThatDoesntWorkOnNonAABBColliders = false;

            if (simpleOldBoundsWayThatDoesntWorkOnNonAABBColliders)
            {
                frontEdge.y = bc.bounds.max.y;
                backEdge.y = bc.bounds.max.y;
                vaultHeight = bc.bounds.max.y - playerFeetWorldSpace.y;
            }
            else if (useDownwardHit)
            {
                //Debug.Log("using downward hit!");
                RaycastHit hdown = downwardHit.Value;

                float yval = hdown.point.y;
                frontEdge.y = yval;
                backEdge.y = yval;
                vaultHeight = yval - playerFeetWorldSpace.y;
            }
            else
            {
                ray.origin = new Vector3(frontEdge.x, bc.bounds.max.y + 1f, frontEdge.z);
                ray.direction = Vector3.down;
                if (bc.Raycast(ray, out hit, (1f + bc.bounds.max.y - bc.bounds.min.y) * 2f))
                {
                    frontEdge.y = hit.point.y;
                    backEdge.y = hit.point.y;
                    vaultHeight = hit.point.y - playerFeetWorldSpace.y;
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.LogError("RAYCAST MISSED");
                    }
                    frontEdge.y = bc.bounds.max.y;
                    backEdge.y = bc.bounds.max.y;
                    vaultHeight = bc.bounds.max.y - playerFeetWorldSpace.y;
                }
            }
            //Debug.LogFormat("VaultHeight[ {0} ]", vaultHeight);

            return true;
        }

        public float GetPlatformDepthFromCapability(NGParkourSettings.ParkourCapability cap, MovementState movementState)
        {
            if ((movementState == MovementState.Running) && (cap.runningEvent != null))
            {
                return cap.runningEvent.maxPlatformDepth;
            }
            else if ((movementState == MovementState.Standing) && (cap.standingEvent != null))
            {
                return cap.standingEvent.maxPlatformDepth;
            }
            else if (cap.runningEvent != null)
            {
                return cap.runningEvent.maxPlatformDepth;
            }
            else if (cap.standingEvent != null)
            {
                return cap.standingEvent.maxPlatformDepth;
            }

            return 0f;
        }

        private NGMxMEventDef GetEvent(NGParkourSettings.ParkourCapability cap, MovementState movementState)
        {
            NGMxMEventDef theEvent = movementState == MovementState.Running ? cap.runningEvent : cap.standingEvent;

            if (theEvent == null) 
            { 
                theEvent = movementState == MovementState.Standing ? cap.runningEvent : cap.standingEvent;
            }

            return theEvent;
        }

        private NGMxMEventDef GetEvent(NGMxMEventDef.NGAnimationEventType parkourType, MovementState movementState)
        {
            // TODO do all the "getting" in start and cache all the results for quick turn.

            //TODO: add randomization (support for alternates)
            NGMxMEventDef theEvent = null;
            NGMxMEventDef backupEvent = null;


            foreach (NGParkourSettings.ParkourCapability cap in config.capabilities)
            {
                if (cap.eventType == parkourType)
                {
                    if (theEvent == null)
                    {
                        theEvent = movementState == MovementState.Running ? cap.runningEvent : cap.standingEvent;
                    }

                    if (backupEvent == null)
                    {
                        backupEvent = movementState == MovementState.Standing ? cap.runningEvent : cap.standingEvent;
                    }

                    if (theEvent != null)
                    {
                        break;
                    }
                }
            }
            return theEvent;
        }

        public bool CheckForObstacle_RampCheck(float distanceToKneecapHit)
        {
            Vector3 forwardVec = controllerWrapper.transform.forward;

            // Check for Obstacle
            RaycastHit hit;
            float angMult = 0.7f;

            // change start point of ray based on speed.
            //float forwardDistToStart = (controllerWrapper.GetCurrentGroundSpeed() - 2f)/3f; 
            Vector3 originLoc = playerFeetWorldSpace + Vector3.up * config.inclineCheckOriginHeightFactor; /*+ forwardVec * forwardDistToStart*/;
            //TODO: handle standing still case

            float maxDist = config.inclineCheckMaxDist / 0.7071f; // ray is cast at 45 degree angle (MB-TODO: change to use cc max slope)

            if (debugMode)
            {
                Debug.DrawLine(originLoc, playerFeetWorldSpace + (forwardVec + angMult * Vector3.up).normalized * maxDist, Color.red, 5f);
            }

            //if (Physics.Raycast(transform.position + Vector3.up*1.2f, LinearInputVector, out hit, maxDist, -1, QueryTriggerInteraction.Ignore))
            //return (distanceToKneecapHit > config.parkourCheckMaxDistToKneeCollision) || Physics.Raycast(originLoc, (forwardVec + angMult*Vector3.up).normalized, out hit, maxDist, layerMask, QueryTriggerInteraction.Ignore);
            return (distanceToKneecapHit > config.parkourCheckMaxDistToKneeCollision) || eventCheckCache.SphereCast((int)MMCEventCheckCache.CollisionCastEnum.ObstacleRampCheck, originLoc, config.inclineCheckRadius, (forwardVec + angMult * Vector3.up).normalized, out hit, maxDist, layerMask, QueryTriggerInteraction.Ignore);
        }

        // internal method, called by SetupContactsAndPickSpecificEventToExecute, you don't want this one.
        private void SetupContacts(ref NGMxMEventDef eventToRun)
        {
            int contactCount = Mathf.Max(eventToRun.ContactCountToMatch, eventToRun.ContactCountToWarp);
            float playerAngle = 0f;

            //if (eventToRun.RotationWarpType != EEventWarpType.None)
            {
                playerAngle = controllerWrapper.transform.rotation.eulerAngles.y;
                if (playerRotation == PlayerRotationMethod.UseFrontAndBackEdges)
                {
                    playerAngle = Quaternion.LookRotation(backEdge - frontEdge).eulerAngles.y;
                }
            }

            //Debug.LogFormat("CONTACT_COUNT = {0}, playerAngle = {1}", contactCount, playerAngle);

            Vector3 frontEdgeContact = frontEdge - playerFeetOffsetFromTransform;

            if (config.applyModelSpecificMultipliers)
            {
                Vector3 origin = playerFeetWorldSpace;

                frontEdgeContact =
                        new Vector3(
                                Mathf.LerpUnclamped(origin.x, frontEdge.x, config.gaitMultiplier),
                                Mathf.LerpUnclamped(origin.y, frontEdge.y /*+ playerFeetOffsetFromTransform.y*/, config.heightMultiplier),
                                Mathf.LerpUnclamped(origin.z, frontEdge.z, config.gaitMultiplier));
            }

            eventToRun.ClearContacts();
            if (contactCount == 0)
            {
                return;
            }
            else if (contactCount == 1)
            {
                if (debugMode)
                {
                    Debug.LogFormat("eventType: {0}", eventToRun.NGEventType.ToString());
                }
                if ((eventToRun.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear) ||
                    (eventToRun.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_mantle) ||
                    (eventToRun.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear_reallyhigh) ||
                        (eventToRun.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_mantle_reallyhigh))
                {
                    //Vector3 normalVec = frontEdge - frontEdgeNormal;
                    eventToRun.AddEventContact(frontEdgeContact, Quaternion.LookRotation(-frontEdgeNormal).eulerAngles.y);
                }
                else
                {
                    //available-feature: I have running and walking animated drops (if running, either hop down to the lower level or drop into a slide to slide off platform to the ground)
                    eventToRun.AddEventContact(frontEdgeContact, playerAngle);
                }
            }
            else if (contactCount == 2)
            {
                eventToRun.AddEventContact(frontEdgeContact, playerAngle);
                eventToRun.AddEventContact(GetLandingSpot(eventToRun), playerAngle);
            }
            else if (contactCount == 3)
            {
                eventToRun.AddEventContact(frontEdgeContact, playerAngle);
                eventToRun.AddEventContact(backEdge, playerAngle);
                eventToRun.AddEventContact(GetLandingSpot(eventToRun), playerAngle);
            }
        }

        public Vector3 GetLandingSpot(NGMxMEventDef eventToRun)
        {

            // find z-difference between 2nd-to-last and last contact:

            NGMxMEventDef.ContactInfo ctLast, ct2nd;
            ctLast = eventToRun.contactInfos[eventToRun.contactInfos.Length - 1];
            ct2nd = eventToRun.contactInfos[eventToRun.contactInfos.Length - 2];

            if ((ctLast == null) || (ct2nd == null))
            {
                //Debug.LogErrorFormat("ContactInfo's not set properly in NGMxMEventDef( {0} ), can't find landing spot",
                //        eventToRun.name);
                return backEdge;
            }

            float zDist = ctLast.relativePosition.z - ct2nd.relativePosition.z;

            Vector3 potentialLandingSpotOffGround = backEdge + controllerWrapper.transform.forward * zDist;

            RaycastHit hitDown;

            // throw a downward ray based on that distance from backEdge

            if (Physics.Raycast(
                    potentialLandingSpotOffGround,
                    Vector3.down,
                    out hitDown,
                    (frontEdge.y - playerFeetWorldSpace.y) * 1.5f,
                    layerMask,
                    QueryTriggerInteraction.Ignore))
            {
                // found the ground!
                return hitDown.point;
            }
            else
            {
                // didn't find the ground
                return new Vector3(potentialLandingSpotOffGround.x, playerFeetWorldSpace.y, potentialLandingSpotOffGround.z);
            }
        }

        private float timeScaleWas = 1.0f;

        private IEnumerator DoTheWork()
        {
            if (debugEventsInSlowMotion)
            {
                timeScaleWas = Time.timeScale;
                Time.timeScale = slowMotionTimeScale;
            }

            // check and remove sprint tag as needed
            sprintWasEnabled = character.Sprinting;
            if (forceDesiredPlaybackSpeedToOne)
            {
                mxmAnimator.DesiredPlaybackSpeed = 1;
                mxmAnimator.PlaybackSpeed = 1;
            }
            if (sprintWasEnabled)
            {
                mxmAnimator.RemoveFavourTag("Sprint");
            }

            while (mxmAnimator.IsEventPlaying && !mxmAnimator.IsEventComplete)
            {
                //Debug.LogFormat("PLAYBACK_SPEED: {0}", mxmAnimator.PlaybackSpeed);
                if (forceDesiredPlaybackSpeedToOne && mxmAnimator.PlaybackSpeed < 1)
                {
                    mxmAnimator.PlaybackSpeed = 1;
                }

                if (gravityWasEnabled && handleEventUserTags && (controllerWrapper.gravityEnabled == mxmAnimator.QueryUserTags(EUserTags.UserTag1)))//(rootMotionApplicator.EnableGravity == mxmAnimator.QueryUserTags(EUserTags.UserTag1)))
                {
                    // tag on means DisableGravity = true
                    if (rootMotionApplicator != null)
                    {
                        // need to toggle gravity
                        //if (rootMotionApplicator.EnableGravity || controllerWrapper.CollisionEnabled)
                        if (controllerWrapper.gravityEnabled || controllerWrapper.CollisionEnabled)
                        {
                            // we are disabling OR collision is enabled
                            //rootMotionApplicator.EnableGravity = !rootMotionApplicator.EnableGravity;
                            controllerWrapper.gravityEnabled = !controllerWrapper.gravityEnabled;
                            //Debug.LogFormat("{0} gravity", rootMotionApplicator.EnableGravity ? "Enabling" : "Disabling");
                            //Debug.LogFormat("{0} gravity", controllerWrapper.gravityEnabled ? "Enabling" : "Disabling");
                            /*if (rootMotionApplicator.EnableGravity)
                            {
                                Debug.Break();
                            }*/
                        }
                        /*else if (!rootMotionApplicator.EnableGravity)
                        {
                            // collision not enabled
                            Debug.Log("won't reenable gravity with collision disabled!");
                        }*/

                    }


                }
                if (handleEventUserTags && (controllerWrapper.CollisionEnabled == mxmAnimator.QueryUserTags(EUserTags.UserTag2)))
                {
                    //Debug.LogFormat("Toggling collisionEnabled to {0}", !controllerWrapper.CollisionEnabled);
                    // tag on means DisableCollision = true
                    if (controllerWrapper != null)
                    {
                        //Debug.Log("    - actually doing it");
                        controllerWrapper.CollisionEnabled = !controllerWrapper.CollisionEnabled;

                        //Debug.LogFormat("{0} collisions", controllerWrapper.CollisionEnabled ? "Enabling" : "Disabling");

                        if (controllerWrapper.CollisionEnabled)
                        {
                            controllerWrapper.BlockNonJumpFlyup(0.5f);
                            //Debug.Break();
                        }
                    }

                }
                yield return null;
            }

            //Debug.Log("Event complete");
            // event is complete
            /*
            if (useFavourTag)
            {
                mxmAnimator.RemoveFavourTag(favourTag);
            }*/

            if (handleEventUserTags && (controllerWrapper != null))
            {
                /*if (!controllerWrapper.CollisionEnabled)
                {
                    Debug.Log("Complete - Reenabling Collisions");
                }*/
                controllerWrapper.CollisionEnabled = true;
            }

            if ((striderWrapper != null) && !striderWrapper.Enabled)
            {
                striderWrapper.EnableSmooth(0.5f);
            }

            character.EnableFootPlacement(20);

            // reapply sprint tag as needed
            if (sprintWasEnabled)
            {
                mxmAnimator.AddFavourTag("Sprint");
            }


            //if (gravityWasEnabled && handleEventUserTags && !rootMotionApplicator.EnableGravity && (delayGravityTime > 0))
            if (gravityWasEnabled && handleEventUserTags && !controllerWrapper.gravityEnabled && (delayGravityTime > 0))
            {
                if (controllerWrapper.Velocity.y > 0)
                {
                    controllerWrapper.Move(Vector3.zero);
                }
                //Debug.Log("delaying gravity reenable");
                yield return new WaitForSeconds(delayGravityTime);
            }
            if (gravityWasEnabled && handleEventUserTags && (rootMotionApplicator != null))
            {
                //Debug.LogFormat("ctrlVelY( {0} )", controllerWrapper.Velocity.y);
                if (controllerWrapper.Velocity.y > 0)
                {
                    controllerWrapper.Move(Vector3.zero);
                }
                //if (!rootMotionApplicator.EnableGravity)
                /*if (!controllerWrapper.gravityEnabled)
                {
                    Debug.Log("Complete - Reenabling Gravity");
                }*/
                //rootMotionApplicator.EnableGravity = true;
                controllerWrapper.gravityEnabled = true;
            }

            if (forceGravityEnableOnComplete)
            {
                controllerWrapper.gravityEnabled = true;
            }

            executionCoroutine = null;

            character.currentState = NGCharacter.CharacterState.Locomotion;
            if (onComplete != null)
            {
                onComplete.Invoke();
            }
            character.FireNamedEvent(PARKOUR_COMPLETE_HASH);

            if (debugEventsInSlowMotion)
            {
                Time.timeScale = timeScaleWas;
            }

            yield return 0;
        }
    }
}