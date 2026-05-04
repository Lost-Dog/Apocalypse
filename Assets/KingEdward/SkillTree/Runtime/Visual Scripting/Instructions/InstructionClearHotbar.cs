using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Clear Hotbar")]
    [Description("Removes all skills from the hotbar")]
    
    [Category("KingEdward/Skill Tree/Hotbar/Clear Hotbar")]
    
    [Parameter("Hotbar UI", "The SkillHotbarUI component to clear")]
    
    [Image(typeof(IconSkillHotbar), ColorTheme.Type.Red)]
    
    [Serializable]
    public class InstructionClearHotbar : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_HotbarUI = GetGameObjectInstance.Create();
        
        public override string Title => "Clear Hotbar";
        
        protected override Task Run(Args args)
        {
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
            
            hotbar.ClearAllSlots();
            
            return DefaultResult;
        }
    }
}
