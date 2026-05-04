using System;
using System.Linq;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Serializable]
    public class ActionReference
    {
        [SerializeField]
        public PropertyGetGameObject action;
    }

    [Version(0, 0, 3)]

    [Title("Run Random Actions")]
    [Description("Executes a random Actions component object from the provided set")]

    [Category("Logic/Run Random Actions")]

    [Parameter(
        "Actions List",
        "The list of Actions objects, one of which will be randomly executed"
    )]
    [Parameter(
        "Wait Until Complete",
        "If true this instruction waits until the selected Actions object finishes running"
    )]

    [Keywords("Execute", "Call", "Instruction", "Action", "Random")]
    [Image(typeof(IconInstructions), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionLogicRunRandomActions : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private ActionReference[] m_ActionsList = new ActionReference[0];
        [SerializeField] private bool m_WaitToFinish = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => string.Format(
            "Run Random Action {0}",
            this.m_WaitToFinish ? "and wait" : string.Empty
        );

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            if (m_ActionsList == null || m_ActionsList.Length == 0) return;

            // Randomly select an Actions object
            PropertyGetGameObject randomActionProperty = m_ActionsList[UnityEngine.Random.Range(0, m_ActionsList.Length)].action;
            Actions randomAction = randomActionProperty.Get<Actions>(args);

            if (randomAction == null) return;

            if (this.m_WaitToFinish) await randomAction.Run(args);
            else _ = randomAction.Run(args);
        }
    }
}
