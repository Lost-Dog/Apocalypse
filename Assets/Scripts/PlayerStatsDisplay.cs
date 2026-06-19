using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Aggregates all player stat displays.
/// Uses GC2-backed provider traits for health, armour/shield and survival stats.
/// </summary>
public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("Manager References")]
    [Tooltip("Legacy reference; UI values are now sourced from provider traits.")]
    public SurvivalManager survivalManager;
    public ProgressionManager progressionManager;
    [Tooltip("Assign GC2PlayerProvider (or any IPlayerProvider). Auto-finds GC2 first.")]
    public MonoBehaviour playerProviderObject;

    [Header("UI Text Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI infectionText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;

    [Header("UI Slider Elements (Optional)")]
    public Slider healthSlider;
    public Slider shieldSlider;
    public Slider xpSlider;
    public Slider temperatureSlider;
    public Slider staminaSlider;
    public Slider infectionSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;

    [Header("Display Settings")]
    public bool showTemperaturePrefix = false;
    public bool showStaminaPrefix     = false;
    public bool showInfectionPrefix   = false;
    public bool showHungerPrefix      = false;
    public bool showThirstPrefix      = false;

    [Header("Auto-Find References")]
    public bool autoFindReferences = true;

    [Header("Hunger/Thirst Color Thresholds (%)")]
    [Range(0f, 100f)] public float hungryThreshold = 40f;
    [Range(0f, 100f)] public float starvingThreshold = 20f;
    [Range(0f, 100f)] public float thirstyThreshold = 40f;
    [Range(0f, 100f)] public float dehydratedThreshold = 20f;

    private const float BindRetryInterval = 1f;

    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;
    private float bindRetryTimer;

    private void Start()
    {
        if (autoFindReferences)
            FindReferences();

        InitializeSliders();
        SubscribeToEvents();
        RefreshAll();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        RefreshAll();
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
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();

        if (progressionManager == null)
            progressionManager = ProgressionManager.Instance ?? FindFirstObjectByType<ProgressionManager>();

        playerProvider = playerProviderObject as IPlayerProvider;
        if (playerProvider == null)
            playerProvider = FindBestPlayerProvider();

        if (playerProvider == null)
            playerProvider = FindAnyPlayerProvider();

        if (playerProvider == null)
            Debug.LogWarning("[PlayerStatsDisplay] No IPlayerProvider found — health and shield will not update.");

        survivalStatsProvider = playerProvider as ISurvivalStatsProvider;
        if (survivalStatsProvider == null)
            survivalStatsProvider = FindAnySurvivalProvider();

        if (survivalStatsProvider == null)
            Debug.LogWarning("[PlayerStatsDisplay] No ISurvivalStatsProvider found — survival trait UI will not update.");
    }

    private void InitializeSliders()
    {
        if (playerProvider != null)
        {
            SetSlider(healthSlider, 0f, playerProvider.MaxHealth, playerProvider.Health);
            SetSlider(shieldSlider, 0f, 100f, playerProvider.MaxShield > 0f
                ? playerProvider.Shield / playerProvider.MaxShield * 100f : 0f);
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.value = progressionManager != null ? Mathf.Clamp01(progressionManager.GetXPProgress()) : 0f;
        }

        if (survivalStatsProvider != null)
        {
            SetSlider(temperatureSlider, 0f, survivalStatsProvider.MaxTemperature, survivalStatsProvider.Temperature);
            SetSlider(staminaSlider, 0f, survivalStatsProvider.MaxStamina, survivalStatsProvider.Stamina);
            SetSlider(infectionSlider, 0f, survivalStatsProvider.MaxInfection, survivalStatsProvider.Infection);
            SetSlider(hungerSlider, 0f, survivalStatsProvider.MaxHunger, survivalStatsProvider.Hunger);
            SetSlider(thirstSlider, 0f, survivalStatsProvider.MaxThirst, survivalStatsProvider.Thirst);
            return;
        }

        if (survivalManager != null)
        {
            SetSlider(temperatureSlider, 0f, survivalManager.maxTemperature, survivalManager.currentTemperature);
            SetSlider(staminaSlider, 0f, survivalManager.maxStamina, survivalManager.currentStamina);
            SetSlider(infectionSlider, 0f, survivalManager.maxInfection, survivalManager.currentInfection);
            SetSlider(hungerSlider, 0f, survivalManager.maxHunger, survivalManager.currentHunger);
            SetSlider(thirstSlider, 0f, survivalManager.maxThirst, survivalManager.currentThirst);
        }
    }

    private void Update()
    {
        // Retry late-bound manager/provider hookups without polling UI every frame.
        bindRetryTimer += Time.deltaTime;
        if (bindRetryTimer < BindRetryInterval) return;

        bindRetryTimer = 0f;

        bool wasMissingSource = playerProvider == null || progressionManager == null || survivalManager == null;
        if (!wasMissingSource) return;

        FindReferences();
        SubscribeToEvents();
        RefreshAll();
    }

    private void SubscribeToEvents()
    {
        if (playerProvider != null)
        {
            playerProvider.OnHealthChanged -= HandleHealthChanged;
            playerProvider.OnHealthChanged += HandleHealthChanged;

            playerProvider.OnArmourChanged -= HandleArmourChanged;
            playerProvider.OnArmourChanged += HandleArmourChanged;
        }

        if (progressionManager != null)
        {
            progressionManager.onXPGained.RemoveListener(HandleXPGained);
            progressionManager.onXPGained.AddListener(HandleXPGained);

            progressionManager.onLevelUp.RemoveListener(HandleLevelUp);
            progressionManager.onLevelUp.AddListener(HandleLevelUp);
        }

        if (survivalManager != null)
        {
            survivalManager.onTemperatureChanged.RemoveListener(HandleTemperatureChanged);
            survivalManager.onTemperatureChanged.AddListener(HandleTemperatureChanged);

            survivalManager.onStaminaChanged.RemoveListener(HandleStaminaChanged);
            survivalManager.onStaminaChanged.AddListener(HandleStaminaChanged);

            survivalManager.onInfectionChanged.RemoveListener(HandleInfectionChanged);
            survivalManager.onInfectionChanged.AddListener(HandleInfectionChanged);

            survivalManager.onHungerChanged.RemoveListener(HandleHungerChanged);
            survivalManager.onHungerChanged.AddListener(HandleHungerChanged);

            survivalManager.onThirstChanged.RemoveListener(HandleThirstChanged);
            survivalManager.onThirstChanged.AddListener(HandleThirstChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (playerProvider != null)
        {
            playerProvider.OnHealthChanged -= HandleHealthChanged;
            playerProvider.OnArmourChanged -= HandleArmourChanged;
        }

        if (progressionManager != null)
        {
            progressionManager.onXPGained.RemoveListener(HandleXPGained);
            progressionManager.onLevelUp.RemoveListener(HandleLevelUp);
        }

        if (survivalManager != null)
        {
            survivalManager.onTemperatureChanged.RemoveListener(HandleTemperatureChanged);
            survivalManager.onStaminaChanged.RemoveListener(HandleStaminaChanged);
            survivalManager.onInfectionChanged.RemoveListener(HandleInfectionChanged);
            survivalManager.onHungerChanged.RemoveListener(HandleHungerChanged);
            survivalManager.onThirstChanged.RemoveListener(HandleThirstChanged);
        }
    }

    private void RefreshAll()
    {
        UpdateHealthDisplay();
        UpdateShieldDisplay();
        UpdateXPDisplay();
        UpdateTemperatureDisplay();
        UpdateStaminaDisplay();
        UpdateInfectionDisplay();
        UpdateHungerDisplay();
        UpdateThirstDisplay();
    }

    private void HandleHealthChanged(float current, float max)
    {
        UpdateHealthDisplay();
    }

    private void HandleArmourChanged(float current, float max)
    {
        UpdateShieldDisplay();
    }

    private void HandleXPGained(int amount)
    {
        UpdateXPDisplay();
    }

    private void HandleLevelUp(int level)
    {
        UpdateXPDisplay();
    }

    private void HandleTemperatureChanged(float value)
    {
        UpdateTemperatureDisplay();
    }

    private void HandleStaminaChanged(float value)
    {
        UpdateStaminaDisplay();
    }

    private void HandleInfectionChanged(float value)
    {
        UpdateInfectionDisplay();
    }

    private void HandleHungerChanged(float value)
    {
        UpdateHungerDisplay();
    }

    private void HandleThirstChanged(float value)
    {
        UpdateThirstDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (playerProvider == null) return;

        float value = playerProvider.Health;
        float maxValue = playerProvider.MaxHealth;

        if (healthText != null)
        {
            float pct = maxValue > 0f ? value / maxValue * 100f : 0f;
            healthText.text = $"{Mathf.RoundToInt(pct)}%";
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxValue;
            healthSlider.value = value;
        }
    }

    private void UpdateShieldDisplay()
    {
        if (playerProvider == null) return;

        float value = playerProvider.Shield;
        float maxValue = playerProvider.MaxShield;

        if (shieldText != null)
        {
            float pct = maxValue > 0f ? value / maxValue * 100f : 0f;
            shieldText.text = $"{Mathf.RoundToInt(pct)}%";
        }

        if (shieldSlider != null)
        {
            shieldSlider.minValue = 0f;
            shieldSlider.maxValue = 100f;
            shieldSlider.value = maxValue > 0f ? value / maxValue * 100f : 0f;
        }
    }

    private void UpdateXPDisplay()
    {
        if (progressionManager == null) return;

        if (levelText != null) levelText.text = $"{progressionManager.currentLevel}";
        if (xpText != null) xpText.text = $"{progressionManager.currentXP}";
        if (xpSlider != null) xpSlider.value = Mathf.Clamp01(progressionManager.GetXPProgress());
    }

    private void UpdateTemperatureDisplay()
    {
        float current;
        float max;

        if (survivalStatsProvider != null)
        {
            current = survivalStatsProvider.Temperature;
            max = Mathf.Max(1f, survivalStatsProvider.MaxTemperature);
        }
        else if (survivalManager != null)
        {
            current = survivalManager.currentTemperature;
            max = Mathf.Max(1f, survivalManager.maxTemperature);
        }
        else
        {
            return;
        }

        if (temperatureText != null)
        {
            string display = showTemperaturePrefix ? $"Temp: {current:F1}°C" : $"{current:F1}°C";
            temperatureText.text = display;
        }

        if (temperatureSlider != null)
        {
            temperatureSlider.maxValue = max;
            temperatureSlider.value = current;
        }
    }

    private void UpdateStaminaDisplay()
    {
        float current;
        float max;

        if (survivalStatsProvider != null)
        {
            current = survivalStatsProvider.Stamina;
            max = Mathf.Max(1f, survivalStatsProvider.MaxStamina);
        }
        else if (survivalManager != null)
        {
            current = survivalManager.currentStamina;
            max = Mathf.Max(1f, survivalManager.maxStamina);
        }
        else
        {
            return;
        }

        if (staminaText != null)
        {
            string display = showStaminaPrefix
                ? $"Stamina: {Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}"
                : $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            staminaText.text = display;
        }

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = current;
        }
    }

    private void UpdateInfectionDisplay()
    {
        float current;
        float max;

        if (survivalStatsProvider != null)
        {
            current = survivalStatsProvider.Infection;
            max = Mathf.Max(1f, survivalStatsProvider.MaxInfection);
        }
        else if (survivalManager != null)
        {
            current = survivalManager.currentInfection;
            max = Mathf.Max(1f, survivalManager.maxInfection);
        }
        else
        {
            return;
        }

        float pct = current / max * 100f;

        if (infectionText != null)
        {
            string display = showInfectionPrefix
                ? $"Immunity: {Mathf.RoundToInt(pct)}%"
                : $"{Mathf.RoundToInt(pct)}%";
            infectionText.text = display;
        }

        if (infectionSlider != null)
        {
            infectionSlider.maxValue = max;
            infectionSlider.value = current;
        }
    }

    private void UpdateHungerDisplay()
    {
        float current;
        float max;

        if (survivalStatsProvider != null)
        {
            current = survivalStatsProvider.Hunger;
            max = Mathf.Max(1f, survivalStatsProvider.MaxHunger);
        }
        else if (survivalManager != null)
        {
            current = survivalManager.currentHunger;
            max = Mathf.Max(1f, survivalManager.maxHunger);
        }
        else
        {
            return;
        }

        float pct = current / max * 100f;

        if (hungerText != null)
        {
            string display = showHungerPrefix ? $"Hunger: {Mathf.RoundToInt(pct)}%" : $"{Mathf.RoundToInt(pct)}%";
            hungerText.text = display;
            hungerText.color = pct <= starvingThreshold ? Color.red
                : pct <= hungryThreshold ? Color.yellow
                : Color.white;
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = max;
            hungerSlider.value = current;
        }
    }

    private void UpdateThirstDisplay()
    {
        float current;
        float max;

        if (survivalStatsProvider != null)
        {
            current = survivalStatsProvider.Thirst;
            max = Mathf.Max(1f, survivalStatsProvider.MaxThirst);
        }
        else if (survivalManager != null)
        {
            current = survivalManager.currentThirst;
            max = Mathf.Max(1f, survivalManager.maxThirst);
        }
        else
        {
            return;
        }

        float pct = current / max * 100f;

        if (thirstText != null)
        {
            string display = showThirstPrefix ? $"Thirst: {Mathf.RoundToInt(pct)}%" : $"{Mathf.RoundToInt(pct)}%";
            thirstText.text = display;
            thirstText.color = pct <= dehydratedThreshold ? Color.red
                : pct <= thirstyThreshold ? new Color(0.3f, 0.7f, 1f)
                : Color.white;
        }

        if (thirstSlider != null)
        {
            thirstSlider.maxValue = max;
            thirstSlider.value = current;
        }
    }

    private static IPlayerProvider FindBestPlayerProvider()
    {
        GC2PlayerProvider gc2Provider = FindFirstObjectByType<GC2PlayerProvider>();
        if (gc2Provider != null)
            return gc2Provider;

        return FindAnyPlayerProvider();
    }

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }

    private static ISurvivalStatsProvider FindAnySurvivalProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is ISurvivalStatsProvider provider)
                return provider;
        }
        return null;
    }

    private static void SetSlider(Slider slider, float min, float max, float value)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
    }
}
