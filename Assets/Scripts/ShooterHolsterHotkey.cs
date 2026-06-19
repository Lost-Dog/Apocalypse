using System.Threading.Tasks;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Shooter;
using UnityEngine;

[DisallowMultipleComponent]
public class ShooterHolsterHotkey : MonoBehaviour
{
    private static readonly Dictionary<int, ShooterHolsterHotkey> ACTIVE_BY_CHARACTER = new Dictionary<int, ShooterHolsterHotkey>();

    [SerializeField] private Character gcCharacter;
    [SerializeField] private KeyCode holsterKey = KeyCode.F2;
    [SerializeField] private bool toggleBackOnSecondPress = true;
    [SerializeField] private bool autoResolveCharacter = true;

    [Header("Keybind Persistence")]
    [SerializeField] private bool loadKeybindFromPrefs = true;
    [SerializeField] private bool saveKeybindToPrefs = true;
    [SerializeField] private string keybindPrefsKey = "ShooterHolsterHotkey.HolsterKey";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool enableDebugOverlay = true;
    [SerializeField, Min(0.5f)] private float debugOverlayDuration = 1.5f;

    [Header("Default Fallback")]
    [Tooltip("Used when no recent shooter weapon is cached. Set this to your default pistol ShooterWeapon.")]
    [SerializeField] private ShooterWeapon defaultPistolWeapon;
    [Tooltip("Optional model prop for the default pistol. If empty, the script tries Character.Combat.GetProp(defaultPistolWeapon).")]
    [SerializeField] private GameObject defaultPistolModel;

    private ShooterWeapon cachedWeapon;
    private GameObject cachedModel;
    private bool isTransitioning;
    private string debugOverlayText;
    private float debugOverlayUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttachToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (player.GetComponent<ShooterHolsterHotkey>() == null)
            player.AddComponent<ShooterHolsterHotkey>();
    }

    private void Awake()
    {
        ResolveCharacter();
        LoadPersistedKeybind();
    }

    private void OnEnable()
    {
        ResolveCharacter();

        if (!TryRegisterAsPrimary())
            return;

        if (gcCharacter == null) return;

        gcCharacter.Combat.EventEquip -= OnEquip;
        gcCharacter.Combat.EventEquip += OnEquip;
    }

    private void OnDisable()
    {
        UnregisterPrimary();

        if (gcCharacter == null) return;

        gcCharacter.Combat.EventEquip -= OnEquip;
    }

    private void Update()
    {
        if (isTransitioning || gcCharacter == null)
            return;

        if (!Input.GetKeyDown(holsterKey))
            return;

        if (TryHolsterActiveWeapon())
            return;

        if (toggleBackOnSecondPress)
            _ = TryReequipWeaponOrDefault();
        else
            ShowDebug("No active weapon to holster.");
    }

    private void OnGUI()
    {
        if (!enableDebugOverlay)
            return;

        if (Time.unscaledTime > debugOverlayUntil || string.IsNullOrEmpty(debugOverlayText))
            return;

        GUI.Box(new Rect(10f, 10f, 420f, 28f), debugOverlayText);
    }

    private void OnEquip(IWeapon weapon, GameObject instance)
    {
        if (weapon is not ShooterWeapon shooterWeapon)
            return;

        cachedWeapon = shooterWeapon;
        cachedModel = instance != null ? instance : gcCharacter.Combat.GetProp(shooterWeapon);
    }

    private void ResolveCharacter()
    {
        if (gcCharacter != null || !autoResolveCharacter)
            return;

        gcCharacter = GetComponent<Character>();
        if (gcCharacter == null)
            gcCharacter = GetComponentInChildren<Character>(true);
    }

    private bool TryRegisterAsPrimary()
    {
        int key = GetOwnershipKey();
        if (key == 0)
            return true;

        if (ACTIVE_BY_CHARACTER.TryGetValue(key, out ShooterHolsterHotkey existing) && existing != null && existing != this)
        {
            ShowDebug($"Disabled duplicate listener on '{name}'. Primary listener is on '{existing.name}'.", isWarning: true);
            enabled = false;
            return false;
        }

        ACTIVE_BY_CHARACTER[key] = this;
        return true;
    }

    private void UnregisterPrimary()
    {
        int key = GetOwnershipKey();
        if (key == 0)
            return;

        if (ACTIVE_BY_CHARACTER.TryGetValue(key, out ShooterHolsterHotkey existing) && existing == this)
            ACTIVE_BY_CHARACTER.Remove(key);
    }

    private int GetOwnershipKey()
    {
        if (gcCharacter != null)
            return gcCharacter.GetInstanceID();

        return gameObject != null ? gameObject.GetInstanceID() : 0;
    }

    private bool TryHolsterActiveWeapon()
    {
        ShooterWeapon weapon = gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>();
        if (weapon == null)
            return false;

        cachedWeapon = weapon;
        cachedModel = gcCharacter.Combat.GetProp(weapon);

        _ = HolsterWeaponAsync(weapon);
        ShowDebug($"Holstered: {weapon.name}");
        return true;
    }

    private async Task HolsterWeaponAsync(ShooterWeapon weapon)
    {
        if (gcCharacter == null || weapon == null)
            return;

        isTransitioning = true;

        try
        {
            ForceResetShooterStance(weapon);
            await gcCharacter.Combat.Unequip(weapon, new Args(gcCharacter.gameObject));
        }
        finally
        {
            isTransitioning = false;
        }
    }

    private void ForceResetShooterStance(ShooterWeapon weapon)
    {
        if (gcCharacter == null || weapon == null)
            return;

        ShooterStance stance = gcCharacter.Combat.RequestStance<ShooterStance>();
        if (stance == null)
            return;

        // Ensure ADS/trigger/reload states are fully released before unequip.
        stance.ExitSight(weapon);
        stance.ReleaseTrigger(weapon);
        stance.CancelTrigger(weapon);
        stance.StopReload(weapon, CancelReason.ForceStop);
    }

    private async Task TryReequipWeaponOrDefault()
    {
        if (gcCharacter == null)
            return;

        ShooterWeapon weaponToEquip = cachedWeapon != null ? cachedWeapon : defaultPistolWeapon;
        GameObject modelToEquip = cachedModel;

        if (weaponToEquip == null)
            return;

        if (modelToEquip == null)
            modelToEquip = gcCharacter.Combat.GetProp(weaponToEquip);

        if (modelToEquip == null && weaponToEquip == defaultPistolWeapon)
            modelToEquip = defaultPistolModel;

        if (modelToEquip == null)
        {
            ShowDebug(
                "Could not resolve model for equip. Assign default model or ensure Combat prop exists.",
                isWarning: true
            );
            return;
        }

        if (gcCharacter.Combat.IsEquipped(weaponToEquip))
            return;

        isTransitioning = true;

        try
        {
            await gcCharacter.Combat.Equip(weaponToEquip, modelToEquip, new Args(gcCharacter.gameObject, modelToEquip));

            cachedWeapon = weaponToEquip;
            cachedModel = modelToEquip;
            ShowDebug($"Equipped: {weaponToEquip.name}");
        }
        finally
        {
            isTransitioning = false;
        }
    }

    public void SetHolsterKey(KeyCode newKey, bool persist = true)
    {
        holsterKey = newKey;

        if (persist)
            PersistKeybind();

        ShowDebug($"Holster key set to {holsterKey}");
    }

    private void LoadPersistedKeybind()
    {
        if (!loadKeybindFromPrefs)
            return;

        if (string.IsNullOrWhiteSpace(keybindPrefsKey))
            return;

        if (!PlayerPrefs.HasKey(keybindPrefsKey))
            return;

        string persisted = PlayerPrefs.GetString(keybindPrefsKey, holsterKey.ToString());
        if (System.Enum.TryParse(persisted, out KeyCode parsedKey))
        {
            holsterKey = parsedKey;
            ShowDebug($"Loaded holster key: {holsterKey}");
        }
    }

    private void PersistKeybind()
    {
        if (!saveKeybindToPrefs)
            return;

        if (string.IsNullOrWhiteSpace(keybindPrefsKey))
            return;

        PlayerPrefs.SetString(keybindPrefsKey, holsterKey.ToString());
        PlayerPrefs.Save();
    }

    private void ShowDebug(string message, bool isWarning = false)
    {
        if (enableDebugLogs)
        {
            string finalMessage = $"[ShooterHolsterHotkey] {message}";
            if (isWarning) Debug.LogWarning(finalMessage);
            else Debug.Log(finalMessage);
        }

        if (enableDebugOverlay)
        {
            debugOverlayText = message;
            debugOverlayUntil = Time.unscaledTime + debugOverlayDuration;
        }
    }
}