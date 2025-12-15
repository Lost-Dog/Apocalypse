using UnityEngine;
using UnityEditor;

public class LootItemCreator : EditorWindow
{
    private string itemName = "New Item";
    private LootManager.Rarity rarity = LootManager.Rarity.Common;
    private LootItemData.ItemType itemType = LootItemData.ItemType.Weapon;
    private int baseGearScore = 100;
    private string description = "";
    private Sprite icon;
    private GameObject worldPrefab;
    
    [MenuItem("Apocalypse/Loot/Create Loot Item")]
    public static void ShowWindow()
    {
        GetWindow<LootItemCreator>("Loot Item Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Create New Loot Item", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        itemName = EditorGUILayout.TextField("Item Name", itemName);
        rarity = (LootManager.Rarity)EditorGUILayout.EnumPopup("Rarity", rarity);
        itemType = (LootItemData.ItemType)EditorGUILayout.EnumPopup("Item Type", itemType);
        baseGearScore = EditorGUILayout.IntField("Base Gear Score", baseGearScore);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Assets", EditorStyles.boldLabel);
        icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
        worldPrefab = (GameObject)EditorGUILayout.ObjectField("World Prefab", worldPrefab, typeof(GameObject), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Loot Item", GUILayout.Height(30)))
        {
            CreateLootItem();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will create a new LootItemData ScriptableObject asset in /Assets/Game/Loot/Items", MessageType.Info);
    }
    
    private void CreateLootItem()
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
        newItem.itemName = itemName;
        newItem.rarity = rarity;
        newItem.itemType = itemType;
        newItem.baseGearScore = baseGearScore;
        newItem.description = description;
        newItem.icon = icon;
        newItem.worldPrefab = worldPrefab;
        
        string assetPath = $"{folderPath}/{itemName}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(newItem, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorGUIUtility.PingObject(newItem);
        Selection.activeObject = newItem;
        
        Debug.Log($"Created loot item: {itemName} at {assetPath}");
        
        itemName = "New Item";
        description = "";
        icon = null;
        worldPrefab = null;
    }
}
