using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Reflection;

namespace ABCToolkit {
    [CustomEditor(typeof(ABC_WeaponPickUp))]
    public class ABC_WeaponPickUp_Editor : Editor {
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





        #region Design Functions
        // ************************* Inspector Design Functions ***********************************


        public void InspectorHeader(string text) {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUI.color = new Color32(168, 189, 240, 215);
            GUI.skin.box.fontSize = 10;
            GUI.skin.box.alignment = TextAnchor.MiddleLeft;
            GUI.skin.box.fontStyle = FontStyle.Bold;
            GUILayout.Box(" " + text, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(19) });
            GUI.color = Color.grey;
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
            GUI.color = Color.white;
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

        public void InspectorSectionHeader(string text, string description = "") {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            GUIStyle myStyle = new GUIStyle("Button");
            if (EditorGUIUtility.isProSkin) {
                myStyle.normal.textColor = inspectorSectionHeaderTextProColor;
            } else {
                myStyle.normal.textColor = inspectorSectionHeaderTextColor;
            }
            myStyle.alignment = TextAnchor.MiddleLeft;
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.fontSize = 10;
            myStyle.wordWrap = true;


            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHeaderProColor;
            } else {
                GUI.color = inspectorSectionHeaderColor;
            }
            GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { });

            GUI.color = Color.grey;
            GUILayout.Box(" ", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(0.01f) });


            GUI.color = Color.white;
            GUI.skin.box.normal.textColor = originalTextColor;


            if (description != "")
                InspectorHelpBox(description);
        }

        public void ResetLabelWidth() {

            EditorGUIUtility.labelWidth = 100;

        }




        // Button Icons
        Texture AddIcon;
        Texture RemoveIcon;
        Texture ExportIcon;
        Texture CopyIcon;
        Texture ImportIcon;
        Texture RefreshIcon;



        #endregion



        ABC_WeaponPickUp weaponPickUpComp;
        SerializedObject GetTarget;
        //SerializedProperty GetTargetCurrentSaveMaster;





        void OnEnable() {

            weaponPickUpComp = (ABC_WeaponPickUp)target;
            GetTarget = new SerializedObject(weaponPickUpComp);
            //GetTargetCurrentSaveMaster = GetTarget.FindProperty("currentSaveMaster");

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");
            RefreshIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Refresh");




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
            EditorGUIUtility.fieldWidth = 55;

            //Update our list
            GetTarget.Update();

            EditorGUILayout.Space();


            EditorGUILayout.Space();
            if (GUILayout.Button("Weapon Pick Up Manager")) {
                // add standard defaults here
                var window = EditorWindow.GetWindow(typeof(ABC_WeaponPickUp_EditorWindow), false);
                window.maxSize = new Vector2(845f, 660f);
                window.minSize = window.maxSize;
            }

            //EditorGUILayout.PropertyField(GetTarget.FindProperty("showHelpInformation"), new GUIContent("Show Help Boxes"));
            EditorGUILayout.PropertyField(GetTarget.FindProperty("updateEditorAtRunTime"));

            InspectorHelpBox("Will decrease game performance but update editors at run time");



            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();
        }
    }
}
