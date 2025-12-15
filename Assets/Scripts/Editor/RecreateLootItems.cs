using UnityEngine;
using UnityEditor;
using System.IO;

public class RecreateLootItems : EditorWindow
{
    [MenuItem("Tools/Recreate All Loot Items")]
    public static void RecreateItems()
    {
        string itemsPath = "Assets/Game/Loot/Items";
        
        if (!Directory.Exists(itemsPath))
        {
            Directory.CreateDirectory(itemsPath);
        }
        
        CreateLootItem("StandardPistol", "Standard Pistol", LootManager.Rarity.Common, 
            LootItemData.ItemType.Weapon, 100, 
            "Assets/Synty/InterfaceApocalypseHUD/Sprites/Icons_Weapons/ICON_SM_Wep_Pistol_01.png",
            "Assets/Synty/PolygonApocalypse/Prefabs/Weapons/Guns/SM_Wep_Pistol_01.prefab",
            "Basic sidearm for close combat");
            
        CreateLootItem("AssaultRifle", "Assault Rifle", LootManager.Rarity.Uncommon, 
            LootItemData.ItemType.Weapon, 150, 
            "Assets/Synty/InterfaceApocalypseHUD/Sprites/Icons_Weapons/ICON_SM_Wep_AssaultRifle_02_Clean.png",
            "Assets/Synty/PolygonApocalypse/Prefabs/Weapons/Guns/SM_Wep_AssaultRifle_01.prefab",
            "Military-grade rifle for medium range");
            
        CreateLootItem("CombatShotgun", "Combat Shotgun", LootManager.Rarity.Rare, 
            LootItemData.ItemType.Weapon, 200, 
            "Assets/Julhiecio TPS Controller/Textures/Generated Icons/Shotgun.png",
            "Assets/Synty/PolygonApocalypse/Prefabs/Weapons/Guns/SM_Wep_Shotgun_01.prefab",
            "High damage close-quarters weapon");
            
        CreateLootItem("MarksmanRifle", "Marksman Rifle", LootManager.Rarity.Rare, 
            LootItemData.ItemType.Weapon, 200, 
            "Assets/Synty/InterfaceApocalypseHUD/Sprites/Icons_Weapons/ICON_SM_Wep_SniperRifle_01.png",
            "Assets/Synty/PolygonApocalypse/Prefabs/Weapons/Guns/SM_Wep_SniperRifle_01.prefab",
            "Precision long-range weapon");
            
        CreateLootItem("TacticalVest", "Tactical Vest", LootManager.Rarity.Uncommon, 
            LootItemData.ItemType.Armor, 120, 
            null, null,
            "Provides moderate protection");
            
        CreateLootItem("CombatHelmet", "Combat Helmet", LootManager.Rarity.Rare, 
            LootItemData.ItemType.Armor, 180, 
            null, null,
            "Military-grade head protection");
            
        CreateLootItem("FieldBackpack", "Field Backpack", LootManager.Rarity.Common, 
            LootItemData.ItemType.Gear, 100, 
            null, null,
            "Increases carrying capacity");
            
        CreateLootItem("TacticalGloves", "Tactical Gloves", LootManager.Rarity.Common, 
            LootItemData.ItemType.Gear, 80, 
            null, null,
            "Improves weapon handling");
            
        CreateLootItem("FirstAidKit", "First Aid Kit", LootManager.Rarity.Uncommon, 
            LootItemData.ItemType.Consumable, 0, 
            null, null,
            "Restores health over time");
            
        CreateLootItem("WeaponParts", "Weapon Parts", LootManager.Rarity.Common, 
            LootItemData.ItemType.Material, 0, 
            null, null,
            "Used for crafting and upgrades");
            
        CreateLootItem("DivisionTech", "Division Tech", LootManager.Rarity.Epic, 
            LootItemData.ItemType.Material, 0, 
            null, null,
            "Rare crafting material");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("<color=green>Successfully created 11 loot items in /Assets/Game/Loot/Items/</color>");
        EditorUtility.DisplayDialog("Success", "Created 11 loot items!\n\nNow click 'Auto-Find All Loot Items in Project' on the LootManager.", "OK");
    }
    
    private static void CreateLootItem(string fileName, string itemName, LootManager.Rarity rarity, 
        LootItemData.ItemType itemType, int baseGearScore, string iconPath, string prefabPath, string description)
    {
        LootItemData item = ScriptableObject.CreateInstance<LootItemData>();
        
        item.itemID = System.Guid.NewGuid().ToString();
        item.itemName = itemName;
        item.rarity = rarity;
        item.itemType = itemType;
        item.baseGearScore = baseGearScore;
        item.description = description;
        
        if (!string.IsNullOrEmpty(iconPath))
        {
            item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        }
        
        if (!string.IsNullOrEmpty(prefabPath))
        {
            item.worldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
        
        string assetPath = $"Assets/Game/Loot/Items/{fileName}.asset";
        
        if (File.Exists(assetPath))
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
        
        AssetDatabase.CreateAsset(item, assetPath);
        
        Debug.Log($"Created: {assetPath}");
    }
}
