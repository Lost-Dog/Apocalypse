using UnityEngine;
using TMPro;

/// <summary>
/// Reads the player's shield value from IPlayerProvider each frame and drives
/// the stat-dial Animator and label text.
/// </summary>
public class PlayerArmourDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI armourText;
    public Animator        dialAnimator;
    [Tooltip("Assign any IPlayerProvider implementation.")]
    public MonoBehaviour playerProviderObject;

    [Header("Display Settings")]
    public string suffix = "%";

    // Exposed for downstream warning/status scripts.
    [HideInInspector] public SurvivalManager survivalManager;

    private IPlayerProvider playerProvider;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        FindReferences();
    }

    private void Update()
    {
        if (playerProvider == null)
        {
            FindReferences();
            return;
        }

        float normalized = GetNormalizedShield();
        UpdateDial(normalized);
        UpdateLabel(normalized);
    }

    // ── Wiring ────────────────────────────────────────────────────────────────

    private void FindReferences()
    {
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance;

        if (survivalManager == null)
            Debug.LogWarning("[PlayerArmourDisplay] SurvivalManager not found.");

        playerProvider = playerProviderObject as IPlayerProvider;

        if (playerProvider == null)
            playerProvider = FindAnyPlayerProvider();

        if (playerProvider == null)
            Debug.LogWarning("[PlayerArmourDisplay] No IPlayerProvider found.");

        if (armourText   == null) armourText   = GetComponentInChildren<TextMeshProUGUI>();
        if (dialAnimator == null) dialAnimator = GetComponentInParent<Animator>();
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    private float GetNormalizedShield()
    {
        if (playerProvider == null) return 0f;
        float max = playerProvider.MaxShield;
        return max > 0f ? Mathf.Clamp01(playerProvider.Shield / max) : 0f;
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void UpdateDial(float normalized)
    {
        if (dialAnimator != null)
            dialAnimator.SetFloat(DialParameter, normalized);
    }

    private void UpdateLabel(float normalized)
    {
        if (armourText == null) return;
        armourText.text = $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Adds the given amount to the player's shield.</summary>
    public void AddArmour(float amount)
    {
        if (playerProvider == null) return;
        playerProvider.SetShield(playerProvider.Shield + amount);
    }

    /// <summary>Removes the given amount from the player's shield.</summary>
    public void RemoveArmour(float amount) => AddArmour(-amount);

    /// <summary>Clears shield to zero.</summary>
    public void ClearArmour() => playerProvider?.SetShield(0f);

    /// <summary>Returns current shield as a 0–1 normalised value.</summary>
    public float GetArmourPercentage() => GetNormalizedShield();

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
