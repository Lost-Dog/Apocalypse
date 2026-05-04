using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Change Material Color+")]
    [Description("Changes over time the Color property of an instantiated material of a Renderer component")]

    [Image(typeof(IconColor), ColorTheme.Type.Yellow)]

    [Category("Renderer/Change Material Color+")]

    [Parameter("Property", "Name of the property to change")]
    [Parameter("Color", "Color target that the instantiated Material turns into")]
    [Parameter("Duration", "How long it takes to perform the transition")]
    [Parameter("Easing", "The change rate of the transition over time")]
    [Parameter("Wait to Complete", "Whether to wait until the transition is finished or not")]

    [Keywords("Set", "Shader", "Hue")]
    [Serializable]
    public class InstructionChangeMaterialColorPlus : TInstructionRenderer
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private PropertyGetString m_Property = new PropertyGetString("_Color");

        [SerializeField]
        private ChangeColor m_Color = new ChangeColor();

        [Space]
        [SerializeField] private Transition m_Transition = new Transition();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Change {this.m_Property} of {this.m_Renderer} {this.m_Color}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_Renderer.Get(args);
            if (gameObject == null) return;

            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            List<Task> allTweens = new List<Task>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                if (materials == null) continue;

                foreach (Material material in materials)
                {
                    if (material == null) continue;

                    string property = this.m_Property.Get(args);
                    int propertyID = Shader.PropertyToID(property);

                    if (!material.HasProperty(propertyID)) continue;

                    Color valueSource = material.GetColor(propertyID);
                    Color valueTarget = this.m_Color.Get(valueSource, args);

                    // Create a unique identifier for the tween
                    string uniqueID = $"{renderer.GetInstanceID()}_{material.GetInstanceID()}_{property}";
                    int hash = uniqueID.GetHashCode();

                    ITweenInput tween = new TweenInput<Color>(
                        valueSource,
                        valueTarget,
                        this.m_Transition.Duration,
                        (a, b, t) => material.SetColor(propertyID, Color.Lerp(a, b, t)),
                        hash, // Use the generated hash
                        this.m_Transition.EasingType,
                        this.m_Transition.Time
                    );

                    Tween.To(gameObject, tween);

                    if (this.m_Transition.WaitToComplete)
                    {
                        allTweens.Add(this.Until(() => tween.IsFinished));
                    }
                }
            }

            if (this.m_Transition.WaitToComplete)
            {
                await Task.WhenAll(allTweens);
            }
        }

    }
}