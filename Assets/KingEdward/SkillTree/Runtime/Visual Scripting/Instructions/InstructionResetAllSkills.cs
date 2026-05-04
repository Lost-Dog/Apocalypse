using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Reset All Skills")]
    [Category("KingEdward/Skill Tree/Reset All Skills")]
    [Description("Resets all skills (unlocks and levels)")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Red)]
    
    [Keywords("Skill", "Tree", "Reset", "Clear", "All")]
    
    [Serializable]
    public class InstructionResetAllSkills : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private bool m_RefundPoints = true;
        
        public override string Title => $"Reset All Skills {(m_RefundPoints ? "(Refund)" : "")}";
        
        protected override Task Run(Args args)
        {
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return DefaultResult;
            
            int refundedPoints = 0;
            
            if (m_RefundPoints)
            {
                // Calculate total points spent using tracked values
                foreach (var skillInstance in skillTreeComponent.GetAllSkillInstances())
                {
                    refundedPoints += skillInstance.totalPointsSpent;
                }
            }
            
            // Clear all skills
            skillTreeComponent.ClearUnlockedSkills();
            
            // Refund points
            if (m_RefundPoints && refundedPoints > 0)
            {
                skillTreeComponent.AddSkillPoints(refundedPoints);
            }
            
            // Trigger UI refresh
            skillTreeComponent.TriggerRefreshAllSkills();
            
            return DefaultResult;
        }
    }
}
