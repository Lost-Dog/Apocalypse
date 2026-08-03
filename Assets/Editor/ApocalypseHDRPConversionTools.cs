using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class ApocalypseHDRPConversionTools
{
    private const string AssetFolder = "Assets/HDRPDefaultResources";
    private const string PipelineAssetPath = AssetFolder + "/ApocalypseHDRPAsset.asset";
    private const string ReportPath = "Assets/HDRPConversionReport.txt";
    private const string DefaultUrpAssetPath = "Assets/Settings/PC_High.asset";

    private static readonly string[] QualityUrpAssetPaths =
    {
        "Assets/URPDefaultResources/Very Low.asset",
        "Assets/URPDefaultResources/Low.asset",
        "Assets/URPDefaultResources/Medium.asset",
        "Assets/URPDefaultResources/High.asset",
        "Assets/URPDefaultResources/Very High.asset",
        "Assets/URPDefaultResources/High.asset"
    };

    [InitializeOnLoadMethod]
    private static void ScheduleFailedConversionRecovery()
    {
        EditorApplication.delayCall += () =>
        {
            if (GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset)
            {
                RestoreUrpNow();
            }
        };
    }

    [MenuItem("Tools/Apocalypse/Rendering/Convert Project to HDRP")]
    public static void ConvertProjectWithConfirmation()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Convert Project to HDRP",
            "This creates and assigns an HDRP asset for every quality level. " +
            "It does not rewrite third-party or custom shaders. Continue?",
            "Convert",
            "Cancel");

        if (confirmed) ConvertProject();
    }

    public static void ConvertProject()
    {
        try
        {
            string report = BuildMaterialReport(out int urpMaterialCount);
            File.WriteAllText(ReportPath, report, Encoding.UTF8);
            AssetDatabase.ImportAsset(ReportPath, ImportAssetOptions.ForceUpdate);

            if (urpMaterialCount > 0)
            {
                string message =
                    $"HDRP switch cancelled because {urpMaterialCount} URP materials still require " +
                    $"replacement or vendor conversion. Review {ReportPath}.";
                Debug.LogError(message);
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("HDRP Conversion Blocked", message, "OK");
                }
                return;
            }

            HDRenderPipelineAsset pipelineAsset = GetOrCreatePipelineAsset();
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            AssignPipelineToAllQualityLevels(pipelineAsset);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"HDRP setup complete. Assigned {PipelineAssetPath} globally and to all " +
                $"quality levels. Review {ReportPath} before converting materials.");

            if (!Application.isBatchMode)
            {
                EditorApplication.ExecuteMenuItem("Window/Rendering/HDRP Wizard");
                Selection.activeObject = pipelineAsset;
                EditorGUIUtility.PingObject(pipelineAsset);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (Application.isBatchMode) throw;
        }
    }

    [MenuItem("Tools/Apocalypse/Rendering/Generate HDRP Material Report")]
    public static void GenerateMaterialReport()
    {
        File.WriteAllText(ReportPath, BuildMaterialReport(out _), Encoding.UTF8);
        AssetDatabase.ImportAsset(ReportPath, ImportAssetOptions.ForceUpdate);
        Debug.Log($"HDRP material report written to {ReportPath}.");
    }

    [MenuItem("Tools/Apocalypse/Rendering/Restore URP Now")]
    public static void RestoreUrpNow()
    {
        RenderPipelineAsset defaultAsset =
            AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(DefaultUrpAssetPath);
        if (defaultAsset == null)
        {
            Debug.LogError($"Cannot restore URP: asset not found at {DefaultUrpAssetPath}.");
            return;
        }

        if (QualitySettings.names.Length != QualityUrpAssetPaths.Length)
        {
            Debug.LogError(
                $"Cannot restore URP: expected {QualityUrpAssetPaths.Length} quality levels but " +
                $"found {QualitySettings.names.Length}.");
            return;
        }

        int originalQualityLevel = QualitySettings.GetQualityLevel();
        GraphicsSettings.defaultRenderPipeline = defaultAsset;

        for (int qualityLevel = 0; qualityLevel < QualityUrpAssetPaths.Length; qualityLevel++)
        {
            RenderPipelineAsset qualityAsset =
                AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(QualityUrpAssetPaths[qualityLevel]);
            if (qualityAsset == null)
            {
                Debug.LogError($"Cannot restore URP: asset not found at {QualityUrpAssetPaths[qualityLevel]}.");
                return;
            }

            QualitySettings.SetQualityLevel(qualityLevel, false);
            QualitySettings.renderPipeline = qualityAsset;
        }

        QualitySettings.SetQualityLevel(originalQualityLevel, true);
        AssetDatabase.SaveAssets();
        Debug.Log("URP restored in the running editor.");
    }

    private static HDRenderPipelineAsset GetOrCreatePipelineAsset()
    {
        HDRenderPipelineAsset pipelineAsset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(PipelineAssetPath);
        if (pipelineAsset != null) return pipelineAsset;

        EnsureAssetFolder();
        pipelineAsset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
        pipelineAsset.name = "Apocalypse HDRP Asset";
        AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
        return pipelineAsset;
    }

    private static void EnsureAssetFolder()
    {
        if (!AssetDatabase.IsValidFolder(AssetFolder))
        {
            AssetDatabase.CreateFolder("Assets", "HDRPDefaultResources");
        }
    }

    private static void AssignPipelineToAllQualityLevels(RenderPipelineAsset pipelineAsset)
    {
        int originalQualityLevel = QualitySettings.GetQualityLevel();

        try
        {
            for (int qualityLevel = 0; qualityLevel < QualitySettings.names.Length; qualityLevel++)
            {
                QualitySettings.SetQualityLevel(qualityLevel, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }
        }
        finally
        {
            QualitySettings.SetQualityLevel(originalQualityLevel, false);
        }
    }

    private static string BuildMaterialReport(out int urpMaterialCount)
    {
        var shaderCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var urpMaterials = new List<string>();
        var builtInMaterials = new List<string>();
        var missingShaderMaterials = new List<string>();

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) continue;

            Shader shader = material.shader;
            if (shader == null)
            {
                missingShaderMaterials.Add(path);
                continue;
            }

            string shaderName = shader.name;
            shaderCounts.TryGetValue(shaderName, out int count);
            shaderCounts[shaderName] = count + 1;

            string pipelineTag = material.GetTag("RenderPipeline", false, string.Empty);
            if (pipelineTag.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                shaderName.StartsWith("Universal Render Pipeline/", StringComparison.OrdinalIgnoreCase))
            {
                urpMaterials.Add(path);
            }
            else if (string.IsNullOrEmpty(pipelineTag))
            {
                builtInMaterials.Add(path);
            }
        }

        var report = new StringBuilder(4096);
        report.AppendLine("Apocalypse HDRP Conversion Report");
        report.AppendLine($"Generated: {DateTime.Now:O}");
        report.AppendLine($"Materials scanned: {materialGuids.Length}");
        report.AppendLine($"URP materials requiring replacement or vendor conversion: {urpMaterials.Count}");
        report.AppendLine($"Built-in/custom untagged materials requiring review: {builtInMaterials.Count}");
        report.AppendLine($"Materials with missing shaders: {missingShaderMaterials.Count}");
        report.AppendLine();
        report.AppendLine("Shader usage:");

        foreach (KeyValuePair<string, int> entry in shaderCounts.OrderByDescending(entry => entry.Value))
        {
            report.AppendLine($"{entry.Value,6}  {entry.Key}");
        }

        AppendPaths(report, "URP material paths", urpMaterials);
        AppendPaths(report, "Built-in/custom untagged material paths", builtInMaterials);
        AppendPaths(report, "Missing shader material paths", missingShaderMaterials);
        urpMaterialCount = urpMaterials.Count;
        return report.ToString();
    }

    private static void AppendPaths(StringBuilder report, string heading, List<string> paths)
    {
        report.AppendLine();
        report.AppendLine(heading + ":");
        foreach (string path in paths) report.AppendLine(path);
    }
}