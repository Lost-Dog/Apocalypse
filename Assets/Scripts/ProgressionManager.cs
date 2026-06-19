using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages player progression (level, XP, skill points) and applies stat
/// scaling to Game Creator 2 Traits.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("GC2 Traits")]
    [Tooltip("Assign the Game Creator 2 Traits component. Auto-found if left empty.")]
    [SerializeField] private Traits playerTraits;
    [SerializeField] private bool autoResolveTraitsOnPlayer = true;

    [Serializable]
    private class ScaledStat
    {
        public string statId;
        public float baseValue;
        public float valuePerLevel;
    }

    [Serializable]
    private class ScaledAttribute
    {
        public string attributeId;
        public float baseValue;
        public float valuePerLevel;
    }

    [Header("GC2 Level Scaling")]
    [Tooltip("Runtime Stats to scale with level using Base + (Level-1) * PerLevel.")]
    [SerializeField] private List<ScaledStat> scaledStats = new List<ScaledStat>();
    [Tooltip("Runtime Attributes to scale with level using Base + (Level-1) * PerLevel.")]
    [SerializeField] private List<ScaledAttribute> scaledAttributes = new List<ScaledAttribute>();
    [Tooltip("Optional Stat ID used as armour cap (for SurvivalManager sync).")]
    [SerializeField] private string maxArmourStatId = "max_armour";
    [Tooltip("Optional Attribute ID used as armour cap fallback when no armour stat exists.")]
    [SerializeField] private string maxArmourAttributeId = string.Empty;
    [Tooltip("In the Unity Editor, auto-create missing GC2 Stat assets/class entries for survival and progression IDs.")]
    [SerializeField] private bool autoCreateMissingGc2StatsFromSurvival = true;

    private static readonly string[] MaxHealthStatAliases =
    {
        "max_health", "health_max", "health", "maxhp", "max_hp"
    };

    private static readonly string[] MaxStaminaStatAliases =
    {
        "max_stamina", "stamina_max", "stamina"
    };

    private static readonly string[] MaxArmourStatAliases =
    {
        "max_armour", "max_armor", "armour_max", "armor_max", "armour", "armor"
    };

    private static readonly string[] MaxHungerStatAliases =
    {
        "max_hunger", "hunger_max", "hunger"
    };

    private static readonly string[] MaxThirstStatAliases =
    {
        "max_thirst", "thirst_max", "thirst"
    };

    private static readonly string[] MaxTemperatureStatAliases =
    {
        "max_temperature", "temperature_max", "temperature"
    };

    private static readonly string[] MaxInfectionStatAliases =
    {
        "max_infection", "infection_max", "immunity_max", "max_immunity"
    };

    private static readonly string[] AttackStatAliases =
    {
        "attack", "attack_power", "damage", "weapon_damage"
    };

    private static readonly string[] DefenseStatAliases =
    {
        "defense", "defence", "damage_reduction", "resistance"
    };

    [Header("Player Progression")]
    public int currentLevel = 1;
    public int currentXP    = 0;
    public int skillPoints  = 0;

    [Header("Gear Score")]
    public int currentGearScore  = 0;
    public int equippedGearScore = 0;

    [Header("Level Settings")]
    public int maxLevel = 1000;

    [Header("XP Formula")]
    [Tooltip("Cumulative XP to reach level N = floor(xpBase × N ^ xpExponent). " +
             "Raise xpExponent for a steeper curve (harder to level at higher tiers).")]
    public float xpBase     = 100f;
    public float xpExponent = 1.8f;

    [Header("Trait Reset")]
    [Tooltip("Seconds the player must wait before using the trait reset again.")]
    public float traitResetCooldown = 180f;   // 3 minutes

    [Header("Progression Events")]
    public UnityEvent<int> onLevelUp;
    public UnityEvent<int> onXPGained;
    public UnityEvent<int> onSkillPointGained;
    public UnityEvent<int> onGearScoreChanged;
    /// <summary>Fired when a trait reset is performed. Parameter is the cooldown duration in seconds.</summary>
    public UnityEvent<float> onTraitReset;

    // ── Private state ─────────────────────────────────────────────────────────

    private const int SkillPointsPerLevel = 1;

    private float _traitResetCooldownRemaining = 0f;
    private int _currentMaxArmour = 100;

    public int CurrentMaxArmour => _currentMaxArmour;

    /// <summary>Returns [0, 1] representing how much of the reset cooldown has elapsed (1 = ready).</summary>
    public float TraitResetCooldownProgress =>
        traitResetCooldown <= 0f ? 1f : 1f - Mathf.Clamp01(_traitResetCooldownRemaining / traitResetCooldown);

    /// <summary>True when the trait reset ability is off cooldown and can be used.</summary>
    public bool IsTraitResetReady => _traitResetCooldownRemaining <= 0f;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Resolve early so ApplyLevel in Start is guaranteed to find the runtime
        // even if Traits.Awake() runs after this Awake().
        ResolveTraits();
    }

    private void Start()
    {
        ResolveTraits();

        SurvivalManager survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();
        EnsureDefaultScalingConfigured(survivalManager);

#if UNITY_EDITOR
        if (BootstrapMissingGC2StatsFromSurvivalIfNeeded(survivalManager))
        {
            ResolveTraitsRuntimeCaches();
        }
#endif

        SyncXpStatToGC2();
        ApplyLevelToTraits(currentLevel);
    }

    private void Update()
    {
        if (_traitResetCooldownRemaining > 0f)
            _traitResetCooldownRemaining -= Time.deltaTime;
    }

    // ── Public API — XP & level ───────────────────────────────────────────────

    /// <summary>Adds experience points and triggers a level-up check.</summary>
    public void AddExperience(int amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;
        onXPGained?.Invoke(amount);
        SyncXpStatToGC2();

        Debug.Log($"Gained {amount} XP. Total: {currentXP}");

        CheckLevelUp();
    }

    // ── Public API — skill points ─────────────────────────────────────────────

    /// <summary>Spends one skill point. Returns false if none are available.</summary>
    public bool SpendSkillPoint()
    {
        if (skillPoints <= 0) return false;
        skillPoints--;
        return true;
    }

    /// <summary>Refunds one skill point (e.g. when unlearning a skill).</summary>
    public void RefundSkillPoint()
    {
        skillPoints++;
    }

    /// <summary>
    /// Resets all player traits to level 1 defaults: level, XP, skill points, and all
    /// stat scaling. Fired by holding LB+RB on a gamepad or Numpad0 on keyboard.
    /// Enforces a <see cref="traitResetCooldown"/> between uses.
    /// </summary>
    /// <returns>True if the reset was applied; false when still on cooldown.</returns>
    public bool ResetAllTraits()
    {
        if (!IsTraitResetReady)
        {
            Debug.Log($"[ProgressionManager] Trait reset on cooldown ({_traitResetCooldownRemaining:F1}s remaining).");
            return false;
        }

        currentLevel = 1;
        currentXP    = 0;
        skillPoints  = 0;

        SyncXpStatToGC2();
        ApplyLevelToTraits(currentLevel);

        onLevelUp?.Invoke(currentLevel);
        onTraitReset?.Invoke(traitResetCooldown);

        _traitResetCooldownRemaining = traitResetCooldown;

        Debug.Log("[ProgressionManager] All traits reset to level 1.");
        return true;
    }

    // ── Public API — XP progress helpers ─────────────────────────────────────

    /// <summary>Returns [0, 1] progress toward the next level.</summary>
    public float GetXPProgress()
    {
        if (currentLevel >= maxLevel) return 1f;

        int currentRequired = GetRequiredXPForLevel(currentLevel - 1);
        int nextRequired    = GetRequiredXPForLevel(currentLevel);

        return (float)(currentXP - currentRequired) / (nextRequired - currentRequired);
    }

    /// <summary>
    /// Returns the cumulative XP threshold that marks the start of <paramref name="level"/>.
    /// Formula: <c>floor(xpBase × level ^ xpExponent)</c>.
    /// level 0 always returns 0 (start of level 1).
    /// </summary>
    public int GetRequiredXPForLevel(int level)
    {
        if (level <= 0)   return 0;
        if (level >= maxLevel) return int.MaxValue;

        return Mathf.FloorToInt(xpBase * Mathf.Pow(level, xpExponent));
    }

    /// <summary>Returns XP remaining until the next level.</summary>
    public int GetXPToNextLevel()
    {
        if (currentLevel >= maxLevel) return 0;
        return GetRequiredXPForLevel(currentLevel) - currentXP;
    }

    public bool IsMaxLevel() => currentLevel >= maxLevel;

    // ── Public API — gear score ───────────────────────────────────────────────

    /// <summary>Updates the total inventory gear score and fires the change event.</summary>
    public void UpdateGearScore(int newGearScore)
    {
        int oldGearScore = currentGearScore;
        currentGearScore = newGearScore;

        if (oldGearScore != currentGearScore)
        {
            onGearScoreChanged?.Invoke(currentGearScore);
            Debug.Log($"Gear Score updated: {currentGearScore}");
        }
    }

    /// <summary>Updates the equipped-only gear score.</summary>
    public void UpdateEquippedGearScore(int newEquippedGearScore)
    {
        equippedGearScore = newEquippedGearScore;
    }

    /// <summary>Returns player level + equipped gear score contribution.</summary>
    public int   GetPowerLevel()      => currentLevel + (equippedGearScore / 100);
    public float GetPowerLevelFloat() => currentLevel + (equippedGearScore / 100f);

    /// <summary>Returns recommended gear score for the current level.</summary>
    public int GetRecommendedGearScore() => 100 + (currentLevel * 40);

    public bool IsUndergeared()
    {
        int recommended = GetRecommendedGearScore();
        return equippedGearScore < (recommended * 0.75f);
    }

    public bool IsOvergeared()
    {
        int recommended = GetRecommendedGearScore();
        return equippedGearScore > (recommended * 1.25f);
    }

    public float GetGearQuality()
    {
        int recommended = GetRecommendedGearScore();
        if (recommended == 0) return 1f;
        return Mathf.Clamp01((float)equippedGearScore / recommended);
    }

    // ── Private — level-up logic ──────────────────────────────────────────────

    private void CheckLevelUp()
    {
        if (currentLevel >= maxLevel) return;

        int requiredXP = GetRequiredXPForLevel(currentLevel);

        while (currentXP >= requiredXP && currentLevel < maxLevel)
        {
            LevelUp();
            requiredXP = GetRequiredXPForLevel(currentLevel);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        skillPoints += SkillPointsPerLevel;

        onLevelUp?.Invoke(currentLevel);
        onSkillPointGained?.Invoke(SkillPointsPerLevel);

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerLevel(currentLevel);

        ApplyLevelToTraits(currentLevel);

        Debug.Log($"LEVEL UP! Now level {currentLevel}. Skill Points: {skillPoints}");
    }

    // ── Private — GC2 traits application ─────────────────────────────────────

    private void ApplyLevelToTraits(int level)
    {
        ResolveTraits();
        if (playerTraits == null) return;

        EnsureDefaultScalingConfigured(SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>());

        for (int i = 0; i < scaledStats.Count; i++)
        {
            ScaledStat scaledStat = scaledStats[i];
            if (string.IsNullOrWhiteSpace(scaledStat.statId)) continue;

            RuntimeStatData runtimeStat = ResolveRuntimeStat(scaledStat.statId);
            if (runtimeStat == null)
            {
                Debug.LogWarning($"[ProgressionManager] Could not find GC2 Stat '{scaledStat.statId}' on '{playerTraits.gameObject.name}'.");
                continue;
            }

            runtimeStat.Base = EvaluateScaledValue(scaledStat.baseValue, scaledStat.valuePerLevel, level);
        }

        for (int i = 0; i < scaledAttributes.Count; i++)
        {
            ScaledAttribute scaledAttribute = scaledAttributes[i];
            if (string.IsNullOrWhiteSpace(scaledAttribute.attributeId)) continue;

            try
            {
                RuntimeAttributeData runtimeAttribute = playerTraits.RuntimeAttributes.Get(new IdString(scaledAttribute.attributeId));
                runtimeAttribute.Value = EvaluateScaledValue(scaledAttribute.baseValue, scaledAttribute.valuePerLevel, level);
            }
            catch (Exception)
            {
                Debug.LogWarning($"[ProgressionManager] Could not find GC2 Attribute '{scaledAttribute.attributeId}' on '{playerTraits.gameObject.name}'.");
            }
        }

        RefreshArmourCapFromTraits();
        SyncSurvivalManagerCapsFromTraits();
    }

    private static double EvaluateScaledValue(float baseValue, float valuePerLevel, int level)
    {
        return baseValue + valuePerLevel * Mathf.Max(0, level - 1);
    }

    /// <summary>
    /// Writes <see cref="currentXP"/> into the GC2 <c>xp</c> stat so that table-driven
    /// formulas (e.g. <c>table.level[stat[xp]]</c>) reflect the current progression state.
    /// </summary>
    private void SyncXpStatToGC2()
    {
        if (playerTraits == null) return;
        try
        {
            RuntimeStatData xpStat = playerTraits.RuntimeStats.Get(new IdString("xp"));
            if (xpStat != null)
                xpStat.Base = currentXP;
        }
        catch (Exception)
        {
            // xp stat not present on this Traits component — ignore.
        }
    }

    private void RefreshArmourCapFromTraits()
    {
        if (playerTraits == null) return;

        if (!string.IsNullOrWhiteSpace(maxArmourStatId))
        {
            RuntimeStatData armourStat = ResolveRuntimeStat(maxArmourStatId);
            if (armourStat != null)
            {
                _currentMaxArmour = Mathf.Max(0, Mathf.RoundToInt((float)armourStat.Value));
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(maxArmourAttributeId))
        {
            try
            {
                RuntimeAttributeData armourAttribute = playerTraits.RuntimeAttributes.Get(new IdString(maxArmourAttributeId));
                _currentMaxArmour = Mathf.Max(0, Mathf.RoundToInt((float)armourAttribute.Value));
            }
            catch (Exception)
            {
                // Keep last known value.
            }
        }
    }

    private void EnsureDefaultScalingConfigured(SurvivalManager survivalManager)
    {
        float survivalMaxStamina = survivalManager != null ? survivalManager.maxStamina : 200f;
        float survivalMaxArmour = survivalManager != null ? survivalManager.maxArmour : 100f;
        float survivalMaxHunger = survivalManager != null ? survivalManager.maxHunger : 100f;
        float survivalMaxThirst = survivalManager != null ? survivalManager.maxThirst : 100f;
        float survivalMaxTemperature = survivalManager != null ? survivalManager.maxTemperature : 100f;
        float survivalMaxInfection = survivalManager != null ? survivalManager.maxInfection : 100f;

        bool addedAny = false;
        addedAny |= EnsureScaledStatEntry("max_health", 100f, 20f);
        addedAny |= EnsureScaledStatEntry("max_stamina", survivalMaxStamina, 10f);
        addedAny |= EnsureScaledStatEntry("max_armour", survivalMaxArmour, 20f);
        addedAny |= EnsureScaledStatEntry("attack", 1f, 0.1f);
        addedAny |= EnsureScaledStatEntry("defense", 0f, 0.03f);

        // Survival systems read these caps directly; keep them represented in GC2 too.
        addedAny |= EnsureScaledStatEntry("max_hunger", survivalMaxHunger, 0f);
        addedAny |= EnsureScaledStatEntry("max_thirst", survivalMaxThirst, 0f);
        addedAny |= EnsureScaledStatEntry("max_temperature", survivalMaxTemperature, 0f);
        addedAny |= EnsureScaledStatEntry("max_infection", survivalMaxInfection, 0f);

        if (string.IsNullOrWhiteSpace(maxArmourStatId))
            maxArmourStatId = "max_armour";

        if (addedAny)
            Debug.Log("[ProgressionManager] Ensured default GC2 scaling map includes core + survival traits.");
    }

    private bool EnsureScaledStatEntry(string statId, float baseValue, float valuePerLevel)
    {
        if (string.IsNullOrWhiteSpace(statId)) return false;

        for (int i = 0; i < scaledStats.Count; i++)
        {
            if (string.Equals(scaledStats[i].statId, statId, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        scaledStats.Add(new ScaledStat
        {
            statId = statId,
            baseValue = baseValue,
            valuePerLevel = valuePerLevel
        });

        return true;
    }

    private RuntimeStatData FindRuntimeStatByAliases(IEnumerable<string> aliases)
    {
        if (playerTraits == null || aliases == null) return null;

        foreach (string alias in aliases)
        {
            if (string.IsNullOrWhiteSpace(alias)) continue;
            try
            {
                RuntimeStatData stat = playerTraits.RuntimeStats.Get(new IdString(alias));
                if (stat != null) return stat;
            }
            catch (Exception)
            {
                // Keep trying aliases.
            }
        }

        return null;
    }

    private RuntimeStatData ResolveRuntimeStat(string configuredId)
    {
        if (string.IsNullOrWhiteSpace(configuredId) || playerTraits == null) return null;

        string normalized = configuredId.Trim().ToLowerInvariant();
        if (normalized == "max_health" || normalized == "health")
            return FindRuntimeStatByAliases(MaxHealthStatAliases);

        if (normalized == "max_stamina" || normalized == "stamina")
            return FindRuntimeStatByAliases(MaxStaminaStatAliases);

        if (normalized == "max_armour" || normalized == "max_armor" || normalized == "armour" || normalized == "armor")
            return FindRuntimeStatByAliases(MaxArmourStatAliases);

        if (normalized == "max_hunger" || normalized == "hunger")
            return FindRuntimeStatByAliases(MaxHungerStatAliases);

        if (normalized == "max_thirst" || normalized == "thirst")
            return FindRuntimeStatByAliases(MaxThirstStatAliases);

        if (normalized == "max_temperature" || normalized == "temperature")
            return FindRuntimeStatByAliases(MaxTemperatureStatAliases);

        if (normalized == "max_infection" || normalized == "infection" || normalized == "immunity")
            return FindRuntimeStatByAliases(MaxInfectionStatAliases);

        if (normalized == "attack" || normalized == "attack_power" || normalized == "damage")
            return FindRuntimeStatByAliases(AttackStatAliases);

        if (normalized == "defense" || normalized == "defence" || normalized == "damage_reduction")
            return FindRuntimeStatByAliases(DefenseStatAliases);

        try
        {
            return playerTraits.RuntimeStats.Get(new IdString(configuredId));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void ResolveTraits()
    {
        if (playerTraits != null) return;

        if (autoResolveTraitsOnPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTraits = player.GetComponent<Traits>();
        }

        if (playerTraits == null)
            playerTraits = FindFirstObjectByType<Traits>();

        if (playerTraits == null)
            Debug.LogWarning("[ProgressionManager] Could not find GC2 Traits component — stat scaling disabled.");
        else
            Debug.Log($"[ProgressionManager] Resolved GC2 Traits on '{playerTraits.gameObject.name}'.");
    }

    private void SyncSurvivalManagerCapsFromTraits()
    {
        SurvivalManager survivalManager = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();
        if (survivalManager == null || playerTraits == null) return;

        TryApplySurvivalCap("max_stamina", value =>
        {
            survivalManager.maxStamina = Mathf.Max(1f, value);
            survivalManager.currentStamina = Mathf.Clamp(survivalManager.currentStamina, 0f, survivalManager.maxStamina);
        });

        TryApplySurvivalCap("max_hunger", value =>
        {
            survivalManager.maxHunger = Mathf.Max(1f, value);
            survivalManager.currentHunger = Mathf.Clamp(survivalManager.currentHunger, 0f, survivalManager.maxHunger);
        });

        TryApplySurvivalCap("max_thirst", value =>
        {
            survivalManager.maxThirst = Mathf.Max(1f, value);
            survivalManager.currentThirst = Mathf.Clamp(survivalManager.currentThirst, 0f, survivalManager.maxThirst);
        });

        TryApplySurvivalCap("max_temperature", value =>
        {
            survivalManager.maxTemperature = Mathf.Max(1f, value);
            survivalManager.currentTemperature = Mathf.Clamp(survivalManager.currentTemperature, 0f, survivalManager.maxTemperature);
        });

        TryApplySurvivalCap("max_infection", value =>
        {
            survivalManager.maxInfection = Mathf.Max(1f, value);
            survivalManager.currentInfection = Mathf.Clamp(survivalManager.currentInfection, 0f, survivalManager.maxInfection);
        });
    }

    private void TryApplySurvivalCap(string statId, Action<float> apply)
    {
        RuntimeStatData runtimeStat = ResolveRuntimeStat(statId);
        if (runtimeStat == null || apply == null) return;

        apply(Mathf.Max(0f, (float) runtimeStat.Value));
    }

#if UNITY_EDITOR
    private struct StatBootstrapDefinition
    {
        public string id;
        public string assetName;
        public float baseValue;
    }

    private bool BootstrapMissingGC2StatsFromSurvivalIfNeeded(SurvivalManager survivalManager)
    {
        if (!autoCreateMissingGc2StatsFromSurvival) return false;
        if (playerTraits == null || playerTraits.Class == null) return false;

        List<StatBootstrapDefinition> definitions = BuildBootstrapDefinitions(survivalManager);
        bool modifiedClass = false;

        for (int i = 0; i < definitions.Count; i++)
        {
            StatBootstrapDefinition definition = definitions[i];
            if (ClassContainsStatId(playerTraits.Class, definition.id)) continue;

            Stat statAsset = GetOrCreateGeneratedStatAsset(definition);
            if (statAsset == null) continue;

            AddStatToClass(playerTraits.Class, statAsset);
            modifiedClass = true;

            Debug.Log($"[ProgressionManager] Added missing GC2 stat '{definition.id}' to class '{playerTraits.Class.name}'.");
        }

        if (modifiedClass)
        {
            EditorUtility.SetDirty(playerTraits.Class);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return modifiedClass;
    }

    private List<StatBootstrapDefinition> BuildBootstrapDefinitions(SurvivalManager survivalManager)
    {
        List<StatBootstrapDefinition> definitions = new List<StatBootstrapDefinition>();

        float maxStamina = survivalManager != null ? survivalManager.maxStamina : 200f;
        float maxArmour = survivalManager != null ? survivalManager.maxArmour : 100f;
        float maxHunger = survivalManager != null ? survivalManager.maxHunger : 100f;
        float maxThirst = survivalManager != null ? survivalManager.maxThirst : 100f;
        float maxTemperature = survivalManager != null ? survivalManager.maxTemperature : 100f;
        float maxInfection = survivalManager != null ? survivalManager.maxInfection : 100f;

        AddBootstrapDefinition(definitions, "max_health", "Max_Health", 100f);
        AddBootstrapDefinition(definitions, "max_stamina", "Max_Stamina", maxStamina);
        AddBootstrapDefinition(definitions, "max_armour", "Max_Armour", maxArmour);
        AddBootstrapDefinition(definitions, "attack", "Attack", 1f);
        AddBootstrapDefinition(definitions, "defense", "Defense", 0f);
        AddBootstrapDefinition(definitions, "max_hunger", "Max_Hunger", maxHunger);
        AddBootstrapDefinition(definitions, "max_thirst", "Max_Thirst", maxThirst);
        AddBootstrapDefinition(definitions, "max_temperature", "Max_Temperature", maxTemperature);
        AddBootstrapDefinition(definitions, "max_infection", "Max_Infection", maxInfection);

        return definitions;
    }

    private void AddBootstrapDefinition(List<StatBootstrapDefinition> list, string id, string assetName, float fallbackBase)
    {
        for (int i = 0; i < scaledStats.Count; i++)
        {
            if (string.Equals(scaledStats[i].statId, id, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new StatBootstrapDefinition
                {
                    id = id,
                    assetName = assetName,
                    baseValue = scaledStats[i].baseValue
                });
                return;
            }
        }

        list.Add(new StatBootstrapDefinition
        {
            id = id,
            assetName = assetName,
            baseValue = fallbackBase
        });
    }

    private static string GetGeneratedStatsDirectory()
    {
        return "Assets/Data/GC2/GeneratedStats";
    }

    private Stat GetOrCreateGeneratedStatAsset(StatBootstrapDefinition definition)
    {
        string directory = GetGeneratedStatsDirectory();
        EnsureDirectoryExists(directory);

        string safeAssetName = string.IsNullOrWhiteSpace(definition.assetName)
            ? definition.id
            : definition.assetName;

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{safeAssetName}.asset");

        // Reuse by ID if an asset already exists in the target folder.
        string[] existingGuids = AssetDatabase.FindAssets("t:Stat", new[] { directory });
        for (int i = 0; i < existingGuids.Length; i++)
        {
            string existingPath = AssetDatabase.GUIDToAssetPath(existingGuids[i]);
            Stat existing = AssetDatabase.LoadAssetAtPath<Stat>(existingPath);
            if (existing != null && string.Equals(existing.ID.String, definition.id, StringComparison.OrdinalIgnoreCase))
            {
                return existing;
            }
        }

        Stat statAsset = ScriptableObject.CreateInstance<Stat>();

        SerializedObject statSerialized = new SerializedObject(statAsset);
        SerializedProperty idProperty = statSerialized.FindProperty("m_ID").FindPropertyRelative("m_String");
        if (idProperty != null)
            idProperty.stringValue = definition.id;

        SerializedProperty baseProperty = statSerialized.FindProperty("m_Data").FindPropertyRelative("m_Base");
        if (baseProperty != null)
            baseProperty.doubleValue = definition.baseValue;

        statSerialized.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(statAsset, assetPath);
        AssetDatabase.ImportAsset(assetPath);

        return statAsset;
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] chunks = path.Split('/');
        string current = chunks[0];

        for (int i = 1; i < chunks.Length; i++)
        {
            string next = current + "/" + chunks[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, chunks[i]);

            current = next;
        }
    }

    private bool ClassContainsStatId(Class classAsset, string statId)
    {
        if (classAsset == null || string.IsNullOrWhiteSpace(statId)) return false;

        int length = classAsset.StatsLength;
        for (int i = 0; i < length; i++)
        {
            StatItem item = classAsset.GetStat(i);
            if (item == null || item.Stat == null) continue;

            if (string.Equals(item.Stat.ID.String, statId, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void AddStatToClass(Class classAsset, Stat statAsset)
    {
        if (classAsset == null || statAsset == null) return;

        SerializedObject classSerialized = new SerializedObject(classAsset);
        SerializedProperty statsContainer = classSerialized.FindProperty("m_Stats");
        SerializedProperty statsArray = statsContainer != null
            ? statsContainer.FindPropertyRelative("m_Stats")
            : null;

        if (statsArray == null || !statsArray.isArray)
        {
            Debug.LogWarning("[ProgressionManager] Could not access Class.m_Stats serialized array.");
            return;
        }

        int index = statsArray.arraySize;
        statsArray.InsertArrayElementAtIndex(index);

        SerializedProperty itemProperty = statsArray.GetArrayElementAtIndex(index);
        itemProperty.managedReferenceValue = new StatItem();

        SerializedProperty hiddenProperty = itemProperty.FindPropertyRelative("m_IsHidden");
        if (hiddenProperty != null)
            hiddenProperty.boolValue = false;

        SerializedProperty statReferenceProperty = itemProperty.FindPropertyRelative("m_Stat");
        if (statReferenceProperty != null)
            statReferenceProperty.objectReferenceValue = statAsset;

        SerializedProperty changeBaseProperty = itemProperty.FindPropertyRelative("m_ChangeBase");
        if (changeBaseProperty != null)
        {
            SerializedProperty enabledProperty = changeBaseProperty.FindPropertyRelative("m_IsEnabled");
            if (enabledProperty != null)
                enabledProperty.boolValue = false;
        }

        SerializedProperty changeFormulaProperty = itemProperty.FindPropertyRelative("m_ChangeFormula");
        if (changeFormulaProperty != null)
        {
            SerializedProperty enabledProperty = changeFormulaProperty.FindPropertyRelative("m_IsEnabled");
            if (enabledProperty != null)
                enabledProperty.boolValue = false;
        }

        classSerialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private void ResolveTraitsRuntimeCaches()
    {
        if (playerTraits == null) return;

        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type traitsType = typeof(Traits);

        traitsType.GetField("m_RuntimeStats", flags)?.SetValue(playerTraits, null);
        traitsType.GetField("m_RuntimeAttributes", flags)?.SetValue(playerTraits, null);
        traitsType.GetField("m_RuntimeStatusEffects", flags)?.SetValue(playerTraits, null);

        // Accessing the properties forces GC2 to rebuild runtime caches.
        _ = playerTraits.RuntimeStats;
        _ = playerTraits.RuntimeAttributes;
        _ = playerTraits.RuntimeStatusEffects;
    }
#endif
}
