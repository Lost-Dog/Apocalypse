using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="FieldOfView"/> sensor.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Field Of View Sensor Example")]
    public class JU_AI_FieldOfViewSensorExample : JUCharacterAIBase
    {
        private Vector3 _destination;

        /// <summary>
        /// The character head, used by the field of view.
        /// </summary>
        public Transform Head;

        /// <summary>
        /// The field of view sensor.
        /// </summary>
        public FieldOfView FOV;

        /// <summary>
        /// Move to target position.
        /// </summary>
        public FollowPoint FollowTarget;

        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();

            if (FOV == null)
                FOV = new FieldOfView();

            FOV.Setup(this);
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            base.Reset();

            FOV.Reset();
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            FOV.Setup(this);
            FollowTarget.Setup(this);

            _destination = transform.position;
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();

            FOV.Update(Head);

            // Move to the target position (if is seeing).
            if (FOV.HasCollidersInView)
                _destination = FOV.LastColliderViewedPosition;

            FollowTarget.Update(_destination, ref control);
            Control = control;
        }

        /// <inheritdoc/>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            FollowTarget.DrawGizmos();
            FOV.DrawGizmos();
        }
    }
}