using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Hide Skill Indicator")]
    [Category("KingEdward/Skill Tree/Hide Skill Indicator")]
    [Description("Hides the skill ground indicator.")]

    [Image(typeof(IconDamageRadius), ColorTheme.Type.TextNormal)]

    [Serializable]
    public class InstructionHideSkillIndicator : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Controller = GetGameObjectSelf.Create();

        public override string Title => "Hide Skill Indicator";

        protected override Task Run(Args args)
        {
            GameObject go = m_Controller.Get(args);
            SkillIndicatorController controller = go != null ? go.GetComponent<SkillIndicatorController>() : null;
            if (controller == null)
            {
                controller = SkillIndicatorController.Instance;
            }

            controller?.Hide();
            return DefaultResult;
        }
    }
}
