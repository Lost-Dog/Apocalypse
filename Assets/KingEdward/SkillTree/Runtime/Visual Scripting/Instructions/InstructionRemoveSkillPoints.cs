using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Remove Skill Points")]
    [Category("KingEdward/Skill Tree/Remove Skill Points")]
    [Description("Removes skill points from the player")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Red)]
    
    [Keywords("Skill", "Tree", "Points", "Remove", "Subtract")]
    
    [Serializable]
    public class InstructionRemoveSkillPoints : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetInteger m_Amount = new PropertyGetInteger(1);
        
        public override string Title => $"Remove {m_Amount} Skill Points";
        
        protected override Task Run(Args args)
        {
            int amount = (int)m_Amount.Get(args);
            
            if (amount <= 0)
            {
                Debug.LogWarning("[SkillTree] Cannot remove negative or zero Skill Points");
                return DefaultResult;
            }
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null)
            {
                Debug.LogError("[SkillTree] SkillTreeComponent not found!");
                return DefaultResult;
            }
            
            int currentPoints = skillTreeComponent.CurrentSkillPoints;
            int newPoints = Mathf.Max(0, currentPoints - amount);
            skillTreeComponent.SetSkillPoints(newPoints);
            return DefaultResult;
        }
    }
}
