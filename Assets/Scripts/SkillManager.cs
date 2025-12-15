using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Database")]
    public List<SkillData> allSkills = new List<SkillData>();
    
    [Header("Learned Skills")]
    public List<SkillData> learnedSkills = new List<SkillData>();
    
    [Header("Skill Events")]
    public UnityEvent<SkillData> onSkillLearned;
    public UnityEvent<SkillData> onSkillActivated;
    
    private const string SKILL_RESOURCE_PATH = "Skills/Skill Data";
    
    public void Initialize()
    {
        LoadSkills();
    }
    
    private void LoadSkills()
    {
        SkillData[] loadedSkills = Resources.LoadAll<SkillData>(SKILL_RESOURCE_PATH);
        allSkills = new List<SkillData>(loadedSkills);
        
        Debug.Log($"Loaded {allSkills.Count} skills from Resources/{SKILL_RESOURCE_PATH}");
    }
    
    public bool LearnSkill(SkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("Cannot learn null skill!");
            return false;
        }
        
        if (learnedSkills.Contains(skill))
        {
            Debug.LogWarning($"Skill already learned: {skill.skillName}");
            return false;
        }
        
        if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
        {
            if (!GameManager.Instance.progressionManager.SpendSkillPoint())
            {
                Debug.LogWarning("Not enough skill points!");
                return false;
            }
        }
        
        learnedSkills.Add(skill);
        onSkillLearned?.Invoke(skill);
        
        Debug.Log($"Learned skill: {skill.skillName}");
        return true;
    }
    
    public bool UnlearnSkill(SkillData skill)
    {
        if (!learnedSkills.Contains(skill)) return false;
        
        learnedSkills.Remove(skill);
        
        if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
        {
            GameManager.Instance.progressionManager.RefundSkillPoint();
        }
        
        Debug.Log($"Unlearned skill: {skill.skillName}");
        return true;
    }
    
    public bool HasSkill(SkillData skill)
    {
        return learnedSkills.Contains(skill);
    }
    
    public bool HasSkill(string skillName)
    {
        return learnedSkills.Any(s => s.skillName == skillName);
    }
    
    public List<SkillData> GetAvailableSkills()
    {
        return allSkills.Where(s => !learnedSkills.Contains(s)).ToList();
    }
    
    public List<SkillData> GetSkillsBySpecialization(string specialization)
    {
        return allSkills.Where(s => s.specialization == specialization).ToList();
    }
    
    public float GetTotalStatBonus(string statName)
    {
        float totalBonus = 0f;
        
        foreach (var skill in learnedSkills)
        {
            totalBonus += skill.GetStatBonus(statName);
        }
        
        return totalBonus;
    }
    
    public void ResetAllSkills()
    {
        int refundedPoints = learnedSkills.Count;
        learnedSkills.Clear();
        
        if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
        {
            for (int i = 0; i < refundedPoints; i++)
            {
                GameManager.Instance.progressionManager.RefundSkillPoint();
            }
        }
        
        Debug.Log($"Reset all skills. Refunded {refundedPoints} skill points.");
    }
}
