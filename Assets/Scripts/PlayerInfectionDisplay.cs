using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;

/// <summary>
/// Tracks and displays the player's infection level.
/// Damage is applied to the player's health Attribute via GC2 Traits when infection is critical.
/// </summary>
public class PlayerInfectionDisplay : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Infection Settings")]
    [Range(0f, 100f)] public float currentInfection = 0f;
    public float maxInfection = 100f;
    public float infectionGrowthRate = 0.5f;
    public float infectionDecayRate = 1f;

    [Header("Health Damage Settings")]
    public bool enableHealthDamage = true;
    public float healthDamagePerSecond = 2f;
    public float damageTickInterval = 1f;

    [Header("Display Settings")]
    public bool showStatus = true;
    public bool showPrefix = false;
    public string prefix = "Infection: ";
    public string suffix = "%";

    [Header("References")]
    public TextMeshProUGUI infectionText;
    public Slider infectionSlider;

    [Header("Auto-Find")]
    public bool autoFindReferences = true;

    private Traits playerTraits;
    private float damageTimer;

    private void Start()
    {
        if (autoFindReferences)
            FindReferences();

        InitializeSlider();
        UpdateDisplay();
        damageTimer = damageTickInterval;
    }

    private void FindReferences()
    {
        if (playerTraits == null)
        {
            GameObject player = ShortcutPlayer.Instance;
            if (player != null)
                playerTraits = player.GetComponent<Traits>();

            if (playerTraits == null)
                Debug.LogWarning("[PlayerInfectionDisplay] Could not find player Traits component!");
        }

        if (infectionText == null)
            infectionText = GetComponent<TextMeshProUGUI>();

        if (infectionSlider == null)
            infectionSlider = GetComponent<Slider>();
    }

    private void InitializeSlider()
    {
        if (infectionSlider == null) return;

        infectionSlider.minValue = 0f;
        infectionSlider.maxValue = maxInfection;
        infectionSlider.value = currentInfection;
    }

    private void Update()
    {
        UpdateInfection();
        ApplyInfectionDamage();
        UpdateDisplay();
    }

    private void UpdateInfection()
    {
        if (currentInfection > 0f)
            currentInfection = Mathf.Max(0f, currentInfection - infectionDecayRate * Time.deltaTime);
    }

    private void ApplyInfectionDamage()
    {
        if (!enableHealthDamage || currentInfection < maxInfection || playerTraits == null) return;

        damageTimer -= Time.deltaTime;
        if (damageTimer > 0f) return;

        damageTimer = damageTickInterval;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            health.Value -= healthDamagePerSecond * damageTickInterval;
        }
        catch (System.Exception) { }
    }

    private void UpdateDisplay()
    {
        if (infectionSlider != null)
            infectionSlider.value = currentInfection;

        if (infectionText != null)
        {
            string displayText = $"{Mathf.RoundToInt(currentInfection)}{suffix}";
            if (showStatus)
                displayText += $" ({GetInfectionStatus()})";

            infectionText.text = showPrefix ? $"{prefix}{displayText}" : displayText;
        }
    }

    private string GetInfectionStatus()
    {
        if (currentInfection == 0f) return "None";
        if (currentInfection < 25f) return "Mild";
        if (currentInfection < 50f) return "Moderate";
        if (currentInfection < 75f) return "Severe";
        return "Critical";
    }

    // PUBLIC API ----------------------------------------------------------------------------------

    /// <summary>Adds the given amount to the current infection level.</summary>
    public void AddInfection(float amount) =>
        currentInfection = Mathf.Clamp(currentInfection + amount, 0f, maxInfection);

    /// <summary>Reduces the current infection level by the given amount.</summary>
    public void RemoveInfection(float amount) =>
        currentInfection = Mathf.Max(0f, currentInfection - amount);

    /// <summary>Clears all infection immediately.</summary>
    public void CureInfection() => currentInfection = 0f;

    /// <summary>Returns true if the player has any infection.</summary>
    public bool IsInfected() => currentInfection > 0f;

    /// <summary>Returns infection as a 0–1 normalised value.</summary>
    public float GetInfectionPercentage() => currentInfection / maxInfection;
}
