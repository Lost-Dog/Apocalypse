using Invector;
using Invector.vCharacterController;
using UnityEngine;

/// <summary>
/// IPlayerProvider implementation backed by Invector's vThirdPersonController.
/// Health is read from vHealthController (base class of the controller).
/// Movement speed is read from vThirdPersonMotor.moveSpeed.
/// Shield is not a native Invector concept; MaxShield is always 0.
/// Place this component on any persistent GameObject (e.g. GameSystems)
/// and assign it via the Inspector, or let dependent systems auto-find it.
/// </summary>
public class InvectorPlayerProvider : MonoBehaviour, IPlayerProvider
{
    private const string LogPrefix = "[InvectorPlayerProvider]";

    [Header("Player Reference")]
    [Tooltip("Assign the player GameObject that has vThirdPersonController. " +
             "If left empty, the component will search for one automatically on Start.")]
    [SerializeField] private vThirdPersonController playerController;

    // ── IPlayerProvider ───────────────────────────────────────────────────────

    public GameObject PlayerObject => playerController != null ? playerController.gameObject : null;
    public bool       IsAlive      => playerController != null && !playerController.isDead;

    public float Health    => playerController != null ? playerController.currentHealth : 0f;
    public float MaxHealth => playerController != null ? playerController.maxHealth      : 1f;

    /// <summary>Sets the player's health directly, clamped to [0, MaxHealth].</summary>
    public void SetHealth(float value)
    {
        if (playerController == null) return;
        float clamped = Mathf.Clamp(value, 0f, MaxHealth);
        playerController.ChangeHealth(Mathf.RoundToInt(clamped));
    }

    /// <summary>Applies damage to the player (positive = damage, negative = heal).</summary>
    public void ApplyDamage(float amount)
    {
        if (playerController == null) return;
        if (amount >= 0f)
        {
            // Reduce incoming damage by the trait defense reduction if available.
            float defenseReduction = PlayerTraitsRuntime.Instance != null
                ? PlayerTraitsRuntime.Instance.DefenseReduction
                : 0f;
            float finalAmount = amount * (1f - defenseReduction);

            // Route through Invector's TakeDamage so all its events fire correctly.
            var damage = new vDamage(Mathf.RoundToInt(finalAmount));
            playerController.TakeDamage(damage);
        }
        else
        {
            // Negative amount = heal — add health directly.
            playerController.AddHealth(Mathf.RoundToInt(-amount));
        }
    }

    // Invector has no built-in shield concept; expose zero so downstream
    // systems that check MaxShield > 0 before rendering a shield bar work correctly.
    public float Shield    => 0f;
    public float MaxShield => 0f;

    /// <summary>No-op — Invector has no shield system.</summary>
    public void SetShield(float value) { }

    public float MoveSpeed => playerController != null ? playerController.moveSpeed : 0f;

    public event System.Action           OnDeath;
    public event System.Action<float, float> OnHealthChanged;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (playerController == null)
            FindPlayerController();
    }

    private void OnDestroy()
    {
        UnbindController();
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

        // Broadcast the current health immediately so any subscriber that already
        // resolved this provider (e.g. PlayerHealthDisplay) gets the real value
        // without waiting for the first damage/heal event.
        OnHealthChanged?.Invoke(Health, MaxHealth);
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

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns health as a 0–1 normalised value.</summary>
    public float HealthNormalized => MaxHealth > 0f ? Health / MaxHealth : 0f;

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
