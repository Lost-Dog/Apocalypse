using UnityEngine;
using UnityEditor;

public class HealthOnKillSetup : EditorWindow
{
    [MenuItem("Division Game/Setup Temperature Restore On Kill")]
    public static void ShowWindow()
    {
        GetWindow<HealthOnKillSetup>("Temperature On Kill Setup").Show();
    }

    private float temperaturePercentage = 0.1f;
    private bool applyToAllEnemies = true;

    private void OnGUI()
    {
        GUILayout.Label("Temperature On Kill Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This adds the TemperatureRestoreOnKill component to enemy prefabs.\n\n" +
            "On kill, player will restore a percentage of max temperature.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        temperaturePercentage = EditorGUILayout.Slider(
            "Temperature Restore %", 
            temperaturePercentage, 
            0f, 
            1f
        );
        EditorGUILayout.LabelField($"  = {temperaturePercentage * 100f:F0}% of max temperature");

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        applyToAllEnemies = EditorGUILayout.Toggle("Apply to All Enemy Prefabs", applyToAllEnemies);

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply to All Enemy Prefabs", GUILayout.Height(40)))
        {
            ApplyToAllEnemyPrefabs();
        }

        if (Selection.activeGameObject != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Selected: {Selection.activeGameObject.name}", MessageType.None);
            
            if (GUILayout.Button("Apply to Selected Prefab/GameObject", GUILayout.Height(30)))
            {
                ApplyToGameObject(Selection.activeGameObject);
                EditorUtility.SetDirty(Selection.activeGameObject);
                
                if (PrefabUtility.IsPartOfPrefabAsset(Selection.activeGameObject))
                {
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

    private void ApplyToAllEnemyPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Character_Prefabs/Enemies" });
        
        int processedCount = 0;
        int updatedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                processedCount++;
                
                if (ApplyToGameObject(prefab))
                {
                    updatedCount++;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✅ Processed {processedCount} enemy prefabs, updated {updatedCount}</color>");
        
        EditorUtility.DisplayDialog(
            "Success!",
            $"Temperature on Kill setup complete!\n\n" +
            $"Processed: {processedCount} prefabs\n" +
            $"Updated: {updatedCount} prefabs\n\n" +
            $"Players will now restore {temperaturePercentage * 100f:F0}% temperature on kill!",
            "OK"
        );
    }

    private bool ApplyToGameObject(GameObject obj)
    {
        TemperatureRestoreOnKill rewardHandler = obj.GetComponent<TemperatureRestoreOnKill>();
        
        if (rewardHandler == null)
        {
            rewardHandler = obj.AddComponent<TemperatureRestoreOnKill>();
            Debug.Log($"Added TemperatureRestoreOnKill to {obj.name}");
        }

        SerializedObject so = new SerializedObject(rewardHandler);

        so.FindProperty("skillActive").boolValue = true;
        so.FindProperty("activateOnStart").boolValue = true;
        so.FindProperty("temperatureRestorePercentage").floatValue = temperaturePercentage;
        
        so.ApplyModifiedProperties();
        
        return true;
    }

    [MenuItem("Division Game/Quick Setup 10% Temperature On Kill")]
    public static void QuickSetup10Percent()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Character_Prefabs/Enemies" });
        
        int updatedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                TemperatureRestoreOnKill rewardHandler = prefab.GetComponent<TemperatureRestoreOnKill>();
                
                if (rewardHandler == null)
                {
                    rewardHandler = prefab.AddComponent<TemperatureRestoreOnKill>();
                }

                SerializedObject so = new SerializedObject(rewardHandler);

                so.FindProperty("skillActive").boolValue = true;
                so.FindProperty("activateOnStart").boolValue = true;
                so.FindProperty("temperatureRestorePercentage").floatValue = 0.1f;
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
                
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✅ Applied 10% health/stamina restore to {updatedCount} enemy prefabs!</color>");
        
        EditorUtility.DisplayDialog(
            "Quick Setup Complete!",
            $"Applied 10% temperature restore on kill to {updatedCount} enemy prefabs!\n\n" +
            "Test by killing enemies in Play Mode.",
            "OK"
        );
    }
}
