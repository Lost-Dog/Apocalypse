using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for reusing GameObjects instead of instantiating/destroying them.
/// Significantly reduces GC allocations and improves performance.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("Prefab to instantiate for this pool")]
    public GameObject prefab;

    [Tooltip("Number of objects to pre-instantiate")]
    public int initialPoolSize = 10;

    [Tooltip("Maximum pool size (0 = unlimited)")]
    public int maxPoolSize = 50;

    [Tooltip("Allow pool to grow beyond initial size")]
    public bool canGrow = true;

    [Tooltip("Parent inactive objects under this transform")]
    public Transform poolParent;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private Queue<GameObject> availableObjects = new Queue<GameObject>();
    private List<GameObject> allPooledObjects = new List<GameObject>();
    private int totalCreated = 0;

    private void Awake()
    {
        if (poolParent == null)
        {
            GameObject parent = new GameObject($"{prefab.name}_Pool");
            poolParent = parent.transform;
            poolParent.SetParent(transform);
        }

        InitializePool();
    }

    /// <summary>
    /// Pre-instantiate initial pool objects
    /// </summary>
    private void InitializePool()
    {
        if (prefab == null)
        {
            Debug.LogError($"[ObjectPool] No prefab assigned to pool on {gameObject.name}");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPooledObject();
        }

        if (showDebugInfo)
        {
            Debug.Log($"[ObjectPool] Initialized pool for {prefab.name} with {initialPoolSize} objects");
        }
    }

    /// <summary>
    /// Creates a new pooled object and adds it to the available queue
    /// </summary>
    private GameObject CreateNewPooledObject()
    {
        // Instantiate below the world so Awake() physics registration (Rigidbody, Collider, NavMeshAgent)
        // never fires at the world origin and interferes with gameplay objects there.
        GameObject obj = Instantiate(prefab, Vector3.down * 1000f, Quaternion.identity, poolParent);
        obj.SetActive(false);
        obj.name = $"{prefab.name}_{totalCreated}";

        availableObjects.Enqueue(obj);
        allPooledObjects.Add(obj);
        totalCreated++;

        return obj;
    }

    /// <summary>
    /// Get an object from the pool at the specified position and rotation
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        // Try to get from available queue
        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        // Create new if pool can grow
        else if (canGrow && (maxPoolSize == 0 || totalCreated < maxPoolSize))
        {
            obj = CreateNewPooledObject();
            availableObjects.Dequeue(); // Remove it since we're using it

            if (showDebugInfo)
            {
                Debug.Log($"[ObjectPool] Pool grew to {totalCreated} objects");
            }
        }
        // Pool exhausted and can't grow
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[ObjectPool] Pool exhausted for {prefab.name}. Consider increasing maxPoolSize.");
            }
            return null;
        }

        // Configure and activate object
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    /// <summary>
    /// Get an object from the pool at the specified position with default rotation
    /// </summary>
    public GameObject Get(Vector3 position)
    {
        return Get(position, Quaternion.identity);
    }

    /// <summary>
    /// Return an object to the pool for reuse
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[ObjectPool] Attempted to return null object to pool");
            return;
        }

        // Check if this object belongs to this pool
        if (!allPooledObjects.Contains(obj))
        {
            Debug.LogWarning($"[ObjectPool] Object {obj.name} does not belong to this pool!");
            return;
        }

        // Reset object state
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);

        // Return to available queue
        availableObjects.Enqueue(obj);
    }

    /// <summary>
    /// Return all active objects to the pool
    /// </summary>
    public void ReturnAll()
    {
        for (int i = 0; i < allPooledObjects.Count; i++)
        {
            if (allPooledObjects[i].activeInHierarchy)
            {
                Return(allPooledObjects[i]);
            }
        }
    }

    /// <summary>
    /// Get count of available objects in pool
    /// </summary>
    public int AvailableCount => availableObjects.Count;

    /// <summary>
    /// Get count of active objects from this pool
    /// </summary>
    public int ActiveCount => totalCreated - availableObjects.Count;

    /// <summary>
    /// Get total count of all pooled objects
    /// </summary>
    public int TotalCount => totalCreated;

    private void OnDestroy()
    {
        // Clean up all pooled objects
        for (int i = 0; i < allPooledObjects.Count; i++)
        {
            if (allPooledObjects[i] != null)
            {
                Destroy(allPooledObjects[i]);
            }
        }

        availableObjects.Clear();
        allPooledObjects.Clear();
    }
}
