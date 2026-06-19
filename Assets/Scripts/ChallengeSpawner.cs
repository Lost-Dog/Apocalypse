using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class ChallengeSpawner : MonoBehaviour
{
    public static ChallengeSpawner Instance { get; private set; }

    [Header("Zone Integration")]
    [SerializeField] private bool useDynamicZones = true;
    [SerializeField] private bool assignNearestDynamicZone = true;

    [Header("Spawn Placement")]
    [SerializeField] private int maxNavMeshAttempts = 50;
    [SerializeField] private float navMeshSampleDistance = 10f;
    [SerializeField] private float navMeshSpawnRadius = 10f;
    [SerializeField] private float minimumSpawnDistance = 2f;
    [SerializeField] private float fallbackSpawnRadius = 20f;
    [SerializeField] private bool useTransformSpawnPoints = false;

    [Header("Hierarchy")]
    [SerializeField] private bool parentSpawnedUnderSpawner = false;
    [SerializeField] private string runtimeRootName = "ChallengeRuntimeSpawns";

    [Header("Debug")]
    [SerializeField] private bool logSpawnDetails = false;

    private sealed class SpawnRuntimeData
    {
        public readonly List<GameObject> spawnedObjects = new List<GameObject>();
        public readonly List<ChallengeSpawnedActor> trackedActors = new List<ChallengeSpawnedActor>();
        public int enemiesSpawned;
        public int civiliansSpawned;
        public bool assignedZone;
    }

    private readonly Dictionary<ActiveChallenge, SpawnRuntimeData> _runtimeByChallenge =
        new Dictionary<ActiveChallenge, SpawnRuntimeData>();

    private Transform _runtimeRoot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        EnsureRuntimeRoot();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SpawnChallengeContent(
        ActiveChallenge challenge,
        ChallengeData data,
        GameObject worldMarkerPrefab,
        GameObject compassMarkerPrefab,
        Transform compassContainer,
        Transform worldspaceUIContainer = null)
    {
        if (challenge == null || data == null) return;

        CleanupChallenge(challenge);

        SpawnRuntimeData runtime = new SpawnRuntimeData();
        _runtimeByChallenge[challenge] = runtime;

        TryAssignDynamicZone(challenge, data, runtime);
        SpawnMarkers(challenge, worldMarkerPrefab, compassMarkerPrefab, compassContainer, worldspaceUIContainer, runtime);
        SpawnConfiguredItems(challenge, data, runtime);

        challenge.totalEnemiesSpawned = runtime.enemiesSpawned;

        if (logSpawnDetails)
        {
            Debug.Log($"[ChallengeSpawner] Spawned '{data.challengeName}' | enemies={runtime.enemiesSpawned}, civilians={runtime.civiliansSpawned}, objects={runtime.spawnedObjects.Count}");
        }
    }

    public void CleanupChallenge(ActiveChallenge challenge)
    {
        if (challenge == null) return;
        if (!_runtimeByChallenge.TryGetValue(challenge, out SpawnRuntimeData runtime)) return;

        for (int i = 0; i < runtime.trackedActors.Count; i++)
        {
            if (runtime.trackedActors[i] != null)
            {
                runtime.trackedActors[i].SuppressCallbacks();
            }
        }

        for (int i = runtime.spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = runtime.spawnedObjects[i];
            if (obj == null) continue;

            PooledObject pooled = obj.GetComponent<PooledObject>();
            if (pooled != null)
            {
                pooled.ReturnToPool();
            }
            else
            {
                Destroy(obj);
            }
        }

        if (runtime.assignedZone && DynamicZoneManager.Instance != null)
        {
            DynamicZoneManager.Instance.ReleaseChallengeZone(challenge);
        }

        _runtimeByChallenge.Remove(challenge);
    }

    private void SpawnConfiguredItems(ActiveChallenge challenge, ChallengeData data, SpawnRuntimeData runtime)
    {
        if (data.spawnItems == null || data.spawnItems.Count == 0) return;

        List<ChallengeData.SpawnableItem> orderedItems = data.spawnItems
            .Where(item => item != null)
            .OrderByDescending(item => item.priority)
            .ToList();

        for (int itemIndex = 0; itemIndex < orderedItems.Count; itemIndex++)
        {
            ChallengeData.SpawnableItem item = orderedItems[itemIndex];
            GameObject prefab = ResolveSpawnPrefab(item);

            if (prefab == null)
            {
                if (item.required)
                {
                    Debug.LogWarning($"[ChallengeSpawner] Required spawn item '{item.itemName}' has no prefab.");
                }

                continue;
            }

            int minCount = Mathf.Max(0, item.minCount);
            int maxCount = Mathf.Max(minCount, item.maxCount);
            int targetCount = Random.Range(minCount, maxCount + 1);

            for (int spawnIndex = 0; spawnIndex < targetCount; spawnIndex++)
            {
                if (!TryGetSpawnPose(challenge, data, item, spawnIndex, targetCount, runtime, out Vector3 spawnPosition, out Quaternion spawnRotation))
                {
                    if (item.required)
                    {
                        Debug.LogWarning($"[ChallengeSpawner] Failed to place required '{item.itemName}' ({spawnIndex + 1}/{targetCount}) for challenge '{data.challengeName}'.");
                    }

                    continue;
                }

                GameObject spawned = Instantiate(prefab, spawnPosition, spawnRotation, GetSpawnParent());
                bool enemyLikeByConfig = IsEnemyCategory(item.category);
                ApplyPostSpawnPlacement(spawned, spawnPosition, item.requireNavMesh || enemyLikeByConfig);
                runtime.spawnedObjects.Add(spawned);

                bool enemyLike = enemyLikeByConfig || IsLikelyEnemyObject(spawned, prefab, item.itemName);

                if (enemyLike)
                {
                    runtime.enemiesSpawned++;
                    TrackActor(spawned, challenge, ChallengeData.SpawnableCategory.Enemy, runtime);
                    ApplyEnemyScaling(spawned, challenge);
                }
                else if (item.category == ChallengeData.SpawnableCategory.Civilian)
                {
                    runtime.civiliansSpawned++;
                    TrackActor(spawned, challenge, item.category, runtime);
                }
            }
        }
    }

    private void SpawnMarkers(
        ActiveChallenge challenge,
        GameObject worldMarkerPrefab,
        GameObject compassMarkerPrefab,
        Transform compassContainer,
        Transform worldspaceUIContainer,
        SpawnRuntimeData runtime)
    {
        if (worldMarkerPrefab != null)
        {
            GameObject worldMarker = Instantiate(worldMarkerPrefab, challenge.position, Quaternion.identity, worldspaceUIContainer);
            runtime.spawnedObjects.Add(worldMarker);

            ChallengeWorldMarker marker = worldMarker.GetComponent<ChallengeWorldMarker>();
            if (marker != null)
            {
                marker.SetChallenge(challenge);
            }

            ChallengeWorldspaceUI worldspaceMarker = worldMarker.GetComponent<ChallengeWorldspaceUI>();
            if (worldspaceMarker != null)
            {
                worldspaceMarker.SetChallenge(challenge);
            }
        }

        if (compassMarkerPrefab != null)
        {
            GameObject compassMarker = Instantiate(compassMarkerPrefab, challenge.position, Quaternion.identity, compassContainer);
            runtime.spawnedObjects.Add(compassMarker);

            ChallengeCompassMarker marker = compassMarker.GetComponent<ChallengeCompassMarker>();
            if (marker != null)
            {
                marker.SetChallenge(challenge);
            }
        }

    }

    private void TrackActor(GameObject actor, ActiveChallenge challenge, ChallengeData.SpawnableCategory category, SpawnRuntimeData runtime)
    {
        ChallengeSpawnedActor tracker = actor.GetComponent<ChallengeSpawnedActor>();
        if (tracker == null)
        {
            tracker = actor.AddComponent<ChallengeSpawnedActor>();
        }

        tracker.Initialize(challenge, category);
        runtime.trackedActors.Add(tracker);
    }

    private void ApplyEnemyScaling(GameObject enemyRoot, ActiveChallenge challenge)
    {
        if (enemyRoot == null || challenge == null) return;

        DifficultyDamageMultiplier damageMultiplier = enemyRoot.GetComponent<DifficultyDamageMultiplier>();
        if (damageMultiplier == null)
        {
            damageMultiplier = enemyRoot.AddComponent<DifficultyDamageMultiplier>();
        }

        damageMultiplier.multiplier = Mathf.Max(0.01f, challenge.enemyDamageMultiplier);

        DifficultyHealthMultiplier healthMultiplier = enemyRoot.GetComponent<DifficultyHealthMultiplier>();
        if (healthMultiplier == null)
        {
            healthMultiplier = enemyRoot.AddComponent<DifficultyHealthMultiplier>();
        }

        healthMultiplier.multiplier = Mathf.Max(0.01f, challenge.enemyHealthMultiplier);
        healthMultiplier.TryApplyToCommonHealthFields(enemyRoot);
    }

    private GameObject ResolveSpawnPrefab(ChallengeData.SpawnableItem item)
    {
        if (item.prefab != null) return item.prefab;

        if (DynamicZoneManager.Instance == null) return null;

        if (item.category == ChallengeData.SpawnableCategory.Enemy || item.category == ChallengeData.SpawnableCategory.Boss)
        {
            return DynamicZoneManager.Instance.defaultEnemyPrefab;
        }

        // Legacy/migrated challenge data may leave combat entries as 'Other' with missing prefab refs.
        if (item.category == ChallengeData.SpawnableCategory.Other && IsLikelyEnemyName(item.itemName))
        {
            return DynamicZoneManager.Instance.defaultEnemyPrefab;
        }

        if (item.category == ChallengeData.SpawnableCategory.Civilian)
        {
            return DynamicZoneManager.Instance.defaultCivilianPrefab;
        }

        return null;
    }

    private static bool IsEnemyCategory(ChallengeData.SpawnableCategory category)
    {
        return category == ChallengeData.SpawnableCategory.Enemy || category == ChallengeData.SpawnableCategory.Boss;
    }

    private static bool IsLikelyEnemyObject(GameObject spawned, GameObject sourcePrefab, string itemName)
    {
        if (spawned == null && sourcePrefab == null) return false;

        if (spawned != null && spawned.CompareTag("Enemy")) return true;

        if (spawned != null)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0 && spawned.layer == enemyLayer)
            {
                return true;
            }
        }

        string combinedName = string.Empty;
        if (itemName != null) combinedName += itemName + " ";
        if (spawned != null) combinedName += spawned.name + " ";
        if (sourcePrefab != null) combinedName += sourcePrefab.name;

        return IsLikelyEnemyName(combinedName);
    }

    private static bool IsLikelyEnemyName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        string key = value.ToLowerInvariant();
        return key.Contains("enemy")
            || key.Contains("zombie")
            || key.Contains("soldier")
            || key.Contains("hostile")
            || key.Contains("boss")
            || key.Contains("elite")
            || key.Contains("patrol");
    }

    private bool TryGetSpawnPose(
        ActiveChallenge challenge,
        ChallengeData data,
        ChallengeData.SpawnableItem item,
        int spawnIndex,
        int spawnCount,
        SpawnRuntimeData runtime,
        out Vector3 position,
        out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        Transform point = null;
        bool hasTransformPoint = useTransformSpawnPoints && TryGetTransformSpawnPoint(challenge, data, item, out point);
        int attempts = Mathf.Max(1, maxNavMeshAttempts);

        if (hasTransformPoint)
        {
            attempts = 1;
        }

        for (int i = 0; i < attempts; i++)
        {
            Vector3 candidate = hasTransformPoint
                ? point.position + item.offset
                : GetProceduralSpawnPosition(challenge.position, item, spawnIndex, spawnCount);

            if (item.requireNavMesh && !TryProjectToNavMesh(candidate, out candidate))
            {
                continue;
            }

            if (minimumSpawnDistance > 0f && !IsFarEnoughFromExisting(candidate, runtime))
            {
                continue;
            }

            position = candidate;
            rotation = item.randomRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : Quaternion.Euler(item.fixedRotation);
            return true;
        }

        Vector3 fallback = hasTransformPoint
            ? point.position + item.offset
            : GetProceduralSpawnPosition(challenge.position, item, spawnIndex, spawnCount);

        if (!item.requireNavMesh || TryProjectToNavMesh(fallback, out fallback))
        {
            position = fallback;
            rotation = item.randomRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : Quaternion.Euler(item.fixedRotation);
            return true;
        }

        position = challenge.position + item.offset;
        return false;
    }

    private Vector3 GetNavMeshRadiusCandidate(Vector3 center)
    {
        float radius = Mathf.Max(0.1f, navMeshSpawnRadius);
        Vector2 random = Random.insideUnitCircle * radius;
        return center + new Vector3(random.x, 0f, random.y);
    }

    private bool TryGetTransformSpawnPoint(ActiveChallenge challenge, ChallengeData data, ChallengeData.SpawnableItem item, out Transform point)
    {
        point = null;

        List<Transform> candidates = new List<Transform>();

        if (item.customSpawnPoints != null && item.customSpawnPoints.Length > 0)
        {
            for (int i = 0; i < item.customSpawnPoints.Length; i++)
            {
                if (item.customSpawnPoints[i] != null)
                {
                    candidates.Add(item.customSpawnPoints[i]);
                }
            }
        }

        if (candidates.Count == 0 && data.sharedSpawnPoints != null && data.sharedSpawnPoints.Length > 0)
        {
            for (int i = 0; i < data.sharedSpawnPoints.Length; i++)
            {
                if (data.sharedSpawnPoints[i] != null)
                {
                    candidates.Add(data.sharedSpawnPoints[i]);
                }
            }
        }

        if (candidates.Count == 0 && DynamicZoneManager.Instance != null)
        {
            DynamicChallengeZone assignedZone = DynamicZoneManager.Instance.GetZoneForChallenge(challenge);
            if (assignedZone != null)
            {
                List<Transform> zonePoints = DynamicZoneManager.Instance.GetSpawnPointsForChallenge(challenge);
                for (int i = 0; i < zonePoints.Count; i++)
                {
                    if (zonePoints[i] != null)
                    {
                        candidates.Add(zonePoints[i]);
                    }
                }
            }
        }

        if (candidates.Count == 0) return false;

        point = candidates[Random.Range(0, candidates.Count)];
        return point != null;
    }

    private Vector3 GetProceduralSpawnPosition(Vector3 center, ChallengeData.SpawnableItem item, int spawnIndex, int spawnCount)
    {
        float radius = Mathf.Max(1f, item.spawnRadius > 0f ? item.spawnRadius : fallbackSpawnRadius);
        Vector3 offset = item.offset;

        switch (item.spawnLocation)
        {
            case ChallengeData.SpawnLocationType.AtCenter:
                return center + offset;

            case ChallengeData.SpawnLocationType.RandomOnEdge:
            {
                Vector2 edge = Random.insideUnitCircle.normalized * radius;
                return center + offset + new Vector3(edge.x, 0f, edge.y);
            }

            case ChallengeData.SpawnLocationType.AroundPerimeter:
            {
                float angle = (360f / Mathf.Max(1, spawnCount)) * spawnIndex;
                float radians = angle * Mathf.Deg2Rad;
                float jitter = Random.Range(-radius * 0.2f, radius * 0.2f);
                float radialDistance = Mathf.Max(1f, radius + jitter);
                return center + offset + new Vector3(Mathf.Cos(radians) * radialDistance, 0f, Mathf.Sin(radians) * radialDistance);
            }

            case ChallengeData.SpawnLocationType.Grid:
            {
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, spawnCount)));
                int row = spawnIndex / gridSize;
                int col = spawnIndex % gridSize;
                float spacing = Mathf.Max(2f, radius / Mathf.Max(1, gridSize - 1));
                float originOffset = (gridSize - 1) * spacing * 0.5f;
                return center + offset + new Vector3((col * spacing) - originOffset, 0f, (row * spacing) - originOffset);
            }

            case ChallengeData.SpawnLocationType.RandomInRadius:
            default:
            {
                Vector2 random = Random.insideUnitCircle * radius;
                return center + offset + new Vector3(random.x, 0f, random.y);
            }
        }
    }

    private bool TryProjectToNavMesh(Vector3 sourcePosition, out Vector3 projectedPosition)
    {
        projectedPosition = sourcePosition;
        if (NavMesh.SamplePosition(sourcePosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            projectedPosition = hit.position;
            return true;
        }

        return false;
    }

    private void ApplyPostSpawnPlacement(GameObject spawned, Vector3 intendedPosition, bool requireNavMesh)
    {
        if (spawned == null) return;

        Vector3 finalPosition = intendedPosition;
        if (requireNavMesh && TryProjectToNavMesh(finalPosition, out Vector3 projected))
        {
            finalPosition = projected;
        }

        NavMeshAgent agent = spawned.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
        {
            if (agent.enabled)
            {
                if (agent.isOnNavMesh)
                {
                    agent.Warp(finalPosition);
                }
                else if (TryProjectToNavMesh(finalPosition, out Vector3 warpPosition))
                {
                    spawned.transform.position = warpPosition;
                }
                else
                {
                    spawned.transform.position = finalPosition;
                }

                return;
            }
        }

        spawned.transform.position = finalPosition;
    }

    private bool IsFarEnoughFromExisting(Vector3 candidate, SpawnRuntimeData runtime)
    {
        float minSqr = minimumSpawnDistance * minimumSpawnDistance;

        for (int i = 0; i < runtime.spawnedObjects.Count; i++)
        {
            GameObject existing = runtime.spawnedObjects[i];
            if (existing == null) continue;

            Vector3 delta = candidate - existing.transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < minSqr)
            {
                return false;
            }
        }

        return true;
    }

    private void TryAssignDynamicZone(ActiveChallenge challenge, ChallengeData data, SpawnRuntimeData runtime)
    {
        if (!useDynamicZones || DynamicZoneManager.Instance == null) return;

        DynamicChallengeZone currentZone = DynamicZoneManager.Instance.GetZoneForChallenge(challenge);
        if (currentZone != null)
        {
            challenge.position = currentZone.GetCenterPosition();
            return;
        }

        DynamicChallengeZone selectedZone = assignNearestDynamicZone
            ? DynamicZoneManager.Instance.GetClosestAvailableZone(challenge.position, data.challengeType)
            : DynamicZoneManager.Instance.GetRandomAvailableZone(data.challengeType);

        if (selectedZone == null) return;

        DynamicZoneManager.Instance.AssignZoneToChallenge(challenge, selectedZone);
        challenge.position = selectedZone.GetCenterPosition();
        runtime.assignedZone = true;
    }

    private Transform GetSpawnParent()
    {
        if (!parentSpawnedUnderSpawner) return null;
        EnsureRuntimeRoot();
        return _runtimeRoot;
    }

    private void EnsureRuntimeRoot()
    {
        if (_runtimeRoot != null) return;

        Transform existing = transform.Find(runtimeRootName);
        if (existing != null)
        {
            _runtimeRoot = existing;
            return;
        }

        GameObject root = new GameObject(runtimeRootName);
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        _runtimeRoot = root.transform;
    }
}
