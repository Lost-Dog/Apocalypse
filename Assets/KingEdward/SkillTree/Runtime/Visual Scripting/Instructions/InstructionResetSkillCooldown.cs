using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Reset Skill Cooldown")]
    [Category("KingEdward/Skill Tree/Reset Skill Cooldown")]
    [Description("Resets the cooldown of a specific skill")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Keywords("Skill", "Tree", "Cooldown", "Reset")]
    
    [Serializable]
    public class InstructionResetSkillCooldown : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        
        public override string Title => $"Reset Cooldown {m_Skill?.name ?? "(none)"}";
        
        protected override Task Run(Args args)
        {
            if (m_Skill == null)
            {
                Debug.LogWarning("[SkillTree] Cannot reset cooldown for null skill");
                return DefaultResult;
            }
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null)
            {
                Debug.LogError("[SkillTree] SkillTreeComponent not found!");
                return DefaultResult;
            }
            
            var skillInstance = skillTreeComponent.GetSkill(m_Skill);
            if (skillInstance != null)
            {
                skillInstance.ResetCooldown();
            }
            else
            {
                Debug.LogWarning($"[SkillTree] Skill not found: {m_Skill.name}");
            }
            
            return DefaultResult;
        }
    }
}
