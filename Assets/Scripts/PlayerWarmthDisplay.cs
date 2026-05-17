using UnityEngine;
using TMPro;

/// <summary>
/// Reads the player's temperature from SurvivalManager and drives a stat-dial
/// Animator and label text. Reactive — subscribes to onTemperatureChanged.
/// </summary>
public class PlayerWarmthDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI warmthText;
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
            Debug.LogWarning("[PlayerWarmthDisplay] SurvivalManager not found.");

        if (warmthText   == null) warmthText   = GetComponentInChildren<TextMeshProUGUI>();
        if (dialAnimator == null) dialAnimator = GetComponentInParent<Animator>();
    }

    private void SubscribeEvents()
    {
        if (survivalManager != null)
            survivalManager.onTemperatureChanged.AddListener(OnTemperatureChanged);
    }

    private void UnsubscribeEvents()
    {
        if (survivalManager != null)
            survivalManager.onTemperatureChanged.RemoveListener(OnTemperatureChanged);
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnTemperatureChanged(float newTemperature)
    {
        float normalized = survivalManager.maxTemperature > 0f
            ? Mathf.Clamp01(newTemperature / survivalManager.maxTemperature)
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
        if (warmthText == null) return;
        warmthText.text = $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
    }

    /// <summary>Forces an immediate refresh from SurvivalManager's current temperature.</summary>
    public void Refresh()
    {
        if (survivalManager == null)
        {
            FindReferences();
            if (survivalManager == null) return;
        }

        OnTemperatureChanged(survivalManager.currentTemperature);
    }
}
