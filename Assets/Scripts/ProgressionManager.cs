using UnityEngine;
using UnityEngine.Events;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Stats;

/// <summary>
/// Manages player progression (level, XP, skill points) and mirrors values
/// into the GC2 Traits system on the player character.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // GC2 Stat ID constants — must match the IDs defined in the Stat assets.
    // -------------------------------------------------------------------------
    private const string StatLevel      = "level";
    private const string StatExperience = "experience";
    private const string StatMaxHealth  = "max-health";
    private const string StatMaxMana    = "max-mana";
    private const string StatMaxStamina = "max-stamina";
    private const string StatAttack     = "attack";
    private const string StatDefense    = "defense";

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("Player Reference")]
    [Tooltip("Assign at design time or leave null to auto-find by 'Player' tag at runtime.")]
    public Traits playerTraits;

    [Header("Player Progression")]
    public int currentLevel  = 1;
    public int currentXP     = 0;
    public int skillPoints   = 0;

    [Header("Gear Score")]
    public int currentGearScore  = 0;
    public int equippedGearScore = 0;

    [Header("Level Settings")]
    public int maxLevel = 10;

    [Header("Stat Bonuses Per Level")]
    [Tooltip("Flat bonus added to max-health for each level above 1.")]
    public float healthBonusPerLevel  = 20f;
    [Tooltip("Flat bonus added to max-mana for each level above 1.")]
    public float manaBonusPerLevel    = 10f;
    [Tooltip("Flat bonus added to max-stamina for each level above 1.")]
    public float staminaBonusPerLevel = 5f;
    [Tooltip("Flat bonus added to attack for each level above 1.")]
    public float attackBonusPerLevel  = 5f;
    [Tooltip("Flat bonus added to defense for each level above 1.")]
    public float defenseBonusPerLevel = 3f;

    [Header("Progression Events")]
    public UnityEvent<int> onLevelUp;
    public UnityEvent<int> onXPGained;
    public UnityEvent<int> onSkillPointGained;
    public UnityEvent<int> onGearScoreChanged;

    // -------------------------------------------------------------------------
    // Private state — tracks modifiers currently applied so old ones can be
    // removed before fresh ones are applied on level-up.
    // -------------------------------------------------------------------------
    private float m_AppliedHealthBonus  = 0f;
    private float m_AppliedManaBonus    = 0f;
    private float m_AppliedStaminaBonus = 0f;
    private float m_AppliedAttackBonus  = 0f;
    private float m_AppliedDefenseBonus = 0f;

    private const int SKILL_POINTS_PER_LEVEL = 1;

    private readonly int[] xpRequirements =
    {
        0,      // Level 1 (start)
        100,    // Level 2
        300,    // Level 3
        600,    // Level 4
        1000,   // Level 5
        1500,   // Level 6
        2100,   // Level 7
        2800,   // Level 8
        3600,   // Level 9
        4500    // Level 10
    };

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ResolvePlayerTraits();
        SyncAllStatsToGC2();
    }

    // =========================================================================
    // Public API — XP & level
    // =========================================================================

    /// <summary>Adds experience points and triggers a level-up check.</summary>
    public void AddExperience(int amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;
        onXPGained?.Invoke(amount);

        Debug.Log($"Gained {amount} XP. Total: {currentXP}");

        SyncExperienceToGC2();
        CheckLevelUp();
    }

    // =========================================================================
    // Public API — skill points
    // =========================================================================

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

    // =========================================================================
    // Public API — XP progress helpers
    // =========================================================================

    /// <summary>Returns [0,1] progress toward the next level.</summary>
    public float GetXPProgress()
    {
        if (currentLevel >= maxLevel) return 1f;

        int currentRequired = GetRequiredXPForLevel(currentLevel - 1);
        int nextRequired    = GetRequiredXPForLevel(currentLevel);

        return (float)(currentXP - currentRequired) / (nextRequired - currentRequired);
    }

    /// <summary>Returns XP required to reach the given level.</summary>
    public int GetRequiredXPForLevel(int level)
    {
        if (level < 0 || level >= xpRequirements.Length)
            return int.MaxValue;

        return xpRequirements[level];
    }

    /// <summary>Returns XP remaining until the next level.</summary>
    public int GetXPToNextLevel()
    {
        if (currentLevel >= maxLevel) return 0;
        return GetRequiredXPForLevel(currentLevel) - currentXP;
    }

    public bool IsMaxLevel() => currentLevel >= maxLevel;

    // =========================================================================
    // Public API — gear score
    // =========================================================================

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
    public int GetPowerLevel() => currentLevel + (equippedGearScore / 100);

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

    // =========================================================================
    // Private — level-up logic
    // =========================================================================

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
        skillPoints += SKILL_POINTS_PER_LEVEL;

        onLevelUp?.Invoke(currentLevel);
        onSkillPointGained?.Invoke(SKILL_POINTS_PER_LEVEL);

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerLevel(currentLevel);

        Debug.Log($"LEVEL UP! Now level {currentLevel}. Skill Points: {skillPoints}");

        SyncAllStatsToGC2();
    }

    // =========================================================================
    // Private — GC2 Traits sync
    // =========================================================================

    /// <summary>
    /// Attempts to find the player Traits component if not already assigned.
    /// </summary>
    private void ResolvePlayerTraits()
    {
        if (playerTraits != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTraits = player.GetComponent<Traits>();

        if (playerTraits == null)
        {
            Character character = FindFirstObjectByType<Character>();
            if (character != null)
                playerTraits = character.GetComponent<Traits>();
        }

        if (playerTraits == null)
            Debug.LogWarning("ProgressionManager: Could not find player Traits. GC2 stat sync disabled.");
        else
            Debug.Log($"ProgressionManager: Syncing to Traits on '{playerTraits.gameObject.name}'.");
    }

    /// <summary>
    /// Writes level, XP, and all stat bonuses into GC2 Traits.
    /// Safe to call multiple times — removes old modifiers before re-applying.
    /// </summary>
    private void SyncAllStatsToGC2()
    {
        if (playerTraits == null) return;

        SyncLevelToGC2();
        SyncExperienceToGC2();
        SyncStatBonusesToGC2();
    }

    /// <summary>Sets the GC2 'lvl' stat base value to match currentLevel.</summary>
    private void SyncLevelToGC2()
    {
        if (playerTraits == null) return;

        try
        {
            playerTraits.RuntimeStats.Get(StatLevel).Base = currentLevel;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ProgressionManager: Could not sync '{StatLevel}' stat — {e.Message}");
        }
    }

    /// <summary>Sets the GC2 'experience' stat base value to match currentXP.</summary>
    private void SyncExperienceToGC2()
    {
        if (playerTraits == null) return;

        try
        {
            playerTraits.RuntimeStats.Get(StatExperience).Base = currentXP;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ProgressionManager: Could not sync '{StatExperience}' stat — {e.Message}");
        }
    }

    /// <summary>
    /// Removes the previously-applied per-level constant modifiers and adds
    /// fresh ones based on (currentLevel - 1) bonus steps.
    /// </summary>
    private void SyncStatBonusesToGC2()
    {
        if (playerTraits == null) return;

        int bonusLevels = currentLevel - 1; // level 1 = 0 bonus steps

        ApplyStatModifier(StatMaxHealth,  ref m_AppliedHealthBonus,  bonusLevels * healthBonusPerLevel);
        ApplyStatModifier(StatMaxMana,    ref m_AppliedManaBonus,    bonusLevels * manaBonusPerLevel);
        ApplyStatModifier(StatMaxStamina, ref m_AppliedStaminaBonus, bonusLevels * staminaBonusPerLevel);
        ApplyStatModifier(StatAttack,     ref m_AppliedAttackBonus,  bonusLevels * attackBonusPerLevel);
        ApplyStatModifier(StatDefense,    ref m_AppliedDefenseBonus, bonusLevels * defenseBonusPerLevel);
    }

    /// <summary>
    /// Removes the previously tracked constant modifier value and applies the
    /// new one, then stores the new value so it can be removed next time.
    /// </summary>
    private void ApplyStatModifier(string statId, ref float trackedValue, float newValue)
    {
        try
        {
            RuntimeStatData stat = playerTraits.RuntimeStats.Get(statId);

            if (Mathf.Abs(trackedValue) > float.Epsilon)
                stat.RemoveModifier(ModifierType.Constant, trackedValue);

            if (Mathf.Abs(newValue) > float.Epsilon)
                stat.AddModifier(ModifierType.Constant, newValue);

            trackedValue = newValue;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ProgressionManager: Could not apply modifier to '{statId}' — {e.Message}");
        }
    }
}
