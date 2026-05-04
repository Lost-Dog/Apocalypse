using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Has Skill Points")]
    [Description("Checks if player has enough skill points")]
    
    [Category("KingEdward/Skill Tree/Has Skill Points")]
    
    [Keywords("Skill", "Tree", "Points", "Has", "Check", "Condition")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Purple)]
    
    [Serializable]
    public class ConditionHasSkillPoints : Condition
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetInteger m_RequiredPoints = new PropertyGetInteger(1);
        
        protected override string Summary => $"Has {m_RequiredPoints} Skill Points";
        
        protected override bool Run(Args args)
        {
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            int required = (int)m_RequiredPoints.Get(args);
            return skillTreeComponent.CurrentSkillPoints >= required;
        }
    }
}
