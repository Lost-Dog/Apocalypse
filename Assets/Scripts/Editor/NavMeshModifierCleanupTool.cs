using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NavMeshModifierCleanupTool
{
    [MenuItem("Tools/Navigation/Remove NavMeshModifier And Set NotWalkable (Active Scene)")]
    public static void CleanupActiveSceneNavMeshModifiers()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            EditorUtility.DisplayDialog("No Active Scene", "Open a scene before running this tool.", "OK");
            return;
        }

        Type navMeshModifierType = FindNavMeshModifierType();
        if (navMeshModifierType == null)
        {
            EditorUtility.DisplayDialog(
                "NavMeshModifier Not Found",
                "Could not find a NavMeshModifier type in loaded assemblies.",
                "OK");
            return;
        }

        int targetLayer = LayerMask.NameToLayer("NotWalkable");
        if (targetLayer < 0)
        {
            targetLayer = LayerMask.NameToLayer("Not Walkable");
        }

        if (targetLayer < 0)
        {
            EditorUtility.DisplayDialog(
                "Layer Missing",
                "Neither 'NotWalkable' nor 'Not Walkable' layer exists. Create one and run again.",
                "OK");
            return;
        }

        List<Component> modifiers = new List<Component>(256);
        GameObject[] roots = activeScene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Component[] found = root.GetComponentsInChildren(navMeshModifierType, true);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null)
                {
                    modifiers.Add(found[i]);
                }
            }
        }

        if (modifiers.Count == 0)
        {
            EditorUtility.DisplayDialog("Done", "No NavMeshModifier components found in the active scene.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Cleanup NavMeshModifier Components");
        int undoGroup = Undo.GetCurrentGroup();

        int componentsRemoved = 0;
        int layersChanged = 0;

        foreach (Component modifier in modifiers)
        {
            if (modifier == null) continue;

            GameObject go = modifier.gameObject;
            if (go.layer != targetLayer)
            {
                Undo.RecordObject(go, "Set NotWalkable Layer");
                go.layer = targetLayer;
                EditorUtility.SetDirty(go);
                layersChanged++;
            }

            Undo.DestroyObjectImmediate(modifier);
            componentsRemoved++;
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(activeScene);

        Debug.Log($"[NavMeshModifierCleanupTool] Scene '{activeScene.name}': removed {componentsRemoved} NavMeshModifier component(s), set layer on {layersChanged} object(s) to NotWalkable.");
        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"Removed components: {componentsRemoved}\nLayer set to NotWalkable: {layersChanged}",
            "OK");
    }

    private static Type FindNavMeshModifierType()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type[] types;
            try
            {
                types = assemblies[i].GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            if (types == null) continue;

            for (int t = 0; t < types.Length; t++)
            {
                Type type = types[t];
                if (type == null) continue;
                if (type.Name != "NavMeshModifier") continue;
                if (!typeof(Component).IsAssignableFrom(type)) continue;
                return type;
            }
        }

        return null;
    }
}