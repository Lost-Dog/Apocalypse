using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlowLightingBakeTool
{
    private const string EnvironmentScenePath = "Assets/Scenes/Apocalypse_GC2.unity";
    private const string EffectsScenePath = "Assets/Scenes/Oasis/OasisScene.unity";
    private const string GameplayScenePath = "Assets/Scenes/Gameplay Scene.unity";

    [MenuItem("Tools/Scene Flow/Bake Lighting (Environment + Oasis)")]
    public static void BakeEnvironmentAndOasis()
    {
        if (!EnsureSceneExists(EnvironmentScenePath) || !EnsureSceneExists(EffectsScenePath))
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[SceneFlowLightingBakeTool] Bake canceled.");
            return;
        }

        EditorSceneManager.OpenScene(EnvironmentScenePath, OpenSceneMode.Single);
        EditorSceneManager.OpenScene(EffectsScenePath, OpenSceneMode.Additive);

        // Optional: include gameplay scene if it contains static geometry you want baked.
        if (EnsureSceneExists(GameplayScenePath))
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Additive);
        }

        Scene environmentScene = SceneManager.GetSceneByPath(EnvironmentScenePath);
        if (environmentScene.IsValid())
        {
            SceneManager.SetActiveScene(environmentScene);
        }

        Lightmapping.BakeAsync();
        Debug.Log("[SceneFlowLightingBakeTool] Started async bake with Environment active and Oasis loaded additively.");
    }

    private static bool EnsureSceneExists(string scenePath)
    {
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("[SceneFlowLightingBakeTool] Missing scene: " + scenePath);
            return false;
        }

        return true;
    }
}
