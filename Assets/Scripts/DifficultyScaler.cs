using GameCreator.Runtime.Stats;
using System;
using UnityEngine;

/// <summary>
/// Scales enemy health and damage based on player/zone level.
/// Health scaling sets the character's current HP via GC2 Stats RuntimeAttributeData.
/// Note: the GC2 Stat that drives the attribute's MaxValue is not overridden here —
/// max health scaling requires adjusting the underlying Stat value via the formula system.
/// </summary>
public class DifficultyScaler : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Base Stats")]
    [Tooltip("Base health at level 1")]
    public float baseHealth = 100f;

    [Tooltip("Base damage at level 1")]
    public float baseDamage = 10f;

    [Header("Scaling Multipliers")]
    [Tooltip("Health increase per level (0.2 = 20% increase)")]
    public float healthMultiplierPerLevel = 0.2f;

    [Tooltip("Damage increase per level (0.15 = 15% increase)")]
    public float damageMultiplierPerLevel = 0.15f;

    [Header("Current Stats")]
    [Tooltip("Current difficulty level")]
    public int currentLevel = 1;

    [Tooltip("Calculated health after scaling")]
    public float scaledHealth;

    [Tooltip("Calculated damage after scaling")]
    public float scaledDamage;

    [Header("Auto-Scaling")]
    [Tooltip("Automatically scale to player level on spawn")]
    public bool autoScaleToPlayerLevel = false;

    [Tooltip("Use zone level if higher than player level")]
    public bool respectZoneLevel = true;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private Traits traits;
    private bool hasAppliedScaling = false;

    private void Start()
    {
        traits = GetComponent<Traits>();

        if (traits != null && baseHealth <= 0f)
        {
            try
            {
                RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
                baseHealth = (float) health.MaxValue;
            }
            catch (Exception) { }
        }

        if (autoScaleToPlayerLevel)
            ApplyScaling(GetEffectiveLevel());
    }

    private void OnEnable()
    {
        hasAppliedScaling = false;

        if (autoScaleToPlayerLevel)
            ApplyScaling(GetEffectiveLevel());
    }

    // PUBLIC API ---------------------------------------------------------------------------------

    /// <summary>
    /// Applies difficulty scaling for the given level.
    /// Sets the character's current HP via GC2 Stats and notifies PoolableCharacter.
    /// </summary>
    public void ApplyScaling(int level)
    {
        if (level < 1)
        {
            Debug.LogWarning($"{gameObject.name}: Invalid level {level}, using level 1");
            level = 1;
        }

        currentLevel  = level;
        scaledHealth  = CalculateScaledHealth(level);
        scaledDamage  = CalculateScaledDamage(level);

        ApplyStatsToCharacter();
        hasAppliedScaling = true;

        if (showDebugLogs)
            Debug.Log($"{gameObject.name} scaled to level {level}: HP={scaledHealth:F0} (base: {baseHealth}), Dmg={scaledDamage:F1}");
    }

    // PRIVATE ------------------------------------------------------------------------------------

    private float CalculateScaledHealth(int level) =>
        baseHealth * (1f + (level - 1) * healthMultiplierPerLevel);

    private float CalculateScaledDamage(int level) =>
        baseDamage * (1f + (level - 1) * damageMultiplierPerLevel);

    private void ApplyStatsToCharacter()
    {
        if (traits == null) traits = GetComponent<Traits>();

        if (traits != null)
        {
            try
            {
                RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
                // Value setter clamps to the attribute's current MaxValue (driven by GC2 Stat formula).
                // To truly scale max HP, adjust the underlying Stat via the GC2 Stats formula system.
                health.Value = scaledHealth;
            }
            catch (Exception e)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"{gameObject.name}: DifficultyScaler could not set health attribute — {e.Message}");
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"{gameObject.name}: DifficultyScaler could not find Traits component!");
        }

        // Notify PoolableCharacter so health resets correctly on re-spawn
        PoolableCharacter poolable = GetComponent<PoolableCharacter>();
        if (poolable != null)
            poolable.SetScaledHealth(scaledHealth);
    }

    private int GetEffectiveLevel()
    {
        if (GameManager.Instance != null)
        {
            int playerLevel = GameManager.Instance.currentPlayerLevel;
            return respectZoneLevel ? Mathf.Max(currentLevel, playerLevel) : playerLevel;
        }

        return currentLevel;
    }

    // HELPERS ------------------------------------------------------------------------------------

    public float GetHealthMultiplier() => baseHealth > 0 ? scaledHealth / baseHealth : 1f;

    public float GetDamageMultiplier() => baseDamage > 0 ? scaledDamage / baseDamage : 1f;

    public void SetBaseStats(float health, float damage)
    {
        baseHealth = health;
        baseDamage = damage;
        hasAppliedScaling = false;
    }

    public void ReapplyScaling()
    {
        hasAppliedScaling = false;
        ApplyScaling(currentLevel);
    }
}
