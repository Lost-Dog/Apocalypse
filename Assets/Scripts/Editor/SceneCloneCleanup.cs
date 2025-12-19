using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public class SceneCloneCleanup : Editor
{
    private const string AUTO_CLEANUP_KEY = "SceneCloneCleanup_AutoCleanup";
    
    static SceneCloneCleanup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (EditorPrefs.GetBool(AUTO_CLEANUP_KEY, true))
            {
                EditorApplication.delayCall += () => CleanupClonesInActiveScene(true);
            }
        }
    }
    
    [MenuItem("Tools/Scene Cleanup/Remove All Clones from Active Scene")]
    public static void CleanupClonesMenuItem()
    {
        CleanupClonesInActiveScene(false);
    }
    
    [MenuItem("Tools/Scene Cleanup/Remove All Clones from All Open Scenes")]
    public static void CleanupClonesAllScenes()
    {
        int totalRemoved = 0;
        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                totalRemoved += CleanupClonesInScene(scene, false);
            }
        }
        
        if (totalRemoved > 0)
        {
            Debug.Log($"<color=green>✓ Scene Cleanup: Removed {totalRemoved} clone objects from all open scenes</color>");
            EditorSceneManager.SaveOpenScenes();
        }
        else
        {
            Debug.Log("<color=cyan>Scene Cleanup: No clone objects found in any open scene</color>");
        }
    }
    
    [MenuItem("Tools/Scene Cleanup/Toggle Auto-Cleanup on Play Mode Exit")]
    public static void ToggleAutoCleanup()
    {
        bool currentValue = EditorPrefs.GetBool(AUTO_CLEANUP_KEY, true);
        EditorPrefs.SetBool(AUTO_CLEANUP_KEY, !currentValue);
        
        string status = !currentValue ? "ENABLED" : "DISABLED";
        Debug.Log($"<color=yellow>Auto-cleanup on Play Mode exit: {status}</color>");
    }
    
    [MenuItem("Tools/Scene Cleanup/Toggle Auto-Cleanup on Play Mode Exit", true)]
    public static bool ToggleAutoCleanupValidate()
    {
        bool isEnabled = EditorPrefs.GetBool(AUTO_CLEANUP_KEY, true);
        Menu.SetChecked("Tools/Scene Cleanup/Toggle Auto-Cleanup on Play Mode Exit", isEnabled);
        return true;
    }
    
    private static void CleanupClonesInActiveScene(bool isAutomatic)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        int removedCount = CleanupClonesInScene(activeScene, isAutomatic);
        
        if (removedCount > 0)
        {
            string prefix = isAutomatic ? "[Auto-Cleanup]" : "";
            Debug.Log($"<color=green>✓ {prefix} Scene Cleanup: Removed {removedCount} clone objects from '{activeScene.name}'</color>");
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
        else if (!isAutomatic)
        {
            Debug.Log($"<color=cyan>Scene Cleanup: No clone objects found in '{activeScene.name}'</color>");
        }
    }
    
    private static int CleanupClonesInScene(Scene scene, bool isAutomatic)
    {
        List<GameObject> clonesToDelete = new List<GameObject>();
        
        GameObject[] rootObjects = scene.GetRootGameObjects();
        
        foreach (GameObject rootObj in rootObjects)
        {
            FindClonesRecursive(rootObj.transform, clonesToDelete);
        }
        
        if (clonesToDelete.Count > 0 && !isAutomatic)
        {
            string objectList = "";
            int maxDisplay = Mathf.Min(clonesToDelete.Count, 10);
            for (int i = 0; i < maxDisplay; i++)
            {
                objectList += $"\n  • {clonesToDelete[i].name}";
            }
            if (clonesToDelete.Count > 10)
            {
                objectList += $"\n  ... and {clonesToDelete.Count - 10} more";
            }
            
            bool confirm = EditorUtility.DisplayDialog(
                "Remove Clone Objects",
                $"Found {clonesToDelete.Count} clone objects in scene '{scene.name}':{objectList}\n\nRemove all these objects?",
                "Remove All",
                "Cancel"
            );
            
            if (!confirm)
            {
                return 0;
            }
        }
        
        foreach (GameObject clone in clonesToDelete)
        {
            if (clone != null)
            {
                DestroyImmediate(clone);
            }
        }
        
        return clonesToDelete.Count;
    }
    
    private static void FindClonesRecursive(Transform parent, List<GameObject> cloneList)
    {
        if (parent.name.Contains("(Clone)"))
        {
            cloneList.Add(parent.gameObject);
            return;
        }
        
        foreach (Transform child in parent)
        {
            FindClonesRecursive(child, cloneList);
        }
    }
}
