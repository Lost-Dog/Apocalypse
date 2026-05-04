using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Is Skill In Hotbar")]
    [Description("Checks if a skill is assigned to any hotbar slot")]
    
    [Category("KingEdward/Skill Tree/Is Skill In Hotbar")]
    
    [Keywords("Skill", "Tree", "Hotbar", "Assigned", "Check", "Condition")]
    
    [Image(typeof(IconSkillHotbar), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class ConditionIsSkillInHotbar : Condition
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private bool m_CheckSpecificSlot = false;
        [SerializeField] private PropertyGetInteger m_SlotIndex = new PropertyGetInteger(0);
        
        protected override string Summary
        {
            get
            {
                if (m_CheckSpecificSlot)
                {
                    return $"{m_Skill?.name ?? "(none)"} in Slot {m_SlotIndex}";
                }
                return $"{m_Skill?.name ?? "(none)"} in Hotbar";
            }
        }
        
        protected override bool Run(Args args)
        {
            if (m_Skill == null) return false;
            
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            SkillHotbarUI hotbarUI = skillTreeComponent.GetSkillHotbarUI();
            if (hotbarUI == null) return false;
            
            if (m_CheckSpecificSlot)
            {
                // Check specific slot
                int slotIndex = (int)m_SlotIndex.Get(args);
                
                if (slotIndex < 0 || slotIndex >= hotbarUI.slots.Count)
                {
                    Debug.LogWarning($"[SkillTree] Invalid slot index: {slotIndex}");
                    return false;
                }
                
                return hotbarUI.slots[slotIndex].skill == m_Skill;
            }
            else
            {
                // Check any slot
                foreach (var slot in hotbarUI.slots)
                {
                    if (slot.skill == m_Skill)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
