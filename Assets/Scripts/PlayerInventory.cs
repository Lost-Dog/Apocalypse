using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class InventoryItem
{
    public string itemID;
    public string itemName;
    public LootManager.Rarity rarity;
    public int gearScore;
    public LootItemData.ItemType itemType;
    public string description;
    public bool isEquipped;
    
    public System.DateTime acquiredDate;
    
    public InventoryItem(LootItemData data, int score, LootManager.Rarity itemRarity)
    {
        itemID = data.itemID;
        itemName = data.itemName;
        rarity = itemRarity;
        gearScore = score;
        itemType = data.itemType;
        description = data.description;
        isEquipped = false;
        acquiredDate = System.DateTime.Now;
    }
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData> items = new List<InventoryItemSaveData>();
}

[System.Serializable]
public class InventoryItemSaveData
{
    public string itemID;
    public string itemName;
    public int rarity;
    public int gearScore;
    public int itemType;
    public string description;
    public bool isEquipped;
    public string acquiredDateString;
    public int baseGearScore;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    
    [Header("Inventory Settings")]
    public int maxInventorySize = 100;
    public bool autoSaveOnChange = true;
    
    [Header("Current Inventory")]
    public List<InventoryItem> items = new List<InventoryItem>();
    
    [Header("Statistics")]
    public int totalItemsCollected = 0;
    public int highestGearScore = 0;
    
    private const string INVENTORY_SAVE_KEY = "PlayerInventory_SaveData";
    
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
    }
    
    private void Start()
    {
        LoadInventory();
    }
    
    public bool AddItem(LootItemData itemData, int gearScore, LootManager.Rarity rarity)
    {
        if (items.Count >= maxInventorySize)
        {
            Debug.LogWarning("Inventory is full!");
            return false;
        }
        
        InventoryItem newItem = new InventoryItem(itemData, gearScore, rarity);
        items.Add(newItem);
        
        totalItemsCollected++;
        
        if (gearScore > highestGearScore)
        {
            highestGearScore = gearScore;
        }
        
        Debug.Log($"Added to inventory: {itemData.itemName} (GS {gearScore})");
        
        if (autoSaveOnChange)
        {
            SaveInventory();
        }
        
        return true;
    }
    
    public bool RemoveItem(InventoryItem item)
    {
        bool removed = items.Remove(item);
        
        if (removed && autoSaveOnChange)
        {
            SaveInventory();
        }
        
        return removed;
    }
    
    public List<InventoryItem> GetItemsByType(LootItemData.ItemType type)
    {
        return items.Where(item => item.itemType == type).ToList();
    }
    
    public List<InventoryItem> GetItemsByRarity(LootManager.Rarity rarity)
    {
        return items.Where(item => item.rarity == rarity).ToList();
    }
    
    public List<InventoryItem> GetEquippedItems()
    {
        return items.Where(item => item.isEquipped).ToList();
    }
    
    public int GetTotalGearScore()
    {
        return items.Where(item => item.isEquipped).Sum(item => item.gearScore);
    }
    
    public int GetAverageGearScore()
    {
        if (items.Count == 0) return 0;
        return (int)items.Average(item => item.gearScore);
    }
    
    public void SortByGearScore(bool descending = true)
    {
        if (descending)
        {
            items = items.OrderByDescending(item => item.gearScore).ToList();
        }
        else
        {
            items = items.OrderBy(item => item.gearScore).ToList();
        }
    }
    
    public void SortByRarity(bool descending = true)
    {
        if (descending)
        {
            items = items.OrderByDescending(item => item.rarity).ToList();
        }
        else
        {
            items = items.OrderBy(item => item.rarity).ToList();
        }
    }
    
    public void SortByAcquiredDate(bool mostRecent = true)
    {
        if (mostRecent)
        {
            items = items.OrderByDescending(item => item.acquiredDate).ToList();
        }
        else
        {
            items = items.OrderBy(item => item.acquiredDate).ToList();
        }
    }
    
    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();
        
        foreach (InventoryItem item in items)
        {
            InventoryItemSaveData itemSave = new InventoryItemSaveData
            {
                itemID = item.itemID,
                itemName = item.itemName,
                rarity = (int)item.rarity,
                gearScore = item.gearScore,
                itemType = (int)item.itemType,
                description = item.description,
                isEquipped = item.isEquipped,
                acquiredDateString = item.acquiredDate.ToString("o"),
                baseGearScore = 100
            };
            
            saveData.items.Add(itemSave);
        }
        
        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(INVENTORY_SAVE_KEY, json);
        PlayerPrefs.SetInt("TotalItemsCollected", totalItemsCollected);
        PlayerPrefs.SetInt("HighestGearScore", highestGearScore);
        PlayerPrefs.Save();
        
        Debug.Log($"Inventory saved: {items.Count} items");
    }
    
    public void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(INVENTORY_SAVE_KEY))
        {
            Debug.Log("No saved inventory found. Starting fresh.");
            return;
        }
        
        string json = PlayerPrefs.GetString(INVENTORY_SAVE_KEY);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
        
        items.Clear();
        
        LootManager lootManager = LootManager.Instance;
        
        foreach (InventoryItemSaveData itemSave in saveData.items)
        {
            LootItemData itemData = null;
            
            if (lootManager != null)
            {
                itemData = lootManager.GetLootItemByID(itemSave.itemID);
            }
            
            if (itemData == null)
            {
                itemData = ScriptableObject.CreateInstance<LootItemData>();
                itemData.itemID = itemSave.itemID;
                itemData.itemName = itemSave.itemName;
                itemData.itemType = (LootItemData.ItemType)itemSave.itemType;
                itemData.description = itemSave.description;
                itemData.baseGearScore = itemSave.baseGearScore;
            }
            
            InventoryItem item = new InventoryItem(
                itemData,
                itemSave.gearScore,
                (LootManager.Rarity)itemSave.rarity
            );
            
            item.isEquipped = itemSave.isEquipped;
            
            if (System.DateTime.TryParse(itemSave.acquiredDateString, out System.DateTime date))
            {
                item.acquiredDate = date;
            }
            
            items.Add(item);
        }
        
        totalItemsCollected = PlayerPrefs.GetInt("TotalItemsCollected", 0);
        highestGearScore = PlayerPrefs.GetInt("HighestGearScore", 0);
        
        Debug.Log($"Inventory loaded: {items.Count} items");
    }
    
    public void ClearInventory()
    {
        items.Clear();
        totalItemsCollected = 0;
        highestGearScore = 0;
        
        if (autoSaveOnChange)
        {
            SaveInventory();
        }
        
        Debug.Log("Inventory cleared");
    }
    
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(INVENTORY_SAVE_KEY);
        PlayerPrefs.DeleteKey("TotalItemsCollected");
        PlayerPrefs.DeleteKey("HighestGearScore");
        PlayerPrefs.Save();
        
        items.Clear();
        totalItemsCollected = 0;
        highestGearScore = 0;
        
        Debug.Log("Inventory save data reset");
    }
    
    private void OnApplicationQuit()
    {
        SaveInventory();
    }
}
