using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(CharacterSpawner))]
public class CharacterSpawnerEditor : Editor
{
    private bool showDiagnostics = true;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CharacterSpawner spawner = (CharacterSpawner)target;
        SerializedObject so = new SerializedObject(spawner);
        
        EditorGUILayout.Space(10);
        
        DrawDiagnostics(spawner, so);
        
        EditorGUILayout.Space(10);
        DrawQuickFixes(spawner, so);
        
        EditorGUILayout.Space(10);
        DrawRuntimeControls(spawner);
    }
    
    private void DrawDiagnostics(CharacterSpawner spawner, SerializedObject so)
    {
        showDiagnostics = EditorGUILayout.BeginFoldoutHeaderGroup(showDiagnostics, "Diagnostics");
        
        if (showDiagnostics)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty civilianPrefabsProp = so.FindProperty("civilianPrefabs");
            int prefabCount = civilianPrefabsProp.arraySize;
            
            if (prefabCount == 0)
            {
                EditorGUILayout.HelpBox("⚠️ NO CIVILIAN PREFABS ASSIGNED! Spawner will not work.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField($"✓ {prefabCount} civilian prefab(s) assigned", EditorStyles.boldLabel);
            }
            
            int maxActive = so.FindProperty("maxActiveCharacters").intValue;
            int poolSize = so.FindProperty("initialPoolSize").intValue;
            
            if (poolSize < maxActive)
            {
                EditorGUILayout.HelpBox($"⚠️ Initial Pool Size ({poolSize}) is less than Max Active Characters ({maxActive}). This may cause spawning to create new instances.", MessageType.Warning);
            }
            
            if (poolSize < 10)
            {
                EditorGUILayout.HelpBox($"⚠️ Initial Pool Size ({poolSize}) is very low. Recommended: 20-30 for smooth spawning.", MessageType.Warning);
            }
            
            bool autoSpawn = so.FindProperty("enableAutoSpawn").boolValue;
            if (!autoSpawn)
            {
                EditorGUILayout.HelpBox("⚠️ Auto Spawn is DISABLED. Characters won't spawn automatically.", MessageType.Warning);
            }
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                EditorGUILayout.HelpBox("⚠️ No GameObject with 'Player' tag found! Spawner needs a player reference.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("✓ Player found", EditorStyles.label);
            }
            
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Runtime Status:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Active Characters: {spawner.GetActiveCharacterCount()} / {maxActive}");
                EditorGUILayout.LabelField($"Available in Pool: {spawner.GetPooledCharacterCount()}");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    
    private void DrawQuickFixes(CharacterSpawner spawner, SerializedObject so)
    {
        EditorGUILayout.LabelField("Quick Fixes", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        SerializedProperty civilianPrefabsProp = so.FindProperty("civilianPrefabs");
        
        if (civilianPrefabsProp.arraySize == 0)
        {
            if (GUILayout.Button("Auto-Find Civilian Prefabs"))
            {
                AutoFindCivilianPrefabs(so, civilianPrefabsProp);
            }
        }
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Set Recommended Settings"))
        {
            Undo.RecordObject(spawner, "Set Recommended Settings");
            so.FindProperty("maxActiveCharacters").intValue = 20;
            so.FindProperty("initialPoolSize").intValue = 30;
            so.FindProperty("spawnInterval").floatValue = 2f;
            so.FindProperty("minSpawnDistance").floatValue = 30f;
            so.FindProperty("maxSpawnDistance").floatValue = 100f;
            so.FindProperty("deactivateDistance").floatValue = 120f;
            so.FindProperty("enableAutoSpawn").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
            Debug.Log("✓ Applied recommended spawner settings");
        }
        
        if (GUILayout.Button("Enable Debug Logging"))
        {
            Undo.RecordObject(spawner, "Enable Debug Logging");
            so.FindProperty("logSpawnEvents").boolValue = true;
            so.FindProperty("showDebugGizmos").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
            Debug.Log("✓ Enabled debug logging and gizmos");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawRuntimeControls(CharacterSpawner spawner)
    {
        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Spawn Random"))
            {
                GameObject spawned = spawner.SpawnRandomCharacter();
                if (spawned != null)
                {
                    Debug.Log($"✓ Spawned: {spawned.name}");
                }
                else
                {
                    Debug.LogWarning("Failed to spawn character - check console for errors");
                }
            }
            
            if (GUILayout.Button("Despawn All"))
            {
                spawner.DespawnAllCharacters();
                Debug.Log("✓ Despawned all characters");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Force Spawn 5 Characters"))
            {
                for (int i = 0; i < 5; i++)
                {
                    spawner.SpawnRandomCharacter();
                }
                Debug.Log("✓ Attempted to spawn 5 characters");
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Runtime controls available in Play Mode", MessageType.Info);
        }
    }
    
    private void AutoFindCivilianPrefabs(SerializedObject so, SerializedProperty civilianPrefabsProp)
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab SM_Chr", new[] { "Assets" });
        
        if (prefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Prefabs Found", "Could not find any character prefabs starting with 'SM_Chr'.", "OK");
            return;
        }
        
        Undo.RecordObject(so.targetObject, "Auto-Find Civilian Prefabs");
        
        civilianPrefabsProp.ClearArray();
        
        int addedCount = 0;
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && !path.Contains("Zombie") && !path.Contains("Enemy") && !path.Contains("Soldier"))
            {
                civilianPrefabsProp.InsertArrayElementAtIndex(civilianPrefabsProp.arraySize);
                civilianPrefabsProp.GetArrayElementAtIndex(civilianPrefabsProp.arraySize - 1).objectReferenceValue = prefab;
                addedCount++;
            }
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(so.targetObject);
        
        Debug.Log($"✓ Auto-found and assigned {addedCount} civilian prefabs");
    }
}
