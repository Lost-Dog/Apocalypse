using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace ABCToolkit {
    [CustomEditor(typeof(ABC_ParkourObstacle))]


    public class ABC_ParkourObstacle_Editor : Editor {


        #region Window Colors
        public Color inspectorBackgroundColor = Color.white;
        public Color inspectorBackgroundProColor = new Color32(155, 185, 255, 255);

        public Color inspectorSectionHeaderColor = new Color32(137, 134, 134, 210);
        public Color inspectorSectionHeaderProColor = new Color32(165, 195, 255, 255);

        public Color inspectorSectionHeaderTextColor = new Color(0, 0.45f, 1, 1);
        public Color inspectorSectionHeaderTextProColor = new Color(1, 1, 1, 1f);

        public Color inspectorSectionBoxColor = new Color32(255, 255, 255, 190);
        public Color inspectorSectionBoxProColor = new Color32(0, 0, 0, 255);

        public Color inspectorSectionHelpColor = new Color32(113, 151, 243, 200);
        public Color inspectorSectionHelpProColor = new Color32(215, 235, 255, 255);
        #endregion

        #region Inspector Design Functions


        public void ResetLabelWidth() {

            EditorGUIUtility.labelWidth = 110;

        }
        public void InspectorHelpBox(string text) {
                if (EditorGUIUtility.isProSkin) {
                    GUI.color = inspectorSectionHelpProColor;
                } else {
                    GUI.color = inspectorSectionHelpColor;
                }
                EditorGUILayout.HelpBox(text, MessageType.None, true);
            
            GUI.color = Color.white;
            EditorGUILayout.Space();
        }

        public void InspectorParkourList(SerializedProperty property) {

            EditorGUILayout.BeginVertical("Box");

            SerializedProperty element = property;

            Color originalTextsColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            GUILayout.EndHorizontal();

            GUI.skin.button.normal.textColor = originalTextsColor;

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }


            EditorGUILayout.BeginVertical("Box");



            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PropertyField(element.FindPropertyRelative("parkourAnimation"), new GUIContent("Animation Clip"), GUILayout.MaxWidth(290));
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PropertyField(element.FindPropertyRelative("parkourAnimationVitalPercentagePoint"), new GUIContent("Vital (%) Point"), GUILayout.MaxWidth(290));
            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(element.FindPropertyRelative("enableRootMotion"), new GUIContent("Enable Root Motion at (%)"), GUILayout.MaxWidth(290));
            ResetLabelWidth();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(element.FindPropertyRelative("parkourAnimationSpeed"), new GUIContent("Animation Speed"), GUILayout.MaxWidth(180));
            EditorGUIUtility.labelWidth = 140;
            EditorGUILayout.PropertyField(element.FindPropertyRelative("parkourVitalPointHeightOffset"), new GUIContent("Vital Point Height Offset"), GUILayout.MaxWidth(200));

            if (element.FindPropertyRelative("enableRootMotion").boolValue == true) {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("enableRootMotionPercentage"), new GUIContent("Percentage (%)"), GUILayout.MaxWidth(200));
            }
            GUILayout.EndVertical();

            ResetLabelWidth();

            GUILayout.BeginVertical();
  
            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            //if (GUILayout.Button(UpArrowSymbol.ToString(), GUILayout.Width(40))) {
            //    movementAudioTags.MoveArrayElement(i, i - 1);
            //}
            //if (GUILayout.Button(DownArrowSymbol.ToString(), GUILayout.Width(40))) {
            //    movementAudioTags.MoveArrayElement(i, i + 1);
            //}

            GUI.skin.button.normal.textColor = originalTextsColor;

            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();


            ResetLabelWidth();


            EditorGUILayout.EndVertical();

                

            

            GUI.skin.button.normal.textColor = originalTextsColor;

            EditorGUILayout.EndVertical();


        }


        #endregion

        ABC_ParkourObstacle parkourObstacle;
        SerializedObject GetTarget;


        void OnEnable() {
            parkourObstacle = (ABC_ParkourObstacle)target;
            GetTarget = new SerializedObject(parkourObstacle);
        }



        public override void OnInspectorGUI() {

            if (EditorGUIUtility.isProSkin) {
                GUI.backgroundColor = inspectorBackgroundProColor;
                GUI.contentColor = Color.white;
            } else {
                GUI.backgroundColor = inspectorBackgroundColor;
                GUI.contentColor = Color.black;
            }

            EditorGUIUtility.labelWidth = 190;
            EditorGUIUtility.fieldWidth = 35;

            //Update our list
            GetTarget.Update();

            ResetLabelWidth();


            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(GetTarget.FindProperty("parkourType"), new GUIContent("Parkour Type"), GUILayout.MaxWidth(260));
            InspectorHelpBox("Define what type of Parkour Movement to activate on this obstacle.");
            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.PropertyField(GetTarget.FindProperty("activateWithoutInput"), new GUIContent("Activate Without Input"));
            InspectorHelpBox("If enabled then the parkour will activate without input being required.");


            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.PropertyField(GetTarget.FindProperty("enableDynamicParkour"), new GUIContent("Enable Dynamic Parkour"), GUILayout.MaxWidth(260));
            InspectorHelpBox("If enabled then a dynamic parkour movement can be setup just for this obstacle");
            ResetLabelWidth();

            if (GetTarget.FindProperty("enableDynamicParkour").boolValue == false) {
      
                EditorGUIUtility.labelWidth = 230;
                EditorGUILayout.PropertyField(GetTarget.FindProperty("activateSpecificParkour"), new GUIContent("Activate Specific Parkour Movement"));
                ResetLabelWidth();

                if (GetTarget.FindProperty("activateSpecificParkour").boolValue == true) {
                    EditorGUILayout.PropertyField(GetTarget.FindProperty("specificParkourTag"), new GUIContent("Parkour Tag"));

                }

                InspectorHelpBox("Option to set which Parkour Animation to play by adding a matching Tag which can be defined in the entities ABC Movement Controller. If not enabled a random movement from the type selected will activate.");

            } else {

                InspectorParkourList(GetTarget.FindProperty("dynamicParkourMovement"));

                EditorGUILayout.Space();
            }
            

            EditorGUILayout.Space();


            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();

        }
    }
}