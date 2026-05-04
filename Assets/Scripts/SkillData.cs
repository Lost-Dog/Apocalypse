using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Division Game/Skill Data")]
public class SkillData : ScriptableObject
{
    public enum SkillType
    {
        MaxHealth         = 0,
        MaxMana           = 1,
        MaxStamina        = 2,
        HealthRegen       = 3,
        ManaRegen         = 4,
        StaminaRegen      = 5,
        Damage            = 6,
        CritChance        = 7,
        CritDamage        = 8,
        MovementSpeed     = 9,
        XPBonus           = 10,
        LootBonus         = 11,
        DamageReduction   = 12,
        CooldownReduction = 13,
        AttackSpeed       = 14,
    }

    [Header("Identity")]
    public int skillId;
    public string skillName;
    [TextArea(3, 6)] public string description;
    public Sprite icon;

    [Header("Requirements")]
    public int requiredLevel = 1;
    public int baseCost = 100;
    public int prerequisiteSkills;

    [Header("Progression")]
    public int maxLevel = 5;
    public float costMultiplierPerLevel = 1.5f;

    [Header("Effect")]
    public SkillType skillType;
    public float baseValue;
    public float valuePerLevel;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns the effect value at the given learned level (1-based).</summary>
    public float GetValueAtLevel(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);
        return baseValue + valuePerLevel * (level - 1);
    }

    /// <summary>Returns the skill point cost at the given learned level (1-based).</summary>
    public int GetCostAtLevel(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplierPerLevel, level - 1));
    }

    /// <summary>
    /// Compatibility shim used by SkillManager.GetTotalStatBonus(string).
    /// Returns baseValue when the skill's SkillType maps to the given stat name.
    /// </summary>
    public float GetStatBonus(string statName)
    {
        return string.Equals(SkillTypeToStatName(skillType), statName,
            System.StringComparison.OrdinalIgnoreCase) ? baseValue : 0f;
    }

    /// <summary>Returns true if this skill contributes to the named stat.</summary>
    public bool HasStatBonus(string statName)
    {
        return string.Equals(SkillTypeToStatName(skillType), statName,
            System.StringComparison.OrdinalIgnoreCase);
    }

    private static string SkillTypeToStatName(SkillType type)
    {
        switch (type)
        {
            case SkillType.MaxHealth:         return "MaxHealth";
            case SkillType.MaxMana:           return "MaxMana";
            case SkillType.MaxStamina:        return "MaxStamina";
            case SkillType.HealthRegen:       return "HealthRegen";
            case SkillType.ManaRegen:         return "ManaRegen";
            case SkillType.StaminaRegen:      return "StaminaRegen";
            case SkillType.Damage:            return "WeaponDamage";
            case SkillType.CritChance:        return "CritChance";
            case SkillType.CritDamage:        return "CritDamage";
            case SkillType.MovementSpeed:     return "MovementSpeed";
            case SkillType.XPBonus:           return "XPBonus";
            case SkillType.LootBonus:         return "LootBonus";
            case SkillType.DamageReduction:   return "DamageReduction";
            case SkillType.CooldownReduction: return "CooldownReduction";
            case SkillType.AttackSpeed:       return "AttackSpeed";
            default:                          return string.Empty;
        }
    }
}
