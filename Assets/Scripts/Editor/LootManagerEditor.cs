using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(LootManager))]
public class LootManagerEditor : Editor
{
    private SerializedProperty lootableItems;
    private SerializedProperty lootPools;
    
    private bool showLootableItems = true;
    private bool showLootPools = true;
    private bool showGearScoreSettings = true;
    private bool showRarityChances = true;
    private bool showLevelScaling = true;
    private bool showDebugTools = false;
    
    private int previewLevel = 1;
    
    private void OnEnable()
    {
        lootableItems = serializedObject.FindProperty("lootableItems");
        lootPools = serializedObject.FindProperty("lootPools");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LootManager lootManager = (LootManager)target;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("LOOT MANAGER", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Configure lootable items, drop chances, and gear score ranges.\n" +
            "Items are added to the player's inventory on pickup.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        showLootableItems = EditorGUILayout.BeginFoldoutHeaderGroup(showLootableItems, "Lootable Items Database");
        if (showLootableItems)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox(
                "Add all lootable items here. Each item needs:\n" +
                "• Unique Item ID\n" +
                "• Name and Description\n" +
                "• Rarity (affects drop chance)\n" +
                "• Item Type (weapon, armor, etc)\n" +
                "• World Prefab (optional, uses pool if not set)",
                MessageType.None
            );
            
            if (GUILayout.Button("Auto-Find All Loot Items in Project", GUILayout.Height(25)))
            {
                AutoFindLootItems(lootManager);
            }
            
            EditorGUILayout.Space(5);
            
            int newSize = EditorGUILayout.IntField("Number of Items", lootableItems.arraySize);
            if (newSize != lootableItems.arraySize)
            {
                lootableItems.arraySize = newSize;
            }
            
            EditorGUILayout.Space(5);
            
            for (int i = 0; i < lootableItems.arraySize; i++)
            {
                EditorGUILayout.PropertyField(lootableItems.GetArrayElementAtIndex(i), new GUIContent($"Item {i}"));
            }
            
            if (lootableItems.arraySize == 0)
            {
                EditorGUILayout.HelpBox("⚠️ No lootable items configured! Add items to enable loot drops.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Total Items: {lootableItems.arraySize}", EditorStyles.miniLabel);
                
                var itemsByRarity = GetItemCountsByRarity(lootManager);
                EditorGUILayout.LabelField(
                    $"C:{itemsByRarity[LootRarity.Common]} " +
                    $"U:{itemsByRarity[LootRarity.Uncommon]} " +
                    $"R:{itemsByRarity[LootRarity.Rare]} " +
                    $"E:{itemsByRarity[LootRarity.Epic]} " +
                    $"L:{itemsByRarity[LootRarity.Legendary]}",
                    EditorStyles.miniLabel
                );
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        showLootPools = EditorGUILayout.BeginFoldoutHeaderGroup(showLootPools, "Loot Prefab Pools (Fallback)");
        if (showLootPools)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox(
                "Used when LootItemData doesn't have a worldPrefab assigned.\n" +
                "Configure generic loot prefabs for each rarity.",
                MessageType.None
            );
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultLootPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lootDropForce"));
            
            EditorGUILayout.Space(5);
            
            int poolSize = EditorGUILayout.IntField("Number of Pools", lootPools.arraySize);
            if (poolSize != lootPools.arraySize)
            {
                lootPools.arraySize = poolSize;
            }
            
            for (int i = 0; i < lootPools.arraySize; i++)
            {
                EditorGUILayout.PropertyField(lootPools.GetArrayElementAtIndex(i), new GUIContent($"Pool {i}"), true);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        showGearScoreSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGearScoreSettings, "Gear Score Settings");
        if (showGearScoreSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minGearScore"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxGearScore"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Gear Score Formula:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Base (100) + (Level × 40) + (Rarity × 50) ± Random(10)", EditorStyles.miniLabel);
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        showRarityChances = EditorGUILayout.BeginFoldoutHeaderGroup(showRarityChances, "Rarity Drop Chances");
        if (showRarityChances)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox(
                "Base rarity chances (at Level 1).\n" +
                "These will be scaled with player level - see Level Scaling below.",
                MessageType.None
            );
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("commonChance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uncommonChance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rareChance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("epicChance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("legendaryChance"));
            
            float total = lootManager.commonChance + lootManager.uncommonChance + 
                         lootManager.rareChance + lootManager.epicChance + lootManager.legendaryChance;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Total Chance: {total:F2}", EditorStyles.boldLabel);
            
            if (total < 0.99f || total > 1.01f)
            {
                EditorGUILayout.HelpBox($"Total chances should equal 1.0 (currently {total:F2})", MessageType.Warning);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        showLevelScaling = EditorGUILayout.BeginFoldoutHeaderGroup(showLevelScaling, "Level Scaling");
        if (showLevelScaling)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox(
                "As player level increases, rare drops become more common.\n" +
                "Common drops decrease to make room for better loot.",
                MessageType.Info
            );
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rarityBonusPerLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScalingLevel"));
            
            EditorGUILayout.Space(10);
            
            previewLevel = EditorGUILayout.IntSlider("Preview at Level", previewLevel, 1, lootManager.maxScalingLevel);
            
            lootManager.GetScaledRarityChances(previewLevel, 
                out float common, out float uncommon, out float rare, out float epic, out float legendary);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Drop Chances at Level {previewLevel}:", EditorStyles.boldLabel);
            
            DrawRarityBar("Common", common, new Color(0.7f, 0.7f, 0.7f));
            DrawRarityBar("Uncommon", uncommon, Color.green);
            DrawRarityBar("Rare", rare, Color.blue);
            DrawRarityBar("Epic", epic, new Color(0.6f, 0f, 1f));
            DrawRarityBar("Legendary", legendary, new Color(1f, 0.5f, 0f));
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Compare All Levels", GUILayout.Height(25)))
            {
                ShowLevelComparison(lootManager);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);

        EditorGUILayout.Space(5);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onLootDropped"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onItemCollected"));
        
        EditorGUILayout.Space(10);
        
        showDebugTools = EditorGUILayout.BeginFoldoutHeaderGroup(showDebugTools, "Debug Tools");
        if (showDebugTools)
        {
            EditorGUI.indentLevel++;
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Debug tools only available in Play Mode", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Drop Common Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 1, LootRarity.Common);
                }
                if (GUILayout.Button("Drop Rare Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 10, LootRarity.Rare);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Drop Epic Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 20, LootRarity.Epic);
                }
                if (GUILayout.Button("Drop Legendary Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 30, LootRarity.Legendary);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private System.Collections.Generic.Dictionary<LootRarity, int> GetItemCountsByRarity(LootManager manager)
    {
        var counts = new System.Collections.Generic.Dictionary<LootRarity, int>();
        
        foreach (LootRarity rarity in System.Enum.GetValues(typeof(LootRarity)))
        {
            counts[rarity] = manager.lootableItems.Count(item => item != null && item.rarity == rarity);
        }
        
        return counts;
    }
    
    private Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position + Vector3.forward * 2f;
        }
        
        return Vector3.zero;
    }
    
    private void AutoFindLootItems(LootManager lootManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:LootItemData");
        lootManager.lootableItems.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LootItemData item = AssetDatabase.LoadAssetAtPath<LootItemData>(path);
            if (item != null)
            {
                lootManager.lootableItems.Add(item);
            }
        }
        
        EditorUtility.SetDirty(lootManager);
        serializedObject.Update();
        Debug.Log($"Found and added {lootManager.lootableItems.Count} loot items to the database.");
    }
    
    private void DrawRarityBar(string label, float percentage, Color color)
    {
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField(label, GUILayout.Width(80));
        
        Rect rect = GUILayoutUtility.GetRect(100, 18);
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * (percentage / 100f), rect.height);
        EditorGUI.DrawRect(fillRect, color);
        
        EditorGUILayout.LabelField($"{percentage:F1}%", GUILayout.Width(50));
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void ShowLevelComparison(LootManager lootManager)
    {
        string comparison = "=== RARITY SCALING BY LEVEL ===\n\n";
        
        int[] levels = { 1, 5, 10, 15, 20, 25, 30 };
        
        foreach (int level in levels)
        {
            lootManager.GetScaledRarityChances(level, 
                out float common, out float uncommon, out float rare, out float epic, out float legendary);
            
            comparison += $"Level {level}:\n";
            comparison += $"  Common:    {common:F1}%\n";
            comparison += $"  Uncommon:  {uncommon:F1}%\n";
            comparison += $"  Rare:      {rare:F1}%\n";
            comparison += $"  Epic:      {epic:F1}%\n";
            comparison += $"  Legendary: {legendary:F1}%\n\n";
        }
        
        Debug.Log(comparison);
        
        EditorUtility.DisplayDialog("Level Comparison", 
            "Rarity chances for different levels have been logged to Console.\n\n" +
            "Check the Console window to see the breakdown.", 
            "OK");
    }
}
