using System;
using System.Collections.Generic;
using System.Threading;
using JU.AI;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Profiling;

namespace JU.CharacterSystem.AI.EscapeSystem
{
    /// <summary>
    /// Escape AI state, use this to AI escape from explosions areas,
    /// danger areas for NPCs or any other area that the AI must avoid.
    /// </summary>
    [Serializable]
    public class Escape : JU_AIActionBase
    {
        // Used to find wich areas must be avoided by AIs.
        private class AI_EscapeUpdateManager : MonoBehaviour
        {
            private static AI_EscapeUpdateManager _instance;

            // Num of AIs that can be updated on each frame.
            private const int MAX_UPDATES_PER_FRAME = 5;

            private int _updateQueueCount;
            public Queue<Escape> _updateAsyncQueue; // Store wich AIs must be updated yet.

            public static AI_EscapeUpdateManager Instance
            {
                get
                {
                    CreateInstanceIfNull();
                    return _instance;
                }
            }

            public AI_EscapeUpdateManager()
            {
                _updateAsyncQueue = new Queue<Escape>();
            }

            private void Start()
            {
                RequestFindAreasAsync();
            }

            private void Update()
            {
                if (_updateQueueCount <= 0)
                    return;

                for (int i = 0; i < MAX_UPDATES_PER_FRAME; i++)
                {
                    Escape escapeInstance = _updateAsyncQueue.Peek();

                    if (escapeInstance != null)
                    {
                        // Ignore this instance if it's already calculing the areas.
                        if (escapeInstance.IsRunningAsync)
                        {
                            continue;
                        }

                        escapeInstance.FindAreasToEscapeAsync();
                    }

                    _updateAsyncQueue.Dequeue();
                    _updateQueueCount -= 1;

                    if (_updateQueueCount <= 0)
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Request all AIs to recalculate the areas to escape.
            /// </summary>
            public static void RequestFindAreasAsync()
            {
                // Do not access the Instance property because this can create a new instance during game closing.
                if (!_instance)
                    return;

                Profiler.BeginSample(nameof(RequestFindAreasAsync));

                _instance._updateAsyncQueue.Clear();
                foreach (var instance in _escapeInstances)
                    _instance._updateAsyncQueue.Enqueue(instance);

                _instance._updateQueueCount = _instance._updateAsyncQueue.Count;

                Profiler.EndSample();
            }

            public static void CreateInstanceIfNull()
            {
                if (_instance)
                    return;

                _instance = new GameObject("JU AI Escape Manager").AddComponent<AI_EscapeUpdateManager>();
                _instance.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private struct AreaAndTagData
        {
            public Bounds AreaBounds;
            public JUTag AreaTag;
        }

        [Serializable]
        private struct EscapeAreaData
        {
            public Bounds AreaBounds;
            public bool Run;
            public bool BlockGunAttacks;
            public bool EscapeOnlyIfOnView;
            public float MaxViewAngle;
            public LayerMask ObstaclesLayer;
        }

        /// <summary>
        /// Stores all area settings.
        /// </summary>
        [Serializable]
        public struct EscapeAreaSettings : ISerializationCallbackReceiver
        {
            /// <summary>
            /// The area name.
            /// </summary>
            public string Name;

            /// <summary>
            /// The area tag, used to try escape only from specific areas.
            /// </summary>
            [Header("Area Detection")]
            public JUTag AreaTag;

            /// <summary>
            /// If true, the AI will try escape from the area only if is on view.
            /// </summary>
            public bool EscapeOnlyIfOnView;

            /// <summary>
            /// Does not allow AI use gun weapons if is escaping an area.
            /// </summary>
            public bool BlockGunAttacks;

            /// <summary>
            /// The max view angle, used to ignore escape areas if not on view if <see cref="EscapeOnlyIfOnView"/> is false.
            /// </summary>
            [Range(1f, 180)]
            public float MaxViewAngle;

            /// <summary>
            /// Layer to identify obstacles (like walls) to not escape from areas if <see cref="EscapeOnlyIfOnView"/> is true.
            /// </summary>
            public LayerMask ObstaclesLayer;

            /// <summary>
            /// If true, the AI will run on try escape from the area.
            /// </summary>
            [Header("Behavior")]
            public bool Run;

            /// <summary>
            /// Called by editor to fill empty values before serialize.
            /// </summary>
            public void OnBeforeSerialize()
            {
                if (MaxViewAngle == 0)
                    MaxViewAngle = 90;
            }

            /// <summary>
            /// Called by editor after serialize.
            /// </summary>
            public void OnAfterDeserialize()
            {
            }
        }

        // The position to move when is trying to escape from an area.
        private bool _positionToMoveFound;
        private Vector3 _positionToMove;

        // Stores all instantied escape areas that are used by all AIs. Used to sync the areas with
        // newer AIs that is instantied in runtime.
        private static Dictionary<Guid, AreaAndTagData> _allGlobalAreas;

        // Stores escape areas that will be used by each instance.
        private Dictionary<Guid, EscapeAreaData> _rawAreasToEscape;

        // Stores the ID of the escape area that the instance is inside or "default value" if isn't inside of
        // any escape area from _simplifiedAreasToEscape.
        private Guid _currentAreaId;

        // Stores the ID of the escape area that the AI maybe will enter on next frame.
        // Cached value only to avoid performance loss, not relevant for the logic.
        private Guid _possibleFutureAreaId;

        // Stores a simplified list of escape areas (from _rawAreas) that will be used by each instance.
        // The simplified list merges areas that are overlaping itself. 
        private Dictionary<Guid, EscapeAreaData> _simplifiedAreasToEscape;

        // All instances of this system.
        private static List<Escape> _escapeInstances;

        // The thread used to merge the areas that is overlaping itself.
        private Thread _calculateAreasThread;
        private EscapeAreaData _currentAreaToEscape;

        [SerializeField] private EscapeAreaSettings[] _allowedAreas;

        /// <summary>
        /// Return true if is processing data that is used to escape on multi-thread.
        /// If multi-thread is running, the system must be paused to avoid bugs about
        /// data syncronization. Wait this return false to continue with escape logic.
        /// The multi-thread is used to not have low framerate on calculate wich areas
        /// that must be avoided or calculate the path to escape. 
        /// </summary>
        private bool IsRunningAsync
        {
            get
            {
                if (_calculateAreasThread == null)
                    return false;

                return _calculateAreasThread.IsAlive;
            }
        }

        /// <summary>
        /// Return true if is trying escape from some escape area.
        /// </summary>
        public bool IsTryingEscape { get; private set; }

        /// <summary>
        /// Return true if the AI is inside some escape area.
        /// </summary>
        public bool IsInsideOfSomeEscapeArea { get; private set; }

        /// <summary>
        /// Return a read-only collection that contains all areas that the AI will try to escape.
        /// </summary>
        public ReadOnlyArray<EscapeAreaSettings> AllowedAreas
        {
            get => new ReadOnlyArray<EscapeAreaSettings>(_allowedAreas);
        }

        /// <inheritdoc/>
        public Escape() : base()
        {
            _allowedAreas = new EscapeAreaSettings[0];
        }

        /// <inheritdoc/>
        public override void OnValidate()
        {
            base.OnValidate();

            LayerMask[] defaultObstacleLayers = {
                LayerMask.NameToLayer("Default"),
                LayerMask.NameToLayer("Wall"),
                LayerMask.NameToLayer("Walls"),
                LayerMask.NameToLayer("Obstacle"),
                LayerMask.NameToLayer("Obstacles"),
                LayerMask.NameToLayer("Terrain"),
                LayerMask.NameToLayer("Character"),
                LayerMask.NameToLayer("Characters"),
                LayerMask.NameToLayer("Player"),
                LayerMask.NameToLayer("Players"),
                LayerMask.NameToLayer("Vehicle"),
                LayerMask.NameToLayer("Vehicles")
            };

            for (int i = 0; i < _allowedAreas.Length; i++)
            {
                var area = _allowedAreas[i];

                if (string.IsNullOrEmpty(area.Name))
                    area.Name = $"Area {i + 1}";

                if (area.ObstaclesLayer == 0)
                {
                    for (int x = 0; x < defaultObstacleLayers.Length; x++)
                    {
                        var defaultObstacleLayer = defaultObstacleLayers[x];
                        if (defaultObstacleLayer != -1)
                            area.ObstaclesLayer |= 1 << defaultObstacleLayer;
                    }
                }

                _allowedAreas[i] = area;
            }
        }

        /// <inheritdoc/>
        public override void Setup(JUCharacterAIBase ai)
        {
            if (_rawAreasToEscape == null)
                _rawAreasToEscape = new Dictionary<Guid, EscapeAreaData>();

            if (_simplifiedAreasToEscape == null)
                _simplifiedAreasToEscape = new Dictionary<Guid, EscapeAreaData>();

            if (_escapeInstances == null)
                _escapeInstances = new List<Escape>();

            _escapeInstances.Add(this);

            base.Setup(ai);

            SyncWithGlobalAreas();
            AI_EscapeUpdateManager.CreateInstanceIfNull();

#if UNITY_EDITOR || DEVELOPMENT_BUILD 

            // Validate allowed areas.
            for (int i = 0; i < _allowedAreas.Length; i++)
            {
                if (!_allowedAreas[i].AreaTag)
                    Debug.Assert(_allowedAreas[i].AreaTag, $"{ai.name} AI {typeof(Escape).Name} action have invalid areas to escape. Assign the area tag on the Allowed Aras field.");
            }
#endif
        }

        /// <inheritdoc/>
        public override void Unsetup()
        {
            base.Unsetup();

            if (IsRunningAsync)
                _calculateAreasThread.Abort();

            if (_escapeInstances != null)
                _escapeInstances.Remove(this);
        }

        /// <summary>
        /// Update the state to escape from an area if is within any.
        /// </summary>
        /// <param name="control">The current AI control data.</param>
        public void Update(ref JUCharacterAIBase.AIControlData control)
        {
            if (IsRunningAsync)
                return;

            if (_simplifiedAreasToEscape.Count == 0)
                return;

            bool isInsideOfSomeArea = IsInsideOfSomeArea(Ai.Center, ref _currentAreaId, _simplifiedAreasToEscape);

            // The AI moved out a escape area, but must find areas to escape again to be sure that isn't
            // inside a new area that was not found before.
            if (!isInsideOfSomeArea && IsInsideOfSomeEscapeArea)
                FindAreasToEscapeAsync();

            IsInsideOfSomeEscapeArea = isInsideOfSomeArea;

            if (IsInsideOfSomeEscapeArea)
            {
                EscapeAreaData currentArea = _simplifiedAreasToEscape[_currentAreaId];
                if (!currentArea.Equals(_currentAreaToEscape))
                {
                    _currentAreaToEscape = _simplifiedAreasToEscape[_currentAreaId];
                    IsTryingEscape = MustEscapeFromArea(Ai.Center, Ai.transform.forward, _currentAreaToEscape);

                    // Reset flag to try find a new position again to escape from the area when
                    // the area is changed.
                    _positionToMoveFound = false;
                }
            }
            else
            {
                _currentAreaToEscape = default;
                _positionToMoveFound = false;
                IsTryingEscape = false;

                bool isMoving = control.MoveToDirection.magnitude > 0.1f;

                if (isMoving)
                {
                    float aiColliderRadius = Ai.BodyCollider.bounds.size.magnitude;
                    Vector3 forward = control.MoveToDirection / control.MoveToDirection.magnitude;
                    Vector3 futureAiPosition = Ai.Center + (forward * aiColliderRadius);

                    // Stop to avoid entering the area.
                    if (IsInsideOfSomeArea(futureAiPosition, ref _possibleFutureAreaId))
                        control.MoveToDirection = Vector3.zero;
                }
            }

            EscapeFromCurrentAreaIfHave(ref control);
        }

        private void EscapeFromCurrentAreaIfHave(ref JUCharacterAIBase.AIControlData control)
        {
            if (!IsTryingEscape || _currentAreaToEscape.AreaBounds.size == Vector3.zero)
            {
                _positionToMoveFound = false;
                return;
            }

            bool forceUpdateNavigation = false;

            // Try find a position to escape from the current area.
            if (!_positionToMoveFound)
            {
                bool useNavmesh = Ai.NavigationSettings.Mode == JUCharacterAIBase.NavigationModes.UseNavmesh;
                _positionToMoveFound = TryFindPositionToMove(_currentAreaToEscape.AreaBounds, out _positionToMove, useNavmesh);

                // There is no a position to move yet.
                if (!_positionToMoveFound)
                    return;

                // Force update navigation when the AI finds a new position to move (to escape).
                // This will avoid delays to update navigation when a escape area is created/changed.
                forceUpdateNavigation = true;
            }

            UpdatePathToDestination(_positionToMove, forceUpdateNavigation);

            Vector3 moveDirection = Vector3.zero;
            switch (Ai.NavigationSettings.Mode)
            {
                case JUCharacterAIBase.NavigationModes.Simple:
                    moveDirection = RawDestination - Ai.Center;
                    break;
                case JUCharacterAIBase.NavigationModes.UseNavmesh:

                    if (NavmeshPath.Count > 1)
                        moveDirection = NavmeshPath[CurrentNavmeshWaypoint] - Ai.Center;
                    break;
                default:
                    throw new InvalidOperationException("Invalid option.");
            }

            bool isOnFirePose = control.IsAttackPose && Ai.Character.RightHandWeapon && !_currentAreaToEscape.BlockGunAttacks;

            if (!isOnFirePose)
            {
                control.IsAttacking = false;
                control.IsAttackPose = false;
                control.LookToDirection = Ai.Center - _currentAreaToEscape.AreaBounds.center;
            }

            moveDirection.Normalize();
            control.MoveToDirection = moveDirection;
            control.IsRunning = _currentAreaToEscape.Run;
        }

        private bool TryFindPositionToMove(Bounds areaToEscape, out Vector3 positionToMove, bool useNavmesh)
        {
            Debug.Assert(!areaToEscape.Equals(default), "Invalid area to try to escape.");

            positionToMove = Vector3.zero;
            float areaSize = areaToEscape.size.magnitude / 2;
            Vector3 areaCenter = areaToEscape.center;
            Vector3 aiPosition = Ai.Center;

            if (useNavmesh)
            {
                // Try find the AI position inside the navmesh.
                if (!JU_Ai.ClosestToNavMesh(aiPosition, out aiPosition))
                    return false;

                // The area to escape isn't inside or near of the navmesh.
                if (!JU_Ai.ClosestToNavMesh(areaCenter, out areaCenter))
                    return false;
            }

            Vector3 directionToOutsideArea = (aiPosition - areaCenter).normalized;
            positionToMove = areaCenter + (directionToOutsideArea * areaSize);

            if (useNavmesh)
            {
                if (!JU_Ai.ClosestToNavMesh(positionToMove, out positionToMove))
                    return false;

                // The position to move is inside the area that the AI must escape.
                // This can happen if the navmesh is small and there are no way to go far.
                if (areaToEscape.Contains(positionToMove))
                    return false;
            }

            return true;
        }

        private static bool MustEscapeFromArea(Vector3 aiPosition, Vector3 aiForward, EscapeAreaData areaData)
        {
            if (!areaData.EscapeOnlyIfOnView)
                return true;

            aiForward = Vector3.ProjectOnPlane(aiForward, Vector3.up);
            aiForward /= aiForward.magnitude;

            Profiler.BeginSample(nameof(MustEscapeFromArea));

            Vector3 areaCenter = areaData.AreaBounds.center;
            Vector3 directionToEscapeArea = Vector3.ProjectOnPlane(areaCenter - aiPosition, Vector3.up);
            directionToEscapeArea /= directionToEscapeArea.magnitude;

            bool mustEscape = true;

            // Don't need to escape if the escape area isn't on field of view.
            if (Vector3.Angle(directionToEscapeArea, aiForward) > areaData.MaxViewAngle)
                mustEscape = false;

            // Don't need to escape if have an obstacle between the AI and the escape area (a wall as example).
            if (mustEscape && Physics.Linecast(aiPosition, areaCenter, areaData.ObstaclesLayer, QueryTriggerInteraction.Ignore))
                mustEscape = false;

            Profiler.EndSample();

            return mustEscape;
        }

        internal override void DrawGizmosSelected()
        {
#if UNITY_EDITOR
            base.DrawGizmosSelected();

            if (_simplifiedAreasToEscape == null || _simplifiedAreasToEscape.Count == 0 || IsRunningAsync)
                return;

            Gizmos.color = Color.red * 0.5f;
            foreach (KeyValuePair<Guid, EscapeAreaData> area in _simplifiedAreasToEscape)
                UnityEditor.Handles.DrawWireCube(area.Value.AreaBounds.center, area.Value.AreaBounds.size);
#endif
        }

        private void RequestFindAreasToEscape()
        {
            AI_EscapeUpdateManager.RequestFindAreasAsync();
        }

        private void FindAreasToEscapeAsync()
        {
            Profiler.BeginSample(nameof(FindAreasToEscapeAsync));

            // Stop the thread is is running to restart after.
            if (IsRunningAsync)
                _calculateAreasThread.Abort();

            if (_simplifiedAreasToEscape == null)
                _simplifiedAreasToEscape = new Dictionary<Guid, EscapeAreaData>();

            _simplifiedAreasToEscape.Clear();

            Vector3 aiPosition = Ai.Center;
            Vector3 aiForward = Ai.transform.forward;

            // Find areas that the AI must escape, like areas that is on field of view.
            // Can't use multi-thread because the checks uses raycast (thread-safe)
            foreach (KeyValuePair<Guid, EscapeAreaData> v in _rawAreasToEscape)
            {
                if (!MustEscapeFromArea(aiPosition, aiForward, v.Value))
                    continue;

                _simplifiedAreasToEscape.Add(v.Key, v.Value);
            }

            // This algoritm simplifies the list of escape areas checking
            // if have collisions between each area and creating a new list
            // of all encapsulated areas.
            // Use multithreading to avoid performance loss.
            _calculateAreasThread = new Thread(() =>
            {
                bool hasChanges;

                do
                {
                    hasChanges = false;
                    foreach (KeyValuePair<Guid, EscapeAreaData> a in _simplifiedAreasToEscape)
                    {
                        foreach (KeyValuePair<Guid, EscapeAreaData> b in _simplifiedAreasToEscape)
                        {
                            if (a.Key == b.Key)
                                continue;

                            // Encapsule each one if is colliding.
                            if (a.Value.AreaBounds.Intersects(b.Value.AreaBounds))
                            {
                                EscapeAreaData aAreaSettings = a.Value;
                                EscapeAreaData bAreaSettings = b.Value;
                                EscapeAreaData newAreaData = _simplifiedAreasToEscape[a.Key];

                                // Merge the two area settings into a single area.
                                // Give priority to areas that has no field of view or 
                                // that have more sensitive values. This will do the AI
                                // try to escape from the area even if is inside of multiple areas that
                                // have different settings.
                                newAreaData = new EscapeAreaData
                                {
                                    // Force escape (ignoring field of view) if one of these areas does not uses field of view.
                                    EscapeOnlyIfOnView = aAreaSettings.EscapeOnlyIfOnView && bAreaSettings.EscapeOnlyIfOnView,

                                    // Disable weapon if one of these areas block weapons.
                                    BlockGunAttacks = aAreaSettings.BlockGunAttacks || bAreaSettings.BlockGunAttacks,

                                    // Use the highest field of view.
                                    MaxViewAngle = Mathf.Max(aAreaSettings.MaxViewAngle, bAreaSettings.MaxViewAngle),

                                    // Use both obstacles layer.
                                    ObstaclesLayer = aAreaSettings.ObstaclesLayer | bAreaSettings.ObstaclesLayer,

                                    // Force run, if any of these areas require run.
                                    Run = aAreaSettings.Run | bAreaSettings.Run,
                                };

                                // Merge two area abounds that were in collision.
                                Bounds mergedAreas = a.Value.AreaBounds;
                                mergedAreas.Encapsulate(b.Value.AreaBounds);
                                newAreaData.AreaBounds = mergedAreas;

                                _simplifiedAreasToEscape[a.Key] = newAreaData;
                                _simplifiedAreasToEscape.Remove(b.Key);
                                hasChanges = true;
                                break;
                            }
                        }

                        if (hasChanges)
                            break;
                    }

                } while (hasChanges);
            });

            _calculateAreasThread.Start();

            Profiler.EndSample();
        }

        private static bool IsInsideOfSomeArea(Vector3 point, ref Guid currentAreaId, Dictionary<Guid, EscapeAreaData> areas)
        {
            if (areas == null)
            {
                currentAreaId = default;
                return false;
            }

            Profiler.BeginSample(nameof(IsInsideOfSomeArea));
            bool isValidId = !currentAreaId.Equals(Guid.Empty); // First needs to check if the current area ID is not null.

            if (isValidId) // If the ID is not null, check if exist in the areas dictionary. (Maybe was removed).
            {
                if (!areas.ContainsKey(currentAreaId))
                    isValidId = false;
            }

            // Check if the point is inside the current area.
            if (isValidId)
            {
                if (areas[currentAreaId].AreaBounds.Contains(point))
                    return true;
            }

            // Check if the point is inside some area.
            foreach (KeyValuePair<Guid, EscapeAreaData> area in areas)
            {
                // The point is inside this area, so, select it as the current area.
                if (area.Value.AreaBounds.Contains(point))
                {
                    currentAreaId = area.Key;
                    return true;
                }
            }

            currentAreaId = default;
            Profiler.EndSample();

            return false;
        }

        private bool IsInsideOfSomeArea(Vector3 point, ref Guid currentAreaId, bool checkIfMustEscape = true)
        {
            Profiler.BeginSample(nameof(IsInsideOfSomeArea));

            bool isInside = false;
            if (IsInsideOfSomeArea(point, ref currentAreaId, _simplifiedAreasToEscape))
                isInside = true;

            if (isInside && checkIfMustEscape)
                isInside = MustEscapeFromArea(point, Ai.transform.forward, _simplifiedAreasToEscape[currentAreaId]);

            Profiler.EndSample();

            return isInside;
        }

        /// <summary>
        /// Check if a point is inside an escape area.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="checkIfMustEscape">If true, this will verify if the area that the point is inside must be avoided by the AI.</param>
        /// <returns>Return true if the point is inside an area. The result will be false if the AI does not need to avoid the area and if <see cref="checkIfMustEscape"/> is true.</returns>
        public bool IsInsideOfSomeArea(Vector3 point, bool checkIfMustEscape = true)
        {
            Guid areaThatIsInside = Guid.Empty;
            return IsInsideOfSomeArea(point, ref areaThatIsInside, checkIfMustEscape);
        }

        // Will add the escapeArea only if have the correct tag.
        private void TryAddEscapeArea(Bounds escapeArea, Guid areaId, JUTag tag, bool recalculateAreas = true)
        {
            ThrowErrorIfAreaIdIsNull(areaId);
            ThrowErrorIfAreaIsEmpty(escapeArea);
            ThrowErrorIfTagIsNull(tag);

            if (_rawAreasToEscape.ContainsKey(areaId))
            {
                Debug.LogError($"A bounds with ID {areaId} was alread added");
                return;
            }

            EscapeAreaData areaData = new EscapeAreaData
            {
                AreaBounds = escapeArea
            };

            // Try find the correct area settings using the especified tag.

            Profiler.BeginSample(nameof(TryAddEscapeArea));
            bool tagFound = false;
            for (int i = 0; i < _allowedAreas.Length; i++)
            {
                var areaSettings = _allowedAreas[i];

                ThrowErrorIfTagIsNull(areaSettings.AreaTag);
                if (areaSettings.EscapeOnlyIfOnView && areaSettings.MaxViewAngle <= 1f)
                    Debug.LogWarning($"The AI {Ai.name} have invalid Escape Area settings on {_allowedAreas} -> Tag {areaSettings.AreaTag}");

                if (areaSettings.AreaTag == tag)
                {
                    tagFound = true;

                    // Copy the area settings to the created area data.
                    areaData.EscapeOnlyIfOnView = areaSettings.EscapeOnlyIfOnView;
                    areaData.BlockGunAttacks = areaSettings.BlockGunAttacks;
                    areaData.MaxViewAngle = areaSettings.MaxViewAngle;
                    areaData.ObstaclesLayer = areaSettings.ObstaclesLayer;
                    areaData.Run = areaSettings.Run;
                    break;
                }
            }
            Profiler.EndSample();

            if (!tagFound)
                return;

            _rawAreasToEscape.Add(areaId, areaData);

            if (recalculateAreas)
                RequestFindAreasToEscape();
        }

        private void RemoveArea(Guid areaId)
        {
            ThrowErrorIfAreaIdIsNull(areaId);

            if (!_rawAreasToEscape.ContainsKey(areaId))
                return;

            _rawAreasToEscape.Remove(areaId);
            RequestFindAreasToEscape();
        }

        private void UpdateArea(Bounds newEscapeArea, Guid areaId)
        {
            Profiler.BeginSample(nameof(UpdateArea));
            ThrowErrorIfAreaIdIsNull(areaId);
            ThrowErrorIfAreaIsEmpty(newEscapeArea);

            if (!_rawAreasToEscape.ContainsKey(areaId))
            {
                Profiler.EndSample();
                return;
            }

            var newData = _rawAreasToEscape[areaId];
            newData.AreaBounds = newEscapeArea;

            _rawAreasToEscape[areaId] = newData;
            RequestFindAreasToEscape();

            Profiler.EndSample();
        }

        /// <summary>
        /// Try get an area bounds.
        /// </summary>
        /// <param name="areaId">The are ID.</param>
        /// <param name="areaBounds">The are if was found.</param>
        /// <returns>Return true if the area with the ID exist and if is used by this instance.</returns>
        public bool TryGetArea(Guid areaId, out Bounds areaBounds)
        {
            ThrowErrorIfAreaIdIsNull(areaId);

            if (!_rawAreasToEscape.ContainsKey(areaId))
            {
                areaBounds = default;
                return false;
            }

            areaBounds = _rawAreasToEscape[areaId].AreaBounds;
            return true;
        }

        /// <summary>
        /// Add a new area to escape.
        /// </summary>
        /// <param name="escapeArea">The new area bounds.</param>
        /// <param name="areaId">Return an ID that can be used to get, update or remove the escape area.</param>
        /// <param name="tag">The area tag. Used to filter areas to escape from onlycorrect areas.</param>
        public static void AddEscapeArea(Bounds escapeArea, out Guid areaId, JUTag tag = null)
        {
            ThrowErrorIfTagIsNull(tag);

            if (_allGlobalAreas == null)
                _allGlobalAreas = new Dictionary<Guid, AreaAndTagData>();

            if (_escapeInstances == null)
                _escapeInstances = new List<Escape>();

            areaId = Guid.NewGuid();
            _allGlobalAreas.Add(areaId, new AreaAndTagData
            {
                AreaBounds = escapeArea,
                AreaTag = tag
            });

            SyncInstancesWithGlobalAreas();
        }

        // Sync the instance adding all missing areas from _allGlobalAreas.
        // Call this if you instantiate a new Instance or have created, or destroyed an area.
        // All instances must have the areas synchronized to escape from the correct instantied areas.
        private void SyncWithGlobalAreas()
        {
            if (_allGlobalAreas == null)
                _allGlobalAreas = new Dictionary<Guid, AreaAndTagData>();

            foreach (KeyValuePair<Guid, AreaAndTagData> areaData in _allGlobalAreas)
            {
                if (_rawAreasToEscape.ContainsKey(areaData.Key))
                    continue;

                TryAddEscapeArea(areaData.Value.AreaBounds, areaData.Key, areaData.Value.AreaTag, false);
            }

            RequestFindAreasToEscape();
        }

        // Sync all instances adding all missing areas from _allGlobalAreas.
        private static void SyncInstancesWithGlobalAreas()
        {
            foreach (Escape instance in _escapeInstances)
                instance.SyncWithGlobalAreas();
        }

        /// <summary>
        /// Update position/scale of an area using a new Bounds.
        /// </summary>
        /// <param name="newEscapeArea">The new area bounds.</param>
        /// <param name="areaId">The area ID to update.</param>
        public static void UpdateEscapeArea(Bounds newEscapeArea, Guid areaId)
        {
            ThrowErrorIfAreaIdIsNull(areaId);

            if (_allGlobalAreas.ContainsKey(areaId))
            {
                AreaAndTagData updatedArea = _allGlobalAreas[areaId];
                updatedArea.AreaBounds = newEscapeArea;
                _allGlobalAreas[areaId] = updatedArea;
            }

            foreach (Escape instance in _escapeInstances)
                instance.UpdateArea(newEscapeArea, areaId);
        }

        /// <summary>
        /// Remove an area.
        /// </summary>
        /// <param name="areaId">The area ID.</param>
        public static void RemoveEscapeArea(Guid areaId)
        {
            if (_allGlobalAreas.ContainsKey(areaId))
                _allGlobalAreas.Remove(areaId);

            foreach (Escape instance in _escapeInstances)
                instance.RemoveArea(areaId);
        }

        /// <summary>
        /// Return true if a specific area is enveloping AI.
        /// </summary>
        /// <param name="areaId">The area to check.</param>
        /// <returns></returns>
        public bool CheckIfIsInsideEscapeArea(Guid areaId)
        {
            ThrowErrorIfAreaIdIsNull(areaId);

            if (!_rawAreasToEscape.ContainsKey(areaId))
            {
                Debug.LogWarning($"Escape area does not exist with {areaId}, maybe already removed?");
                return false;
            }

            return _rawAreasToEscape[areaId].AreaBounds.Contains(Ai.BodyCollider.bounds.center);
        }

        /// <summary>
        /// Return true if a specific area is enveloping a 3D point.
        /// </summary>
        /// <param name="areaId">The area ID.</param>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public static bool CheckIfIsInsideEscapeArea(Guid areaId, Vector3 point)
        {
            ThrowErrorIfAreaIdIsNull(areaId);

            if (!_allGlobalAreas.ContainsKey(areaId))
            {
                Debug.LogWarning($"Escape area does not exist with {areaId}, maybe already removed?");
                return false;
            }

            return _allGlobalAreas[areaId].AreaBounds.Contains(point);
        }

        private static void ThrowErrorIfAreaIdIsNull(Guid id)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(id != default, "The area ID can't be empty.");
#endif
        }

        private static void ThrowErrorIfTagIsNull(JUTag tag)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(tag, "The area tag can't be null.");
#endif
        }

        private static void ThrowErrorIfAreaIsEmpty(Bounds area)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(area.size != Vector3.zero, "The area can't be empty.");
#endif
        }
    }
}