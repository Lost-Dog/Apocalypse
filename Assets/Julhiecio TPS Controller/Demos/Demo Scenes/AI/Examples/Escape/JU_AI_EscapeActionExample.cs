using JU.CharacterSystem.AI.EscapeSystem;
using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="Escape"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Escape Action Example")]
    public class JU_AI_EscapeActionExample : JUCharacterAIBase
    {
        /// <summary>
        /// The escape area detector.
        /// </summary>
        public Escape Escape;

        /// <inheritdoc/>
        protected override void Reset()
        {
            base.Reset();

            Escape.Reset();
        }

        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();

            Escape.OnValidate();
        }

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            Escape.Setup(this);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Escape.Unsetup();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            var control = new AIControlData();

            // Try escape from some area if is inside.
            Escape.Update(ref control);

            Control = control;
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Escape.DrawGizmos();
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Escape.DrawGizmosSelected();
        }
    }
}


