using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Set Number with Easing Plus")]
    [Description("Sets a number property with a transition over time, applying an easing effect")]

    [Category("Math/Arithmetic/Set Number Plus")]

    [Parameter("Set", "The value to set")]
    [Parameter("From", "The value to transition to")]
    [Parameter("Duration", "How long it takes to perform the transition")]
    [Parameter("Easing", "The easing curve for the transition")]
    [Parameter("Wait to Complete", "Whether to wait for the transition to complete")]

    [Keywords("Set", "Float", "Integer", "Number")]
    [Image(typeof(IconArrowCircleDown), ColorTheme.Type.Red)]

    [Serializable]
    public class InstructionArithmeticSetNumber : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private PropertySetNumber m_Set = SetNumberGlobalName.Create;

        [SerializeField]
        private PropertyGetDecimal m_From = new PropertyGetDecimal();

        [Space]
        [SerializeField] private Transition m_Transition = new Transition();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Set {this.m_Set} to {this.m_From} over {this.m_Transition.Duration}s with {this.m_Transition.EasingType} easing";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            double initialValue = this.m_Set.Get(args);
            double targetValue = this.m_From.Get(args);

            GameObject gameObject = args.Self;
            if (gameObject == null) return;

            ITweenInput tween = new TweenInput<float>(
                (float)initialValue,
                (float)targetValue,
                this.m_Transition.Duration,
                (a, b, t) => this.m_Set.Set(Mathf.Lerp(a, b, t), args),
                Tween.GetHash(typeof(PropertySetNumber), this.m_Set.ToString()),
                this.m_Transition.EasingType,
                this.m_Transition.Time
            );

            Tween.To(gameObject, tween);
            if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
        }
    }
}
