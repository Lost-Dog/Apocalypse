using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CityBlockCullingSetup
{
    [MenuItem("Tools/Apocalypse/Optimization/Setup City Block Culling (Block4+Block5)")]
    public static void SetupCityBlockCulling()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        SceneActorCullingManager manager = Object.FindFirstObjectByType<SceneActorCullingManager>();
        if (manager == null)
        {
            GameObject go = new GameObject("SceneActorCullingManager");
            Undo.RegisterCreatedObjectUndo(go, "Create SceneActorCullingManager");
            manager = go.AddComponent<SceneActorCullingManager>();
        }

        Undo.RecordObject(manager, "Configure SceneActorCullingManager");
        manager.autoFindPlayerByTag = true;
        manager.playerTag = "Player";
        manager.globalActiveDistance = 120f;
        manager.globalCullDistance = 170f;
        manager.updateInterval = 0.2f;
        manager.maxTargetsPerTick = 256;
        manager.refreshTargetsInterval = 2f;
        EditorUtility.SetDirty(manager);

        GameObject[] roots = scene.GetRootGameObjects();
        int configured = 0;

        for (int i = 0; i < roots.Length; i++)
        {
            configured += ConfigureBlockRecursive(roots[i].transform);
        }

        if (configured == 0)
        {
            Debug.LogWarning("No GameObjects named with Block4/Block5 were found in the active scene.");
        }
        else
        {
            Debug.Log($"Configured per-building city culling on {configured} Block4/Block5 child objects.");
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    private static int ConfigureBlockRecursive(Transform root)
    {
        int configured = 0;

        if (root != null && IsCityBlockName(root.name))
        {
            // Remove any existing block-level target so the whole chunk doesn't pop.
            SceneActorCullingTarget blockTarget = root.GetComponent<SceneActorCullingTarget>();
            if (blockTarget != null)
            {
                Undo.DestroyObjectImmediate(blockTarget);
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (!HasRenderableGeometry(child))
                {
                    continue;
                }

                SceneActorCullingTarget target = child.GetComponent<SceneActorCullingTarget>();
                if (target == null)
                {
                    target = Undo.AddComponent<SceneActorCullingTarget>(child.gameObject);
                }

                Undo.RecordObject(target, "Configure SceneActorCullingTarget");
                target.overrideDistances = true;
                target.activeDistance = 135f;
                target.cullDistance = 185f;
                target.distanceBias = 0f;
                target.ignoreIfTaggedPlayer = false;

                target.manageAnimator = false;
                target.manageNavMeshAgent = false;
                target.manageAIBehaviours = false;
                target.manageRenderers = true;
                target.manageColliders = false;

                target.AutoFillStaticChunkComponents();
                target.CacheOriginalState();

                EditorUtility.SetDirty(target);
                configured++;
            }
        }

        if (root == null)
        {
            return configured;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            configured += ConfigureBlockRecursive(root.GetChild(i));
        }

        return configured;
    }

    private static bool HasRenderableGeometry(Transform node)
    {
        if (node == null)
        {
            return false;
        }

        Renderer[] renderers = node.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (renderer is ParticleSystemRenderer)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool IsCityBlockName(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;

        string lower = value.ToLowerInvariant();
        return lower.Contains("block4") || lower.Contains("block 4") || lower.Contains("block5") || lower.Contains("block 5");
    }
}
