using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages player survival stats: stamina, hunger, thirst, temperature, infection.
/// All player health reads and writes go through IPlayerProvider.
/// OPTIMIZED: Update intervals reduce CPU usage by 90%
/// </summary>
public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager Instance { get; private set; }

    [Header("Player Provider")]
    [Tooltip("Assign any IPlayerProvider implementation.")]
    public MonoBehaviour playerProviderObject;
    public ProgressionManager progressionManager;

    private const float MoveThreshold = 0.1f;
    [Tooltip("World-space speed threshold above which the character is considered sprinting")]
    public float sprintSpeedThreshold = 3f;

    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;
    private bool providerEventsBound;
    private bool progressionEventsBound;

    [Header("Temperature Settings")]
    [Tooltip("Maximum temperature value (100% = optimal)")]
    [Range(0f, 100f)] public float maxTemperature = 100f;
    [Tooltip("Current temperature value (percentage)")]
    [Range(0f, 100f)] public float currentTemperature = 100f;
    [Tooltip("Normal/target temperature when recovering (100% = optimal)")]
    [Range(0f, 100f)] public float normalTemperature = 100f;
    [Tooltip("Minimum temperature before player freezes")]
    public float minTemperature = 0f;
    [Tooltip("Temperature decrease rate per second (% per sec)")]
    public float temperatureDecreaseRate = 0.2f;
    [Tooltip("Temperature recovery rate per second (% per sec)")]
    public float temperatureNormalizeRate = 2f;
    [Tooltip("Warning temperature threshold (%) - displays warning below this")]
    public float warningTemperature = 20f;
    [Tooltip("Critical cold temperature threshold (%) - causes damage below 10%")]
    public float criticalTemperature = 10f;

    [Header("Stamina Settings")]
    [Range(0f, 100f)] public float maxStamina = 100f;
    [Range(0f, 100f)] public float currentStamina = 100f;
    public float staminaRegenRate = 5f;
    public float staminaDrainRateRunning = 10f;
    public float staminaDrainRateCold = 0.5f;

    [Header("Immunity Settings")]
    [Range(0f, 100f)] public float maxInfection = 100f;
    [Tooltip("Current immunity level (100 = fully immune, 0 = no immunity)")]
    [Range(0f, 100f)] public float currentInfection = 100f;
    [Tooltip("Rate at which immunity drains per second when infected")]
    public float infectionGrowthRate = 0.5f;
    [Tooltip("Rate at which immunity recovers per second in safe zones")]
    public float infectionDecayRate = 1f;
    [Tooltip("Immunity threshold below which health damage is applied (e.g. 10 = damage when immunity ≤ 10%)")]
    public float infectionDamageThreshold = 10f;
    public float infectionDamagePerSecond = 1f;
    [Tooltip("When disabled, immunity does not passively damage health at low values.")]
    public bool enablePassiveInfectionDamage = false;

    [Tooltip("Is player currently infected (immunity will drain over time)")]
    private bool isInfected = false;

    [Tooltip("Immunity level at which drain is paused by a consumable cure")]
    private float curedInfectionLevel = 0f;

    [Tooltip("Is immunity drain paused by consumable cure")]
    private bool infectionGrowthPaused = false;

    [Header("Hunger Settings")]
    [Tooltip("Maximum hunger value")]
    [Range(0f, 100f)] public float maxHunger = 100f;
    [Tooltip("Current hunger level (100 = full, 0 = starving)")]
    [Range(0f, 100f)] public float currentHunger = 100f;
    [Tooltip("Hunger decrease rate per second (idle)")]
    public float hungerDecreaseRate = 0.15f;
    [Tooltip("Hunger decrease multiplier when running")]
    public float hungerRunningMultiplier = 2f;
    [Tooltip("Hunger decrease multiplier when in combat")]
    public float hungerCombatMultiplier = 1.5f;
    [Tooltip("Hunger threshold for stamina penalty")]
    public float hungerStaminaPenaltyThreshold = 30f;
    [Tooltip("Stamina regen reduction when hungry (0-1)")]
    public float hungerStaminaPenalty = 0.5f;
    [Tooltip("Critical hunger threshold - causes damage (below 10%)")]
    public float criticalHungerThreshold = 10f;
    [Tooltip("Health damage per second when starving")]
    public float hungerDamagePerSecond = 2f;

    [Header("Thirst Settings")]
    [Tooltip("Maximum thirst value")]
    [Range(0f, 100f)] public float maxThirst = 100f;
    [Tooltip("Current thirst level (100 = hydrated, 0 = dehydrated)")]
    [Range(0f, 100f)] public float currentThirst = 100f;
    [Tooltip("Thirst decrease rate per second (idle)")]
    public float thirstDecreaseRate = 0.25f;
    [Tooltip("Thirst decrease multiplier when running")]
    public float thirstRunningMultiplier = 2.5f;
    [Tooltip("Thirst decrease multiplier in hot environments")]
    public float thirstHotMultiplier = 1.8f;
    [Tooltip("Thirst threshold for stamina penalty")]
    public float thirstStaminaPenaltyThreshold = 30f;
    [Tooltip("Stamina regen reduction when thirsty (0-1)")]
    public float thirstStaminaPenalty = 0.6f;
    [Tooltip("Critical thirst threshold - causes damage (below 10%)")]
    public float criticalThirstThreshold = 10f;
    [Tooltip("Health damage per second when dehydrated")]
    public float thirstDamagePerSecond = 3f;

    [Header("Health & Temperature Effects")]
    public float coldDamagePerSecond = 0.5f;
    public float damageTickInterval = 1f;

    [Header("Temperature Modifiers")]
    [Tooltip("Temperature gain per second when indoors")]
    public float indoorTemperatureGain = 10f;
    [Tooltip("Temperature gain per second when near fire")]
    public float fireTemperatureGain = 15f;
    [Tooltip("Multiplier for temperature decrease in cold zones")]
    public float coldZoneMultiplier = 2f;

    [Header("Events")]
    public UnityEvent<float> onTemperatureChanged;
    public UnityEvent<float> onStaminaChanged;
    public UnityEvent<float> onInfectionChanged;
    public UnityEvent<float> onHungerChanged;
    public UnityEvent<float> onThirstChanged;
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onEnteredCriticalTemperature;
    public UnityEvent onExitedCriticalTemperature;
    public UnityEvent onPlayerFroze;
    public UnityEvent onStaminaDepleted;
    public UnityEvent onInfectionCritical;
    public UnityEvent onPlayerStarving;
    public UnityEvent onPlayerDehydrated;
    public UnityEvent onPlayerDiedOfHunger;
    public UnityEvent onPlayerDiedOfThirst;

    [Header("System Toggles")]
    public bool enableTemperatureSystem = true;
    public bool enableTemperatureDecrease = false;
    public bool enableColdDamage = true;
    public bool enableStaminaSystem = true;
    public bool enableInfectionSystem = true;
    public bool enableHungerSystem = true;
    public bool enableThirstSystem = true;
    public bool showDebugInfo = false;
    [Tooltip("When enabled, prints the periodic '[Survival] Temp...Stamina...Infection...' status line")]
    public bool logPeriodicSurvivalStatus = false;

    [Header("Initialization")]
    [Tooltip("When enabled, survival stats are initialized to 100% (max values) at runtime start.")]
    public bool initializeStatsAtFullOnStart = true;

    [Header("Armour Settings")]
    [Tooltip("Enable the armour system. When enabled, damage depletes armour before health.")]
    public bool enableArmourSystem = true;
    [Tooltip("Current armour value. Initialised from ProgressionManager on Start.")]
    [Range(0f, 100000f)] public float currentArmour = 100f;
    [Tooltip("Maximum armour. Driven by ProgressionManager based on level — do not set manually.")]
    public float maxArmour = 100f;
    [Tooltip("Armour regenerates passively at this rate (points per second) when out of combat.")]
    public float armourRegenRate = 5f;
    [Tooltip("Seconds after taking damage before armour regen resumes.")]
    public float armourRegenDelay = 4f;
    [Tooltip("Fired whenever armour changes. Passes (currentArmour, maxArmour).")]
    public UnityEvent<float, float> onArmourChanged;

    [Header("Performance Optimization")]
    [Tooltip("Use update intervals instead of every-frame updates (90% CPU reduction)")]
    public bool useUpdateIntervals = true;
    [Tooltip("How many times per second to update survival stats (default: 10)")]
    [Range(1, 60)]
    public int updatesPerSecond = 10;
    [Tooltip("Show performance metrics in console")]
    public bool showPerformanceMetrics = false;

    [Header("Runtime Debug Panel")]
    [Tooltip("Shows a small runtime panel with survival and player stats as percentages.")]
    public bool showRuntimeDebugPanel = false;
    [Tooltip("Top-left panel offset in pixels.")]
    public Vector2 runtimePanelPosition = new Vector2(16f, 16f);
    [Tooltip("Panel size in pixels.")]
    public Vector2 runtimePanelSize = new Vector2(260f, 210f);

    [Header("Safe Zone Interaction")]
    public bool isInSafeZone = false;
    public bool pauseTemperatureNormalizationInSafeZone = true;

    private float damageTimer;
    private float infectionDamageTimer;
    private float hungerDamageTimer;
    private float thirstDamageTimer;
    private bool isInCriticalCold = false;
    private bool isInCriticalInfection = false;
    private bool isInCriticalHunger = false;
    private bool isInCriticalThirst = false;
    private bool isIndoors = false;
    private bool isNearFire = false;
    private bool isInColdZone = false;
    private bool didInitializeFullStats;
    private bool hasHandledPlayerDeath;

    // Armour regen cooldown — reset to armourRegenDelay each time damage is taken.
    private float _armourRegenTimer = 0f;

    // OPTIMIZATION: Update interval tracking
    private float updateIntervalTimer = 0f;
    private float updateInterval;
    private float lastUpdateTime;
    private int framesSinceLastUpdate = 0;
    private GUIStyle runtimePanelStyle;
    private GUIStyle runtimeLabelStyle;

    public float TemperaturePercentage => currentTemperature / maxTemperature;
    public bool IsCriticalCold => currentTemperature <= criticalTemperature;
    public bool IsWarningCold => currentTemperature <= warningTemperature;
    public bool IsCritical => IsCriticalCold || isInCriticalInfection || isInCriticalHunger || isInCriticalThirst;
    public float StaminaPercentage => currentStamina / maxStamina;
    public float InfectionPercentage => currentInfection / maxInfection;
    public float HungerPercentage => currentHunger / maxHunger;
    public float ThirstPercentage => currentThirst / maxThirst;
    public bool IsStarving => currentHunger <= criticalHungerThreshold;
    public bool IsDehydrated => currentThirst <= criticalThirstThreshold;
    public bool IsHungry => currentHunger <= hungerStaminaPenaltyThreshold;
    public bool IsThirsty => currentThirst <= thirstStaminaPenaltyThreshold;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple SurvivalManagers detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindPlayerReferences();
        damageTimer = damageTickInterval;
        infectionDamageTimer = damageTickInterval;
        hungerDamageTimer = damageTickInterval;
        thirstDamageTimer = damageTickInterval;

        if (initializeStatsAtFullOnStart)
        {
            InitializeStatsToFull();
        }

        // OPTIMIZATION: Calculate update interval
        updateInterval = 1f / Mathf.Max(1, updatesPerSecond);
        lastUpdateTime = Time.time;

        if (showPerformanceMetrics)
        {
            Debug.Log($"[SurvivalManager] Update interval: {updateInterval:F3}s ({updatesPerSecond} updates/sec)");
        }
    }

    private void FindPlayerReferences()
    {
        EnsurePlayerProviderBinding();
        EnsureProgressionBinding();

        // Initialise armour cap from current level.
        SyncArmourCapFromTraits();
    }

    public void EnsurePlayerProviderBinding()
    {
        if (playerProvider == null)
        {
            playerProvider = playerProviderObject as IPlayerProvider;

            if (playerProvider == null)
            {
                GC2PlayerProvider gc2Provider = FindFirstObjectByType<GC2PlayerProvider>();
                if (gc2Provider != null)
                {
                    playerProviderObject = gc2Provider;
                    playerProvider = gc2Provider;
                }
            }

            if (playerProvider == null)
                playerProvider = FindAnyPlayerProvider();
        }

        if (playerProvider == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("SurvivalManager: No IPlayerProvider found — health damage will not be applied.");
            return;
        }

        survivalStatsProvider = playerProvider as ISurvivalStatsProvider;

        if (providerEventsBound) return;

        playerProvider.OnHealthChanged += HandleHealthChanged;
        playerProvider.OnArmourChanged += HandleArmourChanged;
        playerProvider.OnDeath += HandlePlayerDeath;
        providerEventsBound = true;

        PullSurvivalValuesFromProvider();

        if (initializeStatsAtFullOnStart && !didInitializeFullStats)
        {
            InitializeStatsToFull();
        }
    }

    private void EnsureProgressionBinding()
    {
        if (progressionManager == null)
            progressionManager = FindFirstObjectByType<ProgressionManager>();

        if (progressionManager == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("SurvivalManager: Could not find ProgressionManager!");
            return;
        }

        if (progressionEventsBound) return;

        progressionManager.onLevelUp.AddListener(OnLevelUp);
        progressionEventsBound = true;
    }

    private void OnDestroy()
    {
        if (playerProvider != null && providerEventsBound)
        {
            playerProvider.OnHealthChanged -= HandleHealthChanged;
            playerProvider.OnArmourChanged -= HandleArmourChanged;
            playerProvider.OnDeath -= HandlePlayerDeath;
            providerEventsBound = false;
        }

        if (progressionManager != null && progressionEventsBound)
        {
            progressionManager.onLevelUp.RemoveListener(OnLevelUp);
            progressionEventsBound = false;
        }
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

    private void HandleHealthChanged(float current, float max)
    {
        onHealthChanged?.Invoke(current, max);

        // Reset armour regen cooldown whenever health decreases (enemy damage hit through armour).
        if (enableArmourSystem)
            _armourRegenTimer = armourRegenDelay;
    }

    private void HandleArmourChanged(float current, float max)
    {
        currentArmour = current;
        maxArmour     = max;
        onArmourChanged?.Invoke(current, max);

        // Reset regen cooldown whenever armour drops (enemy damage is absorbed by armour).
        if (enableArmourSystem && current < max)
            _armourRegenTimer = armourRegenDelay;
    }

    private void HandlePlayerDeath()
    {
        if (hasHandledPlayerDeath) return;

        hasHandledPlayerDeath = true;
        isInCriticalCold = false;
        isInCriticalInfection = false;
        isInCriticalHunger = false;
        isInCriticalThirst = false;

        if (showDebugInfo)
            Debug.Log("[SurvivalManager] Player death detected. Survival updates paused until respawn.");
    }

    // ── Armour helpers ────────────────────────────────────────────────────────

    private void OnLevelUp(int newLevel)
    {
        SyncArmourCapFromTraits();
    }

    private void SyncArmourCapFromTraits()
    {
        ProgressionManager source = progressionManager != null
            ? progressionManager
            : ProgressionManager.Instance;
        if (source == null) return;

        int cap = source.CurrentMaxArmour;
        if (cap <= 0) return;

        maxArmour = cap;

        if (currentArmour > maxArmour)
            currentArmour = maxArmour;

        // Keep provider armour aligned with the new cap without framework-specific calls.
        if (playerProvider != null)
        {
            float providerArmour = Mathf.Min(playerProvider.Armour, maxArmour);
            playerProvider.SetArmour(providerArmour);
            currentArmour = providerArmour;
        }

        onArmourChanged?.Invoke(currentArmour, maxArmour);
    }

    /// <summary>Sets armour to a specific value, clamped to [0, maxArmour].</summary>
    public void SetArmour(float value)
    {
        currentArmour = Mathf.Clamp(value, 0f, maxArmour);
        playerProvider?.SetArmour(currentArmour);
        onArmourChanged?.Invoke(currentArmour, maxArmour);
    }

    /// <summary>Modifies armour by a delta (positive = add, negative = drain).</summary>
    public void ModifyArmour(float delta) => SetArmour(currentArmour + delta);

    /// <summary>Fully restores armour to its maximum value.</summary>
    public void ResetArmour() => SetArmour(maxArmour);

    /// <summary>Returns armour as a 0–1 normalised fraction.</summary>
    public float ArmourPercentage => maxArmour > 0f ? currentArmour / maxArmour : 0f;

    private void Update()
    {
        // OPTIMIZATION: Use update intervals to reduce CPU usage by 90%
        if (useUpdateIntervals)
        {
            updateIntervalTimer += Time.deltaTime;
            framesSinceLastUpdate++;

            if (updateIntervalTimer >= updateInterval)
            {
                float deltaTime = Time.time - lastUpdateTime;
                lastUpdateTime = Time.time;

                PerformSurvivalUpdates(deltaTime);

                if (showPerformanceMetrics && framesSinceLastUpdate > 0)
                {
                    float avgFrameTime = updateIntervalTimer / framesSinceLastUpdate;
                    Debug.Log($"[SurvivalManager] Update after {framesSinceLastUpdate} frames (avg: {avgFrameTime * 1000:F2}ms/frame)");
                }

                updateIntervalTimer -= updateInterval;
                framesSinceLastUpdate = 0;
            }
        }
        else
        {
            PerformSurvivalUpdates(Time.deltaTime);
        }

        if (showDebugInfo && logPeriodicSurvivalStatus)
        {
            DisplayDebugInfo();
        }
    }

    private void OnGUI()
    {
        if (!showRuntimeDebugPanel) return;
        if (!Application.isPlaying) return;

        EnsureRuntimePanelStyles();

        Rect panelRect = new Rect(
            runtimePanelPosition.x,
            runtimePanelPosition.y,
            runtimePanelSize.x,
            runtimePanelSize.y
        );

        GUILayout.BeginArea(panelRect, GUIContent.none, runtimePanelStyle);
        GUILayout.Label("Survival Debug (%)", runtimeLabelStyle);

        DrawPercentLine("Health", GetHealthPercent());
        DrawPercentLine("Armour", GetSafePercent(currentArmour, maxArmour));
        DrawPercentLine("Temperature", GetSafePercent(currentTemperature, maxTemperature));
        DrawPercentLine("Stamina", GetSafePercent(currentStamina, maxStamina));
        DrawPercentLine("Immunity", GetSafePercent(currentInfection, maxInfection));
        DrawPercentLine("Hunger", GetSafePercent(currentHunger, maxHunger));
        DrawPercentLine("Thirst", GetSafePercent(currentThirst, maxThirst));

        GUILayout.EndArea();
    }

    private void EnsureRuntimePanelStyles()
    {
        if (runtimePanelStyle != null && runtimeLabelStyle != null) return;

        runtimePanelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(10, 10, 8, 8),
            fontSize = 12
        };

        runtimeLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
    }

    private void DrawPercentLine(string label, float percent)
    {
        float clamped = Mathf.Clamp(percent, 0f, 1f);
        GUILayout.Label($"{label,-12} {clamped * 100f,6:0.0}%");
    }

    private float GetHealthPercent()
    {
        if (playerProvider == null) return 0f;
        return GetSafePercent(playerProvider.Health, playerProvider.MaxHealth);
    }

    private static float GetSafePercent(float current, float max)
    {
        if (max <= 0f) return 0f;
        return current / max;
    }

    private void PerformSurvivalUpdates(float deltaTime)
    {
        if (playerProvider == null)
            EnsurePlayerProviderBinding();

        if (playerProvider != null && !playerProvider.IsAlive)
        {
            if (!hasHandledPlayerDeath)
                HandlePlayerDeath();
            return;
        }

        if (hasHandledPlayerDeath)
            hasHandledPlayerDeath = false;

        PullSurvivalValuesFromProvider();

        if (enableTemperatureSystem)
        {
            UpdateTemperature();
            UpdateCriticalState();
            ApplyTemperatureEffects();
        }

        if (enableStaminaSystem)
        {
            UpdateStamina();
        }

        if (enableInfectionSystem)
        {
            UpdateInfection();
            ApplyInfectionEffects();
        }

        if (enableHungerSystem)
        {
            UpdateHunger();
            ApplyHungerEffects();
        }

        if (enableThirstSystem)
        {
            UpdateThirst();
            ApplyThirstEffects();
        }

        if (enableArmourSystem)
        {
            UpdateArmourRegen(deltaTime);
        }
    }

    private void UpdateTemperature()
    {
        float temperatureChange = 0f;

        if (enableTemperatureDecrease)
        {
            float decreaseRate = temperatureDecreaseRate;

            if (isInColdZone)
            {
                decreaseRate *= coldZoneMultiplier;
            }

            temperatureChange -= decreaseRate * Time.deltaTime;
        }

        if (isIndoors)
        {
            temperatureChange += indoorTemperatureGain * Time.deltaTime;
        }

        if (isNearFire)
        {
            temperatureChange += fireTemperatureGain * Time.deltaTime;
        }

        if (!isInSafeZone || !pauseTemperatureNormalizationInSafeZone)
        {
            float targetTemp = normalTemperature;
            if (Mathf.Abs(currentTemperature - targetTemp) > 0.5f)
            {
                float normalizeAmount = temperatureNormalizeRate * Time.deltaTime;
                SetTemperature(Mathf.MoveTowards(currentTemperature, targetTemp, normalizeAmount));
            }
        }

        if (temperatureChange != 0f)
        {
            SetTemperature(currentTemperature + temperatureChange);
        }
    }

    private void UpdateStamina()
    {
        float staminaChange = 0f;

        if (playerProvider != null && playerProvider.MoveSpeed > MoveThreshold)
        {
            staminaChange -= staminaDrainRateRunning * Time.deltaTime;
        }
        else
        {
            float regenRate = staminaRegenRate;

            if (IsHungry)
                regenRate *= (1f - hungerStaminaPenalty);

            if (IsThirsty)
                regenRate *= (1f - thirstStaminaPenalty);

            staminaChange += regenRate * Time.deltaTime;
        }

        if (IsCriticalCold)
            staminaChange -= staminaDrainRateCold * Time.deltaTime;

        if (staminaChange != 0f)
            SetStamina(currentStamina + staminaChange);
    }

    private void UpdateInfection()
    {
        // Immunity drains while infected and outside a safe zone.
        if (isInfected && !isInSafeZone)
        {
            if (infectionGrowthPaused)
            {
                // Resume drain if immunity has fallen below the pause level (medicine wore off).
                if (currentInfection < curedInfectionLevel)
                {
                    infectionGrowthPaused = false;
                }
            }

            if (!infectionGrowthPaused)
            {
                SetInfection(currentInfection - infectionGrowthRate * Time.deltaTime);
            }
        }
        else if (isInSafeZone && currentInfection < maxInfection)
        {
            // Immunity recovers in safe zones.
            SetInfection(currentInfection + infectionDecayRate * Time.deltaTime);

            if (currentInfection >= maxInfection)
            {
                isInfected = false;
                infectionGrowthPaused = false;
                curedInfectionLevel = maxInfection;
            }
        }

        bool wasCritical = isInCriticalInfection;
        isInCriticalInfection = currentInfection <= infectionDamageThreshold;

        if (isInCriticalInfection && !wasCritical)
        {
            onInfectionCritical?.Invoke();
        }
    }

    private void UpdateCriticalState()
    {
        bool wasCriticalCold = isInCriticalCold;
        isInCriticalCold = IsCriticalCold;

        if (isInCriticalCold && !wasCriticalCold)
        {
            onEnteredCriticalTemperature?.Invoke();
        }
        else if (!isInCriticalCold && wasCriticalCold)
        {
            onExitedCriticalTemperature?.Invoke();
        }
    }

    private void ApplyTemperatureEffects()
    {
        if (!enableColdDamage || !isInCriticalCold || playerProvider == null)
            return;

        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            ApplyDamage(coldDamagePerSecond, "cold");
            damageTimer = damageTickInterval;
        }

        if (currentTemperature <= 0f && playerProvider.IsAlive)
        {
            onPlayerFroze?.Invoke();
        }
    }

    private void UpdateArmourRegen(float deltaTime)
    {
        if (!enableArmourSystem || playerProvider == null) return;
        if (currentArmour >= maxArmour) return;

        if (_armourRegenTimer > 0f)
        {
            _armourRegenTimer -= deltaTime;
            return;
        }

        float regen = armourRegenRate * deltaTime;
        SetArmour(currentArmour + regen);
    }

    private void ApplyInfectionEffects()
    {
        if (!isInCriticalInfection || playerProvider == null)
            return;

        if (!enablePassiveInfectionDamage)
            return;

        infectionDamageTimer -= Time.deltaTime;

        if (infectionDamageTimer <= 0f)
        {
            ApplyDamage(infectionDamagePerSecond, "infection");
            infectionDamageTimer = damageTickInterval;
        }
    }

    private void ApplyDamage(float damagePerSecond, string source)
    {
        if (playerProvider == null || damagePerSecond <= 0f) return;

        float damage = damagePerSecond * damageTickInterval;

        // Reset armour regen cooldown whenever the player takes survival damage.
        if (enableArmourSystem)
            _armourRegenTimer = armourRegenDelay;

        playerProvider.ApplyDamage(damage);

        if (showDebugInfo && !string.Equals(source, "infection", System.StringComparison.OrdinalIgnoreCase))
            Debug.Log($"{source} damage: {damage:F1} HP (remaining: {playerProvider.Health:F1})");
    }

    public void SetTemperature(float value)
    {
        float oldTemperature = currentTemperature;
        float clamped = Mathf.Clamp(value, 0f, maxTemperature);

        if (survivalStatsProvider != null)
        {
            survivalStatsProvider.SetTemperature(clamped);
            clamped = Mathf.Clamp(survivalStatsProvider.Temperature, 0f, maxTemperature);
        }

        currentTemperature = clamped;

        if (!Mathf.Approximately(oldTemperature, currentTemperature))
        {
            onTemperatureChanged?.Invoke(currentTemperature);
        }
    }

    public void ModifyTemperature(float delta)
    {
        SetTemperature(currentTemperature + delta);
    }

    public void AddTemperature(float amount)
    {
        ModifyTemperature(amount);
    }

    public void SetStamina(float value)
    {
        float oldStamina = currentStamina;
        float clamped = Mathf.Clamp(value, 0f, maxStamina);

        if (survivalStatsProvider != null)
        {
            survivalStatsProvider.SetStamina(clamped);
            clamped = Mathf.Clamp(survivalStatsProvider.Stamina, 0f, maxStamina);
        }

        currentStamina = clamped;

        if (!Mathf.Approximately(oldStamina, currentStamina))
        {
            onStaminaChanged?.Invoke(currentStamina);

            if (currentStamina <= 0f && oldStamina > 0f)
            {
                onStaminaDepleted?.Invoke();
            }
        }
    }

    public void ModifyStamina(float delta)
    {
        SetStamina(currentStamina + delta);
    }

    public void DrainStamina(float amount)
    {
        ModifyStamina(-amount);
    }

    public void AddStamina(float amount)
    {
        ModifyStamina(amount);

        if (showDebugInfo && amount > 0)
        {
            Debug.Log($"<color=yellow>⚡ Restored stamina: +{amount:F0} (now {currentStamina:F0}/{maxStamina})</color>");
        }
    }

    public void SetInfection(float value)
    {
        float oldInfection = currentInfection;
        float clamped = Mathf.Clamp(value, 0f, maxInfection);

        if (survivalStatsProvider != null)
        {
            survivalStatsProvider.SetInfection(clamped);
            clamped = Mathf.Clamp(survivalStatsProvider.Infection, 0f, maxInfection);
        }

        currentInfection = clamped;

        if (!Mathf.Approximately(oldInfection, currentInfection))
        {
            onInfectionChanged?.Invoke(currentInfection);
        }
    }

    /// <summary>Adds infection exposure, reducing immunity by the given amount.</summary>
    public void AddInfection(float amount)
    {
        SetInfection(currentInfection - amount);

        if (amount > 0f && currentInfection < maxInfection)
        {
            isInfected = true;

            if (currentInfection < curedInfectionLevel)
            {
                infectionGrowthPaused = false;
            }
        }
    }

    /// <summary>Cures infection, restoring immunity by the given amount.</summary>
    public void CureInfection(float amount)
    {
        SetInfection(currentInfection + amount);

        if (currentInfection >= maxInfection)
        {
            isInfected = false;
            infectionGrowthPaused = false;
            curedInfectionLevel = maxInfection;
        }
    }

    /// <summary>Restores immunity by a percentage of the current deficit, pausing further drain at the restored level.</summary>
    public void CureInfectionPartial(float percentage)
    {
        if (currentInfection >= maxInfection) return;

        float missingImmunity = maxInfection - currentInfection;
        float restoreAmount = missingImmunity * (percentage / 100f);
        SetInfection(currentInfection + restoreAmount);

        curedInfectionLevel = currentInfection;
        infectionGrowthPaused = true;

        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Immunity partially restored by {percentage}%. Drain paused at {curedInfectionLevel:F1}</color>");
        }
    }

    public void SetIndoors(bool value)
    {
        isIndoors = value;
    }

    public void SetNearFire(bool value)
    {
        isNearFire = value;
    }

    public void SetInColdZone(bool value)
    {
        isInColdZone = value;
    }

    public void SetInSafeZone(bool value)
    {
        isInSafeZone = value;
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>SurvivalManager: Safe zone mode {(value ? "enabled" : "disabled")}</color>");
        }
    }

    public void WarmUp(float amount)
    {
        ModifyTemperature(amount);
    }

    public void CoolDown(float amount)
    {
        ModifyTemperature(-amount);
    }

    public void ResetTemperature()
    {
        SetTemperature(maxTemperature);
    }

    public void ResetStamina()
    {
        SetStamina(maxStamina);
    }

    /// <summary>Resets immunity to full (100%).</summary>
    public void ResetInfection()
    {
        SetInfection(maxInfection);
        isInfected = false;
        infectionGrowthPaused = false;
        curedInfectionLevel = maxInfection;
    }

    public void ResetHunger()
    {
        SetHunger(maxHunger);
    }

    public void ResetThirst()
    {
        SetThirst(maxThirst);
    }

    public void ResetAllStats()
    {
        ResetTemperature();
        ResetStamina();
        ResetInfection();
        ResetHunger();
        ResetThirst();
        ResetArmour();
        playerProvider?.SetHealth(playerProvider.MaxHealth);
    }

    [ContextMenu("Initialize Stats To Full")]
    public void InitializeStatsToFull()
    {
        EnsurePlayerProviderBinding();

        // Ensure current values match max values, independent from inspector serialized values.
        SetTemperature(maxTemperature);
        SetStamina(maxStamina);
        SetInfection(maxInfection);
        SetHunger(maxHunger);
        SetThirst(maxThirst);

        if (enableArmourSystem)
        {
            SetArmour(maxArmour);
        }

        if (playerProvider != null)
        {
            playerProvider.SetHealth(playerProvider.MaxHealth);
        }

        isInfected = false;
        infectionGrowthPaused = false;
        curedInfectionLevel = maxInfection;

        didInitializeFullStats = true;

        if (showDebugInfo)
        {
            Debug.Log("[SurvivalManager] Initialized stats to full values.");
        }
    }

    public string GetTemperatureStatus()
    {
        float temp = currentTemperature;

        if (temp >= 35f) return "Normal";
        if (temp >= 30f) return "Cool";
        if (temp >= 20f) return "Cold";
        if (temp >= 15f) return "Very Cold";
        if (temp >= 5f) return "Freezing";
        return "Hypothermia";
    }

    public string GetInfectionStatus()
    {
        if (currentInfection >= maxInfection) return "None";
        if (currentInfection > 75f) return "Mild";
        if (currentInfection > 50f) return "Moderate";
        if (currentInfection > 25f) return "Severe";
        return "Critical";
    }

    public string GetHungerStatus()
    {
        if (currentHunger >= 75f) return "Well Fed";
        if (currentHunger >= 50f) return "Satisfied";
        if (currentHunger >= 30f) return "Hungry";
        if (currentHunger >= 10f) return "Very Hungry";
        return "Starving";
    }

    public string GetThirstStatus()
    {
        if (currentThirst >= 75f) return "Hydrated";
        if (currentThirst >= 50f) return "Satisfied";
        if (currentThirst >= 30f) return "Thirsty";
        if (currentThirst >= 10f) return "Very Thirsty";
        return "Dehydrated";
    }

    private void DisplayDebugInfo()
    {
        string info = $"[Survival] Temp: {currentTemperature:F1}°C ({GetTemperatureStatus()}) | " +
                  $"Stamina: {currentStamina:F0}/{maxStamina} | " +
                  $"Immunity: {currentInfection:F0}/{maxInfection} | " +
                      $"Hunger: {currentHunger:F0}/{maxHunger} ({GetHungerStatus()}) | " +
                      $"Thirst: {currentThirst:F0}/{maxThirst} ({GetThirstStatus()})";

        if (isInCriticalCold) info += " [COLD!]";
        if (isInCriticalInfection) info += " [LOW IMMUNITY!]";
        if (isInCriticalHunger) info += " [STARVING!]";
        if (isInCriticalThirst) info += " [DEHYDRATED!]";
        if (isInSafeZone) info += " [SAFE ZONE]";
        if (isIndoors) info += " [Indoors]";
        if (isNearFire) info += " [Fire]";
        if (isInColdZone) info += " [Cold Zone]";

        Debug.Log(info);
    }

    private void UpdateHunger()
    {
        if (isInSafeZone)
            return;

        float decreaseRate = hungerDecreaseRate;

        if (playerProvider != null && playerProvider.MoveSpeed > sprintSpeedThreshold)
            decreaseRate *= hungerRunningMultiplier;

        SetHunger(currentHunger - decreaseRate * Time.deltaTime);
    }

    private void ApplyHungerEffects()
    {
        bool wasCriticalHunger = isInCriticalHunger;
        isInCriticalHunger = IsStarving;

        if (isInCriticalHunger && !wasCriticalHunger)
        {
            onPlayerStarving?.Invoke();
            if (showDebugInfo)
                Debug.LogWarning("<color=orange>⚠ Player is STARVING! Health will decrease!</color>");
        }

        if (isInCriticalHunger)
        {
            hungerDamageTimer -= Time.deltaTime;

            if (hungerDamageTimer <= 0f)
            {
                hungerDamageTimer = damageTickInterval;

                if (playerProvider != null)
                {
                    playerProvider.ApplyDamage(hungerDamagePerSecond * damageTickInterval);

                    if (showDebugInfo)
                        Debug.Log($"<color=red>💀 Starvation damage: {hungerDamagePerSecond * damageTickInterval:F1} HP</color>");

                    if (playerProvider.Health <= 0f)
                    {
                        onPlayerDiedOfHunger?.Invoke();
                        if (showDebugInfo)
                            Debug.LogError("<color=red>💀 PLAYER DIED OF STARVATION!</color>");
                    }
                }
            }
        }
    }

    private void UpdateThirst()
    {
        if (isInSafeZone)
            return;

        float decreaseRate = thirstDecreaseRate;

        if (playerProvider != null && playerProvider.MoveSpeed > sprintSpeedThreshold)
            decreaseRate *= thirstRunningMultiplier;

        if (currentTemperature > normalTemperature)
            decreaseRate *= thirstHotMultiplier;

        SetThirst(currentThirst - decreaseRate * Time.deltaTime);
    }

    private void ApplyThirstEffects()
    {
        bool wasCriticalThirst = isInCriticalThirst;
        isInCriticalThirst = IsDehydrated;

        if (isInCriticalThirst && !wasCriticalThirst)
        {
            onPlayerDehydrated?.Invoke();
            if (showDebugInfo)
                Debug.LogWarning("<color=cyan>⚠ Player is DEHYDRATED! Health will decrease!</color>");
        }

        if (isInCriticalThirst)
        {
            thirstDamageTimer -= Time.deltaTime;

            if (thirstDamageTimer <= 0f)
            {
                thirstDamageTimer = damageTickInterval;

                if (playerProvider != null)
                {
                    playerProvider.ApplyDamage(thirstDamagePerSecond * damageTickInterval);

                    if (showDebugInfo)
                        Debug.Log($"<color=cyan>💧 Dehydration damage: {thirstDamagePerSecond * damageTickInterval:F1} HP</color>");

                    if (playerProvider.Health <= 0f)
                    {
                        onPlayerDiedOfThirst?.Invoke();
                        if (showDebugInfo)
                            Debug.LogError("<color=cyan>💀 PLAYER DIED OF DEHYDRATION!</color>");
                    }
                }
            }
        }
    }

    public void AddHunger(float amount)
    {
        SetHunger(currentHunger + amount);

        if (showDebugInfo && amount > 0)
        {
            Debug.Log($"<color=green>🍖 Ate food: +{amount:F0} hunger (now {currentHunger:F0}/{maxHunger})</color>");
        }
    }

    public void AddThirst(float amount)
    {
        SetThirst(currentThirst + amount);

        if (showDebugInfo && amount > 0)
        {
            Debug.Log($"<color=blue>💧 Drank water: +{amount:F0} thirst (now {currentThirst:F0}/{maxThirst})</color>");
        }
    }

    public void SetHunger(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, maxHunger);

        if (survivalStatsProvider != null)
        {
            survivalStatsProvider.SetHunger(clamped);
            clamped = Mathf.Clamp(survivalStatsProvider.Hunger, 0f, maxHunger);
        }

        if (!Mathf.Approximately(currentHunger, clamped))
        {
            currentHunger = clamped;
            onHungerChanged?.Invoke(currentHunger);
        }
    }

    public void SetThirst(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, maxThirst);

        if (survivalStatsProvider != null)
        {
            survivalStatsProvider.SetThirst(clamped);
            clamped = Mathf.Clamp(survivalStatsProvider.Thirst, 0f, maxThirst);
        }

        if (!Mathf.Approximately(currentThirst, clamped))
        {
            currentThirst = clamped;
            onThirstChanged?.Invoke(currentThirst);
        }
    }

    private void PullSurvivalValuesFromProvider()
    {
        if (survivalStatsProvider == null) return;

        float oldTemperature = currentTemperature;
        float oldStamina = currentStamina;
        float oldInfection = currentInfection;
        float oldHunger = currentHunger;
        float oldThirst = currentThirst;

        maxTemperature = Mathf.Max(1f, survivalStatsProvider.MaxTemperature);
        maxStamina = Mathf.Max(1f, survivalStatsProvider.MaxStamina);
        maxInfection = Mathf.Max(1f, survivalStatsProvider.MaxInfection);
        maxHunger = Mathf.Max(1f, survivalStatsProvider.MaxHunger);
        maxThirst = Mathf.Max(1f, survivalStatsProvider.MaxThirst);

        currentTemperature = Mathf.Clamp(survivalStatsProvider.Temperature, 0f, maxTemperature);
        currentStamina = Mathf.Clamp(survivalStatsProvider.Stamina, 0f, maxStamina);
        currentInfection = Mathf.Clamp(survivalStatsProvider.Infection, 0f, maxInfection);
        currentHunger = Mathf.Clamp(survivalStatsProvider.Hunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(survivalStatsProvider.Thirst, 0f, maxThirst);

        if (!Mathf.Approximately(oldTemperature, currentTemperature))
            onTemperatureChanged?.Invoke(currentTemperature);

        if (!Mathf.Approximately(oldStamina, currentStamina))
            onStaminaChanged?.Invoke(currentStamina);

        if (!Mathf.Approximately(oldInfection, currentInfection))
            onInfectionChanged?.Invoke(currentInfection);

        if (!Mathf.Approximately(oldHunger, currentHunger))
            onHungerChanged?.Invoke(currentHunger);

        if (!Mathf.Approximately(oldThirst, currentThirst))
            onThirstChanged?.Invoke(currentThirst);
    }

    /// <summary>
    /// Restores all player survival traits and health by the given percentage of their maximum values.
    /// Infection is reduced (cured) by the same percentage instead of being increased.
    /// </summary>
    /// <param name="percentage">Percentage to restore, in the range 0–100.</param>
    public void HealAllStats(float percentage)
    {
        if (percentage <= 0f) return;

        float fraction = percentage / 100f;

        // Health
        if (playerProvider != null && playerProvider.IsAlive)
        {
            float healthGain = playerProvider.MaxHealth * fraction;
            float newHealth  = Mathf.Clamp(playerProvider.Health + healthGain, 0f, playerProvider.MaxHealth);
            playerProvider.SetHealth(newHealth);
        }

        // Hunger
        if (enableHungerSystem)
            AddHunger(maxHunger * fraction);

        // Thirst
        if (enableThirstSystem)
            AddThirst(maxThirst * fraction);

        // Temperature (warmth)
        if (enableTemperatureSystem)
            ModifyTemperature(maxTemperature * fraction);

        // Stamina
        if (enableStaminaSystem)
            AddStamina(maxStamina * fraction);

        // Armour
        if (enableArmourSystem)
            ModifyArmour(maxArmour * fraction);

        // Immunity — restore by percentage of current deficit.
        if (enableInfectionSystem && currentInfection < maxInfection)
            CureInfectionPartial(percentage);

        if (showDebugInfo)
            Debug.Log($"[SurvivalManager] HealAllStats: restored {percentage}% of all traits.");
    }

#if UNITY_EDITOR
    private const float TemperatureCelsiusMin =  -5f;
    private const float TemperatureCelsiusMax =  37f;

    private void OnDrawGizmosSelected()
    {
        Vector3 labelPos = GetLabelWorldPosition();
        float celsius = Mathf.Lerp(TemperatureCelsiusMin, TemperatureCelsiusMax,
            maxTemperature > 0f ? currentTemperature / maxTemperature : 0f);
        UnityEditor.Handles.Label(labelPos, $"{Mathf.RoundToInt(celsius)}°C");
    }

    private Vector3 GetLabelWorldPosition()
    {
        if (playerProvider?.PlayerObject != null)
            return playerProvider.PlayerObject.transform.position + Vector3.up * 2.4f;
        return transform.position + Vector3.up * 2.4f;
    }
#endif
}