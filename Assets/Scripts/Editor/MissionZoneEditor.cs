using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MissionZone))]
public class MissionZoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MissionZone missionZone = (MissionZone)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Spawn Point Setup", EditorStyles.boldLabel);
        
        if (missionZone.linkedChallengeData == null)
        {
            EditorGUILayout.HelpBox("Assign a Challenge Data asset to generate spawn items.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"Linked Challenge: {missionZone.linkedChallengeData.challengeName}", MessageType.Info);
            
            // Count spawn points by category
            int enemyCount = 0;
            int bossCount = 0;
            int civilianCount = 0;
            int otherCount = 0;
            
            foreach (var point in missionZone.spawnPoints)
            {
                if (point.transform == null) continue;
                
                switch (point.category)
                {
                    case ChallengeData.SpawnableCategory.Enemy:
                        enemyCount++;
                        break;
                    case ChallengeData.SpawnableCategory.Boss:
                        bossCount++;
                        break;
                    case ChallengeData.SpawnableCategory.Civilian:
                        civilianCount++;
                        break;
                    default:
                        otherCount++;
                        break;
                }
            }
            
            EditorGUILayout.LabelField("Spawn Point Summary:");
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Enemies: {enemyCount}");
            EditorGUILayout.LabelField($"Bosses: {bossCount}");
            EditorGUILayout.LabelField($"Civilians: {civilianCount}");
            EditorGUILayout.LabelField($"Other: {otherCount}");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate Spawn Items for Challenge", GUILayout.Height(30)))
            {
                missionZone.GenerateSpawnItemsForChallenge();
                EditorUtility.SetDirty(missionZone.linkedChallengeData);
                AssetDatabase.SaveAssets();
                Debug.Log($"✓ Generated spawn items for {missionZone.linkedChallengeData.challengeName}. Challenge Data has been saved.");
            }
            GUI.backgroundColor = Color.white;
        }
        
        EditorGUILayout.Space(5);
        
        // Quick setup tools
        EditorGUILayout.LabelField("Quick Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Find Child Spawn Points"))
        {
            AutoFindChildSpawnPoints(missionZone);
        }
        
        if (GUILayout.Button("Create Enemy Spawn Point"))
        {
            CreateSpawnPoint(missionZone, "EnemySpawn", ChallengeData.SpawnableCategory.Enemy);
        }
        
        if (GUILayout.Button("Create Boss Spawn Point"))
        {
            CreateSpawnPoint(missionZone, "BossSpawn", ChallengeData.SpawnableCategory.Boss);
        }
        
        if (GUILayout.Button("Create Civilian Spawn Point"))
        {
            CreateSpawnPoint(missionZone, "CivilianSpawn", ChallengeData.SpawnableCategory.Civilian);
        }
    }
    
    private void AutoFindChildSpawnPoints(MissionZone missionZone)
    {
        Transform[] children = missionZone.GetComponentsInChildren<Transform>();
        int added = 0;
        
        foreach (Transform child in children)
        {
            if (child == missionZone.transform) continue; // Skip self
            
            // Check if already in list
            bool alreadyAdded = false;
            foreach (var point in missionZone.spawnPoints)
            {
                if (point.transform == child)
                {
                    alreadyAdded = true;
                    break;
                }
            }
            
            if (!alreadyAdded)
            {
                MissionZone.SpawnPoint newPoint = new MissionZone.SpawnPoint();
                newPoint.pointName = child.name;
                newPoint.transform = child;
                
                // Try to auto-detect category from name
                string lowerName = child.name.ToLower();
                if (lowerName.Contains("enemy"))
                    newPoint.category = ChallengeData.SpawnableCategory.Enemy;
                else if (lowerName.Contains("boss"))
                    newPoint.category = ChallengeData.SpawnableCategory.Boss;
                else if (lowerName.Contains("civilian"))
                    newPoint.category = ChallengeData.SpawnableCategory.Civilian;
                else if (lowerName.Contains("loot"))
                    newPoint.category = ChallengeData.SpawnableCategory.LootBox;
                
                missionZone.spawnPoints.Add(newPoint);
                added++;
            }
        }
        
        if (added > 0)
        {
            EditorUtility.SetDirty(missionZone);
            Debug.Log($"✓ Auto-found and added {added} spawn points to {missionZone.zoneName}");
        }
        else
        {
            Debug.Log("No new spawn points found. All children are already in the list.");
        }
    }
    
    private void CreateSpawnPoint(MissionZone missionZone, string baseName, ChallengeData.SpawnableCategory category)
    {
        // Find a unique name
        int counter = 1;
        string finalName = $"{baseName}_{counter:00}";
        
        while (missionZone.transform.Find(finalName) != null)
        {
            counter++;
            finalName = $"{baseName}_{counter:00}";
        }
        
        // Create the GameObject
        GameObject spawnPointObj = new GameObject(finalName);
        spawnPointObj.transform.parent = missionZone.transform;
        spawnPointObj.transform.localPosition = Vector3.zero;
        
        // Add to spawn points list
        MissionZone.SpawnPoint newPoint = new MissionZone.SpawnPoint();
        newPoint.pointName = finalName;
        newPoint.transform = spawnPointObj.transform;
        newPoint.category = category;
        
        missionZone.spawnPoints.Add(newPoint);
        
        // Register undo
        Undo.RegisterCreatedObjectUndo(spawnPointObj, "Create Spawn Point");
        EditorUtility.SetDirty(missionZone);
        
        // Select the new spawn point for positioning
        Selection.activeGameObject = spawnPointObj;
        
        Debug.Log($"✓ Created {finalName} at {missionZone.zoneName}");
    }
    
    // Draw spawn point gizmos in scene view
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawMissionZoneGizmos(MissionZone missionZone, GizmoType gizmoType)
    {
        if (missionZone.spawnPoints == null) return;
        
        foreach (var point in missionZone.spawnPoints)
        {
            if (point.transform == null) continue;
            
            Color color = GetCategoryColor(point.category);
            
            // Draw sphere at spawn point
            Gizmos.color = color;
            Gizmos.DrawWireSphere(point.transform.position, 0.5f);
            
            // Draw direction arrow
            Gizmos.color = new Color(color.r, color.g, color.b, 0.7f);
            Gizmos.DrawRay(point.transform.position, point.transform.forward * 2f);
            
            // Draw line to mission zone center
            Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
            Gizmos.DrawLine(missionZone.transform.position, point.transform.position);
        }
    }
    
    private static Color GetCategoryColor(ChallengeData.SpawnableCategory category)
    {
        switch (category)
        {
            case ChallengeData.SpawnableCategory.Enemy: return Color.red;
            case ChallengeData.SpawnableCategory.Boss: return new Color(1f, 0f, 0.5f);
            case ChallengeData.SpawnableCategory.Civilian: return Color.green;
            case ChallengeData.SpawnableCategory.LootBox: return Color.yellow;
            case ChallengeData.SpawnableCategory.Objective: return Color.cyan;
            case ChallengeData.SpawnableCategory.Cover: return Color.gray;
            default: return Color.white;
        }
    }
}
