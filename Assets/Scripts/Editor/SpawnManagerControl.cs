using UnityEngine;
using UnityEditor;

public class SpawnManagerControl : EditorWindow
{
    private CharacterSpawner characterSpawner;
    private bool autoRefresh = true;
    
    [MenuItem("Division Game/Spawn Management/Spawn Manager Control Panel")]
    public static void ShowWindow()
    {
        SpawnManagerControl window = GetWindow<SpawnManagerControl>("Spawn Control");
        window.minSize = new Vector2(400, 300);
    }
    
    private void OnEnable()
    {
        FindSpawners();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Spawn Manager Control Panel", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Control all spawning systems from one place.\n\n" +
            "CharacterSpawner: Ambient civilian spawning\n" +
            "ChallengeSpawner: Mission-based spawning (managed by ChallengeManager)",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        if (characterSpawner == null)
        {
            if (GUILayout.Button("Find Spawners", GUILayout.Height(30)))
            {
                FindSpawners();
            }
            
            if (characterSpawner == null)
            {
                EditorGUILayout.HelpBox("CharacterSpawner not found in scene!", MessageType.Warning);
                return;
            }
        }
        
        DrawCharacterSpawnerControls();
        EditorGUILayout.Space(10);
        
        DrawChallengeSpawnerInfo();
        EditorGUILayout.Space(10);
        
        DrawQuickActions();
    }
    
    private void DrawCharacterSpawnerControls()
    {
        EditorGUILayout.LabelField("CharacterSpawner (Ambient Civilians)", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        SerializedObject so = new SerializedObject(characterSpawner);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Auto-Spawn Enabled:", GUILayout.Width(150));
        
        SerializedProperty enableAutoSpawnProp = so.FindProperty("enableAutoSpawn");
        bool currentValue = enableAutoSpawnProp.boolValue;
        bool newValue = EditorGUILayout.Toggle(currentValue);
        
        if (newValue != currentValue)
        {
            enableAutoSpawnProp.boolValue = newValue;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(characterSpawner);
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (enableAutoSpawnProp.boolValue)
        {
            EditorGUILayout.HelpBox("✅ Ambient civilians are spawning automatically around the player.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("❌ Auto-spawn disabled. Civilians will only spawn through manual calls or challenges.", MessageType.Warning);
        }
        
        EditorGUILayout.Space(5);
        
        SerializedProperty maxActiveProp = so.FindProperty("maxActiveCharacters");
        EditorGUILayout.PropertyField(maxActiveProp, new GUIContent("Max Active Characters"));
        
        SerializedProperty intervalProp = so.FindProperty("spawnInterval");
        EditorGUILayout.PropertyField(intervalProp, new GUIContent("Spawn Interval (seconds)"));
        
        SerializedProperty minDistProp = so.FindProperty("minSpawnDistance");
        EditorGUILayout.PropertyField(minDistProp, new GUIContent("Min Spawn Distance"));
        
        SerializedProperty maxDistProp = so.FindProperty("maxSpawnDistance");
        EditorGUILayout.PropertyField(maxDistProp, new GUIContent("Max Spawn Distance"));
        
        SerializedProperty deactivateDistProp = so.FindProperty("deactivateDistance");
        EditorGUILayout.PropertyField(deactivateDistProp, new GUIContent("Deactivate Distance"));
        
        so.ApplyModifiedProperties();
        
        EditorGUILayout.EndVertical();
        
        if (GUILayout.Button("Select CharacterSpawner in Hierarchy", GUILayout.Height(25)))
        {
            Selection.activeGameObject = characterSpawner.gameObject;
            EditorGUIUtility.PingObject(characterSpawner.gameObject);
        }
    }
    
    private void DrawChallengeSpawnerInfo()
    {
        EditorGUILayout.LabelField("ChallengeSpawner (Mission Content)", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.HelpBox(
            "✅ ChallengeSpawner is managed by ChallengeManager.\n\n" +
            "Mission Zones spawn content when challenges activate and cleanup when challenges complete.\n\n" +
            "Use Mission Zone State Manager to control zones.",
            MessageType.Info
        );
        
        EditorGUILayout.EndVertical();
        
        if (GUILayout.Button("Open Mission Zone State Manager", GUILayout.Height(25)))
        {
            MissionZoneStateManager.ShowWindow();
        }
    }
    
    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Enable Ambient Civilians", GUILayout.Height(30)))
        {
            SerializedObject so = new SerializedObject(characterSpawner);
            so.FindProperty("enableAutoSpawn").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(characterSpawner);
            Debug.Log("CharacterSpawner: Ambient civilian spawning ENABLED");
        }
        
        if (GUILayout.Button("Disable Ambient Civilians", GUILayout.Height(30)))
        {
            SerializedObject so = new SerializedObject(characterSpawner);
            so.FindProperty("enableAutoSpawn").boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(characterSpawner);
            Debug.Log("CharacterSpawner: Ambient civilian spawning DISABLED");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Preset Configurations:", EditorStyles.miniLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Busy City (High Density)"))
        {
            ApplyPreset(20, 3f);
        }
        
        if (GUILayout.Button("Moderate Population"))
        {
            ApplyPreset(10, 5f);
        }
        
        if (GUILayout.Button("Sparse (Low Density)"))
        {
            ApplyPreset(5, 10f);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void ApplyPreset(int maxActive, float interval)
    {
        SerializedObject so = new SerializedObject(characterSpawner);
        so.FindProperty("maxActiveCharacters").intValue = maxActive;
        so.FindProperty("spawnInterval").floatValue = interval;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(characterSpawner);
        Debug.Log($"Applied preset: Max={maxActive}, Interval={interval}s");
    }
    
    private void FindSpawners()
    {
        characterSpawner = FindFirstObjectByType<CharacterSpawner>();
        
        if (characterSpawner != null)
        {
            Debug.Log("Found CharacterSpawner in scene");
        }
        else
        {
            Debug.LogWarning("CharacterSpawner not found in scene!");
        }
        
        Repaint();
    }
}
