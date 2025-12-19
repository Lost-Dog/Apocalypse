using UnityEngine;
using UnityEngine.Events;
using JUTPS;

public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager Instance { get; private set; }
    
    [Header("Player Reference")]
    public JUCharacterController playerController;
    public JUHealth playerHealth;
    public ProgressionManager progressionManager;
    
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
    }
    
    private void FindPlayerReferences()
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<JUCharacterController>();
            }
        }
        
        if (playerController != null && playerHealth == null)
        {
            playerHealth = playerController.GetComponent<JUHealth>();
        }
        
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<ProgressionManager>();
        }
        
        if (playerController == null)
        {
            Debug.LogWarning("SurvivalManager: Could not find player controller!");
        }
        
        if (playerHealth == null)
        {
            Debug.LogWarning("SurvivalManager: Could not find player health!");
        }
        
        if (progressionManager == null)
        {
            Debug.LogWarning("SurvivalManager: Could not find ProgressionManager!");
        }
    }
    
    private void Update()
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
        
        if (showDebugInfo)
        {
            DisplayDebugInfo();
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
            // Only normalize if temperature is significantly different (more than 0.5°C)
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
        
        if (playerController != null && playerController.IsRunning)
        {
            staminaChange -= staminaDrainRateRunning * Time.deltaTime;
        }
        else
        {
            float regenRate = staminaRegenRate;
            
            if (IsHungry)
            {
                regenRate *= (1f - hungerStaminaPenalty);
            }
            
            if (IsThirsty)
            {
                regenRate *= (1f - thirstStaminaPenalty);
            }
            
            staminaChange += regenRate * Time.deltaTime;
        }
        
        if (IsCriticalCold)
        {
            staminaChange -= staminaDrainRateCold * Time.deltaTime;
        }
        
        if (staminaChange != 0f)
        {
            SetStamina(currentStamina + staminaChange);
        }
    }
    
    private void UpdateInfection()
    {
        // If player is infected and not in safe zone, infection grows over time
        if (isInfected && !isInSafeZone)
        {
            // If growth is paused by consumable, check if we should resume
            if (infectionGrowthPaused)
            {
                // Growth resumes only if infection goes above cured level (new damage taken)
                if (currentInfection > curedInfectionLevel)
                {
                    infectionGrowthPaused = false;
                }
            }
            
            // Grow infection if not paused
            if (!infectionGrowthPaused)
            {
                float infectionChange = infectionGrowthRate * Time.deltaTime;
                SetInfection(currentInfection + infectionChange);
            }
        }
        // Natural decay only in safe zones when not infected
        else if (isInSafeZone && currentInfection > 0f)
        {
            float infectionChange = -infectionDecayRate * Time.deltaTime;
            SetInfection(currentInfection + infectionChange);
            
            // Clear infection flags when fully cured in safe zone
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
        if (!enableColdDamage || !isInCriticalCold || playerHealth == null)
            return;
        
        damageTimer -= Time.deltaTime;
        
        if (damageTimer <= 0f)
        {
            ApplyDamage(coldDamagePerSecond, "cold");
            damageTimer = damageTickInterval;
        }
        
        if (currentTemperature <= 0f && !playerHealth.IsDead)
        {
            onPlayerFroze?.Invoke();
        }
    }
    
    private void ApplyInfectionEffects()
    {
        if (!isInCriticalInfection || playerHealth == null)
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
        if (playerHealth != null && damagePerSecond > 0f)
        {
            float damage = damagePerSecond * damageTickInterval;
            
            // Disable blood screen flash for cold damage
            bool originalFlashSetting = playerHealth.BloodScreenEffect;
            if (source == "cold")
            {
                playerHealth.BloodScreenEffect = false;
            }
            
            playerHealth.DoDamage(damage);
            
            // Restore original setting
            playerHealth.BloodScreenEffect = originalFlashSetting;
            
            if (showDebugInfo)
            {
                Debug.Log($"{source} damage: {damage} HP");
            }
        }
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
        
        // Mark player as infected when taking infection damage
        if (amount > 0f && currentInfection > 0f)
        {
            isInfected = true;
            
            // If infection exceeds cured level, resume growth
            if (currentInfection > curedInfectionLevel)
            {
                infectionGrowthPaused = false;
            }
        }
    }
    
    public void CureInfection(float amount)
    {
        SetInfection(currentInfection - amount);
        
        // Fully clear infection flags only if completely cured
        if (currentInfection <= 0f)
        {
            isInfected = false;
            infectionGrowthPaused = false;
            curedInfectionLevel = 0f;
        }
    }
    
    /// <summary>
    /// Cure infection by percentage and pause growth at this level (consumable cure)
    /// </summary>
    public void CureInfectionPartial(float percentage)
    {
        if (currentInfection <= 0f) return;
        
        float cureAmount = currentInfection * (percentage / 100f);
        SetInfection(currentInfection - cureAmount);
        
        // Remember this level and pause growth
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
        if (playerHealth != null)
        {
            playerHealth.Health = playerHealth.MaxHealth;
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
        
        if (playerController != null && playerController.IsSprinting)
        {
            decreaseRate *= hungerRunningMultiplier;
        }
        
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
                
                if (playerHealth != null)
                {
                    playerHealth.DoDamage(hungerDamagePerSecond * damageTickInterval);
                    
                    if (showDebugInfo)
                        Debug.Log($"<color=red>💀 Starvation damage: {hungerDamagePerSecond * damageTickInterval:F1} HP</color>");
                    
                    if (playerHealth.Health <= 0f)
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
        
        if (playerController != null && playerController.IsSprinting)
        {
            decreaseRate *= thirstRunningMultiplier;
        }
        
        if (currentTemperature > normalTemperature)
        {
            decreaseRate *= thirstHotMultiplier;
        }
        
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
                
                if (playerHealth != null)
                {
                    playerHealth.DoDamage(thirstDamagePerSecond * damageTickInterval);
                    
                    if (showDebugInfo)
                        Debug.Log($"<color=cyan>💧 Dehydration damage: {thirstDamagePerSecond * damageTickInterval:F1} HP</color>");
                    
                    if (playerHealth.Health <= 0f)
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
