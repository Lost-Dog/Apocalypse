using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Show Skill Indicator")]
    [Category("KingEdward/Skill Tree/Show Skill Indicator")]
    [Description("Shows the ground indicator for a skill. Use with Hold-to-Aim or during cast phase. Position follows cursor.")]

    [Image(typeof(IconDamageRadius), ColorTheme.Type.Red)]

    [Serializable]
    public class InstructionShowSkillIndicator : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Controller = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill;

        public override string Title => $"Show Indicator: {m_Skill?.name ?? "(none)"}";

        protected override Task Run(Args args)
        {
            if (m_Skill == null) return DefaultResult;

            GameObject go = m_Controller.Get(args);
            SkillIndicatorController controller = go != null ? go.GetComponent<SkillIndicatorController>() : null;
            if (controller == null)
            {
                controller = SkillIndicatorController.Instance;
            }

            controller?.ShowForSkill(m_Skill);
            return DefaultResult;
        }
    }
}
