using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Reflection;

namespace ABCToolkit {
    public class ABC_WeaponPickUp_EditorWindow : EditorWindow {

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
            GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(19) });
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
            if (weaponPickUpManager.showHelpInformation == true || alwaysShow == true) {

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
            GUILayout.Box(header, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(19) });
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


        ABC_WeaponPickUp weaponPickUpManager;
        SerializedObject GetTarget;

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

        public int toolbarABCSelection;
        //public string[] toolbarABC = new string[] { "Settings", "Target Settings", "Ability Groups", "AI" };
        public GUIContent[] toolbarABC;

        char StarSymbol = '\u2605';


        public int generalSettingsToolbarSelection;
        public string[] generalSettingsToolbar = new string[] { "General", "Animations", "UI" };

        Dictionary<ABC_GlobalElement, string> GlobalWeapons = new Dictionary<ABC_GlobalElement, string>();
        int globalWeaponsListChoice = 0;

        private void PopulateWeaponList() {

            if (weaponPickUpManager == null)
                return;

            if (this.GlobalWeapons != null)
                this.GlobalWeapons.Clear();


            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ABC_GlobalElement).Name);
            ABC_GlobalElement[] a = new ABC_GlobalElement[guids.Length];
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<ABC_GlobalElement>(path);

                if (a[i].elementType == ABC_GlobalElement.GlobalElementType.Weapon) {
                    this.GlobalWeapons.Add(a[i], path);
                }

            }

        }


        //  ****************************************** Setup Re-Ordablelists and define abilityController************************************************************

        void OnFocus() {

            // get new target 
            GameObject sel = Selection.activeGameObject;

            // get ABC from current target 
            if (sel != null && sel.GetComponent<ABC_WeaponPickUp>() != null) {
                weaponPickUpManager = sel.GetComponent<ABC_WeaponPickUp>();

                GUIContent titleContent = new GUIContent(sel.gameObject.name + "'s Weapon Pick Up Manager");
                GetWindow<ABC_WeaponPickUp_EditorWindow>().titleContent = titleContent;
            }

            //If we have controller then setup abilities 
            if (weaponPickUpManager != null) {

                //Retrieve the main serialized object. This is the main property which is updated to retrieve current state, fields changed and then modifications applied back to the real object
                GetTarget = new SerializedObject(weaponPickUpManager);

                PopulateWeaponList();

            }

        }




        void OnGUI() {


            if (weaponPickUpManager != null) {

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
                toolbarABCSelection = GUILayout.Toolbar(toolbarABCSelection, toolbarABC);
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



                switch ((int)toolbarABCSelection) {

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


                        generalSettingsToolbarSelection = GUILayout.SelectionGrid(generalSettingsToolbarSelection, generalSettingsToolbar, 1);




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

                        switch ((int)generalSettingsToolbarSelection) {
                            case 0:


                                #region SideBySide

                                InspectorSectionHeader("Weapon Settings");

                                EditorGUILayout.BeginHorizontal();

                                #region PickUp Settings

                                InspectorVerticalBox(true);



                                ResetLabelWidth();

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("pickUpMode"), GUILayout.MaxWidth(180));
                                InspectorHelpBox("Determine which weapon will be enabled/equipped during pickup. Can choose to either link to the weapon via an ID or a Name.", false);



                                EditorGUIUtility.labelWidth = 190;

                                if ((string)(GetTarget.FindProperty("pickUpMode")).enumNames[GetTarget.FindProperty("pickUpMode").enumValueIndex] == "Enable") {

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("enableWeaponUsingName"), new GUIContent("Enable Using Weapon Name"));

                                    ResetLabelWidth();
                                    if (GetTarget.FindProperty("enableWeaponUsingName").boolValue == true) {
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponLinkName"), new GUIContent("Weapon Name"), GUILayout.MaxWidth(250));
                                    } else {
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponLinkID"), new GUIContent("Weapon ID"), GUILayout.MaxWidth(220));
                                    }

                                    EditorGUILayout.Space();

                                } else if ((string)(GetTarget.FindProperty("pickUpMode")).enumNames[GetTarget.FindProperty("pickUpMode").enumValueIndex] == "Import") {

                                    EditorGUILayout.BeginHorizontal();
                                    this.globalWeaponsListChoice = EditorGUILayout.Popup(this.globalWeaponsListChoice, this.GlobalWeapons.Keys.Select(e => e.name).ToArray());

                                    if (GUILayout.Button("Update", GUILayout.Width(180))) {


                                        weaponPickUpManager.ImportGlobalWeapon(this.GlobalWeapons.Keys.ToArray()[this.globalWeaponsListChoice]);

                                        //Update serialized object with new weapon assignment
                                        GetTarget.Update();

                                        EditorUtility.SetDirty(weaponPickUpManager);

                                    }



                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.Space();
                                    string importingWeaponName = "Weapon Not Set";

                                    if (weaponPickUpManager.GlobalElementToImport != null) {
                                        importingWeaponName = "(" + StarSymbol + ") " + weaponPickUpManager.GlobalElementToImport.ElementWeapon.weaponName;
                                    }

                                    EditorGUILayout.LabelField("Selected Weapon: " + importingWeaponName, EditorStyles.boldLabel);
                                    EditorGUILayout.Space();
                                    if (weaponPickUpManager.GlobalElementToImport != null) {

                                        if (GUILayout.Button(new GUIContent("Load Global Weapon"), GUILayout.Width(180)))
                                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(GlobalWeapons[weaponPickUpManager.GlobalElementToImport]);


                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 190;
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("importGlobalElementEnableGameTypeModification"), new GUIContent("Enable Game Type Modification"));
                                        ResetLabelWidth();

                                        if (GetTarget.FindProperty("importGlobalElementEnableGameTypeModification").boolValue == true) {
                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("importGlobalElementGameTypeModification"), new GUIContent("Game Type"), GUILayout.MaxWidth(250));
                                        }

                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.Space();
                                        if (GetTarget.FindProperty("importGlobalElementEnableGameTypeModification").boolValue == true) {

                                            switch ((ABC_GlobalPortal.GameType)GetTarget.FindProperty("importGlobalElementGameTypeModification").enumValueIndex) {

                                                case ABC_GlobalPortal.GameType.Action:
                                                    InspectorHelpBox("Abilities will be setup based on the 'Action' Game Type. Ability will activate on the the nearest enemy targetted and will always activate even if no target exists");
                                                    break;
                                                case ABC_GlobalPortal.GameType.FPS:
                                                    InspectorHelpBox("Abilities will be setup based on the 'FPS' Game Type. Ability will activate towards the Crosshair");
                                                    break;
                                                case ABC_GlobalPortal.GameType.TPS:
                                                    InspectorHelpBox("Abilities will be setup based on the 'TPS' Game Type. Ability will activate towards the Crosshair, Melee Ability will attack the nearest enemy targetted.");
                                                    break;
                                                case ABC_GlobalPortal.GameType.RPGMMO:
                                                    InspectorHelpBox("Abilities will be setup based on the 'RPG/MMO' Game Type. Ability will require a target before activating. Melee attacks will always hit.");
                                                    break;
                                                case ABC_GlobalPortal.GameType.MOBA:
                                                    InspectorHelpBox("Abilities will be setup based on the 'MOBA' Game Type. Ability will need to be chosen before a second click determines the direction the Ability will travel.");
                                                    break;
                                                case ABC_GlobalPortal.GameType.TopDownAction:
                                                    InspectorHelpBox("Abilities will be setup based on the 'Top Down Action' Game Type. Abilities and Attacks will activate towards mouse direction. If mouse direction is near a target the Ability will activate towards the target instead.");
                                                    break;
                                            }
                                        }

                                    }

                                    EditorGUILayout.Space();




                                }



                                EditorGUILayout.Space();
                                EditorGUIUtility.labelWidth = 180;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableOrImportWeaponDelay"), new GUIContent("Enable/Import Delay"), GUILayout.MaxWidth(230));
                                InspectorHelpBox("Delay before the weapon is enabled (time this with animation).");




                                EditorGUILayout.EndVertical();


                                #endregion

                                #region Ammo/Equip

                                InspectorVerticalBox(true);


                                EditorGUIUtility.labelWidth = 180;

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("updateAmmoOnPickup"));
                                if (GetTarget.FindProperty("updateAmmoOnPickup").boolValue == true) {
                                    EditorGUIUtility.labelWidth = 90;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("ammoAmount"), new GUIContent("Ammo"), GUILayout.MaxWidth(150));
                                    EditorGUILayout.Space();
                                }
                                EditorGUILayout.EndHorizontal();
                                InspectorHelpBox("If ticked then the weapons ammo will be updated on pickup to the value set");

                                EditorGUIUtility.labelWidth = 180;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("equipWeaponOnPickup"));
                                InspectorHelpBox("If ticked then the weapon will be equipped on pickup, else it will just be enabled.");

                                EditorGUIUtility.labelWidth = 260;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("dropCurrentWeaponOnPickUp"));

                                EditorGUIUtility.labelWidth = 260;
                                if (GetTarget.FindProperty("dropCurrentWeaponOnPickUp").boolValue == false) {
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("unequipCurrentWeaponBeforePickUp"));
                                }

                                InspectorHelpBox("Depending on if drop or unequip is ticked then the entity will either drop the entities current weapon or unequip it before picking up the new weapon.");


                                EditorGUILayout.EndVertical();


                                #endregion

                                EditorGUILayout.EndHorizontal();

                                #endregion

                                #region SideBySide 

                                InspectorSectionHeader("General Settings");

                                EditorGUILayout.BeginHorizontal();

                                #region Key/Tag Settings

                                InspectorVerticalBox(true);

                                EditorGUIUtility.labelWidth = 200;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("pickUpTagRequired"), new GUIContent("Tags Required To Pick Up"));

                                if (GetTarget.FindProperty("pickUpTagRequired").boolValue == true) {
                                    InspectorListBox("Tags Required:", GetTarget.FindProperty("pickUpTags"));
                                    InspectorListBox("Tags Restricted:", GetTarget.FindProperty("pickUpRestrictedTags"));
                                }


                                InspectorHelpBox("If enabled then only specific tags can pick up and interact with the weapon.");

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("triggerRequiredToPickUp"), new GUIContent("Trigger Required To Pick Up"));
                                ResetLabelWidth();

                                if (GetTarget.FindProperty("triggerRequiredToPickUp").boolValue == true) {

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("pickUpInputType"), new GUIContent("Input Type"), GUILayout.MaxWidth(250));

                                    if (((string)GetTarget.FindProperty("pickUpInputType").enumNames[GetTarget.FindProperty("pickUpInputType").enumValueIndex]) == "Key") {

                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("key"), new GUIContent("Key"), GUILayout.MaxWidth(250));

                                    } else {

                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("keyButton"), new GUIContent("Button"), GUILayout.MaxWidth(250));

                                    }
                                }

                                InspectorHelpBox("If enabled then the player needs to trigger the pickup with input, if not enabled the entity will automatically pickup on collision");

                                EditorGUIUtility.labelWidth = 210;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("disableOnPickUp"), new GUIContent("Disable Pick Up Once Equipped"));
                                InspectorHelpBox("If ticked then weapon pick up object will be disabled once it is picked up stopping additional entities from picking up the weapon");

                                ResetLabelWidth();

                                EditorGUILayout.EndVertical();

                                #endregion


                                #region Graphic/Collider Settings

                                InspectorVerticalBox(true);

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("usePickUpGraphic"), new GUIContent("Use Graphics"));

                                ResetLabelWidth();
                                if (GetTarget.FindProperty("usePickUpGraphic").boolValue == true) {

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("pickUpGraphic"));

                                }


                                InspectorHelpBox("Use graphics at run time, decide if you want to add your own graphic or use the graphic of the weapon linked to this pickup.");

                                EditorGUILayout.PropertyField(GetTarget.FindProperty("addPickupCollider"), new GUIContent("Add Collider"));

                                if (GetTarget.FindProperty("addPickupCollider").boolValue == true) {
                                    EditorGUILayout.LabelField("Collider Offset", GUILayout.MaxWidth(100));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("colliderOffset"), new GUIContent(""), GUILayout.MaxWidth(500));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("isTrigger"), new GUIContent("Is Trigger"));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("colliderRadius"), new GUIContent("Radius"), GUILayout.MaxWidth(170));




                                }

                                InspectorHelpBox("If disabled then the weapon pickup will be created without a collider being added. Any colliders already present on the object will remain.");


                                EditorGUILayout.EndVertical();


                                #endregion

                                EditorGUILayout.EndHorizontal();

                                #endregion

#if ABC_GC_Integration

                                #region AllWay

                            InspectorSectionHeader("Game Creator Action On Pick Up");

                            InspectorVerticalBox();


                            EditorGUIUtility.labelWidth = 150;

                            SerializedProperty gcPickUpActionList = GetTarget.FindProperty("gcPickUpActionList");
                            SetupGCActionList(ref gcPickUpActionList, ref gcActionListEditor, "Assets/ABC/Scripts/ABC-Resources/Resources/ABC-GCActions/", weaponPickUpManager.gameObject.name + ".prefab");
                            gcActionListEditor.OnInspectorGUI();


                            ResetLabelWidth();

                            EditorGUILayout.EndVertical();


                                #endregion

#endif

#if ABC_GC_2_Integration

                                #region AllWay

                            InspectorSectionHeader("Game Creator 2 Action - On Pick Up");

                            InspectorVerticalBox();


                            EditorGUIUtility.labelWidth = 150;

                            SerializedProperty gc2PickUpAction = GetTarget.FindProperty("gc2PickUpAction");
                            SetupGC2Action(ref gc2PickUpAction, "Assets/ABC - Game Creator 2 Integration/Global Elements/ABC-GC2 Actions/", weaponPickUpManager.gameObject.name + ".prefab");

                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(gc2PickUpAction, new GUIContent(""));
                            EditorGUILayout.Space();

                            ResetLabelWidth();

                            EditorGUILayout.EndVertical();


                                #endregion

#endif


                                break;
                            case 1:

                                InspectorSectionHeader("Animations");

                                InspectorVerticalBox();

                                // what to play Aesthetically when picking up the weapon
                                EditorGUILayout.BeginHorizontal();
                                EditorGUIUtility.labelWidth = 200;
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("usePickUpAnimations"));
                                ResetLabelWidth();
                                EditorGUILayout.EndHorizontal();
                                InspectorHelpBox("Run an animation when an entity picks up the weapon.", false);

                                EditorGUILayout.EndVertical();

                                if (GetTarget.FindProperty("usePickUpAnimations").boolValue == true) {

                                    InspectorSectionHeader("Weapon Pick Up Animations");

                                    #region SideBySide 

                                    EditorGUILayout.BeginHorizontal();

                                    #region Pick-Up Animation Runner 

                                    InspectorVerticalBox(true);

                                    EditorGUILayout.Space();

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimationRunnerClip"), new GUIContent("Animation Clip"), GUILayout.MaxWidth(315));

                                    if (GetTarget.FindProperty("weaponPickUpAnimationRunnerClip").FindPropertyRelative("refVal").objectReferenceValue != null) {
                                        InspectorHelpBox("Select an animation clip to play, the duration, speed and delay. The clip is played using the ABC animation runner and does not use Unity's Animator.");

                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimationRunnerMask"), new GUIContent("Avatar Mask"));

                                        EditorGUIUtility.labelWidth = 75;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimationRunnerClipDuration"), new GUIContent("Duration"), GUILayout.MaxWidth(125));
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimationRunnerClipSpeed"), new GUIContent("Speed"), GUILayout.MaxWidth(125));
                                        EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimationRunnerClipDelay"), new GUIContent("Delay"), GUILayout.MaxWidth(125));


                                    }

                                    ResetLabelWidth();
                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndVertical();

                                    #endregion


                                    #region Pick-Up Animation 

                                    InspectorVerticalBox(true);

                                    EditorGUILayout.Space();

                                    EditorGUIUtility.labelWidth = 145;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimatorParameter"), new GUIContent("Animator Parameter"), GUILayout.MaxWidth(315));

                                    if (GetTarget.FindProperty("weaponPickUpAnimatorParameter").stringValue != "") {

                                        InspectorHelpBox("Enter in the name of the animation in your animator. Then the parameter type and the start and stop values. Note: Animation will keep repeating until entity is no longer initiating");


                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimatorParameterType"), new GUIContent("Parameter Type"), GUILayout.MaxWidth(250));

                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 150;



                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimatorDuration"), new GUIContent("Animation Duration"), GUILayout.MaxWidth(230));


                                        if (((string)GetTarget.FindProperty("weaponPickUpAnimatorParameterType").enumNames[GetTarget.FindProperty("weaponPickUpAnimatorParameterType").enumValueIndex]) != "Trigger") {
                                            //EditorGUILayout.BeginHorizontal();
                                            // if not trigger we need to know the value to switch on and off
                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimatorOnValue"), new GUIContent("On Value"), GUILayout.MaxWidth(230));

                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("weaponPickUpAnimatorOffValue"), new GUIContent("Off Value"), GUILayout.MaxWidth(230));

                                        }
                                    }


                                    ResetLabelWidth();
                                    EditorGUILayout.Space();



                                    EditorGUILayout.EndVertical();


                                    #endregion

                                    EditorGUILayout.EndHorizontal();

                                    #endregion

                                }

                                break;

                            case 2:

                                InspectorSectionHeader("Pick-Up UI ");

                                #region AllWay

                                InspectorVerticalBox();


                                EditorGUIUtility.labelWidth = 150;


                                EditorGUILayout.PropertyField(GetTarget.FindProperty("displayPickUpText"));
                                InspectorHelpBox("If ticked then text can be setup to be displayed to the user to help with picking up the weapon.");

                                ResetLabelWidth();

                                if (GetTarget.FindProperty("displayPickUpText").boolValue == true) {

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("displayTextObject"), new GUIContent("Text Object"), GUILayout.MaxWidth(270));
                                    InspectorHelpBox("Unity Text Object where the pick up text information will be displayed.");

                                    EditorGUILayout.LabelField("Text:");
                                    GetTarget.FindProperty("textToDisplay").stringValue = EditorGUILayout.TextArea(GetTarget.FindProperty("textToDisplay").stringValue, GUILayout.MaxHeight(40f));
                                    InspectorHelpBox("Text to display. Placeholders: #Key# - Input to pick up weapon");

                                    EditorGUIUtility.labelWidth = 200;

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("onlyDisplayWhenColliding"));
                                    InspectorHelpBox("If ticked then the display will only show when the pick up weapon collides with another object");

                                    if (GetTarget.FindProperty("onlyDisplayWhenColliding").boolValue == true) {

                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("onlyDisplayForPickUpTags"));
                                        InspectorHelpBox("If ticked then the display text will only show when the pick up tags set up collide (limits when the display will show)");
                                    }

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("alwaysFaceCamera"));
                                    InspectorHelpBox("If true then the text will always turn to face the camera. Should only be used if the text is an object in game and not used on a canvas ");
                                }

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


                    case 1:

                        //new tab?
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

            toolbarABC = new GUIContent[] { new GUIContent(" Settings", Resources.Load("ABC-EditorIcons/Settings", typeof(Texture2D)) as Texture2D, "Settings") };

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

            if (weaponPickUpManager != null) {

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(weaponPickUpManager);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTarget.UpdateIfRequiredOrScript();


                //Will update values in editor at runtime
                if (weaponPickUpManager.updateEditorAtRunTime == true) {
                    Repaint();
                }
            }

        }

    }
}