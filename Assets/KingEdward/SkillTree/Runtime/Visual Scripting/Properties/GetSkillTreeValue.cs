using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace GameCreator.Runtime.Common
{
    [Title("Get Skill Tree Value")]
    [Description("Gets values from the Skill Tree system (unlocked skills count, etc.)")]
    
    [Category("Skill Tree/Get Skill Tree Value")]
    
    [Keywords("Skill", "Tree", "System", "Unlocked", "Count")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Purple)]
    
    [Serializable]
    public class GetSkillTreeValue : PropertyTypeGetDecimal
    {
        public enum SkillTreeValueType
        {
            UnlockedSkillsCount,
            TotalSkillsCount,
            UnlockedPercentage
        }
        
        [SerializeField] private SkillTreeValueType m_ValueType = SkillTreeValueType.UnlockedSkillsCount;
        
        public override double Get(Args args)
        {
            SkillTreeComponent skillTreeComponent = args.Self.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent == null)
            {
                skillTreeComponent = UnityEngine.Object.FindFirstObjectByType<SkillTreeComponent>();
            }
            
            if (skillTreeComponent == null) return 0;
            
            switch (m_ValueType)
            {
                case SkillTreeValueType.UnlockedSkillsCount:
                    return skillTreeComponent.GetUnlockedSkillCount();
                    
                case SkillTreeValueType.TotalSkillsCount:
                    return skillTreeComponent.skillTree?.allSkills?.Count ?? 0;
                    
                case SkillTreeValueType.UnlockedPercentage:
                    int total = skillTreeComponent.skillTree?.allSkills?.Count ?? 0;
                    if (total == 0) return 0;
                    return (double)skillTreeComponent.GetUnlockedSkillCount() / total * 100.0;
                    
                default:
                    return 0;
            }
        }
        
        public override string String => $"Skill Tree {m_ValueType}";
    }
}
