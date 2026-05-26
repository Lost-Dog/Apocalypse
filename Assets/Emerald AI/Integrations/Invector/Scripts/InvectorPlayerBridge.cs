using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;
using EmeraldAI.Utility;
using Invector.vCharacterController;

namespace EmeraldAI.Integrations
{
    [RequireComponent(typeof(TargetPositionModifier))]
    [RequireComponent(typeof(FactionExtension))]
    public class InvectorPlayerBridge : EmeraldPlayerBridge
    {
        vThirdPersonController m_vThirdPersonController;
#if INVECTOR_MELEE && !INVECTOR_SHOOTER
        vMeleeCombatInput m_vMeleeCombatInput;
#elif INVECTOR_SHOOTER && INVECTOR_MELEE
        vMeleeCombatInput m_vMeleeCombatInput;
        Invector.vShooter.vShooterManager m_vShooterManager;
#endif

        public override void Start()
        {
            m_vThirdPersonController = GetComponent<vThirdPersonController>();
#if INVECTOR_MELEE && !INVECTOR_SHOOTER
            m_vMeleeCombatInput = GetComponent<vMeleeCombatInput>();
#elif INVECTOR_SHOOTER && INVECTOR_MELEE
            m_vShooterManager = GetComponent<Invector.vShooter.vShooterManager>();
            m_vMeleeCombatInput = GetComponent<vMeleeCombatInput>();
#endif

            if (transform.Find("HeadTrackSensor") != null)
            {
                //Unfortunately, the HeadTrackSensor causes issues (for anyone) trying to do integrations as it creates a collider over the whole player
                //that interferes with collisions, detection, projectiles, etc. The easiest way to fix this is to just disable it.
                //If you need it, you will have to comment out the two lines below, set all abilities to ignore the HeadTrack layer,
                //and have all AI ignore the HeadTrack layer through their Obstruction Ignore Layer layermask (located within the Detection Settings foldout).
                vHeadTrackSensor m_vHeadTrackSensor = transform.Find("HeadTrackSensor").GetComponent<vHeadTrackSensor>();
                if (m_vHeadTrackSensor != null) m_vHeadTrackSensor.gameObject.SetActive(false);
            }

            //Set the health equal to the health from the Invector settings.
            StartHealth = (int)m_vThirdPersonController.maxHealth;
            Health = (int)m_vThirdPersonController.maxHealth;
        }

        /// <summary>
        /// Used for passing damage to any script that has an IDamageable component.
        /// </summary>
        public override void DamageCharacterController(int DamageAmount, Transform AttackerTransform)
        {
            DamageInvectorPlayer(DamageAmount, AttackerTransform);
        }

        /// <summary>
        /// A custom damage function for damaging Invector.
        /// </summary>
        void DamageInvectorPlayer(int DamageAmount, Transform Target)
        {
            if (GetComponent<vCharacter>())
            {
#if INVECTOR_MELEE || INVECTOR_SHOOTER
                //Applies damage to Invector and allows its melee weapons to block incoming Emerald AI damage.
                if (GetComponent<vMeleeCombatInput>().meleeManager != null)
                {
                    var PlayerInput = GetComponent<vMeleeCombatInput>();
                    var MeleeManager = GetComponent<vMeleeCombatInput>().meleeManager;

                    if (PlayerInput.isBlocking)
                    {
                        GetComponent<vThirdPersonAnimator>().animator.SetTrigger("TriggerRecoil");
                        var _Damage = new Invector.vDamage(DamageAmount);
                        var DamageReduction = MeleeManager != null ? MeleeManager.GetDefenseRate() : 0;
                        if (DamageReduction > 0)
                            _Damage.ReduceDamage(DamageReduction);
                        MeleeManager.OnDefense();
                        _Damage.recoil_id = 0;
                        _Damage.sender = Target;
                        _Damage.hitPosition = Target.position;
                        GetComponent<vCharacter>().TakeDamage(_Damage);
                        DisplayDamageText(DamageAmount);
                    }
                    else if (m_vThirdPersonController.isRolling && m_vThirdPersonController.noDamageWhileRolling)
                    {
                        //Can add custom code here, if desired.
                    }
                    else
                    {
                        var _Damage = new Invector.vDamage(DamageAmount);
                        _Damage.sender = Target;
                        _Damage.hitPosition = Target.position;
                        _Damage.reaction_id = 0;
                        GetComponent<vCharacter>().TakeDamage(_Damage);
                        DisplayDamageText(DamageAmount);
                    }
                }
                //If no Melee Manager is found, cause unreduced damage.
                else
                {
                    var _Damage = new Invector.vDamage(DamageAmount);
                    _Damage.sender = Target;
                    _Damage.hitPosition = Target.position;
                    _Damage.reaction_id = 0;
                    GetComponent<vCharacter>().TakeDamage(_Damage);
                    DisplayDamageText(DamageAmount);
                }
#else
                    var _Damage = new Invector.vDamage(DamageAmount);
                    _Damage.sender = Target;
                    _Damage.hitPosition = Target.position;
                    _Damage.reaction_id = 0;
                    GetComponent<vCharacter>().TakeDamage(_Damage);
                    DisplayDamageText(DamageAmount);
#endif

                Health = (int)m_vThirdPersonController.currentHealth;
            }
        }

        /// <summary>
        /// Used for detecting when the Invector player is attacking (can be modified as needed).
        /// </summary>
        public override bool IsAttacking()
        {
#if INVECTOR_MELEE && !INVECTOR_SHOOTER
            return m_vMeleeCombatInput != null && m_vMeleeCombatInput.isAttacking;
#elif INVECTOR_SHOOTER && INVECTOR_MELEE
            return m_vMeleeCombatInput != null && m_vMeleeCombatInput.isAttacking || m_vShooterManager != null && m_vShooterManager.isShooting;
#endif
        }

        /// <summary>
        /// Used for detecting when the Invector player is blocking (can be modified as needed).
        /// </summary>
        public override bool IsBlocking()
        {
            return m_vMeleeCombatInput.isBlocking;
        }

        /// <summary>
        /// Used for detecting when the Invector player is rolling (can be modified as needed).
        /// </summary>
        public override bool IsDodging()
        {
            return m_vThirdPersonController.isRolling;
        }

        /// <summary>
        /// Used when an AI triggers a successful stun (not implemented here, but can be modified as needed).
        /// </summary>
        public override void TriggerStun(float StunLength)
        {
            //Can add custom stun effect here, if desired. This is called through Emerald AI when applicable with certain abilities.
        }

        /// <summary>
        /// Used for getting the transform of the target.
        /// </summary>
        public override Transform TargetTransform()
        {
            return transform;
        }
    }
}