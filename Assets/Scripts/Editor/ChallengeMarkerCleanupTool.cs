using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChallengeMarkerCleanupTool : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Cleanup Duplicate Markers")]
    public static void ShowWindow()
    {
        ChallengeMarkerCleanupTool window = GetWindow<ChallengeMarkerCleanupTool>("Cleanup Duplicate Markers");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    private Vector2 scrollPosition;
    private Dictionary<Transform, List<GameObject>> duplicateMarkers;
    
    private void OnEnable()
    {
        ScanForDuplicates();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge World Marker Cleanup", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will clean up duplicate WorldMarker instances on ChallengePoints.\n\n" +
            "The challenge system spawns markers dynamically when challenges activate, " +
            "so you don't need static markers in the scene.",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Refresh Scan", GUILayout.Height(30)))
        {
            ScanForDuplicates();
        }
        
        EditorGUILayout.Space(10);
        
        if (duplicateMarkers != null && duplicateMarkers.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {duplicateMarkers.Count} ChallengePoints with markers:", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var kvp in duplicateMarkers)
            {
                Transform challengePoint = kvp.Key;
                List<GameObject> markers = kvp.Value;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{challengePoint.name}: {markers.Count} markers", GUILayout.Width(200));
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = challengePoint.gameObject;
                    EditorGUIUtility.PingObject(challengePoint.gameObject);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Cleanup Options:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Remove ALL WorldMarkers (Recommended)", GUILayout.Height(40)))
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Remove All WorldMarkers",
                    "This will remove ALL WorldMarker instances from ChallengePoints.\n\n" +
                    "The ChallengeManager will spawn markers dynamically when challenges activate.\n\n" +
                    "Continue?",
                    "Yes, Remove All",
                    "Cancel");
                
                if (confirm)
                {
                    RemoveAllWorldMarkers();
                }
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Keep Only One Marker Per ChallengePoint", GUILayout.Height(40)))
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Keep One Marker",
                    "This will remove duplicate WorldMarkers, keeping only the first one in each ChallengePoint.\n\n" +
                    "Note: You'll need to manually wire these to activate with challenges.\n\n" +
                    "Continue?",
                    "Yes, Remove Duplicates",
                    "Cancel");
                
                if (confirm)
                {
                    RemoveDuplicates();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No duplicate WorldMarkers found!", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Additional Actions:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Deactivate All WorldMarkers", GUILayout.Height(30)))
        {
            DeactivateAllWorldMarkers();
        }
    }
    
    private void ScanForDuplicates()
    {
        duplicateMarkers = new Dictionary<Transform, List<GameObject>>();
        
        GameObject challengeZones = GameObject.Find("GameSystems/Zones/ChallengeZones");
        
        if (challengeZones == null)
        {
            Debug.LogWarning("ChallengeZones not found!");
            return;
        }
        
        foreach (Transform challengePoint in challengeZones.transform)
        {
            List<GameObject> markers = new List<GameObject>();
            
            foreach (Transform child in challengePoint)
            {
                if (child.name == "WorldMarker")
                {
                    markers.Add(child.gameObject);
                }
            }
            
            if (markers.Count > 0)
            {
                duplicateMarkers[challengePoint] = markers;
            }
        }
        
        Debug.Log($"Scan complete. Found {duplicateMarkers.Count} ChallengePoints with WorldMarkers.");
    }
    
    private void RemoveAllWorldMarkers()
    {
        int totalRemoved = 0;
        
        foreach (var kvp in duplicateMarkers)
        {
            foreach (GameObject marker in kvp.Value)
            {
                Undo.DestroyObjectImmediate(marker);
                totalRemoved++;
            }
        }
        
        Debug.Log($"<color=green>✓ Removed {totalRemoved} WorldMarker instances!</color>");
        
        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"Removed {totalRemoved} WorldMarker instances.\n\n" +
            "The ChallengeManager will now spawn markers dynamically when challenges activate.",
            "OK");
        
        ScanForDuplicates();
    }
    
    private void RemoveDuplicates()
    {
        int totalRemoved = 0;
        int totalKept = 0;
        
        foreach (var kvp in duplicateMarkers)
        {
            List<GameObject> markers = kvp.Value;
            
            for (int i = 1; i < markers.Count; i++)
            {
                Undo.DestroyObjectImmediate(markers[i]);
                totalRemoved++;
            }
            
            if (markers.Count > 0)
            {
                markers[0].SetActive(false);
                totalKept++;
            }
        }
        
        Debug.Log($"<color=green>✓ Removed {totalRemoved} duplicate WorldMarkers! Kept {totalKept} markers (deactivated).</color>");
        
        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"Removed {totalRemoved} duplicate WorldMarkers.\n" +
            $"Kept {totalKept} markers (deactivated).\n\n" +
            "Note: You'll need to activate these markers when challenges spawn.",
            "OK");
        
        ScanForDuplicates();
    }
    
    private void DeactivateAllWorldMarkers()
    {
        int totalDeactivated = 0;
        
        foreach (var kvp in duplicateMarkers)
        {
            foreach (GameObject marker in kvp.Value)
            {
                if (marker.activeSelf)
                {
                    Undo.RecordObject(marker, "Deactivate WorldMarker");
                    marker.SetActive(false);
                    totalDeactivated++;
                }
            }
        }
        
        Debug.Log($"<color=yellow>Deactivated {totalDeactivated} WorldMarker instances.</color>");
        
        EditorUtility.DisplayDialog(
            "Deactivation Complete",
            $"Deactivated {totalDeactivated} WorldMarker instances.",
            "OK");
    }
}
