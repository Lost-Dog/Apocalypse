using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Threepeat
{
    [CreateAssetMenu(fileName = "NGInputScheme_InputSystem", menuName = "Threepeat/Input Scheme/InputSystem (new system)")]
    public class NGInputScheme_InputSystem : NGInputSchemeInputDriven
    {
#if (ENABLE_INPUT_SYSTEM)
        [Header("InputSystem settings")]
        public InputActionAsset inputActionAsset;
        public string inputActionMapActive = "Player";
        
        [Tooltip("Player movement:  ActionType: Value / Vector2")]
        public string actionNameMovement = "move";
        [Tooltip("Jump:  ActionType: Button")]
        public string actionNameJump = "jump_mmlc";
        [Tooltip("Sprint (hold to sprint):  ActionType: Button")]
        public string actionNameSprint = "sprint_mmlc";
        [Tooltip("Crouching (toggle):  ActionType: Button")]
        public string actionNameCrouch = "crouchToggle_mmlc";

        [Tooltip("Strafing (toggle):  ActionType: Button")]
        public string actionNameStrafe = "strafeToggle_mmlc";

        private PlayerInput playerInput = null;
        
        private InputAction actionMove;
        private InputAction actionJump;
        private InputAction actionSprint;
        private InputAction actionCrouch;
        private InputAction actionStrafe;

        Dictionary<InputAction, ContextualActionProcessor> inputCaps = new Dictionary<InputAction, ContextualActionProcessor>();

        public void ConnectCAPToInput(ContextualActionProcessor cap, InputAction act)
        {
            inputCaps.Add(act, cap);
        }

#endif

        public override string GetSchemeName()
        {
            return "InputSystem";
        }

#if (ENABLE_INPUT_SYSTEM)

        public override void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen)
        {
            base.Initialize(pCharacter, pTrajGen);

            //TODO: ADD PlayerInput component to character if not already there, and activate it regardless
            playerInput = character.GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                playerInput = character.gameObject.AddComponent<PlayerInput>();
                playerInput.actions = inputActionAsset;
                playerInput.defaultActionMap = inputActionMapActive;
                playerInput.camera = camTransform.GetComponent<Camera>();
                playerInput.notificationBehavior = PlayerNotifications.SendMessages;
            }
            playerInput.enabled = true;
            playerInput.ActivateInput();
            playerInput.enabled = true;

            actionMove = playerInput.actions[actionNameMovement];
            actionJump = playerInput.actions[actionNameJump];
            actionSprint = playerInput.actions[actionNameSprint];
            actionCrouch = playerInput.actions[actionNameCrouch];
            actionStrafe = playerInput.actions[actionNameStrafe];
            ConnectCAPToInput(keyProcessorJumpParkour, actionJump);
            ConnectCAPToInput(keyProcessorCrouchToggle, actionCrouch);
            ConnectCAPToInput(keyProcessorSprintHold, actionSprint);
            ConnectCAPToInput(keyProcessorStrafeToggle, actionStrafe);
        }

        public override void Deactivate()
        {
            Debug.Log("InputSystem::Deactivate");
            // disable PlayerInput script
            playerInput.DeactivateInput();
            playerInput.enabled = false;

            character.UnregisterCustomUpdateMethod(CustomUpdate);
            base.Deactivate();
        }

        protected override Vector3 GetInputVector()
        {
            Vector2 val = actionMove.ReadValue<Vector2>();
            
            Vector3 rawInput = new Vector3(val.x, 0f, val.y);

            return GetFinalInputVector(rawInput);
        }


        protected void CheckInputs()
        {
            foreach (var actMapping in inputCaps)
            {
                if (actMapping.Key.triggered)
                {
                    actMapping.Value.ProcessEvent_InputTrigger();
                }
                else if (actMapping.Key.WasReleasedThisFrame())
                {
                    actMapping.Value.ProcessEvent_InputRelease();
                }
            }
        }

        // this is added to the character by the predecessor (NGInputSchemeInputDriven)
        public override void CustomUpdate()
        {
            CheckInputs();
        
            //Debug.LogFormat("CustomUpdate( instance {0} ) called!", this.GetInstanceID());
            bool grounded = character.controllerWrapper.IsGrounded;


            /* available: have strafing, but it increases the complexity and would need a copy of camera in use by the actual project to make sure Motion Matching handles rotation smoothly.
             * UNCOMMENTING this and the below block marked "STRAFESTRAFE" will enable a basic strafing capability if desired.
             * 
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.JoystickButton5))
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
            }
            */

            /*if (character.currentMode == NGCharacter.CharacterMode.Locomotion)
            {
                if (sprintKeyIsDown && !character.Sprinting)
                {
                    // keep trying to enter sprint
                    character.Sprinting = true;
                }
            }
            else if (character.currentMode == NGCharacter.CharacterMode.Sprint)
            {
                //if (!Input.GetKey(keycodeSprint) || (useJoystickAxes && (leftTrigger < 0.3f)))
                if (actionSprint.WasReleasedThisFrame())
                {
                    character.Sprinting = false;
                }
            }*/
        }
#else   

    public string InputSystemNotInstalled = "Please install InputSystem from Package Manager -> Unity Registry";

    public override void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen) {}
    public override void Deactivate() {}
    protected override Vector3 GetInputVector() { return Vector3.zero; }
#endif

    }
}