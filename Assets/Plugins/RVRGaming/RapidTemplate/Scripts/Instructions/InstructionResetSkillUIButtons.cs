using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Version(0, 0, 1)]

    [Title("Reset Skill UI Button")]
    [Description("Resets all Skill UI buttons in the scene to their default state")]

    [Category("Skills/Reset Skill UI Button")]

    [Keywords("Reset", "UI", "Skill", "Button")]
    [Image(typeof(IconUIButton), ColorTheme.Type.Blue, typeof(OverlayMinus))]

    [Serializable]
    public class InstructionResetSkillUIButtons : Instruction
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Reset All Skill UI Buttons";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            SkillUIButton[] buttons = UnityEngine.Object.FindObjectsOfType<SkillUIButton>();
            foreach (SkillUIButton button in buttons)
            {
                button.ResetUI();
            }
            
            return Task.CompletedTask;
        }
    }
}
