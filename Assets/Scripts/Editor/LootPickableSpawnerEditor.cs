using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LootPickableSpawner))]
public class LootPickableSpawnerEditor : Editor
{
    private SerializedProperty maxTotalSpawns;
    private SerializedProperty spawnInterval;
    private SerializedProperty enableAutoSpawn;
    private SerializedProperty minSpawnDistance;
    private SerializedProperty maxSpawnDistance;
    private SerializedProperty maxSpawnAttempts;
    private SerializedProperty navMeshSampleDistance;
    private SerializedProperty showDebugGizmos;
    private SerializedProperty logSpawnEvents;
    
    private void OnEnable()
    {
        maxTotalSpawns = serializedObject.FindProperty("maxTotalSpawns");
        spawnInterval = serializedObject.FindProperty("spawnInterval");
        enableAutoSpawn = serializedObject.FindProperty("enableAutoSpawn");
        minSpawnDistance = serializedObject.FindProperty("minSpawnDistance");
        maxSpawnDistance = serializedObject.FindProperty("maxSpawnDistance");
        maxSpawnAttempts = serializedObject.FindProperty("maxSpawnAttempts");
        navMeshSampleDistance = serializedObject.FindProperty("navMeshSampleDistance");
        showDebugGizmos = serializedObject.FindProperty("showDebugGizmos");
        logSpawnEvents = serializedObject.FindProperty("logSpawnEvents");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LootPickableSpawner spawner = (LootPickableSpawner)target;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Loot Pickable Spawner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Spawns loot items around the player at intervals.\n" +
            "Loot rarity is controlled by LootManager (check LootManager for rarity chances).\n" +
            "Loot despawns naturally when picked up by the player.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(maxTotalSpawns, new GUIContent("Max Total Spawns", "Total loot items to spawn before stopping"));
        EditorGUILayout.PropertyField(spawnInterval, new GUIContent("Spawn Interval (s)", "How often to spawn new loot"));
        EditorGUILayout.PropertyField(enableAutoSpawn, new GUIContent("Auto Spawn Enabled", "Automatically spawn loot over time"));
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Distance Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(minSpawnDistance, new GUIContent("Min Spawn Distance", "Minimum distance from player"));
        EditorGUILayout.PropertyField(maxSpawnDistance, new GUIContent("Max Spawn Distance", "Maximum distance from player"));
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Rarity Settings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Rarity chances are controlled by LootManager.\n" +
            "Select /GameSystems/LootManager to adjust rarity percentages.",
            MessageType.Info
        );
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("NavMesh Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(maxSpawnAttempts);
        EditorGUILayout.PropertyField(navMeshSampleDistance);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showDebugGizmos);
        EditorGUILayout.PropertyField(logSpawnEvents);
        
        EditorGUILayout.Space(10);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Spawned: {spawner.GetTotalSpawnedCount()} / {maxTotalSpawns.intValue}");
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Spawn Loot Now"))
            {
                spawner.SpawnRandomLoot();
            }
            
            if (GUILayout.Button("Reset Count"))
            {
                spawner.ResetSpawnCount();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see runtime controls", MessageType.Info);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
