using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixCharacterSpawnerTool : EditorWindow
{
    private CharacterSpawner spawner;
    private int targetPrefabCount = 10;
    private int targetPoolSize = 10;
    private int targetMaxActive = 8;
    
    [MenuItem("Division Game/Spawn Management/Fix Civilian Spawner")]
    public static void ShowWindow()
    {
        FixCharacterSpawnerTool window = GetWindow<FixCharacterSpawnerTool>("Fix Spawner");
        window.minSize = new Vector2(450, 400);
    }
    
    private void OnEnable()
    {
        spawner = FindFirstObjectByType<CharacterSpawner>();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Fix Character Spawner - Reduce Pool Size", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (spawner == null)
        {
            EditorGUILayout.HelpBox("CharacterSpawner not found in scene!", MessageType.Error);
            if (GUILayout.Button("Search Again"))
            {
                spawner = FindFirstObjectByType<CharacterSpawner>();
            }
            return;
        }
        
        EditorGUILayout.HelpBox(
            "PROBLEM: CharacterSpawner is creating too many pooled instances because you have " +
            "too many civilian prefabs assigned.\n\n" +
            "SOLUTION: Reduce the number of prefabs to match your actual needs.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        DrawCurrentStatus();
        EditorGUILayout.Space(10);
        
        DrawFixOptions();
    }
    
    private void DrawCurrentStatus()
    {
        EditorGUILayout.LabelField("Current Status:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty civilianPrefabsProp = so.FindProperty("civilianPrefabs");
        
        int prefabCount = civilianPrefabsProp.arraySize;
        int poolSize = so.FindProperty("initialPoolSize").intValue;
        int maxActive = so.FindProperty("maxActiveCharacters").intValue;
        
        int poolSizePerPrefab = prefabCount > 0 ? Mathf.CeilToInt((float)poolSize / prefabCount) : 0;
        int totalPooled = poolSizePerPrefab * prefabCount;
        int initialSpawn = Mathf.Min(maxActive / 2, poolSize);
        
        EditorGUILayout.LabelField($"Civilian Prefabs Assigned: {prefabCount}");
        EditorGUILayout.LabelField($"Initial Pool Size: {poolSize}");
        EditorGUILayout.LabelField($"Max Active Characters: {maxActive}");
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Calculated Results:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"  Pool Size Per Prefab: {poolSizePerPrefab}");
        EditorGUILayout.LabelField($"  Total Pooled Instances: {totalPooled}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Initial Active Spawn: {initialSpawn}");
        
        EditorGUILayout.Space(5);
        
        if (totalPooled > 15)
        {
            EditorGUILayout.HelpBox($"⚠️ Creating {totalPooled} pooled instances is excessive!", MessageType.Warning);
        }
        else if (totalPooled > 10)
        {
            EditorGUILayout.HelpBox($"Creating {totalPooled} pooled instances is acceptable but could be optimized.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"✅ Pool size of {totalPooled} is well optimized!", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawFixOptions()
    {
        EditorGUILayout.LabelField("Quick Fix Options:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.HelpBox(
            "These presets will automatically configure the CharacterSpawner for optimal performance.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("RECOMMENDED: 10 Diverse Civilians (10 pooled)", GUILayout.Height(40)))
        {
            ApplyFix(10, 10, 8);
        }
        
        EditorGUILayout.Space(3);
        
        if (GUILayout.Button("Moderate: 10 Civilians + Variety (20 pooled)", GUILayout.Height(30)))
        {
            ApplyFix(10, 20, 10);
        }
        
        EditorGUILayout.Space(3);
        
        if (GUILayout.Button("Minimal: 5 Civilians (5 pooled)", GUILayout.Height(30)))
        {
            ApplyFix(5, 5, 5);
        }
        
        EditorGUILayout.Space(3);
        
        if (GUILayout.Button("Busy City: 15 Civilians (30 pooled)", GUILayout.Height(30)))
        {
            ApplyFix(15, 30, 15);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        DrawCustomFix();
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Select CharacterSpawner in Hierarchy", GUILayout.Height(25)))
        {
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);
        }
    }
    
    private void DrawCustomFix()
    {
        EditorGUILayout.LabelField("Custom Configuration:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        targetPrefabCount = EditorGUILayout.IntSlider("Target Prefab Count:", targetPrefabCount, 3, 25);
        targetPoolSize = EditorGUILayout.IntSlider("Initial Pool Size:", targetPoolSize, 5, 50);
        targetMaxActive = EditorGUILayout.IntSlider("Max Active:", targetMaxActive, 3, 20);
        
        int poolPerPrefab = targetPrefabCount > 0 ? Mathf.CeilToInt((float)targetPoolSize / targetPrefabCount) : 0;
        int totalPool = poolPerPrefab * targetPrefabCount;
        
        EditorGUILayout.LabelField($"Result: {totalPool} total pooled instances");
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Apply Custom Configuration", GUILayout.Height(30)))
        {
            ApplyFix(targetPrefabCount, targetPoolSize, targetMaxActive);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void ApplyFix(int prefabCount, int poolSize, int maxActive)
    {
        if (spawner == null)
        {
            EditorUtility.DisplayDialog("Error", "CharacterSpawner not found!", "OK");
            return;
        }
        
        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty civilianPrefabsProp = so.FindProperty("civilianPrefabs");
        
        int currentCount = civilianPrefabsProp.arraySize;
        
        if (currentCount < prefabCount)
        {
            EditorUtility.DisplayDialog(
                "Not Enough Prefabs",
                $"You currently have {currentCount} prefabs but want {prefabCount}.\n\n" +
                "The tool will keep all {currentCount} prefabs and adjust pool settings.",
                "OK"
            );
            prefabCount = currentCount;
        }
        
        Undo.RecordObject(spawner, "Fix Character Spawner");
        
        if (currentCount > prefabCount)
        {
            List<GameObject> keptPrefabs = new List<GameObject>();
            
            for (int i = 0; i < prefabCount && i < currentCount; i++)
            {
                GameObject prefab = civilianPrefabsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (prefab != null)
                {
                    keptPrefabs.Add(prefab);
                }
            }
            
            civilianPrefabsProp.ClearArray();
            civilianPrefabsProp.arraySize = keptPrefabs.Count;
            
            for (int i = 0; i < keptPrefabs.Count; i++)
            {
                civilianPrefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = keptPrefabs[i];
            }
        }
        
        so.FindProperty("initialPoolSize").intValue = poolSize;
        so.FindProperty("maxActiveCharacters").intValue = maxActive;
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
        
        int finalPrefabCount = civilianPrefabsProp.arraySize;
        int poolPerPrefab = Mathf.CeilToInt((float)poolSize / finalPrefabCount);
        int totalPooled = poolPerPrefab * finalPrefabCount;
        
        EditorUtility.DisplayDialog(
            "Character Spawner Fixed!",
            $"Configuration applied:\n\n" +
            $"Civilian Prefabs: {finalPrefabCount}\n" +
            $"Pool Size: {poolSize}\n" +
            $"Max Active: {maxActive}\n\n" +
            $"Result:\n" +
            $"Total Pooled Instances: {totalPooled}\n" +
            $"Pool Per Prefab: {poolPerPrefab}\n" +
            $"Initial Spawn: ~{Mathf.Min(maxActive / 2, poolSize)}",
            "OK"
        );
        
        Debug.Log($"CharacterSpawner fixed: {finalPrefabCount} prefabs, {totalPooled} total pool size");
        
        Repaint();
    }
}
