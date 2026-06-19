using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Use Skill")]
    [Category("KingEdward/Skill Tree/Use Skill")]
    [Description("Uses/activates a specific skill")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Yellow)]
    
    [Keywords("Skill", "Tree", "Use", "Cast", "Activate")]
    
    [Serializable]
    public class InstructionUseSkill : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        
        public override string Title => $"Use {m_Skill?.name ?? "(none)"}";
        
        protected override Task Run(Args args)
        {
            if (m_Skill == null) return DefaultResult;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return DefaultResult;
            
            skillTreeComponent.UseSkill(m_Skill);
            
            return DefaultResult;
        }
    }
}
