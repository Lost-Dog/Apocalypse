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
    public class ABC_WelcomeHub_EditorWindow : EditorWindow {

        [MenuItem("Window/ABC/Welcome Hub")]

        public static void ShowWindow() {

            EditorWindow window = EditorWindow.GetWindow(typeof(ABC_WelcomeHub_EditorWindow), false);
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
        public int minimumAllWaySectionWidth = (int)windowWidth;

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

        public void InspectorLink(string text, string URL, int length = 50) {

            int maxString = length;
            string url = URL;

            if (text.Length > maxString)
                text = text.Substring(0, maxString) + "...";

            if (GUILayout.Button(text, EditorStyles.linkLabel)) {
                Application.OpenURL(url);
            }

        }

        public void InspectorImageLink(Texture Image, string URL, int height = 50, int width = 50) {

            GUI.contentColor = Color.white;

            if (GUILayout.Button(Image, EditorStyles.linkLabel, GUILayout.MinHeight(height), GUILayout.Width(width))) {
                Application.OpenURL(URL);
            }

            if (EditorGUIUtility.isProSkin == false) {
                GUI.backgroundColor = inspectorBackgroundColor;
                GUI.contentColor = Color.black;
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


        public void InspectorBoldHorizontalLine() {
            GUI.color = Color.white;
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2f) });
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

        public void IntegrationButton(string IntegrationName, string IntegrationDefineSymbol, string HelpText = "") {
            EditorGUILayout.BeginHorizontal();

            if (ABC_Utilities.IntegrationDefineSymbolExists(IntegrationDefineSymbol)) {

                if (GUILayout.Button(new GUIContent(" Disable " + IntegrationName + " Integration"), GUILayout.Width(220)))
                    ABC_Utilities.RemoveIntegrationDefineSymbols(IntegrationDefineSymbol);

            } else {

                if (GUILayout.Button(new GUIContent(" Enable " + IntegrationName + " Integration"), GUILayout.Width(220))) {
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

        //Images 

        Texture ABCWelcomeImage;
        Texture DSExpansion;
        Texture BowExpansion;
        Texture GC2Expansion;

        Vector2 editorScrollPos;
        Vector2 listScrollPos;

        GUIStyle textureButton = new GUIStyle();

        #endregion

        GameObject CharacterObject;

        ABC_GlobalPortal globalPortal;
        SerializedObject GetTarget;


        public void OnEnable() {

            globalPortal = (ABC_GlobalPortal)Resources.Load("ABC-GlobalPortal/GlobalPortal");
            GetTarget = new SerializedObject(globalPortal);

            if (GetTarget != null) {
                GUIContent titleContent = new GUIContent("Welcome Hub");
                GetWindow<ABC_WelcomeHub_EditorWindow>().titleContent = titleContent;
            }

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");
            RemoveIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Remove");
            CopyIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Copy");
            ExportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Export");
            ImportIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Import");


            globalIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Global");
            noteIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Note");

            ABCWelcomeImage = (Texture2D)Resources.Load("ABC-EditorIcons/Logo");
            DSExpansion = (Texture2D)Resources.Load("ABC-EditorIcons/DSExpansion");
            BowExpansion = (Texture2D)Resources.Load("ABC-EditorIcons/BowExpansion");
            GC2Expansion = (Texture2D)Resources.Load("ABC-EditorIcons/GC2Expansion");


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

                //EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(windowWidth - 10));
                ////GUILayout.Label(Resources.Load("ABC-EditorIcons/logo", typeof(Texture2D)) as Texture2D, GUILayout.MaxWidth(4));
                //globalPortalToolbarSelection = GUILayout.Toolbar(globalPortalToolbarSelection, globalPortalToolbar, GUILayout.Height(31));
                //EditorGUILayout.EndHorizontal();
                //EditorGUILayout.Space();


                #endregion

                #region Body

                if (EditorGUIUtility.isProSkin) {
                    GUI.backgroundColor = inspectorBackgroundProColor;
                    GUI.contentColor = Color.white;
                } else {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }


                #region 1st Panel

                InspectorSectionHeader("Welcome to ABC!");

                EditorGUILayout.BeginHorizontal();

                #region SideBySide

                #region Logo

                InspectorSizedVerticalBox(70);

                GUI.contentColor = Color.white;

                GUILayout.Label(ABCWelcomeImage, GUILayout.Width(150), GUILayout.Height(160));


                if (EditorGUIUtility.isProSkin == false) {
                    GUI.backgroundColor = inspectorBackgroundColor;
                    GUI.contentColor = Color.black;
                }

                EditorGUILayout.EndVertical();

                #endregion


                #region Welcome Text

                InspectorSizedVerticalBox(40);

                InspectorHeader("Thank you for supporting ABC - Ability & Combat Toolkit!", false);


                GUILayout.Label("Our mission is to create the best tool for adding combat related functionality to your games whilst supporting an amazing community of game makers. \n\nIf possible we would be forever grateful if you could leave ABC a review which really helps support us and provides valuable feedback!", GUILayout.Width(350), GUILayout.Height(115));

                InspectorLink("Write a Review", "https://assetstore.unity.com/packages/tools/game-toolkits/ability-combat-toolkit-155168#reviews");


                EditorGUILayout.EndVertical();

                #endregion



                #region Welcome Text

                InspectorSizedVerticalBox(300);

                InspectorHeader("Join The ABC Community!", false);


                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Come give us a wave on Discord:");
                InspectorLink("Discord", "https://discord.com/invite/ZhAjYuy");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Follow us on Twitter & Youtube:");
                InspectorLink("Twitter", "https://twitter.com/ABCToolkit");
                InspectorLink("Youtube", "https://www.youtube.com/channel/UCYkxwqvv3NZvwc96Sv2lzwg/featured");
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                InspectorBoldHorizontalLine();


                EditorGUILayout.EndVertical();

                #endregion

                #endregion

                EditorGUILayout.EndHorizontal();

                #endregion

                InspectorBoldHorizontalLine();

                #region 2nd Panel

                InspectorHeader("ABC Expansions - Click an expansion to be directed to the Unity Asset Store", false);

                EditorGUILayout.BeginHorizontal();


                #region SideBySide

                EditorGUILayout.Space();

                #region Pack 1

                EditorGUILayout.BeginVertical();

                InspectorImageLink(DSExpansion, "https://assetstore.unity.com/packages/tools/game-toolkits/dual-swords-abc-expansion-218525?aid=1011lcMhd", 200, 220);

                EditorGUILayout.EndVertical();

                #endregion

                EditorGUILayout.Space();

                #region Pack 2

                EditorGUILayout.BeginVertical();

                InspectorImageLink(BowExpansion, "https://assetstore.unity.com/packages/tools/game-toolkits/bow-arrow-abc-expansion-227344?aid=1011lcMhd", 200, 220);

                EditorGUILayout.EndVertical();

                #endregion

                EditorGUILayout.Space();

                #region Pack 3

                EditorGUILayout.BeginVertical();

                InspectorImageLink(GC2Expansion, "https://assetstore.unity.com/packages/tools/game-toolkits/abc-integration-game-creator-2-218546?aid=1011lcMhd", 200, 220);

                EditorGUILayout.EndVertical();

                #endregion

                EditorGUILayout.Space();


                #endregion

                EditorGUILayout.EndHorizontal();

                #endregion

                InspectorBoldHorizontalLine();

                #region 3rd Panel

                EditorGUILayout.BeginHorizontal();

                #region SideBySide

                #region Welcome Text

                InspectorSizedVerticalBox(500);

                InspectorHeader("Getting Started", false);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("- To start you on your journey with ABC please check out the", EditorStyles.wordWrappedLabel);

                InspectorLink("Documentation", "https://dicelockstudio.gitbook.io/abc-toolkit/");
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();



                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("- Open the ABC Global Portal to add, create or edit weapons/abilities/effects and setup characters", EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Global Portal", GUILayout.Width(220))) {
                    // add standard defaults here
                    EditorWindow window = EditorWindow.GetWindow(typeof(ABC_GlobalPortal_EditorWindow), false);
                    window.maxSize = new Vector2(windowWidth, windowHeight);
                    window.minSize = window.maxSize;
                }


                if (AssetDatabase.FindAssets("ABC_GameCreator2Utilities").Count() > 0) {
                    EditorGUILayout.Space();
                    IntegrationButton("GC 2", "ABC_GC_2_Integration");
                }


                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();



                #endregion



                #region Welcome Text

                InspectorSizedVerticalBox(300);

                InspectorHeader("Tutorials", false);
                EditorGUILayout.LabelField("Click below for tutorials on ABC", EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space();
                InspectorLink("- Official ABC Tutorials", "https://www.youtube.com/playlist?list=PLMHULhgHh-TzVS-QjviDkr3ZPc1hbZa_H");
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                InspectorLink("- GC 2 Integration Tutorials by US Studios", "https://www.youtube.com/watch?v=2qjHlaZXIS0&list=PL4nQzoXI-5QFTLJfeLwqaLwZ7iiracYQE");
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
                #endregion

                #endregion

                EditorGUILayout.EndHorizontal();

                #endregion



                EditorGUIUtility.labelWidth = 180;
                EditorGUILayout.PropertyField(GetTarget.FindProperty("displayWelcomeOnStartUp"), new GUIContent("Show This Panel On Start-Up"), GUILayout.MaxWidth(250));
                EditorGUIUtility.labelWidth = 110;
                EditorGUILayout.Space();


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

        }


        [InitializeOnLoad]
        class StartupHelper {
            static StartupHelper() {
                EditorApplication.update += Startup;
            }
            static void Startup() {
                EditorApplication.update -= Startup;

                ABC_GlobalPortal globalPortal = (ABC_GlobalPortal)Resources.Load("ABC-GlobalPortal/GlobalPortal");

                if (globalPortal.displayWelcomeOnStartUp == true && EditorApplication.isPlaying == false && EditorApplication.timeSinceStartup < 300)
                    ShowWindow();
            }
        }




    }
}