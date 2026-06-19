using UnityEngine;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Represents a prerequisite skill with optional level requirement
    /// </summary>
    [System.Serializable]
    public class SkillPrerequisite
    {
        [Tooltip("The skill that must be unlocked")]
        public Skill skill;
        
        [Tooltip("Minimum level required (0 = just unlocked, 1+ = specific level)")]
        public int requiredLevel = 0;
        
        /// <summary>
        /// Check if this prerequisite is met
        /// </summary>
        public bool IsMet(SkillTreeComponent skillTreeComponent)
        {
            if (skill == null || skillTreeComponent == null)
                return false;
            
            // Check if skill is unlocked
            if (!skillTreeComponent.IsUnlocked(skill))
                return false;
            
            // If no level requirement, just being unlocked is enough
            if (requiredLevel <= 0)
                return true;
            
            // Check level requirement
            SkillInstance skillInstance = skillTreeComponent.GetSkill(skill);
            if (skillInstance == null)
                return false;
            
            return skillInstance.currentLevel >= requiredLevel;
        }
        
        /// <summary>
        /// Get a description of this prerequisite
        /// </summary>
        public string GetDescription(SkillTreeComponent skillTreeComponent)
        {
            if (skill == null)
                return "Prerequisite skill not set";
            
            if (requiredLevel <= 0)
                return $"Requires: {skill.SkillName}";
            else
                return $"Requires: {skill.SkillName} (Level {requiredLevel})";
        }
        
        /// <summary>
        /// Get current status for UI display
        /// </summary>
        public string GetStatusText(SkillTreeComponent skillTreeComponent)
        {
            if (skill == null)
                return "❌ Skill not set";
            
            if (!skillTreeComponent.IsUnlocked(skill))
                return $"❌ {skill.SkillName} not unlocked";
            
            if (requiredLevel <= 0)
                return $"✅ {skill.SkillName} unlocked";
            
            SkillInstance skillInstance = skillTreeComponent.GetSkill(skill);
            if (skillInstance == null)
                return $"❌ {skill.SkillName} instance not found";
            
            if (skillInstance.currentLevel >= requiredLevel)
                return $"✅ {skill.SkillName} (Level {skillInstance.currentLevel}/{requiredLevel})";
            else
                return $"❌ {skill.SkillName} (Level {skillInstance.currentLevel}/{requiredLevel})";
        }
    }
}







