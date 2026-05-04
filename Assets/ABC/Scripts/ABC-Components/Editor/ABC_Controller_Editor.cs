using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal; 
using UnityEditor;
using System.Reflection;

namespace ABCToolkit {
    [CustomEditor(typeof(ABC_Controller))]



    public class ABC_Controller_Editor : Editor {

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


        public void InspectorHelpBox(string text, bool space = true) {
            if (abilityCont.showHelpInformation == true) {
                if (EditorGUIUtility.isProSkin) {
                    GUI.color = inspectorSectionHelpProColor;
                } else {
                    GUI.color = inspectorSectionHelpColor;
                }
                EditorGUILayout.HelpBox(text, MessageType.None, true);
            }
            GUI.color = Color.white;

            if (space == true)
                EditorGUILayout.Space();
        }

        public void InspectorSectionHeader(string text, string description = "") {
            GUIStyle myStyle = new GUIStyle("Button");
            Color originalTextColor = GUI.skin.button.normal.textColor;
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

        public void IntegrationButton(string IntegrationName, string IntegrationDefineSymbol, string HelpText = "") {
            EditorGUILayout.BeginHorizontal();

            if (ABC_Utilities.IntegrationDefineSymbolExists(IntegrationDefineSymbol)) {

                if (GUILayout.Button(new GUIContent(" Disable " + IntegrationName + " Integration"), GUILayout.Width(280)))
                    ABC_Utilities.RemoveIntegrationDefineSymbols(IntegrationDefineSymbol);

            } else {

                if (GUILayout.Button(new GUIContent(" Enable " + IntegrationName + " Integration"), GUILayout.Width(280))) {
                    ABC_Utilities.AddIntegrationDefineSymbols(IntegrationDefineSymbol);

                    if (IntegrationDefineSymbol == "ABC_GC_2_Stats_Integration") {
                        ABC_Utilities.ConvertGlobalElementsForGC2();
                    }
                }


            }

            EditorGUILayout.EndHorizontal();

            if (HelpText != "")
                InspectorHelpBox(HelpText);

        }



        // Button Icons
        Texture AddIcon;
        Texture RemoveIcon;
        Texture ExportIcon;
        Texture CopyIcon;
        Texture ImportIcon;
        Texture RefreshIcon;



        #endregion


        private ReorderableList reorderableListAbilities;
        private ReorderableList reorderableListAIRules;


        ABC_Controller abilityCont;
        SerializedObject GetTarget;
        SerializedProperty meAbilityList;



        public List<ABC_Ability> PreDefinedAbilities = new List<ABC_Ability>();




        void OnEnable() {

            abilityCont = (ABC_Controller)target;
            GetTarget = new SerializedObject(abilityCont);
            meAbilityList = GetTarget.FindProperty("Abilities"); // Find the List in our script and create a refrence of it 


            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");
            RefreshIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Refresh");


            #region Ability ReorderableList

            // ********************************** ReorderableList Ability *******************************************

            // reorderable list for abilities
            reorderableListAbilities = new ReorderableList(serializedObject,
                                                           meAbilityList,
                                                           true, true, false, false);

            // name the header
            reorderableListAbilities.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Quick Edit Abilities: ");
            };


            // when we select any of the list then it will set the current ability to show the ability details ready to be changed
            reorderableListAbilities.onSelectCallback = (ReorderableList l) => {
                abilityCont.CurrentAbility = l.index;

            };

            // when the + sign is called it will add a new ability
            reorderableListAbilities.onAddCallback = (ReorderableList l) => {
                // add standard defaults here
                ABC_Ability ability = new ABC_Ability();
                ability.abilityID = ABC_Utilities.GenerateUniqueID();
                abilityCont.Abilities.Add(ability);
            };


            // design of the reorderable list (with rect parameters we show in a line: active, name, recast time and key) have also added space for user to be able to select line
            reorderableListAbilities.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = reorderableListAbilities.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

            // draws empty box 
            //			EditorGUI.DrawRect(
            //				new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight),
            //				new Color(211F, 211F, 211F, 0.5F));

            EditorGUI.PropertyField(
                    new Rect(rect.x + 10, rect.y, 60, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("abilityEnabled"), GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 50, rect.y, rect.width - 250, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("name"), GUIContent.none);

                EditorGUI.PrefixLabel(
                    new Rect(rect.x + rect.width - 170, rect.y, 30, EditorGUIUtility.singleLineHeight),
                    1, new GUIContent("Export:"));

                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width - 120, rect.y, 30, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("enableExport"), GUIContent.none);

                if (element.FindPropertyRelative("scrollAbility").boolValue == false) {
                    EditorGUI.PropertyField(
                        new Rect(rect.x + rect.width - 85, rect.y, 70, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("key"), GUIContent.none);
                } else {
                    EditorGUI.PrefixLabel(
                        new Rect(rect.x + rect.width - 85, rect.y, 70, EditorGUIUtility.singleLineHeight),
                        1, new GUIContent("Scrollable"));
                }
            };

            #endregion



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
            if (GUILayout.Button("Ability Manager")) {
                // add standard defaults here
                var window = EditorWindow.GetWindow(typeof(ABC_ControllerAbility_EditorWindow), false);
                window.maxSize = new Vector2(1025f, 660f);
                window.minSize = window.maxSize;
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Controller Manager")) {
                // add standard defaults here
                var window = EditorWindow.GetWindow(typeof(ABC_Controller_EditorWindow), false);
                window.maxSize = new Vector2(855f, 660f);
                window.minSize = window.maxSize;


            }

            EditorGUILayout.Space();

            if (Application.isPlaying) {
                if (GUILayout.Button(new GUIContent(" Refresh ABC", RefreshIcon), GUILayout.Width(150))) {
                    ABC_Utilities.ReloadAllABCEntities();
                }
                InspectorHelpBox("Will refresh ABC during play. Reloading any changes made to graphics or global settings");
            }


            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(GetTarget.FindProperty("isChildInHierarchy"), new GUIContent("Child In Hierarchy"));
            InspectorHelpBox("Tick if ABC controller has been set on a child object (useful for making an ABC prefab to be used globally on other entities)");


            EditorGUIUtility.labelWidth = 190;

            EditorGUILayout.PropertyField(GetTarget.FindProperty("updateEditorAtRunTime"));

            InspectorHelpBox("Will decrease game performance but update editors realtime");





            EditorGUILayout.PropertyField(GetTarget.FindProperty("enableTagConversions"));
            if (GetTarget.FindProperty("enableTagConversions").boolValue == true) {

                EditorGUILayout.BeginVertical("Box", GUILayout.Width(350));

                if (GUILayout.Button(new GUIContent(" Add Tag Conversion", "Add Tag Conversion"), GUILayout.Width(360))) {
                    // add standard defaults here
                    abilityCont.tagConversions.Add(new ABC_Utilities.TagConverter());
                }


                SerializedProperty tagConversions = GetTarget.FindProperty("tagConversions");

                for (int i = 0; i < tagConversions.arraySize; i++) {

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Replace Tag ", GUILayout.Width(80));

                    EditorGUILayout.PropertyField(tagConversions.GetArrayElementAtIndex(i).FindPropertyRelative("tagBefore"), new GUIContent(""), GUILayout.Width(100));

                    EditorGUILayout.LabelField("With ", GUILayout.Width(40));
                    EditorGUILayout.PropertyField(tagConversions.GetArrayElementAtIndex(i).FindPropertyRelative("tagAfter"), new GUIContent(""), GUILayout.Width(100));

                    GUI.skin.button.normal.textColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(30))) {
                        tagConversions.DeleteArrayElementAtIndex(i);
                    }
                    GUI.skin.button.normal.textColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

            }

            InspectorHelpBox("Will convert ABC tags setup into a new tag during play. Allowing you for example to change all 'Enemy' defined tags to 'Player'. This is applied to all Tags setup in ABC Controller/Abilities/StateManager. Changes are done in real time and" +
                " are not shown in editor.");

            EditorGUILayout.Space();

            ResetLabelWidth();


            GetTarget.FindProperty("foldOutIntegration").boolValue = EditorGUILayout.Foldout(GetTarget.FindProperty("foldOutIntegration").boolValue, "Integrations");

            if (GetTarget.FindProperty("foldOutIntegration").boolValue == true) {

                EditorGUILayout.Space();
                IntegrationButton("Game Creator", "ABC_GC_Integration");
                IntegrationButton("Game Creator Stats", "ABC_GC_Stats_Integration", "Click to enable / disable integration with Game Creator");

                //if (AssetDatabase.FindAssets("ABC_GameCreator2Utilities").Count() > 0) {
                EditorGUILayout.Space();
                IntegrationButton("Game Creator 2", "ABC_GC_2_Integration");
                IntegrationButton("Game Creator 2 Stats", "ABC_GC_2_Stats_Integration", "Click to enable / disable integration with Game Creator 2. Requires the separate integration package on the Asset Store.");
                //}

                EditorGUILayout.Space();
                IntegrationButton("Animancer", "ABC_Animancer_Integration", "Click to enable / disable integration with Animancer");

                EditorGUILayout.Space();
                IntegrationButton("Emerald AI", "ABC_EmeraldAI_Integration", "Click to enable / disable integration with Emerald AI");
            }


            EditorGUILayout.Space();


            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();

        }
    }
}