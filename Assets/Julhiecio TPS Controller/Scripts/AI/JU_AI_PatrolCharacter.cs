using JU.AI;
using JU.CharacterSystem.AI.EscapeSystem;
using JU.CharacterSystem.AI.HearSystem;
using JUTPS;
using JUTPS.AI;
using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Patrol AI controller.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Patrol AI")]
    public class JU_AI_PatrolCharacter : JUCharacterAIBase, IOnSetTarget, IOnHear
    {
        /// <summary>
        /// Patrol AI states.
        /// </summary>
        public enum PatrolStates
        {
            /// <summary>
            /// Idle or moving to a location.
            /// </summary>
            Patrol,

            /// <summary>
            /// Moving to the possible target position.
            /// </summary>
            MovingToPossibleTargetPosition,

            /// <summary>
            /// Is searching the lost target.
            /// </summary>
            SearchingForLostTarget,

            /// <summary>
            /// Attacking a target.
            /// </summary>
            Attacking
        }

        /// <summary>
        /// General AI settings.
        /// </summary>
        [System.Serializable]
        public struct GeneralSettings
        {
            public float MaxSearchTargetTime;

            /// <summary>
            /// The time to lose target if isn't visible. After this, will search 
            /// the target or go back to the path/walk random.
            /// </summary>
            public float LoseTargetDelay;

            /// <summary>
            /// Will be alert if see a enemy/target.
            /// If alert, the Ai will be more aggressive. 
            /// Instead of move to possible target position upon hearing some suspect sound, it will directly search the target
            /// and attack if find him.
            /// The max time is the duration of the "aggressive" state after see the target and lose it.
            /// </summary>
            public float AlertMaxTime;
        }

        private float _loseTargetTimer;
        private float _searchTargetTimer;

        private Collider _target;
        private JUHealth _targetHealth;
        private Vector3 _possibleTargetPosition;

        private Vector3 _spawnPosition;

        private float _inAlertTimeTargetLosed;

        private JU_AIActionBase _currentAction;

        /// <summary>
        /// The character head, used by the field of view.
        /// </summary>
        public Transform Head;

        /// <summary>
        ///  General AI settings.
        /// </summary>
        [Header("Patrol AI")]
        public GeneralSettings General;

        /// <summary>
        /// AI field of view.
        /// </summary>
        [Header("Sensors")]
        public FieldOfView FieldOfView;

        /// <summary>
        /// AI hear sensor.
        /// </summary>
        public HearSensor HearSensor;

        /// <summary>
        /// The waypoint path used to patrol.
        /// </summary>
        [Header("Patrol Areas")]
        public WaypointPath PatrolPath;

        /// <summary>
        /// The area to patrol if not have <see cref="PatrolPath"/>.
        /// </summary>
        public JUBoxArea PatrolArea;

        /// <summary>
        /// Will move randomly if not have <see cref="PatrolPath"/> or <see cref="PatrolArea"/> assigned.
        /// </summary>
        [Header("States")]
        public bool PatrolRandomlyIfNotHavePath;

        /// <summary>
        /// Move randomly around the spawn position.
        /// </summary>
        public MoveRandomAroundPoint MoveRandom;

        /// <summary>
        /// Move through <see cref="PatrolPath"/>.
        /// </summary>
        public FollowWaypoint FollowPatrolPath;

        /// <summary>
        /// Move randomly inside <see cref="PatrolArea"/>.
        /// </summary>
        public MoveRandomInsideArea MoveRandomPatrolArea;

        /// <summary>
        /// Move to possible target position on hear sounds.
        /// </summary>
        public FollowPoint MoveToPossibleTargetPosition;

        /// <summary>
        /// Search the losed target.
        /// </summary>
        public MoveRandomAroundPoint SearchLosedTarget;

        /// <summary>
        /// Damage detector system, used to AI look to the object that caused damage.
        /// </summary>
        public DamageDetector DamageDetector;

        /// <summary>
        /// The attack state if have a target.
        /// </summary>
        public Attack Attack;

        /// <summary>
        /// Used to escape from escape areas, like granades or explosions.
        /// </summary>
        public Escape EscapeAreas;

        /// <summary>
        /// Called on detect a target.
        /// </summary>
        public event UnityAction<GameObject> OnSetTarget;

        /// <summary>
        /// Called on hear something and <see cref="CurrentState"/> is not <see cref="PatrolStates.Attacking"/>.
        /// </summary>
        public event UnityAction<Vector3, GameObject> OnHear;

        /// <summary>
        /// Patrol AI current state;
        /// </summary>
        public PatrolStates CurrentState { get; private set; }

        /// <summary>
        /// If alert, the AI will be more aggressive. Actived after see a target/enemy and lose it.
        /// Adjustable by <see cref="GeneralSettings.AlertMaxTime"/> in <see cref="General"/>.
        /// </summary>
        public bool IsAlert { get; private set; }

        /// <summary>
        /// Current target to attack.
        /// </summary>
        public Collider CurrentTarget
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    _target = value;
                    _targetHealth = null;

                    OnSetTarget?.Invoke(value ? value.gameObject : null);
                }
            }
        }

        /// <summary>
        /// Current target health.
        /// </summary>
        public JUHealth CurrentTargetHealth
        {
            get
            {
                if (!_targetHealth && _target)
                {
                    _targetHealth = CurrentTarget.GetComponent<JUHealth>();

                    if (!_targetHealth && _target.transform.parent)
                        _targetHealth = _target.transform.parent.GetComponent<JUHealth>();
                }

                return _targetHealth;
            }
        }

        /// <inheritdoc/>
        public JU_AI_PatrolCharacter() : base()
        {
            General = new GeneralSettings()
            {
                LoseTargetDelay = 10,
                AlertMaxTime = 20,
                MaxSearchTargetTime = 30
            };

            PatrolRandomlyIfNotHavePath = true;

            FieldOfView = new FieldOfView();
            HearSensor = new HearSensor();

            DamageDetector = new DamageDetector();
            MoveRandom = new MoveRandomAroundPoint();
            FollowPatrolPath = new FollowWaypoint();
            MoveRandomPatrolArea = new MoveRandomInsideArea();
            SearchLosedTarget = new MoveRandomAroundPoint();
            MoveToPossibleTargetPosition = new FollowPoint();
            Attack = new Attack();
            EscapeAreas = new Escape();

            MoveRandom.StartRunDistance = 20;
            MoveRandom.ChangeDestinationInterval = 10;

            SearchLosedTarget.UseFirePose = true;
            MoveToPossibleTargetPosition.UseFirePose = true;
            DamageDetector.ForceFirePose = true;
        }

        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();

            EscapeAreas.OnValidate();
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            base.Reset();

            // Try find the head.
            if (gameObject.TryGetComponent<Animator>(out var anim))
            {
                if (anim.isHuman)
                {
                    Head = anim.GetBoneTransform(HumanBodyBones.Head);
                }
            }

            Attack.Reset();
            FieldOfView.Reset();
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            _spawnPosition = transform.position;

            if (JU_Ai.ClosestToNavMesh(_spawnPosition, out var spawnPointOnNavmesh))
            {
                _spawnPosition = spawnPointOnNavmesh;
            }

            base.Start();

            FieldOfView.Setup(this);
            HearSensor.Setup(this);

            DamageDetector.Setup(this);
            MoveRandom.Setup(this);
            FollowPatrolPath.Setup(this);
            MoveRandomPatrolArea.Setup(this);
            MoveToPossibleTargetPosition.Setup(this);
            SearchLosedTarget.Setup(this);
            Attack.Setup(this);
            EscapeAreas.Setup(this);

            HearSensor.OnHear.AddListener(OnHearSomething);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            DamageDetector.Unsetup();
            MoveRandom.Unsetup();
            FollowPatrolPath.Unsetup();
            MoveRandomPatrolArea.Unsetup();
            MoveToPossibleTargetPosition.Unsetup();
            SearchLosedTarget.Unsetup();
            Attack.Unsetup();
            EscapeAreas.Unsetup();

            HearSensor.OnHear.RemoveListener(OnHearSomething);
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            FieldOfView.Update(Head);

            if (FieldOfView.NearestColliderInView)
                CurrentTarget = FieldOfView.NearestColliderInView;

            if (CurrentTarget && !FieldOfView.IsOnView(CurrentTarget))
            {
                // Lose the target.
                _loseTargetTimer += Time.deltaTime;
                if (_loseTargetTimer > General.LoseTargetDelay)
                {
                    _possibleTargetPosition = CurrentTarget.transform.position;
                    CurrentState = PatrolStates.MovingToPossibleTargetPosition;
                    CurrentTarget = null;
                }
            }
            else
                _loseTargetTimer = 0;

            if (CurrentTarget)
                CurrentState = PatrolStates.Attacking;

            // The current target was destroyed, so let's go back to patrol.
            if ((CurrentState == PatrolStates.Attacking && !CurrentTarget) || (CurrentTargetHealth && CurrentTargetHealth.IsDead))
            {
                CurrentTarget = null;
                CurrentState = PatrolStates.Patrol;
            }

            UpdateAlertMode();

            AIControlData control = UpdateCurrentState();

            Control = control;
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (_currentAction != null)
                _currentAction.DrawGizmos();
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            base.OnDrawGizmosSelected();

            if (_currentAction != null)
                _currentAction.DrawGizmosSelected();

            FieldOfView.DrawGizmos();

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

        private void UpdateAlertMode()
        {
            if (CurrentTarget)
            {
                IsAlert = true;
                _inAlertTimeTargetLosed = 0;
                return;
            }

            if (IsAlert)
            {
                _inAlertTimeTargetLosed += Time.deltaTime;
                if (_inAlertTimeTargetLosed > General.AlertMaxTime)
                    IsAlert = false;
            }
        }

        private AIControlData UpdateCurrentState()
        {
            AIControlData control = new AIControlData();

            switch (CurrentState)
            {
                case PatrolStates.Patrol:

                    UpdateFollowPathState(ref control);
                    break;
                case PatrolStates.MovingToPossibleTargetPosition:

                    UpdateMoveToPossibleTargetPositionState(ref control);
                    break;

                case PatrolStates.SearchingForLostTarget:

                    UpdateSearchForLosedTarget(ref control);

                    break;
                case PatrolStates.Attacking:

                    UpdateAttackState(ref control);
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            // Will try avoid escape areas (like granades/explosions) if have.
            EscapeAreas.Update(ref control);
            if (EscapeAreas.IsTryingEscape)
                _currentAction = EscapeAreas;

            // Auto exit search state.
            if (CurrentState == PatrolStates.SearchingForLostTarget)
            {
                _searchTargetTimer += Time.deltaTime;
                if (_searchTargetTimer > General.MaxSearchTargetTime)
                {
                    _searchTargetTimer = 0;
                    CurrentState = PatrolStates.Patrol;
                }
            }
            else
                _searchTargetTimer = 0;

            if (CurrentState != PatrolStates.Attacking)
                DamageDetector.Update(ref control);

            return control;
        }

        private void UpdateFollowPathState(ref AIControlData control)
        {
            if (PatrolPath)
            {
                _currentAction = FollowPatrolPath;
                FollowPatrolPath.Update(PatrolPath, ref control);
            }
            else
            {
                _currentAction = MoveRandomPatrolArea;
                MoveRandomPatrolArea.Update(PatrolArea, ref control);
            }

            if (!PatrolPath && !PatrolArea && PatrolRandomlyIfNotHavePath)
            {
                _currentAction = MoveRandom;
                MoveRandom.Update(_spawnPosition, ref control);
            }
        }

        private void UpdateMoveToPossibleTargetPositionState(ref AIControlData control)
        {
            _currentAction = MoveToPossibleTargetPosition;
            MoveToPossibleTargetPosition.Update(_possibleTargetPosition, ref control);

            if (MoveToPossibleTargetPosition.IsStopedClosestToDestination)
                CurrentState = IsAlert ? PatrolStates.SearchingForLostTarget : PatrolStates.Patrol;
        }

        private void UpdateAttackState(ref AIControlData control)
        {
            if (CurrentTarget)
            {
                _currentAction = Attack;
                Attack.Update(CurrentTarget.gameObject, ref control);
            }
        }

        private void UpdateSearchForLosedTarget(ref AIControlData control)
        {
            _currentAction = SearchLosedTarget;
            SearchLosedTarget.Update(_possibleTargetPosition, ref control);
        }

        private void OnHearSomething(Vector3 position, GameObject source)
        {
            if (CurrentState == PatrolStates.Attacking)
                return;

            _possibleTargetPosition = position;

            if (CurrentState != PatrolStates.SearchingForLostTarget)
                CurrentState = PatrolStates.MovingToPossibleTargetPosition;

            OnHear?.Invoke(position, source);
        }
    }
}
