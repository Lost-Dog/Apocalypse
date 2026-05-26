using UnityEngine;

/// <summary>
/// Abstracts all player-specific data access so game systems remain independent
/// of the underlying character framework.
/// Implement this on a MonoBehaviour and assign it to every system that needs player data.
/// </summary>
public interface IPlayerProvider
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>The root GameObject of the player character.</summary>
    GameObject PlayerObject { get; }

    /// <summary>True when the player is alive (not dead or respawning).</summary>
    bool IsAlive { get; }

    // ── Health ────────────────────────────────────────────────────────────────

    float Health    { get; }
    float MaxHealth { get; }

    /// <summary>Sets health directly, clamped to [0, MaxHealth].</summary>
    void SetHealth(float value);

    /// <summary>
    /// Applies a damage amount (positive = damage, negative = heal).
    /// Damage is routed through armour first: armour absorbs all damage until it
    /// drops below 25% of its maximum, after which health begins taking damage.
    /// </summary>
    void ApplyDamage(float amount);

    // ── Armour ────────────────────────────────────────────────────────────────

    /// <summary>Current armour value.</summary>
    float Armour    { get; }

    /// <summary>Maximum armour value for the current level.</summary>
    float MaxArmour { get; }

    /// <summary>Sets armour directly, clamped to [0, MaxArmour].</summary>
    void SetArmour(float value);

    // ── Shield (legacy — kept for backward compatibility) ─────────────────────

    float Shield    { get; }
    float MaxShield { get; }

    /// <summary>Sets shield directly, clamped to [0, MaxShield].</summary>
    void SetShield(float value);

    // ── Movement ──────────────────────────────────────────────────────────────

    /// <summary>Current world-space velocity magnitude of the player character.</summary>
    float MoveSpeed { get; }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired once when the player dies.</summary>
    event System.Action OnDeath;

    /// <summary>Fired whenever Health changes. Passes (currentHealth, maxHealth).</summary>
    event System.Action<float, float> OnHealthChanged;

    /// <summary>Fired whenever Armour changes. Passes (currentArmour, maxArmour).</summary>
    event System.Action<float, float> OnArmourChanged;
}
