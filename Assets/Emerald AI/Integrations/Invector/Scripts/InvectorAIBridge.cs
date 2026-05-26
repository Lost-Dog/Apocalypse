using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using EmeraldAI.Utility;

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
        EmeraldSystem m_EmeraldSystem;

        protected override void Start()
        {
            base.Start();

            m_IDamageable  = GetComponent<IDamageable>();
            m_EmeraldHealth = GetComponent<EmeraldHealth>();
            m_EmeraldSystem = GetComponent<EmeraldSystem>();

            maxHealth      = m_EmeraldHealth.StartingHealth;
            _currentHealth = m_EmeraldHealth.CurrentHealth;
        }

        public override void TakeDamage (vDamage damage)
        {
            base.TakeDamage(damage);
            m_IDamageable.Damage((int)damage.damageValue, damage.sender, 100);
        }

        /// <summary>
        /// Called by PoolableCharacter.ResetHealth() when this enemy is re-used from the pool.
        /// Resets both the Invector health side and Emerald AI's dead state and components
        /// so the agent can take damage again immediately.
        /// </summary>
        public override void ResetHealth()
        {
            base.ResetHealth();

            if (m_EmeraldHealth == null || m_EmeraldSystem == null) return;

            // Reset Emerald health values.
            m_EmeraldHealth.InstantlyRefillAIHealth();

            // Clear the dead flag and re-enable all AI components that were
            // shut down by EmeraldCombatManager.DisableComponents on death.
            m_EmeraldSystem.AnimationComponent.IsDead = false;
            EmeraldCombatManager.EnableComponents(m_EmeraldSystem);
        }
    }
}