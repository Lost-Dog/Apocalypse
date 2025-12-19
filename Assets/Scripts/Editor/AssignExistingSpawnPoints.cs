using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssignExistingSpawnPoints : EditorWindow
{
    [MenuItem("Division Game/Assign Existing Spawn Points")]
    public static void ShowWindow()
    {
        GetWindow<AssignExistingSpawnPoints>("Assign Spawn Points").Show();
    }

    private ChallengeData targetChallenge;
    private GameObject spawnPointsRoot;

    private void OnGUI()
    {
        GUILayout.Label("Assign Existing Spawn Points", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool assigns already-created spawn points in your scene to a Challenge Data asset.\n\n" +
            "Your spawn points should be organized like:\n" +
            "  SpawnPoints_Root\n" +
            "    ├── Regular Enemies (parent)\n" +
            "    │   └── Regular Enemies_01 (spawn point)\n" +
            "    └── Boss Enemy (parent)\n" +
            "        └── Boss Enemy_01 (spawn point)",
            MessageType.Info
        );

        EditorGUILayout.Space();

        targetChallenge = (ChallengeData)EditorGUILayout.ObjectField(
            "Challenge Data",
            targetChallenge,
            typeof(ChallengeData),
            false
        );

        spawnPointsRoot = (GameObject)EditorGUILayout.ObjectField(
            "Spawn Points Root",
            spawnPointsRoot,
            typeof(GameObject),
            true
        );

        EditorGUILayout.Space();

        if (targetChallenge != null && spawnPointsRoot != null)
        {
            if (GUILayout.Button("🔗 Assign Spawn Points to Challenge", GUILayout.Height(40)))
            {
                AssignSpawnPoints();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Preview Assignment", GUILayout.Height(25)))
            {
                PreviewAssignment();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select both a Challenge Data and a Spawn Points Root GameObject.", MessageType.Warning);
        }
    }

    private void PreviewAssignment()
    {
        Debug.Log("<color=cyan>===== Preview Spawn Point Assignment =====</color>");
        Debug.Log($"Challenge: {targetChallenge.challengeName}");
        Debug.Log($"Spawn Points Root: {spawnPointsRoot.name}");
        Debug.Log($"Spawn Items: {targetChallenge.spawnItems.Count}");

        Dictionary<string, List<Transform>> groupedSpawnPoints = new Dictionary<string, List<Transform>>();

        foreach (Transform child in spawnPointsRoot.transform)
        {
            string groupName = child.name;
            List<Transform> points = new List<Transform>();

            foreach (Transform point in child)
            {
                points.Add(point);
            }

            if (points.Count > 0)
            {
                groupedSpawnPoints[groupName] = points;
                Debug.Log($"\n<color=yellow>{groupName}</color>: {points.Count} spawn points");
                foreach (var p in points)
                {
                    Debug.Log($"  • {p.name} at {p.position}");
                }
            }
        }

        Debug.Log("\n<color=cyan>Challenge Spawn Items:</color>");
        for (int i = 0; i < targetChallenge.spawnItems.Count; i++)
        {
            var item = targetChallenge.spawnItems[i];
            Debug.Log($"\n[{i}] {item.itemName}");
            Debug.Log($"  Prefab: {(item.prefab != null ? item.prefab.name : "❌ NONE")}");
            Debug.Log($"  Current Spawn Points: {(item.customSpawnPoints != null ? item.customSpawnPoints.Length : 0)}");
        }
    }

    private void AssignSpawnPoints()
    {
        if (targetChallenge == null || spawnPointsRoot == null)
            return;

        Debug.Log($"<color=cyan>===== Assigning Spawn Points to {targetChallenge.challengeName} =====</color>");

        Dictionary<string, List<Transform>> groupedSpawnPoints = new Dictionary<string, List<Transform>>();

        foreach (Transform child in spawnPointsRoot.transform)
        {
            string groupName = child.name;
            List<Transform> points = new List<Transform>();

            foreach (Transform point in child)
            {
                points.Add(point);
            }

            if (points.Count > 0)
            {
                groupedSpawnPoints[groupName] = points;
            }
        }

        SerializedObject so = new SerializedObject(targetChallenge);
        SerializedProperty spawnItemsProp = so.FindProperty("spawnItems");

        int assignedCount = 0;

        for (int i = 0; i < spawnItemsProp.arraySize; i++)
        {
            SerializedProperty itemProp = spawnItemsProp.GetArrayElementAtIndex(i);
            string itemName = itemProp.FindPropertyRelative("itemName").stringValue;

            List<Transform> matchingPoints = null;

            foreach (var kvp in groupedSpawnPoints)
            {
                if (kvp.Key.Equals(itemName, System.StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Contains(itemName) ||
                    itemName.Contains(kvp.Key))
                {
                    matchingPoints = kvp.Value;
                    break;
                }
            }

            if (matchingPoints != null && matchingPoints.Count > 0)
            {
                SerializedProperty spawnPointsProp = itemProp.FindPropertyRelative("customSpawnPoints");
                spawnPointsProp.arraySize = matchingPoints.Count;

                for (int j = 0; j < matchingPoints.Count; j++)
                {
                    spawnPointsProp.GetArrayElementAtIndex(j).objectReferenceValue = matchingPoints[j];
                }

                itemProp.FindPropertyRelative("minCount").intValue = matchingPoints.Count;
                itemProp.FindPropertyRelative("maxCount").intValue = matchingPoints.Count;

                Debug.Log($"✓ Assigned {matchingPoints.Count} spawn points to '{itemName}'");
                assignedCount++;
            }
            else
            {
                Debug.LogWarning($"❌ No matching spawn points found for '{itemName}'");
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetChallenge);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>✅ Assignment complete! {assignedCount}/{spawnItemsProp.arraySize} spawn items configured</color>");

        EditorUtility.DisplayDialog(
            "Spawn Points Assigned!",
            $"Successfully assigned spawn points to {assignedCount} spawn item(s)!\n\n" +
            $"Challenge '{targetChallenge.challengeName}' is ready to test.",
            "OK"
        );
    }

    [MenuItem("Division Game/Quick Assign from Selection")]
    public static void QuickAssignFromSelection()
    {
        if (Selection.gameObjects.Length > 0)
        {
            GameObject selected = Selection.gameObjects[0];
            
            var window = GetWindow<AssignExistingSpawnPoints>("Assign Spawn Points");
            window.spawnPointsRoot = selected;
            window.Show();
            window.Focus();

            Debug.Log($"Selected spawn points root: {selected.name}");
            Debug.Log("Now drag your Challenge Data into the window and click 'Assign Spawn Points to Challenge'");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Select a spawn points GameObject in the Hierarchy first!", "OK");
        }
    }
}
