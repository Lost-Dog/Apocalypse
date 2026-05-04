using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal; 
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace ABCToolkit {
    [CustomEditor(typeof(ABC_GlobalElement))]

    public class ABC_GlobalElement_Editor : Editor {

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
            myStyle.fontSize = 13;
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

        public void InspectorHorizontalSpace(int width) {
            EditorGUILayout.LabelField("", GUILayout.Width(width));
        }


        // Button Icons
        Texture AddIcon;
        Texture RemoveIcon;
        Texture ExportIcon;
        Texture CopyIcon;
        Texture ImportIcon;


        #endregion

        private ReorderableList reorderableListGlobalAbilities;

        // our exported ability script
        ABC_GlobalElement globElement;
        SerializedObject GetTarget;
        SerializedProperty meAbilityList;
        SerializedProperty meEffectList;

        ABC_ControllerAbility_EditorWindow abilityEditor;



        // how many exported abilities in list
        int AbilityCount;
        // the index of the current selected ability in the export list
        int? selectedAbilityIndex = null;
        // the current selected ability in the global list
        ABC_Ability selectedAbility;

        // scroll position on export list 
        Vector2 abilityScrollPos;






        void OnEnable() {


            abilityEditor = ScriptableObject.CreateInstance<ABC_ControllerAbility_EditorWindow>();


            globElement = (ABC_GlobalElement)target;
            GetTarget = new SerializedObject(globElement);
            meAbilityList = GetTarget.FindProperty("ElementAbilities"); // Find the List in our script and create a reference of it 
            meEffectList = GetTarget.FindProperty("ElementEffects");

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");

            char StarSymbol = '\u2605';

            #region reorderable list
            // ********************************** ReorderableList Ability *******************************************

            // reorderable list for abilities
            reorderableListGlobalAbilities = new ReorderableList(serializedObject,
                                                                   meAbilityList,
                                                                   false, false, false, false);


            //reorderableListExportedAbilities.drawHeaderCallback = (Rect rect) => {
            //    EditorGUI.LabelField(rect, "Abilities:" + AbilityCount);
            //};

            // design of the reorderable list (with rect parameters we show in a line: active, name, recast time and key) have also added space for user to be able to select line
            reorderableListGlobalAbilities.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) => {
                if (index < AbilityCount) {
                    ABC_Ability ability = globElement.ElementAbilities[index];

                    string name = "";


                    if (ability.globalElementSource != null || ability.globalAbilities != null) {
                        name += StarSymbol.ToString();
                    }


                    if (ability.globalAbilities != null)
                        name = name + ability.globalAbilities.name;
                    else
                        name = name + ability.name;

                    rect.y += 2;

                    EditorGUIUtility.labelWidth = 360f;

                    EditorGUI.PrefixLabel(
                        new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight),
                        1, new GUIContent(name));
                }

            };

            // when we select any of the list then it will set the current ability to show the ability details ready to be changed
            reorderableListGlobalAbilities.onSelectCallback = (ReorderableList l) => {
                selectedAbilityIndex = l.index;
                selectedAbility = globElement.ElementAbilities[Convert.ToInt32(selectedAbilityIndex)];

            };

            #endregion

        }



        // ********************************** GUI Layout *******************************************

        public override void OnInspectorGUI() {

            if (EditorGUIUtility.isProSkin) {
                GUI.backgroundColor = inspectorBackgroundProColor;
                GUI.contentColor = Color.white;
            } else {
                GUI.backgroundColor = inspectorBackgroundColor;
                GUI.contentColor = Color.black;
            }

            EditorGUIUtility.labelWidth = 140;
            EditorGUIUtility.fieldWidth = 35;

            // keep up to date with count
            AbilityCount = meAbilityList.arraySize;


            //Update our list
            GetTarget.Update();


            EditorGUILayout.Space();


            EditorGUILayout.Space();

            InspectorSectionHeader("Global Element Details");


            EditorGUILayout.Space();

            if (globElement.elementType == ABC_GlobalElement.GlobalElementType.Weapon) {
                EditorGUILayout.PropertyField(GetTarget.FindProperty("showWeaponPreview"));
            }

            if (globElement.elementType != ABC_GlobalElement.GlobalElementType.Weapon || globElement.elementType == ABC_GlobalElement.GlobalElementType.Weapon && GetTarget.FindProperty("showWeaponPreview").boolValue == false) {
                EditorGUILayout.PropertyField(GetTarget.FindProperty("elementIcon"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(GetTarget.FindProperty("elementDescription"), GUILayout.MinHeight(50));
            EditorGUILayout.PropertyField(GetTarget.FindProperty("elementTags"));

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Created By: " + GetTarget.FindProperty("createdBy").stringValue);
            EditorGUILayout.LabelField("Creation Date: " + GetTarget.FindProperty("creationDate").stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();


            EditorGUILayout.HelpBox("It is recommend to refresh ID's after duplicating a Global Element and before applying.", MessageType.Info);
            if (GUILayout.Button("Refresh Unique IDs", GUILayout.Width(150)) && EditorUtility.DisplayDialog("Refresh Unique IDs", "This will refresh ID's for all Abilities and Weapons. Are you sure you want to continue?", "Yes", "No")) {

                this.globElement.RefreshUniqueIDs();

            }


            EditorGUILayout.Space();


            if (globElement.elementType == ABC_GlobalElement.GlobalElementType.Weapon) {
                InspectorSectionHeader("Weapon");

                EditorGUILayout.Space();

                if (GUILayout.Button("Modify Weapon")) {
                    // add standard defaults here
                    ABC_GlobalElement_EditorWindow window = (ABC_GlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_GlobalElement_EditorWindow), false);
                    window.globElement = globElement;
                    window.WindowType = ABC_GlobalElement.GlobalElementType.Weapon;
                    GUIContent titleContent = new GUIContent(globElement.name + " - Modify Weapon");
                    window.titleContent = titleContent;
                    window.OnEnable();
                    window.maxSize = new Vector2(870f, 660f);
                    window.minSize = window.maxSize;
                }

                InspectorHelpBox("Below shows the exported Weapon.");



                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Weapon Name: " + globElement.ElementWeapon.weaponName);
                EditorGUILayout.Space();
                GUILayout.Label("Description: " + globElement.ElementWeapon.weaponDescription);
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                ABC_Controller.Weapon.WeaponObj weaponGraphic = globElement.ElementWeapon.weaponGraphics.FirstOrDefault();


                if (weaponGraphic != null && weaponGraphic.weaponObjMainGraphic.GameObject != null) {
                    Texture2D getImage = UnityEditor.AssetPreview.GetAssetPreview(weaponGraphic.weaponObjMainGraphic.GameObject);

                    GUI.contentColor = Color.white;

                    GUILayout.Label(getImage, GUILayout.Height(75));
   

                    if (EditorGUIUtility.isProSkin == false) {
                        GUI.backgroundColor = inspectorBackgroundColor;
                        GUI.contentColor = Color.black;
                    }

                }
                EditorGUILayout.EndHorizontal();


            }

            EditorGUILayout.Space();

            if (globElement.elementType == ABC_GlobalElement.GlobalElementType.Weapon || globElement.elementType == ABC_GlobalElement.GlobalElementType.Abilities) {

                InspectorSectionHeader("Abilities");

                EditorGUILayout.Space();

                InspectorHelpBox("Below shows a list of the Abilities included in this element.");

                if (GUILayout.Button("Modify Abilities")) {
                    // add standard defaults here
                    ABC_GlobalElement_EditorWindow window = (ABC_GlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_GlobalElement_EditorWindow), false);
                    window.globElement = globElement;
                    window.WindowType = ABC_GlobalElement.GlobalElementType.Abilities;
                    GUIContent titleContent = new GUIContent(globElement.name + " - Modify Abilities");
                    window.titleContent = titleContent;
                    window.OnEnable();
                    window.maxSize = new Vector2(1025f, 660f);
                    window.minSize = window.maxSize;
                }


                if (GUILayout.Button("Modify AI Rules")) {
                    // add standard defaults here
                    ABC_GlobalElement_EditorWindow window = (ABC_GlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_GlobalElement_EditorWindow), false);
                    window.globElement = globElement;
                    window.WindowType = ABC_GlobalElement.GlobalElementType.AIRules;
                    GUIContent titleContent = new GUIContent(globElement.name + " - Modify AI Rules");
                    window.titleContent = titleContent;
                    window.OnEnable();
                    window.maxSize = new Vector2(850f, 660f);
                    window.minSize = window.maxSize;
                }



                // if count is over 11 then we will add a scroll view and a max height 
                if (AbilityCount > 11) {
                    EditorGUILayout.BeginVertical("Box", GUILayout.MaxHeight(300));
                    abilityScrollPos = EditorGUILayout.BeginScrollView(abilityScrollPos, false, false);

                } else {
                    EditorGUILayout.BeginVertical("Box");
                }
                // show reordable list defined in on enable
                reorderableListGlobalAbilities.DoLayoutList();
                if (AbilityCount > 11) {
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();


                if (selectedAbilityIndex != null) {

                    Texture2D iconImage = null;

                    if (selectedAbility.globalAbilities != null)
                        iconImage = selectedAbility.globalAbilities.elementIcon;
                    else
                        iconImage = selectedAbility.iconImage.Texture2D;

                    if (iconImage != null) {

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        GUI.contentColor = Color.white;


                        GUILayout.Label(iconImage, GUILayout.Height(150));

                        if (EditorGUIUtility.isProSkin == false) {
                            GUI.backgroundColor = inspectorBackgroundColor;
                            GUI.contentColor = Color.black;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginVertical("Box");




                    EditorGUILayout.Space();
                    GUILayout.Label(selectedAbility.globalAbilities == null ? selectedAbility.description : selectedAbility.globalAbilities.elementDescription);

                    if (selectedAbility.globalAbilities == null) {
                        EditorGUILayout.Space();
                        GUILayout.Label("Type: " + selectedAbility.abilityType.ToString());
                        GUILayout.Label("Starting Position: " + selectedAbility.startingPosition.ToString());
                        GUILayout.Label("Travel Type: " + selectedAbility.travelType.ToString());
                        EditorGUILayout.Space();
                    }


                    EditorGUILayout.EndVertical();

                }


            }


            if (globElement.elementType == ABC_GlobalElement.GlobalElementType.Effect) {

                InspectorSectionHeader("Global Effect");

                InspectorHelpBox("The effects that make up this element are listed below. Modify any of the effects by clicking the 'Modify Effects' Button");

                if (GUILayout.Button("Modify Effect")) {
                    // add standard defaults here
                    ABC_GlobalElement_EditorWindow window = (ABC_GlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_GlobalElement_EditorWindow), false);
                    window.globElement = globElement;
                    window.WindowType = ABC_GlobalElement.GlobalElementType.Effect;
                    GUIContent titleContent = new GUIContent(globElement.name + " - Modify Effects");
                    window.titleContent = titleContent;
                    window.OnEnable();
                    window.maxSize = new Vector2(650f, 660f);
                    window.minSize = window.maxSize;
                }


                EditorGUILayout.Space();

                for (int n = 0; n < meEffectList.arraySize; n++) {

                    EditorGUILayout.Space();

                    SerializedProperty meStateEffect = meEffectList.GetArrayElementAtIndex(n);
                    SerializedProperty effectName = meStateEffect.FindPropertyRelative("effectName");
                    SerializedProperty potency = meStateEffect.FindPropertyRelative("potency");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("◆ " + effectName.stringValue);
                    EditorGUILayout.LabelField(" Potency: " + potency.floatValue.ToString());
                    EditorGUILayout.EndHorizontal();
                }


            }



            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();

        }


    }
}