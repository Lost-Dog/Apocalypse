using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Assign Skill to Hotbar Slot")]
    [Description("Assigns a specific skill to a hotbar slot by index")]
    
    [Category("KingEdward/Skill Tree/Hotbar/Assign Skill to Slot")]
    
    [Parameter("Hotbar UI", "The SkillHotbarUI component")]
    [Parameter("Skill", "The skill to assign")]
    [Parameter("Slot Index", "The slot index (0-based)")]
    
    [Image(typeof(IconSkillHotbar), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class InstructionAssignSkillToHotbarSlot : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_HotbarUI = GetGameObjectInstance.Create();
        [SerializeField] private Skill m_Skill;
        [SerializeField] private PropertyGetInteger m_SlotIndex = new PropertyGetInteger(0);
        
        public override string Title => $"Assign {(m_Skill != null ? m_Skill.name : "Skill")} to Slot {m_SlotIndex}";
        
        protected override Task Run(Args args)
        {
            if (m_Skill == null)
            {
                Debug.LogWarning("[SkillTree] Skill is null");
                return DefaultResult;
            }
            
            GameObject hotbarObject = m_HotbarUI.Get(args);
            if (hotbarObject == null)
            {
                Debug.LogWarning("[SkillTree] Hotbar GameObject is null");
                return DefaultResult;
            }
            
            SkillHotbarUI hotbar = hotbarObject.GetComponent<SkillHotbarUI>();
            if (hotbar == null)
            {
                Debug.LogWarning("[SkillTree] No SkillHotbarUI component found");
                return DefaultResult;
            }
            
            int slotIndex = (int)m_SlotIndex.Get(args);
            
            // AssignSkillToSlot already validates the slot index internally
            
            hotbar.AssignSkillToSlot(m_Skill, slotIndex);
            
            return DefaultResult;
        }
    }
}
