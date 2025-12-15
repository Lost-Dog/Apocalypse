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
    
    [Header("Requirements")]
    public int recommendedLevel = 1;
    public int requiredPlayerLevel = 1;
    public float timeLimit = 600f;
    public float detectionRadius = 50f;
    
    [Header("Objectives")]
    public bool requireNoDeaths = false;
    public bool requireStealth = false;
    public string objectiveDescription = "Complete the challenge";
    
    [Header("Flexible Spawning System")]
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
    
    [Header("Rewards")]
    public int xpReward = 200;
    public int currencyReward = 100;
    public LootManager.Rarity guaranteedLootRarity = LootManager.Rarity.Common;
    public int guaranteedLootCount = 1;
    public List<GameObject> bonusRewards = new List<GameObject>();
    
    [Header("UI")]
    public Sprite challengeIcon;
    public Color difficultyColor = Color.white;
    
    [Header("Audio")]
    public AudioClip startSound;
    public AudioClip completeSound;
    public AudioClip failSound;

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
}
