using UnityEngine;
using UnityEditor;

public class ChallengeMarkerSetupTool : EditorWindow
{
    private GameObject markerPrefab;
    private Transform challengeZonesParent;
    private float markerHeightOffset = 10f;
    private bool onlyMissingMarkers = true;
    
    [MenuItem("Division Game/Challenge System/Setup UI Markers")]
    public static void ShowWindow()
    {
        ChallengeMarkerSetupTool window = GetWindow<ChallengeMarkerSetupTool>("Challenge Marker Setup");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnEnable()
    {
        LoadDefaultReferences();
    }
    
    private void LoadDefaultReferences()
    {
        string prefabPath = "Assets/Prefabs/ChallengeWorldMarker.prefab";
        markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        GameObject zonesParentObj = GameObject.Find("GameSystems/Zones/ChallengeZones");
        if (zonesParentObj == null)
        {
            zonesParentObj = GameObject.Find("ChallengeZones");
        }
        
        if (zonesParentObj != null)
        {
            challengeZonesParent = zonesParentObj.transform;
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge UI Marker Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Automatically add ChallengeWorldMarker components to all challenge points.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        
        markerPrefab = EditorGUILayout.ObjectField("Marker Prefab", markerPrefab, typeof(GameObject), false) as GameObject;
        challengeZonesParent = EditorGUILayout.ObjectField("Challenge Zones Parent", challengeZonesParent, typeof(Transform), true) as Transform;
        markerHeightOffset = EditorGUILayout.FloatField("Height Offset", markerHeightOffset);
        onlyMissingMarkers = EditorGUILayout.Toggle("Only Add Missing", onlyMissingMarkers);
        
        EditorGUILayout.Space(10);
        
        if (markerPrefab == null)
        {
            EditorGUILayout.HelpBox("Please assign the ChallengeWorldMarker prefab!", MessageType.Warning);
        }
        
        if (challengeZonesParent == null)
        {
            EditorGUILayout.HelpBox("Please assign the ChallengeZones parent transform!", MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(markerPrefab == null || challengeZonesParent == null);
        
        if (GUILayout.Button("Setup All Challenge Markers", GUILayout.Height(40)))
        {
            SetupAllMarkers();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Remove All Challenge Markers", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Remove Markers", 
                "Are you sure you want to remove all ChallengeWorldMarker components?", 
                "Yes", "Cancel"))
            {
                RemoveAllMarkers();
            }
        }
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find ChallengeWorldMarker Prefab"))
        {
            string prefabPath = "Assets/Prefabs/ChallengeWorldMarker.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                markerPrefab = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log("Found ChallengeWorldMarker prefab!");
            }
            else
            {
                Debug.LogWarning("ChallengeWorldMarker.prefab not found at: " + prefabPath);
            }
        }
        
        if (GUILayout.Button("Select ChallengeZones in Hierarchy"))
        {
            if (challengeZonesParent != null)
            {
                Selection.activeGameObject = challengeZonesParent.gameObject;
                EditorGUIUtility.PingObject(challengeZonesParent.gameObject);
            }
        }
    }
    
    private void SetupAllMarkers()
    {
        if (challengeZonesParent == null || markerPrefab == null)
        {
            Debug.LogError("Missing required references!");
            return;
        }
        
        int markersAdded = 0;
        int markersSkipped = 0;
        
        foreach (Transform challengePoint in challengeZonesParent)
        {
            if (!challengePoint.gameObject.activeSelf)
                continue;
                
            if (onlyMissingMarkers)
            {
                ChallengeWorldMarker existing = challengePoint.GetComponentInChildren<ChallengeWorldMarker>();
                if (existing != null)
                {
                    markersSkipped++;
                    continue;
                }
            }
            
            GameObject markerInstance = PrefabUtility.InstantiatePrefab(markerPrefab) as GameObject;
            
            if (markerInstance != null)
            {
                markerInstance.transform.SetParent(challengePoint);
                markerInstance.transform.localPosition = new Vector3(0, markerHeightOffset, 0);
                markerInstance.transform.localRotation = Quaternion.identity;
                markerInstance.name = "WorldMarker";
                
                Undo.RegisterCreatedObjectUndo(markerInstance, "Create Challenge Marker");
                
                markersAdded++;
                
                Debug.Log($"Added marker to: {challengePoint.name}");
            }
        }
        
        EditorUtility.DisplayDialog("Setup Complete", 
            $"Challenge markers setup complete!\n\nAdded: {markersAdded}\nSkipped: {markersSkipped}", 
            "OK");
        
        Debug.Log($"<color=green>✓ Challenge marker setup complete! Added {markersAdded} markers, skipped {markersSkipped}.</color>");
    }
    
    private void RemoveAllMarkers()
    {
        if (challengeZonesParent == null)
        {
            Debug.LogError("Missing ChallengeZones parent!");
            return;
        }
        
        int markersRemoved = 0;
        
        foreach (Transform challengePoint in challengeZonesParent)
        {
            ChallengeWorldMarker[] markers = challengePoint.GetComponentsInChildren<ChallengeWorldMarker>();
            
            foreach (var marker in markers)
            {
                Undo.DestroyObjectImmediate(marker.gameObject);
                markersRemoved++;
            }
        }
        
        Debug.Log($"<color=yellow>Removed {markersRemoved} challenge markers.</color>");
        
        EditorUtility.DisplayDialog("Removal Complete", 
            $"Removed {markersRemoved} challenge markers.", 
            "OK");
    }
}
