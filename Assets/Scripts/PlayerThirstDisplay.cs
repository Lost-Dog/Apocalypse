using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Reads the player's thirst from SurvivalManager and drives a stat-dial
/// Animator and label text. Reactive — subscribes to onThirstChanged.
/// </summary>
public class PlayerThirstDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI thirstText;
    public Animator        dialAnimator;
    public Slider          thirstSlider;

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
            Debug.LogWarning("[PlayerThirstDisplay] SurvivalManager not found.");

        if (thirstText   == null) thirstText   = GetComponentInChildren<TextMeshProUGUI>();
        if (dialAnimator == null) dialAnimator = GetComponentInParent<Animator>();
        if (thirstSlider == null) thirstSlider = GetComponentInChildren<Slider>(true);
    }

    private void SubscribeEvents()
    {
        if (survivalManager != null)
            survivalManager.onThirstChanged.AddListener(OnThirstChanged);
    }

    private void UnsubscribeEvents()
    {
        if (survivalManager != null)
            survivalManager.onThirstChanged.RemoveListener(OnThirstChanged);
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnThirstChanged(float newThirst)
    {
        float normalized = survivalManager.maxThirst > 0f
            ? Mathf.Clamp01(newThirst / survivalManager.maxThirst)
            : 0f;

        UpdateDial(normalized);
        UpdateLabel(normalized);
        UpdateSlider(normalized);
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void UpdateDial(float normalized)
    {
        if (dialAnimator != null)
            dialAnimator.SetFloat(DialParameter, normalized);
    }

    private void UpdateLabel(float normalized)
    {
        if (thirstText == null) return;
        thirstText.text = $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
    }

    private void UpdateSlider(float normalized)
    {
        if (thirstSlider == null) return;

        thirstSlider.minValue = 0f;
        thirstSlider.maxValue = 1f;
        thirstSlider.value = normalized;
    }

    /// <summary>Forces an immediate refresh from SurvivalManager's current state.</summary>
    public void Refresh()
    {
        if (survivalManager == null)
        {
            FindReferences();
            if (survivalManager == null) return;
        }

        OnThirstChanged(survivalManager.currentThirst);
    }
}
