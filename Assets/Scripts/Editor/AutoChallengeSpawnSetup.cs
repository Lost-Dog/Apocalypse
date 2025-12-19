using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AutoChallengeSpawnSetup : EditorWindow
{
    [MenuItem("Division Game/Auto-Setup Challenge Spawns")]
    public static void ShowWindow()
    {
        GetWindow<AutoChallengeSpawnSetup>("Auto Spawn Setup").Show();
    }

    private ChallengeData selectedChallenge;
    private float spawnRadius = 30f;
    private Vector3 centerOffset = new Vector3(50, 0, 0);

    private void OnGUI()
    {
        GUILayout.Label("Automatic Challenge Spawn Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool automatically:\n" +
            "• Creates spawn points in the scene\n" +
            "• Assigns them to the correct spawn items\n" +
            "• Fixes missing prefabs\n" +
            "• Sets proper counts",
            MessageType.Info
        );

        EditorGUILayout.Space();

        selectedChallenge = (ChallengeData)EditorGUILayout.ObjectField(
            "Challenge Data",
            selectedChallenge,
            typeof(ChallengeData),
            false
        );

        if (selectedChallenge != null)
        {
            EditorGUILayout.Space();
            GUILayout.Label($"Challenge: {selectedChallenge.challengeName}", EditorStyles.boldLabel);
            GUILayout.Label($"Type: {selectedChallenge.challengeType}");
            
            EditorGUILayout.Space();
            GUILayout.Label("Spawn Settings", EditorStyles.boldLabel);
            
            spawnRadius = EditorGUILayout.Slider("Spawn Radius", spawnRadius, 10f, 100f);
            centerOffset = EditorGUILayout.Vector3Field("Center Offset from Player", centerOffset);

            EditorGUILayout.Space();

            if (selectedChallenge.spawnItems == null || selectedChallenge.spawnItems.Count == 0)
            {
                EditorGUILayout.HelpBox("No spawn items! Add spawn items first in the Challenge Data Inspector.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("Spawn Items:", EditorStyles.boldLabel);
                for (int i = 0; i < selectedChallenge.spawnItems.Count; i++)
                {
                    var item = selectedChallenge.spawnItems[i];
                    string status = item.prefab != null ? "✓" : "❌";
                    EditorGUILayout.LabelField($"  {status} {item.itemName} ({item.minCount}-{item.maxCount})");
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("🚀 AUTO-SETUP EVERYTHING", GUILayout.Height(40)))
                {
                    AutoSetupChallenge();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select a Challenge Data asset above to begin.", MessageType.Warning);
        }
    }

    private void AutoSetupChallenge()
    {
        if (selectedChallenge == null)
        {
            EditorUtility.DisplayDialog("Error", "No challenge selected!", "OK");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Cannot find Player in scene!", "OK");
            return;
        }

        Debug.Log($"<color=cyan>===== AUTO-SETUP: {selectedChallenge.challengeName} =====</color>");

        Vector3 centerPosition = player.transform.position + centerOffset;

        GameObject mainContainer = new GameObject($"{selectedChallenge.challengeName}_SpawnPoints");
        mainContainer.transform.position = centerPosition;

        SerializedObject so = new SerializedObject(selectedChallenge);
        SerializedProperty spawnItemsProp = so.FindProperty("spawnItems");

        int totalFixed = 0;
        int totalSpawnPoints = 0;

        for (int i = 0; i < spawnItemsProp.arraySize; i++)
        {
            SerializedProperty itemProp = spawnItemsProp.GetArrayElementAtIndex(i);
            string itemName = itemProp.FindPropertyRelative("itemName").stringValue;
            int minCount = itemProp.FindPropertyRelative("minCount").intValue;
            int maxCount = itemProp.FindPropertyRelative("maxCount").intValue;
            SerializedProperty prefabProp = itemProp.FindPropertyRelative("prefab");

            int countToCreate = Mathf.Max(minCount, maxCount);
            if (countToCreate == 0) countToCreate = 1;

            Debug.Log($"Processing: {itemName} (needs {countToCreate} spawn points)");

            if (prefabProp.objectReferenceValue == null)
            {
                GameObject defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Prefabs/Character_Prefabs/Enemies/Patrol AI_Elite.prefab"
                );
                if (defaultPrefab != null)
                {
                    prefabProp.objectReferenceValue = defaultPrefab;
                    Debug.Log($"  ✓ Assigned default enemy prefab");
                    totalFixed++;
                }
            }

            GameObject itemContainer = new GameObject(itemName);
            itemContainer.transform.SetParent(mainContainer.transform);
            itemContainer.transform.position = centerPosition;

            List<Transform> spawnPoints = new List<Transform>();

            bool isBoss = itemName.ToLower().Contains("boss") || 
                         itemName.ToLower().Contains("lieutenant") ||
                         itemName.ToLower().Contains("elite");

            float radiusMultiplier = isBoss ? 0.5f : 1f;
            float currentRadius = spawnRadius * radiusMultiplier;

            for (int j = 0; j < countToCreate; j++)
            {
                float angle = (360f / countToCreate) * j;
                float rad = angle * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(
                    Mathf.Cos(rad) * currentRadius,
                    0,
                    Mathf.Sin(rad) * currentRadius
                );

                GameObject spawnPoint = new GameObject($"{itemName}_{j + 1:00}");
                spawnPoint.transform.SetParent(itemContainer.transform);
                spawnPoint.transform.position = centerPosition + offset;
                spawnPoint.transform.LookAt(centerPosition);

                spawnPoints.Add(spawnPoint.transform);
                totalSpawnPoints++;
            }

            SerializedProperty spawnPointsProp = itemProp.FindPropertyRelative("customSpawnPoints");
            spawnPointsProp.arraySize = spawnPoints.Count;
            for (int j = 0; j < spawnPoints.Count; j++)
            {
                spawnPointsProp.GetArrayElementAtIndex(j).objectReferenceValue = spawnPoints[j];
            }

            itemProp.FindPropertyRelative("minCount").intValue = spawnPoints.Count;
            itemProp.FindPropertyRelative("maxCount").intValue = spawnPoints.Count;

            Debug.Log($"  ✓ Created {spawnPoints.Count} spawn points");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(selectedChallenge);
        AssetDatabase.SaveAssets();

        Undo.RegisterCreatedObjectUndo(mainContainer, "Auto-Setup Challenge Spawns");
        Selection.activeGameObject = mainContainer;
        SceneView.lastActiveSceneView.FrameSelected();

        Debug.Log($"<color=green>✅ COMPLETE: Created {totalSpawnPoints} spawn points, fixed {totalFixed} prefabs</color>");

        EditorUtility.DisplayDialog(
            "Auto-Setup Complete!",
            $"Successfully configured '{selectedChallenge.challengeName}'!\n\n" +
            $"• Created {totalSpawnPoints} spawn points\n" +
            $"• Fixed {totalFixed} missing prefabs\n" +
            $"• Assigned to {spawnItemsProp.arraySize} spawn items\n\n" +
            $"Adjust spawn point positions in Scene View if needed.\n" +
            $"The challenge is ready to test!",
            "OK"
        );
    }

    [MenuItem("Division Game/Quick Fix Selected Challenge")]
    public static void QuickFixSelected()
    {
        if (Selection.activeObject is ChallengeData challenge)
        {
            var window = GetWindow<AutoChallengeSpawnSetup>("Auto Spawn Setup");
            window.selectedChallenge = challenge;
            window.Show();
            window.Focus();
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Select a ChallengeData asset in the Project window first!", "OK");
        }
    }
}
