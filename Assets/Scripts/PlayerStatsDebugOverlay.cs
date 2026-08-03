using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Diagnostics-only overlay that dumps the player's current stats as plain text.
/// Intended for development/QA builds; disable or remove before shipping.
/// </summary>
public class PlayerStatsDebugOverlay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Text element the diagnostics dump is written to. Auto-assigned from this GameObject if left empty.")]
    public TextMeshProUGUI statsText;

    [Header("Behaviour")]
    [Tooltip("Master toggle for the overlay; useful for quickly hiding diagnostics without removing the component.")]
    public bool isEnabled = true;

    [Tooltip("How often, in seconds, the displayed stats are refreshed.")]
    public float refreshInterval = 0.2f;

    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;
    private ProgressionManager progressionManager;
    private float refreshTimer;

    private void Awake()
    {
        if (statsText == null)
            statsText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        FindReferences();
        RefreshText();
    }

    private void Update()
    {
        if (statsText == null) return;

        statsText.enabled = isEnabled;
        if (!isEnabled) return;

        refreshTimer += Time.deltaTime;
        if (refreshTimer < refreshInterval) return;

        refreshTimer = 0f;

        if (playerProvider == null || progressionManager == null)
            FindReferences();

        RefreshText();
    }

    private void FindReferences()
    {
        if (playerProvider == null)
            playerProvider = FindAnyPlayerProvider();

        survivalStatsProvider = playerProvider as ISurvivalStatsProvider ?? FindAnySurvivalProvider();

        if (progressionManager == null)
            progressionManager = ProgressionManager.Instance ?? FindFirstObjectByType<ProgressionManager>();
    }

    private void RefreshText()
    {
        if (statsText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>PLAYER STATS (DEBUG)</b>");

        if (playerProvider != null)
        {
            sb.AppendLine($"Health: {playerProvider.Health:F0}/{playerProvider.MaxHealth:F0}");
            sb.AppendLine($"Armour: {playerProvider.Armour:F0}/{playerProvider.MaxArmour:F0}");
            sb.AppendLine($"Shield: {playerProvider.Shield:F0}/{playerProvider.MaxShield:F0}");
            sb.AppendLine($"Speed: {playerProvider.MoveSpeed:F1} m/s");
            sb.AppendLine($"Alive: {playerProvider.IsAlive}");
        }
        else
        {
            sb.AppendLine("Health: n/a (no IPlayerProvider found)");
        }

        if (progressionManager != null)
        {
            sb.AppendLine($"Level: {progressionManager.currentLevel}  XP: {progressionManager.currentXP} ({progressionManager.GetXPProgress() * 100f:F0}%)");
        }

        if (survivalStatsProvider != null)
        {
            sb.AppendLine($"Temp: {survivalStatsProvider.Temperature:F1}/{survivalStatsProvider.MaxTemperature:F1}");
            sb.AppendLine($"Stamina: {survivalStatsProvider.Stamina:F0}/{survivalStatsProvider.MaxStamina:F0}");
            sb.AppendLine($"Infection: {survivalStatsProvider.Infection:F0}/{survivalStatsProvider.MaxInfection:F0}");
            sb.AppendLine($"Hunger: {survivalStatsProvider.Hunger:F0}/{survivalStatsProvider.MaxHunger:F0}");
            sb.AppendLine($"Thirst: {survivalStatsProvider.Thirst:F0}/{survivalStatsProvider.MaxThirst:F0}");
        }

        statsText.text = sb.ToString();
    }

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }

    private static ISurvivalStatsProvider FindAnySurvivalProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is ISurvivalStatsProvider provider)
                return provider;
        }
        return null;
    }
}
