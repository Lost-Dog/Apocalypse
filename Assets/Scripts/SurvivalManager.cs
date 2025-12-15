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
    [Tooltip("Maximum temperature value (percentage: 0-100)")]
    [Range(0f, 100f)] public float maxTemperature = 100f;
    [Tooltip("Current temperature value (percentage: 0-100)")]
    [Range(0f, 100f)] public float currentTemperature = 100f;
    [Tooltip("Normal/target temperature when recovering (percentage: 0-100)")]
    [Range(0f, 100f)] public float normalTemperature = 100f;
    [Tooltip("Minimum temperature before player freezes")]
    public float minTemperature = 0f;
    [Tooltip("Temperature decrease rate per second (100 to 0 in ~3 minutes)")]
    public float temperatureDecreaseRate = 0.56f;
    [Tooltip("Temperature recovery rate per second")]
    public float temperatureNormalizeRate = 5f;
    [Tooltip("Critical cold threshold (0-1 as percentage of max)")]
    [Range(0f, 1f)] public float criticalColdThreshold = 0.2f;
    
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
    public float infectionDamageThreshold = 50f;
    public float infectionDamagePerSecond = 1f;
    
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
    public UnityEvent onEnteredCriticalTemperature;
    public UnityEvent onExitedCriticalTemperature;
    public UnityEvent onPlayerFroze;
    public UnityEvent onStaminaDepleted;
    public UnityEvent onInfectionCritical;
    
    [Header("System Toggles")]
    public bool enableTemperatureSystem = true;
    public bool enableTemperatureDecrease = false;
    public bool enableColdDamage = true;
    public bool enableStaminaSystem = true;
    public bool enableInfectionSystem = true;
    public bool showDebugInfo = false;
    
    [Header("Safe Zone Interaction")]
    public bool isInSafeZone = false;
    public bool pauseTemperatureNormalizationInSafeZone = true;
    
    private float damageTimer;
    private float infectionDamageTimer;
    private bool isInCriticalCold = false;
    private bool isInCriticalInfection = false;
    private bool isIndoors = false;
    private bool isNearFire = false;
    private bool isInColdZone = false;
    
    public float TemperaturePercentage => currentTemperature / maxTemperature;
    public bool IsCriticalCold => TemperaturePercentage <= criticalColdThreshold;
    public bool IsCritical => IsCriticalCold || isInCriticalInfection;
    public float StaminaPercentage => currentStamina / maxStamina;
    public float InfectionPercentage => currentInfection / maxInfection;
    
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
            float normalizeAmount = temperatureNormalizeRate * Time.deltaTime;
            currentTemperature = Mathf.MoveTowards(currentTemperature, targetTemp, normalizeAmount);
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
            staminaChange += staminaRegenRate * Time.deltaTime;
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
        if (currentInfection > 0f)
        {
            float infectionChange = -infectionDecayRate * Time.deltaTime;
            SetInfection(currentInfection + infectionChange);
        }
        
        bool wasCritical = isInCriticalInfection;
        isInCriticalInfection = currentInfection >= infectionDamageThreshold;
        
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
            playerHealth.DoDamage(damage);
            
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
    }
    
    public void CureInfection(float amount)
    {
        SetInfection(currentInfection - amount);
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
    
    public void ResetAllStats()
    {
        ResetTemperature();
        ResetStamina();
        ResetInfection();
        if (playerHealth != null)
        {
            playerHealth.Health = playerHealth.MaxHealth;
        }
    }
    
    public string GetTemperatureStatus()
    {
        float percentage = TemperaturePercentage * 100f;
        
        if (percentage >= 95f) return "Warm";
        if (percentage >= 80f) return "Normal";
        if (percentage >= 60f) return "Cool";
        if (percentage >= 40f) return "Cold";
        if (percentage >= 20f) return "Very Cold";
        if (percentage >= 10f) return "Freezing";
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
    
    private void DisplayDebugInfo()
    {
        string info = $"[Survival] Temp: {currentTemperature:F1}% ({GetTemperatureStatus()}) | Stamina: {currentStamina:F0}/{maxStamina} | Infection: {currentInfection:F0}/{maxInfection}";
        
        if (isInCriticalCold) info += " [COLD!]";
        if (isInCriticalInfection) info += " [INFECTED!]";
        if (isInSafeZone) info += " [SAFE ZONE]";
        if (isIndoors) info += " [Indoors]";
        if (isNearFire) info += " [Fire]";
        if (isInColdZone) info += " [Cold Zone]";
        
        Debug.Log(info);
    }
}
