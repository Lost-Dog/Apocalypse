using UnityEngine;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Control a <see cref="JUCharacterAIBase"/> to move to a specific position.
    /// </summary>
    [System.Serializable]
    public class FollowPoint : JU_AIActionBase
    {
        /// <summary>
        /// The distance to stop when near of the target.
        /// </summary>
        public float StopDistance;

        /// <summary>
        /// The target distance to start run.
        /// </summary>
        public float StartRunDistance;

        /// <summary>
        /// Use fire pose.
        /// </summary>
        public bool UseFirePose;

        /// <summary>
        /// Return true if the AI is stoped and closest to the current destination. Check <see cref="StopDistance"/>.
        /// </summary>
        public bool IsStopedClosestToDestination { get; private set; }

        /// <inheritdoc/>
        public FollowPoint() : base()
        {
            StopDistance = 2f;
            StartRunDistance = 10f;
        }

        /// <summary>
        /// Control the AI to move to a destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="controlData">The AI control data.</param>
        /// <exception cref="UnityException"></exception>
        public void Update(Vector3 destination, ref JUCharacterAIBase.AIControlData controlData)
        {
            IsStopedClosestToDestination = true;
            UpdatePathToDestination(destination, false);

            controlData.IsAttackPose = UseFirePose;
            controlData.IsAttacking = false;

            Vector3 moveDirection;
            Vector3 lookDirection = Ai.transform.forward;
            Vector3 selfPosision = Ai.transform.position;

            switch (NavigationSettings.Mode)
            {
                // Move to destination.
                case JUCharacterAIBase.NavigationModes.Simple:
                    moveDirection = GetMoveDirection(DestinationOnNavmesh);
                    break;

                // Move to destination using navmesh path.
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (NavmeshPath.Count > 0)
                        moveDirection = (NavmeshPath[CurrentNavmeshWaypoint] - selfPosision).normalized;

                    else
                        moveDirection = Vector3.zero;
                    break;
                default:
                    throw new UnityException("Invalid navigation mode.");
            }

            float distanceToFinalPoint = Vector3.Distance(Ai.transform.position, DestinationOnNavmesh);

            IsStopedClosestToDestination = distanceToFinalPoint < StopDistance;
            if (IsStopedClosestToDestination)
                moveDirection = Vector3.zero;

            if (moveDirection != Vector3.zero)
                lookDirection = moveDirection;

            bool running = distanceToFinalPoint > StartRunDistance;

            controlData.IsRunning = running;
            controlData.MoveToDirection = moveDirection;
            controlData.LookToDirection = lookDirection;
        }

        /// <summary>
        /// Force recalculate the path to a destination.
        /// </summary>
        /// <param name="destination"></param>
        public void ForceRecalculatePath(Vector3 destination)
        {
            UpdatePathToDestination(destination, true);
        }

        private Vector3 GetMoveDirection(Vector3 destination)
        {
            Vector3 direction = destination - Ai.transform.position;
            Vector3 moveDirection = Vector3.ProjectOnPlane(direction.normalized, Vector3.up);
            return moveDirection.normalized;
        }
    }
}