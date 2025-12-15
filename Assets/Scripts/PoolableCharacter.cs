using UnityEngine;
using JUTPS;
using JUTPS.PhysicsScripts;
using JUTPS.InventorySystem;

public class PoolableCharacter : MonoBehaviour
{
    [Header("Pool Settings")]
    public bool returnToPoolOnDeath = true;
    public float deactivateDelay = 3f;
    
    [Header("Ragdoll Settings")]
    public bool disableRagdollBeforeReturn = true;
    
    [Header("Reset Settings")]
    public bool resetHealthOnSpawn = true;
    public bool resetInventoryOnSpawn = true;
    
    [Header("References")]
    public JUHealth health;
    
    [Header("Debug")]
    public bool debugLogging = false;
    
    private bool hasBeenReturnedToPool = false;
    private AdvancedRagdollController ragdollController;
    private JUInventory inventory;
    private float initialHealth;
    
    private void OnValidate()
    {
        if (health == null)
        {
            health = GetComponent<JUHealth>();
        }
    }
    
    private void Start()
    {
        if (health == null)
        {
            health = GetComponent<JUHealth>();
        }
        
        ragdollController = GetComponent<AdvancedRagdollController>();
        inventory = GetComponent<JUInventory>();
        
        if (health != null)
        {
            initialHealth = health.MaxHealth;
            
            if (returnToPoolOnDeath)
            {
                health.OnDeath.AddListener(OnCharacterDeath);
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: PoolableCharacter requires JUHealth component!", this);
        }
    }
    
    private void OnEnable()
    {
        hasBeenReturnedToPool = false;
        ResetCharacter();
    }
    
    private void ResetCharacter()
    {
        if (resetHealthOnSpawn && health != null)
        {
            health.Health = initialHealth > 0 ? initialHealth : health.MaxHealth;
            
            if (debugLogging)
            {
                Debug.Log($"{gameObject.name}: Reset health to {health.Health}", this);
            }
        }
        
        if (resetInventoryOnSpawn && inventory != null)
        {
            inventory.SetupItems();
            
            if (debugLogging)
            {
                Debug.Log($"{gameObject.name}: Reset inventory", this);
            }
        }
        
        if (ragdollController != null && ragdollController.State == AdvancedRagdollController.RagdollState.Ragdolled)
        {
            ragdollController.SetActiveRagdoll(false);
            
            if (debugLogging)
            {
                Debug.Log($"{gameObject.name}: Disabled ragdoll on respawn", this);
            }
        }
    }
    
    private void OnCharacterDeath()
    {
        if (hasBeenReturnedToPool)
            return;
        
        if (debugLogging)
        {
            Debug.Log($"{gameObject.name}: Character died, will return to pool in {deactivateDelay} seconds", this);
        }
        
        Invoke(nameof(ReturnToPool), deactivateDelay);
    }
    
    private void ReturnToPool()
    {
        if (hasBeenReturnedToPool)
            return;
        
        hasBeenReturnedToPool = true;
        
        if (disableRagdollBeforeReturn && ragdollController != null)
        {
            ragdollController.SetActiveRagdoll(false);
        }
        
        if (CharacterSpawner.Instance != null)
        {
            CharacterSpawner.Instance.DespawnCharacter(gameObject);
            
            if (debugLogging)
            {
                Debug.Log($"{gameObject.name}: Returned to CharacterSpawner pool", this);
            }
        }
        else
        {
            gameObject.SetActive(false);
            
            if (debugLogging)
            {
                Debug.Log($"{gameObject.name}: CharacterSpawner not found, deactivating GameObject", this);
            }
        }
    }
    
    public void ForceReturnToPool()
    {
        CancelInvoke(nameof(ReturnToPool));
        ReturnToPool();
    }
}
