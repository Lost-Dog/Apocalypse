using UnityEngine;
using UnityEditor;
using System.Linq;

public class ChallengeDataQuickFix : EditorWindow
{
    [MenuItem("Division Game/Fix Challenge Data Issues")]
    public static void FixAllChallengeData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        int fixedCount = 0;
        int issuesFound = 0;

        Debug.Log("<color=cyan>===== Checking All Challenge Data =====</color>");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);

            if (challenge == null) continue;

            bool needsFix = false;
            string issues = "";

            if (challenge.spawnItems == null || challenge.spawnItems.Count == 0)
            {
                issues += "\n  ❌ No spawn items configured";
                issuesFound++;
            }
            else
            {
                for (int i = 0; i < challenge.spawnItems.Count; i++)
                {
                    var item = challenge.spawnItems[i];
                    
                    if (item.prefab == null)
                    {
                        issues += $"\n  ❌ Spawn item '{item.itemName}' has no prefab";
                        issuesFound++;
                        needsFix = true;
                        
                        if (item.itemName.Contains("Boss") || item.itemName.Contains("Lieutenant"))
                        {
                            GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                                "Assets/Prefabs/Character_Prefabs/Enemies/Patrol AI_Elite.prefab"
                            );
                            if (bossPrefab != null)
                            {
                                SerializedObject so = new SerializedObject(challenge);
                                SerializedProperty spawnItemsProp = so.FindProperty("spawnItems");
                                SerializedProperty itemProp = spawnItemsProp.GetArrayElementAtIndex(i);
                                itemProp.FindPropertyRelative("prefab").objectReferenceValue = bossPrefab;
                                so.ApplyModifiedProperties();
                                
                                issues += $"\n    ✓ Fixed: Assigned 'Patrol AI_Elite' prefab";
                                fixedCount++;
                            }
                        }
                    }
                    
                    if (item.customSpawnPoints == null || item.customSpawnPoints.Length == 0)
                    {
                        issues += $"\n  ⚠ Spawn item '{item.itemName}' has no spawn points (enemies won't spawn!)";
                        issuesFound++;
                    }
                }
            }

            if (!string.IsNullOrEmpty(issues))
            {
                Debug.Log($"<color=yellow>{challenge.challengeName}</color>{issues}");
            }
        }

        Debug.Log($"<color=cyan>===== Scan Complete =====</color>");
        Debug.Log($"Found {issuesFound} issues across {guids.Length} challenges");
        
        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>Automatically fixed {fixedCount} issues</color>");
            AssetDatabase.SaveAssets();
        }

        if (issuesFound > fixedCount)
        {
            Debug.LogWarning($"<color=orange>{issuesFound - fixedCount} issues require manual setup</color>");
            Debug.LogWarning("Use 'Division Game → Challenge System → Setup Spawn Points' to create spawn points");
        }
    }

    [MenuItem("Division Game/Check Current Challenge")]
    public static void CheckCurrentChallenge()
    {
        if (Selection.activeObject is ChallengeData challenge)
        {
            Debug.Log($"<color=cyan>===== Checking: {challenge.challengeName} =====</color>");
            
            Debug.Log($"Type: {challenge.challengeType}");
            Debug.Log($"Difficulty: {challenge.difficulty}");
            
            if (challenge.spawnItems == null || challenge.spawnItems.Count == 0)
            {
                Debug.LogError("❌ NO SPAWN ITEMS! Enemies cannot spawn!");
                Debug.Log("Add spawn items in Inspector → Flexible Spawning System → Spawn Items");
                return;
            }
            
            Debug.Log($"Spawn Items: {challenge.spawnItems.Count}");
            
            for (int i = 0; i < challenge.spawnItems.Count; i++)
            {
                var item = challenge.spawnItems[i];
                Debug.Log($"\n[{i}] {item.itemName}:");
                Debug.Log($"  Prefab: {(item.prefab != null ? item.prefab.name : "❌ NONE")}");
                Debug.Log($"  Category: {item.category}");
                Debug.Log($"  Count: {item.minCount}-{item.maxCount}");
                Debug.Log($"  Spawn Points: {(item.customSpawnPoints != null ? item.customSpawnPoints.Length : 0)}");
                
                if (item.prefab == null)
                {
                    Debug.LogError($"  ❌ No prefab assigned! This item won't spawn!");
                }
                
                if (item.customSpawnPoints == null || item.customSpawnPoints.Length == 0)
                {
                    Debug.LogError($"  ❌ No spawn points! Random NavMesh spawning is disabled!");
                    Debug.LogError($"     Use 'Division Game → Challenge System → Setup Spawn Points'");
                }
                else
                {
                    Debug.Log($"  ✓ {item.customSpawnPoints.Length} spawn points configured");
                }
            }
        }
        else
        {
            Debug.LogWarning("Select a ChallengeData asset to check it");
        }
    }
}
