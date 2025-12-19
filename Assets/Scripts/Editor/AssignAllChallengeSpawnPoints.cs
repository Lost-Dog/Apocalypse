using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AssignAllChallengeSpawnPoints : EditorWindow
{
    [MenuItem("Division Game/Auto-Assign All Challenge Spawn Points")]
    public static void AutoAssignAll()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        
        GameObject challengeZonesParent = null;
        foreach (GameObject obj in rootObjects)
        {
            Transform found = obj.transform.Find("Zones/ChallengeZones");
            if (found != null)
            {
                challengeZonesParent = found.gameObject;
                break;
            }
        }
        
        if (challengeZonesParent == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find ChallengeZones in scene!", "OK");
            return;
        }
        
        int processedCount = 0;
        int assignedCount = 0;
        
        foreach (Transform challengeZone in challengeZonesParent.transform)
        {
            MissionZone missionZone = challengeZone.GetComponent<MissionZone>();
            if (missionZone == null || missionZone.linkedChallengeData == null)
                continue;
            
            processedCount++;
            
            Transform spawnPointsContainer = null;
            foreach (Transform child in challengeZone)
            {
                if (child.name.StartsWith("SpawnPoints"))
                {
                    spawnPointsContainer = child;
                    break;
                }
            }
            
            if (spawnPointsContainer == null)
            {
                Debug.LogWarning($"No spawn points container found for {challengeZone.name}");
                continue;
            }
            
            List<Transform> allSpawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in spawnPointsContainer)
            {
                allSpawnPoints.Add(spawnPoint);
            }
            
            if (allSpawnPoints.Count == 0)
            {
                Debug.LogWarning($"No spawn points found in {spawnPointsContainer.name}");
                continue;
            }
            
            ChallengeData challengeData = missionZone.linkedChallengeData;
            SerializedObject so = new SerializedObject(challengeData);
            SerializedProperty sharedSpawnPointsProp = so.FindProperty("sharedSpawnPoints");
            
            sharedSpawnPointsProp.arraySize = allSpawnPoints.Count;
            for (int i = 0; i < allSpawnPoints.Count; i++)
            {
                sharedSpawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = allSpawnPoints[i];
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(challengeData);
            
            Debug.Log($"<color=green>✓ Assigned {allSpawnPoints.Count} spawn points to '{challengeData.challengeName}'</color>");
            assignedCount++;
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=cyan>===== Auto-Assign Complete =====</color>");
        Debug.Log($"Processed {processedCount} challenge zones");
        Debug.Log($"Assigned spawn points to {assignedCount} challenges");
        
        EditorUtility.DisplayDialog(
            "Success!",
            $"Auto-assigned spawn points!\n\n" +
            $"Processed: {processedCount} zones\n" +
            $"Updated: {assignedCount} challenges\n\n" +
            "All challenges should now spawn enemies!",
            "OK"
        );
    }
}
