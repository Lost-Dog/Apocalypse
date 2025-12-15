using JUTPS.AI;
using UnityEngine;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Follow a waypoint.
    /// </summary>
    [System.Serializable]
    public class FollowWaypoint : JU_AIActionBase
    {
        private WaypointPath _path;

        /// <summary>
        /// If true, the Ai character will run.
        /// </summary>
        public bool Run;

        /// <summary>
        /// Use fire pose.
        /// </summary>
        public bool UseFirePose;

        /// <summary>
        /// What should the AI must do when it finishes the path.
        /// </summary>
        public EndPathModes EndPathMode;

        /// <summary>
        /// If true, the character will move along the waypoint in the reverse direction.
        /// </summary>
        public bool ReversePath;

        /// <summary>
        /// The distance to stop when near of the target.
        /// </summary>
        public float StopDistance;

        /// <summary>
        /// The current waypoint on the path.
        /// </summary>
        public int CurrentWaypointIndex { get; private set; }

        /// <inheritdoc/>
        public FollowWaypoint() : base()
        {
            Run = false;
            EndPathMode = EndPathModes.InvertPath;
            ReversePath = false;
            StopDistance = 3;
        }

        /// <summary>
        /// Process the Ai to move along a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="controlData">The current AI control data.</param>
        public void Update(WaypointPath path, ref JUCharacterAIBase.AIControlData controlData)
        {
            if (_path != path)
            {
                _path = path;
                if (_path)
                    CurrentWaypointIndex = GetNearestWaypointIndex(Ai.transform.position, _path.WaypointPathPositions);
            }

            if (!_path)
            {
                controlData.LookToDirection = Ai.transform.forward;
                controlData.MoveToDirection = Vector3.zero;
                controlData.IsAttackPose = UseFirePose;
                controlData.IsAttacking = false;
                controlData.IsRunning = false;
                return;
            }

            Vector3 aiPosition = Ai.transform.position;
            int currentWaypointIndex = CurrentWaypointIndex;
            bool waypointChanged = UpdateCurrentWaypoint(aiPosition, ref currentWaypointIndex, _path.WaypointPathPositions, 1f, EndPathMode, ref ReversePath);
            CurrentWaypointIndex = currentWaypointIndex;

            Vector3 currentWaypoint = _path.WaypointPathPositions[CurrentWaypointIndex];
            UpdatePathToDestination(currentWaypoint, forceUpdate: waypointChanged);

            Vector3 moveDirection = Vector3.zero;

            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:
                    moveDirection = currentWaypoint - aiPosition;
                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (NavmeshPath.Count > 0)
                        moveDirection = (NavmeshPath[CurrentNavmeshWaypoint] - aiPosition).normalized;
                    break;
                default:
                    throw new UnityException("Invalid navigation mode.");
            }

            Vector3 lookDirection = moveDirection;

            if (lookDirection.magnitude < 1f)
                lookDirection = Ai.transform.forward;

            int lastWaypointIndex = ReversePath ? 0 : _path.WaypointPathPositions.Length - 1;
            if (currentWaypointIndex == lastWaypointIndex)
            {
                Vector3 lastWaypoint = ReversePath ? _path.WaypointPathPositions[0] : _path.WaypointPathPositions[lastWaypointIndex];
                float lastWaypointDistance = Vector3.Distance(aiPosition, lastWaypoint);
                bool isPathFinished = lastWaypointDistance < StopDistance;

                if (isPathFinished && (EndPathMode == EndPathModes.Stop || path.WaypointPathPositions.Length < 2))
                    moveDirection = Vector3.zero;
            }

            controlData.IsRunning = Run;
            controlData.IsAttackPose = UseFirePose;
            controlData.MoveToDirection = moveDirection;
            controlData.LookToDirection = lookDirection;
        }

        private int GetNearestWaypointIndex(Vector3 center, Vector3[] path)
        {
            float minDistance = Mathf.Infinity;
            int nearestWaypoint = 0;

            for (int i = 0; i < path.Length; i++)
            {
                float distance = Vector3.Distance(center, path[i]);
                if (distance < minDistance)
                {
                    nearestWaypoint = i;
                    minDistance = distance;
                }
            }

            return nearestWaypoint;
        }
    }
}
