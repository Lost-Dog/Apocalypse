using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public int minEnemies = 2;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;

    [Header("Respawn Settings")]
    public bool shouldRespawn = true;
    public float respawnTime = 300f;

    [Header("Difficulty")]
    public int zoneLevel = 1;
    public bool scaleWithPlayerLevel = false;

    [Header("Spawn Behavior")]
    public bool spawnOnStart = true;
    public bool useNavMesh = true;

    [Header("Object Pooling")]
    [Tooltip("Use object pooling for enemies (recommended for performance)")]
    public bool useObjectPooling = true;
    [Tooltip("Initial pool size per enemy type")]
    public int poolSizePerType = 5;

    [Header("Debug")]
    public bool showSpawnRadius = true;
    public Color gizmoColor = Color.red;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private float respawnTimer;
    private bool hasSpawned = false;
    private Dictionary<GameObject, ObjectPool> enemyPools = new Dictionary<GameObject, ObjectPool>();
    
    private void Start()
    {
        if (useObjectPooling)
        {
            InitializeEnemyPools();
        }

        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// Initialize object pools for each enemy prefab
    /// </summary>
    private void InitializeEnemyPools()
    {
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            GameObject prefab = enemyPrefabs[i];
            if (prefab == null) continue;

            // Create pool for this enemy type
            GameObject poolObj = new GameObject($"Pool_{prefab.name}");
            poolObj.transform.SetParent(transform);

            ObjectPool pool = poolObj.AddComponent<ObjectPool>();
            pool.prefab = prefab;
            pool.initialPoolSize = poolSizePerType;
            pool.maxPoolSize = poolSizePerType * 3; // Allow growth up to 3x initial size
            pool.canGrow = true;

            enemyPools[prefab] = pool;
        }

        Debug.Log($"[EnemySpawner] Initialized {enemyPools.Count} enemy pools");
    }
    
    private void Update()
    {
        if (!shouldRespawn || !hasSpawned) return;
        
        if (AllEnemiesDefeated())
        {
            respawnTimer -= Time.deltaTime;
            
            if (respawnTimer <= 0f)
            {
                SpawnEnemies();
            }
        }
    }
    
    public void SpawnEnemies()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"No enemy prefabs assigned to spawner: {gameObject.name}");
            return;
        }
        
        ClearDeadEnemies();
        
        int count = Random.Range(minEnemies, maxEnemies + 1);
        int effectiveLevel = GetEffectiveZoneLevel();
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemy = SpawnEnemy(spawnPos, effectiveLevel);
            
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
            }
        }
        
        hasSpawned = true;
        Debug.Log($"Spawned {count} enemies at level {effectiveLevel}");
    }
    
    private GameObject SpawnEnemy(Vector3 position, int level)
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        GameObject enemy;

        // OPTIMIZATION: Use object pooling if enabled
        if (useObjectPooling && enemyPools.TryGetValue(prefab, out ObjectPool pool))
        {
            enemy = pool.Get(position, Quaternion.identity);

            if (enemy == null)
            {
                Debug.LogWarning($"[EnemySpawner] Pool exhausted for {prefab.name}, falling back to Instantiate");
                enemy = Instantiate(prefab, position, Quaternion.identity);
            }
        }
        else
        {
            enemy = Instantiate(prefab, position, Quaternion.identity);
        }

        ApplyDifficultyScaling(enemy, level);

        return enemy;
    }
    
    private void ApplyDifficultyScaling(GameObject enemy, int level)
    {
        DifficultyScaler scaler = enemy.GetComponent<DifficultyScaler>();
        if (scaler == null)
        {
            scaler = enemy.AddComponent<DifficultyScaler>();
        }
        
        scaler.ApplyScaling(level);
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPosition = Vector3.zero;
        bool validPositionFound = false;
        int maxAttempts = 10;
        int attempts = 0;
        
        while (!validPositionFound && attempts < maxAttempts)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            randomPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            if (useNavMesh)
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(randomPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    randomPosition = hit.position;
                    validPositionFound = true;
                }
            }
            else
            {
                validPositionFound = true;
            }
            
            attempts++;
        }
        
        return randomPosition;
    }
    
    private int GetEffectiveZoneLevel()
    {
        if (scaleWithPlayerLevel && GameManager.Instance != null)
        {
            return Mathf.Max(zoneLevel, GameManager.Instance.currentPlayerLevel);
        }
        
        return zoneLevel;
    }
    
    private bool AllEnemiesDefeated()
    {
        // OPTIMIZED: Use for loop instead of foreach to avoid allocations
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null) return false;
        }

        return spawnedEnemies.Count > 0;
    }
    
    private void ClearDeadEnemies()
    {
        spawnedEnemies.RemoveAll(e => e == null);
        respawnTimer = respawnTime;
    }
    
    public void ClearAllEnemies()
    {
        // OPTIMIZED: Use for loop instead of foreach to avoid allocations
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
            {
                // OPTIMIZATION: Return to pool if using pooling, otherwise destroy
                if (useObjectPooling)
                {
                    ReturnEnemyToPool(spawnedEnemies[i]);
                }
                else
                {
                    Destroy(spawnedEnemies[i]);
                }
            }
        }

        spawnedEnemies.Clear();
    }

    /// <summary>
    /// Return an enemy to its appropriate pool
    /// </summary>
    private void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        // Find the pool that owns this enemy
        foreach (var kvp in enemyPools)
        {
            if (enemy.name.StartsWith(kvp.Key.name))
            {
                kvp.Value.Return(enemy);
                return;
            }
        }

        // If not found in any pool, just destroy it
        Debug.LogWarning($"[EnemySpawner] Could not find pool for {enemy.name}, destroying instead");
        Destroy(enemy);
    }
    
    private void OnDrawGizmos()
    {
        if (!showSpawnRadius) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
