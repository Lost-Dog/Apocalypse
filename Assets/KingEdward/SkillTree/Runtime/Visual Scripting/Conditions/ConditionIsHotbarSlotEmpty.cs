using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("Is Hotbar Slot Empty")]
    [Description("Checks if a hotbar slot is empty")]
    
    [Category("KingEdward/Skill Tree/Is Hotbar Slot Empty")]
    
    [Keywords("Skill", "Tree", "Hotbar", "Slot", "Empty", "Check", "Condition")]
    
    [Image(typeof(IconSkillHotbar), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class ConditionIsHotbarSlotEmpty : Condition
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetInteger m_SlotIndex = new PropertyGetInteger(0);
        
        protected override string Summary => $"Hotbar Slot {m_SlotIndex} is Empty";
        
        protected override bool Run(Args args)
        {
            SkillTreeComponent skillTreeComponent = m_SkillTreeComponent.Get<SkillTreeComponent>(args);
            if (skillTreeComponent == null) return false;
            
            SkillHotbarUI hotbarUI = skillTreeComponent.GetSkillHotbarUI();
            if (hotbarUI == null) return false;
            
            int slotIndex = (int)m_SlotIndex.Get(args);
            
            // Check if slot index is valid
            if (slotIndex < 0 || slotIndex >= hotbarUI.slots.Count)
            {
                Debug.LogWarning($"[SkillTree] Invalid slot index: {slotIndex}");
                return false;
            }
            
            // Check if slot is empty
            return hotbarUI.slots[slotIndex].skill == null;
        }
    }
}
