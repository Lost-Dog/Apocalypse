using JUTPS;
using JUTPS.FX;
using UnityEngine;

namespace JU.CharacterSystem.AI.HearSystem
{
    /// <summary>
    /// Emit an alert if the <see cref="JUCharacterController"/> is in moviment and closest to a AI character that have <see cref="HearSensor"/>.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Hear Sensor/Character Sound Source")]
    [RequireComponent(typeof(JUCharacterController), typeof(JUFootstep))]
    public class JU_AI_CharacterSoundSource : MonoBehaviour
    {
        /// <summary>
        /// Character sound Settings.
        /// </summary>
        [System.Serializable]
        public struct SoundSettings
        {
            /// <summary>
            /// Sound distance when is walking.
            /// </summary>
            public float WalkingSoundDistance;

            /// <summary>
            /// Sound disance when is running.
            /// </summary>
            public float RunningSoundDistance;

            /// <summary>
            /// Sound distance when is walking crouched.
            /// </summary>
            public float WalkingCrouchedSoundDistance;
        }

        private JUFootstep _footstep;
        private JUCharacterController _character;

        /// <summary>
        /// The sound tag.
        /// </summary>
        public JUTag SoundTag;

        /// <summary>
        /// The Sound Settings.
        /// </summary>
        public SoundSettings Sound;

        private void Start()
        {
            _character = GetComponent<JUCharacterController>();
            _footstep = GetComponent<JUFootstep>();

            Debug.Assert(_character && _footstep, $"The character {name} must have a {nameof(JUCharacterController)} and a {nameof(JUFootstep)}");

            _footstep.OnLeftFootHit.AddListener(OnFootstepHit);
            _footstep.OnRightFootHit.AddListener(OnFootstepHit);
        }

        private void OnDestroy()
        {
            if (!_character || !_footstep)
                return;

            _footstep.OnLeftFootHit.RemoveListener(OnFootstepHit);
            _footstep.OnRightFootHit.RemoveListener(OnFootstepHit);
        }

        private void OnFootstepHit(RaycastHit hit)
        {
            var soundDistance = Sound.WalkingSoundDistance;

            if (_character.IsRunning) soundDistance = Sound.RunningSoundDistance;
            else if (_character.IsCrouched) soundDistance = Sound.WalkingCrouchedSoundDistance;

            HearSensor.AddSoundSource(hit.point, soundDistance, _character.gameObject, SoundTag);
        }
    }
}