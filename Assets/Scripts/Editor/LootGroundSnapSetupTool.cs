using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LootGroundSnapSetupTool : EditorWindow
{
    private bool autoApplyToNewLoot = true;
    private bool enableGroundSnap = true;
    private float snapDelay = 1f;
    private float maxGroundDistance = 10f;
    private float groundOffset = 0.1f;
    private bool freezeWhenSettled = true;
    private bool alignToGroundNormal = false;
    private bool showDebugLogs = false;
    
    private List<GameObject> selectedLootPrefabs = new List<GameObject>();
    
    [MenuItem("Division Game/Setup/Loot Ground Snap Setup")]
    public static void ShowWindow()
    {
        GetWindow<LootGroundSnapSetupTool>("Loot Ground Snap Setup");
    }
    
    private void OnEnable()
    {
        RefreshSelection();
    }
    
    private void OnSelectionChange()
    {
        RefreshSelection();
        Repaint();
    }
    
    private void RefreshSelection()
    {
        selectedLootPrefabs.Clear();
        
        foreach (Object obj in Selection.objects)
        {
            if (obj is GameObject go)
            {
                string path = AssetDatabase.GetAssetPath(go);
                if (path.Contains("Prefab") || path.EndsWith(".prefab"))
                {
                    selectedLootPrefabs.Add(go);
                }
            }
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Loot Ground Snap Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "This tool adds LootGroundSnap component to loot prefabs.\n\n" +
            "• Prevents loot from floating\n" +
            "• Snaps to exact ground position\n" +
            "• Freezes physics when settled\n" +
            "• Improves performance",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField($"Selected Loot Prefabs: {selectedLootPrefabs.Count}", EditorStyles.boldLabel);
        
        if (selectedLootPrefabs.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (GameObject prefab in selectedLootPrefabs)
            {
                EditorGUILayout.LabelField($"• {prefab.name}");
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No loot prefabs selected. Select loot prefabs in the Project window.", MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Ground Snap Settings", EditorStyles.boldLabel);
        enableGroundSnap = EditorGUILayout.Toggle("Enable Ground Snap", enableGroundSnap);
        snapDelay = EditorGUILayout.FloatField("Snap Delay (sec)", snapDelay);
        maxGroundDistance = EditorGUILayout.FloatField("Max Ground Distance", maxGroundDistance);
        groundOffset = EditorGUILayout.FloatField("Ground Offset", groundOffset);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Physics Settings", EditorStyles.boldLabel);
        freezeWhenSettled = EditorGUILayout.Toggle("Freeze When Settled", freezeWhenSettled);
        alignToGroundNormal = EditorGUILayout.Toggle("Align To Ground", alignToGroundNormal);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        showDebugLogs = EditorGUILayout.Toggle("Show Debug Logs", showDebugLogs);
        
        EditorGUILayout.Space(15);
        
        GUI.enabled = selectedLootPrefabs.Count > 0;
        
        if (GUILayout.Button("Add Ground Snap to Selected Prefabs", GUILayout.Height(40)))
        {
            ApplyToSelected();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Find All Loot Prefabs in Project"))
        {
            FindAllLootPrefabs();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Recommended Settings:\n" +
            "• Snap Delay: 1-2 sec (wait for physics)\n" +
            "• Freeze When Settled: ON (saves CPU)\n" +
            "• Align To Ground: OFF (keeps loot upright)\n" +
            "• Show Debug Logs: ON (for testing)",
            MessageType.Info);
    }
    
    private void ApplyToSelected()
    {
        int addedCount = 0;
        int updatedCount = 0;
        
        foreach (GameObject prefab in selectedLootPrefabs)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            
            LootGroundSnap groundSnap = instance.GetComponent<LootGroundSnap>();
            
            if (groundSnap == null)
            {
                groundSnap = instance.AddComponent<LootGroundSnap>();
                addedCount++;
            }
            else
            {
                updatedCount++;
            }
            
            groundSnap.enableGroundSnap = enableGroundSnap;
            groundSnap.snapDelay = snapDelay;
            groundSnap.maxGroundDistance = maxGroundDistance;
            groundSnap.groundOffset = groundOffset;
            groundSnap.freezeWhenSettled = freezeWhenSettled;
            groundSnap.alignToGroundNormal = alignToGroundNormal;
            groundSnap.showDebugLogs = showDebugLogs;
            
            Rigidbody rb = instance.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = instance.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.linearDamping = 2f;
                rb.angularDamping = 1f;
                rb.useGravity = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = $"Applied LootGroundSnap:\n• Added to {addedCount} prefab(s)\n• Updated {updatedCount} existing component(s)";
        Debug.Log($"<color=green>{message}</color>");
        EditorUtility.DisplayDialog("Success", message, "OK");
    }
    
    private void FindAllLootPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        List<GameObject> lootPrefabs = new List<GameObject>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                bool isLoot = prefab.name.ToLower().Contains("loot") ||
                              prefab.name.ToLower().Contains("pickup") ||
                              prefab.GetComponent<LootItem>() != null ||
                              path.ToLower().Contains("loot");
                
                if (isLoot)
                {
                    lootPrefabs.Add(prefab);
                }
            }
        }
        
        Selection.objects = lootPrefabs.ToArray();
        RefreshSelection();
        
        Debug.Log($"<color=cyan>Found {lootPrefabs.Count} loot prefabs</color>");
    }
}
