using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ConvertToDynamicZones : EditorWindow
{
    [MenuItem("Division Game/Convert to Dynamic Zone System")]
    public static void ShowWindow()
    {
        GetWindow<ConvertToDynamicZones>("Convert to Dynamic Zones").Show();
    }

    private GameObject defaultEnemyPrefab;
    private GameObject defaultCivilianPrefab;
    
    private void OnGUI()
    {
        GUILayout.Label("Dynamic Zone System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool converts existing challenge zones to the new Dynamic Zone system:\n\n" +
            "✓ Auto-detects spawn points from children\n" +
            "✓ Challenges can spawn at ANY zone\n" +
            "✓ Random zone selection\n" +
            "✓ No per-challenge zone assignment needed",
            MessageType.Info
        );

        EditorGUILayout.Space();
        
        defaultEnemyPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Enemy Prefab", 
            defaultEnemyPrefab, 
            typeof(GameObject), 
            false
        );
        
        defaultCivilianPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Civilian Prefab", 
            defaultCivilianPrefab, 
            typeof(GameObject), 
            false
        );

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("1. Create DynamicZoneManager", GUILayout.Height(40)))
        {
            CreateDynamicZoneManager();
        }
        
        EditorGUILayout.Space();

        if (GUILayout.Button("2. Convert All Zones to Dynamic", GUILayout.Height(40)))
        {
            ConvertAllZones();
        }
        
        EditorGUILayout.Space();

        if (GUILayout.Button("3. Setup Complete System (Both Steps)", GUILayout.Height(40)))
        {
            CreateDynamicZoneManager();
            ConvertAllZones();
        }
    }

    private void CreateDynamicZoneManager()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        
        GameObject gameSystemsObj = null;
        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == "GameSystems")
            {
                gameSystemsObj = obj;
                break;
            }
        }
        
        if (gameSystemsObj == null)
        {
            gameSystemsObj = new GameObject("GameSystems");
            Debug.Log("Created GameSystems root object");
        }
        
        DynamicZoneManager existing = FindFirstObjectByType<DynamicZoneManager>();
        if (existing != null)
        {
            Debug.LogWarning("DynamicZoneManager already exists!");
            
            if (defaultEnemyPrefab != null)
                existing.defaultEnemyPrefab = defaultEnemyPrefab;
            if (defaultCivilianPrefab != null)
                existing.defaultCivilianPrefab = defaultCivilianPrefab;
            
            EditorUtility.SetDirty(existing);
            return;
        }
        
        GameObject managerObj = new GameObject("DynamicZoneManager");
        managerObj.transform.SetParent(gameSystemsObj.transform);
        
        DynamicZoneManager manager = managerObj.AddComponent<DynamicZoneManager>();
        manager.defaultEnemyPrefab = defaultEnemyPrefab;
        manager.defaultCivilianPrefab = defaultCivilianPrefab;
        
        EditorUtility.SetDirty(managerObj);
        
        Debug.Log("<color=green>✓ Created DynamicZoneManager</color>");
        
        EditorUtility.DisplayDialog(
            "Success!",
            "DynamicZoneManager created successfully!\n\nNow run step 2 to convert your zones.",
            "OK"
        );
    }

    private void ConvertAllZones()
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
        
        int convertedCount = 0;
        int skippedCount = 0;
        
        List<Transform> zones = new List<Transform>();
        foreach (Transform child in challengeZonesParent.transform)
        {
            zones.Add(child);
        }
        
        foreach (Transform zoneTransform in zones)
        {
            DynamicChallengeZone existing = zoneTransform.GetComponent<DynamicChallengeZone>();
            if (existing != null)
            {
                skippedCount++;
                continue;
            }
            
            MissionZone oldZone = zoneTransform.GetComponent<MissionZone>();
            
            DynamicChallengeZone dynamicZone = zoneTransform.gameObject.AddComponent<DynamicChallengeZone>();
            
            if (oldZone != null)
            {
                dynamicZone.zoneName = oldZone.zoneName;
                dynamicZone.zoneRadius = oldZone.zoneRadius;
                dynamicZone.showGizmos = oldZone.showGizmos;
                dynamicZone.allowedChallengeTypes.Add(oldZone.missionType);
            }
            else
            {
                dynamicZone.zoneName = zoneTransform.name;
            }
            
            dynamicZone.autoDetectSpawnPoints = true;
            dynamicZone.DetectSpawnPoints();
            
            EditorUtility.SetDirty(zoneTransform.gameObject);
            
            convertedCount++;
            Debug.Log($"<color=green>✓ Converted '{dynamicZone.zoneName}' to DynamicChallengeZone ({dynamicZone.detectedSpawnPoints.Count} spawn points)</color>");
        }
        
        DynamicZoneManager manager = FindFirstObjectByType<DynamicZoneManager>();
        if (manager != null)
        {
            manager.FindAllZones();
        }
        
        Debug.Log($"<color=cyan>===== Conversion Complete =====</color>");
        Debug.Log($"Converted: {convertedCount} zones");
        Debug.Log($"Skipped (already dynamic): {skippedCount} zones");
        
        EditorUtility.DisplayDialog(
            "Conversion Complete!",
            $"Successfully converted {convertedCount} zones to Dynamic Zone system!\n\n" +
            $"Skipped: {skippedCount} (already converted)\n\n" +
            "Your challenges can now spawn at any zone randomly!",
            "OK"
        );
    }
}
