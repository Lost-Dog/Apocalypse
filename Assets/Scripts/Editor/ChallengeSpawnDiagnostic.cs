using UnityEngine;
using UnityEditor;
using System.Linq;

public class ChallengeSpawnDiagnostic : EditorWindow
{
    private Vector2 scrollPos;
    
    [MenuItem("Division Game/Challenge System/Diagnose Spawn Issues")]
    public static void ShowWindow()
    {
        ChallengeSpawnDiagnostic window = GetWindow<ChallengeSpawnDiagnostic>("Spawn Diagnostic");
        window.minSize = new Vector2(600, 500);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Spawn Diagnostic Tool", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool diagnoses and fixes common spawn issues:\n\n" +
            "• Missing/broken prefab references\n" +
            "• Invalid spawn radii\n" +
            "• NavMesh configuration issues\n" +
            "• Spawn location problems",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(40)))
        {
            RunFullDiagnostic();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Quick Fix: Set Default Spawn Settings", GUILayout.Height(35)))
        {
            QuickFixSpawnSettings();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Fix All Broken Prefabs", GUILayout.Height(35)))
        {
            FixBrokenPrefabs();
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Individual Challenge Fixes", EditorStyles.boldLabel);
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge == null || challenge.spawnItems == null || challenge.spawnItems.Count == 0)
                continue;
            
            EditorGUILayout.BeginHorizontal();
            
            // Check for issues
            int brokenPrefabs = challenge.spawnItems.Count(item => item.prefab == null);
            int zeroRadius = challenge.spawnItems.Count(item => 
                item.spawnLocation != ChallengeData.SpawnLocationType.AtCenter && item.spawnRadius <= 0);
            
            Color color = GUI.color;
            if (brokenPrefabs > 0 || zeroRadius > 0)
            {
                GUI.color = Color.yellow;
            }
            
            EditorGUILayout.LabelField(challenge.challengeName, GUILayout.Width(200));
            
            if (brokenPrefabs > 0)
            {
                EditorGUILayout.LabelField($"⚠ {brokenPrefabs} broken", GUILayout.Width(100));
            }
            if (zeroRadius > 0)
            {
                EditorGUILayout.LabelField($"⚠ {zeroRadius} zero radius", GUILayout.Width(120));
            }
            
            GUI.color = color;
            
            if (GUILayout.Button("Fix", GUILayout.Width(60)))
            {
                FixChallenge(challenge);
            }
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = challenge;
                EditorGUIUtility.PingObject(challenge);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void RunFullDiagnostic()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        int totalChallenges = 0;
        int brokenChallenges = 0;
        int totalBrokenPrefabs = 0;
        int totalZeroRadius = 0;
        
        Debug.Log("=== Challenge Spawn Diagnostic ===");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge == null) continue;
            totalChallenges++;
            
            if (challenge.spawnItems == null || challenge.spawnItems.Count == 0)
            {
                Debug.LogWarning($"⚠ {challenge.challengeName}: No spawn items configured", challenge);
                brokenChallenges++;
                continue;
            }
            
            bool hasIssues = false;
            
            for (int i = 0; i < challenge.spawnItems.Count; i++)
            {
                var item = challenge.spawnItems[i];
                
                if (item.prefab == null)
                {
                    Debug.LogError($"❌ {challenge.challengeName} - Item #{i}: Missing prefab!", challenge);
                    totalBrokenPrefabs++;
                    hasIssues = true;
                }
                
                if (item.spawnLocation != ChallengeData.SpawnLocationType.AtCenter && item.spawnRadius <= 0)
                {
                    Debug.LogWarning($"⚠ {challenge.challengeName} - Item #{i}: Zero spawn radius with {item.spawnLocation} mode!", challenge);
                    totalZeroRadius++;
                    hasIssues = true;
                }
            }
            
            if (hasIssues)
            {
                brokenChallenges++;
            }
        }
        
        Debug.Log("=== Diagnostic Complete ===");
        Debug.Log($"Total Challenges: {totalChallenges}");
        Debug.Log($"Challenges with Issues: {brokenChallenges}");
        Debug.Log($"Broken Prefabs: {totalBrokenPrefabs}");
        Debug.Log($"Zero Radius Issues: {totalZeroRadius}");
        
        EditorUtility.DisplayDialog(
            "Diagnostic Complete",
            $"Scan Results:\n\n" +
            $"Total Challenges: {totalChallenges}\n" +
            $"Issues Found: {brokenChallenges}\n" +
            $"Broken Prefabs: {totalBrokenPrefabs}\n" +
            $"Zero Radius: {totalZeroRadius}\n\n" +
            "Check Console for details.",
            "OK");
    }
    
    private void QuickFixSpawnSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        int fixedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge == null || challenge.spawnItems == null) continue;
            
            bool modified = false;
            
            foreach (var item in challenge.spawnItems)
            {
                // Fix zero radius for non-center spawns
                if (item.spawnLocation != ChallengeData.SpawnLocationType.AtCenter && item.spawnRadius <= 0)
                {
                    item.spawnRadius = 15f; // Default safe radius
                    modified = true;
                }
                
                // Set requireNavMesh to false for non-enemy categories
                if (item.category != ChallengeData.SpawnableCategory.Enemy && 
                    item.category != ChallengeData.SpawnableCategory.Boss)
                {
                    item.requireNavMesh = false;
                    modified = true;
                }
            }
            
            if (modified)
            {
                EditorUtility.SetDirty(challenge);
                fixedCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✓ Fixed spawn settings for {fixedCount} challenges</color>");
        
        EditorUtility.DisplayDialog(
            "Quick Fix Complete",
            $"Fixed spawn settings for {fixedCount} challenges!\n\n" +
            "Changes:\n" +
            "• Set default radius (15m) for zero-radius spawns\n" +
            "• Disabled NavMesh requirement for non-enemies\n\n" +
            "Broken prefabs still need manual fixing.",
            "OK");
    }
    
    private void FixBrokenPrefabs()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Fix Broken Prefabs",
            "This will remove all spawn items with missing prefab references.\n\n" +
            "You will need to re-add prefabs manually in Inspector.\n\n" +
            "Continue?",
            "Yes, Clean Up",
            "Cancel");
        
        if (!confirm) return;
        
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        int fixedCount = 0;
        int removed = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge == null || challenge.spawnItems == null) continue;
            
            int before = challenge.spawnItems.Count;
            challenge.spawnItems.RemoveAll(item => item.prefab == null);
            int after = challenge.spawnItems.Count;
            
            if (before != after)
            {
                removed += (before - after);
                EditorUtility.SetDirty(challenge);
                fixedCount++;
                Debug.Log($"Removed {before - after} broken items from {challenge.challengeName}", challenge);
            }
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>✓ Cleaned {removed} broken spawn items from {fixedCount} challenges</color>");
        
        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"Removed {removed} broken spawn items from {fixedCount} challenges.\n\n" +
            "You should now:\n" +
            "1. Select each challenge in Project\n" +
            "2. Add proper prefabs to spawn items\n" +
            "3. Configure spawn settings",
            "OK");
    }
    
    private void FixChallenge(ChallengeData challenge)
    {
        bool modified = false;
        
        // Fix zero radius
        foreach (var item in challenge.spawnItems)
        {
            if (item.spawnLocation != ChallengeData.SpawnLocationType.AtCenter && item.spawnRadius <= 0)
            {
                item.spawnRadius = 15f;
                modified = true;
            }
            
            if (item.category != ChallengeData.SpawnableCategory.Enemy && 
                item.category != ChallengeData.SpawnableCategory.Boss)
            {
                item.requireNavMesh = false;
                modified = true;
            }
        }
        
        // Remove broken prefabs
        int before = challenge.spawnItems.Count;
        challenge.spawnItems.RemoveAll(item => item.prefab == null);
        int after = challenge.spawnItems.Count;
        
        if (before != after)
        {
            modified = true;
            Debug.Log($"Removed {before - after} broken prefabs from {challenge.challengeName}", challenge);
        }
        
        if (modified)
        {
            EditorUtility.SetDirty(challenge);
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>✓ Fixed {challenge.challengeName}</color>", challenge);
            
            EditorUtility.DisplayDialog(
                "Challenge Fixed",
                $"{challenge.challengeName} has been fixed!\n\n" +
                $"Broken prefabs removed: {before - after}\n" +
                "Default spawn settings applied.\n\n" +
                "Add proper prefabs in Inspector.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Issues", $"{challenge.challengeName} has no spawn issues!", "OK");
        }
    }
}
