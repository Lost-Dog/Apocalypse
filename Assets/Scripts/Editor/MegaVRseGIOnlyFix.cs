using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MegaVRseGIOnlyFix
{
    private const string TargetName = "--- Megavrse3";

    [MenuItem("Tools/Apocalypse/Lighting/Set Megavrse3 Receive GI Only")]
    public static void SetMegavrse3ReceiveGIOnly()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        int targetObjects = 0;
        int renderersUpdated = 0;

        for (int i = 0; i < roots.Length; i++)
        {
            renderersUpdated += UpdateMatchingHierarchy(roots[i].transform, ref targetObjects);
        }

        if (targetObjects == 0)
        {
            Debug.LogWarning("Could not find object named '--- Megavrse3' in active scene.");
            return;
        }

        if (renderersUpdated > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"Megavrse3 GI update complete. Targets: {targetObjects}, MeshRenderers updated: {renderersUpdated}.");
    }

    private static int UpdateMatchingHierarchy(Transform node, ref int targetObjects)
    {
        int changed = 0;

        if (node.name == TargetName)
        {
            targetObjects++;
            MeshRenderer[] renderers = node.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                if (renderer == null) continue;

                if (renderer.receiveGI != ReceiveGI.Lightmaps)
                {
                    Undo.RecordObject(renderer, "Set Receive GI Only");
                    renderer.receiveGI = ReceiveGI.Lightmaps;
                    EditorUtility.SetDirty(renderer);
                    changed++;
                }
            }
        }

        for (int i = 0; i < node.childCount; i++)
        {
            changed += UpdateMatchingHierarchy(node.GetChild(i), ref targetObjects);
        }

        return changed;
    }
}
