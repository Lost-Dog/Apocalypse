using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[Title("Unlock Skill")]
[Description("Unlocks a skill dynamically without scene reference")]

[Category("Skills/Unlock Skill")]

[Parameter("Skill Item", "The skill item to unlock")]
[Parameter("Bag", "The Bag that contains the player's inventory and equipment")]
[Parameter("Unlock Cost", "Cost required to unlock the skill")]

[Keywords("Skill", "Unlock", "Ability", "Activate", "Learn")]
[Image(typeof(IconItem), ColorTheme.Type.Yellow, typeof(OverlayPlus))]

[Serializable]
public class InstructionUnlockSkill : Instruction
{
    [SerializeField] private PropertyGetGameObject m_SkillButton = GetGameObjectInstance.Create();

    public override string Title => $"Simulate Click on {this.m_SkillButton}";

    protected override Task Run(Args args)
    {
        GameObject skillButtonObject = this.m_SkillButton.Get(args);
        if (skillButtonObject == null)
        {
            return DefaultResult;
        }

        SkillUIButton skillButton = skillButtonObject.GetComponent<SkillUIButton>();
        if (skillButton == null)
        {
            return DefaultResult;
        }

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        skillButton.OnPointerClick(pointerEventData);

        return DefaultResult;
    }
}
