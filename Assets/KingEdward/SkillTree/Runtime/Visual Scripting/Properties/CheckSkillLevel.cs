using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace GameCreator.Runtime.Common
{
    [Title("Check Skill Level")]
    [Description("Checks if a skill meets a specific level requirement")]
    
    [Category("Skill Tree/Check Skill Level")]
    
    [Keywords("Skill", "Tree", "Level", "Check", "Condition")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class CheckSkillLevel : PropertyTypeGetBool
    {
        public enum ComparisonType
        {
            Equal,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual
        }
        
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.GreaterThanOrEqual;
        [SerializeField] private PropertyGetInteger m_RequiredLevel = new PropertyGetInteger(1);
        
        public override bool Get(Args args)
        {
            if (m_Skill == null) return false;
            
            // Find SkillTreeComponent in the target
            SkillTreeComponent skillTreeComponent = args.Self.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent == null)
            {
                // Try to find it in the target game object
                GameObject target = args.Target;
                if (target != null)
                {
                    skillTreeComponent = target.GetComponent<SkillTreeComponent>();
                }
            }
            
            if (skillTreeComponent == null)
            {
                Debug.LogError($"[SkillTree] CheckSkillLevel: Could not find SkillTreeComponent for {m_Skill.name}");
                return false;
            }
            
            // Get the SkillInstance
            var skillInstance = skillTreeComponent.GetSkill(m_Skill);
            if (skillInstance == null)
            {
                // If no skill instance exists, the skill is not unlocked (level 0)
                int currentLevel = 0;
                int requiredLevel = (int)m_RequiredLevel.Get(args);
                
                return m_Comparison switch
                {
                    ComparisonType.Equal => currentLevel == requiredLevel,
                    ComparisonType.GreaterThan => currentLevel > requiredLevel,
                    ComparisonType.GreaterThanOrEqual => currentLevel >= requiredLevel,
                    ComparisonType.LessThan => currentLevel < requiredLevel,
                    ComparisonType.LessThanOrEqual => currentLevel <= requiredLevel,
                    _ => false
                };
            }
            
            // Check if skill is unlocked
            if (!skillTreeComponent.IsUnlocked(m_Skill))
            {
                // If skill is not unlocked, it's at level 0
                int currentLevel = 0;
                int requiredLevel = (int)m_RequiredLevel.Get(args);
                
                return m_Comparison switch
                {
                    ComparisonType.Equal => currentLevel == requiredLevel,
                    ComparisonType.GreaterThan => currentLevel > requiredLevel,
                    ComparisonType.GreaterThanOrEqual => currentLevel >= requiredLevel,
                    ComparisonType.LessThan => currentLevel < requiredLevel,
                    ComparisonType.LessThanOrEqual => currentLevel <= requiredLevel,
                    _ => false
                };
            }
            
            int currentLevelUnlocked = skillInstance.currentLevel;
            int requiredLevelUnlocked = (int)m_RequiredLevel.Get(args);
            
            return m_Comparison switch
            {
                ComparisonType.Equal => currentLevelUnlocked == requiredLevelUnlocked,
                ComparisonType.GreaterThan => currentLevelUnlocked > requiredLevelUnlocked,
                ComparisonType.GreaterThanOrEqual => currentLevelUnlocked >= requiredLevelUnlocked,
                ComparisonType.LessThan => currentLevelUnlocked < requiredLevelUnlocked,
                ComparisonType.LessThanOrEqual => currentLevelUnlocked <= requiredLevelUnlocked,
                _ => false
            };
        }
        
        public override string String => $"Skill {m_Skill?.name ?? "(none)"} Level {m_Comparison} {m_RequiredLevel}";
    }
}
