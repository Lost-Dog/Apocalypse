using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages pool lifecycle for NPC characters. On death the object is returned to the
/// CharacterSpawner pool; on re-enable health is reset via GC2 Stats.
/// Ragdoll reset is handled by re-enabling kinematic on all child Rigidbodies.
/// </summary>
public class PoolableCharacter : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Pool Settings")]
    public bool returnToPoolOnDeath = true;
    public float deactivateDelay = 3f;

    [Header("Ragdoll Settings")]
    public bool disableRagdollBeforeReturn = true;

    [Header("Reset Settings")]
    public bool resetHealthOnSpawn = true;
    [Tooltip("Reserved — GC2 Bag content reset is not yet implemented. Stock is set per-prefab in Awake.")]
    public bool resetInventoryOnSpawn = true;

    [Header("Debug")]
    public bool debugLogging = false;

    private Character character;
    private Traits traits;

    // Cached ragdoll Rigidbodies — all child Rigidbodies excluding the root (which the
    // GC2 Character controller owns). Populated once in Start() to avoid per-frame allocation.
    private readonly List<Rigidbody> ragdollBodies = new List<Rigidbody>();

    private bool hasBeenReturnedToPool = false;
    private double initialHealthValue;

    private void Start()
    {
        character = GetComponent<Character>();
        traits    = GetComponent<Traits>();

        CacheRagdollBodies();
        CacheInitialHealth();

        if (character != null)
        {
            if (returnToPoolOnDeath)
                character.EventDie += OnCharacterDeath;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: PoolableCharacter requires a GC2 Character component!", this);
        }
    }

    private void OnDestroy()
    {
        if (character != null)
            character.EventDie -= OnCharacterDeath;
    }

    private void OnEnable()
    {
        hasBeenReturnedToPool = false;
        ResetCharacter();
    }

    // RESET --------------------------------------------------------------------------------------

    /// <summary>
    /// Caches all child Rigidbodies that form the ragdoll skeleton (excludes the root Rigidbody
    /// owned by the GC2 Character controller).
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

    /// <summary>
    /// Returns true if any ragdoll Rigidbody is currently non-kinematic (i.e. ragdoll is active).
    /// </summary>
    private bool IsRagdolled()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != null && !rb.isKinematic)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Disables the ragdoll by making all child Rigidbodies kinematic and zeroing their velocity.
    /// </summary>
    private void DisableRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null) continue;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void CacheInitialHealth()
    {
        if (traits == null) return;

        try
        {
            RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
            initialHealthValue = health.MaxValue;
        }
        catch (Exception)
        {
            initialHealthValue = 0;
        }
    }

    private void ResetCharacter()
    {
        ResetHealth();
        ResetRagdoll();
    }

    private void ResetHealth()
    {
        if (!resetHealthOnSpawn || traits == null) return;

        try
        {
            RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
            double resetValue = initialHealthValue > 0 ? initialHealthValue : health.MaxValue;
            health.Value = resetValue;

            // Revive if the character is still flagged as dead from the previous life
            if (character != null && character.IsDead)
                character.IsDead = false;

            if (debugLogging)
                Debug.Log($"{gameObject.name}: Health reset to {health.Value}", this);
        }
        catch (Exception e)
        {
            if (debugLogging)
                Debug.LogWarning($"{gameObject.name}: Could not reset health attribute — {e.Message}", this);
        }
    }

    private void ResetRagdoll()
    {
        if (!IsRagdolled()) return;

        DisableRagdoll();

        if (debugLogging)
            Debug.Log($"{gameObject.name}: Ragdoll disabled on respawn", this);
    }

    // DEATH / POOL -------------------------------------------------------------------------------

    private void OnCharacterDeath()
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
    /// Does not change the underlying GC2 Stat — current HP is set within the attribute's
    /// existing max value bounds.
    /// </summary>
    public void SetScaledHealth(double scaledValue)
    {
        initialHealthValue = scaledValue;

        if (traits == null) return;

        try
        {
            RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
            health.Value = scaledValue;

            if (debugLogging)
                Debug.Log($"{gameObject.name}: Scaled health applied — {health.Value}", this);
        }
        catch (Exception e)
        {
            if (debugLogging)
                Debug.LogWarning($"{gameObject.name}: SetScaledHealth failed — {e.Message}", this);
        }
    }
}
