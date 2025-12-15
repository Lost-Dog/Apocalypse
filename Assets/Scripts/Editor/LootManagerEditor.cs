using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(LootManager))]
public class LootManagerEditor : Editor
{
    private SerializedProperty lootableItems;
    private SerializedProperty lootPools;
    private SerializedProperty playerInventory;
    
    private bool showLootableItems = true;
    private bool showLootPools = true;
    private bool showGearScoreSettings = true;
    private bool showRarityChances = true;
    private bool showLevelScaling = true;
    private bool showVisibilitySettings = true;
    private bool showDebugTools = false;
    
    private int previewLevel = 1;
    
    private void OnEnable()
    {
        lootableItems = serializedObject.FindProperty("lootableItems");
        lootPools = serializedObject.FindProperty("lootPools");
        playerInventory = serializedObject.FindProperty("playerInventory");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LootManager lootManager = (LootManager)target;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("LOOT MANAGER", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Configure lootable items, drop chances, and gear score ranges.\n" +
            "Items will persist in PlayerInventory between game sessions.",
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
                    $"C:{itemsByRarity[LootManager.Rarity.Common]} " +
                    $"U:{itemsByRarity[LootManager.Rarity.Uncommon]} " +
                    $"R:{itemsByRarity[LootManager.Rarity.Rare]} " +
                    $"E:{itemsByRarity[LootManager.Rarity.Epic]} " +
                    $"L:{itemsByRarity[LootManager.Rarity.Legendary]}",
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
        
        EditorGUILayout.PropertyField(playerInventory);
        
        EditorGUILayout.Space(5);
        
        showVisibilitySettings = EditorGUILayout.BeginFoldoutHeaderGroup(showVisibilitySettings, "🎨 Loot Visibility Settings");
        if (showVisibilitySettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox(
                "Control how visible spawned loot is to players.\\n" +
                "Simple Outline = Low performance impact (recommended)\\n" +
                "Advanced = Light beams, rings, particles (higher cost)\\n" +
                "Compass Markers = Off-screen UI markers",
                MessageType.Info
            );
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableVisibilityHelpers"), 
                new GUIContent("Enable Visibility Helpers", "Master toggle for all visibility aids"));
            
            if (lootManager.enableVisibilityHelpers)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useSimpleOutline"), 
                    new GUIContent("✓ Simple Outline Glow", "Emission glow on loot materials (RECOMMENDED)"));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useAdvancedVisibility"), 
                    new GUIContent("⚡ Advanced Visibility", "Light beams, ground rings, particles (performance cost)"));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useCompassMarkers"), 
                    new GUIContent("🧭 Compass Markers", "UI markers for off-screen loot"));
                
                EditorGUILayout.Space(10);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 12;
                EditorGUILayout.LabelField("Current Configuration:", headerStyle);
                
                EditorGUILayout.Space(3);
                
                if (lootManager.useSimpleOutline && !lootManager.useAdvancedVisibility && !lootManager.useCompassMarkers)
                {
                    EditorGUILayout.LabelField("⭐ Mode: BALANCED (Recommended)", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("  • Emission glow on all loot", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("  • Color-coded by rarity", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("  • Low performance impact", EditorStyles.miniLabel);
                }
                else if (lootManager.useAdvancedVisibility && lootManager.useSimpleOutline)
                {
                    EditorGUILayout.LabelField("🌟 Mode: HIGH VISIBILITY", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("  • Light beams and ground rings", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("  • Pulsing glow effects", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("  • ⚠️ Higher performance cost", EditorStyles.miniLabel);
                }
                else if (!lootManager.useSimpleOutline && !lootManager.useAdvancedVisibility)
                {
                    EditorGUILayout.LabelField("Mode: Minimal", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("  • No visual aids enabled", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("Mode: Custom", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("  • Custom configuration", EditorStyles.miniLabel);
                }
                
                if (lootManager.useCompassMarkers)
                {
                    EditorGUILayout.LabelField("  • Off-screen markers enabled", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("Rarity Colors:", EditorStyles.boldLabel);
                DrawRarityColorLegend();
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("📖 Open Visibility System Guide", GUILayout.Height(30)))
                {
                    OpenVisibilityGuide();
                }
                
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Visibility helpers are disabled. Loot may be hard to see!", MessageType.Warning);
                
                if (GUILayout.Button("Enable Default Visibility (Recommended)", GUILayout.Height(25)))
                {
                    lootManager.enableVisibilityHelpers = true;
                    lootManager.useSimpleOutline = true;
                    lootManager.useAdvancedVisibility = false;
                    lootManager.useCompassMarkers = false;
                    EditorUtility.SetDirty(lootManager);
                }
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
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
                    lootManager.DropLootWithRarity(pos, 1, LootManager.Rarity.Common);
                }
                if (GUILayout.Button("Drop Rare Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 10, LootManager.Rarity.Rare);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Drop Epic Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 20, LootManager.Rarity.Epic);
                }
                if (GUILayout.Button("Drop Legendary Loot"))
                {
                    Vector3 pos = GetPlayerPosition();
                    lootManager.DropLootWithRarity(pos, 30, LootManager.Rarity.Legendary);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                if (lootManager.playerInventory != null)
                {
                    EditorGUILayout.LabelField($"Inventory: {lootManager.playerInventory.items.Count} items", EditorStyles.boldLabel);
                    
                    if (GUILayout.Button("View Inventory"))
                    {
                        Selection.activeObject = lootManager.playerInventory;
                    }
                }
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private System.Collections.Generic.Dictionary<LootManager.Rarity, int> GetItemCountsByRarity(LootManager manager)
    {
        var counts = new System.Collections.Generic.Dictionary<LootManager.Rarity, int>();
        
        foreach (LootManager.Rarity rarity in System.Enum.GetValues(typeof(LootManager.Rarity)))
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
    
    private void DrawRarityColorLegend()
    {
        DrawColorLegendItem("Common", new Color(0.8f, 0.8f, 0.8f));
        DrawColorLegendItem("Uncommon", new Color(0.1f, 0.9f, 0.1f));
        DrawColorLegendItem("Rare", new Color(0.2f, 0.5f, 1f));
        DrawColorLegendItem("Epic", new Color(0.64f, 0.21f, 0.93f));
        DrawColorLegendItem("Legendary", new Color(1f, 0.5f, 0f));
    }
    
    private void DrawColorLegendItem(string label, Color color)
    {
        EditorGUILayout.BeginHorizontal();
        Rect colorRect = GUILayoutUtility.GetRect(12, 12, GUILayout.ExpandWidth(false));
        EditorGUI.DrawRect(colorRect, color);
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }
    
    private void OpenVisibilityGuide()
    {
        string guidePath = "Assets/Pages/Loot Visibility System Guide.md";
        UnityEngine.Object guideAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(guidePath);
        if (guideAsset != null)
        {
            Selection.activeObject = guideAsset;
            EditorGUIUtility.PingObject(guideAsset);
        }
        else
        {
            Debug.LogWarning("Could not find Loot Visibility System Guide at: " + guidePath);
        }
    }
}
