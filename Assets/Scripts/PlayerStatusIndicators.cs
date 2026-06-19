using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        [HideInInspector] public Color activeColor = Color.white;
        [HideInInspector] public float activePulseSpeed = 0f;
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
    [Tooltip("Legacy reference; indicator checks now use provider traits.")]
    public SurvivalManager survivalManager;
    [Tooltip("Legacy reference; indicator checks now use provider traits.")]
    public PlayerInfectionDisplay infectionDisplay;

    [Header("Player Provider")]
    [Tooltip("Assign GC2PlayerProvider (or any IPlayerProvider). Auto-finds GC2 first if left empty.")]
    [SerializeField] private MonoBehaviour playerProviderObject;
    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;
    
    [Header("Health Thresholds")]
    [Range(0f, 1f)] public float healthWarningThreshold = 0.5f;
    [Range(0f, 1f)] public float healthCriticalThreshold = 0.25f;
    
    [Header("Temperature Thresholds")]
    [Tooltip("Temperature warning threshold (Celsius) - warns below this value")]
    public float temperatureWarningThreshold = 15f;
    [Tooltip("Temperature critical threshold (Celsius) - critical below this value")]
    public float temperatureCriticalThreshold = 5f;
    
    [Header("Infection Thresholds")]
    [Tooltip("Show warning when immunity drops at or below this value (0–100)")]
    public float infectionWarningThreshold = 50f;
    [Tooltip("Show critical indicator when immunity drops at or below this value (0–100)")]
    public float infectionCriticalThreshold = 25f;
    
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
    private float bindRetryTimer = 0f;
    private const float BindRetryInterval = 1f;
    
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
        SubscribeToEvents();
        RefreshIndicators();
        
        if (startDisabled)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        RefreshIndicators();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void FindReferences()
    {
        // Resolve IPlayerProvider — prefer the serialized field, then search the scene.
        if (playerProvider == null)
            playerProvider = playerProviderObject as IPlayerProvider;

        if (playerProvider == null)
        {
            GC2PlayerProvider gc2Provider = FindFirstObjectByType<GC2PlayerProvider>();
            if (gc2Provider != null)
                playerProvider = gc2Provider;

            if (playerProvider == null)
            {
                foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                {
                    if (mb is IPlayerProvider provider)
                    {
                        playerProvider = provider;
                        break;
                    }
                }
            }
        }

        if (playerProvider == null)
            Debug.LogWarning("[PlayerStatusIndicators] No IPlayerProvider found — health indicator disabled.");

        survivalStatsProvider = playerProvider as ISurvivalStatsProvider;
        if (survivalStatsProvider == null)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is ISurvivalStatsProvider provider)
                {
                    survivalStatsProvider = provider;
                    break;
                }
            }
        }

        if (survivalStatsProvider == null)
            Debug.LogWarning("[PlayerStatusIndicators] No ISurvivalStatsProvider found — survival indicators disabled.");

        if (survivalManager == null)
            survivalManager = FindFirstObjectByType<SurvivalManager>();

        if (infectionDisplay == null)
            infectionDisplay = FindFirstObjectByType<PlayerInfectionDisplay>();
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
        if (enablePulseEffect && hasActiveWarnings)
        {
            UpdatePulse(healthIndicator);
            UpdatePulse(temperatureIndicator);
            UpdatePulse(infectionIndicator);
            UpdatePulse(staminaIndicator);
            UpdatePulse(hungerIndicator);
            UpdatePulse(thirstIndicator);
        }

        bindRetryTimer += Time.deltaTime;
        if (bindRetryTimer < BindRetryInterval) return;

        bindRetryTimer = 0f;

        bool missingSource = playerProvider == null || survivalManager == null;
        if (!missingSource) return;

        FindReferences();
        SubscribeToEvents();
        RefreshIndicators();
    }

    private void SubscribeToEvents()
    {
        if (playerProvider != null)
        {
            playerProvider.OnHealthChanged -= OnHealthChanged;
            playerProvider.OnHealthChanged += OnHealthChanged;
        }

        if (survivalManager != null)
        {
            survivalManager.onTemperatureChanged.RemoveListener(OnTemperatureChanged);
            survivalManager.onTemperatureChanged.AddListener(OnTemperatureChanged);

            survivalManager.onInfectionChanged.RemoveListener(OnInfectionChanged);
            survivalManager.onInfectionChanged.AddListener(OnInfectionChanged);

            survivalManager.onStaminaChanged.RemoveListener(OnStaminaChanged);
            survivalManager.onStaminaChanged.AddListener(OnStaminaChanged);

            survivalManager.onHungerChanged.RemoveListener(OnHungerChanged);
            survivalManager.onHungerChanged.AddListener(OnHungerChanged);

            survivalManager.onThirstChanged.RemoveListener(OnThirstChanged);
            survivalManager.onThirstChanged.AddListener(OnThirstChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (playerProvider != null)
            playerProvider.OnHealthChanged -= OnHealthChanged;

        if (survivalManager != null)
        {
            survivalManager.onTemperatureChanged.RemoveListener(OnTemperatureChanged);
            survivalManager.onInfectionChanged.RemoveListener(OnInfectionChanged);
            survivalManager.onStaminaChanged.RemoveListener(OnStaminaChanged);
            survivalManager.onHungerChanged.RemoveListener(OnHungerChanged);
            survivalManager.onThirstChanged.RemoveListener(OnThirstChanged);
        }
    }

    private void RefreshIndicators()
    {
        UpdateHealthIndicator();
        UpdateTemperatureIndicator();
        UpdateInfectionIndicator();
        UpdateStaminaIndicator();
        UpdateHungerIndicator();
        UpdateThirstIndicator();
        UpdatePanelVisibility();
    }

    private void OnHealthChanged(float current, float max)
    {
        UpdateHealthIndicator();
        UpdatePanelVisibility();
    }

    private void OnTemperatureChanged(float value)
    {
        UpdateTemperatureIndicator();
        UpdatePanelVisibility();
    }

    private void OnInfectionChanged(float value)
    {
        UpdateInfectionIndicator();
        UpdatePanelVisibility();
    }

    private void OnStaminaChanged(float value)
    {
        UpdateStaminaIndicator();
        UpdatePanelVisibility();
    }

    private void OnHungerChanged(float value)
    {
        UpdateHungerIndicator();
        UpdatePanelVisibility();
    }

    private void OnThirstChanged(float value)
    {
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
        if (playerProvider == null || healthIndicator.indicatorObject == null) return;

        float healthPercentage = playerProvider.MaxHealth > 0f
            ? playerProvider.Health / playerProvider.MaxHealth
            : 1f;

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
        if (survivalStatsProvider == null || temperatureIndicator.indicatorObject == null) return;
        if (survivalStatsProvider.MaxTemperature <= 0f) return;
        
        float currentTemp = survivalStatsProvider.Temperature;
        
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
        if (survivalStatsProvider == null || infectionIndicator.indicatorObject == null) return;
        if (survivalStatsProvider.MaxInfection <= 0f) return;
        
        // Infection value is treated as immunity (max = fully immune, 0 = no immunity).
        float immunity = survivalStatsProvider.Infection / survivalStatsProvider.MaxInfection * 100f;
        
        if (immunity <= infectionCriticalThreshold)
        {
            SetIndicatorState(infectionIndicator, true, infectionIndicator.criticalColor, criticalPulseSpeed);
            UpdateLabel(infectionIndicator.labelText, "CRITICAL IMMUNITY");
        }
        else if (immunity <= infectionWarningThreshold)
        {
            SetIndicatorState(infectionIndicator, true, infectionIndicator.warningColor, warningPulseSpeed);
            UpdateLabel(infectionIndicator.labelText, "LOW IMMUNITY");
        }
        else
        {
            SetIndicatorActive(infectionIndicator, false);
        }
    }
    
    private void UpdateStaminaIndicator()
    {
        if (survivalStatsProvider == null || staminaIndicator.indicatorObject == null) return;
        if (survivalStatsProvider.MaxStamina <= 0f) return;
        
        float staminaPercentage = Mathf.Clamp01(survivalStatsProvider.Stamina / survivalStatsProvider.MaxStamina);
        
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
        if (survivalStatsProvider == null || hungerIndicator.indicatorObject == null) return;
        if (survivalStatsProvider.MaxHunger <= 0f) return;
        
        float hungerPercentage = Mathf.Clamp01(survivalStatsProvider.Hunger / survivalStatsProvider.MaxHunger);
        
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
        if (survivalStatsProvider == null || thirstIndicator.indicatorObject == null) return;
        if (survivalStatsProvider.MaxThirst <= 0f) return;
        
        float thirstPercentage = Mathf.Clamp01(survivalStatsProvider.Thirst / survivalStatsProvider.MaxThirst);
        
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
            indicator.activeColor = color;
            indicator.activePulseSpeed = pulseSpeed;

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI);
            Color pulseColor = color;
            pulseColor.a = pulse * 0.8f;
            indicator.pulseImage.color = pulseColor;
        }
    }

    private void UpdatePulse(StatusIndicator indicator)
    {
        if (!indicator.isActive || indicator.pulseImage == null) return;

        float speed = indicator.activePulseSpeed > 0f ? indicator.activePulseSpeed : normalPulseSpeed;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * speed * Mathf.PI);
        Color pulseColor = indicator.activeColor;
        pulseColor.a = pulse * 0.8f;
        indicator.pulseImage.color = pulseColor;
    }
    
    private void SetIndicatorActive(StatusIndicator indicator, bool active)
    {
        if (indicator.indicatorObject != null)
        {
            indicator.indicatorObject.SetActive(active);
            indicator.isActive = active;

            if (!active)
            {
                indicator.activePulseSpeed = 0f;
            }
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
