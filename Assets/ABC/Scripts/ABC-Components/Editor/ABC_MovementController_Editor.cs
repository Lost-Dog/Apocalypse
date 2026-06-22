using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace ABCToolkit {
    [CustomEditor(typeof(ABC_MovementController))]


    public class ABC_MovementController_Editor : Editor {


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


        #endregion

        ABC_MovementController movementManager;
        SerializedObject GetTarget;


        void OnEnable() {
            movementManager = (ABC_MovementController)target;
            GetTarget = new SerializedObject(movementManager);
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


            EditorGUILayout.Space();

            if (GUILayout.Button("Movement Manager")) {
                // add standard defaults here
                var window = EditorWindow.GetWindow(typeof(ABC_MovementController_EditorWindow), false);
                window.maxSize = new Vector2(837f, 660f);
                window.minSize = window.maxSize;
            }
            InspectorHelpBox("Click the above button to configure movement settings");
            EditorGUILayout.Space();




            EditorGUILayout.Space();


            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();

        }
    }
}