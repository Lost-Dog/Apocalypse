using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;

/// <summary>
/// Displays the player's health attribute from the GC2 Traits component.
/// </summary>
public class PlayerHealthDisplay : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("References")]
    public TextMeshProUGUI healthText;
    public Slider healthSlider;

    [Header("Display Settings")]
    public bool showAsPercentage = false;
    public bool showFraction = true;
    public bool showPrefix = false;
    public string prefix = "HP: ";

    [Header("Auto-Find")]
    public bool autoFindReferences = true;

    private Traits playerTraits;

    private void Start()
    {
        if (autoFindReferences)
            FindReferences();

        UpdateDisplay();
    }

    private void FindReferences()
    {
        if (playerTraits == null)
        {
            GameObject player = ShortcutPlayer.Instance;
            if (player != null)
                playerTraits = player.GetComponent<Traits>();

            if (playerTraits == null)
                Debug.LogWarning("[PlayerHealthDisplay] Could not find player Traits component!");
        }

        if (healthText == null)
            healthText = GetComponent<TextMeshProUGUI>();

        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();
    }

    private void Update() => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (playerTraits == null) return;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            double value = health.Value;
            double maxValue = health.MaxValue;

            if (healthSlider != null)
            {
                healthSlider.maxValue = (float)maxValue;
                healthSlider.value = (float)value;
            }

            if (healthText != null)
            {
                string displayText;

                if (showAsPercentage)
                    displayText = $"{Mathf.RoundToInt((float)(value / maxValue) * 100f)}%";
                else if (showFraction)
                    displayText = $"{Mathf.RoundToInt((float)value)}/{Mathf.RoundToInt((float)maxValue)}";
                else
                    displayText = Mathf.RoundToInt((float)value).ToString();

                healthText.text = showPrefix ? $"{prefix}{displayText}" : displayText;
            }
        }
        catch (System.Exception) { }
    }
}
