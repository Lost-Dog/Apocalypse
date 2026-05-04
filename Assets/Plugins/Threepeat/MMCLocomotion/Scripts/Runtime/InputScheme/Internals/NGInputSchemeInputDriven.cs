using MxM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public abstract class NGInputSchemeInputDriven : NGInputSchemeBase
    {
        protected Transform camTransform = null;

        // Sprinting internal input state
        protected bool sprintJoystickInputIsPressed = false;

        // Jumping internal input state
        protected float jumpTimeKeyDown = 0f;
        protected bool jumpKeyIsDown = false;
        protected bool jumpIgnoreKeyUp = false;
        protected bool jumpIsBigJump = false;
        protected bool jumpParkourWantedDuringEndOfEvent = false;
        protected float jumpParkourWantedDuringEndOfEventTime = -1f;


        public const string ACTION_JUMPIMMEDIATE = "JumpImmediate-Sprint";
        public const string ACTION_JUMPDELAYED = "JumpDelayed";
        public const string ACTION_CROUCHTOGGLE = "CrouchToggle";
        public const string ACTION_STRAFETOGGLE = "StrafeToggle";
        public const string ACTION_SPRINTHOLD = "SprintHold";

        public ContextualActionProcessor keyProcessorJumpParkour;
        public ContextualActionProcessor keyProcessorSprintHold;
        public ContextualActionProcessor keyProcessorCrouchToggle;
        public ContextualActionProcessor keyProcessorStrafeToggle;

        [Header("Jumping")]
        [Tooltip("Jump will occur on key up or expiration of the time duration (in seconds) defined here.  Set to zero to disable big jump and jump immediately on button press.  Lower values increase responsiveness, higher values allow single-key regular+big jump mechanic.")]
        public float jumpMaxKeyHoldTimeBeforeJump = 0.35f;

        [Tooltip("Whether, when sprinting, a big jump should be immediately performed on key down to increase responsiveness.")]
        public bool jumpInstantBigJumpWhenSprinting = true;

        public enum InputButtonStatus
        {
            NoChangeFromLastFrame,
            JustTriggered,
            JustReleased
        }

        public bool Jump_CanPerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            bool grounded = character.controllerWrapper.IsGrounded;
            if ((character.currentState == NGCharacter.CharacterState.Locomotion) && !character.mxmAnimator.IsEventPlaying)
            {
                if (grounded && character.movement.canJump && !character.Crouching) 
                {
                    switch (trigger)
                    {
                        case ContextualActionTrigger.TriggeringCondition.InputTriggered:
                            if (character.Sprinting && jumpInstantBigJumpWhenSprinting)
                            {
                                jumpIsBigJump = true;
                                return true;
                            }
                            else if (jumpMaxKeyHoldTimeBeforeJump <= 0)
                            {
                                return true;
                            }
                            break;
                        case ContextualActionTrigger.TriggeringCondition.InputHeldDown:
                            jumpIsBigJump = true;
                            return true;
                        case ContextualActionTrigger.TriggeringCondition.InputReleased:
                            return true;
                    }
                }
            }
            else
            {
                // we're currently in a non-locomotion state, if it's an event and we're in the 
                // follow-through or recovery stages, let's save the desire to perform this action
                // for after the event completes.
                // this will be happened in our Custom Update function.
                /*string currentMxMEventState = "None";
                if (grounded && character.mxmAnimator.IsEventPlaying)
                {
                    currentMxMEventState = character.mxmAnimator.CurrentEventState.ToString();
                }*/
                //Debug.LogFormat("JUMP PRE-ACTION: grounded( {0} ), currentState( {1} ), currentMxMEventState( {2} )", grounded, currentState.ToString(), currentMxMEventState);
                EEventState currEventState = character.mxmAnimator.CurrentEventState;
                if ((character.currentState == NGCharacter.CharacterState.Parkour) && ((character.mxmAnimator.IsEventPlaying == false) || (currEventState == EEventState.FollowThrough) || (currEventState == EEventState.Recovery)))
                {
                    jumpParkourWantedDuringEndOfEvent = true;
                    jumpParkourWantedDuringEndOfEventTime = Time.time;
                }
            }
            return false;
        }

        public ContextualAction.Propagation Jump_PerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            jumpParkourWantedDuringEndOfEvent = false;
            jumpIgnoreKeyUp = true;
            character.DoJump(jumpIsBigJump);
            jumpIsBigJump = false;
            return ContextualAction.Propagation.StopPropagationForAllSuccessiveEvents;
        }

        public bool CrouchAndStrafe_CanPerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            return (character.currentState == NGCharacter.CharacterState.Locomotion) &&
                    character.controllerWrapper.IsGrounded && !character.mxmAnimator.IsEventPlaying;
        }

        public ContextualAction.Propagation CrouchToggle_PerformAction(ContextualActionTrigger.TriggeringCondition trigger) 
        {
            character.Crouching = !character.Crouching;
            return ContextualAction.Propagation.StopPropagation;
        }

        public ContextualAction.Propagation StrafeToggle_PerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            character.Strafing = !character.Strafing;
            return ContextualAction.Propagation.StopPropagation;
        }


        public bool SprintHold_CanPerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            return (character.movement.currentMode == NGCharacter.CharacterMode.Locomotion) ||
                   (character.movement.currentMode == NGCharacter.CharacterMode.Sprint);
        }

        public ContextualAction.Propagation SprintHold_PerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            if (trigger == ContextualActionTrigger.TriggeringCondition.InputTriggered)
            {
                character.Sprinting = true;
            }
            else if (trigger == ContextualActionTrigger.TriggeringCondition.InputReleased)
            {
                character.Sprinting = false;
            }
            return ContextualAction.Propagation.StopPropagation;
        }


        public virtual void CustomUpdate()
        {
            const float JUMP_WANTED_GRACE_PERIOD = 0.4f;

            if (character.controllerWrapper.IsGrounded && jumpParkourWantedDuringEndOfEvent && !character.mxmAnimator.IsEventPlaying) 
            {
                jumpParkourWantedDuringEndOfEvent = false;
                if ((Time.time - jumpParkourWantedDuringEndOfEventTime) < JUMP_WANTED_GRACE_PERIOD)
                {
                    if (jumpIsBigJump)
                    {
                        Jump_PerformAction(ContextualActionTrigger.TriggeringCondition.InputHeldDown);
                    }
                    else
                    {
                        Jump_PerformAction(ContextualActionTrigger.TriggeringCondition.InputReleased);
                    }
                }

            }

            /*
            if (Input.GetKeyDown(KeyCode.M))
            {
                keyProcessorJumpParkour.ProcessEvent_InputTrigger();
            }
            if (Input.GetKeyUp(KeyCode.M))
            {
                keyProcessorJumpParkour.ProcessEvent_InputRelease();
            }*/

        }

        public override void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen)
        {
            base.Initialize(pCharacter, pTrajGen);
            camTransform = character.mxmTrajectoryGenerator.RelativeCameraTransform;
            if (camTransform == null)
            {
                Debug.LogError("Invalid camera transform reference in the player character's MxMTrajectoryGenerator component.");
                return;
            }

            // setup trajectory generator.
            ConfigureTrajectoryGenerator();
            character.RegisterContextualAction(ACTION_JUMPIMMEDIATE, Jump_CanPerformAction, Jump_PerformAction);
            character.RegisterContextualAction(ACTION_JUMPDELAYED, Jump_CanPerformAction, Jump_PerformAction);
            character.RegisterContextualAction(ACTION_CROUCHTOGGLE, CrouchAndStrafe_CanPerformAction, CrouchToggle_PerformAction);
            character.RegisterContextualAction(ACTION_STRAFETOGGLE, CrouchAndStrafe_CanPerformAction, StrafeToggle_PerformAction);
            character.RegisterContextualAction(ACTION_SPRINTHOLD, SprintHold_CanPerformAction, SprintHold_PerformAction);
            character.RegisterCustomUpdateMethod(CustomUpdate);
            InitializeKeyProcessors();
        }

        [Header("Edit Mode Only:")]
        [Tooltip("These lists are modifiable only in Edit Mode.  To change these at runtime, interact with the InputScheme's ContextualActionProcessor through code (e.g. keyProcessorJumpParkour)")]
        // See ContextualActionTrigger source for configuration examples:
        public ContextualActionTrigger[] jumpParkourTriggers =
        {
                new ContextualActionTrigger(ACTION_JUMPIMMEDIATE),
                new ContextualActionTrigger(ACTION_JUMPDELAYED, ContextualActionTrigger.TriggeringCondition.InputHeldDown, 0.35f),
                new ContextualActionTrigger(ACTION_JUMPIMMEDIATE, ContextualActionTrigger.TriggeringCondition.InputReleased)
        };

        [Tooltip("These lists are modifiable only in Edit Mode.  To change these at runtime, interact with the InputScheme's ContextualActionProcessor through code (e.g. keyProcessorJumpParkour)")]
        public ContextualActionTrigger[] sprintHoldTriggers =
        {
                new ContextualActionTrigger(ACTION_SPRINTHOLD),
                new ContextualActionTrigger(ACTION_SPRINTHOLD, ContextualActionTrigger.TriggeringCondition.InputReleased)
            };

        [Tooltip("These lists are modifiable only in Edit Mode.  To change these at runtime, interact with the InputScheme's ContextualActionProcessor through code (e.g. keyProcessorJumpParkour)")]
        public ContextualActionTrigger[] crouchToggleTriggers =
        {
                new ContextualActionTrigger(ACTION_CROUCHTOGGLE)
            };

        [Tooltip("These lists are modifiable only in Edit Mode.  To change these at runtime, interact with the InputScheme's ContextualActionProcessor through code (e.g. keyProcessorJumpParkour)")]
        public ContextualActionTrigger[] strafeToggleTriggers =
        {
                new ContextualActionTrigger(ACTION_STRAFETOGGLE)
            };

        private void InitializeKeyProcessors()
        {
            keyProcessorJumpParkour = new ContextualActionProcessor(character, jumpParkourTriggers);
            keyProcessorSprintHold = new ContextualActionProcessor(character, sprintHoldTriggers);
            keyProcessorCrouchToggle = new ContextualActionProcessor(character, crouchToggleTriggers);
            keyProcessorStrafeToggle = new ContextualActionProcessor(character, strafeToggleTriggers);
        }

        public override void Deactivate()
        {
            character.UnregisterCustomUpdateMethod(CustomUpdate);
            character.UnregisterContextualAction(ACTION_JUMPIMMEDIATE);
            character.UnregisterContextualAction(ACTION_JUMPDELAYED);
            character.UnregisterContextualAction(ACTION_CROUCHTOGGLE);
            character.UnregisterContextualAction(ACTION_SPRINTHOLD);
            character.UnregisterContextualAction(ACTION_STRAFETOGGLE);
            base.Deactivate();
        }

        public override bool IsInputDriven()
        {
            return true;
        }

        protected void ConfigureTrajectoryGenerator()
        {
            TrajectoryGeneratorModule tgmod = ScriptableObject.CreateInstance<TrajectoryGeneratorModule>();
            Transform oldCameraTransform = mxmTrajectoryGenerator.RelativeCameraTransform;
            tgmod.MaxSpeed = mxmTrajectoryGenerator.MaxSpeed;
            tgmod.PosBias = mxmTrajectoryGenerator.PositionBias;
            tgmod.DirBias = mxmTrajectoryGenerator.DirectionBias;
            tgmod.ControlMode = ETrajectoryControlMode.UserInput;
            tgmod.TrajectoryMode = ETrajectoryMoveMode.Normal;
            tgmod.FlattenTrajectory = false;
            tgmod.CustomInput = true;
            tgmod.FaceDirectionOnIdle = mxmTrajectoryGenerator.FaceDirectionOnIdle;
            tgmod.StoppingDistance = mxmTrajectoryGenerator.StoppingDistance;
            tgmod.ApplyRootSpeedToNavAgent = mxmTrajectoryGenerator.ApplyRootSpeedToNavAgent;
            tgmod.ResetDirectionOnNoInput = true;
            //tgmod.CamTransform = mxmTrajectoryGenerator.RelativeCameraTransform;
            tgmod.InputProfile = mxmTrajectoryGenerator.InputProfile;
            mxmTrajectoryGenerator.SetTrajectoryModule(tgmod);
            mxmTrajectoryGenerator.RelativeCameraTransform = oldCameraTransform;
        }

        protected Vector3 GetFinalInputVector(Vector3 rawInput)
        {
            if (doEnvironmentAwareTrajectories)
            {
                if (camTransform == null)
                {
                    camTransform = mxmTrajectoryGenerator.RelativeCameraTransform;

                    if (camTransform == null)
                    {
                        Debug.LogError("Invalid camera transform reference in the player character's MxMTrajectoryGenerator component.");
                        return Vector3.zero;
                    }
                }


                float newScale = GetDesiredTrajectoryScale(camTransform, rawInput);
                //Debug.Log($"GetFinalInputVector now raw( {rawInput.x}, {rawInput.y}, {rawInput.z} ) newScale( {newScale} )");
                return rawInput * newScale;
            }

            return rawInput;
        }
    }
}
