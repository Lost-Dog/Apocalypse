using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ApocalypseGC2OptimizationTools
{
    private const string TargetSceneName = "Apocalypse_GC2";

    [MenuItem("Tools/Apocalypse/Optimization/Analyze Open Scene")]
    public static void AnalyzeOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int shadowedLights = 0;
        int realtimeLights = 0;
        int mixedLights = 0;
        int bakedLights = 0;
        int directionalLights = 0;
        int pointLights = 0;
        int spotLights = 0;

        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null || light.gameObject.scene != scene) continue;

            if (light.shadows != LightShadows.None) shadowedLights++;

            switch (light.lightmapBakeType)
            {
                case LightmapBakeType.Realtime:
                    realtimeLights++;
                    break;
                case LightmapBakeType.Mixed:
                    mixedLights++;
                    break;
                case LightmapBakeType.Baked:
                    bakedLights++;
                    break;
            }

            switch (light.type)
            {
                case LightType.Directional:
                    directionalLights++;
                    break;
                case LightType.Point:
                    pointLights++;
                    break;
                case LightType.Spot:
                    spotLights++;
                    break;
            }
        }

        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int sceneCameras = CountInScene(cameras, scene);

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int sceneCanvases = CountInScene(canvases, scene);

        MeshRenderer[] meshRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int sceneMeshRenderers = CountInScene(meshRenderers, scene);

        SkinnedMeshRenderer[] skinnedMeshRenderers = Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int sceneSkinnedMeshRenderers = CountInScene(skinnedMeshRenderers, scene);

        Terrain[] terrains = Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int sceneTerrains = CountInScene(terrains, scene);

        StringBuilder sb = new StringBuilder(512);
        sb.AppendLine($"Optimization Report: {scene.name}");
        sb.AppendLine($"Cameras: {sceneCameras}");
        sb.AppendLine($"Canvases: {sceneCanvases}");
        sb.AppendLine($"MeshRenderers: {sceneMeshRenderers}");
        sb.AppendLine($"SkinnedMeshRenderers: {sceneSkinnedMeshRenderers}");
        sb.AppendLine($"Terrains: {sceneTerrains}");
        sb.AppendLine($"Lights: {CountInScene(lights, scene)}");
        sb.AppendLine($"Shadowed lights: {shadowedLights}");
        sb.AppendLine($"Directional lights: {directionalLights}");
        sb.AppendLine($"Point lights: {pointLights}");
        sb.AppendLine($"Spot lights: {spotLights}");
        sb.AppendLine($"Realtime lights: {realtimeLights}");
        sb.AppendLine($"Mixed lights: {mixedLights}");
        sb.AppendLine($"Baked lights: {bakedLights}");

        Debug.Log(sb.ToString());
    }

    [MenuItem("Tools/Apocalypse/Optimization/Setup Light Shadow Optimizer")]
    public static void SetupLightShadowOptimizer()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        if (scene.name != TargetSceneName)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Different Scene Open",
                $"Active scene is '{scene.name}', not '{TargetSceneName}'. Continue anyway?",
                "Continue",
                "Cancel"
            );

            if (!proceed) return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (cameras.Length > 0) mainCamera = cameras[0];
        }

        if (mainCamera == null)
        {
            Debug.LogError("No camera found in scene to attach LightShadowOptimizer.");
            return;
        }

        Undo.RecordObject(mainCamera.gameObject, "Setup Light Shadow Optimizer");

        LightShadowOptimizer optimizer = mainCamera.GetComponent<LightShadowOptimizer>();
        if (optimizer == null)
        {
            optimizer = Undo.AddComponent<LightShadowOptimizer>(mainCamera.gameObject);
        }

        optimizer.highQualityDistance = 18f;
        optimizer.mediumQualityDistance = 40f;
        optimizer.lowQualityDistance = 75f;
        optimizer.highQualityResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        optimizer.mediumQualityResolution = UnityEngine.Rendering.LightShadowResolution.Low;
        optimizer.lowQualityResolution = UnityEngine.Rendering.LightShadowResolution.Low;
        optimizer.updateInterval = 0.25f;
        optimizer.disableShadowsBeyondMaxDistance = true;
        optimizer.minShadowStateChangeInterval = 1.0f;
        optimizer.allowRuntimeShadowResolutionChanges = false;
        optimizer.disableShadowsBehindCamera = true;
        optimizer.inFrontDotThreshold = 0f;
        optimizer.limitActiveShadowCasters = true;
        optimizer.maxActiveShadowCasters = 6;
        optimizer.optimizeDirectionalLights = false;
        optimizer.autoFindLights = true;

        EditorUtility.SetDirty(mainCamera.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"LightShadowOptimizer is configured on camera '{mainCamera.name}'.");
    }

    [MenuItem("Tools/Apocalypse/Optimization/Convert Non-Directional Soft Shadows To Hard")]
    public static void ConvertSoftToHardShadows()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int changed = 0;

        Undo.SetCurrentGroupName("Convert Soft Shadows To Hard");
        int undoGroup = Undo.GetCurrentGroup();

        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null || light.gameObject.scene != scene) continue;
            if (light.type == LightType.Directional) continue;
            if (light.shadows != LightShadows.Soft) continue;

            Undo.RecordObject(light, "Convert Soft Shadow");
            light.shadows = LightShadows.Hard;
            EditorUtility.SetDirty(light);
            changed++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"Converted {changed} non-directional lights from Soft to Hard shadows in '{scene.name}'.");
    }

    [MenuItem("Tools/Apocalypse/Optimization/Force Non-Directional Lights To No Shadow Or Baked")]
    public static void ForceNonDirectionalLightsNoShadowOrBaked()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No active loaded scene found.");
            return;
        }

        int choice = EditorUtility.DisplayDialogComplex(
            "Non-Directional Light Optimization",
            "Choose how to optimize non-directional lights in the active scene.",
            "No Shadow",
            "Cancel",
            "Baked Only"
        );

        if (choice == 1)
        {
            return;
        }

        bool bakedOnly = choice == 2;
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int changed = 0;

        Undo.SetCurrentGroupName("Force Non-Directional Lights");
        int undoGroup = Undo.GetCurrentGroup();

        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null || light.gameObject.scene != scene) continue;
            if (light.type == LightType.Directional) continue;

            bool needsChange = false;
            if (light.shadows != LightShadows.None)
            {
                needsChange = true;
            }

            if (bakedOnly && light.lightmapBakeType != LightmapBakeType.Baked)
            {
                needsChange = true;
            }

            if (!needsChange) continue;

            Undo.RecordObject(light, "Update Non-Directional Light");
            light.shadows = LightShadows.None;
            if (bakedOnly)
            {
                light.lightmapBakeType = LightmapBakeType.Baked;
            }

            EditorUtility.SetDirty(light);
            changed++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        string modeText = bakedOnly ? "Baked Only" : "No Shadow";
        Debug.Log($"Updated {changed} non-directional lights to {modeText} in '{scene.name}'.");
    }

    private static int CountInScene<T>(T[] items, Scene scene) where T : Component
    {
        int count = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].gameObject.scene == scene)
            {
                count++;
            }
        }

        return count;
    }
}
