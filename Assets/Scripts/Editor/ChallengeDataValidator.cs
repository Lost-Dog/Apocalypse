using UnityEngine;
using UnityEditor;
using System.Linq;

public class ChallengeDataValidator : EditorWindow
{
    [MenuItem("Tools/Challenges/Validate All Challenge Data")]
    public static void ValidateAllChallenges()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        int totalChallenges = 0;
        int brokenPrefabs = 0;
        int fixedCount = 0;
        
        Debug.Log("=== Challenge Data Validation ===");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge == null) continue;
            totalChallenges++;
            
            bool challengeHasIssues = false;
            
            for (int i = 0; i < challenge.spawnItems.Count; i++)
            {
                var item = challenge.spawnItems[i];
                
                if (item.prefab == null)
                {
                    string itemName = string.IsNullOrEmpty(item.itemName) ? $"[Unnamed {item.category}]" : item.itemName;
                    Debug.LogError($"❌ BROKEN: '{challenge.challengeName}' → Item #{i} '{itemName}' has NULL prefab!", challenge);
                    challengeHasIssues = true;
                    brokenPrefabs++;
                }
            }
            
            if (challengeHasIssues)
            {
                Debug.LogWarning($"⚠️ Challenge '{challenge.challengeName}' at {path} needs attention", challenge);
            }
        }
        
        Debug.Log($"=== Validation Complete ===");
        Debug.Log($"Total Challenges: {totalChallenges}");
        Debug.Log($"Broken Prefab References: {brokenPrefabs}");
        
        if (brokenPrefabs > 0)
        {
            Debug.LogError($"Found {brokenPrefabs} broken prefab references! Click the error logs above to open the affected challenges.");
            Debug.LogError("Fix: Select the challenge asset, find the spawn item with missing prefab, and reassign it in the Inspector.");
        }
        else
        {
            Debug.Log("✅ All challenges are valid!");
        }
    }
    
    [MenuItem("Tools/Challenges/List All Challenge Spawn Items")]
    public static void ListAllSpawnItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        
        Debug.Log("=== Challenge Spawn Items ===");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge == null || challenge.spawnItems.Count == 0) continue;
            
            Debug.Log($"\n<b>{challenge.challengeName}</b> ({challenge.spawnItems.Count} items):");
            
            for (int i = 0; i < challenge.spawnItems.Count; i++)
            {
                var item = challenge.spawnItems[i];
                string prefabName = item.prefab != null ? item.prefab.name : "❌ NULL";
                string itemName = string.IsNullOrEmpty(item.itemName) ? "[Unnamed]" : item.itemName;
                
                Debug.Log($"  [{i}] {itemName} ({item.category}) → Prefab: {prefabName} | Count: {item.minCount}-{item.maxCount} | Radius: {item.spawnRadius}m", challenge);
            }
        }
    }
}
