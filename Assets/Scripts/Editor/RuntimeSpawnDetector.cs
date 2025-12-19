using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class RuntimeSpawnDetector
{
    private static int lastKnownPatrolAICount = 0;

    static RuntimeSpawnDetector()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.delayCall += () =>
            {
                lastKnownPatrolAICount = 0;
                EditorApplication.update += CheckForNewPatrolAI;
                Debug.Log("<color=yellow>[RuntimeSpawnDetector] Started monitoring for Patrol AI spawns...</color>");
            };
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= CheckForNewPatrolAI;
            Debug.Log("<color=yellow>[RuntimeSpawnDetector] Stopped monitoring</color>");
        }
    }

    private static void CheckForNewPatrolAI()
    {
        if (!Application.isPlaying) return;

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int currentCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Patrol AI"))
            {
                currentCount++;
                
                if (currentCount > lastKnownPatrolAICount)
                {
                    Debug.LogWarning($"<color=red>[SPAWN DETECTED] New '{obj.name}' spawned! " +
                        $"Parent: {(obj.transform.parent != null ? obj.transform.parent.name : "ROOT")} | " +
                        $"Active: {obj.activeSelf} | " +
                        $"Position: {obj.transform.position}</color>", obj);
                    
                    LogComponentsOnSpawn(obj);
                }
            }
        }

        if (currentCount != lastKnownPatrolAICount)
        {
            Debug.Log($"<color=orange>[RuntimeSpawnDetector] Total Patrol AI count changed: {lastKnownPatrolAICount} → {currentCount}</color>");
            lastKnownPatrolAICount = currentCount;
        }
    }

    private static void LogComponentsOnSpawn(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        string componentList = "";
        foreach (Component comp in components)
        {
            if (comp != null)
                componentList += comp.GetType().Name + ", ";
        }
        Debug.Log($"<color=cyan>[RuntimeSpawnDetector] Components on '{obj.name}': {componentList}</color>");
    }
}
