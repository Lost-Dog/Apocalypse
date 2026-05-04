using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{

    [CreateAssetMenu(fileName = "NGInputScheme_InputManager", menuName = "Threepeat/Input Scheme/InputManager")]
    public class NGInputScheme_InputManager : NGInputSchemeInputDriven
    {
        [Header("InputManager settings")]
        public string moveAxisHorizontal = "Horizontal";
        public string moveAxisVertical = "Vertical";
        

        [Header("Event Input Assignments")]
        public KeyCode keycodeCrouchPrimary = KeyCode.LeftControl;
        public KeyCode keycodeCrouchSecondary = KeyCode.JoystickButton8;
        public KeyCode keycodeSprint = KeyCode.LeftShift;
        public KeyCode keycodeJumpPrimary = KeyCode.Space;
        public KeyCode keycodeJumpSecondary = KeyCode.Joystick1Button0;
        public KeyCode keycodeStrafePrimary = KeyCode.LeftAlt;
        public KeyCode keycodeStrafeSecondary = KeyCode.Joystick1Button9;

        [Tooltip("See documentation for instructions to set up InputManager to support Joystick input.")]
        public bool useJoystickAxes = false;
        private bool joystickAxesDefined = false;

        /*[InspectorButton("HelpButton_Joystick", ButtonWidth=200)]
        public bool DocsOnWeb_InputHelp = false;*/

        //[Header("Player Only - Jumping and Parkour")]
        Dictionary<KeyCode, ContextualActionProcessor> inputCaps = new Dictionary<KeyCode, ContextualActionProcessor>();
        public void ConnectCAPToInput(ContextualActionProcessor cap, KeyCode act)
        {
            inputCaps.Add(act, cap);
        }

        public override void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen)
        {
            base.Initialize(pCharacter, pTrajGen);
            ConnectCAPToInput(keyProcessorJumpParkour, keycodeJumpPrimary);
            ConnectCAPToInput(keyProcessorJumpParkour, keycodeJumpSecondary);
            ConnectCAPToInput(keyProcessorCrouchToggle, keycodeCrouchPrimary);
            ConnectCAPToInput(keyProcessorCrouchToggle, keycodeCrouchSecondary);
            ConnectCAPToInput(keyProcessorStrafeToggle, keycodeStrafePrimary);
            ConnectCAPToInput(keyProcessorStrafeToggle, keycodeStrafeSecondary);
            ConnectCAPToInput(keyProcessorSprintHold, keycodeSprint);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        // this is added to the character by the predecessor (NGInputSchemeInputDriven)
        public override void CustomUpdate()
        {
            base.CustomUpdate();
            //Debug.LogFormat("CustomUpdate( instance {0} ) called!", this.GetInstanceID());
            bool grounded = character.controllerWrapper.IsGrounded;

            float leftTrigger = 0f;

            if (useJoystickAxes)
            {
                if (joystickAxesDefined)
                {
                    leftTrigger = Input.GetAxis("JoystickLeftTrigger");
                }
                else
                {
                    try
                    {
                        leftTrigger = Input.GetAxis("JoystickLeftTrigger");
                        joystickAxesDefined = true;
                    }
                    catch
                    {
                        Debug.LogError("JoystickLeftTrigger (sprint key mapping) is not defined in Input Manager");
                        useJoystickAxes = false;
                    }
                }
            }

            bool sprintJoystickInputWasPressed = sprintJoystickInputIsPressed;

            sprintJoystickInputIsPressed = (useJoystickAxes && (leftTrigger > 0.5f));

            if (useJoystickAxes && (sprintJoystickInputWasPressed != sprintJoystickInputIsPressed))
            {
                if (sprintJoystickInputIsPressed)
                {
                    keyProcessorSprintHold.ProcessEvent_InputTrigger();
                }
                else
                {
                    keyProcessorSprintHold.ProcessEvent_InputRelease();
                }
            }

            foreach (var actMapping in inputCaps)
            {
                if (Input.GetKeyDown(actMapping.Key))
                {
                    actMapping.Value.ProcessEvent_InputTrigger();
                }
                else if (Input.GetKeyUp(actMapping.Key))
                {
                    actMapping.Value.ProcessEvent_InputRelease();
                }
            }

            /*if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                character.Strafing = !character.Strafing;
            }*/
                /* available: have strafing, but it increases the complexity and would need a copy of camera in use by the actual project to make sure Motion Matching handles rotation smoothly.
                 * UNCOMMENTING this and the below block marked "STRAFESTRAFE" will enable a basic strafing capability if desired.
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

        }

        protected override Vector3 GetInputVector()
        {
            Vector3 rawInput = new Vector3(Input.GetAxis(moveAxisHorizontal), 0f, Input.GetAxis(moveAxisVertical));

            return GetFinalInputVector(rawInput);
        }

        public override string GetSchemeName()
        {
            return "InputManager";
        }
    }
}