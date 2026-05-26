using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;

namespace EmeraldAI
{
    /// <summary>
    /// This script works through Invector's vHealthController script by receiving damage from the TakeDamage function and passing to Emerald AI's Damage function.
    /// Note: This script needs to be applied to Emerald AI agents in order for them to be damaged by Invector.
    /// </summary>
    public class InvectorAIBridge : vHealthController
    {
        IDamageable m_IDamageable;
        EmeraldHealth m_EmeraldHealth;

        protected override void Start()
        {
            base.Start();

            m_IDamageable = GetComponent<IDamageable>();
            m_EmeraldHealth = GetComponent<EmeraldHealth>();

            maxHealth = m_EmeraldHealth.StartingHealth;
            _currentHealth = m_EmeraldHealth.CurrentHealth;
        }

        public override void TakeDamage (vDamage damage)
        {
            base.TakeDamage(damage);
            m_IDamageable.Damage((int)damage.damageValue, damage.sender, 100);
        }
    }
}