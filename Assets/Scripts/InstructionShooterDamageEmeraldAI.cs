using System;
using System.Threading.Tasks;
using EmeraldAI;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Shooter;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]
    [Title("Damage Emerald AI Target")]
    [Description("Applies damage to an Emerald AI target hit by a Shooter weapon")]
    [Category("Shooter/Hit/Damage Emerald AI")]
    [Keywords("Shooter", "Emerald", "Damage", "Hit", "Bullet", "Projectile")]
    [Image(typeof(IconReaction), ColorTheme.Type.Red)]

    [Serializable]
    public class InstructionShooterDamageEmeraldAI : Instruction
    {
        public override string Title => "Damage Emerald AI Target";

        protected override Task Run(Args args)
        {
            GameObject target = args.Target;
            if (target == null) return DefaultResult;

            LocationBasedDamageArea locationBasedDamageArea =
                target.GetComponent<LocationBasedDamageArea>() ??
                target.GetComponentInParent<LocationBasedDamageArea>();

            EmeraldHealth emeraldHealth =
                target.GetComponent<EmeraldHealth>() ??
                target.GetComponentInParent<EmeraldHealth>();

            if (locationBasedDamageArea == null && emeraldHealth == null)
            {
                return DefaultResult;
            }

            ShotData shotData = ShooterWeapon.LastShotData;
            ShooterWeapon weapon = shotData.Weapon;

            int damageAmount = 1;
            int ragdollForce = 100;

            if (weapon != null)
            {
                damageAmount = Mathf.Max(1, Mathf.RoundToInt((float) weapon.Fire.Power(args)));
                ragdollForce = weapon.Fire.ForceEnabled
                    ? Mathf.Max(0, Mathf.RoundToInt(weapon.Fire.Force))
                    : 100;
            }

            Transform attackerTransform = shotData.Source != null ? shotData.Source.transform : args.Self != null ? args.Self.transform : null;

            damageAmount = ApplyPlayerLevelScaling(damageAmount, attackerTransform);

            if (locationBasedDamageArea != null)
            {
                locationBasedDamageArea.DamageArea(damageAmount, attackerTransform, ragdollForce, false);
                return DefaultResult;
            }

            emeraldHealth.Damage(damageAmount, attackerTransform, ragdollForce, false);
            return DefaultResult;
        }

        /// <summary>
        /// Scales damage dealt by the player's weapon using the player's current level, via
        /// ProgressionManager.GetWeaponDamageMultiplier(). Damage from non-player attackers
        /// (e.g. AI vs AI shots) is left unscaled.
        /// </summary>
        private static int ApplyPlayerLevelScaling(int damageAmount, Transform attackerTransform)
        {
            if (attackerTransform == null) return damageAmount;
            if (ProgressionManager.Instance == null) return damageAmount;

            bool isPlayer = attackerTransform.CompareTag("Player") ||
                             attackerTransform.GetComponentInParent<GC2PlayerProvider>() != null;

            if (!isPlayer) return damageAmount;

            float multiplier = ProgressionManager.Instance.GetWeaponDamageMultiplier();
            return Mathf.Max(1, Mathf.RoundToInt(damageAmount * multiplier));
        }
    }
}
