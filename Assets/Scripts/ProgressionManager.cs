using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player progression (level, XP, skill points) and applies stat
/// scaling to the Invector player via <see cref="PlayerTraitsRuntime"/>.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Traits Runtime")]
    [Tooltip("Assign the PlayerTraitsRuntime component. Auto-found if left empty.")]
    [SerializeField] private PlayerTraitsRuntime playerTraitsRuntime;

    [Header("Player Progression")]
    public int currentLevel = 1;
    public int currentXP    = 0;
    public int skillPoints  = 0;

    [Header("Gear Score")]
    public int currentGearScore  = 0;
    public int equippedGearScore = 0;

    [Header("Level Settings")]
    public int maxLevel = 1000;

    [Header("XP Formula")]
    [Tooltip("Cumulative XP to reach level N = floor(xpBase × N ^ xpExponent). " +
             "Raise xpExponent for a steeper curve (harder to level at higher tiers).")]
    public float xpBase     = 100f;
    public float xpExponent = 1.8f;

    [Header("Progression Events")]
    public UnityEvent<int> onLevelUp;
    public UnityEvent<int> onXPGained;
    public UnityEvent<int> onSkillPointGained;
    public UnityEvent<int> onGearScoreChanged;

    // ── Private state ─────────────────────────────────────────────────────────

    private const int SkillPointsPerLevel = 1;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Resolve early so ApplyLevel in Start is guaranteed to find the runtime
        // even if PlayerTraitsRuntime.Awake() runs after this Awake().
        ResolveTraitsRuntime();
    }

    private void Start()
    {
        ResolveTraitsRuntime();
        playerTraitsRuntime?.ApplyLevel(currentLevel);
    }

    // ── Public API — XP & level ───────────────────────────────────────────────

    /// <summary>Adds experience points and triggers a level-up check.</summary>
    public void AddExperience(int amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;
        onXPGained?.Invoke(amount);

        Debug.Log($"Gained {amount} XP. Total: {currentXP}");

        CheckLevelUp();
    }

    // ── Public API — skill points ─────────────────────────────────────────────

    /// <summary>Spends one skill point. Returns false if none are available.</summary>
    public bool SpendSkillPoint()
    {
        if (skillPoints <= 0) return false;
        skillPoints--;
        return true;
    }

    /// <summary>Refunds one skill point (e.g. when unlearning a skill).</summary>
    public void RefundSkillPoint()
    {
        skillPoints++;
    }

    // ── Public API — XP progress helpers ─────────────────────────────────────

    /// <summary>Returns [0, 1] progress toward the next level.</summary>
    public float GetXPProgress()
    {
        if (currentLevel >= maxLevel) return 1f;

        int currentRequired = GetRequiredXPForLevel(currentLevel - 1);
        int nextRequired    = GetRequiredXPForLevel(currentLevel);

        return (float)(currentXP - currentRequired) / (nextRequired - currentRequired);
    }

    /// <summary>
    /// Returns the cumulative XP threshold that marks the start of <paramref name="level"/>.
    /// Formula: <c>floor(xpBase × level ^ xpExponent)</c>.
    /// level 0 always returns 0 (start of level 1).
    /// </summary>
    public int GetRequiredXPForLevel(int level)
    {
        if (level <= 0)   return 0;
        if (level >= maxLevel) return int.MaxValue;

        return Mathf.FloorToInt(xpBase * Mathf.Pow(level, xpExponent));
    }

    /// <summary>Returns XP remaining until the next level.</summary>
    public int GetXPToNextLevel()
    {
        if (currentLevel >= maxLevel) return 0;
        return GetRequiredXPForLevel(currentLevel) - currentXP;
    }

    public bool IsMaxLevel() => currentLevel >= maxLevel;

    // ── Public API — gear score ───────────────────────────────────────────────

    /// <summary>Updates the total inventory gear score and fires the change event.</summary>
    public void UpdateGearScore(int newGearScore)
    {
        int oldGearScore = currentGearScore;
        currentGearScore = newGearScore;

        if (oldGearScore != currentGearScore)
        {
            onGearScoreChanged?.Invoke(currentGearScore);
            Debug.Log($"Gear Score updated: {currentGearScore}");
        }
    }

    /// <summary>Updates the equipped-only gear score.</summary>
    public void UpdateEquippedGearScore(int newEquippedGearScore)
    {
        equippedGearScore = newEquippedGearScore;
    }

    /// <summary>Returns player level + equipped gear score contribution.</summary>
    public int   GetPowerLevel()      => currentLevel + (equippedGearScore / 100);
    public float GetPowerLevelFloat() => currentLevel + (equippedGearScore / 100f);

    /// <summary>Returns recommended gear score for the current level.</summary>
    public int GetRecommendedGearScore() => 100 + (currentLevel * 40);

    public bool IsUndergeared()
    {
        int recommended = GetRecommendedGearScore();
        return equippedGearScore < (recommended * 0.75f);
    }

    public bool IsOvergeared()
    {
        int recommended = GetRecommendedGearScore();
        return equippedGearScore > (recommended * 1.25f);
    }

    public float GetGearQuality()
    {
        int recommended = GetRecommendedGearScore();
        if (recommended == 0) return 1f;
        return Mathf.Clamp01((float)equippedGearScore / recommended);
    }

    // ── Private — level-up logic ──────────────────────────────────────────────

    private void CheckLevelUp()
    {
        if (currentLevel >= maxLevel) return;

        int requiredXP = GetRequiredXPForLevel(currentLevel);

        while (currentXP >= requiredXP && currentLevel < maxLevel)
        {
            LevelUp();
            requiredXP = GetRequiredXPForLevel(currentLevel);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        skillPoints += SkillPointsPerLevel;

        onLevelUp?.Invoke(currentLevel);
        onSkillPointGained?.Invoke(SkillPointsPerLevel);

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerLevel(currentLevel);

        playerTraitsRuntime?.ApplyLevel(currentLevel);

        Debug.Log($"LEVEL UP! Now level {currentLevel}. Skill Points: {skillPoints}");
    }

    // ── Private — runtime resolution ──────────────────────────────────────────

    private void ResolveTraitsRuntime()
    {
        if (playerTraitsRuntime != null) return;

        playerTraitsRuntime = PlayerTraitsRuntime.Instance;

        if (playerTraitsRuntime == null)
            playerTraitsRuntime = FindFirstObjectByType<PlayerTraitsRuntime>();

        if (playerTraitsRuntime == null)
            Debug.LogWarning("[ProgressionManager] Could not find PlayerTraitsRuntime — stat scaling disabled.");
        else
            Debug.Log($"[ProgressionManager] Resolved PlayerTraitsRuntime on '{playerTraitsRuntime.gameObject.name}'.");
    }
}
