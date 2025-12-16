using UnityEngine;
using UnityEngine.Events;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }
    
    [Header("Player Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int skillPoints = 0;
    
    [Header("Gear Score")]
    public int currentGearScore = 0;
    public int equippedGearScore = 0;
    
    [Header("Level Settings")]
    public int maxLevel = 10;
    
    [Header("Progression Events")]
    public UnityEvent<int> onLevelUp;
    public UnityEvent<int> onXPGained;
    public UnityEvent<int> onSkillPointGained;
    public UnityEvent<int> onGearScoreChanged;
    
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
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void AddExperience(int amount)
    {
        if (currentLevel >= maxLevel) return;
        
        currentXP += amount;
        onXPGained?.Invoke(amount);
        
        Debug.Log($"Gained {amount} XP. Total: {currentXP}");
        
        CheckLevelUp();
    }
    
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
        {
            GameManager.Instance.UpdatePlayerLevel(currentLevel);
        }
        
        Debug.Log($"LEVEL UP! Now level {currentLevel}. Skill Points: {skillPoints}");
    }
    
    public bool SpendSkillPoint()
    {
        if (skillPoints <= 0) return false;
        
        skillPoints--;
        return true;
    }
    
    public void RefundSkillPoint()
    {
        skillPoints++;
    }
    
    public float GetXPProgress()
    {
        if (currentLevel >= maxLevel) return 1f;
        
        int currentRequired = GetRequiredXPForLevel(currentLevel - 1);
        int nextRequired = GetRequiredXPForLevel(currentLevel);
        
        return (float)(currentXP - currentRequired) / (nextRequired - currentRequired);
    }
    
    public int GetRequiredXPForLevel(int level)
    {
        if (level < 0 || level >= xpRequirements.Length)
        {
            return int.MaxValue;
        }
        
        return xpRequirements[level];
    }
    
    public int GetXPToNextLevel()
    {
        if (currentLevel >= maxLevel) return 0;
        
        return GetRequiredXPForLevel(currentLevel) - currentXP;
    }
    
    public bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }
    
    /// <summary>
    /// Update the player's current gear score (total inventory)
    /// </summary>
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
    
    /// <summary>
    /// Update the equipped gear score (only equipped items)
    /// </summary>
    public void UpdateEquippedGearScore(int newEquippedGearScore)
    {
        equippedGearScore = newEquippedGearScore;
    }
    
    /// <summary>
    /// Calculate combined power level (player level + gear score contribution)
    /// </summary>
    public int GetPowerLevel()
    {
        // Power level = player level + (equipped gear score / 100)
        // This gives roughly equal weight to leveling and gearing
        return currentLevel + (equippedGearScore / 100);
    }
    
    /// <summary>
    /// Get power level as a float for more precision
    /// </summary>
    public float GetPowerLevelFloat()
    {
        return currentLevel + (equippedGearScore / 100f);
    }
    
    /// <summary>
    /// Get recommended gear score for current level
    /// </summary>
    public int GetRecommendedGearScore()
    {
        // Recommended GS = 100 + (level * 40)
        // This matches the LootManager gear score formula
        return 100 + (currentLevel * 40);
    }
    
    /// <summary>
    /// Check if player is undergeared for their level
    /// </summary>
    public bool IsUndergeared()
    {
        int recommended = GetRecommendedGearScore();
        return equippedGearScore < (recommended * 0.75f); // Less than 75% of recommended
    }
    
    /// <summary>
    /// Check if player is overgeared for their level
    /// </summary>
    public bool IsOvergeared()
    {
        int recommended = GetRecommendedGearScore();
        return equippedGearScore > (recommended * 1.25f); // More than 125% of recommended
    }
    
    /// <summary>
    /// Get gear quality rating (0-100%)
    /// </summary>
    public float GetGearQuality()
    {
        int recommended = GetRecommendedGearScore();
        if (recommended == 0) return 1f;
        
        float quality = (float)equippedGearScore / recommended;
        return Mathf.Clamp01(quality);
    }
}
