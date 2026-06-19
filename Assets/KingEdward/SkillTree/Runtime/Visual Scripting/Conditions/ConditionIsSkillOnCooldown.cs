using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Is Skill On Cooldown")]
    [Description("Checks if a skill is on cooldown")]
    
    [Category("KingEdward/Skill Tree/Is Skill On Cooldown")]
    
    [Keywords("Skill", "Tree", "Cooldown", "Check", "Condition")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Red)]
    
    [Serializable]
    public class ConditionIsSkillOnCooldown : Condition
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        
        protected override string Summary => $"Is {m_Skill?.name ?? "(none)"} On Cooldown";
        
        protected override bool Run(Args args)
        {
            if (m_Skill == null) return false;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            var skillInstance = skillTreeComponent.GetSkill(m_Skill);
            if (skillInstance == null) return false;
            
            return skillInstance.isOnCooldown;
        }
    }
}
