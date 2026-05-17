using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Aggregates all player stat displays.
/// Health and shield are read through IPlayerProvider; survival stats come from SurvivalManager.
/// </summary>
public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("Manager References")]
    public SurvivalManager   survivalManager;
    public ProgressionManager progressionManager;
    [Tooltip("Assign any IPlayerProvider implementation.")]
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

    private IPlayerProvider playerProvider;

    private void Start()
    {
        if (autoFindReferences)
            FindReferences();

        InitializeSliders();
    }

    private void FindReferences()
    {
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();

        if (progressionManager == null)
            progressionManager = FindFirstObjectByType<ProgressionManager>();

        playerProvider = playerProviderObject as IPlayerProvider;

        if (playerProvider == null)
            playerProvider = FindAnyPlayerProvider();

        if (playerProvider == null)
            Debug.LogWarning("[PlayerStatsDisplay] No IPlayerProvider found — health and shield will not update.");
    }

    private void InitializeSliders()
    {
        if (playerProvider != null)
        {
            SetSlider(healthSlider, 0f, playerProvider.MaxHealth, playerProvider.Health);
            SetSlider(shieldSlider, 0f, playerProvider.MaxShield, playerProvider.Shield);
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
        }

        if (survivalManager != null)
        {
            SetSlider(temperatureSlider, 0f, survivalManager.maxTemperature, survivalManager.currentTemperature);
            SetSlider(staminaSlider,     0f, survivalManager.maxStamina,      survivalManager.currentStamina);
            SetSlider(infectionSlider,   0f, survivalManager.maxInfection,    survivalManager.currentInfection);
            SetSlider(hungerSlider,      0f, survivalManager.maxHunger,       survivalManager.currentHunger);
            SetSlider(thirstSlider,      0f, survivalManager.maxThirst,       survivalManager.currentThirst);
        }
    }

    private void Update()
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

    private void UpdateHealthDisplay()
    {
        if (playerProvider == null) return;

        float value    = playerProvider.Health;
        float maxValue = playerProvider.MaxHealth;

        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(maxValue)}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxValue;
            healthSlider.value    = value;
        }
    }

    private void UpdateShieldDisplay()
    {
        if (playerProvider == null) return;

        float value    = playerProvider.Shield;
        float maxValue = playerProvider.MaxShield;

        if (shieldText != null)
            shieldText.text = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(maxValue)}";

        if (shieldSlider != null)
        {
            shieldSlider.maxValue = maxValue;
            shieldSlider.value    = value;
        }
    }

    private void UpdateXPDisplay()
    {
        if (progressionManager == null) return;

        if (levelText != null) levelText.text = $"{progressionManager.currentLevel}";
        if (xpText != null)    xpText.text    = $"{progressionManager.currentXP}";
        if (xpSlider != null)  xpSlider.value = progressionManager.GetXPProgress();
    }

    private void UpdateTemperatureDisplay()
    {
        if (survivalManager == null) return;

        if (temperatureText != null)
        {
            string display = showTemperaturePrefix
                ? $"Temp: {survivalManager.currentTemperature:F1}°C"
                : $"{survivalManager.currentTemperature:F1}°C";
            temperatureText.text = display;
        }

        if (temperatureSlider != null) temperatureSlider.value = survivalManager.currentTemperature;
    }

    private void UpdateStaminaDisplay()
    {
        if (survivalManager == null) return;

        if (staminaText != null)
        {
            string display = showStaminaPrefix
                ? $"Stamina: {Mathf.RoundToInt(survivalManager.currentStamina)}/{Mathf.RoundToInt(survivalManager.maxStamina)}"
                : $"{Mathf.RoundToInt(survivalManager.currentStamina)}/{Mathf.RoundToInt(survivalManager.maxStamina)}";
            staminaText.text = display;
        }

        if (staminaSlider != null) staminaSlider.value = survivalManager.currentStamina;
    }

    private void UpdateInfectionDisplay()
    {
        if (survivalManager == null) return;

        if (infectionText != null)
        {
            string display = showInfectionPrefix
                ? $"Infection: {Mathf.RoundToInt(survivalManager.currentInfection)}%"
                : $"{Mathf.RoundToInt(survivalManager.currentInfection)}%";
            infectionText.text = display;
        }

        if (infectionSlider != null) infectionSlider.value = survivalManager.currentInfection;
    }

    private void UpdateHungerDisplay()
    {
        if (survivalManager == null) return;

        if (hungerText != null)
        {
            string display = showHungerPrefix
                ? $"Hunger: {Mathf.RoundToInt(survivalManager.currentHunger)}%"
                : $"{Mathf.RoundToInt(survivalManager.currentHunger)}%";
            hungerText.text  = display;
            hungerText.color = survivalManager.IsStarving ? Color.red
                             : survivalManager.IsHungry   ? Color.yellow
                             : Color.white;
        }

        if (hungerSlider != null) hungerSlider.value = survivalManager.currentHunger;
    }

    private void UpdateThirstDisplay()
    {
        if (survivalManager == null) return;

        if (thirstText != null)
        {
            string display = showThirstPrefix
                ? $"Thirst: {Mathf.RoundToInt(survivalManager.currentThirst)}%"
                : $"{Mathf.RoundToInt(survivalManager.currentThirst)}%";
            thirstText.text  = display;
            thirstText.color = survivalManager.IsDehydrated ? Color.red
                             : survivalManager.IsThirsty    ? new Color(0.3f, 0.7f, 1f)
                             : Color.white;
        }

        if (thirstSlider != null) thirstSlider.value = survivalManager.currentThirst;
    }

    // HELPERS -------------------------------------------------------------------------------------

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }

    private static void SetSlider(Slider slider, float min, float max, float value)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value    = value;
    }
}
