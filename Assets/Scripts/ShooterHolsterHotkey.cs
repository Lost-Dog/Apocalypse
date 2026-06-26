using System.Threading.Tasks;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class ShooterHolsterHotkey : MonoBehaviour
{
    [System.Serializable]
    private struct WeaponItemBinding
    {
        public ShooterWeapon weapon;
        public Item inventoryItem;
    }

    private static readonly Dictionary<int, ShooterHolsterHotkey> ACTIVE_BY_CHARACTER = new Dictionary<int, ShooterHolsterHotkey>();

    [SerializeField] private Character gcCharacter;
    [SerializeField] private KeyCode holsterKey = KeyCode.F2;
    [SerializeField] private bool enableGamepadFaceButton = true;
    [SerializeField] private KeyCode gamepadFaceButton = KeyCode.JoystickButton3;
    [SerializeField] private bool enableGamepadDpadUp = true;
    [SerializeField] private KeyCode gamepadDpadUpButton = KeyCode.JoystickButton13;
    [SerializeField] private string gamepadDpadVerticalAxis = "DPadY";
    [SerializeField, Range(0.1f, 1f)] private float gamepadDpadPressThreshold = 0.5f;
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

    [Header("Inventory Transfer")]
    [SerializeField] private bool transferMappedWeaponsToInventory = true;
    [SerializeField] private bool requireMappedItemInBagForUnholster;
    [SerializeField] private Bag fallbackPlayerBag;
    [SerializeField] private WeaponItemBinding[] weaponItemBindings = new WeaponItemBinding[0];

    [Header("Equip Behavior")]
    [SerializeField] private bool enforceSingleShooterWeapon = true;
    [SerializeField] private bool hideStaleWeaponInstances = true;

    private ShooterWeapon cachedWeapon;
    private GameObject cachedModel;
    private bool isTransitioning;
    private bool isResolvingEquipStack;
    private string debugOverlayText;
    private float debugOverlayUntil;
    private bool wasDpadUpHeld;
    private bool dpadAxisUnavailableLogged;
    private bool cachedWeaponStoredInInventory;

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

        if (!IsHolsterTogglePressed())
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
        cachedWeaponStoredInInventory = false;

        if (enforceSingleShooterWeapon)
            _ = EnforceSingleShooterWeaponAsync(shooterWeapon, cachedModel);
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
            return gcCharacter.GetEntityId().GetHashCode();

        return gameObject != null ? gameObject.GetEntityId().GetHashCode() : 0;
    }

    private bool TryHolsterActiveWeapon()
    {
        ShooterWeapon weapon = gcCharacter.Combat.GetActiveWeapon<ShooterWeapon>();
        if (weapon == null)
            return false;

        cachedWeapon = weapon;
        cachedModel = gcCharacter.Combat.GetProp(weapon);
        cachedWeaponStoredInInventory = TryStoreWeaponInInventory(weapon, out Item mappedItem);

        _ = HolsterWeaponAsync(weapon);

        if (mappedItem != null)
        {
            ShowDebug(cachedWeaponStoredInInventory
                ? $"Holstered: {weapon.name} (stored in inventory)"
                : $"Holstered: {weapon.name} (inventory store failed)",
                isWarning: !cachedWeaponStoredInInventory
            );
        }
        else
        {
            ShowDebug($"Holstered: {weapon.name}");
        }

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

        if (!TryConsumeWeaponFromInventory(weaponToEquip, out _))
            return;

        isTransitioning = true;

        try
        {
            await gcCharacter.Combat.Equip(weaponToEquip, modelToEquip, new Args(gcCharacter.gameObject, modelToEquip));

            cachedWeapon = weaponToEquip;
            cachedModel = modelToEquip;
            cachedWeaponStoredInInventory = false;
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

    private bool IsHolsterTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (WasKeyPressedThisFrame(holsterKey))
            return true;

        if (enableGamepadFaceButton && WasGamepadButtonPressedThisFrame(GetConfiguredGamepadFaceButton()))
            return true;

        if (!enableGamepadDpadUp)
            return false;

        if (WasGamepadButtonPressedThisFrame(gamepadDpadUpButton))
            return true;

        if (Gamepad.current == null)
            return false;

        bool isHeld = Gamepad.current.dpad.up.isPressed;
        bool wasPressedThisFrame = Gamepad.current.dpad.up.wasPressedThisFrame || (isHeld && !wasDpadUpHeld);
        wasDpadUpHeld = isHeld;

        return wasPressedThisFrame;
#else
        if (Input.GetKeyDown(holsterKey))
            return true;

        if (enableGamepadFaceButton && Input.GetKeyDown(GetConfiguredGamepadFaceButton()))
            return true;

        if (!enableGamepadDpadUp)
            return false;

        if (Input.GetKeyDown(gamepadDpadUpButton))
            return true;

        if (string.IsNullOrWhiteSpace(gamepadDpadVerticalAxis))
            return false;

        float axis;
        try
        {
            axis = Input.GetAxisRaw(gamepadDpadVerticalAxis);
        }
        catch (UnityException)
        {
            if (!dpadAxisUnavailableLogged)
            {
                dpadAxisUnavailableLogged = true;
                ShowDebug($"D-pad axis '{gamepadDpadVerticalAxis}' is not configured. Using button fallback only.", isWarning: true);
            }

            return false;
        }

        bool isHeld = axis >= gamepadDpadPressThreshold;
        bool wasPressedThisFrame = isHeld && !wasDpadUpHeld;
        wasDpadUpHeld = isHeld;

        return wasPressedThisFrame;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static bool WasKeyPressedThisFrame(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Mouse0:
                return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            case KeyCode.Mouse1:
                return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            case KeyCode.Mouse2:
                return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
            default:
                if (Keyboard.current == null)
                    return false;

                Key inputKey = ToInputSystemKey(key);
                return inputKey != Key.None && Keyboard.current[inputKey].wasPressedThisFrame;
        }
    }

    private static bool WasGamepadButtonPressedThisFrame(KeyCode key)
    {
        if (Gamepad.current == null)
            return false;

        switch (key)
        {
            case KeyCode.JoystickButton0:
                return Gamepad.current.buttonSouth.wasPressedThisFrame;
            case KeyCode.JoystickButton1:
                return Gamepad.current.buttonEast.wasPressedThisFrame;
            case KeyCode.JoystickButton2:
                return Gamepad.current.buttonWest.wasPressedThisFrame;
            case KeyCode.JoystickButton3:
                return Gamepad.current.buttonNorth.wasPressedThisFrame;
            case KeyCode.JoystickButton4:
                return Gamepad.current.leftShoulder.wasPressedThisFrame;
            case KeyCode.JoystickButton5:
                return Gamepad.current.rightShoulder.wasPressedThisFrame;
            case KeyCode.JoystickButton6:
                return Gamepad.current.selectButton.wasPressedThisFrame;
            case KeyCode.JoystickButton7:
                return Gamepad.current.startButton.wasPressedThisFrame;
            case KeyCode.JoystickButton8:
                return Gamepad.current.leftStickButton.wasPressedThisFrame;
            case KeyCode.JoystickButton9:
                return Gamepad.current.rightStickButton.wasPressedThisFrame;
            case KeyCode.JoystickButton13:
                return Gamepad.current.dpad.up.wasPressedThisFrame;
            case KeyCode.JoystickButton14:
                return Gamepad.current.dpad.right.wasPressedThisFrame;
            case KeyCode.JoystickButton15:
                return Gamepad.current.dpad.down.wasPressedThisFrame;
            case KeyCode.JoystickButton16:
                return Gamepad.current.dpad.left.wasPressedThisFrame;
            default:
                return false;
        }
    }

    private static Key ToInputSystemKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.None: return Key.None;
            case KeyCode.Space: return Key.Space;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.BackQuote: return Key.Backquote;
            case KeyCode.Quote: return Key.Quote;
            case KeyCode.Semicolon: return Key.Semicolon;
            case KeyCode.Comma: return Key.Comma;
            case KeyCode.Period: return Key.Period;
            case KeyCode.Slash: return Key.Slash;
            case KeyCode.Backslash: return Key.Backslash;
            case KeyCode.LeftBracket: return Key.LeftBracket;
            case KeyCode.RightBracket: return Key.RightBracket;
            case KeyCode.Minus: return Key.Minus;
            case KeyCode.Equals: return Key.Equals;
            case KeyCode.A: return Key.A;
            case KeyCode.B: return Key.B;
            case KeyCode.C: return Key.C;
            case KeyCode.D: return Key.D;
            case KeyCode.E: return Key.E;
            case KeyCode.F: return Key.F;
            case KeyCode.G: return Key.G;
            case KeyCode.H: return Key.H;
            case KeyCode.I: return Key.I;
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.L: return Key.L;
            case KeyCode.M: return Key.M;
            case KeyCode.N: return Key.N;
            case KeyCode.O: return Key.O;
            case KeyCode.P: return Key.P;
            case KeyCode.Q: return Key.Q;
            case KeyCode.R: return Key.R;
            case KeyCode.S: return Key.S;
            case KeyCode.T: return Key.T;
            case KeyCode.U: return Key.U;
            case KeyCode.V: return Key.V;
            case KeyCode.W: return Key.W;
            case KeyCode.X: return Key.X;
            case KeyCode.Y: return Key.Y;
            case KeyCode.Z: return Key.Z;
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Alpha8: return Key.Digit8;
            case KeyCode.Alpha9: return Key.Digit9;
            case KeyCode.Alpha0: return Key.Digit0;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftAlt: return Key.LeftAlt;
            case KeyCode.RightAlt: return Key.RightAlt;
            case KeyCode.AltGr: return Key.AltGr;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightControl: return Key.RightCtrl;
            case KeyCode.LeftWindows: return Key.LeftWindows;
            case KeyCode.RightWindows: return Key.RightWindows;
            case KeyCode.LeftCommand: return Key.LeftCommand;
            case KeyCode.RightCommand: return Key.RightCommand;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            case KeyCode.Backspace: return Key.Backspace;
            case KeyCode.PageDown: return Key.PageDown;
            case KeyCode.PageUp: return Key.PageUp;
            case KeyCode.Home: return Key.Home;
            case KeyCode.End: return Key.End;
            case KeyCode.Insert: return Key.Insert;
            case KeyCode.Delete: return Key.Delete;
            case KeyCode.CapsLock: return Key.CapsLock;
            case KeyCode.Numlock: return Key.NumLock;
            case KeyCode.Print: return Key.PrintScreen;
            case KeyCode.ScrollLock: return Key.ScrollLock;
            case KeyCode.Pause: return Key.Pause;
            case KeyCode.KeypadEnter: return Key.NumpadEnter;
            case KeyCode.KeypadDivide: return Key.NumpadDivide;
            case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
            case KeyCode.KeypadPlus: return Key.NumpadPlus;
            case KeyCode.KeypadMinus: return Key.NumpadMinus;
            case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
            case KeyCode.KeypadEquals: return Key.NumpadEquals;
            case KeyCode.Keypad0: return Key.Numpad0;
            case KeyCode.Keypad1: return Key.Numpad1;
            case KeyCode.Keypad2: return Key.Numpad2;
            case KeyCode.Keypad3: return Key.Numpad3;
            case KeyCode.Keypad4: return Key.Numpad4;
            case KeyCode.Keypad5: return Key.Numpad5;
            case KeyCode.Keypad6: return Key.Numpad6;
            case KeyCode.Keypad7: return Key.Numpad7;
            case KeyCode.Keypad8: return Key.Numpad8;
            case KeyCode.Keypad9: return Key.Numpad9;
            case KeyCode.F1: return Key.F1;
            case KeyCode.F2: return Key.F2;
            case KeyCode.F3: return Key.F3;
            case KeyCode.F4: return Key.F4;
            case KeyCode.F5: return Key.F5;
            case KeyCode.F6: return Key.F6;
            case KeyCode.F7: return Key.F7;
            case KeyCode.F8: return Key.F8;
            case KeyCode.F9: return Key.F9;
            case KeyCode.F10: return Key.F10;
            case KeyCode.F11: return Key.F11;
            case KeyCode.F12: return Key.F12;
            default: return Key.None;
        }
    }
#endif

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

    private KeyCode GetConfiguredGamepadFaceButton()
    {
        return gamepadFaceButton != KeyCode.None
            ? gamepadFaceButton
            : KeyCode.JoystickButton3;
    }

    private Item ResolveInventoryItem(ShooterWeapon weapon)
    {
        if (weapon == null)
            return null;

        for (int i = 0; i < weaponItemBindings.Length; i++)
        {
            if (weaponItemBindings[i].weapon != weapon)
                continue;

            return weaponItemBindings[i].inventoryItem;
        }

        return null;
    }

    private Bag ResolvePlayerBag()
    {
        if (gcCharacter != null)
        {
            Bag onCharacter = gcCharacter.GetComponent<Bag>();
            if (onCharacter != null) return onCharacter;

            Bag inChildren = gcCharacter.GetComponentInChildren<Bag>(true);
            if (inChildren != null) return inChildren;
        }

        if (fallbackPlayerBag != null)
            return fallbackPlayerBag;

        return FindFirstObjectByType<Bag>();
    }

    private bool TryStoreWeaponInInventory(ShooterWeapon weapon, out Item inventoryItem)
    {
        inventoryItem = ResolveInventoryItem(weapon);

        if (!transferMappedWeaponsToInventory || inventoryItem == null)
            return false;

        Bag bag = ResolvePlayerBag();
        if (bag == null)
        {
            ShowDebug("No GC2 Bag found. Weapon item was not stored.", isWarning: true);
            return false;
        }

        if (!bag.Content.CanAddType(inventoryItem, true))
        {
            ShowDebug($"No inventory space for '{inventoryItem.name}'.", isWarning: true);
            return false;
        }

        RuntimeItem added = bag.Content.AddType(inventoryItem, true);
        return added != null;
    }

    private bool TryConsumeWeaponFromInventory(ShooterWeapon weapon, out Item inventoryItem)
    {
        inventoryItem = ResolveInventoryItem(weapon);

        if (!transferMappedWeaponsToInventory || inventoryItem == null)
            return true;

        Bag bag = ResolvePlayerBag();
        if (bag == null)
        {
            ShowDebug("No GC2 Bag found. Cannot remove mapped weapon item.", isWarning: true);
            return !requireMappedItemInBagForUnholster;
        }

        if (!bag.Content.ContainsType(inventoryItem, 1))
        {
            ShowDebug($"Inventory missing '{inventoryItem.name}' for unholster.", isWarning: true);
            return !requireMappedItemInBagForUnholster;
        }

        RuntimeItem removed = bag.Content.RemoveType(inventoryItem);
        if (removed == null)
        {
            ShowDebug($"Failed to remove '{inventoryItem.name}' from inventory.", isWarning: true);
            return !requireMappedItemInBagForUnholster;
        }

        return true;
    }

    private async Task EnforceSingleShooterWeaponAsync(ShooterWeapon keepWeapon, GameObject keepInstance)
    {
        if (gcCharacter == null || keepWeapon == null)
            return;

        if (isResolvingEquipStack)
            return;

        isResolvingEquipStack = true;

        try
        {
            Weapon[] equippedWeapons = gcCharacter.Combat.Weapons;
            for (int i = 0; i < equippedWeapons.Length; i++)
            {
                IWeapon asset = equippedWeapons[i].Asset;
                if (asset is not ShooterWeapon shooterWeapon)
                    continue;

                if (ReferenceEquals(shooterWeapon, keepWeapon))
                    continue;

                GameObject staleInstance = equippedWeapons[i].Instance;
                ForceResetShooterStance(shooterWeapon);
                await gcCharacter.Combat.Unequip(shooterWeapon, new Args(gcCharacter.gameObject));

                HideStaleWeaponInstance(staleInstance, keepInstance);
                ShowDebug($"Unequipped stacked weapon: {shooterWeapon.name}");
            }
        }
        finally
        {
            isResolvingEquipStack = false;
        }
    }

    private void HideStaleWeaponInstance(GameObject staleInstance, GameObject keepInstance)
    {
        if (!hideStaleWeaponInstances)
            return;

        if (staleInstance == null || staleInstance == keepInstance)
            return;

        if (!staleInstance.scene.IsValid())
            return;

        Transform characterRoot = gcCharacter != null ? gcCharacter.transform : null;
        if (characterRoot == null)
            return;

        if (!staleInstance.transform.IsChildOf(characterRoot))
            return;

        staleInstance.SetActive(false);
        ShowDebug($"Hid stale weapon instance: {staleInstance.name}");
    }
}