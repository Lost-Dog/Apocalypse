using JUTPS.JUInputSystem;
using JUTPSActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JUDashSkill : JUTPSAnimatedAction
{
    [Header("Animation Settings")]
    [SerializeField] private string DashStateName = "Dash";
    public int AnimatorLayerIndex = 9;


    [Header("Dash Settings")]
    public float DashSpeed;
    public AnimationCurve ForwardMovementCurve, SidesMovementCurve, UpMovementCurve;
    public bool UseGravity = true;
    //Double Shift Dash
    public bool UseDoubleRunButtonToDash = true;
    public bool RotateToDesiredDirection = true;
    private bool pressedRunButton;
    private float timeToDisable_PressedRunButton = 0.4f;
    private float currentTimeToDisable_PressedRunButton;
    //Input
    public JUTPS.InputEvents.MultipleActionEvent CustomInputs;
    //Dash Timer
    public float TimeToUseDashAgain = 1;
    [Header("Unity Events")]
    public UnityEvent OnDash;
    public UnityEvent OnEndDash;

    [Header("IK Disabling")]
    public bool DisableIK = true;
    public float TimeToEnableIK = 0.4f;

    [Header("State")]
    public bool CanDash = true;

    private void OnEnable()
    {
        if (CustomInputs.Actions.Count > 0)
        {
            CustomInputs.Enable();
            CustomInputs.OnButtonsDown.AddListener(StartDash);
        }
    }
    private void OnDisable()
    {
        if (CustomInputs.Actions.Count > 0)
        {
            CustomInputs.Disable();
            CustomInputs.OnButtonsDown.RemoveListener(StartDash);
        }
    }

    private void Start()
    {
        SwitchAnimationLayer(AnimatorLayerIndex);
    }



    public override void ActionCondition()
    {
        DoubleShiftToDash();
    }
    public override void OnActionStarted()
    {
        if (RotateToDesiredDirection) transform.rotation = TPSCharacter.DirectionTransform.rotation;
        DisableCharacterMovement();
        PlayAnimation(DashStateName);
        if (DisableIK) { TPSCharacter.InverseKinematics = false; CancelInvoke(nameof(enableIK)); }

        //Disable Dash
        if (TimeToUseDashAgain > 0) { CancelInvoke(nameof(enableDash)); disableDash(); }

        //On Start Dash Event Call
        OnDash.Invoke();
    }
    public override void OnActionEnded()
    {
        TPSCharacter.enableMove();
        if (DisableIK) Invoke(nameof(enableIK), TimeToEnableIK);

        //Enable Dash
        if (TimeToUseDashAgain > 0) { Invoke(nameof(enableDash), TimeToUseDashAgain); }

        //On Stop Dash Event Call
        OnEndDash.Invoke();
    }



    public void StartDash()
    {
        if (CanDash == false) return;
        StartAction();
    }
    private void enableIK() { TPSCharacter.InverseKinematics = true; }
    private void enableDash() => CanDash = true;
    private void disableDash() => CanDash = false;



    public override void OnActionIsPlaying()
    {
        float ActionCurrentDuration = ActionCurrentTime / ActionDuration;
        if (UseGravity)
        {
            Vector3 forwardMovement = transform.forward * ForwardMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 sidesMovement = transform.right * SidesMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 gravityMovement = Vector3.up * rb.linearVelocity.y;
            rb.linearVelocity = forwardMovement + sidesMovement + gravityMovement;
        }
        else
        {
            Vector3 forwardMovement = transform.forward * ForwardMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 sidesMovement = transform.right * SidesMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 upMovement = transform.up * UpMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            rb.linearVelocity = forwardMovement + sidesMovement + upMovement;
        }
    }



    //This is double shift to dash, if you don't want to use this input you can remove this function below
    private void DoubleShiftToDash()
    {
        if (UseDoubleRunButtonToDash == false) return;

        if (TPSCharacter.Inputs && TPSCharacter.UseDefaultControllerInput && TPSCharacter.Inputs.IsRunTriggered)
        {
            if (pressedRunButton)
            {
                StartDash();
            }
            else
            {
                pressedRunButton = true;
            }
        }

        //Timer between shift presses
        if (pressedRunButton)
        {
            currentTimeToDisable_PressedRunButton += Time.deltaTime;
            if (currentTimeToDisable_PressedRunButton >= timeToDisable_PressedRunButton)
            {
                pressedRunButton = false;
                currentTimeToDisable_PressedRunButton = 0;
            }
        }
    }
}
