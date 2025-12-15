using JUTPS.WeaponSystem;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <inheritdoc/>
    [RequireComponent(typeof(Weapon))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Weapon")]
    public class JUSaveLoadWeapon : JUSaveLoadItem<Weapon>
    {
        private const string BULLETS_PER_MAGAZINE_KEY = "Bullets Per Magazine";
        private const string TOTAL_BULLETS_KEY = "Bullets Total Bullets";
        private const string BULLETS_AMOUNT_KEY = "Bullets Amount";
        private const string NUMBER_OF_SPAWNS_PER_SHOT_KEY = "Spawn Count Per Shot";
        private const string USE_INFINITY_AMMO_KEY = "Use Infinite Ammo";
        private const string PRECISION_KEY = "Precision";
        private const string LOSS_OF_ACCURACY_PER_SHOT_KEY = "Loss Of Accuracy Per Shot";

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            Item.BulletsPerMagazine = GetValue(BULLETS_PER_MAGAZINE_KEY, Item.BulletsPerMagazine);
            Item.TotalBullets = GetValue(TOTAL_BULLETS_KEY, Item.TotalBullets);
            Item.BulletsAmounts = GetValue(BULLETS_AMOUNT_KEY, Item.BulletsAmounts);
            Item.NumberOfShotgunBulletsPerShot = GetValue(NUMBER_OF_SPAWNS_PER_SHOT_KEY, Item.NumberOfShotgunBulletsPerShot);
            Item.InfiniteAmmo = GetValue(USE_INFINITY_AMMO_KEY, Item.InfiniteAmmo);
            Item.Precision = GetValue(PRECISION_KEY, Item.Precision);
            Item.LossOfAccuracyPerShot = GetValue(LOSS_OF_ACCURACY_PER_SHOT_KEY, Item.LossOfAccuracyPerShot);
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(BULLETS_PER_MAGAZINE_KEY, Item.BulletsPerMagazine);
            SetValue(TOTAL_BULLETS_KEY, Item.TotalBullets);
            SetValue(BULLETS_AMOUNT_KEY, Item.BulletsAmounts);
            SetValue(NUMBER_OF_SPAWNS_PER_SHOT_KEY, Item.NumberOfShotgunBulletsPerShot);
            SetValue(USE_INFINITY_AMMO_KEY, Item.InfiniteAmmo);
            SetValue(PRECISION_KEY, Item.Precision);
            SetValue(LOSS_OF_ACCURACY_PER_SHOT_KEY, Item.LossOfAccuracyPerShot);
        }
    }
}