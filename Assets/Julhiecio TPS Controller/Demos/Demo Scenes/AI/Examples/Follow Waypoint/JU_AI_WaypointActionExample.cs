using JUTPS.AI;
using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="FollowWaypoint"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Follow Waypoint Action Example")]
    public class JU_AI_WaypointActionExample : JUCharacterAIBase
    {
        /// <summary>
        /// The path.
        /// </summary>
        public WaypointPath Path;

        /// <summary>
        /// The action that control the AI to move throught a waypoint path.
        /// </summary>
        public FollowWaypoint FollowWaypoint;

        ///inheritdoc
        protected override void Start()
        {
            base.Start();
            FollowWaypoint.Setup(this);
        }

        ///inheritdoc
        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();

            // Move throught a waypoint path.
            FollowWaypoint.Update(Path, ref control);
            Control = control;
        }

        ///inheritdoc
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            FollowWaypoint.DrawGizmos();
        }
    }
}