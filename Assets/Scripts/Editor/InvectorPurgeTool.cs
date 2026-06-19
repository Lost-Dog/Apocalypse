using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InvectorPurgeTool
{
    private static readonly string[] ScenePathSkipTokens =
    {
        "/_Recovery/",
        "/Temp/__Backupscenes/"
    };

    private static readonly string[] InvectorDefineTokens =
    {
        "INVECTOR_BASIC",
        "INVECTOR_MELEE",
        "INVECTOR_SHOOTER",
        "INVECTOR_AI_TEMPLATE"
    };

    [MenuItem("Tools/Project Migration/Purge Invector/Run Full Purge")]
    public static void RunFullPurge()
    {
        int removedPrefabComponents = PurgePrefabs();
        int removedSceneComponents = PurgeScenes();
        int removedDefines = PurgeScriptingDefines();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[InvectorPurgeTool] Complete. Removed {removedPrefabComponents} prefab components, " +
            $"{removedSceneComponents} scene components, and {removedDefines} define entries."
        );
    }

    [MenuItem("Tools/Project Migration/Purge Invector/Purge Prefabs")]
    public static int PurgePrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int removed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject root = PrefabUtility.LoadPrefabContents(path);

                try
                {
                    int localRemoved = PurgeFromHierarchy(root);
                    int remainingMissingScripts = CountMissingScriptsInHierarchy(root);

                    if (remainingMissingScripts > 0)
                    {
                        Debug.LogWarning(
                            $"[InvectorPurgeTool] Skipping save for prefab '{path}' because {remainingMissingScripts} missing scripts remain " +
                            "(likely on immutable nested/model-prefab content)."
                        );
                        continue;
                    }

                    if (localRemoved > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        removed += localRemoved;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        Debug.Log($"[InvectorPurgeTool] Prefab purge removed {removed} components.");
        return removed;
    }

    [MenuItem("Tools/Project Migration/Purge Invector/Purge Scenes")]
    public static int PurgeScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        int removed = 0;

        string activeScenePath = SceneManager.GetActiveScene().path;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

            if (ShouldSkipScene(path))
            {
                Debug.Log($"[InvectorPurgeTool] Skipping scene '{path}' (recovery/temporary scene).");
                continue;
            }

            try
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int localRemoved = 0;

                foreach (GameObject root in scene.GetRootGameObjects())
                    localRemoved += PurgeFromHierarchy(root);

                if (localRemoved > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    removed += localRemoved;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[InvectorPurgeTool] Skipping scene '{path}' because it could not be opened/purged. " +
                    $"This is usually caused by missing optional package assets. Details: {exception.Message}"
                );
            }
        }

        if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);

        Debug.Log($"[InvectorPurgeTool] Scene purge removed {removed} components.");
        return removed;
    }

    private static bool ShouldSkipScene(string path)
    {
        if (string.IsNullOrEmpty(path)) return true;

        string normalized = path.Replace('\\', '/');
        for (int i = 0; i < ScenePathSkipTokens.Length; i++)
        {
            if (normalized.IndexOf(ScenePathSkipTokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    [MenuItem("Tools/Project Migration/Purge Invector/Remove INVECTOR Defines")]
    public static int PurgeScriptingDefines()
    {
        int removedCount = 0;

        foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (group == BuildTargetGroup.Unknown) continue;

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (string.IsNullOrEmpty(defines)) continue;

            List<string> tokens = new List<string>(defines.Split(';'));
            int before = tokens.Count;

            tokens.RemoveAll(t => IsInvectorDefine(t));

            if (tokens.Count != before)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", tokens));
                removedCount += before - tokens.Count;
            }
        }

        Debug.Log($"[InvectorPurgeTool] Removed {removedCount} INVECTOR define entries.");
        return removedCount;
    }

    private static bool IsInvectorDefine(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        string trimmed = token.Trim();
        for (int i = 0; i < InvectorDefineTokens.Length; i++)
        {
            if (string.Equals(trimmed, InvectorDefineTokens[i], StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static int PurgeFromHierarchy(GameObject root)
    {
        int removed = 0;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            removed += RemoveMissingScripts(child.gameObject);
            removed += PurgeComponents(child.gameObject);
        }

        return removed;
    }

    private static int PurgeComponents(GameObject gameObject)
    {
        int removed = 0;
        bool hasMissingScripts = false;

        Component[] components = gameObject.GetComponents<Component>();
        for (int i = components.Length - 1; i >= 0; i--)
        {
            Component component = components[i];
            if (component == null)
            {
                hasMissingScripts = true;
                continue;
            }

            string typeName = component.GetType().FullName;
            if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Invector", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                UnityEngine.Object.DestroyImmediate(component, true);
                removed++;
            }
        }

        if (hasMissingScripts)
            removed += RemoveMissingScripts(gameObject);

        return removed;
    }

    private static int RemoveMissingScripts(GameObject gameObject)
    {
        // Unity's built-in API safely removes missing MonoBehaviour scripts from
        // prefab contents and scene objects without direct m_Component mutation.
        return GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
    }

    private static int CountMissingScriptsInHierarchy(GameObject root)
    {
        int count = 0;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);

        return count;
    }
}
