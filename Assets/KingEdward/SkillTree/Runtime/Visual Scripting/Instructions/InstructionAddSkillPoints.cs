using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;
using System;
using System.Threading.Tasks;

namespace KingEdward.SkillTree.Instructions
{
    /// <summary>
    /// Instruction to add Skill Points to the player
    /// </summary>
    [Title("Add Skill Points")]
    [Category("KingEdward/Skill Tree/Add Skill Points")]
    [Description("Adds Skill Points to the player's Skill Tree")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Keywords("Skill", "Points", "Add", "Currency")]
    
    [Serializable]
    public class InstructionAddSkillPoints : Instruction
    {
        [SerializeField] private PropertyGetInteger m_Amount = new PropertyGetInteger(1);
        
        public override string Title => $"Add {m_Amount} Skill Points";
        
        protected override Task Run(Args args)
        {
            int amount = (int)m_Amount.Get(args);
            
            if (amount <= 0)
            {
                Debug.LogWarning("[SkillTree] Cannot add negative or zero Skill Points");
                return DefaultResult;
            }
            
            // Find SkillTreeComponent
            SkillTreeComponent skillTreeComponent = args.Self.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent == null)
            {
                skillTreeComponent = UnityEngine.Object.FindFirstObjectByType<SkillTreeComponent>();
            }
            if (skillTreeComponent == null)
            {
                Debug.LogError("[SkillTree] SkillTreeComponent not found!");
                return DefaultResult;
            }
            
            // Add Skill Points
            bool success = skillTreeComponent.AddSkillPoints(amount);
            if (success)
            {
                // Skill points added
            }
            else
            {
                Debug.LogWarning($"[SkillTree] Failed to add {amount} Skill Points (already at max)");
            }
            
            return DefaultResult;
        }
    }
}
