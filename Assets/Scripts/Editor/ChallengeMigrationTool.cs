using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChallengeMigrationTool : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ChallengeData> challenges = new List<ChallengeData>();
    
    [MenuItem("Division Game/Challenge System/Migration Tool")]
    public static void ShowWindow()
    {
        ChallengeMigrationTool window = GetWindow<ChallengeMigrationTool>("Challenge Migration");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }
    
    private void OnEnable()
    {
        FindAllChallenges();
    }
    
    private void FindAllChallenges()
    {
        challenges.Clear();
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            if (challenge != null)
            {
                challenges.Add(challenge);
            }
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Data Migration Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool helps you understand your existing ChallengeData assets.\n\n" +
            "The old fields (enemyCount, civilianCount, etc.) are automatically read by the new GetEnemyCount() and GetCivilianCount() methods from the spawnItems list.\n\n" +
            "Old assets will continue to work, but you can migrate to the new Spawn Items system for more flexibility.",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Refresh Challenge List", GUILayout.Height(30)))
        {
            FindAllChallenges();
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Found {challenges.Count} ChallengeData Assets", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (ChallengeData challenge in challenges)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(challenge.challengeName, EditorStyles.boldLabel, GUILayout.Width(250));
            
            if (GUILayout.Button("Select", GUILayout.Width(80)))
            {
                Selection.activeObject = challenge;
                EditorGUIUtility.PingObject(challenge);
            }
            
            EditorGUILayout.EndHorizontal();
            
            int spawnItemCount = challenge.spawnItems != null ? challenge.spawnItems.Count : 0;
            int enemyCount = challenge.GetEnemyCount();
            int civilianCount = challenge.GetCivilianCount();
            
            EditorGUILayout.LabelField($"  Type: {challenge.challengeType}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  Difficulty: {challenge.difficulty}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  Spawn Items Configured: {spawnItemCount}", EditorStyles.miniLabel);
            
            if (spawnItemCount > 0)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField($"  ✓ Using New System: {enemyCount} enemies, {civilianCount} civilians", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField($"  ⚠ No Spawn Items - Click 'Select' to configure", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "How to use the new system:\n" +
            "1. Select a ChallengeData asset\n" +
            "2. Scroll to 'Flexible Spawning System'\n" +
            "3. Click 'Add Common Spawn Presets' for quick setup\n" +
            "4. Or manually add Spawn Items using the + button",
            MessageType.Info);
    }
}
