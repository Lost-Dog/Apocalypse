using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawns random objects from a prefab list at NavMesh positions within a radius ring
/// around the player. Uses object pooling per prefab. Active objects beyond DespawnDistance
/// are automatically returned to the pool.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const int MaxNavMeshSampleAttempts = 15;
    private const float NavMeshSampleRadius = 5f;

    [Header("Prefabs")]
    [Tooltip("Pool of prefabs to spawn from. One is chosen at random per spawn.")]
    public List<GameObject> spawnPrefabs = new List<GameObject>();

    [Header("Pool Settings")]
    [Tooltip("Number of pre-warmed instances created per prefab on Start.")]
    public int initialPoolSizePerPrefab = 5;

    [Header("Spawn Settings")]
    [Tooltip("Maximum number of objects that can be active in the scene simultaneously.")]
    public int maxItemsInScene = 20;
    [Tooltip("Seconds between each spawn attempt.")]
    public float spawnInterval = 3f;
    [Tooltip("Minimum distance from the player at which objects may spawn.")]
    public float minSpawnRadius = 10f;
    [Tooltip("Maximum distance from the player at which objects may spawn.")]
    public float maxSpawnRadius = 40f;

    [Header("Despawn Settings")]
    [Tooltip("Objects further than this distance from the player are returned to the pool.")]
    public float despawnDistance = 80f;
    [Tooltip("How often (in seconds) the despawn distance check runs.")]
    public float despawnCheckInterval = 2f;

    [Header("Debug")]
    public bool showGizmos = true;
    public bool logEvents = false;

    // ── Pooling ──────────────────────────────────────────────────────────────

    private class PrefabPool
    {
        public GameObject prefab;
        public Queue<GameObject> available = new Queue<GameObject>();
    }

    private Dictionary<GameObject, PrefabPool> pools = new Dictionary<GameObject, PrefabPool>();

    // Maps active instance → its source prefab so we can return it to the right pool.
    private Dictionary<GameObject, GameObject> activeInstanceToPrefab = new Dictionary<GameObject, GameObject>();

    [Header("Player Reference")]
    [Tooltip("Assign the player's root Transform here. If left empty the spawner will search " +
             "for the first GameObject tagged 'Player'.")]
    public Transform playerOverride;

    // ── State ─────────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private Coroutine spawnCoroutine;
    private float despawnTimer;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        FindPlayer();
        InitializePools();
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnCheckInterval)
        {
            despawnTimer = 0f;
            CheckDespawnDistance();
        }
    }

    private void OnDestroy()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void FindPlayer()
    {
        // Explicit override takes priority — drag the player here in the Inspector.
        if (playerOverride != null)
        {
            playerTransform = playerOverride;
            return;
        }

        // Fall back to the first tagged player.
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(PlayerTag);
        foreach (GameObject candidate in candidates)
        {
            playerTransform = candidate.transform;
            return;
        }

        // Last resort: just take the first tagged object.
        if (candidates.Length > 0)
        {
            playerTransform = candidates[0].transform;
        }
        else
        {
            Debug.LogWarning($"[ObjectSpawner] No GameObject tagged '{PlayerTag}' found in scene.", this);
        }
    }

    private void InitializePools()
    {
        foreach (GameObject prefab in spawnPrefabs)
        {
            if (prefab == null) continue;
            if (pools.ContainsKey(prefab)) continue;

            PrefabPool pool = new PrefabPool { prefab = prefab };

            for (int i = 0; i < initialPoolSizePerPrefab; i++)
            {
                GameObject instance = CreateInstance(prefab);
                pool.available.Enqueue(instance);
            }

            pools[prefab] = pool;

            if (logEvents)
                Debug.Log($"[ObjectSpawner] Pool created for '{prefab.name}' ({initialPoolSizePerPrefab} instances).", this);
        }
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, transform);
        instance.SetActive(false);
        return instance;
    }

    // ── Spawn Loop ────────────────────────────────────────────────────────────

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (playerTransform == null) return;
        if (spawnPrefabs.Count == 0) return;
        if (activeInstanceToPrefab.Count >= maxItemsInScene) return;

        if (!TryGetSpawnPosition(out Vector3 spawnPosition)) return;

        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];
        GameObject instance = GetFromPool(prefab);
        if (instance == null) return;

        instance.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        instance.SetActive(true);
        activeInstanceToPrefab[instance] = prefab;

        if (logEvents)
            Debug.Log($"[ObjectSpawner] Spawned '{prefab.name}' at {spawnPosition}. Active: {activeInstanceToPrefab.Count}/{maxItemsInScene}.", this);
    }

    // ── NavMesh Sampling ──────────────────────────────────────────────────────

    private bool TryGetSpawnPosition(out Vector3 result)
    {
        Vector3 playerPos = playerTransform.position;

        for (int i = 0; i < MaxNavMeshSampleAttempts; i++)
        {
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector2 disc = Random.insideUnitCircle.normalized;
            Vector3 candidate = playerPos + new Vector3(disc.x, 0f, disc.y) * radius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        if (logEvents)
            Debug.LogWarning("[ObjectSpawner] Could not find a valid NavMesh position after max attempts.", this);

        result = Vector3.zero;
        return false;
    }

    // ── Despawn ───────────────────────────────────────────────────────────────

    private void CheckDespawnDistance()
    {
        if (playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        var toReturn = new List<GameObject>();

        foreach (KeyValuePair<GameObject, GameObject> pair in activeInstanceToPrefab)
        {
            if (pair.Key == null) continue;

            if (Vector3.Distance(playerPos, pair.Key.transform.position) > despawnDistance)
                toReturn.Add(pair.Key);
        }

        foreach (GameObject instance in toReturn)
            ReturnToPool(instance);
    }

    /// <summary>
    /// Manually returns an active instance to the pool.
    /// Call this from external scripts when the object is consumed or destroyed logically.
    /// </summary>
    public void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;

        if (!activeInstanceToPrefab.TryGetValue(instance, out GameObject sourcePrefab))
        {
            if (logEvents)
                Debug.LogWarning($"[ObjectSpawner] ReturnToPool called for an untracked instance '{instance.name}'.", this);
            return;
        }

        instance.SetActive(false);

        activeInstanceToPrefab.Remove(instance);

        if (pools.TryGetValue(sourcePrefab, out PrefabPool pool))
            pool.available.Enqueue(instance);

        if (logEvents)
            Debug.Log($"[ObjectSpawner] '{instance.name}' returned to pool. Active: {activeInstanceToPrefab.Count}/{maxItemsInScene}.", this);
    }

    /// <summary>
    /// Returns all active instances to their pools immediately.
    /// </summary>
    public void ReturnAllToPool()
    {
        var instances = new List<GameObject>(activeInstanceToPrefab.Keys);
        foreach (GameObject instance in instances)
            ReturnToPool(instance);
    }

    // ── Pool Retrieval ────────────────────────────────────────────────────────

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out PrefabPool pool))
        {
            Debug.LogWarning($"[ObjectSpawner] No pool found for prefab '{prefab.name}'. Skipping spawn.", this);
            return null;
        }

        if (pool.available.Count > 0)
            return pool.available.Dequeue();

        // Pool exhausted — grow it by one.
        if (logEvents)
            Debug.Log($"[ObjectSpawner] Pool for '{prefab.name}' exhausted — expanding.", this);

        return CreateInstance(prefab);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        Gizmos.color = Color.green;
        DrawGizmoCircle(center, minSpawnRadius);

        Gizmos.color = Color.yellow;
        DrawGizmoCircle(center, maxSpawnRadius);

        Gizmos.color = Color.red;
        DrawGizmoCircle(center, despawnDistance);
    }

    private static void DrawGizmoCircle(Vector3 center, float radius, int segments = 32)
    {
        float step = 360f / segments * Mathf.Deg2Rad;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    // ── Public Query ──────────────────────────────────────────────────────────

    /// <summary>Returns the number of currently active spawned objects.</summary>
    public int ActiveCount => activeInstanceToPrefab.Count;

}
