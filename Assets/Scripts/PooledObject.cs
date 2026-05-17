using UnityEngine;

/// <summary>
/// Helper component for pooled objects.
/// Handles automatic return to pool after lifetime and provides cleanup hooks.
/// </summary>
public class PooledObject : MonoBehaviour
{
    [Header("Auto Return Settings")]
    [Tooltip("Automatically return to pool after this time (0 = disabled)")]
    public float lifetime = 0f;

    [Tooltip("Return to pool when this object is disabled")]
    public bool returnOnDisable = false;

    [Header("Cleanup")]
    [Tooltip("Reset velocity on return to pool")]
    public bool resetVelocity = true;

    [Tooltip("Reset rotation on return to pool")]
    public bool resetRotation = false;

    private float spawnTime;
    private Rigidbody rb;
    private ObjectPool parentPool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (lifetime > 0f && Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    private void OnDisable()
    {
        if (returnOnDisable)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// Return this object to its pool
    /// </summary>
    public void ReturnToPool()
    {
        // Try PoolManager first
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(gameObject);
            CleanupBeforeReturn();
            return;
        }

        // Try to find parent pool
        if (parentPool != null)
        {
            CleanupBeforeReturn();
            parentPool.Return(gameObject);
            return;
        }

        // Last resort: just disable
        CleanupBeforeReturn();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Return to pool after a delay
    /// </summary>
    public void ReturnToPoolAfterDelay(float delay)
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.DespawnAfterDelay(gameObject, delay);
        }
        else
        {
            Invoke(nameof(ReturnToPool), delay);
        }
    }

    /// <summary>
    /// Cleanup actions before returning to pool
    /// </summary>
    private void CleanupBeforeReturn()
    {
        if (resetVelocity && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (resetRotation)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Set the parent pool (called by ObjectPool)
    /// </summary>
    public void SetParentPool(ObjectPool pool)
    {
        parentPool = pool;
    }
}
