using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Canvas Group")]
    [Category("UI/Canvas Group")]

    [Image(typeof(IconUICanvasGroup), ColorTheme.Type.TextLight)]
    [Description("Changes the alpha, block raycasts, and interactable properties of the Canvas Group")]

    [Parameter("Canvas Group", "The Canvas Group component that changes its properties")]
    [Parameter("Alpha", "The new opacity value for the Canvas Group (optional)")]
    [Parameter("Duration", "How long the alpha transition takes (optional)")]
    [Parameter("Easing", "The easing function for the alpha transition (optional)")]
    [Parameter("Wait to Complete", "Whether to wait until the alpha transition is finished (optional)")]
    [Parameter("Block Raycasts", "If true, the Canvas Group and its children block raycasts (optional)")]
    [Parameter("Interactable", "The interactable state of the Canvas Group (optional)")]

    [Serializable]
    public class InstructionUICanvasGroup : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_CanvasGroup = GetGameObjectInstance.Create();
        [SerializeField] private ChangeDecimal m_Alpha = new ChangeDecimal(null);
        [SerializeField] private Transition m_Transition = new Transition();
        [SerializeField] private PropertyGetBool m_BlockRaycasts = GetBoolValue.Create(true);
        [SerializeField] private PropertyGetBool m_Interactable = GetBoolValue.Create(true);

        public override string Title => $"{this.m_CanvasGroup} Properties";

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_CanvasGroup.Get(args);
            if (gameObject == null) return;

            CanvasGroup canvasGroup = gameObject.Get<CanvasGroup>();
            if (canvasGroup == null) return;

            if (this.m_Alpha != null)
            {
                float valueSource = canvasGroup.alpha;
                float valueTarget = (float)this.m_Alpha.Get(valueSource, args);

                ITweenInput tween = new TweenInput<float>(
                    valueSource,
                    valueTarget,
                    this.m_Transition.Duration,
                    (a, b, t) => canvasGroup.alpha = Mathf.Lerp(a, b, t),
                    Tween.GetHash(typeof(CanvasGroup), "canvas-group-properties-alpha"),
                    this.m_Transition.EasingType,
                    this.m_Transition.Time
                );

                Tween.To(gameObject, tween);
                if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
            }

            if (this.m_BlockRaycasts != null)
            {
                canvasGroup.blocksRaycasts = this.m_BlockRaycasts.Get(args);
            }

            if (this.m_Interactable != null)
            {
                canvasGroup.interactable = this.m_Interactable.Get(args);
            }
        }
    }
}
