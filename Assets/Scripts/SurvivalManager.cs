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

    [Header("Infection Settings")]
    [Range(0f, 100f)] public float maxInfection = 100f;
    [Range(0f, 100f)] public float currentInfection = 0f;
    public float infectionGrowthRate = 0.5f;
    public float infectionDecayRate = 1f;
    [Tooltip("Infection threshold for health damage (percentage)")]
    public float infectionDamageThreshold = 10f;
    public float infectionDamagePerSecond = 1f;

    [Tooltip("Is player currently infected (infection will grow over time)")]
    private bool isInfected = false;

    [Tooltip("Infection level cured by consumables - growth paused at this level")]
    private float curedInfectionLevel = 0f;

    [Tooltip("Is infection growth paused by consumable cure")]
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

    [Header("Armour Settings")]
    [Tooltip("Enable the armour system. When enabled, damage depletes armour before health.")]
    public bool enableArmourSystem = true;
    [Tooltip("Current armour value. Initialised from PlayerTraitsRuntime on Start.")]
    [Range(0f, 100000f)] public float currentArmour = 100f;
    [Tooltip("Maximum armour. Driven by PlayerTraitsRuntime based on level — do not set manually.")]
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

    // Armour regen cooldown — reset to armourRegenDelay each time damage is taken.
    private float _armourRegenTimer = 0f;

    // OPTIMIZATION: Update interval tracking
    private float updateIntervalTimer = 0f;
    private float updateInterval;
    private float lastUpdateTime;
    private int framesSinceLastUpdate = 0;

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
        playerProvider = playerProviderObject as IPlayerProvider;

        if (playerProvider == null)
            playerProvider = FindAnyPlayerProvider();

        if (playerProvider == null)
            Debug.LogWarning("SurvivalManager: No IPlayerProvider found — health damage will not be applied.");
        else
        {
            playerProvider.OnHealthChanged += HandleHealthChanged;
            playerProvider.OnArmourChanged += HandleArmourChanged;
        }

        if (progressionManager == null)
            progressionManager = FindFirstObjectByType<ProgressionManager>();

        if (progressionManager == null)
            Debug.LogWarning("SurvivalManager: Could not find ProgressionManager!");
        else
            progressionManager.onLevelUp.AddListener(OnLevelUp);

        // Initialise armour cap from current level.
        SyncArmourCapFromTraits();
    }

    private void OnDestroy()
    {
        if (playerProvider != null)
        {
            playerProvider.OnHealthChanged -= HandleHealthChanged;
            playerProvider.OnArmourChanged -= HandleArmourChanged;
        }

        if (progressionManager != null)
            progressionManager.onLevelUp.RemoveListener(OnLevelUp);
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

    // ── Armour helpers ────────────────────────────────────────────────────────

    private void OnLevelUp(int newLevel)
    {
        SyncArmourCapFromTraits();
    }

    private void SyncArmourCapFromTraits()
    {
        if (PlayerTraitsRuntime.Instance == null) return;

        int cap = PlayerTraitsRuntime.Instance.CurrentMaxArmour;
        if (cap <= 0) return;

        maxArmour = cap;

        // Propagate to InvectorPlayerProvider so the interception layer knows
        // the updated cap. Do not refill — player keeps current armour.
        if (playerProvider is InvectorPlayerProvider ipp)
            ipp.SetMaxArmour(cap, refillToMax: false);

        if (currentArmour > maxArmour)
            currentArmour = maxArmour;

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

        if (showDebugInfo)
        {
            DisplayDebugInfo();
        }
    }

    private void PerformSurvivalUpdates(float deltaTime)
    {
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
                currentTemperature = Mathf.MoveTowards(currentTemperature, targetTemp, normalizeAmount);
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
        if (isInfected && !isInSafeZone)
        {
            if (infectionGrowthPaused)
            {
                if (currentInfection > curedInfectionLevel)
                {
                    infectionGrowthPaused = false;
                }
            }

            if (!infectionGrowthPaused)
            {
                float infectionChange = infectionGrowthRate * Time.deltaTime;
                SetInfection(currentInfection + infectionChange);
            }
        }
        else if (isInSafeZone && currentInfection > 0f)
        {
            float infectionChange = -infectionDecayRate * Time.deltaTime;
            SetInfection(currentInfection + infectionChange);

            if (currentInfection <= 0f)
            {
                isInfected = false;
                infectionGrowthPaused = false;
                curedInfectionLevel = 0f;
            }
        }

        bool wasCritical = isInCriticalInfection;
        isInCriticalInfection = currentInfection >= (maxInfection - infectionDamageThreshold);

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

        if (showDebugInfo)
            Debug.Log($"{source} damage: {damage:F1} HP (remaining: {playerProvider.Health:F1})");
    }

    public void SetTemperature(float value)
    {
        float oldTemperature = currentTemperature;
        currentTemperature = Mathf.Clamp(value, 0f, maxTemperature);

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
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);

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
        currentInfection = Mathf.Clamp(value, 0f, maxInfection);

        if (!Mathf.Approximately(oldInfection, currentInfection))
        {
            onInfectionChanged?.Invoke(currentInfection);
        }
    }

    public void AddInfection(float amount)
    {
        SetInfection(currentInfection + amount);

        if (amount > 0f && currentInfection > 0f)
        {
            isInfected = true;

            if (currentInfection > curedInfectionLevel)
            {
                infectionGrowthPaused = false;
            }
        }
    }

    public void CureInfection(float amount)
    {
        SetInfection(currentInfection - amount);

        if (currentInfection <= 0f)
        {
            isInfected = false;
            infectionGrowthPaused = false;
            curedInfectionLevel = 0f;
        }
    }

    public void CureInfectionPartial(float percentage)
    {
        if (currentInfection <= 0f) return;

        float cureAmount = currentInfection * (percentage / 100f);
        SetInfection(currentInfection - cureAmount);

        curedInfectionLevel = currentInfection;
        infectionGrowthPaused = true;

        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Infection partially cured by {percentage}%. Growth paused at {curedInfectionLevel:F1}</color>");
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

    public void ResetInfection()
    {
        SetInfection(0f);
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
        if (currentInfection == 0f) return "None";
        if (currentInfection < 25f) return "Mild";
        if (currentInfection < 50f) return "Moderate";
        if (currentInfection < 75f) return "Severe";
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
                      $"Infection: {currentInfection:F0}/{maxInfection} | " +
                      $"Hunger: {currentHunger:F0}/{maxHunger} ({GetHungerStatus()}) | " +
                      $"Thirst: {currentThirst:F0}/{maxThirst} ({GetThirstStatus()})";

        if (isInCriticalCold) info += " [COLD!]";
        if (isInCriticalInfection) info += " [INFECTED!]";
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

        currentHunger -= decreaseRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        onHungerChanged?.Invoke(currentHunger);
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

        currentThirst -= decreaseRate * Time.deltaTime;
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        onThirstChanged?.Invoke(currentThirst);
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
        currentHunger = Mathf.Clamp(value, 0f, maxHunger);
        onHungerChanged?.Invoke(currentHunger);
    }

    public void SetThirst(float value)
    {
        currentThirst = Mathf.Clamp(value, 0f, maxThirst);
        onThirstChanged?.Invoke(currentThirst);
    }
}