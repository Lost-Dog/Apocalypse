using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Is Skill Unlocked")]
    [Description("Checks if a skill is unlocked")]
    
    [Category("KingEdward/Skill Tree/Is Skill Unlocked")]
    
    [Keywords("Skill", "Tree", "Unlock", "Check", "Condition")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionIsSkillUnlocked : Condition
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        
        protected override string Summary => $"Is {m_Skill?.name ?? "(none)"} Unlocked";
        
        protected override bool Run(Args args)
        {
            if (m_Skill == null) return false;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            return skillTreeComponent.IsUnlocked(m_Skill);
        }
    }
}
