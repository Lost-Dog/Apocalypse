using JUTPS.ArmorSystem;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <inheritdoc/>
    [RequireComponent(typeof(Armor))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Armor")]
    public class JUSaveLoadArmor : JUSaveLoadItem<Armor>
    {
        private const string HEALTH_ENABLED_KEY = "Health Enabled";
        private const string PROTECTION_ENABLED_KEY = "Protection Enabled";
        private const string HEALTH_KEY = "Health";
        private const string DAMAGE_MULTIPLIER_KEY = "Damage Multiplier";

        /// <inheritdoc/>
        public JUSaveLoadArmor()
        {
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(HEALTH_ENABLED_KEY, Item.EnableArmorHealth);
            SetValue(PROTECTION_ENABLED_KEY, Item.EnableArmorProtection);
            SetValue(HEALTH_KEY, Item.Health);
            SetValue(DAMAGE_MULTIPLIER_KEY, Item.DamageMultiplier);
        }

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            Item.EnableArmorHealth = GetValue(HEALTH_ENABLED_KEY, Item.EnableArmorHealth);
            Item.EnableArmorProtection = GetValue(PROTECTION_ENABLED_KEY, Item.EnableArmorProtection);
            Item.Health = GetValue(HEALTH_KEY, Item.Health);
            Item.DamageMultiplier = GetValue(DAMAGE_MULTIPLIER_KEY, Item.DamageMultiplier);
        }
    }
}