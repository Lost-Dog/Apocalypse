using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using Invector.vItemManager;

/// <summary>
/// Handles loot spawning, rarity rolling, gear score calculation and world-drop physics.
/// Item collection is delegated to Invector's vItemManager via AddItem(ItemReference).
/// </summary>
public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }

    [System.Serializable]
    public class LootPool
    {
        public LootRarity rarity;
        public List<GameObject> lootPrefabs = new List<GameObject>();
    }

    [Header("Lootable Items Database")]
    [Tooltip("Configure all lootable items here")]
    public List<LootItemData> lootableItems = new List<LootItemData>();

    [Header("Loot Prefab Pools")]
    public List<LootPool> lootPools = new List<LootPool>();
    public GameObject defaultLootPrefab;
    public float lootDropForce = 5f;
    [Tooltip("Height offset when spawning loot (0 = ground level)")]
    public float spawnHeightOffset = 0.5f;
    [Tooltip("Use ground detection to prevent floating loot")]
    public bool useGroundDetection = true;
    [Tooltip("Maximum raycast distance for ground detection")]
    public float groundCheckDistance = 10f;
    [Tooltip("Layer mask for ground detection (leave at 0 for default)")]
    public LayerMask groundLayer;

    [Header("Gear Score Ranges")]
    public int minGearScore = 100;
    public int maxGearScore = 500;

    [Header("Rarity Chances")]
    [Range(0f, 1f)] public float commonChance = 0.50f;
    [Range(0f, 1f)] public float uncommonChance = 0.25f;
    [Range(0f, 1f)] public float rareChance = 0.15f;
    [Range(0f, 1f)] public float epicChance = 0.08f;
    [Range(0f, 1f)] public float legendaryChance = 0.02f;

    [Header("Level Scaling")]
    [Tooltip("Bonus to rare drop chances per player level (%)")]
    [Range(0f, 5f)] public float rarityBonusPerLevel = 0.5f;
    [Tooltip("Maximum level to scale rarity bonuses")]
    public int maxScalingLevel = 30;

    [Header("Loot Events")]
    public UnityEvent<LootRarity, int> onLootDropped;
    public UnityEvent<LootItemData, int, LootRarity> onItemCollected;

    private const int GEAR_SCORE_BASE = 100;
    private const int GEAR_SCORE_PER_LEVEL = 40;

    // OPTIMIZATION: Cache filtered loot items by rarity to avoid repeated LINQ queries
    private Dictionary<LootRarity, List<LootItemData>> cachedLootItemsByRarity;

    // OPTIMIZATION: Object pooling for loot drops
    [Header("Object Pooling")]
    [Tooltip("Use object pooling for loot drops (recommended for performance)")]
    public bool useObjectPooling = true;
    [Tooltip("Initial pool size for each loot prefab")]
    public int lootPoolSize = 20;

    private Dictionary<GameObject, ObjectPool> lootObjectPools = new Dictionary<GameObject, ObjectPool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // OPTIMIZATION: Build cache of loot items by rarity
        RebuildLootItemCache();

        // OPTIMIZATION: Initialize object pools for loot
        if (useObjectPooling)
        {
            InitializeLootPools();
        }
    }

    /// <summary>
    /// OPTIMIZATION: Initialize object pools for all loot prefabs
    /// </summary>
    private void InitializeLootPools()
    {
        // Create pools for default loot prefab
        if (defaultLootPrefab != null)
        {
            CreateLootPool(defaultLootPrefab);
        }

        // Create pools for rarity-specific prefabs
        for (int i = 0; i < lootPools.Count; i++)
        {
            LootPool pool = lootPools[i];
            for (int j = 0; j < pool.lootPrefabs.Count; j++)
            {
                if (pool.lootPrefabs[j] != null)
                {
                    CreateLootPool(pool.lootPrefabs[j]);
                }
            }
        }

        Debug.Log($"[LootManager] Initialized {lootObjectPools.Count} loot pools");
    }

    /// <summary>
    /// Create an object pool for a specific loot prefab
    /// </summary>
    private void CreateLootPool(GameObject prefab)
    {
        if (lootObjectPools.ContainsKey(prefab)) return;

        GameObject poolObj = new GameObject($"Pool_{prefab.name}");
        poolObj.transform.SetParent(transform);

        ObjectPool pool = poolObj.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        pool.initialPoolSize = lootPoolSize;
        pool.maxPoolSize = lootPoolSize * 2;
        pool.canGrow = true;

        lootObjectPools[prefab] = pool;
    }

    /// <summary>
    /// OPTIMIZATION: Rebuilds the cached dictionary of loot items by rarity.
    /// Call this if lootableItems list changes at runtime.
    /// </summary>
    private void RebuildLootItemCache()
    {
        cachedLootItemsByRarity = new Dictionary<LootRarity, List<LootItemData>>();

        // Initialize lists for each rarity
        foreach (LootRarity rarity in System.Enum.GetValues(typeof(LootRarity)))
        {
            cachedLootItemsByRarity[rarity] = new List<LootItemData>();
        }

        // Populate lists - single pass through all items
        for (int i = 0; i < lootableItems.Count; i++)
        {
            if (lootableItems[i] != null)
            {
                cachedLootItemsByRarity[lootableItems[i].rarity].Add(lootableItems[i]);
            }
        }
    }

    /// <summary>
    /// Returns a loot drop GameObject to its pool. Returns false if the object
    /// does not belong to any managed pool and should be destroyed by the caller.
    /// </summary>
    public bool TryReturnToPool(GameObject lootDrop)
    {
        if (!useObjectPooling) return false;

        foreach (var kvp in lootObjectPools)
        {
            ObjectPool pool = kvp.Value;
            if (pool == null) continue;

            // ObjectPool tracks all objects it owns; delegate the check to it.
            pool.Return(lootDrop);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Rolls rarity and spawns a loot drop at the given position.
    /// </summary>
    public void DropLoot(Vector3 position, int playerLevel)
    {
        LootRarity rarity = RollRarity(playerLevel);
        int gearScore = CalculateGearScore(playerLevel, rarity);

        LootItemData itemData = GetRandomLootItemByRarity(rarity);
        SpawnLootDrop(position, itemData, rarity, gearScore);

        onLootDropped?.Invoke(rarity, gearScore);
    }

    /// <summary>
    /// Spawns a loot drop at the given position with a forced rarity tier.
    /// </summary>
    public void DropLootWithRarity(Vector3 position, int playerLevel, LootRarity forcedRarity)
    {
        int gearScore = CalculateGearScore(playerLevel, forcedRarity);

        LootItemData itemData = GetRandomLootItemByRarity(forcedRarity);
        SpawnLootDrop(position, itemData, forcedRarity, gearScore);

        onLootDropped?.Invoke(forcedRarity, gearScore);
    }

    /// <summary>
    /// Adds a loot item to the player's Invector vItemManager.
    /// The player GameObject must have a vItemManager component attached.
    /// </summary>
    public void AddItemToPlayerInventory(LootItemData itemData, int gearScore, LootRarity rarity, GameObject player)
    {
        if (itemData == null || player == null) return;

        vItemManager itemManager = player.GetComponent<vItemManager>();
        if (itemManager == null)
        {
            Debug.LogWarning($"[LootManager] No vItemManager on {player.name}. Cannot add item.");
            return;
        }

        ItemReference itemRef = new ItemReference(itemData.invectorItemID)
        {
            amount = 1
        };

        itemManager.AddItem(itemRef);
        onItemCollected?.Invoke(itemData, gearScore, rarity);
        Debug.Log($"[LootManager] Added to Invector inventory: {itemData.itemName} (GS {gearScore}, {rarity})");
    }

    /// <summary>
    /// Returns a random LootItemData matching the requested rarity, falling back to Common.
    /// OPTIMIZED: Uses cached dictionary instead of LINQ queries.
    /// </summary>
    public LootItemData GetRandomLootItemByRarity(LootRarity rarity)
    {
        // OPTIMIZATION: Use cached dictionary instead of LINQ
        if (cachedLootItemsByRarity == null || cachedLootItemsByRarity.Count == 0)
        {
            RebuildLootItemCache();
        }

        List<LootItemData> itemsOfRarity = cachedLootItemsByRarity[rarity];

        if (itemsOfRarity.Count == 0)
        {
            // Fallback to Common rarity
            itemsOfRarity = cachedLootItemsByRarity[LootRarity.Common];

            if (itemsOfRarity.Count == 0)
            {
                Debug.LogWarning($"[LootManager] No lootable items found for rarity {rarity} or Common!");
                return null;
            }
        }

        return itemsOfRarity[Random.Range(0, itemsOfRarity.Count)];
    }

    /// <summary>
    /// Looks up a LootItemData by its unique string ID.
    /// </summary>
    public LootItemData GetLootItemByID(string itemID)
    {
        return lootableItems.FirstOrDefault(item => item != null && item.itemID == itemID);
    }

    /// <summary>
    /// Returns the display name string for a rarity tier.
    /// </summary>
    public string GetRarityName(LootRarity rarity) => rarity.ToString();

    /// <summary>
    /// Returns the tint color associated with a rarity tier.
    /// </summary>
    public Color GetRarityColor(LootRarity rarity)
    {
        switch (rarity)
        {
            case LootRarity.Common:    return Color.white;
            case LootRarity.Uncommon:  return Color.green;
            case LootRarity.Rare:      return Color.blue;
            case LootRarity.Epic:      return new Color(0.6f, 0f, 1f);
            case LootRarity.Legendary: return new Color(1f, 0.5f, 0f);
            default:                   return Color.white;
        }
    }

    /// <summary>
    /// Returns scaled rarity chances as percentages (0–100) for the given player level.
    /// </summary>
    public void GetScaledRarityChances(int playerLevel, out float common, out float uncommon, out float rare, out float epic, out float legendary)
    {
        float levelBonus = Mathf.Min(playerLevel, maxScalingLevel) * (rarityBonusPerLevel / 100f);

        common    = Mathf.Max(0.05f, commonChance - (levelBonus * 2f));
        uncommon  = uncommonChance + (levelBonus * 0.5f);
        rare      = rareChance + (levelBonus * 0.75f);
        epic      = epicChance + (levelBonus * 1f);
        legendary = legendaryChance + (levelBonus * 0.75f);

        float total = common + uncommon + rare + epic + legendary;
        common    = (common / total) * 100f;
        uncommon  = (uncommon / total) * 100f;
        rare      = (rare / total) * 100f;
        epic      = (epic / total) * 100f;
        legendary = (legendary / total) * 100f;
    }

    private LootRarity RollRarity(int playerLevel = 1)
    {
        float levelBonus = Mathf.Min(playerLevel, maxScalingLevel) * (rarityBonusPerLevel / 100f);

        float adjCommon    = Mathf.Max(0.05f, commonChance - (levelBonus * 2f));
        float adjUncommon  = uncommonChance + (levelBonus * 0.5f);
        float adjRare      = rareChance + (levelBonus * 0.75f);
        float adjEpic      = epicChance + (levelBonus * 1f);
        float adjLegendary = legendaryChance + (levelBonus * 0.75f);

        float total = adjCommon + adjUncommon + adjRare + adjEpic + adjLegendary;
        float roll  = Random.Range(0f, total);

        float cumulative = adjLegendary;
        if (roll < cumulative) return LootRarity.Legendary;

        cumulative += adjEpic;
        if (roll < cumulative) return LootRarity.Epic;

        cumulative += adjRare;
        if (roll < cumulative) return LootRarity.Rare;

        cumulative += adjUncommon;
        if (roll < cumulative) return LootRarity.Uncommon;

        return LootRarity.Common;
    }

    private int CalculateGearScore(int level, LootRarity rarity)
    {
        int baseScore   = GEAR_SCORE_BASE + (level * GEAR_SCORE_PER_LEVEL);
        int rarityBonus = (int)rarity * 50;
        int variance    = Random.Range(-10, 11);

        return Mathf.Clamp(baseScore + rarityBonus + variance, minGearScore, maxGearScore);
    }

    private void SpawnLootDrop(Vector3 position, LootItemData itemData, LootRarity rarity, int gearScore)
    {
        GameObject prefabToSpawn = (itemData != null && itemData.worldPrefab != null)
            ? itemData.worldPrefab
            : GetRandomLootPrefab(rarity) ?? defaultLootPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[LootManager] No loot prefab available for rarity: {rarity}");
            return;
        }

        Vector3 spawnPosition = useGroundDetection
            ? GetGroundPosition(position)
            : position + Vector3.up * spawnHeightOffset;

        GameObject lootDrop;

        // OPTIMIZATION: Use object pooling if enabled
        if (useObjectPooling)
        {
            // Ensure pool exists for this prefab
            if (!lootObjectPools.ContainsKey(prefabToSpawn))
            {
                CreateLootPool(prefabToSpawn);
            }

            lootDrop = lootObjectPools[prefabToSpawn].Get(spawnPosition, Quaternion.identity);

            if (lootDrop == null)
            {
                Debug.LogWarning($"[LootManager] Pool exhausted for {prefabToSpawn.name}, falling back to Instantiate");
                lootDrop = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            }
        }
        else
        {
            lootDrop = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }

        // Get or add physics components
        Rigidbody rb = lootDrop.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = lootDrop.AddComponent<Rigidbody>();
        }

        rb.mass = 1f;
        rb.linearDamping = 2f;
        rb.angularDamping = 1f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Collider existingCollider = lootDrop.GetComponent<Collider>();
        if (existingCollider == null)
        {
            SphereCollider col = lootDrop.AddComponent<SphereCollider>();
            col.radius = 0.5f;
        }
        else if (existingCollider.isTrigger)
        {
            BoxCollider physicsCol = lootDrop.GetComponent<BoxCollider>();
            if (physicsCol == null)
            {
                physicsCol = lootDrop.AddComponent<BoxCollider>();
                physicsCol.size = Vector3.one;
            }
        }

        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1.5f),
            Random.Range(-1f, 1f)
        ).normalized;

        rb.AddForce(randomDir * lootDropForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * lootDropForce * 0.5f, ForceMode.Impulse);
    }

    private Vector3 GetGroundPosition(Vector3 position)
    {
        Vector3 rayStart  = position + Vector3.up * 5f;
        LayerMask layer   = groundLayer.value != 0 ? groundLayer : ~0;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance, layer))
        {
            return hit.point + Vector3.up * spawnHeightOffset;
        }

        return position + Vector3.up * spawnHeightOffset;
    }

    private GameObject GetRandomLootPrefab(LootRarity rarity)
    {
        LootPool pool = lootPools.Find(p => p.rarity == rarity);
        if (pool == null || pool.lootPrefabs.Count == 0) return null;
        return pool.lootPrefabs[Random.Range(0, pool.lootPrefabs.Count)];
    }
}
