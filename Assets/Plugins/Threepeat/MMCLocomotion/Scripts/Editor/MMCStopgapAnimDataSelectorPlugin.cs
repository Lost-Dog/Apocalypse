using MxM;
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
#if ENABLE_THREEPEAT_DEV_MODE
    [CreateAssetMenu(fileName = "MMCAnimDataSelectorPlugin", menuName = "Threepeat/Dev/AnimDataSelectorPlugin")]
#endif
    public class MMCStopgapAnimDataSelectorPlugin : MMCIntegrationBase
    {
        public override string GetDescription()
        {
            return "Stop-gap temporary way to select which AnimData to use for your character.";
        }

        
        public MxMAnimData animData;

        public List<MMCAnimDataConfig> animDataConfigs = new List<MMCAnimDataConfig>();
        
        public bool doEditorConfigs = false;
        public List<MMCAnimDataEditorConfig> animDataEditorConfigs = new List<MMCAnimDataEditorConfig>();
        protected string[] animDataGuiNames;

        public int selectedIndex = 0;

        public bool parkourRequired = false;
        public GameObject ifDoingEditorConfigs_modelPrefabForADECCheck = null;

        public override bool IsMajorIntegration()
        {
            return true;
        }

        private bool wasParkourRequired = false;

        private bool ignoreParkour = true;

        public MMCStopgapAnimDataSelectorPlugin(bool doEditorConfigs = false)
        {
            this.doEditorConfigs = doEditorConfigs;
        }

        public void GatherAnimDataConfigs()
        {

            List<string> gnames = new List<string>();

            if (doEditorConfigs)
            {
                animDataEditorConfigs.Clear();
                string[] tmpGuids = AssetDatabase.FindAssets("t:MMCAnimDataEditorConfig");
                Debug.LogFormat("Found {0} Editor Configs", tmpGuids.Length);
                for (int ii = 0; ii < tmpGuids.Length; ii++)
                {
                    MMCAnimDataEditorConfig theAsset =
                            AssetDatabase.LoadAssetAtPath<MMCAnimDataEditorConfig>(
                                            AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                    if (theAsset.name.Contains("IGNORE_"))
                    {
                        continue;
                    }
                    animDataEditorConfigs.Add(theAsset);
                    gnames.Add(theAsset.guiName);
                    //Debug.LogFormat("GatherFBXModifiers: found {0}", theAsset.name);
                }
            }
            else
            {
                animDataConfigs.Clear();
                string[] tmpGuids = AssetDatabase.FindAssets("t:MMCAnimDataConfig");

                for (int ii = 0; ii < tmpGuids.Length; ii++)
                {
                    MMCAnimDataConfig theAsset =
                            AssetDatabase.LoadAssetAtPath<MMCAnimDataConfig>(
                                            AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                    if (theAsset.name.Contains("IGNORE_"))
                    {
                        continue;
                    }
                    //Debug.LogFormat("checking {0} : ignore ( {1} ), has( {1} )", theAsset.name, ignoreParkour, theAsset.hasParkour);
                    if ((parkourRequired && !theAsset.hasParkour) || (ignoreParkour && theAsset.hasParkour))
                    {
                        continue;
                    }

                    animDataConfigs.Add(theAsset);
                    gnames.Add(theAsset.guiName);
                    //Debug.LogFormat("GatherFBXModifiers: found {0}", theAsset.name);
                }
            }
            animDataGuiNames = gnames.ToArray();
        }

        public override void MakeGUI()
        {
            parkourRequired = wasParkourRequired = false;
            ignoreParkour = true;

            if ((wasParkourRequired != parkourRequired) || (animDataConfigs != null) && (animDataConfigs.Count <= 0) && (animDataEditorConfigs.Count <= 0))
            {
                wasParkourRequired = parkourRequired;
                GatherAnimDataConfigs();
            }
            EditorGUILayout.Space();

            /*parkourRequired = EditorGUILayout.BeginToggleGroup(new GUIContent("Only show animation sets with parkour", "Whether to show all animation sets, or only those containing parkour events and configuration"), parkourRequired);
            EditorGUILayout.EndToggleGroup();*/

            selectedIndex = EditorGUILayout.Popup(doEditorConfigs ? "Custom AnimData" : "Override AnimData", selectedIndex, animDataGuiNames);

            EditorGUILayout.Space();

            if (GUILayout.Button("Rescan for Animation Sets"))
            {
                Debug.Log("Rechecking");
                GatherAnimDataConfigs();
            }
            
            EditorGUILayout.Space();

        }

        public override string GetIntegrationName()
        {
            return "Stop-gap Animation Data Selector";
        }

        public override bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject = null)
        {
            if (selected)
            {
                MxMAnimator mxma = modelObject.GetComponent<MxMAnimator>();
                mxma.AnimData = new MxMAnimData[] { animDataConfigs[selectedIndex].animData };

                character.config = animDataConfigs[selectedIndex].config;

                if (animDataConfigs[selectedIndex].calibrationModule != null)
                {
                    mxma.OverrideCalibration = animDataConfigs[selectedIndex].calibrationModule;
                }
            }
            return true;
        }
    }
}