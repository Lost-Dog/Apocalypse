using UnityEngine;
using TMPro;

/// <summary>
/// Reads the player's hunger from SurvivalManager and drives the stat-dial Animator and label text.
/// Subscribes to SurvivalManager.onHungerChanged for event-driven updates.
/// </summary>
public class PlayerHungerDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI hungerText;
    public Animator        dialAnimator;

    [Header("Display Settings")]
    public string suffix = "%";

    private SurvivalManager survivalManager;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        FindReferences();
        SubscribeEvents();
        Refresh();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // ── Wiring ────────────────────────────────────────────────────────────────

    private void FindReferences()
    {
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance;

        if (survivalManager == null)
            Debug.LogWarning("[PlayerHungerDisplay] SurvivalManager not found.");

        if (hungerText == null)
            hungerText = GetComponentInChildren<TextMeshProUGUI>();

        if (dialAnimator == null)
            dialAnimator = GetComponentInParent<Animator>();
    }

    private void SubscribeEvents()
    {
        if (survivalManager != null)
            survivalManager.onHungerChanged.AddListener(OnHungerChanged);
    }

    private void UnsubscribeEvents()
    {
        if (survivalManager != null)
            survivalManager.onHungerChanged.RemoveListener(OnHungerChanged);
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnHungerChanged(float newHunger)
    {
        float normalized = survivalManager != null && survivalManager.maxHunger > 0f
            ? Mathf.Clamp01(newHunger / survivalManager.maxHunger)
            : 0f;

        UpdateDial(normalized);
        UpdateLabel(normalized);
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void UpdateDial(float normalized)
    {
        if (dialAnimator != null)
            dialAnimator.SetFloat(DialParameter, normalized);
    }

    private void UpdateLabel(float normalized)
    {
        if (hungerText == null) return;
        hungerText.text = $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
    }

    /// <summary>Forces an immediate refresh from SurvivalManager's current state.</summary>
    public void Refresh()
    {
        if (survivalManager == null)
        {
            FindReferences();
            if (survivalManager == null) return;
        }

        OnHungerChanged(survivalManager.currentHunger);
    }
}
