using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace GameCreator.Runtime.Common
{
    [Title("Get Skill Value")]
    [Description("Gets current values from a Skill (damage, cooldown, level, etc.)")]
    
    [Category("Skill Tree/Get Skill Value")]
    
    [Keywords("Skill", "Tree", "Value", "Damage", "Cooldown", "Level")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class GetSkillValue : PropertyTypeGetDecimal
    {
        public enum SkillValueType
        {
            CurrentLevel,
            MaxLevel,
            CooldownDuration,
            CanLevelUp,
            IsMaxLevel
        }
        
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private SkillValueType m_ValueType = SkillValueType.CurrentLevel;
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        
        public override double Get(Args args)
        {
            if (m_Skill == null) return 0;
            
            // Obtém o SkillTreeComponent
            var skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return 0;
            
            // Find existing skill instance without creating new one
            SkillInstance skillInstance = skillTreeComponent.GetSkill(m_Skill);
            
            if (skillInstance == null) return 0;
            
            return m_ValueType switch
            {
                SkillValueType.CurrentLevel => skillInstance.currentLevel,
                SkillValueType.MaxLevel => skillInstance.skillReference.maxLevel,
                SkillValueType.CooldownDuration => skillInstance.CooldownDuration,
                SkillValueType.CanLevelUp => skillInstance.CanLevelUp ? 1 : 0,
                SkillValueType.IsMaxLevel => skillInstance.IsMaxLevel ? 1 : 0,
                _ => 0
            };
        }
        
        public override string String => $"Skill {m_ValueType}: {m_Skill?.name ?? "(none)"}";
    }
}

