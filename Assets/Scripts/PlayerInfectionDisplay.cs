using UnityEngine;
using TMPro;

/// <summary>
/// Reads the player's infection level from SurvivalManager and drives the
/// stat-dial Animator and label text. Also applies periodic health damage when
/// infection is at maximum via IPlayerProvider.
/// </summary>
public class PlayerInfectionDisplay : MonoBehaviour
{
    private const string DialParameter = "Health";

    [Header("References")]
    public TextMeshProUGUI infectionText;
    public Animator        dialAnimator;

    [Header("Health Damage at Max Infection")]
    public bool  enableHealthDamage    = true;
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
        damageTimer = damageTickInterval;
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
    }

    private void Update()
    {
        if (survivalManager == null)
        {
            FindReferences();
            return;
        }

        float normalized = GetNormalizedInfection();
        UpdateDial(normalized);
        UpdateLabel(normalized);
        ApplyInfectionDamage(normalized);
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

    // ── Damage ────────────────────────────────────────────────────────────────

    private void ApplyInfectionDamage(float normalized)
    {
        if (!enableHealthDamage || normalized < 1f || playerProvider == null) return;

        damageTimer -= Time.deltaTime;
        if (damageTimer > 0f) return;

        damageTimer = damageTickInterval;
        playerProvider.ApplyDamage(healthDamagePerSecond * damageTickInterval);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Adds the given amount to the infection level in SurvivalManager.</summary>
    public void AddInfection(float amount)
    {
        if (survivalManager == null) return;
        survivalManager.currentInfection = Mathf.Clamp(
            survivalManager.currentInfection + amount,
            0f,
            survivalManager.maxInfection
        );
    }

    /// <summary>Removes the given amount from the infection level.</summary>
    public void RemoveInfection(float amount) => AddInfection(-amount);

    /// <summary>Clears infection to zero in SurvivalManager.</summary>
    public void CureInfection()
    {
        if (survivalManager != null)
            survivalManager.currentInfection = 0f;
    }

    /// <summary>Returns true if infection is above zero.</summary>
    public bool IsInfected() => GetNormalizedInfection() > 0f;

    /// <summary>Returns infection as a 0–1 normalised value.</summary>
    public float GetInfectionPercentage() => GetNormalizedInfection();
}
