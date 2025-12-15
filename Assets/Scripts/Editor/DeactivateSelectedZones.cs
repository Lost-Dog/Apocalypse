using UnityEngine;
using UnityEditor;

public class DeactivateSelectedZones : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Deactivate Selected Mission Zones")]
    public static void DeactivateZones()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select Mission Zone GameObjects in the Hierarchy first.", "OK");
            return;
        }
        
        int zonesDeactivated = 0;
        int nonZonesSkipped = 0;
        
        Undo.RecordObjects(selectedObjects, "Deactivate Mission Zones");
        
        foreach (GameObject obj in selectedObjects)
        {
            MissionZone zone = obj.GetComponent<MissionZone>();
            
            if (zone != null)
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                    zonesDeactivated++;
                    Debug.Log($"Deactivated Mission Zone: {obj.name}");
                }
            }
            else
            {
                nonZonesSkipped++;
            }
        }
        
        string message = $"Mission Zones Deactivated: {zonesDeactivated}\n";
        
        if (nonZonesSkipped > 0)
        {
            message += $"Non-zone objects skipped: {nonZonesSkipped}\n";
        }
        
        message += "\nThese zones will now be automatically managed by ChallengeManager:\n";
        message += "• Activated when challenges spawn\n";
        message += "• Deactivated when challenges complete\n";
        message += "• Dynamically reused for new challenges";
        
        EditorUtility.DisplayDialog("Mission Zones Deactivated", message, "OK");
        
        EditorUtility.SetDirty(Selection.activeGameObject);
    }
    
    [MenuItem("Division Game/Challenge System/Deactivate Selected Mission Zones", true)]
    public static bool ValidateDeactivateZones()
    {
        return Selection.gameObjects.Length > 0;
    }
}
