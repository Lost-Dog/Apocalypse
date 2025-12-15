using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }
    
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    [System.Serializable]
    public class LootPool
    {
        public Rarity rarity;
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
    [Tooltip("Add ground snap component to ensure loot settles on ground")]
    public bool addGroundSnapComponent = true;
    
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
    
    [Header("Inventory Integration")]
    public PlayerInventory playerInventory;
    
    [Header("Visibility Settings")]
    [Tooltip("Add visibility helpers to spawned loot")]
    public bool enableVisibilityHelpers = true;
    
    [Tooltip("Use advanced visibility (light beams, rings, etc)")]
    public bool useAdvancedVisibility = false;
    
    [Tooltip("Use simple outline glow")]
    public bool useSimpleOutline = true;
    
    [Tooltip("Add compass markers for distant loot")]
    public bool useCompassMarkers = false;
    
    [Header("Loot Events")]
    public UnityEvent<Rarity, int> onLootDropped;
    public UnityEvent<LootItemData, int, Rarity> onItemCollected;
    
    private const int GEAR_SCORE_BASE = 100;
    private const int GEAR_SCORE_PER_LEVEL = 40;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
            
            if (playerInventory == null)
            {
                GameObject inventoryObj = new GameObject("PlayerInventory");
                playerInventory = inventoryObj.AddComponent<PlayerInventory>();
                Debug.Log("Created PlayerInventory automatically");
            }
        }
    }
    
    public void DropLoot(Vector3 position, int playerLevel)
    {
        Rarity rarity = RollRarity(playerLevel);
        int gearScore = CalculateGearScore(playerLevel, rarity);
        
        LootItemData itemData = GetRandomLootItemByRarity(rarity);
        SpawnLootDrop(position, itemData, rarity, gearScore);
        
        onLootDropped?.Invoke(rarity, gearScore);
        
        Debug.Log($"Dropped {rarity} loot with Gear Score {gearScore} at {position}");
    }
    
    public void DropLootWithRarity(Vector3 position, int playerLevel, Rarity forcedRarity)
    {
        int gearScore = CalculateGearScore(playerLevel, forcedRarity);
        
        LootItemData itemData = GetRandomLootItemByRarity(forcedRarity);
        SpawnLootDrop(position, itemData, forcedRarity, gearScore);
        
        onLootDropped?.Invoke(forcedRarity, gearScore);
    }
    
    public void AddItemToInventory(LootItemData itemData, int gearScore, Rarity rarity)
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory not assigned!");
            return;
        }
        
        bool added = playerInventory.AddItem(itemData, gearScore, rarity);
        
        if (added)
        {
            onItemCollected?.Invoke(itemData, gearScore, rarity);
            Debug.Log($"Added to inventory: {itemData.itemName} (GS {gearScore})");
        }
    }
    
    public LootItemData GetRandomLootItemByRarity(Rarity rarity)
    {
        List<LootItemData> itemsOfRarity = lootableItems.Where(item => item != null && item.rarity == rarity).ToList();
        
        if (itemsOfRarity.Count == 0)
        {
            itemsOfRarity = lootableItems.Where(item => item != null && item.rarity == Rarity.Common).ToList();
            
            if (itemsOfRarity.Count == 0)
            {
                Debug.LogWarning($"No lootable items found for rarity {rarity} or Common!");
                return null;
            }
        }
        
        int randomIndex = Random.Range(0, itemsOfRarity.Count);
        return itemsOfRarity[randomIndex];
    }
    
    public LootItemData GetLootItemByID(string itemID)
    {
        return lootableItems.FirstOrDefault(item => item != null && item.itemID == itemID);
    }
    
    private Rarity RollRarity(int playerLevel = 1)
    {
        float levelBonus = Mathf.Min(playerLevel, maxScalingLevel) * (rarityBonusPerLevel / 100f);
        
        float adjustedCommonChance = Mathf.Max(0.05f, commonChance - (levelBonus * 2f));
        float adjustedUncommonChance = uncommonChance + (levelBonus * 0.5f);
        float adjustedRareChance = rareChance + (levelBonus * 0.75f);
        float adjustedEpicChance = epicChance + (levelBonus * 1f);
        float adjustedLegendaryChance = legendaryChance + (levelBonus * 0.75f);
        
        float totalChance = adjustedCommonChance + adjustedUncommonChance + adjustedRareChance + 
                           adjustedEpicChance + adjustedLegendaryChance;
        float roll = Random.Range(0f, totalChance);
        
        float cumulative = 0f;
        
        cumulative += adjustedLegendaryChance;
        if (roll < cumulative) return Rarity.Legendary;
        
        cumulative += adjustedEpicChance;
        if (roll < cumulative) return Rarity.Epic;
        
        cumulative += adjustedRareChance;
        if (roll < cumulative) return Rarity.Rare;
        
        cumulative += adjustedUncommonChance;
        if (roll < cumulative) return Rarity.Uncommon;
        
        return Rarity.Common;
    }
    
    private int CalculateGearScore(int level, Rarity rarity)
    {
        int baseScore = GEAR_SCORE_BASE + (level * GEAR_SCORE_PER_LEVEL);
        
        int rarityBonus = (int)rarity * 50;
        
        int variance = Random.Range(-10, 11);
        
        int finalScore = baseScore + rarityBonus + variance;
        
        return Mathf.Clamp(finalScore, minGearScore, maxGearScore);
    }
    
    private void SpawnLootDrop(Vector3 position, LootItemData itemData, Rarity rarity, int gearScore)
    {
        GameObject prefabToSpawn = null;
        
        if (itemData != null && itemData.worldPrefab != null)
        {
            prefabToSpawn = itemData.worldPrefab;
        }
        else
        {
            prefabToSpawn = GetRandomLootPrefab(rarity);
        }
        
        if (prefabToSpawn == null)
        {
            prefabToSpawn = defaultLootPrefab;
        }
        
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"No loot prefab available for rarity: {rarity}");
            return;
        }
        
        Vector3 spawnPosition = position + Vector3.up * spawnHeightOffset;
        
        if (useGroundDetection)
        {
            spawnPosition = GetGroundPosition(position);
        }
        
        GameObject lootDrop = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        LootItem lootItem = lootDrop.GetComponent<LootItem>();
        if (lootItem == null)
        {
            lootItem = lootDrop.AddComponent<LootItem>();
            lootItem.rarityLight = lootDrop.GetComponentInChildren<Light>();
        }
        
        if (lootItem != null && itemData != null)
        {
            lootItem.Initialize(itemData, gearScore, rarity);
        }
        
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
        rb.constraints = RigidbodyConstraints.None;
        
        Collider lootCollider = lootDrop.GetComponent<Collider>();
        if (lootCollider == null)
        {
            SphereCollider col = lootDrop.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            col.isTrigger = false;
        }
        else if (lootCollider.isTrigger)
        {
            BoxCollider physicsCollider = lootDrop.AddComponent<BoxCollider>();
            physicsCollider.size = Vector3.one;
            physicsCollider.isTrigger = false;
        }
        
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1.5f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        rb.AddForce(randomDirection * lootDropForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * lootDropForce * 0.5f, ForceMode.Impulse);
        
        if (addGroundSnapComponent)
        {
            LootGroundSnap groundSnap = lootDrop.GetComponent<LootGroundSnap>();
            if (groundSnap == null)
            {
                groundSnap = lootDrop.AddComponent<LootGroundSnap>();
                groundSnap.enableGroundSnap = true;
                groundSnap.groundLayer = groundLayer;
            }
        }
        
        if (enableVisibilityHelpers)
        {
            AddVisibilityHelpers(lootDrop, rarity);
        }
    }
    
    private void AddVisibilityHelpers(GameObject lootObject, Rarity rarity)
    {
        if (useAdvancedVisibility)
        {
            LootVisibilityHelper helper = lootObject.AddComponent<LootVisibilityHelper>();
            helper.itemRarity = rarity;
            helper.useRarityColors = true;
            helper.showLightBeam = true;
            helper.beamHeight = 5f;
            helper.beamIntensity = 1.5f;
            helper.showGroundRing = true;
            helper.ringRadius = 1f;
            helper.showOuterGlow = true;
            helper.pulseGlow = true;
            helper.pulseSpeed = 1f;
            helper.scaleWithDistance = true;
        }
        
        if (useSimpleOutline)
        {
            MeshRenderer[] renderers = lootObject.GetComponentsInChildren<MeshRenderer>();
            Color outlineColor = GetRarityVisualColor(rarity);
            
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    Material mat = renderer.material;
                    
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", outlineColor * 2f);
                    }
                }
            }
        }
        
        if (useCompassMarkers)
        {
            LootCompassMarker marker = lootObject.AddComponent<LootCompassMarker>();
            marker.showOffScreenMarkers = true;
            marker.maxMarkerDistance = 100f;
            marker.minMarkerDistance = 5f;
        }
    }
    
    private Color GetRarityVisualColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return new Color(0.8f, 0.8f, 0.8f);
            case Rarity.Uncommon:
                return new Color(0.1f, 0.9f, 0.1f);
            case Rarity.Rare:
                return new Color(0.2f, 0.5f, 1f);
            case Rarity.Epic:
                return new Color(0.64f, 0.21f, 0.93f);
            case Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.yellow;
        }
    }
    
    private Vector3 GetGroundPosition(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayStart = position + Vector3.up * 5f;
        
        LayerMask layerToUse = groundLayer.value != 0 ? groundLayer : ~0;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, layerToUse))
        {
            return hit.point + Vector3.up * spawnHeightOffset;
        }
        
        return position + Vector3.up * spawnHeightOffset;
    }
    
    private GameObject GetRandomLootPrefab(Rarity rarity)
    {
        LootPool pool = lootPools.Find(p => p.rarity == rarity);
        
        if (pool == null || pool.lootPrefabs.Count == 0)
        {
            return null;
        }
        
        int randomIndex = Random.Range(0, pool.lootPrefabs.Count);
        return pool.lootPrefabs[randomIndex];
    }
    
    public Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Color.white;
            case Rarity.Uncommon:
                return Color.green;
            case Rarity.Rare:
                return Color.blue;
            case Rarity.Epic:
                return new Color(0.6f, 0f, 1f);
            case Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }
    
    public string GetRarityName(Rarity rarity)
    {
        return rarity.ToString();
    }
    
    public void GetScaledRarityChances(int playerLevel, out float common, out float uncommon, out float rare, out float epic, out float legendary)
    {
        float levelBonus = Mathf.Min(playerLevel, maxScalingLevel) * (rarityBonusPerLevel / 100f);
        
        common = Mathf.Max(0.05f, commonChance - (levelBonus * 2f));
        uncommon = uncommonChance + (levelBonus * 0.5f);
        rare = rareChance + (levelBonus * 0.75f);
        epic = epicChance + (levelBonus * 1f);
        legendary = legendaryChance + (levelBonus * 0.75f);
        
        float total = common + uncommon + rare + epic + legendary;
        
        common = (common / total) * 100f;
        uncommon = (uncommon / total) * 100f;
        rare = (rare / total) * 100f;
        epic = (epic / total) * 100f;
        legendary = (legendary / total) * 100f;
    }
}
