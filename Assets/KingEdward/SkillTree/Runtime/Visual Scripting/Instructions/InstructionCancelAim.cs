using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Cancel Aim")]
    [Category("KingEdward/Skill Tree/Skill Indicator")]
    [Description("Cancels the current aiming mode (hides the indicator) without casting the skill.")]

    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.TextNormal)]

    [Serializable]
    public class InstructionCancelAim : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = GetGameObjectSelf.Create();

        public override string Title => "Cancel Aim";

        protected override Task Run(Args args)
        {
            GameObject go = this.m_SkillTreeComponent.Get(args);
            SkillTreeComponent comp = go != null ? go.GetComponent<SkillTreeComponent>() : null;

            if (comp == null)
            {
                comp = UnityEngine.Object.FindFirstObjectByType<SkillTreeComponent>();
            }

            comp?.CancelAim();
            return DefaultResult;
        }
    }
}

