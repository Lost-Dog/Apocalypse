using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Threepeat.NGCharacter;

namespace Threepeat
{
    [System.Serializable]
    public class NGCharacter_MovementHelper : NGCharacterHelper
    {
        [HideInInspector] public MxMAnimator mxmAnimator;
        [HideInInspector] public MxMTrajectoryGenerator mxmTrajectoryGenerator;

        protected bool isSprinting = false;
        protected bool isCrouching = false;
        protected bool isStrafing = false;

        public NGCharacter_MovementHelper(NGCharacter ch) : base(ch)
        {
        }
        
        public override void Initialize()
        {
            if (character == null)
            {
                Debug.Log("Character null?!");
            }
            mxmAnimator = character.mxmAnimator;
            mxmTrajectoryGenerator = character.mxmTrajectoryGenerator;

            speedRamp = new NGLocomotionTagController(mxmAnimator, mxmTrajectoryGenerator, character);
            character.RegisterCustomUpdateMethod(CustomUpdate);
            couldRun = !canRun;
        }
   
        public bool Sprinting
        {
            get => isSprinting;

            set
            {
                if (value != isSprinting)
                {
                    if (value)
                    {
                        StartSprint();
                    }
                    else
                    {
                        StopSprint();
                    }
                }

            }
        }

        public bool Crouching
        {
            get => isCrouching;

            set
            {
                if (value != isCrouching)
                {
                    if (value)
                    {
                        StartCrouch();
                    }
                    else
                    {
                        StopCrouch();
                    }
                    isCrouching = value;
                }
            }
        }

        public bool Strafing
        {
            get => isStrafing;

            set
            {
                if (value != isStrafing)
                {
                    if (value)
                    {
                        StartStrafe();
                    }
                    else
                    {
                        StopStrafe();
                    }
                    isStrafing = value;
                }
            }
        }

        [Tooltip("")]
        public NGCharacter.CharacterMode currentMode = NGCharacter.CharacterMode.Locomotion;
        protected NGCharacter.CharacterMode lastMode = NGCharacter.CharacterMode.Locomotion;


        protected Transform strafeTarget = null;

        protected CustomStrafeIVPP strafeIVPP;

        protected Transform strafeBackupCameraTransform = null;

        public NGLocomotionTagController speedRamp;

        bool wasGrounded = true;
        float timeLastGrounded = 0f;

        private bool couldRun = true;

        public bool canRun = true;

        public bool forceHardLandingOneShot = false;

        public bool doFallDetection = true;

        [Header("Jumping")]
        [Tooltip("enable/disable character ability to jump")]
        public bool canJump = true;

        protected float timeStartedFalling = -1f;

        [Header("Character Events")]
        public UnityEvent<bool> onJump = new UnityEvent<bool>();
        [Tooltip("UnityEvent(int) called on landing event.  LandingType will be provided in the value (0 = Normal, 1 = Heavy Landing with Front Roll, 2 = Heavy Landing without Roll)")]
        public UnityEvent<int> onLand = new UnityEvent<int>();

        // Jump Internal State stuff
        protected bool wasGravityEnabledInRootMotionApplicator = true;
        protected EMxMRootMotion initialRMMode;
        protected Vector3 initialVelocity = Vector3.zero;

        private float MODE_BEHAVIOR_UPDATE_INTERVAL = 0.05f;
        private float MODE_BEHAVIOR_ADJUSTMENT_TIME_DEG_PER_SEC_SQUARED = 10f;
        private float lastTimeModeBehaviorUpdate = 0f;
        private float desiredAngularWarpRate = -1f;

        ~NGCharacter_MovementHelper()
        {
            character.UnregisterCustomUpdateMethod(CustomUpdate);
        }

        public void SetCanRun(bool doRun)
        {
            canRun = doRun;
        }

        private void UpdateSpeedBehavior()
        {
            if (isStrafing)
            {
                SetTrajectoryData(config.strafeSettings, character.inputProfileStrafe, canRun ? config.strafeSettings.speed : config.walkSettings.speed);
            }
            else if (canRun)
            {
                SetTrajectoryData(config.runSettings, character.inputProfileLocomotion);
            }
            else
            {
                SetTrajectoryData(config.walkSettings, character.inputProfileLocomotion);
            }
        }

        private (Vector3 moveDelta, float angleDelta) motion;
        private float cachedGoalSpeed;

        private void UpdateModeBehavior()
        {
            //float speed = controllerWrapper.Velocity.magnitude;

            /*
            float futureTimeOffset = 0.6f;
            motion = mxmTrajectoryGenerator.ExtractMotion(futureTimeOffset);
            //Debug.LogFormat("Trajectory Speed( {0} ), Ang( {1} deg)", motion.moveDelta.magnitude, motion.angleDelta);
            speed = motion.moveDelta.magnitude * 1.0f / futureTimeOffset;
            //angDiff = Mathf.DeltaAngle(lastAngleDelta, motion.angleDelta) * 1.0f / futureTimeOffset;
            */
            if ((Time.time - lastTimeModeBehaviorUpdate) >= MODE_BEHAVIOR_UPDATE_INTERVAL)
            {
                TrajectoryPoint[] tpoints = mxmTrajectoryGenerator.GetCurrentGoal();
                cachedGoalSpeed = tpoints[tpoints.Length - 1].Position.magnitude / mxmAnimator.CurrentAnimData.PosePredictionTimes[tpoints.Length - 1];


                if ((mxmAnimator.LongErrorWarpType == ELongitudinalErrorWarp.Stride) && (character.currentState == NGCharacter.CharacterState.Locomotion) && (config.minSpeedForStrider > 0))
                {
                    if (Mathf.Min(cachedGoalSpeed, controllerWrapper.GetCurrentGroundSpeed()) >= config.minSpeedForStrider)
                    {
                        character.EnableStriderIfInUse();
                    }
                    else
                    {
                        character.DisableStriderIfInUse();
                    }
                }

                lastTimeModeBehaviorUpdate = Time.time;
            }
            //Debug.LogFormat("{0}", last.Position);

            if (isCrouching && isStrafing)
            {
                desiredAngularWarpRate = config.crouchStrafeSettings.angularWarpRate;
            }
            else if (isCrouching)
            {
                /*if (isStrafing)
                {
                    if (cachedGoalSpeed <= config.crouchStrafeSettings.speed)
                    {
                        desiredAngularWarpRate =
                            Mathf.Lerp(
                                    config.crouchSettings.angularWarpRate,
                                    config.crouchStrafeSettings.angularWarpRate,
                                    cachedGoalSpeed / config.crouchStrafeSettings.speed);
                    }
                    else
                    {
                        desiredAngularWarpRate = config.strafeSettings.angularWarpRate;
                    }
                }
                else*/
                {
                    desiredAngularWarpRate = config.crouchSettings.angularWarpRate;
                }
            }
            else if (cachedGoalSpeed <= config.walkSettings.speed)
            {
                desiredAngularWarpRate = config.walkSettings.angularWarpRate;
            }
            else if (isStrafing)
            {
                if (cachedGoalSpeed <= config.strafeSettings.speed)
                {
                    desiredAngularWarpRate =
                        Mathf.Lerp(
                                config.walkSettings.angularWarpRate,
                                config.strafeSettings.angularWarpRate,
                                (cachedGoalSpeed - config.walkSettings.speed) / (config.runSettings.speed - config.walkSettings.speed));
                }
                else
                {
                    desiredAngularWarpRate = config.strafeSettings.angularWarpRate;
                }
            }
            else if (cachedGoalSpeed <= config.runSettings.speed)
            {
                desiredAngularWarpRate =
                    Mathf.Lerp(
                            config.walkSettings.angularWarpRate,
                            config.runSettings.angularWarpRate,
                            (cachedGoalSpeed - config.walkSettings.speed) / (config.runSettings.speed - config.walkSettings.speed));
            }
            else
            {
                desiredAngularWarpRate = config.sprintSettings.angularWarpRate;
            }

            if ((mxmAnimator.AngularErrorWarpRate < desiredAngularWarpRate) || (MODE_BEHAVIOR_ADJUSTMENT_TIME_DEG_PER_SEC_SQUARED < 0))
            {
                mxmAnimator.AngularErrorWarpRate = desiredAngularWarpRate;
            }
            else if (mxmAnimator.AngularErrorWarpRate == desiredAngularWarpRate)
            {
                // do nothing
            }
            else
            {
                mxmAnimator.AngularErrorWarpRate = Mathf.Lerp(mxmAnimator.AngularErrorWarpRate, desiredAngularWarpRate, MODE_BEHAVIOR_ADJUSTMENT_TIME_DEG_PER_SEC_SQUARED * Time.deltaTime);
            }
        }


        protected void CustomUpdate()
        {
            if (canRun != couldRun)
            {
                UpdateSpeedBehavior();
                couldRun = canRun;
            }

            //if ((Time.time - lastTimeModeBehaviorUpdate) >= MODE_BEHAVIOR_UPDATE_INTERVAL)
            {
                UpdateModeBehavior();
                //  lastTimeModeBehaviorUpdate = Time.time;
            }


            bool grounded = (controllerWrapper != null) ? controllerWrapper.IsGrounded : true;

            if (!wasGrounded && grounded && (character.currentState != NGCharacter.CharacterState.JumpingFalling))
            {
                if (character.debugMode)
                {
                    Debug.LogFormat("DETECTED NO-FALL LANDING {0} - state {1}", controllerWrapper.Velocity.y, character.currentState);
                }
                if (forceHardLandingOneShot)
                {
                    if (character.debugMode) { Debug.Log("calling land function"); }
                    Vector3 groundSpeed = new Vector3(controllerWrapper.Velocity.x, 0f, controllerWrapper.Velocity.z);
                    character.StartCoroutine(DoLand(config.configJump.landing_HeavySpeedThreshold - 1, groundSpeed));
                }
            }

            wasGrounded = grounded;

            character.UpdateInput();

            if (character.currentState == NGCharacter.CharacterState.Locomotion)
            {
                if (grounded)
                {
                    timeStartedFalling = -1f;
                }
                else
                {
                    if (timeStartedFalling > 0)
                    {
                        if (doFallDetection && ((Time.time - timeStartedFalling) > config.fallDetectionTimeDelay))
                        {
                            //Debug.Log("FALLING DETECTED!");
                            DoFall();
                        }
                    }
                    else
                    {
                        timeStartedFalling = Time.time;
                    }
                }

            }

            if (currentMode == NGCharacter.CharacterMode.Sprint)
            {
                if (config.doSprintRamp && (mxmTrajectoryGenerator.MaxSpeed < config.sprintSettings.speed))
                {
                    //Debug.Log("ramping");
                    mxmTrajectoryGenerator.MaxSpeed = Mathf.Min(
                        config.sprintSettings.speed, mxmTrajectoryGenerator.MaxSpeed + config.sprintRampSpeedRate * Time.deltaTime);
                }
            }

            speedRamp.UpdateSpeedRamp();

            lastMode = currentMode;

        }

        public void SetTrajectoryData(
                NGCharacterBaseConfig.LocomotionModeSettings modeSet,
                MxMInputProfile inputProfile,
        float overrideMaxSpeed = -1f)
        {
            if (!modeSet.doLongitudinalErrorWarpWhenNoStrider)
            {
                mxmAnimator.DesiredPlaybackSpeed = modeSet.playbackSpeedMultiplierIfNoLongitudeWarping;
            }
            mxmTrajectoryGenerator.MaxSpeed = (overrideMaxSpeed < 0) ? modeSet.speed : overrideMaxSpeed;
            mxmTrajectoryGenerator.PositionBias = modeSet.moveBias;
            mxmTrajectoryGenerator.DirectionBias = modeSet.dirBias;
            mxmTrajectoryGenerator.InputProfile = inputProfile;
            mxmAnimator.AngularErrorWarpRate = modeSet.angularWarpRate;
            mxmAnimator.AngularErrorWarpThreshold = modeSet.angularWarpRateMagnitudeThreshold;
            
            if (mxmAnimator.LongErrorWarpType != ELongitudinalErrorWarp.Stride)
            {
                if (modeSet.doLongitudinalErrorWarpWhenNoStrider)
                {
                    mxmAnimator.SetLongitudinalErrorWarping(ELongitudinalErrorWarp.Speed, modeSet.longitudinalErrorWarpRange);
                }
                else
                {
                    mxmAnimator.DesiredPlaybackSpeed = modeSet.playbackSpeedMultiplierIfNoLongitudeWarping; //1.0f;
                    mxmAnimator.SetLongitudinalErrorWarping(ELongitudinalErrorWarp.None, Vector2.zero);
                }
            }
        }

        protected void StartSprint()
        {
            if (isSprinting || isCrouching)
            {
                // already sprinting or in a state that can't sprint.
                return;
            }

            if (currentMode == NGCharacter.CharacterMode.Locomotion)
            {
                //Debug.LogFormat("Sprint-START {0}", doSprintRamp ? "WITH-SPRINT-RAMP" : "");
                //lastMode = currentMode;
                // we can sprint
                currentMode = NGCharacter.CharacterMode.Sprint;
                //mxmSpeedRamp.BeginSprint();
                speedRamp.BeginSprint();
                mxmAnimator.SetCalibrationData("Sprint");
                mxmAnimator.AddFavourTag("Sprint");
                SetTrajectoryData(
                    config.sprintSettings,
                    character.inputProfileSprint,
                    config.doSprintRamp ?
                            Mathf.Min(config.sprintSettings.speed, mxmTrajectoryGenerator.MaxSpeed + config.sprintRampSpeedRate * Time.deltaTime) :
                            config.sprintSettings.speed);

                isSprinting = true;
            }

        }

        protected void StopSprint()
        {
            if (!isSprinting)
            {
                // already not sprinting
                return;
            }

            if (currentMode == NGCharacter.CharacterMode.Sprint)
            {
                //Debug.Log("Sprint-STOP");
                //lastMode = currentMode;
                currentMode = NGCharacter.CharacterMode.Locomotion;
                //mxmSpeedRamp.ResetFromSprint();
                speedRamp.StopSprint(canRun);
                mxmAnimator.SetCalibrationData("General");
                if (canRun)
                {
                    SetTrajectoryData(config.runSettings, character.inputProfileLocomotion);
                }
                else
                {
                    SetTrajectoryData(config.walkSettings, character.inputProfileLocomotion);
                }
                //Debug.Log("Removing sprint tag");
                mxmAnimator.RemoveFavourTag("Sprint");
            }
            isSprinting = false;
        }

        protected void StartCrouch()
        {
            // start crouching
            mxmAnimator.AddRequiredTag("Crouch");
            isCrouching = true;
            if (isStrafing)
            {
                mxmAnimator.SetCalibrationData("Strafe");
                SetTrajectoryData(config.crouchStrafeSettings, character.inputProfileStrafe);
            }
            else
            {
                mxmAnimator.SetCalibrationData("General");
                SetTrajectoryData(config.crouchSettings, character.inputProfileLocomotion);
            }
        }

        protected void StopCrouch()
        {
            // stop crouching
            mxmAnimator.RemoveRequiredTag("Crouch");
            if (!isStrafing)
            {
                mxmAnimator.SetCalibrationData("General");
            }
            isCrouching = false;

            if (isStrafing)
            {
                StartStrafe();
            }
            else if (isSprinting)
            {
                StartSprint();
            }
            else if (canRun)
            {
                SetTrajectoryData(config.runSettings, character.inputProfileLocomotion);
            }
            else
            {
                SetTrajectoryData(config.walkSettings, character.inputProfileLocomotion);
            }
        }

        private void StartStrafe()
        {
            //Debug.Log("Strafe-START");
            mxmAnimator.AddRequiredTag("Strafe");
            mxmAnimator.SetCalibrationData("Strafe");
            mxmAnimator.AngularErrorWarpMethod = EAngularErrorWarpMethod.TrajectoryFacing;

            if (isCrouching)
            {
                SetTrajectoryData(config.crouchStrafeSettings, character.inputProfileStrafe);
                mxmAnimator.SetAngularErrorWarping(mxmAnimator.AngularErrorWarpType, mxmAnimator.AngularErrorWarpMethod, mxmAnimator.AngularErrorWarpRate, mxmAnimator.AngularErrorWarpThreshold, 180f);
            }
            else
            {
                SetTrajectoryData(config.strafeSettings, character.inputProfileStrafe, canRun ? config.strafeSettings.speed : config.walkSettings.speed);
                mxmAnimator.SetAngularErrorWarping(mxmAnimator.AngularErrorWarpType, mxmAnimator.AngularErrorWarpMethod, mxmAnimator.AngularErrorWarpRate, mxmAnimator.AngularErrorWarpThreshold, 180f);
            }
            mxmAnimator.PastTrajectoryMode = EPastTrajectoryMode.CopyFromCurrentPose;
            mxmTrajectoryGenerator.Strafing = true;
            if (!isStrafing)
            {
                character.wasFaceDirectionOnIdleEnabled = mxmTrajectoryGenerator.FaceDirectionOnIdle;
            }
            mxmTrajectoryGenerator.FaceDirectionOnIdle = true;
        }

        private void StopStrafe()
        {
            //Debug.Log("Strafe-STOP");
            //stop strafe
            mxmAnimator.RemoveRequiredTag("Strafe");
            mxmAnimator.SetCalibrationData("General");
            mxmAnimator.AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
            if (isCrouching)
            {
                SetTrajectoryData(config.crouchSettings, character.inputProfileLocomotion);
            }
            else if (isSprinting)
            {
                SetTrajectoryData(config.sprintSettings, character.inputProfileSprint);
            }
            else if (canRun)
            {
                SetTrajectoryData(config.runSettings, character.inputProfileLocomotion);
            }
            else
            {
                SetTrajectoryData(config.walkSettings, character.inputProfileLocomotion);
            }
            mxmAnimator.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
            mxmTrajectoryGenerator.Strafing = false;
            mxmTrajectoryGenerator.FaceDirectionOnIdle = character.wasFaceDirectionOnIdleEnabled || config.faceAwayFromCameraOnIdleInNonStrafeModes;
            mxmAnimator.SetAngularErrorWarping(mxmAnimator.AngularErrorWarpType, mxmAnimator.AngularErrorWarpMethod, mxmAnimator.AngularErrorWarpRate, mxmAnimator.AngularErrorWarpThreshold, 60f);
        }





        public void SetStrafeTarget(Transform target, bool enterStrafe = true)
        {
            bool alreadyStrafing = Strafing;

            if (enterStrafe)
            {
                Strafing = true;
            }

            if ((strafeTarget != null) && (target == null))
            {
                ClearStrafeTarget();
                return;
            }

            strafeTarget = target;

            if (strafeIVPP == null)
            {
                Debug.Log("setting strafeIVPP");
                strafeIVPP = CustomStrafeIVPP.CreateInstance<CustomStrafeIVPP>();
                strafeIVPP.camTransform = character.mxmTrajectoryGenerator.RelativeCameraTransform;
                List<NGInputVectorPostProcessorBase> pps = new List<NGInputVectorPostProcessorBase>(character.InputScheme.postProcessors);
                pps.Add(strafeIVPP);
                character.InputScheme.postProcessors = pps.ToArray();
            }

            strafeIVPP.postProcessorActive = true;
            strafeBackupCameraTransform = character.mxmTrajectoryGenerator.RelativeCameraTransform;
            character.mxmTrajectoryGenerator.RelativeCameraTransform = null;

            character.RegisterCustomUpdateMethod(StrafeTarget_CustomUpdate);
        }

        protected void StrafeTarget_CustomUpdate()
        {
            if (Strafing && (strafeTarget != null))
            {
                character.mxmTrajectoryGenerator.StrafeDirection = GetStrafeDirection(character.transform.position, strafeTarget.position);
            }
        }

        public void ClearStrafeTarget(bool exitStrafe = false)
        {
            if (strafeTarget != null)
            {
                character.UnregisterCustomUpdateMethod(StrafeTarget_CustomUpdate);
            }
            strafeTarget = null;

            character.mxmTrajectoryGenerator.StrafeDirection = Vector3.forward;
            if (strafeBackupCameraTransform != null)
            {
                character.mxmTrajectoryGenerator.RelativeCameraTransform = strafeBackupCameraTransform;
            }

            if (strafeIVPP != null)
            {
                strafeIVPP.postProcessorActive = false;
            }

            if (exitStrafe)
            {
                Strafing = false;
            }
        }

        public class CustomStrafeIVPP : NGInputVectorPostProcessorBase
        {
            public Transform camTransform;
            public override void PostProcessInputVector(NGInputSchemeBase inputScheme, ref Vector3 inputVectorLocalSpace)
            {
                // Get Cam-relative vector
                Vector3 forward = Vector3.ProjectOnPlane(camTransform.forward, Vector3.up);

                //Rotate our input vector relative to the camera
                inputVectorLocalSpace = Quaternion.FromToRotation(Vector3.forward, forward) * inputVectorLocalSpace;
            }
        }
        private Vector3 GetStrafeDirection(Vector3 character, Vector3 target)
        {
            Vector3 vec = target - character;
            return new Vector3(vec.x, 0, vec.z);
        }

        /***************************************************** NOT YET CLEANED *******************************/
        public void UpdateMovementLogic(float a_deltaTime)
        {
            if (character.decoupler == null)
            {
                return;
            }

            if (character.currentState != NGCharacter.CharacterState.Locomotion)
            {
                return;
            }

            //For this example controller we just extract the motion at the start of the trajectory. 
            //Here I take the first 0.3s of the trajectory
            var motion = mxmTrajectoryGenerator.ExtractMotion(0.3f);

            Quaternion rotDelta = Quaternion.Inverse(character.transform.rotation) * Quaternion.AngleAxis(motion.angleDelta, Vector3.up);

            motion.angleDelta = rotDelta.eulerAngles.y;

            //To get the average motion of that 0.3s trajectory per Time.deltaTime we multiply by (Time.deltaTime / 0.3f)
            motion.moveDelta *= (a_deltaTime / 0.3f);
            motion.angleDelta *= (a_deltaTime / 0.3f);

            //The movement extracted from the trajectory is then blended in based on Root motion blending settings on the MxMAnimationDecoupler
            //You only need to do this if you want root motion blending
            motion = character.decoupler.CalculateRootMotionBlending(motion.moveDelta, motion.angleDelta, mxmTrajectoryGenerator.HasMovementInput());

            //Now we apply gravity on top of the root motion blended movement delta. This can be done manually or use built in functionality
            //if (!charController.IsGrounded)
            if (!controllerWrapper.IsGrounded)
            {
                motion.moveDelta.y = character.decoupler.CalculateGravityMoveDelta(a_deltaTime);
            }

            //Now that we have the final move delta we can apply it to our generic controller wrapper
            controllerWrapper.Move(motion.moveDelta);

            //For this particular movement control I've decided that the rotation of the capsule will always be the same as the model rotation
            //rotation is not particularly important for the controller itself so its relatively trivial. Best to keep it in line with what
            //the player is seeing.
            //m_charController.Rotate(Quaternion.AngleAxis(motion.angleDelta, Vector3.up));

            //transform.rotation = m_trajectoryGenerator.transform.rotation;
        }


        public void DoJump(bool jumpIsBigJump = false)
        {
            character.SetCharacterState(CharacterState.JumpingFalling);



            // kick off jump animation
            if (config.jumpImpulseOnKeypress && jumpIsBigJump)
            {
                controllerWrapper.Jump(config.configJump.jumpBigHeight);
            }
            else if (config.jumpImpulseOnKeypress)
            {
                controllerWrapper.Jump(config.configJump.jumpHeight);
            }
            // add vertical speed to character

            onJump.Invoke(jumpIsBigJump);

            config.configJump.jump_EventDef.ClearContacts();

            if (jumpIsBigJump)
            {
                //Debug.LogFormat("Adding Favour tag {0}", favourTag);
                mxmAnimator.AddFavourTag(config.configJump.jumpBig_favourTag);
            }
            wasGravityEnabledInRootMotionApplicator = controllerWrapper.gravityEnabled; //rootMotionApplicator.EnableGravity;

            // get initial conditions:
            initialVelocity = controllerWrapper.Velocity;
            initialRMMode = mxmAnimator.RootMotion;

            //TODO: this thresh needs to be a jumpsettings param
            //TODO: standingJumpMaxVel needs to be there too

            if (initialVelocity.sqrMagnitude < config.configJump.standingJumpMaxCharacterSpeedSquared)
            {
                bool oldval = character.InputScheme.doEnvironmentAwareTrajectories;
                character.InputScheme.doEnvironmentAwareTrajectories = false;
                initialVelocity = character.InputScheme.GetRawWorldSpaceInputVectorNoCache() * config.configJump.standingJumpMaxHorizontalJumpSpeed;
                //Debug.LogFormat("Firing with jump vel: {0}", initialVelocity);
                character.InputScheme.doEnvironmentAwareTrajectories = oldval;
            }

            mxmAnimator.RootMotion = EMxMRootMotion.Off;
            mxmAnimator.BeginEvent(config.configJump.jump_EventDef);

            character.StartCoroutine(this.ExecuteJumpAndOrFallEvent(true, jumpIsBigJump));
            jumpIsBigJump = false;
        }

        protected void DoFall()
        {
            bool canPlayerFall = CheckMinFallHeight();
            if (!canPlayerFall)
            {
                return;
            }
            character.SetCharacterState(CharacterState.JumpingFalling);
            wasGravityEnabledInRootMotionApplicator = controllerWrapper.gravityEnabled; //rootMotionApplicator.EnableGravity;
            // get initial conditions:
            initialVelocity = controllerWrapper.Velocity;
            initialRMMode = mxmAnimator.RootMotion;

            mxmAnimator.RootMotion = EMxMRootMotion.Off;
            //mxmAnimator.BeginEvent(configJump.jump_EventDef);

            character.StartCoroutine(this.ExecuteJumpAndOrFallEvent(false, false));
        }

        private bool CheckMinFallHeight()
        {
            float heightAboveGround = controllerWrapper.GetHeightAboveGround(config.minHeightToStartFalling + 0.1f);
            //Debug.LogFormat("HeightAboveGround {0}", heightAboveGround);
            return (heightAboveGround < 0) || (heightAboveGround > config.minHeightToStartFalling);
        }

        protected IEnumerator DoLand(float landingImpactSpeed, Vector3 groundSpeedVec)
        {
            Vector3 forwardVector = Vector3.Project(groundSpeedVec, character.transform.forward);

            float forwardSpeed = forwardVector.magnitude; //groundSpeedVec.magnitude;
            LandingType landingType = LandingType.Normal;

            // Get landing impact speed and determine whether path ahead of character is clear to determine landing 
            // type

            // Get landing impact speed:
            //float landingImpactSpeed = -20f;
            //float currGroundSpeed = controllerWrapper.GetCurrentGroundSpeed(); ;

            if (character.debugMode)
            {
                Debug.LogFormat("DoLAND - Impact Speed( {0} ), forwardSpeed( {1} )", landingImpactSpeed, forwardSpeed);
            }

            MxMEventDefinition landingEvent = config.configJump.landing_EventDef;
            bool doRootMotionForLanding = true; // forceHardLandingOneShot; //&& !Sprinting; // forwardSpeed < 3f;
            /*if (doRootMotionForLanding)
            {
                groundSpeedVec = Vector3.zero;
            }*/
            bool earlyOut = !doFallDetection;
            // pick event based on landing speed and whether path ahead of character is clear:
            if (forceHardLandingOneShot || (landingImpactSpeed < config.configJump.landing_HeavySpeedThreshold))        // impact speed will be negative since it's down in Y direction
            {
                forceHardLandingOneShot = false;
                // heavy landing

                // Determine of Clear path ahead of character:
                if ((forwardSpeed > 3f) && IsPathAheadOfCharacterClear(config.configJump.landing_distanceToCheckForClearPathAhead))
                {
                    landingType = LandingType.HeavyForwardPathClear;
                    landingEvent = config.configJump.landingHeavyForwardClear_EventDef;
                    //Debug.Log("Heavy-fwd!");
                }
                else
                {
                    landingType = LandingType.HeavyForwardPathBlocked;
                    landingEvent = config.configJump.landingHeavy_EventDef;
                    doRootMotionForLanding = true;
                    //Debug.Log("Heavy!");
                }
            }
            else
            {
                if (forwardSpeed < config.configJump.landing_minForwardSpeedForLandingEvent)
                {
                    // Don't fire a landing event if it's a standing/walking small jump, as none is needed.
                    character.SetCharacterState(CharacterState.Locomotion);
                    earlyOut = true;
                }
            }
            onLand.Invoke((int)landingType);
            if (!earlyOut)
            {
                if (character.debugMode)
                {
                    Debug.LogFormat("LANDING TYPE = {0}", landingType.ToString());
                }

                //TODO: don't set locomotion, just kick off the landing event and let it do it.
                character.SetCharacterState(CharacterState.Landing);

                initialRMMode = mxmAnimator.RootMotion;
                mxmAnimator.RootMotion = doRootMotionForLanding ? EMxMRootMotion.On : EMxMRootMotion.Off;
                yield return ExecuteLandingEvent(landingEvent, groundSpeedVec, doRootMotionForLanding);
            }
        }

        protected IEnumerator ExecuteLandingEvent(MxMEventDefinition eventDefLanding, Vector3 groundSpeed, bool doRootMotionForLanding = true)
        {

            character.DisableStriderIfInUse();

            if (doRootMotionForLanding)
            {
                mxmAnimator.RootMotion = EMxMRootMotion.RootMotionApplicator;
            }
            //Debug.LogFormat("MxMLand: Doing a landing anim {0}", doRootMotionForLanding ? "with RM" : "");
            float prevFavMult = mxmAnimator.FavourMultiplier;
            mxmAnimator.SetFavourMultiplier(0.2f);
            // LANDING EVENT
            //TODO: add favour tags based on impact velocity
            eventDefLanding.ClearContacts();
            eventDefLanding.AddEventContact(Vector3.zero, controllerWrapper.transform.rotation.eulerAngles.y);
            //eventDefLanding.AddEventContact(landingContact, referenceTransform.rotation.eulerAngles.y);
            mxmAnimator.BeginEvent(eventDefLanding);
            //Vector3 currVelocity = new Vector3(controllerWrapper.Velocity.x, 0f, controllerWrapper.Velocity.z);

            bool haveZeroized = false;
            bool newDoRootMotionForLanding = false;

            while (mxmAnimator.IsEventPlaying && !mxmAnimator.IsEventComplete)
            {
                // Need to check user tags.
                newDoRootMotionForLanding = !character.tagManager.Query(NGTagManager.NGUserTag.IgnoreRootMotion);
                if (!haveZeroized && character.tagManager.Query(NGTagManager.NGUserTag.ZeroizeGroundSpeed))
                {
                    haveZeroized = true;
                    groundSpeed = Vector3.zero;
                }

                if (!doRootMotionForLanding && newDoRootMotionForLanding)
                {
                    Debug.Log("DoLand: enabling root motion");
                    // enable rootmotion
                    mxmAnimator.RootMotion = EMxMRootMotion.RootMotionApplicator;
                    doRootMotionForLanding = true;
                }
                else if (doRootMotionForLanding && !newDoRootMotionForLanding)
                {
                    Debug.Log("DoLand: disabling root motion");
                    mxmAnimator.RootMotion = EMxMRootMotion.Off;
                    doRootMotionForLanding = false;
                }

                if (!doRootMotionForLanding)
                {
                    controllerWrapper.Move(groundSpeed * Time.deltaTime);
                }
                //TODO: potentially alter the motion of the event left/right based on input
                // wait for event to complete
                yield return null;
            }
            mxmAnimator.SetFavourMultiplier(prevFavMult);

            mxmAnimator.RootMotion = EMxMRootMotion.RootMotionApplicator; //initialRMMode;

            character.EnableStriderIfInUse();

            character.SetCharacterState(CharacterState.Locomotion);
            //Debug.Log("LANDING EVENT DONE");
        }



        public bool IsPathAheadOfCharacterClear(float distanceAheadToCheck = 4f)
        {

            // cast a ray to check
            float charHeight = controllerWrapper.Height;
            RaycastHit hitForward;

            bool retval = !Physics.Raycast(controllerWrapper.transform.position + Vector3.up * charHeight * 0.1f, controllerWrapper.transform.TransformDirection(Vector3.forward), out hitForward, distanceAheadToCheck);

            // pass on clear path
            return retval;
        }

        [HideInInspector] public float maxFallSpeed = 0; // convenience variable that will always hold the maximum vertical speed achieved prior to last landing event.
        [HideInInspector] public bool lastLandingWasFromJump = false;


        protected IEnumerator ExecuteJumpAndOrFallEvent(bool doJump = true, bool isBigJump = false)
        {
            bool doFall = true;
            bool forceLandingTriggerOnReferenceTransformCharacter = false;
            Transform referenceTransform = controllerWrapper.transform;


            bool haveFiredJumpImpulse = config.jumpImpulseOnKeypress;

            Vector3 currVelocity = initialVelocity;
            //currVelocity.x += initialVelocityImpulse.x;
            character.SetCharacterState(CharacterState.JumpingFalling);
            /*if (doJump)
            {
                float upForceVal = isBigJump ? configJump.jumpForceBig : configJump.jumpForce;
                currVelocity.y = upForceVal; //* -2f * Physics.gravity.y;  //initialVelocityImpulse.y;
            }
            else*/
            {
                //currVelocity.y += initialVelocityImpulse.y;
                //Debug.LogFormat("RESETTING Y-VEL {0} TO ZERO", initialVelocity.y);
                currVelocity.y = 0;
            }
            //currVelocity.z += initialVelocityImpulse.z;

            character.DisableStriderIfInUse();

            lastLandingWasFromJump = doJump;

            EEventState currEventState;

            if (doJump)
            {
                if (character.debugMode)
                {
                    Debug.Log("Starting JUMP!");
                }
                //bool jumpStarting = true;
                while (mxmAnimator.IsEventPlaying && !mxmAnimator.IsEventComplete)
                {
                    if (!haveFiredJumpImpulse && (mxmAnimator.CurrentEventState != EEventState.Windup))
                    {
                        if (isBigJump)
                        {
                            controllerWrapper.Jump(config.configJump.jumpBigHeight);
                        }
                        else
                        {
                            controllerWrapper.Jump(config.configJump.jumpHeight);
                        }
                        haveFiredJumpImpulse = true;
                    }

                    controllerWrapper.Move(currVelocity * Time.deltaTime);

                    /*if (!controllerWrapper.IsGrounded || jumpStarting)
                    {
                        currVelocity.y += Physics.gravity.y * Time.deltaTime;
                    }
                    else
                    {
                        //Debug.Log("SETTING VELOCITY TO ZERO");
                        currVelocity.y = 0f;
                    }*/

                    /*if (!controllerWrapper.IsGrounded)
                    {
                        jumpStarting = false;
                    }*/

                    //if (wasGravityEnabledInRootMotionApplicator && (rootMotionApplicator.EnableGravity == mxmAnimator.QueryUserTags(EUserTags.UserTag1)))
                    if (wasGravityEnabledInRootMotionApplicator && (controllerWrapper.gravityEnabled == mxmAnimator.QueryUserTags(EUserTags.UserTag1)))
                    {
                        // tag on means DisableGravity = true
                        //if (rootMotionApplicator != null)
                        {
                            // need to toggle gravity
                            //if (rootMotionApplicator.EnableGravity || controllerWrapper.CollisionEnabled)
                            if (controllerWrapper.gravityEnabled || controllerWrapper.CollisionEnabled)
                            {
                                // we are disabling OR collision is enabled
                                //rootMotionApplicator.EnableGravity = !rootMotionApplicator.EnableGravity;
                                controllerWrapper.gravityEnabled = !controllerWrapper.gravityEnabled;
                                //Debug.LogFormat("{0} gravity", rootMotionApplicator.EnableGravity ? "Enabling" : "Disabling");
                                Debug.LogFormat("{0} gravity", controllerWrapper.gravityEnabled ? "Enabling" : "Disabling");
                            }
                            /*else if (!rootMotionApplicator.EnableGravity)
                            {
                                // collision not enabled
                                Debug.Log("won't reenable gravity with collision disabled!");
                            }*/

                        }


                    }
                    if (controllerWrapper.CollisionEnabled == mxmAnimator.QueryUserTags(EUserTags.UserTag2))
                    {
                        // tag on means DisableCollision = true
                        if (controllerWrapper != null)
                        {
                            controllerWrapper.CollisionEnabled = !controllerWrapper.CollisionEnabled;

                            //Debug.LogFormat("{0} collisions", controllerWrapper.CollisionEnabled ? "Enabling" : "Disabling");
                        }

                    }
                    yield return null;
                }
            }
            maxFallSpeed = controllerWrapper.Velocity.y;

            Vector3 groundSpeedVec = new Vector3(currVelocity.x, 0, currVelocity.z);
            float speed = groundSpeedVec.magnitude;
            if (doFall)
            {
                if (character.debugMode)
                {
                    Debug.Log("Starting FALL!");
                }

                //Debug.Log("FALLING");
                if (!controllerWrapper.IsGrounded)
                {
                    //TODO: don't assume horizontal speed means forward, figure out the heading
                    float speedAdjusted = speed / 8f;   //TODO: just use max speed instead of 8f
                    mxmAnimator.BeginLoopBlend("Falling", Mathf.Min(speedAdjusted, 1f), 0f);
                }
                float lastTime = Time.time;
                float timeSinceLast = 0f;
                /* WHAT IS THIS FOR?!
                if (speed < 0.05f)
                {
                    currVelocity += controllerWrapper.transform.forward * 0.05f;
                }*/

                while (!controllerWrapper.IsGrounded)
                {
                    timeSinceLast = (Time.time - lastTime);
                    //Debug.LogFormat("CurrVelocity( {0} )", currVelocity.ToString());
                    controllerWrapper.Move(currVelocity * timeSinceLast); //Time.deltaTime);
                    //currVelocity.y += Physics.gravity.y * timeSinceLast;
                    float fallingSpeedAdjusted = controllerWrapper.Velocity.y / 4f; //currVelocity.y / 4f;
                    mxmAnimator.SetBlendSpacePositionY(Mathf.Max(fallingSpeedAdjusted, -1f));
                    //Debug.LogFormat("Falling: {0}, timeSinceLast( {1} )", currVelocity.y, Time.time - lastTime);
                    lastTime = Time.time;

                    maxFallSpeed = Mathf.Min(maxFallSpeed, controllerWrapper.Velocity.y);
                    yield return null;
                }
                //Debug.LogFormat("Landing speed: {0}", currVelocity.y);
                if (forceLandingTriggerOnReferenceTransformCharacter && (referenceTransform != null))
                {
                    /*TODO: sort this event out
                    Character character = referenceTransform.GetComponent<Character>();
                    float avcSpeed = character.GetComponent<ActualVelocityComputer>().currentVerticalSpeedFromPositionDiff;

                    if (character != null)
                    {
                        //Debug.LogFormat("INVOKING Landing trigger on char( {0} )", character.name);
                        character.onLand.Invoke(avcSpeed + 4f); //currVelocity.y); //+ 5.5f);
                    }
                    */
                }
                mxmAnimator.EndLoopBlend();

            }

            if (character.debugMode)
            {
                Debug.Log("Done with Fall!");
            }


            mxmAnimator.RootMotion = initialRMMode;

            character.EnableStriderIfInUse();

            // event is complete
            if (isBigJump)
            {
                mxmAnimator.RemoveFavourTag(config.configJump.jumpBig_favourTag);
            }

            if (controllerWrapper != null)
            {
                if (!controllerWrapper.CollisionEnabled)
                {
                    //Debug.Log("Complete - Reenabling Collisions");
                }
                controllerWrapper.CollisionEnabled = true;
            }

            //if (wasGravityEnabledInRootMotionApplicator && !rootMotionApplicator.EnableGravity && (configJump.delayGravityTime > 0))
            if (wasGravityEnabledInRootMotionApplicator && !controllerWrapper.gravityEnabled && (config.configJump.delayGravityTime > 0))
            {
                //Debug.Log("delaying gravity reenable");
                yield return new WaitForSeconds(config.configJump.delayGravityTime);
            }
            if (wasGravityEnabledInRootMotionApplicator && (character.rootMotionApplicator != null))
            {
                //if (!rootMotionApplicator.EnableGravity)
                if (!controllerWrapper.gravityEnabled)
                {
                    //Debug.Log("Complete - Reenabling Gravity");
                }
                //rootMotionApplicator.EnableGravity = true;
                controllerWrapper.gravityEnabled = true;
            }

            if (controllerWrapper.IsGrounded)
            {
                character.SetCharacterState(CharacterState.Landing);
                //Debug.LogFormat("landed - {0} maxfall {1}", controllerWrapper.Velocity.y, maxFallSpeed);
                //yield return DoLand(controllerWrapper.Velocity.y, groundSpeedVec);
                yield return DoLand(maxFallSpeed, groundSpeedVec);
            }
            yield return 0;
        }


    }
}