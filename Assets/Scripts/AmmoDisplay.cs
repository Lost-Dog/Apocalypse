using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Shooter;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the active GC2 ShooterWeapon ammo on two TextMeshProUGUI elements:
///   Magazine Text  →  "12 / 30"  (rounds in magazine / magazine capacity)
///   Reserve Text   →  "90"       (total remaining rounds outside the magazine)
/// Reacts to munition changes via events; polls on weapon equip/unequip to rebind.
/// </summary>
public class AmmoDisplay : MonoBehaviour
{
    private const string InfiniteText = "∞";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI magazineText;
    [SerializeField] private TextMeshProUGUI reserveText;

    [Header("Settings")]
    [Tooltip("Text shown when no weapon is equipped.")]
    [SerializeField] private string noWeaponText = "--";

    private Character playerCharacter;
    private ShooterWeapon trackedWeapon;
    private ShooterMunition trackedMunition;
    private Args args;

    private void Start()
    {
        playerCharacter = ShortcutPlayer.Get<Character>();

        if (playerCharacter == null)
        {
            Debug.LogWarning("[AmmoDisplay] Could not find the player Character.");
            ShowNoWeapon();
            return;
        }

        args = new Args(playerCharacter.gameObject);

        playerCharacter.Combat.EventEquip += OnWeaponEquipped;
        playerCharacter.Combat.EventUnequip += OnWeaponUnequipped;

        // Bind to whatever is already equipped at startup
        RefreshTrackedWeapon();
    }

    private void OnDestroy()
    {
        if (playerCharacter != null)
        {
            playerCharacter.Combat.EventEquip -= OnWeaponEquipped;
            playerCharacter.Combat.EventUnequip -= OnWeaponUnequipped;
        }

        UnsubscribeMunition();
    }

    // WEAPON TRACKING: ---------------------------------------------------------------------------

    private void OnWeaponEquipped(IWeapon weapon, GameObject prop)
    {
        if (weapon is ShooterWeapon) RefreshTrackedWeapon();
    }

    private void OnWeaponUnequipped(IWeapon weapon, GameObject prop)
    {
        if (weapon is ShooterWeapon) RefreshTrackedWeapon();
    }

    private void RefreshTrackedWeapon()
    {
        UnsubscribeMunition();

        trackedWeapon = playerCharacter.Combat.GetActiveWeapon<ShooterWeapon>();

        if (trackedWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        trackedMunition = playerCharacter.Combat.RequestMunition(trackedWeapon) as ShooterMunition;
        if (trackedMunition != null)
        {
            trackedMunition.EventChange += Refresh;
        }

        Refresh();
    }

    private void UnsubscribeMunition()
    {
        if (trackedMunition != null)
        {
            trackedMunition.EventChange -= Refresh;
            trackedMunition = null;
        }

        trackedWeapon = null;
    }

    // DISPLAY: -----------------------------------------------------------------------------------

    /// <summary>
    /// Reads the active weapon's ammo counts and updates both text fields.
    /// </summary>
    public void Refresh()
    {
        if (trackedWeapon == null || trackedMunition == null)
        {
            ShowNoWeapon();
            return;
        }

        int inMagazine = trackedMunition.InMagazine;
        int magazineSize = trackedWeapon.Magazine.GetHasMagazine(args)
            ? trackedWeapon.Magazine.GetMagazineSize(args)
            : 0;

        int total = trackedWeapon.Magazine.GetTotalAmmo(args);
        bool isInfinite = total >= int.MaxValue;

        int reserve = isInfinite ? int.MaxValue : Mathf.Max(0, total - inMagazine);

        SetMagazineText($"{inMagazine} / {magazineSize}");
        SetReserveText(isInfinite ? InfiniteText : reserve.ToString());
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
