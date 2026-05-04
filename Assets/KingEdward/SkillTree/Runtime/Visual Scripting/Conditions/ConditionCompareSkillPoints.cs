using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Compare Skill Points")]
    [Description("Compares current skill points with a value")]
    
    [Category("KingEdward/Skill Tree/Compare Skill Points")]
    
    [Keywords("Skill", "Tree", "Points", "Compare", "Check", "Condition")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Purple)]
    
    [Serializable]
    public class ConditionCompareSkillPoints : Condition
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
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.GreaterOrEqual;
        [SerializeField] private PropertyGetInteger m_Value = new PropertyGetInteger(10);
        
        protected override string Summary => $"Skill Points {GetComparisonSymbol()} {m_Value}";
        
        protected override bool Run(Args args)
        {
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            int currentPoints = skillTreeComponent.CurrentSkillPoints;
            int compareValue = (int)m_Value.Get(args);
            
            switch (m_Comparison)
            {
                case ComparisonType.Equal:
                    return currentPoints == compareValue;
                case ComparisonType.NotEqual:
                    return currentPoints != compareValue;
                case ComparisonType.Greater:
                    return currentPoints > compareValue;
                case ComparisonType.GreaterOrEqual:
                    return currentPoints >= compareValue;
                case ComparisonType.Less:
                    return currentPoints < compareValue;
                case ComparisonType.LessOrEqual:
                    return currentPoints <= compareValue;
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
