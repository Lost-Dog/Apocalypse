using UnityEngine;
using UnityEngine.Events;
using JUTPS;

/// <summary>
/// Applies continuous fire/burn damage to the player when they enter the explosion trigger.
/// Supports burn VFX effects on the player.
/// </summary>
public class ExplosionDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Fire damage per second")]
    public float fireBaseDamage = 5f;
    
    [Tooltip("Burn damage per second (continues after leaving)")]
    public float burnDamagePerSecond = 2f;
    
    [Tooltip("How long the burn effect lasts after leaving the explosion")]
    public float burnDuration = 3f;
    
    [Tooltip("Damage tick interval (how often damage is applied)")]
    public float damageTickInterval = 0.5f;
    
    [Header("Trigger Settings")]
    [Tooltip("Radius of the damage trigger sphere")]
    public float damageRadius = 5f;
    
    [Tooltip("Tag to check for player")]
    public string playerTag = "Player";
    
    [Header("Visual Effects")]
    [Tooltip("Burn VFX prefab to spawn on player")]
    public GameObject burnVFXPrefab;
    
    [Tooltip("Position offset for VFX attachment")]
    public Vector3 vfxOffset = Vector3.zero;
    
    [Tooltip("Should VFX be attached to player?")]
    public bool attachVFXToPlayer = true;
    
    [Header("Audio")]
    [Tooltip("Sound to play when player enters fire")]
    public AudioClip fireEnterSound;
    
    [Tooltip("Looping burn sound")]
    public AudioClip burnLoopSound;
    
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    
    [Header("Events")]
    public UnityEvent onPlayerEnterFire;
    public UnityEvent onPlayerExitFire;
    public UnityEvent onBurnStart;
    public UnityEvent onBurnEnd;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private JUHealth playerHealth;
    private GameObject playerObject;
    private bool playerInFire = false;
    private float damageTimer = 0f;
    private float burnTimer = 0f;
    private bool isBurning = false;
    private GameObject activeBurnVFX;
    private AudioSource audioSource;
    private SphereCollider triggerCollider;
    
    private void Start()
    {
        SetupTrigger();
        SetupAudio();
    }
    
    private void SetupTrigger()
    {
        // Check for existing sphere collider
        triggerCollider = GetComponent<SphereCollider>();
        
        if (triggerCollider == null)
        {
            // Add sphere collider if none exists
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            if (showDebugInfo)
            {
                Debug.Log($"[ExplosionDamage] Added SphereCollider to '{gameObject.name}'");
            }
        }
        
        // Configure as trigger
        triggerCollider.isTrigger = true;
        triggerCollider.radius = damageRadius;
        
        if (showDebugInfo)
        {
            Debug.Log($"[ExplosionDamage] Trigger configured - Radius: {damageRadius}");
        }
    }
    
    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (fireEnterSound != null || burnLoopSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = soundVolume;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerHealth = other.GetComponent<JUHealth>();
            playerObject = other.gameObject;
            
            if (playerHealth != null)
            {
                playerInFire = true;
                damageTimer = 0f;
                
                OnPlayerEnterFire();
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag) && playerInFire && playerHealth != null)
        {
            ApplyFireDamage();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && playerInFire)
        {
            playerInFire = false;
            OnPlayerExitFire();
            StartBurnEffect();
        }
    }
    
    private void Update()
    {
        // Handle burn damage after leaving fire
        if (isBurning && !playerInFire)
        {
            ApplyBurnDamage();
        }
    }
    
    private void ApplyFireDamage()
    {
        damageTimer -= Time.deltaTime;
        
        if (damageTimer <= 0f)
        {
            damageTimer = damageTickInterval;
            
            if (playerHealth != null && fireBaseDamage > 0f)
            {
                float damage = fireBaseDamage * damageTickInterval;
                
                JUHealth.DamageInfo damageInfo = new JUHealth.DamageInfo
                {
                    Damage = damage,
                    HitPosition = playerObject.transform.position,
                    HitDirection = (playerObject.transform.position - transform.position).normalized,
                    HitOriginPosition = transform.position,
                    HitOwner = gameObject
                };
                
                playerHealth.DoDamage(damageInfo);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExplosionDamage] Applied {damage} fire damage to player");
                }
            }
        }
    }
    
    private void ApplyBurnDamage()
    {
        burnTimer -= Time.deltaTime;
        damageTimer -= Time.deltaTime;
        
        if (burnTimer <= 0f)
        {
            // Burn effect expired
            StopBurnEffect();
            return;
        }
        
        if (damageTimer <= 0f)
        {
            damageTimer = damageTickInterval;
            
            if (playerHealth != null && burnDamagePerSecond > 0f)
            {
                float damage = burnDamagePerSecond * damageTickInterval;
                
                JUHealth.DamageInfo damageInfo = new JUHealth.DamageInfo
                {
                    Damage = damage,
                    HitPosition = playerObject.transform.position,
                    HitDirection = Vector3.zero,
                    HitOriginPosition = transform.position,
                    HitOwner = gameObject
                };
                
                playerHealth.DoDamage(damageInfo);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExplosionDamage] Applied {damage} burn damage to player ({burnTimer:F1}s remaining)");
                }
            }
        }
    }
    
    private void OnPlayerEnterFire()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[ExplosionDamage] Player entered fire zone");
        }
        
        // Play enter sound
        if (audioSource != null && fireEnterSound != null)
        {
            audioSource.PlayOneShot(fireEnterSound, soundVolume);
        }
        
        // Start looping burn sound
        if (audioSource != null && burnLoopSound != null)
        {
            audioSource.clip = burnLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Spawn burn VFX
        SpawnBurnVFX();
        
        // Invoke event
        onPlayerEnterFire?.Invoke();
    }
    
    private void OnPlayerExitFire()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[ExplosionDamage] Player exited fire zone");
        }
        
        // Stop looping sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Invoke event
        onPlayerExitFire?.Invoke();
    }
    
    private void StartBurnEffect()
    {
        isBurning = true;
        burnTimer = burnDuration;
        damageTimer = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"[ExplosionDamage] Started burn effect ({burnDuration}s duration)");
        }
        
        // Keep VFX playing during burn
        // VFX was already spawned in OnPlayerEnterFire
        
        onBurnStart?.Invoke();
    }
    
    private void StopBurnEffect()
    {
        isBurning = false;
        burnTimer = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"[ExplosionDamage] Stopped burn effect");
        }
        
        // Remove burn VFX
        RemoveBurnVFX();
        
        onBurnEnd?.Invoke();
    }
    
    private void SpawnBurnVFX()
    {
        if (burnVFXPrefab == null || playerObject == null)
            return;
        
        // Remove existing VFX first
        RemoveBurnVFX();
        
        if (attachVFXToPlayer)
        {
            // Attach to player
            activeBurnVFX = Instantiate(burnVFXPrefab, playerObject.transform);
            activeBurnVFX.transform.localPosition = vfxOffset;
        }
        else
        {
            // Spawn at player position
            activeBurnVFX = Instantiate(burnVFXPrefab, playerObject.transform.position + vfxOffset, Quaternion.identity);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[ExplosionDamage] Spawned burn VFX on player");
        }
    }
    
    private void RemoveBurnVFX()
    {
        if (activeBurnVFX != null)
        {
            // Stop particle system before destroying
            ParticleSystem ps = activeBurnVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }
            
            Destroy(activeBurnVFX, 2f); // Delay to allow particles to fade
            activeBurnVFX = null;
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExplosionDamage] Removed burn VFX from player");
            }
        }
    }
    
    private void OnDisable()
    {
        // Clean up when disabled
        if (playerInFire)
        {
            playerInFire = false;
        }
        
        if (isBurning)
        {
            StopBurnEffect();
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up VFX
        RemoveBurnVFX();
    }
    
    // Public API
    
    /// <summary>
    /// Set the damage radius of the trigger sphere
    /// </summary>
    public void SetDamageRadius(float radius)
    {
        damageRadius = radius;
        if (triggerCollider != null)
        {
            triggerCollider.radius = radius;
        }
    }
    
    /// <summary>
    /// Force stop burn effect on player
    /// </summary>
    public void ClearBurnEffect()
    {
        if (isBurning)
        {
            StopBurnEffect();
        }
    }
    
    /// <summary>
    /// Check if player is currently burning
    /// </summary>
    public bool IsPlayerBurning()
    {
        return isBurning || playerInFire;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize damage radius in editor
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange semi-transparent
        Gizmos.DrawSphere(transform.position, damageRadius);
        
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f); // Orange outline
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
