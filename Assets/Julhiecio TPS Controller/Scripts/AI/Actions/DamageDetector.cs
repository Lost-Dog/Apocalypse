using JUTPS;
using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Detect damage hits and alert the AI looking to the object that caused the damage.
    /// </summary>
    [System.Serializable]
    public class DamageDetector : JU_AIActionBase
    {
        private bool _wasDamaged;
        private double _lastDamageTime;

        /// <summary>
        /// The max time the AI will look to the direction of the hit when takes damage.
        /// </summary>
        public float LookToDamageDirectionTime;

        /// <summary>
        /// Use fire pose when looking to the direction of the hit when takes damage.
        /// </summary>
        public bool ForceFirePose;

        /// <summary>
        /// Stop when looking to the direction of the hit when takes damage.
        /// </summary>
        public bool ForceStop;

        /// <summary>
        /// The <see cref="JU_AIActionBase.Ai"/> health component.
        /// </summary>
        public JUHealth AiHealth { get; private set; }

        /// <summary>
        /// Return true if is looking to the last damage hit direction.
        /// </summary>
        public bool IsLookinToDamageDirection { get; private set; }

        /// <summary>
        /// Return the last damage information, if was damaged.
        /// </summary>
        public JUHealth.DamageInfo LastDamage { get; private set; }

        /// <summary>
        /// Called when a damage hit is detected.
        /// </summary>
        public UnityEvent<JUHealth.DamageInfo> OnDetectDamage;

        /// <inheritdoc/>
        public DamageDetector()
        {
            LookToDamageDirectionTime = 5f;
            ForceFirePose = true;
            ForceStop = false;
        }

        /// <inheritdoc/>
        public override void Setup(JUCharacterAIBase ai)
        {
            base.Setup(ai);

            AiHealth = ai.Character.CharacterHealth;

            if (AiHealth)
                AiHealth.OnDamaged.AddListener(OnDamaged);
        }

        /// <inheritdoc/>
        public override void Unsetup()
        {
            base.Unsetup();

            if (AiHealth)
                AiHealth.OnDamaged.RemoveListener(OnDamaged);
        }

        /// <summary>
        /// Look to the hit direction when takes damage.
        /// </summary>
        /// <param name="control">The AI control data.</param>
        public void Update(ref JUCharacterAIBase.AIControlData control)
        {
            if (!_wasDamaged || Ai.Character.IsRagdolled)
                return;

            if ((Time.timeAsDouble - _lastDamageTime) > LookToDamageDirectionTime)
            {
                _wasDamaged = false;
                IsLookinToDamageDirection = false;
                return;
            }

            bool isUsingWeapon = Ai.Character.RightHandWeapon;
            bool isFirePose = isUsingWeapon && control.IsAttackPose;

            IsLookinToDamageDirection = true;
            if (ForceFirePose && isUsingWeapon) control.IsAttackPose = true;
            if (ForceStop) control.MoveToDirection = Vector3.zero;

            // Stop moving if is not on fire mode to look to direction works correctly.
            if (!isFirePose)
                control.MoveToDirection = Vector3.zero;

            control.LookToDirection = LastDamage.HitDirection;
        }

        private void OnDamaged(JUHealth.DamageInfo damageInfo)
        {
            if (damageInfo.HitDirection.magnitude < 0.1f)
                return;

            _wasDamaged = true;
            LastDamage = damageInfo;
            _lastDamageTime = Time.time;

            OnDetectDamage?.Invoke(damageInfo);
        }
    }
}