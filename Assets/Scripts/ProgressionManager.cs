using UnityEngine;
using UnityEngine.Events;

public class ProgressionManager : MonoBehaviour
{
    [Header("Player Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int skillPoints = 0;
    
    [Header("Level Settings")]
    public int maxLevel = 10;
    
    [Header("Progression Events")]
    public UnityEvent<int> onLevelUp;
    public UnityEvent<int> onXPGained;
    public UnityEvent<int> onSkillPointGained;
    
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
}
