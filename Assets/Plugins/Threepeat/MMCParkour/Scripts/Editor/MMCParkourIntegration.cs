using MxM;
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ThreepeatEditor
{

    //[CreateAssetMenu(fileName = "MMCParkourIntegration", menuName = "Threepeat/Parkour Integration SO")]
    public class MMCParkourIntegration : MMCIntegrationBase
    {
        public override string GetDescription()
        {
            return "Adds Threepeat's Motion-Matching Parkour Controller to MMLC";
        }

        public override string GetIntegrationName()
        {
            return "Motion-Matching Parkour Controller (MMLC Parkour)";
        }

        public override string GetHelpLink()
        {
            return "https://threepeatgames.com/mmc_parkour";
        }

        public override bool IsMajorIntegration()
        {
            return true;
        }

        public override void OnIntegrationEnable()
        {
            base.OnIntegrationEnable();
            /*
            MMCConfigWizardWindow wiz = MMCConfigWizardWindow.GetWindowInstance();

            MMCStopgapAnimDataSelectorPlugin ads = (MMCStopgapAnimDataSelectorPlugin)wiz.GetAvailableIntegration(typeof(MMCStopgapAnimDataSelectorPlugin));

            if (ads == null)
            {
                Debug.Log("Couldn't get MMCStopgapAnimationDataSelectorPlugin");
            }
            else
            {
                Debug.Log("Got it!");
                ads.parkourRequired = true;
            }*/

        }


        protected GameObject originalCharacterPrefab;
        protected LayerMask parkourableLayers = new LayerMask();

        public List<ParkourAnimModuleConfig> availableParkourSets = new List<ParkourAnimModuleConfig>();
        public string[] availableParkourSets_GUINames;

        public int selectedParkourSetIndex = 0;

        protected NGParkourSettings overrideParkourSettings;

        public string defaultParkourSettingsGuid;
        public string defaultParkourContainingAnimDataGuid;

        public class AnimSetup
        {
            public MMCAnimDataConfig adc;
            public MMCAnimDataEditorConfig adec;
        }


        protected List<AnimSetup> nonParkourConfigs = new List<AnimSetup>();
        protected List<AnimSetup> parkourConfigs = new List<AnimSetup>();

        public void GatherParkourAnimModuleConfigs()
        {
            availableParkourSets.Clear();
            string[] tmpGuids = AssetDatabase.FindAssets("t:ParkourAnimModuleConfig");
            List<string> gnames = new List<string>();
            for (int ii = 0; ii < tmpGuids.Length; ii++)
            {
                ParkourAnimModuleConfig theAsset =
                        AssetDatabase.LoadAssetAtPath<ParkourAnimModuleConfig>(
                                        AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                if (theAsset.name.Contains("IGNORE_"))
                {
                    continue;
                }
                availableParkourSets.Add(theAsset);
                gnames.Add(theAsset.guiName);
                //Debug.LogFormat("GatherFBXModifiers: found {0}", theAsset.name);
            }

            availableParkourSets_GUINames = gnames.ToArray();
        }

        public override void MakeGUI()
        {
            EditorGUILayout.Space();
            /*originalCharacterPrefab = (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("Original Character Model", "The original character model is required to reprocess/retarget MxM animation data for your character.  This needs to be the original model, not a downstream prefab."),
                    originalCharacterPrefab, typeof(GameObject), false);*/

            LayerMask tempMask = 
                    EditorGUILayout.MaskField(
                            new GUIContent("Parkourable Layers", 
                                    "Which layers should be vaultable, climbable.  If using Final IK w/ Grounder, include the GrounderOnly layer instead of GrounderIgnore to get best hand placement and motion warping."), 
                            UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(parkourableLayers), 
                            UnityEditorInternal.InternalEditorUtility.layers);

            parkourableLayers = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            GatherParkourAnimModuleConfigs();

            selectedParkourSetIndex = EditorGUILayout.Popup("Parkour Animations to use", selectedParkourSetIndex, availableParkourSets_GUINames);

            overrideParkourSettings = (NGParkourSettings)EditorGUILayout.ObjectField(
                    new GUIContent("Override Parkour Settings", "Allows you to select/customize the specific parkour abilities of this character."),
                    overrideParkourSettings, typeof(NGParkourSettings), false);
            EditorGUILayout.Space();
            if (EditorStyles.helpBox.fontSize == 10)
            {
                EditorStyles.helpBox.fontSize = 12;
            }
            EditorGUILayout.HelpBox("1. Add your character's original prefab model to Original Character Model\n2. Select your Parkourable layers (layers that should be treated as obstacles and platforms).\n3. Select your base locomotion animation set with the Stopgap Animation Data Selector, above.\n\nIf a compiled AnimData doesn't exist for the combination of { your base animation set, your character prefab, and parkour animation set }, it will be automatically generated and placed in Plugins/Threepeat/GeneratedData", MessageType.Info);

            /*if (GUILayout.Button("Check for Match"))
            {
                TestMatch();
            }*/
        }

        private void TestMatch()
        {
            MMCConfigWizardWindow wiz = MMCConfigWizardWindow.GetWindowInstance();

            MMCStopgapAnimDataSelectorPlugin ads = (MMCStopgapAnimDataSelectorPlugin)wiz.GetAvailableIntegration(typeof(MMCStopgapAnimDataSelectorPlugin));

            if (ads == null)
            {
                EditorUtility.DisplayDialog("Can't find Stopgap Animation Data Selector", "Please check the console for errors, and if error-free, reimport MMLC.  If issue persists, please reach out on Discord for support.", "Ok");
                return;
            }

            if (!ads.selected || (ads.selectedIndex < 0) || (ads.selectedIndex > ads.animDataConfigs.Count))
            {
                EditorUtility.DisplayDialog("No Base Animation Set selected", "Please select a base animation set in stopgap animation data selector.", "Ok");
                return;
            }

            AnimSetup animSetup = FindExistingAnimData(originalCharacterPrefab, ads.animDataConfigs[ads.selectedIndex]);

            if (animSetup != null)
            {
                // need to generate AnimData for this triple, as one doesn't exist.
                Debug.LogFormat("Found match with ADEC( {0} )!", animSetup.adec.name);
            }

        }

        public AnimSetup FindExistingAnimData(GameObject modelPrefab, MMCAnimDataConfig adc, bool hasParkour = true, bool doMatchModel = false) 
        {
            // Find ADEC/ADC with PAS parkour + modelPrefab + base animation set
            // return null if can't be found.
            nonParkourConfigs.Clear();
            parkourConfigs.Clear();
            string[] tmpGuids = AssetDatabase.FindAssets("t:MMCAnimDataEditorConfig");

            AnimSetup theOne = null;

            for (int ii = 0; ii < tmpGuids.Length; ii++)
            {
                MMCAnimDataEditorConfig theAsset =
                        AssetDatabase.LoadAssetAtPath<MMCAnimDataEditorConfig>(
                                        AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                if (theAsset.name.Contains("IGNORE_"))
                {
                    continue;
                }

                if (theAsset.animDataConfig == null)
                {
                    continue;
                }
                AnimSetup animSetup = new AnimSetup();
                animSetup.adec = theAsset;
                animSetup.adc = theAsset.animDataConfig;

                bool modelMatch = !doMatchModel || ((animSetup.adec.modelPrefab != null) && modelPrefab.name.Equals(animSetup.adec.modelPrefab.name));

                //Debug.LogFormat("checking {0} : ignore ( {1} ), has( {1} )", theAsset.name, ignoreParkour, theAsset.hasParkour);
                if (theAsset.animDataConfig.hasParkour)
                {
                    if (hasParkour && modelMatch && adc.name.Equals(animSetup.adec.baseAnimDataConfigName))
                    {
                        theOne = animSetup;
                    }

                    parkourConfigs.Add(animSetup);
                }
                else
                {
                    Debug.LogFormat("Checking non-parkour: model( {0} ),( {1} ) adcName( {2} ),( {3} )", doMatchModel ? modelPrefab.name : "not-checked", animSetup.adec.modelPrefab.name, adc.name, animSetup.adec.animDataConfig.name);
                    if (!hasParkour && modelMatch && adc.name.Equals(animSetup.adec.animDataConfig.name))
                    {
                        theOne = animSetup;
                    }

                    nonParkourConfigs.Add(animSetup);
                }

            }
            return theOne;
        }

        public override bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject = null)
        {
            if (!selected)
            {
                return true;
            }

            MMCConfigWizardWindow wiz = MMCConfigWizardWindow.GetWindowInstance();

            MMCStopgapAnimDataSelectorPlugin ads = (MMCStopgapAnimDataSelectorPlugin)wiz.GetAvailableIntegration(typeof(MMCStopgapAnimDataSelectorPlugin));

            if (ads == null)
            {
                EditorUtility.DisplayDialog("Can't find Stopgap Animation Data Selector", "Please check the console for errors, and if error-free, reimport MMLC.  If issue persists, please reach out on Discord for support.", "Ok");
                return false;
            }

            if (!ads.selected || (ads.selectedIndex < 0) || (ads.selectedIndex >= ads.animDataConfigs.Count))
            {
                EditorUtility.DisplayDialog("No Base Animation Set selected", "Please select a base animation set in stopgap animation data selector.", "Ok");
                return false;
            }
            MxMAnimator mxma = modelObject.GetComponent<MxMAnimator>();

            // Parkour Ability Component
            NGAbilityParkour parkour = rootObject.GetComponent<NGAbilityParkour>();

            if (parkour == null)
            {
                parkour = rootObject.AddComponent<NGAbilityParkour>();
            }

            if (overrideParkourSettings != null)
            {
                parkour.config = overrideParkourSettings;
            }
            else
            {
                parkour.config = AssetDatabase.LoadAssetAtPath<NGParkourSettings>(AssetDatabase.GUIDToAssetPath(defaultParkourSettingsGuid));
                if (parkour.config == null)
                {
                    Debug.LogError("MMC Configuration Wizard: Unable to find Parkour Config, please reimport the Parkour Controller asset.");
                }
            }

            parkour.layerMask = parkourableLayers;

            AnimSetup animSetup = FindExistingAnimData(originalCharacterPrefab, ads.animDataConfigs[ads.selectedIndex]);

            if (animSetup == null)
            {

                if ((selectedParkourSetIndex < 0) || (selectedParkourSetIndex >= availableParkourSets.Count))
                {
                    EditorUtility.DisplayDialog("No Parkour Animations selected", "Please select the Parkour Animations set to use.", "Ok");
                    return false;
                }

                ParkourAnimModuleConfig pamConfig = availableParkourSets[selectedParkourSetIndex];

                // need to generate AnimData for this triple, as one doesn't exist.
                EditorUtility.DisplayDialog("Model-specific Animation Set Creation", "An parkour AnimData doesn't exist for this specific model and base animation set.  It will now be created and put into Plugins/Threepeat/GeneratedData", "Ok");

                // Need the base ADEC:
                animSetup = FindExistingAnimData(originalCharacterPrefab, ads.animDataConfigs[ads.selectedIndex], false, false);

                if (animSetup == null) 
                {
                    EditorUtility.DisplayDialog("Couldn't find base configs", "Please reinstall MMLC, parkour controller, and animations assets.  Then check the console for errors.  If issue persists, please reach out on Discord for support.", "Ok");
                    return false;
                }

                MxMAnimData animData = MMCConfigWizardAnimConfigTab.MakeAndBake(pamConfig, animSetup.adec, originalCharacterPrefab, "Assets/Plugins/Threepeat/GeneratedData");

                // Mods to MxMAnimator
                mxma.AnimData = new MxMAnimData[] { animData };

            }
            else
            {
                mxma.AnimData = new MxMAnimData[] { animSetup.adc.animData };
            }

            MMCEventIKManager eventIKMgr = modelObject.GetComponent<MMCEventIKManager>();

            if (eventIKMgr == null)
            {
                eventIKMgr = modelObject.AddComponent<MMCEventIKManager>();
            }

            eventIKMgr.IKEnabled = true;
            eventIKMgr.parkour = parkour;

            Animator animator = modelObject.GetComponent<Animator>();

            if (animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = 
                        AssetDatabase.LoadAssetAtPath<AnimatorController>(
                                AssetDatabase.GUIDToAssetPath("c11e2ecd48cce6549a7c13388c333a5d"));
            }
            else
            {
                AnimatorController rac = (AnimatorController)animator.runtimeAnimatorController;
                bool hasIK = false;
                foreach (AnimatorControllerLayer layer in rac.layers)
                {
                    if (layer.iKPass)
                    {
                        hasIK = true;
                        break;
                    }
                }

                if (!hasIK)
                {
                    EditorUtility.DisplayDialog("IK Pass not enabled", "The Animator Controller in your Character prefab does not have IK pass enabled for any of the layers.  Please make sure to enable it or IK will not work for e.g. Parkour or iStep", "OK");
                }

            }



            return true;
        }
        
    }
}