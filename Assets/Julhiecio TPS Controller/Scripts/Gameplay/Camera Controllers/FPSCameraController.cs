using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.JUInputSystem;
using JUTPS.WeaponSystem;
using JUTPS.VehicleSystem;

namespace JUTPS.CameraSystems
{
    [AddComponentMenu("JU TPS/Cameras/JU FPS Camera Controller")]
    public class FPSCameraController : JUCameraController
    {
        private JUTPS.CharacterBrain.JUCharacterBrain characterTarget;
        float ymouse, xmouse;
        float SmoothedYMouse, SmoothedXMouse;
        float ScopeModeRecoil;
        float weight;
        Vector3 CamPosition;

        Vector3 SmoothedCameraPosition;

        public JUPlayerCharacterInputAsset InputAsset;
        public CameraState FPSCameraState = new CameraState("FPS Camera State", 0, 100, 50);
        public CameraState AimModeCameraState = new CameraState("FPS Camera State", 0, 100, 50);
        public CameraState DrivingModeCameraState = new CameraState("FPS Camera State", 0, 1000, 70);

        [Header("Weapon Sway Config")]
        public float AimInSpeed = 6;
        public float AimOutSpeed = 6;
        public float SwaySpeed = 5;
        public float HorizontalIntensity = 1;
        public float VerticalIntensity = 1;
        public float AimHorizontalIntensity = 1;
        public float AimVerticalIntensity = 1;
        // Start is called before the first frame update
        protected override void Start()
        {
            var defaultTargetToFollow = TargetToFollow;

            // Find the target to follow if is null.
            base.Start();

            if (TargetToFollow.root.TryGetComponent(out JUCharacterController JUcharacter))
            {
                characterTarget = JUcharacter;
                characterTarget.LocomotionMode = JUTPS.CharacterBrain.JUCharacterBrain.MovementMode.AwaysInFireMode;

                // Only use the spline only if there was not a default transform to follow.
                if (defaultTargetToFollow == null)
                    TargetToFollow = characterTarget.HumanoidSpine;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked && JUGameManager.IsMobileControls == false)
            {
                ymouse = 0;
                xmouse = 0;
                return;
            }

            // Mouse Input
            if (InputAsset)
            {
                ymouse = (Aiming ? 30 : 100) * InputAsset.LookAxis.y / 100;
                xmouse = (Aiming ? 30 : 100) * InputAsset.LookAxis.x / 100;
            }

            // Driving Camera 
            if (characterTarget != null)
            {
                if (characterTarget.IsDriving)
                {
                    ymouse = 0;
                    xmouse = 0;
                    SetCameraRotation(TargetToFollow.transform.rotation.x, characterTarget.DriveVehicles.CurrentVehicle.transform.eulerAngles.y, false);

                }
            }
            else
            {
                RotateCamera(ymouse, xmouse, upward: characterTarget == null ? TargetToFollow.up : characterTarget.transform.up);
                return;
            }


            characterTarget.IsRolling = false;

            if (characterTarget.IsDriving)
            {
                SetCameraStateTransition(GetCurrentCameraState, DrivingModeCameraState);

                JUVehicle drivingVehicle = characterTarget.DriveVehicles.CurrentVehicle;
                RotateCamera(ymouse, xmouse, upward: drivingVehicle.transform.up, AlternativeTargetToCalculate: drivingVehicle.transform);
            }
            else
            {
                SetCameraStateTransition(GetCurrentCameraState, Aiming ? AimModeCameraState : FPSCameraState);
                RotateCamera(ymouse, xmouse, upward: characterTarget == null ? TargetToFollow.up : characterTarget.transform.up);
            }

        }
        private void LateUpdate()
        {
            SetFieldOfView(GetCurrentCameraState.CameraFieldOfView);
            SetCameraPositionToScopePosition();
        }
        private void FixedUpdate()
        {
            SetPivotCameraPosition(GetCurrentCameraState.GetCameraPivotPosition(TargetToFollow), false);
        }
        public override void RecoilReaction(float Force)
        {
            base.RecoilReaction(Force);

            ScopeModeRecoil -= Force / 30;
        }
        public void SetCameraPositionToScopePosition()
        {
            if (characterTarget == null) return;

            Aiming = characterTarget.IsAiming;

            if (characterTarget.RightHandWeapon == null || characterTarget.IsDriving) return;

            if (characterTarget.RightHandWeapon.AimMode != Weapon.WeaponAimMode.None && characterTarget.FiringMode)
            {
                var gun = characterTarget.RightHandWeapon;

                SmoothedYMouse = Mathf.Lerp(SmoothedYMouse, xmouse * (Aiming ? AimHorizontalIntensity : HorizontalIntensity), SwaySpeed * Time.deltaTime);
                SmoothedXMouse = Mathf.Lerp(SmoothedXMouse, ymouse * (Aiming ? AimVerticalIntensity : VerticalIntensity), SwaySpeed * Time.deltaTime);
                ScopeModeRecoil = Mathf.Lerp(ScopeModeRecoil, 0, 5 * Time.deltaTime);
                //Debug.Log("rot int = " + SmoothedXMouse);

                Vector3 scopePosition = gun.transform.position
                    + gun.transform.right * (gun.CameraAimingPosition.x - SmoothedYMouse / 20)
                    + gun.transform.up * (gun.CameraAimingPosition.y - SmoothedXMouse / 20)
                    + mCamera.transform.parent.forward * (gun.CameraAimingPosition.z - ScopeModeRecoil);

                //var target = TargetToFollow;
                Vector3 normalPosition = TargetToFollow.transform.position
                    + mCamera.transform.parent.right * (GetCurrentCameraState.RightCameraOffset - SmoothedYMouse / 10)
                    + mCamera.transform.parent.up * (GetCurrentCameraState.UpCameraOffset - SmoothedXMouse / 4)
                    + mCamera.transform.parent.forward * GetCurrentCameraState.ForwardCameraOffset;

                if (Aiming == false)
                {
                    weight = Mathf.MoveTowards(weight, 1, AimInSpeed * Time.deltaTime);
                }
                else
                {
                    weight = Mathf.MoveTowards(weight, 0, AimOutSpeed * Time.deltaTime);
                }

                CamPosition = Vector3.Lerp(scopePosition, normalPosition, weight);
                SmoothedCameraPosition = Vector3.Slerp(SmoothedCameraPosition, CamPosition, 60 * Time.deltaTime);
                SetCameraPosition(CamPosition, false);

                //Set Field Of View
                AimModeCameraState.CameraFieldOfView = Mathf.Lerp(AimModeCameraState.CameraFieldOfView, gun.CameraFOV, 15 * Time.deltaTime);
            }
            else
            {
                AimModeCameraState.CameraFieldOfView = GetCurrentCameraState.CameraFieldOfView;
            }
        }
    }

}