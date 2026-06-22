using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class DogSpawner : MonoBehaviour
{
    public static DogSpawner Instance { get; private set; }

    [Header("Dog Prefabs")]
    [SerializeField] private List<GameObject> dogPrefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    [SerializeField] private int maxActiveDogs = 3;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private float spawnInterval = 3f;

    [Header("Distance Settings")]
    [SerializeField] private float minSpawnDistance = 25f;
    [SerializeField] private float maxSpawnDistance = 90f;
    [SerializeField] private float deactivateDistance = 110f;

    [Header("NavMesh Settings")]
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float navMeshSampleDistance = 5f;

    [Header("Performance Settings")]
    [SerializeField] private float distanceCheckInterval = 1f;
    [SerializeField] private bool enableAutoSpawn = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool logSpawnEvents = false;

    private Dictionary<GameObject, Pool> dogPools = new Dictionary<GameObject, Pool>();
    private List<PooledDog> activeDogs = new List<PooledDog>();
    private List<GameObject> availablePrefabsForSpawn = new List<GameObject>();
    private Transform playerTransform;
    private float spawnTimer;
    private float distanceCheckTimer;

    private class Pool
    {
        public GameObject prefab;
        public Queue<GameObject> available = new Queue<GameObject>();
        public List<GameObject> inUse = new List<GameObject>();
    }

    private class PooledDog
    {
        public GameObject gameObject;
        public Transform transform;
        public GameObject originalPrefab;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[DogSpawner] Multiple DogSpawner instances detected.");
            Instance = this;
        }
    }

    private void Start()
    {
        InitializePlayer();
        InitializePools();

        if (enableAutoSpawn)
        {
            SpawnInitialDogs();
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            InitializePlayer();
            return;
        }

        if (enableAutoSpawn)
        {
            UpdateAutoSpawn();
        }

        UpdateDistanceChecks();
    }

    private void InitializePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[DogSpawner] Player not found. Make sure the player has the 'Player' tag.");
        }
    }

    private void InitializePools()
    {
        if (dogPrefabs.Count == 0)
        {
            Debug.LogWarning("[DogSpawner] No dog prefabs assigned!");
            return;
        }

        dogPrefabs = DeduplicatePrefabs(dogPrefabs);

        foreach (GameObject prefab in dogPrefabs)
        {
            Pool pool = new Pool { prefab = prefab };
            int poolSizePerPrefab = Mathf.CeilToInt((float)initialPoolSize / dogPrefabs.Count);

            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab, Vector3.down * 1000f, Quaternion.identity, transform);
                obj.name = $"{prefab.name}_{i}";
                obj.SetActive(false);
                pool.available.Enqueue(obj);
            }

            dogPools[prefab] = pool;

            if (logSpawnEvents)
                Debug.Log($"[DogSpawner] Initialized pool for '{prefab.name}' with {poolSizePerPrefab} instances.");
        }

        RefillAvailablePrefabs();
    }

    /// <summary>Returns a deduplicated list with null entries removed.</summary>
    private List<GameObject> DeduplicatePrefabs(List<GameObject> source)
    {
        var seen = new HashSet<GameObject>();
        var result = new List<GameObject>(source.Count);

        foreach (GameObject prefab in source)
        {
            if (prefab == null) continue;

            if (!seen.Add(prefab))
            {
                Debug.LogWarning($"[DogSpawner] Duplicate prefab entry '{prefab.name}' removed. Check the dogPrefabs list in the Inspector.");
                continue;
            }

            result.Add(prefab);
        }

        return result;
    }

    private void SpawnInitialDogs()
    {
        int toSpawn = Mathf.Min(maxActiveDogs, initialPoolSize);

        for (int i = 0; i < toSpawn; i++)
        {
            SpawnRandomDog();
        }

        if (logSpawnEvents)
            Debug.Log($"[DogSpawner] Spawned {toSpawn} initial dogs.");
    }

    private void UpdateAutoSpawn()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            if (activeDogs.Count < maxActiveDogs)
            {
                SpawnRandomDog();
            }
        }
    }

    private void UpdateDistanceChecks()
    {
        distanceCheckTimer += Time.deltaTime;

        if (distanceCheckTimer >= distanceCheckInterval)
        {
            distanceCheckTimer = 0f;
            CheckDogDistances();
        }
    }

    private void CheckDogDistances()
    {
        if (playerTransform == null) return;

        for (int i = activeDogs.Count - 1; i >= 0; i--)
        {
            PooledDog dog = activeDogs[i];

            if (dog.gameObject == null || dog.transform == null)
            {
                activeDogs.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(playerTransform.position, dog.transform.position);

            if (distance > deactivateDistance)
            {
                ReturnToPool(dog);

                if (logSpawnEvents)
                    Debug.Log($"[DogSpawner] Deactivated '{dog.gameObject.name}' (distance: {distance:F1}m).");
            }
        }
    }

    /// <summary>Spawns a random dog at a valid NavMesh position near the player.</summary>
    public GameObject SpawnRandomDog()
    {
        if (dogPrefabs.Count == 0 || playerTransform == null)
            return null;

        if (availablePrefabsForSpawn.Count == 0)
            RefillAvailablePrefabs();

        if (!FindValidSpawnPosition(out Vector3 spawnPosition))
            return null;

        int randomIndex = Random.Range(0, availablePrefabsForSpawn.Count);
        GameObject selectedPrefab = availablePrefabsForSpawn[randomIndex];
        availablePrefabsForSpawn.RemoveAt(randomIndex);

        return SpawnDog(selectedPrefab, spawnPosition);
    }

    private void RefillAvailablePrefabs()
    {
        availablePrefabsForSpawn.Clear();

        foreach (GameObject prefab in dogPrefabs)
        {
            if (prefab != null && dogPools.ContainsKey(prefab))
                availablePrefabsForSpawn.Add(prefab);
        }

        if (logSpawnEvents)
            Debug.Log($"[DogSpawner] Refilled prefab list with {availablePrefabsForSpawn.Count} unique prefabs.");
    }

    /// <summary>Spawns a specific dog prefab at the given world position.</summary>
    public GameObject SpawnDog(GameObject prefab, Vector3 position)
    {
        if (prefab == null || !dogPools.ContainsKey(prefab))
        {
            Debug.LogWarning($"[DogSpawner] Prefab '{prefab?.name}' is not in the pool!");
            return null;
        }

        Pool pool = dogPools[prefab];
        GameObject obj;

        if (pool.available.Count > 0)
        {
            obj = pool.available.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, position, Quaternion.identity, transform);
            obj.name = $"{prefab.name}_Extra_{pool.inUse.Count}";

            if (logSpawnEvents)
                Debug.Log($"[DogSpawner] Pool exhausted — created new instance of '{prefab.name}'.");
        }

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        obj.SetActive(true);

        pool.inUse.Add(obj);

        PooledDog pooledDog = new PooledDog
        {
            gameObject = obj,
            transform = obj.transform,
            originalPrefab = prefab
        };
        activeDogs.Add(pooledDog);

        if (logSpawnEvents)
            Debug.Log($"[DogSpawner] Spawned '{obj.name}' at {position}.");

        return obj;
    }

    /// <summary>Returns a specific dog GameObject back to the pool.</summary>
    public void DespawnDog(GameObject dog)
    {
        if (dog == null) return;

        PooledDog pooledDog = activeDogs.Find(d => d.gameObject == dog);
        if (pooledDog != null)
        {
            ReturnToPool(pooledDog);
        }
    }

    private void ReturnToPool(PooledDog dog)
    {
        if (dog == null || dog.gameObject == null) return;

        dog.gameObject.SetActive(false);
        activeDogs.Remove(dog);

        if (dogPools.ContainsKey(dog.originalPrefab))
        {
            Pool pool = dogPools[dog.originalPrefab];
            pool.inUse.Remove(dog.gameObject);
            pool.available.Enqueue(dog.gameObject);
        }
    }

    private bool FindValidSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;

        if (playerTransform == null)
            return false;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            Vector3 randomDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 targetPosition = playerTransform.position + randomDirection * distance;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
        }

        if (logSpawnEvents)
            Debug.LogWarning("[DogSpawner] Failed to find a valid spawn position after max attempts.");

        return false;
    }

    // ── Public control API ────────────────────────────────────────────────────

    /// <summary>Despawns all currently active dogs and returns them to their pools.</summary>
    public void DespawnAllDogs()
    {
        for (int i = activeDogs.Count - 1; i >= 0; i--)
        {
            ReturnToPool(activeDogs[i]);
        }

        activeDogs.Clear();
    }

    /// <summary>Returns the number of currently active dogs.</summary>
    public int GetActiveDogCount() => activeDogs.Count;

    /// <summary>Returns the total number of dogs sitting idle in all pools.</summary>
    public int GetPooledDogCount()
    {
        int total = 0;
        foreach (Pool pool in dogPools.Values)
            total += pool.available.Count;
        return total;
    }

    public void SetMaxActiveDogs(int count) => maxActiveDogs = Mathf.Max(0, count);
    public void SetSpawnInterval(float interval) => spawnInterval = Mathf.Max(0.1f, interval);

    public void SetMinSpawnDistance(float distance)
    {
        minSpawnDistance = Mathf.Max(0f, distance);
        if (minSpawnDistance > maxSpawnDistance)
            maxSpawnDistance = minSpawnDistance + 10f;
    }

    public void SetMaxSpawnDistance(float distance) => maxSpawnDistance = Mathf.Max(minSpawnDistance, distance);
    public void SetDeactivateDistance(float distance) => deactivateDistance = Mathf.Max(maxSpawnDistance, distance);
    public void SetAutoSpawnEnabled(bool isEnabled) => enableAutoSpawn = isEnabled;

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || playerTransform == null)
            return;

        Gizmos.color = Color.yellow;
        DrawCircle(playerTransform.position, minSpawnDistance);

        Gizmos.color = Color.green;
        DrawCircle(playerTransform.position, maxSpawnDistance);

        Gizmos.color = Color.red;
        DrawCircle(playerTransform.position, deactivateDistance);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        const int Segments = 32;
        float angleStep = 360f / Segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= Segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
