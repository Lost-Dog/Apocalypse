using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's health using IPlayerProvider.
/// Subscribes to IPlayerProvider.OnHealthChanged for immediate updates and
/// avoids polling in Update. Supports any IPlayerProvider implementation —
/// assign via Inspector or let auto-find pick it up.
/// </summary>
public class PlayerHealthDisplay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI healthText;
    public Slider          healthSlider;
    [Tooltip("Assign any IPlayerProvider implementation.")]
    public MonoBehaviour playerProviderObject;

    [Header("Display Settings")]
    public bool showAsPercentage = true;
    public bool showFraction     = false;
    public bool showPrefix       = false;
    public string prefix         = "HP: ";

    [Header("Auto-Find")]
    public bool autoFindReferences = true;

    private IPlayerProvider playerProvider;

    private void Start()
    {
        ResolveProvider();
        // Defer the first read by one frame so all Start() methods — including
        // InvectorPlayerProvider.BindController() — have completed first.
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return null;
        UpdateDisplay(playerProvider?.Health ?? 0f, playerProvider?.MaxHealth ?? 1f);
    }

    private void OnDestroy()
    {
        if (playerProvider != null)
            playerProvider.OnHealthChanged -= UpdateDisplay;
    }

    /// <summary>Resolves the IPlayerProvider and subscribes to its health event.</summary>
    private void ResolveProvider()
    {
        if (autoFindReferences)
        {
            // Try the explicitly assigned object first.
            playerProvider = playerProviderObject as IPlayerProvider;

            // Fall back to any MonoBehaviour in the scene that implements the interface.
            if (playerProvider == null)
                playerProvider = FindAnyPlayerProvider();

            if (healthText   == null) healthText   = GetComponent<TextMeshProUGUI>();
            if (healthSlider == null) healthSlider = GetComponent<Slider>();
        }
        else
        {
            playerProvider = playerProviderObject as IPlayerProvider;
        }

        if (playerProvider == null)
        {
            Debug.LogWarning("[PlayerHealthDisplay] No IPlayerProvider found.");
            return;
        }

        playerProvider.OnHealthChanged += UpdateDisplay;
    }

    /// <summary>
    /// Searches the scene for the first MonoBehaviour that implements IPlayerProvider,
    /// without being tied to a specific concrete type.
    /// </summary>
    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }

    /// <summary>Called by IPlayerProvider.OnHealthChanged; also safe to call manually.</summary>
    public void UpdateDisplay(float value, float maxValue)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxValue;
            healthSlider.value    = value;
        }

        if (healthText != null)
        {
            string displayText;

            if (showAsPercentage)
                displayText = $"{Mathf.RoundToInt(maxValue > 0f ? value / maxValue * 100f : 0f)}%";
            else if (showFraction)
                displayText = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(maxValue)}";
            else
                displayText = Mathf.RoundToInt(value).ToString();

            healthText.text = showPrefix ? $"{prefix}{displayText}" : displayText;
        }
    }
}
