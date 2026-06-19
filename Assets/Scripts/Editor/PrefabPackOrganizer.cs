using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PrefabPackOrganizer
{
    private sealed class PackConfig
    {
        public string DisplayName;
        public string RootFolder;
        public string PackLabel;
    }

    private static readonly PackConfig[] Packs =
    {
        new PackConfig
        {
            DisplayName = "CityPack5",
            RootFolder = "Assets/CityPack5/Prefabs",
            PackLabel = "pack-citypack5"
        },
        new PackConfig
        {
            DisplayName = "MegaVRse",
            RootFolder = "Assets/MegaVRse/Prefabs",
            PackLabel = "pack-megavrse"
        },
        new PackConfig
        {
            DisplayName = "MegaVRse 2",
            RootFolder = "Assets/MegaVRse 2/Prefabs",
            PackLabel = "pack-megavrse2"
        }
    };

    [MenuItem("Tools/Apocalypse/Assets/Organize Building Packs (Tag + Group)")]
    public static void OrganizeBuildingPacks()
    {
        bool proceed = EditorUtility.DisplayDialog(
            "Organize Building Packs",
            "This will move prefab assets inside selected pack Prefabs folders into category subfolders and apply labels. Continue?",
            "Organize",
            "Cancel"
        );

        if (!proceed)
        {
            return;
        }

        int movedCount = 0;
        int labeledCount = 0;
        int skippedCount = 0;
        int alreadyOrganizedCount = 0;
        List<string> errors = new List<string>();

        // Ensure destination folders exist and are registered before any move.
        for (int i = 0; i < Packs.Length; i++)
        {
            PackConfig pack = Packs[i];
            if (!AssetDatabase.IsValidFolder(pack.RootFolder))
            {
                continue;
            }

            EnsureCategoryFolders(pack.RootFolder);
        }

        AssetDatabase.Refresh();

        for (int i = 0; i < Packs.Length; i++)
        {
            PackConfig pack = Packs[i];
            if (!AssetDatabase.IsValidFolder(pack.RootFolder))
            {
                continue;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { pack.RootFolder });
            for (int g = 0; g < prefabGuids.Length; g++)
            {
                string guid = prefabGuids[g];
                string oldPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(oldPath) || !oldPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    skippedCount++;
                    continue;
                }

                string fileName = System.IO.Path.GetFileName(oldPath);
                PrefabClassificationUtility.PrefabCategory category = PrefabClassificationUtility.Classify(fileName);
                string categoryFolder = PrefabClassificationUtility.GetCategoryFolderName(category);
                string newPath = $"{pack.RootFolder}/{categoryFolder}/{fileName}";

                if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                {
                    string moveError = AssetDatabase.MoveAsset(oldPath, newPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        errors.Add($"Move failed: {oldPath} -> {newPath} | {moveError}");
                        continue;
                    }

                    movedCount++;
                    oldPath = newPath;
                }
                else
                {
                    alreadyOrganizedCount++;
                }

                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(oldPath);
                if (asset == null)
                {
                    skippedCount++;
                    continue;
                }

                string[] labels =
                {
                    "apocalypse-prefab",
                    pack.PackLabel,
                    "category-" + PrefabClassificationUtility.ToLabelSuffix(category)
                };

                AssetDatabase.SetLabels(asset, labels);
                labeledCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary =
            $"Prefab organization complete.\n\nMoved: {movedCount}\nAlready Organized: {alreadyOrganizedCount}\nLabeled: {labeledCount}\nSkipped: {skippedCount}\nErrors: {errors.Count}";

        if (errors.Count > 0)
        {
            Debug.LogError(summary + "\n" + string.Join("\n", errors));
            EditorUtility.DisplayDialog("Prefab Organizer", summary + "\n\nSee Console for error details.", "OK");
        }
        else
        {
            Debug.Log(summary);
            if (movedCount == 0)
            {
                EditorUtility.DisplayDialog("Prefab Organizer", summary + "\n\nNo files needed moving. The packs are already organized.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Prefab Organizer", summary, "OK");
            }
        }
    }

    private static void EnsureCategoryFolders(string rootFolder)
    {
        CreateFolderIfMissing(rootFolder, "Buildings");
        CreateFolderIfMissing(rootFolder, "Structural Modules");
        CreateFolderIfMissing(rootFolder, "Roads and Overpass");
        CreateFolderIfMissing(rootFolder, "Props and Deco");
    }

    private static void CreateFolderIfMissing(string parent, string child)
    {
        string path = parent + "/" + child;
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        AssetDatabase.CreateFolder(parent, child);
    }

}
