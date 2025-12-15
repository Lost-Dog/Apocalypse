using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="FollowPoint"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Follow Point Action Example")]
    public class JU_AI_FollowPointActionExample : JUCharacterAIBase
    {
        /// <summary>
        /// The target to follow.
        /// </summary>
        public Transform Target;

        /// <summary>
        /// The action to move the AI to the target position.
        /// </summary>
        public FollowPoint FollowPoint;

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            FollowPoint.Setup(this);
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            if (Character.IsDead)
            {
                enabled = false;
                return;
            }

            base.Update();

            AIControlData control = new AIControlData();

            // Move to the target position.
            Vector3 movePosition = Target ? Target.position : transform.position;
            FollowPoint.Update(movePosition, ref control);

            Control = control;
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            FollowPoint.DrawGizmos();
        }
    }
}