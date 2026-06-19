using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Reads the player's immunity level from SurvivalManager and drives the
/// stat-dial Animator and label text. Also applies periodic health damage when
/// immunity is at zero via IPlayerProvider.
/// </summary>
public class PlayerInfectionDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI infectionText;
    public Animator        dialAnimator;
    public Slider          infectionSlider;

    [Header("Health Damage at Max Infection")]
    [Tooltip("Legacy fallback only. Keep disabled when SurvivalManager infection effects are enabled to avoid duplicate hidden health drain.")]
    public bool  enableHealthDamage    = false;
    public float healthDamagePerSecond = 2f;
    public float damageTickInterval    = 1f;

    [Header("Display Settings")]
    public string suffix = "%";

    private SurvivalManager survivalManager;
    private IPlayerProvider playerProvider;
    private float damageTimer;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        FindReferences();
        Subscribe();
        damageTimer = damageTickInterval;
        RefreshFromState();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshFromState();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void FindReferences()
    {
        if (survivalManager == null)
            survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();

        if (survivalManager == null)
            Debug.LogWarning("[PlayerInfectionDisplay] SurvivalManager not found.");

        if (playerProvider == null)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is IPlayerProvider provider)
                {
                    playerProvider = provider;
                    break;
                }
            }
        }

        if (infectionText == null)
            infectionText = GetComponentInChildren<TextMeshProUGUI>();

        if (dialAnimator == null)
            dialAnimator = GetComponentInParent<Animator>();

        if (infectionSlider == null)
            infectionSlider = GetComponentInChildren<Slider>(true);
    }

    private void Update()
    {
        if (survivalManager == null)
        {
            FindReferences();
            Subscribe();
            return;
        }

        // Only tick fallback damage logic per-frame when that legacy path is active.
        if (enableHealthDamage)
            ApplyInfectionDamage(GetNormalizedInfection());
    }

    private void Subscribe()
    {
        if (survivalManager == null)
            return;

        survivalManager.onInfectionChanged.RemoveListener(OnInfectionChanged);
        survivalManager.onInfectionChanged.AddListener(OnInfectionChanged);
    }

    private void Unsubscribe()
    {
        if (survivalManager != null)
            survivalManager.onInfectionChanged.RemoveListener(OnInfectionChanged);
    }

    private void OnInfectionChanged(float value)
    {
        RefreshFromState();
    }

    private void RefreshFromState()
    {
        float normalized = GetNormalizedInfection();
        UpdateDial(normalized);
        UpdateLabel(normalized);
        UpdateSlider(normalized);
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    private float GetNormalizedInfection()
    {
        if (survivalManager == null || survivalManager.maxInfection <= 0f) return 0f;
        return Mathf.Clamp01(survivalManager.currentInfection / survivalManager.maxInfection);
    }

    /// <summary>Current infection value in the 0–100 range (for backwards compatibility).</summary>
    public float currentInfection => GetNormalizedInfection() * 100f;

    // ── Display ───────────────────────────────────────────────────────────────

    private void UpdateDial(float normalized)
    {
        if (dialAnimator != null)
            dialAnimator.SetFloat(DialParameter, normalized);
    }

    private void UpdateLabel(float normalized)
    {
        if (infectionText == null) return;
        infectionText.text = $"{Mathf.RoundToInt(normalized * 100f)}{suffix}";
    }

    private void UpdateSlider(float normalized)
    {
        if (infectionSlider == null) return;

        infectionSlider.minValue = 0f;
        infectionSlider.maxValue = 1f;
        infectionSlider.value = normalized;
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    private void ApplyInfectionDamage(float normalized)
    {
        // SurvivalManager is the authoritative infection damage source.
        // This fallback prevents hidden duplicate damage when both systems are active.
        if (survivalManager != null && survivalManager.enableInfectionSystem)
            return;

        if (!enableHealthDamage || normalized > 0f || playerProvider == null) return;

        damageTimer -= Time.deltaTime;
        if (damageTimer > 0f) return;

        damageTimer = damageTickInterval;
        playerProvider.ApplyDamage(healthDamagePerSecond * damageTickInterval);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Adds infection exposure, reducing immunity by the given amount in SurvivalManager.</summary>
    public void AddInfection(float amount)
    {
        if (survivalManager == null) return;
        survivalManager.AddInfection(amount);
    }

    /// <summary>Removes infection exposure, restoring immunity by the given amount.</summary>
    public void RemoveInfection(float amount) => AddInfection(-amount);

    /// <summary>Restores immunity to full in SurvivalManager.</summary>
    public void CureInfection()
    {
        if (survivalManager != null)
            survivalManager.CureInfection(survivalManager.maxInfection);
    }

    /// <summary>Returns true if immunity is below maximum (player has some infection).</summary>
    public bool IsInfected() => GetNormalizedInfection() < 1f;

    /// <summary>Returns immunity as a 0–1 normalised value (1 = fully immune, 0 = no immunity).</summary>
    public float GetInfectionPercentage() => GetNormalizedInfection();
}
