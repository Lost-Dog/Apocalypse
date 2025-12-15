using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Division Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    [TextArea(3, 6)] public string description;
    public Sprite icon;
    
    [Header("Specialization")]
    public string specialization;
    
    [Header("Stat Bonuses")]
    public List<StatBonus> statBonuses = new List<StatBonus>();
    
    public float GetStatBonus(string statName)
    {
        foreach (var bonus in statBonuses)
        {
            if (bonus.statName == statName)
            {
                return bonus.bonusValue;
            }
        }
        
        return 0f;
    }
    
    public bool HasStatBonus(string statName)
    {
        return statBonuses.Exists(b => b.statName == statName);
    }
}

[System.Serializable]
public class StatBonus
{
    public string statName;
    public float bonusValue;
    public bool isPercentage;
}
