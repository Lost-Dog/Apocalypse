using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Reflection;

namespace ABCToolkit {
    public class ABC_MovementController_EditorWindow : EditorWindow {

        public static void ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(ABC_MovementController_EditorWindow));
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

        public void InspectorParkourList(SerializedProperty list, string Header, ABC_MovementController.ParkourType ParkourType) {

            InspectorVerticalBox();

            SerializedProperty parkourVaultAnimations = list;

            Color originalTextsColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(Header, new GUILayoutOption[] { GUILayout.MinWidth(minimumAllWaySectionWidth - 50), GUILayout.Height(20) });
            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {

                var stateIndex = parkourVaultAnimations.arraySize;
                parkourVaultAnimations.InsertArrayElementAtIndex(stateIndex);
                SerializedProperty element = parkourVaultAnimations.GetArrayElementAtIndex(stateIndex);
                element.FindPropertyRelative("parkourAnimationSpeed").floatValue = 1;
                element.FindPropertyRelative("parkourAnimationVitalPercentagePoint").floatValue = 40;
                element.FindPropertyRelative("parkourType").intValue = (int)ParkourType;
                element.FindPropertyRelative("enableRootMotion").boolValue = false;
                element.FindPropertyRelative("enableRootMotionPercentage").floatValue = 70;
            }
            GUILayout.EndHorizontal();

            GUI.skin.button.normal.textColor = originalTextsColor;

            if (parkourVaultAnimations.arraySize > 0) {

                for (int i = 0; i < parkourVaultAnimations.arraySize; i++) {
                    SerializedProperty element = parkourVaultAnimations.GetArrayElementAtIndex(i);

                    if (EditorGUIUtility.isProSkin) {
                        GUI.color = inspectorSectionBoxProColor;
                    } else {
                        GUI.color = inspectorSectionBoxColor;
                    }


                    EditorGUILayout.BeginVertical("Box", GUILayout.MinWidth(minimumAllWaySectionWidth - 10));
                    


                    GUI.color = Color.white;

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    EditorGUIUtility.labelWidth = 100;
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("parkourTag"), new GUIContent("Tag"), GUILayout.MaxWidth(290));
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

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (element.FindPropertyRelative("enableRootMotion").boolValue == true){
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("enableRootMotionPercentage"), new GUIContent("Percentage (%)"), GUILayout.MaxWidth(200));
                    }
                    GUILayout.EndVertical();

                    ResetLabelWidth();

                    GUILayout.BeginVertical();
                    GUI.skin.button.normal.textColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(40))) {
                        parkourVaultAnimations.DeleteArrayElementAtIndex(i);
                        continue;
                    }
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

                }

            }

            GUI.skin.button.normal.textColor = originalTextsColor;

            EditorGUILayout.EndVertical();


        }

        ABC_GlobalPortal.GameType gameTypes;

        public void InspectorApplyGameType() {


            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }

            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            EditorGUILayout.LabelField("Apply Game Type");

            EditorGUILayout.BeginHorizontal();

            gameTypes = (ABC_GlobalPortal.GameType)EditorGUILayout.EnumPopup("", gameTypes, GUILayout.Width(90));

            if (GUILayout.Button("Update")) {

                movementManager.ConvertToGameType(gameTypes);

                if (movementManager != null)
                    EditorUtility.SetDirty(movementManager);

            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
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


        ABC_MovementController movementManager;
        SerializedObject GetTarget;
     



        public GUIContent[] toolbarABC;
        public string[] generalSettingsToolbar = new string[] { "General", "Jump & Gravity", "Parkour"};
        public string[] animationSettingsToolbar = new string[] { "Animation"};
        public string[] audioSettingsToolbar = new string[] { "Audio"};




        //  ****************************************** Setup Re-Ordablelists and define abilityController************************************************************

        void OnFocus() {

            // get new target 
            GameObject sel = Selection.activeGameObject;

            // get ABC from current target 
            if (sel != null && sel.GetComponent<ABC_MovementController>() != null) {
                movementManager = sel.GetComponent<ABC_MovementController>();

                // Create the instance of GUIContent to assign to the window. Gives the title "RBSettings" and the icon
                GUIContent titleContent = new GUIContent(sel.gameObject.name + "'s Movement Controller");
                GetWindow<ABC_MovementController_EditorWindow>().titleContent = titleContent;
            }

            //If we have controller then setup abilities 
            if (movementManager != null) {

                //Retrieve the main serialized object. This is the main property which is updated to retrieve current state, fields changed and then modifications applied back to the real object
                GetTarget = new SerializedObject(movementManager);

            }

        }




        void OnGUI() {


            if (movementManager != null) {

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
                movementManager.toolbarMovementManagerSelection = GUILayout.Toolbar(movementManager.toolbarMovementManagerSelection, toolbarABC);
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



                switch ((int)movementManager.toolbarMovementManagerSelection) {

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


                        movementManager.toolbarMovementManagerGeneralSettingsSelection = GUILayout.SelectionGrid(movementManager.toolbarMovementManagerGeneralSettingsSelection, generalSettingsToolbar, 1);

                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();


                        this.InspectorApplyGameType();


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

                        switch ((int)movementManager.toolbarMovementManagerGeneralSettingsSelection) {
                            case 0:

                                InspectorSectionHeader("General Settings");

                                #region AllWay

                                InspectorVerticalBox();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("allowMovement"), new GUIContent("Allow Movement"), GUILayout.MaxWidth(315));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("ABCIntegration"), new GUIContent("ABC Integration"), GUILayout.MaxWidth(315));
                                EditorGUILayout.EndHorizontal();
                                InspectorHelpBox("Will toggle movement for the entity. The ABC integration will ensure movement & gravity is toggled in sync with ABC.");

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("FPSMode"), new GUIContent("FPS Mode"), GUILayout.MaxWidth(315));
                   
                                EditorGUIUtility.labelWidth = 170;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableMovementDelay"), new GUIContent("Enable Movement Delay"), GUILayout.MaxWidth(240));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("disableMovementDelay"), new GUIContent("Disable Movement Delay"), GUILayout.MaxWidth(240));
                                EditorGUILayout.Space();
                                ResetLabelWidth();
                                EditorGUILayout.EndHorizontal();
                                InspectorHelpBox("FPS mode will ensure the entity always faces/rotates with the camera and performs front-facing animations.");

                                EditorGUILayout.EndVertical();


                                #endregion


                                InspectorSectionHeader("Movement & Rotation Settings");

                                #region SideBySide 

                                EditorGUILayout.BeginHorizontal();

                                #region Movement 

                                InspectorVerticalBox(true);

                                EditorGUIUtility.labelWidth = 80;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("moveForce"), new GUIContent("Move Speed"), GUILayout.MaxWidth(150));
#if ENABLE_INPUT_SYSTEM == true
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("moveButton"), new GUIContent("Move Button"), GUILayout.MaxWidth(150));
#endif
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("sprintForce"), new GUIContent("Sprint Speed"), GUILayout.MaxWidth(150));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableSprintToggle"), new GUIContent("Sprint Toggle"), GUILayout.MaxWidth(150));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("sprintKey"), new GUIContent("Sprint Key"), GUILayout.MaxWidth(150));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("sprintButton"), new GUIContent("Sprint Button"), GUILayout.MaxWidth(150));
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("walkToggleKey"), new GUIContent("Walk Key"), GUILayout.MaxWidth(150));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("walkToggleButton"), new GUIContent("Walk Button"), GUILayout.MaxWidth(160));
                                EditorGUILayout.EndHorizontal();

                                EditorGUIUtility.labelWidth = 160;          
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("toggleWalkFromLightInput"), new GUIContent("Enable Light Input Walking"));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("walkInIdleOnly"), new GUIContent("Restrict Walk To Idle Mode"));                                

                                ResetLabelWidth();

                                InspectorHelpBox("Configure the move/sprint speed, walking and button inputs.");

                                EditorGUILayout.BeginHorizontal();
                                EditorGUIUtility.labelWidth = 130;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("useAcceleratedMovement"), new GUIContent("Accelerate Movement"), GUILayout.MaxWidth(195));
                                EditorGUIUtility.labelWidth = 105;
                                if (GetTarget.FindProperty("useAcceleratedMovement").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("moveAcceleration"), new GUIContent("Accelerate Speed"), GUILayout.MaxWidth(140));
                                }
                                EditorGUILayout.EndHorizontal();

                                InspectorHelpBox("Movement will accelerate over time by the speed set.", false);                       
     
                                ResetLabelWidth();

                                EditorGUILayout.EndVertical();

                                #endregion


                                #region Rotation

                                InspectorVerticalBox(true);


                                EditorGUILayout.PropertyField(GetTarget.FindProperty("allowRotation"), new GUIContent("Enable Rotation"), GUILayout.MaxWidth(315));

                                if (GetTarget.FindProperty("allowRotation").boolValue == true) {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("rotationMoveSpeed"), new GUIContent("Rotation Speed"), GUILayout.MaxWidth(315));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("rotationDrag"), new GUIContent("Rotation Drag"), GUILayout.MaxWidth(315));
                                    EditorGUILayout.EndHorizontal();
      
                                }

                                EditorGUILayout.Space();

                                EditorGUIUtility.labelWidth = 270;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("rotationToggleWithMovement"), new GUIContent("Disable Rotation When Movement is Disabled"), GUILayout.MaxWidth(315));
                                InspectorHelpBox("If ticked then rotation will be enabled/disabled along with movement");

                                EditorGUIUtility.labelWidth = 240;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("rotationStopMovementLeeway"), new GUIContent("Stop Movement Allow Rotation Delay"), GUILayout.MaxWidth(335));
                                InspectorHelpBox("Will determine how long after stop movement was triggered that rotation can reoccur");

                                ResetLabelWidth();
                                EditorGUILayout.EndVertical();


                                #endregion

                                EditorGUILayout.EndHorizontal();

                                #endregion


                                InspectorSectionHeader("Lock On & Crosshair Settings");

                                #region SideBySide 

                                EditorGUILayout.BeginHorizontal();

                                #region LockOn 

                                InspectorVerticalBox(true);

                                EditorGUIUtility.labelWidth = 150;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("lockOnTarget"), new GUIContent("Current Lock On Target"), GUILayout.MaxWidth(315));
                                InspectorHelpBox("Once a lock on target is set then the entity will always face the target.");

                                EditorGUIUtility.labelWidth = 170;                                                                                                                                                                       
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("useLockOnMovement"), new GUIContent("Enable Lock On Movement"), GUILayout.MaxWidth(315));
                                InspectorHelpBox("Configure lock on speed and if lock on animation will be used (sideways/backwards/forwards animations).");

                                EditorGUIUtility.labelWidth = 190;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("tempLockOffKey"), new GUIContent("Temporarily Lock Off Key"), GUILayout.MaxWidth(315));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("tempLockOffButton"), new GUIContent("Temporarily Lock Off Button"), GUILayout.MaxWidth(315));                                
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("tempLockOffDuration"), new GUIContent("Temporarily Lock Off Duration"), GUILayout.MaxWidth(315));

                                ResetLabelWidth();
                                EditorGUILayout.Space();
                                EditorGUILayout.EndVertical();

                                #endregion


                                #region Crosshair

                                InspectorVerticalBox(true);

                                EditorGUIUtility.labelWidth = 170;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableCrosshairMovement"), new GUIContent("Enable Crosshair Movement"), GUILayout.MaxWidth(230));
                                ResetLabelWidth();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("crosshairModeKey"), new GUIContent("Crosshair Key"), GUILayout.MaxWidth(230));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("crosshairModeButton"), new GUIContent("Crosshair Button"), GUILayout.MaxWidth(230));
                                InspectorHelpBox("Key to hold to activate crosshair rotation which will ensure the entity always faces/rotates with the camera and performs front-facing animations");

                                EditorGUIUtility.labelWidth = 190;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("useCrosshairAnimation"), new GUIContent("Enable Crosshair Animation"), GUILayout.MaxWidth(315));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("tempCrossOffKey"), new GUIContent("Temporarily Cross Off Key"), GUILayout.MaxWidth(315));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("tempCrossOffButton"), new GUIContent("Temporarily Cross Off Button"), GUILayout.MaxWidth(315));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("tempCrossOffDuration"), new GUIContent("Temporarily Cross Off Duration"), GUILayout.MaxWidth(315));
                                ResetLabelWidth();

                                EditorGUILayout.EndVertical();


                                #endregion

                                EditorGUILayout.EndHorizontal();

                                #endregion


                                break;

                            case 1:

                                InspectorSectionHeader("Jump & Gravity Settings");

                                #region AllWay

                                InspectorVerticalBox();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("allowJumping"), new GUIContent("Enable Jumping"), GUILayout.MaxWidth(315));
                                EditorGUIUtility.labelWidth = 140;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("allowDoubleJump"), new GUIContent("Enable Double Jump"), GUILayout.MaxWidth(315));
                                ResetLabelWidth();
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                InspectorHelpBox("Will enable jump & double jump which can be triggered by pressing the jump button a second time in air.");

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("jumpButton"), new GUIContent("Jump Button"), GUILayout.MaxWidth(200));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("jumpForce"), new GUIContent("Jump Force"), GUILayout.MaxWidth(180));
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                InspectorHelpBox("The button to trigger a jump and the amount of force applied to the jump");

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("allowGravity"), new GUIContent("Enable Gravity"), GUILayout.MaxWidth(315));

                                InspectorHelpBox("If enabled then the Unity configured physics gravity will be applied to the entity.");



                                EditorGUILayout.EndVertical();


                                #endregion

                                break;

                            case 2:

                                InspectorSectionHeader("Parkour Settings");

                                #region AllWay

                                InspectorVerticalBox();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableParkour"), new GUIContent("Enable Parkour"), GUILayout.MaxWidth(315));

                                EditorGUIUtility.labelWidth = 120;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableParkourDebug"), new GUIContent("Enable Debug Lines"), GUILayout.MaxWidth(315));
                                EditorGUILayout.EndHorizontal();


                                ResetLabelWidth();
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("parkourRequireInputTrigger"), new GUIContent("Input Required"), GUILayout.MaxWidth(315));


                                if (GetTarget.FindProperty("parkourRequireInputTrigger").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("parkourInputType"), new GUIContent("Input Type"), GUILayout.MaxWidth(220));
                                }
                                EditorGUILayout.EndHorizontal();

                                if (GetTarget.FindProperty("parkourRequireInputTrigger").boolValue == true) {

                                    if (((string)GetTarget.FindProperty("parkourInputType").enumNames[GetTarget.FindProperty("parkourInputType").enumValueIndex]) == "Key") {

                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("parkourInputKey"), GUILayout.MaxWidth(250));

                                    } else {
                                        EditorGUIUtility.labelWidth = 120;
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("parkourInputButton"), GUILayout.MaxWidth(250));

                                    }
                                }


                                ResetLabelWidth();

                                ResetLabelWidth();

                                EditorGUILayout.EndVertical();


                                #endregion

                                if (GetTarget.FindProperty("enableParkour").boolValue == true) {

                                    InspectorSectionHeader("Vault Settings", "Add animations for vaulting over obstacles. For vaulting the Vital Point is the center point of the obstacle. The vital point percentage needs to be the % point of the animation where the entity will be at the center of the obstacle, for example the % point when the left hand grabs the fence before accelerating over with a push.");


                                    #region AllWay

                                    this.InspectorParkourList(GetTarget.FindProperty("parkourVaultAnimations"), "Add Vault Animation", ABC_MovementController.ParkourType.Vault);


                                    #endregion


                                    InspectorSectionHeader("Lift Up Settings");

                                    #region AllWay

                                    this.InspectorParkourList(GetTarget.FindProperty("parkourLiftUpAnimations"), "Add Lift Up Animation", ABC_MovementController.ParkourType.LiftUp);


                                    #endregion


                                    InspectorSectionHeader("Slide Settings");

                                    #region AllWay

                                    this.InspectorParkourList(GetTarget.FindProperty("parkourSlideAnimations"), "Add Slide Animation", ABC_MovementController.ParkourType.Slide);


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


                        movementManager.toolbarMovementManagerAnimationSettingsSelection = GUILayout.SelectionGrid(movementManager.toolbarMovementManagerAnimationSettingsSelection, animationSettingsToolbar, 1);




                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();



                        this.InspectorApplyGameType();


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

                        switch ((int)movementManager.toolbarMovementManagerAnimationSettingsSelection) {
                            case 0:

                                #region AllWay

                                InspectorVerticalBox();

                                InspectorHelpBox("The below parameters are used in conjunction with the Unity Animator.", false);
                                EditorGUIUtility.labelWidth = 150;

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("rootMotionMode"), new GUIContent("Root Motion Mode"), GUILayout.MaxWidth(315));

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("animationParameter"), new GUIContent("Movement"), GUILayout.MaxWidth(280));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("animationAcceleratedParameter"), new GUIContent("Accelerated Move"), GUILayout.MaxWidth(310));
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("jumpAniParameter"), new GUIContent("Jump"), GUILayout.MaxWidth(280));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("doubleJumpAniParameter"), new GUIContent("Double Jump"), GUILayout.MaxWidth(310));
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("fallAniParameter"), new GUIContent("Fall"), GUILayout.MaxWidth(280));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("walkAniParameter"), new GUIContent("Walk"), GUILayout.MaxWidth(310));
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();
                      
                                EditorGUILayout.Space();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("lockOnSideAniParameter"), new GUIContent("Lock On Side Step"), GUILayout.MaxWidth(280));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("lockOnForwardAniParameter"), new GUIContent("Lock On Forward Step"), GUILayout.MaxWidth(310));
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("crossHairSideAniParameter"), new GUIContent("Crosshair Side Step"), GUILayout.MaxWidth(280));
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("crossHairForwardAniParameter"), new GUIContent("Crosshair Forward Step"), GUILayout.MaxWidth(310));
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();
                           


                                EditorGUILayout.EndVertical();


                                #endregion

                                InspectorSectionHeader("Sprint & Landing Animations");

                                #region SideBySide 

                                EditorGUILayout.BeginHorizontal();

                                #region Sprint

                                InspectorVerticalBox(true);


                                EditorGUIUtility.labelWidth = 140;

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableSprintJumping"), new GUIContent("Sprint Jump Animation"), GUILayout.MaxWidth(315));
                                InspectorHelpBox("Enable a seperate jump animation to run when sprinting");

                                EditorGUIUtility.labelWidth = 140;

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("animateIntoSprint"), new GUIContent("Animate Into Sprint"), GUILayout.MaxWidth(315));

                                if (GetTarget.FindProperty("animateIntoSprint").boolValue == true) {

                                    EditorGUIUtility.labelWidth = 30;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("animateIntoSprintClip"), new GUIContent("Clip"), GUILayout.MaxWidth(435));
                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("animateIntoSprintClipSpeed"), new GUIContent("Clip Speed"), GUILayout.MaxWidth(285));
                                    EditorGUILayout.EndHorizontal();
             
                                    EditorGUIUtility.labelWidth = 170;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("animateIntoSprintGraphic"), new GUIContent("Animate Into Sprint Graphic"), GUILayout.MaxWidth(365));
                                }

                                InspectorHelpBox("Configure for an animation and graphic to run when first going into a sprint");

                                EditorGUIUtility.labelWidth = 140;

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("animateAfterSprint"), new GUIContent("Animate After Sprint"), GUILayout.MaxWidth(315));

                                if (GetTarget.FindProperty("animateAfterSprint").boolValue == true) {

                                    EditorGUIUtility.labelWidth = 30;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("animateAfterSprintClip"), new GUIContent("Clip"), GUILayout.MaxWidth(435));
                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("animateAfterSprintClipSpeed"), new GUIContent("Clip Speed"), GUILayout.MaxWidth(285));
                                    EditorGUILayout.EndHorizontal();
                                }

                                InspectorHelpBox("Configure for an animation to run when sprinting ends", false);

                                ResetLabelWidth();

                                EditorGUILayout.EndVertical();


                                #endregion


                                #region Landing

                                InspectorVerticalBox(true);


                                EditorGUIUtility.labelWidth = 180;

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableLandingAnimations"), new GUIContent("Enable Landing Animations"), GUILayout.MaxWidth(315));
                                InspectorHelpBox("Configure for animation clips to run during different landing scenarios", false);

                                if (GetTarget.FindProperty("enableLandingAnimations").boolValue == true) {

                                    EditorGUILayout.Space();
                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingClip"), new GUIContent("Land"), GUILayout.MaxWidth(435));
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingRunClip"), new GUIContent("Land Run"), GUILayout.MaxWidth(435));
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingRunClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactClip"), new GUIContent("Land Impact"), GUILayout.MaxWidth(435));
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUILayout.EndHorizontal();

            
                                    EditorGUILayout.Space();


                                    EditorGUIUtility.labelWidth = 180;

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactShakeCamera"), new GUIContent("Landing Impact Shake Camera"), GUILayout.MaxWidth(315));

                                    ResetLabelWidth();


                                    if (GetTarget.FindProperty("landingImpactShakeCamera").boolValue == true) {

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactShakeCameraDelay"), new GUIContent("Shake Delay"), GUILayout.MaxWidth(315));
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactShakeCameraDuration"), new GUIContent("Shake Duration"), GUILayout.MaxWidth(315));
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactShakeCameraAmount"), new GUIContent("Shake Amount"), GUILayout.MaxWidth(315));
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("landingImpactShakeCameraSpeed"), new GUIContent("Shake Speed"), GUILayout.MaxWidth(315));
                                        EditorGUILayout.EndHorizontal();

                                        InspectorHelpBox("Configure for a camera shake when the impact land animation is used", false);
                                    }




                                }



                                EditorGUILayout.EndVertical();


                                #endregion

                          


                               

                                EditorGUILayout.EndHorizontal();

                                #endregion


                                InspectorSectionHeader("Advanced Rotation Animations");

                                #region Advanced Rotation 

                                InspectorVerticalBox();

                                EditorGUIUtility.labelWidth = 210;


                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableMaxAcceleratedAnimation"), new GUIContent("Enable Accelerated Animation"), GUILayout.MaxWidth(315));

                                EditorGUIUtility.labelWidth = 220;
                                if (GetTarget.FindProperty("enableMaxAcceleratedAnimation").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("intervalFromMaxAccelerationToAnimation"), new GUIContent("Max Acceleration To Animation Delay"), GUILayout.MaxWidth(280));
                                }
                                EditorGUILayout.EndHorizontal();
                                InspectorHelpBox("When entity reaches max speed a new animation will play", false);


                                EditorGUIUtility.labelWidth = 200;                  

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableAdvancedRotation"), new GUIContent("Enable Advanced Rotation"), GUILayout.MaxWidth(315));

                                if (GetTarget.FindProperty("enableAdvancedRotation").boolValue == true) {
                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("acceleratedRotationClip"), new GUIContent("Accelerated"), GUILayout.MaxWidth(300));                                
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("acceleratedRotationClipMask"), new GUIContent("Mask"), GUILayout.MaxWidth(220));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("acceleratedRotationClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("acceleratedRotationStopPercentage"), new GUIContent("Stop Animation At (%)"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotation180Clip"), new GUIContent("180 Turn"), GUILayout.MaxWidth(300));
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotation180ClipMask"), new GUIContent("Mask"), GUILayout.MaxWidth(220));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotation180ClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotation180StopPercentage"), new GUIContent("Stop Animation At (%)"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationRightClip"), new GUIContent("Right Turn"), GUILayout.MaxWidth(300));
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationRightClipMask"), new GUIContent("Mask"), GUILayout.MaxWidth(220));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationRightClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationRightStopPercentage"), new GUIContent("Stop Animation At (%)"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationLeftClip"), new GUIContent("Left Turn"), GUILayout.MaxWidth(300));
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationLeftClipMask"), new GUIContent("Mask"), GUILayout.MaxWidth(220));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationLeftClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(100));
                                    EditorGUIUtility.labelWidth = 130;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("advancedRotationLeftStopPercentage"), new GUIContent("Stop Animation At (%)"), GUILayout.MaxWidth(180));
                                    EditorGUILayout.EndHorizontal();
                                }

                                InspectorHelpBox("If enabled, rotation animations will play when turning and will stop once the animation reaches the % provided.", false);



                                ResetLabelWidth();
                                EditorGUILayout.EndVertical();

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

                        #region Controls
                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(settingButtonsWidth));


                        #region Selected Group Controls

                        EditorGUILayout.BeginVertical("Box");

                        EditorGUILayout.Space();


                        movementManager.toolbarMovementManagerAudioSettingsSelection = GUILayout.SelectionGrid(movementManager.toolbarMovementManagerAudioSettingsSelection, audioSettingsToolbar, 1);




                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();



                        this.InspectorApplyGameType();

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

                        switch ((int)movementManager.toolbarMovementManagerAudioSettingsSelection) {
                            case 0:


                                InspectorSectionHeader("Jump, Land & Movement Audio");

                                #region SideBySide 

                                EditorGUILayout.BeginHorizontal();

                                #region Default Movement Audio

                                InspectorVerticalBox(true);

                                EditorGUIUtility.labelWidth = 160;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableMovementAudio"), new GUIContent("Enable Movement Audio"), GUILayout.MaxWidth(315));
                                ResetLabelWidth();

                                EditorGUILayout.Space();

                                if (GetTarget.FindProperty("enableMovementAudio").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("movementAudioVolume"), new GUIContent("Audio Volume"), GUILayout.MaxWidth(315));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("movementAudioDefault"), new GUIContent("Movement Audio"), GUILayout.MaxWidth(315));
                                }


                                EditorGUILayout.EndVertical();


                                #endregion

                                #region Jump/Land Audio

                                InspectorVerticalBox(true);


                                EditorGUIUtility.labelWidth = 140;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableJumpingAudio"), new GUIContent("Enable Jump Audio"), GUILayout.MaxWidth(315));
                                ResetLabelWidth();


                                if (GetTarget.FindProperty("enableJumpingAudio").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("jumpingAudio"), new GUIContent("Jump Audio"), GUILayout.MaxWidth(315));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("jumpingAudioVolume"), new GUIContent("Audio Volume"), GUILayout.MaxWidth(315));
                                }

                                InspectorHelpBox("Configure for an audio to play when jumping.");
                 

                                EditorGUIUtility.labelWidth = 140;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableLandingAudio"), new GUIContent("Enable Landing Audio"), GUILayout.MaxWidth(315));
                                ResetLabelWidth();
               

                                if (GetTarget.FindProperty("enableLandingAudio").boolValue == true) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingAudio"), new GUIContent("Landing Audio"), GUILayout.MaxWidth(315));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("landingAudioVolume"), new GUIContent("Audio Volume"), GUILayout.MaxWidth(315));
                                }

                                InspectorHelpBox("Configure for an audio to play when landing.");



                                ResetLabelWidth();
                                EditorGUILayout.Space();
                                EditorGUILayout.EndVertical();

                                #endregion




                                EditorGUILayout.EndHorizontal();

                                #endregion


                                InspectorSectionHeader("Movement Audio Tag Map", "Configure for different audio sounds to play when moving on tagged environments");

                                #region AllWay

                                InspectorVerticalBox();

                                SerializedProperty movementAudioTags = GetTarget.FindProperty("movementAudioTagMaps");

                                Color originalTextsColor = GUI.skin.button.normal.textColor;

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Box("Add Movement Audio Tag Map", new GUILayoutOption[] { GUILayout.MinWidth(minimumAllWaySectionWidth - 50), GUILayout.Height(20) });
                                GUI.color = Color.white;
                                GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                                if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {

                                    var stateIndex = movementAudioTags.arraySize;
                                    movementAudioTags.InsertArrayElementAtIndex(stateIndex);
                                }
                                GUILayout.EndHorizontal();

                                GUI.skin.button.normal.textColor = originalTextsColor;

                                if (movementAudioTags.arraySize > 0) {

                                    for (int i = 0; i < movementAudioTags.arraySize; i++) {
                                        SerializedProperty element = movementAudioTags.GetArrayElementAtIndex(i);
                                        InspectorVerticalBox();

                                        EditorGUILayout.BeginHorizontal();

                                        GUILayout.BeginVertical();
                                        EditorGUIUtility.labelWidth = 70;
                                        EditorGUILayout.PropertyField(element.FindPropertyRelative("StepTag"), GUILayout.MaxWidth(290));
                                        ResetLabelWidth();
                                        GUILayout.EndVertical();

                                        GUILayout.BeginVertical();
                                        InspectorListBox("Movement Audio", element.FindPropertyRelative("StepAudio"));
                                        GUILayout.EndVertical();



                                        GUILayout.BeginVertical();
                                        GUI.skin.button.normal.textColor = Color.red;
                                        if (GUILayout.Button("X", GUILayout.Width(40))) {
                                            movementAudioTags.DeleteArrayElementAtIndex(i);
                                            continue;
                                        }
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

                                    }

                                }

                                GUI.skin.button.normal.textColor = originalTextsColor;
                            
                                                              

                                EditorGUILayout.EndVertical();


                                #endregion




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

            toolbarABC = new GUIContent[] { new GUIContent("General", Resources.Load("ABC-EditorIcons/Settings", typeof(Texture2D)) as Texture2D, " Settings"),
            new GUIContent(" Animation", Resources.Load("ABC-EditorIcons/Settings", typeof(Texture2D)) as Texture2D, "General"),
            new GUIContent(" Audio", Resources.Load("ABC-EditorIcons/Settings", typeof(Texture2D)) as Texture2D, "General")};

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

            if (movementManager != null) {

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(movementManager);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTarget.UpdateIfRequiredOrScript();


                ////Will update values in editor at runtime
                //if (movementManager.updateEditorAtRunTime == true) {
                //    Repaint();
                //}
            }

        }

    }
}