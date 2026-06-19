using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Transitions 
{
    [Version(0, 1, 1)]

    [Title("Transition Start")]
    [Description("Starts executing a transition without ending it")]

    [Category("Transitions/Transition Start")]

    [Parameter(
        "Transition",
        "The transition to use"
    )]
    
    [Parameter(
        "Wait to Finish",
        "Whether to wait until the effect has completed"
    )]
    
    [Image(typeof(IconGradient), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    
    [Serializable]
    public class InstructionTransitionsRunIn : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Transition m_Transition;
        [SerializeField] private bool m_WaitToFinish;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => string.Format(
            "Transition in {0}{1}",
            this.m_Transition != null ? this.m_Transition.name : "(none)",
            this.m_WaitToFinish ? " and wait" : string.Empty
        );

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override async Task Run(Args args)
        {
            if (this.m_Transition == null) return;

            if (this.m_WaitToFinish) await this.m_Transition.In();
            else _ = this.m_Transition.In();
        }
    }
}