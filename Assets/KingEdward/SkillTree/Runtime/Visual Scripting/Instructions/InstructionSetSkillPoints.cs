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
    /// Instruction to set Skill Points to a specific value
    /// </summary>
    [Title("Set Skill Points")]
    [Category("KingEdward/Skill Tree/Set Skill Points")]
    [Description("Sets Skill Points to a specific value")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Green)]
    
    [Keywords("Skill", "Points", "Set", "Currency")]
    
    [Serializable]
    public class InstructionSetSkillPoints : Instruction
    {
        [SerializeField] private PropertyGetInteger m_Amount = new PropertyGetInteger(10);
        
        public override string Title => $"Set Skill Points to {m_Amount}";
        
        protected override Task Run(Args args)
        {
            int amount = (int)m_Amount.Get(args);
            
            if (amount < 0) return DefaultResult;
            
            // Find SkillTreeComponent
            SkillTreeComponent skillTreeComponent = args.Self.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent == null)
            {
                skillTreeComponent = UnityEngine.Object.FindFirstObjectByType<SkillTreeComponent>();
            }
            if (skillTreeComponent == null) return DefaultResult;
            
            skillTreeComponent.SetSkillPoints(amount);
            
            return DefaultResult;
        }
    }
}
