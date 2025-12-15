using UnityEngine;
using JU.CharacterSystem.AI.HearSystem;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="HearSystem.HearSensor"/> AI sensor.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Hear Sensor Example")]
    public class JU_AI_HearActionExample : JUCharacterAIBase
    {
        private Vector3 _heardSoundPosition;

        /// <summary>
        /// The sensor.
        /// </summary>
        public HearSensor HearSensor;

        /// <summary>
        /// The action that control the AI to move to the position of the heard sound.
        /// </summary>
        public FollowPoint FollowHearPosition;

        protected override void Start()
        {
            base.Start();

            _heardSoundPosition = Character.transform.position;

            HearSensor.Setup(this);
            HearSensor.OnHear.AddListener(OnHear);
            FollowHearPosition.Setup(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();

            // Move to the heard sound position.
            FollowHearPosition.Update(_heardSoundPosition, ref control);
            Control = control;
        }

        private void OnHear(Vector3 position, GameObject source)
        {
            _heardSoundPosition = position;
            FollowHearPosition.ForceRecalculatePath(_heardSoundPosition);
        }
    }
}