using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-shot editor utility: removes all MonoBehaviours with missing scripts
/// from every GameObject in all loaded scenes, then deletes itself.
/// </summary>
public static class RemoveMissingScripts
{
    [MenuItem("Tools/Remove All Missing Scripts")]
    public static void Run()
    {
        int totalRemoved = 0;
        int goCount = 0;

        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            Scene scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                totalRemoved += ProcessGameObject(root, ref goCount);
            }
        }

        Debug.Log($"[RemoveMissingScripts] Removed {totalRemoved} missing script(s) across {goCount} GameObject(s).");
        EditorUtility.SetDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[0]);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    private static int ProcessGameObject(GameObject go, ref int goCount)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0) goCount++;

        foreach (Transform child in go.transform)
            removed += ProcessGameObject(child.gameObject, ref goCount);

        return removed;
    }
}
