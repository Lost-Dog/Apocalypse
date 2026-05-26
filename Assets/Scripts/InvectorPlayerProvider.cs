using Invector;
using Invector.vCharacterController;
using UnityEngine;

/// <summary>
/// IPlayerProvider implementation backed by Invector's vThirdPersonController.
/// Health is read from vHealthController (base class of the controller).
/// Movement speed is read from vThirdPersonMotor.moveSpeed.
///
/// Armour is tracked internally (Invector has no shield concept).
/// All incoming damage is routed through armour first. Armour must drop below
/// 25% of its maximum before health starts taking damage.
///
/// Place this component on any persistent GameObject (e.g. GameSystems)
/// and assign it via the Inspector, or let dependent systems auto-find it.
/// </summary>
public class InvectorPlayerProvider : MonoBehaviour, IPlayerProvider
{
    private const string LogPrefix = "[InvectorPlayerProvider]";

    /// <summary>
    /// Armour threshold fraction. While armour / maxArmour > this value,
    /// all incoming damage is absorbed by armour and health is untouched.
    /// </summary>
    private const float ArmourThreshold = 0.25f;

    [Header("Player Reference")]
    [Tooltip("Assign the player GameObject that has vThirdPersonController. " +
             "If left empty, the component will search for one automatically on Start.")]
    [SerializeField] private vThirdPersonController playerController;

    // ── Armour state ──────────────────────────────────────────────────────────

    private float _currentArmour;
    private float _maxArmour;

    // ── IPlayerProvider — identity ─────────────────────────────────────────────

    public GameObject PlayerObject => playerController != null ? playerController.gameObject : null;
    public bool       IsAlive      => playerController != null && !playerController.isDead;

    // ── IPlayerProvider — health ───────────────────────────────────────────────

    public float Health    => playerController != null ? playerController.currentHealth : 0f;
    public float MaxHealth => playerController != null ? playerController.maxHealth      : 1f;

    /// <summary>Sets the player's health directly, clamped to [0, MaxHealth].</summary>
    public void SetHealth(float value)
    {
        if (playerController == null) return;
        float clamped = Mathf.Clamp(value, 0f, MaxHealth);
        playerController.ChangeHealth(Mathf.RoundToInt(clamped));
    }

    /// <summary>
    /// Applies damage to the player with armour interception.
    /// Positive = damage, negative = heal (bypasses armour).
    /// While armour is above 25% of max, all damage is absorbed by armour.
    /// Once armour drops to or below 25%, overflow damage hits health.
    /// </summary>
    public void ApplyDamage(float amount)
    {
        if (playerController == null) return;

        if (amount < 0f)
        {
            // Negative amount = heal — bypass armour entirely.
            playerController.AddHealth(Mathf.RoundToInt(-amount));
            return;
        }

        // Apply trait defense reduction first.
        float defenseReduction = PlayerTraitsRuntime.Instance != null
            ? PlayerTraitsRuntime.Instance.DefenseReduction
            : 0f;
        float incoming = amount * (1f - defenseReduction);

        // ── Armour interception ────────────────────────────────────────────────
        float armourFloor = _maxArmour * ArmourThreshold;

        if (_currentArmour > armourFloor)
        {
            // Armour is above the threshold — absorb as much as possible.
            float armourAbsorb = Mathf.Min(incoming, _currentArmour - armourFloor);
            SetArmour(_currentArmour - armourAbsorb);
            incoming -= armourAbsorb;

            // If armour absorption ate all the damage, health is untouched.
            if (incoming <= 0f) return;
        }

        // Health takes whatever damage remains after armour absorption.
        var damage = new vDamage(Mathf.RoundToInt(incoming));
        playerController.TakeDamage(damage);
    }

    // ── IPlayerProvider — armour ───────────────────────────────────────────────

    public float Armour    => _currentArmour;
    public float MaxArmour => _maxArmour;

    /// <summary>Sets armour, clamped to [0, MaxArmour], and fires OnArmourChanged.</summary>
    public void SetArmour(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, _maxArmour);
        if (Mathf.Approximately(clamped, _currentArmour)) return;
        _currentArmour = clamped;
        OnArmourChanged?.Invoke(_currentArmour, _maxArmour);
    }

    /// <summary>Updates the armour cap (called by SurvivalManager on level-up) and refills to new cap.</summary>
    public void SetMaxArmour(float newMax, bool refillToMax = false)
    {
        _maxArmour = Mathf.Max(0f, newMax);
        if (refillToMax)
            _currentArmour = _maxArmour;
        else
            _currentArmour = Mathf.Min(_currentArmour, _maxArmour);
        OnArmourChanged?.Invoke(_currentArmour, _maxArmour);
    }

    // ── IPlayerProvider — shield (legacy) ─────────────────────────────────────

    // Redirect legacy Shield reads to Armour so any old code still compiles.
    public float Shield    => _currentArmour;
    public float MaxShield => _maxArmour;

    /// <summary>Legacy — maps to SetArmour.</summary>
    public void SetShield(float value) => SetArmour(value);

    // ── IPlayerProvider — movement ─────────────────────────────────────────────

    public float MoveSpeed => playerController != null ? playerController.moveSpeed : 0f;

    // ── IPlayerProvider — events ───────────────────────────────────────────────

    public event System.Action                OnDeath;
    public event System.Action<float, float>  OnHealthChanged;
    public event System.Action<float, float>  OnArmourChanged;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Start()
    {
        if (playerController == null)
            FindPlayerController();

        // Sync armour cap from PlayerTraitsRuntime if already initialised.
        if (PlayerTraitsRuntime.Instance != null)
        {
            int cap = PlayerTraitsRuntime.Instance.CurrentMaxArmour;
            if (cap > 0) SetMaxArmour(cap, refillToMax: true);

            PlayerTraitsRuntime.Instance.OnArmourCapChanged += OnArmourCapChanged_Handler;
        }
    }

    private void OnDestroy()
    {
        UnbindController();

        if (PlayerTraitsRuntime.Instance != null)
            PlayerTraitsRuntime.Instance.OnArmourCapChanged -= OnArmourCapChanged_Handler;
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void FindPlayerController()
    {
        playerController = FindFirstObjectByType<vThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogWarning($"{LogPrefix} Could not find a vThirdPersonController in the scene.");
            return;
        }

        BindController();
    }

    private void BindController()
    {
        if (playerController == null) return;

        playerController.onDead.AddListener(HandleDeath);
        playerController.onChangeHealth.AddListener(HandleHealthChanged);

        Debug.Log($"{LogPrefix} Bound to '{playerController.name}'.");

        // Broadcast current values immediately so any subscriber gets the real values
        // without waiting for the first damage/heal event.
        OnHealthChanged?.Invoke(Health, MaxHealth);
        OnArmourChanged?.Invoke(_currentArmour, _maxArmour);
    }

    private void UnbindController()
    {
        if (playerController == null) return;

        playerController.onDead.RemoveListener(HandleDeath);
        playerController.onChangeHealth.RemoveListener(HandleHealthChanged);
    }

    private void HandleDeath(GameObject deadObject)
    {
        OnDeath?.Invoke();
        OnHealthChanged?.Invoke(0f, MaxHealth);
    }

    private void HandleHealthChanged(float newHealth)
    {
        OnHealthChanged?.Invoke(newHealth, MaxHealth);
    }

    private void OnArmourCapChanged_Handler(int newMax)
    {
        // On level-up, extend the armour cap. Do not refill — the player keeps
        // whatever armour they had and gains only the additional headroom.
        SetMaxArmour(newMax, refillToMax: false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns health as a 0–1 normalised value.</summary>
    public float HealthNormalized => MaxHealth > 0f ? Health / MaxHealth : 0f;

    /// <summary>Returns armour as a 0–1 normalised value.</summary>
    public float ArmourNormalized => _maxArmour > 0f ? _currentArmour / _maxArmour : 0f;

    /// <summary>
    /// Allows late binding if the player spawns after this component's Start.
    /// Call this after the Invector player is instantiated.
    /// </summary>
    public void BindToController(vThirdPersonController controller)
    {
        UnbindController();
        playerController = controller;
        BindController();
    }
}
