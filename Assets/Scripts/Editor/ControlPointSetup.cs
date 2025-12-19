using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class ControlPointSetup : EditorWindow
{
    [MenuItem("Tools/Control Points/Disable All Control Points")]
    public static void DisableAllControlPoints()
    {
        if (!Application.isPlaying)
        {
            int count = SetAllControlPointsActiveState(false);
            EditorUtility.DisplayDialog("Control Points Disabled", 
                $"Disabled {count} Control Point GameObjects.\n\nThey will now only activate when ChallengeManager spawns control point challenges.", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Cannot Modify In Play Mode", 
                "Please exit Play mode before modifying Control Point states.", 
                "OK");
        }
    }
    
    [MenuItem("Tools/Control Points/Enable All Control Points")]
    public static void EnableAllControlPoints()
    {
        if (!Application.isPlaying)
        {
            int count = SetAllControlPointsActiveState(true);
            EditorUtility.DisplayDialog("Control Points Enabled", 
                $"Enabled {count} Control Point GameObjects.\n\nWARNING: This will spawn 25 enemies at scene start if they have spawnOnStart=true!", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Cannot Modify In Play Mode", 
                "Please exit Play mode before modifying Control Point states.", 
                "OK");
        }
    }
    
    [MenuItem("Tools/Control Points/Set All Zones To Manager-Controlled")]
    public static void SetAllZonesToManagerControlled()
    {
        if (!Application.isPlaying)
        {
            int count = 0;
            ControlZone[] allZones = FindObjectsByType<ControlZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (ControlZone zone in allZones)
            {
                SerializedObject so = new SerializedObject(zone);
                so.FindProperty("spawnOnStart").boolValue = false;
                so.FindProperty("requiresManagerActivation").boolValue = true;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(zone);
                count++;
            }
            
            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log($"<color=green>✓ Set {count} ControlZones to manager-controlled mode (spawnOnStart=false, requiresManagerActivation=true)</color>");
            }
            
            EditorUtility.DisplayDialog("Zones Updated", 
                $"Set {count} ControlZones to manager-controlled mode.\n\n" +
                "• spawnOnStart = false\n" +
                "• requiresManagerActivation = true\n\n" +
                "Zones will now only spawn when activated by ChallengeManager.", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Cannot Modify In Play Mode", 
                "Please exit Play mode before modifying Control Zone settings.", 
                "OK");
        }
    }
    
    [MenuItem("Tools/Control Points/Show Control Point Status")]
    public static void ShowControlPointStatus()
    {
        GameObject zonesParent = GameObject.Find("GameSystems/Zones/ControlPointZones");
        if (zonesParent == null)
        {
            EditorUtility.DisplayDialog("Not Found", "Could not find GameSystems/Zones/ControlPointZones in the scene.", "OK");
            return;
        }
        
        string report = "CONTROL POINT STATUS:\n\n";
        int activeCount = 0;
        int inactiveCount = 0;
        
        foreach (Transform child in zonesParent.transform)
        {
            if (child.gameObject.activeSelf)
            {
                report += $"✓ {child.name} - ACTIVE\n";
                activeCount++;
            }
            else
            {
                report += $"○ {child.name} - INACTIVE\n";
                inactiveCount++;
            }
        }
        
        report += $"\nTotal: {activeCount + inactiveCount} | Active: {activeCount} | Inactive: {inactiveCount}";
        
        Debug.Log($"<color=cyan>{report}</color>");
        EditorUtility.DisplayDialog("Control Point Status", report, "OK");
    }
    
    private static int SetAllControlPointsActiveState(bool active)
    {
        GameObject zonesParent = GameObject.Find("GameSystems/Zones/ControlPointZones");
        if (zonesParent == null)
        {
            Debug.LogError("Could not find GameSystems/Zones/ControlPointZones in the scene!");
            return 0;
        }
        
        int count = 0;
        foreach (Transform child in zonesParent.transform)
        {
            if (child.name.StartsWith("ControlPoint"))
            {
                Undo.RecordObject(child.gameObject, active ? "Enable Control Point" : "Disable Control Point");
                child.gameObject.SetActive(active);
                count++;
            }
        }
        
        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"<color={( active ? "yellow" : "green")}>Set {count} Control Point GameObjects to {(active ? "ACTIVE" : "INACTIVE")}</color>");
        }
        
        return count;
    }
}
