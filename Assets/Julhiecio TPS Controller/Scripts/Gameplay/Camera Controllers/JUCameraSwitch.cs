using JU.Editor;
using JUTPS;
using JUTPS.ActionScripts;
using JUTPS.CameraSystems;
using JUTPS.CharacterBrain;
using JUTPS.CrossPlataform;
using JUTPS.InputEvents;
using JUTPS.InventorySystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("JU TPS/Cameras/Camera Switch System/JU Camera Switch")]
public class JUCameraSwitch : MonoBehaviour
{
    public InputEvent SwitchCameraInput;

    private int currentCam;
    public JUCharacterController Character;
    public FPSCameraController FpsCam;
    public TPSCameraController TpsCam;
    public TDCameraController TdCam;

    public Renderer[] PartsToDisableOnFps;

    public enum CameraStyles { ThirdPerson, FirstPerson, TopDown }

    public bool CanSwitch
    {
        get
        {
            if (JUPauseGame.IsPaused || !JUEditor.IsGameFocused)
                return false;

            if (!Character.UseDefaultControllerInput)
                return false;

            if (Character.DisableAllMove)
                return false;

            return true;
        }
    }

    void Start()
    {
        FpsCam.gameObject.SetActive(false);
        Refresh();
    }
    private void OnEnable()
    {
        SwitchCameraInput.SetupListeners();
        SwitchCameraInput.OnInputPerformed.AddListener(SwitchToNextCamera);
    }
    private void OnDisable()
    {
        SwitchCameraInput.RemoveListeners();
    }


    public void SwitchToNextCamera()
    {
        if (!CanSwitch)
            return;

        currentCam += 1;
        if (currentCam > 2) currentCam = 0;

        // Check null cameras
        if (currentCam == 0 && FpsCam == null) { currentCam = 1; }
        if (currentCam == 1 && TpsCam == null) { currentCam = 2; }
        if (currentCam == 2 && TdCam == null) { currentCam = 0; }

        Refresh();
    }

    public void SwitchCamera(CameraStyles cameraStyle)
    {
        if (!CanSwitch)
            return;

        Character.BlockFireModeOnPunching = false;

        //Get essential components
        var topDownAimMouse = Character.GetComponent<AimOnMousePosition>();
        var topDownAimGamepad = Character.GetComponent<AimOnRightJoystickDirection>();
        var topDownAimControlSwitcher = Character.GetComponent<AimControllSwitcher>();

        var isFps = false;
        switch (cameraStyle)
        {
            case CameraStyles.FirstPerson: // FPS
                isFps = true;
                if (FpsCam != null) FpsCam.gameObject.SetActive(true);
                if (TpsCam != null) TpsCam.gameObject.SetActive(false);
                if (TdCam != null) TdCam.gameObject.SetActive(false);

                // Set Camera Position
                if (FpsCam != null) FpsCam.transform.position = Character.transform.position - FpsCam.transform.forward * 5f;

                Character.MyPivotCamera = FpsCam;
                Character.LookAtPosition = Vector3.zero;

                if (topDownAimMouse) topDownAimMouse.enabled = false;
                if (topDownAimGamepad) topDownAimGamepad.enabled = false;
                if (topDownAimControlSwitcher) topDownAimControlSwitcher.enabled = false;

                JUCameraController.LockMouse(true, true);
                break;
            case CameraStyles.ThirdPerson: // TPS
                if (FpsCam != null) FpsCam.gameObject.SetActive(false);
                if (TpsCam != null) TpsCam.gameObject.SetActive(true);
                if (TdCam != null) TdCam.gameObject.SetActive(false);

                // Set Camera Position
                if (TpsCam != null) TpsCam.transform.position = Character.transform.position - TpsCam.transform.forward * 5f;

                Character.MyPivotCamera = TpsCam;
                Character.LookAtPosition = Vector3.zero;

                if (topDownAimMouse) topDownAimMouse.enabled = false;
                if (topDownAimGamepad) topDownAimGamepad.enabled = false;
                if (topDownAimControlSwitcher) topDownAimControlSwitcher.enabled = false;

                JUCameraController.LockMouse(true, true);
                break;
            case CameraStyles.TopDown: // TOP DOWN
                if (FpsCam != null) FpsCam.gameObject.SetActive(false);
                if (TpsCam != null) TpsCam.gameObject.SetActive(false);
                if (TdCam != null) TdCam.gameObject.SetActive(true);

                // Set Camera Position
                if (TdCam != null) TdCam.transform.position = Character.transform.position - TdCam.transform.forward * 5f;

                Character.MyPivotCamera = TdCam;
                Character.BlockFireModeOnCursorVisible = false;

                if (topDownAimMouse) topDownAimMouse.enabled = true;
                if (topDownAimGamepad) topDownAimGamepad.enabled = true;
                if (topDownAimControlSwitcher) topDownAimControlSwitcher.enabled = true;

                JUCameraController.LockMouse(false, true);
                break;
            default:
                break;
        }

        foreach (var part in PartsToDisableOnFps)
            part.shadowCastingMode = !isFps ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;


        // Fix Camera Rotation
        if (!isFps)
        {
            if (FpsCam && TpsCam) TpsCam.SetCameraRotation(FpsCam.rotX, FpsCam.rotY, false);
            if (FpsCam && TpsCam) TpsCam.rotxtarget = FpsCam.rotxtarget;
            if (FpsCam && TpsCam) TpsCam.rotytarget = FpsCam.rotytarget;
        }
        else
        {
            if (FpsCam && TpsCam) FpsCam.SetCameraRotation(TpsCam.rotX, TpsCam.rotY, false);
            if (FpsCam && TpsCam) FpsCam.rotxtarget = TpsCam.rotxtarget;
            if (FpsCam && TpsCam) FpsCam.rotytarget = TpsCam.rotytarget;
        }

        Character.SetAnimatorParameters();
    }

    private void Refresh()
    {
        // Switch Camera Styles
        switch (currentCam)
        {
            case 0: // FPS
                SwitchCamera(CameraStyles.FirstPerson);
                break;
            case 1: // TPS
                SwitchCamera(CameraStyles.ThirdPerson);
                break;
            case 2: // TOP DOWN
                SwitchCamera(CameraStyles.TopDown);
                break;
            default:
                break;
        }
    }
}