using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChallengeSpawnPointSetup : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Setup Spawn Points")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChallengeSpawnPointSetup>("Spawn Points Setup");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private ChallengeData selectedChallenge;
    private Vector2 scrollPos;
    private Transform spawnPointsContainer;

    private void OnGUI()
    {
        GUILayout.Label("Challenge Spawn Points Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Set specific spawn positions for challenges to avoid NavMesh/obstruction issues.\n\n" +
            "Two methods available:\n" +
            "1. Use MissionZone (recommended for complex setups)\n" +
            "2. Add custom spawn points to Challenge Data directly",
            MessageType.Info);

        GUILayout.Space(10);

        // Method 1: MissionZone
        EditorGUILayout.LabelField("Method 1: Use Mission Zone", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Create a MissionZone GameObject with spawn point children:\n" +
            "• Enemies spawn at exact positions\n" +
            "• No NavMesh failures\n" +
            "• Reusable for multiple challenges",
            MessageType.None);

        if (GUILayout.Button("Create Mission Zone for Current Selection", GUILayout.Height(30)))
        {
            CreateMissionZone();
        }

        GUILayout.Space(10);

        // Method 2: Direct spawn points
        EditorGUILayout.LabelField("Method 2: Custom Spawn Points (Direct)", EditorStyles.boldLabel);
        
        selectedChallenge = (ChallengeData)EditorGUILayout.ObjectField(
            "Challenge Data", selectedChallenge, typeof(ChallengeData), false);

        if (selectedChallenge != null)
        {
            spawnPointsContainer = (Transform)EditorGUILayout.ObjectField(
                "Spawn Points Container", spawnPointsContainer, typeof(Transform), true);

            EditorGUILayout.HelpBox(
                "Select a parent GameObject with child transforms.\n" +
                "Each child will be used as a spawn point.",
                MessageType.None);

            if (spawnPointsContainer != null)
            {
                int childCount = spawnPointsContainer.childCount;
                EditorGUILayout.LabelField($"Found {childCount} child transforms");

                if (childCount > 0 && GUILayout.Button("Assign Spawn Points to Challenge", GUILayout.Height(30)))
                {
                    AssignSpawnPointsToChallenge();
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Show Current Spawn Points", GUILayout.Height(25)))
            {
                ShowCurrentSpawnPoints();
            }
        }

        GUILayout.Space(10);

        // Quick create spawn points
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
        if (GUILayout.Button("Create 10 Spawn Points Around Selection", GUILayout.Height(30)))
        {
            CreateSpawnPointsAroundSelection(10, 20f);
        }

        if (GUILayout.Button("Create 15 Spawn Points Around Selection", GUILayout.Height(30)))
        {
            CreateSpawnPointsAroundSelection(15, 30f);
        }
    }

    private void CreateMissionZone()
    {
        Transform selected = Selection.activeTransform;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Select a GameObject in the scene first!", "OK");
            return;
        }

        GameObject missionZone = new GameObject("MissionZone_" + selected.name);
        missionZone.transform.position = selected.position;
        
        MissionZone zone = missionZone.AddComponent<MissionZone>();
        zone.zoneName = "Mission Zone - " + selected.name;
        zone.zoneRadius = 30f;

        // Create spawn points container
        GameObject spawnContainer = new GameObject("SpawnPoints");
        spawnContainer.transform.SetParent(missionZone.transform);
        spawnContainer.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(missionZone, "Create Mission Zone");

        Selection.activeGameObject = missionZone;
        EditorGUIUtility.PingObject(missionZone);

        Debug.Log($"✅ Created Mission Zone at {selected.position}");
        Debug.Log("Next steps:");
        Debug.Log("1. Add child GameObjects under 'SpawnPoints' for each enemy");
        Debug.Log("2. Configure MissionZone.spawnPoints in Inspector");
        Debug.Log("3. Link to ChallengeData in Inspector");

        EditorUtility.DisplayDialog("Success",
            "Mission Zone created!\n\n" +
            "Next steps:\n" +
            "1. Add child GameObjects under 'SpawnPoints'\n" +
            "2. Position them where enemies should spawn\n" +
            "3. Configure spawn points in MissionZone Inspector\n" +
            "4. Link to ChallengeData", "OK");
    }

    private void AssignSpawnPointsToChallenge()
    {
        if (selectedChallenge == null || spawnPointsContainer == null)
            return;

        List<Transform> spawnPoints = new List<Transform>();
        foreach (Transform child in spawnPointsContainer)
        {
            spawnPoints.Add(child);
        }

        if (spawnPoints.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No child transforms found!", "OK");
            return;
        }

        // Create or get first spawn item
        if (selectedChallenge.spawnItems.Count == 0)
        {
            selectedChallenge.spawnItems.Add(new ChallengeData.SpawnableItem());
        }

        selectedChallenge.spawnItems[0].customSpawnPoints = spawnPoints.ToArray();
        selectedChallenge.spawnItems[0].minCount = spawnPoints.Count;
        selectedChallenge.spawnItems[0].maxCount = spawnPoints.Count;

        EditorUtility.SetDirty(selectedChallenge);

        Debug.Log($"✅ Assigned {spawnPoints.Count} spawn points to {selectedChallenge.challengeName}");
        EditorUtility.DisplayDialog("Success",
            $"Assigned {spawnPoints.Count} spawn points!\n\n" +
            "Make sure to assign the enemy prefab in the Challenge Data Inspector.",
            "OK");
    }

    private void ShowCurrentSpawnPoints()
    {
        if (selectedChallenge == null || selectedChallenge.spawnItems.Count == 0)
        {
            Debug.Log("No spawn items configured");
            return;
        }

        Debug.Log($"=== Spawn Points for {selectedChallenge.challengeName} ===");
        
        for (int i = 0; i < selectedChallenge.spawnItems.Count; i++)
        {
            var item = selectedChallenge.spawnItems[i];
            Debug.Log($"Item {i}: {item.itemName}");
            
            if (item.customSpawnPoints != null && item.customSpawnPoints.Length > 0)
            {
                Debug.Log($"  Custom spawn points: {item.customSpawnPoints.Length}");
                for (int j = 0; j < item.customSpawnPoints.Length; j++)
                {
                    if (item.customSpawnPoints[j] != null)
                    {
                        Debug.Log($"    {j}: {item.customSpawnPoints[j].name} at {item.customSpawnPoints[j].position}");
                    }
                    else
                    {
                        Debug.LogWarning($"    {j}: NULL");
                    }
                }
            }
            else
            {
                Debug.Log($"  Using random spawning: {item.spawnLocation}, Radius: {item.spawnRadius}m");
            }
        }
    }

    private void CreateSpawnPointsAroundSelection(int count, float radius)
    {
        Transform selected = Selection.activeTransform;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Select a GameObject in the scene first!", "OK");
            return;
        }

        GameObject container = new GameObject($"SpawnPoints_{count}");
        container.transform.position = selected.position;
        
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * radius,
                0,
                Mathf.Sin(rad) * radius
            );

            GameObject spawnPoint = new GameObject($"SpawnPoint_{i + 1:00}");
            spawnPoint.transform.SetParent(container.transform);
            spawnPoint.transform.position = selected.position + offset;
            spawnPoint.transform.LookAt(selected.position);
        }

        Undo.RegisterCreatedObjectUndo(container, "Create Spawn Points");
        Selection.activeGameObject = container;

        Debug.Log($"✅ Created {count} spawn points in a circle (radius: {radius}m)");
        EditorUtility.DisplayDialog("Success",
            $"Created {count} spawn points!\n\n" +
            "Assign this container to 'Spawn Points Container' above,\n" +
            "then click 'Assign Spawn Points to Challenge'.",
            "OK");
    }
}
