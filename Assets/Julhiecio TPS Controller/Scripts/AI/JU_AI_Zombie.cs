using JU.AI;
using JUTPS.AI;
using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Zombie AI Contorller.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Zombie AI")]
    public class JU_AI_Zombie : JUCharacterAIBase, IOnSetTarget, IOnHear
    {
        /// <summary>
        /// The zombie's states.
        /// </summary>
        public enum ZombieState
        {
            /// <summary>
            /// Moving on a path or on a specific area.
            /// </summary>
            Patrolling,

            /// <summary>
            /// Is attacking an object.
            /// </summary>
            Attacking,

            /// <summary>
            /// The target was lost, move to the last target position.
            /// </summary>
            MoveToLastTargetPosition,

            /// <summary>
            /// Is searching the last target moving randomly closest to the last target position.
            /// </summary>
            SearhLastTarget,
        }

        /// <summary>
        /// Zombie general settings.
        /// </summary>
        [System.Serializable]
        public struct GeneralSettings
        {
            /// <summary>
            /// The maximum time that the zombie can seach for the target.
            /// </summary>
            public float SearhLastTargetTime;
        }

        private Collider _currentTarget;

        private Vector3 _spawnPosition;
        private Vector3 _lastTargetPosition;
        private float _searchLastTargetTimer;

        private JU_AIActionBase _currentAction;

        /// <summary>
        /// The character head, used by the field of view.
        /// </summary>
        public Transform Head;

        /// <summary>
        /// The path to move.
        /// </summary>
        public WaypointPath PatrolPath;

        /// <summary>
        /// The area to patrol if not have <see cref="PatrolPath"/>
        /// </summary>
        public JUBoxArea PatrolArea;

        /// <summary>
        /// Zombie general settings.
        /// </summary>
        [Space]
        public GeneralSettings General;

        /// <summary>
        /// The field of view, used to detect targets.
        /// </summary>
        [Header("Sensors")]
        public FieldOfView FieldOfView;

        /// <summary>
        /// The damage detector, used to look to the damage source when takes a hit.
        /// </summary>
        public DamageDetector DamageDetector;

        /// <summary>
        /// Hear sensor, used to move to the possible target position when listen some sound, like a shot or explosion.
        /// </summary>
        public HearSystem.HearSensor Hear;

        /// <summary>
        /// Will move randomly if not have <see cref="PatrolPath"/> or <see cref="PatrolArea"/> assigned.
        /// </summary>
        [Header("Actions")]
        public bool PatrolRandomlyIfNotHavePath;

        /// <summary>
        /// Move randomly around the spawn position.
        /// </summary>
        public MoveRandomAroundPoint MoveRandom;

        /// <summary>
        /// Control the AI to move throught a path when not have a target to attack.
        /// </summary>
        public FollowWaypoint FollowPatrolPath;

        /// <summary>
        /// Control the AI to move randomly inside an specific area when not have a target to attack, check <see cref="PatrolArea"/>.
        /// </summary>
        public MoveRandomInsideArea PatrolInsideArea;

        /// <summary>
        /// Control the AI to move to the possible target position on lost the last target that was attacking or when hear some sound
        /// using the <see cref="Hear"/> sensor.
        /// </summary>
        public FollowPoint MoveToLastTargetPosition;

        /// <summary>
        /// Control the AI to search the losed target moving randomly closest to the last losed target position.
        /// </summary>
        public MoveRandomAroundPoint SearchLastTarget;

        /// <summary>
        /// Control the AI to attack the <see cref="CurrentTarget"/>.
        /// </summary>
        public Attack Attack;

        /// <summary>
        /// Called when have a target to attack.
        /// </summary>
        public event UnityAction<GameObject> OnSetTarget;

        /// <summary>
        /// Called on hear sound using <see cref="Hear"/> sensor.
        /// </summary>
        public event UnityAction<Vector3, GameObject> OnHear;

        /// <summary>
        /// The current target that the AI is attacking.
        /// </summary>
        public Collider CurrentTarget
        {
            get => _currentTarget;
            private set
            {
                if (_currentTarget == value)
                    return;

                _currentTarget = value;
                OnSetTarget?.Invoke(value ? value.gameObject : null);
            }
        }

        /// <summary>
        /// The current zombie state.
        /// </summary>
        public ZombieState CurrentState { get; private set; }

        /// <inheritdoc/>
        public JU_AI_Zombie()
        {
            General = new GeneralSettings
            {
                SearhLastTargetTime = 15
            };

            PatrolRandomlyIfNotHavePath = true;

            MoveRandom = new MoveRandomAroundPoint();
            FollowPatrolPath = new FollowWaypoint();
            PatrolInsideArea = new MoveRandomInsideArea();
            MoveToLastTargetPosition = new FollowPoint();
            SearchLastTarget = new MoveRandomAroundPoint();
            Attack = new Attack();
            FieldOfView = new FieldOfView();

            FieldOfView.Distance = 10;

            MoveRandom.StartRunDistance = 20;
            MoveRandom.ChangeDestinationInterval = 10;

            FollowPatrolPath.Run = false;
            FollowPatrolPath.UseFirePose = false;
            FollowPatrolPath.StopDistance = 2f;
            FollowPatrolPath.EndPathMode = JU_AIActionBase.EndPathModes.InvertPath;

            PatrolInsideArea.Run = false;
            PatrolInsideArea.UseFirePose = false;
            PatrolInsideArea.StopDistance = 2f;
            PatrolInsideArea.ChangeDestinationTime = 10f;

            MoveToLastTargetPosition.StartRunDistance = 2f;
            MoveToLastTargetPosition.StopDistance = 2f;
            MoveToLastTargetPosition.UseFirePose = false;

            SearchLastTarget.StopDistance = 2f;
            SearchLastTarget.StartRunDistance = 20f;
            SearchLastTarget.Range = 10f;
            SearchLastTarget.UseFirePose = false;

            Attack.MeleeAttack.StartRunDistance = 0.5f;
            Attack.MeleeAttack.AttackDistance = 0.9f;
        }

        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!Application.isPlaying)
            {
                // Setup on editor to DebugDraw gizmos works.
                FieldOfView.Setup(this);
            }
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            base.Reset();

            Attack.Reset();
            FieldOfView.Reset();

            // Try find the head.
            if (gameObject.TryGetComponent<Animator>(out var anim))
            {
                if (anim.isHuman)
                {
                    Head = anim.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            _spawnPosition = transform.position;

            if (JU_Ai.ClosestToNavMesh(_spawnPosition, out var spawnPointOnNavmesh))
            {
                _spawnPosition = spawnPointOnNavmesh;
            }

            MoveRandom.Setup(this);
            FollowPatrolPath.Setup(this);
            PatrolInsideArea.Setup(this);
            MoveToLastTargetPosition.Setup(this);
            SearchLastTarget.Setup(this);
            Attack.Setup(this);

            FieldOfView.Setup(this);
            DamageDetector.Setup(this);

            Hear.Setup(this);
            Hear.OnHear.AddListener(OnZombieHear);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            MoveRandom.Unsetup();
            FollowPatrolPath.Unsetup();
            PatrolInsideArea.Unsetup();
            MoveToLastTargetPosition.Unsetup();
            SearchLastTarget.Unsetup();
            Attack.Unsetup();

            DamageDetector.Unsetup();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            UpdateCurrentState();
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            base.OnDrawGizmosSelected();

            FieldOfView.DrawGizmos();

            if (_currentAction != null)
                _currentAction.DrawGizmosSelected();

            if (!Application.isPlaying)
                return;

            // Editor viewport camera.
            Camera viewportCamera = UnityEditor.SceneView.currentDrawingSceneView?.camera;

            // Game camera.
            if (!viewportCamera)
                viewportCamera = Camera.main;

            if (viewportCamera)
            {
                Vector3 debugTextPosition = BodyCollider.bounds.center + (Vector3.up * BodyCollider.bounds.size.y);
                float cameraDistance = Vector3.Distance(viewportCamera.transform.position, debugTextPosition);
                float debugTextLineSpace = 0.02f * cameraDistance;
                float line = 0f;

                if (_currentAction != null)
                {
                    line += debugTextLineSpace;
                    string actionName = _currentAction.GetType().Name;
                    UnityEditor.Handles.Label(debugTextPosition + (Vector3.up * line), $"CURRENT ACTION: {actionName}");
                }

                line += debugTextLineSpace;
                UnityEditor.Handles.Label(debugTextPosition + (Vector3.up * line), $"STATE: {CurrentState}");
            }
#endif
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (_currentAction != null)
                _currentAction.DrawGizmos();
        }

        private void UpdateCurrentState()
        {
            AIControlData control = new AIControlData();

            FieldOfView.Update(Head);
            CurrentTarget = FieldOfView.NearestColliderInView;

            if (CurrentTarget)
            {
                _searchLastTargetTimer = 0;
                _lastTargetPosition = CurrentTarget.bounds.center;
                CurrentState = ZombieState.Attacking;
            }

            switch (CurrentState)
            {
                case ZombieState.Patrolling:
                    UpdatePatrolState(ref control);
                    break;
                case ZombieState.Attacking:
                    UpdateAttackState(ref control);
                    break;
                case ZombieState.MoveToLastTargetPosition:
                    UpdateMoveToLastTargetPositionState(ref control);
                    break;
                case ZombieState.SearhLastTarget:
                    UpdateSearchLastTargetState(ref control);
                    break;
                default:
                    throw new UnityException("Invalid option.");
            }

            // Used to detect when takes a hit and look to the hit direction.
            if (CurrentState != ZombieState.Attacking)
                DamageDetector.Update(ref control);

            Control = control;
        }

        private void UpdatePatrolState(ref AIControlData control)
        {
            if (!PatrolPath && !PatrolArea && PatrolRandomlyIfNotHavePath)
            {
                _currentAction = MoveRandom;
                MoveRandom.Update(_spawnPosition, ref control);
                return;
            }

            if (PatrolPath)
            {
                _currentAction = FollowPatrolPath;
                FollowPatrolPath.Update(PatrolPath, ref control);
            }
            else
            {
                _currentAction = PatrolInsideArea;
                PatrolInsideArea.Update(PatrolArea, ref control);
            }
        }

        private void UpdateAttackState(ref AIControlData control)
        {
            if (CurrentState == ZombieState.Attacking && !CurrentTarget)
            {
                CurrentState = ZombieState.MoveToLastTargetPosition;
                return;
            }

            _currentAction = Attack;
            Attack.Update(CurrentTarget.gameObject, ref control);
        }

        private void UpdateMoveToLastTargetPositionState(ref AIControlData control)
        {
            _currentAction = MoveToLastTargetPosition;
            MoveToLastTargetPosition.Update(_lastTargetPosition, ref control);

            if (MoveToLastTargetPosition.IsStopedClosestToDestination)
                CurrentState = ZombieState.SearhLastTarget;
        }

        private void UpdateSearchLastTargetState(ref AIControlData control)
        {
            _currentAction = SearchLastTarget;
            _searchLastTargetTimer += Time.deltaTime;
            if (_searchLastTargetTimer > General.SearhLastTargetTime)
            {
                CurrentState = ZombieState.Patrolling;
                return;
            }

            SearchLastTarget.Update(_lastTargetPosition, ref control);
        }

        private void OnZombieHear(Vector3 position, GameObject source)
        {
            if (CurrentState == ZombieState.Attacking)
                return;

            _lastTargetPosition = position;
            CurrentState = ZombieState.MoveToLastTargetPosition;

            OnHear?.Invoke(position, source);
        }
    }
}
