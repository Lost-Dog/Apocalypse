using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Reflection;

namespace ABCToolkit {
    public class ABC_ControllerAbility_EditorWindow : EditorWindow {

        public static void ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(ABC_ControllerAbility_EditorWindow));
            window.maxSize = new Vector2(windowWidth, windowHeight);
            window.minSize = window.maxSize;
        }



        #region Window Sizes

        static float windowHeight = 660f;
        static float windowWidth = 1025f;


        //Width of ability list in left side 
        int abilityListWidth = 160;

        //Width of first column in left part of main body 
        int abilityInfoWidth = 170;

        public int minimumSectionHeight = 0;
        public int minimumSideBySideSectionWidth = 312;
        public int minimumAllWaySectionWidth = 628;

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


        #region Design Functions

        public void InspectorToolbarAndAbilityInfo(SerializedProperty AbilityList, ref int toolbarSelection, ref string[] toolbar) {

            //return if no abilities or we just deleted an ability (current index is greater then list count)
            if (AbilityCount == 0 || currentAbilityIndex > Abilities.Count - 1)
                return;


            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }


            EditorGUILayout.BeginVertical("Box");

            GUI.color = Color.white;

            EditorGUILayout.Space();

            toolbarSelection = GUILayout.SelectionGrid(toolbarSelection, toolbar, 1);


            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();




            InspectorHeader(AbilityList.GetArrayElementAtIndex(currentAbilityIndex).FindPropertyRelative("name").stringValue);

            Texture2D iconImage = (Texture2D)AbilityList.GetArrayElementAtIndex(currentAbilityIndex).FindPropertyRelative("iconImage").FindPropertyRelative("refVal").objectReferenceValue;
            string description = AbilityList.GetArrayElementAtIndex(currentAbilityIndex).FindPropertyRelative("description").stringValue;



            if (description == "" || description == " ") {
                description = "Ability Description \n";
            }

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }

            EditorGUILayout.BeginVertical("Box");

            GUI.color = Color.white;

            GUILayout.Label(description, GUILayout.MinWidth(abilityInfoWidth));
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }




            EditorGUILayout.BeginVertical("Box");

            GUI.color = Color.white;

            //uncomment below to modify ID
            //EditorGUILayout.PropertyField(abilityID, GUILayout.MaxWidth(180));

            EditorGUILayout.LabelField("ID: " + abilityID.intValue.ToString(), GUILayout.MaxWidth(abilityInfoWidth));

            if (Application.isPlaying == false && GUILayout.Button(new GUIContent(" Copy ID", CopyIcon, "Copy ID"))) {
                GUIUtility.systemCopyBuffer = abilityID.intValue.ToString();
            }


            if (Application.isPlaying && abilityCont != null && GUILayout.Button("Activate")) {
                abilityCont.TriggerAbility(abilityID.intValue);
            } else if (Application.isPlaying && abilityCont == null && GUILayout.Button("Refresh ABC")) {
                ABC_Utilities.ReloadAllABCEntities();
            }





            //If application is playing then show a disable, enable button to call right method
            if (Application.isPlaying && abilityCont != null) {
                EditorGUILayout.LabelField("Enabled: " + abilityEnabled.boolValue.ToString(), GUILayout.MaxWidth(abilityInfoWidth));

                if (GUILayout.Button((abilityEnabled.boolValue) ? "Disable" : "Enable")) {

                    if (abilityEnabled.boolValue) {
                        abilityCont.DisableAbility(abilityID.intValue);
                    } else {
                        abilityCont.EnableAbility(abilityID.intValue);
                    }
                }


            } else {

                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.PropertyField(abilityEnabled, new GUIContent("Enabled"));
                EditorGUILayout.PropertyField(autoCast, new GUIContent("Auto Cast"));
            }


            EditorGUIUtility.labelWidth = 80;

            if (this.Abilities[currentAbilityIndex].globalElementSource == null && this.Abilities[currentAbilityIndex].globalElementSource == null) {
                EditorGUILayout.PropertyField(AbilityList.GetArrayElementAtIndex(currentAbilityIndex).FindPropertyRelative("enableExport"), new GUIContent("Export"));
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();




            #region Parent Group Assignment / Show Child

            InspectorHelpBox("Parent assignment is for list grouping only and does not affect game mechanics", false, true);

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }

            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            //Assign ability to a parent ability to sit under (will only show child abilities in inspector when parent is selected) used for UI grouping only
            List<string> parentAbilitiesList = new List<string>();
            //Add default none
            parentAbilitiesList.Add("None");
            //Get list of ability names
            parentAbilitiesList.AddRange(this.Abilities.Where(a => a.globalAbilities == null && a.abilityID != abilityID.intValue).Select(item => item.name).ToList());

            if (parentAbilityID.intValue > 0 && this.Abilities.Where(a => a.abilityID == parentAbilityID.intValue).FirstOrDefault() != null) {
                //Track which ability is currently selected as a parent
                EditorGUILayout.LabelField("Parent: " + this.Abilities.Where(a => a.abilityID == parentAbilityID.intValue).FirstOrDefault().name);
            } else {
                EditorGUILayout.LabelField("Parent: None ");
            }

            EditorGUILayout.BeginHorizontal();
            inspectorParentAbilityListChoice.intValue = EditorGUILayout.Popup(inspectorParentAbilityListChoice.intValue, parentAbilitiesList.ToArray(), GUILayout.Width(110));
            if (GUILayout.Button(new GUIContent("Update"))) {
                //Record the ID of the parent if greater then 0 
                if (inspectorParentAbilityListChoice.intValue == 0) {
                    parentAbilityID.intValue = 0;
                } else {
                    parentAbilityID.intValue = this.Abilities.Where(a => a.globalAbilities == null && a.abilityID != abilityID.intValue).ToList()[inspectorParentAbilityListChoice.intValue - 1].abilityID;
                }

                if (abilityCont != null)
                    EditorUtility.SetDirty(abilityCont);
                else
                    EditorUtility.SetDirty(globElement);

                CreateAbilityReorderableList();
            }
            EditorGUILayout.EndHorizontal();





            if (this.Abilities.Where(a => a.parentAbilityID == abilityID.intValue).Count() > 0) {
                if (GUILayout.Button((showChildrenInInspector.boolValue) ? "Hide Child Abilities" : "Show Child Abilities")) {

                    if (showChildrenInInspector.boolValue) {
                        this.Abilities[currentAbilityIndex].showChildrenInInspector = false;
                    } else {
                        this.Abilities[currentAbilityIndex].showChildrenInInspector = true;
                    }

                    if (abilityCont != null)
                        EditorUtility.SetDirty(abilityCont);
                    else
                        EditorUtility.SetDirty(globElement);


                    CreateAbilityReorderableList();
                }

            }



            EditorGUILayout.EndVertical();

            #endregion

            #region Global Element Source

            GUI.color = Color.white;
            if (this.Abilities.Count > 0 && this.Abilities.Count - 1 >= currentAbilityIndex && this.Abilities[currentAbilityIndex].globalElementSource != null) {

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.HelpBox("Ability created from the Global Ability: " + this.Abilities[currentAbilityIndex].globalElementSource.name, MessageType.Warning);

                if (EditorGUIUtility.isProSkin) {
                    GUI.color = inspectorSectionBoxProColor;
                } else {
                    GUI.color = inspectorSectionBoxColor;
                }

                EditorGUILayout.BeginVertical("Box");
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("Load Global Abilities"))) {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(GlobalAbilities[this.Abilities[currentAbilityIndex].globalElementSource]);
                }


                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

            }
            #endregion


            if (this.abilityCont == null || this.abilityCont != null && EditorApplication.isPlaying == false) {
                #region Game Type Update

                InspectorHelpBox("Apply the selected game type to the Ability. This will change the Abilities settings when Update is pressed. ", false, true);


                if (EditorGUIUtility.isProSkin) {
                    GUI.color = inspectorSectionBoxProColor;
                } else {
                    GUI.color = inspectorSectionBoxColor;
                }

                EditorGUILayout.BeginVertical("Box");
                GUI.color = Color.white;

                EditorGUILayout.LabelField("Apply Game Type");

                EditorGUILayout.BeginHorizontal();

                gameTypes = (ABC_GlobalPortal.GameType)EditorGUILayout.EnumPopup("", gameTypes, GUILayout.Width(110));

                if (GUILayout.Button("Update")) {

                    this.Abilities[currentAbilityIndex].ConvertToGameType(gameTypes);


                    if (abilityCont != null)
                        EditorUtility.SetDirty(abilityCont);
                    else
                        EditorUtility.SetDirty(globElement);

                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();

                #endregion
            }



        }

        public void InspectorHeader(string text) {
            EditorGUILayout.Space();
            Color originalTextColor = GUI.skin.button.normal.textColor;


            GUIStyle myStyle = new GUIStyle("Box");
            if (EditorGUIUtility.isProSkin) {
                myStyle.normal.textColor = inspectorSectionHeaderTextProColor;
            } else {
                myStyle.normal.textColor = inspectorSectionHeaderTextColor;
            }
            myStyle.alignment = TextAnchor.MiddleCenter;
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.fontSize = 11;
            myStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHeaderProColor;
            } else {
                GUI.color = inspectorSectionHeaderColor;
            }
            GUI.contentColor = Color.white;
            GUI.skin.box.normal.textColor = Color.white;
            GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.MinHeight(30) });
            GUI.color = Color.white;
            GUI.skin.box.normal.textColor = originalTextColor;
        }

        public void InspectorSectionHeader(string text, string description = "", bool AllWay = false) {
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

            if (AllWay == false)
                GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { GUILayout.MaxWidth(minimumAllWaySectionWidth) });
            else
                GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { });

            GUI.color = Color.red;
            GUILayout.Box(" ", new GUILayoutOption[] { GUILayout.MaxWidth(minimumAllWaySectionWidth), GUILayout.Height(0.01f) });


            GUI.color = Color.white;
            GUI.skin.box.normal.textColor = originalTextColor;


            if (description != "")
                InspectorHelpBox(description, false, true);

        }

        public void InspectorVerticalBox(bool SideBySide = false) {



            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }


            if (SideBySide) {
                EditorGUILayout.BeginVertical("Box", GUILayout.MinHeight(minimumSectionHeight), GUILayout.MinWidth(minimumSideBySideSectionWidth));
            } else {
                EditorGUILayout.BeginVertical("Box", GUILayout.MinWidth(minimumAllWaySectionWidth));
            }


            GUI.color = Color.white;

        }

        public void InspectorVerticalBoxFullWidth() {



            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }



            EditorGUILayout.BeginVertical("Box");



            GUI.color = Color.white;

        }

        public void InspectorSmallText(string text) {

            GUIStyle myStyle = GUI.skin.GetStyle("label");
            myStyle.richText = true;
            myStyle.wordWrap = true;
            myStyle.fixedWidth = 0;
            myStyle.fontSize = 10;
            myStyle.fontStyle = FontStyle.Normal;

            EditorGUILayout.LabelField(text, myStyle, GUILayout.MaxWidth(minimumAllWaySectionWidth));

        }

        public void InspectorHelpBox(string text, bool space = true, bool alwaysShow = false) {

            GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
            myStyle.richText = true;
            myStyle.wordWrap = true;
            myStyle.fixedWidth = 0;
            myStyle.fontSize = 10;
            myStyle.fontStyle = FontStyle.Normal;

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHelpProColor;
            } else {
                GUI.color = inspectorSectionHelpColor;
            }
            EditorGUILayout.LabelField(text, myStyle, GUILayout.MaxWidth(minimumAllWaySectionWidth));

            if (space == true) {
                EditorGUILayout.Space();
            }



            GUI.color = Color.white;



        }


        public void InspectorHelpBoxFullWidth(string text, bool space = true, bool alwaysShow = false) {

            GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
            myStyle.richText = true;
            myStyle.wordWrap = true;
            myStyle.fixedWidth = 0;
            myStyle.fontSize = 10;
            myStyle.fontStyle = FontStyle.Normal;

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHelpProColor;
            } else {
                GUI.color = inspectorSectionHelpColor;
            }
            EditorGUILayout.LabelField(text, myStyle);

            if (space == true) {
                EditorGUILayout.Space();
            }



            GUI.color = Color.white;



        }

        public void InspectorPropertyBox(string header, SerializedProperty list, int listIndex) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginVertical();
            GUI.color = new Color32(208, 212, 211, 255);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(header, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(19) });
            GUI.skin.button.normal.textColor = Color.red;

            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            if (GUILayout.Button(UpArrowSymbol.ToString())) {
                list.MoveArrayElement(listIndex, listIndex - 1);
            }
            if (GUILayout.Button(DownArrowSymbol.ToString())) {
                list.MoveArrayElement(listIndex, listIndex + 1);
            }


            GUI.skin.button.normal.textColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(40))) {
                list.DeleteArrayElementAtIndex(listIndex);
            }

            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = originalTextColor;
            EditorGUILayout.EndVertical();

        }

        public void InspectorBoldVerticleLine() {
            GUI.color = Color.white;
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.Width(1f), GUILayout.ExpandHeight(true) });
            GUI.color = Color.white;


        }

        public void ResetLabelWidth() {

            EditorGUIUtility.labelWidth = 110;

        }

        public void InspectorHorizontalSpace(int width) {
            EditorGUILayout.LabelField("", GUILayout.Width(width));
        }

        public void InspectorListBox(string title, SerializedProperty list) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginVertical();
            GUI.color = new Color32(208, 212, 211, 255);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(title, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(21) });
            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {
                list.InsertArrayElementAtIndex(list.arraySize);
                if (list.type == "String") {
                    list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = "";
                }
            }
            GUILayout.EndHorizontal();


            if (list.arraySize > 0) {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < list.arraySize; i++) {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element, new GUIContent(""));
                    GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                    if (GUILayout.Button(UpArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i - 1);
                    }
                    if (GUILayout.Button(DownArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i + 1);
                    }


                    GUI.skin.button.normal.textColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(40))) {
                        list.DeleteArrayElementAtIndex(i);
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            GUI.skin.button.normal.textColor = originalTextColor;
        }

        // places an ability select list 
        public void InspectorAbilityListBox(string title, SerializedProperty list) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginVertical();
            GUI.color = new Color32(208, 212, 211, 255);
            GUILayout.BeginHorizontal("box");

            //get list of ability list
            List<ABC_Ability> abilityList = new List<ABC_Ability>();

            //Fill it with global ability elements
            foreach (ABC_Ability ability in this.Abilities) {

                if (ability.globalAbilities != null) {

                    foreach (ABC_Ability globalAbility in ability.globalAbilities.ElementAbilities) {
                        abilityList.Add(globalAbility);
                    }

                } else { // else just normal ability
                    abilityList.Add(ability);
                }

            }


            GUI.color = Color.white;
            abilityListChoice = EditorGUILayout.Popup(title, abilityListChoice, abilityList.Select(item => item.name).ToArray(), GUILayout.Width(250));
            GUI.color = new Color32(208, 212, 211, 255);
            GUI.color = Color.white;
            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {

                var stateIndex = list.arraySize;
                list.InsertArrayElementAtIndex(stateIndex);

                SerializedProperty ability = list.GetArrayElementAtIndex(stateIndex);

                ability.intValue = abilityList[abilityListChoice].abilityID;


            }

            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            if (list.arraySize > 0) {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < list.arraySize; i++) {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();


                    if (abilityList.Where(item => item.abilityID == element.intValue).FirstOrDefault() != null)
                        EditorGUILayout.LabelField(abilityList.Where(item => item.abilityID == element.intValue).FirstOrDefault().name);

                    GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                    if (GUILayout.Button(UpArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i - 1);
                    }
                    if (GUILayout.Button(DownArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i + 1);
                    }

                    GUI.skin.button.normal.textColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(40))) {
                        list.DeleteArrayElementAtIndex(i);
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();


                    GUI.skin.button.normal.textColor = originalTextColor;

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        // places an group select list 
        public void InspectorAbilityGroupListBox(string title, SerializedProperty list, SerializedProperty listChoice) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginVertical();
            GUI.color = new Color32(208, 212, 211, 255);
            GUILayout.BeginHorizontal("box");


            GUI.color = Color.white;
            listChoice.intValue = EditorGUILayout.Popup(title, listChoice.intValue, abilityCont.AbilityGroups.Select(item => item.groupName).ToArray());

            GUI.color = Color.white;
            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(40))) {

                var stateIndex = list.arraySize;
                list.InsertArrayElementAtIndex(stateIndex);

                SerializedProperty groupID = list.GetArrayElementAtIndex(stateIndex);

                groupID.intValue = abilityCont.AbilityGroups[listChoice.intValue].groupID;


            }

            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            // 3 verticle lists
            GUILayout.BeginHorizontal();


            if (list.arraySize > 0) {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < list.arraySize; i++) {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(abilityCont.AbilityGroups.Where(item => item.groupID == element.intValue).FirstOrDefault().groupName);

                    GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                    if (GUILayout.Button(UpArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i - 1);
                    }
                    if (GUILayout.Button(DownArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i + 1);
                    }

                    GUI.skin.button.normal.textColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(40))) {
                        list.DeleteArrayElementAtIndex(i);
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();


                    GUI.skin.button.normal.textColor = originalTextColor;

                }
                EditorGUILayout.EndVertical();
            }


            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }




        // symbols used for aesthetics only
        char SmallRightArrowSymbol = '\u25B8';
        char SmallDownArrowSymbol = '\u25BE';
        char BranchedSymbol = '\u251C';
        char DiamondSymbol = '\u25C6';
        char UpArrowSymbol = '\u2191';
        char DownArrowSymbol = '\u2193';
        char StarSymbol = '\u2605';


        // used to space out button text 
        char buttonSpace = '\u0590';

        // Button Icons
        Texture AddIcon;
        Texture SortIcon;
        Texture RemoveIcon;
        Texture ExportIcon;
        Texture CopyIcon;
        Texture ImportIcon;
        Texture ImportBlueIcon;


        Vector2 editorScrollPos;
        Vector2 listScrollPos;


        GUIStyle textureButton = new GUIStyle();

        #endregion


        SerializedObject GetTarget;

        private ReorderableList reorderableListAbilities;


        public ABC_Controller abilityCont;
        public ABC_GlobalElement globElement;

        SerializedProperty meAbilityList;

        //Used for selecting abilities
        List<ABC_Ability> reorderableAbilitylist = new List<ABC_Ability>();
        //Used for filtering abilities
        private string abilitySearchString = "";
        //Used to keep track of what we last filtered
        private string previousAbilitySearchString = "";

        Dictionary<ABC_GlobalElement, string> GlobalEffects = new Dictionary<ABC_GlobalElement, string>();
        public int globalEffectsListChoice = 0;


        Dictionary<ABC_GlobalElement, string> GlobalAbilities = new Dictionary<ABC_GlobalElement, string>();
        public int globalAbilitiesListChoice = 0;

        public int toolbarAbiltyManagerSelection;

        public int ToolbarAbiltyManagerSelection {
            get {
                if (abilityCont != null)
                    return abilityCont.toolbarAbiltyManagerSelection;
                else
                    return toolbarAbiltyManagerSelection;
            }

        }

        public int toolbarAbilityManagerGeneralSettingsSelection;

        public int ToolbarAbilityManagerGeneralSettingsSelection {
            get {
                if (abilityCont != null)
                    return abilityCont.toolbarAbilityManagerGeneralSettingsSelection;
                else
                    return toolbarAbilityManagerGeneralSettingsSelection;
            }

        }

        public int toolbarAbilityManagerPositionTravelSettingsSelection;

        public int ToolbarAbilityManagerPositionTravelSettingsSelection {
            get {
                if (abilityCont != null)
                    return abilityCont.toolbarAbilityManagerPositionTravelSettingsSelection;
                else
                    return toolbarAbilityManagerPositionTravelSettingsSelection;
            }

        }

        public int toolbarAbilityManagerCollisionImpactSettingsSelection;

        public int ToolbarAbilityManagerCollisionImpactSettingsSelection {
            get {
                if (abilityCont != null)
                    return abilityCont.toolbarAbilityManagerCollisionImpactSettingsSelection;
                else
                    return toolbarAbilityManagerCollisionImpactSettingsSelection;
            }

        }

        public int toolbarAbilityManagerAnimationSettingsSelection;

        public int ToolbarAbilityManagerAnimationSettingsSelection {
            get {
                if (abilityCont != null)
                    return abilityCont.toolbarAbilityManagerAnimationSettingsSelection;
                else
                    return toolbarAbilityManagerAnimationSettingsSelection;
            }

        }




        public GUIContent[] toolbarABC;


        public string[] generalSettingsToolbar = new string[] { "Settings", "Key & Combo", "Effects", "Additional Settings", };


        public string[] positiontravelSettingsToolbar = new string[] { "Position & Travel Type", "Travelling", "Additional Starting Positions" };


        public string[] collisionimpactSettingsToolbar = new string[] { "Collider", "Collision", "Impact" };


        public string[] animationSettingsToolbar = new string[] { "Preparing", "Initiating", "Ability End", "Additional Graphic", "Scrollable Ability", "Reload" };


        // ENUM Used for clicing effects in Inspector () 

        enumEffects effects;
        ABC_GlobalPortal.GameType gameTypes;

#if ABC_GC_Integration
    private GameCreator.Core.IActionsListEditor gcActionListEditor;


    private void SetupGCActionList(ref SerializedProperty SP, ref GameCreator.Core.IActionsListEditor editor, string prefabPath, string prefabName) {

        if (SP.objectReferenceValue == null) {

            GameCreator.Core.GameCreatorUtilities.CreateFolderStructure(prefabPath);
            string actionPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(prefabPath, prefabName));

            GameObject sceneInstance = new GameObject("Actions");
            sceneInstance.AddComponent<GameCreator.Core.Actions>();

            GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, actionPath);
            DestroyImmediate(sceneInstance);

            GameCreator.Core.Actions prefabActions = prefabInstance.GetComponent<GameCreator.Core.Actions>();
            prefabActions.destroyAfterFinishing = true;
            SP.objectReferenceValue = prefabActions.actionsList;
        }


        editor = Editor.CreateEditor(SP.objectReferenceValue, typeof(GameCreator.Core.IActionsListEditor)) as GameCreator.Core.IActionsListEditor;

    }


#endif

#if ABC_GC_2_Integration

    private void SetupGC2Action(ref SerializedProperty SP, string prefabPath, string prefabName) {

        if (SP.objectReferenceValue == null) {

            ABC_Utilities.CreateFolderStructure(prefabPath);
            string actionPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(prefabPath, prefabName));

            GameObject sceneInstance = new GameObject("Actions");
            sceneInstance.AddComponent<GameCreator.Runtime.VisualScripting.Actions>();

            GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, actionPath);
            DestroyImmediate(sceneInstance);

            GameCreator.Runtime.VisualScripting.Actions prefabActions = prefabInstance.GetComponent<GameCreator.Runtime.VisualScripting.Actions>();
            SP.objectReferenceValue = prefabActions;
        }


    }


#endif

        // ************************* Settings / Serialized Properties ***********************************

        #region Settings (Serialized Properties)

        /// <summary>
        /// Will get the abilities or the global abilities depending on config screen 
        /// </summary>
        public List<ABC_Ability> Abilities {
            get {

                List<ABC_Ability> retVal = new List<ABC_Ability>();


                if (abilityCont != null) {

                    if (EditorApplication.isPlaying) {
                        retVal = abilityCont._currentAbilities;
                    } else {

                        retVal = abilityCont.Abilities;
                    }
                } else {
                    if (globElement != null)
                        retVal = globElement.ElementAbilities;
                }

                return retVal;
            }
        }


        int _currentAbility = 0;

        int currentAbility {
            get {

                if (abilityCont != null)
                    return abilityCont.CurrentAbility;
                else
                    return this._currentAbility;
            }
            set {

                if (abilityCont != null)
                    abilityCont.CurrentAbility = value;
                else
                    _currentAbility = value;


            }
        }


        int currentAbilityIndex = 0;
        // which is currently selected from list
        int abilityListChoice;

        //Which is currently selected from the side list filter
        List<string> sideListFilterOptions = new List<string>();


        int AbilityCount;


        SerializedProperty MyListRef;
        new SerializedProperty name;
        SerializedProperty globalAbilitiesEnableGameTypeModification;
        SerializedProperty globalAbilitiesGameTypeModification;
        SerializedProperty abilityID;
        SerializedProperty abilityTags;
        SerializedProperty allowAbilityGroupAssignment;
        SerializedProperty assignedAbilityGroupIDs;
        SerializedProperty abilityGroupListChoice;
        SerializedProperty assignedAbilityGroupNames;
        SerializedProperty inspectorParentAbilityListChoice;
        SerializedProperty parentAbilityID;
        SerializedProperty showChildrenInInspector;
        SerializedProperty description;
        SerializedProperty iconImage;
        SerializedProperty iconUIs;
        SerializedProperty autoCast;
        SerializedProperty globalAbilityOverrideEnableStatus;
        SerializedProperty abilityEnabled;
        SerializedProperty enableDuration;
        SerializedProperty enableAfterEvent;
        SerializedProperty enableAfterAbilityIDsActivated;
        SerializedProperty enableAfterAbilityIDsCollided;
        SerializedProperty enableAbilityActivationLinks;
        SerializedProperty enableAbilityTriggerLinks;
        SerializedProperty triggerLinkAbilityIDs;
        SerializedProperty activationLinkAbilityIDs;
        SerializedProperty manaCost;
        SerializedProperty statCost;
        SerializedProperty statCostIntegrationType;
        SerializedProperty statCostName;
        SerializedProperty reduceManaWhenActive;
        SerializedProperty neverSleep;
        SerializedProperty isKinematic;
        SerializedProperty mass;
        SerializedProperty mainGraphic;
        SerializedProperty scaleAbilityGraphic;
        SerializedProperty abilityGraphicScale;
        SerializedProperty chooseLayer;
        SerializedProperty abLayer;
        SerializedProperty childParticle;
        SerializedProperty useColliderTrigger;
        SerializedProperty applyGravity;
        SerializedProperty applyGravityDelay;
        SerializedProperty travelType;
        SerializedProperty travelPhysics;
        SerializedProperty travelDrag;
        SerializedProperty customTravelScript;
        SerializedProperty crossHairRaycastRadius;
        SerializedProperty crossHairRecordTargetOnActivation;
        SerializedProperty crossHairRaycastReturnDistancePointOnly;
        SerializedProperty crossHairRaycastReturnedDistance;
        SerializedProperty targetTravel;
        SerializedProperty travelWithCaster;
        SerializedProperty boomerangMode;
        SerializedProperty boomerangDelay;
        SerializedProperty requireCrossHair;
        SerializedProperty scrollSetUnsetRaiseEvent;
        SerializedProperty scrollQuickInputType;
        SerializedProperty scrollQuickButton;
        SerializedProperty scrollQuickKey;
        SerializedProperty scrollSwapDuration;
        SerializedProperty enableCriticalHit;
        SerializedProperty potencyCriticalHitBaseChance;
        SerializedProperty potencyCriticalHitEnableChanceStatModification;
        SerializedProperty potencyCriticalHitChanceStatModification;
        SerializedProperty enableAbilityEffectLinks;
        SerializedProperty effectLinkAbilityIDs;
#if ABC_GC_Integration
    SerializedProperty gcEffectActionList;
#endif

#if ABC_GC_2_Integration
    SerializedProperty gc2EffectAction;
#endif

        SerializedProperty targetFacing;
        SerializedProperty noTargetStillTravel;
        SerializedProperty auxiliarySoftTarget;
        SerializedProperty startingPositionAuxiliarySoftTarget;
        SerializedProperty seekTargetDelay;
        SerializedProperty selectedTargetRestrictTargets;
        SerializedProperty selectedTargetOnlyCastOnTag;
        SerializedProperty continuouslyTurnToDestination;
        SerializedProperty affectOnlyTarget;
        SerializedProperty scrollAbility;



        SerializedProperty tempAbilityActivationIntervalAdjustment;
        SerializedProperty neverResetOtherCombos;
        SerializedProperty abilityCombo;
        SerializedProperty comboNextActivateTime;
        SerializedProperty comboHitRequired;

        SerializedProperty randomlySwapAbilityPosition;
        SerializedProperty LandOrAir;
        SerializedProperty castableDuringHitPrevention;
        SerializedProperty hitPreventionWontInterruptActivation;
        SerializedProperty forceActivation;
        SerializedProperty forceActivationInterruptCurrentActivation;
        SerializedProperty airAbilityDistanceFromGround;

        SerializedProperty triggerType;
        SerializedProperty inputCombo;

        SerializedProperty keyInputType;
        SerializedProperty keyButton;
        SerializedProperty key;
        SerializedProperty globalAbilityOverrideKeyTrigger;
        SerializedProperty onKeyPress;
        SerializedProperty onKeyDown;
        SerializedProperty enableMovementValidation;
        SerializedProperty movementValidation;

        SerializedProperty requireAdditionalKeyInput;
        SerializedProperty additionalKeyInputType;
        SerializedProperty additionalKeyButton;
        SerializedProperty additionalKey;
        SerializedProperty additionalOnKeyPress;
        SerializedProperty additionalOnKeyDown;

        SerializedProperty abilityRecast;
        SerializedProperty startRecastAfterAbilityEnd;
        SerializedProperty travelSpeed;
        SerializedProperty travelDelay;
        SerializedProperty applyTravelDuration;
        SerializedProperty travelDurationOriginatorTagsRequired;
        SerializedProperty travelDurationTime;
        SerializedProperty travelDurationStopSuddenly;
        SerializedProperty duration;
        SerializedProperty abilityToggle;
        SerializedProperty canCastWhenToggled;
        SerializedProperty repeatInitiatingAnimationWhilstToggled;
        SerializedProperty stateEffects;
        SerializedProperty additionalStartPositions;
        SerializedProperty onEnter;
        SerializedProperty onStay;
        SerializedProperty onStayInterval;
        SerializedProperty onExit;
        SerializedProperty particleCollision;

        SerializedProperty enableCollisionAfterKeyPress;
        SerializedProperty enableCollisionAfterKeyInputType;
        SerializedProperty enableCollisionAfterKeyButton;
        SerializedProperty enableCollisionAfterKey;

        SerializedProperty abilityRequiredTag;
        SerializedProperty abilityIgnoreTag;
        SerializedProperty startingPosition;
        SerializedProperty affectOriginObject;
        SerializedProperty affectLayer;

        SerializedProperty startingPositionOnObject;
        SerializedProperty startingPositionOnTag;
        SerializedProperty startingPositionOffset;
        SerializedProperty startingPositionForwardOffset;
        SerializedProperty startingPositionRightOffset;

        SerializedProperty startingRotation;
        SerializedProperty setEulerRotation;
        SerializedProperty prepareTime;
        SerializedProperty ignoreGlobalPrepareTimeAdjustments;
        SerializedProperty prepareTriggerHoldRequied;
        SerializedProperty abilityInitiatingBaseSpeedAdjustment;
        SerializedProperty ignoreGlobalInitiatingSpeedAdjustments;
        SerializedProperty modifyAbilityInitiatingBaseSpeedByStat;
        SerializedProperty abilityInitiatingBaseSpeedStatModification;
        SerializedProperty preparingAestheticsPositionOffset;
        SerializedProperty moveInteruptPreparation;
        SerializedProperty waitForKeyBeforeInitiating;
        SerializedProperty waitBeforeInitiatingInputType;
        SerializedProperty waitBeforeInitiatingButton;
        SerializedProperty waitBeforeInitiatingKey;
        SerializedProperty waitForKeyBeforeInitiatingDelay;
        SerializedProperty waitForKeyAllowChangeOfTarget;
        SerializedProperty distanceInteruptPreparation;
        SerializedProperty abilityCollisionIgnores;
        SerializedProperty impactDestroy;
        SerializedProperty destroyDelay;
        SerializedProperty destroyIgnoreTag;

        SerializedProperty includeSurroundingObject;
        SerializedProperty surroundingObjectTarget;
        SerializedProperty lockSurroundingObject;
        SerializedProperty minimumScatterRange;
        SerializedProperty maximumScatterRange;
        SerializedProperty surroundingObjectTargetRange;
        SerializedProperty destroySurroundingObject;
        SerializedProperty projectileAffectSurroundingObject;
        SerializedProperty surroundingObjectTargetRequired;
        SerializedProperty surroundingObjectTargetRestrict;
        SerializedProperty surroundingObjectTargetAffectTag;
        SerializedProperty surroundingObjectAuxiliarySoftTarget;

        SerializedProperty surroundingObjectTags;
        SerializedProperty surroundingObjectTagRequired;
        SerializedProperty surroundingObjectTagAffectTag;
        SerializedProperty surroundingObjectTagLimit;
        SerializedProperty surroundingObjectTagsRange;
        SerializedProperty applyColliderToSurroundingObjects;
        SerializedProperty applyColliderWhenProjectileReached;

        SerializedProperty sendObjectToProjectile;
        SerializedProperty objectToProjectileDuration;
        SerializedProperty useScrollAbilityAesthetics;
        SerializedProperty scrollAbilityAnimateOnScrollGraphic;
        SerializedProperty scrollAbilityAnimateOnEntity;
        SerializedProperty scrollAbilityAnimateOnWeapon;





        SerializedProperty scrollAbilityAestheticsPositionOffset;
        SerializedProperty scrollAbilityAnimatorParameter;
        SerializedProperty scrollAbilityAnimatorParameterType;
        SerializedProperty scrollAbilityAnimatorOnValue;
        SerializedProperty scrollAbilityAnimatorOffValue;
        SerializedProperty scrollAbilityAnimatorDuration;
        SerializedProperty scrollAbilityParticle;
        SerializedProperty scrollAbilityObject;
        SerializedProperty scrollAbilityGraphicActivateDelay;
        SerializedProperty scrollAbilityGraphicDeactivateDelay;

        SerializedProperty scrollAbilityStartPosition;
        SerializedProperty scrollAbilityPositionOnObject;
        SerializedProperty scrollAbilityPositionOnTag;
        SerializedProperty scrollAbilityAestheticUseDuration;
        SerializedProperty scrollAbilityAestheticDuration;
        SerializedProperty scrollAbilityPersistantAestheticInactivePosition;
        SerializedProperty scrollAbilityPersistantAestheticInactivePositionOnObject;
        SerializedProperty scrollAbilityPersistantAestheticInactivePositionOnTag;

        SerializedProperty scrollAbilityAnimationRunnerClip;
        SerializedProperty scrollAbilityAnimationRunnerMask;
        SerializedProperty scrollAbilityAnimationRunnerClipSpeed;
        SerializedProperty scrollAbilityAnimationRunnerClipDelay;
        SerializedProperty scrollAbilityAnimationRunnerClipDuration;
        SerializedProperty scrollAbilityAnimationRunnerOnEntity;
        SerializedProperty scrollAbilityAnimationRunnerOnScrollGraphic;
        SerializedProperty scrollAbilityAnimationRunnerOnWeapon;


        SerializedProperty scrollAbilityDeactivateAnimationRunnerClip;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerMask;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerClipSpeed;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerClipDelay;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerClipDuration;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerOnEntity;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerOnScrollGraphic;
        SerializedProperty scrollAbilityDeactivateAnimationRunnerOnWeapon;


        SerializedProperty scrollAbilityDeactivateAnimateOnScrollGraphic;
        SerializedProperty scrollAbilityDeactivateAnimateOnEntity;
        SerializedProperty scrollAbilityDeactivateAnimateOnWeapon;
        SerializedProperty scrollAbilityDeactivateAnimatorParameter;
        SerializedProperty scrollAbilityDeactivateAnimatorParameterType;
        SerializedProperty scrollAbilityDeactivateAnimatorOnValue;
        SerializedProperty scrollAbilityDeactivateAnimatorOffValue;
        SerializedProperty scrollAbilityDeactivateAnimatorDuration;

        SerializedProperty useReloadAbilityAesthetics;
        SerializedProperty reloadAbilityAnimateOnScrollGraphic;
        SerializedProperty reloadAbilityAnimateOnEntity;
        SerializedProperty reloadAbilityAnimateOnWeapon;
        SerializedProperty reloadAbilityAestheticsPositionOffset;
        SerializedProperty reloadAbilityAnimatorParameter;
        SerializedProperty reloadAbilityAnimatorParameterType;
        SerializedProperty reloadAbilityAnimatorOnValue;
        SerializedProperty reloadAbilityAnimatorOffValue;
        SerializedProperty reloadAbilityParticle;
        SerializedProperty reloadAbilityObject;
        SerializedProperty reloadAbilityAestheticDuration;
        SerializedProperty reloadAbilityStartPosition;
        SerializedProperty reloadAbilityPositionOnObject;
        SerializedProperty reloadAbilityPositionOnTag;

        SerializedProperty reloadAbilityAnimationRunnerClip;
        SerializedProperty reloadAbilityAnimationRunnerMask;
        SerializedProperty reloadAbilityAnimationRunnerClipSpeed;
        SerializedProperty reloadAbilityAnimationRunnerClipDelay;
        SerializedProperty reloadAbilityAnimationRunnerOnEntity;
        SerializedProperty reloadAbilityAnimationRunnerOnScrollGraphic;
        SerializedProperty reloadAbilityAnimationRunnerOnWeapon;


        SerializedProperty useProjectileToStartPosition;
        SerializedProperty projectileToStartType;
        SerializedProperty projToStartDelay;
        SerializedProperty useOriginalProjectilePTS;
        SerializedProperty projToStartPosParticle;
        SerializedProperty projToStartPosObject;
        SerializedProperty projToStartPosDuration;
        SerializedProperty projToStartStartingPosition;
        SerializedProperty projToStartRotateToTarget;
        SerializedProperty projToStartRotation;
        SerializedProperty projToStartSetEulerRotation;
        SerializedProperty projToStartPositionOnObject;
        SerializedProperty projToStartPositionOnTag;
        SerializedProperty projToStartPositionOffset;
        SerializedProperty projToStartPositionForwardOffset;
        SerializedProperty projToStartPositionRightOffset;
        SerializedProperty projToStartHoverOnSpot;
        SerializedProperty projToStartHoverDistance;
        SerializedProperty projToStartTravelToAbilityStartPosition;
        SerializedProperty projToStartmoveWithTarget;
        SerializedProperty projToStartReachPositionTime;
        SerializedProperty projToStartTravelDelay;

        SerializedProperty UseAmmo;
        SerializedProperty useEquippedWeaponAmmo;
        SerializedProperty ammoCount;
        SerializedProperty reduceAmmoWhilstActive;
        SerializedProperty useReload;
        SerializedProperty reloadDuration;
        SerializedProperty reloadRestrictAbilityActivationDuration;
        SerializedProperty clipSize;
        SerializedProperty autoReloadWhenRequired;
        SerializedProperty reloadFillClip;
        SerializedProperty reloadFillClipRepeatGraphic;


        SerializedProperty bounceMode;
        SerializedProperty bounceAmount;
        SerializedProperty bounceTarget;
        SerializedProperty bounceTag;
        SerializedProperty bounceRange;
        SerializedProperty bounceOnCaster;
        SerializedProperty enableRandomBounce;
        SerializedProperty startBounceTagRequired;
        SerializedProperty startBounceRequiredTags;
        SerializedProperty bouncePositionOffset;
        SerializedProperty bouncePositionForwardOffset;
        SerializedProperty bouncePositionRightOffset;


        SerializedProperty useGraphicRadius;
        SerializedProperty colliderRadius;
        SerializedProperty applyColliderSettingsToParent;
        SerializedProperty applyColliderSettingsToChildren;

        SerializedProperty meleeKeepRotatingToSelectedTarget;
        SerializedProperty rotateToSelectedTarget;
        SerializedProperty noTargetRotateBehaviour;
        SerializedProperty hitsStopMeleeAttack;

        SerializedProperty colliderOffset;
        SerializedProperty useDestroySplashEffect;
        SerializedProperty destroySplashRadius;
        SerializedProperty destroySplashExplosion;
        SerializedProperty destroySplashExplosionPower;
        SerializedProperty destroySplashExplosionTagLimit;
        SerializedProperty destroySplashExplosionAffectTag;
        SerializedProperty destroySplashExplosionRadius;
        SerializedProperty destroySplashExplosionUplift;

        SerializedProperty globalImpactRequiredTag;

        SerializedProperty modifyGameSpeedOnInitiation;
        SerializedProperty modifyGameSpeedOnInitiationSpeedFactor;
        SerializedProperty modifyGameSpeedOnInitiationDelay;
        SerializedProperty modifyGameSpeedOnInitiationDuration;

        SerializedProperty modifyGameSpeedOnImpact;
        SerializedProperty modifyGameSpeedOnImpactSpeedFactor;
        SerializedProperty modifyGameSpeedOnImpactDuration;
        SerializedProperty modifyGameSpeedOnImpactDelay;

        SerializedProperty shakeCameraOnInitiation;
        SerializedProperty shakeCameraOnInitiationDelay;
        SerializedProperty shakeCameraOnInitiationDuration;
        SerializedProperty shakeCameraOnInitiationAmount;
        SerializedProperty shakeCameraOnInitiationSpeed;

        SerializedProperty shakeCameraOnImpact;
        SerializedProperty shakeCameraOnImpactDelay;
        SerializedProperty shakeCameraOnImpactDuration;
        SerializedProperty shakeCameraOnImpactAmount;
        SerializedProperty shakeCameraOnImpactSpeed;

        SerializedProperty pushEntityOnImpact;
        SerializedProperty pushEntityOnImpactDelay;
        SerializedProperty pushEntityOnImpactAmount;
        SerializedProperty pushEntityOnImpactLiftForce;

        SerializedProperty defyEntityGravityOnImpact;
        SerializedProperty defyEntityGravityOnImpactDelay;
        SerializedProperty defyEntityGravityOnImpactDuration;

        SerializedProperty shakeEntityOnImpact;
        SerializedProperty shakeEntityOnImpactShakeAmount;
        SerializedProperty shakeEntityOnImpactShakeDecay;
        SerializedProperty shakeEntityOnImpactShakeDelay;

        SerializedProperty attachToObjectOnImpact;
        SerializedProperty attachToObjectProbabilityMinValue;
        SerializedProperty attachToObjectProbabilityMaxValue;
        SerializedProperty attachToObjectStickOutFactor;
        SerializedProperty attachToObjectNearestBone;

        SerializedProperty switchColorOnImpact;
        SerializedProperty switchColorOnImpactColor;
        SerializedProperty switchColorOnImpactDelay;
        SerializedProperty switchColorOnImpactDuration;
        SerializedProperty switchColorOnImpactUseEmission;

        SerializedProperty enableHitStopOnImpact;
        SerializedProperty hitStopOnImpactDelay;
        SerializedProperty hitStopOnImpactDuration;
        SerializedProperty hitStopOnImpactEntityHitDelay;


        SerializedProperty useInitiatingAesthetics;
#if ABC_GC_Integration
    SerializedProperty gcInitiatingActionList;
#endif

#if ABC_GC_2_Integration
    SerializedProperty gc2InitiatingAction;
#endif

        SerializedProperty initiatingAnimateOnEntity;
        SerializedProperty initiatingAnimateOnScrollGraphic;
        SerializedProperty initiatingAnimateOnWeapon;
        SerializedProperty defyGravityInitiating;
        SerializedProperty defyGravityInitiatingDuration;
        SerializedProperty defyGravityInitiatingDelay;
        SerializedProperty defyGravityInitiatingRaiseEvent;
        SerializedProperty initiatingAestheticsPositionOffset;
        SerializedProperty initiatingAnimatorParameter;
        SerializedProperty initiatingParticle;
        SerializedProperty initiatingObject;
        SerializedProperty initiatingAestheticActivateWithAbility;
        SerializedProperty initiatingAestheticDuration;
        SerializedProperty initiatingAestheticDelay;
        SerializedProperty initiatingAestheticDetachFromParentAfterDelay;
        SerializedProperty initiatingAestheticDetachDelay;
        SerializedProperty initiatingUseWeaponTrail;
        SerializedProperty initiatingWeaponTrailGraphicIteration;

        SerializedProperty intiatingProjectileDelayType;
        SerializedProperty delayBetweenInitiatingAndProjectile;
        SerializedProperty initiatingProjectileDelayAnimationPercentage;
        SerializedProperty initiatingStartPosition;
        SerializedProperty initiatingPositionOnObject;
        SerializedProperty initiatingPositionOnTag;
        SerializedProperty initiatingAnimatorParameterType;
        SerializedProperty initiatingAnimatorOnValue;
        SerializedProperty initiatingAnimatorOffValue;
        SerializedProperty initiatingAnimatorDuration;

        SerializedProperty initiatingAnimationRunnerClip;
        SerializedProperty initiatingAnimationRunnerMask;
        SerializedProperty initiatingAnimationRunnerClipSpeed;
        SerializedProperty initiatingAnimationRunnerClipDelay;
        SerializedProperty initiatingAnimationRunnerClipDuration;
        SerializedProperty initiatingAnimationRunnerOnEntity;
        SerializedProperty initiatingAnimationRunnerOnScrollGraphic;
        SerializedProperty initiatingAnimationRunnerOnWeapon;

        SerializedProperty moveSelfWhenInitiating;
        SerializedProperty moveSelfInitiatingOffset;
        SerializedProperty moveSelfInitiatingForwardOffset;
        SerializedProperty moveSelfInitiatingRightOffset;
        SerializedProperty moveSelfInitiatingDelay;
        SerializedProperty moveSelfInitiatingDuration;

        SerializedProperty moveSelfToTargetWhenInitiating;
        SerializedProperty moveSelfToTargetInitiatingDelay;
        SerializedProperty moveSelfToTargetInitiatingDuration;
        SerializedProperty moveSelfToTargetInitiatingStopDistance;
        SerializedProperty moveSelfToTargetInitiatingOffset;
        SerializedProperty moveSelfToTargetInitiatingForwardOffset;
        SerializedProperty moveSelfToTargetInitiatingRightOffset;



        SerializedProperty initiatingAestheticsPositionForwardOffset;
        SerializedProperty initiatingAestheticsPositionRightOffset;
        SerializedProperty preparingAestheticsPositionForwardOffset;
        SerializedProperty preparingAestheticsPositionRightOffset;
        SerializedProperty scrollAbilityAestheticsPositionForwardOffset;
        SerializedProperty scrollAbilityAestheticsPositionRightOffset;
        SerializedProperty reloadAbilityAestheticsPositionForwardOffset;
        SerializedProperty reloadAbilityAestheticsPositionRightOffset;
        SerializedProperty selectedTargetForwardOffset;
        SerializedProperty selectedTargetRightOffset;
        SerializedProperty abilityCanMiss;


        SerializedProperty useAbilityEndAesthetics;
        SerializedProperty abilityEndUseEffectGraphic;
        SerializedProperty abilityEndActivateOnEnvironmentOnly;
        SerializedProperty abEndParticle;
        SerializedProperty abEndObject;
        SerializedProperty scaleAbilityEndGraphic;
        SerializedProperty abilityEndGraphicScale;
        SerializedProperty abEndAestheticDuration;

        SerializedProperty showPrepareTimeOnGUI;
        SerializedProperty defyGravityPreparing;
        SerializedProperty defyGravityPreparingDuration;
        SerializedProperty defyGravityPreparingDelay;
        SerializedProperty defyGravityPreparingRaiseEvent;
        SerializedProperty usePreparingAesthetics;

#if ABC_GC_Integration
    SerializedProperty gcPreparingActionList;
#endif

#if ABC_GC_2_Integration
    SerializedProperty gc2PreparingAction;
#endif

        SerializedProperty preparingAnimateOnEntity;
        SerializedProperty preparingAnimateOnScrollGraphic;
        SerializedProperty preparingAnimateOnWeapon;
        SerializedProperty preparingAnimatorParameter;
        SerializedProperty preparingAnimatorParameterType;
        SerializedProperty preparingAnimatorOnValue;
        SerializedProperty preparingAnimatorOffValue;
        SerializedProperty preparingParticle;
        SerializedProperty preparingObject;
        SerializedProperty preparingAestheticDurationUsePrepareTime;
        SerializedProperty preparingAestheticDuration;
        SerializedProperty preparingStartPosition;
        SerializedProperty preparingPositionOnObject;
        SerializedProperty preparingPositionOnTag;


        SerializedProperty moveSelfToTargetWhenPreparing;
        SerializedProperty moveSelfToTargetPreparingDelay;
        SerializedProperty moveSelfToTargetPreparingDuration;
        SerializedProperty moveSelfToTargetPreparingStopDistance;
        SerializedProperty moveSelfToTargetActivatePreparingAnimationOnlyWhenMoving;
        SerializedProperty moveSelfToTargetPreparingOffset;
        SerializedProperty moveSelfToTargetPreparingForwardOffset;
        SerializedProperty moveSelfToTargetPreparingRightOffset;

        SerializedProperty preparingAnimationRunnerClip;
        SerializedProperty preparingAnimationRunnerMask;
        SerializedProperty preparingAnimationRunnerClipSpeed;
        SerializedProperty preparingAnimationRunnerClipDelay;
        SerializedProperty preparingAnimationRunnerOnEntity;
        SerializedProperty preparingAnimationRunnerOnScrollGraphic;
        SerializedProperty preparingAnimationRunnerOnWeapon;

        SerializedProperty moveSelfWhenPreparing;
        SerializedProperty moveSelfPreparingOffset;
        SerializedProperty moveSelfPreparingForwardOffset;
        SerializedProperty moveSelfPreparingRightOffset;
        SerializedProperty moveSelfPreparingDelay;
        SerializedProperty moveSelfPreparingDuration;

        SerializedProperty colliderTimeDelay;
        SerializedProperty colliderDelayTime;
        SerializedProperty colliderKeyPressDelay;
        SerializedProperty colliderDelayInputType;
        SerializedProperty colliderDelayButton;
        SerializedProperty colliderDelayKey;
        SerializedProperty ignoreActiveTerrain;
        SerializedProperty overrideIgnoreAbilityCollision;
        SerializedProperty overrideWeaponParrying;
        SerializedProperty overrideWeaponBlocking;
        SerializedProperty reduceWeaponBlockDurability;


        SerializedProperty activateAnimationFromHit;
        SerializedProperty activateAnimationFromHitDelay;
        SerializedProperty activateAnimationFromHitUseAirAnimation;
        SerializedProperty activateSpecificHitAnimation;
        SerializedProperty hitAnimationToActivate;
        SerializedProperty activateSpecificHitAnimationUseClip;
        SerializedProperty hitAnimationClipToActivate;


        SerializedProperty persistIK;

        SerializedProperty spawnObject;
        SerializedProperty spawningObject;
        SerializedProperty spawnObjectOnDestroy;
        SerializedProperty spawnObjectOnCollide;

        SerializedProperty useRange;
        SerializedProperty selectedTargetRangeGreaterThan;
        SerializedProperty selectedTargetRangeLessThan;
        SerializedProperty selectedTargetOffset;
        SerializedProperty addAbilityCollider;
        SerializedProperty abilityType;
        SerializedProperty rayCastRadius;
        SerializedProperty raycastHitAmount;
        SerializedProperty raycastBlockable;
        SerializedProperty raycastIgnoreTerrain;
        SerializedProperty abilityBeforeTarget;
        SerializedProperty loopTillTargetFound;


        SerializedProperty abilityMouseTargetIndicatorImage;
        SerializedProperty abilityRangeIndicatorImage;
        SerializedProperty abilityWorldTargetIndicatorImage;

        SerializedProperty abilityBeforeTargetWorldIndicatorScaleToEffectRadius;
        SerializedProperty abilityBeforeTargetWorldIndicatorScale;
        SerializedProperty abilityBeforeTargetMouseTargetIndicatorLength;

        SerializedProperty rayCastLength;
        SerializedProperty rayCastSingleHit;
        SerializedProperty maxActiveAtOnce;
        SerializedProperty limitActiveAtOnce;
        SerializedProperty abilityActivationRaiseEvent;
        SerializedProperty abilityActivationCompleteRaiseEvent;
        SerializedProperty abilityActivationCompleteEventType;
        SerializedProperty loggingEnabled;

        SerializedProperty stopMovementOnInitiate;
        SerializedProperty stopMovementOnInitiateDuration;
        SerializedProperty stopMovementOnInitiateFreezePosition;
        SerializedProperty stopMovementOnInitiateDisableComponents;
        SerializedProperty stopMovementOnInitiateRaiseEvent;

        SerializedProperty stopMovementOnPreparing;
        SerializedProperty stopMovementOnPreparingDuration;
        SerializedProperty stopMovementOnPreparingFreezePosition;
        SerializedProperty stopMovementOnPreparingDisableComponents;
        SerializedProperty stopMovementOnPreparingRaiseEvent;

        SerializedProperty mouseForwardLockX;
        SerializedProperty mouseForwardLockY;
        SerializedProperty mouseForwardLockZ;
        SerializedProperty mouseFrontOnly;

        SerializedProperty travelNearestTagList;
        SerializedProperty travelNearestTagRange;
        SerializedProperty travelNearestTagIgnoreOriginator;
        SerializedProperty travelNearestTagRandomiseSearch;

        #endregion

        void GetGlobalElements() {

            if (this.GlobalEffects != null)
                this.GlobalEffects.Clear();

            if (this.GlobalAbilities != null)
                this.GlobalAbilities.Clear();


            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ABC_GlobalElement).Name);
            ABC_GlobalElement[] a = new ABC_GlobalElement[guids.Length];
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<ABC_GlobalElement>(path);

                if (a[i].elementType == ABC_GlobalElement.GlobalElementType.Effect) {
                    this.GlobalEffects.Add(a[i], path);
                }


                if (a[i].elementType == ABC_GlobalElement.GlobalElementType.Abilities || a[i].elementType == ABC_GlobalElement.GlobalElementType.Weapon && a[i].ElementAbilities.Count > 0) {
                    this.GlobalAbilities.Add(a[i], path);
                }
            }

        }

        /// <summary>
        /// All abilities currently on the entity including the global elements
        /// </summary>
        public List<ABC_Ability> AllAbilities {

            get {
                List<ABC_Ability> retVal = new List<ABC_Ability>();
                if (abilityCont != null) {
                    foreach (ABC_Ability ability in this.Abilities) {

                        if (ability.globalAbilities != null) {

                            foreach (ABC_Ability globalAbility in ABC_Utilities.GetAbilitiesFromGlobalElement(ability.globalAbilities)) {
                                retVal.Add(globalAbility);
                            }

                        } else {
                            retVal.Add(ability);
                        }
                    }




                } else {
                    if (globElement != null)
                        retVal = globElement.ElementAbilities;
                }

                return retVal;

            }
        }

        /// <summary>
        /// All weapons currently on the entity including global elements
        /// </summary>
        public List<ABC_Controller.Weapon> AllWeapons {
            get {

                List<ABC_Controller.Weapon> retVal = new List<ABC_Controller.Weapon>();
                if (abilityCont != null) {
                    foreach (ABC_Controller.Weapon weapon in abilityCont.Weapons) {

                        if (weapon.globalWeapon != null) {

                            retVal.Add(weapon.globalWeapon.ElementWeapon);

                        } else {
                            retVal.Add(weapon);
                        }
                    }
                }

                return retVal;
            }
        }


        void GetProperties(int elementIteration) {

            // return if no abilities exist
            if (abilityCont != null && this.Abilities.Count == 0)
                return;


            if (meAbilityList == null || meAbilityList.arraySize == 0)
                return;


            if (elementIteration >= meAbilityList.arraySize)
                elementIteration = 0;


            if (elementIteration == -1)
                return;

            // remembers what spell we are currently displaying details display details for
            currentAbilityIndex = elementIteration;

            MyListRef = meAbilityList.GetArrayElementAtIndex(elementIteration);
            globalAbilitiesEnableGameTypeModification = MyListRef.FindPropertyRelative("globalAbilitiesEnableGameTypeModification");
            globalAbilitiesGameTypeModification = MyListRef.FindPropertyRelative("globalAbilitiesGameTypeModification");
            name = MyListRef.FindPropertyRelative("name");
            abilityID = MyListRef.FindPropertyRelative("abilityID");
            abilityTags = MyListRef.FindPropertyRelative("abilityTags");
            allowAbilityGroupAssignment = MyListRef.FindPropertyRelative("allowAbilityGroupAssignment");
            assignedAbilityGroupNames = MyListRef.FindPropertyRelative("assignedAbilityGroupNames");
            assignedAbilityGroupIDs = MyListRef.FindPropertyRelative("assignedAbilityGroupIDs");
            abilityGroupListChoice = MyListRef.FindPropertyRelative("abilityGroupListChoice");
            inspectorParentAbilityListChoice = MyListRef.FindPropertyRelative("inspectorParentAbilityListChoice");
            parentAbilityID = MyListRef.FindPropertyRelative("parentAbilityID");
            showChildrenInInspector = MyListRef.FindPropertyRelative("showChildrenInInspector");
            description = MyListRef.FindPropertyRelative("description");
            iconImage = MyListRef.FindPropertyRelative("iconImage");
            autoCast = MyListRef.FindPropertyRelative("autoCast");
            globalAbilityOverrideEnableStatus = MyListRef.FindPropertyRelative("globalAbilityOverrideEnableStatus");
            abilityEnabled = MyListRef.FindPropertyRelative("abilityEnabled");
            enableDuration = MyListRef.FindPropertyRelative("enableDuration");
            enableAfterEvent = MyListRef.FindPropertyRelative("enableAfterEvent");
            enableAfterAbilityIDsActivated = MyListRef.FindPropertyRelative("enableAfterAbilityIDsActivated");
            enableAfterAbilityIDsCollided = MyListRef.FindPropertyRelative("enableAfterAbilityIDsCollide");
            enableAbilityActivationLinks = MyListRef.FindPropertyRelative("enableAbilityActivationLinks");
            activationLinkAbilityIDs = MyListRef.FindPropertyRelative("activationLinkAbilityIDs");
            enableAbilityTriggerLinks = MyListRef.FindPropertyRelative("enableAbilityTriggerLinks");
            triggerLinkAbilityIDs = MyListRef.FindPropertyRelative("triggerLinkAbilityIDs");

            manaCost = MyListRef.FindPropertyRelative("manaCost");
            statCost = MyListRef.FindPropertyRelative("statCost");
            statCostIntegrationType = MyListRef.FindPropertyRelative("statCostIntegrationType");
            statCostName = MyListRef.FindPropertyRelative("statCostName");
            reduceManaWhenActive = MyListRef.FindPropertyRelative("reduceManaWhenActive");
            neverSleep = MyListRef.FindPropertyRelative("neverSleep");
            isKinematic = MyListRef.FindPropertyRelative("isKinematic");
            mass = MyListRef.FindPropertyRelative("mass");
            mainGraphic = MyListRef.FindPropertyRelative("mainGraphic");
            scaleAbilityGraphic = MyListRef.FindPropertyRelative("scaleAbilityGraphic");
            abilityGraphicScale = MyListRef.FindPropertyRelative("abilityGraphicScale");
            chooseLayer = MyListRef.FindPropertyRelative("chooseLayer");
            abLayer = MyListRef.FindPropertyRelative("abLayer");
            childParticle = MyListRef.FindPropertyRelative("subGraphic");
            useColliderTrigger = MyListRef.FindPropertyRelative("useColliderTrigger");
            applyGravity = MyListRef.FindPropertyRelative("applyGravity");
            applyGravityDelay = MyListRef.FindPropertyRelative("applyGravityDelay");
            travelType = MyListRef.FindPropertyRelative("travelType");
            travelPhysics = MyListRef.FindPropertyRelative("travelPhysics");
            travelDrag = MyListRef.FindPropertyRelative("travelDrag");
            customTravelScript = MyListRef.FindPropertyRelative("customTravelScript");
            crossHairRaycastRadius = MyListRef.FindPropertyRelative("crossHairRaycastRadius");
            crossHairRecordTargetOnActivation = MyListRef.FindPropertyRelative("crossHairRecordTargetOnActivation");
            crossHairRaycastReturnDistancePointOnly = MyListRef.FindPropertyRelative("crossHairRaycastReturnDistancePointOnly");
            crossHairRaycastReturnedDistance = MyListRef.FindPropertyRelative("crossHairRaycastReturnedDistance");
            targetTravel = MyListRef.FindPropertyRelative("targetTravel");
            travelWithCaster = MyListRef.FindPropertyRelative("travelWithCaster");
            boomerangMode = MyListRef.FindPropertyRelative("boomerangMode");
            boomerangDelay = MyListRef.FindPropertyRelative("boomerangDelay");
            requireCrossHair = MyListRef.FindPropertyRelative("requireCrossHairOverride");

            scrollSetUnsetRaiseEvent = MyListRef.FindPropertyRelative("scrollSetUnsetRaiseEvent");
            scrollQuickInputType = MyListRef.FindPropertyRelative("scrollQuickInputType");
            scrollQuickButton = MyListRef.FindPropertyRelative("scrollQuickButton");
            scrollQuickKey = MyListRef.FindPropertyRelative("scrollQuickKey");
            scrollSwapDuration = MyListRef.FindPropertyRelative("scrollSwapDuration");

            enableCriticalHit = MyListRef.FindPropertyRelative("enableCriticalHit");
            potencyCriticalHitBaseChance = MyListRef.FindPropertyRelative("potencyCriticalHitBaseChance");
            potencyCriticalHitEnableChanceStatModification = MyListRef.FindPropertyRelative("potencyCriticalHitEnableChanceStatModification");
            potencyCriticalHitChanceStatModification = MyListRef.FindPropertyRelative("potencyCriticalHitChanceStatModification");

            enableAbilityEffectLinks = MyListRef.FindPropertyRelative("enableAbilityEffectLinks");
            effectLinkAbilityIDs = MyListRef.FindPropertyRelative("effectLinkAbilityIDs");


#if ABC_GC_Integration
        gcEffectActionList = MyListRef.FindPropertyRelative("gcEffectActionList");
#endif

#if ABC_GC_2_Integration
        gc2EffectAction = MyListRef.FindPropertyRelative("gc2EffectAction");        
#endif


            targetFacing = MyListRef.FindPropertyRelative("targetFacing");
            noTargetStillTravel = MyListRef.FindPropertyRelative("noTargetStillTravel");
            auxiliarySoftTarget = MyListRef.FindPropertyRelative("auxiliarySoftTarget");
            startingPositionAuxiliarySoftTarget = MyListRef.FindPropertyRelative("startingPositionAuxiliarySoftTarget");
            seekTargetDelay = MyListRef.FindPropertyRelative("seekTargetDelay");
            selectedTargetRestrictTargets = MyListRef.FindPropertyRelative("selectedTargetRestrictTargets");
            selectedTargetOnlyCastOnTag = MyListRef.FindPropertyRelative("selectedTargetOnlyCastOnTag");
            continuouslyTurnToDestination = MyListRef.FindPropertyRelative("continuouslyTurnToDestination");
            affectOnlyTarget = MyListRef.FindPropertyRelative("affectOnlyTarget");
            scrollAbility = MyListRef.FindPropertyRelative("scrollAbility");
            randomlySwapAbilityPosition = MyListRef.FindPropertyRelative("randomlySwapAbilityPosition");
            LandOrAir = MyListRef.FindPropertyRelative("LandOrAir");
            castableDuringHitPrevention = MyListRef.FindPropertyRelative("castableDuringHitPrevention");
            hitPreventionWontInterruptActivation = MyListRef.FindPropertyRelative("hitPreventionWontInterruptActivation");
            forceActivation = MyListRef.FindPropertyRelative("forceActivation");
            forceActivationInterruptCurrentActivation = MyListRef.FindPropertyRelative("forceActivationInterruptCurrentActivation");
            airAbilityDistanceFromGround = MyListRef.FindPropertyRelative("airAbilityDistanceFromGround");

            tempAbilityActivationIntervalAdjustment = MyListRef.FindPropertyRelative("tempAbilityActivationIntervalAdjustment");
            neverResetOtherCombos = MyListRef.FindPropertyRelative("neverResetOtherCombos");
            abilityCombo = MyListRef.FindPropertyRelative("abilityCombo");
            comboNextActivateTime = MyListRef.FindPropertyRelative("comboNextActivateTime");
            comboHitRequired = MyListRef.FindPropertyRelative("comboHitRequired");

            triggerType = MyListRef.FindPropertyRelative("triggerType");
            inputCombo = MyListRef.FindPropertyRelative("keyInputCombo");

            keyInputType = MyListRef.FindPropertyRelative("keyInputType");
            keyButton = MyListRef.FindPropertyRelative("keyButton");
            key = MyListRef.FindPropertyRelative("key");

            globalAbilityOverrideKeyTrigger = MyListRef.FindPropertyRelative("globalAbilityOverrideKeyTrigger");

            onKeyPress = MyListRef.FindPropertyRelative("onKeyPress");
            onKeyDown = MyListRef.FindPropertyRelative("onKeyDown");
            enableMovementValidation = MyListRef.FindPropertyRelative("enableMovementValidation");
            movementValidation = MyListRef.FindPropertyRelative("movementValidation");


            requireAdditionalKeyInput = MyListRef.FindPropertyRelative("requireAdditionalKeyInput");
            additionalKeyInputType = MyListRef.FindPropertyRelative("additionalKeyInputType");
            additionalKeyButton = MyListRef.FindPropertyRelative("additionalKeyButton");
            additionalKey = MyListRef.FindPropertyRelative("additionalKey");
            additionalOnKeyPress = MyListRef.FindPropertyRelative("additionalOnKeyPress");
            additionalOnKeyDown = MyListRef.FindPropertyRelative("additionalOnKeyDown");

            abilityRecast = MyListRef.FindPropertyRelative("abilityRecast");
            startRecastAfterAbilityEnd = MyListRef.FindPropertyRelative("startRecastAfterAbilityEnd");
            travelSpeed = MyListRef.FindPropertyRelative("travelSpeed");
            travelDelay = MyListRef.FindPropertyRelative("travelDelay");
            applyTravelDuration = MyListRef.FindPropertyRelative("applyTravelDuration");
            travelDurationOriginatorTagsRequired = MyListRef.FindPropertyRelative("travelDurationOriginatorTagsRequired");
            travelDurationTime = MyListRef.FindPropertyRelative("travelDurationTime");
            travelDurationStopSuddenly = MyListRef.FindPropertyRelative("travelDurationStopSuddenly");
            duration = MyListRef.FindPropertyRelative("duration");
            abilityToggle = MyListRef.FindPropertyRelative("abilityToggle");
            canCastWhenToggled = MyListRef.FindPropertyRelative("canCastWhenToggled");
            repeatInitiatingAnimationWhilstToggled = MyListRef.FindPropertyRelative("repeatInitiatingAnimationWhilstToggled");
            stateEffects = MyListRef.FindPropertyRelative("effects");
            additionalStartPositions = MyListRef.FindPropertyRelative("additionalStartingPositions");
            onEnter = MyListRef.FindPropertyRelative("onEnter");
            onStay = MyListRef.FindPropertyRelative("onStay");
            onStayInterval = MyListRef.FindPropertyRelative("onStayInterval");
            onExit = MyListRef.FindPropertyRelative("onExit");
            particleCollision = MyListRef.FindPropertyRelative("particleCollision");

            enableCollisionAfterKeyPress = MyListRef.FindPropertyRelative("enableCollisionAfterKeyPress");
            enableCollisionAfterKeyInputType = MyListRef.FindPropertyRelative("enableCollisionAfterKeyInputType");
            enableCollisionAfterKeyButton = MyListRef.FindPropertyRelative("enableCollisionAfterKeyButton");
            enableCollisionAfterKey = MyListRef.FindPropertyRelative("enableCollisionAfterKey");

            abilityRequiredTag = MyListRef.FindPropertyRelative("abilityRequiredTag");
            abilityIgnoreTag = MyListRef.FindPropertyRelative("abilityIgnoreTag");
            startingPosition = MyListRef.FindPropertyRelative("startingPosition");
            affectOriginObject = MyListRef.FindPropertyRelative("affectOriginObject");
            affectLayer = MyListRef.FindPropertyRelative("affectLayer");

            startingPositionOnObject = MyListRef.FindPropertyRelative("startingPositionOnObject");
            startingPositionOnTag = MyListRef.FindPropertyRelative("startingPositionOnTag");
            startingPositionOffset = MyListRef.FindPropertyRelative("startingPositionOffset");
            startingPositionForwardOffset = MyListRef.FindPropertyRelative("startingPositionForwardOffset");
            startingPositionRightOffset = MyListRef.FindPropertyRelative("startingPositionRightOffset");

            startingRotation = MyListRef.FindPropertyRelative("startingRotation");
            setEulerRotation = MyListRef.FindPropertyRelative("setEulerRotation");
            prepareTime = MyListRef.FindPropertyRelative("prepareTime");
            ignoreGlobalPrepareTimeAdjustments = MyListRef.FindPropertyRelative("ignoreGlobalPrepareTimeAdjustments");
            prepareTriggerHoldRequied = MyListRef.FindPropertyRelative("prepareTriggerHoldRequied");
            abilityInitiatingBaseSpeedAdjustment = MyListRef.FindPropertyRelative("abilityInitiatingBaseSpeedAdjustment");
            ignoreGlobalInitiatingSpeedAdjustments = MyListRef.FindPropertyRelative("ignoreGlobalInitiatingSpeedAdjustments");
            modifyAbilityInitiatingBaseSpeedByStat = MyListRef.FindPropertyRelative("modifyAbilityInitiatingBaseSpeedByStat");
            abilityInitiatingBaseSpeedStatModification = MyListRef.FindPropertyRelative("abilityInitiatingBaseSpeedStatModification");
            preparingAestheticsPositionOffset = MyListRef.FindPropertyRelative("preparingAestheticsPositionOffset");
            moveInteruptPreparation = MyListRef.FindPropertyRelative("moveInteruptPreparation");
            waitForKeyBeforeInitiating = MyListRef.FindPropertyRelative("waitForKeyBeforeInitiating");
            waitBeforeInitiatingInputType = MyListRef.FindPropertyRelative("waitBeforeInitiatingInputType");
            waitBeforeInitiatingButton = MyListRef.FindPropertyRelative("waitBeforeInitiatingButton");
            waitBeforeInitiatingKey = MyListRef.FindPropertyRelative("waitBeforeInitiatingKey");
            waitForKeyBeforeInitiatingDelay = MyListRef.FindPropertyRelative("waitForKeyBeforeInitiatingDelay");
            waitForKeyAllowChangeOfTarget = MyListRef.FindPropertyRelative("waitForKeyAllowChangeOfTarget");
            distanceInteruptPreparation = MyListRef.FindPropertyRelative("distanceInteruptPreparation");
            abilityCollisionIgnores = MyListRef.FindPropertyRelative("abilityCollisionIgnores");
            impactDestroy = MyListRef.FindPropertyRelative("impactDestroy");
            destroyDelay = MyListRef.FindPropertyRelative("destroyDelay");
            destroyIgnoreTag = MyListRef.FindPropertyRelative("destroyIgnoreTag");

            includeSurroundingObject = MyListRef.FindPropertyRelative("includeSurroundingObject");
            surroundingObjectTarget = MyListRef.FindPropertyRelative("surroundingObjectTarget");
            lockSurroundingObject = MyListRef.FindPropertyRelative("lockSurroundingObject");
            minimumScatterRange = MyListRef.FindPropertyRelative("minimumScatterRange");
            maximumScatterRange = MyListRef.FindPropertyRelative("maximumScatterRange");
            surroundingObjectTargetRange = MyListRef.FindPropertyRelative("surroundingObjectTargetRange");
            destroySurroundingObject = MyListRef.FindPropertyRelative("destroySurroundingObject");
            projectileAffectSurroundingObject = MyListRef.FindPropertyRelative("projectileAffectSurroundingObject");
            surroundingObjectTargetRequired = MyListRef.FindPropertyRelative("surroundingObjectTargetRequired");
            surroundingObjectTargetRestrict = MyListRef.FindPropertyRelative("surroundingObjectTargetRestrict");
            surroundingObjectTargetAffectTag = MyListRef.FindPropertyRelative("surroundingObjectTargetAffectTag");
            surroundingObjectAuxiliarySoftTarget = MyListRef.FindPropertyRelative("surroundingObjectAuxiliarySoftTarget");

            surroundingObjectTags = MyListRef.FindPropertyRelative("surroundingObjectTags");
            surroundingObjectTagRequired = MyListRef.FindPropertyRelative("surroundingObjectTagRequired");
            surroundingObjectTagAffectTag = MyListRef.FindPropertyRelative("surroundingObjectTagAffectTag");
            surroundingObjectTagLimit = MyListRef.FindPropertyRelative("surroundingObjectTagLimit");
            surroundingObjectTagsRange = MyListRef.FindPropertyRelative("surroundingObjectTagsRange");
            applyColliderToSurroundingObjects = MyListRef.FindPropertyRelative("applyColliderToSurroundingObjects");
            applyColliderWhenProjectileReached = MyListRef.FindPropertyRelative("applyColliderWhenProjectileReached");

            sendObjectToProjectile = MyListRef.FindPropertyRelative("sendObjectToProjectile");
            objectToProjectileDuration = MyListRef.FindPropertyRelative("objectToProjectileDuration");
            useScrollAbilityAesthetics = MyListRef.FindPropertyRelative("useScrollAbilityAesthetics");
            scrollAbilityAnimateOnEntity = MyListRef.FindPropertyRelative("scrollAbilityAnimateOnEntity");
            scrollAbilityAnimateOnScrollGraphic = MyListRef.FindPropertyRelative("scrollAbilityAnimateOnScrollGraphic");
            scrollAbilityAnimateOnWeapon = MyListRef.FindPropertyRelative("scrollAbilityAnimateOnWeapon");



            scrollAbilityAestheticsPositionOffset = MyListRef.FindPropertyRelative("scrollAbilityAestheticsPositionOffset");
            scrollAbilityAnimatorParameter = MyListRef.FindPropertyRelative("scrollAbilityAnimatorParameter");
            scrollAbilityAnimatorParameterType = MyListRef.FindPropertyRelative("scrollAbilityAnimatorParameterType");
            scrollAbilityAnimatorOnValue = MyListRef.FindPropertyRelative("scrollAbilityAnimatorOnValue");
            scrollAbilityAnimatorOffValue = MyListRef.FindPropertyRelative("scrollAbilityAnimatorOffValue");
            scrollAbilityAnimatorDuration = MyListRef.FindPropertyRelative("scrollAbilityAnimatorDuration");

            scrollAbilityAnimationRunnerClip = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerClip");
            scrollAbilityAnimationRunnerMask = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerMask");
            scrollAbilityAnimationRunnerClipSpeed = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerClipSpeed");
            scrollAbilityAnimationRunnerClipDelay = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerClipDelay");
            scrollAbilityAnimationRunnerClipDuration = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerClipDuration");
            scrollAbilityAnimationRunnerOnEntity = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerOnEntity");
            scrollAbilityAnimationRunnerOnScrollGraphic = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerOnScrollGraphic");
            scrollAbilityAnimationRunnerOnWeapon = MyListRef.FindPropertyRelative("scrollAbilityAnimationRunnerOnWeapon");

            scrollAbilityDeactivateAnimationRunnerClip = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerClip");
            scrollAbilityDeactivateAnimationRunnerMask = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerMask");
            scrollAbilityDeactivateAnimationRunnerClipSpeed = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerClipSpeed");
            scrollAbilityDeactivateAnimationRunnerClipDelay = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerClipDelay");
            scrollAbilityDeactivateAnimationRunnerClipDuration = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerClipDuration");
            scrollAbilityDeactivateAnimationRunnerOnEntity = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerOnEntity");
            scrollAbilityDeactivateAnimationRunnerOnScrollGraphic = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerOnScrollGraphic");
            scrollAbilityDeactivateAnimationRunnerOnWeapon = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimationRunnerOnWeapon");


            scrollAbilityDeactivateAnimateOnEntity = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimateOnEntity");
            scrollAbilityDeactivateAnimateOnScrollGraphic = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimateOnScrollGraphic");
            scrollAbilityDeactivateAnimateOnWeapon = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimateOnWeapon");
            scrollAbilityDeactivateAnimatorParameter = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimatorParameter");
            scrollAbilityDeactivateAnimatorParameterType = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimatorParameterType");
            scrollAbilityDeactivateAnimatorOnValue = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimatorOnValue");
            scrollAbilityDeactivateAnimatorOffValue = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimatorOffValue");
            scrollAbilityDeactivateAnimatorDuration = MyListRef.FindPropertyRelative("scrollAbilityDeactivateAnimatorDuration");

            scrollAbilityParticle = MyListRef.FindPropertyRelative("scrollAbilityGraphic");
            scrollAbilityObject = MyListRef.FindPropertyRelative("scrollAbilitySubGraphic");
            scrollAbilityGraphicActivateDelay = MyListRef.FindPropertyRelative("scrollAbilityGraphicActivateDelay");
            scrollAbilityGraphicDeactivateDelay = MyListRef.FindPropertyRelative("scrollAbilityGraphicDeactivateDelay");
            scrollAbilityStartPosition = MyListRef.FindPropertyRelative("scrollAbilityStartPosition");
            scrollAbilityPositionOnObject = MyListRef.FindPropertyRelative("scrollAbilityPositionOnObject");
            scrollAbilityPositionOnTag = MyListRef.FindPropertyRelative("scrollAbilityPositionOnTag");
            scrollAbilityAestheticUseDuration = MyListRef.FindPropertyRelative("scrollAbilityAestheticDurationType");
            scrollAbilityAestheticDuration = MyListRef.FindPropertyRelative("scrollAbilityAestheticDuration");
            scrollAbilityPersistantAestheticInactivePosition = MyListRef.FindPropertyRelative("scrollAbilityPersistantAestheticInactivePosition");
            scrollAbilityPersistantAestheticInactivePositionOnObject = MyListRef.FindPropertyRelative("scrollAbilityPersistantAestheticInactivePositionOnObject");
            scrollAbilityPersistantAestheticInactivePositionOnTag = MyListRef.FindPropertyRelative("scrollAbilityPersistantAestheticInactivePositionOnTag");

            useReloadAbilityAesthetics = MyListRef.FindPropertyRelative("useReloadAbilityAesthetics");
            reloadAbilityAnimateOnEntity = MyListRef.FindPropertyRelative("reloadAbilityAnimateOnEntity");
            reloadAbilityAnimateOnScrollGraphic = MyListRef.FindPropertyRelative("reloadAbilityAnimateOnScrollGraphic");
            reloadAbilityAnimateOnWeapon = MyListRef.FindPropertyRelative("reloadAbilityAnimateOnWeapon");
            reloadAbilityAestheticsPositionOffset = MyListRef.FindPropertyRelative("reloadAbilityAestheticsPositionOffset");
            reloadAbilityAnimatorParameter = MyListRef.FindPropertyRelative("reloadAbilityAnimatorParameter");
            reloadAbilityAnimatorParameterType = MyListRef.FindPropertyRelative("reloadAbilityAnimatorParameterType");
            reloadAbilityAnimatorOnValue = MyListRef.FindPropertyRelative("reloadAbilityAnimatorOnValue");
            reloadAbilityAnimatorOffValue = MyListRef.FindPropertyRelative("reloadAbilityAnimatorOffValue");
            reloadAbilityParticle = MyListRef.FindPropertyRelative("reloadAbilityGraphic");
            reloadAbilityObject = MyListRef.FindPropertyRelative("reloadAbilitySubGraphic");
            reloadAbilityAestheticDuration = MyListRef.FindPropertyRelative("reloadAbilityAestheticDuration");
            reloadAbilityStartPosition = MyListRef.FindPropertyRelative("reloadAbilityStartPosition");
            reloadAbilityPositionOnObject = MyListRef.FindPropertyRelative("reloadAbilityPositionOnObject");
            reloadAbilityPositionOnTag = MyListRef.FindPropertyRelative("reloadAbilityPositionOnTag");

            reloadAbilityAnimationRunnerClip = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerClip");
            reloadAbilityAnimationRunnerMask = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerMask");
            reloadAbilityAnimationRunnerClipSpeed = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerClipSpeed");
            reloadAbilityAnimationRunnerClipDelay = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerClipDelay");
            reloadAbilityAnimationRunnerOnEntity = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerOnEntity");
            reloadAbilityAnimationRunnerOnScrollGraphic = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerOnScrollGraphic");
            reloadAbilityAnimationRunnerOnWeapon = MyListRef.FindPropertyRelative("reloadAbilityAnimationRunnerOnWeapon");


            useProjectileToStartPosition = MyListRef.FindPropertyRelative("useProjectileToStartPosition");
            projectileToStartType = MyListRef.FindPropertyRelative("projectileToStartType");
            projToStartDelay = MyListRef.FindPropertyRelative("projToStartDelay");
            useOriginalProjectilePTS = MyListRef.FindPropertyRelative("useOriginalProjectilePTS");
            projToStartPosParticle = MyListRef.FindPropertyRelative("projToStartPosGraphic");
            projToStartPosObject = MyListRef.FindPropertyRelative("projToStartPosSubGraphic");
            projToStartPosDuration = MyListRef.FindPropertyRelative("projToStartPosDuration");
            projToStartStartingPosition = MyListRef.FindPropertyRelative("projToStartStartingPosition");
            projToStartRotateToTarget = MyListRef.FindPropertyRelative("projToStartRotateToTarget");
            projToStartRotation = MyListRef.FindPropertyRelative("projToStartRotation");
            projToStartSetEulerRotation = MyListRef.FindPropertyRelative("projToStartSetEulerRotation");
            projToStartPositionOnObject = MyListRef.FindPropertyRelative("projToStartPositionOnObject");
            projToStartPositionOnTag = MyListRef.FindPropertyRelative("projToStartPositionOnTag");
            projToStartPositionOffset = MyListRef.FindPropertyRelative("projToStartPositionOffset");
            projToStartPositionForwardOffset = MyListRef.FindPropertyRelative("projToStartPositionForwardOffset");
            projToStartPositionRightOffset = MyListRef.FindPropertyRelative("projToStartPositionRightOffset");
            projToStartHoverOnSpot = MyListRef.FindPropertyRelative("projToStartHoverOnSpot");
            projToStartHoverDistance = MyListRef.FindPropertyRelative("projToStartHoverDistance");
            projToStartmoveWithTarget = MyListRef.FindPropertyRelative("projToStartMoveWithTarget");
            projToStartTravelToAbilityStartPosition = MyListRef.FindPropertyRelative("projToStartTravelToAbilityStartPosition");
            projToStartReachPositionTime = MyListRef.FindPropertyRelative("projToStartReachPositionTime");
            projToStartTravelDelay = MyListRef.FindPropertyRelative("projToStartTravelDelay");

            UseAmmo = MyListRef.FindPropertyRelative("UseAmmo");
            useEquippedWeaponAmmo = MyListRef.FindPropertyRelative("useEquippedWeaponAmmo");
            ammoCount = MyListRef.FindPropertyRelative("ammoCount");
            reduceAmmoWhilstActive = MyListRef.FindPropertyRelative("reduceAmmoWhilstActive");
            useReload = MyListRef.FindPropertyRelative("useReload");
            reloadDuration = MyListRef.FindPropertyRelative("reloadDuration");
            reloadRestrictAbilityActivationDuration = MyListRef.FindPropertyRelative("reloadRestrictAbilityActivationDuration");
            clipSize = MyListRef.FindPropertyRelative("clipSize");
            autoReloadWhenRequired = MyListRef.FindPropertyRelative("autoReloadWhenRequired");
            reloadFillClip = MyListRef.FindPropertyRelative("reloadFillClip");
            reloadFillClipRepeatGraphic = MyListRef.FindPropertyRelative("reloadFillClipRepeatGraphic");

            bounceMode = MyListRef.FindPropertyRelative("bounceMode");
            bounceAmount = MyListRef.FindPropertyRelative("bounceAmount");
            startBounceTagRequired = MyListRef.FindPropertyRelative("startBounceTagRequired");
            startBounceRequiredTags = MyListRef.FindPropertyRelative("startBounceRequiredTags");
            bounceTarget = MyListRef.FindPropertyRelative("bounceTarget");
            bounceTag = MyListRef.FindPropertyRelative("bounceTag");
            bounceRange = MyListRef.FindPropertyRelative("bounceRange");
            bounceOnCaster = MyListRef.FindPropertyRelative("bounceOnCaster");
            enableRandomBounce = MyListRef.FindPropertyRelative("enableRandomBounce");
            bouncePositionOffset = MyListRef.FindPropertyRelative("bouncePositionOffset");
            bouncePositionForwardOffset = MyListRef.FindPropertyRelative("bouncePositionForwardOffset");
            bouncePositionRightOffset = MyListRef.FindPropertyRelative("bouncePositionRightOffset");


            useGraphicRadius = MyListRef.FindPropertyRelative("useGraphicRadius");
            colliderRadius = MyListRef.FindPropertyRelative("colliderRadius");
            applyColliderSettingsToParent = MyListRef.FindPropertyRelative("applyColliderSettingsToParent");
            applyColliderSettingsToChildren = MyListRef.FindPropertyRelative("applyColliderSettingsToChildren");




            meleeKeepRotatingToSelectedTarget = MyListRef.FindPropertyRelative("meleeKeepRotatingToSelectedTarget");
            rotateToSelectedTarget = MyListRef.FindPropertyRelative("rotateToSelectedTarget");
            noTargetRotateBehaviour = MyListRef.FindPropertyRelative("noTargetRotateBehaviour");
            hitsStopMeleeAttack = MyListRef.FindPropertyRelative("hitsStopMeleeAttack");

            colliderOffset = MyListRef.FindPropertyRelative("colliderOffset");
            useDestroySplashEffect = MyListRef.FindPropertyRelative("useDestroySplashEffect");
            destroySplashRadius = MyListRef.FindPropertyRelative("destroySplashRadius");
            destroySplashExplosion = MyListRef.FindPropertyRelative("destroySplashExplosion");
            destroySplashExplosionPower = MyListRef.FindPropertyRelative("destroySplashExplosionPower");
            destroySplashExplosionTagLimit = MyListRef.FindPropertyRelative("destroySplashExplosionTagLimit");
            destroySplashExplosionAffectTag = MyListRef.FindPropertyRelative("destroySplashExplosionAffectTag");
            destroySplashExplosionRadius = MyListRef.FindPropertyRelative("destroySplashExplosionRadius");
            destroySplashExplosionUplift = MyListRef.FindPropertyRelative("destroySplashExplosionUplift");


            globalImpactRequiredTag = MyListRef.FindPropertyRelative("globalImpactRequiredTag");

            modifyGameSpeedOnInitiation = MyListRef.FindPropertyRelative("modifyGameSpeedOnInitiation");
            modifyGameSpeedOnInitiationSpeedFactor = MyListRef.FindPropertyRelative("modifyGameSpeedOnInitiationSpeedFactor");
            modifyGameSpeedOnInitiationDelay = MyListRef.FindPropertyRelative("modifyGameSpeedOnInitiationDelay");
            modifyGameSpeedOnInitiationDuration = MyListRef.FindPropertyRelative("modifyGameSpeedOnInitiationDuration");

            modifyGameSpeedOnImpact = MyListRef.FindPropertyRelative("modifyGameSpeedOnImpact");
            modifyGameSpeedOnImpactSpeedFactor = MyListRef.FindPropertyRelative("modifyGameSpeedOnImpactSpeedFactor");
            modifyGameSpeedOnImpactDuration = MyListRef.FindPropertyRelative("modifyGameSpeedOnImpactDuration");
            modifyGameSpeedOnImpactDelay = MyListRef.FindPropertyRelative("modifyGameSpeedOnImpactDelay");

            shakeCameraOnInitiation = MyListRef.FindPropertyRelative("shakeCameraOnInitiation");
            shakeCameraOnInitiationDelay = MyListRef.FindPropertyRelative("shakeCameraOnInitiationDelay");
            shakeCameraOnInitiationDuration = MyListRef.FindPropertyRelative("shakeCameraOnInitiationDuration");
            shakeCameraOnInitiationAmount = MyListRef.FindPropertyRelative("shakeCameraOnInitiationAmount");
            shakeCameraOnInitiationSpeed = MyListRef.FindPropertyRelative("shakeCameraOnInitiationSpeed");

            shakeCameraOnImpact = MyListRef.FindPropertyRelative("shakeCameraOnImpact");
            shakeCameraOnImpactDelay = MyListRef.FindPropertyRelative("shakeCameraOnImpactDelay");
            shakeCameraOnImpactDuration = MyListRef.FindPropertyRelative("shakeCameraOnImpactDuration");
            shakeCameraOnImpactAmount = MyListRef.FindPropertyRelative("shakeCameraOnImpactAmount");
            shakeCameraOnImpactSpeed = MyListRef.FindPropertyRelative("shakeCameraOnImpactSpeed");

            pushEntityOnImpact = MyListRef.FindPropertyRelative("pushEntityOnImpact");
            pushEntityOnImpactDelay = MyListRef.FindPropertyRelative("pushEntityOnImpactDelay");
            pushEntityOnImpactAmount = MyListRef.FindPropertyRelative("pushEntityOnImpactAmount");
            pushEntityOnImpactLiftForce = MyListRef.FindPropertyRelative("pushEntityOnImpactLiftForce");

            defyEntityGravityOnImpact = MyListRef.FindPropertyRelative("defyEntityGravityOnImpact");
            defyEntityGravityOnImpactDelay = MyListRef.FindPropertyRelative("defyEntityGravityOnImpactDelay");
            defyEntityGravityOnImpactDuration = MyListRef.FindPropertyRelative("defyEntityGravityOnImpactDuration");


            shakeEntityOnImpact = MyListRef.FindPropertyRelative("shakeEntityOnImpact");
            shakeEntityOnImpactShakeAmount = MyListRef.FindPropertyRelative("shakeEntityOnImpactShakeAmount");
            shakeEntityOnImpactShakeDecay = MyListRef.FindPropertyRelative("shakeEntityOnImpactShakeDecay");
            shakeEntityOnImpactShakeDelay = MyListRef.FindPropertyRelative("shakeEntityOnImpactShakeDelay");

            attachToObjectOnImpact = MyListRef.FindPropertyRelative("attachToObjectOnImpact");
            attachToObjectProbabilityMinValue = MyListRef.FindPropertyRelative("attachToObjectProbabilityMinValue");
            attachToObjectProbabilityMaxValue = MyListRef.FindPropertyRelative("attachToObjectProbabilityMaxValue");
            attachToObjectStickOutFactor = MyListRef.FindPropertyRelative("attachToObjectStickOutFactor");
            attachToObjectNearestBone = MyListRef.FindPropertyRelative("attachToObjectNearestBone");


            switchColorOnImpact = MyListRef.FindPropertyRelative("switchColorOnImpact");
            switchColorOnImpactColor = MyListRef.FindPropertyRelative("switchColorOnImpactColor");
            switchColorOnImpactDelay = MyListRef.FindPropertyRelative("switchColorOnImpactDelay");
            switchColorOnImpactDuration = MyListRef.FindPropertyRelative("switchColorOnImpactDuration");
            switchColorOnImpactUseEmission = MyListRef.FindPropertyRelative("switchColorOnImpactUseEmission");

            enableHitStopOnImpact = MyListRef.FindPropertyRelative("enableHitStopOnImpact");
            hitStopOnImpactDelay = MyListRef.FindPropertyRelative("hitStopOnImpactDelay");
            hitStopOnImpactDuration = MyListRef.FindPropertyRelative("hitStopOnImpactDuration");
            hitStopOnImpactEntityHitDelay = MyListRef.FindPropertyRelative("hitStopOnImpactEntityHitDelay");


            useInitiatingAesthetics = MyListRef.FindPropertyRelative("useInitiatingAesthetics");

#if ABC_GC_Integration
        gcInitiatingActionList = MyListRef.FindPropertyRelative("gcInitiatingActionList");
#endif

#if ABC_GC_2_Integration
        gc2InitiatingAction = MyListRef.FindPropertyRelative("gc2InitiatingAction");
#endif

            initiatingAnimateOnEntity = MyListRef.FindPropertyRelative("initiatingAnimateOnEntity");
            initiatingAnimateOnScrollGraphic = MyListRef.FindPropertyRelative("initiatingAnimateOnScrollGraphic");
            initiatingAnimateOnWeapon = MyListRef.FindPropertyRelative("initiatingAnimateOnWeapon");
            defyGravityInitiating = MyListRef.FindPropertyRelative("defyGravityInitiating");
            defyGravityInitiatingDuration = MyListRef.FindPropertyRelative("defyGravityInitiatingDuration");
            defyGravityInitiatingDelay = MyListRef.FindPropertyRelative("defyGravityInitiatingDelay");
            defyGravityInitiatingRaiseEvent = MyListRef.FindPropertyRelative("defyGravityInitiatingRaiseEvent");
            initiatingAestheticsPositionOffset = MyListRef.FindPropertyRelative("initiatingAestheticsPositionOffset");
            initiatingAnimatorParameter = MyListRef.FindPropertyRelative("initiatingAnimatorParameter");
            initiatingParticle = MyListRef.FindPropertyRelative("initiatingGraphic");
            initiatingObject = MyListRef.FindPropertyRelative("initiatingSubGraphic");
            initiatingAestheticDelay = MyListRef.FindPropertyRelative("initiatingAestheticDelay");
            initiatingAestheticDuration = MyListRef.FindPropertyRelative("initiatingAestheticDuration");
            initiatingAestheticActivateWithAbility = MyListRef.FindPropertyRelative("initiatingAestheticActivateWithAbility");
            initiatingAestheticDetachFromParentAfterDelay = MyListRef.FindPropertyRelative("initiatingAestheticDetachFromParentAfterDelay");
            initiatingAestheticDetachDelay = MyListRef.FindPropertyRelative("initiatingAestheticDetachDelay");
            intiatingProjectileDelayType = MyListRef.FindPropertyRelative("intiatingProjectileDelayType");
            delayBetweenInitiatingAndProjectile = MyListRef.FindPropertyRelative("delayBetweenInitiatingAndProjectile");
            initiatingProjectileDelayAnimationPercentage = MyListRef.FindPropertyRelative("initiatingProjectileDelayAnimationPercentage");
            initiatingStartPosition = MyListRef.FindPropertyRelative("initiatingStartPosition");
            initiatingPositionOnObject = MyListRef.FindPropertyRelative("initiatingPositionOnObject");
            initiatingPositionOnTag = MyListRef.FindPropertyRelative("initiatingPositionOnTag");
            initiatingAnimatorParameterType = MyListRef.FindPropertyRelative("initiatingAnimatorParameterType");
            initiatingAnimatorOnValue = MyListRef.FindPropertyRelative("initiatingAnimatorOnValue");
            initiatingAnimatorOffValue = MyListRef.FindPropertyRelative("initiatingAnimatorOffValue");
            initiatingAnimatorDuration = MyListRef.FindPropertyRelative("initiatingAnimatorDuration");
            initiatingUseWeaponTrail = MyListRef.FindPropertyRelative("initiatingUseWeaponTrail");
            initiatingWeaponTrailGraphicIteration = MyListRef.FindPropertyRelative("initiatingWeaponTrailGraphicIteration");

            initiatingAnimationRunnerClip = MyListRef.FindPropertyRelative("initiatingAnimationRunnerClip");
            initiatingAnimationRunnerMask = MyListRef.FindPropertyRelative("initiatingAnimationRunnerMask");
            initiatingAnimationRunnerClipSpeed = MyListRef.FindPropertyRelative("initiatingAnimationRunnerClipSpeed");
            initiatingAnimationRunnerClipDelay = MyListRef.FindPropertyRelative("initiatingAnimationRunnerClipDelay");
            initiatingAnimationRunnerClipDuration = MyListRef.FindPropertyRelative("initiatingAnimationRunnerClipDuration");
            initiatingAnimationRunnerOnEntity = MyListRef.FindPropertyRelative("initiatingAnimationRunnerOnEntity");
            initiatingAnimationRunnerOnScrollGraphic = MyListRef.FindPropertyRelative("initiatingAnimationRunnerOnScrollGraphic");
            initiatingAnimationRunnerOnWeapon = MyListRef.FindPropertyRelative("initiatingAnimationRunnerOnWeapon");

            moveSelfWhenInitiating = MyListRef.FindPropertyRelative("moveSelfWhenInitiating");
            moveSelfInitiatingOffset = MyListRef.FindPropertyRelative("moveSelfInitiatingOffset");
            moveSelfInitiatingForwardOffset = MyListRef.FindPropertyRelative("moveSelfInitiatingForwardOffset");
            moveSelfInitiatingRightOffset = MyListRef.FindPropertyRelative("moveSelfInitiatingRightOffset");
            moveSelfInitiatingDelay = MyListRef.FindPropertyRelative("moveSelfInitiatingDelay");
            moveSelfInitiatingDuration = MyListRef.FindPropertyRelative("moveSelfInitiatingDuration");

            moveSelfToTargetWhenInitiating = MyListRef.FindPropertyRelative("moveSelfToTargetWhenInitiating");
            moveSelfToTargetInitiatingDelay = MyListRef.FindPropertyRelative("moveSelfToTargetInitiatingDelay");
            moveSelfToTargetInitiatingDuration = MyListRef.FindPropertyRelative("moveSelfToTargetInitiatingDuration");
            moveSelfToTargetInitiatingStopDistance = MyListRef.FindPropertyRelative("moveSelfToTargetInitiatingStopDistance");
            moveSelfToTargetInitiatingOffset = MyListRef.FindPropertyRelative("moveSelfToTargetInitiatingOffset");
            moveSelfToTargetInitiatingForwardOffset = MyListRef.FindPropertyRelative("moveSelfToTargetInitiatingForwardOffset");
            moveSelfToTargetInitiatingRightOffset = MyListRef.FindPropertyRelative("moveSelfToTargetInitiatingRightOffset");

            initiatingAestheticsPositionForwardOffset = MyListRef.FindPropertyRelative("initiatingAestheticsPositionForwardOffset");
            initiatingAestheticsPositionRightOffset = MyListRef.FindPropertyRelative("initiatingAestheticsPositionRightOffset");
            preparingAestheticsPositionForwardOffset = MyListRef.FindPropertyRelative("preparingAestheticsPositionForwardOffset");
            preparingAestheticsPositionRightOffset = MyListRef.FindPropertyRelative("preparingAestheticsPositionRightOffset");
            scrollAbilityAestheticsPositionForwardOffset = MyListRef.FindPropertyRelative("scrollAbilityAestheticsPositionForwardOffset");
            scrollAbilityAestheticsPositionRightOffset = MyListRef.FindPropertyRelative("scrollAbilityAestheticsPositionRightOffset");
            reloadAbilityAestheticsPositionForwardOffset = MyListRef.FindPropertyRelative("reloadAbilityAestheticsPositionForwardOffset");
            reloadAbilityAestheticsPositionRightOffset = MyListRef.FindPropertyRelative("reloadAbilityAestheticsPositionRightOffset");
            selectedTargetForwardOffset = MyListRef.FindPropertyRelative("selectedTargetForwardOffset");
            selectedTargetRightOffset = MyListRef.FindPropertyRelative("selectedTargetRightOffset");
            abilityCanMiss = MyListRef.FindPropertyRelative("abilityCanMiss");

            useAbilityEndAesthetics = MyListRef.FindPropertyRelative("useAbilityEndAesthetics");
            abilityEndUseEffectGraphic = MyListRef.FindPropertyRelative("abilityEndUseEffectGraphic");
            abilityEndActivateOnEnvironmentOnly = MyListRef.FindPropertyRelative("abilityEndActivateOnEnvironmentOnly");
            abEndParticle = MyListRef.FindPropertyRelative("abilityEndGraphic");
            abEndObject = MyListRef.FindPropertyRelative("abilityEndSubGraphic");
            scaleAbilityEndGraphic = MyListRef.FindPropertyRelative("scaleAbilityEndGraphic");
            abilityEndGraphicScale = MyListRef.FindPropertyRelative("abilityEndGraphicScale");

            abEndAestheticDuration = MyListRef.FindPropertyRelative("abEndAestheticDuration");

            showPrepareTimeOnGUI = MyListRef.FindPropertyRelative("showPrepareTimeOnGUI");
            defyGravityPreparing = MyListRef.FindPropertyRelative("defyGravityPreparing");
            defyGravityPreparingDuration = MyListRef.FindPropertyRelative("defyGravityPreparingDuration");
            defyGravityPreparingDelay = MyListRef.FindPropertyRelative("defyGravityPreparingDelay");
            defyGravityPreparingRaiseEvent = MyListRef.FindPropertyRelative("defyGravityPreparingRaiseEvent");
            usePreparingAesthetics = MyListRef.FindPropertyRelative("usePreparingAesthetics");

#if ABC_GC_Integration
        gcPreparingActionList = MyListRef.FindPropertyRelative("gcPreparingActionList");
#endif

#if ABC_GC_2_Integration
        gc2PreparingAction = MyListRef.FindPropertyRelative("gc2PreparingAction");
#endif

            preparingAnimatorParameter = MyListRef.FindPropertyRelative("preparingAnimatorParameter");
            preparingAnimatorParameterType = MyListRef.FindPropertyRelative("preparingAnimatorParameterType");
            preparingAnimateOnEntity = MyListRef.FindPropertyRelative("preparingAnimateOnEntity");
            preparingAnimateOnScrollGraphic = MyListRef.FindPropertyRelative("preparingAnimateOnScrollGraphic");
            preparingAnimateOnWeapon = MyListRef.FindPropertyRelative("preparingAnimateOnWeapon");
            preparingAnimatorOnValue = MyListRef.FindPropertyRelative("preparingAnimatorOnValue");
            preparingAnimatorOffValue = MyListRef.FindPropertyRelative("preparingAnimatorOffValue");
            preparingParticle = MyListRef.FindPropertyRelative("preparingGraphic");
            preparingObject = MyListRef.FindPropertyRelative("preparingSubGraphic");
            preparingAestheticDurationUsePrepareTime = MyListRef.FindPropertyRelative("preparingAestheticDurationUsePrepareTime");
            preparingAestheticDuration = MyListRef.FindPropertyRelative("preparingAestheticDuration");
            preparingStartPosition = MyListRef.FindPropertyRelative("preparingStartPosition");
            preparingPositionOnObject = MyListRef.FindPropertyRelative("preparingPositionOnObject");
            preparingPositionOnTag = MyListRef.FindPropertyRelative("preparingPositionOnTag");

            moveSelfToTargetWhenPreparing = MyListRef.FindPropertyRelative("moveSelfToTargetWhenPreparing");
            moveSelfToTargetPreparingDelay = MyListRef.FindPropertyRelative("moveSelfToTargetPreparingDelay");
            moveSelfToTargetPreparingDuration = MyListRef.FindPropertyRelative("moveSelfToTargetPreparingDuration");
            moveSelfToTargetPreparingStopDistance = MyListRef.FindPropertyRelative("moveSelfToTargetPreparingStopDistance");
            moveSelfToTargetActivatePreparingAnimationOnlyWhenMoving = MyListRef.FindPropertyRelative("moveSelfToTargetActivatePreparingAnimationOnlyWhenMoving");
            moveSelfToTargetPreparingOffset = MyListRef.FindPropertyRelative("moveSelfToTargetPreparingOffset");
            moveSelfToTargetPreparingForwardOffset = MyListRef.FindPropertyRelative("moveSelfToTargetPreparingForwardOffset");
            moveSelfToTargetPreparingRightOffset = MyListRef.FindPropertyRelative("moveSelfToTargetPreparingRightOffset");

            preparingAnimationRunnerClip = MyListRef.FindPropertyRelative("preparingAnimationRunnerClip");
            preparingAnimationRunnerMask = MyListRef.FindPropertyRelative("preparingAnimationRunnerMask");
            preparingAnimationRunnerClipSpeed = MyListRef.FindPropertyRelative("preparingAnimationRunnerClipSpeed");
            preparingAnimationRunnerClipDelay = MyListRef.FindPropertyRelative("preparingAnimationRunnerClipDelay");
            preparingAnimationRunnerOnEntity = MyListRef.FindPropertyRelative("preparingAnimationRunnerOnEntity");
            preparingAnimationRunnerOnScrollGraphic = MyListRef.FindPropertyRelative("preparingAnimationRunnerOnScrollGraphic");
            preparingAnimationRunnerOnWeapon = MyListRef.FindPropertyRelative("preparingAnimationRunnerOnWeapon");


            moveSelfWhenPreparing = MyListRef.FindPropertyRelative("moveSelfWhenPreparing");
            moveSelfPreparingOffset = MyListRef.FindPropertyRelative("moveSelfPreparingOffset");
            moveSelfPreparingForwardOffset = MyListRef.FindPropertyRelative("moveSelfPreparingForwardOffset");
            moveSelfPreparingRightOffset = MyListRef.FindPropertyRelative("moveSelfPreparingRightOffset");
            moveSelfPreparingDelay = MyListRef.FindPropertyRelative("moveSelfPreparingDelay");
            moveSelfPreparingDuration = MyListRef.FindPropertyRelative("moveSelfPreparingDuration");

            colliderTimeDelay = MyListRef.FindPropertyRelative("colliderTimeDelay");
            colliderDelayTime = MyListRef.FindPropertyRelative("colliderDelayTime");
            colliderKeyPressDelay = MyListRef.FindPropertyRelative("colliderKeyPressDelay");
            colliderDelayInputType = MyListRef.FindPropertyRelative("colliderDelayInputType");
            colliderDelayButton = MyListRef.FindPropertyRelative("colliderDelayButton");
            colliderDelayKey = MyListRef.FindPropertyRelative("colliderDelayKey");
            ignoreActiveTerrain = MyListRef.FindPropertyRelative("ignoreActiveTerrain");
            overrideIgnoreAbilityCollision = MyListRef.FindPropertyRelative("overrideIgnoreAbilityCollision");
            overrideWeaponBlocking = MyListRef.FindPropertyRelative("overrideWeaponBlocking");
            overrideWeaponParrying = MyListRef.FindPropertyRelative("overrideWeaponParrying");
            reduceWeaponBlockDurability = MyListRef.FindPropertyRelative("reduceWeaponBlockDurability");
            activateAnimationFromHit = MyListRef.FindPropertyRelative("activateAnimationFromHit");
            activateAnimationFromHitDelay = MyListRef.FindPropertyRelative("activateAnimationFromHitDelay");
            activateAnimationFromHitUseAirAnimation = MyListRef.FindPropertyRelative("activateAnimationFromHitUseAirAnimation");
            activateSpecificHitAnimation = MyListRef.FindPropertyRelative("activateSpecificHitAnimation");
            hitAnimationToActivate = MyListRef.FindPropertyRelative("hitAnimationToActivate");
            activateSpecificHitAnimationUseClip = MyListRef.FindPropertyRelative("activateSpecificHitAnimationUseClip");
            hitAnimationClipToActivate = MyListRef.FindPropertyRelative("hitAnimationClipToActivate");

            persistIK = MyListRef.FindPropertyRelative("persistIK");

            spawnObject = MyListRef.FindPropertyRelative("spawnObject");
            spawningObject = MyListRef.FindPropertyRelative("spawningObject");
            spawnObjectOnDestroy = MyListRef.FindPropertyRelative("spawnObjectOnDestroy");
            spawnObjectOnCollide = MyListRef.FindPropertyRelative("spawnObjectOnCollide");

            useRange = MyListRef.FindPropertyRelative("useRange");
            selectedTargetRangeGreaterThan = MyListRef.FindPropertyRelative("selectedTargetRangeGreaterThan");
            selectedTargetRangeLessThan = MyListRef.FindPropertyRelative("selectedTargetRangeLessThan");
            selectedTargetOffset = MyListRef.FindPropertyRelative("selectedTargetOffset");
            addAbilityCollider = MyListRef.FindPropertyRelative("addAbilityCollider");
            abilityType = MyListRef.FindPropertyRelative("abilityType");
            rayCastRadius = MyListRef.FindPropertyRelative("rayCastRadius");
            raycastHitAmount = MyListRef.FindPropertyRelative("raycastHitAmount");
            raycastBlockable = MyListRef.FindPropertyRelative("raycastBlockable");
            raycastIgnoreTerrain = MyListRef.FindPropertyRelative("raycastIgnoreTerrain");
            abilityBeforeTarget = MyListRef.FindPropertyRelative("abilityBeforeTarget");
            loopTillTargetFound = MyListRef.FindPropertyRelative("loopTillTargetFound");

            abilityBeforeTargetWorldIndicatorScaleToEffectRadius = MyListRef.FindPropertyRelative("abilityBeforeTargetWorldIndicatorScaleToEffectRadius");
            abilityBeforeTargetWorldIndicatorScale = MyListRef.FindPropertyRelative("abilityBeforeTargetWorldIndicatorScale");
            abilityBeforeTargetMouseTargetIndicatorLength = MyListRef.FindPropertyRelative("abilityBeforeTargetMouseTargetIndicatorLength");

            abilityMouseTargetIndicatorImage = MyListRef.FindPropertyRelative("abilityMouseTargetIndicatorImage");
            abilityRangeIndicatorImage = MyListRef.FindPropertyRelative("abilityRangeIndicatorImage");
            abilityWorldTargetIndicatorImage = MyListRef.FindPropertyRelative("abilityWorldTargetIndicatorImage");

            rayCastSingleHit = MyListRef.FindPropertyRelative("rayCastSingleHit");
            rayCastLength = MyListRef.FindPropertyRelative("rayCastLength");
            maxActiveAtOnce = MyListRef.FindPropertyRelative("maxActiveAtOnce");
            limitActiveAtOnce = MyListRef.FindPropertyRelative("limitActiveAtOnce");
            abilityActivationRaiseEvent = MyListRef.FindPropertyRelative("abilityActivationRaiseEvent");
            abilityActivationCompleteRaiseEvent = MyListRef.FindPropertyRelative("abilityActivationCompleteRaiseEvent");
            abilityActivationCompleteEventType = MyListRef.FindPropertyRelative("abilityActivationCompleteEventType");

            loggingEnabled = MyListRef.FindPropertyRelative("loggingEnabled");
            stopMovementOnInitiate = MyListRef.FindPropertyRelative("stopMovementOnInitiate");
            stopMovementOnInitiateDuration = MyListRef.FindPropertyRelative("stopMovementOnInitiateDuration");
            stopMovementOnInitiateFreezePosition = MyListRef.FindPropertyRelative("stopMovementOnInitiateFreezePosition");
            stopMovementOnInitiateDisableComponents = MyListRef.FindPropertyRelative("stopMovementOnInitiateDisableComponents");
            stopMovementOnInitiateRaiseEvent = MyListRef.FindPropertyRelative("stopMovementOnInitiateRaiseEvent");

            stopMovementOnPreparing = MyListRef.FindPropertyRelative("stopMovementOnPreparing");
            stopMovementOnPreparingDuration = MyListRef.FindPropertyRelative("stopMovementOnPreparingDuration");
            stopMovementOnPreparingFreezePosition = MyListRef.FindPropertyRelative("stopMovementOnPreparingFreezePosition");
            stopMovementOnPreparingDisableComponents = MyListRef.FindPropertyRelative("stopMovementOnPreparingDisableComponents");
            stopMovementOnPreparingRaiseEvent = MyListRef.FindPropertyRelative("stopMovementOnPreparingRaiseEvent");


            mouseForwardLockX = MyListRef.FindPropertyRelative("mouseForwardLockX");
            mouseForwardLockY = MyListRef.FindPropertyRelative("mouseForwardLockY");
            mouseForwardLockZ = MyListRef.FindPropertyRelative("mouseForwardLockZ");
            mouseFrontOnly = MyListRef.FindPropertyRelative("mouseFrontOnly");

            travelNearestTagList = MyListRef.FindPropertyRelative("travelNearestTagList");
            travelNearestTagRange = MyListRef.FindPropertyRelative("travelNearestTagRange");
            travelNearestTagIgnoreOriginator = MyListRef.FindPropertyRelative("travelNearestTagIgnoreOriginator");
            travelNearestTagRandomiseSearch = MyListRef.FindPropertyRelative("travelNearestTagRandomiseSearch");
        }

        void GetSharedStartingPositionTargetAndOnWorldSettings() {

            ResetLabelWidth();


            EditorGUIUtility.labelWidth = 110;
            EditorGUILayout.PropertyField(useRange);

            EditorGUIUtility.labelWidth = 130;
            if (useRange.boolValue == true) {

                EditorGUILayout.PropertyField(selectedTargetRangeGreaterThan, new GUIContent("Range Greater Than"), GUILayout.MaxWidth(210));
                EditorGUILayout.PropertyField(selectedTargetRangeLessThan, new GUIContent("Range Less Than"), GUILayout.MaxWidth(210));

            }

            InspectorHelpBox("Ability will only activate if target is in the range defined");

            EditorGUIUtility.labelWidth = 140;

            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target") {

                EditorGUILayout.PropertyField(targetFacing, new GUIContent("Face Target Required"));
                InspectorHelpBox("If the entity has a target then the Ability will only activate if the entity is facing the target");

                EditorGUILayout.PropertyField(selectedTargetRestrictTargets, new GUIContent("Restrict Targets"));
                if (selectedTargetRestrictTargets.boolValue == true) {
                    InspectorListBox("Only Activate On Following Tags", selectedTargetOnlyCastOnTag);
                }
            }


            EditorGUILayout.Space();
        }

        void GetSharedAbilityBeforeTargetSettings() {


            EditorGUIUtility.labelWidth = 140;
            EditorGUILayout.PropertyField(abilityBeforeTarget);
            InspectorHelpBox("If ticked then the user will have to select the target after the Ability key has been pressed.");


            if (abilityBeforeTarget.boolValue == true) {

                EditorGUILayout.PropertyField(loopTillTargetFound, new GUIContent("Loop Till Target Found"));
                InspectorHelpBox("If disabled then the ability will cancel if the target was not correctly selected, else it will wait until a correct target is selected");


                if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(abilityWorldTargetIndicatorImage, new GUIContent("World Indicator Image"));

                    if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {

                        Texture texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_WorldTargetIndicatorImage", typeof(Texture)) as Texture;

                        if (texture != null) {
                            abilityWorldTargetIndicatorImage.FindPropertyRelative("refVal").objectReferenceValue = texture;
                            abilityWorldTargetIndicatorImage.FindPropertyRelative("refName").stringValue = texture.name;
                        }

                    }
                    EditorGUILayout.EndHorizontal();


                    InspectorHelpBox("The indicator which will display showing the world position the ability will travel towards");

                    EditorGUIUtility.labelWidth = 230;
                    EditorGUILayout.PropertyField(abilityBeforeTargetWorldIndicatorScaleToEffectRadius, new GUIContent("Scale World Indicator to Effect Radius"));

                    EditorGUIUtility.labelWidth = 140;

                    if (abilityBeforeTargetWorldIndicatorScaleToEffectRadius.boolValue == false) {
                        EditorGUILayout.PropertyField(abilityBeforeTargetWorldIndicatorScale, new GUIContent("World Indicator Scale"), GUILayout.MaxWidth(210));
                    }
                    InspectorHelpBox("If true then the world indicate will scale to show the effect radius of the ability or if false a scale can be set");


                }

                if (useRange.boolValue == true && selectedTargetRangeGreaterThan.floatValue == 0) {

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(abilityRangeIndicatorImage, new GUIContent("Range Indicator Image"));

                    if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {

                        Texture texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_RangeIndicatorImage", typeof(Texture)) as Texture;

                        if (texture != null) {
                            abilityRangeIndicatorImage.FindPropertyRelative("refVal").objectReferenceValue = texture;
                            abilityRangeIndicatorImage.FindPropertyRelative("refName").stringValue = texture.name;
                        }

                    }
                    EditorGUILayout.EndHorizontal();


                    InspectorHelpBox("The indicator which will display showing the range of the ability");
                }


            }
        }

        public void GetAbilitySettings() {

            #region setup
            // keep up to date with count
            AbilityCount = meAbilityList.arraySize;

            if (AbilityCount > 0 && (this.currentAbility > this.Abilities.Count() - 1 || this.currentAbility == -1)) {
                this.currentAbility = 0;
            }

            //If new ability has been picked or an abilityID property has not been retrieved yet then populate properties for the current selected ability 
            if (this.currentAbilityIndex != currentAbility || abilityID == null) {
                this.GetProperties(currentAbility);
            }


            // formats for UI
            GUI.skin.button.wordWrap = true;
            GUI.skin.label.wordWrap = true;
            EditorStyles.textField.wordWrap = true;

            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 35;

            EditorGUILayout.Space();

            #endregion


            #region Body
            EditorGUILayout.BeginHorizontal();



            #region Left Section

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }


            EditorGUILayout.BeginVertical(GUILayout.Width(abilityListWidth));

            GUI.color = Color.white;

            #region Logo

            if (meAbilityList.arraySize > 0) {

                Texture2D iconImage = null;

                if (currentAbilityIndex < AbilityCount) {
                    if (this.Abilities[currentAbilityIndex].globalAbilities != null) {
                        iconImage = this.Abilities[currentAbilityIndex].globalAbilities.elementIcon;

                    } else if (this.Abilities[currentAbilityIndex] != null && this.Abilities[currentAbilityIndex] != null && this.Abilities[currentAbilityIndex].iconImage != null) {
                        iconImage = (Texture2D)meAbilityList.GetArrayElementAtIndex(currentAbilityIndex).FindPropertyRelative("iconImage").FindPropertyRelative("refVal").objectReferenceValue;
                    }
                }

                if (iconImage != null) {
                    EditorGUILayout.BeginVertical();

                    GUI.contentColor = Color.white;

                    GUILayout.Label(iconImage, GUILayout.MinWidth(abilityInfoWidth - 20), GUILayout.Height(150));

                    if (EditorGUIUtility.isProSkin == false) {
                        GUI.backgroundColor = inspectorBackgroundColor;
                        GUI.contentColor = Color.black;
                    }

                    EditorGUILayout.EndVertical();
                } else {
                    EditorGUILayout.BeginHorizontal();

                    GUI.contentColor = Color.white;

                    GUILayout.Label(Resources.Load("ABC-EditorIcons/Logo", typeof(Texture2D)) as Texture2D, GUILayout.MinWidth(abilityInfoWidth - 20), GUILayout.Height(150));

                    if (EditorGUIUtility.isProSkin == false) {
                        GUI.backgroundColor = inspectorBackgroundColor;
                        GUI.contentColor = Color.black;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }




            #endregion



            #region Ability Select List


            if (abilityCont != null) {
                if (GUILayout.Button(new GUIContent((abilityCont.DraggableMode) ? " Disable Drag Mode" : " Enable Drag Mode", SortIcon, " Enable/Disable Drag Mode"))) {
                    if (abilityCont.DraggableMode == false) {
                        abilityCont.DraggableMode = true;
                    } else {
                        abilityCont.DraggableMode = false;
                    }

                    EditorUtility.SetDirty(abilityCont);
                    CreateAbilityReorderableList();
                }

                abilityCont.abilitySideListFilterChoice = EditorGUILayout.Popup("", abilityCont.abilitySideListFilterChoice, sideListFilterOptions.ToArray());

                //Only recreate list if a new search string has been entered
                if (abilityCont.abilitySideListFilterChoice != abilityCont.abilitySideListPreviousFilterChoice) {
                    abilityCont.abilitySideListPreviousFilterChoice = abilityCont.abilitySideListFilterChoice;
                    CreateAbilityReorderableList(abilitySearchString, abilityCont.abilitySideListFilterChoice);
                }
            }

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }

            EditorGUILayout.BeginVertical("Box");

            GUI.color = Color.white;

            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));


            string toolbarSearch = "ToolbarSeachTextField";
            string toolbarSearchCancel = "ToolbarSeachCancelButton";

#if UNITY_2022_3_OR_NEWER && !UNITY_2022_3_0
            toolbarSearch = "ToolbarSearchTextField";
            toolbarSearchCancel = "ToolbarSearchCancelButton";
#endif


            if (abilityCont != null) {
                abilitySearchString = GUILayout.TextField(abilitySearchString, GUI.skin.FindStyle(toolbarSearch));


                //If filter has been cleared then get all abilities
                if (GUILayout.Button("", GUI.skin.FindStyle(toolbarSearchCancel))) {
                    // Remove focus if cleared
                    abilitySearchString = "";
                    previousAbilitySearchString = "";
                    CreateAbilityReorderableList();
                    GUI.FocusControl(null);
                }

                //Only recreate list if a new search string has been entered
                if (abilitySearchString != previousAbilitySearchString) {
                    previousAbilitySearchString = abilitySearchString;
                    CreateAbilityReorderableList(abilitySearchString, abilityCont.abilitySideListFilterChoice);
                }
            }




            GUILayout.EndHorizontal();

            listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos,
                                                                 false,
                                                                 false);



            reorderableListAbilities.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            #endregion

            #region Selected Ability Controls

            //InspectorHeader("Ability Controls");
            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }
            EditorGUILayout.BeginVertical("Box");

            GUI.color = Color.white;

            EditorGUILayout.Space();


            #region Global Ability Import/Link
            if (Application.isPlaying == false) {

                string label = "Add Global Ability";


                EditorGUILayout.LabelField(label + ": (" + StarSymbol + ") ", GUILayout.Width(135));
                EditorGUILayout.BeginHorizontal();

                if (abilityCont != null) {
                    this.globalAbilitiesListChoice = EditorGUILayout.Popup(this.globalAbilitiesListChoice, this.GlobalAbilities.Keys.ToList().OrderBy(n => n.name).Select(e => e.name).ToArray());
                } else {
                    this.globalAbilitiesListChoice = EditorGUILayout.Popup(this.globalAbilitiesListChoice, this.GlobalAbilities.Keys.Where(k => k != globElement).ToList().OrderBy(n => n.name).Select(e => e.name).ToArray());
                }

                if (GUILayout.Button(new GUIContent(ImportBlueIcon), GUILayout.Width(30))) {

                    ABC_ImportGlobalElement_EditorWindow wizard = (ABC_ImportGlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_ImportGlobalElement_EditorWindow), true);


                    wizard.elementType = ABC_ImportGlobalElement_EditorWindow.GlobalElementType.Abilities;

                    if (abilityCont != null) {
                        wizard.globalElement = this.GlobalAbilities.Keys.OrderBy(n => n.name).ToArray()[this.globalAbilitiesListChoice];
                    } else {
                        wizard.globalElement = this.GlobalAbilities.Keys.Where(k => k != globElement).OrderBy(n => n.name).ToArray()[this.globalAbilitiesListChoice];
                    }



                    wizard.CurrentAbilityWindow = this;

                    if (abilityCont != null) {
                        wizard.importingEntity = abilityCont;
                    } else {
                        wizard.importingGlobalElement = globElement;
                        wizard.globalAbilitiesEnableGameTypeModification = false;
                    }





                }
                EditorGUILayout.EndHorizontal();
            }

            #endregion

            if (GUILayout.Button(new GUIContent(" Add New Ability", AddIcon, "Add New Ability"))) {

                ABC_Ability ability = new ABC_Ability();
                ability.abilityID = ABC_Utilities.GenerateUniqueID();
                ability.name = "New Ability";
                ability.affectLayer = LayerMask.NameToLayer("Everything");

                //world target indicator
                ABC_TextureReference worldIndicator = new ABC_TextureReference();
                worldIndicator.Texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_WorldTargetIndicatorImage", typeof(Texture)) as Texture;

                if (worldIndicator.Texture != null)
                    ability.abilityWorldTargetIndicatorImage = worldIndicator;

                //range indicator
                ABC_TextureReference rangeIndicator = new ABC_TextureReference();
                rangeIndicator.Texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_RangeIndicatorImage", typeof(Texture)) as Texture;

                if (rangeIndicator.Texture != null)
                    ability.abilityRangeIndicatorImage = rangeIndicator;

                //Mouse indicator
                ABC_TextureReference mouseIndicator = new ABC_TextureReference();
                mouseIndicator.Texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_MouseTargetIndicatorImage", typeof(Texture)) as Texture;

                if (mouseIndicator.Texture != null)
                    ability.abilityMouseTargetIndicatorImage = mouseIndicator;


                this.Abilities.Add(ability);


                if (abilityCont != null) {
                    EditorUtility.SetDirty(abilityCont);
                    CreateAbilityReorderableList(previousAbilitySearchString, abilityCont.abilitySideListPreviousFilterChoice);
                } else {
                    EditorUtility.SetDirty(globElement);
                    CreateAbilityReorderableList();
                }

                currentAbility = this.Abilities.IndexOf(reorderableAbilitylist[reorderableAbilitylist.Count() - 1]);
                this.currentAbilityIndex = currentAbility;

            }

            if (meAbilityList.arraySize > 0) {


                if (GUILayout.Button(new GUIContent(" Copy Ability     " + buttonSpace.ToString(), CopyIcon, "Copy Selected Ability"))) {


                    ABC_Ability clone = this.Abilities[currentAbilityIndex];

                    ABC_Ability newAb = new ABC_Ability();

                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(clone), newAb);

                    // change name as we can't have dupes
                    newAb.name = "Copy Of " + newAb.name;


                    // once we have the latest ID then we plus 1 for a unique number 
                    newAb.abilityID = ABC_Utilities.GenerateUniqueID();


                    this.Abilities.Add(newAb);



                    if (abilityCont != null) {
                        EditorUtility.SetDirty(abilityCont);
                        CreateAbilityReorderableList(previousAbilitySearchString, abilityCont.abilitySideListPreviousFilterChoice);
                    } else {
                        EditorUtility.SetDirty(globElement);
                        CreateAbilityReorderableList();
                    }


                }




                string abilityName = this.Abilities[currentAbilityIndex].name;

                if (this.Abilities[currentAbilityIndex].globalAbilities != null)
                    abilityName = this.Abilities[currentAbilityIndex].globalAbilities.name;



                //Delete Ability 
                if (GUILayout.Button(new GUIContent(" Delete Ability   " + buttonSpace.ToString(), RemoveIcon, "Delete Selected Ability")) && EditorUtility.DisplayDialog("Delete Ability?", "Are you sure you want to delete " + abilityName, "Yes", "No")) {

                    currentAbility = 0;
                    this.Abilities.RemoveAt(currentAbilityIndex);



                    if (abilityCont != null) {
                        EditorUtility.SetDirty(abilityCont);
                        CreateAbilityReorderableList(previousAbilitySearchString, abilityCont.abilitySideListPreviousFilterChoice);
                    } else {
                        EditorUtility.SetDirty(globElement);
                        CreateAbilityReorderableList();
                    }

                }

                //Export abilities
                if (GUILayout.Button(new GUIContent(" Export Abilities   " + buttonSpace.ToString(), ExportIcon, "Export Abilities"))) {

                    if (this.Abilities.All(a => a.enableExport == false)) {

                        EditorUtility.DisplayDialog("Can't Export Abilities", "No Abilities have been marked for export. Choose which Abilities you wish to export by ticking the export tickbox after selecting the ability.", "Continue");



                    } else {

                        ABC_ExportGlobalElement_EditorWindow wizard = (ABC_ExportGlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_ExportGlobalElement_EditorWindow), true);

                        if (abilityCont != null) {
                            wizard.exportingEntity = abilityCont;
                        } else {
                            wizard.exportingAbilities = this.Abilities.Where(a => a.enableExport == true).ToList();
                        }


                        wizard.elementType = ABC_ExportGlobalElement_EditorWindow.GlobalElementType.Abilities;

                    }



                }

            }



            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            #endregion

            EditorGUILayout.EndVertical();




            #endregion

            InspectorBoldVerticleLine();



            #region Right Section


            EditorGUILayout.BeginVertical();


            // if array size is bigger then we need to update
            if (AbilityCount != meAbilityList.arraySize) {
                while (AbilityCount > meAbilityList.arraySize) {
                    meAbilityList.InsertArrayElementAtIndex(meAbilityList.arraySize);

                }
                while (AbilityCount < meAbilityList.arraySize) {
                    meAbilityList.DeleteArrayElementAtIndex(meAbilityList.arraySize - 1);
                }
            }

            #region Right Section - Top Bar
            if (currentAbilityIndex < this.Abilities.Count() && this.Abilities[currentAbilityIndex].globalAbilities == null) {


                if (abilityCont != null)
                    abilityCont.toolbarAbiltyManagerSelection = GUILayout.Toolbar(abilityCont.toolbarAbiltyManagerSelection, toolbarABC);
                else
                    toolbarAbiltyManagerSelection = GUILayout.Toolbar(toolbarAbiltyManagerSelection, toolbarABC);


            }
            #endregion

            EditorGUILayout.BeginHorizontal();



            #region Right Section - Main 


            EditorGUILayout.BeginVertical();



            if (AbilityCount > 0 && currentAbilityIndex < AbilityCount) {

                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }



                #region GlobalAbility

                if (this.Abilities.Count > 0 && this.Abilities.Count - 1 >= currentAbilityIndex && this.Abilities[currentAbilityIndex].globalAbilities != null) {

                    ABC_GlobalElement globalAbilitiesElement = this.Abilities[currentAbilityIndex].globalAbilities;


                    InspectorSectionHeader("Global Abilities: " + globalAbilitiesElement.name, "", true);
                    InspectorVerticalBoxFullWidth();
                    EditorGUILayout.HelpBox("The following Abilities listed below will be added during play and marked with a " + StarSymbol + " symbol. The below Abilities are stored globally and can be configured by clicking the 'Load Global Abilities' button. " +
                        "Changes made to the Abilities created during play will not be saved globally. During play any global changes made to these Abilities will not come into effect unless the 'Refresh ABC' button is pressed.", MessageType.Warning);
                    if (GUILayout.Button(new GUIContent("Load Global Abilities: " + globalAbilitiesElement.name))) {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(GlobalAbilities[this.Abilities[currentAbilityIndex].globalAbilities]);
                    }


                    if (abilityCont != null) {


                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 190;
                        EditorGUILayout.PropertyField(globalAbilityOverrideEnableStatus, new GUIContent("Override Enable Status"));

                        ResetLabelWidth();
                        if (globalAbilityOverrideEnableStatus.boolValue == true) {
                            EditorGUILayout.PropertyField(abilityEnabled, GUILayout.MaxWidth(250));
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 190;
                        EditorGUILayout.PropertyField(globalAbilityOverrideKeyTrigger, new GUIContent("Override Input Triggers"));
                        ResetLabelWidth();

                        if (globalAbilityOverrideKeyTrigger.boolValue == true) {
                            
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUIUtility.labelWidth = 80;
                            EditorGUILayout.PropertyField(key, GUILayout.MaxWidth(175));
                            EditorGUIUtility.labelWidth = 60;
                            EditorGUILayout.PropertyField(keyButton, new GUIContent("Button"), GUILayout.MaxWidth(160));
                      
                            EditorGUILayout.Space();
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();



                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 190;
                        EditorGUILayout.PropertyField(globalAbilitiesEnableGameTypeModification, new GUIContent("Enable Game Type Modification"));
                        ResetLabelWidth();

                        if (globalAbilitiesEnableGameTypeModification.boolValue == true) {
                            EditorGUIUtility.labelWidth = 80;
                            EditorGUILayout.PropertyField(globalAbilitiesGameTypeModification, new GUIContent("Game Type"), GUILayout.MaxWidth(180));
                  
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        if (globalAbilitiesEnableGameTypeModification.boolValue == true) {

                            switch ((ABC_GlobalPortal.GameType)globalAbilitiesGameTypeModification.enumValueIndex) {

                                case ABC_GlobalPortal.GameType.Action:
                                    InspectorHelpBoxFullWidth("Abilities will be setup based on the 'Action' Game Type. Ability will activate on the the nearest enemy targetted and will always activate even if no target exists");
                                    break;
                                case ABC_GlobalPortal.GameType.FPS:
                                    InspectorHelpBoxFullWidth("Abilities will be setup based on the 'FPS' Game Type. Ability will activate towards the Crosshair");
                                    break;
                                case ABC_GlobalPortal.GameType.TPS:
                                    InspectorHelpBoxFullWidth("Abilities will be setup based on the 'TPS' Game Type. Ability will activate towards the Crosshair, Melee Ability will attack the nearest enemy targetted.");
                                    break;
                                case ABC_GlobalPortal.GameType.RPGMMO:
                                    InspectorHelpBoxFullWidth("Abilities will be setup based on the 'RPG/MMO' Game Type. Ability will require a target before activating. Melee attacks will always hit.");
                                    break;
                                case ABC_GlobalPortal.GameType.MOBA:
                                    InspectorHelpBoxFullWidth("Abilities will be setup based on the 'MOBA' Game Type. Ability will need to be chosen before a second click determines the direction the Ability will travel.");
                                    break;
                                case ABC_GlobalPortal.GameType.TopDownAction:
                                    InspectorHelpBoxFullWidth("Abilities will be setup based on the 'Top Down Action' Game Type. Abilities and Attacks will activate towards mouse direction. If mouse direction is near a target the Ability will activate towards the target instead.");
                                    break;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

                    editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos, false, false);

                    foreach (ABC_Ability globalAbility in globalAbilitiesElement.ElementAbilities) {



                        //If nested global ability
                        if (globalAbility.globalAbilities != null) {


                            EditorGUILayout.Space();
                            InspectorSectionHeader(StarSymbol + "Global Abilities: " + globalAbility.globalAbilities.name, "", true);

                            EditorGUILayout.BeginHorizontal();

                            InspectorVerticalBoxFullWidth();


                            if (GUILayout.Button(new GUIContent("Load Global Abilities: " + globalAbility.globalAbilities.name))) {
                                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(GlobalAbilities[globalAbility.globalAbilities]);
                            }


                            EditorGUILayout.HelpBox("Nested Global Ability", MessageType.Info);

                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Description: ");
                            EditorGUILayout.LabelField(globalAbility.globalAbilities.elementDescription);

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(80));
                            EditorGUILayout.Space();

                            GUI.contentColor = Color.white;

                            GUILayout.Label(globalAbility.globalAbilities.elementIcon, GUILayout.MinWidth(abilityInfoWidth - 20), GUILayout.Height(105));


                            if (EditorGUIUtility.isProSkin == false) {
                                GUI.backgroundColor = inspectorBackgroundColor;
                                GUI.contentColor = Color.black;
                            }

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndHorizontal();




                        } else {

                            //Normal ability

                            EditorGUILayout.Space();
                            InspectorSectionHeader("Ability: " + globalAbility.name, "", true);

                            EditorGUILayout.BeginHorizontal();

                            InspectorVerticalBoxFullWidth();

                            EditorGUILayout.LabelField("Type: " + globalAbility.abilityType);
                            EditorGUILayout.LabelField("Starting Position: " + globalAbility.startingPosition.ToString());
                            EditorGUILayout.LabelField("Travel Type: " + globalAbility.travelType.ToString());
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Description: ");
                            EditorGUILayout.LabelField(globalAbility.description);

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(80));
                            EditorGUILayout.Space();

                            GUI.contentColor = Color.white;

                            GUILayout.Label((Texture2D)globalAbility.iconImage.refVal, GUILayout.MinWidth(abilityInfoWidth - 20), GUILayout.Height(105));

                            if (EditorGUIUtility.isProSkin == false) {
                                GUI.backgroundColor = inspectorBackgroundColor;
                                GUI.contentColor = Color.black;
                            }

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndHorizontal();
                        }

                    }


                    EditorGUILayout.EndScrollView();

                }

                #endregion


                #region Ability Settings
                if (currentAbilityIndex < this.Abilities.Count() && this.Abilities[currentAbilityIndex].globalAbilities == null) {
                    switch ((int)ToolbarAbiltyManagerSelection) {
                        case 0:

                            EditorGUILayout.BeginHorizontal();



                            #region Controls/ability info
                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(abilityInfoWidth));

                            if (abilityCont != null)
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref abilityCont.toolbarAbilityManagerGeneralSettingsSelection, ref generalSettingsToolbar);
                            else
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref toolbarAbilityManagerGeneralSettingsSelection, ref generalSettingsToolbar);

                            EditorGUILayout.EndVertical();

                            #endregion


                            InspectorBoldVerticleLine();



                            #region General Settings
                            editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                                 false,
                                                                                 false);

                            EditorGUILayout.BeginVertical();


                            #region Settings



                            switch ((int)ToolbarAbilityManagerGeneralSettingsSelection) {
                                case 0:


                                    InspectorSectionHeader("Ability Settings");

                                    #region SideBySide 


                                    EditorGUILayout.BeginHorizontal();

                                    #region Ability Description 


                                    InspectorVerticalBox(true);

                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(name);
                                    EditorGUILayout.PropertyField(iconImage);
                                    EditorGUILayout.Space();
                                    description.stringValue = EditorGUILayout.TextArea(description.stringValue, GUILayout.MaxHeight(80f));

                                    EditorGUILayout.Space();
                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                        EditorGUILayout.PropertyField(mainGraphic, new GUIContent("Main Graphic"));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(childParticle, new GUIContent("Sub Graphic"));

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 100;
                                        EditorGUILayout.PropertyField(scaleAbilityGraphic, new GUIContent("Scale Graphic"));
                                        if (scaleAbilityGraphic.boolValue == true) {
                                            EditorGUILayout.PropertyField(abilityGraphicScale, new GUIContent("Scale"), GUILayout.MaxWidth(150));
                                        }
                                        EditorGUILayout.Space();
                                        ResetLabelWidth();
                                        EditorGUILayout.EndHorizontal();

                                    } else {

                                        EditorGUILayout.HelpBox("RayCast Abilities do not require graphics.", MessageType.Warning);
                                    }



                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion



                                    #region Ability Type 


                                    InspectorVerticalBox(true);



                                    EditorGUILayout.Space();


                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 60;
                                    EditorGUILayout.PropertyField(abilityType, new GUIContent("Type"), GUILayout.MaxWidth(150));
                                    ResetLabelWidth();
                                    EditorGUILayout.PropertyField(scrollAbility);
                                    EditorGUILayout.EndHorizontal();
                                    InspectorHelpBox("If enabled the Ability has to be equipped by going through a list and activated via one button.", false);

                                    EditorGUIUtility.labelWidth = 275;
                                    EditorGUILayout.PropertyField(castableDuringHitPrevention, new GUIContent("Can Activate During Hit Prevention"));
                                    EditorGUILayout.PropertyField(hitPreventionWontInterruptActivation, new GUIContent("Hit Prevention Won't Interrupt Activation"));
                                    ResetLabelWidth();

                                    EditorGUILayout.Space();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        // InspectorHelpBox("If On/Off then ability can be switched on and off with the button and can cast other abilities. If held the ability will stay active as long as the button is held down and no other abilities can be cast");

                                        EditorGUILayout.PropertyField(abilityToggle, GUILayout.MaxWidth(250));

                                        if ((string)abilityToggle.enumNames[abilityToggle.enumValueIndex] != "Off") {
                                            EditorGUIUtility.labelWidth = 230;
                                            EditorGUILayout.PropertyField(canCastWhenToggled, new GUIContent("Can Activate Abilities When Toggled"));
                                            EditorGUIUtility.labelWidth = 250;
                                            if (canCastWhenToggled.boolValue == false) {
                                                EditorGUILayout.PropertyField(repeatInitiatingAnimationWhilstToggled);
                                            }
                                            EditorGUILayout.Space();
                                            ResetLabelWidth();

                                        }

                                    }


                                    EditorGUILayout.PropertyField(LandOrAir, GUILayout.MaxWidth(250));

                                    if ((string)LandOrAir.enumNames[LandOrAir.enumValueIndex] == "Air") {
                                        EditorGUIUtility.labelWidth = 190;
                                        EditorGUILayout.PropertyField(airAbilityDistanceFromGround, new GUIContent("Distance From Ground"), GUILayout.MaxWidth(230));
                                        ResetLabelWidth();
                                    }
                                    InspectorHelpBox("Determines the elevation condition for ability to activate.", false);

                                    EditorGUIUtility.labelWidth = 120;
                                    EditorGUILayout.PropertyField(forceActivation);

                                    if (forceActivation.boolValue == true) {
                                        EditorGUIUtility.labelWidth = 185;
                                        EditorGUILayout.PropertyField(forceActivationInterruptCurrentActivation, new GUIContent("Interrupt Current Activation"));
                                        ResetLabelWidth();
                                    }


                                    EditorGUILayout.Space();





                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion


                                    #region AllWay 

                                    #region Ability Preparation 

                                    InspectorVerticalBox();

                                    ResetLabelWidth();

                                    InspectorHelpBox("How long the ability will prepare for before initiating. Set to add for example spell casting time to this ability.", false);
                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUILayout.PropertyField(prepareTime, GUILayout.Width(480));

                                    if (prepareTime.floatValue > 0) {
                                        EditorGUILayout.PropertyField(showPrepareTimeOnGUI, new GUIContent("Show on GUI"));
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUIUtility.labelWidth = 245;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(ignoreGlobalPrepareTimeAdjustments);
                                        EditorGUILayout.PropertyField(prepareTriggerHoldRequied, new GUIContent("Prepare Whilst Trigger Is Pressed Down"));
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 170;
                                        EditorGUILayout.PropertyField(moveInteruptPreparation, new GUIContent("Moving Interrupts Preparing"));
                                        if (moveInteruptPreparation.boolValue == true) {

                                            EditorGUILayout.PropertyField(distanceInteruptPreparation, new GUIContent("Distance to Interrupt"), GUILayout.MaxWidth(210));
                                            EditorGUILayout.Space();
                                        }
                                        ResetLabelWidth();
                                    }
                                    EditorGUILayout.EndHorizontal();



                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion

                                    #region AllWay 

                                    #region Ability Initiating 

                                    InspectorVerticalBox();

                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUIUtility.labelWidth = 215;
                                    EditorGUILayout.PropertyField(abilityInitiatingBaseSpeedAdjustment, new GUIContent("Initiating Base Speed Adjustment (%)"), GUILayout.MaxWidth(270));
                                    EditorGUILayout.Space();
                                    EditorGUIUtility.labelWidth = 245;
                                    EditorGUILayout.PropertyField(ignoreGlobalInitiatingSpeedAdjustments);
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 185;

                                    EditorGUILayout.PropertyField(modifyAbilityInitiatingBaseSpeedByStat, new GUIContent("Modify Base Speed using Stat"), GUILayout.MaxWidth(290));

                                    if (modifyAbilityInitiatingBaseSpeedByStat.boolValue == true) {

                                        EditorGUILayout.BeginVertical("box");
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(abilityInitiatingBaseSpeedStatModification.FindPropertyRelative("arithmeticOperator"), new GUIContent(""), GUILayout.Width(65));
                                        EditorGUILayout.PropertyField(abilityInitiatingBaseSpeedStatModification.FindPropertyRelative("percentageValue"), new GUIContent(""), GUILayout.Width(50));
                                        EditorGUIUtility.labelWidth = 40;
                                        EditorGUILayout.PropertyField(abilityInitiatingBaseSpeedStatModification.FindPropertyRelative("statIntegrationType"), new GUIContent("% of"), GUILayout.Width(160));
                                        EditorGUILayout.PropertyField(abilityInitiatingBaseSpeedStatModification.FindPropertyRelative("statName"), new GUIContent("Stat:"), GUILayout.Width(150));
                                        ResetLabelWidth();
                                        GUILayout.EndHorizontal();

                                        EditorGUILayout.EndVertical();


                                    }


                                    InspectorHelpBox("Determines the base initiating speed of the ability which is adjustable by stats. Can also set for the ability to ignore initiating speed modifications due to global adjustments (from effects etc).", false);

                                    ResetLabelWidth();


                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion


                                    #region SideBySide

                                    EditorGUILayout.BeginHorizontal();

                                    #region Ability Duration and Travel 

                                    InspectorVerticalBox(true);


                                    ResetLabelWidth();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "RayCast") {
                                        EditorGUILayout.PropertyField(rayCastLength, new GUIContent("Raycast Length"));
                                        EditorGUILayout.PropertyField(affectLayer, GUILayout.MaxWidth(250));

                                        EditorGUIUtility.labelWidth = 125;
                                        EditorGUILayout.PropertyField(rayCastSingleHit, GUILayout.MaxWidth(250));
                                        InspectorHelpBox("If ticked then the RayCast will only hit one entity else it will be a RayCast sphere capable of hitting multiple entities (shotgun)", false);

                                        if (rayCastSingleHit.boolValue == false) {
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 105;
                                            EditorGUILayout.PropertyField(rayCastRadius, new GUIContent("Raycast Radius"), GUILayout.MaxWidth(170));
                                            EditorGUILayout.PropertyField(raycastHitAmount, new GUIContent("Raycast Max Hits"), GUILayout.MaxWidth(170));
                                            EditorGUILayout.EndHorizontal();
                                            ResetLabelWidth();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(raycastBlockable, new GUIContent("Raycast Blockable"), GUILayout.MaxWidth(180));
                                            EditorGUIUtility.labelWidth = 155;
                                            EditorGUILayout.PropertyField(raycastIgnoreTerrain, new GUIContent("Raycast Ignore Terrain"), GUILayout.MaxWidth(180));
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();
                                            InspectorHelpBox("How many entities can the raycast affect in one shot. If the max is 0 then the effect has no limit. If Blockable is true then non ABC objects can stop the raycast. If Ignore Terrain is ticked then the raycast will ignore terrain collisions");
                                        }

                                    } else if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        InspectorHelpBox("Duration and Travel Speed of the ability. If Duration is set to 0 then the ability will have an unlimited duration.", false);

                                        EditorGUIUtility.labelWidth = 80;
                                        EditorGUILayout.PropertyField(duration, GUILayout.MaxWidth(140));




                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(travelSpeed);
                                        }





                                    }

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion



                                    #region Ability Mana and Recast 


                                    InspectorVerticalBox(true);


                                    //InspectorSectionHeader("Ability details 2", "Details determine preparation time, and if any ammo should be used. If the ability is a scroll then you can also determine reload");


                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(manaCost, GUILayout.MaxWidth(140));
                                    EditorGUIUtility.labelWidth = 60;
                                    EditorGUILayout.PropertyField(statCost, GUILayout.MaxWidth(140));
                                    EditorGUILayout.EndHorizontal();


                                    EditorGUIUtility.labelWidth = 60;
                                    if (statCost.floatValue > 0) {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(statCostIntegrationType, new GUIContent("Stat Type"), GUILayout.MaxWidth(140));
                                        EditorGUILayout.PropertyField(statCostName, new GUIContent("Stat ID"), GUILayout.MaxWidth(150));
                                        EditorGUILayout.EndHorizontal();
                                    }




                                    EditorGUIUtility.labelWidth = 220;
                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) != "RayCast") {
                                        EditorGUILayout.PropertyField(reduceManaWhenActive, new GUIContent("Reduce Whilst Active"));
                                    }
                                    ResetLabelWidth();
                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(abilityRecast, new GUIContent("Cooldown"), GUILayout.MaxWidth(140));
                                    EditorGUIUtility.labelWidth = 220;
                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) != "RayCast") {
                                        EditorGUILayout.PropertyField(startRecastAfterAbilityEnd, new GUIContent("Start Cooldown After Ability Ends"));
                                    }
                                    ResetLabelWidth();







                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region AllWay 

                                    #region Ability Ammo 

                                    InspectorVerticalBox();



                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(UseAmmo);
                                    if (UseAmmo.boolValue == true) {
                                        EditorGUIUtility.labelWidth = 210;
                                        EditorGUILayout.PropertyField(useEquippedWeaponAmmo);
                                        EditorGUIUtility.labelWidth = 190;
                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) != "RayCast") {
                                            EditorGUILayout.PropertyField(reduceAmmoWhilstActive, new GUIContent("Reduce Ammo Whilst Active"));
                                        }
                                        ResetLabelWidth();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();

                                        if (useEquippedWeaponAmmo.boolValue == false) {
                                            EditorGUILayout.PropertyField(ammoCount, GUILayout.MaxWidth(180));
                                            InspectorHelpBox("Amount of times ability can be used. Reload fill will not waste clip but repeatedly add ammo to clip by 1 using the duration as an interval, graphic will also repeat each duration", false);

                                            if (scrollAbility.boolValue == true) {

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(useReload);

                                                if (useReload.boolValue == true) {

                                                    EditorGUILayout.PropertyField(clipSize, GUILayout.MaxWidth(180));

                                                    if (clipSize.intValue == 0) {
                                                        clipSize.intValue = 1;
                                                    }

                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.PropertyField(reloadDuration, GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.EndHorizontal();

                                                    EditorGUILayout.Space();

                                                    EditorGUIUtility.labelWidth = 180;
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(autoReloadWhenRequired);
                                                    ResetLabelWidth();
                                                    EditorGUILayout.PropertyField(reloadFillClip);
                                                    if (reloadFillClip.boolValue == true) {
                                                        EditorGUIUtility.labelWidth = 180;
                                                        EditorGUILayout.PropertyField(reloadFillClipRepeatGraphic, new GUIContent("Fill Clip Repeat Graphic"));
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUIUtility.labelWidth = 270;
                                                    EditorGUILayout.PropertyField(reloadRestrictAbilityActivationDuration, GUILayout.MaxWidth(330));

                                                    ResetLabelWidth();

                                                } else {
                                                    EditorGUILayout.EndHorizontal();
                                                }


                                                EditorGUILayout.Space();


                                            }
                                        }

                                    } else {
                                        EditorGUILayout.EndHorizontal();

                                    }


                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion

                                    InspectorSectionHeader("Group & Tag Assignment");

                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Group Assignment 

                                    InspectorVerticalBox(true);


                                    EditorGUIUtility.labelWidth = 170;

                                    EditorGUILayout.PropertyField(allowAbilityGroupAssignment, new GUIContent("Allow Group Assignment"));
                                    ResetLabelWidth();

                                    InspectorHelpBox("Assign this ability to a group using either the drop down or typing in the group name. Grouped abilities can be set to become enabled all at the same time.", false);

                                    if (abilityCont == null || abilityCont.AbilityGroups.Count > 0) {

                                        if (allowAbilityGroupAssignment.boolValue == true) {

                                            if (abilityCont != null) {
                                                InspectorAbilityGroupListBox("Assign By Group", assignedAbilityGroupIDs, abilityGroupListChoice);
                                            }

                                            EditorGUILayout.Space();
                                            InspectorListBox("Assign By Group Name", assignedAbilityGroupNames);


                                        }


                                    } else {
                                        EditorGUILayout.HelpBox("Unable to assign abilities as no groups have been setup. Please add a new group in the ABC Settings. ", MessageType.Warning);
                                    }





                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region Tag Assignment 

                                    InspectorVerticalBox(true);

                                    InspectorHelpBox("Add Tags to the ability which can be used to filter on", false);

                                    InspectorListBox("Ability Tags", abilityTags);




                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion





                                    break;


                                case 1:

                                    InspectorSectionHeader("Key Press Settings");

                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Key Press Settings

                                    InspectorVerticalBox(true);


                                    InspectorHelpBox("Setup what key initates the Ability. If set as a 'Scroll Ability' then it will only initate if scrolled to and the scroll key is pressed. Scroll key bindings are setup in Settings.");

                                    if (scrollAbility.boolValue == false) {

                                        EditorGUILayout.PropertyField(triggerType, new GUIContent("Trigger Type"), GUILayout.MaxWidth(250));
                                    }

                                    if (((string)triggerType.enumNames[triggerType.enumValueIndex]) == "Input" || scrollAbility.boolValue == true) {

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(onKeyPress);
                                        EditorGUILayout.PropertyField(onKeyDown);
                                        EditorGUILayout.EndHorizontal();
                                    }


                                    EditorGUILayout.Space();
                                    EditorGUIUtility.labelWidth = 175;
                                    EditorGUILayout.PropertyField(enableMovementValidation, new GUIContent("Enable Movement Validation"), GUILayout.MaxWidth(180));

                                    if (enableMovementValidation.boolValue == true) {
                                        EditorGUILayout.PropertyField(movementValidation, new GUIContent(""), GUILayout.MaxWidth(170));
                                    }

                                    ResetLabelWidth();


                                    EditorGUILayout.Space();

                                    if (scrollAbility.boolValue == false) {

                                        if (((string)triggerType.enumNames[triggerType.enumValueIndex]) == "InputCombo") {

                                            InspectorHelpBox("Enter in the key combination that needs to be inputted to trigger the ability. The list order represents which order the inputs need to be pressed");

                                            InspectorListBox("Input Combination", inputCombo);
                                        } else {

                                            EditorGUILayout.PropertyField(keyInputType, new GUIContent("Input Type"), GUILayout.MaxWidth(250));

                                            if (((string)keyInputType.enumNames[keyInputType.enumValueIndex]) == "Key") {

                                                EditorGUILayout.PropertyField(key, GUILayout.MaxWidth(250));

                                            } else {

                                                EditorGUILayout.PropertyField(keyButton, GUILayout.MaxWidth(250));

                                            }

                                            EditorGUILayout.Space();
                                            ResetLabelWidth();
                                            InspectorHelpBox("If additional input is enabled then ability will require an additional trigger.", false);


                                            EditorGUILayout.PropertyField(requireAdditionalKeyInput, new GUIContent("Additional Input"));

                                            if (requireAdditionalKeyInput.boolValue == true) {

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(additionalOnKeyPress, new GUIContent("On Key Press"));
                                                EditorGUILayout.PropertyField(additionalOnKeyDown, new GUIContent("On Key Down"));
                                                EditorGUILayout.EndHorizontal();

                                                EditorGUILayout.PropertyField(additionalKeyInputType, new GUIContent("Input Type"), GUILayout.MaxWidth(250));

                                                if (((string)additionalKeyInputType.enumNames[additionalKeyInputType.enumValueIndex]) == "Key") {

                                                    EditorGUILayout.PropertyField(additionalKey, GUILayout.MaxWidth(250));

                                                } else {

                                                    EditorGUILayout.PropertyField(additionalKeyButton, GUILayout.MaxWidth(250));

                                                }


                                                EditorGUILayout.Space();
                                            }
                                        }



                                    } else {


                                        InspectorHelpBox("By using a quick key the player can quickly switch to this ability by pressing the key. Swap Duration stops any ability activating whilst swapping abilities");


                                        EditorGUIUtility.labelWidth = 150;

                                        EditorGUILayout.PropertyField(scrollQuickInputType, new GUIContent("Input Type"), GUILayout.MaxWidth(250));



                                        if (((string)scrollQuickInputType.enumNames[scrollQuickInputType.enumValueIndex]) == "Key") {

                                            EditorGUILayout.PropertyField(scrollQuickKey, GUILayout.MaxWidth(250));

                                        } else {

                                            EditorGUILayout.PropertyField(scrollQuickButton, GUILayout.MaxWidth(250));

                                        }


                                        EditorGUILayout.PropertyField(scrollSwapDuration, new GUIContent("Swap Duration"), GUILayout.MaxWidth(180));




                                        EditorGUILayout.PropertyField(scrollSetUnsetRaiseEvent, new GUIContent("Set/Unset Raise Event"), GUILayout.MaxWidth(180));
                                        ResetLabelWidth();
                                    }

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region wait for key to initialise 

                                    InspectorVerticalBox(true);


                                    EditorGUIUtility.labelWidth = 190;
                                    InspectorHelpBox("After preparation Ability will wait for the second key press (after delay) to initiate. No other abilities can be used during this time. ", false);
                                    EditorGUILayout.PropertyField(waitForKeyBeforeInitiating, new GUIContent("Wait For Key to Initiate"));
                                    ResetLabelWidth();
                                    EditorGUILayout.Space();
                                    if (waitForKeyBeforeInitiating.boolValue == true) {

                                        EditorGUILayout.PropertyField(waitBeforeInitiatingInputType, new GUIContent("Input Type"), GUILayout.MaxWidth(250));


                                        if (((string)waitBeforeInitiatingInputType.enumNames[waitBeforeInitiatingInputType.enumValueIndex]) == "Key") {

                                            EditorGUILayout.PropertyField(waitBeforeInitiatingKey, new GUIContent("Key"), GUILayout.MaxWidth(250));

                                        } else {

                                            EditorGUILayout.PropertyField(waitBeforeInitiatingButton, new GUIContent("Button"), GUILayout.MaxWidth(250));

                                        }


                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 150;
                                        EditorGUILayout.PropertyField(waitForKeyBeforeInitiatingDelay, new GUIContent("Delay Till Recognised"), GUILayout.MaxWidth(200));
                                        ResetLabelWidth();


                                        EditorGUIUtility.labelWidth = 150;

                                        EditorGUILayout.PropertyField(waitForKeyAllowChangeOfTarget, new GUIContent("Allow Target Change"));
                                        ResetLabelWidth();



                                    }



                                    InspectorHelpBox("Ability will only activate if Override Crosshair is present on screen. ", false);
                                    EditorGUIUtility.labelWidth = 190;
                                    EditorGUILayout.PropertyField(requireCrossHair, new GUIContent("Require Override Crosshair"));
                                    EditorGUIUtility.labelWidth = 110;

                                    EditorGUILayout.Space();


                                    InspectorHelpBox("Any abilities linked will always have the same key/button trigger as this ability", false);

                                    EditorGUIUtility.labelWidth = 210;
                                    EditorGUILayout.PropertyField(enableAbilityTriggerLinks);
                                    ResetLabelWidth();

                                    if (enableAbilityTriggerLinks.boolValue == true) {
                                        InspectorAbilityListBox("Trigger Links:", triggerLinkAbilityIDs);
                                    }

                                    EditorGUILayout.EndVertical();



                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    InspectorSectionHeader("Ability Combo Settings");

                                    #region AllWay 

                                    #region Ability Combo 

                                    InspectorVerticalBox();


                                    EditorGUIUtility.labelWidth = 190;
                                    if (scrollAbility.boolValue == false && ((string)triggerType.enumNames[triggerType.enumValueIndex]) == "Input") {

                                        InspectorHelpBox("Combo Next Time is the duration in which the trigger key has to be pressed again to activate the next combo ability in the list with the same trigger. If key hasn't been pressed after this time it will reset the combo. If never reset other combos is disabled then using this ability will stop and reset all other combos.", false);



                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(abilityCombo);
                                        EditorGUILayout.PropertyField(comboNextActivateTime, new GUIContent("Combo Next Time"), GUILayout.MaxWidth(230));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(comboHitRequired);
                                    }
                                    EditorGUILayout.PropertyField(neverResetOtherCombos);
                                    EditorGUILayout.Space();

                                    if (scrollAbility.boolValue == false && ((string)triggerType.enumNames[triggerType.enumValueIndex]) == "Input") {
                                        EditorGUILayout.EndHorizontal();
                                    }

                                    ResetLabelWidth();







                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion

                                    #region AllWay 

                                    #region Ability swap and activation adjustment 

                                    InspectorVerticalBox();



                                    InspectorHelpBox("Swap setting will randomly mix up abilities in the same trigger group also set to be randomly swapped. Interval adjustment will on activation temporarily modify the wait between ability activations, the adjustment is reset after the interval is over.", false);

                                    EditorGUIUtility.labelWidth = 210;

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(randomlySwapAbilityPosition);
                                    EditorGUIUtility.labelWidth = 290;
                                    EditorGUILayout.PropertyField(tempAbilityActivationIntervalAdjustment, new GUIContent("Activation Interval Temporary Adjustment"), GUILayout.MaxWidth(340));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndHorizontal();
                                    ResetLabelWidth();







                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion


                                    #region AllWay 

                                    #region Enable After Event

                                    InspectorVerticalBox();


                                    //InspectorSectionHeader("Enable After Event", "If enable after event has been setup then the ability will be enabled after certain ABC events have occured.");

                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(enableDuration, new GUIContent("Enabled Duration"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.PropertyField(enableAfterEvent, new GUIContent("Enable After Event"));
                                    EditorGUILayout.EndHorizontal();
                                    InspectorHelpBox("If enabled duration is greater then 0 the ability when enabled will become disabled after the time entered.", false);



                                    EditorGUIUtility.labelWidth = 130;


                                    ResetLabelWidth();


                                    if (enableAfterEvent.boolValue == true) {
                                        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(400), GUILayout.MinHeight(minimumSectionHeight));
                                        EditorGUILayout.Space();
                                        InspectorAbilityListBox("Ability Activated:", enableAfterAbilityIDsActivated);
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                        InspectorAbilityListBox("Ability Collision:", enableAfterAbilityIDsCollided);
                                        EditorGUILayout.Space();

                                        EditorGUILayout.EndHorizontal();
                                    }

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion

                                    InspectorSectionHeader("Ability Activation Links");

                                    #region AllWay 

                                    #region Ability Activation Link 

                                    InspectorVerticalBox();

                                    InspectorHelpBox("Any abilities linked will activate when this ability activates (ignoring triggers and activation restrictions)", false);

                                    EditorGUIUtility.labelWidth = 210;
                                    EditorGUILayout.PropertyField(enableAbilityActivationLinks);
                                    ResetLabelWidth();

                                    if (enableAbilityActivationLinks.boolValue == true) {
                                        InspectorAbilityListBox("Activate Links:", activationLinkAbilityIDs);
                                    }

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion


                                    break;

                                case 2:





                                    #region AllWay 

                                    #region Ability Effect Links

                                    InspectorVerticalBox();
                                    EditorGUIUtility.labelWidth = 120;
                                    InspectorHelpBox("Set for a chance to critical hit which will enhance the potency of effects. The crit chance can also be increased by stats.", false);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(enableCriticalHit, new GUIContent("Enable Critical Hits"), GUILayout.Width(150));

                                    if (enableCriticalHit.boolValue == true) {

                                        EditorGUIUtility.labelWidth = 100;

                                        EditorGUILayout.PropertyField(potencyCriticalHitBaseChance, new GUIContent("Crit Chance (%)"), GUILayout.Width(150));
                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.PropertyField(potencyCriticalHitEnableChanceStatModification, new GUIContent("Stat Modification"), GUILayout.Width(180));
                                        
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    if (enableCriticalHit.boolValue == true && potencyCriticalHitEnableChanceStatModification.boolValue == true) {

                                        EditorGUILayout.BeginVertical("box");
                                        SerializedProperty element = potencyCriticalHitChanceStatModification;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 40;
                                        EditorGUILayout.PropertyField(element.FindPropertyRelative("percentageValue"), new GUIContent("Add "), GUILayout.Width(90));
                                        EditorGUILayout.PropertyField(element.FindPropertyRelative("statIntegrationType"), new GUIContent("% of"), GUILayout.Width(170));
                                        EditorGUILayout.PropertyField(element.FindPropertyRelative("statName"), new GUIContent("Stat:"), GUILayout.Width(140));
                                        ResetLabelWidth();

                                        GUILayout.EndHorizontal();
                                        EditorGUILayout.EndVertical();
                                    }
                                    
                                    ResetLabelWidth();
                            

                                    InspectorHelpBox("When this ability applies effects it will also apply effects of the abilities linked below.", false);
                                    EditorGUIUtility.labelWidth = 190;
                                    EditorGUILayout.PropertyField(enableAbilityEffectLinks);
                                    ResetLabelWidth();

                                    if (enableAbilityEffectLinks.boolValue == true) {

                                        InspectorAbilityListBox("Effect Links:", effectLinkAbilityIDs);
                                    }


                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion



                                    #region Effects


                                    this.GetEffectSettings(stateEffects);


                                    #endregion



                                    EditorGUILayout.Space();



                                    EditorGUILayout.Space();







                                    EditorGUILayout.Space();



                                    break;

                                case 3:

                                    InspectorSectionHeader("Additional Settings");

                                    #region AllWay 

                                    #region Ability Spawn Objects / Limit Active 

                                    InspectorVerticalBox();

                                    EditorGUILayout.PropertyField(persistIK);
                                    InspectorHelpBox("If ticked then the IK applied by ABC will remain whilst the ability activates");

                                    EditorGUIUtility.labelWidth = 120;
                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || (string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(spawnObject);

                                        if (spawnObject.boolValue == true) {
                                            EditorGUILayout.PropertyField(spawningObject);
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(spawnObjectOnDestroy, new GUIContent("Destroy Spawn"));
                                            EditorGUILayout.PropertyField(spawnObjectOnCollide, new GUIContent("Collide Spawn"));
                                            EditorGUILayout.EndHorizontal();

                                        } else {
                                            EditorGUILayout.EndHorizontal();
                                        }
                                        InspectorHelpBox("If enabled then this will spawn the provided objects when the ability is destroyed or collides");

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(limitActiveAtOnce, new GUIContent("Limit Active"));
                                        EditorGUILayout.Space();
                                        if (limitActiveAtOnce.boolValue == true) {
                                            EditorGUILayout.PropertyField(maxActiveAtOnce, new GUIContent("Max Active"), GUILayout.MaxWidth(180));
                                        }
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();
                                        InspectorHelpBox("Will limit the amount of the abilities currently in play by destroying the oldest active ability when the limit is reached from a new one being created.");


                                    } else {

                                        EditorGUILayout.HelpBox("Raycast abilities can not spawn objects or limit active.", MessageType.Warning);
                                    }


                                    EditorGUIUtility.labelWidth = 120;
                                    EditorGUILayout.PropertyField(loggingEnabled, new GUIContent("Logging Enabled"), GUILayout.MaxWidth(180));
                                    InspectorHelpBox("If enabled then the ability states (Preparing, Activating, Cooldown etc) can write to any GUI Logs setup.");

                                    EditorGUIUtility.labelWidth = 210;
                                    EditorGUILayout.PropertyField(abilityActivationRaiseEvent, new GUIContent("Raise Ability Activation Event"), GUILayout.MaxWidth(240));
                                    InspectorHelpBox("If enabled then on ability activation an event delegate will be raised to let linked scripts know");
                                    ResetLabelWidth();


                                    EditorGUIUtility.labelWidth = 280;
                                    EditorGUILayout.PropertyField(abilityActivationCompleteRaiseEvent, new GUIContent("Raise Ability Activation Completed Event"), GUILayout.MaxWidth(290));
                                    if (abilityActivationCompleteRaiseEvent.boolValue == true) {
                                        EditorGUILayout.PropertyField(abilityActivationCompleteEventType, new GUIContent("Ability Activation Completed Event Type"), GUILayout.MaxWidth(390));
                                    }
                                    ResetLabelWidth();
                                    InspectorHelpBox("If enabled then when ability activation is completed depending on the type selected (when ability initiates or is destroyed) an event delegate will be raised to let linked scripts know");




                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion

                                    InspectorSectionHeader("Surrounding Objects");


                                    #region AllWay

                                    EditorGUILayout.BeginHorizontal();

                                    #region SurroundingObjects 

                                    InspectorVerticalBox();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || (string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {

                                        InspectorHelpBox("If enabled and setup then surrounding objects configured will travel alongside the ability, like telekensis.");

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 220;
                                        EditorGUILayout.PropertyField(includeSurroundingObject);
                                        if (includeSurroundingObject.boolValue == true) {
                                            EditorGUILayout.PropertyField(lockSurroundingObject, new GUIContent("Lock To Projectile"));
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        if (includeSurroundingObject.boolValue == true) {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(minimumScatterRange, GUILayout.MaxWidth(270));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(maximumScatterRange, GUILayout.MaxWidth(270));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(sendObjectToProjectile, new GUIContent("Send Object To Projectile"));
                                            EditorGUILayout.Space();
                                            if (sendObjectToProjectile.boolValue == true) {
                                                EditorGUILayout.PropertyField(objectToProjectileDuration, new GUIContent("Object To Projectile Duration"), GUILayout.MaxWidth(270));
                                            }
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();
                                            InspectorHelpBox("If Send Object To Projectile is ticked the object will move towards the object location. " +
                                                                         "If Object To Projectile Duration is 0 then the object will hover on the spot", false);
                                        }

                                        ResetLabelWidth();




                                    } else {


                                        EditorGUILayout.HelpBox("Raycast abilities can't include surrounding objects", MessageType.Warning);
                                    }
                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region AllWay

                                    EditorGUILayout.BeginHorizontal();

                                    #region SurroundingObjects Collision & Collider

                                    InspectorVerticalBox();

                                    EditorGUILayout.Space();

                                    if (includeSurroundingObject.boolValue == false) {
                                        EditorGUILayout.HelpBox("Surrounding Objects not enabled", MessageType.Warning);
                                    } else {


                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || (string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {


                                            EditorGUIUtility.labelWidth = 220;

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(applyColliderToSurroundingObjects, new GUIContent("Enable Collider"));
                                            if (applyColliderToSurroundingObjects.boolValue == true) {
                                                EditorGUILayout.PropertyField(applyColliderWhenProjectileReached, new GUIContent("Enable When Projectile Reached"));
                                            }
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(projectileAffectSurroundingObject, new GUIContent("Effect on Collision"));
                                            EditorGUILayout.PropertyField(destroySurroundingObject, new GUIContent("Destroy on Collision"));
                                            EditorGUILayout.EndHorizontal();


                                            ResetLabelWidth();

                                            EditorGUILayout.Space();



                                        } else {


                                            EditorGUILayout.HelpBox("Raycast abilities can include surrounding objects", MessageType.Warning);
                                        }
                                    }
                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Surrounding Object Tag 

                                    InspectorVerticalBox(true);


                                    //InspectorSectionHeader("Surrounding Object Tag", "Tag Options.");

                                    if (includeSurroundingObject.boolValue == false) {
                                        EditorGUILayout.HelpBox("Surrounding Objects not enabled", MessageType.Warning);
                                    } else {

                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || (string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {

                                            EditorGUIUtility.labelWidth = 220;

                                            EditorGUILayout.PropertyField(surroundingObjectTags, new GUIContent("Surrounding Tags"));

                                            if (surroundingObjectTags.boolValue == true) {
                                                EditorGUILayout.PropertyField(surroundingObjectTagsRange, new GUIContent("Surrounding Range"), GUILayout.MaxWidth(280));
                                                EditorGUILayout.PropertyField(surroundingObjectTagRequired, new GUIContent("Surrounding Object Required"), GUILayout.MaxWidth(280));
                                                EditorGUILayout.PropertyField(surroundingObjectTagLimit, new GUIContent("Object Limit"));

                                                EditorGUILayout.Space();


                                                InspectorListBox("Affected Tags", surroundingObjectTagAffectTag);
                                            }




                                            ResetLabelWidth();

                                            EditorGUILayout.Space();



                                        } else {


                                            EditorGUILayout.HelpBox("Raycast abilities can include surrounding objects", MessageType.Warning);
                                        }
                                    }



                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region Surronding Object Target

                                    InspectorVerticalBox(true);

                                    if (includeSurroundingObject.boolValue == false) {
                                        EditorGUILayout.HelpBox("Surrounding Objects not enabled", MessageType.Warning);
                                    } else {

                                        //InspectorSectionHeader("SurroundingObjects Target", "Todo put this at bottom");

                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || (string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {

                                            EditorGUIUtility.labelWidth = 220;



                                            EditorGUILayout.PropertyField(surroundingObjectTarget, new GUIContent("Surrounding Target Select"));

                                            if (surroundingObjectTarget.boolValue == true) {
                                                EditorGUILayout.PropertyField(surroundingObjectTargetRange, new GUIContent("Surrounding Range"), GUILayout.MaxWidth(280));
                                                EditorGUILayout.PropertyField(surroundingObjectAuxiliarySoftTarget, new GUIContent("Auxiliary SoftTarget"));
                                                EditorGUILayout.PropertyField(surroundingObjectTargetRequired, new GUIContent("Surrounding Object Required"), GUILayout.MaxWidth(280));
                                                EditorGUILayout.PropertyField(surroundingObjectTargetRestrict, new GUIContent("Only Affect Tag"));

                                                if (surroundingObjectTargetRestrict.boolValue == true) {
                                                    InspectorListBox("Only Affect Targets with the following Tags", surroundingObjectTargetAffectTag);
                                                }

                                            }




                                            EditorGUIUtility.labelWidth = 110;

                                            EditorGUILayout.Space();



                                        } else {


                                            EditorGUILayout.HelpBox("Raycast abilities can include surrounding objects", MessageType.Warning);
                                        }
                                    }





                                    EditorGUILayout.Space();





                                    EditorGUILayout.EndVertical();

                                    #endregion



                                    EditorGUILayout.EndHorizontal();

                                    #endregion



                                    break;



                            }

                            #endregion

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndScrollView();
                            #endregion




                            EditorGUILayout.EndHorizontal();

                            break;



                        case 1:

                            EditorGUILayout.BeginHorizontal();

                            #region Controls/ability info
                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(abilityInfoWidth));


                            if (abilityCont != null)
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref abilityCont.toolbarAbilityManagerPositionTravelSettingsSelection, ref positiontravelSettingsToolbar);
                            else
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref toolbarAbilityManagerPositionTravelSettingsSelection, ref positiontravelSettingsToolbar);

                            EditorGUILayout.EndVertical();

                            #endregion

                            InspectorBoldVerticleLine();


                            #region Position & Travel 



                            editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                                false,
                                                                                false);

                            EditorGUILayout.BeginVertical();

                            #region Settings


                            switch ((int)ToolbarAbilityManagerPositionTravelSettingsSelection) {
                                case 0:


                                    InspectorSectionHeader("Travel Type & Starting Position");

                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Targeting/Travel

                                    InspectorVerticalBox(true);


                                    InspectorHelpBox("Travel type determines how the ability reaches it's destination. From selected targets to mouse position or just to travel forward from the entity position", false);


                                    if ((string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {

                                        EditorGUILayout.HelpBox("Travel type setting does not exist for Melee Abilities", MessageType.Warning);


                                    } else {

                                        EditorGUIUtility.labelWidth = 100;

                                        EditorGUILayout.PropertyField(travelType, GUILayout.MaxWidth(250));

                                        switch ((string)travelType.enumNames[travelType.enumValueIndex]) {

                                            #region CustomScript targeting Settings
                                            case "CustomScript":

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.Space();
                                                    InspectorHelpBox("Custom script will be added to the Ability. Please note unless added to the custom script the Ability will lose normal features found in this section");
                                                    EditorGUILayout.PropertyField(customTravelScript, new GUIContent("Custom Script"), GUILayout.MaxWidth(400));
                                                    // custom script has no travel or misc settings or additional settings

                                                } else {


                                                    EditorGUILayout.HelpBox(" Custom Script does not have specific settings whilst ability is a raycast.", MessageType.Warning);
                                                }



                                                break;
                                            #endregion

                                            #region NoTravel targeting Settings
                                            case "NoTravel":

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.HelpBox("No travel does not have specific settings .", MessageType.Warning);
                                                    // none added yet 
                                                } else {


                                                    EditorGUILayout.HelpBox("No Travel can not be used whilst ability is a Raycast. Ability will default to Forward Settings.", MessageType.Warning);

                                                }


                                                break;
                                            #endregion

                                            #region Mouse2D targeting Settings
                                            case "Mouse2D":

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                }



                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(selectedTargetForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.PropertyField(selectedTargetRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();

                                                    EditorGUILayout.PropertyField(abilityCanMiss, new GUIContent("Ability Can Miss"), GUILayout.MaxWidth(180));
                                                    InspectorHelpBox("If true then the ability has the potential to miss the target depending on the Originators miss chance and values");

                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);

                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");

                                                }


                                                break;

                                            #endregion

                                            #region Forward3D targeting Settings
                                            case "Forward":
                                                EditorGUILayout.Space();
                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                } else {


                                                    EditorGUILayout.HelpBox(" Forward does not have specific settings whilst ability is a raycast.", MessageType.Warning);
                                                }

                                                break;

                                            #endregion

                                            #region Selected Target targeting Settings 
                                            case "SelectedTarget":

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                }

                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(selectedTargetForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                                EditorGUILayout.PropertyField(selectedTargetRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                                EditorGUILayout.PropertyField(abilityCanMiss, new GUIContent("Ability Can Miss"), GUILayout.MaxWidth(180));
                                                InspectorHelpBox("If true then the ability has the potential to miss the target depending on the Originators miss chance and values");

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);

                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");

                                                }




                                                break;

                                            #endregion

                                            #region NearestTag targeting Settings 
                                            case "NearestTag":



                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                }

                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(selectedTargetForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                                EditorGUILayout.PropertyField(selectedTargetRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                                EditorGUILayout.PropertyField(abilityCanMiss, new GUIContent("Ability Can Miss"), GUILayout.MaxWidth(180));
                                                InspectorHelpBox("If true then the ability has the potential to miss the target depending on the Originators miss chance and values");


                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);

                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");

                                                }



                                                break;

                                            #endregion

                                            #region Self targeting Settings 
                                            case "Self":

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));

                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                    EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(selectedTargetForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.PropertyField(selectedTargetRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();

                                                    EditorGUILayout.PropertyField(abilityCanMiss, new GUIContent("Ability Can Miss"), GUILayout.MaxWidth(180));
                                                    InspectorHelpBox("If true then the ability has the potential to miss the target depending on the Originators miss chance and values");


                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);

                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");

                                                } else {


                                                    EditorGUILayout.HelpBox("Self Type does not have specific settings whilst ability is a raycast.", MessageType.Warning);




                                                }





                                                break;

                                            #endregion

                                            #region ToWorld targeting Settings

                                            case "ToWorld":

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));

                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                    EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(selectedTargetForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.PropertyField(selectedTargetRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();

                                                    EditorGUILayout.PropertyField(abilityCanMiss, new GUIContent("Ability Can Miss"), GUILayout.MaxWidth(180));
                                                    InspectorHelpBox("If true then the ability has the potential to miss the target depending on the Originators miss chance and values");


                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);

                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");

                                                } else {

                                                    EditorGUILayout.HelpBox(" To World can not be used whilst ability is a Raycast. Ability will default to Forward Settings.", MessageType.Warning);

                                                }



                                                break;

                                            #endregion

                                            #region MouseTarget targeting Settings 
                                            case "MouseTarget":



                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                }

                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(selectedTargetForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.PropertyField(selectedTargetRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();

                                                    EditorGUILayout.PropertyField(abilityCanMiss, new GUIContent("Ability Can Miss"), GUILayout.MaxWidth(180));
                                                    InspectorHelpBox("If true then the ability has the potential to miss the target depending on the Originators miss chance and values");


                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);
                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");


                                                }








                                                break;
                                            #endregion

                                            #region MouseForward targeting Settings
                                            case "MouseForward":
                                                EditorGUILayout.Space();
                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                } else {


                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                    EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                    EditorGUILayout.EndHorizontal();

                                                }
                                                EditorGUILayout.Space();


                                                break;
                                            #endregion

                                            #region CameraCenterFPS targeting Settings
                                            case "Crosshair":




                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.PropertyField(travelPhysics, GUILayout.MaxWidth(250));
                                                }

                                                EditorGUILayout.Space();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));
                                                EditorGUILayout.PropertyField(selectedTargetOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();

                                                if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(targetTravel);

                                                    if (targetTravel.boolValue == true) {
                                                        EditorGUILayout.Space();
                                                        EditorGUIUtility.labelWidth = 125;

                                                        EditorGUILayout.PropertyField(seekTargetDelay, GUILayout.MaxWidth(180));

                                                        EditorGUIUtility.labelWidth = 110;
                                                        EditorGUILayout.Space();
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("Does the projectile travel to target or already appear there. If seek target delay is enabled then ability will travel forward in its starting direction until the delay is over and then will start heading towards target");
                                                }



                                                break;
                                                #endregion

                                        }


                                    }


                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region Starting Position

                                    InspectorVerticalBox(true);


                                    InspectorHelpBox("Settings define where the ability will start after initiation.");



                                    EditorGUIUtility.labelWidth = 130;
                                    if ((string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {

                                        // If melee then change travel script to no travel 
                                        travelType.enumValueIndex = 5;
                                    }

                                    EditorGUILayout.PropertyField(startingPosition, GUILayout.MaxWidth(250));



                                    // get enum value index and if it's on object display new layout
                                    if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnObject") {


                                        EditorGUILayout.PropertyField(startingPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(400));
                                    }




                                    // get enum value index and if it's on object display new layout
                                    if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnTag" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnSelfTag") {

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 10;
                                        EditorGUILayout.LabelField("Select Tag");
                                        ResetLabelWidth();
                                        startingPositionOnTag.stringValue = EditorGUILayout.TagField(startingPositionOnTag.stringValue, GUILayout.MaxWidth(200));
                                        EditorGUILayout.EndHorizontal();


                                    }


                                    // get enum value index and if it's on object display new layout
                                    if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target") {


                                        EditorGUILayout.PropertyField(startingPositionAuxiliarySoftTarget, new GUIContent("Auxiliary SoftTarget"), GUILayout.MaxWidth(400));
                                    }
                                    ResetLabelWidth();



                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Position Offset", GUILayout.MaxWidth(100));
                                    EditorGUILayout.PropertyField(startingPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(startingPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.PropertyField(startingPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Space();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                        EditorGUILayout.PropertyField(startingRotation, GUILayout.MaxWidth(250));
                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 130;
                                        EditorGUILayout.PropertyField(setEulerRotation, GUILayout.MaxWidth(250));
                                    }
                                    ResetLabelWidth();
                                    EditorGUILayout.EndVertical();

                                    #endregion





                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    if ((string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") {
                                        InspectorSectionHeader("Melee Settings");
                                    } else {
                                        InspectorSectionHeader((string)travelType.enumNames[travelType.enumValueIndex] + " Settings");
                                    }

                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Travel Type Left Side Settings


                                    switch ((string)travelType.enumNames[travelType.enumValueIndex]) {

                                        #region CustomScript targeting Settings
                                        case "CustomScript":

                                            InspectorVerticalBox();

                                            EditorGUILayout.HelpBox("Custom Script does not have specific settings.", MessageType.Warning);


                                            EditorGUILayout.EndVertical();

                                            break;
                                        #endregion

                                        #region NoTravel/Melee targeting Settings
                                        case "NoTravel":

                                            #region Side By Side

                                            #region left 

                                            if ((string)abilityType.enumNames[abilityType.enumValueIndex] != "Melee" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {
                                                InspectorVerticalBox(true);
                                            } else {
                                                InspectorVerticalBox();
                                            }

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                ResetLabelWidth();
                                                EditorGUILayout.PropertyField(travelWithCaster, new GUIContent("Travel w/ Entity"));
                                                EditorGUILayout.Space();


                                                if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                    ResetLabelWidth();

                                                    EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                    InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");


                                                    GetSharedStartingPositionTargetAndOnWorldSettings();
                                                }


                                            } else if ((string)abilityType.enumNames[abilityType.enumValueIndex] == "Melee") { // bespoke melee settings as its not a travel type enum      
                                                EditorGUIUtility.labelWidth = 120;

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(noTargetStillTravel, new GUIContent("Always Activate"));
                                                EditorGUILayout.PropertyField(auxiliarySoftTarget, new GUIContent("Auxiliary SoftTarget"));
                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("If Always Activate is enabled then a selected target is not required to activate the ability. If Aux SoftTarget is ticked then the ability will as backup treat the current softTarget as the main target if not current target exists. ");

                                                EditorGUIUtility.labelWidth = 150;

                                                EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                EditorGUILayout.PropertyField(meleeKeepRotatingToSelectedTarget, new GUIContent("Keep Rotating to Target"));

                                                InspectorHelpBox("Rotate to target if enabled will always rotate the entity to face the current target when preparing and initiating. Keep rotating will make sure the entity is rotated towards the target for the whole melee ability");

                                                EditorGUIUtility.labelWidth = 170;

                                                if (noTargetStillTravel.boolValue == true) {
                                                    EditorGUILayout.PropertyField(noTargetRotateBehaviour, GUILayout.MaxWidth(380));
                                                    InspectorHelpBox("Determines the entities rotate behaviour when a target doesn't exist or entity is set to not rotate to the current target");
                                                }

                                                EditorGUIUtility.labelWidth = 290;
                                                EditorGUILayout.PropertyField(hitsStopMeleeAttack, new GUIContent("Hit Prevention Will Immediately Stop Melee Attack"));

                                                InspectorHelpBox("If true then hits received can stagger the melee attack stopping the projectile and animation immediately (after activation)");

                                                EditorGUIUtility.labelWidth = 170;


                                                if (noTargetStillTravel.boolValue == false || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                    EditorGUILayout.PropertyField(useRange);
                                                    if (useRange.boolValue == true) {
                                                        EditorGUILayout.PropertyField(selectedTargetRangeGreaterThan, new GUIContent("Range Greater Than"), GUILayout.MaxWidth(230));
                                                        EditorGUILayout.PropertyField(selectedTargetRangeLessThan, new GUIContent("Range Less Than"), GUILayout.MaxWidth(230));

                                                    }
                                                    InspectorHelpBox("Ability will only activate if target is in the range defined");

                                                    EditorGUILayout.PropertyField(targetFacing, new GUIContent("Face Target Required"));
                                                    InspectorHelpBox("If the entity has a target then the Ability will only activate if the entity is facing the target");


                                                    EditorGUIUtility.labelWidth = 140;

                                                    EditorGUILayout.PropertyField(selectedTargetRestrictTargets, new GUIContent("Restrict Targets"));
                                                    if (selectedTargetRestrictTargets.boolValue == true) {
                                                        InspectorListBox("Only Activate On Following Tags", selectedTargetOnlyCastOnTag);
                                                    }


                                                }




                                            } else if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                ResetLabelWidth();

                                                EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");


                                                GetSharedStartingPositionTargetAndOnWorldSettings();



                                            } else {

                                                EditorGUILayout.HelpBox("No Travel does not have specific settings whilst ability is a raycast.", MessageType.Warning);

                                            }





                                            EditorGUILayout.EndVertical();
                                            #endregion


                                            #endregion




                                            break;
                                        #endregion

                                        #region Mouse2D targeting Settings
                                        case "Mouse2D":


                                            #region Side by Side 
                                            #region Left Side
                                            InspectorVerticalBox(true);

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                EditorGUIUtility.labelWidth = 210;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination);
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");


                                                EditorGUIUtility.labelWidth = 150;

                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                InspectorHelpBox("Entity will face selected target when using this Ability", false);

                                                ResetLabelWidth();

                                                EditorGUILayout.Space();

                                            } else if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "Target" && ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "OnWorld") {


                                                EditorGUILayout.HelpBox(" Mouse2D does not have specific settings whilst ability is a raycast.", MessageType.Warning);
                                            }



                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedStartingPositionTargetAndOnWorldSettings();


                                            }





                                            EditorGUILayout.EndVertical();
                                            #endregion




                                            #endregion

                                            break;

                                        #endregion

                                        #region Forward3D targeting Settings
                                        case "Forward":

                                            #region Side By Side

                                            #region Left Settings
                                            InspectorVerticalBox(true);

                                            EditorGUIUtility.labelWidth = 170;

                                            EditorGUILayout.PropertyField(noTargetRotateBehaviour, GUILayout.MaxWidth(380));
                                            InspectorHelpBox("Determines the entities rotate behaviour before activating the ability", false);

                                            EditorGUILayout.Space();

                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedStartingPositionTargetAndOnWorldSettings();

                                            }


                                            EditorGUILayout.EndVertical();
                                            #endregion




                                            #endregion

                                            break;

                                        #endregion

                                        #region Selected Target targeting Settings 
                                        case "SelectedTarget":

                                            #region SideBySide

                                            #region Left Settings
                                            InspectorVerticalBox(true);

                                            EditorGUIUtility.labelWidth = 120;


                                            if (abilityBeforeTarget.boolValue == false) {

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(noTargetStillTravel, new GUIContent("Always Activate"));

                                                EditorGUILayout.PropertyField(auxiliarySoftTarget, new GUIContent("Auxiliary SoftTarget"));
                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("If Always Activate is enabled then a selected target is not required to activate the ability. If Aux SoftTarget is ticked then the ability will as backup treat the current softTarget as the main target if not current target exists. ");

                                            }


                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination, new GUIContent("Projectile Continuously Turn To Target"));
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");
                                                EditorGUIUtility.labelWidth = 170;
                                                EditorGUILayout.PropertyField(affectOnlyTarget, new GUIContent("Projectile Affect Target Only"));
                                                InspectorHelpBox("If ticked then the Ability will only collide and effect the selected target. Ignoring any other entity on the way. ");
                                            }




                                            EditorGUIUtility.labelWidth = 150;

                                            EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                            EditorGUILayout.PropertyField(targetFacing, new GUIContent("Face Target Required"));



                                            EditorGUIUtility.labelWidth = 170;

                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(noTargetRotateBehaviour, GUILayout.MaxWidth(380));
                                            InspectorHelpBox("Determines the entities rotate behaviour when a target doesn't exist or entity is set to not rotate to the current target");

                                            ResetLabelWidth();

                                            ResetLabelWidth();


                                            EditorGUIUtility.labelWidth = 110;
                                            EditorGUILayout.PropertyField(useRange);

                                            EditorGUIUtility.labelWidth = 130;
                                            if (useRange.boolValue == true) {

                                                EditorGUILayout.PropertyField(selectedTargetRangeGreaterThan, new GUIContent("Range Greater Than"), GUILayout.MaxWidth(210));
                                                EditorGUILayout.PropertyField(selectedTargetRangeLessThan, new GUIContent("Range Less Than"), GUILayout.MaxWidth(210));

                                            }

                                            InspectorHelpBox("Ability will only activate if target is in the range defined");

                                            EditorGUIUtility.labelWidth = 140;



                                            EditorGUILayout.PropertyField(selectedTargetRestrictTargets, new GUIContent("Restrict Targets"));
                                            if (selectedTargetRestrictTargets.boolValue == true) {
                                                InspectorListBox("Only Activate On Following Tags", selectedTargetOnlyCastOnTag);
                                            }



                                            EditorGUILayout.EndVertical();
                                            #endregion




                                            #endregion

                                            break;

                                        #endregion

                                        #region NearestTag targeting Settings 
                                        case "NearestTag":

                                            #region SideBySide

                                            #region Left Settings
                                            InspectorVerticalBox(true);


                                            EditorGUIUtility.labelWidth = 120;


                                            EditorGUILayout.PropertyField(noTargetStillTravel, new GUIContent("Always Activate"));
                                            InspectorHelpBox("If Always Activate is enabled then a selected target is not required to activate the ability.");


                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination, new GUIContent("Projectile Continuously Turn To Target"));
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");
                                                EditorGUIUtility.labelWidth = 170;
                                                EditorGUILayout.PropertyField(affectOnlyTarget, new GUIContent("Projectile Affect Target Only"));
                                                InspectorHelpBox("If ticked then the Ability will only collide and effect the selected target. Ignoring any other entity on the way. ");
                                            }

                                            ResetLabelWidth();

                                            EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                            InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");

                                            EditorGUIUtility.labelWidth = 140;
                                            EditorGUILayout.PropertyField(targetFacing, new GUIContent("Face Target Required"));
                                            InspectorHelpBox("If the entity has a target then the Ability will only activate if the entity is facing the target");

                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {


                                                EditorGUIUtility.labelWidth = 110;
                                                EditorGUILayout.PropertyField(useRange);

                                                EditorGUIUtility.labelWidth = 130;
                                                if (useRange.boolValue == true) {

                                                    EditorGUILayout.PropertyField(selectedTargetRangeGreaterThan, new GUIContent("Range Greater Than"), GUILayout.MaxWidth(210));
                                                    EditorGUILayout.PropertyField(selectedTargetRangeLessThan, new GUIContent("Range Less Than"), GUILayout.MaxWidth(210));

                                                }

                                                InspectorHelpBox("Ability will only activate if target is in the range defined");

                                                EditorGUIUtility.labelWidth = 140;

                                                if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target") {

                                                    EditorGUILayout.PropertyField(selectedTargetRestrictTargets, new GUIContent("Restrict Targets"));
                                                    if (selectedTargetRestrictTargets.boolValue == true) {
                                                        InspectorListBox("Only Activate On Following Tags", selectedTargetOnlyCastOnTag);
                                                    }
                                                }


                                                EditorGUILayout.Space();



                                            }



                                            ResetLabelWidth();

                                            EditorGUILayout.EndVertical();
                                            #endregion



                                            #endregion


                                            break;

                                        #endregion

                                        #region Self targeting Settings 
                                        case "Self":

                                            #region SideBySide

                                            #region Left Settings
                                            InspectorVerticalBox(true);

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {


                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination, new GUIContent("Projectile Continuously Turn To Target"));
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");
                                                EditorGUIUtility.labelWidth = 170;
                                                EditorGUILayout.PropertyField(affectOnlyTarget, new GUIContent("Projectile Affect Target Only"));
                                                InspectorHelpBox("If ticked then the Ability will only collide and effect the selected target. Ignoring any other entity on the way. ");



                                            } else if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "Target" && ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "OnWorld") {


                                                EditorGUILayout.HelpBox(" self does not have have specific settings whilst ability is a raycast.", MessageType.Warning);
                                            }


                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedStartingPositionTargetAndOnWorldSettings();

                                            }


                                            EditorGUILayout.EndVertical();
                                            #endregion




                                            #endregion



                                            break;

                                        #endregion

                                        #region ToWorld targeting Settings

                                        case "ToWorld":

                                            #region SideBySide

                                            #region Left Settings
                                            InspectorVerticalBox(true);


                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination, new GUIContent("Projectile Continuously Turn To Target"));
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");

                                                ResetLabelWidth();

                                                EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");


                                                GetSharedStartingPositionTargetAndOnWorldSettings();




                                                EditorGUILayout.Space();



                                            } else {


                                                EditorGUILayout.HelpBox(" To World does not have specific settings whilst ability is a raycast.", MessageType.Warning);
                                            }


                                            EditorGUILayout.EndVertical();
                                            #endregion




                                            #endregion



                                            ResetLabelWidth();

                                            break;

                                        #endregion

                                        #region MouseTarget targeting Settings 
                                        case "MouseTarget":

                                            #region SideBySide

                                            #region Left Settings
                                            InspectorVerticalBox(true);

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination, new GUIContent("Projectile Continuously Turn To Target"));
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");

                                            }

                                            ResetLabelWidth();

                                            EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                            InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");


                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                ResetLabelWidth();

                                                EditorGUILayout.PropertyField(mouseFrontOnly);
                                                InspectorHelpBox("If enabled the target position will always be infront of the player. If false then player will activate all around in world position. ", false);

                                                EditorGUILayout.Space();

                                                EditorGUIUtility.labelWidth = 60;

                                                EditorGUILayout.PropertyField(mouseForwardLockX, new GUIContent("Lock X"));
                                                EditorGUILayout.PropertyField(mouseForwardLockY, new GUIContent("Lock Y"));
                                                EditorGUILayout.PropertyField(mouseForwardLockZ, new GUIContent("Lock Z"));
                                                InspectorHelpBox("By ticking the Locks the Ability will not record the corresponding co-ordinate for the target. Example: By locking Y the ability no matter where the mouse has clicked will never travel up or down.", false);

                                                EditorGUILayout.Space();
                                            }



                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedStartingPositionTargetAndOnWorldSettings();

                                            }


                                            ResetLabelWidth();



                                            EditorGUILayout.EndVertical();
                                            #endregion




                                            #endregion



                                            break;
                                        #endregion

                                        #region MouseForward targeting Settings
                                        case "MouseForward":

                                            #region Side by Side

                                            #region Left Side
                                            InspectorVerticalBox(true);

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                ResetLabelWidth();

                                                EditorGUILayout.PropertyField(mouseFrontOnly);
                                                InspectorHelpBox("If enabled the target position will always be infront of the player. If false then player will activate all around in world position. ", false);

                                                EditorGUILayout.Space();

                                                EditorGUIUtility.labelWidth = 60;

                                                EditorGUILayout.PropertyField(mouseForwardLockX, new GUIContent("Lock X"));
                                                EditorGUILayout.PropertyField(mouseForwardLockY, new GUIContent("Lock Y"));
                                                EditorGUILayout.PropertyField(mouseForwardLockZ, new GUIContent("Lock Z"));
                                                InspectorHelpBox("By ticking the Locks the Ability will not record the corresponding co-ordinate for the target. Example: By locking Y the ability no matter where the mouse has clicked will never travel up or down.", false);

                                                EditorGUILayout.Space();

                                            } else if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "Target" && ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "OnWorld") {
                                                EditorGUILayout.HelpBox("No additional Mouse Forward settings exist when ability is set to raycast mode", MessageType.Warning);
                                            }



                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedStartingPositionTargetAndOnWorldSettings();

                                            }

                                            ResetLabelWidth();

                                            EditorGUILayout.EndVertical();
                                            #endregion



                                            #endregion


                                            break;
                                        #endregion

                                        #region CrossHair targeting Settings
                                        case "Crosshair":

                                            #region Side By Side

                                            #region Left Side
                                            InspectorVerticalBox(true);


                                            EditorGUIUtility.labelWidth = 220;
                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(continuouslyTurnToDestination, new GUIContent("Projectile Continuously Turn To Target"));
                                                InspectorHelpBox("If enabled then the projectile will stop at target else if not ticked it will carry on travelling.");

                                                ResetLabelWidth();
                                                EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");


                                                EditorGUIUtility.labelWidth = 90;


                                                EditorGUILayout.PropertyField(affectLayer, GUILayout.MaxWidth(250));

                                                EditorGUIUtility.labelWidth = 170;
                                                EditorGUILayout.PropertyField(crossHairRaycastRadius, GUILayout.MaxWidth(250));


                                                EditorGUILayout.PropertyField(crossHairRaycastReturnedDistance, new GUIContent("Returned Distance"), GUILayout.MaxWidth(250));
                                                InspectorHelpBox("The distance point along the raycast which is returned if no objects have been hit or ability has been configured to always return a distance point only");

                                                EditorGUILayout.PropertyField(crossHairRaycastReturnDistancePointOnly, new GUIContent("Distance Point Only"));
                                                InspectorHelpBox("If selected then the ray cast will always get the distance point x units away instead of looking for points on objects hit by the cast");

                                                EditorGUILayout.PropertyField(crossHairRecordTargetOnActivation, new GUIContent("Record Target On Activation"));
                                                InspectorHelpBox("If selected then ray will be casted during ability activation otherwise it is done when the ability is initiated after being prepared etc");


                                            } else if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "Target" && ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) != "OnWorld") {

                                                ResetLabelWidth();
                                                EditorGUILayout.PropertyField(rotateToSelectedTarget, new GUIContent("Rotate to Target"));
                                                InspectorHelpBox("If enabled then entity will rotate to target when preparing and initating the ability");

                                            }



                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedStartingPositionTargetAndOnWorldSettings();

                                            }


                                            ResetLabelWidth();



                                            EditorGUILayout.EndVertical();
                                            #endregion


                                            #endregion



                                            break;
                                            #endregion

                                    }

                                    #endregion

                                    #region Travel Type Right Side Settings

                                    switch ((string)travelType.enumNames[travelType.enumValueIndex]) {

                                        #region CustomScript targeting Settings
                                        case "CustomScript":

                                            break;
                                        #endregion

                                        #region NoTravel/Melee targeting Settings
                                        case "NoTravel":


                                            #region Right Settings                                


                                            if ((string)abilityType.enumNames[abilityType.enumValueIndex] != "Melee" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {
                                                InspectorVerticalBox(true);


                                                GetSharedAbilityBeforeTargetSettings();


                                                EditorGUILayout.EndVertical();

                                            }


                                            ResetLabelWidth();


                                            #endregion


                                            break;
                                        #endregion

                                        #region Shared Ability Before Target Settings
                                        case "Mouse2D":
                                        case "Forward":
                                        case "Self":
                                        case "MouseForward":
                                        case "Crosshair":

                                            #region Right Settings                                


                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {
                                                InspectorVerticalBox(true);


                                                GetSharedAbilityBeforeTargetSettings();


                                                EditorGUILayout.EndVertical();

                                            }


                                            ResetLabelWidth();


                                            #endregion

                                            break;

                                        #endregion

                                        #region Selected Target targeting Settings 
                                        case "SelectedTarget":


                                            #region Right Settings
                                            InspectorVerticalBox(true);

                                            GetSharedAbilityBeforeTargetSettings();


                                            EditorGUILayout.EndVertical();
                                            #endregion


                                            break;

                                        #endregion

                                        #region NearestTag targeting Settings 
                                        case "NearestTag":


                                            #region Right Settings
                                            InspectorVerticalBox(true);

                                            EditorGUIUtility.labelWidth = 180;


                                            EditorGUILayout.PropertyField(travelNearestTagRange, new GUIContent("Nearest Tag Range Amount"), GUILayout.MaxWidth(240));
                                            EditorGUILayout.PropertyField(travelNearestTagIgnoreOriginator, new GUIContent("Ignore Activating Entity"));
                                            EditorGUILayout.PropertyField(travelNearestTagRandomiseSearch, new GUIContent("Randomise Search"));

                                            InspectorHelpBox("The ability will find a target by searching within the range setup for any tags provided in the list below. Tags will be searched in the order listed below. Randomise search will shuffle the entities found before checking tags");


                                            InspectorListBox("Target Nearest Tag:", travelNearestTagList);


                                            ResetLabelWidth();


                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedAbilityBeforeTargetSettings();


                                            }



                                            EditorGUILayout.EndVertical();
                                            #endregion

                                            break;

                                        #endregion

                                        #region ToWorld targeting Settings

                                        case "ToWorld":

                                            #region Right Settings
                                            InspectorVerticalBox(true);


                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                                EditorGUIUtility.labelWidth = 140;
                                                EditorGUILayout.PropertyField(abilityBeforeTarget);
                                                InspectorHelpBox("If ticked then the user will have to select the target after the Ability key has been pressed.");

                                                if (abilityBeforeTarget.boolValue == true) {

                                                    EditorGUILayout.PropertyField(loopTillTargetFound, new GUIContent("Loop Till Target Found"));
                                                    InspectorHelpBox("If disabled then the ability will cancel if the target was not correctly selected, else it will wait until a correct target is selected");


                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(abilityWorldTargetIndicatorImage, new GUIContent("World Indicator Image"));

                                                    if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {

                                                        Texture texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_WorldTargetIndicatorImage", typeof(Texture)) as Texture;

                                                        if (texture != null) {
                                                            abilityWorldTargetIndicatorImage.FindPropertyRelative("refVal").objectReferenceValue = texture;
                                                            abilityWorldTargetIndicatorImage.FindPropertyRelative("refName").stringValue = texture.name;
                                                        }

                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                    InspectorHelpBox("The indicator which will display showing the world position the ability will travel towards");


                                                    EditorGUIUtility.labelWidth = 230;
                                                    EditorGUILayout.PropertyField(abilityBeforeTargetWorldIndicatorScaleToEffectRadius, new GUIContent("Scale World Indicator to Effect Radius"));

                                                    EditorGUIUtility.labelWidth = 140;

                                                    if (abilityBeforeTargetWorldIndicatorScaleToEffectRadius.boolValue == false) {
                                                        EditorGUILayout.PropertyField(abilityBeforeTargetWorldIndicatorScale, new GUIContent("World Indicator Scale"), GUILayout.MaxWidth(210));
                                                    }
                                                    InspectorHelpBox("If true then the world indicate will scale to show the effect radius of the ability or if false a scale can be set");


                                                    EditorGUIUtility.labelWidth = 140;
                                                    if (useRange.boolValue == true && selectedTargetRangeGreaterThan.floatValue == 0) {

                                                        EditorGUILayout.BeginHorizontal();

                                                        EditorGUILayout.PropertyField(abilityRangeIndicatorImage, new GUIContent("Range Indicator Image"));

                                                        if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {

                                                            Texture texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_RangeIndicatorImage", typeof(Texture)) as Texture;

                                                            if (texture != null) {
                                                                abilityRangeIndicatorImage.FindPropertyRelative("refVal").objectReferenceValue = texture;
                                                                abilityRangeIndicatorImage.FindPropertyRelative("refName").stringValue = texture.name;
                                                            }

                                                        }
                                                        EditorGUILayout.EndHorizontal();
                                                        InspectorHelpBox("The indicator which will display showing the range of the ability");
                                                    }


                                                }



                                            } else {


                                                EditorGUILayout.HelpBox(" To World does not have specific settings whilst ability is a raycast.", MessageType.Warning);
                                            }


                                            EditorGUILayout.EndVertical();
                                            #endregion


                                            break;

                                        #endregion

                                        #region MouseTarget targeting Settings 
                                        case "MouseTarget":

                                            #region Right Settings



                                            InspectorVerticalBox(true);

                                            if (((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "Target" || ((string)startingPosition.enumNames[startingPosition.enumValueIndex]) == "OnWorld") {

                                                GetSharedAbilityBeforeTargetSettings();


                                            } else {

                                                EditorGUIUtility.labelWidth = 130;
                                                EditorGUILayout.PropertyField(abilityBeforeTarget);
                                                InspectorHelpBox("If ticked then the user will have to select the target after the Ability key has been pressed.");
                                            }

                                            if (abilityBeforeTarget.boolValue == true) {



                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(abilityMouseTargetIndicatorImage, new GUIContent("Indicator Image"));

                                                if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {

                                                    Texture texture = Resources.Load("ABC-TargetIndicator/IndicatorImages/ABC_MouseTargetIndicatorImage", typeof(Texture)) as Texture;

                                                    if (texture != null) {
                                                        abilityMouseTargetIndicatorImage.FindPropertyRelative("refVal").objectReferenceValue = texture;
                                                        abilityMouseTargetIndicatorImage.FindPropertyRelative("refName").stringValue = texture.name;
                                                    }

                                                }
                                                EditorGUILayout.EndHorizontal();

                                                InspectorHelpBox("The indicator which will display showing the direction the ability will travel towards (following mouse location)");

                                                EditorGUILayout.PropertyField(abilityBeforeTargetMouseTargetIndicatorLength, new GUIContent("Indicator Length"), GUILayout.MaxWidth(210));
                                                InspectorHelpBox("The length of the indicator");


                                            }


                                            EditorGUILayout.EndVertical();






                                            #endregion

                                            break;
                                            #endregion

                                    }


                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    break;
                                case 1:

                                    InspectorSectionHeader("Travel Settings");


                                    #region AllWay 

                                    InspectorVerticalBox();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                        switch ((string)travelType.enumNames[travelType.enumValueIndex]) {
                                            #region NoTravel Travel & Misc Settings
                                            case "NoTravel":
                                            case "CustomScript":


                                                EditorGUIUtility.labelWidth = 130;

                                                EditorGUILayout.HelpBox("Travel Types: No Travel and Custom Script can not have travel delay or durations", MessageType.Warning);



                                                break;
                                            #endregion

                                            #region  Travel & Misc Settings for rest of travel types
                                            // these at present all share the same abilities (Except onworld/toworld which adds a little setting) this has been setup this way for future development
                                            // if more unique settings was needed for each of these scripts

                                            case "Mouse2D":
                                            case "Forward":
                                            case "SelectedTarget":
                                            case "Self":
                                            case "ToWorld":
                                            case "MouseTarget":
                                            case "MouseForward":
                                            case "Crosshair":
                                            case "NearestTag":


                                                EditorGUIUtility.labelWidth = 140;



                                                EditorGUILayout.BeginHorizontal();

                                                EditorGUILayout.PropertyField(travelDelay, GUILayout.MaxWidth(180));


                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("Delay time before ability will start travelling");
                                                EditorGUIUtility.labelWidth = 150;
                                                EditorGUILayout.PropertyField(applyTravelDuration);

                                                GUILayout.BeginHorizontal();


                                                // travel duration only applies to abilities that do not always follow a target 
                                                if (applyTravelDuration.boolValue == true) {
                                                    EditorGUIUtility.labelWidth = 150;
                                                    EditorGUILayout.PropertyField(travelDurationTime, GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.PropertyField(travelDurationStopSuddenly, new GUIContent("Stop Instantly"));

                                                }
                                                ResetLabelWidth();
                                                GUILayout.EndHorizontal();

                                                InspectorHelpBox("How long the ability will travel for and if it will stop instantly or over time with physics");

                                                if (applyTravelDuration.boolValue == true) {

                                                    InspectorListBox("Originator Tags Required To Apply Travel Duration", travelDurationOriginatorTagsRequired);

                                                }


                                                break;
                                                #endregion
                                        }


                                    } else {

                                        EditorGUILayout.HelpBox("RayCast Abilities do not currently have travel settings.", MessageType.Warning);
                                    }



                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region AllWay 

                                    InspectorVerticalBox();


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        EditorGUILayout.Space();
                                        switch ((string)travelType.enumNames[travelType.enumValueIndex]) {
                                            #region NoTravel Travel & Misc Settings
                                            case "NoTravel":
                                            case "CustomScript":


                                                EditorGUIUtility.labelWidth = 130;


                                                EditorGUILayout.BeginHorizontal();

                                                EditorGUILayout.PropertyField(mass, GUILayout.MaxWidth(180));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(travelDrag, GUILayout.MaxWidth(180));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.EndHorizontal();

                                                EditorGUILayout.Space();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(applyGravity, GUILayout.MaxWidth(305));

                                                if (applyGravity.boolValue == true) {

                                                    EditorGUILayout.PropertyField(applyGravityDelay, new GUIContent("Gravity Delay"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                }

                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("Add gravity to the ability after the delay time set");

                                                EditorGUILayout.PropertyField(isKinematic, GUILayout.MaxWidth(180));

                                                EditorGUILayout.PropertyField(neverSleep, GUILayout.MaxWidth(180));
                                                InspectorHelpBox("If enabled then the ability will never sleep and always look for collisions. Sleeping occurs when a Rigidbody is moving slower than a defined minimum linear or rotational speed, the physics engine assumes it has come to a halt");
                                                break;
                                            #endregion

                                            #region  Travel & Misc Settings for rest of travel types
                                            // these at present all share the same abilities (Except onworld/toworld which adds a little setting) this has been setup this way for future development
                                            // if more unique settings was needed for each of these scripts

                                            case "Mouse2D":
                                            case "Forward":
                                            case "SelectedTarget":
                                            case "Self":
                                            case "ToWorld":
                                            case "MouseTarget":
                                            case "MouseForward":
                                            case "Crosshair":
                                            case "NearestTag":






                                                EditorGUILayout.BeginHorizontal();

                                                EditorGUILayout.PropertyField(mass, GUILayout.MaxWidth(180));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(travelDrag, GUILayout.MaxWidth(180));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.EndHorizontal();

                                                EditorGUILayout.Space();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(applyGravity, GUILayout.MaxWidth(305));

                                                if (applyGravity.boolValue == true) {

                                                    EditorGUILayout.PropertyField(applyGravityDelay, new GUIContent("Gravity Delay"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                }

                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("Add gravity to the ability after the delay time set");

                                                EditorGUILayout.PropertyField(neverSleep, GUILayout.MaxWidth(180));
                                                InspectorHelpBox("If enabled then the ability will never sleep and always look for collisions. Sleeping occurs when a Rigidbody is moving slower than a defined minimum linear or rotational speed, the physics engine assumes it has come to a halt");

                                                break;
                                                #endregion
                                        }

                                    } else {

                                        EditorGUILayout.HelpBox("RayCast Abilities do not currently have travel settings.", MessageType.Warning);
                                    }




                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    InspectorSectionHeader("Bounce & Boomerang");

                                    #region AllWay 

                                    InspectorVerticalBox();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                        switch ((string)travelType.enumNames[travelType.enumValueIndex]) {
                                            #region NoTravel Travel & Misc Settings
                                            case "NoTravel":
                                            case "CustomScript":




                                                EditorGUILayout.HelpBox("Travel Types: No Travel and Custom Script can not boomerang or bounce", MessageType.Warning);



                                                break;
                                            #endregion

                                            #region  Travel & Misc Settings for rest of travel types
                                            // these at present all share the same abilities (Except onworld/toworld which adds a little setting) this has been setup this way for future development
                                            // if more unique settings was needed for each of these scripts

                                            case "Mouse2D":
                                            case "Forward":
                                            case "SelectedTarget":
                                            case "Self":
                                            case "ToWorld":
                                            case "MouseTarget":
                                            case "MouseForward":
                                            case "Crosshair":
                                            case "NearestTag":



                                                EditorGUIUtility.labelWidth = 130;

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(boomerangMode, new GUIContent("Boomerang"), GUILayout.MaxWidth(306));

                                                if (boomerangMode.boolValue == true) {
                                                    EditorGUILayout.PropertyField(boomerangDelay, new GUIContent("Delay"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                }

                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("Boomerang Effect - after delay the ability will return back to the caster. Once caster is reached ability will dissapear", false);

                                                EditorGUILayout.Space();

                                                EditorGUILayout.PropertyField(bounceMode, new GUIContent("Bounce"));
                                                InspectorHelpBox("Bounce Effect - After collision the ability will bounce to near targets depending on bounce targeting type.", false);

                                                if (bounceMode.boolValue == true) {
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(bounceAmount, new GUIContent("Bounce Amount"), GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.PropertyField(bounceRange, GUILayout.MaxWidth(180));
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.EndHorizontal();


                                                    EditorGUIUtility.labelWidth = 210;
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(enableRandomBounce, new GUIContent("Random Bounce"));
                                                    EditorGUILayout.PropertyField(bounceOnCaster, new GUIContent("Bounce On Activating Entity"));
                                                    EditorGUILayout.EndHorizontal();


                                                    EditorGUIUtility.labelWidth = 210;
                                                    EditorGUILayout.PropertyField(startBounceTagRequired, new GUIContent("Start Bounce On Specific Tag"));
                                                    ResetLabelWidth();

                                                    if (startBounceTagRequired.boolValue == true) {
                                                        InspectorListBox("Tag Required To Start Ability Bouncing", startBounceRequiredTags);
                                                    }
                                                    EditorGUILayout.Space();

                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.LabelField("Position Offset", GUILayout.MaxWidth(100));

                                                    EditorGUILayout.PropertyField(bouncePositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                    EditorGUILayout.EndHorizontal();
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.PropertyField(bouncePositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.PropertyField(bouncePositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.EndHorizontal();

                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.PropertyField(bounceTarget, GUILayout.MaxWidth(310));
                                                    EditorGUILayout.Space();

                                                    if (((string)bounceTarget.enumNames[bounceTarget.enumValueIndex]) == "NearestTag") {

                                                        InspectorListBox("Bounce Tag", bounceTag);
                                                    }
                                                }



                                                EditorGUILayout.Space();

                                                break;
                                                #endregion
                                        }

                                    } else {

                                        EditorGUILayout.HelpBox("RayCast Abilities can't bounce or boomerang.", MessageType.Warning);
                                    }




                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    break;

                                case 2:

                                    InspectorSectionHeader("Additional Starting Positions", "After initiating starts an extra Ability object will be created one after another for each Additional Starting Position. The object is a duplicate of the normal " +
                                        "Ability object and will operate in the same way. This functionality can be used for example " +
                                        "to have 2 fireballs fire from both hands or for melee attacks to have each fist apply damage.");

                                    #region Additional Starting Position

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        if (GUILayout.Button(new GUIContent(" Add Additional Starting Position", AddIcon, "Add Additional Starting Position"))) {
                                            var additionalSPIndex = additionalStartPositions.arraySize;
                                            additionalStartPositions.InsertArrayElementAtIndex(additionalSPIndex);

                                        }

                                        for (int n = 0; n < additionalStartPositions.arraySize; n++) {

                                            #region AllWay 

                                            InspectorVerticalBox();

                                            InspectorPropertyBox("Additional Start Position", additionalStartPositions, n);

                                            EditorGUILayout.Space();

                                            if (additionalStartPositions.arraySize == 0 || n > additionalStartPositions.arraySize - 1) {
                                                break;
                                            }


                                            SerializedProperty meAdditionalStartPosition = additionalStartPositions.GetArrayElementAtIndex(n);
                                            SerializedProperty addStartingPosition = meAdditionalStartPosition.FindPropertyRelative("startingPosition");
                                            SerializedProperty addstartingDelayType = meAdditionalStartPosition.FindPropertyRelative("startingDelayType");
                                            SerializedProperty addStartingDelay = meAdditionalStartPosition.FindPropertyRelative("startingDelay");
                                            SerializedProperty addstartingDelayInitiatingAnimationPercentage = meAdditionalStartPosition.FindPropertyRelative("startingDelayInitiatingAnimationPercentage");

                                            SerializedProperty addStartingPositionOnObject = meAdditionalStartPosition.FindPropertyRelative("startingPositionOnObject");
                                            SerializedProperty addStartingPositionOnTag = meAdditionalStartPosition.FindPropertyRelative("startingPositionOnTag");
                                            SerializedProperty addStartingPositionOffset = meAdditionalStartPosition.FindPropertyRelative("startingPositionOffset");
                                            SerializedProperty addStartingPositionForwardOffset = meAdditionalStartPosition.FindPropertyRelative("startingPositionForwardOffset");
                                            SerializedProperty addStartingPositionRightOffset = meAdditionalStartPosition.FindPropertyRelative("startingPositionRightOffset");
                                            SerializedProperty addStartingRotation = meAdditionalStartPosition.FindPropertyRelative("startingRotation");
                                            SerializedProperty addSetEulerRotation = meAdditionalStartPosition.FindPropertyRelative("setEulerRotation");
                                            SerializedProperty addStartingPositionrepeatInitiatingGraphic = meAdditionalStartPosition.FindPropertyRelative("repeatInitiatingGraphic");
                                            SerializedProperty addStartingPositionrepeatInitiatingGraphicDelay = meAdditionalStartPosition.FindPropertyRelative("repeatInitiatingGraphicDelay");
                                            SerializedProperty addStartingPositionUseWeaponTrail = meAdditionalStartPosition.FindPropertyRelative("useWeaponTrail");
                                            SerializedProperty addStartingPositionWeaponTrailGraphicIteration = meAdditionalStartPosition.FindPropertyRelative("weaponTrailGraphicIteration");



                                            ResetLabelWidth();

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUIUtility.labelWidth = 65;
                                            EditorGUILayout.PropertyField(addstartingDelayType, new GUIContent("Activate"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.Space();
                                            if ((string)addstartingDelayType.enumNames[addstartingDelayType.enumValueIndex] == "AfterDelay") {
                                                EditorGUIUtility.labelWidth = 55;
                                                EditorGUILayout.PropertyField(addStartingDelay, new GUIContent("Delay"), GUILayout.MaxWidth(120));
                                            } else if ((string)addstartingDelayType.enumNames[addstartingDelayType.enumValueIndex] == "AtAnimationPercentage") {
                                                EditorGUIUtility.labelWidth = 120;
                                                EditorGUILayout.PropertyField(addstartingDelayInitiatingAnimationPercentage, new GUIContent("Initiating Percentage"), GUILayout.MaxWidth(320));
                                            }
                                            EditorGUILayout.Space();

                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            ResetLabelWidth();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(addStartingPosition, GUILayout.MaxWidth(230));

                                            // get enum value index and if it's on object display new layout
                                            if (((string)addStartingPosition.enumNames[addStartingPosition.enumValueIndex]) == "OnObject") {


                                                EditorGUILayout.PropertyField(addStartingPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(400));
                                            }




                                            // get enum value index and if it's on object display new layout
                                            if (((string)addStartingPosition.enumNames[addStartingPosition.enumValueIndex]) == "OnTag" || ((string)addStartingPosition.enumNames[addStartingPosition.enumValueIndex]) == "OnSelfTag") {


                                                EditorGUIUtility.labelWidth = 10;
                                                EditorGUILayout.LabelField("Select Tag");
                                                addStartingPositionOnTag.stringValue = EditorGUILayout.TagField(addStartingPositionOnTag.stringValue, GUILayout.MaxWidth(200));



                                            }
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();

                                            ResetLabelWidth();


                                            EditorGUILayout.Space();
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Position Offset", GUILayout.MaxWidth(100));
                                            EditorGUILayout.PropertyField(addStartingPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(addStartingPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.PropertyField(addStartingPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(addStartingRotation, GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();
                                                EditorGUIUtility.labelWidth = 130;
                                                EditorGUILayout.PropertyField(addSetEulerRotation, GUILayout.MaxWidth(250));
                                                EditorGUILayout.EndHorizontal();

                                            }

                                            InspectorHelpBox("If enabled then the weapon trail setup on the current equipped weapon will activate when the ability initiates.");

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 170;
                                            EditorGUILayout.PropertyField(addStartingPositionUseWeaponTrail, new GUIContent("Use Equipped Weapon Trail"), GUILayout.MaxWidth(350));

                                            if (addStartingPositionUseWeaponTrail.boolValue == true) {

                                                EditorGUILayout.PropertyField(addStartingPositionWeaponTrailGraphicIteration, new GUIContent("Weapon Graphic Iteration"), GUILayout.MaxWidth(230));

                                            }

                                            EditorGUILayout.EndHorizontal();

                                            InspectorHelpBox("If enabled then the weapon trail setup on the current equipped weapon will activate, depending on the graphic iteration picked 0 being the first weapon graphic in the equipped weapon config");


                                            EditorGUILayout.Space();
                                            EditorGUIUtility.labelWidth = 160;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(addStartingPositionrepeatInitiatingGraphic, GUILayout.MaxWidth(250));
                                            if (addStartingPositionrepeatInitiatingGraphic.boolValue == true && ((string)abilityType.enumNames[abilityType.enumValueIndex]) != "Melee" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee" && initiatingAestheticActivateWithAbility.boolValue == false) {
                                                EditorGUILayout.PropertyField(addStartingPositionrepeatInitiatingGraphicDelay, new GUIContent("Delay From Initiation Start"), GUILayout.MaxWidth(250));
                                            }
                                            EditorGUILayout.EndHorizontal();
                                            InspectorHelpBox("Will repeat the initiating graphic at the additional starting position, after the delay starting from when the ability starts initiating");

                                            ResetLabelWidth();


                                            EditorGUILayout.EndVertical();

                                            #endregion


                                        }


                                    } else {

                                        EditorGUILayout.HelpBox("RayCast Abilities can't have additional starting positions.", MessageType.Warning);
                                    }

                                    #endregion

                                    break;

                            }


                            #endregion


                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndScrollView();
                            #endregion


                            EditorGUILayout.EndHorizontal();



                            break;
                        case 2:

                            EditorGUILayout.BeginHorizontal();

                            #region Controls/ability info
                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(abilityInfoWidth));

                            if (abilityCont != null)
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref abilityCont.toolbarAbilityManagerCollisionImpactSettingsSelection, ref collisionimpactSettingsToolbar);
                            else
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref toolbarAbilityManagerCollisionImpactSettingsSelection, ref collisionimpactSettingsToolbar);


                            EditorGUILayout.EndVertical();

                            #endregion

                            InspectorBoldVerticleLine();


                            #region Collision & Physics 

                            editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                           false,
                                                                           false);

                            EditorGUILayout.BeginVertical();

                            #region Settings

                            switch ((int)ToolbarAbilityManagerCollisionImpactSettingsSelection) {
                                case 0:

                                    InspectorSectionHeader("Add Collider Settings");

                                    #region AllWay 



                                    InspectorVerticalBox();

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        EditorGUILayout.PropertyField(addAbilityCollider, new GUIContent("Add Collider"));
                                        InspectorHelpBox("If disabled then the ability will initiate without a collider being added. Any colliders already present on the object will remain.", false);

                                        if (addAbilityCollider.boolValue == true) {
                                            EditorGUILayout.PropertyField(useColliderTrigger, new GUIContent("Is Trigger"));

                                            ResetLabelWidth();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(useGraphicRadius, new GUIContent("Graphic Radius"));

                                            if (useGraphicRadius.boolValue == false) {
                                                EditorGUILayout.PropertyField(colliderRadius, GUILayout.MaxWidth(180));
                                                EditorGUILayout.Space();
                                            }
                                            EditorGUILayout.EndHorizontal();


                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Collider Offset", GUILayout.MaxWidth(100));
                                            EditorGUILayout.PropertyField(colliderOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            InspectorHelpBox("If enabled then the projectile radius is calculated depending on the renderer size of the projectile. If disabled a radius can be picked");


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 130;
                                            EditorGUILayout.PropertyField(applyColliderSettingsToParent, new GUIContent("Apply To Parent"));
                                            EditorGUILayout.PropertyField(applyColliderSettingsToChildren, new GUIContent("Apply To Children"));
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();
                                            InspectorHelpBox("You can select to apply the settings to the abilities parent collider and/or colliders found in the children of the ability");

                                            GUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(colliderTimeDelay, new GUIContent("Time Delay"));

                                            if (colliderTimeDelay.boolValue == true) {
                                                EditorGUILayout.PropertyField(colliderDelayTime, new GUIContent("Time"), GUILayout.MaxWidth(180));
                                            }
                                            EditorGUILayout.Space();
                                            GUILayout.EndHorizontal();
                                            InspectorHelpBox("Collider will come on after the delay time.");

                                            EditorGUILayout.PropertyField(colliderKeyPressDelay, new GUIContent("Key Delay"));

                                            GUILayout.BeginHorizontal();

                                            if (colliderKeyPressDelay.boolValue == true) {

                                                EditorGUILayout.PropertyField(colliderDelayInputType, new GUIContent("Input Type"), GUILayout.MaxWidth(250));

                                                EditorGUILayout.Space();


                                                if (((string)colliderDelayInputType.enumNames[colliderDelayInputType.enumValueIndex]) == "Key") {

                                                    EditorGUILayout.PropertyField(colliderDelayKey, new GUIContent("Key"), GUILayout.MaxWidth(250));

                                                } else {

                                                    EditorGUILayout.PropertyField(colliderDelayButton, new GUIContent("Button"), GUILayout.MaxWidth(250));

                                                }

                                                EditorGUILayout.Space();

                                            }
                                            GUILayout.EndHorizontal();
                                            InspectorHelpBox("Collider will come on after the key has been pressed.");

                                        }
                                    } else {


                                        EditorGUILayout.HelpBox("Collider can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    InspectorSectionHeader("Collider Collision Type");

                                    #region AllWay 



                                    InspectorVerticalBox();


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        InspectorHelpBox("Any colliders attached to the ability added manually or automatically through the above settings will collide using the types selected in the boxes below. If Particle Collison is true make sure the World Collision setting is enabled on the mainGraphic.", false);

                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(onEnter);
                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.PropertyField(onStay);
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(onExit);
                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.PropertyField(particleCollision);

                                        EditorGUILayout.EndHorizontal();

                                        if (onStay.boolValue == true) {
                                            EditorGUILayout.PropertyField(onStayInterval, GUILayout.MaxWidth(180));
                                            InspectorHelpBox("How often On Stay collisions will occur. If set to 0 then On Stay collisions will run with the update loop", false);
                                        }

                                        ResetLabelWidth();

                                        EditorGUILayout.Space();

                                        EditorGUIUtility.labelWidth = 240;
                                        EditorGUILayout.PropertyField(enableCollisionAfterKeyPress);

                                        if (enableCollisionAfterKeyPress.boolValue == true) {

                                            EditorGUILayout.PropertyField(enableCollisionAfterKeyInputType, new GUIContent("Input Type"), GUILayout.MaxWidth(350));

                                            if (((string)enableCollisionAfterKeyInputType.enumNames[enableCollisionAfterKeyInputType.enumValueIndex]) == "Key") {

                                                EditorGUILayout.PropertyField(enableCollisionAfterKey, new GUIContent("Key"), GUILayout.MaxWidth(350));

                                            } else {

                                                EditorGUILayout.PropertyField(enableCollisionAfterKeyButton, new GUIContent("Button"), GUILayout.MaxWidth(350));

                                            }

                                        }

                                        InspectorHelpBox("Enable collisions after a key has been pressed, useful for setting up remote bomb type abilities.", false);
                                        ResetLabelWidth();


                                    } else {


                                        EditorGUILayout.HelpBox("Collider can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion



                                    break;

                                case 1:

                                    InspectorSectionHeader("Settings");

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        #region AllWay 

                                        EditorGUILayout.BeginHorizontal();

                                        #region Collision Settings

                                        InspectorVerticalBox();




                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 200;
                                        EditorGUILayout.PropertyField(affectOriginObject, new GUIContent("Collide w/ Activating Entity"));
                                        ResetLabelWidth();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(abilityCollisionIgnores, new GUIContent("Ability Collision"), GUILayout.MaxWidth(350));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();
                                        InspectorHelpBox("How does the ability behave when colliding against other abilities and can it collide with the activating entity.");
                                        ResetLabelWidth();


                                        EditorGUILayout.BeginHorizontal();
                                        if (((string)impactDestroy.enumNames[impactDestroy.enumValueIndex]) != "DestroyOnTerrainOnly") {
                                            EditorGUILayout.PropertyField(ignoreActiveTerrain, new GUIContent("Ignore Terrain"));
                                        }
                                        EditorGUIUtility.labelWidth = 200;
                                        EditorGUILayout.PropertyField(overrideIgnoreAbilityCollision);
                                        EditorGUILayout.EndHorizontal();
                                        InspectorHelpBox("If the entity is currently in ignore ability collision mode then this setting if true, will override that and still allow the ability to collide");

                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile") {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(chooseLayer, new GUIContent("Add to Layer"));
                                            if (chooseLayer.boolValue == true) {
                                                EditorGUILayout.PropertyField(abLayer, new GUIContent("Layer"), GUILayout.MaxWidth(250));
                                            }
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();

                                            InspectorHelpBox("Add name of Layer to add ability too");

                                        }









                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndHorizontal();

                                        #endregion

                                    }


                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Block/Parry Settings

                                    InspectorVerticalBox(true);

                                    EditorGUIUtility.labelWidth = 200;

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                        EditorGUILayout.PropertyField(overrideWeaponParrying);
                                    }
                                    EditorGUILayout.PropertyField(overrideWeaponBlocking);

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                        InspectorHelpBox("If ticked then the ability will ignore any weapon blocking and/or parrying");
                                    } else {
                                        InspectorHelpBox("If ticked then the ability will ignore weapon blocking");
                                    }

                                    if (overrideWeaponBlocking.boolValue == false) {
                                        EditorGUILayout.PropertyField(reduceWeaponBlockDurability);

                                        InspectorHelpBox("If ticked then the block durability will be decreased on the entity that blocks this ability");
                                    }



                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region Animation Settings

                                    InspectorVerticalBox(true);

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 170;
                                    EditorGUILayout.PropertyField(activateAnimationFromHit, new GUIContent("Activate Animation On Hit"));

                                    if (activateAnimationFromHit.boolValue == true) {
                                        EditorGUIUtility.labelWidth = 50;
                                        EditorGUILayout.PropertyField(activateAnimationFromHitDelay, new GUIContent("Delay"), GUILayout.MaxWidth(180));
                                    }

                                    EditorGUILayout.EndHorizontal();


                                    if (activateAnimationFromHit.boolValue == true) {
                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.PropertyField(activateAnimationFromHitUseAirAnimation, new GUIContent("Use Air Animations"), GUILayout.MaxWidth(280));
                                        ResetLabelWidth();
                                    }

                                    InspectorHelpBox("If true then a animation will activate when this ability hits");

                                    EditorGUIUtility.labelWidth = 190;

                                    if (activateAnimationFromHit.boolValue == true) {

                                        EditorGUILayout.PropertyField(activateSpecificHitAnimation);
                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 170;
                                        if (activateSpecificHitAnimation.boolValue == true) {

                                            EditorGUIUtility.labelWidth = 60;

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(activateSpecificHitAnimationUseClip, new GUIContent("Use Clip"));


                                            if (activateSpecificHitAnimationUseClip.boolValue == true) {
                                                EditorGUILayout.PropertyField(hitAnimationClipToActivate, new GUIContent(""), GUILayout.MaxWidth(340));
                                                EditorGUILayout.EndHorizontal();

                                                InspectorHelpBox("Enter the animation clip to activate on hit");
                                            } else {
                                                EditorGUIUtility.labelWidth = 90;
                                                EditorGUILayout.PropertyField(hitAnimationToActivate, new GUIContent("Hit Animation"), GUILayout.MaxWidth(280));
                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("If ticked then then the name of the 'Hit Animation' entered will activate, If animation not found then a random hit animation will play instead");
                                            }


                                        }




                                    }

                                    ResetLabelWidth();




                                    EditorGUILayout.EndVertical();

                                    #endregion



                                    EditorGUILayout.EndHorizontal();

                                    #endregion



                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Collide Required Settings

                                    InspectorVerticalBox(true);

                                    InspectorListBox("Ability Collide Required Tags", abilityRequiredTag);
                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    #region Collide Ignore Settings

                                    InspectorVerticalBox(true);

                                    InspectorListBox("Ability Collide Ignore Tags", abilityIgnoreTag);
                                    EditorGUILayout.Space();

                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion



                                    InspectorSectionHeader("Destroy Settings");


                                    #region All Way 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Collision Settings 2

                                    InspectorVerticalBox();

                                    InspectorHelpBox("Settings below define the rules on destroying the ability on collision", false, true);

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                        EditorGUILayout.PropertyField(destroyDelay, GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();

                                        EditorGUILayout.PropertyField(impactDestroy, GUILayout.MaxWidth(250));
                                        EditorGUILayout.Space();

                                        InspectorHelpBox("Determines what will destroy the ability. This can be used to filter out colliders which should not destroy.", false);
                                        InspectorListBox("Don't Destroy on Below Tags", destroyIgnoreTag);



                                        EditorGUILayout.Space();

                                    } else {


                                        EditorGUILayout.HelpBox("Collider can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion



                                    break;
                                case 2:

                                    InspectorSectionHeader("Splash Settings");

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();

                                    #region Splash Explosion  Settings 

                                    InspectorVerticalBox(true);

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {


                                        EditorGUIUtility.labelWidth = 130;
                                        EditorGUILayout.PropertyField(destroySplashExplosion, new GUIContent("Splash Explosion"));

                                        if (destroySplashExplosion.boolValue == true) {

                                            ResetLabelWidth();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(destroySplashExplosionRadius, new GUIContent("Explosion Radius"), GUILayout.MaxWidth(250));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(destroySplashExplosionPower, new GUIContent("Explosion Power"), GUILayout.MaxWidth(250));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            EditorGUILayout.PropertyField(destroySplashExplosionUplift, new GUIContent("Uplift Power"), GUILayout.MaxWidth(150));

                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 150;
                                            EditorGUILayout.PropertyField(destroySplashExplosionTagLimit, new GUIContent("Only Explode on Tags"));
                                            ResetLabelWidth();

                                            if (destroySplashExplosionTagLimit.boolValue == true) {
                                                InspectorListBox("Only Explode on following tags", destroySplashExplosionAffectTag);
                                            }
                                        }
                                        InspectorHelpBox("Will add an explosive force to rigidbodies in a set radius when the ability is destroyed.");


                                    } else {


                                        EditorGUILayout.HelpBox("Splash Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }




                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region Splash Effects  Settings 

                                    InspectorVerticalBox(true);

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {



                                        EditorGUILayout.PropertyField(useDestroySplashEffect, new GUIContent("Splash Effects"));
                                        if (useDestroySplashEffect.boolValue == true) {
                                            EditorGUILayout.PropertyField(destroySplashRadius, new GUIContent("Radius"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                        }

                                        InspectorHelpBox("Will apply effects to entities in a set radius when the ability is destroyed.");


                                    } else {


                                        EditorGUILayout.HelpBox("Splash Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }




                                    EditorGUILayout.EndVertical();



                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    InspectorSectionHeader("Global Impact Settings");

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();

                                    #region BulletTime on initation Settings 

                                    InspectorVerticalBox(true);

                                    EditorGUIUtility.labelWidth = 210;
                                    EditorGUILayout.PropertyField(modifyGameSpeedOnInitiation, new GUIContent("Modify Game Speed On Initiation"));

                                    EditorGUIUtility.labelWidth = 90;


                                    if (modifyGameSpeedOnInitiation.boolValue == true) {

                                        EditorGUILayout.PropertyField(modifyGameSpeedOnInitiationDelay, new GUIContent("Delay"), GUILayout.MaxWidth(150));

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(modifyGameSpeedOnInitiationSpeedFactor, new GUIContent("Speed Factor"), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(modifyGameSpeedOnInitiationDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();
                                    }



                                    InspectorHelpBox("Will slow down or speed up the game for a set duration when the ability is initiated.", false);

                                    EditorGUILayout.Space();


                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region BulletTime on impact Settings 

                                    InspectorVerticalBox(true);

                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {



                                        EditorGUIUtility.labelWidth = 210;
                                        EditorGUILayout.PropertyField(modifyGameSpeedOnImpact, new GUIContent("Modify Game Speed On Impact"));

                                        EditorGUIUtility.labelWidth = 90;

                                        if (modifyGameSpeedOnImpact.boolValue == true) {
                                            EditorGUILayout.PropertyField(modifyGameSpeedOnImpactDelay, new GUIContent("Delay"), GUILayout.MaxWidth(150));

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(modifyGameSpeedOnImpactSpeedFactor, new GUIContent("Speed Factor"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(modifyGameSpeedOnImpactDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("Will slow down or speed up the game for a set duration when the ability collides with an entity.", false);

                                        EditorGUILayout.Space();



                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }


                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();

                                    #region Camera Shake Impact Settings 

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {



                                        EditorGUIUtility.labelWidth = 160;
                                        EditorGUILayout.PropertyField(shakeCameraOnImpact, new GUIContent("Shake Camera On Impact"));

                                        EditorGUIUtility.labelWidth = 90;

                                        if (shakeCameraOnImpact.boolValue == true) {

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.PropertyField(shakeCameraOnImpactDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(shakeCameraOnImpactDelay, new GUIContent("Delay"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();

                                            EditorGUILayout.EndHorizontal();
                                            ResetLabelWidth();

                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 90;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(shakeCameraOnImpactAmount, new GUIContent("Shake Amount"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(shakeCameraOnImpactSpeed, new GUIContent("Shake Speed"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();

                                            EditorGUILayout.EndHorizontal();

                                            ResetLabelWidth();
                                        }

                                        InspectorHelpBox("Will shake the camera by the amount, speed and duration provided when the ability collides with an entity.");


                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #region Global Impact Tag Required

                                    InspectorVerticalBox(true);



                                    ResetLabelWidth();
                                    InspectorListBox("Tags Required for Global Impacts", globalImpactRequiredTag);

                                    InspectorHelpBox("Global Impact will only apply on tags activating the ability or included in the ability collision", false);



                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();



                                    #region Camera Shake Initiation Settings 

                                    InspectorVerticalBox(true);




                                    EditorGUIUtility.labelWidth = 160;
                                    EditorGUILayout.PropertyField(shakeCameraOnInitiation, new GUIContent("Shake Camera On Initiation"));

                                    EditorGUIUtility.labelWidth = 90;

                                    if (shakeCameraOnInitiation.boolValue == true) {

                                        EditorGUILayout.BeginHorizontal();

                                        EditorGUILayout.PropertyField(shakeCameraOnInitiationDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(shakeCameraOnInitiationDelay, new GUIContent("Delay"), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();

                                        EditorGUILayout.EndHorizontal();
                                        ResetLabelWidth();

                                        EditorGUILayout.Space();

                                        EditorGUIUtility.labelWidth = 90;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(shakeCameraOnInitiationAmount, new GUIContent("Shake Amount"), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(shakeCameraOnInitiationSpeed, new GUIContent("Shake Speed"), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();

                                        EditorGUILayout.EndHorizontal();

                                        ResetLabelWidth();
                                    }

                                    InspectorHelpBox("Will shake the camera by the amount, speed and duration provided when the ability collides with an entity.");



                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion



                                    InspectorSectionHeader("Impact Settings");

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();

                                    #region Hit Stop Settings

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {


                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.PropertyField(enableHitStopOnImpact, new GUIContent("Hit Stop On Impact"));

                                        EditorGUIUtility.labelWidth = 100;
                                        if (enableHitStopOnImpact.boolValue == true) {

                                            EditorGUILayout.PropertyField(hitStopOnImpactDuration, new GUIContent("Duration"), GUILayout.MaxWidth(150));


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(hitStopOnImpactDelay, new GUIContent("Start Delay"));
                                            EditorGUILayout.PropertyField(hitStopOnImpactEntityHitDelay, new GUIContent("Entity Hit Delay"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("If enabled then Ability on impact will after the Start Delay freeze the attack and then additionally after the Entity Hit Delay freeze the object hit for the duration provided.");

                                        ResetLabelWidth();


                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();


                                    #endregion


                                    #region Push Entity Settings

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {


                                        EditorGUIUtility.labelWidth = 140;
                                        EditorGUILayout.PropertyField(pushEntityOnImpact, new GUIContent("Push Entity On Impact"));

                                        EditorGUIUtility.labelWidth = 100;
                                        if (pushEntityOnImpact.boolValue == true) {

                                            EditorGUILayout.PropertyField(pushEntityOnImpactDelay, new GUIContent("Delay"), GUILayout.MaxWidth(150));


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(pushEntityOnImpactAmount, new GUIContent("Push Amount"));
                                            EditorGUILayout.PropertyField(pushEntityOnImpactLiftForce, new GUIContent("Lift Force"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("If enabled then Ability on impact will after the delay push and/or lift the entity");

                                        ResetLabelWidth();


                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();


                                    #endregion




                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();

                                    #region Object Shake Settings 

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {



                                        ResetLabelWidth();
                                        EditorGUILayout.PropertyField(shakeEntityOnImpact, new GUIContent("Shake Entity Hit"));

                                        EditorGUIUtility.labelWidth = 90;

                                        if (shakeEntityOnImpact.boolValue == true) {

                                            EditorGUILayout.PropertyField(shakeEntityOnImpactShakeDelay, new GUIContent("Delay"), GUILayout.MaxWidth(150));


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(shakeEntityOnImpactShakeAmount, new GUIContent("Shake Amount"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.PropertyField(shakeEntityOnImpactShakeDecay, new GUIContent("Shake Decay"), GUILayout.MaxWidth(180));

                                            EditorGUILayout.EndHorizontal();

                                            ResetLabelWidth();
                                        }

                                        InspectorHelpBox("Will shake the entity by the amount provided. The amount of the shake decreases each cycle which determines the duration, once shake decays to 0 the shake will stop.", false);

                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    #region Defy Gravity Settings

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {


                                        EditorGUIUtility.labelWidth = 180;
                                        EditorGUILayout.PropertyField(defyEntityGravityOnImpact, new GUIContent("Entity Defy Gravity On Impact"));

                                        EditorGUIUtility.labelWidth = 80;
                                        if (defyEntityGravityOnImpact.boolValue == true) {


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(defyEntityGravityOnImpactDuration, new GUIContent("Duration"), GUILayout.MaxWidth(240));
                                            EditorGUILayout.PropertyField(defyEntityGravityOnImpactDelay, new GUIContent("Delay"), GUILayout.MaxWidth(240));
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("If enabled then ability on impact will, after the delay, make the entity defy gravity for the duration set");

                                        ResetLabelWidth();


                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion







                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    #region Side By Side

                                    EditorGUILayout.BeginHorizontal();


                                    #region Attach to Object On Impact Settings 

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {


                                        EditorGUIUtility.labelWidth = 200;
                                        EditorGUILayout.PropertyField(attachToObjectOnImpact, new GUIContent("Attach To Object On Impact"));

                                        EditorGUIUtility.labelWidth = 110;
                                        if (attachToObjectOnImpact.boolValue == true) {

                                            EditorGUILayout.PropertyField(attachToObjectProbabilityMinValue, new GUIContent("Probability Min"), GUILayout.MaxWidth(300));
                                            EditorGUILayout.PropertyField(attachToObjectProbabilityMaxValue, new GUIContent("Probability Max"), GUILayout.MaxWidth(300));

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 140;

                                            EditorGUILayout.PropertyField(attachToObjectNearestBone, new GUIContent("Attach to Nearest Bone"));

                                            EditorGUIUtility.labelWidth = 100;

                                            EditorGUILayout.PropertyField(attachToObjectStickOutFactor, new GUIContent("Stick Out Factor"), GUILayout.MaxWidth(160));

                                            EditorGUILayout.EndHorizontal();
                                        }

                                        InspectorHelpBox("If enabled then ability on impact will attach to the object until duration is up");



                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    #region Switch Color Settings 

                                    InspectorVerticalBox(true);


                                    if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {


                                        EditorGUIUtility.labelWidth = 200;
                                        EditorGUILayout.PropertyField(switchColorOnImpact, new GUIContent("Switch Objects Color On Impact"));

                                        EditorGUIUtility.labelWidth = 80;
                                        if (switchColorOnImpact.boolValue == true) {


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(switchColorOnImpactColor, new GUIContent("Color"), GUILayout.MaxWidth(180));
                                            EditorGUIUtility.labelWidth = 130;
                                            EditorGUILayout.PropertyField(switchColorOnImpactUseEmission, new GUIContent("Use Emission Color"));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 80;
                                            EditorGUILayout.PropertyField(switchColorOnImpactDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.PropertyField(switchColorOnImpactDelay, new GUIContent("Delay"), GUILayout.MaxWidth(180));
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("If enabled then ability on impact will change the objects color for a duration before reverting back to its original color");

                                        ResetLabelWidth();


                                    } else {


                                        EditorGUILayout.HelpBox("Impact Settings can not be used on this ability as it's not either a Particle or Melee type.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                    break;



                            }



                            #endregion

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndScrollView();

                            #endregion



                            EditorGUILayout.EndHorizontal();




                            break;
                        case 3:

                            EditorGUILayout.BeginHorizontal();

                            #region Controls/ability info
                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(abilityInfoWidth));


                            if (abilityCont != null)
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref abilityCont.toolbarAbilityManagerAnimationSettingsSelection, ref animationSettingsToolbar);
                            else
                                InspectorToolbarAndAbilityInfo(meAbilityList, ref toolbarAbilityManagerAnimationSettingsSelection, ref animationSettingsToolbar);

                            EditorGUILayout.EndVertical();

                            #endregion

                            EditorGUIUtility.labelWidth = 160;

                            InspectorBoldVerticleLine();

                            #region Aesthetic & Animation 




                            editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                         false,
                                                                         false);

                            EditorGUILayout.BeginVertical();

                            #region Settings

                            switch ((int)ToolbarAbilityManagerAnimationSettingsSelection) {
                                case 0:



                                    #region AllWay 


                                    InspectorVerticalBox();


                                    if (prepareTime.floatValue <= 0) {

                                        EditorGUILayout.HelpBox("Ability is not current set to prepare. To enable this add a prepare time in the General tab.", MessageType.Warning);

                                    } else {
                                        // what to play Aesthetically when iniating ability 
                                        EditorGUILayout.PropertyField(usePreparingAesthetics);
                                        InspectorHelpBox("If disabled then none of the below will come into effect when preparing.", false);
                                    }

                                    EditorGUILayout.EndVertical();



                                    #endregion

                                    if (usePreparingAesthetics.boolValue == true) {

                                        InspectorSectionHeader("Animation & Movement");

                                        #region SideBySide 

                                        EditorGUILayout.BeginHorizontal();

                                        EditorGUILayout.BeginVertical();

                                        #region Preparing Animation Runner 

                                        InspectorVerticalBox(true);


                                        if (prepareTime.floatValue <= 0) {

                                            EditorGUILayout.HelpBox("Ability is not current set to prepare. To enable this add a prepare time in the General tab.", MessageType.Warning);

                                        } else {

                                            EditorGUILayout.Space();

                                            ResetLabelWidth();
                                            EditorGUILayout.PropertyField(preparingAnimationRunnerClip, new GUIContent("Animation Clip"), GUILayout.MaxWidth(315));

                                            if (preparingAnimationRunnerClip.FindPropertyRelative("refVal").objectReferenceValue != null) {
                                                InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");

                                                EditorGUILayout.PropertyField(preparingAnimationRunnerMask, new GUIContent("Avatar Mask"));

                                                EditorGUIUtility.labelWidth = 225;
                                                EditorGUILayout.PropertyField(preparingAnimationRunnerOnEntity, new GUIContent("Animate on Entity"));
                                                EditorGUILayout.PropertyField(preparingAnimationRunnerOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                                EditorGUILayout.PropertyField(preparingAnimationRunnerOnWeapon, new GUIContent("Animate on Weapon"));

                                                InspectorHelpBox("Determines if the animation clip is run on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own animation");

                                                EditorGUIUtility.labelWidth = 75;
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(preparingAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));
                                                EditorGUILayout.PropertyField(preparingAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                            }

                                            ResetLabelWidth();
                                        }

                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #region Preparing Animation 

                                        InspectorVerticalBox(true);


                                        if (prepareTime.floatValue <= 0) {

                                            EditorGUILayout.HelpBox("Ability is not current set to prepare. To enable this add a prepare time in the General tab.", MessageType.Warning);

                                        } else {

                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 145;
                                            EditorGUILayout.PropertyField(preparingAnimatorParameter, new GUIContent("Animator Parameter"), GUILayout.MaxWidth(315));
                                            InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values. Note: Animation will keep repeating until entity is no longer preparing");

                                            if (preparingAnimatorParameter.stringValue != "") {

                                                EditorGUILayout.PropertyField(preparingAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));


                                                EditorGUILayout.Space();

                                                EditorGUIUtility.labelWidth = 225;
                                                EditorGUILayout.PropertyField(preparingAnimateOnEntity, new GUIContent("Animate on Entity"));
                                                EditorGUILayout.PropertyField(preparingAnimateOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                                EditorGUILayout.PropertyField(preparingAnimateOnWeapon, new GUIContent("Animate on Weapon"));

                                                InspectorHelpBox("Determines if the animation command is sent on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own  animator/animation");


                                                EditorGUIUtility.labelWidth = 150;


                                                if (((string)preparingAnimatorParameterType.enumNames[preparingAnimatorParameterType.enumValueIndex]) != "Trigger") {
                                                    //EditorGUILayout.BeginHorizontal();
                                                    // if not trigger we need to know the value to switch on and off
                                                    EditorGUILayout.PropertyField(preparingAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));

                                                    EditorGUILayout.PropertyField(preparingAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));

                                                }

                                            }



                                            EditorGUILayout.Space();

                                        }

                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndVertical();

                                        #region Preparing Movement

                                        InspectorVerticalBox(true);



                                        if (prepareTime.floatValue <= 0) {

                                            EditorGUILayout.HelpBox("Ability is not current set to prepare. To enable this add a prepare time in the General tab.", MessageType.Warning);

                                        } else {



                                            EditorGUIUtility.labelWidth = 210;
                                            EditorGUILayout.PropertyField(stopMovementOnPreparing, new GUIContent("Restrict Movement When Preparing"));
                                            if (stopMovementOnPreparing.boolValue == true) {

                                                ResetLabelWidth();
                                                EditorGUILayout.PropertyField(stopMovementOnPreparingDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));

                                                EditorGUILayout.BeginHorizontal();

                                                EditorGUILayout.PropertyField(stopMovementOnPreparingFreezePosition, new GUIContent("Freeze Position"));
                                                EditorGUIUtility.labelWidth = 140;
                                                EditorGUILayout.PropertyField(stopMovementOnPreparingDisableComponents, new GUIContent("Disable Components"));
                                                EditorGUILayout.EndHorizontal();
                                            }


                                            InspectorHelpBox("If enabled then entity will not be able to move when preparing, if 0 duration then movement will enable after preparation animation ends", false);

                                            EditorGUIUtility.labelWidth = 200;
                                            EditorGUILayout.PropertyField(stopMovementOnPreparingRaiseEvent, new GUIContent("Raise Restrict Movement Event"));

                                            ResetLabelWidth();
                                            if (stopMovementOnPreparingRaiseEvent.boolValue == true) {
                                                EditorGUILayout.PropertyField(stopMovementOnPreparingDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                            }

                                            InspectorHelpBox("If enabled then a restrict movement event delegate will be invoked to let linked scripts know to prevent movement. if 0 duration then the enable movement event will invoke after preparing animation ends");

                                            ResetLabelWidth();


                                            EditorGUIUtility.labelWidth = 190;
                                            EditorGUILayout.PropertyField(moveSelfWhenPreparing);
                                            ResetLabelWidth();

                                            if (moveSelfWhenPreparing.boolValue == true) {

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Move Offset", GUILayout.MaxWidth(100));

                                                EditorGUILayout.PropertyField(moveSelfPreparingOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                                ResetLabelWidth();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(moveSelfPreparingForwardOffset, new GUIContent("Forward Offset"));
                                                EditorGUIUtility.labelWidth = 90;
                                                EditorGUILayout.PropertyField(moveSelfPreparingRightOffset, new GUIContent("Right Offset"));
                                                ResetLabelWidth();
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(moveSelfPreparingDelay, new GUIContent("Start Delay"));
                                                EditorGUIUtility.labelWidth = 90;
                                                EditorGUILayout.PropertyField(moveSelfPreparingDuration, new GUIContent("Duration"));
                                                ResetLabelWidth();
                                                EditorGUILayout.EndHorizontal();

                                            }
                                            InspectorHelpBox("Move entity when preparing. Useful if animation has no rootmotion. Entity will move by an offset given below. ");

                                            EditorGUIUtility.labelWidth = 210;
                                            EditorGUILayout.PropertyField(moveSelfToTargetWhenPreparing, new GUIContent("Move To Target When Preparing"));

                                            if (moveSelfToTargetWhenPreparing.boolValue == true) {

                                                ResetLabelWidth();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));

                                                EditorGUILayout.PropertyField(moveSelfToTargetPreparingOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                                ResetLabelWidth();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(moveSelfToTargetPreparingForwardOffset, new GUIContent("Forward Offset"));
                                                EditorGUIUtility.labelWidth = 90;
                                                EditorGUILayout.PropertyField(moveSelfToTargetPreparingRightOffset, new GUIContent("Right Offset"));
                                                ResetLabelWidth();
                                                EditorGUILayout.EndHorizontal();

                                                EditorGUILayout.Space();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(moveSelfToTargetPreparingDelay, new GUIContent("Start Delay"));
                                                EditorGUIUtility.labelWidth = 90;
                                                EditorGUILayout.PropertyField(moveSelfToTargetPreparingDuration, new GUIContent("Duration"));

                                                EditorGUILayout.EndHorizontal();
                                                EditorGUIUtility.labelWidth = 110;
                                                EditorGUILayout.PropertyField(moveSelfToTargetPreparingStopDistance, new GUIContent("Stop Distance"), GUILayout.MaxWidth(160));
                                                EditorGUIUtility.labelWidth = 230;
                                                EditorGUILayout.PropertyField(moveSelfToTargetActivatePreparingAnimationOnlyWhenMoving, new GUIContent("Only Activate Animation When Moving"));
                                                ResetLabelWidth();
                                            }

                                            InspectorHelpBox("Moves entity to the ability target during preparation. If ability doesn't use targets (Forward Travel Type etc) then entity will move towards the" +
                                                " current target or soft target", false);


                                            EditorGUILayout.Space();

                                            ResetLabelWidth();
                                            EditorGUILayout.PropertyField(defyGravityPreparing, new GUIContent("Defy Gravity"));
                                            if (defyGravityPreparing.boolValue == true) {

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(defyGravityPreparingDuration, new GUIContent("Defy Duration"), GUILayout.MaxWidth(230));
                                                EditorGUILayout.PropertyField(defyGravityPreparingDelay, new GUIContent("Defy Delay"), GUILayout.MaxWidth(230));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.PropertyField(defyGravityPreparingRaiseEvent, new GUIContent("Raise Event"), GUILayout.MaxWidth(170));
                                            }

                                            InspectorHelpBox("If enabled then when preparing the ability the activating entity will defy gravity for a duration set when preparing. Like floating when charging an attack");

                                        }

                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndHorizontal();

                                        #endregion

                                        InspectorSectionHeader("Graphic");

                                        #region AllWay 

                                        #region Preparing Graphic 

                                        InspectorVerticalBox();

                                        InspectorHelpBox("Settings below for the graphic to show when preparing.");

                                        if (prepareTime.floatValue <= 0) {

                                            EditorGUILayout.HelpBox("Ability is not current set to prepare. To enable this add a prepare time in the General tab.", MessageType.Warning);

                                        } else {


                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(preparingParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));
                                            InspectorHorizontalSpace(65);
                                            EditorGUILayout.PropertyField(preparingObject, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 125;
                                            EditorGUILayout.PropertyField(preparingStartPosition, new GUIContent("Graphic Position"), GUILayout.MaxWidth(250));
                                            ResetLabelWidth();
                                            if (((string)preparingStartPosition.enumNames[preparingStartPosition.enumValueIndex]) == "OnObject") {
                                                EditorGUILayout.PropertyField(preparingPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(350));
                                            }

                                            if (((string)preparingStartPosition.enumNames[preparingStartPosition.enumValueIndex]) == "OnTag" || ((string)preparingStartPosition.enumNames[preparingStartPosition.enumValueIndex]) == "OnSelfTag") {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.LabelField("Select Tag");
                                                preparingPositionOnTag.stringValue = EditorGUILayout.TagField(preparingPositionOnTag.stringValue, GUILayout.MaxWidth(200));
                                                EditorGUILayout.Space();
                                            }

                                            EditorGUILayout.EndHorizontal();


                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 190;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(preparingAestheticDurationUsePrepareTime, new GUIContent("Use Prepare Time Duration"), GUILayout.MaxWidth(230));
                                            if (preparingAestheticDurationUsePrepareTime.boolValue == false) {
                                                EditorGUIUtility.labelWidth = 130;
                                                EditorGUILayout.PropertyField(preparingAestheticDuration, new GUIContent("Graphic Duration"), GUILayout.MaxWidth(230));
                                            }
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Graphic Offset", GUILayout.MaxWidth(100));

                                            EditorGUILayout.PropertyField(preparingAestheticsPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(preparingAestheticsPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.PropertyField(preparingAestheticsPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();


                                            EditorGUILayout.Space();

                                        }

                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #endregion

#if ABC_GC_Integration

                                        #region AllWay

                                    InspectorSectionHeader("Game Creator Action On Preparation");

                                    InspectorVerticalBox();


                                    EditorGUIUtility.labelWidth = 150;

                                    SetupGCActionList(ref gcPreparingActionList, ref gcActionListEditor, "Assets/ABC/Scripts/ABC-Resources/Resources/ABC-GCActions/", name.stringValue  + "_Preparing.prefab");
                                    gcActionListEditor.OnInspectorGUI();


                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();


                                        #endregion

#endif

#if ABC_GC_2_Integration

                                        #region AllWay

                                    InspectorSectionHeader("Game Creator 2 Action - On Preparation");

                                    InspectorVerticalBox();


                                    EditorGUIUtility.labelWidth = 150;

                                    SetupGC2Action(ref gc2PreparingAction, "Assets/ABC - Game Creator 2 Integration/Global Elements/ABC-GC2 Actions/", name.stringValue + "_Preparing.prefab");

                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(gc2PreparingAction, new GUIContent(""));
                                    EditorGUILayout.Space();

                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();


                                        #endregion

#endif


                                    }

                                    break;

                                case 1:

                                    #region AllWay 


                                    InspectorVerticalBox();


                                    // what to play Aesthetically when iniating ability 
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 150;
                                    EditorGUILayout.PropertyField(useInitiatingAesthetics);


                                    EditorGUIUtility.labelWidth = 180;

                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.PropertyField(intiatingProjectileDelayType, new GUIContent("After Initiating Activate Ability "), GUILayout.MaxWidth(342));

                                    ResetLabelWidth();

                                    if ((string)intiatingProjectileDelayType.enumNames[intiatingProjectileDelayType.enumValueIndex] == "AfterDelay") {
                                        EditorGUIUtility.labelWidth = 55;
                                        EditorGUILayout.PropertyField(delayBetweenInitiatingAndProjectile, new GUIContent("Delay"), GUILayout.MaxWidth(120));
                                    } else if ((string)intiatingProjectileDelayType.enumNames[intiatingProjectileDelayType.enumValueIndex] == "AtAnimationPercentage") {
                                        EditorGUILayout.PropertyField(initiatingProjectileDelayAnimationPercentage, new GUIContent("Percentage"), GUILayout.MaxWidth(290));
                                    }
                                    EditorGUILayout.EndVertical();
                                    ResetLabelWidth();
                                    EditorGUILayout.EndHorizontal();
                                    InspectorHelpBox("After initiating the ability/raycast will appear in game after a delay or at a defined initiating animation percentage (0-100%)", false);

                                    EditorGUILayout.EndVertical();


                                    #endregion

                                    if (useInitiatingAesthetics.boolValue == true) {

                                        InspectorSectionHeader("Animation & Movement");

                                        #region SideBySide 

                                        EditorGUILayout.BeginHorizontal();

                                        EditorGUILayout.BeginVertical();

                                        #region Initiating Animation Runner 

                                        InspectorVerticalBox(true);

                                        EditorGUILayout.Space();

                                        EditorGUILayout.PropertyField(initiatingAnimationRunnerClip, new GUIContent("Animation Clip"), GUILayout.MaxWidth(315));

                                        if (initiatingAnimationRunnerClip.FindPropertyRelative("refVal").objectReferenceValue != null) {

                                            if (this.Abilities[currentAbilityIndex].initiatingAnimationRunnerClip.AnimationClip != null) {
                                                InspectorSmallText("Clip Duration: " + this.Abilities[currentAbilityIndex].initiatingAnimationRunnerClip.AnimationClip.length);
                                            }

                                            InspectorHelpBox("The clip selected is played using the ABC animation runner and does not use Unity's Animator");

                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerMask, new GUIContent("Avatar Mask"));

                                            EditorGUIUtility.labelWidth = 225;
                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerOnEntity, new GUIContent("Animate on Entity"));
                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerOnWeapon, new GUIContent("Animate on Weapon"));

                                            InspectorHelpBox("Determines if the animation clip is run on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own animation");

                                            EditorGUIUtility.labelWidth = 75;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerClipDuration, new GUIContent("Duration"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.PropertyField(initiatingAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));


                                        }

                                        ResetLabelWidth();
                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #region Initiating Animation 

                                        InspectorVerticalBox(true);

                                        EditorGUILayout.Space();

                                        EditorGUIUtility.labelWidth = 145;
                                        EditorGUILayout.PropertyField(initiatingAnimatorParameter, new GUIContent("Animator Parameter"), GUILayout.MaxWidth(315));

                                        if (initiatingAnimatorParameter.stringValue != "") {

                                            InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values. Note: Animation will keep repeating until entity is no longer initiating");


                                            EditorGUILayout.PropertyField(initiatingAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));

                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 225;
                                            EditorGUILayout.PropertyField(initiatingAnimateOnEntity, new GUIContent("Animate on Entity"));
                                            EditorGUILayout.PropertyField(initiatingAnimateOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                            EditorGUILayout.PropertyField(initiatingAnimateOnWeapon, new GUIContent("Animate on Weapon"));

                                            InspectorHelpBox("Determines if the animation command is sent on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own  animator/animation");

                                            EditorGUIUtility.labelWidth = 150;


                                            if ((string)abilityToggle.enumNames[abilityToggle.enumValueIndex] == "Off" || (string)abilityToggle.enumNames[abilityToggle.enumValueIndex] != "Off" && (repeatInitiatingAnimationWhilstToggled.boolValue == false || canCastWhenToggled.boolValue == true)) {
                                                EditorGUILayout.PropertyField(initiatingAnimatorDuration, new GUIContent("Animation Duration"), GUILayout.MaxWidth(230));
                                            }

                                            if (((string)initiatingAnimatorParameterType.enumNames[initiatingAnimatorParameterType.enumValueIndex]) != "Trigger") {
                                                //EditorGUILayout.BeginHorizontal();
                                                // if not trigger we need to know the value to switch on and off
                                                EditorGUILayout.PropertyField(initiatingAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));

                                                EditorGUILayout.PropertyField(initiatingAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));

                                            }
                                        }


                                        ResetLabelWidth();
                                        EditorGUILayout.Space();



                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndVertical();

                                        #region initiating Movement

                                        InspectorVerticalBox(true);



                                        EditorGUIUtility.labelWidth = 230;
                                        EditorGUILayout.PropertyField(stopMovementOnInitiate, new GUIContent("Restrict Movement When Initiating"));
                                        if (stopMovementOnInitiate.boolValue == true) {
                                            ResetLabelWidth();
                                            EditorGUILayout.PropertyField(stopMovementOnInitiateDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.PropertyField(stopMovementOnInitiateFreezePosition, new GUIContent("Freeze Position"));
                                            EditorGUIUtility.labelWidth = 140;
                                            EditorGUILayout.PropertyField(stopMovementOnInitiateDisableComponents, new GUIContent("Disable Components"));
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("If enabled then entity will not be able to move when initiating. if 0 duration then movement will enable after initiation animation ends", false);

                                        EditorGUIUtility.labelWidth = 230;
                                        EditorGUILayout.PropertyField(stopMovementOnInitiateRaiseEvent, new GUIContent("Raise Restrict Movement Event"));

                                        ResetLabelWidth();
                                        if (stopMovementOnInitiateRaiseEvent.boolValue == true) {
                                            EditorGUILayout.PropertyField(stopMovementOnInitiateDuration, new GUIContent("Duration"), GUILayout.MaxWidth(180));
                                        }
                                        InspectorHelpBox("If enabled then a restrict movement event delegate will be invoked to let linked scripts know to prevent movement. if 0 duration then the enable movement event will invoke after initiation animation ends");

                                        ResetLabelWidth();



                                        EditorGUIUtility.labelWidth = 200;
                                        EditorGUILayout.PropertyField(moveSelfWhenInitiating);
                                        ResetLabelWidth();

                                        if (moveSelfWhenInitiating.boolValue == true) {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Move Offset", GUILayout.MaxWidth(100));

                                            EditorGUILayout.PropertyField(moveSelfInitiatingOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                            ResetLabelWidth();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(moveSelfInitiatingForwardOffset, new GUIContent("Forward Offset"));
                                            EditorGUIUtility.labelWidth = 90;
                                            EditorGUILayout.PropertyField(moveSelfInitiatingRightOffset, new GUIContent("Right Offset"));
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(moveSelfInitiatingDelay, new GUIContent("Start Delay"));
                                            EditorGUIUtility.labelWidth = 90;
                                            EditorGUILayout.PropertyField(moveSelfInitiatingDuration, new GUIContent("Duration"));
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();

                                        }

                                        InspectorHelpBox("Move entity when initiating. Useful if animation has no rootmotion. Entity will move by an offset given below. ");


                                        EditorGUIUtility.labelWidth = 230;
                                        EditorGUILayout.PropertyField(moveSelfToTargetWhenInitiating, new GUIContent("Move To Target When Initiating"));

                                        if (moveSelfToTargetWhenInitiating.boolValue == true) {

                                            ResetLabelWidth();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Target Offset", GUILayout.MaxWidth(100));

                                            EditorGUILayout.PropertyField(moveSelfToTargetInitiatingOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                            ResetLabelWidth();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(moveSelfToTargetInitiatingForwardOffset, new GUIContent("Forward Offset"));
                                            EditorGUIUtility.labelWidth = 90;
                                            EditorGUILayout.PropertyField(moveSelfToTargetInitiatingRightOffset, new GUIContent("Right Offset"));
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(moveSelfToTargetInitiatingDelay, new GUIContent("Start Delay"));
                                            EditorGUIUtility.labelWidth = 90;
                                            EditorGUILayout.PropertyField(moveSelfToTargetInitiatingDuration, new GUIContent("Duration"));

                                            EditorGUILayout.EndHorizontal();
                                            EditorGUIUtility.labelWidth = 110;
                                            EditorGUILayout.PropertyField(moveSelfToTargetInitiatingStopDistance, new GUIContent("Stop Distance"), GUILayout.MaxWidth(160));
                                            ResetLabelWidth();
                                        }

                                        InspectorHelpBox("Moves entity to the ability target during initiation. If ability doesn't use targets (Forward Travel Type etc) then entity will move towards the" +
                                            " current target or soft target", false);

                                        EditorGUILayout.Space();

                                        ResetLabelWidth();
                                        EditorGUILayout.PropertyField(defyGravityInitiating, new GUIContent("Defy Gravity"));
                                        if (defyGravityInitiating.boolValue == true) {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(defyGravityInitiatingDuration, new GUIContent("Defy Duration"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.PropertyField(defyGravityInitiatingDelay, new GUIContent("Defy Delay"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.PropertyField(defyGravityInitiatingRaiseEvent, new GUIContent("Raise Event"), GUILayout.MaxWidth(170));



                                        }

                                        InspectorHelpBox("If enabled then when initiating the ability the activating entity will defy gravity for a duration set when preparing. Like floating when charging an attack. Requires Rigidbody.");



                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndHorizontal();

                                        #endregion

                                        InspectorSectionHeader("Graphic");

                                        #region SideBySide 

                                        EditorGUILayout.BeginHorizontal();

                                        #region Initiating Graphic 

                                        InspectorVerticalBox(true);

                                        InspectorHelpBox("Settings below for the graphic to show when initiating.");


                                        EditorGUILayout.PropertyField(initiatingParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));
                                        EditorGUILayout.PropertyField(initiatingObject, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));
                                        EditorGUIUtility.labelWidth = 230;
                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                            EditorGUILayout.PropertyField(initiatingAestheticActivateWithAbility, new GUIContent("Activate Graphic When Ability Initiates"));
                                        }

                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 125;
                                        EditorGUILayout.PropertyField(initiatingStartPosition, new GUIContent("Graphic Position"), GUILayout.MaxWidth(250));
                                        ResetLabelWidth();
                                        if (((string)initiatingStartPosition.enumNames[initiatingStartPosition.enumValueIndex]) == "OnObject") {
                                            EditorGUILayout.PropertyField(initiatingPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(350));
                                        }
                                        if (((string)initiatingStartPosition.enumNames[initiatingStartPosition.enumValueIndex]) == "OnTag" || ((string)initiatingStartPosition.enumNames[initiatingStartPosition.enumValueIndex]) == "OnSelfTag") {
                                            EditorGUILayout.LabelField("Select Tag");
                                            initiatingPositionOnTag.stringValue = EditorGUILayout.TagField(initiatingPositionOnTag.stringValue, GUILayout.MaxWidth(200));
                                            EditorGUILayout.Space();
                                        }



                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) != "Melee" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee" && initiatingAestheticActivateWithAbility.boolValue == false) {
                                            EditorGUIUtility.labelWidth = 105;

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(initiatingAestheticDuration, new GUIContent("Graphic Duration"), GUILayout.MaxWidth(200));
                                            EditorGUILayout.PropertyField(initiatingAestheticDelay, new GUIContent("Graphic Delay"), GUILayout.MaxWidth(200));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 200;
                                            EditorGUILayout.PropertyField(initiatingAestheticDetachFromParentAfterDelay, new GUIContent("Detach From Parent After Delay"), GUILayout.MaxWidth(230));
                                            EditorGUIUtility.labelWidth = 85;
                                            if (initiatingAestheticDetachFromParentAfterDelay.boolValue == true) {
                                                EditorGUILayout.PropertyField(initiatingAestheticDetachDelay, new GUIContent("Detach Delay"), GUILayout.MaxWidth(200));
                                            }
                                        }


                                        ResetLabelWidth();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("Graphic Offset", GUILayout.MaxWidth(100));

                                        EditorGUILayout.PropertyField(initiatingAestheticsPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(initiatingAestheticsPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(230));
                                        EditorGUILayout.PropertyField(initiatingAestheticsPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(230));
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.Space();





                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #region Weapon Trail

                                        InspectorVerticalBox(true);


                                        if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {

                                            InspectorHelpBox("If enabled then the weapon trail setup on the current equipped weapon will activate when the ability initiates.");

                                            EditorGUIUtility.labelWidth = 170;
                                            EditorGUILayout.PropertyField(initiatingUseWeaponTrail, new GUIContent("Use Equipped Weapon Trail"), GUILayout.MaxWidth(350));

                                            if (initiatingUseWeaponTrail.boolValue == true) {

                                                EditorGUILayout.PropertyField(initiatingWeaponTrailGraphicIteration, new GUIContent("Weapon Graphic Iteration"), GUILayout.MaxWidth(230));
                                                InspectorHelpBox("Determines which weapon graphic to activate the weapon trail on, 0 being the first weapon graphic in the equipped weapon. If a number higher then that setup is listed, it will instead activate on the first graphic as a default");
                                            }

                                            ResetLabelWidth();

                                        } else {
                                            EditorGUILayout.HelpBox("Weapon Trail setting are only available for Melee type Abilities.", MessageType.Warning);
                                        }







                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndHorizontal();

                                        #endregion

#if ABC_GC_Integration

                                        #region AllWay

                                    InspectorSectionHeader("Game Creator Action On Initiation");

                                    InspectorVerticalBox();


                                    EditorGUIUtility.labelWidth = 150;

                                    SetupGCActionList(ref gcInitiatingActionList, ref gcActionListEditor, "Assets/ABC/Scripts/ABC-Resources/Resources/ABC-GCActions/", name.stringValue + "_Initiation.prefab");
                                    gcActionListEditor.OnInspectorGUI();


                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();


                                        #endregion

#endif


#if ABC_GC_2_Integration

                                        #region AllWay

                                    InspectorSectionHeader("Game Creator 2 Action - On Initiation");

                                    InspectorVerticalBox();


                                    EditorGUIUtility.labelWidth = 150;

                                    SetupGC2Action(ref gc2InitiatingAction, "Assets/ABC - Game Creator 2 Integration/Global Elements/ABC-GC2 Actions/", name.stringValue + "_Initiation.prefab");

                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(gc2InitiatingAction, new GUIContent(""));
                                    EditorGUILayout.Space();



                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();


                                        #endregion

#endif
                                    }



                                    break;

                                case 2:

                                    #region AllWay 


                                    InspectorVerticalBox();

                                    // what to play Aesthetically when iniating ability 
                                    EditorGUILayout.PropertyField(useAbilityEndAesthetics);
                                    InspectorHelpBox("Graphics which will display when the ability ends e.g is destroyed or duration is up.");

                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndVertical();


                                    #endregion

                                    InspectorSectionHeader("Graphic");

                                    #region AllWay 

                                    if (useAbilityEndAesthetics.boolValue == true) {

                                        #region Ability End Graphic 

                                        InspectorVerticalBox();

                                        EditorGUIUtility.labelWidth = 120;


                                        EditorGUILayout.PropertyField(abilityEndUseEffectGraphic, new GUIContent("Use Effect Graphic"), GUILayout.MaxWidth(350));
                                        InspectorHelpBox("If ticked then the end graphic will play a graphic setup in the abilities effects. Prioritising the first Adjust Health graphic found.");


                                        if (abilityEndUseEffectGraphic.boolValue == false) {
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(abEndParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));
                                            InspectorHorizontalSpace(65);
                                            EditorGUILayout.PropertyField(abEndObject, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.PropertyField(scaleAbilityEndGraphic, new GUIContent("Scale Graphic"));
                                            if (scaleAbilityEndGraphic.boolValue == true) {
                                                EditorGUIUtility.labelWidth = 70;
                                                EditorGUILayout.PropertyField(abilityEndGraphicScale, new GUIContent("Scale"), GUILayout.MaxWidth(150));
                                            }
                                            EditorGUILayout.Space();
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                        }


                                        EditorGUILayout.PropertyField(abEndAestheticDuration, new GUIContent("Graphic Duration"), GUILayout.MaxWidth(180));

                                        EditorGUILayout.Space();

                                        EditorGUIUtility.labelWidth = 180;

                                        EditorGUILayout.PropertyField(abilityEndActivateOnEnvironmentOnly, new GUIContent("Activate on Environment Only"), GUILayout.MaxWidth(350));
                                        InspectorHelpBox("If ticked then the end graphic will only activate on non ABC objects (objects without ABC StateManager Component) like the environment.");

                                        ResetLabelWidth();


                                        EditorGUILayout.EndVertical();

                                        #endregion
                                    }

                                    #endregion

                                    break;


                                case 3:


                                    #region AllWay 


                                    InspectorVerticalBox();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 160;
                                    EditorGUILayout.PropertyField(useProjectileToStartPosition, new GUIContent("Enable Additional Graphic"));
                                    EditorGUIUtility.labelWidth = 140;
                                    if (useProjectileToStartPosition.boolValue == true) {
                                        EditorGUILayout.PropertyField(projectileToStartType, new GUIContent("Show Graphic at Ability"), GUILayout.MaxWidth(240));
                                        ResetLabelWidth();
                                        EditorGUILayout.LabelField(" Stage");
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    if (useProjectileToStartPosition.boolValue == true) {
                                        EditorGUILayout.PropertyField(projToStartDelay, new GUIContent("Graphic Delay"), GUILayout.MaxWidth(180));
                                    }
                                    ResetLabelWidth();
                                    InspectorHelpBox("If enabled then an additional graphic will appear during either the preparing or intiating stage.", false);


                                    EditorGUILayout.EndVertical();


                                    #endregion

                                    if (useProjectileToStartPosition.boolValue == true) {

                                        if (projectileToStartType.intValue == 0 && prepareTime.floatValue <= 0) {
                                            InspectorVerticalBox();
                                            EditorGUILayout.HelpBox("Ability is not current set to prepare. To enable this functionality add a prepare time in the General tab or switch type to Initiating.", MessageType.Warning);
                                            EditorGUILayout.EndVertical();
                                        } else {


                                            InspectorSectionHeader("Graphic");

                                            #region AllWay 

                                            #region  Graphic 

                                            InspectorVerticalBox();


                                            EditorGUIUtility.labelWidth = 150;

                                            if (((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Projectile" || ((string)abilityType.enumNames[abilityType.enumValueIndex]) == "Melee") {
                                                EditorGUILayout.PropertyField(useOriginalProjectilePTS, new GUIContent("Use Main Graphic"));
                                                InspectorHelpBox("If selected then the main ability graphic set for this ability will be created earlier and used.");
                                            }
                                            ResetLabelWidth();
                                            if (useOriginalProjectilePTS.boolValue == false) {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(projToStartPosParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));
                                                InspectorHorizontalSpace(65);
                                                EditorGUILayout.PropertyField(projToStartPosObject, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUIUtility.labelWidth = 110;
                                                EditorGUILayout.PropertyField(projToStartPosDuration, new GUIContent("Graphic Duration"), GUILayout.MaxWidth(210));
                                                ResetLabelWidth();
                                                InspectorHelpBox("If graphic duration is 0 then the graphic will be destroyed when the ability initiates. Else it will be destroyed after the duration");
                                            }



                                            EditorGUILayout.EndVertical();

                                            #endregion

                                            #endregion

                                            InspectorSectionHeader("Graphic Starting Position");

                                            #region AllWay 



                                            InspectorVerticalBox();



                                            InspectorHelpBox("Setup where the additional graphic will be created and position the preparation/initiation stage.");



                                            EditorGUIUtility.labelWidth = 150;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(projToStartStartingPosition, new GUIContent("Graphic Start Position"), GUILayout.MaxWidth(260));

                                            ResetLabelWidth();

                                            if (((string)projToStartStartingPosition.enumNames[projToStartStartingPosition.enumValueIndex]) == "OnObject") {
                                                EditorGUILayout.PropertyField(projToStartPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(300));
                                            }

                                            if (((string)projToStartStartingPosition.enumNames[projToStartStartingPosition.enumValueIndex]) == "OnTag" || ((string)projToStartStartingPosition.enumNames[projToStartStartingPosition.enumValueIndex]) == "OnSelfTag") {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.LabelField("Select Tag");
                                                projToStartPositionOnTag.stringValue = EditorGUILayout.TagField(projToStartPositionOnTag.stringValue, GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();

                                            }
                                            EditorGUILayout.Space();
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();




                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Position Offset", GUILayout.MaxWidth(100));

                                            EditorGUILayout.PropertyField(projToStartPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(projToStartPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.PropertyField(projToStartPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                            EditorGUILayout.PropertyField(projToStartRotation, new GUIContent("Starting Rotation"));
                                            EditorGUILayout.Space();
                                            EditorGUIUtility.labelWidth = 130;
                                            EditorGUILayout.PropertyField(projToStartSetEulerRotation, new GUIContent("Set Euler Rotation"));
                                            ResetLabelWidth();





                                            EditorGUILayout.EndVertical();


                                            #endregion

                                            InspectorSectionHeader("Travel & Misc");
                                            #region AllWay 


                                            InspectorVerticalBox();

                                            EditorGUIUtility.labelWidth = 180;

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(projToStartTravelToAbilityStartPosition, new GUIContent("Travel to Ability Start Position"), GUILayout.MaxWidth(230));

                                            if (projToStartTravelToAbilityStartPosition.boolValue == true) {
                                                EditorGUILayout.PropertyField(projToStartReachPositionTime, new GUIContent("Seconds To Starting Position"), GUILayout.MaxWidth(230));
                                            }
                                            EditorGUILayout.EndHorizontal();

                                            if (projToStartTravelToAbilityStartPosition.boolValue == true) {
                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUIUtility.labelWidth = 90;
                                                EditorGUILayout.PropertyField(projToStartTravelDelay, new GUIContent("Travel Delay"), GUILayout.MaxWidth(140));
                                                EditorGUIUtility.labelWidth = 160;
                                                EditorGUILayout.PropertyField(projToStartRotateToTarget, new GUIContent("Rotate To Starting Position"));
                                                EditorGUILayout.EndHorizontal();
                                            }

                                            InspectorHelpBox("If enabled then the graphic will travel to the Ability starting position. Example of this implementation could be receiving lightning from the sky before sending it onwards.");

                                            ResetLabelWidth();
                                            EditorGUILayout.PropertyField(projToStartmoveWithTarget, new GUIContent("Attach to Object"));
                                            InspectorHelpBox("Will attach the graphic to either the graphics starting position object or the ability start position object once reached (if set to travel to ability starting position).");

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.PropertyField(projToStartHoverOnSpot, new GUIContent("Hover On Spot"));

                                            if (projToStartHoverOnSpot.boolValue == true) {
                                                EditorGUILayout.PropertyField(projToStartHoverDistance, new GUIContent("Hover Distance"), GUILayout.MaxWidth(230));
                                                EditorGUILayout.Space();
                                            }
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            ResetLabelWidth();
                                            EditorGUILayout.Space();

                                            EditorGUILayout.EndVertical();


                                            #endregion


                                        }
                                    }

                                    break;

                                case 4:

                                    #region AllWay 


                                    InspectorVerticalBox();


                                    if (scrollAbility.boolValue == false) {

                                        EditorGUILayout.HelpBox("Ability is not currently scrollable. To enable this tick the Scroll Ability box in the General tab.", MessageType.Warning);

                                    } else {

                                        EditorGUIUtility.labelWidth = 190;
                                        // what to play Aesthetically when iniating ability 
                                        EditorGUILayout.PropertyField(useScrollAbilityAesthetics);
                                        ResetLabelWidth();
                                        InspectorHelpBox("If disabled then none of the below will come into effect when activating/deactivating a scrollable ability.", false);
                                    }

                                    EditorGUILayout.EndVertical();


                                    #endregion

                                    if (useScrollAbilityAesthetics.boolValue == true) {

                                        InspectorSectionHeader("Animation");

                                        InspectorHelpBox("Settings below for when entity enables (equips) a scroll ability", false, true);

                                        #region SideBySide 

                                        EditorGUILayout.BeginHorizontal();

                                        #region Scrollable Ability Activate Animation Runner

                                        InspectorVerticalBox(true);


                                        EditorGUIUtility.labelWidth = 155;
                                        EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerClip, new GUIContent("Enable Animation Clip"), GUILayout.MaxWidth(335));

                                        if (scrollAbilityAnimationRunnerClip.FindPropertyRelative("refVal").objectReferenceValue != null) {
                                            InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");

                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerMask, new GUIContent("Avatar Mask"));

                                            EditorGUIUtility.labelWidth = 225;
                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerOnEntity, new GUIContent("Animate on Entity"));
                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerOnWeapon, new GUIContent("Animate on Weapon"));

                                            InspectorHelpBox("Determines if the animation clip is run on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own animation");

                                            EditorGUIUtility.labelWidth = 75;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerClipDuration, new GUIContent("Duration"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.PropertyField(scrollAbilityAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));


                                        }

                                        ResetLabelWidth();
                                        EditorGUILayout.EndVertical();



                                        #endregion

                                        #region Scrollable Ability Activate Animation 

                                        InspectorVerticalBox(true);

                                        if (scrollAbility.boolValue == false) {

                                            EditorGUILayout.HelpBox("Ability is not currently scrollable. To enable this tick the Scroll Ability box in the General tab.", MessageType.Warning);

                                        } else {

                                            EditorGUIUtility.labelWidth = 200;
                                            EditorGUILayout.PropertyField(scrollAbilityAnimatorParameter, new GUIContent("Enable Animator Parameter"), GUILayout.Width(300));
                                            ResetLabelWidth();
                                            if (scrollAbilityAnimatorParameter.stringValue != "") {

                                                InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values.");

                                                EditorGUILayout.PropertyField(scrollAbilityAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();


                                                EditorGUIUtility.labelWidth = 225;

                                                EditorGUILayout.PropertyField(scrollAbilityAnimateOnEntity, new GUIContent("Animate on Entity"));
                                                EditorGUILayout.PropertyField(scrollAbilityAnimateOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                                EditorGUILayout.PropertyField(scrollAbilityAnimateOnWeapon, new GUIContent("Animate on Weapon"));

                                                InspectorHelpBox("Determines if the animation command is sent on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own  animator/animation");


                                                EditorGUIUtility.labelWidth = 150;

                                                EditorGUILayout.PropertyField(scrollAbilityAnimatorDuration, new GUIContent("Animation Duration"), GUILayout.MaxWidth(230));

                                                if (((string)scrollAbilityAnimatorParameterType.enumNames[scrollAbilityAnimatorParameterType.enumValueIndex]) != "Trigger") {


                                                    // if not trigger we need to know the value to switch on and off
                                                    EditorGUILayout.PropertyField(scrollAbilityAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.PropertyField(scrollAbilityAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.Space();



                                                    ResetLabelWidth();
                                                    EditorGUILayout.Space();



                                                }
                                            }

                                        }


                                        EditorGUILayout.EndVertical();

                                        #endregion


                                        EditorGUILayout.EndHorizontal();
                                        #endregion

                                        InspectorHelpBox("Settings below for when entity disables (unequips) a scroll ability", false, true);

                                        #region SideBySide 

                                        EditorGUILayout.BeginHorizontal();

                                        #region Scrollable Ability Activate Animation Runner

                                        InspectorVerticalBox(true);


                                        EditorGUIUtility.labelWidth = 155;
                                        EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerClip, new GUIContent("Disable Animation Clip"), GUILayout.MaxWidth(335));

                                        if (scrollAbilityDeactivateAnimationRunnerClip.FindPropertyRelative("refVal").objectReferenceValue != null) {
                                            InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");

                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerMask, new GUIContent("Avatar Mask"));

                                            EditorGUIUtility.labelWidth = 225;
                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerOnEntity, new GUIContent("Animate on Entity"));
                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerOnWeapon, new GUIContent("Animate on Weapon"));

                                            InspectorHelpBox("Determines if the animation clip is run on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own animation");

                                            EditorGUIUtility.labelWidth = 75;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerClipDuration, new GUIContent("Duration"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));


                                        }

                                        ResetLabelWidth();
                                        EditorGUILayout.EndVertical();



                                        #endregion

                                        #region Scrollable Ability deactivate Animation 

                                        InspectorVerticalBox(true);

                                        if (scrollAbility.boolValue == false) {

                                            EditorGUILayout.HelpBox("Ability is not currently scrollable. To enable this tick the Scroll Ability box in the General tab.", MessageType.Warning);

                                        } else {
                                            EditorGUIUtility.labelWidth = 200;

                                            EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimatorParameter, new GUIContent("Disable Animator Parameter"), GUILayout.Width(300));
                                            ResetLabelWidth();
                                            if (scrollAbilityDeactivateAnimatorParameter.stringValue != "") {

                                                InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values.");

                                                EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();

                                                EditorGUIUtility.labelWidth = 225;

                                                EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimateOnEntity, new GUIContent("Animate on Entity"));
                                                EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimateOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                                EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimateOnWeapon, new GUIContent("Animate on Weapon"));

                                                InspectorHelpBox("Determines if the animation command is sent on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own  animator/animation");

                                                EditorGUIUtility.labelWidth = 150;

                                                EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimatorDuration, new GUIContent("Animation Duration"), GUILayout.MaxWidth(230));

                                                if (((string)scrollAbilityDeactivateAnimatorParameterType.enumNames[scrollAbilityDeactivateAnimatorParameterType.enumValueIndex]) != "Trigger") {


                                                    // if not trigger we need to know the value to switch on and off
                                                    EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.PropertyField(scrollAbilityDeactivateAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.Space();



                                                    ResetLabelWidth();


                                                }
                                            }

                                        }


                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        EditorGUILayout.EndHorizontal();
                                        #endregion

                                        InspectorSectionHeader("Graphic");

                                        #region AllWay 

                                        #region Scrollable Ability Activate Graphic 

                                        InspectorVerticalBox();

                                        if (scrollAbility.boolValue == false) {

                                            EditorGUILayout.HelpBox("Ability is not currently scrollable. To enable this tick the Scroll Ability box in the General tab.", MessageType.Warning);

                                        } else {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(scrollAbilityParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));
                                            InspectorHorizontalSpace(65);
                                            EditorGUILayout.PropertyField(scrollAbilityObject, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 125;
                                            EditorGUILayout.PropertyField(scrollAbilityStartPosition, new GUIContent("Graphic Position"), GUILayout.MaxWidth(250));
                                            ResetLabelWidth();
                                            if (((string)scrollAbilityStartPosition.enumNames[scrollAbilityStartPosition.enumValueIndex]) == "OnObject") {
                                                EditorGUILayout.PropertyField(scrollAbilityPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(350));
                                            }

                                            if (((string)scrollAbilityStartPosition.enumNames[scrollAbilityStartPosition.enumValueIndex]) == "OnTag" || ((string)scrollAbilityStartPosition.enumNames[scrollAbilityStartPosition.enumValueIndex]) == "OnSelfTag") {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.LabelField("Select Tag");
                                                scrollAbilityPositionOnTag.stringValue = EditorGUILayout.TagField(scrollAbilityPositionOnTag.stringValue, GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();

                                            }
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(scrollAbilityGraphicActivateDelay, new GUIContent("Graphic Delay"), GUILayout.MaxWidth(250));

                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            ResetLabelWidth();
                                            EditorGUILayout.PropertyField(scrollAbilityAestheticUseDuration, new GUIContent("Duration Type"), GUILayout.MaxWidth(250));
                                            ResetLabelWidth();
                                            if (((string)scrollAbilityAestheticUseDuration.enumNames[scrollAbilityAestheticUseDuration.enumValueIndex]) == "Duration") {
                                                EditorGUILayout.Space();
                                                EditorGUIUtility.labelWidth = 125;
                                                EditorGUILayout.PropertyField(scrollAbilityAestheticDuration, new GUIContent("Graphic Duration"), GUILayout.MaxWidth(230));
                                                ResetLabelWidth();
                                                EditorGUILayout.Space();
                                                EditorGUILayout.EndHorizontal();
                                                InspectorHelpBox("If no duration is set then graphic will have an infinite duration until ability is switched over");

                                            } else {

                                                EditorGUILayout.PropertyField(scrollAbilityGraphicDeactivateDelay, new GUIContent("Inactive Delay"), GUILayout.MaxWidth(332));

                                                EditorGUILayout.EndHorizontal();

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PropertyField(scrollAbilityPersistantAestheticInactivePosition, new GUIContent("Inactive Position"), GUILayout.MaxWidth(250));

                                                if (((string)scrollAbilityPersistantAestheticInactivePosition.enumNames[scrollAbilityPersistantAestheticInactivePosition.enumValueIndex]) == "OnObject") {
                                                    EditorGUILayout.PropertyField(scrollAbilityPersistantAestheticInactivePositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(350));
                                                }

                                                if (((string)scrollAbilityPersistantAestheticInactivePosition.enumNames[scrollAbilityPersistantAestheticInactivePosition.enumValueIndex]) == "OnTag" || ((string)scrollAbilityPersistantAestheticInactivePosition.enumNames[scrollAbilityPersistantAestheticInactivePosition.enumValueIndex]) == "OnSelfTag") {
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.LabelField("Select Tag");
                                                    scrollAbilityPositionOnTag.stringValue = EditorGUILayout.TagField(scrollAbilityPersistantAestheticInactivePositionOnTag.stringValue, GUILayout.MaxWidth(250));
                                                    EditorGUILayout.Space();

                                                }
                                                EditorGUILayout.EndHorizontal();


                                                InspectorHelpBox("Graphic is persistant so will always show as long as ability is enabled. Inactive position is where the graphic will be placed when the ability is not 'equipped'", false);

                                            }


                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Graphic Offset", GUILayout.MaxWidth(100));

                                            EditorGUILayout.PropertyField(scrollAbilityAestheticsPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(scrollAbilityAestheticsPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.PropertyField(scrollAbilityAestheticsPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                        }


                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #endregion
                                    }



                                    break;


                                case 5:

                                    #region AllWay 


                                    InspectorVerticalBox();

                                    if (scrollAbility.boolValue == true && useReload.boolValue == true && UseAmmo.boolValue == true && useEquippedWeaponAmmo.boolValue == false) {

                                        EditorGUIUtility.labelWidth = 210;
                                        EditorGUILayout.PropertyField(useReloadAbilityAesthetics);
                                        ResetLabelWidth();
                                        InspectorHelpBox("If disabled then none of the below will come into effect when reloading a scrollable ability.");


                                    } else {

                                        EditorGUILayout.HelpBox("Ability is not currently setup to reload. To enable reloading please make sure ability is scrollable, not using equipped weapon ammo and both 'Use Reload' and 'Use Ammo' are ticked in the General tab.", MessageType.Warning);

                                    }

                                    EditorGUILayout.EndVertical();


                                    #endregion

                                    if (useReloadAbilityAesthetics.boolValue == true && scrollAbility.boolValue == true && useReload.boolValue == true && UseAmmo.boolValue == true && useEquippedWeaponAmmo.boolValue == false) {

                                        InspectorSectionHeader("Animation");



                                        #region AllWay 

                                        #region Reloading Animation Runner 

                                        InspectorVerticalBox();

                                        InspectorHelpBox("Settings below for when entity reloads a scrollable ability", false);

                                        EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerClip, new GUIContent("Animation Clip"), GUILayout.MaxWidth(315));

                                        if (reloadAbilityAnimationRunnerClip.FindPropertyRelative("refVal").objectReferenceValue != null) {
                                            InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");

                                            EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerMask, new GUIContent("Avatar Mask"), GUILayout.MaxWidth(315));

                                            EditorGUIUtility.labelWidth = 225;
                                            EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerOnEntity, new GUIContent("Animate on Entity"));
                                            EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                            EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerOnWeapon, new GUIContent("Animate on Weapon"));

                                            InspectorHelpBox("Determines if the animation clip is run on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own animation");

                                            EditorGUIUtility.labelWidth = 75;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.PropertyField(reloadAbilityAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.EndHorizontal();




                                        }

                                        ResetLabelWidth();
                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #endregion

                                        #region AllWay 

                                        #region Reload Animation 

                                        InspectorVerticalBox();



                                        EditorGUIUtility.labelWidth = 150;

                                        if (scrollAbility.boolValue == true && useReload.boolValue == true && UseAmmo.boolValue == true) {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(reloadAbilityAnimatorParameter, new GUIContent("Animator Parameter"), GUILayout.MaxWidth(280));
                                            if (reloadAbilityAnimatorParameter.stringValue != "") {

                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(reloadAbilityAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();

                                                EditorGUIUtility.labelWidth = 225;
                                                EditorGUILayout.PropertyField(reloadAbilityAnimateOnEntity, new GUIContent("Animate on Entity"));
                                                EditorGUILayout.PropertyField(reloadAbilityAnimateOnScrollGraphic, new GUIContent("Animate on Scroll Ability Graphic"));
                                                EditorGUILayout.PropertyField(reloadAbilityAnimateOnWeapon, new GUIContent("Animate on Weapon"));

                                                InspectorHelpBox("Determines if the animation command is sent on either the entity, current scroll graphic, weapon or all of them. Animate on graphic/weapon should be used if the graphic object has it's own  animator/animation");

                                                EditorGUIUtility.labelWidth = 150;

                                                if (((string)reloadAbilityAnimatorParameterType.enumNames[reloadAbilityAnimatorParameterType.enumValueIndex]) != "Trigger") {
                                                    EditorGUILayout.BeginHorizontal();
                                                    // if not trigger we need to know the value to switch on and off
                                                    EditorGUILayout.PropertyField(reloadAbilityAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.PropertyField(reloadAbilityAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));
                                                    EditorGUILayout.Space();
                                                    EditorGUILayout.EndHorizontal();


                                                }
                                                InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values. Note: Animation will keep repeating until reload duration is up.");
                                            } else {
                                                EditorGUILayout.EndHorizontal();
                                            }

                                        } else {

                                            EditorGUILayout.HelpBox("Ability is not currently setup to reload. To enable reloading please make sure ability is scrollable and both 'Use Reload' and 'Use Ammo' are ticked in the General tab.", MessageType.Warning);

                                        }


                                        EditorGUILayout.EndVertical();

                                        ResetLabelWidth();

                                        #endregion

                                        #endregion

                                        InspectorSectionHeader("Graphic");

                                        #region AllWay 

                                        #region Reload Graphic 

                                        InspectorVerticalBox();

                                        InspectorHelpBox("Settings below for the graphic to show when scrollable ability reloads");

                                        ResetLabelWidth();
                                        if (scrollAbility.boolValue == true && useReload.boolValue == true && UseAmmo.boolValue == true) {

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(reloadAbilityParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));

                                            EditorGUILayout.PropertyField(reloadAbilityObject, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 125;
                                            EditorGUILayout.PropertyField(reloadAbilityStartPosition, new GUIContent("Graphic Position"), GUILayout.MaxWidth(250));
                                            ResetLabelWidth();

                                            if (((string)reloadAbilityStartPosition.enumNames[reloadAbilityStartPosition.enumValueIndex]) == "OnObject") {
                                                EditorGUILayout.PropertyField(reloadAbilityPositionOnObject, new GUIContent("Select Object"), GUILayout.MaxWidth(350));
                                            }

                                            if (((string)reloadAbilityStartPosition.enumNames[reloadAbilityStartPosition.enumValueIndex]) == "OnTag" || ((string)reloadAbilityStartPosition.enumNames[reloadAbilityStartPosition.enumValueIndex]) == "OnSelfTag") {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.LabelField("Select Tag");
                                                reloadAbilityPositionOnTag.stringValue = EditorGUILayout.TagField(reloadAbilityPositionOnTag.stringValue, GUILayout.MaxWidth(250));
                                                EditorGUILayout.Space();
                                            }
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUIUtility.labelWidth = 125;
                                            EditorGUILayout.PropertyField(reloadAbilityAestheticDuration, new GUIContent("Graphic Duration"), GUILayout.MaxWidth(230));
                                            ResetLabelWidth();
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Graphic Offset", GUILayout.MaxWidth(100));

                                            EditorGUILayout.PropertyField(reloadAbilityAestheticsPositionOffset, new GUIContent(""), GUILayout.MaxWidth(500));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(reloadAbilityAestheticsPositionForwardOffset, new GUIContent("Forward Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.PropertyField(reloadAbilityAestheticsPositionRightOffset, new GUIContent("Right Offset"), GUILayout.MaxWidth(230));
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                            ResetLabelWidth();

                                        } else {

                                            EditorGUILayout.HelpBox("Ability is not currently setup to reload. To enable reloading please make sure ability is scrollable and both 'Use Reload' and 'Use Ammo' are ticked in the General tab.", MessageType.Warning);

                                        }



                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #endregion

                                    }

                                    break;

                            }


                            #endregion


                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndScrollView();

                            #endregion


                            EditorGUILayout.EndHorizontal();

                            ResetLabelWidth();

                            break;

                    }
                }
                #endregion

                GUI.backgroundColor = inspectorBackgroundColor;

            }


            EditorGUILayout.EndVertical();
            #endregion



            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();
            #endregion



            EditorGUILayout.EndHorizontal();

            #endregion

        }

        public void GetEffectSettings(SerializedProperty EffectList, bool GlobalEditor = false) {

            GUI.color = Color.white;

            InspectorVerticalBox();
            InspectorHelpBox((abilityCont != null ? "Add either Global Effects already created or " : "") + "Create new Effects from the dropdown below. Effects will run in order shown. Toggle down new effects" +
                " to add graphics and configure additional settings", false, true);


            if (GlobalEditor == false) {
                #region Global Effect
                GUI.color = new Color32(208, 212, 211, 255);
                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.white;
                EditorGUILayout.LabelField("Add Global Effect: (" + StarSymbol + ") ", GUILayout.Width(130));

                this.globalEffectsListChoice = EditorGUILayout.Popup(this.globalEffectsListChoice, this.GlobalEffects.Keys.Select(e => e.name).ToArray(), GUILayout.Width(300));

                if (GUILayout.Button(new GUIContent(ImportIcon), GUILayout.Width(30))) {

                    int option = EditorUtility.DisplayDialogComplex("Global Effect Import", "Do you wish to link to the Global Effect or copy the effects from it?", "Link", "Cancel", "Copy");



                    switch (option) {
                        case 0: //Link 

                            Effect newGlobalEffect = new Effect();

                            newGlobalEffect.globalEffect = this.GlobalEffects.Keys.ToArray()[this.globalEffectsListChoice];

                            // get unique effect ID
                            newGlobalEffect.effectID = ABC_Utilities.GenerateUniqueID();

                            if (abilityID != null) {
                                // link effect to ability
                                newGlobalEffect.effectAbilityID = abilityID.intValue;
                            }

                            this.Abilities[currentAbilityIndex].effects.Add(newGlobalEffect);

                            break;

                        case 1: // Cancel
                            break;

                        case 2: // Copy

                            foreach (Effect eff in this.GlobalEffects.Keys.ToArray()[this.globalEffectsListChoice].ElementEffects) {
                                Effect newEffect = new Effect();
                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(eff), newEffect);
                                this.Abilities[currentAbilityIndex].effects.Add(newEffect);
                            }


                            break;
                    }


                }

                EditorGUILayout.EndHorizontal();
                #endregion
            }

            EditorGUILayout.Space();

            #region effect picker
            GUI.color = new Color32(208, 212, 211, 255);
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            EditorGUILayout.LabelField("Add New Effect: ", GUILayout.Width(130));
            effects = (enumEffects)EditorGUILayout.EnumPopup("", effects, GUILayout.Width(300));

            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {

                var stateIndex = EffectList.arraySize;
                EffectList.InsertArrayElementAtIndex(stateIndex);

                SerializedProperty meStateEffect = EffectList.GetArrayElementAtIndex(stateIndex);
                SerializedProperty effectID = meStateEffect.FindPropertyRelative("effectID");
                SerializedProperty effectAbilityID = meStateEffect.FindPropertyRelative("effectAbilityID");
                SerializedProperty effectName = meStateEffect.FindPropertyRelative("effectName");

                meStateEffect.FindPropertyRelative("globalEffect").objectReferenceValue = null;


                if (effects.ToString() != "New") {
                    // if not new then change value to the selected enum effect
                    effectName.stringValue = (effects).ToString();
                } else {
                    // new element so blank field ready for entry 
                    effectName.stringValue = "New Effect";
                }

                // get unique effect ID
                effectID.intValue = ABC_Utilities.GenerateUniqueID();

                if (abilityID != null) {
                    // link effect to ability
                    effectAbilityID.intValue = abilityID.intValue;
                }

                //set some default values
                meStateEffect.FindPropertyRelative("effectDuration").floatValue = 0.5f;
                meStateEffect.FindPropertyRelative("effectGraphicDuration").floatValue = 2f;

            }
            #endregion

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();

#if ABC_GC_Integration

        if (GlobalEditor == false) { 

            #region AllWay

            InspectorSectionHeader("Game Creator Action On Effect");

            InspectorVerticalBox();


            EditorGUIUtility.labelWidth = 150;

            SetupGCActionList(ref gcEffectActionList, ref gcActionListEditor, "Assets/ABC/Scripts/ABC-Resources/Resources/ABC-GCActions/", name.stringValue + "_Effect.prefab");
            gcActionListEditor.OnInspectorGUI();


            ResetLabelWidth();

            EditorGUILayout.EndVertical();


            #endregion
        }
#endif

#if ABC_GC_2_Integration

        if (GlobalEditor == false) { 

            #region AllWay

            InspectorSectionHeader("Game Creator 2 Action - On Effect");

            InspectorVerticalBox();


            EditorGUIUtility.labelWidth = 150;

            SetupGC2Action(ref gc2EffectAction, "Assets/ABC - Game Creator 2 Integration/Global Elements/ABC-GC2 Actions/", name.stringValue + "_Effect.prefab");

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(gc2EffectAction, new GUIContent(""));
            EditorGUILayout.Space();

            ResetLabelWidth();

            EditorGUILayout.EndVertical();


            #endregion
        }
#endif

            for (int n = 0; n < EffectList.arraySize; n++) {

                #region Properties


                EditorGUILayout.Space();

                SerializedProperty meStateEffect = EffectList.GetArrayElementAtIndex(n);
                SerializedProperty foldOut = meStateEffect.FindPropertyRelative("foldOut");
                SerializedProperty effectID = meStateEffect.FindPropertyRelative("effectID");
                SerializedProperty effectName = meStateEffect.FindPropertyRelative("effectName");

                SerializedProperty allowDuplicateEffectActivation = meStateEffect.FindPropertyRelative("allowDuplicateEffectActivation");
                SerializedProperty limitNoOfDuplicateEffectActivations = meStateEffect.FindPropertyRelative("limitNoOfDuplicateEffectActivations");
                SerializedProperty maxNoDuplicateEffectActivations = meStateEffect.FindPropertyRelative("maxNoDuplicateEffectActivations");
                SerializedProperty potency = meStateEffect.FindPropertyRelative("potency");
                SerializedProperty modifyPotencyUsingStats = meStateEffect.FindPropertyRelative("modifyPotencyUsingStats");
                SerializedProperty potencyStatModifications = meStateEffect.FindPropertyRelative("potencyStatModifications");

                SerializedProperty enhancePotencyFromCriticalHit = meStateEffect.FindPropertyRelative("enhancePotencyFromCriticalHit");
                SerializedProperty potencyCriticalHitBasePercentageEnhancement = meStateEffect.FindPropertyRelative("potencyCriticalHitBasePercentageEnhancement");
                SerializedProperty potencyCriticalHitEnablePercentageEnhancementStatModification = meStateEffect.FindPropertyRelative("potencyCriticalHitEnablePercentageEnhancementStatModification");
                SerializedProperty potencyCriticalHitPercentageEnhancementStatModification = meStateEffect.FindPropertyRelative("potencyCriticalHitPercentageEnhancementStatModification");
                SerializedProperty potencyCriticalHitAudio = meStateEffect.FindPropertyRelative("potencyCriticalHitAudio");
                SerializedProperty potencyCriticalHitColor = meStateEffect.FindPropertyRelative("potencyCriticalHitColor");

                SerializedProperty miscellaneousProperty = meStateEffect.FindPropertyRelative("miscellaneousProperty");
                SerializedProperty miscellaneousAltProperty = meStateEffect.FindPropertyRelative("miscellaneousAltProperty");
                SerializedProperty delay = meStateEffect.FindPropertyRelative("delay");
                SerializedProperty activateOn = meStateEffect.FindPropertyRelative("activateOn");
                SerializedProperty eventType = meStateEffect.FindPropertyRelative("eventType");
                SerializedProperty effectIgnoreTag = meStateEffect.FindPropertyRelative("effectIgnoreTag");
                SerializedProperty effectIgnoreTagCheckObject = meStateEffect.FindPropertyRelative("effectIgnoreTagCheckObject");
                SerializedProperty effectIgnoreTagCheckEntity = meStateEffect.FindPropertyRelative("effectIgnoreTagCheckEntity");

                SerializedProperty specificPrepareTimeRequried = meStateEffect.FindPropertyRelative("specificPrepareTimeRequried");
                SerializedProperty specificPrepareTimeArithmeticComparison = meStateEffect.FindPropertyRelative("specificPrepareTimeArithmeticComparison");
                SerializedProperty prepareTimeToActivate = meStateEffect.FindPropertyRelative("prepareTimeToActivate");

                SerializedProperty requireSplashToActivate = meStateEffect.FindPropertyRelative("requireSplashToActivate");
                SerializedProperty requireCriticalHitToActivate = meStateEffect.FindPropertyRelative("requireCriticalHitToActivate");

                SerializedProperty effectRequiredTag = meStateEffect.FindPropertyRelative("effectRequiredTag");
                SerializedProperty effectRequiredTagCheckObject = meStateEffect.FindPropertyRelative("effectRequiredTagCheckObject");
                SerializedProperty effectRequiredTagCheckEntity = meStateEffect.FindPropertyRelative("effectRequiredTagCheckEntity");


                SerializedProperty effectDuration = meStateEffect.FindPropertyRelative("effectDuration");
                SerializedProperty playEffect = meStateEffect.FindPropertyRelative("playEffect");
                SerializedProperty addToEffectText = meStateEffect.FindPropertyRelative("addToEffectText");
                SerializedProperty effectText = meStateEffect.FindPropertyRelative("effectText");
                SerializedProperty effectTextDisplayTime = meStateEffect.FindPropertyRelative("effectTextDuration");
                SerializedProperty effectTextColour = meStateEffect.FindPropertyRelative("effectTextColour");
                SerializedProperty addToEffectLog = meStateEffect.FindPropertyRelative("addToEffectLog");
                SerializedProperty effectLogText = meStateEffect.FindPropertyRelative("effectLogText");


                SerializedProperty effectRandomChance = meStateEffect.FindPropertyRelative("effectRandomChance");
                SerializedProperty effectRandomChanceProbabilityMin = meStateEffect.FindPropertyRelative("effectRandomChanceProbabilityMin");
                SerializedProperty effectRandomChanceProbabilityMax = meStateEffect.FindPropertyRelative("effectRandomChanceProbabilityMax");

                SerializedProperty hitStopsMovement = meStateEffect.FindPropertyRelative("hitStopsMovement");
                SerializedProperty hitStopsMovementRandomChance = meStateEffect.FindPropertyRelative("hitStopsMovementRandomChance");
                SerializedProperty hitStopsMovementProbabilityMin = meStateEffect.FindPropertyRelative("hitStopsMovementProbabilityMin");
                SerializedProperty hitStopsMovementProbabilityMax = meStateEffect.FindPropertyRelative("hitStopsMovementProbabilityMax");

                SerializedProperty hitPreventsCasting = meStateEffect.FindPropertyRelative("hitPreventsCasting");
                SerializedProperty hitPreventsCastingRandomChance = meStateEffect.FindPropertyRelative("hitPreventsCastingRandomChance");
                SerializedProperty hitPreventsCastingProbabilityMin = meStateEffect.FindPropertyRelative("hitPreventsCastingProbabilityMin");
                SerializedProperty hitPreventsCastingProbabilityMax = meStateEffect.FindPropertyRelative("hitPreventsCastingProbabilityMax");
                SerializedProperty hitInterruptsCasting = meStateEffect.FindPropertyRelative("hitInterruptsCasting");
                SerializedProperty hitInterruptsCastingRandomChance = meStateEffect.FindPropertyRelative("hitInterruptsCastingRandomChance");
                SerializedProperty hitInterruptsCastingProbabilityMin = meStateEffect.FindPropertyRelative("hitInterruptsCastingProbabilityMin");
                SerializedProperty hitInterruptsCastingProbabilityMax = meStateEffect.FindPropertyRelative("hitInterruptsCastingProbabilityMax");
                SerializedProperty repeatEffect = meStateEffect.FindPropertyRelative("repeatEffect");
                SerializedProperty repeatDuration = meStateEffect.FindPropertyRelative("repeatDuration");
                SerializedProperty repeatInterval = meStateEffect.FindPropertyRelative("repeatInterval");
                SerializedProperty repeatAesthetics = meStateEffect.FindPropertyRelative("repeatAesthetics");

                SerializedProperty showNonActivationReason = meStateEffect.FindPropertyRelative("showNonActivationReason");

                SerializedProperty enableRemoveEffect = meStateEffect.FindPropertyRelative("enableRemoveEffect");
                SerializedProperty dispellable = meStateEffect.FindPropertyRelative("dispellable");
                SerializedProperty removeEffectAddToEffectText = meStateEffect.FindPropertyRelative("removeEffectAddToEffectText");
                SerializedProperty removeEffectText = meStateEffect.FindPropertyRelative("removeEffectText");
                SerializedProperty removeEffectTextDisplayTime = meStateEffect.FindPropertyRelative("removeEffectTextDuration");
                SerializedProperty removeEffectTextColour = meStateEffect.FindPropertyRelative("removeEffectTextColour");
                SerializedProperty removeEffectAddToEffectLog = meStateEffect.FindPropertyRelative("removeEffectAddToEffectLog");
                SerializedProperty removeEffectLogText = meStateEffect.FindPropertyRelative("removeEffectLogText");

                SerializedProperty enableEffectActivationRange = meStateEffect.FindPropertyRelative("enableEffectActivationRange");
                SerializedProperty effectActivationRange = meStateEffect.FindPropertyRelative("effectActivationRange");
                SerializedProperty useProjectileForActivationRange = meStateEffect.FindPropertyRelative("useProjectileForActivationRange");


                SerializedProperty effectParticle = meStateEffect.FindPropertyRelative("effectGraphic");
                SerializedProperty effectChildParticle = meStateEffect.FindPropertyRelative("effectChildGraphic");
                SerializedProperty scaleEffectGraphic = meStateEffect.FindPropertyRelative("scaleEffectGraphic");
                SerializedProperty effectGraphicScale = meStateEffect.FindPropertyRelative("effectGraphicScale");
                SerializedProperty effectGraphicOffset = meStateEffect.FindPropertyRelative("effectGraphicOffset");
                SerializedProperty effectGraphicForwardOffset = meStateEffect.FindPropertyRelative("effectGraphicForwardOffset");
                SerializedProperty effectGraphicRightOffset = meStateEffect.FindPropertyRelative("effectGraphicRightOffset");

                SerializedProperty effectParticleDelay = meStateEffect.FindPropertyRelative("effectGraphicDelay");
                SerializedProperty effectParticleDuration = meStateEffect.FindPropertyRelative("effectGraphicDuration");
                SerializedProperty effectFollowTarget = meStateEffect.FindPropertyRelative("effectFollowTarget");
                SerializedProperty effectOnHitPosition = meStateEffect.FindPropertyRelative("effectOnHitPosition");

                #endregion

                //If null then loading globally
                Effect effectItem = null;

                if (this.Abilities.Count > 0 && currentAbilityIndex < AbilityCount && n < this.Abilities[currentAbilityIndex].effects.Count) {
                    effectItem = this.Abilities[currentAbilityIndex].effects[n];
                }



                InspectorSectionHeader((effectItem == null || effectItem.globalEffect == null) ? effectName.stringValue : "(" + StarSymbol + ") Global Effect: " + effectItem.globalEffect.name);
                InspectorVerticalBox();


                EditorGUILayout.BeginHorizontal();

                if (effectItem != null && effectItem.globalEffect != null) {

                    if (GUILayout.Button(new GUIContent("Load Global Effect"), GUILayout.Width(350))) {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(GlobalEffects[effectItem.globalEffect]);
                    }

                } else {


                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(potency, GUILayout.Width(100));
                    InspectorHorizontalSpace(5);
                    EditorGUIUtility.labelWidth = 80;
                    EditorGUILayout.PropertyField(delay, new GUIContent("Start Delay"), GUILayout.Width(120));
                    InspectorHorizontalSpace(5);
                    EditorGUIUtility.labelWidth = 65;
                    EditorGUILayout.PropertyField(effectDuration, new GUIContent("Duration"), GUILayout.Width(100));
                    InspectorHorizontalSpace(5);
                    EditorGUIUtility.labelWidth = 130;
                    ResetLabelWidth();
                }


                if (GUILayout.Button("Up")) {
                    EffectList.MoveArrayElement(n, n - 1);
                }


                if (GUILayout.Button("Down")) {
                    EffectList.MoveArrayElement(n, n + 1);
                }


                Color originalTextColor = GUI.skin.button.normal.textColor;
                GUI.skin.button.normal.textColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(40))) {
                    EffectList.DeleteArrayElementAtIndex(n);
                    GUI.skin.button.normal.textColor = originalTextColor;
                    // we can go to next loop as this no longer exists 
                    continue;
                }

                GUI.skin.button.normal.textColor = originalTextColor;

                EditorGUILayout.EndHorizontal();

                if (effectItem == null || effectItem.globalEffect == null) {
                    foldOut.boolValue = EditorGUILayout.Foldout(foldOut.boolValue, "More Settings");
                }

                EditorGUILayout.EndVertical();

                if ((effectItem == null || effectItem.globalEffect == null) && foldOut.boolValue == true) {

                    #region AllWay 

                    #region Effect Properties 

                    InspectorVerticalBox();

                    //InspectorSectionHeader("Effect Properties", "Loop this");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID: " + effectID.intValue.ToString(), GUILayout.MaxWidth(100));

                    EditorGUILayout.PropertyField(effectName, GUILayout.Width(300));
                    EditorGUIUtility.labelWidth = 150;
                    EditorGUILayout.PropertyField(enableRemoveEffect);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(dispellable);
                    EditorGUIUtility.labelWidth = 170;
                    EditorGUILayout.PropertyField(requireSplashToActivate);
                    EditorGUILayout.PropertyField(requireCriticalHitToActivate, new GUIContent("Require Crit To Activate"));
                    EditorGUILayout.EndHorizontal();

                    InspectorHelpBox("The name of the effect will be used later by ABC to determine what code to run. If duration of -1 is set then effect will have an infinite duration. If dispellable is true then effects can be removed instantly from other effects. If require splash to activate" +
                        " is ticked then the effect will only be added if the effect was applied from an ability splashing", false);



                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.PropertyField(specificPrepareTimeRequried);
                    if (specificPrepareTimeRequried.boolValue == true) {
                        EditorGUIUtility.labelWidth = 130;
                        EditorGUILayout.PropertyField(specificPrepareTimeArithmeticComparison, new GUIContent(""));
                        EditorGUIUtility.labelWidth = 150;
                        EditorGUILayout.PropertyField(prepareTimeToActivate, new GUIContent("Prepare Time (Seconds)"));
                    }
                    EditorGUILayout.EndHorizontal();
                    InspectorHelpBox("Configure if preparation time needs to be higher or lower a certain time for the effect to activate ");


                    ResetLabelWidth();


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(activateOn, GUILayout.Width(250));
                    EditorGUIUtility.labelWidth = 120;
                    EditorGUILayout.PropertyField(miscellaneousProperty, new GUIContent("Misc Property"), GUILayout.Width(300));
                    EditorGUILayout.EndHorizontal();
                    ResetLabelWidth();
                    //InspectorHelpBox("Entity - Will apply the effect on the entity hit. Originator - Will apply the effect on the originator. Both - Will apply the effect simultaneously on both the entity hit and the originator. Misc Property is A field which can be used to transfer further information about the effect. Can be used to pass ID's etc.");
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(eventType, GUILayout.Width(250));
                    EditorGUIUtility.labelWidth = 120;
                    EditorGUILayout.PropertyField(miscellaneousAltProperty, new GUIContent("Alt Misc Property"), GUILayout.Width(300));
                    EditorGUILayout.EndHorizontal();
                    InspectorHelpBox("Determines the event type when activating and deactiving the effect. Standard will run the normal code, RaiseEvent will invoke the event delegate and both will do everything.");



                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.PropertyField(effectRandomChance);

                    EditorGUILayout.BeginHorizontal();
                    ResetLabelWidth();

                    if (effectRandomChance.boolValue == true) {
                        EditorGUILayout.PropertyField(effectRandomChanceProbabilityMin, new GUIContent("Probability Min"));
                        EditorGUILayout.PropertyField(effectRandomChanceProbabilityMax, new GUIContent("Probability Max"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.PropertyField(hitStopsMovement);

                    if (hitStopsMovement.boolValue == true) {
                        EditorGUILayout.PropertyField(hitStopsMovementRandomChance, new GUIContent("Stop Movement Chance"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    ResetLabelWidth();

                    if (hitStopsMovement.boolValue == true && hitStopsMovementRandomChance.boolValue == true) {
                        EditorGUILayout.PropertyField(hitStopsMovementProbabilityMin, new GUIContent("Probability Min"));
                        EditorGUILayout.PropertyField(hitStopsMovementProbabilityMax, new GUIContent("Probability Max"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.PropertyField(hitPreventsCasting, new GUIContent("Hit Prevent Activation"));

                    if (hitPreventsCasting.boolValue == true) {
                        EditorGUILayout.PropertyField(hitPreventsCastingRandomChance, new GUIContent("Prevent Activation Chance"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    ResetLabelWidth();

                    if (hitPreventsCasting.boolValue == true && hitPreventsCastingRandomChance.boolValue == true) {
                        EditorGUILayout.PropertyField(hitPreventsCastingProbabilityMin, new GUIContent("Probability Min"));
                        EditorGUILayout.PropertyField(hitPreventsCastingProbabilityMax, new GUIContent("Probability Max"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(hitInterruptsCasting, new GUIContent("Hit Interupt Activation"));

                    if (hitInterruptsCasting.boolValue == true) {
                        EditorGUILayout.PropertyField(hitInterruptsCastingRandomChance, new GUIContent("Interupt Activation Chance"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                    ResetLabelWidth();
                    if (hitInterruptsCasting.boolValue == true && hitInterruptsCastingRandomChance.boolValue == true) {
                        EditorGUILayout.PropertyField(hitInterruptsCastingProbabilityMin, new GUIContent("Probability Min"));
                        EditorGUILayout.PropertyField(hitInterruptsCastingProbabilityMax, new GUIContent("Probability Max"));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();


                    EditorGUILayout.EndVertical();

                    #endregion

                    #endregion

                    #region AllWay 

                    #region Effect Stat Modifications 

                    InspectorVerticalBox();

                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.PropertyField(modifyPotencyUsingStats);
                    InspectorHelpBox("Use stats from either the target entity and/or the originator to modify the potency value");
                    ResetLabelWidth();


                    if (modifyPotencyUsingStats.boolValue == true) {




                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Box("Potency - Stat Modifications", new GUILayoutOption[] { GUILayout.MinWidth(minimumAllWaySectionWidth - 50), GUILayout.Height(19) });

                        GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                        if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {
                            potencyStatModifications.InsertArrayElementAtIndex(potencyStatModifications.arraySize);
                            potencyStatModifications.GetArrayElementAtIndex(potencyStatModifications.arraySize - 1).FindPropertyRelative("statSource").enumValueIndex = 1;
                            potencyStatModifications.GetArrayElementAtIndex(potencyStatModifications.arraySize - 1).FindPropertyRelative("percentageValue").floatValue = 100;
                        }
                        GUILayout.EndHorizontal();

                        GUI.skin.button.normal.textColor = originalTextColor;

                        if (potencyStatModifications.arraySize > 0) {
                            EditorGUILayout.BeginVertical("box");
                            for (int i = 0; i < potencyStatModifications.arraySize; i++) {
                                SerializedProperty element = potencyStatModifications.GetArrayElementAtIndex(i);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(element.FindPropertyRelative("arithmeticOperator"), new GUIContent(""), GUILayout.Width(65));
                                EditorGUILayout.PropertyField(element.FindPropertyRelative("percentageValue"), new GUIContent(""), GUILayout.Width(50));
                                EditorGUIUtility.labelWidth = 40;
                                EditorGUILayout.PropertyField(element.FindPropertyRelative("statSource"), new GUIContent("% of"), GUILayout.Width(150));
                                EditorGUILayout.PropertyField(element.FindPropertyRelative("statIntegrationType"), new GUIContent(""), GUILayout.Width(90));
                                EditorGUILayout.PropertyField(element.FindPropertyRelative("statName"), new GUIContent("Stat:"), GUILayout.Width(140));
                                ResetLabelWidth();
                                GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);

                                if (GUILayout.Button(UpArrowSymbol.ToString())) {
                                    potencyStatModifications.MoveArrayElement(i, i - 1);
                                }
                                if (GUILayout.Button(DownArrowSymbol.ToString())) {
                                    potencyStatModifications.MoveArrayElement(i, i + 1);
                                }


                                GUI.skin.button.normal.textColor = Color.red;
                                if (GUILayout.Button("X", GUILayout.Width(40))) {
                                    potencyStatModifications.DeleteArrayElementAtIndex(i);
                                }
                                GUI.skin.button.normal.textColor = originalTextColor;
                                GUILayout.EndHorizontal();

                            }
                            EditorGUILayout.EndVertical();

                        }


                    }



                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();

                    #endregion

                    #endregion

                    #region AllWay 

                    #region Critical Hit Potency Modifications 

                    InspectorVerticalBox();

                    EditorGUIUtility.labelWidth = 210;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(enhancePotencyFromCriticalHit, GUILayout.Width(250));

                    if (enhancePotencyFromCriticalHit.boolValue == true) {
                        EditorGUIUtility.labelWidth = 90;
                        EditorGUILayout.PropertyField(potencyCriticalHitColor, new GUIContent("Crit Text Color"), GUILayout.Width(160));
                        EditorGUILayout.PropertyField(potencyCriticalHitAudio, new GUIContent("Crit Audio"), GUILayout.Width(200));
                        

                    }


                    EditorGUILayout.EndHorizontal();
                    InspectorHelpBox("If critical hit enhance the potency (after stats) by a percentage value. How much potency is enhanced can also be increased by stats.", false);
                    ResetLabelWidth();


                    if (enhancePotencyFromCriticalHit.boolValue == true) {                      

                        EditorGUIUtility.labelWidth = 180;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(potencyCriticalHitBasePercentageEnhancement, new GUIContent("Crit Potency Enhancement (%)"), GUILayout.Width(230));
                        EditorGUIUtility.labelWidth = 120;
                        EditorGUILayout.PropertyField(potencyCriticalHitEnablePercentageEnhancementStatModification, new GUIContent("Stat Modification"), GUILayout.Width(180));
                        EditorGUILayout.EndHorizontal();

                        if (potencyCriticalHitEnablePercentageEnhancementStatModification.boolValue == true) {


                            EditorGUILayout.BeginVertical("box");
                            SerializedProperty element = potencyCriticalHitPercentageEnhancementStatModification;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUIUtility.labelWidth = 120;
                            EditorGUILayout.PropertyField(element.FindPropertyRelative("percentageValue"), new GUIContent("Enhance Potency By "), GUILayout.Width(180));
                            EditorGUIUtility.labelWidth = 40;
                            EditorGUILayout.PropertyField(element.FindPropertyRelative("statIntegrationType"), new GUIContent("% of"), GUILayout.Width(170));
                            EditorGUILayout.PropertyField(element.FindPropertyRelative("statName"), new GUIContent("Stat:"), GUILayout.Width(140));
                            ResetLabelWidth();

                            GUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                        }

                     




                    }



                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();

                    #endregion

                    #endregion

                    #region SideBySide

                    EditorGUILayout.BeginHorizontal();

                    #region Effect Activation 

                    InspectorVerticalBox(true);

                    EditorGUIUtility.labelWidth = 225;

                    EditorGUILayout.PropertyField(allowDuplicateEffectActivation);

                    if (allowDuplicateEffectActivation.boolValue == true) {
                        EditorGUILayout.PropertyField(limitNoOfDuplicateEffectActivations, new GUIContent("Limit Duplicate Effect Activation"));
                        if (limitNoOfDuplicateEffectActivations.boolValue == true) {
                            EditorGUIUtility.labelWidth = 175;
                            EditorGUILayout.PropertyField(maxNoDuplicateEffectActivations, new GUIContent("Max No Of Duplicate Effects"), GUILayout.Width(230));
                        }
                    }


                    InspectorHelpBox("If true then the effect can be applied multiple times. If false then the effect can only be applied once per ABC user. Duplicate effect activation" +
                        " can be limited so only a max number of duplicate effects can activate at one time");


                    ResetLabelWidth();



                    EditorGUIUtility.labelWidth = 150;

                    EditorGUILayout.PropertyField(enableEffectActivationRange, new GUIContent("Activation Range"));

                    if (enableEffectActivationRange.boolValue == true) {


                        EditorGUILayout.PropertyField(useProjectileForActivationRange, new GUIContent("Use Projectiles Range"));


                        if (useProjectileForActivationRange.boolValue == false) {
                            EditorGUILayout.PropertyField(effectActivationRange, GUILayout.Width(190));
                        }
                        ResetLabelWidth();


                    }

                    InspectorHelpBox("Activation range if enabled will only have the effect activate if the entity hit is still in the range set. Does not work for Raycast Abilities.");





                    if (effectDuration.floatValue > 0) {
                        EditorGUILayout.PropertyField(repeatEffect);

                        if (repeatEffect.boolValue == true) {
                            EditorGUILayout.PropertyField(repeatAesthetics);


                            EditorGUILayout.PropertyField(repeatDuration, GUILayout.Width(190));
                            EditorGUILayout.PropertyField(repeatInterval, GUILayout.Width(190));



                        }

                    }






                    EditorGUILayout.EndVertical();

                    #endregion



                    #region Effect Graphic
                    ResetLabelWidth();

                    InspectorVerticalBox(true);



                    EditorGUILayout.PropertyField(playEffect, new GUIContent("Effect Graphic"));
                    if (playEffect.boolValue == true) {
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(effectParticle, new GUIContent("Main Graphic"), GUILayout.MaxWidth(350));

                        EditorGUILayout.PropertyField(effectChildParticle, new GUIContent("Sub Graphic"), GUILayout.MaxWidth(350));

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(scaleEffectGraphic, new GUIContent("Scale Graphic"));
                        if (scaleEffectGraphic.boolValue == true) {
                            EditorGUILayout.PropertyField(effectGraphicScale, new GUIContent("Scale"), GUILayout.MaxWidth(150));
                        }
                        EditorGUILayout.Space();
                        ResetLabelWidth();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 90;
                        EditorGUILayout.PropertyField(effectParticleDelay, new GUIContent("Delay"));
                        EditorGUILayout.PropertyField(effectParticleDuration, new GUIContent("Duration"));
                        EditorGUILayout.EndHorizontal();
                        ResetLabelWidth();

                        EditorGUILayout.Space();
                        EditorGUIUtility.labelWidth = 170;
                        EditorGUILayout.PropertyField(effectFollowTarget, new GUIContent("Follow Affected Object"));
                        EditorGUILayout.PropertyField(effectOnHitPosition);

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Graphic Offset", GUILayout.MaxWidth(100));
                        EditorGUILayout.PropertyField(effectGraphicOffset, new GUIContent(""), GUILayout.MaxWidth(300));

                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 100;
                        EditorGUILayout.PropertyField(effectGraphicForwardOffset, new GUIContent("Forward Offset"));
                        EditorGUILayout.PropertyField(effectGraphicRightOffset, new GUIContent("Right Offset"));
                        EditorGUILayout.EndHorizontal();



                        ResetLabelWidth();
                        EditorGUILayout.Space();
                    }



                    EditorGUILayout.EndVertical();

                    #endregion

                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region SideBySide

                    EditorGUILayout.BeginHorizontal();

                    #region Graphic Log

                    InspectorVerticalBox(true);



                    EditorGUILayout.PropertyField(addToEffectText, new GUIContent("Graphic Log"));
                    InspectorHelpBox("Placeholders: #Potency# - Potency Value, #Origin# - name of GameObject Ability was fired from, #Target# - Name of GameObject hit by Ability, #Ability# - Ability Name, #Effect# - Effect Name");


                    if (addToEffectText.boolValue == true) {
                        EditorGUILayout.PropertyField(effectText, new GUIContent(""));
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 110;
                        EditorGUILayout.PropertyField(effectTextDisplayTime, new GUIContent("Graphic Duration"), GUILayout.Width(170));
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(effectTextColour, new GUIContent("Colour"));
                        EditorGUILayout.EndHorizontal();
                        ResetLabelWidth();
                    }

                    EditorGUILayout.Space();
                    EditorGUIUtility.labelWidth = 160;
                    EditorGUILayout.PropertyField(removeEffectAddToEffectText, new GUIContent("Remove Graphic Log"));
                    ResetLabelWidth();
                    if (removeEffectAddToEffectText.boolValue == true) {
                        EditorGUILayout.PropertyField(removeEffectText, new GUIContent(""));
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 110;
                        EditorGUILayout.PropertyField(removeEffectTextDisplayTime, new GUIContent("Graphic Duration"), GUILayout.Width(170));
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(removeEffectTextColour, new GUIContent("Colour"));
                        EditorGUILayout.EndHorizontal();


                        ResetLabelWidth();
                    }

                    EditorGUILayout.Space();


                    EditorGUILayout.EndVertical();

                    #endregion



                    #region Effect Log
                    ResetLabelWidth();

                    InspectorVerticalBox(true);



                    EditorGUIUtility.labelWidth = 195;
                    EditorGUILayout.PropertyField(addToEffectLog, new GUIContent("Effect Log"));
                    InspectorHelpBox("Set what is shown on the effect GUI log");

                    if (addToEffectLog.boolValue == true) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(effectLogText, new GUIContent(""));
                        EditorGUILayout.EndHorizontal();



                        EditorGUILayout.Space();
                    }


                    EditorGUILayout.PropertyField(removeEffectAddToEffectLog, new GUIContent("Remove Effect Log"));

                    if (removeEffectAddToEffectLog.boolValue == true) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(removeEffectLogText, new GUIContent(""));
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.PropertyField(showNonActivationReason);
                    InspectorHelpBox("If true then the effect log will notify the user why the effect didn't activate (already exists, resisted etc)");

                    ResetLabelWidth();






                    #endregion
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    #region SideBySide

                    EditorGUILayout.BeginHorizontal();

                    #region Effect Ignore Tag

                    InspectorVerticalBox(true);

                    InspectorHelpBox("This effect will not activate on the tags listed", false);

                    EditorGUIUtility.labelWidth = 170;
                    EditorGUILayout.PropertyField(effectIgnoreTagCheckEntity, new GUIContent("Check Tags On Entity"));
                    EditorGUILayout.PropertyField(effectIgnoreTagCheckObject, new GUIContent("Check Tags On Object Hit"));
                    ResetLabelWidth();
                    InspectorListBox("Effect Ignore Tags", effectIgnoreTag);



                    EditorGUILayout.EndVertical();

                    #endregion



                    #region Effect Required Tags
                    ResetLabelWidth();

                    InspectorVerticalBox(true);
                    InspectorHelpBox("This effect will only activate on the tags listed", false);

                    EditorGUIUtility.labelWidth = 170;
                    EditorGUILayout.PropertyField(effectRequiredTagCheckEntity, new GUIContent("Check Tags On Entity"));
                    EditorGUILayout.PropertyField(effectRequiredTagCheckObject, new GUIContent("Check Tags On Object Hit"));
                    ResetLabelWidth();
                    InspectorListBox("Effect Required Tags", effectRequiredTag);


                    #endregion
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    #endregion



                }





            }



        }

        /// <summary>
        /// Determines if the ability can currently be displayed on the reorderable list (recursivly calls itself to check if all parent branches are set to show children)
        /// </summary>
        /// <param name="Ability">Ability to check if can be displayed</param>
        /// <returns>True if ability can be displayed, else false</returns>
        private bool CanDisplayAbilityOnList(ABC_Ability Ability) {

            if (Ability == null)
                return true;

            //If parent ability is 0 then this ability can be displayed as we are not dependant on a parent ability having show children setting set to true
            if (Ability.parentAbilityID == 0)
                return true;


            //If the parent ability is not 0 then this ability is a child of another ability in hiearchy
            //if parent ability show children setting is true then recursivley call this method again to check if the parent can also be displayed (will check parents parent etc etc)
            if (this.Abilities.Where(a => a.abilityID == Ability.parentAbilityID).FirstOrDefault() == null || this.Abilities.Where(a => a.abilityID == Ability.parentAbilityID).FirstOrDefault().showChildrenInInspector == true)
                return CanDisplayAbilityOnList(abilityCont.FindAbility(Ability.parentAbilityID));
            else
                return false; // can't be displayed as parent show children is set to false


        }

        //Creates the ability reorderable list
        private void CreateAbilityReorderableList(string searchfilter = "", int dropdownfilter = 0) {

            sideListFilterOptions.Clear();
            sideListFilterOptions.Add("All");
            sideListFilterOptions.Add("Enabled");
            sideListFilterOptions.Add("---------  Tags   --------");
            sideListFilterOptions.AddRange(this.AllAbilities.SelectMany(item => item.abilityTags.OrderBy(x => x).ToList()));

            sideListFilterOptions.Add("---------  Globals   --------");

            if (Application.isPlaying) {
                sideListFilterOptions.AddRange(this.AllAbilities.Where(a => a.globalElementSource != null).Select(item => StarSymbol + item.globalElementSource.name).ToList());
            } else {
                sideListFilterOptions.AddRange(this.Abilities.Where(a => a.globalAbilities != null).Select(item => StarSymbol + item.globalAbilities.name).ToList());
            }

            if (abilityCont != null) {
                sideListFilterOptions.Add("--------- Weapons --------");
                sideListFilterOptions.AddRange(this.AllWeapons.Select(item => " " + item.weaponName));

                sideListFilterOptions.Add("---------  Groups --------");
                sideListFilterOptions.AddRange(abilityCont.AbilityGroups.Select(item => item.groupName));
            }




            // determines if list can be moved around
            bool isDraggable = false;
            int count = 0;

            if (abilityCont == null || abilityCont.DraggableMode == true) {
                reorderableAbilitylist = this.Abilities.ToList();
                count = this.Abilities.Count;
                isDraggable = true;
            } else if (searchfilter != "") {

                reorderableAbilitylist = this.Abilities.Where(a => a.globalAbilities == null && a.name.Trim().ToUpper().Contains(searchfilter.Trim().ToUpper()) || a.globalAbilities != null && a.globalAbilities.ElementAbilities.Any(ga => ga.name.Trim().ToUpper().Contains(searchfilter.Trim().ToUpper()))).ToList();
                //disable draggable functionality
                isDraggable = false;
                count = reorderableAbilitylist.Count;

            } else {

                reorderableAbilitylist = this.Abilities.Where(a => CanDisplayAbilityOnList(a) == true).ToList();
                count = this.Abilities.Count;
                //disable draggable functionality
                isDraggable = false;
            }




            //Remove those not in the dropdown filter 
            if (abilityCont == null || abilityCont.abilitySideListFilterChoice == 0 || abilityCont.DraggableMode == true) {
                //remove none as ALL
            } else if (abilityCont.abilitySideListFilterChoice == 1) {

                //remove all not enabled 
                reorderableAbilitylist.RemoveAll(i => i.abilityEnabled == false);

                isDraggable = false;

            } else if (abilityCont.abilitySideListFilterChoice > sideListFilterOptions.FindIndex(s => s == "---------  Tags   --------") && abilityCont.abilitySideListFilterChoice < sideListFilterOptions.FindIndex(s => s == "---------  Globals   --------")) {


                //remove all abilities not linked to the tag 
                reorderableAbilitylist.RemoveAll(i => i.abilityTags.Contains(sideListFilterOptions[abilityCont.abilitySideListFilterChoice]) == false && (i.globalAbilities == null || i.globalAbilities != null && i.globalAbilities.ElementAbilities.All(ea => ea.abilityTags.Contains(sideListFilterOptions[abilityCont.abilitySideListFilterChoice]) == false)));
                isDraggable = false;

            } else if (abilityCont.abilitySideListFilterChoice > sideListFilterOptions.FindIndex(s => s == "---------  Globals   --------") && abilityCont.abilitySideListFilterChoice < sideListFilterOptions.FindIndex(s => s == "--------- Weapons --------")) {

                //remove all abilities not linked to the global 
                if (Application.isPlaying) {
                    reorderableAbilitylist.RemoveAll(i => i.globalElementSource == null || StarSymbol + i.globalElementSource.name != sideListFilterOptions[abilityCont.abilitySideListFilterChoice]);
                } else {
                    reorderableAbilitylist.RemoveAll(i => i.globalAbilities == null || StarSymbol + i.globalAbilities.name != sideListFilterOptions[abilityCont.abilitySideListFilterChoice]);
                }


                isDraggable = false;

            } else if (abilityCont.abilitySideListFilterChoice > sideListFilterOptions.FindIndex(s => s == "--------- Weapons --------") && abilityCont.abilitySideListFilterChoice < sideListFilterOptions.FindIndex(s => s == "---------  Groups --------")) {



                ABC_Controller.Weapon weapon = this.AllWeapons.Where(w => w.weaponName == sideListFilterOptions[abilityCont.abilitySideListFilterChoice].Trim()).FirstOrDefault();

                foreach (ABC_Ability ability in reorderableAbilitylist.ToList()) {

                    //Remove all abilities that the weapon does not enable   
                    if (ability.globalAbilities == null && ability.globalElementSource == null && weapon.enableAbilityIDs.Contains(ability.abilityID) == false && weapon.enableAbilityStrings.Contains(ability.abilityID.ToString()) == false && weapon.enableAbilityStrings.Contains(ability.name) == false && weapon.abilityIDsToEnableAfterBlocking.Contains(ability.abilityID) == false && weapon.abilityIDsToEnableAfterParrying.Contains(ability.abilityID) == false && weapon.abilityIDToActivateAfterBlocking != ability.abilityID && weapon.abilityIDToActivateAfterParrying != ability.abilityID) {
                        reorderableAbilitylist.Remove(ability);
                    } else if (Application.isPlaying == false && ability.globalAbilities != null && ABC_Utilities.GetAbilitiesFromGlobalElement(ability.globalAbilities).All(ga => weapon.enableAbilityIDs.Contains(ga.abilityID) == false && weapon.enableAbilityStrings.Contains(ga.abilityID.ToString()) == false && weapon.enableAbilityStrings.Contains(ga.name) == false && weapon.abilityIDsToEnableAfterBlocking.Contains(ga.abilityID) == false && weapon.abilityIDsToEnableAfterParrying.Contains(ga.abilityID) == false && weapon.abilityIDToActivateAfterBlocking != ga.abilityID && weapon.abilityIDToActivateAfterParrying != ga.abilityID)) {

                        //If has global ability then check those instead
                        reorderableAbilitylist.Remove(ability);
                    } else if (Application.isPlaying == true && ability.globalElementSource != null && ABC_Utilities.GetAbilitiesFromGlobalElement(ability.globalElementSource).All(ga => weapon.enableAbilityIDs.Contains(ga.abilityID) == false && weapon.enableAbilityStrings.Contains(ga.abilityID.ToString()) == false && weapon.enableAbilityStrings.Contains(ga.name) == false && weapon.abilityIDsToEnableAfterBlocking.Contains(ga.abilityID) == false && weapon.abilityIDsToEnableAfterParrying.Contains(ga.abilityID) == false && weapon.abilityIDToActivateAfterBlocking != ga.abilityID && weapon.abilityIDToActivateAfterParrying != ga.abilityID)) {

                        //If has element source in game mode ability then check those instead
                        reorderableAbilitylist.Remove(ability);
                    }
                }

                isDraggable = false;

            } else if (abilityCont.abilitySideListFilterChoice > sideListFilterOptions.FindIndex(s => s == "---------  Groups --------")) {


                foreach (ABC_Ability ability in reorderableAbilitylist.ToList()) {

                    int groupID = abilityCont.AbilityGroups.Where(g => g.groupName == sideListFilterOptions[abilityCont.abilitySideListFilterChoice]).FirstOrDefault().groupID;
                    string groupName = abilityCont.AbilityGroups.Where(g => g.groupName == sideListFilterOptions[abilityCont.abilitySideListFilterChoice]).FirstOrDefault().groupName;


                    //remove all abilities not linked to the group 
                    if (ability.globalAbilities == null && (ability.allowAbilityGroupAssignment == false || ability.assignedAbilityGroupIDs.Contains(groupID) == false && ability.assignedAbilityGroupNames.Select(i => i.Trim().ToUpper()).Contains(groupName.Trim().ToUpper()) == false)) {
                        reorderableAbilitylist.Remove(ability);
                    } else if (ability.globalAbilities != null && ability.globalAbilities.ElementAbilities.All(ga => ga.allowAbilityGroupAssignment == false || ga.assignedAbilityGroupIDs.Contains(groupID) == false && ga.assignedAbilityGroupNames.Select(i => i.Trim().ToUpper()).Contains(groupName.Trim().ToUpper()) == false)) {
                        //If has global ability then check those instead
                        reorderableAbilitylist.Remove(ability);
                    }
                }


                isDraggable = false;

            }


            reorderableListAbilities = new ReorderableList(reorderableAbilitylist,
                                                           typeof(ABC_Ability),
                                                           isDraggable, false, false, false);


            // name the header
            reorderableListAbilities.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Abilities:" + reorderableAbilitylist.Count);
            };


            // when we select any of the list then it will set the current ability to show the ability details ready to be changed
            reorderableListAbilities.onSelectCallback = (ReorderableList l) => {
                currentAbility = this.Abilities.IndexOf(reorderableAbilitylist[l.index]);

            };


            reorderableListAbilities.onReorderCallback = (ReorderableList l) => {

                //get current ability
                ABC_Ability movedElement = this.Abilities[currentAbilityIndex];
                //remove current ability
                this.Abilities.Remove(movedElement);
                //insert it back to l.index where the element was dragged to in the list
                this.Abilities.Insert(l.index, movedElement);

                if (abilityCont != null)
                    EditorUtility.SetDirty(abilityCont);
                else
                    EditorUtility.SetDirty(globElement);

            };


            //					 design of the reorderable list 
            reorderableListAbilities.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) => {
                if (index < AbilityCount) {
                    ABC_Ability ability = reorderableAbilitylist[index];

                    string name = "";


                    if (ability.globalElementSource != null || ability.globalAbilities != null) {
                        name += StarSymbol.ToString();
                    }



                    if (ability.enableExport == true) {
                        name += DiamondSymbol.ToString() + " ";
                    }


                //If the ability is a child to a hiearchy parent then show branched symbol
                if (ability.parentAbilityID != 0 && this.Abilities.Where(a => a.abilityID == ability.parentAbilityID).FirstOrDefault() != null && (this.Abilities.Where(a => a.abilityID == ability.parentAbilityID).FirstOrDefault().showChildrenInInspector == true || isDraggable == true)) {
                        name += BranchedSymbol.ToString() + " ";
                    }

                //If the ability is a parent to any hiearchy abilities then show small or right arrow depending on if show children is true
                if (this.Abilities.Where(a => a.parentAbilityID == ability.abilityID).Count() > 0) {
                        if (ability.showChildrenInInspector == true) {
                            name += SmallDownArrowSymbol.ToString() + " ";
                        } else {
                            name += SmallRightArrowSymbol.ToString() + " ";
                        }
                    }




                    if (ability.globalAbilities != null)
                        name = name + ability.globalAbilities.name;
                    else
                        name = name + ability.name;

                    rect.y += 2;


                    EditorGUI.PrefixLabel(
                        new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight),
                        1, new GUIContent(name));
                }
            };


        }




        //  ****************************************** Setup Re-Ordablelists and define abilityController************************************************************


        public void Setup(SerializedProperty AbilityList = null) {
            //If we have controller then setup abilities 
            if (abilityCont != null) {

                //Retrieve the main serialized object. This is the main property which is updated to retrieve current state, fields changed and then modifications applied back to the real object
                GetTarget = new SerializedObject(abilityCont);

                if (EditorApplication.isPlaying) {
                    meAbilityList = GetTarget.FindProperty("_currentAbilities"); // if in game mode then get current abilities list
                } else {

                    meAbilityList = GetTarget.FindProperty("Abilities"); // Find the List in our script and create a refrence of it 
                }


            } else if (AbilityList != null) {
                meAbilityList = AbilityList;
            }


            CreateAbilityReorderableList();
            GetGlobalElements();
        }

        void OnFocus() {


            // get new target 
            GameObject sel = Selection.activeGameObject;

            abilitySearchString = "";
            previousAbilitySearchString = "";

            // get ABC from current target 
            if (sel != null && sel.GetComponent<ABC_Controller>() != null) {
                abilityCont = sel.GetComponent<ABC_Controller>();
                GUIContent titleContent = new GUIContent(sel.gameObject.name + "'s Ability Manager");
                GetWindow<ABC_ControllerAbility_EditorWindow>().titleContent = titleContent;

            }


            this.Setup();


        }




        public void OnGUI() {

            if (abilityCont != null) {

                this.GetAbilitySettings();

                //Apply the changes to our list if an update has been made
                //take current state of the SerializedObject, and updates the real object.
                if (GetTarget.hasModifiedProperties) {
                    GetTarget.ApplyModifiedProperties();
                }


            }




        }


        public void OnEnable() {
            //Collect any icons 
            toolbarABC = new GUIContent[] { new GUIContent(" General", Resources.Load("ABC-EditorIcons/General", typeof(Texture2D)) as Texture2D, " General Settings"),
        new GUIContent(" Position & Travel", Resources.Load("ABC-EditorIcons/Travel", typeof(Texture2D)) as Texture2D, "Position & Travel"),
        new GUIContent(" Collision & Impact", Resources.Load("ABC-EditorIcons/Collision", typeof(Texture2D)) as Texture2D, "Collider & Collision"),
        new GUIContent(" Aesthetic & Animation", Resources.Load("ABC-EditorIcons/Animation", typeof(Texture2D)) as Texture2D, "Aesthetic & Animation")};



            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            SortIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Sort");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");
            ImportBlueIcon = (Texture2D)Resources.Load("ABC-EditorIcons/ImportBlue");

        }

        //Target update and applymodifiedproperties are in the inspector update call to reduce lag. 
        public void OnInspectorUpdate() {


            if (abilityCont != null) {

                //Double check any list edits will get saved
                if (GUI.changed) {
                    if (abilityCont != null)
                        EditorUtility.SetDirty(abilityCont);
                    else
                        EditorUtility.SetDirty(globElement);
                }

                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTarget.UpdateIfRequiredOrScript();


                //Retrieve all properties of current ability
                this.GetProperties(currentAbility);


                //Will update values in editor at runtime
                if (abilityCont.updateEditorAtRunTime == true) {
                    Repaint();
                }
            }






        }
    }
}