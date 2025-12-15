using JUTPS.FX;
using JUTPS.InteractionSystem;
using JUTPS.InteractionSystem.Interactables;
using JUTPS.JUInputSystem;
using JUTPS.PhysicsScripts;
using JUTPS.VehicleSystem;
using UnityEngine;
using UnityEngine.Events;


namespace JUTPS.ActionScripts
{
    /// <summary>
    /// Able the <see cref="JUCharacterController"/> enter or exit of any <see cref="VehicleSystem.Vehicle"/>.
    /// </summary>
    [AddComponentMenu("JU TPS/Third Person System/Actions/Drive Vehicles System")]
    public class DriveVehicles : JUTPSActions.JUTPSAction
    {
        private JUFootstep _footstepSoundsToDisable;
        private AdvancedRagdollController _ragdoller;

        /// Called when the character start enter on a <see cref="Vehicle"/>.
        /// </summary>
        public UnityAction OnStartEnterVehicle;

        /// <summary>
        /// Called when the character start exit from a <see cref="CurrentVehicle"/>.
        /// </summary>
        public UnityAction OnStartExitVehicle;

        /// <summary>
        /// Called when the character cancel enter on a <see cref="Vehicle"/> caused by character movement or ragdoll during the enter vehicle state.
        /// </summary>
        public UnityAction OnCancelEnterVehicle;

        /// <summary>
        /// Called when the character cancel exit from a <see cref="CurrentVehicle"/> caused by character ragdoll during the exit vehicle state.
        /// </summary>
        public UnityAction OnCancelExitVehicle;

        /// <summary>
        /// The vehicle used to start game driving.
        /// </summary>
        [SerializeField] private JUVehicle _startVehicle;

        /// <summary>
        /// If true, the character can exit from the <see cref="CurrentVehicle"/> if is driving.
        /// </summary>
        public bool ExitVehiclesEnabled;

        /// <summary>
        /// Disable the character after enter on the vehicle, usefull for vehicles that not have IK settings for drive animations.
        /// </summary>
        public bool DisableCharacterOnEnter;

        /// <summary>
        /// The time to reactive enter vehicle action after start enter or exit of some <see cref="Vehicle"/>. <para />
        /// Can't be less than 0.1.
        /// </summary>
        [Min(0.1f)] public float DelayToReenableAction;

        /// <summary>
        /// Don't allow the character exit from the <see cref="CurrentVehicle"/> if the <see cref="Vehicle"/> speed is greater than <see cref="MaxVehicleSpeedToExit"/>.
        /// </summary>
        [Min(1)] public float MaxVehicleSpeedToExit;

        /// <summary>
        /// The layer used to detect the ground to set the character position when exit from the <see cref="Vehicle"/>.
        /// </summary>
        public LayerMask GroundLayer;

        /// <summary>
        /// Called when the character enter on a <see cref="Vehicle"/>.
        /// </summary>
        public UnityEvent OnEnterVehicle;

        /// <summary>
        /// Called when the character exit from a <see cref="Vehicle"/>.
        /// </summary>
        public UnityEvent OnExitVehicle;

        /// <summary>
        /// Return true if the character is inside of a <see cref="Vehicle"/>.
        /// </summary>
        public bool IsDriving { get; private set; }

        /// <summary>
        /// Return false if the character has started to enter on a <see cref="Vehicle"/>. <para/>
        /// Must wait the <see cref="DelayToReenableAction"/> to able interact with a <see cref="Vehicle"/> again.
        /// </summary>
        public bool IsCharacterEntering { get; private set; }

        /// <summary>
        /// Return false if the character has started to exit from a <see cref="Vehicle"/>. <para/>
        /// Must wait the <see cref="DelayToReenableAction"/> to able interact with a <see cref="Vehicle"/> again.
        /// </summary>
        public bool IsCharacterExiting { get; private set; }

        /// <summary>
        /// The character interaction system.
        /// </summary>
        public JUInteractionSystem InteractionSystem { get; private set; }

        /// <summary>
        /// The current vehicle that the character is driving.
        /// </summary>
        public JUVehicle CurrentVehicle { get; private set; }

        /// <summary>
        /// Returns a component of <see cref="CurrentVehicle"/> that contains all character IK settings if is driving a <see cref="Vehicle"/>.
        /// </summary>
        public JUVehicleCharacterIK CurrentVehicleCharacterIK { get; private set; }

        /// <summary>
        /// The inputs with interaction keys/buttons that comes from the <see cref="JUTPSActions.JUTPSAction.TPSCharacter"/> inputs.
        /// </summary>
        public JUPlayerCharacterInputAsset Inputs
        {
            get => TPSCharacter ? TPSCharacter.Inputs : null;
        }

        /// <summary>
        /// Return true if the character is driving and can exit from the <see cref="CurrentVehicle"/>.
        /// </summary>
        public bool CanExitVehicle
        {
            get => ExitVehiclesEnabled && IsDriving && !IsCharacterExiting && !IsCharacterEntering && CurrentVehicle.Velocity.magnitude < MaxVehicleSpeedToExit;
        }

        /// <summary>
        /// Create an instance of <see cref="DriveVehicles"/> component.
        /// </summary>
        public DriveVehicles()
        {
            ExitVehiclesEnabled = true;
            DelayToReenableAction = 0.2f;
            GroundLayer = 1;

            MaxVehicleSpeedToExit = 1000;
        }

        private void Reset()
        {
            LayerMask[] defaultGroundLayers = new LayerMask[]
            {
                LayerMask.NameToLayer("Ground"),
                LayerMask.NameToLayer("Default")
            };

            GroundLayer = 0;
            for (int i = 0; i < defaultGroundLayers.Length; i++)
            {
                if (defaultGroundLayers[i] != -1)
                    GroundLayer |= 1 << defaultGroundLayers[i];
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _footstepSoundsToDisable = GetComponent<JUFootstep>();
            _ragdoller = GetComponent<AdvancedRagdollController>();
            InteractionSystem = GetComponent<JUInteractionSystem>();

            Debug.Assert(InteractionSystem, $"The character {name} does not have a {typeof(JUInteractionSystem).Name}");
        }

        private void Start()
        {
            if (InteractionSystem)
                InteractionSystem.OnInteract.AddListener(OnInteract);

            if (_startVehicle)
                DriveVehicle(_startVehicle, _startVehicle.GetComponent<JUVehicleCharacterIK>(), true);
        }

        private void OnDestroy()
        {
            if (InteractionSystem)
                InteractionSystem.OnInteract.RemoveListener(OnInteract);
        }

        private void Update()
        {
            if (Inputs && InteractionSystem && InteractionSystem.UseDefaultInputs)
            {
                if (Inputs.IsInteractTriggered && IsDriving)
                    ExitVehicle();
            }

            if (IsDriving)
            {
                if (_ragdoller && _ragdoller.State == AdvancedRagdollController.RagdollState.Ragdolled)
                {
                    ExitVehicle();
                    return;
                }

                UpdateDrivingState();
            }
            else if (IsCharacterEntering || IsCharacterExiting)
                UpdateEnteringExitingState();
        }

        private void UpdateDrivingState()
        {
            // Physic Changes.
            if (rb)
                rb.linearVelocity = CurrentVehicle.RigidBody.linearVelocity;

            // Update character position inside vehicle.
            if (CurrentVehicleCharacterIK && CurrentVehicleCharacterIK.InverseKinematicTargetPositions.CharacterPosition)
            {
                var seat = CurrentVehicleCharacterIK.InverseKinematicTargetPositions.CharacterPosition;
                transform.position = seat.position;
                transform.rotation = seat.rotation;
            }
            else
            {
                transform.position = CurrentVehicle.transform.position;
                transform.rotation = CurrentVehicle.transform.rotation;
            }
        }

        /// <summary>
        /// Enter on a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to enter.</param>
        /// <param name="vehicleCharacterIK">The vehicle settings for character animation.</param>
        /// <param name="immediately">If true, the character will not have a delay to enter, it's made immediately.</param>
        public void DriveVehicle(JUVehicle vehicle, JUVehicleCharacterIK vehicleCharacterIK, bool immediately = false)
        {
            if (!vehicle || !TPSCharacter || CurrentVehicle)
                return;

            CurrentVehicle = vehicle;
            CurrentVehicleCharacterIK = vehicleCharacterIK;

            if (_footstepSoundsToDisable)
                _footstepSoundsToDisable.enabled = false;

            StartEnterVehicleState();

            if (immediately)
                EndEnterVehicleState();
            else
                Invoke(nameof(EndEnterVehicleState), DelayToReenableAction);
        }

        /// <summary>
        /// Exit from the <see cref="CurrentVehicle"/> if is driving.
        /// </summary>
        public void ExitVehicle()
        {
            if (!CanExitVehicle || !TPSCharacter || !IsDriving)
                return;

            if (_footstepSoundsToDisable)
                _footstepSoundsToDisable.enabled = true;

            if (CurrentVehicleCharacterIK)
                TPSCharacter.transform.position = CurrentVehicleCharacterIK.GetExitPosition(GroundLayer);

            // Try spawn the character on any position around the vehicle if not have an exit position assigned.
            else
                TPSCharacter.transform.position = CurrentVehicle.transform.position + (-CurrentVehicle.transform.right * 3);

            OnCharacterStopDriving();
            StartExitVehicleState();

            Invoke(nameof(EndExitVehicleState), DelayToReenableAction);
        }

        private void UpdateEnteringExitingState()
        {
            TPSCharacter.DisableLocomotion();

            bool isMoving = TPSCharacter.IsMoving;
            bool isRagdolling = (_ragdoller) ? _ragdoller.State == AdvancedRagdollController.RagdollState.Ragdolled : false;

            if (IsCharacterEntering && (isMoving || isRagdolling)) CancelEnterVehicle();
            if (IsCharacterExiting && isRagdolling) CancelExitVehicle();
        }

        private void StartEnterVehicleState()
        {
            if (IsCharacterEntering || IsCharacterExiting)
                return;

            IsCharacterEntering = true;
            InteractionSystem.BlockInteractions = true;

            OnStartEnterVehicle?.Invoke();
        }

        private void CancelEnterVehicle()
        {
            if (!IsCharacterEntering || IsCharacterExiting)
                return;

            IsCharacterEntering = false;
            CurrentVehicle = null;
            CurrentVehicleCharacterIK = null;
            InteractionSystem.BlockInteractions = false;
            TPSCharacter.enableMove();
            CancelInvoke(nameof(EndEnterVehicleState));

            OnCancelEnterVehicle?.Invoke();
        }

        private void EndEnterVehicleState()
        {
            if (!IsCharacterEntering || IsCharacterExiting)
                return;

            OnCharacterStartDriving();

            IsDriving = true;
            CurrentVehicle.IsOn = true;
            IsCharacterEntering = false;

            if (DisableCharacterOnEnter)
                TPSCharacter.gameObject.SetActive(false);

            TPSCharacter.PhysicalIgnore(CurrentVehicle.gameObject, ignore: true);

            UpdateDrivingState();

            OnEnterVehicle?.Invoke();
        }

        private void StartExitVehicleState()
        {
            if (IsCharacterEntering || IsCharacterExiting)
                return;

            IsDriving = false;
            IsCharacterExiting = true;
            CurrentVehicle.IsOn = false;

            if (DisableCharacterOnEnter)
                TPSCharacter.gameObject.SetActive(true);

            OnStartExitVehicle?.Invoke();
        }

        private void CancelExitVehicle()
        {
            EndEnterVehicleState();

            OnCancelExitVehicle?.Invoke();
        }

        private void EndExitVehicleState()
        {
            if (!IsCharacterExiting || IsCharacterEntering)
                return;

            if (!IsDriving && CurrentVehicle)
                TPSCharacter.PhysicalIgnore(CurrentVehicle.gameObject, ignore: false);

            IsCharacterExiting = false;
            CurrentVehicle = null;
            CurrentVehicleCharacterIK = null;
            InteractionSystem.BlockInteractions = false;

            TPSCharacter.enableMove();

            OnExitVehicle?.Invoke();
        }

        private void OnCharacterStartDriving()
        {
            if (!TPSCharacter || !anim || DisableCharacterOnEnter)
                return;

            //Set Driving Animation
            anim.SetBool(TPSCharacter.AnimatorParameters.Driving, true);

            //Change Some Character States

            TPSCharacter.IsJumping = false;
            TPSCharacter.IsGrounded = false;
            TPSCharacter.VelocityMultiplier = 0;

            TPSCharacter.SwitchToItem(-1);
            TPSCharacter.DisableLocomotion();

            // Disable physics.
            if (rb)
                rb.useGravity = false;

            // Change Character Collider Properties.
            if (coll)
                coll.isTrigger = true;

            // Disable Default Animator Layers.
            for (int i = 1; i < 4; i++)
                anim.SetLayerWeight(i, 0);

            // Disable All Locomotion Animator Parameters.
            anim.SetBool(TPSCharacter.AnimatorParameters.Crouch, false);
            anim.SetBool(TPSCharacter.AnimatorParameters.ItemEquiped, false);
            anim.SetBool(TPSCharacter.AnimatorParameters.FireMode, false);
            anim.SetBool(TPSCharacter.AnimatorParameters.Grounded, true);
            anim.SetBool(TPSCharacter.AnimatorParameters.Running, false);
            anim.SetFloat(TPSCharacter.AnimatorParameters.IdleTurn, 0);
            anim.SetFloat(TPSCharacter.AnimatorParameters.Speed, 0);
        }

        private void OnCharacterStopDriving()
        {
            if (!TPSCharacter || !anim)
                return;

            TPSCharacter.DisableAllMove = false;

            if (DisableCharacterOnEnter)
            {
                TPSCharacter.transform.eulerAngles = new Vector3(0, TPSCharacter.transform.eulerAngles.y, 0);
                return;
            }

            // Set Driving Animation.
            anim.SetBool(TPSCharacter.AnimatorParameters.Driving, false);

            // Enable Character Locomotion.
            TPSCharacter.EnableMove();
            TPSCharacter.transform.eulerAngles = new Vector3(0, TPSCharacter.transform.eulerAngles.y, 0);

            // Change Character Collider Properties.
            if (coll) coll.isTrigger = false;
            if (rb) rb.useGravity = true;

            // Disable Default Animator Layers.
            TPSCharacter.ResetDefaultLayersWeight();
        }

        private void OnInteract(JUInteractable interactable)
        {
            if (!(interactable is JUVehicleInteractable vehicleInteractable))
                return;

            JUVehicle vehicle = vehicleInteractable.Vehicle;
            if (!vehicle)
            {
                Debug.LogWarning("JU Vehicle Interactable doesn't have a vehicle to access");
                return;
            }
            JUVehicleCharacterIK vehicleCharacterIK = vehicle.GetComponent<JUVehicleCharacterIK>();
            DriveVehicle(vehicle, vehicleCharacterIK);
        }
    }
}