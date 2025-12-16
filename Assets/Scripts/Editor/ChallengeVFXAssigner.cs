using UnityEngine;
using UnityEditor;

public class ChallengeVFXAssigner : EditorWindow
{
    private GameObject vfxPrefab;
    private float vfxScale = 2f;
    private float vfxDuration = 0f;
    private bool overwriteExisting = false;
    
    private int foundChallenges = 0;
    private int assignedCount = 0;
    
    [MenuItem("Division Game/Challenges/Assign VFX to Challenges")]
    public static void ShowWindow()
    {
        ChallengeVFXAssigner window = GetWindow<ChallengeVFXAssigner>("Challenge VFX Assigner");
        window.minSize = new Vector2(400, 300);
    }
    
    private void OnEnable()
    {
        // Auto-load FX_Healing_Circle_01 if it exists
        string[] guids = AssetDatabase.FindAssets("FX_Healing_Cirle_01 t:GameObject");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        
        ScanChallenges();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge VFX Bulk Assigner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Assign spawn VFX to all ChallengeData assets at once.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // VFX Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("VFX Settings", EditorStyles.boldLabel);
        
        vfxPrefab = (GameObject)EditorGUILayout.ObjectField("VFX Prefab", vfxPrefab, typeof(GameObject), false);
        vfxScale = EditorGUILayout.Slider("VFX Scale", vfxScale, 0.1f, 10f);
        vfxDuration = EditorGUILayout.FloatField("VFX Duration (0 = never)", vfxDuration);
        
        EditorGUILayout.Space(5);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing VFX", overwriteExisting);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Challenge Info
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Challenge Assets Found", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Challenges: {foundChallenges}");
        
        if (assignedCount > 0)
        {
            EditorGUILayout.LabelField($"Last Assignment: {assignedCount} challenges", EditorStyles.helpBox);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Action Buttons
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = vfxPrefab != null;
        if (GUILayout.Button("Assign VFX to All Challenges", GUILayout.Height(40)))
        {
            AssignVFXToAllChallenges();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Refresh Challenge Count"))
        {
            ScanChallenges();
        }
        
        EditorGUILayout.Space(10);
        
        // Quick Actions
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Clear VFX from All Challenges"))
        {
            if (EditorUtility.DisplayDialog("Clear VFX", 
                "Remove VFX from all challenge assets?", 
                "Clear", "Cancel"))
            {
                ClearVFXFromAllChallenges();
            }
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        if (vfxPrefab == null)
        {
            EditorGUILayout.HelpBox("⚠️ Select a VFX prefab to assign!", MessageType.Warning);
        }
    }
    
    private void ScanChallenges()
    {
        string[] challengeGuids = AssetDatabase.FindAssets("t:ChallengeData");
        foundChallenges = challengeGuids.Length;
        Repaint();
    }
    
    private void AssignVFXToAllChallenges()
    {
        if (vfxPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a VFX prefab first!", "OK");
            return;
        }
        
        string[] challengeGuids = AssetDatabase.FindAssets("t:ChallengeData");
        assignedCount = 0;
        int skippedCount = 0;
        
        foreach (string guid in challengeGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge != null)
            {
                // Skip if already has VFX and overwrite is disabled
                if (!overwriteExisting && challenge.spawnVFX != null)
                {
                    skippedCount++;
                    continue;
                }
                
                challenge.spawnVFX = vfxPrefab;
                challenge.spawnVFXScale = vfxScale;
                challenge.spawnVFXDuration = vfxDuration;
                
                EditorUtility.SetDirty(challenge);
                assignedCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        
        string message = $"✓ Assigned VFX to {assignedCount} challenges!";
        if (skippedCount > 0)
        {
            message += $"\n(Skipped {skippedCount} with existing VFX)";
        }
        
        EditorUtility.DisplayDialog("Success", message, "OK");
        Repaint();
    }
    
    private void ClearVFXFromAllChallenges()
    {
        string[] challengeGuids = AssetDatabase.FindAssets("t:ChallengeData");
        int clearedCount = 0;
        
        foreach (string guid in challengeGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge != null && challenge.spawnVFX != null)
            {
                challenge.spawnVFX = null;
                challenge.spawnVFXScale = 1f;
                challenge.spawnVFXDuration = 0f;
                
                EditorUtility.SetDirty(challenge);
                clearedCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Cleared", $"Cleared VFX from {clearedCount} challenges", "OK");
        assignedCount = 0;
        Repaint();
    }
}
