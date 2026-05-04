using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;

/// <summary>
/// Aggregates all player stat displays. Health reads from GC2 Traits; survival stats from SurvivalManager.
/// </summary>
public class PlayerStatsDisplay : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Manager References")]
    public SurvivalManager survivalManager;
    public ProgressionManager progressionManager;

    [Header("UI Text Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI infectionText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;

    [Header("UI Slider Elements (Optional)")]
    public Slider healthSlider;
    public Slider xpSlider;
    public Slider temperatureSlider;
    public Slider staminaSlider;
    public Slider infectionSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;

    [Header("Display Settings")]
    public bool showTemperaturePrefix = false;
    public bool showStaminaPrefix = false;
    public bool showInfectionPrefix = false;
    public bool showHungerPrefix = false;
    public bool showThirstPrefix = false;

    [Header("Auto-Find References")]
    public bool autoFindReferences = true;

    private Traits playerTraits;

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

        if (playerTraits == null)
        {
            GameObject player = ShortcutPlayer.Instance;
            if (player != null)
                playerTraits = player.GetComponent<Traits>();

            if (playerTraits == null)
                Debug.LogWarning("[PlayerStatsDisplay] Could not find player Traits component!");
        }

        if (progressionManager == null)
            progressionManager = FindFirstObjectByType<ProgressionManager>();
    }

    private void InitializeSliders()
    {
        if (healthSlider != null && playerTraits != null)
        {
            try
            {
                RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
                healthSlider.maxValue = (float)health.MaxValue;
                healthSlider.value    = (float)health.Value;
            }
            catch (System.Exception) { }
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
        UpdateXPDisplay();
        UpdateTemperatureDisplay();
        UpdateStaminaDisplay();
        UpdateInfectionDisplay();
        UpdateHungerDisplay();
        UpdateThirstDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (playerTraits == null) return;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            float value    = (float)health.Value;
            float maxValue = (float)health.MaxValue;

            if (healthText != null)
                healthText.text = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(maxValue)}";

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxValue;
                healthSlider.value    = value;
            }
        }
        catch (System.Exception) { }
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
            string status  = survivalManager.GetTemperatureStatus();
            string display = showTemperaturePrefix
                ? $"Temp: {survivalManager.currentTemperature:F1}°C ({status})"
                : $"{survivalManager.currentTemperature:F1}°C ({status})";
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
            string status  = survivalManager.GetInfectionStatus();
            string display = showInfectionPrefix
                ? $"Infection: {Mathf.RoundToInt(survivalManager.currentInfection)}% ({status})"
                : $"{Mathf.RoundToInt(survivalManager.currentInfection)}% ({status})";
            infectionText.text = display;
        }

        if (infectionSlider != null) infectionSlider.value = survivalManager.currentInfection;
    }

    private void UpdateHungerDisplay()
    {
        if (survivalManager == null) return;

        if (hungerText != null)
        {
            string status  = survivalManager.GetHungerStatus();
            string display = showHungerPrefix
                ? $"Hunger: {Mathf.RoundToInt(survivalManager.currentHunger)}% ({status})"
                : $"{Mathf.RoundToInt(survivalManager.currentHunger)}% ({status})";
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
            string status  = survivalManager.GetThirstStatus();
            string display = showThirstPrefix
                ? $"Thirst: {Mathf.RoundToInt(survivalManager.currentThirst)}% ({status})"
                : $"{Mathf.RoundToInt(survivalManager.currentThirst)}% ({status})";
            thirstText.text  = display;
            thirstText.color = survivalManager.IsDehydrated ? Color.red
                             : survivalManager.IsThirsty    ? new Color(0.3f, 0.7f, 1f)
                             : Color.white;
        }

        if (thirstSlider != null) thirstSlider.value = survivalManager.currentThirst;
    }

    // HELPERS -------------------------------------------------------------------------------------

    private static void SetSlider(Slider slider, float min, float max, float value)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value    = value;
    }
}
