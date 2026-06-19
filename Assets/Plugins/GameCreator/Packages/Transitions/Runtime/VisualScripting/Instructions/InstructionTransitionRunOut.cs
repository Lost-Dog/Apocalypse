using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Transitions 
{
    [Version(0, 1, 1)]

    [Title("Transition Complete")]
    [Description("Finishes the currently active transition")]

    [Category("Transitions/Transition Complete")]

    [Parameter(
        "Wait to Finish",
        "Whether to wait until the effect has completed"
    )]
    
    [Image(typeof(IconGradient), ColorTheme.Type.Red, typeof(OverlayArrowLeft))]
    
    [Serializable]
    public class InstructionTransitionRunOut : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private bool m_WaitToFinish;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => string.Format(
            "Transition out{0}",
            this.m_WaitToFinish ? " and wait" : string.Empty
        );

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override async Task Run(Args args)
        {
            if (Transition.Current == null) return;

            if (this.m_WaitToFinish) await Transition.Current.Out();
            else _ = Transition.Current.Out();
        }
    }
}