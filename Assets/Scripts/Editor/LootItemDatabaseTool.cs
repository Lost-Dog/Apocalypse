using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LootItemDatabaseTool : EditorWindow
{
    private LootManager lootManager;
    private Vector2 scrollPosition;
    private bool showTemplates = false;
    private int selectedTemplate = 0;
    
    private string[] templateNames = new string[]
    {
        "Weapon - Pistol",
        "Weapon - Rifle", 
        "Weapon - Shotgun",
        "Weapon - Sniper",
        "Armor - Vest",
        "Armor - Helmet",
        "Gear - Backpack",
        "Gear - Gloves",
        "Consumable - Medkit",
        "Material - Crafting",
        "Collectible - Intel"
    };
    
    [MenuItem("Division Game/Loot System/Item Database Manager")]
    public static void ShowWindow()
    {
        LootItemDatabaseTool window = GetWindow<LootItemDatabaseTool>("Loot Database");
        window.minSize = new Vector2(500, 600);
    }
    
    private void OnEnable()
    {
        FindLootManager();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Loot Item Database Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (lootManager == null)
        {
            EditorGUILayout.HelpBox("LootManager not found! Make sure you have a LootManager component in your scene.", MessageType.Error);
            
            if (GUILayout.Button("Search for LootManager"))
            {
                FindLootManager();
            }
            return;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Connected to: {lootManager.name}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Total Items: {lootManager.lootableItems.Count}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        DrawQuickActions();
        
        EditorGUILayout.Space(10);
        
        DrawItemList();
    }
    
    private void DrawQuickActions()
    {
        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add Blank Item", GUILayout.Height(30)))
        {
            AddBlankItem();
        }
        
        if (GUILayout.Button("Add From Template", GUILayout.Height(30)))
        {
            showTemplates = !showTemplates;
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (showTemplates)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Select Template:", EditorStyles.miniBoldLabel);
            selectedTemplate = EditorGUILayout.Popup(selectedTemplate, templateNames);
            
            if (GUILayout.Button("Create from Template"))
            {
                AddItemFromTemplate(selectedTemplate);
                showTemplates = false;
            }
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Sort by Rarity"))
        {
            SortByRarity();
        }
        
        if (GUILayout.Button("Sort by Type"))
        {
            SortByType();
        }
        
        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog("Clear Database", 
                "Are you sure you want to clear all loot items? This cannot be undone!", 
                "Yes, Clear", "Cancel"))
            {
                ClearAllItems();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawItemList()
    {
        GUILayout.Label($"Loot Items ({lootManager.lootableItems.Count})", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (lootManager.lootableItems.Count == 0)
        {
            EditorGUILayout.HelpBox("No items in database. Click 'Add Blank Item' or 'Add From Template' to start.", MessageType.Info);
        }
        
        for (int i = 0; i < lootManager.lootableItems.Count; i++)
        {
            DrawItemEntry(i);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawItemEntry(int index)
    {
        LootItemData item = lootManager.lootableItems[index];
        
        if (item == null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index + 1}.", GUILayout.Width(30));
            EditorGUILayout.LabelField("⚠️ NULL ITEM - Remove this slot", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                RemoveItem(index);
                return;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
            return;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        Color rarityColor = GetRarityColor(item.rarity);
        GUIStyle rarityStyle = new GUIStyle(EditorStyles.boldLabel);
        rarityStyle.normal.textColor = rarityColor;
        
        EditorGUILayout.LabelField($"{index + 1}.", GUILayout.Width(30));
        EditorGUILayout.LabelField(item.rarity.ToString(), rarityStyle, GUILayout.Width(80));
        
        string displayName = string.IsNullOrEmpty(item.itemName) ? "<Unnamed>" : item.itemName;
        EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
        
        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            RemoveItem(index);
            return;
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.indentLevel++;
        
        item.itemID = EditorGUILayout.TextField("Item ID", item.itemID);
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
        item.rarity = (LootManager.Rarity)EditorGUILayout.EnumPopup("Rarity", item.rarity);
        item.itemType = (LootItemData.ItemType)EditorGUILayout.EnumPopup("Type", item.itemType);
        item.baseGearScore = EditorGUILayout.IntField("Base Gear Score", item.baseGearScore);
        item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);
        item.worldPrefab = (GameObject)EditorGUILayout.ObjectField("World Prefab", item.worldPrefab, typeof(GameObject), false);
        item.description = EditorGUILayout.TextField("Description", item.description, GUILayout.Height(40));
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
    }
    
    private void FindLootManager()
    {
        lootManager = FindFirstObjectByType<LootManager>();
        
        if (lootManager == null)
        {
            GameObject gameSystemsObj = GameObject.Find("GameSystems");
            if (gameSystemsObj != null)
            {
                lootManager = gameSystemsObj.GetComponent<LootManager>();
            }
        }
    }
    
    private void AddBlankItem()
    {
        string folderPath = "Assets/Game/Loot/Items";
        
        if (!AssetDatabase.IsValidFolder("Assets/Game"))
        {
            AssetDatabase.CreateFolder("Assets", "Game");
        }
        
        if (!AssetDatabase.IsValidFolder("Assets/Game/Loot"))
        {
            AssetDatabase.CreateFolder("Assets/Game", "Loot");
        }
        
        if (!AssetDatabase.IsValidFolder("Assets/Game/Loot/Items"))
        {
            AssetDatabase.CreateFolder("Assets/Game/Loot", "Items");
        }
        
        LootItemData newItem = ScriptableObject.CreateInstance<LootItemData>();
        newItem.itemName = "New Item";
        newItem.rarity = LootManager.Rarity.Common;
        newItem.itemType = LootItemData.ItemType.Weapon;
        newItem.baseGearScore = 100;
        newItem.description = "Item description";
        
        string assetPath = $"{folderPath}/NewItem_{System.Guid.NewGuid().ToString().Substring(0, 8)}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(newItem, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        lootManager.lootableItems.Add(newItem);
        EditorUtility.SetDirty(lootManager);
        
        Debug.Log($"Added new loot item: {newItem.itemName} at {assetPath}");
    }
    
    private void AddItemFromTemplate(int templateIndex)
    {
        string folderPath = "Assets/Game/Loot/Items";
        
        if (!AssetDatabase.IsValidFolder("Assets/Game/Loot/Items"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game"))
                AssetDatabase.CreateFolder("Assets", "Game");
            if (!AssetDatabase.IsValidFolder("Assets/Game/Loot"))
                AssetDatabase.CreateFolder("Assets/Game", "Loot");
            AssetDatabase.CreateFolder("Assets/Game/Loot", "Items");
        }
        
        LootItemData newItem = CreateItemFromTemplate(templateIndex);
        
        string assetPath = $"{folderPath}/{newItem.itemName.Replace(" ", "")}_{System.Guid.NewGuid().ToString().Substring(0, 8)}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(newItem, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        lootManager.lootableItems.Add(newItem);
        EditorUtility.SetDirty(lootManager);
        
        Debug.Log($"Created item from template: {newItem.itemName} at {assetPath}");
    }
    
    private LootItemData CreateItemFromTemplate(int templateIndex)
    {
        LootItemData item = ScriptableObject.CreateInstance<LootItemData>();
        
        switch (templateIndex)
        {
            case 0:
                item.itemName = "Standard Pistol";
                item.itemType = LootItemData.ItemType.Weapon;
                item.rarity = LootManager.Rarity.Common;
                item.baseGearScore = 100;
                item.description = "Basic sidearm for close combat";
                break;
                
            case 1:
                item.itemName = "Assault Rifle";
                item.itemType = LootItemData.ItemType.Weapon;
                item.rarity = LootManager.Rarity.Uncommon;
                item.baseGearScore = 150;
                item.description = "Military-grade rifle for medium range";
                break;
                
            case 2:
                item.itemName = "Combat Shotgun";
                item.itemType = LootItemData.ItemType.Weapon;
                item.rarity = LootManager.Rarity.Rare;
                item.baseGearScore = 200;
                item.description = "High damage close-quarters weapon";
                break;
                
            case 3:
                item.itemName = "Marksman Rifle";
                item.itemType = LootItemData.ItemType.Weapon;
                item.rarity = LootManager.Rarity.Epic;
                item.baseGearScore = 250;
                item.description = "Precision long-range weapon";
                break;
                
            case 4:
                item.itemName = "Tactical Vest";
                item.itemType = LootItemData.ItemType.Armor;
                item.rarity = LootManager.Rarity.Uncommon;
                item.baseGearScore = 150;
                item.description = "Protective body armor";
                break;
                
            case 5:
                item.itemName = "Combat Helmet";
                item.itemType = LootItemData.ItemType.Armor;
                item.rarity = LootManager.Rarity.Rare;
                item.baseGearScore = 200;
                item.description = "Head protection gear";
                break;
                
            case 6:
                item.itemName = "Field Backpack";
                item.itemType = LootItemData.ItemType.Gear;
                item.rarity = LootManager.Rarity.Common;
                item.baseGearScore = 100;
                item.description = "Increases carrying capacity";
                break;
                
            case 7:
                item.itemName = "Tactical Gloves";
                item.itemType = LootItemData.ItemType.Gear;
                item.rarity = LootManager.Rarity.Uncommon;
                item.baseGearScore = 150;
                item.description = "Improves weapon handling";
                break;
                
            case 8:
                item.itemName = "First Aid Kit";
                item.itemType = LootItemData.ItemType.Consumable;
                item.rarity = LootManager.Rarity.Common;
                item.baseGearScore = 50;
                item.description = "Restores health";
                break;
                
            case 9:
                item.itemName = "Weapon Parts";
                item.itemType = LootItemData.ItemType.Material;
                item.rarity = LootManager.Rarity.Common;
                item.baseGearScore = 50;
                item.description = "Used for crafting and upgrades";
                break;
                
            case 10:
                item.itemName = "Division Tech";
                item.itemType = LootItemData.ItemType.Collectible;
                item.rarity = LootManager.Rarity.Legendary;
                item.baseGearScore = 500;
                item.description = "Rare high-end crafting material";
                break;
        }
        
        return item;
    }
    
    private void RemoveItem(int index)
    {
        if (index < 0 || index >= lootManager.lootableItems.Count)
            return;
        
        Undo.RecordObject(lootManager, "Remove Loot Item");
        lootManager.lootableItems.RemoveAt(index);
        EditorUtility.SetDirty(lootManager);
    }
    
    private void SortByRarity()
    {
        Undo.RecordObject(lootManager, "Sort by Rarity");
        lootManager.lootableItems.Sort((a, b) => b.rarity.CompareTo(a.rarity));
        EditorUtility.SetDirty(lootManager);
    }
    
    private void SortByType()
    {
        Undo.RecordObject(lootManager, "Sort by Type");
        lootManager.lootableItems.Sort((a, b) => a.itemType.CompareTo(b.itemType));
        EditorUtility.SetDirty(lootManager);
    }
    
    private void ClearAllItems()
    {
        Undo.RecordObject(lootManager, "Clear All Items");
        lootManager.lootableItems.Clear();
        EditorUtility.SetDirty(lootManager);
        Debug.Log("Cleared all loot items");
    }
    
    private Color GetRarityColor(LootManager.Rarity rarity)
    {
        switch (rarity)
        {
            case LootManager.Rarity.Common:
                return Color.white;
            case LootManager.Rarity.Uncommon:
                return Color.green;
            case LootManager.Rarity.Rare:
                return Color.blue;
            case LootManager.Rarity.Epic:
                return new Color(0.6f, 0f, 1f);
            case LootManager.Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }
}
