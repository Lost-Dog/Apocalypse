using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ChallengeData))]
public class ChallengeDataEditor : Editor
{
    private SerializedProperty spawnItemsProp;
    private bool showSpawnItems = true;
    private Dictionary<int, bool> itemFoldouts = new Dictionary<int, bool>();
    
    private void OnEnable()
    {
        spawnItemsProp = serializedObject.FindProperty("spawnItems");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Spawn Items Summary", EditorStyles.boldLabel);
        DrawSpawnSummary();
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Add Common Spawn Presets", GUILayout.Height(30)))
        {
            ShowPresetMenu();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawSpawnSummary()
    {
        ChallengeData data = (ChallengeData)target;
        
        if (data.spawnItems == null || data.spawnItems.Count == 0)
        {
            EditorGUILayout.HelpBox("No spawn items configured. Add items to spawn enemies, loot, props, etc.", MessageType.Info);
            return;
        }
        
        int enemyCount = 0;
        int civilianCount = 0;
        int lootCount = 0;
        int propCount = 0;
        int otherCount = 0;
        
        foreach (var item in data.spawnItems)
        {
            int count = item.maxCount;
            
            switch (item.category)
            {
                case ChallengeData.SpawnableCategory.Enemy:
                case ChallengeData.SpawnableCategory.Boss:
                    enemyCount += count;
                    break;
                case ChallengeData.SpawnableCategory.Civilian:
                    civilianCount += count;
                    break;
                case ChallengeData.SpawnableCategory.LootBox:
                    lootCount += count;
                    break;
                case ChallengeData.SpawnableCategory.Prop:
                case ChallengeData.SpawnableCategory.Cover:
                case ChallengeData.SpawnableCategory.Vehicle:
                    propCount += count;
                    break;
                default:
                    otherCount += count;
                    break;
            }
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"Total Spawn Items: {data.spawnItems.Count}");
        
        if (enemyCount > 0)
            EditorGUILayout.LabelField($"  • Enemies/Bosses: {enemyCount}", EditorStyles.miniLabel);
        if (civilianCount > 0)
            EditorGUILayout.LabelField($"  • Civilians: {civilianCount}", EditorStyles.miniLabel);
        if (lootCount > 0)
            EditorGUILayout.LabelField($"  • Loot Boxes: {lootCount}", EditorStyles.miniLabel);
        if (propCount > 0)
            EditorGUILayout.LabelField($"  • Props/Cover: {propCount}", EditorStyles.miniLabel);
        if (otherCount > 0)
            EditorGUILayout.LabelField($"  • Other: {otherCount}", EditorStyles.miniLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void ShowPresetMenu()
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Enemy Pack (5 Random Enemies)"), false, () => AddPreset_Enemies());
        menu.AddItem(new GUIContent("Civilian Pack (3 Random Civilians)"), false, () => AddPreset_Civilians());
        menu.AddItem(new GUIContent("Loot Pack (3-5 Loot Boxes)"), false, () => AddPreset_Loot());
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Boss + Minions"), false, () => AddPreset_BossEncounter());
        menu.AddItem(new GUIContent("Supply Drop Setup"), false, () => AddPreset_SupplyDrop());
        menu.AddItem(new GUIContent("Control Point Setup"), false, () => AddPreset_ControlPoint());
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Clear All Spawn Items"), false, () => ClearAllItems());
        
        menu.ShowAsContext();
    }
    
    private void AddPreset_Enemies()
    {
        ChallengeData data = (ChallengeData)target;
        Undo.RecordObject(data, "Add Enemy Preset");
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Patrol Enemies",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 20f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 1
        });
        
        EditorUtility.SetDirty(data);
    }
    
    private void AddPreset_Civilians()
    {
        ChallengeData data = (ChallengeData)target;
        Undo.RecordObject(data, "Add Civilian Preset");
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Civilians",
            category = ChallengeData.SpawnableCategory.Civilian,
            minCount = 2,
            maxCount = 3,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 15f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 2
        });
        
        EditorUtility.SetDirty(data);
    }
    
    private void AddPreset_Loot()
    {
        ChallengeData data = (ChallengeData)target;
        Undo.RecordObject(data, "Add Loot Preset");
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Loot Boxes",
            category = ChallengeData.SpawnableCategory.LootBox,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 12f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 0
        });
        
        EditorUtility.SetDirty(data);
    }
    
    private void AddPreset_BossEncounter()
    {
        ChallengeData data = (ChallengeData)target;
        Undo.RecordObject(data, "Add Boss Encounter Preset");
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Boss",
            category = ChallengeData.SpawnableCategory.Boss,
            minCount = 1,
            maxCount = 1,
            spawnLocation = ChallengeData.SpawnLocationType.AtCenter,
            spawnRadius = 0f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 10,
            required = true
        });
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Boss Minions",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 10f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5
        });
        
        EditorUtility.SetDirty(data);
    }
    
    private void AddPreset_SupplyDrop()
    {
        ChallengeData data = (ChallengeData)target;
        Undo.RecordObject(data, "Add Supply Drop Preset");
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Drop Pod",
            category = ChallengeData.SpawnableCategory.Objective,
            minCount = 1,
            maxCount = 1,
            spawnLocation = ChallengeData.SpawnLocationType.AtCenter,
            requireNavMesh = false,
            randomRotation = false,
            priority = 10,
            required = true
        });
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Guard Enemies",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 4,
            maxCount = 6,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 15f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5
        });
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Loot Crates",
            category = ChallengeData.SpawnableCategory.LootBox,
            minCount = 4,
            maxCount = 6,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 12f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 0
        });
        
        EditorUtility.SetDirty(data);
    }
    
    private void AddPreset_ControlPoint()
    {
        ChallengeData data = (ChallengeData)target;
        Undo.RecordObject(data, "Add Control Point Preset");
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Control Terminal",
            category = ChallengeData.SpawnableCategory.Objective,
            minCount = 1,
            maxCount = 1,
            spawnLocation = ChallengeData.SpawnLocationType.AtCenter,
            requireNavMesh = false,
            randomRotation = false,
            priority = 10,
            required = true
        });
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Defenders",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 6,
            maxCount = 8,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 20f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5
        });
        
        data.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Cover Barriers",
            category = ChallengeData.SpawnableCategory.Cover,
            minCount = 6,
            maxCount = 10,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 25f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 1
        });
        
        EditorUtility.SetDirty(data);
    }
    
    private void ClearAllItems()
    {
        if (EditorUtility.DisplayDialog("Clear All Spawn Items", 
            "Are you sure you want to remove all spawn items?", 
            "Yes", "Cancel"))
        {
            ChallengeData data = (ChallengeData)target;
            Undo.RecordObject(data, "Clear Spawn Items");
            data.spawnItems.Clear();
            EditorUtility.SetDirty(data);
        }
    }
}
