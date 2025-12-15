using JUTPS.WeaponSystem;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <inheritdoc/>
    [RequireComponent(typeof(MeleeWeapon))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Melee Weapon")]
    public class JUSaveLoadMeleeWeapon : JUSaveLoadItem<MeleeWeapon>
    {
        private const string USE_HEALTH_KEY = "Use health";
        private const string HEALTH = "Health";
        private const string DAMAGE_PER_USE = "Damage Per Use";

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            Item.EnableHealthLoss = GetValue(USE_HEALTH_KEY, Item.EnableHealthLoss);
            Item.MeleeWeaponHealth = GetValue(HEALTH, Item.MeleeWeaponHealth);
            Item.DamagePerUse = GetValue(DAMAGE_PER_USE, Item.DamagePerUse);
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(USE_HEALTH_KEY, Item.EnableHealthLoss);
            SetValue(HEALTH, Item.MeleeWeaponHealth);
            SetValue(DAMAGE_PER_USE, Item.DamagePerUse);
        }
    }
}