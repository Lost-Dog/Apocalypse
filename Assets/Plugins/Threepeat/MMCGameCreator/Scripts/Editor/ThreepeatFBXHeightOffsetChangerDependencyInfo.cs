using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    [CreateAssetMenu(fileName = "ThreepeatFBXHeightOffsetChangerDependencyInfo", menuName = "Threepeat/FBX Modifier - Height Offset")]
    public class ThreepeatFBXHeightOffsetChangerDependencyInfo : ThreepeatDependencyInfo
    {
        [Serializable]
        public struct AssetInfo
        {
            public string fbxGuid;
            public string fbxOriginalPath;
        }

        public List<AssetInfo> fbxListToModify = new();
        public float offsetToApply = 0.1f;

        public AnimationClip[] clipsToModify;


        [ContextMenu("Populate this SO with selected model prefabs")]
        public void PopulateThisObjectWithSelection()
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                Debug.LogFormat("{0}", assetPath);

                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);

                AssetInfo theInfo = new();
                theInfo.fbxOriginalPath = assetPath;
                theInfo.fbxGuid = AssetDatabase.AssetPathToGUID(assetPath);

                /*if (importer.motionNodeName.Length > 0)
                {
                    Debug.LogFormat("Clearing RMNode for {0}", importer.name);
                    importer.motionNodeName = "";
                }*/
                this.fbxListToModify.Add(theInfo);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        [ContextMenu("Populate this SO with selected AnimationClips")]
        public void PopulateAnimationClips()
        {
            List<AnimationClip> clipsToModUpdated = new(clipsToModify);

            foreach (UnityEngine.Object obj in Selection.objects)
            {
                AnimationClip clip = obj as AnimationClip;
                Debug.Log($"clip: {clip.name}");
                if (clip != null)
                {
                    clipsToModUpdated.Add(clip);
                }
            }

            clipsToModify = clipsToModUpdated.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public override bool HasOverriddenDependencyCheckFunction()
        {
            return true;
        }

        public override bool OverriddenDependencyCheck_IsDependencyMet()
        {
            return (fbxListToModify.Count == 0) || HasModificationAlreadyBeenApplied();
        }

        // returns null if no custom remediation button and button's GUI text otherwise.
        public override string HasOverriddenRemediationButton()
        {
            return "Modify FBX Prefabs";
        }

        public override void OverriddenRemediate()
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "This will apply a height offset to all the referenced animations, effectively putting the character's feet below ground for all non-GameCreator usage.  Backing up your project first is highly recommended.", "Modify FBX Prefabs for MMLC", "Cancel"))
            {
                ApplyOffset();
            }
        }

        public bool HasModificationAlreadyBeenApplied()
        {
            foreach (AssetInfo info in fbxListToModify)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(info.fbxGuid);

                if (string.IsNullOrEmpty(assetPath))
                {
                    //Debug.LogErrorFormat("Can't find asset by guid.  Original import path is {0}", info.fbxOriginalPath);
                    return false;
                }

                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);

                ModelImporterClipAnimation[] importerAnimationsToUse = importer.clipAnimations;

                if (importer.clipAnimations.Length <= 0)
                {
                    importerAnimationsToUse = importer.defaultClipAnimations;
                }

                foreach (ModelImporterClipAnimation clip in importerAnimationsToUse) //.defaultClipAnimations)
                {
                    if (clip.heightOffset != offsetToApply)
                    {
                        return false;
                    }
                }

            }

            foreach (AnimationClip clip in clipsToModify)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                if (settings.level != offsetToApply)
                {
                    return false;
                }
            }



            return true;
        }

        private void ApplyOffset()
        {
            
            foreach (AssetInfo info in fbxListToModify)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(info.fbxGuid);

                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogErrorFormat("Can't find asset by guid.  Original import path is {0}", info.fbxOriginalPath);
                    return;
                }

                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);

                List<ModelImporterClipAnimation> outClips = new();

                ModelImporterClipAnimation[] importerAnimationsToUse = importer.clipAnimations;

                if (importer.clipAnimations.Length <= 0)
                {
                    importerAnimationsToUse = importer.defaultClipAnimations;
                }

                foreach (ModelImporterClipAnimation clip in importerAnimationsToUse) //.defaultClipAnimations)
                {
                    clip.heightOffset = offsetToApply;
                    outClips.Add(clip);
                }

                importer.clipAnimations = outClips.ToArray();
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            }

            foreach (AnimationClip clip in clipsToModify)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                if (settings.orientationOffsetY != offsetToApply)
                {
                    settings.level = offsetToApply;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }
}