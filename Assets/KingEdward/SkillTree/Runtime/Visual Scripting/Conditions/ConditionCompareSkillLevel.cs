using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Compare Skill Level")]
    [Description("Compares a skill's current level with a value")]
    
    [Category("KingEdward/Skill Tree/Compare Skill Level")]
    
    [Keywords("Skill", "Tree", "Level", "Compare", "Check", "Condition")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class ConditionCompareSkillLevel : Condition
    {
        public enum ComparisonType
        {
            Equal,
            NotEqual,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }
        
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.GreaterOrEqual;
        [SerializeField] private PropertyGetInteger m_Level = new PropertyGetInteger(1);
        
        protected override string Summary => $"{m_Skill?.name ?? "(none)"} Level {GetComparisonSymbol()} {m_Level}";
        
        protected override bool Run(Args args)
        {
            if (m_Skill == null) return false;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            SkillInstance skillInstance = skillTreeComponent.GetSkill(m_Skill);
            if (skillInstance == null) return false;
            
            int currentLevel = skillInstance.currentLevel;
            int compareLevel = (int)m_Level.Get(args);
            
            switch (m_Comparison)
            {
                case ComparisonType.Equal:
                    return currentLevel == compareLevel;
                case ComparisonType.NotEqual:
                    return currentLevel != compareLevel;
                case ComparisonType.Greater:
                    return currentLevel > compareLevel;
                case ComparisonType.GreaterOrEqual:
                    return currentLevel >= compareLevel;
                case ComparisonType.Less:
                    return currentLevel < compareLevel;
                case ComparisonType.LessOrEqual:
                    return currentLevel <= compareLevel;
                default:
                    return false;
            }
        }
        
        private string GetComparisonSymbol()
        {
            switch (m_Comparison)
            {
                case ComparisonType.Equal: return "==";
                case ComparisonType.NotEqual: return "!=";
                case ComparisonType.Greater: return ">";
                case ComparisonType.GreaterOrEqual: return ">=";
                case ComparisonType.Less: return "<";
                case ComparisonType.LessOrEqual: return "<=";
                default: return "?";
            }
        }
    }
}
