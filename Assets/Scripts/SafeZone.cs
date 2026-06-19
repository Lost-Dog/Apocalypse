using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trigger zone that restores all player stats and optionally replenishes ammo.
/// Health is restored via IPlayerProvider; survival stats via ISurvivalStatsProvider (GC2 traits).
/// </summary>
public class SafeZone : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    public string safeZoneName = "Safe Zone";
    public bool restoreHealth = true;
    public bool restoreStamina = true;
    public bool cureInfection = true;
    public bool normalizeTemperature = true;
    public bool restoreHunger = true;
    public bool restoreThirst = true;

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
    [Tooltip("Reserved for external ammo systems. Currently no automatic ammo refill is performed.")]
    public bool replenishAmmo = true;
    [Tooltip("Number of rounds to add per ammo type when replenishing")]
    public int roundsToAddPerWeapon = 90;

    [Header("Visual Feedback")]
    public GameObject enterEffect;
    public GameObject healingEffect;
    public Material activeZoneMaterial;
    public Color healingColor = new Color(0f, 1f, 0.5f, 0.3f);

    [Header("Physics")]
    [Tooltip("Preserve existing non-trigger colliders on this object (e.g., building collision) and use a separate trigger collider for SafeZone detection.")]
    public bool preserveExistingColliders = true;
    [Tooltip("Fallback trigger size used when no collider/renderer bounds can be inferred.")]
    public Vector3 fallbackTriggerSize = new Vector3(10f, 5f, 10f);

    [Header("Audio")]
    public AudioClip enterSound;
    public AudioClip healingSound;
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    [Header("UI Feedback")]
    public bool showUIMessage = true;
    public string enterMessage = "Entered Safe Zone - Restoring Stats";
    public float messageDuration = 3f;

    [Header("Safe Zone Player Effects")]
    [Tooltip("Make player invincible (immortal) while in safe zone")]
    public bool makePlayerInvincible = true;

    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    public UnityEvent onRestoreComplete;

    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;
    private Transform playerTransform;
    private SurvivalManager survivalManager;
    private EmeraldGC2PlayerBridge playerBridge;
    private bool playerInZone;
    private float timeInZone;
    private Vector3 lastPlayerPosition;
    private GameObject activeHealingEffect;
    private AudioSource audioSource;
    private Renderer zoneRenderer;
    private Material originalMaterial;
    private bool hasReplenishedAmmo;
    private Collider triggerCollider;
    private bool playerWasImmortal;

    private float restorationProgress;
    private float startHealth;
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
        if (preserveExistingColliders)
        {
            EnsureDedicatedTriggerCollider();
            return;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            MeshCollider meshCol = col as MeshCollider;
            if (meshCol != null && !meshCol.convex)
            {
                Debug.LogWarning($"SafeZone '{safeZoneName}' has a concave MeshCollider - converting.");
                if (meshCol.sharedMesh != null && meshCol.sharedMesh.vertexCount < 256)
                {
                    meshCol.convex = true;
                    meshCol.isTrigger = true;
                    triggerCollider = meshCol;
                }
                else
                {
                    Destroy(meshCol);
                    BoxCollider box = gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(10f, 5f, 10f);
                    triggerCollider = box;
                }
            }
            else
            {
                col.isTrigger = true;
                triggerCollider = col;
            }
        }
        else
        {
            Debug.LogWarning($"SafeZone '{safeZoneName}' has no collider. Adding BoxCollider trigger.");
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(10f, 5f, 10f);
            triggerCollider = box;
        }
    }

    private void EnsureDedicatedTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();

        Collider firstNonTrigger = null;
        Collider firstTrigger = null;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider c = colliders[i];
            if (c == null) continue;

            if (c.isTrigger)
            {
                if (firstTrigger == null) firstTrigger = c;
            }
            else
            {
                if (firstNonTrigger == null) firstNonTrigger = c;
            }
        }

        // If the object has only one trigger collider, it was likely converted by an older SafeZone setup.
        // Restore that collider to solid collision and create a separate trigger collider.
        if (firstNonTrigger == null && firstTrigger != null && colliders.Length == 1)
        {
            firstTrigger.isTrigger = false;
            firstNonTrigger = firstTrigger;
            firstTrigger = null;
        }

        if (firstTrigger != null)
        {
            triggerCollider = firstTrigger;
            triggerCollider.isTrigger = true;
            return;
        }

        BoxCollider newTrigger = gameObject.AddComponent<BoxCollider>();
        newTrigger.isTrigger = true;
        newTrigger.center = Vector3.zero;
        newTrigger.size = InferTriggerSize(firstNonTrigger);
        triggerCollider = newTrigger;
    }

    private Vector3 InferTriggerSize(Collider source)
    {
        if (source is BoxCollider box)
        {
            return box.size;
        }

        if (source is SphereCollider sphere)
        {
            float diameter = sphere.radius * 2f;
            return new Vector3(diameter, diameter, diameter);
        }

        if (source is CapsuleCollider capsule)
        {
            float diameter = capsule.radius * 2f;
            switch (capsule.direction)
            {
                case 0: return new Vector3(capsule.height, diameter, diameter);
                case 1: return new Vector3(diameter, capsule.height, diameter);
                case 2: return new Vector3(diameter, diameter, capsule.height);
            }
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 worldSize = renderer.bounds.size;
            Vector3 local = transform.InverseTransformVector(worldSize);

            return new Vector3(
                Mathf.Abs(local.x) > 0.01f ? Mathf.Abs(local.x) : fallbackTriggerSize.x,
                Mathf.Abs(local.y) > 0.01f ? Mathf.Abs(local.y) : fallbackTriggerSize.y,
                Mathf.Abs(local.z) > 0.01f ? Mathf.Abs(local.z) : fallbackTriggerSize.z
            );
        }

        return fallbackTriggerSize;
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (enterSound != null || healingSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume = soundVolume;
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

        GameObject playerGO = ResolvePlayerRoot(other);
        playerTransform = playerGO != null ? playerGO.transform : other.transform;
        playerProvider = ResolvePlayerProvider(playerGO);
        survivalStatsProvider = ResolveSurvivalProvider(playerGO, playerProvider);
        survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();
        playerBridge = playerGO != null ? playerGO.GetComponentInChildren<EmeraldGC2PlayerBridge>() : other.GetComponentInChildren<EmeraldGC2PlayerBridge>();

        if (playerProvider == null)
            Debug.LogWarning("[SafeZone] Player has no IPlayerProvider - health restore disabled.");

        if (survivalStatsProvider == null)
            Debug.LogWarning("[SafeZone] Player has no ISurvivalStatsProvider - survival stat restore will use SurvivalManager fallback.");

        playerInZone = true;
        timeInZone = 0f;
        lastPlayerPosition = other.transform.position;
        hasReplenishedAmmo = false;

        OnPlayerEnterZone(playerGO != null ? playerGO : other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || !playerInZone) return;

        // Player is suspended invisible/invincible in safe zone - no healing attempts
        timeInZone += Time.deltaTime;
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

        // Make player invincible if enabled
        if (makePlayerInvincible && playerBridge != null)
        {
            playerWasImmortal = playerBridge.Immortal;
            playerBridge.Immortal = true;
        }

        ApplyImmediateFullRestore();

        if (replenishAmmo && !hasReplenishedAmmo)
        {
            ReplenishAmmo(playerGO);
            hasReplenishedAmmo = true;
        }

        if (enterEffect != null)
        {
            Transform effectParent = playerTransform != null ? playerTransform : transform;
            Vector3 spawnPosition = effectParent.position;
            GameObject effect = Instantiate(enterEffect, spawnPosition, Quaternion.identity, effectParent);
            Destroy(effect, 3f);
        }

        if (enterSound != null && audioSource != null)
            audioSource.PlayOneShot(enterSound, soundVolume);

        if (zoneRenderer != null && activeZoneMaterial != null)
            zoneRenderer.material = activeZoneMaterial;

        if (showUIMessage) ShowSafeZoneMessage(enterMessage);

        onPlayerEnter.Invoke();
    }

    private void ApplyImmediateFullRestore()
    {
        bool restoredAny = false;

        if (restoreHealth && playerProvider != null && playerProvider.IsAlive)
        {
            if (playerProvider.Health < playerProvider.MaxHealth)
            {
                playerProvider.SetHealth(playerProvider.MaxHealth);
                restoredAny = true;
            }
        }

        if (survivalStatsProvider != null)
        {
            if (restoreStamina && survivalStatsProvider.Stamina < survivalStatsProvider.MaxStamina)
            {
                survivalStatsProvider.SetStamina(survivalStatsProvider.MaxStamina);
                restoredAny = true;
            }

            if (normalizeTemperature)
            {
                float tempTarget = survivalManager != null
                    ? Mathf.Clamp(survivalManager.normalTemperature, 0f, Mathf.Max(1f, survivalStatsProvider.MaxTemperature))
                    : survivalStatsProvider.MaxTemperature;

                if (survivalStatsProvider.Temperature < tempTarget)
                {
                    survivalStatsProvider.SetTemperature(tempTarget);
                    restoredAny = true;
                }
            }

            if (cureInfection && survivalStatsProvider.Infection < survivalStatsProvider.MaxInfection)
            {
                survivalStatsProvider.SetInfection(survivalStatsProvider.MaxInfection);
                restoredAny = true;
            }

            if (restoreHunger && survivalStatsProvider.Hunger < survivalStatsProvider.MaxHunger)
            {
                survivalStatsProvider.SetHunger(survivalStatsProvider.MaxHunger);
                restoredAny = true;
            }

            if (restoreThirst && survivalStatsProvider.Thirst < survivalStatsProvider.MaxThirst)
            {
                survivalStatsProvider.SetThirst(survivalStatsProvider.MaxThirst);
                restoredAny = true;
            }
        }
        else if (survivalManager != null)
        {
            if (restoreStamina && survivalManager.currentStamina < survivalManager.maxStamina)
            {
                survivalManager.SetStamina(survivalManager.maxStamina);
                restoredAny = true;
            }

            if (normalizeTemperature && survivalManager.currentTemperature < survivalManager.normalTemperature)
            {
                survivalManager.SetTemperature(survivalManager.normalTemperature);
                restoredAny = true;
            }

            if (cureInfection && survivalManager.currentInfection < survivalManager.maxInfection)
            {
                survivalManager.SetInfection(survivalManager.maxInfection);
                restoredAny = true;
            }

            if (restoreHunger && survivalManager.currentHunger < survivalManager.maxHunger)
            {
                survivalManager.SetHunger(survivalManager.maxHunger);
                restoredAny = true;
            }

            if (restoreThirst && survivalManager.currentThirst < survivalManager.maxThirst)
            {
                survivalManager.SetThirst(survivalManager.maxThirst);
                restoredAny = true;
            }
        }

        StopHealing();

        if (restoredAny)
            onRestoreComplete.Invoke();
    }

    private void OnPlayerExitZone()
    {
        Debug.Log($"<color=yellow>Player left {safeZoneName}</color>");

        survivalManager?.SetInSafeZone(false);

        playerInZone = false;
        timeInZone = 0f;

        StopHealing();

        // Restore player vulnerability if was made invincible
        if (makePlayerInvincible && playerBridge != null)
        {
            playerBridge.Immortal = playerWasImmortal;
        }

        if (zoneRenderer != null && originalMaterial != null)
            zoneRenderer.material = originalMaterial;

        if (showUIMessage) ShowSafeZoneMessage("Left Safe Zone");

        onPlayerExit.Invoke();

        playerProvider = null;
        survivalStatsProvider = null;
        playerTransform = null;
        survivalManager = null;
        playerBridge = null;
    }

    private void ReplenishAmmo(GameObject playerGO)
    {
        _ = playerGO;
        Debug.Log("[SafeZone] Ammo replenish is enabled but no active ammo provider integration is configured.");
    }

    private void RestorePlayerStats()
    {
        // Early exit: check if all stats are already at target values
        if (IsAllStatsAtTarget())
        {
            if (activeHealingEffect != null)
            {
                StopHealing();
                onRestoreComplete.Invoke();
            }
            return;
        }

        float duration = Mathf.Max(0.01f, replenishDuration);
        restorationProgress = Mathf.Clamp01(restorationProgress + Time.deltaTime / duration);
        float t = useSmoothTransition ? Mathf.SmoothStep(0f, 1f, restorationProgress) : restorationProgress;
        bool isRestoring = false;

        if (restoreHealth && playerProvider != null && playerProvider.IsAlive)
        {
            float current = playerProvider.Health;
            float max = playerProvider.MaxHealth;
            if (current < max)
            {
                float targetHealth = Mathf.Lerp(startHealth, max, t);
                playerProvider.SetHealth(targetHealth);
                isRestoring = true;
            }
        }

        if (survivalStatsProvider != null)
        {
            if (restoreStamina && survivalStatsProvider.Stamina < survivalStatsProvider.MaxStamina)
            {
                float targetStamina = Mathf.Lerp(startStamina, survivalStatsProvider.MaxStamina, t);
                survivalStatsProvider.SetStamina(targetStamina);
                isRestoring = true;
            }

            float tempTarget = survivalManager != null
                ? Mathf.Clamp(survivalManager.normalTemperature, 0f, Mathf.Max(1f, survivalStatsProvider.MaxTemperature))
                : survivalStatsProvider.MaxTemperature;

            if (normalizeTemperature && survivalStatsProvider.Temperature < tempTarget)
            {
                float targetTemp = Mathf.Lerp(startTemperature, tempTarget, t);
                survivalStatsProvider.SetTemperature(targetTemp);
                isRestoring = true;
            }

            if (cureInfection && survivalStatsProvider.Infection < survivalStatsProvider.MaxInfection)
            {
                float targetInfection = Mathf.Lerp(startInfection, survivalStatsProvider.MaxInfection, t);
                survivalStatsProvider.SetInfection(targetInfection);
                isRestoring = true;
            }

            if (restoreHunger && survivalStatsProvider.Hunger < survivalStatsProvider.MaxHunger)
            {
                float targetHunger = Mathf.Lerp(startHunger, survivalStatsProvider.MaxHunger, t);
                survivalStatsProvider.SetHunger(targetHunger);
                isRestoring = true;
            }

            if (restoreThirst && survivalStatsProvider.Thirst < survivalStatsProvider.MaxThirst)
            {
                float targetThirst = Mathf.Lerp(startThirst, survivalStatsProvider.MaxThirst, t);
                survivalStatsProvider.SetThirst(targetThirst);
                isRestoring = true;
            }
        }
        else if (survivalManager != null)
        {
            if (restoreStamina && survivalManager.currentStamina < survivalManager.maxStamina)
            {
                survivalManager.SetStamina(Mathf.Lerp(startStamina, survivalManager.maxStamina, t));
                isRestoring = true;
            }

            if (normalizeTemperature && survivalManager.currentTemperature < survivalManager.normalTemperature)
            {
                survivalManager.SetTemperature(Mathf.Lerp(startTemperature, survivalManager.normalTemperature, t));
                isRestoring = true;
            }

            if (cureInfection && survivalManager.currentInfection < survivalManager.maxInfection)
            {
                survivalManager.SetInfection(Mathf.Lerp(startInfection, survivalManager.maxInfection, t));
                isRestoring = true;
            }

            if (restoreHunger && survivalManager.currentHunger < survivalManager.maxHunger)
            {
                survivalManager.SetHunger(Mathf.Lerp(startHunger, survivalManager.maxHunger, t));
                isRestoring = true;
            }

            if (restoreThirst && survivalManager.currentThirst < survivalManager.maxThirst)
            {
                survivalManager.SetThirst(Mathf.Lerp(startThirst, survivalManager.maxThirst, t));
                isRestoring = true;
            }
        }

        if (isRestoring)
        {
            if (activeHealingEffect == null)
                StartHealing();
        }
        else if (activeHealingEffect != null)
        {
            StopHealing();
            onRestoreComplete.Invoke();
        }
    }

    private bool IsAllStatsAtTarget()
    {
        if (restoreHealth && playerProvider != null && playerProvider.IsAlive)
        {
            if (playerProvider.Health < playerProvider.MaxHealth)
                return false;
        }

        if (survivalStatsProvider != null)
        {
            if (restoreStamina && survivalStatsProvider.Stamina < survivalStatsProvider.MaxStamina)
                return false;

            float tempTarget = survivalManager != null
                ? Mathf.Clamp(survivalManager.normalTemperature, 0f, Mathf.Max(1f, survivalStatsProvider.MaxTemperature))
                : survivalStatsProvider.MaxTemperature;

            if (normalizeTemperature && survivalStatsProvider.Temperature < tempTarget)
                return false;

            if (cureInfection && survivalStatsProvider.Infection < survivalStatsProvider.MaxInfection)
                return false;

            if (restoreHunger && survivalStatsProvider.Hunger < survivalStatsProvider.MaxHunger)
                return false;

            if (restoreThirst && survivalStatsProvider.Thirst < survivalStatsProvider.MaxThirst)
                return false;
        }
        else if (survivalManager != null)
        {
            if (restoreStamina && survivalManager.currentStamina < survivalManager.maxStamina)
                return false;

            if (normalizeTemperature && survivalManager.currentTemperature < survivalManager.normalTemperature)
                return false;

            if (cureInfection && survivalManager.currentInfection < survivalManager.maxInfection)
                return false;

            if (restoreHunger && survivalManager.currentHunger < survivalManager.maxHunger)
                return false;

            if (restoreThirst && survivalManager.currentThirst < survivalManager.maxThirst)
                return false;
        }

        return true;
    }

    private void StartHealing()
    {
        if (healingEffect != null && activeHealingEffect == null)
        {
            Transform healParent = playerTransform != null ? playerTransform : transform;
            Vector3 spawnPosition = healParent.position;
            activeHealingEffect = Instantiate(healingEffect, spawnPosition, Quaternion.identity, healParent);
        }

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
        MessageDisplay display = messageDisplay != null ? messageDisplay.GetComponent<MessageDisplay>() : null;

        if (display != null)
            display.ShowMessage(message, messageDuration);
        else
            Debug.Log($"<color=cyan>[Safe Zone] {message}</color>");
    }

    private static GameObject ResolvePlayerRoot(Collider other)
    {
        if (other == null) return null;

        if (other.attachedRigidbody != null)
            return other.attachedRigidbody.gameObject;

        Transform root = other.transform.root;
        return root != null ? root.gameObject : other.gameObject;
    }

    private static IPlayerProvider ResolvePlayerProvider(GameObject playerGO)
    {
        if (playerGO == null) return FindAnyPlayerProvider();

        if (playerGO.TryGetComponent(out GC2PlayerProvider gc2Local))
            return gc2Local;

        if (playerGO.TryGetComponent(out MonoBehaviour localBehaviour) && localBehaviour is IPlayerProvider localProvider)
            return localProvider;

        MonoBehaviour[] parentBehaviours = playerGO.GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < parentBehaviours.Length; i++)
        {
            if (parentBehaviours[i] is IPlayerProvider provider)
                return provider;
        }

        MonoBehaviour[] localBehaviours = playerGO.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < localBehaviours.Length; i++)
        {
            if (localBehaviours[i] is IPlayerProvider provider)
                return provider;
        }

        return FindAnyPlayerProvider();
    }

    private static ISurvivalStatsProvider ResolveSurvivalProvider(GameObject playerGO, IPlayerProvider resolvedPlayerProvider)
    {
        if (resolvedPlayerProvider is ISurvivalStatsProvider fromPlayerProvider)
            return fromPlayerProvider;

        if (playerGO != null)
        {
            if (playerGO.TryGetComponent(out GC2PlayerProvider gc2Provider))
                return gc2Provider;

            MonoBehaviour[] parentBehaviours = playerGO.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < parentBehaviours.Length; i++)
            {
                if (parentBehaviours[i] is ISurvivalStatsProvider provider)
                    return provider;
            }

            MonoBehaviour[] localBehaviours = playerGO.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < localBehaviours.Length; i++)
            {
                if (localBehaviours[i] is ISurvivalStatsProvider provider)
                    return provider;
            }
        }

        SurvivalManager survival = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();
        if (survival != null && survival.playerProviderObject is ISurvivalStatsProvider survivalProvider)
            return survivalProvider;

        return FindAnySurvivalProvider();
    }

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < allMonoBehaviours.Length; i++)
        {
            if (allMonoBehaviours[i] is IPlayerProvider provider)
                return provider;
        }

        return null;
    }

    private static ISurvivalStatsProvider FindAnySurvivalProvider()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < allMonoBehaviours.Length; i++)
        {
            if (allMonoBehaviours[i] is ISurvivalStatsProvider provider)
                return provider;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = healingColor;
        Collider col = triggerCollider != null ? triggerCollider : GetComponent<Collider>();
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
        Collider col = triggerCollider != null ? triggerCollider : GetComponent<Collider>();
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
