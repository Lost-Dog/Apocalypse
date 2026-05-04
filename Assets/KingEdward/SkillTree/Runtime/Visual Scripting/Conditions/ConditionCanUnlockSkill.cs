using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Can Unlock Skill")]
    [Description("Checks if a skill can be unlocked (has points and prerequisites)")]
    
    [Category("KingEdward/Skill Tree/Can Unlock Skill")]
    
    [Keywords("Skill", "Tree", "Unlock", "Can", "Check", "Condition")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class ConditionCanUnlockSkill : Condition
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        
        protected override string Summary => $"Can Unlock {m_Skill?.name ?? "(none)"}";
        
        protected override bool Run(Args args)
        {
            if (m_Skill == null) return false;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            return skillTreeComponent.CanUnlock(m_Skill);
        }
    }
}
