using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gameManager = (GameManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Division Game - Core Manager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This is the central manager that coordinates all game systems. Make sure all manager references are assigned.", MessageType.Info);
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Wire All Managers", GUILayout.Height(30)))
        {
            AutoWireManagers(gameManager);
        }
        
        if (GUILayout.Button("Test Initialize All Systems"))
        {
            if (Application.isPlaying)
            {
                if (gameManager.factionManager != null) gameManager.factionManager.Initialize();
                if (gameManager.missionManager != null) gameManager.missionManager.Initialize();
                if (gameManager.challengeManager != null) gameManager.challengeManager.Initialize();
                if (gameManager.skillManager != null) gameManager.skillManager.Initialize();
                
                Debug.Log("Initialized all managers!");
            }
            else
            {
                EditorUtility.DisplayDialog("Play Mode Required", "This button only works in Play Mode!", "OK");
            }
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        
        int missingCount = 0;
        if (gameManager.missionManager == null) { EditorGUILayout.HelpBox("Missing: MissionManager", MessageType.Warning); missingCount++; }
        if (gameManager.factionManager == null) { EditorGUILayout.HelpBox("Missing: FactionManager", MessageType.Warning); missingCount++; }
        if (gameManager.progressionManager == null) { EditorGUILayout.HelpBox("Missing: ProgressionManager", MessageType.Warning); missingCount++; }
        if (gameManager.lootManager == null) { EditorGUILayout.HelpBox("Missing: LootManager", MessageType.Warning); missingCount++; }
        if (gameManager.challengeManager == null) { EditorGUILayout.HelpBox("Missing: ChallengeManager", MessageType.Warning); missingCount++; }
        if (gameManager.skillManager == null) { EditorGUILayout.HelpBox("Missing: SkillManager", MessageType.Warning); missingCount++; }
        
        if (missingCount == 0)
        {
            EditorGUILayout.HelpBox("✓ All managers are assigned!", MessageType.Info);
        }
    }
    
    private void AutoWireManagers(GameManager gameManager)
    {
        GameObject gameSystemsObj = gameManager.gameObject;
        
        if (gameManager.missionManager == null)
        {
            gameManager.missionManager = gameSystemsObj.GetComponent<MissionManager>();
        }
        if (gameManager.factionManager == null)
        {
            gameManager.factionManager = gameSystemsObj.GetComponent<FactionManager>();
        }
        if (gameManager.progressionManager == null)
        {
            gameManager.progressionManager = gameSystemsObj.GetComponent<ProgressionManager>();
        }
        if (gameManager.lootManager == null)
        {
            gameManager.lootManager = gameSystemsObj.GetComponent<LootManager>();
        }
        if (gameManager.challengeManager == null)
        {
            gameManager.challengeManager = gameSystemsObj.GetComponent<ChallengeManager>();
        }
        if (gameManager.skillManager == null)
        {
            gameManager.skillManager = gameSystemsObj.GetComponent<SkillManager>();
        }
        
        EditorUtility.SetDirty(gameManager);
        Debug.Log("Auto-wired all available managers!");
    }
}
