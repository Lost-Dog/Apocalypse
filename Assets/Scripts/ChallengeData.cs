using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Challenge", menuName = "Division Game/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    public enum ChallengeType
    {
        SupplyDrop,
        CivilianRescue,
        ControlPoint,
        HostageRescue,
        ExtractionDefense,
        BossEncounter,
        RivalAgent
    }

    public enum ChallengeFrequency
    {
        WorldEvent,
        Daily,
        Weekly
    }

    public enum ChallengeDifficulty
    {
        Easy,
        Medium,
        Hard,
        Extreme
    }
    
    public enum SpawnLocationType
    {
        RandomInRadius,
        AtCenter,
        RandomOnEdge,
        AroundPerimeter,
        Grid
    }
    
    public enum SpawnableCategory
    {
        Enemy,
        Civilian,
        LootBox,
        Objective,
        Prop,
        Cover,
        Vehicle,
        Boss,
        Other
    }
    
    [Header("Challenge Info")]
    public string challengeName;
    [TextArea(3, 6)] public string description;
    public ChallengeType challengeType;
    public ChallengeFrequency frequency = ChallengeFrequency.WorldEvent;
    public ChallengeDifficulty difficulty = ChallengeDifficulty.Medium;
    
    [Header("Minimap Icon")]
    public Lovatto.MiniMap.bl_MiniMapIconData iconData;
    
    [Header("Requirements")]
    public int recommendedLevel = 1;
    public int requiredPlayerLevel = 1;
    public float timeLimit = 600f;
    public float detectionRadius = 50f;
    
    [Header("Player Agency")]
    [Tooltip("Require player to manually start challenge (vs auto-start on proximity)")]
    public bool requireManualStart = false;
    
    [Tooltip("Allow players to retry after failure")]
    public bool allowRetry = true;
    
    [Tooltip("Seconds before retry is available (0 = immediate)")]
    public float retryCooldownSeconds = 60f;
    
    [Tooltip("Maximum retry attempts (0 = unlimited)")]
    public int maxAttempts = 0;
    
    [Tooltip("Show discovery notification when player gets in range")]
    public bool showDiscoveryNotification = true;
    
    [Header("Objectives")]
    public bool requireNoDeaths = false;
    public bool requireStealth = false;
    public string objectiveDescription = "Complete the challenge";
    
    [Header("Flexible Spawning System")]
    [Tooltip("Shared spawn point pool - any spawnable can use any point")]
    public Transform[] sharedSpawnPoints;
    
    [Tooltip("Add any prefabs you want to spawn for this challenge")]
    public List<SpawnableItem> spawnItems = new List<SpawnableItem>();
    
    [System.Serializable]
    public class SpawnableItem
    {
        [Header("Prefab Setup")]
        [Tooltip("Name for organization (optional)")]
        public string itemName;
        
        [Tooltip("The prefab to spawn")]
        public GameObject prefab;
        
        [Tooltip("Category for organization and filtering")]
        public SpawnableCategory category = SpawnableCategory.Other;
        
        [Header("Spawn Count")]
        [Tooltip("How many to spawn (exact count if min = max)")]
        public int minCount = 1;
        public int maxCount = 1;
        
        [Tooltip("Pool mode: pick randomly from list instead of spawning all")]
        public bool usePoolMode = false;
        
        [Header("Spawn Position")]
        [Tooltip("Use specific spawn transforms instead of random positions")]
        public Transform[] customSpawnPoints;
        
        [Tooltip("How to position the spawned objects")]
        public SpawnLocationType spawnLocation = SpawnLocationType.RandomInRadius;
        
        [Tooltip("Distance from challenge center")]
        public float spawnRadius = 15f;
        
        [Tooltip("Specific offset from center (only used with AtCenter)")]
        public Vector3 offset = Vector3.zero;
        
        [Header("Advanced Settings")]
        [Tooltip("Snap to NavMesh (required for AI)")]
        public bool requireNavMesh = true;
        
        [Tooltip("Random rotation on Y axis")]
        public bool randomRotation = true;
        
        [Tooltip("Custom rotation (if not random)")]
        public Vector3 fixedRotation = Vector3.zero;
        
        [Tooltip("Spawn priority (higher = spawns first)")]
        public int priority = 0;
        
        [Tooltip("Must spawn successfully (warning if failed)")]
        public bool required = false;
    }
    
    [Header("Challenge Modifiers")]
    [Tooltip("Active modifiers that change challenge behavior")]
    public List<ChallengeModifier> modifiers = new List<ChallengeModifier>();
    
    [System.Serializable]
    public class ChallengeModifier
    {
        public ModifierType type;
        public float value = 1.0f;
        public bool isActive = true;
        
        public enum ModifierType
        {
            // Enemy Modifiers
            IncreasedEnemyHealth,      // Multiplies enemy health
            IncreasedEnemyDamage,      // Multiplies enemy damage
            IncreasedEnemySpeed,       // Multiplies enemy movement speed
            IncreasedEnemyAccuracy,    // Improves enemy aim
            EliteEnemiesOnly,          // All enemies are elite variants
            
            // Environmental Modifiers
            TimeTrial,                 // Reduced time limit
            NightMode,                 // Forces night time/reduced visibility
            LimitedAmmo,               // Player starts with limited ammo
            NoHealthRegen,             // Disables health regeneration
            
            // Reward Modifiers
            DoubleXP,                  // 2x XP rewards
            DoubleCurrency,            // 2x Currency rewards
            BonusLootDrop,             // Extra loot drops
            GuaranteedRareLoot,        // Guarantees rare+ loot
            
            // Challenge Specific
            SurvivalMode,              // Endless waves until failure
            IronMan,                   // One death = failure
            Pacifist,                  // Complete without killing (if possible)
            SpeedRunner,               // Bonus rewards for fast completion
            PerfectScore               // No damage taken bonus
        }
    }
    
    [Header("Rewards")]
    public int baseXPReward = 200;
    public int baseCurrencyReward = 100;
    public LootManager.Rarity guaranteedLootRarity = LootManager.Rarity.Common;
    public int guaranteedLootCount = 1;
    public List<GameObject> bonusRewards = new List<GameObject>();
    
    [Header("Bonus Rewards")]
    [Tooltip("Award bonus XP for perfect completion (no deaths)")]
    public bool perfectCompletionBonus = true;
    public float perfectCompletionXPMultiplier = 1.5f;
    
    [Tooltip("Award bonus for speed completion")]
    public bool speedCompletionBonus = true;
    public float speedThresholdPercentage = 0.5f; // Complete in <50% of time limit
    public float speedCompletionXPMultiplier = 1.25f;
    
    [Tooltip("Award bonus for stealth completion")]
    public bool stealthCompletionBonus = false;
    public float stealthCompletionXPMultiplier = 1.3f;
    
    [Header("Difficulty Scaling")]
    [Tooltip("Enable automatic difficulty scaling based on player level")]
    public bool enableDifficultyScaling = true;
    
    [Tooltip("Scale enemy health based on player level")]
    public bool scaleEnemyHealth = true;
    
    [Tooltip("Scale enemy damage based on player level")]
    public bool scaleEnemyDamage = true;
    
    [Tooltip("Scale rewards based on actual difficulty completed")]
    public bool scaleRewards = true;
    
    [Tooltip("Health multiplier per difficulty tier (Easy=0.75, Medium=1.0, Hard=1.5, Extreme=2.0)")]
    public float healthScalingMultiplier = 1.0f;
    
    [Tooltip("Damage multiplier per difficulty tier")]
    public float damageScalingMultiplier = 1.0f;
    
    [Tooltip("Reward multiplier per difficulty tier")]
    public float rewardScalingMultiplier = 1.0f;
    
    [Header("UI")]
    public Sprite challengeIcon;
    public Color difficultyColor = Color.white;
    
    [Header("Audio")]
    public AudioClip startSound;
    public AudioClip completeSound;
    public AudioClip failSound;
    
    [Header("Visual Effects")]
    [Tooltip("VFX to spawn at challenge location when it starts")]
    public GameObject spawnVFX;
    [Tooltip("Scale of the spawn VFX")]
    public float spawnVFXScale = 1f;
    [Tooltip("Auto-destroy VFX after X seconds (0 = never)")]
    public float spawnVFXDuration = 0f;

    public string GetDifficultyText()
    {
        switch (difficulty)
        {
            case ChallengeDifficulty.Easy: return "EASY";
            case ChallengeDifficulty.Medium: return "MEDIUM";
            case ChallengeDifficulty.Hard: return "HARD";
            case ChallengeDifficulty.Extreme: return "EXTREME";
            default: return "UNKNOWN";
        }
    }

    public Color GetDifficultyColor()
    {
        switch (difficulty)
        {
            case ChallengeDifficulty.Easy: return Color.green;
            case ChallengeDifficulty.Medium: return Color.yellow;
            case ChallengeDifficulty.Hard: return new Color(1f, 0.5f, 0f);
            case ChallengeDifficulty.Extreme: return Color.red;
            default: return Color.white;
        }
    }
    
    public int GetEnemyCount()
    {
        int count = 0;
        foreach (var item in spawnItems)
        {
            if (item.category == SpawnableCategory.Enemy || item.category == SpawnableCategory.Boss)
            {
                count += item.maxCount;
            }
        }
        return count;
    }
    
    public int GetCivilianCount()
    {
        int count = 0;
        foreach (var item in spawnItems)
        {
            if (item.category == SpawnableCategory.Civilian)
            {
                count += item.maxCount;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Calculate the actual difficulty tier based on player level and recommended level
    /// </summary>
    public ChallengeDifficulty CalculateScaledDifficulty(int playerLevel)
    {
        if (!enableDifficultyScaling)
            return difficulty;
        
        int levelDifference = playerLevel - recommendedLevel;
        
        // Player is much lower level - make it harder
        if (levelDifference <= -3)
            return ChallengeDifficulty.Extreme;
        else if (levelDifference == -2 || levelDifference == -1)
            return ChallengeDifficulty.Hard;
        // Player is at recommended level
        else if (levelDifference == 0 || levelDifference == 1)
            return difficulty; // Use base difficulty
        // Player is higher level - make it easier
        else if (levelDifference == 2 || levelDifference == 3)
        {
            // Step down one difficulty tier
            int diffIndex = Mathf.Max(0, (int)difficulty - 1);
            return (ChallengeDifficulty)diffIndex;
        }
        else if (levelDifference >= 4)
        {
            // Step down two difficulty tiers
            int diffIndex = Mathf.Max(0, (int)difficulty - 2);
            return (ChallengeDifficulty)diffIndex;
        }
        
        return difficulty;
    }
    
    /// <summary>
    /// Get enemy health multiplier based on difficulty and player level
    /// </summary>
    public float GetEnemyHealthMultiplier(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        if (!scaleEnemyHealth)
            return 1.0f;
        
        float baseMultiplier = 1.0f;
        
        // Difficulty tier multiplier
        switch (actualDifficulty)
        {
            case ChallengeDifficulty.Easy:
                baseMultiplier = 0.75f;
                break;
            case ChallengeDifficulty.Medium:
                baseMultiplier = 1.0f;
                break;
            case ChallengeDifficulty.Hard:
                baseMultiplier = 1.5f;
                break;
            case ChallengeDifficulty.Extreme:
                baseMultiplier = 2.0f;
                break;
        }
        
        // Additional scaling per player level (5% per level)
        float levelMultiplier = 1.0f + (playerLevel * 0.05f);
        
        return baseMultiplier * levelMultiplier * healthScalingMultiplier;
    }
    
    /// <summary>
    /// Get enemy damage multiplier based on difficulty and player level
    /// </summary>
    public float GetEnemyDamageMultiplier(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        if (!scaleEnemyDamage)
            return 1.0f;
        
        float baseMultiplier = 1.0f;
        
        // Difficulty tier multiplier
        switch (actualDifficulty)
        {
            case ChallengeDifficulty.Easy:
                baseMultiplier = 0.8f;
                break;
            case ChallengeDifficulty.Medium:
                baseMultiplier = 1.0f;
                break;
            case ChallengeDifficulty.Hard:
                baseMultiplier = 1.3f;
                break;
            case ChallengeDifficulty.Extreme:
                baseMultiplier = 1.75f;
                break;
        }
        
        // Additional scaling per player level (3% per level)
        float levelMultiplier = 1.0f + (playerLevel * 0.03f);
        
        return baseMultiplier * levelMultiplier * damageScalingMultiplier;
    }
    
    /// <summary>
    /// Get scaled XP reward based on actual difficulty completed
    /// </summary>
    public int GetScaledXPReward(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        if (!scaleRewards)
            return baseXPReward;
        
        float multiplier = 1.0f;
        
        // Difficulty tier multiplier
        switch (actualDifficulty)
        {
            case ChallengeDifficulty.Easy:
                multiplier = 0.75f;
                break;
            case ChallengeDifficulty.Medium:
                multiplier = 1.0f;
                break;
            case ChallengeDifficulty.Hard:
                multiplier = 1.5f;
                break;
            case ChallengeDifficulty.Extreme:
                multiplier = 2.0f;
                break;
        }
        
        // Bonus for completing above-level challenges
        int levelDifference = recommendedLevel - playerLevel;
        if (levelDifference > 0)
        {
            // 10% bonus per level above player
            multiplier *= 1.0f + (levelDifference * 0.1f);
        }
        
        return Mathf.RoundToInt(baseXPReward * multiplier * rewardScalingMultiplier);
    }
    
    /// <summary>
    /// Get scaled currency reward based on actual difficulty completed
    /// </summary>
    public int GetScaledCurrencyReward(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        if (!scaleRewards)
            return baseCurrencyReward;
        
        float multiplier = 1.0f;
        
        switch (actualDifficulty)
        {
            case ChallengeDifficulty.Easy:
                multiplier = 0.75f;
                break;
            case ChallengeDifficulty.Medium:
                multiplier = 1.0f;
                break;
            case ChallengeDifficulty.Hard:
                multiplier = 1.5f;
                break;
            case ChallengeDifficulty.Extreme:
                multiplier = 2.0f;
                break;
        }
        
        return Mathf.RoundToInt(baseCurrencyReward * multiplier * rewardScalingMultiplier);
    }
    
    /// <summary>
    /// Get scaled loot rarity based on difficulty
    /// </summary>
    public LootManager.Rarity GetScaledLootRarity(ChallengeDifficulty actualDifficulty)
    {
        if (!scaleRewards)
            return guaranteedLootRarity;
        
        // Upgrade loot rarity for harder difficulties
        int rarityBonus = 0;
        
        switch (actualDifficulty)
        {
            case ChallengeDifficulty.Easy:
                rarityBonus = -1; // Downgrade
                break;
            case ChallengeDifficulty.Medium:
                rarityBonus = 0;
                break;
            case ChallengeDifficulty.Hard:
                rarityBonus = 1; // Upgrade one tier
                break;
            case ChallengeDifficulty.Extreme:
                rarityBonus = 2; // Upgrade two tiers
                break;
        }
        
        int finalRarity = Mathf.Clamp((int)guaranteedLootRarity + rarityBonus, 0, 4);
        return (LootManager.Rarity)finalRarity;
    }
    
    /// <summary>
    /// Check if a specific modifier is active
    /// </summary>
    public bool HasModifier(ChallengeModifier.ModifierType modifierType)
    {
        return modifiers.Exists(m => m.type == modifierType && m.isActive);
    }
    
    /// <summary>
    /// Get the value of a specific modifier
    /// </summary>
    public float GetModifierValue(ChallengeModifier.ModifierType modifierType)
    {
        var modifier = modifiers.Find(m => m.type == modifierType && m.isActive);
        return modifier != null ? modifier.value : 1.0f;
    }
    
    /// <summary>
    /// Get combined health multiplier including modifiers
    /// </summary>
    public float GetTotalHealthMultiplier(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        float baseMultiplier = GetEnemyHealthMultiplier(playerLevel, actualDifficulty);
        
        if (HasModifier(ChallengeModifier.ModifierType.IncreasedEnemyHealth))
        {
            baseMultiplier *= GetModifierValue(ChallengeModifier.ModifierType.IncreasedEnemyHealth);
        }
        
        return baseMultiplier;
    }
    
    /// <summary>
    /// Get combined damage multiplier including modifiers
    /// </summary>
    public float GetTotalDamageMultiplier(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        float baseMultiplier = GetEnemyDamageMultiplier(playerLevel, actualDifficulty);
        
        if (HasModifier(ChallengeModifier.ModifierType.IncreasedEnemyDamage))
        {
            baseMultiplier *= GetModifierValue(ChallengeModifier.ModifierType.IncreasedEnemyDamage);
        }
        
        return baseMultiplier;
    }
    
    /// <summary>
    /// Calculate total XP reward including all bonuses
    /// </summary>
    public int GetTotalXPReward(int playerLevel, ChallengeDifficulty actualDifficulty, bool perfectCompletion = false, bool speedCompletion = false, bool stealthCompletion = false)
    {
        float baseXP = GetScaledXPReward(playerLevel, actualDifficulty);
        float multiplier = 1.0f;
        
        // Apply modifier bonuses
        if (HasModifier(ChallengeModifier.ModifierType.DoubleXP))
        {
            multiplier *= 2.0f;
        }
        
        // Apply completion bonuses
        if (perfectCompletion && perfectCompletionBonus)
        {
            multiplier *= perfectCompletionXPMultiplier;
        }
        
        if (speedCompletion && speedCompletionBonus)
        {
            multiplier *= speedCompletionXPMultiplier;
        }
        
        if (stealthCompletion && stealthCompletionBonus)
        {
            multiplier *= stealthCompletionXPMultiplier;
        }
        
        // Apply modifiers that increase difficulty = more rewards
        if (HasModifier(ChallengeModifier.ModifierType.IronMan))
        {
            multiplier *= 1.5f; // 50% bonus for ironman mode
        }
        
        if (HasModifier(ChallengeModifier.ModifierType.TimeTrial))
        {
            multiplier *= 1.25f; // 25% bonus for time trial
        }
        
        if (HasModifier(ChallengeModifier.ModifierType.NoHealthRegen))
        {
            multiplier *= 1.3f; // 30% bonus for no health regen
        }
        
        if (HasModifier(ChallengeModifier.ModifierType.LimitedAmmo))
        {
            multiplier *= 1.2f; // 20% bonus for limited ammo
        }
        
        return Mathf.RoundToInt(baseXP * multiplier);
    }
    
    /// <summary>
    /// Calculate total currency reward including modifiers
    /// </summary>
    public int GetTotalCurrencyReward(int playerLevel, ChallengeDifficulty actualDifficulty)
    {
        float baseCurrency = GetScaledCurrencyReward(playerLevel, actualDifficulty);
        float multiplier = 1.0f;
        
        if (HasModifier(ChallengeModifier.ModifierType.DoubleCurrency))
        {
            multiplier *= 2.0f;
        }
        
        return Mathf.RoundToInt(baseCurrency * multiplier);
    }
    
    /// <summary>
    /// Get adjusted loot count including modifiers
    /// </summary>
    public int GetTotalLootCount()
    {
        int count = guaranteedLootCount;
        
        if (HasModifier(ChallengeModifier.ModifierType.BonusLootDrop))
        {
            count += Mathf.RoundToInt(guaranteedLootCount * GetModifierValue(ChallengeModifier.ModifierType.BonusLootDrop));
        }
        
        return count;
    }
    
    /// <summary>
    /// Get adjusted loot rarity including modifiers
    /// </summary>
    public LootManager.Rarity GetTotalLootRarity(ChallengeDifficulty actualDifficulty)
    {
        LootManager.Rarity baseRarity = GetScaledLootRarity(actualDifficulty);
        
        if (HasModifier(ChallengeModifier.ModifierType.GuaranteedRareLoot))
        {
            // Ensure minimum Rare quality
            if ((int)baseRarity < (int)LootManager.Rarity.Rare)
            {
                baseRarity = LootManager.Rarity.Rare;
            }
        }
        
        return baseRarity;
    }
    
    /// <summary>
    /// Get modified time limit
    /// </summary>
    public float GetModifiedTimeLimit()
    {
        float time = timeLimit;
        
        if (HasModifier(ChallengeModifier.ModifierType.TimeTrial))
        {
            time *= GetModifierValue(ChallengeModifier.ModifierType.TimeTrial);
        }
        
        return time;
    }
    
    /// <summary>
    /// Get a description of all active modifiers
    /// </summary>
    public string GetModifiersDescription()
    {
        if (modifiers.Count == 0)
            return "";
        
        string description = "Active Modifiers:\n";
        foreach (var modifier in modifiers)
        {
            if (!modifier.isActive) continue;
            
            switch (modifier.type)
            {
                case ChallengeModifier.ModifierType.IncreasedEnemyHealth:
                    description += $"• Increased Enemy Health ({modifier.value}x)\n";
                    break;
                case ChallengeModifier.ModifierType.IncreasedEnemyDamage:
                    description += $"• Increased Enemy Damage ({modifier.value}x)\n";
                    break;
                case ChallengeModifier.ModifierType.DoubleXP:
                    description += "• Double XP Rewards\n";
                    break;
                case ChallengeModifier.ModifierType.DoubleCurrency:
                    description += "• Double Currency Rewards\n";
                    break;
                case ChallengeModifier.ModifierType.IronMan:
                    description += "• Iron Man: One Death = Failure\n";
                    break;
                case ChallengeModifier.ModifierType.TimeTrial:
                    description += $"• Time Trial: {modifier.value * 100}% Time Limit\n";
                    break;
                case ChallengeModifier.ModifierType.NoHealthRegen:
                    description += "• No Health Regeneration\n";
                    break;
                case ChallengeModifier.ModifierType.LimitedAmmo:
                    description += "• Limited Ammo\n";
                    break;
                case ChallengeModifier.ModifierType.BonusLootDrop:
                    description += $"• Bonus Loot Drops (+{modifier.value * 100}%)\n";
                    break;
                case ChallengeModifier.ModifierType.GuaranteedRareLoot:
                    description += "• Guaranteed Rare+ Loot\n";
                    break;
                case ChallengeModifier.ModifierType.EliteEnemiesOnly:
                    description += "• Elite Enemies Only\n";
                    break;
                case ChallengeModifier.ModifierType.NightMode:
                    description += "• Night Mode\n";
                    break;
                case ChallengeModifier.ModifierType.SpeedRunner:
                    description += "• Speed Runner Bonus\n";
                    break;
                case ChallengeModifier.ModifierType.PerfectScore:
                    description += "• Perfect Score Bonus\n";
                    break;
            }
        }
        
        return description;
    }
}
