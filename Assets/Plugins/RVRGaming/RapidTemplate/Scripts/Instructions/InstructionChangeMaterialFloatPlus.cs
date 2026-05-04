using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Change Material Float+")]
    [Description("Changes over time the Float property of an instantiated material of a Renderer component, including all children renderers")]

    [Image(typeof(IconNumber), ColorTheme.Type.Yellow)]

    [Category("Renderer/Change Material Float+")]

    [Parameter("Property", "Name of the property to change")]
    [Parameter("Float", "Decimal target that the instantiated Material's property turns into")]
    [Parameter("Duration", "How long it takes to perform the transition")]
    [Parameter("Easing", "The change rate of the transition over time")]
    [Parameter("Wait to Complete", "Whether to wait until the transition is finished or not")]

    [Keywords("Set", "Shader", "Hue")]
    [Serializable]
    public class InstructionChangeMaterialFloatPlus : TInstructionRenderer
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private PropertyGetString m_Property = new PropertyGetString("_Glossiness");

        [SerializeField]
        private ChangeDecimal m_Decimal = new ChangeDecimal(1f);

        [Space]
        [SerializeField] private Transition m_Transition = new Transition();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Change {this.m_Property} of {this.m_Renderer} {this.m_Decimal}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_Renderer.Get(args);
            if (gameObject == null) return;

            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.material == null) continue;

                string property = this.m_Property.Get(args);
                int propertyID = Shader.PropertyToID(property);

                if (!renderer.material.HasProperty(propertyID)) continue;

                float valueSource = renderer.material.GetFloat(propertyID);
                float valueTarget = (float)this.m_Decimal.Get(valueSource, args);

                ITweenInput tween = new TweenInput<float>(
                    valueSource,
                    valueTarget,
                    this.m_Transition.Duration,
                    (a, b, t) => renderer.material.SetFloat(propertyID, Mathf.Lerp(a, b, t)),
                    Tween.GetHash(typeof(Renderer), property),
                    this.m_Transition.EasingType,
                    this.m_Transition.Time
                );

                Tween.To(gameObject, tween);
                if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
            }
        }
    }
}
