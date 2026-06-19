using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Reads the player's health from IPlayerProvider and drives a stat-dial
/// Animator and label text. Subscribes to OnHealthChanged for reactive updates.
/// Also holds a reference to SurvivalManager for downstream warning scripts.
/// </summary>
public class PlayerHealthDialDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI healthText;
    public Animator        dialAnimator;
    public Image           fillImage;
    [Tooltip("Assign any IPlayerProvider implementation. Auto-found if left empty.")]
    public MonoBehaviour   playerProviderObject;

    [Header("Display Settings")]
    public string suffix = "%";

    // Exposed so warning/status scripts can query without finding it themselves.
    [HideInInspector] public SurvivalManager survivalManager;

    private IPlayerProvider playerProvider;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        FindReferences();
        if (playerProvider != null)
            playerProvider.OnHealthChanged += OnHealthChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        if (playerProvider != null)
            playerProvider.OnHealthChanged -= OnHealthChanged;
    }

    // ── Wiring ────────────────────────────────────────────────────────────────

    private void FindReferences()
    {
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance;

        if (survivalManager == null)
            Debug.LogWarning("[PlayerHealthDialDisplay] SurvivalManager not found.");

        playerProvider = playerProviderObject as IPlayerProvider;

        if (playerProvider == null)
            playerProvider = FindBestPlayerProvider();

        if (playerProvider == null)
            Debug.LogWarning("[PlayerHealthDialDisplay] No IPlayerProvider found.");

        if (healthText   == null) healthText   = GetComponentInChildren<TextMeshProUGUI>();
        if (dialAnimator == null) dialAnimator = GetComponentInParent<Animator>();

        if (fillImage == null)
            fillImage = GetComponent<Image>();

        if (fillImage == null)
        {
            Transform hudPlayer = transform.Find("HUD_Player");
            if (hudPlayer != null)
                fillImage = hudPlayer.GetComponent<Image>();
        }

        if (fillImage == null)
        {
            GameObject namedHudPlayer = GameObject.Find("HUD_Player");
            if (namedHudPlayer != null)
                fillImage = namedHudPlayer.GetComponent<Image>();
        }
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnHealthChanged(float current, float max)
    {
        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        UpdateDial(normalized);
        UpdateLabel(normalized);
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void UpdateDial(float normalized)
    {
        if (dialAnimator != null)
            dialAnimator.SetFloat(DialParameter, normalized);

        if (fillImage != null)
            fillImage.fillAmount = normalized;
    }

    private void UpdateLabel(float normalized)
    {
        if (healthText == null) return;
        healthText.text = $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
    }

    /// <summary>Forces an immediate refresh from the current provider values.</summary>
    public void Refresh()
    {
        if (playerProvider == null) return;
        OnHealthChanged(playerProvider.Health, playerProvider.MaxHealth);
    }

    /// <summary>Returns current HP as a 0–1 normalised value.</summary>
    public float GetHealthPercentage()
    {
        if (playerProvider == null) return 0f;
        return playerProvider.MaxHealth > 0f
            ? Mathf.Clamp01(playerProvider.Health / playerProvider.MaxHealth)
            : 0f;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IPlayerProvider FindBestPlayerProvider()
    {
        GC2PlayerProvider gc2Provider = Object.FindFirstObjectByType<GC2PlayerProvider>();
        if (gc2Provider != null)
            return gc2Provider;

        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }
}
