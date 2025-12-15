using JU.AI;
using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Control the AI to move randomly inside an <see cref="JUBoxArea"/>.
    /// </summary>
    [System.Serializable]
    public class MoveRandomInsideArea : JU_AIActionBase
    {
        private JUBoxArea _currentArea;
        private bool _newDestinationRequested;
        private float _currentChangeDestinationTimer;

        /// <summary>
        /// Use fire pose while is moving.
        /// </summary>
        public bool UseFirePose;

        /// <summary>
        /// If true, the AI will run.
        /// </summary>
        public bool Run;

        /// <summary>
        /// The distance to stop when is closest to the destination.
        /// </summary>
        public float StopDistance;

        /// <summary>
        /// The time to generate a new random destination.
        /// </summary>
        public float ChangeDestinationTime;

        /// <summary>
        /// Called on change the <see cref="CurrentDestination"/>
        /// </summary>
        public UnityAction<Vector3> OnDestinationChanged;

        /// <summary>
        /// The current Destination.
        /// </summary>
        public Vector3 CurrentDestination { get; private set; }

        /// <inheritdoc/>
        public MoveRandomInsideArea()
        {
            UseFirePose = false;
            Run = false;
            StopDistance = 2;
            ChangeDestinationTime = 10;
        }

        /// <inheritdoc/>
        public override void Setup(JUCharacterAIBase ai)
        {
            base.Setup(ai);
            _currentChangeDestinationTimer = ChangeDestinationTime;
        }

        /// <summary>
        ///  Control the AI to move randomly inside an <see cref="JUBoxArea"/>.
        /// </summary>
        /// <param name="area">The target area.</param>
        /// <param name="control">The AI control.</param>
        public void Update(JUBoxArea area, ref JUCharacterAIBase.AIControlData control)
        {
            bool areaWasChanged = false;

            if (area != _currentArea)
            {
                _currentArea = area;

                if (_currentArea)
                    areaWasChanged = true;
            }

            if (!_currentArea)
            {
                control.MoveToDirection = Vector3.zero;
                control.LookToDirection = Ai.transform.forward;
                control.IsAttackPose = UseFirePose;
                control.IsAttackPose = false;
                control.IsRunning = false;
                return;
            }

            UpdateDestination(areaWasChanged);
            UpdatePathToDestination(CurrentDestination, false);

            Vector3 aiPosition = Ai.transform.position;
            Vector3 moveDirection = Vector3.zero;
            Vector3 lookDirection;

            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:
                    moveDirection = CurrentDestination - aiPosition;
                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (NavmeshPath.Count > 0)
                        moveDirection = NavmeshPath[CurrentNavmeshWaypoint] - aiPosition;
                    break;
                default:
                    throw new UnityException("Invalid Option");
            }

            if (Vector3.Distance(aiPosition, CurrentDestination) < StopDistance)
                moveDirection = Vector3.zero;

            lookDirection = moveDirection.magnitude > 0 ? moveDirection.normalized : Ai.transform.forward;

            control.MoveToDirection = moveDirection;
            control.LookToDirection = lookDirection;
            control.IsAttackPose = UseFirePose;
            control.IsRunning = Run;
            control.IsAttacking = false;
        }

        private void UpdateDestination(bool forceChangeDestination)
        {
            if (!_currentArea)
                return;

            bool changeDestination = forceChangeDestination || _newDestinationRequested;
            if (!changeDestination)
            {
                _currentChangeDestinationTimer += Time.deltaTime;
                if (_currentChangeDestinationTimer >= ChangeDestinationTime)
                    changeDestination = true;
            }

            if (!changeDestination)
                return;

            if (TryFindRandomDestination(_currentArea, out Vector3 newDestination))
            {
                _newDestinationRequested = false;
                _currentChangeDestinationTimer = 0;
                CurrentDestination = newDestination;

                OnDestinationChanged?.Invoke(CurrentDestination);
            }
        }

        private bool TryFindRandomDestination(JUBoxArea area, out Vector3 destination)
        {
            if (!area)
            {
                destination = Vector3.zero;
                return false;
            }

            Bounds areaBounds = area.Bounds;
            Vector3 center = areaBounds.center;
            Vector3 size = areaBounds.size;
            Vector3 newDestination = center + (new Vector3(Random.Range(-size.x, size.x), Random.Range(-size.y, size.y), Random.Range(-size.z, size.z)) * 0.5f);

            switch (NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:

                    destination = newDestination;
                    return true;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (JU_Ai.ClosestToNavMesh(newDestination, out newDestination))
                    {
                        destination = newDestination;
                        return true;
                    }

                    destination = Vector3.zero;
                    return false;
                default:
                    throw new UnityException("Invalid Option");
            }
        }

        /// <summary>
        /// Request a new random destination to character move. Cannot be changed in the current frame
        /// because the generation must be valitaded asynchronously.
        /// Use <see cref="OnDestinationChanged"/> to get the new destination when generated or <see cref="CurrentDestination"/> 
        /// to get the current destination.
        /// </summary>
        public void RequestNewRandomDestination()
        {
            _newDestinationRequested = true;
        }
    }
}