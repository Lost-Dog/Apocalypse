using UnityEngine;
using UnityEditor;

public class FixCompassMarkerComponent : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Fix Compass Marker Component")]
    public static void ShowWindow()
    {
        var window = GetWindow<FixCompassMarkerComponent>("Fix Compass Marker");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Challenge Compass Marker Fix", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "The ChallengeCompassMarker component should NOT be on the main compass GameObject. " +
            "It should only be on individual marker child objects that represent challenge locations.",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Scan Compass GameObjects", GUILayout.Height(30)))
        {
            ScanCompassObjects();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Remove Marker Component from Main Compass", GUILayout.Height(30)))
        {
            RemoveMarkerFromCompass();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "If you need challenge markers on the compass, they should be created as separate child GameObjects " +
            "with the ChallengeCompassMarker component.",
            MessageType.Warning);
    }

    private void ScanCompassObjects()
    {
        Debug.Log("=== SCANNING COMPASS OBJECTS ===");

        // Find all compass objects
        string[] compassNames = new string[] 
        { 
            "HUD_Apocalypse_Compass_01", 
            "HUD_Apocalypse_Compass_02", 
            "HUD_Apocalypse_Compass_03" 
        };

        bool foundIssues = false;

        foreach (string compassName in compassNames)
        {
            GameObject compass = GameObject.Find(compassName);
            if (compass == null)
                continue;

            Debug.Log($"Found compass: {compassName}");

            ChallengeCompassMarker marker = compass.GetComponent<ChallengeCompassMarker>();
            if (marker != null)
            {
                Debug.LogError($"❌ ISSUE: ChallengeCompassMarker found on main compass '{compassName}'!", compass);
                foundIssues = true;
            }
            else
            {
                Debug.Log($"✅ No ChallengeCompassMarker on '{compassName}' - Good!");
            }

            // Check children for properly placed markers
            ChallengeCompassMarker[] childMarkers = compass.GetComponentsInChildren<ChallengeCompassMarker>();
            if (childMarkers.Length > 0)
            {
                Debug.Log($"   Found {childMarkers.Length} marker(s) in children - this is correct");
                foreach (var childMarker in childMarkers)
                {
                    if (childMarker.gameObject != compass)
                    {
                        Debug.Log($"   ✅ Marker on child: {childMarker.gameObject.name}");
                    }
                }
            }
        }

        if (!foundIssues)
        {
            Debug.Log("✅ No issues found! All compass objects are configured correctly.");
            EditorUtility.DisplayDialog("Scan Complete", "No issues found! All compass objects are configured correctly.", "OK");
        }
        else
        {
            Debug.LogWarning("⚠️ Issues found! Use 'Remove Marker Component from Main Compass' to fix.");
            EditorUtility.DisplayDialog("Issues Found", "ChallengeCompassMarker component found on main compass GameObject(s).\n\nCheck the Console for details.\n\nClick 'Remove Marker Component from Main Compass' to fix.", "OK");
        }

        Debug.Log("=== SCAN COMPLETE ===");
    }

    private void RemoveMarkerFromCompass()
    {
        string[] compassNames = new string[] 
        { 
            "HUD_Apocalypse_Compass_01", 
            "HUD_Apocalypse_Compass_02", 
            "HUD_Apocalypse_Compass_03" 
        };

        int removedCount = 0;

        foreach (string compassName in compassNames)
        {
            GameObject compass = GameObject.Find(compassName);
            if (compass == null)
                continue;

            ChallengeCompassMarker marker = compass.GetComponent<ChallengeCompassMarker>();
            if (marker != null)
            {
                Debug.Log($"Removing ChallengeCompassMarker from '{compassName}'");
                DestroyImmediate(marker);
                EditorUtility.SetDirty(compass);
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            Debug.Log($"✅ Removed {removedCount} incorrectly placed ChallengeCompassMarker component(s)");
            EditorUtility.DisplayDialog("Success", 
                $"Removed {removedCount} ChallengeCompassMarker component(s) from main compass GameObject(s).\n\n" +
                "Save the scene to keep these changes.", "OK");
        }
        else
        {
            Debug.Log("No ChallengeCompassMarker components found on main compass objects");
            EditorUtility.DisplayDialog("Info", "No ChallengeCompassMarker components found on main compass GameObjects.", "OK");
        }
    }
}
