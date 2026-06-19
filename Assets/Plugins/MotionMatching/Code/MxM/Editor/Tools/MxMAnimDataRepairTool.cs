using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MxM.Editor
{
    public static class MxMAnimDataRepairTool
    {
        private const string MenuRoot = "Tools/MxM/AnimData/";

        [MenuItem(MenuRoot + "Scan For Missing Clips")]
        public static void ScanForMissingClips()
        {
            RunScan(repair: false);
        }

        [MenuItem(MenuRoot + "Repair Missing Clips (One Pass)")]
        public static void RepairMissingClips()
        {
            if (!EditorUtility.DisplayDialog(
                    "Repair MxMAnimData Missing Clips",
                    "This will modify MxMAnimData assets by replacing missing clip slots with a fallback AnimationClip. Continue?",
                    "Repair",
                    "Cancel"))
            {
                return;
            }

            RunScan(repair: true);
        }

        private static void RunScan(bool repair)
        {
            string[] guids = AssetDatabase.FindAssets("t:MxMAnimData");
            int totalAssets = 0;
            int brokenAssets = 0;
            int fixedAssets = 0;
            int fixedSlots = 0;
            int unfixableAssets = 0;

            List<string> reportLines = new List<string>();
            AnimationClip globalFallback = null;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    MxMAnimData animData = AssetDatabase.LoadAssetAtPath<MxMAnimData>(path);
                    if (animData == null)
                    {
                        continue;
                    }

                    totalAssets++;
                    AnimationClip[] clips = animData.Clips;
                    if (clips == null || clips.Length == 0)
                    {
                        continue;
                    }

                    List<int> missingIndices = null;
                    for (int c = 0; c < clips.Length; c++)
                    {
                        if (clips[c] == null)
                        {
                            if (missingIndices == null)
                            {
                                missingIndices = new List<int>();
                            }

                            missingIndices.Add(c);
                        }
                    }

                    if (missingIndices == null || missingIndices.Count == 0)
                    {
                        continue;
                    }

                    brokenAssets++;
                    reportLines.Add($"BROKEN: {path} | Missing clip slots: {string.Join(",", missingIndices)}");

                    if (!repair)
                    {
                        continue;
                    }

                    AnimationClip replacementClip = FindLocalFallback(clips);
                    if (replacementClip == null)
                    {
                        if (globalFallback == null)
                        {
                            globalFallback = FindGlobalFallback();
                        }

                        replacementClip = globalFallback;
                    }

                    if (replacementClip == null)
                    {
                        unfixableAssets++;
                        reportLines.Add($"UNFIXABLE: {path} | No fallback AnimationClip found in project.");
                        continue;
                    }

                    Undo.RecordObject(animData, "Repair MxMAnimData Missing Clips");
                    for (int m = 0; m < missingIndices.Count; m++)
                    {
                        clips[missingIndices[m]] = replacementClip;
                    }

                    animData.Clips = clips;
                    EditorUtility.SetDirty(animData);

                    fixedAssets++;
                    fixedSlots += missingIndices.Count;
                    reportLines.Add($"FIXED: {path} | Replaced {missingIndices.Count} slots with '{replacementClip.name}'");
                }
            }
            finally
            {
                if (repair)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            string header =
                $"MxM AnimData {(repair ? "repair" : "scan")} complete\n" +
                $"Total MxMAnimData assets: {totalAssets}\n" +
                $"Broken assets: {brokenAssets}\n" +
                (repair
                    ? $"Fixed assets: {fixedAssets}\nFixed clip slots: {fixedSlots}\nUnfixable assets: {unfixableAssets}"
                    : "");

            if (reportLines.Count > 0)
            {
                Debug.Log(header + "\n\n" + string.Join("\n", reportLines));
            }
            else
            {
                Debug.Log(header + "\n\nNo broken clip references detected.");
            }

            EditorUtility.DisplayDialog("MxM AnimData", header + "\n\nDetails are in Console.", "OK");
        }

        private static AnimationClip FindLocalFallback(AnimationClip[] clips)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    return clips[i];
                }
            }

            return null;
        }

        private static AnimationClip FindGlobalFallback()
        {
            string[] clipGuids = AssetDatabase.FindAssets("t:AnimationClip");
            for (int i = 0; i < clipGuids.Length; i++)
            {
                string clipPath = AssetDatabase.GUIDToAssetPath(clipGuids[i]);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }
    }
}
