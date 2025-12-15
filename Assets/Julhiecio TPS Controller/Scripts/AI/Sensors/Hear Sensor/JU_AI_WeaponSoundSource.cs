using JUTPS.WeaponSystem;
using UnityEngine;

namespace JU.CharacterSystem.AI.HearSystem
{
    /// <summary>
    /// Emit an alert if the <see cref="Weapon"/> shots for closest AI character that have <see cref="HearSensor"/>.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Hear Sensor/Weapon Sound Source")]
    [RequireComponent(typeof(Weapon))]
    public class JU_AI_WeaponSoundSource : MonoBehaviour
    {
        private Weapon _weapon;

        /// <summary>
        /// The max distance to be detected by an AI when the weapon shots.
        /// </summary>
        public float MaxSoundDistance;

        /// <summary>
        /// The sound tag.
        /// </summary>
        public JUTag SoundTag;

        /// <summary>
        /// Create component instance.
        /// </summary>
        public JU_AI_WeaponSoundSource()
        {
            MaxSoundDistance = 20;
        }

        private void Start()
        {
            _weapon = GetComponent<Weapon>();
            if (!_weapon)
                return;

            _weapon.OnShot.AddListener(OnShot);
        }

        private void OnShot()
        {
            HearSensor.AddSoundSource(_weapon.transform.position, MaxSoundDistance, _weapon.TPSOwner.gameObject, SoundTag);
        }
    }
}