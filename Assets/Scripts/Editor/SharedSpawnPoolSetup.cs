using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SharedSpawnPoolSetup : EditorWindow
{
    [MenuItem("Division Game/Setup Shared Spawn Pool")]
    public static void ShowWindow()
    {
        GetWindow<SharedSpawnPoolSetup>("Shared Spawn Pool").Show();
    }

    private ChallengeData targetChallenge;
    private GameObject spawnPointsRoot;

    private void OnGUI()
    {
        GUILayout.Label("Shared Spawn Pool Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This sets up a SHARED spawn point pool.\n\n" +
            "• Any spawn item can use any spawn point\n" +
            "• Points are randomly selected and removed from pool\n" +
            "• No duplicate positions\n\n" +
            "Much simpler than assigning specific points to specific enemies!",
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

        EditorGUILayout.HelpBox(
            "Select the parent GameObject that contains all your spawn point children.\n" +
            "All child transforms will be collected into the shared pool.",
            MessageType.None
        );

        EditorGUILayout.Space();

        if (targetChallenge != null && spawnPointsRoot != null)
        {
            int childCount = CountAllChildren(spawnPointsRoot.transform);
            EditorGUILayout.LabelField($"Found {childCount} spawn point(s) in hierarchy");

            EditorGUILayout.Space();

            if (GUILayout.Button("🎯 Assign to Shared Spawn Pool", GUILayout.Height(40)))
            {
                AssignToSharedPool();
            }

            if (targetChallenge.sharedSpawnPoints != null && targetChallenge.sharedSpawnPoints.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    $"Current pool: {targetChallenge.sharedSpawnPoints.Length} spawn points",
                    MessageType.Info
                );

                if (GUILayout.Button("Clear Shared Pool", GUILayout.Height(25)))
                {
                    ClearSharedPool();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select both a Challenge Data asset and a Spawn Points Root GameObject.", MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Spawn Points Circle (10 points, 30m radius)", GUILayout.Height(30)))
        {
            CreateSpawnCircle(10, 30f);
        }

        if (GUILayout.Button("Create Spawn Points Circle (15 points, 40m radius)", GUILayout.Height(30)))
        {
            CreateSpawnCircle(15, 40f);
        }
    }

    private int CountAllChildren(Transform parent)
    {
        int count = 0;
        foreach (Transform child in parent)
        {
            count++;
            count += CountAllChildren(child);
        }
        return count;
    }

    private void CollectAllChildren(Transform parent, List<Transform> collection)
    {
        foreach (Transform child in parent)
        {
            collection.Add(child);
            CollectAllChildren(child, collection);
        }
    }

    private void AssignToSharedPool()
    {
        if (targetChallenge == null || spawnPointsRoot == null)
            return;

        List<Transform> allSpawnPoints = new List<Transform>();
        CollectAllChildren(spawnPointsRoot.transform, allSpawnPoints);

        allSpawnPoints.RemoveAll(t => t == null);

        if (allSpawnPoints.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No child transforms found!", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(targetChallenge);
        SerializedProperty sharedPoolProp = so.FindProperty("sharedSpawnPoints");

        sharedPoolProp.arraySize = allSpawnPoints.Count;
        for (int i = 0; i < allSpawnPoints.Count; i++)
        {
            sharedPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = allSpawnPoints[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetChallenge);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>✅ Assigned {allSpawnPoints.Count} spawn points to shared pool for '{targetChallenge.challengeName}'</color>");

        EditorUtility.DisplayDialog(
            "Success!",
            $"Assigned {allSpawnPoints.Count} spawn points to shared pool!\n\n" +
            $"Challenge '{targetChallenge.challengeName}' will now randomly pick from these points when spawning enemies.",
            "OK"
        );
    }

    private void ClearSharedPool()
    {
        if (targetChallenge == null)
            return;

        SerializedObject so = new SerializedObject(targetChallenge);
        SerializedProperty sharedPoolProp = so.FindProperty("sharedSpawnPoints");
        sharedPoolProp.arraySize = 0;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetChallenge);
        AssetDatabase.SaveAssets();

        Debug.Log($"Cleared shared spawn pool for '{targetChallenge.challengeName}'");
    }

    private void CreateSpawnCircle(int count, float radius)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 center = player != null ? player.transform.position : Vector3.zero;

        GameObject root = new GameObject($"SpawnPoints_{count}");
        root.transform.position = center;

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
            spawnPoint.transform.SetParent(root.transform);
            spawnPoint.transform.position = center + offset;
            spawnPoint.transform.LookAt(center);
        }

        Undo.RegisterCreatedObjectUndo(root, "Create Spawn Points");
        Selection.activeGameObject = root;
        spawnPointsRoot = root;

        Debug.Log($"✅ Created {count} spawn points in circle (radius: {radius}m)");
    }

    [MenuItem("Division Game/Quick Setup Shared Pool")]
    public static void QuickSetup()
    {
        if (Selection.gameObjects.Length > 0 && Selection.activeObject is ChallengeData)
        {
            var window = GetWindow<SharedSpawnPoolSetup>("Shared Spawn Pool");
            window.targetChallenge = Selection.activeObject as ChallengeData;
            window.Show();
            window.Focus();
        }
        else if (Selection.gameObjects.Length > 0)
        {
            var window = GetWindow<SharedSpawnPoolSetup>("Shared Spawn Pool");
            window.spawnPointsRoot = Selection.gameObjects[0];
            window.Show();
            window.Focus();
        }
        else
        {
            ShowWindow();
        }
    }
}
