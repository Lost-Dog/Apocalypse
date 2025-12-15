using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="MoveRandomInsideArea"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Move Random Inside Area Action")]
    public class JU_AI_MoveRandomInsideAreaExample : JUCharacterAIBase
    {
        /// <summary>
        /// The area to move inside.
        /// </summary>
        public JUBoxArea Area;

        /// <summary>
        /// The action to move the AI.
        /// </summary>
        public MoveRandomInsideArea MoveInsideArea;

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            MoveInsideArea.Setup(this);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            MoveInsideArea.Unsetup();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();
            MoveInsideArea.Update(Area, ref control);

            Control = control;
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            MoveInsideArea.DrawGizmos();
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            MoveInsideArea.DrawGizmosSelected();
        }
    }
}