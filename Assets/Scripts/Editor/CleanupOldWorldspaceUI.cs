using UnityEngine;
using UnityEditor;

public class CleanupOldWorldspaceUI : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Cleanup Old WorldSpace UI")]
    public static void ShowWindow()
    {
        if (EditorUtility.DisplayDialog(
            "Cleanup Old WorldSpace UI Components",
            "This will remove the old ChallengeWorldspaceUI components from the Synty HUD elements.\n\n" +
            "These are no longer needed since you're using the new World Space canvas system.\n\n" +
            "This will fix the duplicate marker issue!\n\n" +
            "Continue?",
            "Yes, Clean Up",
            "Cancel"))
        {
            CleanupComponents();
        }
    }
    
    private static void CleanupComponents()
    {
        GameObject worldspaceContainer = GameObject.Find("UI/HUD/WorldSpace");
        
        if (worldspaceContainer == null)
        {
            EditorUtility.DisplayDialog("Not Found", "UI/HUD/WorldSpace container not found in scene!", "OK");
            return;
        }
        
        ChallengeWorldspaceUI[] oldComponents = worldspaceContainer.GetComponentsInChildren<ChallengeWorldspaceUI>(true);
        
        if (oldComponents.Length == 0)
        {
            EditorUtility.DisplayDialog("Already Clean", "No old ChallengeWorldspaceUI components found!", "OK");
            return;
        }
        
        int removedCount = 0;
        
        foreach (ChallengeWorldspaceUI component in oldComponents)
        {
            Undo.DestroyObjectImmediate(component);
            removedCount++;
            Debug.Log($"<color=yellow>Removed ChallengeWorldspaceUI from {component.gameObject.name}</color>");
        }
        
        Debug.Log($"<color=green>✓ Removed {removedCount} old ChallengeWorldspaceUI components!</color>");
        
        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"Successfully removed {removedCount} old ChallengeWorldspaceUI components!\n\n" +
            "The duplicate marker issue should now be fixed.\n\n" +
            "Test in Play Mode to confirm.",
            "OK");
    }
}
