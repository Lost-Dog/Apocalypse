using UnityEngine;
using JUTPS;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// The base to create AIs for <see cref="JUCharacterController"/>.
    /// </summary>
    public class JUCharacterAIBase : MonoBehaviour
    {
        /// <summary>
        /// Navigation modes
        /// </summary>
        public enum NavigationModes
        {
            /// <summary>
            /// Not uses navmesh system.
            /// </summary>
            Simple,

            /// <summary>
            /// Uses navmesh system.
            /// </summary>
            UseNavmesh
        }

        /// <summary>
        /// Navigation ettings.
        /// </summary>
        [System.Serializable]
        public class AINavigationSettings
        {
            /// <summary>
            /// The mode of the navigation.
            /// </summary>
            public NavigationModes Mode;

            /// <summary>
            /// <para>Used only if <see cref="Mode"/> is <see cref="NavigationModes.UseNavmesh"/>.</para>
            /// The refresh rate of the navigation, used to avoid unecessary updates. 
            /// </summary>
            [Space]
            public float NavigationRefreshRate;
        }

        /// <summary>
        /// The character control data.
        /// </summary>
        [System.Serializable]
        public struct AIControlData
        {
            /// <summary>
            /// The character will run if is true.
            /// </summary>
            public bool IsRunning;

            /// <summary>
            /// The character stay on attack pose (IK mode if with an weapon).
            /// </summary>
            public bool IsAttackPose;

            /// <summary>
            /// If true the character will attack, shot if have a weapon, attack if uses an melee weapon or punch if have no weapon.
            /// </summary>
            public bool IsAttacking;

            /// <summary>
            /// The character move direction.
            /// </summary>
            public Vector3 MoveToDirection;

            /// <summary>
            /// The character look direction, usefull for aim to a target.
            /// </summary>
            public Vector3 LookToDirection;
        }

        private Vector3 _currentLookDirection;

        /// <summary>
        /// Speed to look to a target.
        /// </summary>
        [Min(1f)]
        public float AimSpeed;

        /// <summary>
        /// If true, the AI can move.
        /// </summary>
        public bool MoveEnabled;

        /// <summary>
        /// The character navigation settings.
        /// </summary>
        public AINavigationSettings NavigationSettings;

        /// <summary>
        /// The character body collider.
        /// </summary>
        public Collider BodyCollider { get; private set; }

        /// <summary>
        /// The character control data.
        /// Stores the AI behavior, like move direction, attack and any other necessary
        /// data used to control the character.
        /// </summary>
        protected AIControlData Control { get; set; }

        /// <summary>
        /// The character that will be controlled by this AI.
        /// </summary>
        public JUCharacterController Character { get; private set; }

        /// <summary>
        /// The AI character bounds.
        /// </summary>
        public Vector3 Center
        {
            get => BodyCollider ? BodyCollider.bounds.center : transform.position;
        }

        /// <summary>
        /// Create an AI for <see cref="JUCharacterController"/>.
        /// </summary>
        protected JUCharacterAIBase()
        {
            AimSpeed = 200;
            MoveEnabled = true;

            NavigationSettings = new AINavigationSettings
            {
                Mode = NavigationModes.UseNavmesh,
                NavigationRefreshRate = 0.3f
            };
        }

        /// <summary>
        /// Called by editor to validate properties.
        /// </summary>
        protected virtual void OnValidate()
        {
            FindComponents();
        }


        /// <summary>
        /// Called by editor to reset script properties.
        /// </summary>
        protected virtual void Reset()
        {
        }

        /// <summary>
        /// Called on first object update.
        /// </summary>
        protected virtual void Awake()
        {
            FindComponents();

            Debug.Assert(Character, $"The gameObject {name} hasn't a {typeof(JUCharacterController)} component.");

            _currentLookDirection = Character.transform.forward;
            Character.UseDefaultControllerInput = false;
        }

        /// <summary>
        /// Called on first object update after <see cref="Awake"/>.
        /// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        /// Called on object destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// Called every frame update to update control logic.
        /// </summary>
        protected virtual void Update()
        {
            if (Character.IsDead)
            {
                enabled = false;
                return;
            }

            UpdateCharacterLookAt();
            UpdateCharacterControls();
        }

        /// <summary>
        /// Called on every frame to show debug informations.
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
        }

        /// <summary>
        /// Called on every frame to show debug informations if the GameObject is selected.
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
        }

        private void FindComponents()
        {
            if (!BodyCollider) BodyCollider = GetComponent<Collider>();
            if (!Character) Character = GetComponent<JUCharacterController>();
        }

        private void UpdateCharacterLookAt()
        {
            var lookDirection = Control.LookToDirection;

            if (lookDirection.magnitude < 0.5f)
                lookDirection = Character.transform.forward;

            float angleToDirection = Vector3.Angle(_currentLookDirection.normalized, lookDirection);
            float lookToDirectionSpeed = Mathf.Clamp01(Time.deltaTime * (AimSpeed / Mathf.Max(angleToDirection, 0.01f)));
            _currentLookDirection = Vector3.Lerp(_currentLookDirection, lookDirection, lookToDirectionSpeed);

            Vector3 lookAtPosition = transform.position + (_currentLookDirection * 10);
            Character.LookAtPosition = lookAtPosition;
        }

        private void UpdateCharacterControls()
        {
            bool attackPose = Control.IsAttackPose;
            bool attacking = Control.IsAttacking;
            bool running = Control.IsRunning;
            Vector3 moveDirection = Control.MoveToDirection;

            bool isMoving = moveDirection.magnitude > 0.1f;
            if (isMoving)
            {
                moveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
                moveDirection /= moveDirection.magnitude;
            }

            // Force look to the the direction if is not in fire mode because the normal way to look to the direction
            // works only if is on fire mode.
            if (!Control.IsAttackPose && !isMoving && Control.LookToDirection.magnitude > 0)
                Character.DoLookAt(transform.position + (Control.LookToDirection * 10));

            Character.FiringModeIK = attackPose && Character.RightHandWeapon;
            Character.FiringMode = attackPose && Character.RightHandWeapon;
            Character.DefaultUseOfAllItems(attacking, attacking, attacking, true, false, attacking, attacking && !Character.RightHandWeapon);

            if (!MoveEnabled)
                moveDirection = Vector3.zero;

            Character._Move(moveDirection.x, moveDirection.z, running);
        }
    }
}


