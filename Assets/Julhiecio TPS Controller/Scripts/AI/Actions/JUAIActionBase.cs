using JU.AI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Utilities;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// The base to control AI characters.
    /// </summary>
    public class JU_AIActionBase
    {
        /// <summary>
        /// End waypoint path modes.
        /// </summary>
        public enum EndPathModes
        {
            /// <summary>
            /// Stop on finish the path.
            /// </summary>
            Stop,

            /// <summary>
            /// Restart from beginning if hits the end of path.
            /// </summary>
            Restart,

            /// <summary>
            /// Invert the path and continue moving.
            /// </summary>
            InvertPath
        }

        private NavMeshPath _navmeshPathData;

        // Used to update navmesh path if necessary.
        private bool _mustUpdateNavmesh;
        private bool _mustUpdateDestinationOnNavmesh;
        private float _updateNavmeshTimer;
        private float _updateDestinationOnNavmeshTimer;
        private Vector3 _currentDirectionToWaypoint;

        private const float CHARACTER_JUMP_WAYPOINT_NAVMESH_PATH = 1f;

        /// <summary>
        /// The AI Controller.
        /// </summary>
        public JUCharacterAIBase Ai { get; private set; }

        /// <summary>
        /// The AI move destination inside the Navmesh (if <see cref="NavigationSettings"/> uses Navmesh).
        /// </summary>
        protected Vector3 DestinationOnNavmesh { get; private set; }

        /// <summary>
        /// The AI move destination without navmesh.
        /// </summary>
        protected Vector3 RawDestination { get; private set; }

        /// <summary>
        /// The AI controller navigation settings.
        /// </summary>
        public JUCharacterAIBase.AINavigationSettings NavigationSettings
        {
            get => Ai.NavigationSettings;
        }

        /// <summary>
        /// Current waypoint index if is moving along a navmesh path.
        /// </summary>
        public int CurrentNavmeshWaypoint { get; private set; }

        /// <summary>
        /// Return the AI path if is using Navmesh to calculate the path.
        /// </summary>
        public ReadOnlyArray<Vector3> NavmeshPath
        {
            get => _navmeshPathData.corners;
        }

        /// <summary>
        /// Create the action.
        /// </summary>
        protected JU_AIActionBase()
        {
        }

        /// <summary>
        /// Called by editor to validate the properties.
        /// </summary>
        public virtual void OnValidate()
        {
        }

        /// <summary>
        /// Called by editor to reset script properties.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Initial AI action setup.
        /// </summary>
        /// <param name="ai">The AI character controller.</param>
        /// <param name="navigationSettings">AI navigation settings.</param>
        public virtual void Setup(JUCharacterAIBase ai)
        {
            Ai = ai;
            _navmeshPathData = new NavMeshPath();

            Debug.Assert(Ai, $"{typeof(JUCharacterAIBase)} missing.");
            Debug.Assert(Ai.Character, $"{typeof(JUCharacterAIBase)} missing.");

            _updateNavmeshTimer = Ai.NavigationSettings.NavigationRefreshRate;
            _updateDestinationOnNavmeshTimer = Ai.NavigationSettings.NavigationRefreshRate;
        }

        /// <summary>
        /// Unsetup the action. Call it on destroy the AI.
        /// </summary>
        public virtual void Unsetup()
        {

        }

        /// <summary>
        /// Call it on ai controller Update every frame to generate the path to a destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="forceUpdate"><para>If true, the path to the destination will be recalculed instantely.</para>
        /// If false, the path will be updated only one time by X seconds or when necessary. Helpful for optimization.</param>
        protected void UpdatePathToDestination(Vector3 destination, bool forceUpdate)
        {
            if (Ai.Character.IsRagdolled)
                return;

            // the destination is the AI position, ignore the recalculation.
            if ((Ai.transform.position - destination).magnitude < 1f)
            {
                DestinationOnNavmesh = JU_Ai.ClosestToNavMesh(destination);
                RawDestination = destination;
                return;
            }

            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:
                    RawDestination = destination;
                    DestinationOnNavmesh = destination;

                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (forceUpdate)
                    {
                        UpdateDestinationOnNavmesh(destination);
                        UpdatePathToDestination();

                        // Avoid unecessary updates after this update.
                        _mustUpdateDestinationOnNavmesh = false;
                        _mustUpdateNavmesh = false;
                        _updateNavmeshTimer = 0;
                        _updateDestinationOnNavmeshTimer = 0;
                    }
                    else
                    {
                        UpdateDestinationOnNavmeshIfNecessary(destination);
                        UpdatePathToDestinationIfNecessary(DestinationOnNavmesh);
                    }

                    Vector3 aiPosision = Ai.transform.position;
                    int currentWaypoint = CurrentNavmeshWaypoint;

                    // Update AI current waypoint of navmesh path.
                    UpdateCurrentWaypoint(aiPosision, ref currentWaypoint, _navmeshPathData.corners, CHARACTER_JUMP_WAYPOINT_NAVMESH_PATH);
                    CurrentNavmeshWaypoint = currentWaypoint;

                    break;
                default:
                    throw new UnityException("Invalid navigation mode.");
            }
        }

        /// <summary>
        /// Draw AI Gizmos.
        /// </summary>
        internal virtual void DrawGizmos()
        {
#if UNITY_EDITOR

            if (!Application.isPlaying)
                return;

            Vector3 aiPosition = Ai.transform.position;
            Color pathColor = Color.green;

            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:

                    Gizmos.color = pathColor;

                    // Draw path.
                    Gizmos.DrawLine(aiPosition, DestinationOnNavmesh);

                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    Gizmos.color = pathColor;

                    // Draw path.
                    for (int i = 1; i < _navmeshPathData.corners.Length; i++)
                        Gizmos.DrawLine(_navmeshPathData.corners[i - 1], _navmeshPathData.corners[i]);

                    // Draw current waypoint.
                    if (_navmeshPathData.corners.Length > 0)
                        Debug.DrawRay(_navmeshPathData.corners[CurrentNavmeshWaypoint], Vector3.up * 2, Color.green);
                    break;
                default:
                    break;
            }
#endif
        }

        /// <summary>
        /// Draw AI Gizmos if selected.
        /// </summary>
        internal virtual void DrawGizmosSelected()
        {
        }

        private void UpdateDestinationOnNavmeshIfNecessary(Vector3 destination)
        {
            if (NavigationSettings.Mode == JUCharacterAIBase.NavigationModes.Simple)
            {
                DestinationOnNavmesh = destination;
                return;
            }

            if (destination != RawDestination)
            {
                RawDestination = destination;
                _mustUpdateDestinationOnNavmesh = true;
            }

            if (_mustUpdateDestinationOnNavmesh)
            {
                // Update the destination on the navmesh area limited times.
                // It's not necessary recalculate the destination on every frame.
                _updateDestinationOnNavmeshTimer += Time.deltaTime;
                if (_updateDestinationOnNavmeshTimer >= NavigationSettings.NavigationRefreshRate)
                {
                    _updateDestinationOnNavmeshTimer = 0;
                    _mustUpdateDestinationOnNavmesh = false;

                    UpdateDestinationOnNavmesh(destination);
                }
            }
        }

        private void UpdateDestinationOnNavmesh(Vector3 destination)
        {
            DestinationOnNavmesh = JU_Ai.ClosestToNavMesh(destination);
        }

        private void UpdatePathToDestinationIfNecessary(Vector3 destination)
        {
            if (_mustUpdateNavmesh)
            {
                // It's not necessary recalculate the path instantly every frame.
                _updateNavmeshTimer += Time.deltaTime;
                if (_updateNavmeshTimer >= NavigationSettings.NavigationRefreshRate)
                {
                    _updateNavmeshTimer = 0;
                    _mustUpdateNavmesh = false;
                    UpdatePathToDestination();
                }

                return;
            }

            int navmeshPathLength = _navmeshPathData.corners.Length;
            Vector3 aiPosition = Ai.transform.position;
            float destinationDistance = Vector3.Distance(aiPosition, destination);

            // Update if hasn't path.
            if (navmeshPathLength == 0)
            {
                if (destinationDistance < 1f)
                    return;

                UpdatePathToDestination();
                navmeshPathLength = _navmeshPathData.corners.Length;
                return;
            }

            // Update if the destination was changed.
            if (_navmeshPathData.corners[navmeshPathLength - 1] != destination)
            {
                _mustUpdateNavmesh = true;
                return;
            }

            Vector3 currentWaypoint = _navmeshPathData.corners[CurrentNavmeshWaypoint];
            Vector3 directionToWaypoint = (currentWaypoint - aiPosition).normalized;

            // Update path if the AI turning, (the character direction changes, triggering the update).
            // It's not necessary update the path if the character is moving along a straight line.
            // Dot angle return -1 if the target direction is on back and 1 if the target direction is the
            // AI forward direction.
            float angleToWaypoint = Vector3.Dot(directionToWaypoint, _currentDirectionToWaypoint);
            if (angleToWaypoint < 0.9f)
            {
                _currentDirectionToWaypoint = directionToWaypoint;
                _mustUpdateNavmesh = true;
                return;
            }

            if (navmeshPathLength > 1)
            {
                // Check if have obstacles on the path. If have any navmesh obstacle between the navmesh points
                // the navmesh will be changed, creating a "hole" on the navmesh. The raycast will return true if 
                // detect this "hole" on the path so we need to update the path again to avoid this "hole".
                Vector3 lastNavmeshPoint = _navmeshPathData.corners[Mathf.Max(CurrentNavmeshWaypoint - 1, 0)];
                Vector3 currentNavmeshPoint = _navmeshPathData.corners[CurrentNavmeshWaypoint];
                if (NavMesh.Raycast(lastNavmeshPoint, currentNavmeshPoint, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    _mustUpdateNavmesh = true;
                    return;
                }
            }
        }

        private void UpdatePathToDestination()
        {
            Vector3 aiPositionOnNavmesh = JU_Ai.ClosestToNavMesh(Ai.transform.position);
            JU_Ai.CalculatePath(aiPositionOnNavmesh, DestinationOnNavmesh, _navmeshPathData);

            // waypoint 0 is the Ai start position.
            // waypoint 1 is the first waypoint that the AI must to move to.
            CurrentNavmeshWaypoint = 1;
        }

        /// <summary>
        /// <para> Update the current waypoint index of a AI that is moving along a waypoint path. </para>
        /// Check if a position is near of the current waypoint and update to go to the next waypoint.
        /// </summary>
        /// <param name="center">The position inside the path.</param>
        /// <param name="currentWaypointIndex">The current waypoint index.</param>
        /// <param name="path">The path.</param>
        /// <param name="jumpDistance">The distance to change the current waypoint to the next waypoint.</param>
        /// <returns>Return true if the current waypoint was changed.</returns>
        protected static bool UpdateCurrentWaypoint(Vector3 center, ref int currentWaypointIndex, Vector3[] path, float jumpDistance)
        {
            bool isReversePath = false;
            return UpdateCurrentWaypoint(center, ref currentWaypointIndex, path, jumpDistance, EndPathModes.Stop, ref isReversePath);
        }

        /// <summary>
        /// <para> Update the current waypoint index of a AI that is moving along a waypoint path. </para>
        /// Check if a position is near of the current waypoint and update to go to the next waypoint.
        /// </summary>
        /// <param name="center">The position inside the path.</param>
        /// <param name="currentWaypointIndex">The current waypoint index.</param>
        /// <param name="path">The path.</param>
        /// <param name="jumpDistance">The distance to change the current waypoint to the next waypoint.</param>
        /// <param name="endPathMode">Is what the AI must do when it's finished move on the path.</param>
        /// <param name="isReversePath"><para>True if the AI must move along the path in the reversed direction. </para>
        /// <para>Must be ref because the value can be changed if <see cref="endPathMode"/> is <see cref="EndPathModes.InvertPath"/> and if the AI is on end of the path.</para></param>
        /// <returns>Return true if the current waypoint was changed.</returns>
        protected static bool UpdateCurrentWaypoint(Vector3 center, ref int currentWaypointIndex, Vector3[] path, float jumpDistance, EndPathModes endPathMode, ref bool isReversePath)
        {
            if (path.Length < 2)
            {
                if (currentWaypointIndex != 0)
                {
                    currentWaypointIndex = 0;
                    return true;
                }

                return false;
            }

            Vector3 currentWaypoint = path[currentWaypointIndex];
            bool jumpWaypoint = Vector3.Distance(center, currentWaypoint) < jumpDistance;

            if (jumpWaypoint && !isReversePath)
            {
                if (currentWaypointIndex < path.Length - 1)
                {
                    currentWaypointIndex += 1;
                    return true;
                }
                else if (endPathMode == EndPathModes.Restart)
                {
                    currentWaypointIndex = 0;
                    return true;
                }
                else if (endPathMode == EndPathModes.InvertPath)
                {
                    // Inverting the path.
                    currentWaypointIndex = Mathf.Max(path.Length - 2, 0);
                    isReversePath = true;
                    return true;
                }
            }

            if (jumpWaypoint && isReversePath)
            {
                if (currentWaypointIndex > 0)
                {
                    currentWaypointIndex -= 1;
                    return true;
                }
                else if (endPathMode == EndPathModes.Restart)
                {
                    currentWaypointIndex = path.Length - 1;
                    return true;
                }
                else if (endPathMode == EndPathModes.InvertPath)
                {
                    // Inverting the path.
                    currentWaypointIndex = Mathf.Min(path.Length - 1, 1);
                    isReversePath = false;
                    return true;
                }
            }

            return false;
        }
    }
}