using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Reads the player's hunger from provider traits (ISurvivalStatsProvider) and falls back to SurvivalManager.
/// Drives text, dial animator, and optional slider.
/// </summary>
public class PlayerHungerDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI hungerText;
    public Animator        dialAnimator;
    public Slider          hungerSlider;
    [Tooltip("Optional SurvivalManager reference. Auto-found if left empty.")]
    public SurvivalManager survivalManager;
    [Tooltip("Optional provider reference. If assigned, should implement IPlayerProvider/ISurvivalStatsProvider.")]
    public MonoBehaviour   playerProviderObject;

    [Header("Auto-Find")]
    public bool autoFindReferences = true;

    [Header("Display Settings")]
    public string suffix = "%";

    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;

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
        if (autoFindReferences)
        {
            if (survivalManager == null)
                survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();

            if (survivalManager != null)
            {
                survivalManager.EnsurePlayerProviderBinding();
                if (playerProviderObject == null && survivalManager.playerProviderObject != null)
                    playerProviderObject = survivalManager.playerProviderObject;
            }

            playerProvider = playerProviderObject as IPlayerProvider;
            if (playerProvider == null)
                playerProvider = FindBestPlayerProvider();

            if (playerProviderObject == null && playerProvider is MonoBehaviour providerBehaviour)
                playerProviderObject = providerBehaviour;

            survivalStatsProvider = playerProvider as ISurvivalStatsProvider;

            if (hungerText == null)
                hungerText = GetComponentInChildren<TextMeshProUGUI>();

            if (dialAnimator == null)
                dialAnimator = GetComponentInParent<Animator>();

            if (hungerSlider == null)
            {
                Slider[] sliders = GetComponentsInChildren<Slider>(true);
                for (int i = 0; i < sliders.Length; i++)
                {
                    if (sliders[i] != null)
                    {
                        hungerSlider = sliders[i];
                        break;
                    }
                }
            }
        }
        else
        {
            playerProvider = playerProviderObject as IPlayerProvider;
            survivalStatsProvider = playerProvider as ISurvivalStatsProvider;
        }

        if (survivalManager == null)
            Debug.LogWarning("[PlayerHungerDisplay] SurvivalManager not found.");

        if (survivalStatsProvider == null && playerProvider == null)
            Debug.LogWarning("[PlayerHungerDisplay] No player provider found. Falling back to SurvivalManager values only.");
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
        float maxHunger = 100f;

        if (survivalStatsProvider != null)
            maxHunger = Mathf.Max(1f, survivalStatsProvider.MaxHunger);
        else if (survivalManager != null)
            maxHunger = Mathf.Max(1f, survivalManager.maxHunger);

        float normalized = Mathf.Clamp01(newHunger / maxHunger);

        UpdateDial(normalized);
        UpdateLabel(normalized);

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = Mathf.Clamp(newHunger, 0f, maxHunger);
        }
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
        if (survivalStatsProvider != null)
        {
            OnHungerChanged(survivalStatsProvider.Hunger);
            return;
        }

        if (survivalManager == null)
        {
            FindReferences();
            if (survivalManager == null) return;
        }

        OnHungerChanged(survivalManager.currentHunger);
    }

    private static IPlayerProvider FindBestPlayerProvider()
    {
        GC2PlayerProvider gc2Provider = FindFirstObjectByType<GC2PlayerProvider>();
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
