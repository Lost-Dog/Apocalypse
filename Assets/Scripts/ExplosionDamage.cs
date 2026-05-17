using Invector;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Applies continuous fire/burn damage to any Invector character that enters the trigger.
/// Damage is applied via vHealthController.TakeDamage so all Invector damage events fire correctly.
/// </summary>
public class ExplosionDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Fire damage per second while inside the trigger")]
    public float fireBaseDamage = 5f;

    [Tooltip("Burn damage per second that continues after leaving")]
    public float burnDamagePerSecond = 2f;

    [Tooltip("How long the burn effect lasts after the character leaves the trigger")]
    public float burnDuration = 3f;

    [Tooltip("How often damage is applied (seconds between ticks)")]
    public float damageTickInterval = 0.5f;

    [Header("Nuke Cover Resistance")]
    [Tooltip("When enabled, damage is reduced if the player is crouching in cover")]
    public bool isNukeExplosion = false;

    [Tooltip("Damage multiplier at level 1 while in cover (0 = full block, 1 = no reduction)")]
    [Range(0f, 1f)]
    public float baseCoverDamageMultiplier = 0.6f;

    [Tooltip("Minimum damage multiplier reached at max level while in cover")]
    [Range(0f, 1f)]
    public float minCoverDamageMultiplier = 0.1f;

    [Tooltip("Controls how resistance scales from base to min over the level range. " +
             "X = normalised level (0 = level 1, 1 = max level), Y = interpolation weight toward minCoverDamageMultiplier.")]
    public AnimationCurve coverResistanceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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
    private vHealthController playerHealth;
    private vThirdPersonController playerController;
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
            audioSource.playOnAwake  = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume       = soundVolume;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerHealth     = other.GetComponent<vHealthController>();
        playerController = other.GetComponent<vThirdPersonController>();
        playerObject     = other.gameObject;

        if (playerHealth == null) return;

        playerInFire = true;
        damageTimer  = 0f;
        OnPlayerEnterFire();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag) && playerInFire && playerHealth != null)
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
        ApplyDamage(damage);

        if (showDebugInfo)
            Debug.Log($"[ExplosionDamage] Applied {damage:F1} fire damage");
    }

    private void ApplyBurnDamage()
    {
        burnTimer   -= Time.deltaTime;
        damageTimer -= Time.deltaTime;

        if (burnTimer <= 0f)
        {
            StopBurnEffect();
            return;
        }

        if (damageTimer > 0f) return;

        damageTimer = damageTickInterval;
        float damage = burnDamagePerSecond * damageTickInterval;
        ApplyDamage(damage);

        if (showDebugInfo)
            Debug.Log($"[ExplosionDamage] Applied {damage:F1} burn damage ({burnTimer:F1}s remaining)");
    }

    /// <summary>
    /// Routes damage through Invector's TakeDamage so all health events fire correctly.
    /// When <see cref="isNukeExplosion"/> is true, damage is reduced if the player is
    /// crouching (in cover), with further resistance scaling per level from
    /// <see cref="ProgressionManager"/>.
    /// </summary>
    private void ApplyDamage(float amount)
    {
        if (playerHealth == null || playerHealth.isDead || amount <= 0f) return;

        float finalAmount = isNukeExplosion ? ApplyCoverResistance(amount) : amount;

        var damage = new vDamage(Mathf.RoundToInt(finalAmount));
        playerHealth.TakeDamage(damage);
    }

    /// <summary>
    /// Returns the damage amount after applying cover and level-based resistance.
    /// Level is normalised to [0, 1] relative to <see cref="ProgressionManager.maxLevel"/>,
    /// then evaluated on <see cref="coverResistanceCurve"/> to interpolate between
    /// <see cref="baseCoverDamageMultiplier"/> (level 1) and <see cref="minCoverDamageMultiplier"/> (max level).
    /// Only active when the player is crouching.
    /// </summary>
    private float ApplyCoverResistance(float amount)
    {
        if (playerController == null || !playerController.isCrouching)
            return amount;

        int   level    = ProgressionManager.Instance != null ? ProgressionManager.Instance.currentLevel : 1;
        int   maxLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.maxLevel     : 1000;

        float t          = Mathf.Clamp01((float)(level - 1) / Mathf.Max(maxLevel - 1, 1));
        float curveWeight = coverResistanceCurve.Evaluate(t);
        float multiplier  = Mathf.Lerp(baseCoverDamageMultiplier, minCoverDamageMultiplier, curveWeight);
        float reduced     = amount * multiplier;

        if (showDebugInfo)
            Debug.Log($"[ExplosionDamage] Nuke cover resistance — level {level}/{maxLevel}, t={t:F2}, multiplier={multiplier:F2}, {amount:F1} → {reduced:F1}");

        return reduced;
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
        isBurning   = true;
        burnTimer   = burnDuration;
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
