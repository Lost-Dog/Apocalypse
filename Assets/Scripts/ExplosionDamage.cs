using UnityEngine;
using UnityEngine.Events;
using GameCreator.Runtime.Stats;

/// <summary>
/// Applies continuous fire/burn damage to any GC2 Traits-bearing character that enters the trigger.
/// Damage is subtracted directly from the configured health Attribute on the character's Traits component.
/// </summary>
public class ExplosionDamage : MonoBehaviour
{
    private const string DefaultHealthAttributeId = "health";

    [Header("Damage Settings")]
    [Tooltip("Fire damage per second while inside the trigger")]
    public float fireBaseDamage = 5f;

    [Tooltip("Burn damage per second that continues after leaving")]
    public float burnDamagePerSecond = 2f;

    [Tooltip("How long the burn effect lasts after the character leaves the trigger")]
    public float burnDuration = 3f;

    [Tooltip("How often damage is applied (seconds between ticks)")]
    public float damageTickInterval = 0.5f;

    [Tooltip("GC2 Attribute ID to subtract damage from (must match the Traits component)")]
    public string healthAttributeId = DefaultHealthAttributeId;

    [Header("Trigger Settings")]
    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    [Tooltip("Radius of the damage trigger sphere")]
    public float damageRadius = 5f;

    [Header("Visual Effects")]
    [Tooltip("Burn VFX prefab to spawn on the player")]
    public GameObject burnVFXPrefab;

    [Tooltip("Position offset for VFX attachment")]
    public Vector3 vfxOffset = Vector3.zero;

    [Tooltip("Attach VFX to the player instead of spawning at world position")]
    public bool attachVFXToPlayer = true;

    [Header("Audio")]
    [Tooltip("Sound played when the player enters fire")]
    public AudioClip fireEnterSound;

    [Tooltip("Looping sound while in fire")]
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

    // Runtime state
    private Traits playerTraits;
    private GameObject playerObject;
    private bool playerInFire;
    private float damageTimer;
    private float burnTimer;
    private bool isBurning;
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
        triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider == null)
            triggerCollider = gameObject.AddComponent<SphereCollider>();

        triggerCollider.isTrigger = true;
        triggerCollider.radius = damageRadius;
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (fireEnterSound != null || burnLoopSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume = soundVolume;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerTraits = other.GetComponent<Traits>();
        playerObject = other.gameObject;

        if (playerTraits == null) return;

        playerInFire = true;
        damageTimer = 0f;
        OnPlayerEnterFire();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag) && playerInFire && playerTraits != null)
            ApplyFireDamage();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || !playerInFire) return;

        playerInFire = false;
        OnPlayerExitFire();
        StartBurnEffect();
    }

    private void Update()
    {
        if (isBurning && !playerInFire)
            ApplyBurnDamage();
    }

    // DAMAGE -----------------------------------------------------------------------------------------

    private void ApplyFireDamage()
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer > 0f) return;

        damageTimer = damageTickInterval;
        float damage = fireBaseDamage * damageTickInterval;
        SubtractHealth(damage);

        if (showDebugInfo)
            Debug.Log($"[ExplosionDamage] Applied {damage:F1} fire damage");
    }

    private void ApplyBurnDamage()
    {
        burnTimer -= Time.deltaTime;
        damageTimer -= Time.deltaTime;

        if (burnTimer <= 0f)
        {
            StopBurnEffect();
            return;
        }

        if (damageTimer > 0f) return;

        damageTimer = damageTickInterval;
        float damage = burnDamagePerSecond * damageTickInterval;
        SubtractHealth(damage);

        if (showDebugInfo)
            Debug.Log($"[ExplosionDamage] Applied {damage:F1} burn damage ({burnTimer:F1}s remaining)");
    }

    /// <summary>
    /// Subtracts the given amount from the player's health Attribute via GC2 Traits.
    /// </summary>
    private void SubtractHealth(float amount)
    {
        if (playerTraits == null || amount <= 0f) return;

        try
        {
            RuntimeAttributeData healthAttr = playerTraits.RuntimeAttributes.Get(healthAttributeId);
            if (healthAttr != null)
                healthAttr.Value -= amount;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ExplosionDamage] Could not apply damage — {e.Message}");
        }
    }

    // EVENTS -----------------------------------------------------------------------------------------

    private void OnPlayerEnterFire()
    {
        if (showDebugInfo) Debug.Log("[ExplosionDamage] Player entered fire zone");

        if (audioSource != null && fireEnterSound != null)
            audioSource.PlayOneShot(fireEnterSound, soundVolume);

        if (audioSource != null && burnLoopSound != null)
        {
            audioSource.clip = burnLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        SpawnBurnVFX();
        onPlayerEnterFire?.Invoke();
    }

    private void OnPlayerExitFire()
    {
        if (showDebugInfo) Debug.Log("[ExplosionDamage] Player exited fire zone");

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        onPlayerExitFire?.Invoke();
    }

    private void StartBurnEffect()
    {
        isBurning = true;
        burnTimer = burnDuration;
        damageTimer = 0f;

        if (showDebugInfo) Debug.Log($"[ExplosionDamage] Burn started ({burnDuration}s)");

        onBurnStart?.Invoke();
    }

    private void StopBurnEffect()
    {
        isBurning = false;
        burnTimer = 0f;

        if (showDebugInfo) Debug.Log("[ExplosionDamage] Burn ended");

        RemoveBurnVFX();
        onBurnEnd?.Invoke();
    }

    // VFX --------------------------------------------------------------------------------------------

    private void SpawnBurnVFX()
    {
        if (burnVFXPrefab == null || playerObject == null) return;

        RemoveBurnVFX();

        activeBurnVFX = attachVFXToPlayer
            ? Instantiate(burnVFXPrefab, playerObject.transform.position + vfxOffset, Quaternion.identity, playerObject.transform)
            : Instantiate(burnVFXPrefab, playerObject.transform.position + vfxOffset, Quaternion.identity);
    }

    private void RemoveBurnVFX()
    {
        if (activeBurnVFX == null) return;

        ParticleSystem ps = activeBurnVFX.GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop();

        Destroy(activeBurnVFX, 2f);
        activeBurnVFX = null;
    }

    // LIFECYCLE --------------------------------------------------------------------------------------

    private void OnDisable()
    {
        playerInFire = false;
        if (isBurning) StopBurnEffect();
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
    }

    private void OnDestroy() => RemoveBurnVFX();

    // PUBLIC API -------------------------------------------------------------------------------------

    /// <summary>
    /// Updates the damage trigger sphere radius at runtime.
    /// </summary>
    public void SetDamageRadius(float radius)
    {
        damageRadius = radius;
        if (triggerCollider != null) triggerCollider.radius = radius;
    }

    /// <summary>
    /// Immediately clears any active burn effect on the player.
    /// </summary>
    public void ClearBurnEffect()
    {
        if (isBurning) StopBurnEffect();
    }

    /// <summary>
    /// Returns true if the player is currently in fire or still burning.
    /// </summary>
    public bool IsPlayerBurning() => isBurning || playerInFire;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, damageRadius);
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
