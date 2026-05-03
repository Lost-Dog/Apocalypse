using UnityEngine;
using TMPro;
using JUTPS;
using JUTPS.WeaponSystem;
using JUTPS.InventorySystem;

/// <summary>
/// Displays the held weapon's ammo on two separate TextMeshProUGUI elements:
///   Magazine Text  →  "12 / 30"  (bullets in magazine / capacity)
///   Reserve Text   →  "90"       (remaining bullets outside the magazine)
/// </summary>
public class AmmoDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI magazineText;
    [SerializeField] private TextMeshProUGUI reserveText;

    [Header("Settings")]
    [Tooltip("Text shown when no weapon is equipped.")]
    [SerializeField] private string noWeaponText = "--";
    [Tooltip("Update rate in seconds. 0 = every frame.")]
    [SerializeField] private float updateInterval = 0.05f;

    private JUInventory inventory;
    private float updateTimer;

    private void Start()
    {
        JUCharacterController player = FindAnyObjectByType<JUCharacterController>();
        if (player != null)
        {
            inventory = player.GetComponent<JUInventory>();
        }

        if (inventory == null)
        {
            Debug.LogWarning("[AmmoDisplay] Could not find JUInventory on the player.");
        }

        SetMagazineText(noWeaponText);
        SetReserveText(noWeaponText);
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateInterval > 0f && updateTimer < updateInterval)
            return;

        updateTimer = 0f;
        Refresh();
    }

    /// <summary>
    /// Reads the active weapon's ammo and updates both text fields.
    /// </summary>
    public void Refresh()
    {
        if (inventory == null)
        {
            SetMagazineText(noWeaponText);
            SetReserveText(noWeaponText);
            return;
        }

        Weapon weapon = inventory.WeaponInUseInRightHand ?? inventory.WeaponInUseInLeftHand;

        if (weapon == null)
        {
            SetMagazineText(noWeaponText);
            SetReserveText(noWeaponText);
            return;
        }

        SetMagazineText($"{weapon.BulletsAmounts} / {weapon.BulletsPerMagazine}");
        SetReserveText($"{weapon.TotalBullets}");
    }

    private void SetMagazineText(string value)
    {
        if (magazineText != null)
            magazineText.text = value;
    }

    private void SetReserveText(string value)
    {
        if (reserveText != null)
            reserveText.text = value;
    }
}
