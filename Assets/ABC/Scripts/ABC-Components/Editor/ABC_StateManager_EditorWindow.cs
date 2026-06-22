using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Reflection;

namespace ABCToolkit {
    public class ABC_StateManager_EditorWindow : EditorWindow {

        public static void ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(ABC_StateManager_EditorWindow));
            window.maxSize = new Vector2(windowWidth, windowHeight);
            window.minSize = window.maxSize;
        }



        #region Window Sizes

        static float windowHeight = 660f;
        static float windowWidth = 837f;

        //Width of first column in left part of main body 
        int settingButtonsWidth = 170;

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


        // ************************* Inspector Design Functions ***********************************

        #region Design Functions

        public void InspectorHeader(string text, bool space = true) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            if (space == true) {
                EditorGUILayout.Space();
            }

            GUIStyle myStyle = new GUIStyle("Box");
            if (EditorGUIUtility.isProSkin) {
                myStyle.normal.textColor = inspectorSectionHeaderTextProColor;
            } else {
                myStyle.normal.textColor = inspectorSectionHeaderTextColor;
            }
            myStyle.alignment = TextAnchor.MiddleLeft;
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.fontSize = 11;
            myStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHeaderProColor;
            } else {
                GUI.color = inspectorSectionHeaderColor;
            }
            GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(21) });
            GUI.color = Color.white;
            GUI.skin.box.normal.textColor = originalTextColor;
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
            GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { GUILayout.MaxWidth(minimumAllWaySectionWidth) });

            GUI.color = Color.grey;
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

        public void InspectorHelpBox(string text, bool space = true, bool alwaysShow = false) {
            if (stateManager.showHelpInformation == true || alwaysShow == true) {

                GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                myStyle.richText = true;
                myStyle.wordWrap = true;
                myStyle.fixedWidth = 0;

                if (EditorGUIUtility.isProSkin) {
                    GUI.color = inspectorSectionHelpProColor;
                } else {
                    GUI.color = inspectorSectionHelpColor;
                }
                EditorGUILayout.LabelField(text, myStyle, GUILayout.MaxWidth(minimumAllWaySectionWidth));

                if (space == true) {
                    EditorGUILayout.Space();
                }
            } else {

                EditorGUILayout.Space();

            }
            GUI.color = Color.white;



        }

        public void InspectorBoldVerticleLine() {
            GUI.color = Color.white;
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.Width(1f), GUILayout.ExpandHeight(true) });
            GUI.color = Color.white;


        }

        public void ResetLabelWidth() {

            EditorGUIUtility.labelWidth = 110;

        }


        public void InspectorListBox(string title, SerializedProperty list, bool expandWidth = false) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            if (expandWidth) {
                EditorGUILayout.BeginVertical();
            } else {
                EditorGUILayout.BeginVertical(GUILayout.Width(300));
            }

            GUI.color = new Color32(208, 212, 211, 255);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(title, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(21) });
            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {
                list.InsertArrayElementAtIndex(list.arraySize);

                if (list.isArray && list.arrayElementType == "string")
                    list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = "";
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

        public void InspectorPropertyBox(string header, SerializedProperty list, int listIndex, bool includeUpDown = false) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginVertical();
            GUI.color = new Color32(208, 212, 211, 255);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(header, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(21) });
            GUI.color = Color.white;

            if (includeUpDown == true) {
                GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                if (GUILayout.Button(UpArrowSymbol.ToString(), GUILayout.Width(90))) {
                    list.MoveArrayElement(listIndex, listIndex - 1);
                }

                if (GUILayout.Button(DownArrowSymbol.ToString(), GUILayout.Width(90))) {
                    list.MoveArrayElement(listIndex, listIndex + 1);
                }

            }

            GUI.skin.button.normal.textColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(30))) {
                list.DeleteArrayElementAtIndex(listIndex);
            }
            GUILayout.EndHorizontal();

            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = originalTextColor;
            EditorGUILayout.Space();
        }


        // symbols used for aesthetics only
        char UpArrowSymbol = '\u2191';
        char DownArrowSymbol = '\u2193';
        // used to space out button text 

        // Button Icons
        Texture AddIcon;
        Texture RemoveIcon;
        Texture ExportIcon;
        Texture CopyIcon;
        Texture ImportIcon;

        Vector2 editorScrollPos;
        Vector2 listScrollPos;

        GUIStyle textureButton = new GUIStyle();

        #endregion


        ABC_StateManager stateManager;
        SerializedObject GetTarget;
        SerializedProperty meHealthList;
        SerializedProperty meTargeterLimitationList;
        SerializedProperty meEntityStats;
        SerializedProperty meHitAnimations;



        public GUIContent[] toolbarABC;




        public string[] generalSettingsToolbar = new string[] { "General & Stats", "Health", "Hit Animations", "Targeter Limitations" };


        public string[] effectWatcherToolbar = new string[] { "Active Effects", "Effect History" };




        //  ****************************************** Setup Re-Ordablelists and define abilityController************************************************************

        void OnFocus() {

            // get new target 
            GameObject sel = Selection.activeGameObject;

            // get ABC from current target 
            if (sel != null && sel.GetComponent<ABC_StateManager>() != null) {
                stateManager = sel.GetComponent<ABC_StateManager>();

                // Create the instance of GUIContent to assign to the window. Gives the title "RBSettings" and the icon
                GUIContent titleContent = new GUIContent(sel.gameObject.name + "'s State Manager");
                GetWindow<ABC_StateManager_EditorWindow>().titleContent = titleContent;
            }

            //If we have controller then setup abilities 
            if (stateManager != null) {



                //Retrieve the main serialized object. This is the main property which is updated to retrieve current state, fields changed and then modifications applied back to the real object
                GetTarget = new SerializedObject(stateManager);
                meHealthList = GetTarget.FindProperty("HealthGUIList"); // list of health GUI
                meTargeterLimitationList = GetTarget.FindProperty("TargeterLimitations"); // list of targeter limits
                meEntityStats = GetTarget.FindProperty("EntityStatList"); // list of entity stats
                meHitAnimations = GetTarget.FindProperty("HitAnimations"); // list of hit animations

            }

        }




        void OnGUI() {


            if (stateManager != null) {

                #region setup



                // formats for UI
                GUI.skin.button.wordWrap = true;
                GUI.skin.label.wordWrap = true;
                EditorStyles.textField.wordWrap = true;
                EditorGUIUtility.labelWidth = 110;
                EditorGUIUtility.fieldWidth = 35;



                EditorGUILayout.Space();

                #endregion

                #region Top Bar

                EditorGUILayout.BeginHorizontal();
                //GUILayout.Label(Resources.Load("ABC-EditorIcons/logo", typeof(Texture2D)) as Texture2D, GUILayout.MaxWidth(4));
                stateManager.toolbarStateManagerSelection = GUILayout.Toolbar(stateManager.toolbarStateManagerSelection, toolbarABC);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();


                #endregion


                // *************************************** Abilities start here


                #region Body

                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }



                switch ((int)stateManager.toolbarStateManagerSelection) {

                    case 0:

                        EditorGUILayout.BeginHorizontal();

                        #region Controls



                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(settingButtonsWidth));


                        #region Selected Group Controls


                        if (EditorGUIUtility.isProSkin) {
                            GUI.color = inspectorSectionBoxProColor;
                        } else {
                            GUI.color = inspectorSectionBoxColor;
                        }

                        EditorGUILayout.BeginVertical("Box");

                        GUI.color = Color.white;

                        EditorGUILayout.Space();


                        stateManager.toolbarStateManagerGeneralSettingsSelection = GUILayout.SelectionGrid(stateManager.toolbarStateManagerGeneralSettingsSelection, generalSettingsToolbar, 1);




                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        #endregion


                        EditorGUILayout.EndVertical();

                        #endregion

                        InspectorBoldVerticleLine();

                        #region Settings



                        editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                            false,
                                                                            false);

                        EditorGUILayout.BeginVertical();

                        #region General Settings

                        switch ((int)stateManager.toolbarStateManagerGeneralSettingsSelection) {
                            case 0:


                                InspectorSectionHeader("General Settings & Logging");

                                #region SideBySide 


                                EditorGUILayout.BeginHorizontal();



                                #region ABC Tags


                                InspectorVerticalBox(true);

                                InspectorHelpBox("Add ABC Tags below, tags are an alternative tagging system which is identified by all of ABC.");


                                InspectorListBox("ABC Tags:", GetTarget.FindProperty("ABCTag"));

                                EditorGUIUtility.labelWidth = 240;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("ignoreAbilityCollision"), new GUIContent("Ignore All Ability Collisions"));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("blockPushEffects"));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("blockDuplicateEffectActivation"));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("blockSurroundingObjectStatus"));


                                EditorGUILayout.Space();



                                EditorGUILayout.PropertyField(GetTarget.FindProperty("hitsStopMovement"), new GUIContent("Hits Can Stop Movement"));

                                if (GetTarget.FindProperty("hitsStopMovement").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("hitStopMovementDuration"), new GUIContent("Stop Movement Duration"), GUILayout.Width(280));

                                    ResetLabelWidth();
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("hitStopMovementFreezePosition"), new GUIContent("Freeze Position"));
                                    EditorGUIUtility.labelWidth = 140;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("hitStopMovementDisableComponents"), new GUIContent("Disable Components"));
                                    EditorGUILayout.EndHorizontal();
                                }

                                ResetLabelWidth();
                                EditorGUILayout.Space();

                                EditorGUILayout.EndVertical();

                                #endregion

                                #region General Settings


                                InspectorVerticalBox(true);



                                ResetLabelWidth();

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enablePooling"), new GUIContent("Enable Pooling"));
                                InspectorHelpBox("If ticked then graphics and objects will pool automatically", false);

                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("effectLogGUIText"), new GUIContent("GUI Log"));

                                if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {
                                    if (GameObject.Find("ABC_GUIs") == null) {
                                        Instantiate(Resources.Load("ABC-GUIs/ABC_GUIs")).name = "ABC_GUIs";
                                        EditorUtility.DisplayDialog("Creating ABC_GUIs", "ABC_GUIs will be added to your game. This holds all the default GUI objects used by ABC", "Ok");
                                    }

                                    Text txt = GameObject.Find("ABC_GUIs").GetComponentsInChildren<Text>(true).Where(i => i.name == "TextEffectLog").FirstOrDefault();

                                    if (txt != null) {
                                        GetTarget.FindProperty("effectLogGUIText").FindPropertyRelative("refVal").objectReferenceValue = txt;
                                        GetTarget.FindProperty("effectLogGUIText").FindPropertyRelative("refName").stringValue = txt.name;
                                    }
                                }

                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();
                                if (stateManager.effectLogGUIText.text != null) {


                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("effectLogUseDuration"), new GUIContent("Enable Duration"));
                                    ResetLabelWidth();
                                    if (stateManager.effectLogUseDuration == true) {
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("effectLogDuration"), new GUIContent("Log Duration"));

                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("effectLogMaxLines"), new GUIContent("Max Logging Lines"), GUILayout.Width(200));
                                }

                                ResetLabelWidth();

                                InspectorHelpBox("GUI Log where effect information can be displayed");


                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("effectGraphicTextGUI"), new GUIContent("Graphic Log"));
                                if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {

                                    GetTarget.FindProperty("effectGraphicTextGUI").FindPropertyRelative("refVal").objectReferenceValue = (Object)Resources.Load("ABC-EffectTextGUI/ABC_EffectTextCanvas");
                                    GetTarget.FindProperty("effectGraphicTextGUI").FindPropertyRelative("refName").stringValue = ((Object)Resources.Load("ABC-EffectTextGUI/ABC_EffectTextCanvas")).name;
                                }

                                EditorGUILayout.EndHorizontal();

                                if (GetTarget.FindProperty("effectGraphicTextGUI").FindPropertyRelative("refVal").objectReferenceValue != null) {

                                    EditorGUIUtility.labelWidth = 160;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("effectGraphicTextRandomise"), new GUIContent("Random Position & Scale"));
                                    ResetLabelWidth();
                                    EditorGUILayout.Space();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(100));

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("effectGraphicTextOffset"), new GUIContent(""), GUILayout.MaxWidth(500));
                                    EditorGUILayout.EndHorizontal();

                                    ResetLabelWidth();
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("effectGraphicTextForwardOffset"), new GUIContent("Forward Offset"));
                                    EditorGUIUtility.labelWidth = 90;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("effectGraphicTextRightOffset"), new GUIContent("Right Offset"));
                                    ResetLabelWidth();
                                    EditorGUILayout.EndHorizontal();
                                }


                                InspectorHelpBox("Text Canvas which can display effect information in game. e.g damage bubbles");

                                ResetLabelWidth();




                                EditorGUILayout.EndVertical();

                                #endregion

                                EditorGUILayout.EndHorizontal();

                                #endregion


                                InspectorSectionHeader("Entity Stats");

                                InspectorHelpBox("Stats can be used by effects to change the outcome. Adding more power or decreasing the damage due to defence etc");

                                if (GUILayout.Button(new GUIContent(" Add Stat", AddIcon, "Add New Stat"))) {
                                    // add standard defaults here
                                    stateManager.EntityStatList.Add(new ABC_StateManager.EntityStat());

                                }

                                for (int i = 0; i < meEntityStats.arraySize; i++) {

                                    #region AllWay 

                                    #region Entity Stats Settings 



                                    SerializedProperty MyEntityStatListRef = meEntityStats.GetArrayElementAtIndex(i);
                                    SerializedProperty foldOut = MyEntityStatListRef.FindPropertyRelative("foldOut");

                                    InspectorVerticalBox();

                                    EditorGUILayout.BeginVertical();
                                    GUI.color = new Color32(208, 212, 211, 255);
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginHorizontal();
                                    GUI.color = Color.white;

                                    EditorGUILayout.PropertyField(MyEntityStatListRef.FindPropertyRelative("statName"));
                                    EditorGUILayout.PropertyField(MyEntityStatListRef.FindPropertyRelative("statValue"));
                                    Color originalTextColor = GUI.skin.button.normal.textColor;

                                    GUI.skin.button.normal.textColor = Color.red;
                                    if (GUILayout.Button("X", GUILayout.Width(30))) {
                                        meEntityStats.DeleteArrayElementAtIndex(i);
                                    }
                                    GUILayout.EndHorizontal();

                                    GUI.color = Color.white;
                                    GUI.skin.button.normal.textColor = originalTextColor;

                                    if (meEntityStats.arraySize == 0 || i > meEntityStats.arraySize - 1) {
                                        break;
                                    }

                                    foldOut.boolValue = EditorGUILayout.Foldout(foldOut.boolValue, "More Settings");

                                    if (foldOut.boolValue == true) {

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(MyEntityStatListRef.FindPropertyRelative("textStatName"));
                                        EditorGUILayout.PropertyField(MyEntityStatListRef.FindPropertyRelative("textStatValue"));
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUIUtility.labelWidth = 220;
                                        EditorGUILayout.PropertyField(MyEntityStatListRef.FindPropertyRelative("onlyShowTextWhenSelected"));
                                        ResetLabelWidth();

                                    }



                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion
                                }


                                break;
                            case 1:

                                InspectorSectionHeader("Health Settings");

                                #region AllWay 

                                #region Health Settings

                                InspectorVerticalBox();

                                EditorGUIUtility.labelWidth = 175;

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("healthIntergrationType"), GUILayout.Width(320));
                                InspectorHelpBox("The type of health system to use - integrations with other assets can be selected here.");

                                EditorGUIUtility.labelWidth = 125;

                                EditorGUILayout.BeginHorizontal();

                                if (((string)GetTarget.FindProperty("healthIntergrationType").enumNames[GetTarget.FindProperty("healthIntergrationType").enumValueIndex]) == "ABC") {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("maxHealth"), GUILayout.Width(200));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("healthABC"), new GUIContent("Current Health"), GUILayout.Width(230));
                                } else if (((string)GetTarget.FindProperty("healthIntergrationType").enumNames[GetTarget.FindProperty("healthIntergrationType").enumValueIndex]) == "GameCreator" || ((string)GetTarget.FindProperty("healthIntergrationType").enumNames[GetTarget.FindProperty("healthIntergrationType").enumValueIndex]) == "GameCreator2") {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("gcHealthID"), new GUIContent("GC Health ID"), GUILayout.Width(230));
                                }
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("healthRegenPerSecond"), new GUIContent("Regen Per Second"), GUILayout.Width(200));
                                EditorGUILayout.EndHorizontal();

                                if (((string)GetTarget.FindProperty("healthIntergrationType").enumNames[GetTarget.FindProperty("healthIntergrationType").enumValueIndex]) == "GameCreator") {
                                    InspectorHelpBox("Game Creator Integration - If enabled then health value will be retrieved from GC Asset. To work correctly the health value needs to be added as a GC attribute not a stat");
                                }

                                EditorGUIUtility.labelWidth = 190;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("fullHealthOnEnable"));
                                EditorGUILayout.Space();


                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();
                                EditorGUIUtility.labelWidth = 190;
                                EditorGUILayout.Space();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("stopMovementOnZeroHealth"), new GUIContent("Stop Movement On 0 Health"), GUILayout.Width(200));

                                if (GetTarget.FindProperty("stopMovementOnZeroHealth").boolValue == true) {
                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("stopMovementOnZeroHealthFreezePosition"), new GUIContent("Freeze Position"));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("stopMovementOnZeroHealthDisableComponents"), new GUIContent("Disable Components"));
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();
                                EditorGUIUtility.labelWidth = 190;


                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("disableEntityOnZeroHealth"), new GUIContent("Disable On 0 Health"));
                                ResetLabelWidth();
                                EditorGUILayout.Space();
                                if (GetTarget.FindProperty("disableEntityOnZeroHealth").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("disableDelay"), GUILayout.Width(200));
                                }
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();
                                EditorGUIUtility.labelWidth = 190;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("swapModelOnZeroHealth"), new GUIContent("0 Health Swap Model"));
                                ResetLabelWidth();
                                if (GetTarget.FindProperty("swapModelOnZeroHealth").boolValue == true) {
                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("swapModelToDisable"), new GUIContent("Disable Model"));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("swapModelToEnable"), new GUIContent("Enable Model"));
                                    EditorGUILayout.EndHorizontal();
                                }


                                EditorGUILayout.EndVertical();

                                #endregion

                                #endregion


                                #region AllWay 

                                #region Zero Health Animations 

                                SerializedProperty disableAnimationAnimatorParameter = GetTarget.FindProperty("zeroHealthAnimationAnimatorParameter");
                                SerializedProperty disableAnimationAnimatorParameterType = GetTarget.FindProperty("zeroHealthAnimationAnimatorParameterType");
                                SerializedProperty disableAnimationAnimatorOnValue = GetTarget.FindProperty("zeroHealthAnimationAnimatorOnValue");
                                SerializedProperty disableAnimationAnimatorOffValue = GetTarget.FindProperty("zeroHealthAnimationAnimatorOffValue");
                                SerializedProperty disableAnimationAnimatorDuration = GetTarget.FindProperty("zeroHealthAnimationAnimatorDuration");

                                SerializedProperty zeroHealthAnimationRunnerClip = GetTarget.FindProperty("zeroHealthAnimationRunnerClip");
                                SerializedProperty zeroHealthAnimationRunnerMask = GetTarget.FindProperty("zeroHealthAnimationRunnerMask");
                                SerializedProperty zeroHealthAnimationRunnerClipSpeed = GetTarget.FindProperty("zeroHealthAnimationRunnerClipSpeed");
                                SerializedProperty zeroHealthAnimationRunnerClipDelay = GetTarget.FindProperty("zeroHealthAnimationRunnerClipDelay");
                                SerializedProperty zeroHealthAnimationRunnerClipDuration = GetTarget.FindProperty("zeroHealthAnimationRunnerClipDuration");


                                InspectorVerticalBox();


                                InspectorHelpBox("Animation to play when health reaches 0. Can either add an animation clip or use Unity's Animator", false);

                                EditorGUILayout.PropertyField(zeroHealthAnimationRunnerClip, new GUIContent("Animation Clip"), GUILayout.MaxWidth(315));

                                if (zeroHealthAnimationRunnerClip.FindPropertyRelative("refVal").objectReferenceValue != null) {
                                    InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");


                                    EditorGUILayout.PropertyField(zeroHealthAnimationRunnerMask, new GUIContent("Avatar Mask"), GUILayout.MaxWidth(315));

                                    EditorGUIUtility.labelWidth = 75;
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(zeroHealthAnimationRunnerClipDuration, new GUIContent("Duration"), GUILayout.MaxWidth(125));
                                    EditorGUILayout.PropertyField(zeroHealthAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                    EditorGUILayout.PropertyField(zeroHealthAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));
                                    EditorGUILayout.EndHorizontal();

                                }


                                ResetLabelWidth();

                                EditorGUILayout.Space();

                                EditorGUILayout.EndVertical();

                                InspectorVerticalBox();

                                EditorGUIUtility.labelWidth = 210;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(disableAnimationAnimatorParameter, new GUIContent("0 Health Animator Parameter"), GUILayout.MaxWidth(315));
                                EditorGUIUtility.labelWidth = 150;
                                if (disableAnimationAnimatorParameter.stringValue != "") {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(disableAnimationAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Space();

                                    if (((string)disableAnimationAnimatorParameterType.enumNames[disableAnimationAnimatorParameterType.enumValueIndex]) != "Trigger") {

                                        EditorGUILayout.PropertyField(disableAnimationAnimatorDuration, new GUIContent("Animation Duration"), GUILayout.MaxWidth(230));

                                        EditorGUILayout.BeginHorizontal();

                                        // if not trigger we need to know the value to switch on and off
                                        EditorGUILayout.PropertyField(disableAnimationAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(disableAnimationAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();


                                    }
                                } else {
                                    EditorGUILayout.EndHorizontal();
                                }

                                ResetLabelWidth();

                                EditorGUILayout.Space();


                                EditorGUILayout.EndVertical();

                                #endregion

                                #endregion

                                #region AllWay 

                                #region Health Reduction Image



                                InspectorVerticalBox();



                                InspectorHelpBox("GUI Image which will appear for a duration when health is reduced", false);
                                EditorGUIUtility.labelWidth = 220;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("showGUIImageOnHealthReduction"), new GUIContent("Show Image On Health Reduction"));
                                ResetLabelWidth();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUIUtility.labelWidth = 160;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("imageOnHealthReduction"), new GUIContent("Health Reduction Image"));
                                ResetLabelWidth();
                                if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {
                                    if (GameObject.Find("ABC_GUIs") == null) {
                                        Instantiate(Resources.Load("ABC-GUIs/ABC_GUIs")).name = "ABC_GUIs";
                                        EditorUtility.DisplayDialog("Creating ABC_GUIs", "ABC_GUIs will be added to your game. This holds all the default GUI objects used by ABC", "Ok");
                                    }

                                    RawImage texture = GameObject.Find("ABC_GUIs").GetComponentsInChildren<RawImage>(true).Where(i => i.name == "HealthReductionImage").FirstOrDefault();

                                    if (texture != null) {
                                        GetTarget.FindProperty("imageOnHealthReduction").FindPropertyRelative("refVal").objectReferenceValue = texture;
                                        GetTarget.FindProperty("imageOnHealthReduction").FindPropertyRelative("refName").stringValue = texture.name;
                                    }
                                }

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("imageOnHealthReductionDuration"), new GUIContent("Image Duration"), GUILayout.Width(220));

                                EditorGUILayout.EndHorizontal();




                                EditorGUILayout.Space();
                                EditorGUILayout.EndVertical();

                                #endregion

                                #endregion

                                InspectorSectionHeader("Health GUI");

                                if (GUILayout.Button(new GUIContent(" Add Health GUI", AddIcon, "Add New Health GUI"))) {
                                    // add standard defaults here
                                    stateManager.HealthGUIList.Add(new ABC_StateManager.HealthGUI());

                                }

                                InspectorHelpBox("GUIText will show remaining/max details and the slider will show graphically how much health remains. If you want to update the same Health GUI over different entities then make them share the same slider/text making sure that 'Show When Selected' is ticked for both. ", false);


                                for (int i = 0; i < meHealthList.arraySize; i++) {

                                    #region AllWay 

                                    #region Health GUI Settings 


                                    InspectorVerticalBox();

                                    InspectorPropertyBox("Health GUI", meHealthList, i);


                                    if (meHealthList.arraySize == 0 || i > meHealthList.arraySize - 1) {
                                        break;
                                    }

                                    SerializedProperty MyHealthListRef = meHealthList.GetArrayElementAtIndex(i);


                                    EditorGUIUtility.labelWidth = 170;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("healthSlider"));

                                    if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {
                                        if (GameObject.Find("ABC_GUIs") == null) {
                                            Instantiate(Resources.Load("ABC-GUIs/ABC_GUIs")).name = "ABC_GUIs";
                                            EditorUtility.DisplayDialog("Creating ABC_GUIs", "ABC_GUIs will be added to your game. This holds all the default GUI objects used by ABC", "Ok");
                                        }

                                        Slider slider = GameObject.Find("ABC_GUIs").GetComponentsInChildren<Slider>(true).Where(n => n.name == "SliderHealth").FirstOrDefault();

                                        if (slider != null) {
                                            MyHealthListRef.FindPropertyRelative("healthSlider").FindPropertyRelative("refVal").objectReferenceValue = slider;
                                            MyHealthListRef.FindPropertyRelative("healthSlider").FindPropertyRelative("refName").stringValue = slider.name;
                                        }

                                    }

                                    EditorGUILayout.EndHorizontal();



                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("healthOverTimeSlider"));

                                    if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {
                                        if (GameObject.Find("ABC_GUIs") == null) {
                                            Instantiate(Resources.Load("ABC-GUIs/ABC_GUIs")).name = "ABC_GUIs";
                                            EditorUtility.DisplayDialog("Creating ABC_GUIs", "ABC_GUIs will be added to your game. This holds all the default GUI objects used by ABC", "Ok");
                                        }

                                        Slider slider = GameObject.Find("ABC_GUIs").GetComponentsInChildren<Slider>(true).Where(n => n.name == "SliderHealthOverTime").FirstOrDefault();

                                        if (slider != null) {
                                            MyHealthListRef.FindPropertyRelative("healthOverTimeSlider").FindPropertyRelative("refVal").objectReferenceValue = slider;
                                            MyHealthListRef.FindPropertyRelative("healthOverTimeSlider").FindPropertyRelative("refName").stringValue = slider.name;
                                        }

                                    }

                                    EditorGUILayout.EndHorizontal();



                                    if (MyHealthListRef.FindPropertyRelative("healthOverTimeSlider").FindPropertyRelative("refVal").objectReferenceValue != null) {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("healthOverTimeSliderUpdateDelay"), new GUIContent("Update Over Time Delay"), GUILayout.Width(220));
                                        EditorGUIUtility.labelWidth = 185;
                                        EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("healthOverTimeSliderUpdateDuration"), new GUIContent("Update Over Time Duration"), GUILayout.Width(220));
                                        EditorGUILayout.EndHorizontal();
                                    }

                                    EditorGUIUtility.labelWidth = 230;
                                    EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("onlyShowSliderWhenSelected"), new GUIContent("Only Show Sliders When Selected"));
                                    ResetLabelWidth();
                                    EditorGUILayout.Space();

                                    EditorGUIUtility.labelWidth = 170;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("healthText"));


                                    if (GUILayout.Button(new GUIContent(ImportIcon, "Load Default"), textureButton, GUILayout.Width(20)) && EditorUtility.DisplayDialog("Load Default", "Loading defaults will overwrite the current property value. Are you sure you want to continue? ", "Yes", "No")) {
                                        if (GameObject.Find("ABC_GUIs") == null) {
                                            Instantiate(Resources.Load("ABC-GUIs/ABC_GUIs")).name = "ABC_GUIs";
                                            EditorUtility.DisplayDialog("Creating ABC_GUIs", "ABC_GUIs will be added to your game. This holds all the default GUI objects used by ABC", "Ok");
                                        }

                                        Text txt = GameObject.Find("ABC_GUIs").GetComponentsInChildren<Text>(true).Where(n => n.name == "TextHealthValue").FirstOrDefault();

                                        if (txt != null) {
                                            MyHealthListRef.FindPropertyRelative("healthText").FindPropertyRelative("refVal").objectReferenceValue = txt;
                                            MyHealthListRef.FindPropertyRelative("healthText").FindPropertyRelative("refName").stringValue = txt.name;
                                        }

                                    }

                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 230;
                                    EditorGUILayout.PropertyField(MyHealthListRef.FindPropertyRelative("onlyShowTextWhenSelected"));
                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion
                                }

                                break;


                            case 2:

                                InspectorSectionHeader("Hit Animations");

                                #region AllWay 

                                #region Hit Animation Settings 

                                InspectorVerticalBox();

                                InspectorHelpBox("Hit animations setup for the entity. Hit animations will activate from either an ability hit or due to an effect.  The animation which plays depends on the list order. The list will be cycled through until a dice roll matches, animations at the top will be checked first");

                                SerializedProperty showAbilitiesInGroup = GetTarget.FindProperty("randomizeHitAnimations");
                                SerializedProperty activateHitAnimationsFromAbilityHit = GetTarget.FindProperty("activateHitAnimationsFromAbilityHit");
                                SerializedProperty activateHitAnimationsFromAbilityEffect = GetTarget.FindProperty("activateHitAnimationsFromAbilityEffect");

                                EditorGUIUtility.labelWidth = 150;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(activateHitAnimationsFromAbilityHit, new GUIContent("Activate from Hit"));
                                EditorGUILayout.PropertyField(activateHitAnimationsFromAbilityEffect, new GUIContent("Activate from Effect"));
                                EditorGUILayout.EndHorizontal();
                                InspectorHelpBox("Above settings determines when hit animations will activate");

                                EditorGUILayout.PropertyField(showAbilitiesInGroup, new GUIContent("Randomize Animations"));
                                InspectorHelpBox("Will shuffle the order of the animations in the list before checking through them");

                                ResetLabelWidth();
                                EditorGUILayout.Space();


                                EditorGUILayout.EndVertical();

                                #endregion

                                #endregion


                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button(new GUIContent(" Add Hit Animation", AddIcon, "Add New Hit Animation"))) {
                                    // add standard defaults here
                                    stateManager.HitAnimations.Add(new ABC_StateManager.HitAnimation(meHitAnimations.arraySize));

                                }
                                EditorGUILayout.EndHorizontal();

                                for (int n = 0; n < meHitAnimations.arraySize; n++) {

                                    #region AllWay 

                                    #region Hit Animations 




                                    InspectorVerticalBox();

                                    InspectorPropertyBox("Hit Animation", meHitAnimations, n, true);

                                    if (meHitAnimations.arraySize == 0 || n > meHitAnimations.arraySize - 1) {
                                        break;
                                    }

                                    SerializedProperty MyHitAnimationRef = meHitAnimations.GetArrayElementAtIndex(n);

                                    SerializedProperty foldOut = MyHitAnimationRef.FindPropertyRelative("foldOut");
                                    SerializedProperty hitAnimationName = MyHitAnimationRef.FindPropertyRelative("hitAnimationName");
                                    SerializedProperty hitAnimationEnabled = MyHitAnimationRef.FindPropertyRelative("hitAnimationEnabled");
                                    SerializedProperty hitAnimationActivateFromEffectOnly = MyHitAnimationRef.FindPropertyRelative("hitAnimationActivateFromEffectOnly");

                                    SerializedProperty hitAnimationProbabilityMinValue = MyHitAnimationRef.FindPropertyRelative("hitAnimationProbabilityMinValue");
                                    SerializedProperty hitAnimationProbabilityMaxValue = MyHitAnimationRef.FindPropertyRelative("hitAnimationProbabilityMaxValue");

                                    SerializedProperty hitAnimationRunnerClips = MyHitAnimationRef.FindPropertyRelative("hitAnimationRunnerClips");
                                    SerializedProperty hitAnimationAirRunnerClips = MyHitAnimationRef.FindPropertyRelative("hitAnimationAirRunnerClips");
                                    SerializedProperty hitAnimationRunnerMask = MyHitAnimationRef.FindPropertyRelative("hitAnimationRunnerMask");
                                    SerializedProperty hitAnimationRunnerClipSpeed = MyHitAnimationRef.FindPropertyRelative("hitAnimationRunnerClipSpeed");
                                    SerializedProperty hitAnimationRunnerClipDelay = MyHitAnimationRef.FindPropertyRelative("hitAnimationRunnerClipDelay");
                                    SerializedProperty hitAnimationRunnerClipDuration = MyHitAnimationRef.FindPropertyRelative("hitAnimationRunnerClipDuration");
                                    SerializedProperty hitAnimationRunnerInterruptCurrentAnimation = MyHitAnimationRef.FindPropertyRelative("hitAnimationRunnerInterruptCurrentAnimation");

                                    SerializedProperty hitAnimationAnimatorParameter = MyHitAnimationRef.FindPropertyRelative("hitAnimationAnimatorParameter");
                                    SerializedProperty hitAnimationAnimatorParameterType = MyHitAnimationRef.FindPropertyRelative("hitAnimationAnimatorParameterType");
                                    SerializedProperty hitAnimationAnimatorOnValue = MyHitAnimationRef.FindPropertyRelative("hitAnimationAnimatorOnValue");
                                    SerializedProperty hitAnimationAnimatorOffValue = MyHitAnimationRef.FindPropertyRelative("hitAnimationAnimatorOffValue");
                                    SerializedProperty hitAnimationAnimatorDuration = MyHitAnimationRef.FindPropertyRelative("hitAnimationAnimatorDuration");



                                    EditorGUIUtility.labelWidth = 90;

                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUILayout.PropertyField(hitAnimationName, new GUIContent("Name"), GUILayout.MaxWidth(265));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(hitAnimationEnabled, new GUIContent("Enabled"));
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.Space();
                                    foldOut.boolValue = EditorGUILayout.Foldout(foldOut.boolValue, "More Settings");

                                    if (foldOut.boolValue == true) {
                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 270;
                                        EditorGUILayout.PropertyField(hitAnimationActivateFromEffectOnly, new GUIContent("Only Activate From Effect/Direct Activation"));
                                        ResetLabelWidth();
                                        InspectorHelpBox("If enabled then hit animation can only be activated from an effect or through direct activation and will never be activated randomly");

                                        EditorGUILayout.Space();

                                        EditorGUILayout.PropertyField(hitAnimationProbabilityMinValue, new GUIContent("Probability Min"));
                                        EditorGUILayout.PropertyField(hitAnimationProbabilityMaxValue, new GUIContent("Probability Max"));



                                        EditorGUILayout.Space();



                                        InspectorHelpBox("Animation clip to play, if more then one is added then 1 is selected randomly each time");

                                        EditorGUILayout.BeginHorizontal();

                                        InspectorListBox("Animation Clips", hitAnimationRunnerClips);
                                        InspectorListBox("Air Animation Clips", hitAnimationAirRunnerClips);

                                        EditorGUILayout.EndHorizontal();

                                        if (hitAnimationRunnerClips.arraySize > 0 || hitAnimationAirRunnerClips.arraySize > 0) {


                                            EditorGUILayout.PropertyField(hitAnimationRunnerMask, new GUIContent("Avatar Mask"), GUILayout.MaxWidth(315));
                                            EditorGUILayout.Space();

                                            EditorGUIUtility.labelWidth = 75;

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(hitAnimationRunnerClipDuration, new GUIContent("Duration"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.PropertyField(hitAnimationRunnerClipSpeed, new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.PropertyField(hitAnimationRunnerClipDelay, new GUIContent("Delay"), GUILayout.MaxWidth(125));
                                            EditorGUILayout.EndHorizontal();
                                        }

                                        InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");

                                        ResetLabelWidth();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 150;
                                        EditorGUILayout.PropertyField(hitAnimationAnimatorParameter, new GUIContent("Animator Parameter"), GUILayout.MaxWidth(315));
                                        if (hitAnimationAnimatorParameter.stringValue != "") {

                                            EditorGUILayout.PropertyField(hitAnimationAnimatorParameterType, new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));
                                            EditorGUILayout.EndHorizontal();
                                            if (((string)hitAnimationAnimatorParameterType.enumNames[hitAnimationAnimatorParameterType.enumValueIndex]) != "Trigger") {
                                                EditorGUILayout.BeginHorizontal();
                                                // if not trigger we need to know the value to switch on and off
                                                EditorGUILayout.PropertyField(hitAnimationAnimatorOnValue, new GUIContent("On Value"), GUILayout.MaxWidth(230));

                                                EditorGUILayout.PropertyField(hitAnimationAnimatorOffValue, new GUIContent("Off Value"), GUILayout.MaxWidth(230));
                                                EditorGUILayout.EndHorizontal();

                                                EditorGUILayout.PropertyField(hitAnimationAnimatorDuration, new GUIContent("Animation Duration"), GUILayout.MaxWidth(230));

                                            }
                                            InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values. Note: Animation will repeat until the duration is up");
                                        } else {
                                            EditorGUILayout.EndHorizontal();
                                        }

                                        ResetLabelWidth();
                                    }


                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();


                                    EditorGUILayout.EndVertical();

                                    #endregion

                                    #endregion
                                }


                                break;

                            case 3:

                                InspectorSectionHeader("Targeter Limitations");

                                if (GUILayout.Button(new GUIContent(" Add Targeter Limitatation ", AddIcon, "Add New Targeter Limitatation "))) {
                                    // add standard defaults here
                                    stateManager.TargeterLimitations.Add(new ABC_StateManager.TargeterLimitation());

                                }

                                InspectorHelpBox("Targeter Limits will put a restriction on the number of entities that can target this entity at once. Limits will only apply to the tags setup. Once the limit is reached noone else can target unless they have been given priority from being hit with attacks ", false);


                                for (int i = 0; i < meTargeterLimitationList.arraySize; i++) {

                                    #region AllWay 

                                    #region Targeter Limitation List Settings 


                                    InspectorVerticalBox();

                                    InspectorPropertyBox("Targeter Limitation", meTargeterLimitationList, i);


                                    if (meTargeterLimitationList.arraySize == 0 || i > meTargeterLimitationList.arraySize - 1) {
                                        break;
                                    }

                                    SerializedProperty MyTargeterLimitationListRef = meTargeterLimitationList.GetArrayElementAtIndex(i);


                                    EditorGUIUtility.labelWidth = 170;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(MyTargeterLimitationListRef.FindPropertyRelative("enableTargeterLimit"));
                                    EditorGUILayout.PropertyField(MyTargeterLimitationListRef.FindPropertyRelative("maxNumberOfTargeters"), GUILayout.MaxWidth(230));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.Space();
                                    InspectorHelpBox("Will reset the current targetters every interval, giving the chance for other entities to target this entity", false);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(MyTargeterLimitationListRef.FindPropertyRelative("enableCurrentTargeterResets"), new GUIContent("Enable Targetter Resets"));

                                    if (MyTargeterLimitationListRef.FindPropertyRelative("enableCurrentTargeterResets").boolValue == true) {
                                        EditorGUILayout.PropertyField(MyTargeterLimitationListRef.FindPropertyRelative("resetCurrentTargetersInterval"), new GUIContent("Reset Interval"), GUILayout.MaxWidth(230));
                                    }

                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.Space();

                                    InspectorListBox("Targeter Tags", MyTargeterLimitationListRef.FindPropertyRelative("targeterTags"), true);


                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();

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

                        break;


                    case 1:

                        EditorGUILayout.BeginHorizontal();

                        #region Controls
                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(settingButtonsWidth));


                        #region Selected Group Controls

                        EditorGUILayout.BeginVertical("Box");

                        EditorGUILayout.Space();


                        stateManager.toolbarStateManagerEffectWatcherSettingsSelection = GUILayout.SelectionGrid(stateManager.toolbarStateManagerEffectWatcherSettingsSelection, effectWatcherToolbar, 1);




                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        #endregion




                        EditorGUILayout.EndVertical();

                        #endregion

                        InspectorBoldVerticleLine();

                        #region Settings



                        editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                            false,
                                                                            false);

                        EditorGUILayout.BeginVertical();

                        #region General Settings


                        switch ((int)stateManager.toolbarStateManagerEffectWatcherSettingsSelection) {
                            case 0:

                                EditorGUIUtility.labelWidth = 140;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("effectProtection"));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("ignoreAbilityCollision"));
                                ResetLabelWidth();

                                if (GUILayout.Button(new GUIContent(" Remove All Active Effects", RemoveIcon, "Remove All Active Effects"))) {
                                    stateManager.RemoveAllActiveEffects();
                                }

                                EditorGUILayout.Space();


                                foreach (ABC_IEntity key in stateManager.ActiveEffects.Keys) {



                                    InspectorSectionHeader("Active Effects inflicted by " + key.gameObject.name);



                                    foreach (ABC_StateManager.ActiveEffect effect in stateManager.ActiveEffects[key]) {

                                        #region AllWay 

                                        #region Effect Watcher

                                        InspectorVerticalBox();

                                        EditorGUILayout.LabelField(effect.effect.effectName + " from " + effect.abilityName + " - duration of effect: " + effect.effect.effectDuration);

                                        EditorGUILayout.EndVertical();

                                        #endregion

                                        #endregion




                                    }

                                    EditorGUILayout.Space();

                                }




                                break;

                            case 1:


                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("editorRecordActiveEffectHistory"), new GUIContent("Record Effects"));


                                if (GUILayout.Button(new GUIContent(" Clear History Log", RemoveIcon, "Clear History Log"))) {
                                    stateManager.ClearActiveEffectHistory();
                                }

                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                InspectorSectionHeader("Active Effect History");



                                foreach (string item in stateManager.editorActiveEffectHistory) {
                                    #region AllWay 

                                    #region Effect Watcher


                                    InspectorVerticalBox();

                                    EditorGUILayout.LabelField(item);

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
                        break;


                }




                #endregion


                //Apply the changes to our list if an update has been made
                //take current state of the SerializedObject, and updates the real object.
                if (GetTarget.hasModifiedProperties) {
                    GetTarget.ApplyModifiedProperties();
                }



            }

        }


        public void OnEnable() {

            toolbarABC = new GUIContent[] { new GUIContent(" Settings", Resources.Load("ABC-EditorIcons/Settings", typeof(Texture2D)) as Texture2D, "Settings"),
        new GUIContent(" Effect Watcher", Resources.Load("ABC-EditorIcons/Binoculars", typeof(Texture2D)) as Texture2D, "Effect Watcher")};

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");

            //setup styles 
            textureButton.alignment = TextAnchor.MiddleCenter;

        }


        //Target update and applymodifiedproperties are in the inspector update call to reduce lag. 
        public void OnInspectorUpdate() {

            if (stateManager != null) {

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(stateManager);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTarget.UpdateIfRequiredOrScript();


                //Will update values in editor at runtime
                if (stateManager.updateEditorAtRunTime == true) {
                    Repaint();
                }
            }

        }

    }
}