using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System;
using UnityEngine.AI;
using System.Runtime.Remoting.Channels;

namespace ABCToolkit {
    public class ABC_GlobalElement_EditorWindow : EditorWindow {

        public static void ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(ABC_GlobalElement_EditorWindow));
            window.maxSize = new Vector2(windowWidth, windowHeight);
            window.minSize = window.maxSize;
        }



        #region Window Sizes

        static float windowHeight = 600f;
        static float windowWidth = 820f;


        public ABC_GlobalElement.GlobalElementType WindowType = ABC_GlobalElement.GlobalElementType.Abilities;

        public int minimumSectionHeight = 0;
        public int minimumSideBySideSectionWidth = 312;
        public int minimumAllWaySectionWidth = 630;

        public int elementPanelWidth = 130;

        #endregion


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


        public void InspectorBoldVerticleLine() {
            GUI.color = Color.white;
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.Width(1f), GUILayout.ExpandHeight(true) });
            GUI.color = Color.white;


        }


        private ReorderableList reorderableListExportedAbilities;

        public ABC_GlobalElement globElement;
        SerializedObject GetTarget;
        SerializedProperty meWeapon;
        SerializedProperty meAbilityList;
        SerializedProperty meEffectList;
        SerializedProperty meAIRuleList;

        ABC_ControllerAbility_EditorWindow abilityEditor;
        ABC_Controller_EditorWindow contEditor;


        // how many exported abilities in list
        int AbilityCount;
        // the index of the current selected ability in the export list
        int? selectedAbilityIndex;
        // the current selected ability in the global list
        ABC_Ability selectedAbility;
        // scroll position on export list 
        Vector2 listScrollPos;



        public void OnEnable() {


            if (globElement != null) {
                abilityEditor = ScriptableObject.CreateInstance<ABC_ControllerAbility_EditorWindow>();
                contEditor = ScriptableObject.CreateInstance<ABC_Controller_EditorWindow>();


                GetTarget = new SerializedObject(globElement);
                meAbilityList = GetTarget.FindProperty("ElementAbilities"); // Find the List in our script and create a reference of it 
                meEffectList = GetTarget.FindProperty("ElementEffects");
                meWeapon = GetTarget.FindProperty("ElementWeapon");
                meAIRuleList = GetTarget.FindProperty("ElementAIRules");

                contEditor.globElement = this.globElement;
                abilityEditor.globElement = this.globElement;
                abilityEditor.Setup(meAbilityList);
                contEditor.SetupAIRules(meAIRuleList);
                contEditor.SetupAllAbilities();

            }

        }



        void OnGUI() {


            EditorGUIUtility.labelWidth = 140;
            EditorGUIUtility.fieldWidth = 35;

            // keep up to date with count
            AbilityCount = meAbilityList.arraySize;


            //Update our list
            GetTarget.Update();




            if (this.WindowType == ABC_GlobalElement.GlobalElementType.Weapon) {


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(170));
                contEditor.GetWeaponSettingsSelectionGrid();
                EditorGUILayout.EndVertical();
                InspectorBoldVerticleLine();


                listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos,
                                                         false,
                                                         false);


                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }


                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                contEditor.GetWeaponSettings(meWeapon);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();

            }


            if (this.WindowType == ABC_GlobalElement.GlobalElementType.Abilities) {

                abilityEditor.GetAbilitySettings();
            }

            if (this.WindowType == ABC_GlobalElement.GlobalElementType.AIRules) {



                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(170));
                contEditor.GetAIRuleSelectList();
                EditorGUILayout.EndVertical();
                InspectorBoldVerticleLine();

                listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos,
                                 false,
                                 false);

                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }


                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();

                contEditor.GetAIRuleSettings();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndHorizontal();


            }



            if (this.WindowType == ABC_GlobalElement.GlobalElementType.Effect) {

                listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos,
                                             false,
                                             false);

                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                abilityEditor.GetEffectSettings(meEffectList, true);

                EditorGUILayout.EndScrollView();

            }





            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();

        }








        //Target update and applymodifiedproperties are in the inspector update call to reduce lag. 
        public void OnInspectorUpdate() {


            if (globElement != null) {
                //Apply the changes to our list if an update has been made

                //take current state of the SerializedObject, and updates the real object.
                GetTarget.ApplyModifiedProperties();

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(globElement);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTarget.Update();

            }



        }

    }
}