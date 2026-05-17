using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized manager for all object pools in the game.
/// Provides easy access to pools by prefab or by name.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
        public bool canGrow = true;
    }

    [Header("Pool Configurations")]
    [Tooltip("Define all pools to be created at startup")]
    public List<PoolConfig> poolConfigs = new List<PoolConfig>();

    [Header("Runtime Settings")]
    public bool createPoolsOnAwake = true;
    public Transform poolsParent;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private Dictionary<string, ObjectPool> poolsByName = new Dictionary<string, ObjectPool>();
    private Dictionary<GameObject, ObjectPool> poolsByPrefab = new Dictionary<GameObject, ObjectPool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (poolsParent == null)
        {
            poolsParent = transform;
        }

        if (createPoolsOnAwake)
        {
            CreateAllPools();
        }
    }

    /// <summary>
    /// Create all configured pools
    /// </summary>
    public void CreateAllPools()
    {
        for (int i = 0; i < poolConfigs.Count; i++)
        {
            PoolConfig config = poolConfigs[i];

            if (config.prefab == null)
            {
                Debug.LogWarning($"[PoolManager] Pool config at index {i} has no prefab assigned");
                continue;
            }

            CreatePool(config.poolName, config.prefab, config.initialSize, config.maxSize, config.canGrow);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[PoolManager] Created {poolsByName.Count} pools");
        }
    }

    /// <summary>
    /// Create a new pool with the specified configuration
    /// </summary>
    public ObjectPool CreatePool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 50, bool canGrow = true)
    {
        if (prefab == null)
        {
            Debug.LogError($"[PoolManager] Cannot create pool '{poolName}' - prefab is null");
            return null;
        }

        if (poolsByName.ContainsKey(poolName))
        {
            Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists");
            return poolsByName[poolName];
        }

        // Create pool GameObject
        GameObject poolObj = new GameObject($"Pool_{poolName}");
        poolObj.transform.SetParent(poolsParent);

        // Add and configure ObjectPool component
        ObjectPool pool = poolObj.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        pool.initialPoolSize = initialSize;
        pool.maxPoolSize = maxSize;
        pool.canGrow = canGrow;
        pool.showDebugInfo = showDebugInfo;

        // Register pool
        poolsByName[poolName] = pool;
        poolsByPrefab[prefab] = pool;

        if (showDebugInfo)
        {
            Debug.Log($"[PoolManager] Created pool '{poolName}' for prefab '{prefab.name}'");
        }

        return pool;
    }

    /// <summary>
    /// Get an object from a pool by pool name
    /// </summary>
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolsByName.TryGetValue(poolName, out ObjectPool pool))
        {
            Debug.LogError($"[PoolManager] Pool '{poolName}' not found");
            return null;
        }

        return pool.Get(position, rotation);
    }

    /// <summary>
    /// Get an object from a pool by prefab
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolsByPrefab.TryGetValue(prefab, out ObjectPool pool))
        {
            Debug.LogError($"[PoolManager] No pool found for prefab '{prefab.name}'");
            return null;
        }

        return pool.Get(position, rotation);
    }

    /// <summary>
    /// Get an object from a pool at position with default rotation
    /// </summary>
    public GameObject Spawn(string poolName, Vector3 position)
    {
        return Spawn(poolName, position, Quaternion.identity);
    }

    /// <summary>
    /// Return an object to its pool
    /// </summary>
    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        // Try to find which pool this object belongs to
        foreach (var pool in poolsByPrefab.Values)
        {
            if (obj.name.StartsWith(pool.prefab.name))
            {
                pool.Return(obj);
                return;
            }
        }

        Debug.LogWarning($"[PoolManager] Could not find pool for object '{obj.name}'");
    }

    /// <summary>
    /// Return object to pool after a delay
    /// </summary>
    public void DespawnAfterDelay(GameObject obj, float delay)
    {
        if (obj == null) return;
        StartCoroutine(DespawnCoroutine(obj, delay));
    }

    private System.Collections.IEnumerator DespawnCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(obj);
    }

    /// <summary>
    /// Get pool by name
    /// </summary>
    public ObjectPool GetPool(string poolName)
    {
        poolsByName.TryGetValue(poolName, out ObjectPool pool);
        return pool;
    }

    /// <summary>
    /// Get pool by prefab
    /// </summary>
    public ObjectPool GetPool(GameObject prefab)
    {
        poolsByPrefab.TryGetValue(prefab, out ObjectPool pool);
        return pool;
    }

    /// <summary>
    /// Check if a pool exists for the given name
    /// </summary>
    public bool HasPool(string poolName)
    {
        return poolsByName.ContainsKey(poolName);
    }

    /// <summary>
    /// Check if a pool exists for the given prefab
    /// </summary>
    public bool HasPool(GameObject prefab)
    {
        return poolsByPrefab.ContainsKey(prefab);
    }

    /// <summary>
    /// Return all objects in all pools
    /// </summary>
    public void ReturnAllToPool()
    {
        foreach (var pool in poolsByName.Values)
        {
            pool.ReturnAll();
        }
    }

    /// <summary>
    /// Get statistics for all pools
    /// </summary>
    public void LogPoolStatistics()
    {
        Debug.Log("=== Pool Manager Statistics ===");
        foreach (var kvp in poolsByName)
        {
            ObjectPool pool = kvp.Value;
            Debug.Log($"Pool '{kvp.Key}': Active={pool.ActiveCount}, Available={pool.AvailableCount}, Total={pool.TotalCount}");
        }
    }
}
