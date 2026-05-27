using UnityEngine;

/// <summary>
/// ScriptableObject defining the player's base stats and per-level scaling.
/// Create an instance via Assets > Create > Apocalypse > Player Trait Config.
/// </summary>
[CreateAssetMenu(fileName = "PlayerTraitConfig", menuName = "Apocalypse/Player Trait Config")]
public class PlayerTraitConfig : ScriptableObject
{
    [Header("Base Stats (Level 1)")]
    public int   baseMaxHealth  = 100;
    public float baseMaxStamina = 200f;

    [Tooltip("Flat damage multiplier at level 1. 1.0 = no bonus.")]
    public float baseAttack = 1f;

    [Tooltip("Fraction of incoming damage absorbed at level 1. 0 = none, 0.9 = 90% max.")]
    [Range(0f, 0.9f)]
    public float baseDefense = 0f;

    [Header("Base Stats (Level 1) — Armour")]
    [Tooltip("Base armour pool at level 1. Armour must be depleted below 25% before health takes damage.")]
    public int baseArmour = 100;

    [Header("Per-Level Scaling")]
    [Tooltip("Flat health added for each level above 1.")]
    public int   healthPerLevel  = 20;

    [Tooltip("Flat stamina added for each level above 1.")]
    public float staminaPerLevel = 10f;

    [Tooltip("Attack multiplier increase per level. E.g. 0.05 = +5% per level.")]
    public float attackPerLevel = 0.05f;

    [Tooltip("Damage reduction increase per level. E.g. 0.02 = +2% per level.")]
    public float defensePerLevel = 0.02f;

    [Tooltip("Flat armour added for each level above 1.")]
    public int armourPerLevel = 20;

    [Header("Per-Level Scaling — Magazine")]
    [Tooltip("Base magazine bonus added at level 1 on top of the weapon's default clip size. 0 = no bonus at level 1.")]
    public int baseMagazineBonus = 0;

    [Tooltip("Extra bullets added to every equipped weapon's magazine per level above 1.")]
    public int magazineBonusPerLevel = 5;

    // ── Computed values ───────────────────────────────────────────────────────

    /// <summary>Returns the max health value for the given level.</summary>
    public int GetMaxHealth(int level)
        => baseMaxHealth + healthPerLevel * Mathf.Max(0, level - 1);

    /// <summary>Returns the max stamina value for the given level.</summary>
    public float GetMaxStamina(int level)
        => baseMaxStamina + staminaPerLevel * Mathf.Max(0f, level - 1);

    /// <summary>Returns the attack damage multiplier for the given level.</summary>
    public float GetAttackMultiplier(int level)
        => baseAttack + attackPerLevel * Mathf.Max(0f, level - 1);

    /// <summary>
    /// Returns the damage reduction fraction [0, 0.9] for the given level.
    /// A value of 0.3 means 30% of incoming damage is absorbed.
    /// </summary>
    public float GetDefenseReduction(int level)
        => Mathf.Clamp(baseDefense + defensePerLevel * Mathf.Max(0f, level - 1), 0f, 0.9f);

    /// <summary>Returns the max armour pool for the given level.</summary>
    public int GetMaxArmour(int level)
        => baseArmour + armourPerLevel * Mathf.Max(0, level - 1);

    /// <summary>
    /// Returns the flat bonus added to every weapon's base clip size for the given level.
    /// At level 1 this equals <see cref="baseMagazineBonus"/>; each subsequent level
    /// adds <see cref="magazineBonusPerLevel"/> on top.
    /// </summary>
    public int GetMagazineBonus(int level)
        => baseMagazineBonus + magazineBonusPerLevel * Mathf.Max(0, level - 1);
}
