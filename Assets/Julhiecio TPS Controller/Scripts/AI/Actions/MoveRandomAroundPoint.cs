using JU.AI;
using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Moves the AI randomly around a point.
    /// </summary>
    [System.Serializable]
    public class MoveRandomAroundPoint : JU_AIActionBase
    {
        /// <summary>
        /// Aim modes.
        /// </summary>
        public enum ModesOfLookAt
        {
            /// <summary>
            /// Look to the target point.
            /// </summary>
            ToTarget,

            /// <summary>
            /// Look to the move direction.
            /// </summary>
            ToMoveDirection,

            /// <summary>
            /// Look to a random direction.
            /// </summary>
            Random
        }

        private bool _requestNewRandomDestination;

        private float _changeDestinationTimer;
        private float _changeLookDirectionTimer;

        private Vector3 _center;
        private Vector3 _currentLookDirection;

        /// <summary>
        /// Max distance that can move around the center point.
        /// </summary>
        public float Range;

        /// <summary>
        /// The time to chance the random destination around the center point.
        /// </summary>
        public float ChangeDestinationInterval;

        /// <summary>
        /// Runs if the AI is distant from the destination.
        /// </summary>
        [Header("Moviment")]
        public float StartRunDistance;

        /// <summary>
        /// Stop distance from destination.
        /// </summary>
        public float StopDistance;

        /// <summary>
        /// Use fire pose.
        /// </summary>
        public bool UseFirePose;

        /// <summary>
        /// The AI aim mode while is moving around the center.
        /// </summary>
        [Header("Looking")]
        public ModesOfLookAt LookAtMode;

        /// <summary>
        /// The interval to change the look at direction randomly.
        /// </summary>
        public float ChangeLookDirectionInterval;

        /// <summary>
        /// The max random angle to look at direction based on <see cref="LookAtMode"/>
        /// </summary>
        public float MaxLookAtAngle;

        /// <summary>
        /// Called when a new destination is generated.
        /// </summary>
        public UnityAction<Vector3> OnSetRandomDestination;

        /// <summary>
        /// Return the current destination that the AI must go.
        /// </summary>
        public Vector3 CurrentRandomDestination { get; private set; }

        /// <inheritdoc/>
        public MoveRandomAroundPoint()
        {
            Range = 15;
            ChangeDestinationInterval = 5;
            StartRunDistance = 5;
            StopDistance = 2;
            UseFirePose = false;
            LookAtMode = ModesOfLookAt.ToMoveDirection;
            ChangeLookDirectionInterval = 2;
            MaxLookAtAngle = 40;
        }

        /// <inheritdoc/>
        public override void Setup(JUCharacterAIBase ai)
        {
            base.Setup(ai);

            _changeDestinationTimer = ChangeDestinationInterval;
            CurrentRandomDestination = ai.Center;
            _currentLookDirection = ai.transform.forward;
        }

        /// <inheritdoc/>
        public void Update(Vector3 center, ref JUCharacterAIBase.AIControlData control)
        {
            _center = center;
            _changeDestinationTimer += Time.deltaTime;

            bool forceUpdatePath = false;

            // Generate a random destination to move.
            if (_changeDestinationTimer >= ChangeDestinationInterval || _requestNewRandomDestination)
            {
                Vector3 randomPosition = center + (new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)) * Range / 2);

                switch (NavigationSettings.Mode)
                {
                    case JUCharacterAIBase.NavigationModes.Simple:

                        CurrentRandomDestination = randomPosition;
                        _requestNewRandomDestination = false;
                        _changeDestinationTimer = 0;
                        OnSetRandomDestination?.Invoke(CurrentRandomDestination);
                        break;
                    case JUCharacterAIBase.NavigationModes.UseNavmesh:

                        // Update the random destination only if is inside navmesh.
                        if (JU_Ai.ClosestToNavMesh(randomPosition, out randomPosition))
                        {
                            _requestNewRandomDestination = false;
                            _changeDestinationTimer = 0;
                            CurrentRandomDestination = randomPosition;
                            forceUpdatePath = true;
                            OnSetRandomDestination?.Invoke(CurrentRandomDestination);
                        }
                        break;
                    default:
                        throw new System.InvalidOperationException();
                }
            }

            UpdatePathToDestination(CurrentRandomDestination, forceUpdatePath);

            Vector3 aiPosition = Ai.Center;
            Vector3 moveDirection = Vector3.zero;

            // Getting the move direction.
            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:
                    moveDirection = CurrentRandomDestination - aiPosition;
                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (NavmeshPath.Count > 0)
                        moveDirection = NavmeshPath[CurrentNavmeshWaypoint] - aiPosition;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            _changeLookDirectionTimer += Time.deltaTime;

            // Getting the look at direction.
            if (_changeLookDirectionTimer > ChangeLookDirectionInterval)
            {
                _changeLookDirectionTimer = 0;
                Vector3 forward;

                switch (LookAtMode)
                {
                    case ModesOfLookAt.ToTarget:
                        forward = _center - aiPosition;
                        break;
                    case ModesOfLookAt.ToMoveDirection:
                        forward = moveDirection.magnitude > 0.1f ? moveDirection : Ai.transform.forward;
                        break;

                    case ModesOfLookAt.Random:
                        forward = Quaternion.Euler(0, Random.Range(-180, 180), 0) * Vector3.forward;
                        break;
                    default:
                        throw new System.InvalidOperationException();
                }

                forward.Normalize();

                // Add a random direction.
                forward = Quaternion.LookRotation(forward) * Quaternion.Euler(0, Random.Range(-MaxLookAtAngle, MaxLookAtAngle), 0) * Vector3.forward;
                _currentLookDirection = forward;
            }

            float distanceToTarget = Vector3.Distance(CurrentRandomDestination, aiPosition);

            if (distanceToTarget < StopDistance)
                moveDirection = Vector3.zero;

            control.IsAttackPose = UseFirePose;
            control.IsAttacking = false;
            control.IsRunning = distanceToTarget > StartRunDistance;
            control.MoveToDirection = moveDirection;
            control.LookToDirection = _currentLookDirection;
        }

        /// <summary>
        /// Will request a new random destination to move. Can't be done on the current frame because
        /// the verifications isn't on the same frame. Use <see cref="OnSetRandomDestination"/> to get the
        /// random destination when completed.
        /// </summary>
        public void RequestNewRandomDestination()
        {
            _requestNewRandomDestination = true;
        }

        /// <inheritdoc/>
        internal override void DrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            base.DrawGizmos();
            Gizmos.color = Color.yellow * 0.5f;
            Gizmos.DrawWireSphere(_center, Range);
        }
    }
}