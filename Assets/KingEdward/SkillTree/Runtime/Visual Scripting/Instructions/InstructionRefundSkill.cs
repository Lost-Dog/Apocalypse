using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Refund Skill")]
    [Category("KingEdward/Skill Tree/Refund Skill")]
    [Description("Refunds a skill, returning all spent skill points and resetting it to locked state")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Red)]
    
    [Keywords("Skill", "Tree", "Refund", "Reset", "Respec", "Points")]
    
    [Serializable]
    public class InstructionRefundSkill : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill;
        [SerializeField] private bool m_RefundAllLevels = true;
        
        public override string Title => $"Refund {this.m_Skill}";
        
        protected override Task Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[SkillTree] Refund Skill: Target is null");
                return DefaultResult;
            }
            
            SkillTreeComponent skillTree = target.GetComponent<SkillTreeComponent>();
            if (skillTree == null)
            {
                Debug.LogWarning("[SkillTree] Refund Skill: No SkillTreeComponent found");
                return DefaultResult;
            }
            
            if (m_Skill == null)
            {
                Debug.LogWarning("[SkillTree] Refund Skill: Skill is null");
                return DefaultResult;
            }
            
            skillTree.RefundSkill(m_Skill, m_RefundAllLevels);
            
            return DefaultResult;
        }
    }
}
