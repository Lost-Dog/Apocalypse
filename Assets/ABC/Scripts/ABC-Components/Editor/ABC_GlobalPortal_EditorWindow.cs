#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

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
    public class ABC_GlobalPortal_EditorWindow : EditorWindow {

        [MenuItem("Window/ABC/Global Portal")]

        public static void ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(ABC_GlobalPortal_EditorWindow), false);
            window.maxSize = new Vector2(windowWidth, windowHeight);
            window.minSize = window.maxSize;
        }



        #region Window Sizes

        static float windowHeight = 620f;
        static float windowWidth = 835f;

        //Width of first column in left part of main body 
        int settingButtonsWidth = 150;

        public int minimumSectionHeight = 0;
        public int minimumSideBySideSectionWidth = 312;
        public int minimumAllWaySectionWidth = 650;

        public int elementPanelWidth = 100;
        public int elementPanelWidthSix = 85;
        public int elementPanelWidthSeven = 68;

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

        public void InspectorLink(string text, int length = 50) {

            int maxString = length;
            string url = text;

            if (text.Length > maxString)
                text = text.Substring(0, maxString) + "...";

            if (GUILayout.Button(text, EditorStyles.linkLabel)) {
                Application.OpenURL(url);
            }

        }

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

        public void InspectorElementHeader(string text, int PanelWidth, string description = "", int FontSize = 12) {
            Color originalTextColor = GUI.skin.button.normal.textColor;

            GUIStyle myStyle = new GUIStyle("Box");

            if (EditorGUIUtility.isProSkin) {
                myStyle.normal.textColor = inspectorSectionHeaderTextProColor;
            } else {
                myStyle.normal.textColor = inspectorSectionHeaderTextColor;
            }

            myStyle.normal.textColor = Color.white;

            myStyle.alignment = TextAnchor.MiddleCenter;
            myStyle.fontStyle = FontStyle.Bold;


            myStyle.fontSize = FontSize;

            if (text.Count() > 12)
                myStyle.fontSize = FontSize - 1;

            myStyle.wordWrap = true;


            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHeaderProColor;
            } else {
                GUI.color = inspectorSectionHeaderColor;
            }

            GUILayout.Box(" " + text, myStyle, new GUILayoutOption[] { GUILayout.MaxWidth(PanelWidth + 7), GUILayout.MinHeight(40) });

            GUI.color = Color.grey;
            GUILayout.Box(" ", new GUILayoutOption[] { GUILayout.MaxWidth(PanelWidth), GUILayout.Height(0.01f) });


            GUI.color = Color.white;
            GUI.skin.box.normal.textColor = originalTextColor;


            if (description != "")
                InspectorHelpBox(description, false, true);
        }

        public void InspectorSectionElementBox(int PanelWidth) {

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }

            EditorGUILayout.BeginVertical("Box", GUILayout.MinWidth(45), GUILayout.MaxWidth(PanelWidth));



            GUI.color = Color.white;

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


        public void InspectorSizedVerticalBox(float width = 200f) {

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionBoxProColor;
            } else {
                GUI.color = inspectorSectionBoxColor;
            }



            EditorGUILayout.BeginVertical("Box", GUILayout.MinHeight(minimumSectionHeight), GUILayout.MinWidth(width));




            GUI.color = Color.white;

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
            GUILayout.Box(title, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(19) });
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
        Texture globalIcon;
        Texture noteIcon;
        Texture AddIcon;
        Texture RemoveIcon;
        Texture ExportIcon;
        Texture CopyIcon;
        Texture ImportIcon;


        Vector2 editorScrollPos;
        Vector2 listScrollPos;

        GUIStyle textureButton = new GUIStyle();

        #endregion

        GameObject CharacterObject;

        ABC_GlobalPortal globalPortal;
        SerializedObject GetTarget;

        ABC_StateManager stateManager;
        SerializedObject GetTargetStateManager;

        ABC_Controller abcManager;
        SerializedObject GetTargetABCManager;

        Dictionary<ABC_GlobalElement, string> exportedElements = new Dictionary<ABC_GlobalElement, string>();

        int exportedElementCurrentSelectedWeaponTag = 0;
        List<string> exportedElementWeaponTags = new List<string>();

        int exportedElementCurrentSelectedAbilityTag = 0;
        List<string> exportedElementAbilityTags = new List<string>();

        int exportedElementCurrentSelectedEffectTag = 0;
        List<string> exportedElementEffectTags = new List<string>();


        public int globalPortalToolbarSelection;

        public GUIContent[] globalPortalToolbar;


        public int characterCreatorToolbarSelection;
        public string[] characterCreatorToolbar = new string[] { "Character Setup", "Weapons", "Abilities", "Effects" };

        public int ccUIDropDownSelection;


        // ************************* Global Portal Functions ***********************************

        #region Global Portal Functions

        /// <summary>
        /// Determines if a value exists in a serialized property (used for checking if Tags exist in ABC) 
        /// </summary>
        private static bool PropertyExists(SerializedProperty property, int start, int end, string value) {
            for (int i = start; i < end; i++) {
                SerializedProperty t = property.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Will create an ABC tag in Unity if doesn't already exist
        /// </summary>
        /// <param name="tagName">Name of tag to create</param>
        public void CreateTag(string tagName) {

            //Mark all abc tags in the ABC/ Folder
            tagName = "ABC/" + tagName;

            // Open tag manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            // Tags Property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            // if not found, add it
            if (!PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName)) {
                int index = tagsProp.arraySize;
                // Insert new array element
                tagsProp.InsertArrayElementAtIndex(index);
                SerializedProperty sp = tagsProp.GetArrayElementAtIndex(index);
                // Set array element to tagName
                sp.stringValue = tagName;
                Debug.Log("Tag: " + tagName + " has been added");
                // Save settings
                tagManager.ApplyModifiedProperties();

            } else {
                //Debug.Log ("Tag: " + tagName + " already exists");
            }
        }

        /// <summary>
        /// Will add base data to ABC components depending on character type
        /// </summary>
        public async void AddComponentPresets() {

            if (GetTarget.FindProperty("addComponentPresets").boolValue == false) {
                return;
            }



#if ABC_GC_2_Integration

        GameCreator.Runtime.Characters.Character gcCharacter = abcManager.GetComponent<GameCreator.Runtime.Characters.Character>();

        if (gcCharacter != null && gcCharacter.Motion.AngularSpeed == 1800) {
            gcCharacter.Motion.AngularSpeed = 3000;            
        }


#endif

            #region Import Base Data


            //Record current abilities on entity
            List<ABC_Ability> abilityImport = new List<ABC_Ability>();

            foreach (ABC_Ability a in abcManager.Abilities) {

                ABC_Ability newAbility = new ABC_Ability();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(a), newAbility);
                abilityImport.Add(newAbility);

            }

            //Record current weapons on entity
            List<ABC_Controller.Weapon> weaponImport = new List<ABC_Controller.Weapon>();

            foreach (ABC_Controller.Weapon w in abcManager.Weapons) {

                ABC_Controller.Weapon newWeapon = new ABC_Controller.Weapon();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(w), newWeapon);
                weaponImport.Add(newWeapon);

            }

            //record any groups that already existed
            List<ABC_Controller.AbilityGroup> groupImport = new List<ABC_Controller.AbilityGroup>();

            foreach (ABC_Controller.AbilityGroup g in abcManager.AbilityGroups) {

                ABC_Controller.AbilityGroup newGroup = new ABC_Controller.AbilityGroup();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(g), newGroup);
                groupImport.Add(newGroup);

            }

            //record any UI that already existed
            List<ABC_IconUI> uiImport = new List<ABC_IconUI>();

            foreach (ABC_IconUI i in abcManager.IconUIs) {

                ABC_IconUI newIcon = new ABC_IconUI();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(i), newIcon);
                uiImport.Add(newIcon);

            }

            //record any AI that already existed
            List<ABC_Controller.AIRule> aiImport = new List<ABC_Controller.AIRule>();

            foreach (ABC_Controller.AIRule ai in abcManager.AIRules) {

                ABC_Controller.AIRule newAI = new ABC_Controller.AIRule();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(ai), newAI);
                aiImport.Add(newAI);

            }


            //Overwrite ABC manager with the defaults from global portal 
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(globalPortal.ComponentPreset.GetComponent<ABC_Controller>()), abcManager);


            //reImport any abilities that already existed
            if (abilityImport.Count > 0)
                abcManager.Abilities.AddRange(abilityImport);

            //reImport any weapons that already existed
            if (weaponImport.Count > 0)
                abcManager.Weapons.AddRange(weaponImport);

            //reImport any groups that already existed
            if (groupImport.Count > 0)
                abcManager.AbilityGroups.AddRange(groupImport);

            //reImport any weapons that already existed
            if (uiImport.Count > 0)
                abcManager.IconUIs.AddRange(uiImport);

            //reImport any AI that already existed
            if (aiImport.Count > 0)
                abcManager.AIRules.AddRange(aiImport);


            //add tag conversions
            AddTagConversations();

            //If not player then remove target indicators
            if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Friendly || globalPortal.characterType == ABC_GlobalPortal.CharacterType.Enemy) {
                abcManager.softTargetOutlineGlow = false;
                abcManager.softTargetIndicator = null;
                abcManager.softTargetShader = null;
                abcManager.targetOutlineGlow = false;
                abcManager.selectedTargetIndicator = null;
                abcManager.selectedTargetShader = null;

                abcManager.hitsPreventCastingDuration = 1f;
                
            }

            //Persistant crosshair mode 
            if (globalPortal.persistentCrosshairAestheticMode == true && globalPortal.gameType == ABC_GlobalPortal.GameType.FPS && globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player || globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player) {
                abcManager.persistentCrosshairAestheticMode = globalPortal.persistentCrosshairAestheticMode;
            }


#if ABC_GC_2_Stats_Integration

        GameCreator.Runtime.Stats.Traits gcTraits = abcManager.GetComponent<GameCreator.Runtime.Stats.Traits>();

        if (gcTraits != null) {
            abcManager.manaIntergrationType = ABCIntegrationType.GameCreator2;
            abcManager.gcManaID = "mp";
        }

#endif


            //Turn new ABC controller into serialized object and make sure to update
            GetTargetABCManager = new SerializedObject(abcManager);
            GetTargetABCManager.Update();


            //record any stats that already existed
            List<ABC_StateManager.EntityStat> statsImport = new List<ABC_StateManager.EntityStat>();

            foreach (ABC_StateManager.EntityStat stats in stateManager.EntityStatList) {

                ABC_StateManager.EntityStat newStat = new ABC_StateManager.EntityStat();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(stats), newStat);
                statsImport.Add(newStat);

            }

            //Overwrite ABC State Manager with the defaults from global portal 
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(globalPortal.ComponentPreset.GetComponent<ABC_StateManager>()), stateManager);

            //reImport any stats that already existed and don't add defaults
            if (statsImport.Count > 0) {

                //Clear defaults
                stateManager.EntityStatList.Clear();

                //Add stats it had before
                stateManager.EntityStatList.AddRange(statsImport);
            }



            //enable or disable target limitations 
            foreach (ABC_StateManager.TargeterLimitation TL in stateManager.TargeterLimitations) {

                //disable targeter limitations if enemy
                if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Enemy) {
                    TL.enableTargeterLimit = false;
                } else {
                    TL.enableTargeterLimit = true;
                }

            }


            //Change health depending on person type unless already changed
            if (stateManager.maxHealth == 50 && stateManager.currentHealth == 50) {
                if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Enemy) {
                    stateManager.maxHealth = 350;
                    stateManager.currentHealth = 350;
                } else {
                    stateManager.maxHealth = 500;
                    stateManager.currentHealth = 500;
                }
            }


            //Increase hit stop movement for non player
            if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Friendly || globalPortal.characterType == ABC_GlobalPortal.CharacterType.Enemy) {
                stateManager.hitStopMovementDuration = 0.8f;
            }




#if ABC_GC_2_Stats_Integration

        if (gcTraits != null) {
            stateManager.healthIntergrationType = ABCIntegrationType.GameCreator2;
            stateManager.gcHealthID = "hp";

            //Clear stat defaults as we will be using GC
            stateManager.EntityStatList.Clear();

        }

#endif



            //Turn new ABC controller into serialized object and make sure to update
            GetTargetStateManager = new SerializedObject(stateManager);
            GetTargetStateManager.Update();



            await Task.Delay(25);

            //Animator runtime controller 
            Animator meAni = CharacterObject.GetComponentInChildren<Animator>();

            if (meAni == null) {
                meAni = CharacterObject.AddComponent<Animator>();
            }


            //If not player
            if (globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player) {

                //if AI enabled
                if (globalPortal.enableAI == true) {

                    GetTargetABCManager.FindProperty("inIdleMode").boolValue = true;
                    GetTargetABCManager.FindProperty("enableAI").boolValue = true;
                    GetTargetABCManager.FindProperty("navAIEnabled").boolValue = true;



                    //Set stopping distance depending on type 
                    if (globalPortal.typeAI == ABC_GlobalPortal.AIType.CloseCombat) {
                        GetTargetABCManager.FindProperty("minimumStopDistance").floatValue = 5.5f;
                        GetTargetABCManager.FindProperty("maximumStopDistance").floatValue = 5.5f;
                    } else {
                        GetTargetABCManager.FindProperty("minimumStopDistance").floatValue = 10;
                        GetTargetABCManager.FindProperty("maximumStopDistance").floatValue = 15;
                    }

                    //add ABC Animator 
                    if (meAni != null) {
                        meAni.runtimeAnimatorController = globalPortal.AniController;
                    }

                }


                NavMeshAgent navMesh = null;

                if (globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.GameCreator2) {

                    GameObject GC2Mannequin = CharacterObject;

#if ABC_GC_2_Integration

                //find animator as it's in different place in GC2 Character Creation
                if (abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>() != null && abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>().Animim.Mannequin != null)
                    GC2Mannequin = abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>().Animim.Mannequin.gameObject;

#endif


                    //Make sure navmesh has been added
                    navMesh = GC2Mannequin.GetComponent<NavMeshAgent>();

                    if (navMesh == null)
                        navMesh = GC2Mannequin.AddComponent<NavMeshAgent>();


                    //remove nav mesh obstacle
                    if (GC2Mannequin.GetComponent<NavMeshObstacle>() != null)
                        DestroyImmediate(GC2Mannequin.GetComponent<NavMeshObstacle>());


                } else {

                    //Make sure navmesh has been added
                    navMesh = CharacterObject.GetComponent<NavMeshAgent>();

                    if (navMesh == null)
                        navMesh = CharacterObject.AddComponent<NavMeshAgent>();


                    //remove nav mesh obstacle
                    if (CharacterObject.GetComponent<NavMeshObstacle>() != null)
                        DestroyImmediate(CharacterObject.GetComponent<NavMeshObstacle>());

                }


                navMesh.baseOffset = -0.07f;
                navMesh.radius = 1f;

                navMesh.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;


            } else {


                NavMeshObstacle navOb = null;

                if (globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.GameCreator2) {

                    GameObject GC2Mannequin = CharacterObject;

#if ABC_GC_2_Integration

                //find animator as it's in different place in GC2 Character Creation
                if (abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>() != null && abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>().Animim.Mannequin != null)
                    GC2Mannequin = abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>().Animim.Mannequin.gameObject;

#endif

                    //Make sure navOb has been added
                    navOb = GC2Mannequin.GetComponent<NavMeshObstacle>();

                    if (navOb == null)
                        navOb = GC2Mannequin.AddComponent<NavMeshObstacle>();


                    //If player remove nav mesh agent
                    if (GC2Mannequin.GetComponent<NavMeshAgent>() != null)
                        DestroyImmediate(GC2Mannequin.GetComponent<NavMeshAgent>());


                } else {

                    //Make sure navOb has been added
                    navOb = CharacterObject.GetComponent<NavMeshObstacle>();

                    if (navOb == null)
                        navOb = CharacterObject.AddComponent<NavMeshObstacle>();


                    //If player remove nav mesh agent
                    if (CharacterObject.GetComponent<NavMeshAgent>() != null)
                        DestroyImmediate(CharacterObject.GetComponent<NavMeshAgent>());

                }


                navOb.shape = NavMeshObstacleShape.Capsule;
                navOb.center = new Vector3(0, 1, 0);
                navOb.carving = true;
                navOb.carvingMoveThreshold = 50f;
            }



            #endregion

            //If Player then set tag to Player
            if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player) {

                CharacterObject.gameObject.tag = "Player";

            } else if (CharacterObject.tag == "Friendly" || CharacterObject.tag == "Player" || CharacterObject.tag == "Untagged" || CharacterObject.tag == "Enemy") {

                //Else if tag was friendly, enemy, player or untagged then set to enemy/friendly

                if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Enemy)
                    CharacterObject.gameObject.tag = "Enemy";
                else if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Friendly)
                    CharacterObject.gameObject.tag = "Friendly";



            }

            //Capsule Collider
            CapsuleCollider capCol = null;

            if (globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.GameCreator2) {

                GameObject GC2Mannequin = CharacterObject;

#if ABC_GC_2_Integration

            //find animator as it's in different place in GC2 Character Creation
            if (abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>() != null && abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>().Animim.Mannequin != null)
                GC2Mannequin = abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>().Animim.Mannequin.gameObject;

#endif


                capCol = GC2Mannequin.GetComponent<CapsuleCollider>();

                if (capCol == null)
                    capCol = GC2Mannequin.AddComponent<CapsuleCollider>();




            } else {


                capCol = CharacterObject.GetComponent<CapsuleCollider>();

                if (capCol == null)
                    capCol = CharacterObject.AddComponent<CapsuleCollider>();

            }


            capCol.isTrigger = false;
            capCol.center = new Vector3(0, 1, 0);
            capCol.height = 2;
            capCol.radius = 0.6f;

            if (globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player)
                capCol.radius = 0.9f;



        }

        /// <summary>
        /// Will add player input if doesn't exist
        /// </summary>
        public void AddPlayerInput() {
            //If not player
            if (globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player)
                return;

#if ENABLE_INPUT_SYSTEM
      
      
            if (this.CharacterObject.GetComponentInChildren<PlayerInput>() == null) {
                PlayerInput pInput = this.CharacterObject.AddComponent<PlayerInput>();
                pInput.actions = (InputActionAsset)Resources.Load("ABC-InputActions/ABC_InputActions");

                if (Transform.FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() != null)
                    pInput.uiInputModule = Transform.FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            }


#endif




        }

        /// <summary>
        /// Will add tag converstions to the character depending on the type of character (i.e target might be enemy)
        /// </summary>
        public void AddTagConversations() {

            abcManager.tagConversions.Clear();
            stateManager.tagConversions.Clear();


            switch (globalPortal.characterType) {
                case ABC_GlobalPortal.CharacterType.Enemy:



                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[0].tagBefore = "Ally";
                    abcManager.tagConversions[0].tagAfter = "Enemy";

                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[1].tagBefore = "Target";
                    abcManager.tagConversions[1].tagAfter = "Friendly";


                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[2].tagBefore = "TargetPriority";
                    abcManager.tagConversions[2].tagAfter = "FriendlyPriority";




                    break;
                case ABC_GlobalPortal.CharacterType.Player:


                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[0].tagBefore = "Ally";
                    abcManager.tagConversions[0].tagAfter = "Friendly";

                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[1].tagBefore = "Target";
                    abcManager.tagConversions[1].tagAfter = "Enemy";


                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[2].tagBefore = "TargetPriority";
                    abcManager.tagConversions[2].tagAfter = "EnemyPriority";

                    break;
                case ABC_GlobalPortal.CharacterType.Friendly:

                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[0].tagBefore = "Ally";
                    abcManager.tagConversions[0].tagAfter = "Friendly";

                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[1].tagBefore = "Target";
                    abcManager.tagConversions[1].tagAfter = "Enemy";


                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[2].tagBefore = "TargetPriority";
                    abcManager.tagConversions[2].tagAfter = "EnemyPriority";


                    abcManager.tagConversions.Add(new ABC_Utilities.TagConverter());
                    abcManager.tagConversions[3].tagBefore = "Follow";
                    abcManager.tagConversions[3].tagAfter = "Player";

                    break;
            }

            abcManager.enableTagConversions = true;

        }

        /// <summary>
        /// Sets up targetting depending on the game type chosen 
        /// </summary>
        public void SetupCharacterTargetting() {

            if (globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player || GetTarget.FindProperty("setupGameTypeTargetting").boolValue == false) {
                return;
            }



            switch (globalPortal.gameType) {
                case ABC_GlobalPortal.GameType.Action:

                    GetTargetABCManager.FindProperty("autoTargetSelect").boolValue = true;

                    GetTargetABCManager.FindProperty("autoTargetSoftTarget").boolValue = true;
                    GetTargetABCManager.FindProperty("autoTargetSwapClosest").boolValue = true;
                    GetTargetABCManager.FindProperty("autoTargetInCamera").boolValue = true;
                    GetTargetABCManager.FindProperty("autoTargetSelfFacing").boolValue = true;

                    GetTargetABCManager.FindProperty("targetSelectType").intValue = 0;
                    GetTargetABCManager.FindProperty("hoverForTarget").boolValue = false;
                    GetTargetABCManager.FindProperty("clickForTarget").boolValue = true;

                    GetTargetABCManager.FindProperty("crosshairEnabled").boolValue = false;
                    GetTargetABCManager.FindProperty("softTargetConfirmKey").intValue = (int)KeyCode.Mouse2;




                    break;
                case ABC_GlobalPortal.GameType.FPS:

                    GetTargetABCManager.FindProperty("autoTargetSelect").boolValue = false;
                    GetTargetABCManager.FindProperty("targetSelectType").intValue = 0;
                    GetTargetABCManager.FindProperty("crosshairEnabled").boolValue = true;
                    GetTargetABCManager.FindProperty("softTargetConfirmKey").intValue = (int)KeyCode.Mouse2;

                    break;
                case ABC_GlobalPortal.GameType.TPS:

                    GetTargetABCManager.FindProperty("autoTargetSelect").boolValue = true;

                    GetTargetABCManager.FindProperty("autoTargetSoftTarget").boolValue = true;
                    GetTargetABCManager.FindProperty("autoTargetSwapClosest").boolValue = true;
                    GetTargetABCManager.FindProperty("autoTargetInCamera").boolValue = true;
                    GetTargetABCManager.FindProperty("autoTargetSelfFacing").boolValue = false;

                    GetTargetABCManager.FindProperty("targetSelectType").intValue = 0;
                    GetTargetABCManager.FindProperty("hoverForTarget").boolValue = false;
                    GetTargetABCManager.FindProperty("clickForTarget").boolValue = true;

                    GetTargetABCManager.FindProperty("crosshairEnabled").boolValue = true;

                    GetTargetABCManager.FindProperty("softTargetConfirmKey").intValue = (int)KeyCode.Mouse0;


                    break;
                case ABC_GlobalPortal.GameType.RPGMMO:
                    GetTargetABCManager.FindProperty("autoTargetSelect").boolValue = false;

                    GetTargetABCManager.FindProperty("autoTargetSoftTarget").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetSwapClosest").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetInCamera").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetSelfFacing").boolValue = false;

                    GetTargetABCManager.FindProperty("targetSelectType").intValue = 1;

                    if (globalPortal.clickType == ABC_GlobalPortal.PointClickType.Click) {
                        GetTargetABCManager.FindProperty("clickForTarget").boolValue = true;
                        GetTargetABCManager.FindProperty("hoverForTarget").boolValue = false;
                    } else {
                        GetTargetABCManager.FindProperty("clickForTarget").boolValue = false;
                        GetTargetABCManager.FindProperty("hoverForTarget").boolValue = true;
                    }


                    GetTargetABCManager.FindProperty("targetSelectLeeway").boolValue = true;
                    GetTargetABCManager.FindProperty("crosshairEnabled").boolValue = false;
                    GetTargetABCManager.FindProperty("softTargetConfirmKey").intValue = (int)KeyCode.Mouse2;

                    break;
                case ABC_GlobalPortal.GameType.MOBA:
                    GetTargetABCManager.FindProperty("autoTargetSelect").boolValue = false;

                    GetTargetABCManager.FindProperty("autoTargetSoftTarget").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetSwapClosest").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetInCamera").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetSelfFacing").boolValue = false;

                    GetTargetABCManager.FindProperty("targetSelectType").intValue = 1;

                    if (globalPortal.clickType == ABC_GlobalPortal.PointClickType.Click) {
                        GetTargetABCManager.FindProperty("clickForTarget").boolValue = true;
                        GetTargetABCManager.FindProperty("hoverForTarget").boolValue = false;
                    } else {
                        GetTargetABCManager.FindProperty("clickForTarget").boolValue = false;
                        GetTargetABCManager.FindProperty("hoverForTarget").boolValue = true;
                    }

                    GetTargetABCManager.FindProperty("targetSelectLeeway").boolValue = true;
                    GetTargetABCManager.FindProperty("softTargetConfirmKey").intValue = (int)KeyCode.Mouse2;


                    GetTargetABCManager.FindProperty("crosshairEnabled").boolValue = false;

                    break;
                case ABC_GlobalPortal.GameType.TopDownAction:
                    GetTargetABCManager.FindProperty("autoTargetSelect").boolValue = false;

                    GetTargetABCManager.FindProperty("autoTargetSoftTarget").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetSwapClosest").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetInCamera").boolValue = false;
                    GetTargetABCManager.FindProperty("autoTargetSelfFacing").boolValue = false;

                    GetTargetABCManager.FindProperty("targetSelectType").intValue = 0;

                    if (globalPortal.clickType == ABC_GlobalPortal.PointClickType.Click) {
                        GetTargetABCManager.FindProperty("clickForTarget").boolValue = true;
                        GetTargetABCManager.FindProperty("hoverForTarget").boolValue = false;
                    } else {
                        GetTargetABCManager.FindProperty("clickForTarget").boolValue = false;
                        GetTargetABCManager.FindProperty("hoverForTarget").boolValue = true;
                    }

                    GetTargetABCManager.FindProperty("targetSelectLeeway").boolValue = true;
                    GetTargetABCManager.FindProperty("softTargetConfirmKey").intValue = (int)KeyCode.Mouse2;

                    GetTargetABCManager.FindProperty("crosshairEnabled").boolValue = false;

                    break;
            }



        }

        /// <summary>
        /// Will set weapon holders and tags ready to easily select dropdown on weapons
        /// </summary>
        public void SetupTagsAndWeaponHolders() {

            if (GetTarget.FindProperty("setupForWeaponsAndAbilities").boolValue == false) {

                return;
            }


            this.CreateTag("WeaponInitiation");
            this.CreateTag("AdditionalWeaponInitiation");
            this.CreateTag("RightHand");
            this.CreateTag("LeftHand");
            this.CreateTag("RightFoot");
            this.CreateTag("LeftFoot");
            this.CreateTag("Head");
            this.CreateTag("RightWeaponHolder");
            this.CreateTag("LeftWeaponHolder");

            this.CreateTag("RightWeaponSheath");
            this.CreateTag("LeftWeaponSheath");

            this.CreateTag("RightWeaponHolster");
            this.CreateTag("LeftWeaponHolster");


            this.CreateTag("BackRightWeaponSheath");
            this.CreateTag("BackCentreWeaponSheath");
            this.CreateTag("BackLeftWeaponSheath");

            this.CreateTag("BackRightDiagonalWeaponSheath");
            this.CreateTag("BackLeftDiagonalWeaponSheath");

            this.CreateTag("WeaponIK");
            this.CreateTag("AdditionalWeaponIK");

            this.CreateTag("WeaponBase");
            this.CreateTag("WeaponTip");


            //If animator doesn't exist then can't find skeletons to tag
            Animator meAni = CharacterObject.GetComponentInChildren<Animator>();

            if (meAni == null)
                return;


            //Initial Tags

            meAni.GetBoneTransform(HumanBodyBones.Head).transform.tag = "ABC/Head";
            meAni.GetBoneTransform(HumanBodyBones.RightHand).transform.tag = "ABC/RightHand";
            meAni.GetBoneTransform(HumanBodyBones.LeftHand).transform.tag = "ABC/LeftHand";
            meAni.GetBoneTransform(HumanBodyBones.RightFoot).transform.tag = "ABC/RightFoot";
            meAni.GetBoneTransform(HumanBodyBones.LeftFoot).transform.tag = "ABC/LeftFoot";

            // Holders
            GameObject obj = null;

            if (CharacterObject.GetComponentsInChildren<Transform>().Where(wh => wh.name == "ABC_RightWeaponHolder").Count() == 0) {
                obj = Instantiate(globalPortal.tagHolders.Where(wh => wh.name == "ABC_RightWeaponHolder").FirstOrDefault());
                obj.name = obj.name.Replace("(Clone)", "");
                obj.transform.SetParent(meAni.GetBoneTransform(HumanBodyBones.RightHand).transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            if (CharacterObject.GetComponentsInChildren<Transform>().Where(wh => wh.name == "ABC_LeftWeaponHolder").Count() == 0) {
                obj = Instantiate(globalPortal.tagHolders.Where(wh => wh.name == "ABC_LeftWeaponHolder").FirstOrDefault());
                obj.name = obj.name.Replace("(Clone)", "");
                obj.transform.SetParent(meAni.GetBoneTransform(HumanBodyBones.LeftHand).transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            if (CharacterObject.GetComponentsInChildren<Transform>().Where(wh => wh.name == "ABC_WeaponSheath").Count() == 0) {
                obj = Instantiate(globalPortal.tagHolders.Where(wh => wh.name == "ABC_WeaponSheath").FirstOrDefault());
                obj.name = obj.name.Replace("(Clone)", "");
                obj.transform.SetParent(meAni.GetBoneTransform(HumanBodyBones.Hips).transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            if (CharacterObject.GetComponentsInChildren<Transform>().Where(wh => wh.name == "ABC_BackWeaponSheath").Count() == 0) {
                obj = Instantiate(globalPortal.tagHolders.Where(wh => wh.name == "ABC_BackWeaponSheath").FirstOrDefault());
                obj.name = obj.name.Replace("(Clone)", "");
                obj.transform.SetParent(meAni.GetBoneTransform(HumanBodyBones.Spine).transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }


        }

        /// <summary>
        /// Will convert the characters abilities to the game type selected 
        /// </summary>
        public void ConvertAbilitiesToGameType() {

            if (GetTarget.FindProperty("convertAbilitiesToGameType").boolValue == false || abcManager == null) {

                return;
            }


            foreach (ABC_Ability ability in abcManager.Abilities) {

                //If not a global ability then go ahead and convert the game type
                if (ability.globalAbilities == null) {

#if ABC_GC_2_Integration
        if (abcManager.transform.GetComponent<GameCreator.Runtime.Characters.Character>() != null)
                ability.AdjustAbilityForGameCreator2();
#endif

                    ability.ConvertToGameType(globalPortal.gameType);
                } else { // else just apply the game type modification to the global ability 
                    ability.globalAbilitiesEnableGameTypeModification = true;
                    ability.globalAbilitiesGameTypeModification = globalPortal.gameType;
                }

            }


            //Turn new ABC controller into serialized object and make sure to update
            GetTargetABCManager = new SerializedObject(abcManager);
            GetTargetABCManager.Update();






        }

        /// <summary>
        /// Will add sword and gun placeholders to easily allow for adjustment to the holders, can be toggled on and off
        /// </summary>
        public void ToggleWeaponHolderAdjustmentMode() {



            if (CharacterObject.GetComponentsInChildren<Transform>().Where(obj => obj.name == "WeaponAdjustmentObject" || obj.name == "GunAdjustmentObject").Count() > 0) {

                foreach (Transform obj in CharacterObject.GetComponentsInChildren<Transform>().Where(obj => obj.name == "WeaponAdjustmentObject" || obj.name == "GunAdjustmentObject").ToList()) {
                    DestroyImmediate(obj.gameObject);
                }

            } else {

                List<string> weaponHolderTags = new List<string>();
                ABC_IEntity CharacterEntity = new ABC_IEntity(CharacterObject);


                weaponHolderTags.Add("ABC/RightWeaponHolder");
                weaponHolderTags.Add("ABC/LeftWeaponHolder");
                weaponHolderTags.Add("ABC/RightWeaponSheath");
                weaponHolderTags.Add("ABC/LeftWeaponSheath");
                weaponHolderTags.Add("ABC/BackRightWeaponSheath");
                weaponHolderTags.Add("ABC/BackCentreWeaponSheath");
                weaponHolderTags.Add("ABC/BackLeftWeaponSheath");
                weaponHolderTags.Add("ABC/BackRightDiagonalWeaponSheath");
                weaponHolderTags.Add("ABC/BackLeftDiagonalWeaponSheath");


                foreach (string tag in weaponHolderTags) {

                    GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(CharacterEntity, tag, false, false);

                    if (onSelfTagObj != null) {

                        GameObject holderObj = Instantiate(globalPortal.weaponHolderAdjustmentObject);
                        holderObj.name = holderObj.name.Replace("(Clone)", "");

                        holderObj.transform.position = onSelfTagObj.transform.position;
                        holderObj.transform.parent = onSelfTagObj.transform;
                        holderObj.transform.rotation = onSelfTagObj.transform.rotation;

                    }

                }



                weaponHolderTags.Clear();

                weaponHolderTags.Add("ABC/RightWeaponHolster");
                weaponHolderTags.Add("ABC/LeftWeaponHolster");

                foreach (string tag in weaponHolderTags) {

                    GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(CharacterEntity, tag, false, false);

                    if (onSelfTagObj != null) {

                        GameObject holderObj = Instantiate(globalPortal.gunHolderAdjustmentObject);
                        holderObj.name = holderObj.name.Replace("(Clone)", "");

                        holderObj.transform.position = onSelfTagObj.transform.position;
                        holderObj.transform.parent = onSelfTagObj.transform;
                        holderObj.transform.rotation = onSelfTagObj.transform.rotation;

                    }

                }


            }


        }

        /// <summary>
        /// Will add UI to the character depending on the dropdown selected and the items existing within the dropdown prefab
        /// </summary>
        public async void AddCharacterUI() {

            if (GetTarget.FindProperty("addUI").boolValue == false) {
                return;
            }

            GameObject UIObj = null;

            //If UI already added then grab it
            if (CharacterObject.transform.Find(globalPortal.UI[this.ccUIDropDownSelection].name.Replace("(Clone)", "")) != null) {
                UIObj = CharacterObject.transform.Find(globalPortal.UI[this.ccUIDropDownSelection].name.Replace("(Clone)", "")).gameObject;
            } else {
                //Instantiate the Item selected in drop down
                UIObj = Instantiate(globalPortal.UI[this.ccUIDropDownSelection]);
                UIObj.name = UIObj.name.Replace("(Clone)", "");
            }


            //Set character as parent and reset local position
            UIObj.transform.SetParent(CharacterObject.transform);
            UIObj.transform.localPosition = Vector3.zero;

            await Task.Delay(25);

            //If the object contains any items depending on name then set the value in ABC so it links up to the UI

            #region Name
            if (UIObj.GetComponentsInChildren<Text>(true).Length > 0 && UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("Name")).FirstOrDefault() != null)
                UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("Name")).FirstOrDefault().text = CharacterObject.name;
            #endregion

            #region Health
            Slider slider = UIObj.GetComponentsInChildren<Slider>(true).Where(n => n.name.Contains("SliderHealth")).FirstOrDefault();
            Text text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextHealthValue")).FirstOrDefault();

            if (slider != null || text != null) {

                if (GetTargetStateManager.FindProperty("HealthGUIList").arraySize == 0) {
                    GetTargetStateManager.FindProperty("HealthGUIList").InsertArrayElementAtIndex(0);
                    await Task.Delay(50);
                }

                if (GetTarget.FindProperty("alwaysShowUI").boolValue == true) {
                    GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("onlyShowSliderWhenSelected").boolValue = false;
                } else {
                    GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("onlyShowSliderWhenSelected").boolValue = true;
                }

            }

            if (slider != null) {

                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthSlider").FindPropertyRelative("refVal").objectReferenceValue = slider;
                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthSlider").FindPropertyRelative("refName").stringValue = slider.name;

                slider = UIObj.GetComponentsInChildren<Slider>(true).Where(n => n.name.Contains("SliderHealthOverTime")).FirstOrDefault();

                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthOverTimeSlider").FindPropertyRelative("refVal").objectReferenceValue = slider;
                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthOverTimeSlider").FindPropertyRelative("refName").stringValue = slider.name;

                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthOverTimeSliderUpdateDelay").floatValue = 0.7f;
                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthOverTimeSliderUpdateDuration").floatValue = 0.5f;

            }


            if (text != null) {
                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthText").FindPropertyRelative("refVal").objectReferenceValue = text;
                GetTargetStateManager.FindProperty("HealthGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("healthText").FindPropertyRelative("refName").stringValue = text.name;
            }





            #endregion

            #region Mana
            slider = UIObj.GetComponentsInChildren<Slider>(true).Where(n => n.name.Contains("SliderMana")).FirstOrDefault();
            text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextManaValue")).FirstOrDefault();


            if (slider != null || text != null) {

                if (GetTargetABCManager.FindProperty("ManaGUIList").arraySize == 0) {
                    GetTargetABCManager.FindProperty("ManaGUIList").InsertArrayElementAtIndex(0);
                    await Task.Delay(50);
                }


                if (GetTarget.FindProperty("alwaysShowUI").boolValue == true) {
                    GetTargetABCManager.FindProperty("ManaGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("onlyShowSliderWhenSelected").boolValue = false;
                } else {
                    GetTargetABCManager.FindProperty("ManaGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("onlyShowSliderWhenSelected").boolValue = true;
                }
            }


            if (slider != null) {
                GetTargetABCManager.FindProperty("ManaGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("manaSlider").FindPropertyRelative("refVal").objectReferenceValue = slider;
                GetTargetABCManager.FindProperty("ManaGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("manaSlider").FindPropertyRelative("refName").stringValue = slider.name;
            }

            if (text != null) {
                GetTargetABCManager.FindProperty("ManaGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("manaText").FindPropertyRelative("refVal").objectReferenceValue = text;
                GetTargetABCManager.FindProperty("ManaGUIList").GetArrayElementAtIndex(0).FindPropertyRelative("manaText").FindPropertyRelative("refName").stringValue = text.name;
            }





            #endregion

            #region Preparing
            slider = UIObj.GetComponentsInChildren<Slider>(true).Where(n => n.name.Contains("SliderPreparing")).FirstOrDefault();

            if (slider != null) {
                GetTargetABCManager.FindProperty("preparingAbilityGUIBar").FindPropertyRelative("refVal").objectReferenceValue = slider;
                GetTargetABCManager.FindProperty("preparingAbilityGUIBar").FindPropertyRelative("refName").stringValue = slider.name;
            }


            text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("PreparingText")).FirstOrDefault();

            if (text != null) {
                GetTargetABCManager.FindProperty("preparingAbilityGUIText").FindPropertyRelative("refVal").objectReferenceValue = text;
                GetTargetABCManager.FindProperty("preparingAbilityGUIText").FindPropertyRelative("refName").stringValue = text.name;
            }

            #endregion

            #region EffectLog
            text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextEffectLog")).FirstOrDefault();

            if (text != null) {
                GetTargetStateManager.FindProperty("effectLogGUIText").FindPropertyRelative("refVal").objectReferenceValue = text;
                GetTargetStateManager.FindProperty("effectLogGUIText").FindPropertyRelative("refName").stringValue = text.name;
            }
            #endregion

            #region AbilityLog
            text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextAbilityLog")).FirstOrDefault();

            if (text != null) {
                GetTargetABCManager.FindProperty("abilityLogGUIText").FindPropertyRelative("refVal").objectReferenceValue = text;
                GetTargetABCManager.FindProperty("abilityLogGUIText").FindPropertyRelative("refName").stringValue = text.name;
            }
            #endregion

            #region Stats
            if (GetTargetStateManager.FindProperty("EntityStatList").arraySize > 0) {

                text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextStatStrength")).FirstOrDefault();

                if (text != null) {
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(0).FindPropertyRelative("textStatValue").FindPropertyRelative("refVal").objectReferenceValue = text;
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(0).FindPropertyRelative("textStatValue").FindPropertyRelative("refName").stringValue = text.name;

                    if (GetTarget.FindProperty("alwaysShowUI").boolValue == true) {
                        GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(0).FindPropertyRelative("onlyShowTextWhenSelected").boolValue = false;
                    }


                }

                text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextStatDefence")).FirstOrDefault();

                if (text != null) {
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(1).FindPropertyRelative("textStatValue").FindPropertyRelative("refVal").objectReferenceValue = text;
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(1).FindPropertyRelative("textStatValue").FindPropertyRelative("refName").stringValue = text.name;

                    if (GetTarget.FindProperty("alwaysShowUI").boolValue == true) {
                        GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(1).FindPropertyRelative("onlyShowTextWhenSelected").boolValue = false;
                    }
                }

                text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextStatMagic")).FirstOrDefault();

                if (text != null) {
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(2).FindPropertyRelative("textStatValue").FindPropertyRelative("refVal").objectReferenceValue = text;
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(2).FindPropertyRelative("textStatValue").FindPropertyRelative("refName").stringValue = text.name;

                    if (GetTarget.FindProperty("alwaysShowUI").boolValue == true) {
                        GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(2).FindPropertyRelative("onlyShowTextWhenSelected").boolValue = false;
                    }
                }

                text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextStatDexterity")).FirstOrDefault();

                if (text != null) {
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(3).FindPropertyRelative("textStatValue").FindPropertyRelative("refVal").objectReferenceValue = text;
                    GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(3).FindPropertyRelative("textStatValue").FindPropertyRelative("refName").stringValue = text.name;

                    if (GetTarget.FindProperty("alwaysShowUI").boolValue == true) {
                        GetTargetStateManager.FindProperty("EntityStatList").GetArrayElementAtIndex(3).FindPropertyRelative("onlyShowTextWhenSelected").boolValue = false;
                    }
                }

            }

            #endregion

            #region Health Reduction Image
            RawImage image = UIObj.GetComponentsInChildren<RawImage>(true).Where(n => n.name.Contains("HealthReductionImage")).FirstOrDefault();


            if (image != null) {
                GetTargetStateManager.FindProperty("imageOnHealthReduction").FindPropertyRelative("refVal").objectReferenceValue = image;
                GetTargetStateManager.FindProperty("imageOnHealthReduction").FindPropertyRelative("refName").stringValue = image.name;

                GetTargetStateManager.FindProperty("showGUIImageOnHealthReduction").boolValue = true;
            } else {
                GetTargetStateManager.FindProperty("showGUIImageOnHealthReduction").boolValue = false;
            }
            #endregion

            #region Weapon/Ammo Image/Text
            image = UIObj.GetComponentsInChildren<RawImage>(true).Where(n => n.name.Contains("EquippedWeaponImage")).FirstOrDefault();


            if (image != null) {
                GetTargetABCManager.FindProperty("weaponImageGUI").FindPropertyRelative("refVal").objectReferenceValue = image;
                GetTargetABCManager.FindProperty("weaponImageGUI").FindPropertyRelative("refName").stringValue = image.name;
            }


            text = UIObj.GetComponentsInChildren<Text>(true).Where(n => n.name.Contains("TextWeaponAmmo")).FirstOrDefault();

            if (text != null) {
                GetTargetABCManager.FindProperty("weaponAmmoGUIText").FindPropertyRelative("refVal").objectReferenceValue = text;
                GetTargetABCManager.FindProperty("weaponAmmoGUIText").FindPropertyRelative("refName").stringValue = text.name;
            }
            #endregion

        }

        /// <summary>
        /// Will add game camera depending on the game type selected
        /// </summary>
        public async void AddGameCamera() {


            if ((globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player || globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.GameCreator2) || GetTarget.FindProperty("addCamera").boolValue == false) {
                return;
            }


            //Destroy existing cameras
            foreach (ABC_CameraBase camera in GameObject.FindObjectsOfType<ABC_CameraBase>()) {
                if (camera.name.Contains("ABC_Camera"))
                    DestroyImmediate(camera.gameObject);
            }

            GameObject cameraObj = null;
            ABC_CameraBase cbtp = null;

            switch (globalPortal.gameType) {

                case ABC_GlobalPortal.GameType.Action:

                    //create new camera object 
                    cameraObj = Instantiate(globalPortal.gameCameras.Where(obj => obj.name == "ABC_Camera_TP").FirstOrDefault());
                    cameraObj.name = cameraObj.name.Replace("(Clone)", "");

                    await Task.Delay(25);

                    cameraObj.transform.position = CharacterObject.transform.position;
                    cbtp = cameraObj.GetComponent<ABC_CameraBase>();
                    cbtp.followTarget = CharacterObject;


                    break;


                case ABC_GlobalPortal.GameType.TopDownAction:
                case ABC_GlobalPortal.GameType.MOBA:

                    //create new camera object 
                    cameraObj = Instantiate(globalPortal.gameCameras.Where(obj => obj.name == "ABC_Camera_TD").FirstOrDefault());
                    cameraObj.name = cameraObj.name.Replace("(Clone)", "");

                    await Task.Delay(25);

                    cameraObj.transform.position = CharacterObject.transform.position;
                    cbtp = cameraObj.GetComponent<ABC_CameraBase>();
                    cbtp.followTarget = CharacterObject;

                    cbtp.enableRotation = globalPortal.enableCameraRotation;

                    break;
                case ABC_GlobalPortal.GameType.RPGMMO:

                    //create new camera object 
                    cameraObj = Instantiate(globalPortal.gameCameras.Where(obj => obj.name == "ABC_Camera_RPG").FirstOrDefault());
                    cameraObj.name = cameraObj.name.Replace("(Clone)", "");

                    await Task.Delay(25);

                    cameraObj.transform.position = CharacterObject.transform.position;
                    cbtp = cameraObj.GetComponent<ABC_CameraBase>();
                    cbtp.followTarget = CharacterObject;



                    break;
                case ABC_GlobalPortal.GameType.TPS:

                    //create new camera object 
                    cameraObj = Instantiate(globalPortal.gameCameras.Where(obj => obj.name == "ABC_Camera_TPS").FirstOrDefault());
                    cameraObj.name = cameraObj.name.Replace("(Clone)", "");

                    await Task.Delay(25);

                    cameraObj.transform.position = CharacterObject.transform.position;
                    cbtp = cameraObj.GetComponent<ABC_CameraBase>();
                    cbtp.followTarget = CharacterObject;
                    cbtp.invertYAxis = globalPortal.invertYAxis;


                    break;
                case ABC_GlobalPortal.GameType.FPS:

                    //create new camera object 
                    cameraObj = Instantiate(globalPortal.gameCameras.Where(obj => obj.name == "ABC_Camera_FPS").FirstOrDefault());
                    cameraObj.name = cameraObj.name.Replace("(Clone)", "");

                    await Task.Delay(25);

                    cameraObj.transform.position = CharacterObject.transform.position;
                    cbtp = cameraObj.GetComponent<ABC_CameraBase>();
                    cbtp.followTarget = CharacterObject;
                    cbtp.invertYAxis = globalPortal.invertYAxis;


                    break;

            }




#if ENABLE_INPUT_SYSTEM
               cbtp.inputSensitivity = 30;
#endif


        }

        /// <summary>
        /// Will add movement script to the character depending on the game type selected
        /// </summary>
        public void AddMovement() {

            //If not player remove movement controller
            if ((globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player || globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.GameCreator2) && CharacterObject.GetComponent<ABC_MovementController>() != null)
                DestroyImmediate(CharacterObject.GetComponent<ABC_MovementController>());





            if ((globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player || globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.GameCreator2) || GetTarget.FindProperty("addMovement").boolValue == false) {
                return;
            }



            //preset movement 

            #region Movement Components


            //remove rigidbody as we adding character controller        
            if (CharacterObject.GetComponent<Rigidbody>() != null)
                DestroyImmediate(CharacterObject.GetComponent<Rigidbody>());


            //Character controller 
            CharacterController cc = CharacterObject.GetComponent<CharacterController>();

            if (cc == false) {
                cc = CharacterObject.AddComponent<CharacterController>();
                cc.center = new Vector3(0, 1.05f, 0);
                cc.radius = 0.4f;
            }


            ABC_MovementController moveController = CharacterObject.GetComponent<ABC_MovementController>();


            //Save previous parkour setup 
            List<ABC_MovementController.ParkourMovement> parkourVaultImport = new List<ABC_MovementController.ParkourMovement>();
            List<ABC_MovementController.ParkourMovement> parkourLiftUpImport = new List<ABC_MovementController.ParkourMovement>();
            List<ABC_MovementController.ParkourMovement> parkourSlideImport = new List<ABC_MovementController.ParkourMovement>();

            if (moveController != null) { 
                if (moveController.parkourVaultAnimations != null) {
                    foreach (ABC_MovementController.ParkourMovement parkour in moveController.parkourVaultAnimations) {

                        ABC_MovementController.ParkourMovement newParkour = new ABC_MovementController.ParkourMovement();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(parkour), newParkour);
                        parkourVaultImport.Add(newParkour);

                    }
                }

      
                if (moveController.parkourLiftUpAnimations != null) {
                    foreach (ABC_MovementController.ParkourMovement parkour in moveController.parkourLiftUpAnimations) {

                        ABC_MovementController.ParkourMovement newParkour = new ABC_MovementController.ParkourMovement();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(parkour), newParkour);
                        parkourLiftUpImport.Add(newParkour);

                    }
                }


                if (moveController.parkourSlideAnimations != null) {
                    foreach (ABC_MovementController.ParkourMovement parkour in moveController.parkourSlideAnimations) {

                        ABC_MovementController.ParkourMovement newParkour = new ABC_MovementController.ParkourMovement();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(parkour), newParkour);
                        parkourSlideImport.Add(newParkour);

                    }
                }


                //ABC Third Person Script
                DestroyImmediate(CharacterObject.GetComponent<ABC_MovementController>());
             }


            moveController = null;


            if (moveController == false) {
                moveController = CharacterObject.AddComponent<ABC_MovementController>();
            }

                


            if (globalPortal.addMovementPresets == true) {
           
                //Overwrite ABC manager with the defaults from global portal 
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(globalPortal.ComponentPreset.GetComponent<ABC_MovementController>()), moveController);
            }


            //reImport any parkour that already existed and don't add defaults
            if (parkourVaultImport.Count > 0) {

                //Clear defaults
                moveController.parkourVaultAnimations.Clear();

                //Add parkour it had before
                moveController.parkourVaultAnimations.AddRange(parkourVaultImport);
            }

            //reImport any parkour that already existed and don't add defaults
            if (parkourLiftUpImport.Count > 0) {

                //Clear defaults
                moveController.parkourLiftUpAnimations.Clear();

                //Add parkour it had before
                moveController.parkourLiftUpAnimations.AddRange(parkourLiftUpImport);
            }

            //reImport any parkour that already existed and don't add defaults
            if (parkourSlideImport.Count > 0) {

                //Clear defaults
                moveController.parkourSlideAnimations.Clear();

                //Add parkour it had before
                moveController.parkourSlideAnimations.AddRange(parkourSlideImport);
            }

            moveController.enableParkour = globalPortal.enableParkour;

            

            //Enable input script
            moveController.enabled = true;

            if (globalPortal.enableJumping == true)
                moveController.allowJumping = true;


            //If player  then disable the 'Disable Component' on hit stop as this will be done by ABC movement script
            if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player)
                stateManager.hitStopMovementDisableComponents = false;


            //Animator runtime controller 
            Animator meAni = CharacterObject.GetComponentInChildren<Animator>();


            if (meAni != null) {
                meAni.runtimeAnimatorController = globalPortal.AniController;
            }

            #endregion


            moveController.ConvertToGameType(globalPortal.gameType);

            switch (globalPortal.gameType) {

                case ABC_GlobalPortal.GameType.Action:

                    moveController.useLockOnMovement = globalPortal.enableLockOnMovement;

                    break;
                case ABC_GlobalPortal.GameType.TopDownAction:
                case ABC_GlobalPortal.GameType.MOBA:
                case ABC_GlobalPortal.GameType.RPGMMO:


                    break;
                case ABC_GlobalPortal.GameType.TPS:


                    break;
                case ABC_GlobalPortal.GameType.FPS:




                    break;

            }


        }

        /// <summary>
        /// Main method to create the character 
        /// </summary>
        public async void CreateCharacter() {

            //Add ABC Controller if not already added
            abcManager = CharacterObject.GetComponent<ABC_Controller>();

            if (abcManager == null)
                abcManager = CharacterObject.AddComponent<ABC_Controller>();



            //Add ABC StateManager if not already added
            stateManager = CharacterObject.GetComponent<ABC_StateManager>();

            if (stateManager == null)
                stateManager = CharacterObject.AddComponent<ABC_StateManager>();



            await Task.Delay(25);




            this.ConvertAbilitiesToGameType();

            this.AddComponentPresets();

            this.AddPlayerInput();

            this.SetupCharacterTargetting();

            this.SetupTagsAndWeaponHolders();



            this.AddCharacterUI();

            this.AddGameCamera();

            this.AddMovement();


        }

        /// <summary>
        /// List of all global elements in the project including file location
        /// </summary>
        /// <returns></returns>
        private Dictionary<ABC_GlobalElement, string> GetGlobalElements() {

            Dictionary<ABC_GlobalElement, string> retVal = new Dictionary<ABC_GlobalElement, string>();

            exportedElementAbilityTags.Add("All");
            exportedElementWeaponTags.Add("All");
            exportedElementEffectTags.Add("All");

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ABC_GlobalElement).Name);
            ABC_GlobalElement[] a = new ABC_GlobalElement[guids.Length];
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<ABC_GlobalElement>(path);

                retVal.Add(a[i], path);

                if (a[i].elementType == ABC_GlobalElement.GlobalElementType.Abilities) {
                    exportedElementAbilityTags.AddRange(a[i].elementTags);
                } else if (a[i].elementType == ABC_GlobalElement.GlobalElementType.Effect) {
                    exportedElementEffectTags.AddRange(a[i].elementTags);
                } else {
                    exportedElementWeaponTags.AddRange(a[i].elementTags);
                }
            }


            exportedElementAbilityTags = exportedElementAbilityTags.OrderBy(i => i != "All").ThenBy(n => n).ToList();
            exportedElementWeaponTags = exportedElementWeaponTags.OrderBy(i => i != "All").ThenBy(n => n).ToList();
            exportedElementEffectTags = exportedElementEffectTags.OrderBy(i => i != "All").ThenBy(n => n).ToList();

            return retVal;

        }

        #endregion


        public void OnEnable() {

            globalPortal = (ABC_GlobalPortal)Resources.Load("ABC-GlobalPortal/GlobalPortal");
            GetTarget = new SerializedObject(globalPortal);

            if (GetTarget != null) {
                GUIContent titleContent = new GUIContent("Global Portal");
                GetWindow<ABC_GlobalPortal_EditorWindow>().titleContent = titleContent;
            }

            this.exportedElements = this.GetGlobalElements();


            globalPortalToolbar = new GUIContent[] { new GUIContent("  Global Portal", Resources.Load("ABC-EditorIcons/Global", typeof(Texture2D)) as Texture2D, "  GlobalPortal"),
         new GUIContent("  Attribution", Resources.Load("ABC-EditorIcons/Note", typeof(Texture2D)) as Texture2D, "  Attribution")};

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");


            globalIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Global");
            noteIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Note");


            //setup styles 
            textureButton.alignment = TextAnchor.MiddleCenter;

        }



        void OnGUI() {


            if (globalPortal != null) {

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

                EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(windowWidth - 10));
                //GUILayout.Label(Resources.Load("ABC-EditorIcons/logo", typeof(Texture2D)) as Texture2D, GUILayout.MaxWidth(4));
                globalPortalToolbarSelection = GUILayout.Toolbar(globalPortalToolbarSelection, globalPortalToolbar, GUILayout.Height(31));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();


                #endregion

                #region Body

                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }



                switch ((int)globalPortalToolbarSelection) {

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


                        characterCreatorToolbarSelection = GUILayout.SelectionGrid(characterCreatorToolbarSelection, characterCreatorToolbar, 1, GUILayout.MinWidth(130));


                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        #endregion


                        #region Character 

                        #region Character Selection
                        EditorGUILayout.Space();
                        InspectorSectionHeader("Character");

                        EditorGUILayout.BeginVertical("Box");

                        GUI.color = Color.white;

                        EditorGUILayout.Space();

                        InspectorHelpBox("Select a GameObject to use in ABC Character Creator");

                        CharacterObject = (GameObject)EditorGUILayout.ObjectField(CharacterObject, typeof(GameObject), true);

                        EditorGUILayout.Space();

                        Texture2D getImage = UnityEditor.AssetPreview.GetAssetPreview(CharacterObject);

                        GUI.contentColor = Color.white;

                        GUILayout.Label(getImage);

                 

                        if (EditorGUIUtility.isProSkin == false) {
                            GUI.backgroundColor = inspectorBackgroundColor;
                            GUI.contentColor = Color.black;
                        }


                        if (CharacterObject != null) {
                            if (GUILayout.Button("Toggle Weapon Adjustment Mode", GUILayout.Height(35))) {

                                ToggleWeaponHolderAdjustmentMode();


                            }
                            EditorGUILayout.Space();
                        }


                        EditorGUILayout.LabelField("Display:", GUILayout.MaxWidth(100));
                        EditorGUILayout.PropertyField(GetTarget.FindProperty("displayABCTemplates"), new GUIContent("ABC Templates"));
                        EditorGUILayout.PropertyField(GetTarget.FindProperty("displayABCElements"), new GUIContent("ABC Elements"));

                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();

                        #endregion




                        #endregion


                        EditorGUILayout.EndVertical();

                        #endregion

                        InspectorBoldVerticleLine();

                        #region Settings


                        EditorGUILayout.BeginVertical(GUILayout.MinWidth(minimumAllWaySectionWidth + 17));

                        #region  Settings

                        switch ((int)characterCreatorToolbarSelection) {
                            case 0:




                                if (CharacterObject == null) {

                                    EditorGUILayout.HelpBox("Please add an Object in the left section to start character creating", MessageType.Warning);

                                } else {

                                    #region AllWay 

                                    InspectorVerticalBox();

                                    #region Game type Selection

                                    EditorGUILayout.BeginHorizontal();

                                    GUI.color = Color.white;

                                    EditorGUILayout.LabelField("Character Type", GUILayout.MaxWidth(100));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("characterType"), new GUIContent(""), GUILayout.MaxWidth(180));

                                    EditorGUILayout.Space();


#if ABC_GC_2_Integration
                                EditorGUILayout.LabelField("Integration Type", GUILayout.MaxWidth(100));
                                EditorGUILayout.PropertyField(GetTarget.FindProperty("characterIntegrationType"), new GUIContent(""), GUILayout.MaxWidth(180));
#endif

                                    EditorGUILayout.Space();

                                    EditorGUILayout.LabelField("Game Type", GUILayout.MaxWidth(100));
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("gameType"), new GUIContent(""), GUILayout.MaxWidth(180));


                                    EditorGUILayout.EndHorizontal();

                                    #endregion


                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();


                                    #endregion


                                    InspectorSectionHeader("Character & Ability/Weapon Setup");

                                    #region AllWay

                                    InspectorVerticalBox();

                                    ResetLabelWidth();



                                    EditorGUIUtility.labelWidth = 190;
                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("addComponentPresets"), GUILayout.MaxWidth(250));

                                    if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player && globalPortal.gameType == ABC_GlobalPortal.GameType.FPS) {
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("persistentCrosshairAestheticMode"), new GUIContent("Persistent Crosshair Aesthetics"), GUILayout.MaxWidth(250));
                                    }

                                    EditorGUILayout.EndHorizontal();

                                    InspectorHelpBox("If ticked then your character's ABC Controller & StateManager will be setup with game ready settings - adding Hit/Death Animations, AI, Stats and more ready to instantly work with the Character Creator abilities." +
                                        "This will overwrite the current settings on your character ABC components but Abilities/Weapons/UI/Groups/AI Rules will remain");

                                    if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player) {

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("setupGameTypeTargetting"), GUILayout.MaxWidth(250));

                                        if (globalPortal.gameType == ABC_GlobalPortal.GameType.RPGMMO || globalPortal.gameType == ABC_GlobalPortal.GameType.MOBA) {
                                            EditorGUIUtility.labelWidth = 130;
                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("clickType"), GUILayout.MaxWidth(250));
                                            EditorGUIUtility.labelWidth = 190;
                                        }

                                        EditorGUILayout.EndHorizontal();

                                        switch (globalPortal.gameType) {

                                            case ABC_GlobalPortal.GameType.Action:
                                                InspectorHelpBox("Targetting will be setup based on the 'Action' Game Type selected. Character will always have the nearest enemy targetted ready to attack");
                                                break;
                                            case ABC_GlobalPortal.GameType.FPS:
                                                InspectorHelpBox("Targetting will be setup based on the 'FPS' Game Type selected. Crosshair will be enabled ready to aim and shoot!");
                                                break;
                                            case ABC_GlobalPortal.GameType.TPS:
                                                InspectorHelpBox("Targetting will be setup based on the 'TPS' Game Type selected. Crosshair will be enabled ready to aim and shoot! Character will also have the nearest enemy targetted ready for melee attacks.");
                                                break;
                                            case ABC_GlobalPortal.GameType.RPGMMO:
                                                InspectorHelpBox("Targetting will be setup based on the 'RPG MMO' Game Type selected. Abilities will only activate if a target is selected first. Targetting is done through clicking or hovering the mouse depending on the dropdown selected. Melee attacks will always hit");
                                                break;
                                            case ABC_GlobalPortal.GameType.MOBA:
                                                InspectorHelpBox("Targetting will be setup based on the 'MOBA' Game Type selected. Abilities will need to be chosen before a second click determines the direction the Ability will travel.");
                                                break;
                                            case ABC_GlobalPortal.GameType.TopDownAction:
                                                InspectorHelpBox("Targetting will be setup based on the 'Top Down Action' Game Type selected. Abilities and Attacks will always activate towards mouse direction.");
                                                break;
                                        }


                                    }





                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("setupForWeaponsAndAbilities"), GUILayout.MaxWidth(250));
                                    InspectorHelpBox("Will setup the Character ready for weapon and abilites. Adding Tags and Holders on the skeleton ready for weapon graphics and ability starting positions.");


                                    if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player) {
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("convertAbilitiesToGameType"), GUILayout.MaxWidth(250));

                                        switch (globalPortal.gameType) {

                                            case ABC_GlobalPortal.GameType.Action:
                                                InspectorHelpBox("Abilities will be converted to the 'Action' Game Type selected. Abilities will always travel towards the nearest enemy targetted.");
                                                break;
                                            case ABC_GlobalPortal.GameType.FPS:
                                                InspectorHelpBox("Abilities will be converted to the 'FPS' Game Type selected. Abilities will travel towards the Crosshair");
                                                break;
                                            case ABC_GlobalPortal.GameType.TPS:
                                                InspectorHelpBox("Abilities will be converted to the 'TPS' Game Type selected. Abilities will travel towards the Crosshair! Melee abilities will also attack have the nearest enemy.");
                                                break;
                                            case ABC_GlobalPortal.GameType.RPGMMO:
                                                InspectorHelpBox("Abilities will be converted to the 'RPG MMO' Game Type selected. Abilities will only activate if a target is selected first. Melee attacks will always hit");
                                                break;
                                            case ABC_GlobalPortal.GameType.MOBA:
                                                InspectorHelpBox("Abilities will be converted to the 'MOBA' Game Type selected. Abilities will need to be chosen before a second click determines the direction the Ability will travel.");
                                                break;
                                            case ABC_GlobalPortal.GameType.TopDownAction:
                                                InspectorHelpBox("Abilities will be converted to the 'Top Down Action' Game Type selected. Abilities and Attacks will always activate towards mouse direction.");
                                                break;
                                        }
                                    }




                                    EditorGUILayout.EndVertical();




                                    #endregion

                                    InspectorSectionHeader("UI, Camera & Movement Setup");

                                    #region AllWay

                                    InspectorVerticalBox();

                                    ResetLabelWidth();



                                    EditorGUIUtility.labelWidth = 190;

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 80;
                                    EditorGUILayout.PropertyField(GetTarget.FindProperty("addUI"), GUILayout.MaxWidth(150));

                                    if (GetTarget.FindProperty("addUI").boolValue == true) {

                                        EditorGUIUtility.labelWidth = 30;
                                        ccUIDropDownSelection = EditorGUILayout.Popup("UI", ccUIDropDownSelection, (globalPortal.UI.Select(i => i.name).ToArray()), GUILayout.MaxWidth(180));
                                        EditorGUILayout.Space();
                                        EditorGUIUtility.labelWidth = 130;
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("alwaysShowUI"), GUILayout.MaxWidth(250));
                                        EditorGUIUtility.labelWidth = 190;

                                    }

                                    EditorGUILayout.EndHorizontal();

                                    InspectorHelpBox("Will add the selected UI to the character. Local UI will travel above character. Global UI will appear as an overlay in the camera. If 'Always Show UI' is disabled then UI will only" +
                                        "show when the character is targetted");

                                    if (globalPortal.characterType != ABC_GlobalPortal.CharacterType.Player) {
                                        EditorGUIUtility.labelWidth = 80;

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("enableAI"), GUILayout.MaxWidth(250));
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("typeAI"), new GUIContent("AI Type"), GUILayout.MaxWidth(250));
                                        EditorGUILayout.EndHorizontal();

                                        InspectorHelpBox("Will enable AI for the character");
                                    }

                                    if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player && globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.ABC) {

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 100;
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("addCamera"), GUILayout.MaxWidth(250));

                                        EditorGUIUtility.labelWidth = 160;
                                        if (GetTarget.FindProperty("addCamera").boolValue == true && (globalPortal.gameType == ABC_GlobalPortal.GameType.TopDownAction || globalPortal.gameType == ABC_GlobalPortal.GameType.MOBA)) {
                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("enableCameraRotation"), GUILayout.MaxWidth(250));
                                        }

                                        if (GetTarget.FindProperty("addCamera").boolValue == true && (globalPortal.gameType == ABC_GlobalPortal.GameType.TPS || globalPortal.gameType == ABC_GlobalPortal.GameType.FPS)) {
                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("invertYAxis"), GUILayout.MaxWidth(250));
                                        }


                                        EditorGUILayout.EndHorizontal();

                                        InspectorHelpBox("Will add a camera to the scene for the character. Type of Camera added will depend on the game type selected.");
                                    }


                                    if (globalPortal.characterType == ABC_GlobalPortal.CharacterType.Player && globalPortal.characterIntegrationType == ABC_GlobalPortal.CharacterIntegrationType.ABC) {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.labelWidth = 100;
                                        EditorGUILayout.PropertyField(GetTarget.FindProperty("addMovement"), GUILayout.MaxWidth(250));

                                        EditorGUIUtility.labelWidth = 140;
                                        if (GetTarget.FindProperty("addMovement").boolValue == true) {                                     

                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("addMovementPresets"), GUILayout.MaxWidth(250));

                                            EditorGUIUtility.labelWidth = 170;

                                            if (globalPortal.gameType == ABC_GlobalPortal.GameType.Action) {
                                                EditorGUILayout.PropertyField(GetTarget.FindProperty("enableLockOnMovement"), GUILayout.MaxWidth(250));
                                            }

                                            EditorGUIUtility.labelWidth = 100;

                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("enableJumping"), GUILayout.MaxWidth(250));
                                        }

                                        EditorGUILayout.EndHorizontal();

                                        if (GetTarget.FindProperty("addMovement").boolValue == true) {

                                            EditorGUILayout.PropertyField(GetTarget.FindProperty("enableParkour"), GUILayout.MaxWidth(250));
                                        }

                                         InspectorHelpBox("Will add movement script to the scene for the character. Type of movement system added will depend on the game type selected. WARNING: Adding Movement Presets will overwrite current audio and animation clips.");
                                    }

                                    ResetLabelWidth();

                                    EditorGUILayout.EndVertical();

                                    if (GUILayout.Button("Create Character", GUILayout.Height(30), GUILayout.Width(minimumAllWaySectionWidth))) {

                                        CreateCharacter();


                                    }




                                    #endregion

                                }


                                break;
                            case 1:



                                ResetLabelWidth();

                                EditorGUILayout.BeginHorizontal();

                                exportedElementCurrentSelectedWeaponTag = EditorGUILayout.Popup("Filter Weapons: ", exportedElementCurrentSelectedWeaponTag, this.exportedElementWeaponTags.ToArray(), GUILayout.MaxWidth(280));

                                EditorGUILayout.Space();

                                if (GUILayout.Button(new GUIContent(" Add New Global Weapon", AddIcon, "Add New Global Weapon"), GUILayout.Width(200))) {

#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace

                                    // create an empty exported element object
                                    ABC_GlobalElement exportedElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();

                                    if (exportedElement.officialABC == true) {
                                        exportedElement.createdBy = "ABC";
                                    } else {
                                        exportedElement.createdBy = "";
                                    }

                                    exportedElement.creationDate = System.DateTime.Now.ToString();
                                    exportedElement.elementType = ABC_GlobalElement.GlobalElementType.Weapon;

                                    exportedElement.ElementWeapon = new ABC_Controller.Weapon();
                                    exportedElement.ElementWeapon.weaponID = ABC_Utilities.GenerateUniqueID();

                                    //save to path 
                                    string fullPath = UnityEditor.EditorUtility.SaveFilePanel("Save Effect", "Assets", "New ABC Weapon Element", "asset");

                                    if (fullPath != "") {
                                        string basePath = fullPath.Replace(Application.dataPath, "Assets");
                                        UnityEditor.AssetDatabase.CreateAsset(exportedElement, basePath);
                                        this.exportedElements = this.GetGlobalElements();
                                    }
#endif

                                }

                                EditorGUILayout.Space();
                                EditorGUILayout.Space();

                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();



                                editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                 false,
                                                 false);

                                EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(windowWidth));




                                int displayCounter = 0;

                                foreach (KeyValuePair<ABC_GlobalElement, string> expElement in this.exportedElements.OrderBy(x => x.Key.name)) {

                                    //If not showing templates then continue
                                    if (globalPortal.displayABCTemplates == false && expElement.Key.createdBy == "ABC" && expElement.Key.name.Contains("Template"))
                                        continue;

                                    //If not showing ABC element then continue
                                    if (globalPortal.displayABCElements == false && expElement.Key.createdBy == "ABC" && expElement.Key.name.Contains("Template") == false)
                                        continue;


                                    if (expElement.Key.elementType != ABC_GlobalElement.GlobalElementType.Weapon || exportedElementCurrentSelectedWeaponTag != 0 && expElement.Key.elementTags.Contains(this.exportedElementWeaponTags[exportedElementCurrentSelectedWeaponTag]) == false)
                                        continue;

                                    if (displayCounter == 0 || displayCounter % 5 == 0) {
                                        EditorGUILayout.BeginHorizontal();
                                    }

                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical();

                                    #region Exported Element
                                    InspectorElementHeader(expElement.Key.name, elementPanelWidth);

                                    InspectorSectionElementBox(elementPanelWidth);


                                    ABC_Controller.Weapon.WeaponObj weaponGraphic = null;
                                    Texture2D elementImage = null;

                                    //Show weapon graphic if weapon
                                    if (expElement.Key.elementType == ABC_GlobalElement.GlobalElementType.Weapon) {

                                        weaponGraphic = expElement.Key.ElementWeapon.weaponGraphics.FirstOrDefault();

                                        if (expElement.Key.showWeaponPreview == true && weaponGraphic != null && weaponGraphic.weaponObjMainGraphic.GameObject != null) {

                                            elementImage = UnityEditor.AssetPreview.GetAssetPreview(weaponGraphic.weaponObjMainGraphic.GameObject);
                                        } else if (expElement.Key.elementIcon != null) {

                                            elementImage = expElement.Key.elementIcon;
                                        } else if (expElement.Key.elementIcon == null && expElement.Key.ElementWeapon.weaponIconImage != null) {

                                            elementImage = expElement.Key.ElementWeapon.weaponIconImage.Texture2D;
                                        }
                                    } else { // else show element Icon
                                        elementImage = expElement.Key.elementIcon;
                                    }

                                    GUI.contentColor = Color.white;

                                    GUILayout.Label(elementImage, GUILayout.Height(elementPanelWidth), GUILayout.Width(elementPanelWidth));           

                                    if (EditorGUIUtility.isProSkin == false) {
                                        GUI.backgroundColor = inspectorBackgroundColor;
                                        GUI.contentColor = Color.black;
                                    } 

                                    //Select scriptable object
                                    if (GUILayout.Button(new GUIContent("Info"), GUILayout.Width(elementPanelWidth))) {
                                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(expElement.Value);
                                    }


                                    if (CharacterObject != null && GUILayout.Button(new GUIContent("Add Weapon"), GUILayout.Width(elementPanelWidth))) {

                                        ABC_ImportGlobalElement_EditorWindow wizard = (ABC_ImportGlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_ImportGlobalElement_EditorWindow), true);
                                        wizard.importingEntity = CharacterObject.GetComponent<ABC_Controller>();
                                        wizard.elementType = ABC_ImportGlobalElement_EditorWindow.GlobalElementType.Weapon;
                                        wizard.globalElement = expElement.Key;



                                    }






                                    EditorGUILayout.EndVertical();
                                    #endregion

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    displayCounter++;

                                    if (displayCounter == 0 || displayCounter % 5 == 0) {
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }

                                }


                                if (displayCounter % 5 != 0) {

                                    for (int i = displayCounter % 5; i < 5; i++) {

                                        GUILayout.Label("", GUILayout.Height(elementPanelWidth), GUILayout.Width(elementPanelWidth));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }
                                    EditorGUILayout.EndHorizontal();

                                }


                                EditorGUILayout.EndVertical();

                                EditorGUILayout.EndScrollView();


                                break;

                            case 2:

                                ResetLabelWidth();

                                EditorGUILayout.BeginHorizontal();

                                exportedElementCurrentSelectedAbilityTag = EditorGUILayout.Popup("Filter Abilities: ", exportedElementCurrentSelectedAbilityTag, this.exportedElementAbilityTags.ToArray(), GUILayout.MaxWidth(280));

                                EditorGUILayout.Space();

                                if (GUILayout.Button(new GUIContent(" Add New Global Ability", AddIcon, "Add New Global Ability"), GUILayout.Width(200))) {

#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace

                                    // create an empty exported element object
                                    ABC_GlobalElement exportedElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();

                                    if (exportedElement.officialABC == true) {
                                        exportedElement.createdBy = "ABC";
                                    } else {
                                        exportedElement.createdBy = "";
                                    }

                                    exportedElement.creationDate = System.DateTime.Now.ToString();
                                    exportedElement.elementType = ABC_GlobalElement.GlobalElementType.Abilities;

                                    //save to path 
                                    string fullPath = UnityEditor.EditorUtility.SaveFilePanel("Save Effect", "Assets", "New ABC Ability Element", "asset");

                                    if (fullPath != "") {
                                        string basePath = fullPath.Replace(Application.dataPath, "Assets");
                                        UnityEditor.AssetDatabase.CreateAsset(exportedElement, basePath);
                                        this.exportedElements = this.GetGlobalElements();
                                    }
#endif

                                }


                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                EditorGUILayout.Space();



                                editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                 false,
                                                 false);

                                EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(windowWidth + 100));

                                int displayAbilityCounter = 0;

                                foreach (KeyValuePair<ABC_GlobalElement, string> expElement in this.exportedElements.OrderBy(x => x.Key.name)) {

                                    //If not showing templates then continue
                                    if (globalPortal.displayABCTemplates == false && expElement.Key.createdBy == "ABC" && expElement.Key.name.Contains("Template"))
                                        continue;

                                    //If not showing ABC element then continue
                                    if (globalPortal.displayABCElements == false && expElement.Key.createdBy == "ABC" && expElement.Key.name.Contains("Template") == false)
                                        continue;

                                    if (expElement.Key.elementType != ABC_GlobalElement.GlobalElementType.Abilities || exportedElementCurrentSelectedAbilityTag != 0 && expElement.Key.elementTags.Contains(this.exportedElementAbilityTags[exportedElementCurrentSelectedAbilityTag]) == false)
                                        continue;

                                    if (displayAbilityCounter == 0 || displayAbilityCounter % 7 == 0) {
                                        EditorGUILayout.BeginHorizontal();
                                    }

                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical();

                                    #region Exported Element
                                    InspectorElementHeader(expElement.Key.name, elementPanelWidthSeven, "", 11);

                                    InspectorSectionElementBox(elementPanelWidthSeven);


                                    ABC_Controller.Weapon.WeaponObj weaponGraphic = null;
                                    Texture2D elementImage = null;

                                    //Show weapon graphic if weapon
                                    if (expElement.Key.elementType == ABC_GlobalElement.GlobalElementType.Weapon) {

                                        weaponGraphic = expElement.Key.ElementWeapon.weaponGraphics.FirstOrDefault();

                                        if (weaponGraphic != null && weaponGraphic.weaponObjMainGraphic.GameObject != null) {

                                            elementImage = UnityEditor.AssetPreview.GetAssetPreview(weaponGraphic.weaponObjMainGraphic.GameObject);
                                        } else {
                                            elementImage = expElement.Key.elementIcon;
                                        }
                                    } else { // else show element Icon
                                        elementImage = expElement.Key.elementIcon;
                                    }

                                    GUI.contentColor = Color.white;

                                    GUILayout.Label(elementImage, GUILayout.Height(elementPanelWidthSeven), GUILayout.Width(elementPanelWidthSeven));

                                    if (EditorGUIUtility.isProSkin == false) {
                                        GUI.backgroundColor = inspectorBackgroundColor;
                                        GUI.contentColor = Color.black;
                                    } 


                                    //Select scriptable object
                                    if (GUILayout.Button(new GUIContent("Info"), GUILayout.Width(elementPanelWidthSeven))) {
                                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(expElement.Value);
                                    }

                                    if (CharacterObject != null && GUILayout.Button(new GUIContent("Add Ability"), GUILayout.Width(elementPanelWidthSeven))) {

                                        ABC_ImportGlobalElement_EditorWindow wizard = (ABC_ImportGlobalElement_EditorWindow)EditorWindow.GetWindow(typeof(ABC_ImportGlobalElement_EditorWindow), true);
                                        wizard.importingEntity = CharacterObject.GetComponent<ABC_Controller>();
                                        wizard.elementType = ABC_ImportGlobalElement_EditorWindow.GlobalElementType.Abilities;
                                        wizard.globalElement = expElement.Key;


                                    }






                                    EditorGUILayout.EndVertical();
                                    #endregion

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    displayAbilityCounter++;

                                    if (displayAbilityCounter == 0 || displayAbilityCounter % 7 == 0) {
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }

                                }

                                if (displayAbilityCounter % 7 != 0) {

                                    for (int i = displayAbilityCounter % 7; i < 7; i++) {

                                        GUILayout.Label("", GUILayout.Height(elementPanelWidthSeven - 5), GUILayout.Width(elementPanelWidthSeven - 5));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }
                                    EditorGUILayout.EndHorizontal();

                                }


                                EditorGUILayout.EndVertical();

                                EditorGUILayout.EndScrollView();

                                break;
                            case 3:


                                ResetLabelWidth();

                                EditorGUILayout.BeginHorizontal();
                                exportedElementCurrentSelectedEffectTag = EditorGUILayout.Popup("Filter Effects: ", exportedElementCurrentSelectedEffectTag, this.exportedElementEffectTags.ToArray(), GUILayout.MaxWidth(280));

                                EditorGUILayout.Space();

                                if (GUILayout.Button(new GUIContent(" Add New Global Effect", AddIcon, "Add New Global Effect"), GUILayout.Width(200))) {

#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace

                                    // create an empty exported element object
                                    ABC_GlobalElement exportedElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();

                                    if (exportedElement.officialABC == true) {
                                        exportedElement.createdBy = "ABC";
                                    } else {
                                        exportedElement.createdBy = "";
                                    }

                                    exportedElement.creationDate = System.DateTime.Now.ToString();
                                    exportedElement.elementType = ABC_GlobalElement.GlobalElementType.Effect;

                                    //save to path 
                                    string fullPath = UnityEditor.EditorUtility.SaveFilePanel("Save Effect", "Assets", "New ABC Effect Element", "asset");

                                    if (fullPath != "") {
                                        string basePath = fullPath.Replace(Application.dataPath, "Assets");
                                        UnityEditor.AssetDatabase.CreateAsset(exportedElement, basePath);
                                        this.exportedElements = this.GetGlobalElements();
                                    }
#endif

                                }


#if ABC_GC_2_Stats_Integration
                            if (GUILayout.Button(new GUIContent(" Convert for GC 2", ImportIcon, "Convert for GC 2"), GUILayout.Width(200))) {
                                ABC_Utilities.ConvertGlobalElementsForGC2();
                            }
#endif

                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                EditorGUILayout.Space();



                                editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                 false,
                                                 false);

                                EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(windowWidth + 10));


                                int displayEffectCounter = 0;

                                foreach (KeyValuePair<ABC_GlobalElement, string> expElement in this.exportedElements.OrderBy(x => x.Key.name)) {

                                    //If not showing templates then continue
                                    if (globalPortal.displayABCTemplates == false && expElement.Key.createdBy == "ABC" && expElement.Key.name.Contains("Template"))
                                        continue;

                                    //If not showing ABC element then continue
                                    if (globalPortal.displayABCElements == false && expElement.Key.createdBy == "ABC" && expElement.Key.name.Contains("Template") == false)
                                        continue;

                                    if (expElement.Key.elementType != ABC_GlobalElement.GlobalElementType.Effect || exportedElementCurrentSelectedEffectTag != 0 && expElement.Key.elementTags.Contains(this.exportedElementEffectTags[exportedElementCurrentSelectedEffectTag]) == false)
                                        continue;

                                    if (displayEffectCounter == 0 || displayEffectCounter % 7 == 0) {
                                        EditorGUILayout.BeginHorizontal();
                                    }

                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical();

                                    #region Exported Element
                                    InspectorElementHeader(expElement.Key.name, elementPanelWidthSeven, "", 11);

                                    InspectorSectionElementBox(elementPanelWidthSeven);

                                    Texture2D elementImage = null;


                                    elementImage = expElement.Key.elementIcon;

                                    GUI.contentColor = Color.white;

                                    GUILayout.Label(elementImage, GUILayout.Height(elementPanelWidthSeven), GUILayout.Width(elementPanelWidthSeven));

                                    if (EditorGUIUtility.isProSkin == false) {
                                        GUI.backgroundColor = inspectorBackgroundColor;
                                        GUI.contentColor = Color.black;
                                    }

                                    //Select scriptable object
                                    if (GUILayout.Button(new GUIContent("Info"), GUILayout.Width(elementPanelWidthSeven))) {
                                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(expElement.Value);
                                    }



                                    EditorGUILayout.EndVertical();
                                    #endregion

                                    EditorGUILayout.Space();

                                    EditorGUILayout.EndVertical();

                                    displayEffectCounter++;

                                    if (displayEffectCounter == 0 || displayEffectCounter % 7 == 0) {
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }

                                }


                                if (displayEffectCounter % 7 != 0) {

                                    for (int i = displayEffectCounter % 7; i < 7; i++) {

                                        GUILayout.Label("", GUILayout.Height(elementPanelWidthSeven - 5), GUILayout.Width(elementPanelWidthSeven - 5));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }
                                    EditorGUILayout.EndHorizontal();

                                }


                                EditorGUILayout.EndVertical();

                                EditorGUILayout.EndScrollView();

                                break;



                        }


                        #endregion


                        EditorGUILayout.EndVertical();


                        #endregion



                        EditorGUILayout.EndHorizontal();


                        break;

                    case 1:

                        EditorGUILayout.BeginHorizontal();

                        #region Left Side

                        EditorGUILayout.BeginVertical();

                        #region Attribution
                        InspectorSectionHeader("Creditation");

                        InspectorHeader("Weapon & Attack Animations");
                        InspectorSizedVerticalBox(450f);


                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("- US Studios");
                        InspectorLink("https://assetstore.unity.com/publishers/50743", 70);
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();

                        InspectorHeader("Elijah 3D Model");
                        InspectorSizedVerticalBox(450f);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("- Rutz Studios");
                        InspectorLink("https://assetstore.unity.com/packages/3d/characters/elijah-polygonal-modular-character-182985", 70);
                        InspectorLink("https://assetstore.unity.com/publishers/45909", 70);
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();

                        InspectorHeader("Magic Arsenal");
                        InspectorSizedVerticalBox(450f);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("- Archanor VFX");
                        InspectorLink("https://assetstore.unity.com/packages/vfx/particles/spells/magic-arsenal-20869", 70);
                        InspectorLink("https://assetstore.unity.com/publishers/8569", 70);
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();

                        InspectorHeader("The Community!");
                        InspectorSizedVerticalBox(450f);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Thanks to all the wonderful ABC Community for supporting and creating an amazing place to share and enjoy game development.", EditorStyles.wordWrappedLabel);
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Feel free to join the community on Discord:", EditorStyles.wordWrappedLabel);
                        InspectorLink("https://discord.com/invite/ZhAjYuy");
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Follow us on Twitter:", EditorStyles.wordWrappedLabel);
                        InspectorLink("https://twitter.com/ABCToolkit");
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();

                        #endregion

                        EditorGUILayout.EndVertical();
                        #endregion

                        InspectorBoldVerticleLine();

                        #region Right Side

                        EditorGUILayout.BeginVertical();

                        #region Integrations

                        InspectorSectionHeader("Integrations");

                        EditorGUILayout.Space();
                        InspectorSizedVerticalBox(350f);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("- Game Creator");
                        InspectorLink("https://assetstore.unity.com/packages/tools/game-toolkits/game-creator-89443");
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Tutorial Playlist by US Studios:");
                        InspectorLink("https://www.youtube.com/watch?v=ojWJ68Z3ilk&list=PL4nQzoXI-5QFL7KJp9q9EXrQarAlAZGj2");
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();

                        InspectorSizedVerticalBox(350f);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("- Game Creator 2");
                        InspectorLink("https://assetstore.unity.com/packages/tools/game-toolkits/game-creator-2-203069");
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();


                        InspectorSizedVerticalBox(350f);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("- Emerald AI");
                        InspectorLink("https://assetstore.unity.com/packages/tools/ai/emerald-ai-2-0-40199");
                        EditorGUILayout.Space();
                        EditorGUILayout.EndVertical();



                        #endregion

                        EditorGUILayout.EndVertical();
                        #endregion

                        EditorGUILayout.EndHorizontal();
                        break;


                }


                #endregion


            }

        }





        //Target update and applymodifiedproperties are in the inspector update call to reduce lag. 
        public void OnInspectorUpdate() {


            if (globalPortal != null) {
                //Apply the changes to our list if an update has been made

                //take current state of the SerializedObject, and updates the real object.
                GetTarget.ApplyModifiedProperties();

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(globalPortal);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTarget.Update();

            }

            if (GetTargetABCManager != null) {
                //Apply the changes to our list if an update has been made

                //take current state of the SerializedObject, and updates the real object.
                GetTargetABCManager.ApplyModifiedProperties();

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(abcManager);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTargetABCManager.Update();

            }

            if (GetTargetStateManager != null) {
                //Apply the changes to our list if an update has been made

                //take current state of the SerializedObject, and updates the real object.
                GetTargetStateManager.ApplyModifiedProperties();

                //Double check any list edits will get saved
                if (GUI.changed)
                    EditorUtility.SetDirty(stateManager);


                //Update our list (takes the current state of the real object, and updates the SerializedObject)
                GetTargetStateManager.Update();

            }

        }

    }
}