using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Spawns loot at runtime using object pooling and NavMesh positioning
/// Spawns loot at random positions around the player on the NavMesh
/// </summary>
public class RuntimeLootSpawner : MonoBehaviour
{
    public static RuntimeLootSpawner Instance { get; private set; }

    [Header("Loot Prefabs")]
    [SerializeField] private List<GameObject> lootPrefabs = new List<GameObject>();
    
    [Header("FX Settings")]
    [Tooltip("First FX object to spawn with loot (e.g., glow effect)")]
    [SerializeField] private GameObject fx1Prefab;
    [Tooltip("Second FX object to spawn with loot (e.g., particle effect)")]
    [SerializeField] private GameObject fx2Prefab;
    [Tooltip("Attach FX as children of loot objects")]
    [SerializeField] private bool attachFXAsChildren = true;
    
    [Header("Spawn Settings")]
    [SerializeField] private int maxActiveLoot = 50;
    [SerializeField] private int initialPoolSize = 20;
    
    [Header("Distance Settings")]
    [SerializeField] private float minSpawnRadius = 10f;
    [SerializeField] private float maxSpawnRadius = 50f;
    [Tooltip("Distance from player at which loot is automatically despawned")]
    [SerializeField] private float despawnDistance = 100f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float navMeshSampleDistance = 5f;
    [SerializeField] private LayerMask groundLayer = -1;
    
    [Header("Spawn Height")]
    [SerializeField] private float spawnHeight = 1f;
    [Tooltip("Use raycast to find ground level")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private float raycastDistance = 10f;
    
    [Header("Performance")]
    [SerializeField] private float despawnCheckInterval = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool logSpawnEvents = false;
    
    private Dictionary<GameObject, Pool> lootPools = new Dictionary<GameObject, Pool>();
    private List<PooledLoot> activeLoot = new List<PooledLoot>();
    private Transform playerTransform;
    private float despawnCheckTimer;
    
    private class Pool
    {
        public GameObject prefab;
        public Queue<GameObject> available = new Queue<GameObject>();
        public List<GameObject> inUse = new List<GameObject>();
    }
    
    private class PooledLoot
    {
        public GameObject gameObject;
        public Transform transform;
        public GameObject originalPrefab;
        public GameObject fx1Instance;
        public GameObject fx2Instance;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePlayer();
        InitializePools();
    }

    private void Update()
    {
        despawnCheckTimer += Time.deltaTime;
        if (despawnCheckTimer >= despawnCheckInterval)
        {
            despawnCheckTimer = 0f;
            CheckForDistantLoot();
        }
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
            Debug.LogWarning("RuntimeLootSpawner: Player not found. Make sure Player has 'Player' tag.");
        }
    }

    private void InitializePools()
    {
        if (lootPrefabs.Count == 0)
        {
            Debug.LogWarning("RuntimeLootSpawner: No loot prefabs assigned!");
            return;
        }

        foreach (GameObject prefab in lootPrefabs)
        {
            if (prefab == null) continue;
            
            Pool pool = new Pool { prefab = prefab };
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.available.Enqueue(obj);
            }
            
            lootPools[prefab] = pool;
            
            if (logSpawnEvents)
                Debug.Log($"Initialized pool for {prefab.name} with {initialPoolSize} objects");
        }
    }

    /// <summary>
    /// Spawns loot at a random position on NavMesh within radius from player
    /// </summary>
    /// <param name="minRadius">Minimum distance from player</param>
    /// <param name="maxRadius">Maximum distance from player</param>
    /// <returns>The spawned loot GameObject, or null if spawn failed</returns>
    public GameObject SpawnLoot(float minRadius = -1, float maxRadius = -1)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Cannot spawn loot: Player not found");
            return null;
        }

        if (lootPrefabs.Count == 0)
        {
            Debug.LogWarning("Cannot spawn loot: No loot prefabs assigned");
            return null;
        }

        if (activeLoot.Count >= maxActiveLoot)
        {
            if (logSpawnEvents)
                Debug.Log($"Max active loot reached ({maxActiveLoot})");
            return null;
        }

        // Use provided radii or defaults
        float useMinRadius = minRadius > 0 ? minRadius : minSpawnRadius;
        float useMaxRadius = maxRadius > 0 ? maxRadius : maxSpawnRadius;

        Vector3 spawnPosition;
        if (FindSpawnPositionNearPlayer(playerTransform.position, useMinRadius, useMaxRadius, out spawnPosition))
        {
            GameObject prefab = lootPrefabs[Random.Range(0, lootPrefabs.Count)];
            return SpawnLootAtPosition(spawnPosition, prefab);
        }

        if (logSpawnEvents)
            Debug.LogWarning("Failed to find valid spawn position for loot");
        
        return null;
    }

    /// <summary>
    /// Spawns loot at a specific position
    /// </summary>
    public GameObject SpawnLootAtPosition(Vector3 position, GameObject prefab = null)
    {
        if (prefab == null)
        {
            if (lootPrefabs.Count == 0)
            {
                Debug.LogWarning("No loot prefabs available");
                return null;
            }
            prefab = lootPrefabs[Random.Range(0, lootPrefabs.Count)];
        }

        if (!lootPools.ContainsKey(prefab))
        {
            Debug.LogWarning($"Prefab {prefab.name} not in pool");
            return null;
        }

        GameObject lootObj = GetFromPool(prefab);
        if (lootObj == null) return null;

        // Snap to ground if enabled
        if (snapToGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * raycastDistance, Vector3.down, out hit, raycastDistance * 2, groundLayer))
            {
                position = hit.point + Vector3.up * spawnHeight;
            }
        }
        else
        {
            position.y += spawnHeight;
        }

        lootObj.transform.position = position;
        lootObj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        lootObj.SetActive(true);

        // Spawn FX objects
        GameObject fx1 = null;
        GameObject fx2 = null;
        
        if (fx1Prefab != null)
        {
            fx1 = Instantiate(fx1Prefab);
            if (attachFXAsChildren)
            {
                fx1.transform.SetParent(lootObj.transform);
                fx1.transform.localPosition = Vector3.zero;
                fx1.transform.localRotation = Quaternion.identity;
            }
            else
            {
                fx1.transform.position = position;
            }
            fx1.SetActive(true);
        }
        
        if (fx2Prefab != null)
        {
            fx2 = Instantiate(fx2Prefab);
            if (attachFXAsChildren)
            {
                fx2.transform.SetParent(lootObj.transform);
                fx2.transform.localPosition = Vector3.zero;
                fx2.transform.localRotation = Quaternion.identity;
            }
            else
            {
                fx2.transform.position = position;
            }
            fx2.SetActive(true);
        }

        PooledLoot pooledLoot = new PooledLoot
        {
            gameObject = lootObj,
            transform = lootObj.transform,
            originalPrefab = prefab,
            fx1Instance = fx1,
            fx2Instance = fx2
        };
        activeLoot.Add(pooledLoot);

        if (logSpawnEvents)
            Debug.Log($"Spawned loot {prefab.name} at {position}");

        return lootObj;
    }

    /// <summary>
    /// Spawns multiple loot items at random positions around player
    /// </summary>
    public List<GameObject> SpawnMultipleLoot(int count, float minRadius = -1, float maxRadius = -1)
    {
        List<GameObject> spawnedLoot = new List<GameObject>();
        
        for (int i = 0; i < count; i++)
        {
            GameObject loot = SpawnLoot(minRadius, maxRadius);
            if (loot != null)
            {
                spawnedLoot.Add(loot);
            }
        }

        return spawnedLoot;
    }

    /// <summary>
    /// Returns loot to the pool (despawns it)
    /// </summary>
    public void DespawnLoot(GameObject lootObj)
    {
        PooledLoot pooledLoot = activeLoot.Find(l => l.gameObject == lootObj);
        if (pooledLoot != null)
        {
            ReturnToPool(lootObj, pooledLoot.originalPrefab);
            activeLoot.Remove(pooledLoot);
        }
    }

    /// <summary>
    /// Despawns all active loot
    /// </summary>
    public void DespawnAllLoot()
    {
        for (int i = activeLoot.Count - 1; i >= 0; i--)
        {
            PooledLoot pooledLoot = activeLoot[i];
            ReturnToPool(pooledLoot.gameObject, pooledLoot.originalPrefab);
        }
        activeLoot.Clear();
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!lootPools.ContainsKey(prefab))
            return null;

        Pool pool = lootPools[prefab];

        GameObject obj;
        if (pool.available.Count > 0)
        {
            obj = pool.available.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
            if (logSpawnEvents)
                Debug.Log($"Pool expanded for {prefab.name}");
        }

        pool.inUse.Add(obj);
        return obj;
    }

    private void ReturnToPool(GameObject obj, GameObject prefab)
    {
        if (!lootPools.ContainsKey(prefab))
            return;

        Pool pool = lootPools[prefab];
        
        obj.SetActive(false);
        pool.inUse.Remove(obj);
        pool.available.Enqueue(obj);
    }

    private bool FindSpawnPositionNearPlayer(Vector3 center, float minRadius, float maxRadius, out Vector3 spawnPosition)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // Generate random position in annulus (ring) around player
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y) * radius;
            Vector3 testPosition = center + randomOffset;

            // Try to find valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(testPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    private void CheckForDistantLoot()
    {
        if (playerTransform == null) return;

        for (int i = activeLoot.Count - 1; i >= 0; i--)
        {
            PooledLoot loot = activeLoot[i];
            if (loot.gameObject == null) continue;

            float distance = Vector3.Distance(playerTransform.position, loot.transform.position);
            if (distance > despawnDistance)
            {
                if (logSpawnEvents)
                    Debug.Log($"Despawning distant loot at {loot.transform.position}");
                
                ReturnToPool(loot.gameObject, loot.originalPrefab);
                activeLoot.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || playerTransform == null) return;

        // Draw spawn radius rings
        Gizmos.color = Color.green;
        DrawCircle(playerTransform.position, minSpawnRadius);
        
        Gizmos.color = Color.yellow;
        DrawCircle(playerTransform.position, maxSpawnRadius);
        
        Gizmos.color = Color.red;
        DrawCircle(playerTransform.position, despawnDistance);
    }

    private void DrawCircle(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    public int GetActiveLootCount() => activeLoot.Count;
    public int GetMaxActiveLoot() => maxActiveLoot;
}
