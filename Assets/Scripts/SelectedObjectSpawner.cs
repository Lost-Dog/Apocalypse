using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Invector.vCharacterController.vActions;

/// <summary>
/// Spawns a single selected prefab (by index) at NavMesh positions within a radius ring
/// around the player. Uses per-prefab object pooling. Active objects beyond DespawnDistance
/// are automatically returned to the pool.
///
/// Set <see cref="selectedPrefabIndex"/> to pin a specific entry in <see cref="spawnPrefabs"/>,
/// or call <see cref="SetSelectedPrefab"/> at runtime to switch the active prefab.
/// </summary>
public class SelectedObjectSpawner : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const int MaxNavMeshSampleAttempts = 15;
    private const float NavMeshSampleRadius = 5f;

    [Header("Prefabs")]
    [Tooltip("List of candidate prefabs. Only the one at SelectedPrefabIndex is spawned.")]
    public List<GameObject> spawnPrefabs = new List<GameObject>();

    [Tooltip("Index into SpawnPrefabs of the prefab to spawn. Clamped at runtime.")]
    public int selectedPrefabIndex = 0;

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

    [Header("Player Reference")]
    [Tooltip("Assign the player's root Transform here. If left empty the spawner searches " +
             "for the first GameObject tagged 'Player' that has a vThirdPersonController.")]
    public Transform playerOverride;

    [Header("Debug")]
    public bool showGizmos = true;
    public bool logEvents = false;

    // ── Pooling ───────────────────────────────────────────────────────────────

    private class PrefabPool
    {
        public GameObject prefab;
        public Queue<GameObject> available = new Queue<GameObject>();
    }

    private Dictionary<GameObject, PrefabPool> _pools = new Dictionary<GameObject, PrefabPool>();

    // Maps active instance → its source prefab so we can return it to the correct pool.
    private Dictionary<GameObject, GameObject> _activeInstanceToPrefab = new Dictionary<GameObject, GameObject>();

    // ── State ─────────────────────────────────────────────────────────────────

    private Transform _playerTransform;
    private Coroutine _spawnCoroutine;
    private float _despawnTimer;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        FindPlayer();
        InitializePools();
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        _despawnTimer += Time.deltaTime;
        if (_despawnTimer >= despawnCheckInterval)
        {
            _despawnTimer = 0f;
            CheckDespawnDistance();
        }
    }

    private void OnDestroy()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void FindPlayer()
    {
        if (playerOverride != null)
        {
            _playerTransform = playerOverride;
            return;
        }

        GameObject[] candidates = GameObject.FindGameObjectsWithTag(PlayerTag);
        foreach (GameObject candidate in candidates)
        {
            if (candidate.GetComponent<Invector.vCharacterController.vThirdPersonController>() != null)
            {
                _playerTransform = candidate.transform;
                return;
            }
        }

        if (candidates.Length > 0)
        {
            _playerTransform = candidates[0].transform;
        }
        else
        {
            Debug.LogWarning($"[SelectedObjectSpawner] No GameObject tagged '{PlayerTag}' found in scene.", this);
        }
    }

    private void InitializePools()
    {
        foreach (GameObject prefab in spawnPrefabs)
        {
            if (prefab == null) continue;
            if (_pools.ContainsKey(prefab)) continue;

            PrefabPool pool = new PrefabPool { prefab = prefab };

            for (int i = 0; i < initialPoolSizePerPrefab; i++)
                pool.available.Enqueue(CreateInstance(prefab));

            _pools[prefab] = pool;

            if (logEvents)
                Debug.Log($"[SelectedObjectSpawner] Pool created for '{prefab.name}' ({initialPoolSizePerPrefab} instances).", this);
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
        if (_playerTransform == null) return;
        if (spawnPrefabs.Count == 0) return;
        if (_activeInstanceToPrefab.Count >= maxItemsInScene) return;

        GameObject prefab = GetSelectedPrefab();
        if (prefab == null) return;

        if (!TryGetSpawnPosition(out Vector3 spawnPosition)) return;

        GameObject instance = GetFromPool(prefab);
        if (instance == null) return;

        instance.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        instance.SetActive(true);
        _activeInstanceToPrefab[instance] = prefab;

        if (logEvents)
            Debug.Log($"[SelectedObjectSpawner] Spawned '{prefab.name}' at {spawnPosition}. Active: {_activeInstanceToPrefab.Count}/{maxItemsInScene}.", this);
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    private GameObject GetSelectedPrefab()
    {
        if (spawnPrefabs.Count == 0) return null;

        int index = Mathf.Clamp(selectedPrefabIndex, 0, spawnPrefabs.Count - 1);
        GameObject prefab = spawnPrefabs[index];

        if (prefab == null)
        {
            Debug.LogWarning($"[SelectedObjectSpawner] Prefab at index {index} is null.", this);
            return null;
        }

        return prefab;
    }

    /// <summary>
    /// Switches the active prefab at runtime by index and returns all instances of the
    /// previous selection to the pool so the scene refreshes cleanly.
    /// </summary>
    public void SetSelectedPrefab(int index)
    {
        int clamped = Mathf.Clamp(index, 0, spawnPrefabs.Count - 1);
        if (clamped == selectedPrefabIndex) return;

        ReturnAllToPool();
        selectedPrefabIndex = clamped;

        if (logEvents)
            Debug.Log($"[SelectedObjectSpawner] Selected prefab switched to index {clamped} ('{spawnPrefabs[clamped]?.name}').", this);
    }

    // ── Pool Retrieval ────────────────────────────────────────────────────────

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out PrefabPool pool))
        {
            Debug.LogWarning($"[SelectedObjectSpawner] No pool found for prefab '{prefab.name}'. Skipping spawn.", this);
            return null;
        }

        if (pool.available.Count > 0)
            return pool.available.Dequeue();

        // Pool exhausted — grow it by one.
        if (logEvents)
            Debug.Log($"[SelectedObjectSpawner] Pool for '{prefab.name}' exhausted — expanding.", this);

        return CreateInstance(prefab);
    }

    // ── NavMesh Sampling ──────────────────────────────────────────────────────

    private bool TryGetSpawnPosition(out Vector3 result)
    {
        Vector3 playerPos = _playerTransform.position;

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
            Debug.LogWarning("[SelectedObjectSpawner] Could not find a valid NavMesh position after max attempts.", this);

        result = Vector3.zero;
        return false;
    }

    // ── Despawn ───────────────────────────────────────────────────────────────

    private void CheckDespawnDistance()
    {
        if (_playerTransform == null) return;

        Vector3 playerPos = _playerTransform.position;
        var toReturn = new List<GameObject>();

        foreach (KeyValuePair<GameObject, GameObject> pair in _activeInstanceToPrefab)
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
    /// Call this from external scripts when the object is consumed or collected.
    /// </summary>
    public void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;

        if (!_activeInstanceToPrefab.TryGetValue(instance, out GameObject sourcePrefab))
        {
            if (logEvents)
                Debug.LogWarning($"[SelectedObjectSpawner] ReturnToPool called for untracked instance '{instance.name}'.", this);
            return;
        }

        instance.SetActive(false);

        // Reset Invector trigger state so re-pooled collectables are interactable again.
        var trigger = instance.GetComponent<vTriggerGenericAction>();
        if (trigger != null)
            trigger.CanDoAction = true;

        _activeInstanceToPrefab.Remove(instance);

        if (_pools.TryGetValue(sourcePrefab, out PrefabPool pool))
            pool.available.Enqueue(instance);

        if (logEvents)
            Debug.Log($"[SelectedObjectSpawner] '{instance.name}' returned to pool. Active: {_activeInstanceToPrefab.Count}/{maxItemsInScene}.", this);
    }

    /// <summary>Returns all active instances to their pools immediately.</summary>
    public void ReturnAllToPool()
    {
        var instances = new List<GameObject>(_activeInstanceToPrefab.Keys);
        foreach (GameObject instance in instances)
            ReturnToPool(instance);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 center = _playerTransform != null ? _playerTransform.position : transform.position;

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
    public int ActiveCount => _activeInstanceToPrefab.Count;

    /// <summary>Returns the prefab currently selected for spawning, or null if the list is empty.</summary>
    public GameObject SelectedPrefab => GetSelectedPrefab();
}
