using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class OriginObjectFinder
{
    [MenuItem("Tools/Debug/List Objects At Origin")]
    public static void ListObjectsAtOrigin()
    {
        var transforms = GetAllSceneTransforms();
        var count = 0;

        foreach (var t in transforms)
        {
            if (t.position == Vector3.zero)
            {
                var sphere = t.GetComponent<SphereCollider>();
                Debug.Log($"[ORIGIN] {GetPath(t)} | SphereCollider: {(sphere ? "YES" : "NO")}", t.gameObject);
                count++;
            }
        }

        Debug.Log($"Found {count} objects at world origin.");
    }

    [MenuItem("Tools/Debug/Remove SphereColliders At Origin")]
    public static void RemoveSphereCollidersAtOrigin()
    {
        var transforms = GetAllSceneTransforms();
        var removed = 0;

        foreach (var t in transforms)
        {
            if (t.position == Vector3.zero)
            {
                var sphere = t.GetComponent<SphereCollider>();
                if (sphere != null)
                {
                    Undo.DestroyObjectImmediate(sphere);
                    removed++;
                    Debug.Log($"Removed SphereCollider from: {GetPath(t)}", t.gameObject);
                }
            }
        }

        Debug.Log($"Removed {removed} SphereCollider components from objects at world origin.");
    }

    [MenuItem("Tools/Debug/Change Selected To Sprite")]
    public static void ChangeSelectedToSprite()
    {
        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        var changed = 0;
        foreach (var go in selected)
        {
            Undo.RegisterFullObjectHierarchyUndo(go, "Change Selected To Sprite");

            var meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Undo.DestroyObjectImmediate(meshRenderer);
            }

            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Undo.DestroyObjectImmediate(meshFilter);
            }

            if (go.GetComponent<SpriteRenderer>() == null)
            {
                Undo.AddComponent<SpriteRenderer>(go);
            }

            changed++;
            Debug.Log($"Changed to SpriteRenderer: {GetPath(go.transform)}", go);
        }

        Debug.Log($"Changed {changed} selected object(s) to SpriteRenderer.");
    }

    private static IEnumerable<Transform> GetAllSceneTransforms()
    {
        var result = new List<Transform>(1024);
        var sceneCount = SceneManager.sceneCount;
        for (var i = 0; i < sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }

            var roots = scene.GetRootGameObjects();
            for (var r = 0; r < roots.Length; r++)
            {
                CollectTransformsRecursive(roots[r].transform, result);
            }
        }

        return result;
    }

    private static void CollectTransformsRecursive(Transform t, List<Transform> list)
    {
        list.Add(t);
        for (var i = 0; i < t.childCount; i++)
        {
            CollectTransformsRecursive(t.GetChild(i), list);
        }
    }

    private static string GetPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
