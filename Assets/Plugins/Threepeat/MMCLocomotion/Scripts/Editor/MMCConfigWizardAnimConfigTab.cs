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
using System.IO;

namespace ThreepeatEditor
{
    [System.Serializable]
    public class MMCConfigWizardAnimConfigTab
    {
        [SerializeField] public MxMPreProcessData preprocdata;
        [SerializeField] public string outputFolder = "Assets/Plugins/Threepeat/GeneratedAnimData";

        [SerializeField] public MotionMatchConfigModule configModuleToChange;
        [SerializeField] public GameObject characterModel;
        private MMCConfigWizardWindow configWizardWindow;

        private MMCStopgapAnimDataSelectorPlugin animDataSelector;

        public static MxMAnimData MakeAndBake(ParkourAnimModuleConfig pamConfig, MMCAnimDataEditorConfig baseADEC, GameObject modelPrefab, string outputFolder = "Assets/Plugins/Threepeat/GeneratedData")
        {
            if (AssetDatabase.IsValidFolder(outputFolder))
            {
                Debug.Log("Output Folder: exists");

                if (!EditorUtility.DisplayDialog("Output Folder Exists", "The output folder exists, if files for this config exist, THEY WILL BE OVERWRITTEN, are you sure?", "Yes, Do it.", "Cancel"))
                {
                    Debug.Log("Cancelled.");
                    return null;
                }
            }
            else
            {
                Debug.Log("Output Folder: creating...");
                DirectoryInfo dinfo = Directory.CreateDirectory(outputFolder);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.LogFormat("created folder( {0} )", dinfo?.FullName);
            }


            // BASE_PREPROCESSOR
            //AssetDatabase.LoadAssetAtPath<CalibrationModule>(
            //              AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("CalibrationModule"))); //"b74701d3c2c3f8e40857441fa1cd5975"));
            // BASE_CONFIGMODULE

            string basePreprocessorPath = null; //AssetDatabase.GUIDToAssetPath(basePreprocessorGuid);
            string baseConfigModulePath = null; //AssetDatabase.GUIDToAssetPath(baseConfigModuleGuid);

            /*if (preprocdata != null)
            {
                basePreprocessorPath = AssetDatabase.GetAssetPath(preprocdata);
            }*/

            basePreprocessorPath = AssetDatabase.GetAssetPath(baseADEC.preprocessor);
            baseConfigModulePath = AssetDatabase.GetAssetPath(baseADEC.configModule);


            if (string.IsNullOrEmpty(basePreprocessorPath))
            {
                EditorUtility.DisplayDialog("Can't find Base Preprocessor", "Base preprocessor can't be found (by path), please reinstall MMLC.  If you've already tried that, please contact discord support (discord link available at threepeatgames.com).", "OK");
                return null;
            }

            if (string.IsNullOrEmpty(baseConfigModulePath))
            {
                EditorUtility.DisplayDialog("Can't find Base ConfigModule", "Base ConfigModule can't be found (by path), please reinstall MMLC.  If you've already tried that, please contact discord support (discord link available at threepeatgames.com).", "OK");
                return null;
            }

            //string newFileNamePrefix = modelPrefab.name + "__" + baseADEC.animDataConfig.name + "_" + pamConfig.description;
            string newFileNamePrefix = baseADEC.modelPrefab.name + "__" + baseADEC.animDataConfig.name + "_" + pamConfig.description;

            string newPreprocessorPath = outputFolder + "/" + newFileNamePrefix + "_preprocessor.asset";
            string newConfigModulePath = outputFolder + "/" + newFileNamePrefix + "_configModule.asset";
            string newADECPath = outputFolder + "/" + newFileNamePrefix + "_AnimDataEditorConfig.asset";
            string newADCPath = outputFolder + "/" + newFileNamePrefix + "_AnimDataConfig.asset";
            string newAnimDataPath = outputFolder + "/" + newFileNamePrefix + "_animData.asset";
            //TODO: create ADEC and ADC for the parkour asset.

            AssetDatabase.DeleteAsset(newConfigModulePath);
            AssetDatabase.DeleteAsset(newAnimDataPath);

            if (!AssetDatabase.CopyAsset(baseConfigModulePath, newConfigModulePath))
            {
                EditorUtility.DisplayDialog("Can't copy Config Module", "Attempt to copy Config Module failed, check console for more information.", "OK");
                Debug.LogErrorFormat("Failed to copy Config Module from ( {0} ) to ( {1} )", baseConfigModulePath, newConfigModulePath);
                return null ;
            }

            if (!AssetDatabase.CopyAsset(basePreprocessorPath, newPreprocessorPath))
            {
                EditorUtility.DisplayDialog("Can't copy Preprocessor", "Attempt to copy Preprocessor failed, check console for more information.", "OK");
                Debug.LogErrorFormat("Failed to copy Preprocessor from ( {0} ) to ( {1} )", basePreprocessorPath, newPreprocessorPath);
                return null;
            }


            Debug.Log("Loading Preprocessor...");
            MxMPreProcessData preprocdata = AssetDatabase.LoadAssetAtPath<MxMPreProcessData>(newPreprocessorPath);
            MotionMatchConfigModule configModule = AssetDatabase.LoadAssetAtPath<MotionMatchConfigModule>(newConfigModulePath);

            MMCAnimDataEditorConfig newADEC = GameObject.Instantiate<MMCAnimDataEditorConfig>(baseADEC);
            MMCAnimDataConfig newADC = GameObject.Instantiate<MMCAnimDataConfig>(baseADEC.animDataConfig);

            //newADEC.modelPrefab = modelPrefab;
            newADEC.animDataConfig = newADC;
            newADEC.animModules = null;
            newADEC.baseAnimDataConfigName = baseADEC.animDataConfig.name;
            newADEC.guiName = newFileNamePrefix;
            newADEC.description = newFileNamePrefix;
            newADEC.preprocessor = preprocdata;
            newADEC.configModule = configModule;

            newADC.guiName = newADC.description = newFileNamePrefix;
            newADC.hasParkour = true;
            
            //configModule.Prefab = modelPrefab;
            //preprocdata.Prefab = modelPrefab;
            preprocdata.OverrideConfigModule = configModule;
            

            foreach (AnimationModule mod in pamConfig.animModules)
            {
                preprocdata.AddAnimationModule(mod);
            }

            //execute animdata baking
            newADC.animData = PreProcessAnimationData(preprocdata, newAnimDataPath);

            AssetDatabase.CreateAsset(newADC, newADCPath);
            AssetDatabase.CreateAsset(newADEC, newADECPath);

            AssetDatabase.SaveAssets();

            return newADC.animData;
        }

        public MMCConfigWizardAnimConfigTab(MMCConfigWizardWindow mMCConfigWizardWindow)
        {
            this.configWizardWindow = mMCConfigWizardWindow;
            this.animDataSelector = MMCStopgapAnimDataSelectorPlugin.CreateInstance<MMCStopgapAnimDataSelectorPlugin>(); 
            this.animDataSelector.doEditorConfigs = true;
        }

        private void BakeIt()
        {
            if (animDataSelector.selectedIndex < 0)
            {
                EditorUtility.DisplayDialog("Nothing selected", "Please select an MMCAnimDataEditorConfig", "OK");
                return;
            }

            MMCAnimDataEditorConfig cfg = animDataSelector.animDataEditorConfigs[animDataSelector.selectedIndex];
            //pp.Prefab = cfg.modelPrefab;


            string basePreprocessorGuid = configWizardWindow.GetGuidFromManagerKey("BASE_PREPROCESSOR");
            string baseConfigModuleGuid = configWizardWindow.GetGuidFromManagerKey("BASE_CONFIGMODULE");

            if ((preprocdata == null) && string.IsNullOrEmpty(basePreprocessorGuid))
            {
                EditorUtility.DisplayDialog("Can't find Base Preprocessor", "Base preprocessor can't be found (by GUID), please reinstall MMLC.  If you've already tried that, please contact discord support (discord link available at threepeatgames.com).", "OK");
                return;
            }

            if ((preprocdata == null) && string.IsNullOrEmpty(basePreprocessorGuid))
            {
                EditorUtility.DisplayDialog("Can't find Base ConfigModule", "Base ConfigModule can't be found (by GUID), please reinstall MMLC.  If you've already tried that, please contact discord support (discord link available at threepeatgames.com).", "OK");
                return;
            }

            if (AssetDatabase.IsValidFolder(outputFolder))
            {
                Debug.Log("Output Folder: exists");

                if (!EditorUtility.DisplayDialog("Output Folder Exists", "The output folder exists, are you sure?", "Yes, Do it.", "Cancel"))
                {
                    Debug.Log("Cancelled.");
                    return;
                }
            }
            else
            {
                Debug.Log("Output Folder: creating...");
            }


            // BASE_PREPROCESSOR
            //AssetDatabase.LoadAssetAtPath<CalibrationModule>(
            //              AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("CalibrationModule"))); //"b74701d3c2c3f8e40857441fa1cd5975"));
            // BASE_CONFIGMODULE

            string basePreprocessorPath = AssetDatabase.GUIDToAssetPath(basePreprocessorGuid);
            string baseConfigModulePath = AssetDatabase.GUIDToAssetPath(baseConfigModuleGuid);

            if (preprocdata != null)
            {
                basePreprocessorPath = AssetDatabase.GetAssetPath(preprocdata);
            }

            if ((preprocdata == null) && string.IsNullOrEmpty(basePreprocessorPath))
            {
                EditorUtility.DisplayDialog("Can't find Base Preprocessor", "Base preprocessor can't be found (by path), please reinstall MMLC.  If you've already tried that, please contact discord support (discord link available at threepeatgames.com).", "OK");
                return;
            }

            if ((preprocdata == null) && string.IsNullOrEmpty(baseConfigModulePath))
            {
                EditorUtility.DisplayDialog("Can't find Base ConfigModule", "Base ConfigModule can't be found (by path), please reinstall MMLC.  If you've already tried that, please contact discord support (discord link available at threepeatgames.com).", "OK");
                return;
            }

            string newFileNamePrefix = cfg.modelPrefab.name + "__" + cfg.name;

            string newPreprocessorPath = outputFolder + "/" + newFileNamePrefix + "_preprocessor.asset";
            string newConfigModulePath = outputFolder + "/" + newFileNamePrefix + "_configModule.asset";

            if (!AssetDatabase.CopyAsset(baseConfigModulePath, newConfigModulePath))
            {
                EditorUtility.DisplayDialog("Can't copy Config Module", "Attempt to copy Config Module failed, check console for more information.", "OK");
                Debug.LogErrorFormat("Failed to copy Config Module from ( {0} ) to ( {1} )", baseConfigModulePath, newConfigModulePath);
                return;
            }

            if (!AssetDatabase.CopyAsset(basePreprocessorPath, newPreprocessorPath))
            {
                EditorUtility.DisplayDialog("Can't copy Preprocessor", "Attempt to copy Preprocessor failed, check console for more information.", "OK");
                Debug.LogErrorFormat("Failed to copy Preprocessor from ( {0} ) to ( {1} )", basePreprocessorPath, newPreprocessorPath);
                return;
            }


            if (preprocdata == null)
            {
                Debug.Log("Loading Base Preprocessor...");
                preprocdata = AssetDatabase.LoadAssetAtPath<MxMPreProcessData>(newPreprocessorPath);
            }

            MotionMatchConfigModule configModule = AssetDatabase.LoadAssetAtPath<MotionMatchConfigModule>(newConfigModulePath);

            configModule.Prefab = cfg.modelPrefab;



            AssetDatabase.SaveAssets();
        }


        public void DrawGUI()
        {
            preprocdata = (MxMPreProcessData)EditorGUILayout.ObjectField(
                    new GUIContent("Override Base PreProcessData", "Only set this if you know what it does."), preprocdata, typeof(MxMPreProcessData), false);
            outputFolder = EditorGUILayout.TextField("output folder", outputFolder);

            /*
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
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (animDataSelector == null)
            {
                animDataSelector = MMCStopgapAnimDataSelectorPlugin.CreateInstance<MMCStopgapAnimDataSelectorPlugin>();
                animDataSelector.doEditorConfigs = true;
            }

            if (animDataSelector != null)
            {
                animDataSelector.MakeGUI();

                animDataSelector.selected = true;

                if (GUILayout.Button("Bake it"))
                {
                    BakeIt();
                }
                
                /*if (GUILayout.Button("Change AnimData for all Active MMLC Characters in Scene"))
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
                }*/
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

        // Approval to reproduce and publish this function was given by author, written record available on request.
        public static MxMAnimData PreProcessAnimationData(MxMPreProcessData m_data, string _fileName)
        {
    #if UNITY_2020_2_OR_NEWER
            if (m_data.GenerateModifiedClips)
    #else
                if (m_spGenerateModifiedClipsOnPreProcess.boolValue )
    #endif
            {
                //if (!m_spEmbedAnimClipsInAnimData.boolValue)
                //Directory.CreateDirectory(_fileName);

                m_data.GenerateModifiedAnimations(_fileName);
            }

            MxMPreProcessor preProcessor = new MxMPreProcessor();
            preProcessor.SetupSceneForProcessing(m_data);

            MxMAnimData animData = (MxMAnimData)AssetDatabase.LoadAssetAtPath(_fileName + ".asset", typeof(MxMAnimData));

            bool existing = true;
            if (animData == null)
            {
                animData = ScriptableObject.CreateInstance<MxMAnimData>();
                existing = false;
            }

            preProcessor.PreProcessData(animData);

            MxMAnimData existingData = (MxMAnimData)AssetDatabase.LoadAssetAtPath(_fileName + ".asset", typeof(MxMAnimData));
            List<CalibrationData> copyOverCalibrationData = new List<CalibrationData>();
            if (existingData != null)
            {
                foreach (CalibrationData calibration in existingData.CalibrationSets)
                {
                    copyOverCalibrationData.Add(new CalibrationData(calibration));
                }
            }

            animData.InitializeCalibration(copyOverCalibrationData);

            EditorUtility.SetDirty(animData);

            if (!existing)
            {
                AssetDatabase.CreateAsset(animData, _fileName + ".asset");

    #if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
    #else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animData));
    #endif
            }

    #if UNITY_2020_2_OR_NEWER
            if (m_data.EmbedClips)
    #else
                if (m_spEmbedAnimClipsInAnimData.boolValue)
    #endif
            {
                EditorUtility.DisplayProgressBar("Embeded Clips", "Creating Embeded Clips", 0f);
                List<AnimationClip> baseAnims = new List<AnimationClip>(animData.Clips);

                for (int i = 0; i < baseAnims.Count; ++i)
                {
                    AnimationClip clip = baseAnims[i];

                    EditorUtility.DisplayProgressBar("Embeded Clips", "Creating Embeded Clip: " + clip.name, ((float)i) / ((float)baseAnims.Count));
                    AnimationClip newClip = new AnimationClip();
                    EditorUtility.CopySerialized(clip, newClip);
                    AssetDatabase.AddObjectToAsset(newClip, animData);

    #if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
    #else
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newClip));
    #endif
                    animData.Clips[i] = newClip;
                }
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.SetDirty(animData);

    #if UNITY_2020_2_OR_NEWER
            m_data.LastSavedAnimData = animData;
            EditorUtility.SetDirty(m_data);
#else
                m_spLastCreatedAnimData.objectReferenceValue = animData;
#endif
            return animData;
        }
    }

}

