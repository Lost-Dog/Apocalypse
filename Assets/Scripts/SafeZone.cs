using UnityEngine;
using UnityEngine.Events;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.Shooter;

/// <summary>
/// Trigger zone that restores all player stats and replenishes ammo on entry.
/// Health is restored via GC2 Traits; ammo via ShooterMunition; survival stats via SurvivalManager.
/// </summary>
public class SafeZone : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Safe Zone Settings")]
    public string safeZoneName = "Safe Zone";
    public bool restoreHealth       = true;
    public bool restoreStamina      = true;
    public bool cureInfection       = true;
    public bool normalizeTemperature = true;
    public bool restoreHunger       = true;
    public bool restoreThirst       = true;

    [Header("Restoration Settings")]
    [Tooltip("Duration to fully replenish all stats (in seconds)")]
    public float replenishDuration = 5f;
    [Tooltip("Use smooth lerp transition for stat restoration")]
    public bool useSmoothTransition = true;
    [Tooltip("Delay before restoration starts (seconds)")]
    public float restoreDelay = 0f;
    [Tooltip("Only restore when player is idle (not moving)")]
    public bool requireIdle = false;
    [Tooltip("Maximum distance player can move while idle")]
    public float idleMovementThreshold = 0.1f;

    [Header("Ammo Replenishment")]
    [Tooltip("Fill all active weapon magazines when the player enters")]
    public bool replenishAmmo = true;
    [Tooltip("Number of total rounds to add per munition type when replenishing")]
    public int roundsToAddPerWeapon = 90;

    [Header("Visual Feedback")]
    public GameObject enterEffect;
    public GameObject healingEffect;
    public Material activeZoneMaterial;
    public Color healingColor = new Color(0f, 1f, 0.5f, 0.3f);

    [Header("Audio")]
    public AudioClip enterSound;
    public AudioClip healingSound;
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    [Header("UI Feedback")]
    public bool showUIMessage = true;
    public string enterMessage = "Entered Safe Zone - Restoring Stats";
    public float messageDuration = 3f;

    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    public UnityEvent onRestoreComplete;

    // Runtime state
    private Traits playerTraits;
    private Character playerCharacter;
    private SurvivalManager survivalManager;
    private bool playerInZone;
    private float timeInZone;
    private Vector3 lastPlayerPosition;
    private GameObject activeHealingEffect;
    private AudioSource audioSource;
    private Renderer zoneRenderer;
    private Material originalMaterial;
    private bool hasReplenishedAmmo;

    private float restorationProgress;
    private double startHealth;
    private float startStamina;
    private float startTemperature;
    private float startInfection;
    private float startHunger;
    private float startThirst;

    private void Start()
    {
        SetupPhysics();
        SetupAudio();
        SetupVisuals();
    }

    private void SetupPhysics()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            MeshCollider meshCol = col as MeshCollider;
            if (meshCol != null && !meshCol.convex)
            {
                Debug.LogWarning($"SafeZone '{safeZoneName}' has a concave MeshCollider — converting.");
                if (meshCol.sharedMesh != null && meshCol.sharedMesh.vertexCount < 256)
                {
                    meshCol.convex    = true;
                    meshCol.isTrigger = true;
                }
                else
                {
                    Destroy(meshCol);
                    BoxCollider box   = gameObject.AddComponent<BoxCollider>();
                    box.isTrigger     = true;
                    box.size          = new Vector3(10f, 5f, 10f);
                }
            }
            else
            {
                col.isTrigger = true;
            }
        }
        else
        {
            Debug.LogWarning($"SafeZone '{safeZoneName}' has no collider! Adding BoxCollider trigger.");
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger   = true;
            box.size        = new Vector3(10f, 5f, 10f);
        }
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (enterSound != null || healingSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake  = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume       = soundVolume;
        }
    }

    private void SetupVisuals()
    {
        zoneRenderer = GetComponent<Renderer>();
        if (zoneRenderer != null && activeZoneMaterial != null)
            originalMaterial = zoneRenderer.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerCharacter = other.GetComponent<Character>();
        playerTraits    = other.GetComponent<Traits>();
        survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();

        if (playerTraits == null)
        {
            Debug.LogWarning($"[SafeZone] Player has no Traits component — health restore disabled.");
        }

        playerInZone      = true;
        timeInZone        = 0f;
        lastPlayerPosition = other.transform.position;
        hasReplenishedAmmo = false;

        OnPlayerEnterZone(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || !playerInZone) return;

        timeInZone += Time.deltaTime;
        if (timeInZone < restoreDelay) return;

        if (requireIdle)
        {
            float moved = Vector3.Distance(other.transform.position, lastPlayerPosition);
            lastPlayerPosition = other.transform.position;
            if (moved > idleMovementThreshold)
            {
                StopHealing();
                return;
            }
        }

        RestorePlayerStats();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInZone)
            OnPlayerExitZone();
    }

    private void OnPlayerEnterZone(GameObject playerGO)
    {
        Debug.Log($"<color=green>Player entered {safeZoneName}</color>");

        survivalManager?.SetInSafeZone(true);

        restorationProgress = 0f;

        if (playerTraits != null)
        {
            try { startHealth = playerTraits.RuntimeAttributes.Get(HealthAttributeId).Value; }
            catch (System.Exception) { }
        }

        if (survivalManager != null)
        {
            startStamina    = survivalManager.currentStamina;
            startTemperature = survivalManager.currentTemperature;
            startInfection  = survivalManager.currentInfection;
            startHunger     = survivalManager.currentHunger;
            startThirst     = survivalManager.currentThirst;
        }

        if (replenishAmmo && !hasReplenishedAmmo)
        {
            ReplenishAmmo(playerGO);
            hasReplenishedAmmo = true;
        }

        if (enterEffect != null)
        {
            GameObject effect = Instantiate(enterEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (enterSound != null && audioSource != null)
            audioSource.PlayOneShot(enterSound, soundVolume);

        if (zoneRenderer != null && activeZoneMaterial != null)
            zoneRenderer.material = activeZoneMaterial;

        if (showUIMessage) ShowSafeZoneMessage(enterMessage);

        onPlayerEnter.Invoke();
    }

    private void OnPlayerExitZone()
    {
        Debug.Log($"<color=yellow>Player left {safeZoneName}</color>");

        survivalManager?.SetInSafeZone(false);

        playerInZone = false;
        timeInZone   = 0f;

        StopHealing();

        if (zoneRenderer != null && originalMaterial != null)
            zoneRenderer.material = originalMaterial;

        if (showUIMessage) ShowSafeZoneMessage("Left Safe Zone");

        onPlayerExit.Invoke();

        playerTraits    = null;
        playerCharacter = null;
        survivalManager = null;
    }

    private void ReplenishAmmo(GameObject playerGO)
    {
        // ShooterMunition is a plain C# class, not a MonoBehaviour — it lives inside
        // Character.Combat and must be accessed through the GC2 Combat API.
        if (playerCharacter == null) return;

        IMunition[] munitions = playerCharacter.Combat.Munitions;
        int count = 0;

        foreach (IMunition munitionEntry in munitions)
        {
            if (munitionEntry.Value is ShooterMunition shooterMunition)
            {
                shooterMunition.Total += roundsToAddPerWeapon;
                count++;
            }
        }

        if (count > 0)
            Debug.Log($"<color=cyan>[SafeZone] Replenished {count} munition type(s) (+{roundsToAddPerWeapon} each)</color>");
        else
            Debug.Log("[SafeZone] No ShooterMunition entries found in player Combat.");
    }

    private void RestorePlayerStats()
    {
        restorationProgress = Mathf.Clamp01(restorationProgress + Time.deltaTime / replenishDuration);
        float t = useSmoothTransition ? Mathf.SmoothStep(0f, 1f, restorationProgress) : restorationProgress;
        bool isRestoring = false;

        // Health via Traits
        if (restoreHealth && playerTraits != null)
        {
            try
            {
                RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
                if (health.Value < health.MaxValue)
                {
                    health.Value = System.Math.Min(
                        startHealth + (health.MaxValue - startHealth) * t,
                        health.MaxValue
                    );
                    isRestoring = true;
                }
            }
            catch (System.Exception) { }
        }

        if (survivalManager != null)
        {
            if (restoreStamina && survivalManager.currentStamina < survivalManager.maxStamina)
            {
                survivalManager.currentStamina = Mathf.Lerp(startStamina, survivalManager.maxStamina, t);
                isRestoring = true;
            }

            if (cureInfection && survivalManager.currentInfection > 0f)
            {
                survivalManager.currentInfection = Mathf.Lerp(startInfection, 0f, t);
                isRestoring = true;
            }

            if (restoreHunger && survivalManager.currentHunger < survivalManager.maxHunger)
            {
                survivalManager.currentHunger = Mathf.Lerp(startHunger, survivalManager.maxHunger, t);
                isRestoring = true;
            }

            if (restoreThirst && survivalManager.currentThirst < survivalManager.maxThirst)
            {
                survivalManager.currentThirst = Mathf.Lerp(startThirst, survivalManager.maxThirst, t);
                isRestoring = true;
            }
        }

        if (isRestoring)
        {
            if (activeHealingEffect == null)
                StartHealing();
        }
        else
        {
            if (activeHealingEffect != null)
            {
                StopHealing();
                onRestoreComplete.Invoke();
                Debug.Log($"<color=cyan>Player fully restored in {safeZoneName}</color>");
            }
        }
    }

    private void StartHealing()
    {
        if (healingEffect != null && activeHealingEffect == null)
            activeHealingEffect = Instantiate(healingEffect, transform.position, Quaternion.identity, transform);

        if (healingSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = healingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopHealing()
    {
        if (activeHealingEffect != null)
        {
            Destroy(activeHealingEffect);
            activeHealingEffect = null;
        }

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void ShowSafeZoneMessage(string message)
    {
        GameObject messageDisplay = GameObject.Find("MessageDisplay");
        MessageDisplay display = messageDisplay?.GetComponent<MessageDisplay>();

        if (display != null)
            display.ShowMessage(message, messageDuration);
        else
            Debug.Log($"<color=cyan>[Safe Zone] {message}</color>");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = healingColor;
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(transform.position, sphere.radius * transform.localScale.x);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position, sphere.radius * transform.localScale.x);
        }
    }
}
