using System.Collections.Generic;
using System.Reflection;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using Invector.vShooter;
using Invector.vCover;
using Invector.Throw;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bridges Unity's new Input System to Invector's vThirdPersonInput / vShooterMeleeInput.
///
/// HOW IT WORKS:
///   • Suppresses Invector's legacy GenericInput polling by setting lockInput = true,
///     lockMeleeInput = true, and calling SetLockShooterInput(true) on Start.
///   • Subscribes to vThirdPersonInput.onUpdate / onLateUpdate to inject movement,
///     camera, and action inputs directly into the controller each frame.
///   • All bindings are defined in code — no .inputactions asset required.
///
/// SETUP:
///   1. Add this component to the same GameObject as vShooterMeleeInput.
///   2. Optionally assign mouseSensitivity and gamepadSensitivity in the Inspector.
///   3. The bridge auto-finds all Invector components on the same GameObject.
/// </summary>
// Run after vShooterMeleeInput (default order 0) so our Update() fires after InputHandle()
// has already wiped isAimingByInput. This lets us overwrite aim/fire state as the last word.
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(vThirdPersonInput))]
public class InvectorInputBridge : MonoBehaviour
{
    // ── Sensitivity ───────────────────────────────────────────────────────────

    [Header("Mouse & Gamepad Sensitivity")]
    [Tooltip("Multiplier applied to raw mouse delta for camera rotation.")]
    public float mouseSensitivity = 1.5f;

    [Tooltip("Camera rotation speed at full gamepad right-stick deflection, in degrees per second. " +
             "Increase until gamepad feel matches mouse at your preferred mouse sensitivity.")]
    public float gamepadDegreesPerSecond = 180f;

    [Header("Gamepad Feel")]
    [Tooltip("Smoothing factor for gamepad camera rotation. Higher = more responsive but more overshoot. " +
             "Lower = smoother but slightly laggy. 0 = instant (no smoothing).")]
    [Range(0f, 30f)]
    public float gamepadLookSmoothing = 12f;

    [Tooltip("Radial dead zone applied to the left stick before writing movement input. " +
             "Values below this magnitude are treated as zero, eliminating stick drift and jitter.")]
    [Range(0f, 0.3f)]
    public float gamepadMoveDeadZone = 0.12f;

    [Tooltip("Radial dead zone applied to the right stick before writing camera rotation. " +
             "Values below this magnitude are treated as zero.")]
    [Range(0f, 0.3f)]
    public float gamepadLookDeadZone = 0.1f;

    [Header("Aim Assist Smoothing")]
    [Tooltip("Screen-space dead zone in pixels. Correction is skipped when the target is already " +
             "within this distance from the crosshair, preventing micro-oscillation at close range.")]
    public float aimAssistDeadZonePx = 12f;

    [Tooltip("Maximum angular correction applied per second (degrees). Caps the assist speed so " +
             "it cannot overshoot the target and cause wiggling.")]
    public float aimAssistMaxDegPerSec = 8f;

    [Header("Debug")]
    public bool debugMode = false;

    [Header("Input Asset")]
    [Tooltip("Assign the InvectorInputActions asset (renamed from .txt to .inputactions) to " +
             "control all bindings from the Input Action editor. Leave empty to use the " +
             "built-in code-defined bindings as a fallback.")]
    [SerializeField] private InputActionAsset _inputActionAsset;

    // ── Input Actions (built in code) ─────────────────────────────────────────

    // Locomotion
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _zoomAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;
    private InputAction _strafeAction;
    private InputAction _rollAction;
    private InputAction _toggleWalkAction;

    // Shooter
    private InputAction _aimAction;
    private InputAction _fireAction;
    private InputAction _reloadAction;
    private InputAction _switchCamSideAction;
    private InputAction _scopeViewAction;

    // Melee
    private InputAction _weakAttackAction;
    private InputAction _strongAttackAction;
    private InputAction _blockAction;

    // Inventory & weapon slots
    private InputAction _openInventoryAction;
    private InputAction _removeEquipmentAction;
    private InputAction _holsterAction;
    private InputAction _coverAction;
    private InputAction _interactAction;  // generic action / parkour interaction
    // Grenade / throwable
    private InputAction _throwAimAction;  // Numpad2 — equip & aim grenade toggle
    private InputAction _throwReleaseAction; // LMB / RT — throw while aiming
    // Per-slot cycling: index matches changeEquipmentControllers order
    private readonly List<InputAction> _prevSlotActions = new List<InputAction>();
    private readonly List<InputAction> _nextSlotActions = new List<InputAction>();
    private readonly List<InputAction> _useItemActions   = new List<InputAction>();

    private InputActionMap _actionMap;
    private InputActionAsset _clonedAsset; // non-null when loaded from an assigned asset

    // ── Invector references ───────────────────────────────────────────────────

    private vThirdPersonInput   _tpInput;
    private vMeleeCombatInput   _meleeInput;
    private vShooterMeleeInput  _shooterInput;
    private vInventory          _inventory;
    private vCoverController    _coverController;
    private vGenericAction      _genericAction;
    private Invector.vShooter.vDrawHideShooterWeapons _drawHideWeapons;
    private CameraAimAssistentHeadTracking _aimAssist;
    private vThrowManagerBase   _throwManager;

    // Per-frame state fed by performed/cancelled callbacks (thread-safe for main thread use)
    private Vector2 _moveValue;
    private Vector2 _lookValue;
    private Vector2 _smoothedLookValue;
    private float   _zoomValue;

    private bool _sprintHeld;
    private bool _aimHeld;
    private bool _fireHeld;
    private bool _blockHeld;

    // Consumed-per-frame flags (set by performed callback, cleared after processing)
    private bool _jumpPressed;
    private bool _jumpPressedGamepad;   // tracks whether the last jump press came from gamepad
    private bool _crouchPressed;
    private bool _strafePressed;
    private bool _rollPressed;
    private bool _toggleWalkPressed;
    private bool _reloadPressed;
    private bool _switchCamSidePressed;
    private bool _scopeViewPressed;
    private bool _weakAttackPressed;
    private bool _weakAttackDown;
    private bool _strongAttackDown;
    private bool _strongAttackHeld;

    private bool _prevWantsAim;  // tracks last frame's resolved aim state for aim-assist transitions

    // Inventory flags
    private bool _openInventoryPressed;
    private bool _removeEquipmentPressed;
    private bool _holsterPressed;           // draw/hide weapon toggle (X / gamepad buttonWest)
    private bool _coverHeld;                // held state for hold-to-exit-cover logic
    private bool _interactPressed;          // generic action / parkour trigger (E / buttonNorth)
    // Throw / grenade
    private bool _throwAimPressed;          // toggle aim mode (Numpad2)
    private bool _throwReleasePressed;      // launch grenade while aiming (LMB / RT)
    private readonly List<bool> _prevSlotPressed = new List<bool>();
    private readonly List<bool> _nextSlotPressed = new List<bool>();
    private readonly List<bool> _useItemPressed  = new List<bool>();

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (_inputActionAsset != null)
            LoadActionsFromAsset();
        else
            BuildActions();
    }

    private void Start()
    {
        CacheInvectorComponents();
        SuppressLegacyInput();

        _tpInput.onUpdate     += HandleUpdate;
        _tpInput.onLateUpdate += HandleLateUpdate;

        _actionMap.Enable();

        if (debugMode)
            Debug.Log("[InvectorInputBridge] Initialised and listening.");
    }

    private void OnDestroy()
    {
        if (_tpInput != null)
        {
            _tpInput.onUpdate     -= HandleUpdate;
            _tpInput.onLateUpdate -= HandleLateUpdate;
        }

        _actionMap.Disable();

        if (_clonedAsset != null)
            Destroy(_clonedAsset);
        else
            _actionMap.Dispose();
    }

    /// <summary>
    /// Runs after vShooterMeleeInput.Update() (script order 100 vs default 0).
    /// At this point InputHandle() has already fired and reset isAimingByInput via
    /// aimInput.GetButton() = false (legacy input disabled). We overwrite it here as the
    /// final authority, then call the aim canvas / fire path manually.
    /// </summary>
    private void Update()
    {
        if (_shooterInput == null || _tpInput == null || _tpInput.cc == null || _tpInput.cc.isDead)
            return;

        if (_shooterInput.shooterManager == null || _shooterInput.shooterManager.CurrentWeapon == null)
            return;

        // ── Aim ───────────────────────────────────────────────────────────────
        // Write isAimingByInput directly. AimInput() already ran inside InputHandle and set it
        // to false (legacy poll). We set it to the bridge-held state now so everything that reads
        // IsAiming (canvas, strafe, animation) sees the correct value this frame.
        bool canAim = !_shooterInput.isEquipping &&
                      !_shooterInput.isAttacking &&
                      !_tpInput.cc.isRolling &&
                      (!_shooterInput.isReloading || _shooterInput.shooterManager.keepAimingWhenReload);

        bool wantsAim = _aimHeld && canAim;
        SetShooterInputFlag("isAimingByInput", wantsAim);

        // Drive the aim canvas and weapon aim visuals that AimInput() normally handles
        if (_shooterInput.controlAimCanvas != null)
        {
            if (wantsAim && !_shooterInput.controlAimCanvas.isAimActive)
                _shooterInput.controlAimCanvas.SetActiveAim(true);
            else if (!wantsAim && _shooterInput.controlAimCanvas.isAimActive)
                _shooterInput.controlAimCanvas.SetActiveAim(false);
        }

        // Strafe while aiming
        var locomotion = _tpInput.cc.locomotionType;
        if (locomotion == Invector.vCharacterController.vThirdPersonMotor.LocomotionType.FreeWithStrafe &&
            !_tpInput.cc.lockInStrafe)
        {
            if (wantsAim && !_tpInput.cc.isStrafing)
                _tpInput.cc.Strafe();
            else if (!wantsAim && _tpInput.cc.isStrafing &&
                     locomotion != Invector.vCharacterController.vThirdPersonMotor.LocomotionType.OnlyStrafe)
                _tpInput.cc.Strafe();
        }

        // ── Aim assist ────────────────────────────────────────────────────────
        // Activate when the player enters aim mode; deactivate when they leave.
        if (_aimAssist != null && wantsAim != _prevWantsAim)
        {
            _aimAssist.SetAssistActive(wantsAim);

            if (debugMode)
                Debug.Log($"[InvectorInputBridge] Aim assist {(wantsAim ? "enabled" : "disabled")}.");
        }
        _prevWantsAim = wantsAim;

        // ── Fire ──────────────────────────────────────────────────────────────
        // ShotInput() ran inside InputHandle with IsAiming=false, so shootCountA was zeroed.
        // Now that isAimingByInput is correct we can call HandleShotCount directly.
        if (wantsAim && !_shooterInput.shooterManager.isShooting && _shooterInput.aimConditions)
        {
            var weapon = _shooterInput.shooterManager.CurrentActiveWeapon;
            if (weapon != null)
                _shooterInput.HandleShotCount(_shooterInput.shooterManager.CurrentWeapon, _fireHeld);
        }
    }

    // ── Action construction ───────────────────────────────────────────────────

    /// <summary>
    /// Loads and clones the assigned InputActionAsset, then resolves all action
    /// references by name from the "InvectorPlayer" map.
    /// </summary>
    private void LoadActionsFromAsset()
    {
        _clonedAsset = Instantiate(_inputActionAsset);
        _actionMap   = _clonedAsset.FindActionMap("InvectorPlayer", throwIfNotFound: true);

        _moveAction            = _actionMap.FindAction("Move",             throwIfNotFound: true);
        _lookAction            = _actionMap.FindAction("Look",             throwIfNotFound: true);
        _zoomAction            = _actionMap.FindAction("CameraZoom",       throwIfNotFound: true);
        _jumpAction            = _actionMap.FindAction("Jump",             throwIfNotFound: true);
        _sprintAction          = _actionMap.FindAction("Sprint",           throwIfNotFound: true);
        _crouchAction          = _actionMap.FindAction("Crouch",           throwIfNotFound: true);
        _strafeAction          = _actionMap.FindAction("Strafe",           throwIfNotFound: true);
        _rollAction            = _actionMap.FindAction("Roll",             throwIfNotFound: true);
        _toggleWalkAction      = _actionMap.FindAction("ToggleWalk",       throwIfNotFound: true);
        _aimAction             = _actionMap.FindAction("Aim",              throwIfNotFound: true);
        _fireAction            = _actionMap.FindAction("Fire",             throwIfNotFound: true);
        _reloadAction          = _actionMap.FindAction("Reload",           throwIfNotFound: true);
        _switchCamSideAction   = _actionMap.FindAction("SwitchCameraSide", throwIfNotFound: true);
        _scopeViewAction       = _actionMap.FindAction("ScopeView",        throwIfNotFound: true);
        _weakAttackAction      = _actionMap.FindAction("WeakAttack",       throwIfNotFound: true);
        _strongAttackAction    = _actionMap.FindAction("StrongAttack",     throwIfNotFound: true);
        _blockAction           = _actionMap.FindAction("Block",            throwIfNotFound: true);
        _openInventoryAction   = _actionMap.FindAction("OpenInventory",    throwIfNotFound: true);
        _removeEquipmentAction = _actionMap.FindAction("RemoveEquipment",  throwIfNotFound: true);
        _holsterAction         = _actionMap.FindAction("Holster",          throwIfNotFound: true);
        _coverAction           = _actionMap.FindAction("Cover",            throwIfNotFound: true);
        _interactAction        = _actionMap.FindAction("Interact",         throwIfNotFound: true);
        _throwAimAction        = _actionMap.FindAction("ThrowAim",         throwIfNotFound: true);
        _throwReleaseAction    = _actionMap.FindAction("ThrowRelease",     throwIfNotFound: true);

        // Weapon slot actions — populate lists in order (slot 0 then slot 1)
        string[] slots = { "0", "1" };
        foreach (string slot in slots)
        {
            _prevSlotActions.Add(_actionMap.FindAction($"PrevSlot{slot}", throwIfNotFound: true));
            _nextSlotActions.Add(_actionMap.FindAction($"NextSlot{slot}", throwIfNotFound: true));
            _useItemActions.Add(_actionMap.FindAction($"UseItem{slot}",   throwIfNotFound: true));
            _prevSlotPressed.Add(false);
            _nextSlotPressed.Add(false);
            _useItemPressed.Add(false);
        }

        RegisterCallbacks();
    }

    private void BuildActions()
    {
        _actionMap = new InputActionMap("InvectorPlayer");

        // ── Movement (Value / Vector2) ────────────────────────────────────────
        _moveAction = _actionMap.AddAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.AddBinding("<Gamepad>/leftStick");

        // ── Look (Value / Vector2) ────────────────────────────────────────────
        _lookAction = _actionMap.AddAction("Look", InputActionType.Value);
        _lookAction.AddBinding("<Mouse>/delta").WithProcessor("scaleVector2(x=0.1,y=0.1)");
        _lookAction.AddBinding("<Gamepad>/rightStick");

        // ── Camera zoom ───────────────────────────────────────────────────────
        _zoomAction = _actionMap.AddAction("CameraZoom", InputActionType.Value);
        _zoomAction.AddBinding("<Mouse>/scroll/y").WithProcessor("normalize");

        // ── Locomotion buttons ────────────────────────────────────────────────
        // Jump: keyboard Space always jumps. Gamepad buttonSouth (A) conditionally
        // jumps or interacts with cover — tracked separately via _jumpPressedGamepad.
        _jumpAction = _actionMap.AddAction("Jump", InputActionType.Button);
        _jumpAction.AddBinding("<Keyboard>/space");
        _jumpAction.AddBinding("<Gamepad>/buttonSouth");

        _sprintAction = _actionMap.AddAction("Sprint", InputActionType.Button);
        _sprintAction.AddBinding("<Keyboard>/leftShift");
        _sprintAction.AddBinding("<Gamepad>/leftStickPress");

        _crouchAction = _actionMap.AddAction("Crouch", InputActionType.Button);
        _crouchAction.AddBinding("<Keyboard>/c");
        _crouchAction.AddBinding("<Gamepad>/buttonNorth");

        _strafeAction = _actionMap.AddAction("Strafe", InputActionType.Button);
        _strafeAction.AddBinding("<Keyboard>/tab");
        _strafeAction.AddBinding("<Gamepad>/rightStickPress");

        _rollAction = _actionMap.AddAction("Roll", InputActionType.Button);
        _rollAction.AddBinding("<Keyboard>/q");
        _rollAction.AddBinding("<Gamepad>/buttonEast");

        _toggleWalkAction = _actionMap.AddAction("ToggleWalk", InputActionType.Button);
        _toggleWalkAction.AddBinding("<Keyboard>/capsLock");

        // ── Shooter ───────────────────────────────────────────────────────────
        _aimAction = _actionMap.AddAction("Aim", InputActionType.Button);
        _aimAction.AddBinding("<Mouse>/rightButton");
        _aimAction.AddBinding("<Gamepad>/leftTrigger");

        _fireAction = _actionMap.AddAction("Fire", InputActionType.Button);
        _fireAction.AddBinding("<Mouse>/leftButton");
        _fireAction.AddBinding("<Gamepad>/rightTrigger");

        _reloadAction = _actionMap.AddAction("Reload", InputActionType.Button);
        _reloadAction.AddBinding("<Keyboard>/r");
        _reloadAction.AddBinding("<Gamepad>/leftShoulder");

        _switchCamSideAction = _actionMap.AddAction("SwitchCameraSide", InputActionType.Button);
        _switchCamSideAction.AddBinding("<Keyboard>/v");
        // Gamepad: hold left stick to switch — rightStickPress is already used by Strafe
        _switchCamSideAction.AddBinding("<Gamepad>/select");

        _scopeViewAction = _actionMap.AddAction("ScopeView", InputActionType.Button);
        _scopeViewAction.AddBinding("<Keyboard>/z");
        // Gamepad: leftStickPress is shared with Sprint — routed in RegisterCallbacks based on weapon.

        // ── Melee ─────────────────────────────────────────────────────────────
        _weakAttackAction = _actionMap.AddAction("WeakAttack", InputActionType.Button);
        _weakAttackAction.AddBinding("<Mouse>/leftButton");
        _weakAttackAction.AddBinding("<Gamepad>/rightShoulder");

        _strongAttackAction = _actionMap.AddAction("StrongAttack", InputActionType.Button);
        _strongAttackAction.AddBinding("<Keyboard>/1");
        _strongAttackAction.AddBinding("<Gamepad>/rightTrigger");

        _blockAction = _actionMap.AddAction("Block", InputActionType.Button);
        _blockAction.AddBinding("<Mouse>/rightButton");
        // Gamepad: leftShoulder is already used by Reload — use leftTrigger for block when not aiming
        _blockAction.AddBinding("<Gamepad>/leftShoulder");

        // ── Inventory & weapon slots ──────────────────────────────────────────
        _openInventoryAction = _actionMap.AddAction("OpenInventory", InputActionType.Button);
        _openInventoryAction.AddBinding("<Keyboard>/i");
        _openInventoryAction.AddBinding("<Gamepad>/start");

        _removeEquipmentAction = _actionMap.AddAction("RemoveEquipment", InputActionType.Button);
        // rightButton is already Aim/Block — use middle mouse for keyboard, buttonWest for gamepad
        _removeEquipmentAction.AddBinding("<Mouse>/middleButton");
        _removeEquipmentAction.AddBinding("<Gamepad>/buttonWest");

        // ── Holster (draw/hide toggle) ────────────────────────────────────────
        // Replaces vDrawHideMeleeWeapons.hideAndDrawWeaponsInput (default: H) which uses
        // GenericInput and is suppressed by the bridge. X on keyboard, buttonWest on gamepad
        // (buttonWest = X on Xbox / Square on PlayStation — free since RemoveEquipment uses
        // middleButton on keyboard, so there is no device conflict here).
        _holsterAction = _actionMap.AddAction("Holster", InputActionType.Button);
        _holsterAction.AddBinding("<Keyboard>/x");
        _holsterAction.AddBinding("<Gamepad>/buttonWest");

        // ── Cover ─────────────────────────────────────────────────────────────
        // F is the dedicated cover/interact key. vCoverController.enterExitInput is patched
        // in SuppressLegacyInput() to also use "F" so its GenericInput poll matches.
        // On gamepad, cover is triggered via the Jump/Cover conditional (buttonSouth / A)
        // so no separate gamepad binding is needed here.
        _coverAction = _actionMap.AddAction("Cover", InputActionType.Button);
        _coverAction.AddBinding("<Keyboard>/f");

        // ── Interact (generic action / parkour) ───────────────────────────────
        // E on keyboard. On gamepad buttonNorth (Y/Triangle) — this is also bound to Crouch,
        // so gamepad users need to remap one via Rebind() if both are required simultaneously.
        _interactAction = _actionMap.AddAction("Interact", InputActionType.Button);
        _interactAction.AddBinding("<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonNorth");

        // ── Grenade / throwable ───────────────────────────────────────────────
        // Numpad2 — equip & toggle aim mode. While aiming, LMB / RT launches the grenade.
        _throwAimAction = _actionMap.AddAction("ThrowAim", InputActionType.Button);
        _throwAimAction.AddBinding("<Keyboard>/numpad2");
        _throwAimAction.AddBinding("<Gamepad>/leftShoulder");   // LB (free when not reloading)

        _throwReleaseAction = _actionMap.AddAction("ThrowRelease", InputActionType.Button);
        _throwReleaseAction.AddBinding("<Mouse>/leftButton");
        _throwReleaseAction.AddBinding("<Gamepad>/rightTrigger");

        // Weapon slot 0  (primary — e.g. right-hand weapon)
        // Previous = scroll up / left arrow / D-Pad left
        // Next     = scroll down / right arrow / D-Pad right
        var prev0 = _actionMap.AddAction("PrevSlot0", InputActionType.Button);
        prev0.AddBinding("<Keyboard>/leftArrow");
        prev0.AddBinding("<Mouse>/scroll/up");
        prev0.AddBinding("<Gamepad>/dpad/left");
        _prevSlotActions.Add(prev0);
        _prevSlotPressed.Add(false);

        var next0 = _actionMap.AddAction("NextSlot0", InputActionType.Button);
        next0.AddBinding("<Keyboard>/rightArrow");
        next0.AddBinding("<Mouse>/scroll/down");
        next0.AddBinding("<Gamepad>/dpad/right");
        _nextSlotActions.Add(next0);
        _nextSlotPressed.Add(false);

        var use0 = _actionMap.AddAction("UseItem0", InputActionType.Button);
        use0.AddBinding("<Keyboard>/u");
        use0.AddBinding("<Gamepad>/dpad/up");
        _useItemActions.Add(use0);
        _useItemPressed.Add(false);

        // Weapon slot 1  (secondary — left-hand / holster slot)
        var prev1 = _actionMap.AddAction("PrevSlot1", InputActionType.Button);
        prev1.AddBinding("<Keyboard>/pageUp");
        prev1.AddBinding("<Gamepad>/dpad/left");
        _prevSlotActions.Add(prev1);
        _prevSlotPressed.Add(false);

        var next1 = _actionMap.AddAction("NextSlot1", InputActionType.Button);
        next1.AddBinding("<Keyboard>/pageDown");
        next1.AddBinding("<Gamepad>/dpad/right");
        _nextSlotActions.Add(next1);
        _nextSlotPressed.Add(false);

        var use1 = _actionMap.AddAction("UseItem1", InputActionType.Button);
        use1.AddBinding("<Keyboard>/h");
        use1.AddBinding("<Gamepad>/dpad/down");
        _useItemActions.Add(use1);
        _useItemPressed.Add(false);

        // ── Callbacks ─────────────────────────────────────────────────────────
        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        _moveAction.performed  += ctx => _moveValue  = ctx.ReadValue<Vector2>();
        _moveAction.canceled   += ctx => _moveValue  = Vector2.zero;

        _lookAction.performed  += ctx => _lookValue  = ctx.ReadValue<Vector2>();
        _lookAction.canceled   += ctx => _lookValue  = Vector2.zero;

        _zoomAction.performed  += ctx => _zoomValue  = ctx.ReadValue<float>();
        _zoomAction.canceled   += ctx => _zoomValue  = 0f;

        // Held buttons
        // Gamepad leftStickPress is shared between Sprint and ScopeView:
        // if the current weapon has a scopeTarget (i.e. is a scoped/sniper rifle), trigger
        // ScopeView on press; otherwise treat it as Sprint.
        _sprintAction.performed += ctx =>
        {
            if (ctx.control.device is Gamepad && IsCurrentWeaponScoped())
                _scopeViewPressed = true;
            else
                _sprintHeld = true;
        };
        _sprintAction.canceled  += ctx => _sprintHeld = false;

        _aimAction.performed   += ctx => _aimHeld   = true;
        _aimAction.canceled    += ctx => _aimHeld   = false;

        _fireAction.performed  += ctx => _fireHeld  = true;
        _fireAction.canceled   += ctx => _fireHeld  = false;

        _blockAction.performed += ctx => _blockHeld = true;
        _blockAction.canceled  += ctx => _blockHeld = false;

        _strongAttackAction.performed += ctx => _strongAttackHeld = true;
        _strongAttackAction.canceled  += ctx => _strongAttackHeld = false;

        // Down-only flags (consume once per frame)
        _jumpAction.performed += ctx =>
        {
            _jumpPressed = true;
            _jumpPressedGamepad = ctx.control.device is Gamepad;
        };
        _crouchAction.performed      += ctx => _crouchPressed      = true;
        _strafeAction.performed      += ctx => _strafePressed      = true;
        _rollAction.performed        += ctx => _rollPressed        = true;
        _toggleWalkAction.performed  += ctx => _toggleWalkPressed  = true;
        _reloadAction.performed      += ctx => _reloadPressed      = true;
        _switchCamSideAction.performed += ctx => _switchCamSidePressed = true;
        _scopeViewAction.performed   += ctx => _scopeViewPressed   = true;

        _weakAttackAction.performed  += ctx => { _weakAttackDown = true; _weakAttackPressed = true; };
        _weakAttackAction.canceled   += ctx => _weakAttackDown  = false;

        _strongAttackAction.performed += ctx => _strongAttackDown = true;

        // Inventory
        _openInventoryAction.performed   += ctx => _openInventoryPressed    = true;
        _removeEquipmentAction.performed += ctx => _removeEquipmentPressed  = true;
        _holsterAction.performed         += ctx => _holsterPressed          = true;

        // Cover keyboard (F) — held state drives both enter on press and hold-to-exit
        _coverAction.performed += ctx => _coverHeld = true;
        _coverAction.canceled  += ctx => _coverHeld = false;

        // Interact / parkour — consumed once per press
        _interactAction.performed += ctx => _interactPressed = true;

        // Grenade / throwable
        _throwAimAction.performed     += ctx => _throwAimPressed     = true;
        _throwReleaseAction.performed += ctx => _throwReleasePressed = true;

        for (int i = 0; i < _prevSlotActions.Count; i++)
        {
            int idx = i; // capture for closure
            _prevSlotActions[i].performed += ctx => _prevSlotPressed[idx] = true;
            _nextSlotActions[i].performed += ctx => _nextSlotPressed[idx] = true;
            _useItemActions[i].performed  += ctx => _useItemPressed[idx]  = true;
        }
    }

    // ── Invector binding ──────────────────────────────────────────────────────

    private void CacheInvectorComponents()
    {
        _tpInput        = GetComponent<vThirdPersonInput>();
        _meleeInput     = GetComponent<vMeleeCombatInput>();
        _shooterInput   = GetComponent<vShooterMeleeInput>();
        _inventory      = FindFirstObjectByType<vInventory>();
        _coverController = GetComponent<vCoverController>();
        _genericAction  = GetComponent<vGenericAction>();
        _drawHideWeapons = GetComponent<Invector.vShooter.vDrawHideShooterWeapons>();
        _throwManager   = GetComponentInChildren<vThrowManagerBase>(includeInactive: true);
        _aimAssist      = GetComponentInChildren<CameraAimAssistentHeadTracking>(includeInactive: true)
                       ?? FindFirstObjectByType<CameraAimAssistentHeadTracking>();

        // Cache reflection fields for vThirdPersonCamera internal orbit angles.
        var camType = typeof(Invector.vCamera.vThirdPersonCamera);
        const BindingFlags internalField = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        _cameraMouseXField = camType.GetField("_mouseX", internalField);
        _cameraMouseYField = camType.GetField("_mouseY", internalField);

        if (_tpInput == null)
            Debug.LogError("[InvectorInputBridge] vThirdPersonInput not found on this GameObject.");

        if (_inventory == null)
            Debug.LogWarning("[InvectorInputBridge] vInventory not found in scene — inventory & weapon-slot inputs disabled.");

        if (_coverController == null)
            Debug.LogWarning("[InvectorInputBridge] vCoverController not found on this GameObject — cover input not bridged.");

        if (_genericAction == null)
            Debug.LogWarning("[InvectorInputBridge] vGenericAction not found on this GameObject — parkour/interact input not bridged.");

        if (_drawHideWeapons == null)
            Debug.LogWarning("[InvectorInputBridge] vDrawHideShooterWeapons not found on this GameObject — holster input not bridged.");
    }

    /// <summary>
    /// Turns off all legacy GenericInput polling so our bridge is the sole input source.
    /// Uses SetBridgeLockInput so that Invector systems calling SetLockAllInput(false)
    /// (e.g. ExitCover, inventory open/close) cannot accidentally re-enable legacy polling.
    /// </summary>
    private void SuppressLegacyInput()
    {
        // Basic locomotion — use bridge-lock so SetLockBasicInput(false) is a no-op
        if (_tpInput != null)
        {
            _tpInput.SetBridgeLockInput(true);

            // Prevent vThirdPersonInput.CameraInput() from calling tpCamera.RotateCamera()
            // with legacy Mouse X/Y axis values every LateUpdate. The bridge drives camera
            // rotation directly in HandleLateUpdate, so this must be suppressed.
            _tpInput.lockCameraInput = true;
        }

        // Melee
        if (_meleeInput != null)
            _meleeInput.lockMeleeInput = true;

        // Shooter — do NOT call SetLockShooterInput(true) here.
        // Invector's InputHandle() must run its full shooter state machine (equip animations,
        // reload, canvas transitions, etc.). Aim and fire state are injected from this bridge's
        // own Update(), which runs after InputHandle() due to [DefaultExecutionOrder(100)].

        // Inventory — suppress its own GenericInput polling inside LateUpdate.
        // We do NOT permanently set lockInventoryInput=true here because IsLocked() uses it
        // as a gate that also blocks slot cycling and holster. Instead we leave it false and
        // suppress only what matters: GenericInput cannot fire in New Input System-only mode
        // anyway, so the LateUpdate polling is already a no-op. The bridge drives everything
        // directly in HandleInventoryInput().

        // Cover — vCoverController.HandleEnterExitCover() polls GenericInput which relies on
        // vInput.instance.inputDevice being correct and the legacy axis names being registered.
        // Both are unreliable in "Both" input mode. Disable GenericInput polling entirely by
        // setting useInput = false; the bridge drives cover entry/exit directly via reflection.
        if (_coverController != null)
            _coverController.enterExitInput.useInput = false;
    }

    // ── Per-frame input injection ─────────────────────────────────────────────

    /// <summary>
    /// Subscribed to vThirdPersonInput.onUpdate — runs inside Invector's Update loop.
    /// </summary>
    private void HandleUpdate()
    {
        if (_tpInput == null || _tpInput.cc == null || _tpInput.cc.isDead) return;

        var cc = _tpInput.cc;

        // Movement
        // Always inject cc.input even when lockMoveInput is true so that cc.inputSmooth
        // (which is lerped from cc.input in the motor) stays populated. The cover controller
        // reads inputSmooth via ControlInputDirectionInCover to compute inputDirection and
        // cameraAngle — used for corner detection and cover-side changes. If we skip injection
        // while lockMoveInput is true (set by cover's PrepareToMoveOnPath), inputSmooth drains
        // to zero and IsLookingToCorner always returns false, breaking corner traversal.
        // The cover controller still owns actual locomotion — it overrides moveDirection
        // and lockSetMoveSpeed — so writing cc.input here is safe.
        if (!_tpInput.lockMoveInput || (_coverController != null && _coverController.inCover))
        {
            // Apply radial dead zone for gamepad left stick to remove drift and jitter.
            Vector2 move = _moveValue;
            bool usingGamepadMove = Gamepad.current != null &&
                                    Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.0001f;
            if (usingGamepadMove && move.magnitude < gamepadMoveDeadZone)
                move = Vector2.zero;
            else if (usingGamepadMove && move.magnitude > 0f)
                move = move.normalized * Mathf.InverseLerp(gamepadMoveDeadZone, 1f, move.magnitude);

            var input = cc.input;
            input.x = move.x;
            input.z = move.y;
            cc.input = input;
        }

        cc.ControlKeepDirection();

        // Toggle walk
        if (_toggleWalkPressed)
        {
            cc.alwaysWalkByDefault = !cc.alwaysWalkByDefault;
            _toggleWalkPressed = false;
        }

        // Sprint
        cc.Sprint(cc.useContinuousSprint ? ConsumeFlag(ref _sprintHeld) : _sprintHeld);

        // Crouch
        cc.AutoCrouch();
        if (_crouchPressed)
        {
            cc.Crouch();
            _crouchPressed = false;
        }

        // Strafe toggle
        if (_strafePressed)
        {
            cc.Strafe();
            _strafePressed = false;
        }

        // Jump
        // Priority on gamepad (buttonSouth / X on PlayStation):
        //   1. Interact  — if player is near an interactable trigger, pick it up instead of jumping.
        //   2. Cover     — if near a cover point, redirect to cover entry.
        //   3. Jump      — default behaviour.
        // On keyboard (Space): always jump — E is the dedicated interact key, F is cover.
        if (_jumpPressed)
        {
            bool redirectToInteract = _jumpPressedGamepad && IsNearInteractable();
            bool redirectToCover    = _jumpPressedGamepad && !redirectToInteract && IsNearCover();

            if (redirectToInteract)
                TriggerInteract();
            else if (redirectToCover)
                TriggerCoverEnterExit(buttonDown: true, buttonHeld: true);
            else if (JumpConditions(cc))
                cc.Jump(true);

            _jumpPressed        = false;
            _jumpPressedGamepad = false;
        }

        // Cover keyboard (F) — drive enter/exit each frame using the held state
        if (_coverController != null)
            TriggerCoverEnterExit(buttonDown: _coverHeld, buttonHeld: _coverHeld);

        // Interact / parkour (E / buttonNorth).
        if (_interactPressed && _genericAction != null)
        {
            TriggerInteract();
            _interactPressed = false;
        }
        else
        {
            _interactPressed = false;
        }

        // Roll
        if (_rollPressed && RollConditions(cc))
        {
            cc.Roll();
            _rollPressed = false;
        }
        else
        {
            _rollPressed = false;
        }

        // Melee
        if (_meleeInput != null && !_shooterInput?.IsAiming == true)
            HandleMeleeUpdate(cc);

        // Shooter
        if (_shooterInput != null)
            HandleShooterUpdate(cc);

        // Grenade / throwable
        if (_throwManager != null)
            HandleThrowInput();
    }

    private void HandleMeleeUpdate(vThirdPersonController cc)
    {
        if (!CanMeleeAttack()) return;

        // Weak attack — call TriggerWeakAttack directly; condition check done here.
        if (_weakAttackPressed && MeleeAttackStaminaConditions())
        {
            _meleeInput.TriggerWeakAttack();
            _weakAttackPressed = false;
        }
        else
        {
            _weakAttackPressed = false;
        }

        // Strong attack
        if (_strongAttackDown && MeleeAttackStaminaConditions())
        {
            _meleeInput.TriggerStrongAttack();
            _strongAttackDown = false;
        }
        else
        {
            _strongAttackDown = false;
        }

        // Block (held) — mirror BlockingInput() but driven by our action
        bool shouldBlock = _blockHeld && cc.currentStamina > 0 && !cc.customAction && !_meleeInput.isAttacking;
        SetMeleeBlocking(shouldBlock);
    }

    private void HandleShooterUpdate(vThirdPersonController cc)
    {
        if (_shooterInput.shooterManager == null) return;

        bool hasWeapon = _shooterInput.shooterManager.CurrentWeapon != null;

        if (!hasWeapon)
        {
            _reloadPressed        = false;
            _switchCamSidePressed = false;
            _scopeViewPressed     = false;
            return;
        }

        // Reload
        if (_reloadPressed)
        {
            _shooterInput.ReloadInput();
            _reloadPressed = false;
        }

        // Switch camera side
        if (_switchCamSidePressed)
        {
            _shooterInput.SwitchCameraSideInput();
            _switchCamSidePressed = false;
        }

        // Scope view
        if (_scopeViewPressed)
        {
            _shooterInput.ScopeViewInput();
            _scopeViewPressed = false;
        }

        // Aim and fire are injected in this bridge's own Update() (after InputHandle wipes them).
    }

    private void InjectAimInput()
    {
        // Patch aimInput.GetButton() behaviour: set isAimingByInput directly.
        // We call AimInput() on the shooter but first prime the held-button result.
        SetShooterInputFlag("isAimingByInput", _aimHeld);
        _shooterInput.AimInput();
    }

    private void InjectFireInput()
    {
        if (_shooterInput.shooterManager == null) return;
        var weapon = _shooterInput.shooterManager.CurrentWeapon;
        if (weapon == null) return;

        _shooterInput.ShotInput();
    }

    /// <summary>
    /// Subscribed to vThirdPersonInput.onLateUpdate — feeds camera rotation and inventory input.
    /// </summary>
    private void HandleLateUpdate()
    {
        // ── Camera ────────────────────────────────────────────────────────────
        // lockCameraInput is set by SuppressLegacyInput to prevent vThirdPersonInput.CameraInput()
        // from feeding legacy Mouse X/Y into RotateCamera. The bridge is the sole camera driver,
        // so we bypass that flag here and call RotateCamera directly.
        if (_tpInput != null && _tpInput.tpCamera != null)
        {
            bool usingGamepad = Gamepad.current != null &&
                                Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f;

            float x, y;
            if (usingGamepad)
            {
                // Apply radial dead zone to right stick.
                Vector2 rawLook = _lookValue;
                if (rawLook.magnitude < gamepadLookDeadZone)
                    rawLook = Vector2.zero;
                else
                    rawLook = rawLook.normalized * Mathf.InverseLerp(gamepadLookDeadZone, 1f, rawLook.magnitude);

                // Smooth acceleration only — snap to zero immediately on stick release.
                // Lerping on release causes the camera to drift past the intended stop point,
                // which reads as overshoot. Only ramp-up benefits from smoothing.
                if (rawLook.sqrMagnitude > 0f)
                {
                    float smoothRate = gamepadLookSmoothing > 0f ? gamepadLookSmoothing : float.MaxValue;
                    _smoothedLookValue = Vector2.Lerp(_smoothedLookValue, rawLook, smoothRate * Time.deltaTime);
                }
                else
                {
                    _smoothedLookValue = Vector2.zero;
                }

                x = _smoothedLookValue.x * gamepadDegreesPerSecond * Time.deltaTime;
                y = _smoothedLookValue.y * gamepadDegreesPerSecond * Time.deltaTime;
            }
            else
            {
                _smoothedLookValue = Vector2.zero;
                x = _lookValue.x * mouseSensitivity;
                y = _lookValue.y * mouseSensitivity;
            }

            if (_tpInput.invertCameraInputHorizontal) x *= -1f;
            if (_tpInput.invertCameraInputVertical)   y *= -1f;

            _tpInput.tpCamera.RotateCamera(x, y);

            // Aim-assist: screen-space feedback loop.
            // WorldToScreenPoint tells us exactly how many pixels the target chest is away from
            // screen centre — we convert that to degrees using the camera FOV and nudge
            // _mouseX/_mouseY by that amount each frame. Because we measure the actual on-screen
            // position the correction is exact regardless of camera offset, pivot height,
            // switchRight, or offsetMouse.
            if (_aimAssist != null && _aimAssist.assistActive &&
                _aimAssist.HasTarget &&
                _cameraMouseXField != null && _cameraMouseYField != null)
            {
                Vector2 offset = _aimAssist.ScreenOffset; // pixels from screen centre

                // Dead zone: skip correction when crosshair is already close enough.
                // Prevents micro-oscillation once the camera is nearly aligned.
                if (offset.magnitude > aimAssistDeadZonePx)
                {
                    // Degrees per pixel for each axis using the live camera FOV.
                    float vFov  = Camera.main.fieldOfView;
                    float hFov  = Camera.VerticalToHorizontalFieldOfView(vFov, Camera.main.aspect);
                    float dppH  = hFov / Screen.width;
                    float dppV  = vFov / Screen.height;

                    float force = _aimAssist.AssistentForce;

                    // Raw desired correction in degrees this frame.
                    // corrX: positive pixel offset (target right of crosshair) → increase mouseX (rotate right)
                    // corrY: positive pixel offset (target above crosshair)    → decrease mouseY (rotate up)
                    float corrX =  offset.x * dppH * force * Time.deltaTime;
                    float corrY = -offset.y * dppV * force * Time.deltaTime;

                    // Clamp to max degrees-per-second to prevent overshooting and oscillation.
                    float maxStep = aimAssistMaxDegPerSec * Time.deltaTime;
                    corrX = Mathf.Clamp(corrX, -maxStep, maxStep);
                    corrY = Mathf.Clamp(corrY, -maxStep, maxStep);

                    var   cam    = _tpInput.tpCamera;
                    float mouseX = (float)_cameraMouseXField.GetValue(cam);
                    float mouseY = (float)_cameraMouseYField.GetValue(cam);
                    _cameraMouseXField.SetValue(cam, mouseX + corrX);
                    _cameraMouseYField.SetValue(cam, mouseY + corrY);
                }
            }

            _tpInput.tpCamera.Zoom(_zoomValue);
        }

        // ── Inventory ─────────────────────────────────────────────────────────
        HandleInventoryInput();
    }

    // ── Inventory input ───────────────────────────────────────────────────────

    /// <summary>
    /// Drives vInventory open/close, weapon slot cycling, consumable use, and unequip —
    /// all without touching GenericInput / UnityEngine.Input.
    /// Called from HandleLateUpdate to match vInventory.LateUpdate() timing.
    /// </summary>
    private void HandleInventoryInput()
    {
        if (_inventory == null) return;

        // ── Holster — never blocked by IsLocked() ────────────────────────────
        // vDrawHideMeleeWeapons.HandleInput() polls GenericInput which is a no-op
        // in New Input System mode. We call DrawWeapons/HideWeapons directly instead.
        if (_holsterPressed)
        {
            if (_drawHideWeapons != null && !_drawHideWeapons.isLocked)
            {
                if (_drawHideWeapons.weaponsHided)
                    _drawHideWeapons.DrawWeapons();
                else
                    _drawHideWeapons.HideWeapons();

                if (debugMode)
                    Debug.Log($"[InvectorInputBridge] Holster toggled — weaponsHided was {_drawHideWeapons.weaponsHided}.");
            }
            _holsterPressed = false;
        }

        // ── Slot cycling — runs whenever inventory is closed, regardless of IsLocked ──
        // IsLocked() returns true when lockInventoryInput is set, but slot cycling must
        // always be available at runtime. We guard only on the inventory being open.
        if (!_inventory.isOpen)
        {
            var controllers = _inventory.changeEquipmentControllers;
            int slotCount   = Mathf.Min(controllers.Count, _prevSlotActions.Count);

            for (int i = 0; i < slotCount; i++)
            {
                var area = controllers[i]?.equipArea;
                if (area == null) continue;

                if (_prevSlotPressed[i])
                {
                    area.PreviousEquipSlot();
                    _prevSlotPressed[i] = false;

                    if (debugMode)
                        Debug.Log($"[InvectorInputBridge] Slot {i}: PreviousEquipSlot()");
                }

                if (_nextSlotPressed[i])
                {
                    area.NextEquipSlot();
                    _nextSlotPressed[i] = false;

                    if (debugMode)
                        Debug.Log($"[InvectorInputBridge] Slot {i}: NextEquipSlot()");
                }

                // Use consumable
                if (_useItemPressed[i])
                {
                    var display = controllers[i].display;
                    if (display?.item != null &&
                        display.item.type == vItemType.Consumable &&
                        display.item.amount > 0)
                    {
                        _inventory.onUseItem.Invoke(display.item);
                    }
                    _useItemPressed[i] = false;
                }
            }
        }

        // ── Everything below is gated on IsLocked() (open/close + remove) ────
        if (_inventory.IsLocked()) 
        {
            _openInventoryPressed   = false;
            _removeEquipmentPressed = false;
            return;
        }

        // Open / close
        if (_openInventoryPressed && _inventory.canEquip)
        {
            if (_inventory.isOpen) _inventory.CloseInventory();
            else                   _inventory.OpenInventory();
        }
        _openInventoryPressed = false;

        // Remove equipped item from the current area (only while open)
        if (_inventory.isOpen && _removeEquipmentPressed)
        {
            _inventory.equipAreas[0]?.UnequipCurrentItem();
        }
        _removeEquipmentPressed = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool JumpConditions(vThirdPersonController cc)
    {
        return !cc.inJumpStarted && !cc.customAction && !cc.isCrouching &&
               cc.isGrounded && !((int)cc.GroundAngle() > cc.slopeLimit) &&
               cc.currentStamina >= cc.jumpStamina && !cc.isJumping && !cc.isRolling;
    }

    /// <summary>
    /// Returns true when vGenericAction has a valid, active, interactable trigger in range.
    /// Used to redirect gamepad buttonSouth from Jump to Interact.
    /// </summary>
    private bool IsNearInteractable()
    {
        if (_genericAction == null) return false;
        var trigger = _genericAction.triggerAction;
        return trigger != null && trigger.gameObject.activeInHierarchy && trigger.CanDoAction;
    }

    /// <summary>
    /// Fires the nearest interactable trigger, bypassing GenericInput which is compiled
    /// out in New Input System-only mode. Mirrors the GetButtonDown and AutoAction branches
    /// of vGenericAction.TriggerActionInput() without re-polling legacy input.
    /// </summary>
    private void TriggerInteract()
    {
        if (_genericAction == null) return;
        var trigger = _genericAction.triggerAction;
        if (trigger == null || !trigger.gameObject.activeInHierarchy || !trigger.CanDoAction) return;

        bool isButtonDown = trigger.inputType == vTriggerGenericAction.InputType.GetButtonDown;
        bool isAuto       = trigger.inputType == vTriggerGenericAction.InputType.AutoAction;

        if ((isButtonDown || isAuto) && _genericAction.actionConditions)
        {
            _genericAction.TriggerActionEvents();
            _genericAction.TriggerAnimation();
        }
        else if (!isButtonDown && !isAuto)
        {
            // Hold/double-press types manage their own per-frame state internally.
            _genericAction.TriggerActionInput();
        }
    }

    /// <summary>
    /// Returns true only when vCoverController has a highlighted cover point waiting for
    /// player confirmation. Maps to the cover UI highlight state — the same condition
    /// Invector checks before starting EnterCoverPointRoutine.
    /// Used to redirect gamepad buttonSouth from Jump to Cover entry.
    /// </summary>
    private bool IsNearCover()
    {
        return _coverController != null && _coverController.possibleCoverPoint != null;
    }

    /// <summary>
    /// Drives vCoverController cover entry and exit without touching GenericInput.
    /// Mirrors the logic of vCoverController.HandleEnterExitCover() but uses bridge-owned
    /// button state instead of legacy Input polling.
    ///
    /// Call with buttonDown=true on the frame the button is first pressed.
    /// Call with buttonHeld=true on every frame the button remains held (for hold-to-exit).
    /// </summary>
    private void TriggerCoverEnterExit(bool buttonDown, bool buttonHeld)
    {
        if (_coverController == null) return;

        var cc = _tpInput.cc;

        // ── Cover-to-cover dash (already in cover, new point available) ──────
        // Mirrors vCoverController line 535: (inTimer && inCover && wasInCover)
        // enterExitInput.useInput is false so GetButtonTimer never fires — we handle it here.
        if (buttonDown &&
            _coverController.possibleCoverPoint != null &&
            _coverController.inCover && _coverController.wasInCover &&
            !_coverController.goingToCoverPoint &&
            !cc.customAction && !cc.isJumping && !cc.ragdolled && !cc.isDead &&
            !_shooterInput.animator.IsInTransition(0))
        {
            var target = _coverController.possibleCoverPoint;
            _coverController.possibleCoverPoint = null;

            _goToRoutineMethod ??= typeof(vCoverController).GetMethod(
                "GoToCoverPointRoutine",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _wayPathField ??= typeof(vCoverController).GetField(
                "wayPath",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var wayPath  = _wayPathField?.GetValue(_coverController) as System.Collections.Generic.List<Vector3>;
            var pathCopy = wayPath != null ? new System.Collections.Generic.List<Vector3>(wayPath) : new System.Collections.Generic.List<Vector3>();
            var routine  = (System.Collections.IEnumerator)_goToRoutineMethod?.Invoke(_coverController, new object[] { target, pathCopy });

            if (routine != null)
            {
                _currentCoverRoutineField ??= typeof(vCoverController).GetField(
                    "currentCoverRoutine",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                var coroutine = _coverController.StartCoroutine(routine);
                _currentCoverRoutineField?.SetValue(_coverController, coroutine);
            }

            if (debugMode)
                Debug.Log($"[InvectorInputBridge] Cover-to-cover dash triggered → {target.name}");

            return;
        }

        // ── Enter cover (not currently in cover) ─────────────────────────────
        if (buttonDown &&
            _coverController.possibleCoverPoint != null &&
            !_coverController.goingToCoverPoint &&
            !_coverController.inCover &&
            !cc.customAction && !cc.isJumping && !cc.ragdolled && !cc.isDead &&
            !_shooterInput.animator.IsInTransition(0))
        {
            var target = _coverController.possibleCoverPoint;
            _coverController.possibleCoverPoint = null;

            // Mirror Invector's routine selection logic:
            // Use EnterCoverPointRoutine when autoEnterCover is on, or autoTravelToNextCover is off.
            // Otherwise use GoToCoverPointRoutine (which needs the navmesh wayPath).
            bool useEnter = (_coverController.autoEnterCover && !_coverController.inCover)
                            || !_coverController.autoTravelToNextCover;

            System.Collections.IEnumerator routine;
            if (useEnter)
            {
                _enterRoutineMethod ??= typeof(vCoverController).GetMethod(
                    "EnterCoverPointRoutine",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                routine = (System.Collections.IEnumerator)_enterRoutineMethod?.Invoke(_coverController, new object[] { target });
            }
            else
            {
                _goToRoutineMethod ??= typeof(vCoverController).GetMethod(
                    "GoToCoverPointRoutine",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                _wayPathField ??= typeof(vCoverController).GetField(
                    "wayPath",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var wayPath  = _wayPathField?.GetValue(_coverController) as System.Collections.Generic.List<Vector3>;
                var pathCopy = wayPath != null ? new System.Collections.Generic.List<Vector3>(wayPath) : new System.Collections.Generic.List<Vector3>();
                routine = (System.Collections.IEnumerator)_goToRoutineMethod?.Invoke(_coverController, new object[] { target, pathCopy });
            }

            if (routine != null)
            {
                _currentCoverRoutineField ??= typeof(vCoverController).GetField(
                    "currentCoverRoutine",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);

                var coroutine = _coverController.StartCoroutine(routine);
                _currentCoverRoutineField?.SetValue(_coverController, coroutine);
            }

            if (debugMode)
                Debug.Log($"[InvectorInputBridge] Cover enter triggered → {target.name}");

            return; // don't process exit in the same frame
        }

        // ── Exit cover (hold-to-exit after 0.2 s, mirrors the inTimer path) ──
        if (_coverController.inCover && _coverController.wasInCover && buttonHeld)
        {
            _coverHeldTimer += Time.deltaTime;

            if (_coverHeldTimer >= CoverExitHoldTime)
            {
                _coverHeldTimer = 0f;
                _exitCoverMethod ??= typeof(vCoverController).GetMethod(
                    "ExitCover",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                _exitCoverMethod?.Invoke(_coverController, new object[] { false });

                if (debugMode)
                    Debug.Log("[InvectorInputBridge] Cover exit triggered.");
            }
        }
        else
        {
            _coverHeldTimer = 0f;
        }
    }

    // ── Throw / grenade input ─────────────────────────────────────────────────

    /// <summary>
    /// Drives vThrowManagerBase.EnterThrowMode / ExitThrowMode and pressThrowInput
    /// via reflection, bypassing the dead GenericInput polling in New Input System mode.
    ///
    /// Numpad2 toggles aim mode: first press equips and starts aiming, second press cancels.
    /// LMB / RT triggers the throw while the player is aiming.
    /// </summary>
    private void HandleThrowInput()
    {
        if (_throwManager == null || !_throwManager.canUseThrow) return;

        // Cache reflection members lazily
        _throwEnterMethod ??= typeof(vThrowManagerBase).GetMethod(
            "EnterThrowMode",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _throwExitMethod ??= typeof(vThrowManagerBase).GetMethod(
            "ExitThrowMode",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _throwIsAimingProp ??= typeof(vThrowManagerBase).GetProperty(
            "isAiming",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _throwInEnterModeProp ??= typeof(vThrowManagerBase).GetProperty(
            "inEnterThrowMode",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _throwIsThrowingProp ??= typeof(vThrowManagerBase).GetProperty(
            "isThrowing",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _throwPressInputProp ??= typeof(vThrowManagerBase).GetProperty(
            "pressThrowInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        bool isAiming       = _throwIsAimingProp    != null && (bool)_throwIsAimingProp.GetValue(_throwManager);
        bool inEnterMode    = _throwInEnterModeProp != null && (bool)_throwInEnterModeProp.GetValue(_throwManager);
        bool isThrowing     = _throwIsThrowingProp  != null && (bool)_throwIsThrowingProp.GetValue(_throwManager);

        // Numpad2 — toggle aim mode
        if (_throwAimPressed)
        {
            _throwAimPressed = false;

            if (!isAiming && !inEnterMode && !isThrowing)
            {
                // Only equip if we have grenades
                if (_throwManager.CurrentThrowAmount > 0)
                {
                    _throwEnterMethod?.Invoke(_throwManager, null);

                    if (debugMode)
                        Debug.Log("[InvectorInputBridge] Throw: EnterThrowMode()");
                }
            }
            else if ((isAiming || inEnterMode) && !isThrowing)
            {
                _throwExitMethod?.Invoke(_throwManager, null);

                if (debugMode)
                    Debug.Log("[InvectorInputBridge] Throw: ExitThrowMode() via cancel");
            }
        }

        // LMB / RT — launch the grenade while aiming
        if (_throwReleasePressed)
        {
            _throwReleasePressed = false;

            if (isAiming && !isThrowing)
            {
                _throwPressInputProp?.SetValue(_throwManager, true);

                if (debugMode)
                    Debug.Log("[InvectorInputBridge] Throw: pressThrowInput = true");
            }
        }
    }

    // Reflection cache for throw
    private System.Reflection.MethodInfo   _throwEnterMethod;
    private System.Reflection.MethodInfo   _throwExitMethod;
    private System.Reflection.PropertyInfo _throwIsAimingProp;
    private System.Reflection.PropertyInfo _throwInEnterModeProp;
    private System.Reflection.PropertyInfo _throwIsThrowingProp;
    private System.Reflection.PropertyInfo _throwPressInputProp;

    // ── Cover reflection cache ────────────────────────────────────────────────
    private const float CoverExitHoldTime = 0.2f;
    private float _coverHeldTimer;

    private System.Reflection.MethodInfo _handleEnterExitCoverMethod;
    private System.Reflection.MethodInfo _enterRoutineMethod;
    private System.Reflection.MethodInfo _goToRoutineMethod;
    private System.Reflection.MethodInfo _exitCoverMethod;
    private System.Reflection.FieldInfo  _wayPathField;
    private System.Reflection.FieldInfo  _currentCoverRoutineField;

    // Cached reflection access to vThirdPersonCamera.mouseX / mouseY (internal fields).
    // Used by aim assist to nudge the camera's orbit angles directly without going through
    // RotateCamera(), which would apply xMouseSensitivity/yMouseSensitivity a second time.
    private FieldInfo _cameraMouseXField;
    private FieldInfo _cameraMouseYField;

    /// <summary>
    /// Returns true when the currently equipped weapon has a scope target assigned,
    /// meaning it supports ScopeView. Used to resolve the leftStickPress gamepad
    /// conflict between Sprint and ScopeView.
    /// </summary>
    private bool IsCurrentWeaponScoped()
    {
        if (_shooterInput == null || _shooterInput.shooterManager == null) return false;
        var weapon = _shooterInput.shooterManager.CurrentWeapon;
        return weapon != null && weapon.scopeTarget != null;
    }

    private bool RollConditions(vThirdPersonController cc)
    {
        return (!cc.isRolling || cc.canRollAgain) && cc.isGrounded &&
               cc.input != Vector3.zero && !cc.customAction &&
               cc.currentStamina > cc.rollStamina && !cc.isJumping && !cc.isSliding;
    }

    /// <summary>Returns true when conditions allow a melee attack (mirrors MeleeAttackConditions via reflection).</summary>
    private bool CanMeleeAttack()
    {
        if (_meleeInput == null || _meleeInput.lockMeleeInput) return false;

        // Replicate vMeleeCombatInput.MeleeAttackConditions() without calling protected method.
        var cc = _tpInput.cc;
        if (cc == null) return false;
        return cc.isGrounded && !cc.customAction && !cc.isJumping &&
               !cc.isCrouching && !cc.isRolling && !_meleeInput.isEquipping &&
               !_meleeInput.animator.IsInTransition(cc.baseLayer);
    }

    /// <summary>Returns true when stamina allows a melee attack (mirrors MeleeAttackStaminaConditions via reflection).</summary>
    private bool MeleeAttackStaminaConditions()
    {
        if (_meleeInput == null) return false;
        var prop = typeof(vMeleeCombatInput).GetProperty("meleeManager",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // meleeManager field is internal, try direct field access
        var field = typeof(vMeleeCombatInput).GetField("meleeManager",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
        if (field == null) return true; // assume ok if we can't read it
        var mm = field.GetValue(_meleeInput) as Invector.vMelee.vMeleeManager;
        if (mm == null) return true;
        return _tpInput.cc.currentStamina - mm.GetAttackStaminaCost() >= 0f;
    }

    /// <summary>Sets isBlocking on vMeleeCombatInput via reflection (protected set accessor).</summary>
    private void SetMeleeBlocking(bool value)
    {
        if (_meleeInput == null) return;
        var prop = typeof(vMeleeCombatInput).GetProperty(
            "isBlocking",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(_meleeInput, value);
    }

    private bool MeleeAttackConditions()
    {
        return CanMeleeAttack();
    }

    /// <summary>
    /// Sets a property on vShooterMeleeInput via reflection — avoids coupling to
    /// protected members while still driving the shooter's own state machine.
    /// </summary>
    private void SetShooterInputFlag(string propertyName, bool value)
    {
        if (_shooterInput == null) return;
        var prop = typeof(vShooterMeleeInput).GetProperty(
            propertyName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        prop?.SetValue(_shooterInput, value);
    }

    /// <summary>
    /// Returns the current value of a bool flag and resets it to false.
    /// Used for continuous-sprint toggle where only the rising edge matters.
    /// </summary>
    private static bool ConsumeFlag(ref bool flag)
    {
        bool val = flag;
        flag = false;
        return val;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Enables or disables all player actions at runtime (e.g. during UI / cutscenes).
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        if (enabled)
            _actionMap.Enable();
        else
            _actionMap.Disable();

        // Keep inventory locked via its own flag when we're disabled
        if (_inventory != null)
            _inventory.lockInventoryInput = !enabled;

        if (debugMode)
            Debug.Log($"[InvectorInputBridge] Input {(enabled ? "enabled" : "disabled")}.");
    }

    /// <summary>
    /// Rebinds a single action to a new binding path at runtime.
    /// </summary>
    /// <param name="actionName">Name of the action (e.g. "Jump").</param>
    /// <param name="newPath">New Input System path (e.g. "&lt;Keyboard&gt;/space").</param>
    /// <param name="bindingIndex">Index of the binding to replace (default 0).</param>
    public void Rebind(string actionName, string newPath, int bindingIndex = 0)
    {
        InputAction action = _actionMap.FindAction(actionName, throwIfNotFound: false);
        if (action == null)
        {
            Debug.LogWarning($"[InvectorInputBridge] Action '{actionName}' not found.");
            return;
        }

        action.ApplyBindingOverride(bindingIndex, newPath);

        if (debugMode)
            Debug.Log($"[InvectorInputBridge] Rebound '{actionName}[{bindingIndex}]' → '{newPath}'.");
    }
}
