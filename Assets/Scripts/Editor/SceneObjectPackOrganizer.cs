using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneObjectPackOrganizer
{
    private const string RootName = "__Organized Environment";

    [MenuItem("Tools/Apocalypse/Assets/Organize Scene Objects By Pack")]
    public static void OrganizeSceneObjectsByPack()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        bool proceed = EditorUtility.DisplayDialog(
            "Organize Scene Objects",
            "This will regroup active-scene root objects by source prefab pack and category in the Hierarchy. Continue?",
            "Organize",
            "Cancel"
        );

        if (!proceed)
        {
            return;
        }

        Transform organizedRoot = GetOrCreateRoot(scene, RootName);
        var packParents = new Dictionary<PrefabClassificationUtility.PrefabPack, Transform>();
        var categoryParents = new Dictionary<string, Transform>();

        int movedCount = 0;
        int skippedCount = 0;

        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject root = rootObjects[i];
            if (root == null)
            {
                skippedCount++;
                continue;
            }

            if (root.transform == organizedRoot)
            {
                continue;
            }

            if (!TryResolveClassification(root, out PrefabClassificationUtility.PrefabPack pack, out PrefabClassificationUtility.PrefabCategory category))
            {
                skippedCount++;
                continue;
            }

            Transform parent = GetOrCreateCategoryParent(
                organizedRoot,
                pack,
                category,
                packParents,
                categoryParents
            );

            if (root.transform.parent == parent)
            {
                continue;
            }

            Undo.SetTransformParent(root.transform, parent, "Organize Scene Objects By Pack");
            movedCount++;
        }

        if (movedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        string summary =
            $"Scene object organization complete.\n\nMoved: {movedCount}\nSkipped: {skippedCount}";

        Debug.Log(summary);
        EditorUtility.DisplayDialog("Scene Object Organizer", summary, "OK");
    }

    private static bool TryResolveClassification(
        GameObject root,
        out PrefabClassificationUtility.PrefabPack pack,
        out PrefabClassificationUtility.PrefabCategory category)
    {
        pack = PrefabClassificationUtility.PrefabPack.Unknown;
        category = PrefabClassificationUtility.PrefabCategory.PropsDeco;

        Dictionary<PrefabClassificationUtility.PrefabPack, int> packScores = new Dictionary<PrefabClassificationUtility.PrefabPack, int>();
        Dictionary<PrefabClassificationUtility.PrefabCategory, int> categoryScores = new Dictionary<PrefabClassificationUtility.PrefabCategory, int>();

        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
        AddAssetHit(prefabPath, System.IO.Path.GetFileName(prefabPath), 10, packScores, categoryScores);

        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
            AddAssetHit(assetPath, meshFilter.sharedMesh.name, 3, packScores, categoryScores);
        }

        SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinnedRenderers.Length; i++)
        {
            SkinnedMeshRenderer skinned = skinnedRenderers[i];
            if (skinned == null || skinned.sharedMesh == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(skinned.sharedMesh);
            AddAssetHit(assetPath, skinned.sharedMesh.name, 3, packScores, categoryScores);
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null) continue;

            Material[] materials = renderer.sharedMaterials;
            for (int m = 0; m < materials.Length; m++)
            {
                Material material = materials[m];
                if (material == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(material);
                AddAssetHit(assetPath, material.name, 1, packScores, categoryScores);
            }
        }

        pack = GetHighestPack(packScores);
        if (pack == PrefabClassificationUtility.PrefabPack.Unknown)
        {
            return false;
        }

        category = GetHighestCategory(categoryScores, root.name);
        return true;
    }

    private static void AddAssetHit(
        string assetPath,
        string fallbackName,
        int score,
        Dictionary<PrefabClassificationUtility.PrefabPack, int> packScores,
        Dictionary<PrefabClassificationUtility.PrefabCategory, int> categoryScores)
    {
        PrefabClassificationUtility.PrefabPack pack = PrefabClassificationUtility.IdentifyPack(assetPath);
        if (pack == PrefabClassificationUtility.PrefabPack.Unknown)
        {
            return;
        }

        if (!packScores.ContainsKey(pack))
        {
            packScores[pack] = 0;
        }

        packScores[pack] += score;

        PrefabClassificationUtility.TryInferCategoryFromAssetPath(assetPath, fallbackName, out PrefabClassificationUtility.PrefabCategory category);
        if (!categoryScores.ContainsKey(category))
        {
            categoryScores[category] = 0;
        }

        categoryScores[category] += score;
    }

    private static PrefabClassificationUtility.PrefabPack GetHighestPack(Dictionary<PrefabClassificationUtility.PrefabPack, int> packScores)
    {
        PrefabClassificationUtility.PrefabPack bestPack = PrefabClassificationUtility.PrefabPack.Unknown;
        int bestScore = 0;

        foreach (KeyValuePair<PrefabClassificationUtility.PrefabPack, int> pair in packScores)
        {
            if (pair.Value > bestScore)
            {
                bestPack = pair.Key;
                bestScore = pair.Value;
            }
        }

        return bestPack;
    }

    private static PrefabClassificationUtility.PrefabCategory GetHighestCategory(
        Dictionary<PrefabClassificationUtility.PrefabCategory, int> categoryScores,
        string fallbackName)
    {
        PrefabClassificationUtility.PrefabCategory bestCategory = PrefabClassificationUtility.Classify(fallbackName ?? string.Empty);
        int bestScore = 0;

        foreach (KeyValuePair<PrefabClassificationUtility.PrefabCategory, int> pair in categoryScores)
        {
            if (pair.Value > bestScore)
            {
                bestCategory = pair.Key;
                bestScore = pair.Value;
            }
        }

        return bestCategory;
    }

    private static Transform GetOrCreateRoot(Scene scene, string rootName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && roots[i].name == rootName)
            {
                return roots[i].transform;
            }
        }

        GameObject go = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(go, "Create Organized Environment Root");
        SceneManager.MoveGameObjectToScene(go, scene);
        return go.transform;
    }

    private static Transform GetOrCreateCategoryParent(
        Transform organizedRoot,
        PrefabClassificationUtility.PrefabPack pack,
        PrefabClassificationUtility.PrefabCategory category,
        Dictionary<PrefabClassificationUtility.PrefabPack, Transform> packParents,
        Dictionary<string, Transform> categoryParents)
    {
        if (!packParents.TryGetValue(pack, out Transform packParent) || packParent == null)
        {
            string packName = PrefabClassificationUtility.GetPackDisplayName(pack);
            packParent = GetOrCreateChild(organizedRoot, packName);
            packParents[pack] = packParent;
        }

        string categoryName = PrefabClassificationUtility.GetCategoryFolderName(category);
        string key = PrefabClassificationUtility.GetPackDisplayName(pack) + "/" + categoryName;
        if (!categoryParents.TryGetValue(key, out Transform categoryParent) || categoryParent == null)
        {
            categoryParent = GetOrCreateChild(packParent, categoryName);
            categoryParents[key] = categoryParent;
        }

        return categoryParent;
    }

    private static Transform GetOrCreateChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing;
        }

        GameObject go = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(go, "Create Organizer Group");
        go.transform.SetParent(parent, false);
        return go.transform;
    }
}