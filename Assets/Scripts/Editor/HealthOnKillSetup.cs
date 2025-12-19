using UnityEngine;
using UnityEditor;

public class HealthOnKillSetup : EditorWindow
{
    [MenuItem("Division Game/Setup Health on Kill")]
    public static void ShowWindow()
    {
        GetWindow<HealthOnKillSetup>("Health on Kill Setup").Show();
    }

    private float healthPercentage = 0.1f;
    private float staminaPercentage = 0.1f;
    private bool applyToAllEnemies = true;

    private void OnGUI()
    {
        GUILayout.Label("Health/Stamina on Kill Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This adds the EnemyKillRewardHandler to enemy prefabs.\n\n" +
            "On kill, player will restore:\n" +
            "• Health based on percentage of max HP\n" +
            "• Stamina based on percentage of max stamina",
            MessageType.Info
        );

        EditorGUILayout.Space();

        healthPercentage = EditorGUILayout.Slider(
            "Health Restore %", 
            healthPercentage, 
            0f, 
            1f
        );
        EditorGUILayout.LabelField($"  = {healthPercentage * 100f:F0}% of max health");

        EditorGUILayout.Space();

        staminaPercentage = EditorGUILayout.Slider(
            "Stamina Restore %", 
            staminaPercentage, 
            0f, 
            1f
        );
        EditorGUILayout.LabelField($"  = {staminaPercentage * 100f:F0}% of max stamina");

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
            $"Health on Kill setup complete!\n\n" +
            $"Processed: {processedCount} prefabs\n" +
            $"Updated: {updatedCount} prefabs\n\n" +
            $"Players will now restore {healthPercentage * 100f:F0}% health and {staminaPercentage * 100f:F0}% stamina on kill!",
            "OK"
        );
    }

    private bool ApplyToGameObject(GameObject obj)
    {
        EnemyKillRewardHandler rewardHandler = obj.GetComponent<EnemyKillRewardHandler>();
        
        if (rewardHandler == null)
        {
            rewardHandler = obj.AddComponent<EnemyKillRewardHandler>();
            Debug.Log($"Added EnemyKillRewardHandler to {obj.name}");
        }

        SerializedObject so = new SerializedObject(rewardHandler);
        
        so.FindProperty("restoreHealthOnKill").boolValue = true;
        so.FindProperty("healthRestoreAmount").floatValue = 0f;
        so.FindProperty("healthRestorePercentage").floatValue = healthPercentage;
        
        so.FindProperty("restoreStaminaOnKill").boolValue = true;
        so.FindProperty("staminaRestoreAmount").floatValue = 0f;
        so.FindProperty("staminaRestorePercentage").floatValue = staminaPercentage;
        
        so.ApplyModifiedProperties();
        
        return true;
    }

    [MenuItem("Division Game/Quick Setup 10% Health on Kill")]
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
                EnemyKillRewardHandler rewardHandler = prefab.GetComponent<EnemyKillRewardHandler>();
                
                if (rewardHandler == null)
                {
                    rewardHandler = prefab.AddComponent<EnemyKillRewardHandler>();
                }

                SerializedObject so = new SerializedObject(rewardHandler);
                
                so.FindProperty("restoreHealthOnKill").boolValue = true;
                so.FindProperty("healthRestoreAmount").floatValue = 0f;
                so.FindProperty("healthRestorePercentage").floatValue = 0.1f;
                
                so.FindProperty("restoreStaminaOnKill").boolValue = true;
                so.FindProperty("staminaRestoreAmount").floatValue = 0f;
                so.FindProperty("staminaRestorePercentage").floatValue = 0.1f;
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
                
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✅ Applied 10% health/stamina restore to {updatedCount} enemy prefabs!</color>");
        
        EditorUtility.DisplayDialog(
            "Quick Setup Complete!",
            $"Applied 10% health & stamina restore on kill to {updatedCount} enemy prefabs!\n\n" +
            "Test by killing enemies in Play Mode.",
            "OK"
        );
    }
}
