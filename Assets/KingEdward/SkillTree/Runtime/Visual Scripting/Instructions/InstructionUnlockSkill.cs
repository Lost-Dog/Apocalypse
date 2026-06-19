using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Unlock Skill")]
    [Category("KingEdward/Skill Tree/Unlock Skill")]
    [Description("Unlocks a specific skill")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Keywords("Skill", "Tree", "Unlock")]
    
    [Serializable]
    public class InstructionUnlockSkill : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private bool m_IgnoreCost = false;
        
        public override string Title => $"Unlock {m_Skill?.name ?? "(none)"}";
        
        protected override Task Run(Args args)
        {
            if (m_Skill == null) return DefaultResult;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return DefaultResult;
            
            if (m_IgnoreCost)
            {
                // Temporarily save current points
                int savedPoints = skillTreeComponent.CurrentSkillPoints;
                // Set enough points to unlock
                skillTreeComponent.SetSkillPoints(savedPoints + m_Skill.Cost);
                // Unlock
                skillTreeComponent.UnlockSkill(m_Skill);
                // Restore points (the unlock will deduct the cost)
                skillTreeComponent.SetSkillPoints(savedPoints);
            }
            else
            {
                skillTreeComponent.UnlockSkill(m_Skill);
            }
            
            return DefaultResult;
        }
    }
}
