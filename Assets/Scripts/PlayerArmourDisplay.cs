using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's armour using IPlayerProvider.OnArmourChanged events.
/// Subscribes to armour changes rather than polling — zero per-frame overhead.
/// Drives a fill Image (fillAmount 0–1) and an optional TextMeshPro label.
/// </summary>
public class PlayerArmourDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Fill Image whose fillAmount is driven by armour (0 = empty, 1 = full).")]
    public Image           armourFill;
    public TextMeshProUGUI armourText;
    [Tooltip("Assign any IPlayerProvider implementation. Auto-found if left empty.")]
    public MonoBehaviour   playerProviderObject;

    [Header("Display Settings")]
    [Tooltip("Show the raw current/max value instead of a percentage.")]
    public bool   showRawValue = false;
    public string suffix       = "%";

    // Exposed for downstream warning/status scripts.
    [HideInInspector] public SurvivalManager survivalManager;

    private IPlayerProvider _playerProvider;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        FindReferences();
    }

    private void OnDestroy()
    {
        if (_playerProvider != null)
            _playerProvider.OnArmourChanged -= UpdateDisplay;
    }

    // ── Wiring ────────────────────────────────────────────────────────────────

    private void FindReferences()
    {
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance;

        _playerProvider = playerProviderObject as IPlayerProvider;

        if (_playerProvider == null)
            _playerProvider = FindAnyPlayerProvider();

        if (_playerProvider == null)
        {
            Debug.LogWarning("[PlayerArmourDisplay] No IPlayerProvider found.");
            return;
        }

        _playerProvider.OnArmourChanged += UpdateDisplay;

        if (armourText == null)
            armourText = GetComponentInChildren<TextMeshProUGUI>();

        if (armourFill == null)
            armourFill = GetComponentInChildren<Image>();

        // Prime the display immediately.
        UpdateDisplay(_playerProvider.Armour, _playerProvider.MaxArmour);
    }

    // ── Display ───────────────────────────────────────────────────────────────

    /// <summary>Called by IPlayerProvider.OnArmourChanged; also safe to call manually.</summary>
    public void UpdateDisplay(float current, float max)
    {
        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        if (armourFill != null)
            armourFill.fillAmount = normalized;

        if (armourText != null)
        {
            armourText.text = showRawValue
                ? $"{Mathf.RoundToInt(current)}{suffix}"
                : $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Adds the given amount to the player's armour via SurvivalManager.</summary>
    public void AddArmour(float amount)
    {
        if (survivalManager != null)
            survivalManager.ModifyArmour(amount);
    }

    /// <summary>Removes the given amount from the player's armour via SurvivalManager.</summary>
    public void RemoveArmour(float amount) => AddArmour(-amount);

    /// <summary>Clears armour to zero via SurvivalManager.</summary>
    public void ClearArmour() => survivalManager?.SetArmour(0f);

    /// <summary>Returns current armour as a 0–1 normalised value.</summary>
    public float GetArmourPercentage()
    {
        if (_playerProvider == null) return 0f;
        float max = _playerProvider.MaxArmour;
        return max > 0f ? Mathf.Clamp01(_playerProvider.Armour / max) : 0f;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }
}
