using Invector;
using Invector.vCharacterController;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages pool lifecycle for NPC characters. On death the object is returned to the
/// CharacterSpawner pool; on re-enable health is reset via Invector's vHealthController.
/// Ragdoll reset is handled by re-enabling kinematic on all child Rigidbodies.
/// </summary>
public class PoolableCharacter : MonoBehaviour
{
    [Header("Pool Settings")]
    public bool returnToPoolOnDeath = true;
    public float deactivateDelay = 3f;

    [Header("Ragdoll Settings")]
    public bool disableRagdollBeforeReturn = true;

    [Header("Reset Settings")]
    public bool resetHealthOnSpawn = true;

    [Header("Debug")]
    public bool debugLogging = false;

    private vHealthController healthController;

    // Cached ragdoll Rigidbodies — all child Rigidbodies excluding the root.
    // Populated once in Start() to avoid per-frame allocation.
    private readonly List<Rigidbody> ragdollBodies = new List<Rigidbody>();

    private bool hasBeenReturnedToPool = false;
    private float initialMaxHealth;

    private void Start()
    {
        healthController = GetComponent<vHealthController>();

        CacheRagdollBodies();
        CacheInitialHealth();

        if (healthController != null)
        {
            if (returnToPoolOnDeath)
                healthController.onDead.AddListener(OnCharacterDeath);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: PoolableCharacter requires a vHealthController component!", this);
        }
    }

    private void OnDestroy()
    {
        if (healthController != null)
            healthController.onDead.RemoveListener(OnCharacterDeath);
    }

    private void OnEnable()
    {
        hasBeenReturnedToPool = false;
        ResetCharacter();
    }

    // RESET --------------------------------------------------------------------------------------

    /// <summary>
    /// Caches all child Rigidbodies that form the ragdoll skeleton (excludes the root).
    /// </summary>
    private void CacheRagdollBodies()
    {
        ragdollBodies.Clear();
        Rigidbody root = GetComponent<Rigidbody>();

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            if (rb != root)
                ragdollBodies.Add(rb);
        }
    }

    private bool IsRagdolled()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != null && !rb.isKinematic)
                return true;
        }
        return false;
    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null) continue;
            rb.isKinematic     = true;
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void CacheInitialHealth()
    {
        if (healthController != null)
            initialMaxHealth = healthController.maxHealth;
    }

    private void ResetCharacter()
    {
        ResetHealth();
        ResetRagdoll();
    }

    private void ResetHealth()
    {
        if (!resetHealthOnSpawn || healthController == null) return;

        // Restore to max health and revive.
        healthController.ResetHealth();

        if (debugLogging)
            Debug.Log($"{gameObject.name}: Health reset to {healthController.currentHealth}", this);
    }

    private void ResetRagdoll()
    {
        if (!IsRagdolled()) return;

        DisableRagdoll();

        if (debugLogging)
            Debug.Log($"{gameObject.name}: Ragdoll disabled on respawn", this);
    }

    // DEATH / POOL -------------------------------------------------------------------------------

    private void OnCharacterDeath(GameObject deadObject)
    {
        if (hasBeenReturnedToPool) return;

        if (debugLogging)
            Debug.Log($"{gameObject.name}: Character died — returning to pool in {deactivateDelay}s", this);

        Invoke(nameof(ReturnToPool), deactivateDelay);
    }

    private void ReturnToPool()
    {
        if (hasBeenReturnedToPool) return;

        hasBeenReturnedToPool = true;

        if (disableRagdollBeforeReturn)
            DisableRagdoll();

        if (CharacterSpawner.Instance != null)
        {
            CharacterSpawner.Instance.DespawnCharacter(gameObject);

            if (debugLogging)
                Debug.Log($"{gameObject.name}: Returned to CharacterSpawner pool", this);
        }
        else
        {
            gameObject.SetActive(false);

            if (debugLogging)
                Debug.Log($"{gameObject.name}: CharacterSpawner not found — deactivating", this);
        }
    }

    /// <summary>
    /// Forces an immediate pool return, cancelling any pending delayed return.
    /// </summary>
    public void ForceReturnToPool()
    {
        CancelInvoke(nameof(ReturnToPool));
        ReturnToPool();
    }

    /// <summary>
    /// Allows DifficultyScaler to override the health value used when resetting on spawn.
    /// </summary>
    public void SetScaledHealth(float scaledValue)
    {
        initialMaxHealth = scaledValue;

        if (healthController == null) return;

        healthController.maxHealth = Mathf.RoundToInt(scaledValue);
        healthController.ResetHealth();

        if (debugLogging)
            Debug.Log($"{gameObject.name}: Scaled health applied — {scaledValue}", this);
    }
}
