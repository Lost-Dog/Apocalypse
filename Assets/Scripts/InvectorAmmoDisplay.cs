using Invector.vItemManager;
using Invector.vShooter;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays ammo for the currently equipped Invector vShooterWeapon on two
/// TextMeshProUGUI elements:
///   Magazine Text  →  "12 / 30"  (rounds in clip / clip capacity)
///   Reserve Text   →  "90"       (total reserve rounds outside the clip)
/// Subscribes to vShooterManager equip events and polls weapon state on change.
/// </summary>
public class InvectorAmmoDisplay : MonoBehaviour
{
    private const string InfiniteText = "∞";
    private const string LogPrefix    = "[InvectorAmmoDisplay]";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI magazineText;
    [SerializeField] private TextMeshProUGUI reserveText;

    [Header("Settings")]
    [Tooltip("Text shown when no shooter weapon is equipped.")]
    [SerializeField] private string noWeaponText = "--";

    [Header("References")]
    [Tooltip("Leave empty to auto-find on Start.")]
    [SerializeField] private vShooterManager shooterManager;

    private vShooterWeapon trackedWeapon;
    private vAmmoManager   ammoManager;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (shooterManager == null)
            shooterManager = FindFirstObjectByType<vShooterManager>();

        if (shooterManager == null)
        {
            Debug.LogWarning($"{LogPrefix} Could not find a vShooterManager in the scene.");
            ShowNoWeapon();
            return;
        }

        ammoManager = shooterManager.GetComponent<vAmmoManager>();
        if (ammoManager != null)
            ammoManager.updateTotalAmmo += Refresh;

        shooterManager.onEquipWeapon.AddListener(OnWeaponEquipped);
        shooterManager.onUnequipWeapon.AddListener(OnWeaponUnequipped);

        // Bind to whatever is already equipped at startup.
        RefreshTrackedWeapon();
    }

    private void OnDestroy()
    {
        if (shooterManager != null)
        {
            shooterManager.onEquipWeapon.RemoveListener(OnWeaponEquipped);
            shooterManager.onUnequipWeapon.RemoveListener(OnWeaponUnequipped);
        }

        if (ammoManager != null)
            ammoManager.updateTotalAmmo -= Refresh;
    }

    // ── Weapon tracking ───────────────────────────────────────────────────────

    private void OnWeaponEquipped(vShooterWeapon weapon, bool isLeftWeapon)
    {
        RefreshTrackedWeapon();
    }

    private void OnWeaponUnequipped(vShooterWeapon weapon, bool isLeftWeapon)
    {
        RefreshTrackedWeapon();
    }

    private void RefreshTrackedWeapon()
    {
        // Prefer the right-hand weapon; fall back to left.
        trackedWeapon = shooterManager.rWeapon != null ? shooterManager.rWeapon : shooterManager.lWeapon;

        if (trackedWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        Refresh();
    }

    // ── Display ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads the tracked weapon's ammo state and updates both text fields.
    /// </summary>
    public void Refresh()
    {
        if (trackedWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        if (trackedWeapon.isInfinityAmmo)
        {
            SetMagazineText($"{trackedWeapon.ammo} / {trackedWeapon.clipSize}");
            SetReserveText(InfiniteText);
            return;
        }

        int inClip   = trackedWeapon.ammo;
        int clipSize = trackedWeapon.clipSize;
        int reserve  = 0;

        if (ammoManager != null)
        {
            vAmmo ammoEntry = ammoManager.GetAmmo(trackedWeapon.ammoID);
            if (ammoEntry != null)
                reserve = ammoEntry.count;
        }

        SetMagazineText($"{inClip} / {clipSize}");
        SetReserveText(reserve.ToString());
    }

    private void ShowNoWeapon()
    {
        SetMagazineText(noWeaponText);
        SetReserveText(noWeaponText);
    }

    private void SetMagazineText(string value)
    {
        if (magazineText != null) magazineText.text = value;
    }

    private void SetReserveText(string value)
    {
        if (reserveText != null) reserveText.text = value;
    }
}
