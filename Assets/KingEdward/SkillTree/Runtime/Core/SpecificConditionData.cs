using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Represents a specific condition for a skill level
    /// </summary>
    [System.Serializable]
    public class SpecificConditionData
    {
        [Header("Level Configuration")]
        [Tooltip("The level this condition aims to (e.g., 2 for level 1->2, 3 for level 2->3)")]
        public int targetLevel = 1;
        
        [Header("Condition Settings")]
        [Tooltip("If true, uses both specific conditions AND general level up conditions. If false, uses only specific conditions.")]
        public bool useGeneralConditionsToo = true;
        
        [Header("Specific Conditions")]
        [Tooltip("Conditions that must be met to level up to this specific level")]
        public RunConditionsList conditions = new RunConditionsList();
        
        [Header("On Level Up")]
        [Tooltip("Instructions that run when leveling up to this specific level")]
        public RunInstructionsList onLevelUp = new RunInstructionsList();
        
        /// <summary>
        /// Check if this condition applies to the given level
        /// </summary>
        public bool AppliesToLevel(int level)
        {
            return targetLevel == level;
        }
        
        /// <summary>
        /// Check if the conditions are met for this level
        /// </summary>
        public bool CheckConditions(Args args)
        {
            return conditions.Check(args);
        }
        
        /// <summary>
        /// Get a display name for this condition
        /// </summary>
        public string GetDisplayName()
        {
            return $"Level {targetLevel} Conditions";
        }
    }
}
