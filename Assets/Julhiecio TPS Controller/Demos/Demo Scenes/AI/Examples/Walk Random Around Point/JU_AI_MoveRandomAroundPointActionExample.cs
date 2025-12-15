using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="MoveRandomAroundPoint"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Move Random Around Point Action")]
    public class JU_AI_MoveRandomAroundPointActionExample : JUCharacterAIBase
    {
        /// <summary>
        /// The target.
        /// </summary>
        public Transform Target;

        /// <summary>
        /// The action to move randomly around point.
        /// </summary>
        public MoveRandomAroundPoint MoveAroundPoint;

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            MoveAroundPoint.Setup(this);
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();

            // Moving randomly around target position.
            if (Target)
                MoveAroundPoint.Update(Target.position, ref control);

            Control = control;
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            MoveAroundPoint.DrawGizmos();
        }
    }
}