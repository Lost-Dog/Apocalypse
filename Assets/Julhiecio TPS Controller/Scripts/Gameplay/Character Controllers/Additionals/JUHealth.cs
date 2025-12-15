using JU;
using JUTPS.FX;
using JUTPSEditor.JUHeader;
using UnityEngine;
using UnityEngine.Events;

namespace JUTPS
{

    [AddComponentMenu("JU TPS/Third Person System/Additionals/JU Health")]
    public class JUHealth : MonoBehaviour, IIsDead
    {
        [System.Serializable]
        public class DamageEvent : UnityEvent<DamageInfo> { }

        /// <summary>
        /// Stores information about damage.
        /// </summary>
        [System.Serializable]
        public struct DamageInfo
        {
            /// <summary>
            /// The damage count.
            /// </summary>
            public float Damage;

            /// <summary>
            /// The damage hit position on this health gameObject or the owner of this health system.
            /// </summary>
            public Vector3 HitPosition;

            /// <summary>
            /// The direction that the damage came from.
            /// </summary>
            public Vector3 HitDirection;

            /// <summary>
            /// The initial damage position (The position of the shot, as example for weapons or the other character that attacked this AI).
            /// </summary>
            public Vector3 HitOriginPosition;

            /// <summary>
            /// The damage origem  object, like other character that attacked this AI.
            /// </summary>
            public GameObject HitOwner;
        }

        [JUHeader("Settings")]
        public float Health = 100;
        public float MaxHealth = 100;

        [JUHeader("Effects")]
        public bool BloodScreenEffect = false;
        public GameObject BloodHitParticle;

        [JUHeader("On Death Event")]
        public UnityEvent OnDeath;
        public DamageEvent OnDamaged;

        /// <inheritdoc/>
        public bool IsDead { get; private set; }

        void Start()
        {
            LimitHealth();
            InvokeRepeating(nameof(CheckHealthState), 0, 0.5f);
        }
        private void LimitHealth()
        {
            Health = Mathf.Clamp(Health, 0, MaxHealth);
        }

        public void DoDamage(float damage)
        {
            DoDamage(new DamageInfo { Damage = damage });
        }

        public void DoDamage(DamageInfo damageInfo)
        {
            Health -= damageInfo.Damage;
            LimitHealth();
            Invoke(nameof(CheckHealthState), 0.016f);

            if (BloodScreenEffect) BloodScreen.PlayerTakingDamaged();
            if (damageInfo.HitPosition != Vector3.zero && BloodHitParticle != null)
            {
                GameObject fxParticle = Instantiate(BloodHitParticle, damageInfo.HitPosition, Quaternion.identity);
                fxParticle.hideFlags = HideFlags.HideInHierarchy;
                Destroy(fxParticle, 3);
            }

            OnDamaged.Invoke(damageInfo);
        }

        internal void CheckHealthState()
        {
            LimitHealth();

            if (Health <= 0 && IsDead == false)
            {
                Health = 0;
                IsDead = true;

                //Disable all damagers0
                foreach (Damager dmg in GetComponentsInChildren<Damager>()) dmg.gameObject.SetActive(false);

                OnDeath.Invoke();
            }

            if (Health > 0) IsDead = false;
        }

        public void ResetHealth()
        {
            Health = MaxHealth;
            IsDead = false;
            CheckHealthState();
        }
    }
}