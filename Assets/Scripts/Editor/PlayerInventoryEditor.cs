using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(PlayerInventory))]
public class PlayerInventoryEditor : Editor
{
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private LootManager.Rarity filterRarity = LootManager.Rarity.Common;
    private bool useRarityFilter = false;
    private bool showSettings = true;
    private bool showStatistics = true;
    private bool showInventoryList = true;
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        PlayerInventory inventory = (PlayerInventory)target;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("PLAYER INVENTORY", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Persistent inventory system that saves between game sessions.\n" +
            "Items are automatically saved to PlayerPrefs.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showSettings, "Inventory Settings");
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxInventorySize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoSaveOnChange"));
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        showStatistics = EditorGUILayout.BeginFoldoutHeaderGroup(showStatistics, "Statistics");
        if (showStatistics)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Total Items: {inventory.items.Count} / {inventory.maxInventorySize}");
            EditorGUILayout.LabelField($"Items Collected (Lifetime): {inventory.totalItemsCollected}");
            EditorGUILayout.LabelField($"Highest Gear Score: {inventory.highestGearScore}");
            EditorGUILayout.LabelField($"Average Gear Score: {inventory.GetAverageGearScore()}");
            EditorGUILayout.LabelField($"Total Equipped Gear Score: {inventory.GetTotalGearScore()}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            var rarityBreakdown = GetRarityBreakdown(inventory);
            EditorGUILayout.LabelField("Rarity Breakdown:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var kvp in rarityBreakdown)
            {
                Color rarityColor = GetRarityColor(kvp.Key);
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = rarityColor;
                
                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value}", style);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        showInventoryList = EditorGUILayout.BeginFoldoutHeaderGroup(showInventoryList, $"Inventory Items ({inventory.items.Count})");
        if (showInventoryList)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField("Search:", searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            useRarityFilter = EditorGUILayout.Toggle("Filter by Rarity:", useRarityFilter);
            if (useRarityFilter)
            {
                filterRarity = (LootManager.Rarity)EditorGUILayout.EnumPopup(filterRarity);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Sort by Gear Score"))
            {
                inventory.SortByGearScore();
            }
            if (GUILayout.Button("Sort by Rarity"))
            {
                inventory.SortByRarity();
            }
            if (GUILayout.Button("Sort by Date"))
            {
                inventory.SortByAcquiredDate();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            var filteredItems = inventory.items.AsEnumerable();
            
            if (!string.IsNullOrEmpty(searchFilter))
            {
                filteredItems = filteredItems.Where(item => 
                    item.itemName.ToLower().Contains(searchFilter.ToLower()) ||
                    item.itemID.ToLower().Contains(searchFilter.ToLower())
                );
            }
            
            if (useRarityFilter)
            {
                filteredItems = filteredItems.Where(item => item.rarity == filterRarity);
            }
            
            var itemsList = filteredItems.ToList();
            
            if (itemsList.Count == 0)
            {
                EditorGUILayout.HelpBox("No items match the current filter.", MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(300));
                
                foreach (var item in itemsList)
                {
                    DrawInventoryItem(item, inventory);
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Persistence Controls", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Save Inventory"))
        {
            inventory.SaveInventory();
            EditorUtility.DisplayDialog("Saved", "Inventory saved to PlayerPrefs", "OK");
        }
        
        if (GUILayout.Button("Load Inventory"))
        {
            inventory.LoadInventory();
            EditorUtility.DisplayDialog("Loaded", "Inventory loaded from PlayerPrefs", "OK");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Clear Inventory"))
        {
            if (EditorUtility.DisplayDialog("Clear Inventory", 
                "Are you sure you want to clear all items?\nThis can be undone by loading the save.", 
                "Yes", "No"))
            {
                inventory.ClearInventory();
            }
        }
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Reset Save Data"))
        {
            if (EditorUtility.DisplayDialog("Reset Save Data", 
                "Are you sure you want to PERMANENTLY delete all saved inventory data?\nThis cannot be undone!", 
                "Yes, Delete", "No"))
            {
                inventory.ResetSave();
                EditorUtility.DisplayDialog("Reset", "All inventory save data has been deleted", "OK");
            }
        }
        
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawInventoryItem(InventoryItem item, PlayerInventory inventory)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        Color rarityColor = GetRarityColor(item.rarity);
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.normal.textColor = rarityColor;
        
        EditorGUILayout.LabelField(item.itemName, nameStyle, GUILayout.Width(150));
        
        EditorGUILayout.LabelField($"GS: {item.gearScore}", GUILayout.Width(70));
        EditorGUILayout.LabelField(item.rarity.ToString(), GUILayout.Width(80));
        EditorGUILayout.LabelField(item.itemType.ToString(), GUILayout.Width(80));
        
        if (item.isEquipped)
        {
            EditorGUILayout.LabelField("✓ Equipped", EditorStyles.miniLabel, GUILayout.Width(70));
        }
        
        if (GUILayout.Button("Remove", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Remove Item", 
                $"Remove {item.itemName} from inventory?", 
                "Yes", "No"))
            {
                inventory.RemoveItem(item);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (!string.IsNullOrEmpty(item.description))
        {
            EditorGUILayout.LabelField(item.description, EditorStyles.wordWrappedMiniLabel);
        }
        
        EditorGUILayout.LabelField($"ID: {item.itemID} | Acquired: {item.acquiredDate:g}", EditorStyles.miniLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private System.Collections.Generic.Dictionary<LootManager.Rarity, int> GetRarityBreakdown(PlayerInventory inventory)
    {
        var breakdown = new System.Collections.Generic.Dictionary<LootManager.Rarity, int>();
        
        foreach (LootManager.Rarity rarity in System.Enum.GetValues(typeof(LootManager.Rarity)))
        {
            breakdown[rarity] = inventory.items.Count(item => item.rarity == rarity);
        }
        
        return breakdown;
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
