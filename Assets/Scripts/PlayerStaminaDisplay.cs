using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a self-managed stamina bar.
/// Stamina drains when the SurvivalManager reports the player is running and regenerates otherwise.
/// </summary>
public class PlayerStaminaDisplay : MonoBehaviour
{
    [Header("Stamina Settings")]
    [Range(0f, 100f)] public float currentStamina = 100f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 15f;
    public float staminaDrainRate = 8f;

    [Header("References")]
    public TextMeshProUGUI staminaText;
    public Slider staminaSlider;
    public Image staminaDial;

    [Header("Display Settings")]
    public bool showAsPercentage = false;
    public bool showFraction = true;
    public bool showPrefix = false;
    public string prefix = "Stamina: ";

    [Header("Dial Settings")]
    [Tooltip("Enable dial fill and color updates")]
    public bool enableDial = false;

    [Tooltip("Smooth transition speed for dial")]
    public float dialTransitionSpeed = 4f;

    [Header("Dial Colors")]
    public Color fullStaminaColor     = new Color(0f, 1f, 0.2f, 1f);
    public Color highStaminaColor     = new Color(0.5f, 1f, 0f, 1f);
    public Color moderateStaminaColor = new Color(1f, 0.92f, 0.016f, 1f);
    public Color lowStaminaColor      = new Color(1f, 0.5f, 0f, 1f);
    public Color criticalStaminaColor = new Color(1f, 0f, 0f, 1f);

    [Header("Auto-Find")]
    public bool autoFindReferences = true;

    private float currentDialFill = 1f;
    private float targetDialFill  = 1f;

    private void Start()
    {
        if (autoFindReferences)
            FindReferences();

        InitializeSlider();
        UpdateDisplay();
    }

    private void FindReferences()
    {
        if (staminaText == null)
            staminaText = GetComponent<TextMeshProUGUI>();

        if (staminaSlider == null)
            staminaSlider = GetComponent<Slider>();

        if (staminaDial == null && enableDial)
        {
            staminaDial = GetComponent<Image>() ?? GetComponentInChildren<Image>();

            if (staminaDial != null && staminaDial.type != Image.Type.Filled)
            {
                staminaDial.type = Image.Type.Filled;
                staminaDial.fillMethod = Image.FillMethod.Radial360;
            }
        }
    }

    private void InitializeSlider()
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        if (staminaDial != null && enableDial)
        {
            currentDialFill = currentStamina / maxStamina;
            targetDialFill  = currentDialFill;
            staminaDial.fillAmount = currentDialFill;
            UpdateDialColor();
        }
    }

    private void Update()
    {
        UpdateStamina();
        UpdateDisplay();
    }

    private void UpdateStamina()
    {
        // SurvivalManager owns the stamina drain/regen logic when present;
        // fall back to a self-managed version so the bar is never empty.
        bool isDraining = SurvivalManager.Instance != null
            ? SurvivalManager.Instance.currentStamina < SurvivalManager.Instance.maxStamina * 0.5f
            : false;

        currentStamina = isDraining
            ? Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime)
            : Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
    }

    private void UpdateDisplay()
    {
        if (staminaText != null)
        {
            string displayText;

            if (showAsPercentage)
                displayText = $"{Mathf.RoundToInt((currentStamina / maxStamina) * 100f)}%";
            else if (showFraction)
                displayText = $"{Mathf.RoundToInt(currentStamina)}/{Mathf.RoundToInt(maxStamina)}";
            else
                displayText = Mathf.RoundToInt(currentStamina).ToString();

            staminaText.text = showPrefix ? $"{prefix}{displayText}" : displayText;
        }

        if (staminaSlider != null)
            staminaSlider.value = currentStamina;

        if (staminaDial != null && enableDial)
            UpdateDialDisplay();
    }

    private void UpdateDialDisplay()
    {
        targetDialFill  = currentStamina / maxStamina;
        currentDialFill = Mathf.MoveTowards(currentDialFill, targetDialFill, dialTransitionSpeed * Time.deltaTime);
        staminaDial.fillAmount = currentDialFill;
        UpdateDialColor();
    }

    private void UpdateDialColor()
    {
        if (staminaDial == null) return;

        float pct = currentDialFill * 100f;
        Color color;

        if (pct >= 75f)
            color = Color.Lerp(highStaminaColor, fullStaminaColor, Mathf.InverseLerp(75f, 100f, pct));
        else if (pct >= 50f)
            color = Color.Lerp(moderateStaminaColor, highStaminaColor, Mathf.InverseLerp(50f, 75f, pct));
        else if (pct >= 25f)
            color = Color.Lerp(lowStaminaColor, moderateStaminaColor, Mathf.InverseLerp(25f, 50f, pct));
        else if (pct > 0f)
            color = Color.Lerp(criticalStaminaColor, lowStaminaColor, Mathf.InverseLerp(0f, 25f, pct));
        else
            color = criticalStaminaColor;

        staminaDial.color = color;
    }

    // PUBLIC API ----------------------------------------------------------------------------------

    /// <summary>Drains the given amount of stamina immediately.</summary>
    public void DrainStamina(float amount) =>
        currentStamina = Mathf.Max(0f, currentStamina - amount);

    /// <summary>Restores the given amount of stamina.</summary>
    public void RestoreStamina(float amount) =>
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);

    /// <summary>Returns true if at least the given amount of stamina is available.</summary>
    public bool HasStamina(float amount) => currentStamina >= amount;
}
