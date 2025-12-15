using UnityEngine;
namespace JUTPS.WeaponSystem
{

    [AddComponentMenu("JU TPS/Weapon System/Ammunition Box")]
    public class AmmoBox : MonoBehaviour
    {
        [Header("Bullet Amount")]
        public int AmmoCount = 32;
        public GameObject Effect;
        [Header("Weapon ID")]
        public string WeaponName = "AnyWeapon";
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                var pl = other.GetComponent<JUCharacterController>();
                if (pl.IsItemEquiped)
                {
                    if (pl.LeftHandWeapon == null && pl.RightHandWeapon == null) return;

                    if (pl.RightHandWeapon != null)
                    {
                        if (pl.RightHandWeapon.ItemName == WeaponName) pl.RightHandWeapon.TotalBullets += pl.LeftHandWeapon == null ? AmmoCount : AmmoCount / 2;
                    }
                    if (pl.LeftHandWeapon != null)
                    {
                        if (pl.LeftHandWeapon.ItemName == WeaponName) pl.LeftHandWeapon.TotalBullets += pl.RightHandWeapon == null ? AmmoCount : AmmoCount / 2;
                    }
                    if (WeaponName == "AnyWeapon")
                    {
                        if (pl.RightHandWeapon != null)
                            pl.RightHandWeapon.TotalBullets += pl.LeftHandWeapon == null ? AmmoCount : AmmoCount / 2;
                        if (pl.LeftHandWeapon != null)
                            pl.LeftHandWeapon.TotalBullets += pl.RightHandWeapon == null ? AmmoCount : AmmoCount / 2;
                    }
                    GameObject fx = Instantiate(Effect, transform.position, transform.rotation);
                    Destroy(fx, 5);
                    Destroy(this.gameObject, 0.1f);
                }
            }
        }
    }

}