using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class PlayerStatusIndicators : MonoBehaviour
{
    [System.Serializable]
    public class StatusIndicator
    {
        public GameObject indicatorObject;
        public Image iconImage;
        public TextMeshProUGUI labelText;
        public Image pulseImage;
        public Color normalColor = Color.white;
        public Color warningColor = Color.yellow;
        public Color criticalColor = Color.red;
        [HideInInspector] public bool isActive = false;
    }
    
    [Header("Indicator Objects")]
    public StatusIndicator healthIndicator;
    public StatusIndicator temperatureIndicator;
    public StatusIndicator infectionIndicator;
    public StatusIndicator staminaIndicator;
    public StatusIndicator hungerIndicator;
    public StatusIndicator thirstIndicator;
    
    [Header("Auto-Find References")]
    public bool autoFindReferences = true;
    public JUHealth playerHealth;
    public SurvivalManager survivalManager;
    public PlayerInfectionDisplay infectionDisplay;
    
    [Header("Health Thresholds")]
    [Range(0f, 1f)] public float healthWarningThreshold = 0.5f;
    [Range(0f, 1f)] public float healthCriticalThreshold = 0.25f;
    
    [Header("Temperature Thresholds")]
    [Tooltip("Temperature warning threshold (Celsius) - warns below this value")]
    public float temperatureWarningThreshold = 15f;
    [Tooltip("Temperature critical threshold (Celsius) - critical below this value")]
    public float temperatureCriticalThreshold = 5f;
    
    [Header("Infection Thresholds")]
    public float infectionWarningThreshold = 50f;
    public float infectionCriticalThreshold = 75f;
    
    [Header("Stamina Thresholds")]
    [Range(0f, 1f)] public float staminaWarningThreshold = 0.3f;
    [Range(0f, 1f)] public float staminaCriticalThreshold = 0.15f;
    
    [Header("Hunger Thresholds")]
    [Range(0f, 1f)] public float hungerWarningThreshold = 0.3f;
    [Range(0f, 1f)] public float hungerCriticalThreshold = 0.15f;
    
    [Header("Thirst Thresholds")]
    [Range(0f, 1f)] public float thirstWarningThreshold = 0.3f;
    [Range(0f, 1f)] public float thirstCriticalThreshold = 0.15f;
    
    [Header("Visual Effects")]
    public bool enablePulseEffect = true;
    public float normalPulseSpeed = 1f;
    public float warningPulseSpeed = 2f;
    public float criticalPulseSpeed = 4f;
    
    [Header("Panel Behavior")]
    [Tooltip("Should the panel start disabled and only show when warnings are active?")]
    public bool startDisabled = false;
    
    [Tooltip("Hide panel when no warnings are active")]
    public bool autoHideWhenNoWarnings = false;
    
    [Header("Audio")]
    [Tooltip("Audio source for playing notification sounds")]
    public AudioSource audioSource;
    
    [Tooltip("Sound to play when a warning becomes active")]
    public AudioClip warningSound;
    
    private bool hasActiveWarnings = false;
    
    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        InitializeIndicators();
        
        if (startDisabled)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    
    private void FindReferences()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<JUHealth>();
            }
        }
        
        if (survivalManager == null)
        {
            survivalManager = FindFirstObjectByType<SurvivalManager>();
        }
        
        if (infectionDisplay == null)
        {
            infectionDisplay = FindFirstObjectByType<PlayerInfectionDisplay>();
        }
    }
    
    private void InitializeIndicators()
    {
        SetIndicatorActive(healthIndicator, false);
        SetIndicatorActive(temperatureIndicator, false);
        SetIndicatorActive(infectionIndicator, false);
        SetIndicatorActive(staminaIndicator, false);
        SetIndicatorActive(hungerIndicator, false);
        SetIndicatorActive(thirstIndicator, false);
    }
    
    private void Update()
    {
        UpdateHealthIndicator();
        UpdateTemperatureIndicator();
        UpdateInfectionIndicator();
        UpdateStaminaIndicator();
        UpdateHungerIndicator();
        UpdateThirstIndicator();
        
        UpdatePanelVisibility();
    }
    
    private void UpdatePanelVisibility()
    {
        if (!autoHideWhenNoWarnings) return;
        
        bool anyActive = healthIndicator.isActive || 
                        temperatureIndicator.isActive || 
                        infectionIndicator.isActive ||
                        staminaIndicator.isActive ||
                        hungerIndicator.isActive ||
                        thirstIndicator.isActive;
        
        if (anyActive && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        else if (!anyActive && gameObject.activeSelf && autoHideWhenNoWarnings)
        {
            gameObject.SetActive(false);
        }
        
        hasActiveWarnings = anyActive;
    }
    
    private void UpdateHealthIndicator()
    {
        if (playerHealth == null || healthIndicator.indicatorObject == null) return;
        
        float healthPercentage = playerHealth.Health / playerHealth.MaxHealth;
        
        if (healthPercentage <= healthCriticalThreshold)
        {
            SetIndicatorState(healthIndicator, true, healthIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(healthIndicator.labelText, "CRITICAL");
        }
        else if (healthPercentage <= healthWarningThreshold)
        {
            SetIndicatorState(healthIndicator, true, healthIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(healthIndicator.labelText, "LOW HEALTH");
        }
        else
        {
            SetIndicatorActive(healthIndicator, false);
        }
    }
    
    private void UpdateTemperatureIndicator()
    {
        if (survivalManager == null || temperatureIndicator.indicatorObject == null) return;
        if (!survivalManager.enableTemperatureSystem) return;
        
        float currentTemp = survivalManager.currentTemperature;
        
        if (currentTemp <= temperatureCriticalThreshold)
        {
            SetIndicatorState(temperatureIndicator, true, temperatureIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(temperatureIndicator.labelText, "FREEZING");
        }
        else if (currentTemp <= temperatureWarningThreshold)
        {
            SetIndicatorState(temperatureIndicator, true, temperatureIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(temperatureIndicator.labelText, "COLD");
        }
        else
        {
            SetIndicatorActive(temperatureIndicator, false);
        }
    }
    
    private void UpdateInfectionIndicator()
    {
        if (infectionDisplay == null || infectionIndicator.indicatorObject == null) return;
        
        float infection = infectionDisplay.currentInfection;
        
        if (infection >= infectionCriticalThreshold)
        {
            SetIndicatorState(infectionIndicator, true, infectionIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(infectionIndicator.labelText, "SEVERE");
        }
        else if (infection >= infectionWarningThreshold)
        {
            SetIndicatorState(infectionIndicator, true, infectionIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(infectionIndicator.labelText, "INFECTED");
        }
        else
        {
            SetIndicatorActive(infectionIndicator, false);
        }
    }
    
    private void UpdateStaminaIndicator()
    {
        if (survivalManager == null || staminaIndicator.indicatorObject == null) return;
        if (!survivalManager.enableStaminaSystem) return;
        
        float staminaPercentage = survivalManager.currentStamina / survivalManager.maxStamina;
        
        if (staminaPercentage <= staminaCriticalThreshold)
        {
            SetIndicatorState(staminaIndicator, true, staminaIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(staminaIndicator.labelText, "EXHAUSTED");
        }
        else if (staminaPercentage <= staminaWarningThreshold)
        {
            SetIndicatorState(staminaIndicator, true, staminaIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(staminaIndicator.labelText, "LOW STAMINA");
        }
        else
        {
            SetIndicatorActive(staminaIndicator, false);
        }
    }
    
    private void UpdateHungerIndicator()
    {
        if (survivalManager == null || hungerIndicator.indicatorObject == null) return;
        if (!survivalManager.enableHungerSystem) return;
        
        float hungerPercentage = survivalManager.currentHunger / survivalManager.maxHunger;
        
        if (hungerPercentage <= hungerCriticalThreshold)
        {
            SetIndicatorState(hungerIndicator, true, hungerIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(hungerIndicator.labelText, "STARVING");
        }
        else if (hungerPercentage <= hungerWarningThreshold)
        {
            SetIndicatorState(hungerIndicator, true, hungerIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(hungerIndicator.labelText, "HUNGRY");
        }
        else
        {
            SetIndicatorActive(hungerIndicator, false);
        }
    }
    
    private void UpdateThirstIndicator()
    {
        if (survivalManager == null || thirstIndicator.indicatorObject == null) return;
        if (!survivalManager.enableThirstSystem) return;
        
        float thirstPercentage = survivalManager.currentThirst / survivalManager.maxThirst;
        
        if (thirstPercentage <= thirstCriticalThreshold)
        {
            SetIndicatorState(thirstIndicator, true, thirstIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(thirstIndicator.labelText, "DEHYDRATED");
        }
        else if (thirstPercentage <= thirstWarningThreshold)
        {
            SetIndicatorState(thirstIndicator, true, thirstIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(thirstIndicator.labelText, "THIRSTY");
        }
        else
        {
            SetIndicatorActive(thirstIndicator, false);
        }
    }
    
    private void SetIndicatorState(StatusIndicator indicator, bool active, Color color, float pulseSpeed)
    {
        bool wasActive = indicator.isActive;
        
        if (!wasActive && active)
        {
            SetIndicatorActive(indicator, true);
            PlayWarningSound();
        }
        
        if (indicator.iconImage != null)
        {
            indicator.iconImage.color = color;
        }
        
        if (indicator.labelText != null)
        {
            indicator.labelText.color = color;
        }
        
        if (enablePulseEffect && indicator.pulseImage != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI);
            Color pulseColor = color;
            pulseColor.a = pulse * 0.8f;
            indicator.pulseImage.color = pulseColor;
        }
    }
    
    private void SetIndicatorActive(StatusIndicator indicator, bool active)
    {
        if (indicator.indicatorObject != null)
        {
            indicator.indicatorObject.SetActive(active);
            indicator.isActive = active;
        }
    }
    
    private void UpdateLabel(TextMeshProUGUI label, string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }
    
    private void PlayWarningSound()
    {
        if (audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
    }
    
    public bool HasActiveWarnings()
    {
        return hasActiveWarnings;
    }
}
