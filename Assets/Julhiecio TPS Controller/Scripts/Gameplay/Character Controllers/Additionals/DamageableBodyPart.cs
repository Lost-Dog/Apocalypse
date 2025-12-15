using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUTPS.ArmorSystem
{
    [AddComponentMenu("JU TPS/Armor System/Damageable Body Part")]
    public class DamageableBodyPart : MonoBehaviour
    {
        public JUHealth Health;
        public float DamageMultiplier = 1;
        public Armor ArmorProtecting;
        private void Start()
        {
            if (Health == null)
            {
                Health = GetComponentInParent<JUHealth>();
            }
        }
        public float DoDamage(JUHealth.DamageInfo damageInfo)
        {
            if (Health == null)
            {
                Debug.LogWarning("Could not do damage as the Health variable is null");
                return 0;
            }

            damageInfo.Damage *= DamageMultiplier;

            Health.DoDamage(damageInfo);
            if (ArmorProtecting != null && ArmorProtecting.enabled)
            {
                ArmorProtecting.DoDamageOnArmor(damageInfo.Damage);
            }

            return damageInfo.Damage;
        }
    }

}