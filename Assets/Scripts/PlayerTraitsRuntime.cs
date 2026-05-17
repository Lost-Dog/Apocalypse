using Invector.vCharacterController;
using UnityEngine;

/// <summary>
/// Applies <see cref="PlayerTraitConfig"/> values to the Invector player controller at runtime.
/// Acts as the single source of truth for all effective player stats derived from level.
/// Place this component on the player GameObject or any persistent GameSystems object.
/// </summary>
public class PlayerTraitsRuntime : MonoBehaviour
{
    public static PlayerTraitsRuntime Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("ScriptableObject defining base stats and per-level scaling.")]
    [SerializeField] private PlayerTraitConfig config;

    [Header("Player Reference")]
    [Tooltip("Leave empty to auto-find by tag on Start.")]
    [SerializeField] private vThirdPersonController controller;

    // ── Effective runtime stats (read-only from outside) ──────────────────────

    public int   CurrentMaxHealth   { get; private set; }
    public float CurrentMaxStamina  { get; private set; }
    public float AttackMultiplier   { get; private set; }
    public float DefenseReduction   { get; private set; }
    public int   CurrentLevel       { get; private set; }

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        ResolveController();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes all traits for the given level and applies health and stamina
    /// directly to the Invector controller. Safe to call multiple times.
    /// </summary>
    public void ApplyLevel(int level)
    {
        if (config == null)
        {
            Debug.LogWarning("[PlayerTraitsRuntime] No PlayerTraitConfig assigned — traits not applied.");
            return;
        }

        ResolveController();

        CurrentLevel       = level;
        CurrentMaxHealth   = config.GetMaxHealth(level);
        CurrentMaxStamina  = config.GetMaxStamina(level);
        AttackMultiplier   = config.GetAttackMultiplier(level);
        DefenseReduction   = config.GetDefenseReduction(level);

        if (controller != null)
        {
            controller.maxHealth  = CurrentMaxHealth;
            controller.maxStamina = CurrentMaxStamina;
        }

        Debug.Log($"[PlayerTraitsRuntime] Level {level} — MaxHP: {CurrentMaxHealth}, " +
                  $"MaxStamina: {CurrentMaxStamina:F0}, " +
                  $"Attack: {AttackMultiplier:F2}x, " +
                  $"Defense: {DefenseReduction * 100f:F1}%");
    }

    /// <summary>
    /// Allows late-binding if the player controller spawns after this component's Start.
    /// </summary>
    public void BindToController(vThirdPersonController target)
    {
        controller = target;
        if (CurrentLevel > 0)
            ApplyLevel(CurrentLevel);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void ResolveController()
    {
        if (controller != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            controller = player.GetComponent<vThirdPersonController>();

        if (controller == null)
            controller = FindFirstObjectByType<vThirdPersonController>();

        if (controller == null)
            Debug.LogWarning("[PlayerTraitsRuntime] Could not find vThirdPersonController.");
    }
}
