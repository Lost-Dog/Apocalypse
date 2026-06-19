using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Clear Skill Tree Selection")]
    [Category("KingEdward/Skill Tree/UI/Clear Skill Tree Selection")]
    [Description("Clears gamepad/keyboard selection and hides the tooltip. Use after Respec or other actions so focus doesn't jump to the first node. Next stick/d-pad input will allow selecting a node again.")]

    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.TextNormal)]

    [Keywords("Skill", "Tree", "Selection", "Clear", "Gamepad", "Tooltip", "UI")]

    [Serializable]
    public class InstructionClearSkillTreeSelection : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

        public override string Title => "Clear Skill Tree Selection";

        protected override Task Run(Args args)
        {
            GameObject go = m_Target.Get(args);
            if (go == null) return DefaultResult;

            SkillTreeUI skillTreeUI = go.GetComponent<SkillTreeUI>();
            if (skillTreeUI == null)
            {
                SkillTreeComponent comp = go.GetComponent<SkillTreeComponent>();
                if (comp != null)
                    skillTreeUI = comp.GetComponentInChildren<SkillTreeUI>(true);
            }
            skillTreeUI?.ClearSelectionAndTooltip();

            return DefaultResult;
        }
    }
}
