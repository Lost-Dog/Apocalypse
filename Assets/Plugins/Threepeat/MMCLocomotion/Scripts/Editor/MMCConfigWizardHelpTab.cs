using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxMEditor;
using Threepeat;
using System;
using System.Linq;
using MxM;
using UnityEngine.SceneManagement;

namespace ThreepeatEditor
{
    [System.Serializable]
    public class MMCConfigWizardHelpTab
    {
        [SerializeField] public MxMPreProcessData preprocdata;
        [SerializeField] public string filename;

        [SerializeField] public MotionMatchConfigModule configModuleToChange;
        [SerializeField] public GameObject characterModel;
        [SerializeField] public string GUIDToCheck;

        private MMCConfigWizardWindow configWizardWindow;

        private MMCStopgapAnimDataSelectorPlugin animDataSelector;

        public MMCConfigWizardHelpTab(MMCConfigWizardWindow mMCConfigWizardWindow)
        {
            this.configWizardWindow = mMCConfigWizardWindow;
            this.animDataSelector = MMCStopgapAnimDataSelectorPlugin.CreateInstance<MMCStopgapAnimDataSelectorPlugin>();
            this.animDataSelector.doEditorConfigs = true;
        }

        public void DrawGUI()
        {
            /*preprocdata = (MxMPreProcessData)EditorGUILayout.ObjectField("PreProcessData", preprocdata, typeof(MxMPreProcessData), false);
            filename = EditorGUILayout.TextField("output path/filename", filename);

            if (GUILayout.Button("Run PreProcessor as-is"))
            {
                MxMPreprocessFromScript.PreProcessAnimationData(preprocdata, filename);
            }

            configModuleToChange = (MotionMatchConfigModule)EditorGUILayout.ObjectField("ConfigModule", configModuleToChange, typeof(MotionMatchConfigModule), false);
            characterModel = (GameObject)EditorGUILayout.ObjectField("Model", characterModel, typeof(GameObject), false);

            if (GUILayout.Button("Run PreProcessor modifying target prefab"))
            {
                configModuleToChange.Prefab = characterModel;
                EditorUtility.SetDirty(configModuleToChange);
                AssetDatabase.Refresh();
                MxMPreprocessFromScript.PreProcessAnimationData(preprocdata, filename);
            }*/
            
            /*EditorGUILayout.Space();
            EditorGUILayout.Space();


            if (GUILayout.Button("here2"))
            {
                //var myClassType = Type.GetType("MxMAnimator");
                var classType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                            from type in assembly.GetTypes()
                            where type.Name == "MxM.MxMAnimator"
                            select type);
                Debug.LogFormat($"{0}", classType);
            }*/


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Open Online Help (most up to date)"))
            {
                Application.OpenURL("https://threepeatgames.com/mmc_locomotion");
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUIDToCheck = EditorGUILayout.TextField("Guid to find", GUIDToCheck);

            if (GUILayout.Button("Find Guid in Project"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(GUIDToCheck);
                if (String.IsNullOrEmpty(assetPath))
                {
                    Debug.LogFormat("Guid( {0} ) not found in project", GUIDToCheck);
                }
                else
                {
                    Debug.LogFormat("Guid( {0}) -> {1}", GUIDToCheck, assetPath);
                }
            }

            EditorGUILayout.Space();

            if (animDataSelector == null)
            {
                this.animDataSelector = MMCStopgapAnimDataSelectorPlugin.CreateInstance<MMCStopgapAnimDataSelectorPlugin>(); 
                this.animDataSelector.doEditorConfigs = true;
            }

            if (animDataSelector != null)
            {
                animDataSelector.MakeGUI();

                animDataSelector.selected = true;

                if (GUILayout.Button("Change AnimData for all Active MMLC Characters in Scene"))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", "This will replace the animation set in every Active MxMAnimator component in the scene.  Inactive objects will not be modified.", "Do it", "Cancel"))
                    {
                        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

                        foreach (GameObject go in rootObjects)
                        {
                            foreach (MxMAnimator mxma in go.GetComponentsInChildren<MxMAnimator>(false))
                            {
                                if (mxma.isActiveAndEnabled)
                                {
                                    NGCharacter ngchar = mxma.GetComponentInParent<NGCharacter>();
                                    if (ngchar == null)
                                    {
                                        mxma.GetComponent<NGCharacter>();
                                    }

                                    if (ngchar == null)
                                    {
                                        Debug.LogFormat("Couldn't find NGCharacter Component for {0}->{1}, skipping.", go.name, mxma.name);
                                        continue;
                                    }
                                    Debug.LogFormat("changing: {0}->{1}", go.name, mxma.name);
                                    animDataSelector.SetupIntegration(ngchar.gameObject, mxma.gameObject, ngchar);
                                }
                            }
                        }
                    }
                }
            }
            EditorGUILayout.Space();


            
            /*
            if (GUILayout.Button("Add Footstep Events to Movement Animset Pro"))
            {
                FBXAnimationEventInfo mapEventInfo =
                        AssetDatabase.LoadAssetAtPath<FBXAnimationEventInfo>(
                                AssetDatabase.GUIDToAssetPath(
                                    configWizardWindow.GetGuidFromManagerKey("FBXAnimationEventInfo_MovementAnimsetPro")));

                if (mapEventInfo == null)
                {
                    // event infos not found
                    EditorUtility.DisplayDialog("FBXAnimationEventInfo not found", "FBXAnimationEventInfo not found for Movement Animset Pro.  Please reimport the Motion-Matching Locomotion Controller asset.", "OK");
                    return;
                }

                if (EditorUtility.DisplayDialog("Are you sure?", "This will inject Animation Events into Movement Animset Pro FBX Prefabs.  Backing up your project first is highly recommended.", "Add the Events", "Cancel"))
                {
                    Debug.Log("Adding AnimationEvents to Movement Animset Pro.");
                    mapEventInfo.ApplyEvents();
                }
            }*/

            /*if (GUILayout.Button("Remove Root Motion Node from selected prefabs' animations"))
            {
                RemoveRootNodesFromMovementAnimsetPro();
            }*/

            /*if (GUILayout.Button("Testing"))
            {
                EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);

                if (Selection.objects.Length <= 0)
                {
                    return;
                }
                GameObject selectedSceneObject = (GameObject)Selection.objects[0];

                Animator animator = selectedSceneObject.GetComponent<Animator>();

                Transform leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                Selection.objects = new Object[] { leftKnee };
            }*/
        }

        /*private void RemoveRootNodesFromMovementAnimsetPro()
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                Debug.LogFormat("{0}", assetPath);

                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);

                bool changesMade = false;
                List<ModelImporterClipAnimation> outClips = new List<ModelImporterClipAnimation>();


                //Debug.LogFormat("RMNode: {0}, {1}, {2}", importer.motionNodeName, importer.motionNodeName == null, importer.motionNodeName.Equals(""));

                if (importer.motionNodeName.Length > 0)
                {
                    Debug.LogFormat("Clearing RMNode for {0}", importer.name);
                    importer.motionNodeName = "";
                }


                AssetDatabase.WriteImportSettingsIfDirty(assetPath);

            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }*/
    }
}