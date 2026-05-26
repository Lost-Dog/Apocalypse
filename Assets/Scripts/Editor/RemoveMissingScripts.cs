using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility that removes MonoBehaviours with missing scripts.
/// Provides two menu items:
///   - Selection + children (scene instances or prefab assets)
///   - All loaded scenes
/// </summary>
public static class RemoveMissingScripts
{
    // ── Selection ─────────────────────────────────────────────────────────────

    [MenuItem("Tools/Remove Missing Scripts/From Selected (Including Children)", priority = 0)]
    public static void RunOnSelection()
    {
        int totalRemoved = 0;
        int goCount      = 0;

        // Selection.objects covers both Project window assets and Hierarchy GameObjects.
        foreach (Object obj in Selection.objects)
        {
            string assetPath  = AssetDatabase.GetAssetPath(obj);
            bool isPrefabAsset = !string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".prefab");

            if (isPrefabAsset)
            {
                // Load an editable prefab instance, clean it, save back.
                GameObject root = PrefabUtility.LoadPrefabContents(assetPath);

                int removed = ProcessRecursive(root, ref goCount);
                totalRemoved += removed;

                if (removed > 0)
                    PrefabUtility.SaveAsPrefabAsset(root, assetPath);

                PrefabUtility.UnloadPrefabContents(root);
            }
            else if (obj is GameObject sceneGo)
            {
                // Scene instance — clean in place and mark dirty.
                int removed = ProcessRecursive(sceneGo, ref goCount);
                totalRemoved += removed;

                if (removed > 0)
                    EditorSceneManager.MarkAllScenesDirty();
            }
        }

        Debug.Log($"[RemoveMissingScripts] Removed {totalRemoved} missing script(s) across {goCount} GameObject(s).");

        EditorUtility.DisplayDialog("Done",
            $"Removed {totalRemoved} missing script(s) across {goCount} GameObject(s).", "OK");
    }

    [MenuItem("Tools/Remove Missing Scripts/From Selected (Including Children)", validate = true)]
    private static bool ValidateRunOnSelection()
    {
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab")) return true;
            if (obj is GameObject) return true;
        }
        return false;
    }

    // ── Full scene ────────────────────────────────────────────────────────────

    [MenuItem("Tools/Remove Missing Scripts/From All Loaded Scenes", priority = 1)]
    public static void RunOnAllScenes()
    {
        int totalRemoved = 0;
        int goCount      = 0;

        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            Scene scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
                totalRemoved += ProcessRecursive(root, ref goCount);
        }

        if (totalRemoved > 0)
            EditorSceneManager.MarkAllScenesDirty();

        Debug.Log($"[RemoveMissingScripts] Removed {totalRemoved} missing script(s) across {goCount} GameObject(s).");

        EditorUtility.DisplayDialog("Done",
            $"Removed {totalRemoved} missing script(s) across {goCount} GameObject(s).", "OK");
    }

    // ── Shared ────────────────────────────────────────────────────────────────

    private static int ProcessRecursive(GameObject go, ref int goCount)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0) goCount++;

        foreach (Transform child in go.transform)
            removed += ProcessRecursive(child.gameObject, ref goCount);

        return removed;
    }
}
