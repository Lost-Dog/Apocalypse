using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Level Up Skill")]
    [Category("KingEdward/Skill Tree/Level Up Skill")]
    [Description("Levels up a specific skill")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Yellow)]
    
    [Keywords("Skill", "Tree", "Level", "Up", "Upgrade")]
    
    [Serializable]
    public class InstructionLevelUpSkill : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private bool m_IgnoreCost = false;
        
        public override string Title => $"Level Up {m_Skill?.name ?? "(none)"}";
        
        protected override Task Run(Args args)
        {
            if (m_Skill == null) return DefaultResult;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null || !skillTreeComponent.IsUnlocked(m_Skill)) return DefaultResult;
            
            if (m_IgnoreCost)
            {
                // Temporarily save current points
                int savedPoints = skillTreeComponent.CurrentSkillPoints;
                // Set enough points to level up
                skillTreeComponent.SetSkillPoints(savedPoints + m_Skill.Cost);
                // Level up
                skillTreeComponent.LevelUpSkill(m_Skill);
                // Restore points (the level up will deduct the cost)
                skillTreeComponent.SetSkillPoints(savedPoints);
            }
            else
            {
                skillTreeComponent.LevelUpSkill(m_Skill);
            }
            
            return DefaultResult;
        }
    }
}
