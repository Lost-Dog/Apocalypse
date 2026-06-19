using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEngine;


namespace ThreepeatEditor
{
    [System.Serializable]
    public class MMCStriderIntegration : MMCLegacyIntegrationBase
    {
        [SerializeField]public bool useStrider = true;


        public override bool IsPlaceholder()
        {
            return false;
        }

        public override void MakeGUI()
        {
            EditorGUILayout.Space();
            useStrider = EditorGUILayout.BeginToggleGroup(new GUIContent("Use Strider (highly recommended)", "If enabled, strider allows smooth intermediate speeds and gaits between idle, walk, run, and sprint.  If disabled, MxM's built-in longitudinal error warping will be used to attempt to achieve smooth transitions."), useStrider);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        public override bool SetupCharacter(GameObject coreObject, GameObject modelObject)
        {
            if (!useStrider)
            {
                return true;
            }

            // setup strider
            MMCStriderBiped strider = modelObject.AddComponent<MMCStriderBiped>();
            //TODO: Talk to CptKen to request modifications once all known
            //strider.SetStrideScale(0.7f, 1.5f);
            //strider.setManualRootMotionScaleFix(true);
            strider.MaxSpeed = 3f;
            strider.BaseOffset = 0f;
            strider.DynamicOffset = 0f;
            strider.HipAdjustCutoff = 0.25f;
            strider.HipDamping = 0.75f;
            strider.IndependentPlaybackSpeed = 1f;
            strider.MinPlaybackSpeed = 1f;
            strider.MaxPlaybackSpeed = 1f;
            strider.PlaybackSpeedWeight = 0f;


            SerializedObject so = new SerializedObject(strider);
            so.Update();
            SerializedProperty propmin = so.FindProperty("m_minStrideScale");
            SerializedProperty propmax = so.FindProperty("m_maxStrideScale");
            SerializedProperty propman = so.FindProperty("p_manualRootMotionScaleFix");
            propmin.floatValue = 1.0f; //0.7f;
            propmax.floatValue = 1.5f;
            propman.boolValue = true;
            so.ApplyModifiedProperties();


            return true;
        }
    }
}