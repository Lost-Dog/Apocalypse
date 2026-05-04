using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using MxMGameplay;
using UnityEditorInternal;
using System;
using Threepeat;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using MxM;
using MxMEditor;
using UnityEngine.AI;
#if (ENABLE_INPUT_SYSTEM)
using UnityEngine.InputSystem;
#endif

namespace ThreepeatEditor
{

    public class MMCConfigWizardWindow : UnityEditor.EditorWindow
    {
        [MenuItem("Tools/Threepeat/MMC Configuration Wizard")]
        static void CreateMMCConfigWizardWindow()
        {
            GetWindowInstance();   
        }

        static MMCConfigWizardWindow instance;

        public static MMCConfigWizardWindow GetWindowInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = EditorWindow.GetWindow<MMCConfigWizardWindow>("MMC Configuration Wizard");
            return instance;
        }

        [SerializeField] private GameObject characterModel;
        [SerializeField] private GameObject wantedSceneRootObject;
        [SerializeField] private GameObject wantedSceneModelParent;

        [SerializeField] private NGInputSchemeBase overrideInputScheme = null;
        [SerializeField] private NGCharacterBaseConfig overrideCharacterConfig = null;

        [SerializeField] public LayerMask groundLayers = new LayerMask();

        //[SerializeField] private bool useDecoupledController = false;
        [SerializeField] public bool addAudioSourceToRootObject = true;
        [SerializeField] public bool addCinemachineToScene = false;
        [SerializeField] private bool useCharacterControllerForNPC = true;
        [SerializeField] public bool addMxMSearchManagerToScene = true;
        //[SerializeField] private bool useMxMFootIK = true;

        [SerializeField] private int selectedTab;

        [SerializeField] public bool autoRecheckDependenciesOnImport = true;
        [SerializeField] public bool hideUnavailableOptionalDependencies = false;

        public const string PLAYERPREF_AUTOCHECK = "MMLC_AutoCheckDependencies";

        public class RecheckDependenciesAssetPostProcessor : AssetPostprocessor
        {
            static MMCConfigWizardWindow wiz = null;
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
            {
                if (!PlayerPrefs.HasKey(PLAYERPREF_AUTOCHECK) || 
                    (PlayerPrefs.GetInt(PLAYERPREF_AUTOCHECK) != 1))
                {
                    return;
                }
                if (!EditorWindow.HasOpenInstances<MMCConfigWizardWindow>())
                {
                    return;
                }
                if ((importedAssets.Length > 0) || (deletedAssets.Length > 0))
                {
                    if (wiz == null)
                    {
                        wiz = MMCConfigWizardWindow.GetWindowInstance();
                    }
                    /*if (EditorWindow.focusedWindow != wiz)
                    {
                        return;
                    }*/
                    //Debug.LogFormat("Called! {0} {1} {2}", string.Join(",", importedAssets), deletedAssets.Length, didDomainReload);
                    
                    if (wiz.autoRecheckDependenciesOnImport)
                    {
                        wiz.CheckIntegrationStatus();
                    }
                }
            }
        }



        public enum CharacterIntegrationType
        {
            //ExistingSceneObject,
            [InspectorName("New Character from Simple Prefab")]
            NewCharacterFromSimplePrefab,

            [InspectorName("New Character from Complex Scene Hierarchy")]
            NewCharacterFromComplexSceneHierarchy
            ,
            [InspectorName("Update Existing Character (Experimental)")]
            UpdateExistingCharacter
        }

        public enum CharacterInputScheme
        {
#if (ENABLE_INPUT_SYSTEM)
            [InspectorName("InputSystem (Player)")]
            InputSystem=0,
#endif
            [InspectorName("InputManager (Player)")]
            InputManager=1,

            [InspectorName("NavMesh (NPC)")]
            NavMesh=2
        }

        [SerializeField] private CharacterIntegrationType integrationType = CharacterIntegrationType.NewCharacterFromSimplePrefab;

#if (ENABLE_INPUT_SYSTEM)
        [SerializeField] private CharacterInputScheme inputSchemeToUse = CharacterInputScheme.InputSystem;
#else
        [SerializeField] private CharacterInputScheme inputSchemeToUse = CharacterInputScheme.InputManager;
#endif
        public enum EditorTabs
        {
            DependencyChecks = 0,
            /*CustomizeBehavior,*/
            SetupCharacter,
            /*SetupAnimation,*/
            Help
        };
        private string[] tabs = { "1. Dependency Checks", /*"Customize Behavior",*/ "2. Setup Character", /*"Custom Anim Configs",*/ "Help and Utilities" };

        /*
        [SerializeField] private bool doSetupGrounder = false;
        [SerializeField] private int grounderOnlyLayer = 10;
        [SerializeField] private int grounderIgnoreLayer = 11;
        */

        private GameObject rootObject = null;
        private GameObject instantiatedModelObject = null;

        private bool showFinalIK = true;

        private bool showGeneralConfig = true;

        private bool showOptionalConfig = true;

        [SerializeField]
        private MMCFinalIKGrounderIntegration grounderIntegration = new MMCFinalIKGrounderIntegration();

        [SerializeField]
        private MMCStriderIntegration striderIntegration = new MMCStriderIntegration();

        private GUIStyle SectionHeaderStyle = null;
        private GUIStyle SectionHeaderStyleThin = null;

        private bool integrationsChecked = false;
        private bool integrationsFinalIK = false;
        private bool integrationsStrider = false;

        private bool showHelpFinalIK = false;

        [SerializeField] private List<InfoChecker> dependencyInfos = new List<InfoChecker>();
        [SerializeField] private List<string> dependencyInfoHeaders = new List<string>();
        [SerializeField] private List<MMCIntegrationBase> customIntegrationsAvailable = new List<MMCIntegrationBase>();
        [SerializeField] private List<MMCIntegrationBase> customIntegrationsUnmetDependencies = new List<MMCIntegrationBase>();
        private Dictionary<string, bool> customIntegrationsMajorToggle = new Dictionary<string, bool>();

        [SerializeField] private List<FBXAnimationEventInfo> fbxModifiers = new List<FBXAnimationEventInfo>();

        private MMCConfigWizardHelpTab helpTab = null;
        private MMCConfigWizardAnimConfigTab animConfigTab = null;

        public MMCIntegrationBase GetAvailableIntegration(System.Type integrationType)
        {
            foreach (MMCIntegrationBase integration in customIntegrationsAvailable)
            {
                if (integration.GetType() == integrationType)
                {
                    return integration;
                }
            }
            return null;
        }

        private void OnGUI()
        {
            if (helpTab == null)
            {
                helpTab = new MMCConfigWizardHelpTab(this);
            }

            if (animConfigTab == null)
            {
                animConfigTab = new MMCConfigWizardAnimConfigTab(this);
            }

            //TODO: reenable this: if (SectionHeaderStyle == null) {
            SetupStyles();

            if (!integrationsChecked)
            {
                CheckIntegrationStatus();
            }
            // }

            Scene activeScene = SceneManager.GetActiveScene();

            EditorGUILayout.Space();

            int origSelected = selectedTab;
            //GUIStyle s = new GUIStyle(EditorStyles.toolbarButton);
            selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.MinHeight(50));

            EditorGUILayout.Space();

            switch (selectedTab)
            {
                case (int)EditorTabs.DependencyChecks:
                    if (origSelected != selectedTab)
                    {
                        CheckIntegrationStatus();
                    }
                    DrawDependencyChecks();
                    break;

                /*Future:
                case (int)EditorTabs.CustomizeBehavior:
                    DrawCustomizeBehavior();
                    break;*/

                case (int)EditorTabs.SetupCharacter:
                    DrawCharacterSetupGUI();
                    break;

                /*case (int)EditorTabs.SetupAnimation:
                    animConfigTab.DrawGUI();
                    break;*/

                case (int)EditorTabs.Help:
                    helpTab.DrawGUI();
                    break;

            }

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
            EditorGUILayout.LabelField("Active Scene: " + activeScene.name);

        }

        [System.Serializable]
        public class InfoChecker
        {

            public bool guidsToCheckStatus;
            public bool integrationPackageStatus;
            public bool dependencyPackageStatus;
            public List<string> missingDependencyPackages;
            public bool doShowHelp;

            public InfoChecker(ThreepeatDependencyInfo cfgInfo)
            {
                info = cfgInfo;
                integrationPackageStatus = false;
                guidsToCheckStatus = false;
                dependencyPackageStatus = false;
                doShowHelp = false;
            }
            public ThreepeatDependencyInfo info;
            public bool IsDependencyMet()
            {
                bool retval = true;

                if (info.HasOverriddenDependencyCheckFunction())
                {
                    return info.OverriddenDependencyCheck_IsDependencyMet();
                }
                if (info.GUIDsToCheck.Length > 0)
                {
                    retval = retval && guidsToCheckStatus;
                }
                if (info.hasIntegrationPackage)
                {
                    retval = retval && integrationPackageStatus;
                }
                if (info.dependencyPackages.Length > 0)
                {
                    retval = retval && dependencyPackageStatus;
                }

                return retval;
            }

            public void RunCheck()
            {
                RunCheck_DependencyPackages();
                RunCheck_GUIDs();
                RunCheck_IntegrationPackage();
            }

            public void RunCheck_DependencyPackages()
            {
                if (info.dependencyPackages.Length == 0)
                {
                    dependencyPackageStatus = true;
                    return;
                }
                // strip packages by version:
                List<string> strippedPackages = new List<string>();

                foreach (string deppkg in info.dependencyPackages)
                {
                    if (deppkg.Contains("com.unity.jobs") && (string.Compare(Application.unityVersion, "2022", StringComparison.Ordinal) >= 0))
                    {
                        //Debug.LogFormat("jobs check: currver( {0}, {1} )", Application.unityVersion, string.Compare(Application.unityVersion, "2022", StringComparison.Ordinal));
                        //Debug.LogFormat("jobs check: currver( {0}, {1} )", Application.unityVersion, string.Compare(Application.unityVersion, "2021.3.7f1", StringComparison.Ordinal));
                        continue;
                    }
                    strippedPackages.Add(deppkg);
                }
                //ListRequest req = Client.List(true);
                CheckPackages cpack = new CheckPackages();
                //cpack.CheckForPackages(info.dependencyPackages, DepCheckCompletionCallback);
                cpack.CheckForPackages(strippedPackages.ToArray(), DepCheckCompletionCallback);
            }

            private void DepCheckCompletionCallback(bool retrievalSuccess, List<string> packagesMissingFromProject)
            {
                //Debug.Log("Got completion callback!");
                dependencyPackageStatus = retrievalSuccess && (packagesMissingFromProject.Count == 0);
                missingDependencyPackages = packagesMissingFromProject;
                MMCConfigWizardWindow.GetWindowInstance().Repaint();
                //throw new NotImplementedException();
            }

            bool CheckGuidInProject(string theGuid)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(theGuid);
                if (assetPath == "")
                {
                    return false;
                }
                /*if (!System.IO.File.Exists(assetPath))
                {
                    Debug.LogFormat("assetPath doesn't exist: ({0})", assetPath);
                }*/
                return System.IO.File.Exists(assetPath);
            }

            public bool RunCheck_GUIDs()
            {
                guidsToCheckStatus = true;

                if (info.GUIDsToCheck.Length == 0)
                {
                    return guidsToCheckStatus;
                }
                for (int ii=0; ii < info.GUIDsToCheck.Length; ii++)
                {
                    guidsToCheckStatus = guidsToCheckStatus && CheckGuidInProject(info.GUIDsToCheck[ii]);
                    if (guidsToCheckStatus)
                    {
                        Debug.LogFormat("Guid found( {0} )", info.GUIDsToCheck[ii]);
                    }
                }
                return guidsToCheckStatus;
            }

            public bool RunCheck_IntegrationPackage()
            {
                integrationPackageStatus = true;
                if (!info.hasIntegrationPackage)
                {
                    return integrationPackageStatus;
                }

                integrationPackageStatus = CheckGuidInProject(info.integrationPackageGUIDToCheck);

                return integrationPackageStatus;
            }


            /* CheckPackages class is based loosely on: 
             *      https://forum.unity.com/threads/detect-if-a-package-is-installed.1100338/
             */
            [InitializeOnLoad]
            public class CheckPackages
            {
                ListRequest Request;

                public delegate void CompletionCallback(bool retrievalSuccess, List<string> packagesMissingFromProject);

                CompletionCallback completionCallback;

                string[] packagesToCheck;
                
                public void CheckForPackages(string[] cfgPackagesToCheck, CompletionCallback cb)
                {
                    packagesToCheck = cfgPackagesToCheck;
                    completionCallback = cb;
                    Request = Client.List(offlineMode: true);
                    EditorApplication.update += progress;
                }

                void progress()
                {
                    List<string> missingPackages = new List<string>();
                    //string packageId = "com.unity.cinemachine";
                    if (Request.IsCompleted)
                    {
                        if (Request.Status == StatusCode.Success)
                        {
                            foreach (string packageId in packagesToCheck)
                            {
                                if (!ContainsPackage(Request.Result, packageId))
                                {
                                    Debug.LogWarning("Package '" + packageId + "' is missing");
                                    missingPackages.Add(packageId);
                                }
                                else
                                {
                                    Debug.Log("Package '" + packageId + "' found");
                                }
                            }
                        }
                        else if (Request.Status >= StatusCode.Failure)
                        {
                            Debug.Log("Could not check for packages: " + Request.Error.message);
                        }

                        EditorApplication.update -= progress;

                        completionCallback.Invoke(Request.Status == StatusCode.Success, missingPackages);
                        //Debug.Log("Check completed successfully");
                    }
                }

                public static bool ContainsPackage(PackageCollection packages, string packageId)
                {
                    foreach (var package in packages)
                    {
                        if (string.Compare(package.name, packageId) == 0)
                            return true;

                        foreach (var dependencyInfo in package.dependencies)
                            if (string.Compare(dependencyInfo.name, packageId) == 0)
                                return true;
                    }

                    return false;
                }
            }

        }

        private void GatherFBXModifiers()
        {
            fbxModifiers.Clear();

            string[] tmpGuids = AssetDatabase.FindAssets("t:FBXAnimationEventInfo");

            for (int ii = 0; ii < tmpGuids.Length; ii++)
            {
                FBXAnimationEventInfo theAsset =
                        AssetDatabase.LoadAssetAtPath<FBXAnimationEventInfo>(
                                        AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                if (theAsset.name.Contains("IGNORE_"))
                {
                    continue;
                }
                fbxModifiers.Add(theAsset);
                Debug.LogFormat("GatherFBXModifiers: found {0}", theAsset.name);
            }
        }

        private void GatherCustomIntegrations()
        {
            customIntegrationsAvailable.Clear();
            customIntegrationsMajorToggle.Clear();
            customIntegrationsUnmetDependencies.Clear();

            string[] tmpGuids = AssetDatabase.FindAssets("t:MMCIntegrationBase");

            for (int ii = 0; ii < tmpGuids.Length; ii++)
            {
                MMCIntegrationBase theAsset =
                        AssetDatabase.LoadAssetAtPath<MMCIntegrationBase>(
                                        AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                if (theAsset.name.Contains("IGNORE_"))
                {
                    continue;
                }

                //dependencyInfos.Find(s => s.info.)
                bool dependenciesMet = true;
                int foundCount = 0;
                if (theAsset.dependencies != null)
                {
                    foreach (ThreepeatDependencyInfo adep in theAsset.dependencies)
                    {
                        InfoChecker checker = dependencyInfos.Find(s => s.info == adep);
                        if (checker != null)
                        {
                            foundCount++;
                            Debug.LogFormat("Found dependency match for {0}", theAsset.name);
                            if (!checker.IsDependencyMet())
                            {
                                dependenciesMet = false;
                                break;
                            }
                        }
                    }
                    if (foundCount < theAsset.dependencies.Count)
                    {
                        dependenciesMet = false;
                    }
                }


                theAsset.selected = false;
                if (dependenciesMet)
                {
                    customIntegrationsAvailable.Add(theAsset);
                    customIntegrationsMajorToggle[theAsset.GetIntegrationName()] = false;
                }
                else
                {
                    customIntegrationsUnmetDependencies.Add(theAsset);
                }
                
            }
            Debug.LogFormat("Found {0} unmet and {1} available custom MMLC integration{2}.", 
                    customIntegrationsUnmetDependencies.Count, customIntegrationsAvailable.Count, (customIntegrationsUnmetDependencies.Count+customIntegrationsAvailable.Count) >= 2 ? "s" : "") ;
        }

        private void CheckIntegrationStatus()
        {
            dependencyInfoHeaders.Clear();
            //GatherFBXModifiers();
            dependencyInfos.Clear();
            //if (dependencyInfos.Count == 0)
            string[] tmpGuids = AssetDatabase.FindAssets("t:ThreepeatDependencyInfo");

            Debug.LogFormat("Found {0} infos", tmpGuids.Length);

            // check Final IK and Strider
            grounderIntegration = new MMCFinalIKGrounderIntegration();
            striderIntegration = new MMCStriderIntegration();

            integrationsFinalIK = !grounderIntegration.IsPlaceholder();
            integrationsStrider = !striderIntegration.IsPlaceholder();

            dependencyInfoHeaders.Add("EMPTY");
            for (int ii=0; ii < tmpGuids.Length; ii++)
            {
                ThreepeatDependencyInfo theAsset =
                        AssetDatabase.LoadAssetAtPath<ThreepeatDependencyInfo>(
                                        AssetDatabase.GUIDToAssetPath(tmpGuids[ii]));

                if (theAsset.name.Contains("IGNORE_"))
                {
                    continue;
                }

                if (theAsset.onlyShowWhenShowIfGuidIsPresent && (!String.IsNullOrEmpty(theAsset.showIfGuid))) 
                {
                    if (String.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(theAsset.showIfGuid)))
                    {
                        continue;
                    }
                }

                if ((theAsset.MAJOR_SECTION_NAME.Length > 0) && (!dependencyInfoHeaders.Contains(theAsset.MAJOR_SECTION_NAME))) 
                {
                    dependencyInfoHeaders.Add(theAsset.MAJOR_SECTION_NAME);
                }

                InfoChecker infoChecker = 
                        new InfoChecker(theAsset);

                infoChecker.RunCheck();

                if (theAsset.integrationName.Contains("Strider 2") && infoChecker.IsDependencyMet())
                {
                    // strider thinks it's good, but let's make sure it's not a placeholder:
                    if (striderIntegration.IsPlaceholder())
                    {
                        // strider integration package not installed
                        infoChecker.integrationPackageStatus = false;
                    }
                }

                dependencyInfos.Add(infoChecker);
                
            }

            GatherCustomIntegrations();

            integrationsChecked = true;

            if (EditorWindow.focusedWindow == this)
            {
                //Debug.Log("Focused window");
                GetWindowInstance().Repaint();
            }
        }

        public string GetGuidFromManagerKey(string v)
        {
            return MMCConfigWizardUtilities.GetGuidFromManagerKey(ref guidMgr, v);
        }

        private MMCConfigWizardGUIDManager guidMgr = null;

        private Vector2 scrollPosDependencyTab = Vector2.zero;
        private Vector2 scrollPosSetupTab = Vector2.zero;

        private bool simulateAllDepsBad = false;

        private void DrawDependencyChecks()
        {
            scrollPosDependencyTab = EditorGUILayout.BeginScrollView(scrollPosDependencyTab);
            GUIStyle s = new GUIStyle(EditorStyles.label);

            foreach (string section in dependencyInfoHeaders)
            {
                bool isEmpty = section.Equals("EMPTY");

                string sectionName = isEmpty ? "Core MMLC" : section;

                EditorGUILayout.Space();

                EditorGUILayout.BeginFoldoutHeaderGroup(true, sectionName, SectionHeaderStyleThin);

                foreach (InfoChecker depInfo in dependencyInfos)
                {
                    if (isEmpty && (depInfo.info.MAJOR_SECTION_NAME.Length <= 0))
                    {
                        // we are good
                    }
                    else if (depInfo.info.MAJOR_SECTION_NAME.Equals(section))
                    {
                        // we are good
                    }
                    else
                    {
                        continue;
                    }
                    string intString = depInfo.info.integrationName;

                    bool striderMetIfStrider = !depInfo.info.integrationName.Contains("Strider 2") || integrationsStrider;
                    bool thisIsTheSecondStrider = depInfo.info.integrationName.Contains("Strider 2");
                    //if (depInfo.IsDependencyMet() && striderMetIfStrider)

                    if (!simulateAllDepsBad && (depInfo.IsDependencyMet() || (thisIsTheSecondStrider && striderMetIfStrider)))
                    {
                        s.normal.textColor = Color.green;
                        EditorGUILayout.LabelField(" \u2713 " + intString + ": Found \u2192 GOOD", s);
                    }
                    else
                    {
                        string errString = "Not Found";
                        if (depInfo.info.requiredForMMCFunction)
                        {
                            s.normal.textColor = Color.red;
                        }
                        else
                        {
                            if (hideUnavailableOptionalDependencies)
                            {
                                continue;
                            }
                            s.normal.textColor = Color.gray;
                            intString += " (Optional)";
                        }
                        errString = "Status: ";
                        int errcount = 0;
                        if (!depInfo.guidsToCheckStatus)
                        {
                            errString += "Main Pkg not installed";
                            errcount++;
                        }
                        if (!depInfo.integrationPackageStatus)
                        {
                            if (errcount > 0)
                            {
                                errString += ", ";
                            }
                            errString += "Integration Pkg not installed";
                        }
                        if (!depInfo.dependencyPackageStatus)
                        {
                            if (errcount > 0)
                            {
                                errString += ", ";
                            }
                            errString += "Dependencies not installed";
                        }

                        if (depInfo.info.integrationName.Contains("Strider 2"))
                        {
                            errString = "Strider/MMLC Integration Pkg not installed";
                        }

                        s.fontStyle = FontStyle.Bold;
                        depInfo.doShowHelp = ThreepeatEditorGUIUtilities.LabelWithHelp.LabelWithHelpField(" \u2715 " + intString + ": " + errString, depInfo.info.notFoundHelpBoxText, depInfo.doShowHelp, null, s, depInfo.info.showOpenPackageManagerButton, depInfo.info.showOpenProjectSettingsButton, depInfo.info.integrationPackageGUIDOfUnextractedPackage);
                        if (depInfo.info.HasOverriddenRemediationButton() != null)
                        {
                            if (GUILayout.Button(depInfo.info.HasOverriddenRemediationButton()))
                            {
                                depInfo.info.OverriddenRemediate();
                            }
                        }

                    }
                } // foreach dep info
                EditorGUILayout.EndFoldoutHeaderGroup();
            } // for each major section

            EditorGUILayout.Space();

            EditorGUILayout.BeginFoldoutHeaderGroup(true, "Final IK", SectionHeaderStyleThin);

            if (integrationsFinalIK)
            {
                s.normal.textColor = Color.green;
                EditorGUILayout.LabelField(" \u2713 Final IK: Found \u2192 Integration Ready", s);
            }
            else if (!hideUnavailableOptionalDependencies)
            {
                s.normal.textColor = Color.gray;
                s.fontStyle = FontStyle.Bold;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label( //.LabelField(
                        " \u2715 Final IK (Optional): Not found or integration package not installed.",
                            s);

                if (GUILayout.Button("Toggle Help"))
                {
                    showHelpFinalIK = !showHelpFinalIK;
                }
                string finalIKIntegrationPackageGuid = "3317a24db6795d74d84831b5f738eaf1";
                if (GUILayout.Button("Ping Integration Package"))
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(finalIKIntegrationPackageGuid));
                    EditorGUIUtility.PingObject(obj);
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

                if (showHelpFinalIK)
                {
                    EditorGUILayout.HelpBox("1. Import Final IK from the Package Manager\n2. double click Plugins/Threepeat/MMCLocomotion/Integrations/IntegrationMMCFinalIK.unitypackage and import that as well.\n3. Click Recheck Dependencies button, below.", MessageType.Info);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();

            bool currRecheck = PlayerPrefs.HasKey(PLAYERPREF_AUTOCHECK) && (PlayerPrefs.GetInt(PLAYERPREF_AUTOCHECK) == 1);

            autoRecheckDependenciesOnImport = EditorGUILayout.BeginToggleGroup(
                                new GUIContent("Automatically Recheck Dependencies on Import/Delete Assets", "This is a toggle just in case you're on a quest for maximum editor compute efficiency."), currRecheck);
            EditorGUILayout.EndToggleGroup();

            bool doHide = hideUnavailableOptionalDependencies;

            hideUnavailableOptionalDependencies = EditorGUILayout.BeginToggleGroup(
                    new GUIContent("Hide Unavailable Optional Integrations", "I know they're not set up, just hide them!"), hideUnavailableOptionalDependencies);
            EditorGUILayout.EndToggleGroup();

            if (doHide != hideUnavailableOptionalDependencies)
            {
                CheckIntegrationStatus();
            }

            if (autoRecheckDependenciesOnImport != currRecheck)
            {
                PlayerPrefs.SetInt(PLAYERPREF_AUTOCHECK, autoRecheckDependenciesOnImport ? 1 : 0);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Recheck Dependencies"))
            {
                CheckIntegrationStatus();
            }

            if (GUILayout.Button("Open Package Manager"))
            {
                UnityEditor.PackageManager.UI.Window.Open("");
            }

            if (GUILayout.Button("Open Project Settings"))
            {
                //UnityEditor.ProjectSettings //.PackageManager.UI.Window.Open("");
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            }

            /*
            if (GUILayout.Button(simulateAllDepsBad ? "Back to normal" : "Show what everything failed looks like")) 
            {
                simulateAllDepsBad = !simulateAllDepsBad;
            }*/



            //Debug.Log("Good asset: " + assetPath);
            //Debug.Log("Bad asset: " +
            /*        assetPath = AssetDatabase.GUIDToAssetPath(new GUID("6844dcb6a866ab34fa96c67e39eab777"));
                    if (assetPath == null) {
                        Debug.Log("null");
                    }
                    else if (assetPath == "")
                    {
                        Debug.Log("empty string");
                    }*/

        }

        private void DrawCustomizeBehavior()
        {
            if (!AllRequiredDependenciesMet())
            {
                // do a gui label to explain they need to head back to dependency check tab to finish that.
                ShowDependencyFixNeededUI();
                return;
            }


            /*if (GUILayout.Button("Run Test"))
            {
                LocomotionSpeedRamp lsr = GameObject.FindObjectOfType<LocomotionSpeedRamp>();
                SerializedObject so = new SerializedObject(lsr);
                SerializedProperty playerxform = so.FindProperty("m_playerTransform");

                Debug.LogFormat("playerxform: {0}", playerxform.objectReferenceValue.name);

            }*/
        }

/*        private void OnFocus()
        {
            Debug.Log("Focus");
            CheckIntegrationStatus();
        }*/

        private void ShowDependencyFixNeededUI()
        {
            GUILayout.Label("Not all dependencies have been met to configure a Locomotion Controller, please return to the Dependency Checks tab and resolve any issues.");
        }

        private bool AllRequiredDependenciesMet()
        {
            foreach (InfoChecker depInfo in dependencyInfos)
            {
                string intString = depInfo.info.integrationName;
                if (depInfo.info.requiredForMMCFunction && !depInfo.IsDependencyMet())
                {
                    return false;
                }
            }
            return true;
        }

        public AnimationModule[] additionalAnimModules = { };
        SerializedObject serobj;
        SerializedProperty animModuleProps;
        private bool showAdvancedConfig;

        private void OnEnable()
        {
            ScriptableObject trgt = this;
            serobj = new SerializedObject(trgt);
            animModuleProps = serobj.FindProperty("additionalAnimModules");
        }

        private void DrawCharacterSetupGUI()
        {
            scrollPosSetupTab = EditorGUILayout.BeginScrollView(scrollPosSetupTab);
            if (!AllRequiredDependenciesMet())
            {
                // do a gui label to explain they need to head back to dependency check tab to finish that.
                ShowDependencyFixNeededUI();
                EditorGUILayout.EndScrollView();
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();

            bool finalIKIntegrationActive = !grounderIntegration.IsPlaceholder();
            string finalIKHeaderString = "Final IK Integration";
            if (!finalIKIntegrationActive)
            {
                finalIKHeaderString += " - Inactive";
            }

            showGeneralConfig = EditorGUILayout.BeginFoldoutHeaderGroup(showGeneralConfig, (showGeneralConfig ? "\u25BC " : "\u25B6 ") + "General Config", SectionHeaderStyle);

            if (showGeneralConfig)
            {
                MakeGeneralConfigGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            showOptionalConfig = EditorGUILayout.BeginFoldoutHeaderGroup(showOptionalConfig, (showOptionalConfig ? "\u25BC " : "\u25B6 ") + "Optional Config Items", SectionHeaderStyle);

            if (showOptionalConfig)
            {
                MakeOptionalConfigGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();

            GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");
            // Go through Major integrations
            foreach (MMCIntegrationBase integration in customIntegrationsAvailable)
            {
                string intname = integration.GetIntegrationName();
                if (!integration.IsMajorIntegration())
                {
                    continue;
                }
                bool showConfig = false;
                if (customIntegrationsMajorToggle.ContainsKey(intname))
                {
                    showConfig = customIntegrationsMajorToggle[intname];
                }
                else
                {
                    customIntegrationsMajorToggle[intname] = false;
                }

                showConfig = EditorGUILayout.BeginFoldoutHeaderGroup(showConfig, (showConfig ? "\u25BC " : "\u25B6 ") + intname, SectionHeaderStyle);
                customIntegrationsMajorToggle[intname] = showConfig;

                if (showConfig)
                {
                    if (integration.GetHelpLink() != null)
                    {
                        if (GUILayout.Button(helpIcon, GUILayout.Width(helpIcon.image.width * 2)))
                        {
                            Help.BrowseURL(integration.GetHelpLink());
                        }
                    }
                    bool wasSelected = integration.selected;

                    integration.selected = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable " + integration.GetIntegrationName(), integration.GetDescription()), integration.selected);

                    if (!wasSelected && integration.selected)
                    {
                        Debug.Log("Enabling!");
                        integration.OnIntegrationEnable();
                    }

                    if (integration.selected)
                    {
                        integration.MakeGUI();
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.Space();
            }





            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (finalIKIntegrationActive)
            {
                showFinalIK = EditorGUILayout.BeginFoldoutHeaderGroup(showFinalIK, (showFinalIK ? "\u25BC " : "\u25B6 ") + finalIKHeaderString, SectionHeaderStyle);
                //showFinalIK = EditorGUILayout.BeginFoldoutHeaderGroup(showFinalIK, (showFinalIK ? "\u25BE " : "\u25B8 ") + finalIKHeaderString, SectionHeaderStyle);
                if (showFinalIK && finalIKIntegrationActive)
                {
                    /*
                     doSetupGrounder = EditorGUILayout.BeginToggleGroup("Setup Final IK Grounder on Character", doSetupGrounder);
                    grounderOnlyLayer =
                        EditorGUILayout.LayerField(
                            new GUIContent("Grounder-only Layer", "This layer is only used by grounder.  e.g. For stairs, this is the object containing the actual stairs."),
                            grounderOnlyLayer);
                    grounderIgnoreLayer =
                        EditorGUILayout.LayerField(
                            new GUIContent("Grounder-ignore Layer", "This layer is used by physics and characters for actual collisions on grounder surfaces.  e.g. For stairs, this is the smooth incline object."),
                            grounderIgnoreLayer);

                    EditorGUILayout.EndToggleGroup();

                     */
                    grounderIntegration.MakeGUI();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

            }

            showAdvancedConfig = false;
            //showAdvancedConfig = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedConfig, "Advanced Configuration", SectionHeaderStyle);
            //EditorGUILayout.EndFoldoutHeaderGroup();

            if (showAdvancedConfig)
            {
                serobj.Update();
                EditorGUILayout.PropertyField(animModuleProps, new GUIContent("Additional Anim Modules (optional - requires firm knowledge of MxM events and customization)", "Use this to add custom events and additional animations to your motion matching configuration."), true);
                serobj.ApplyModifiedProperties();
            }


            EditorGUILayout.Space();

            addCinemachineToScene = EditorGUILayout.BeginToggleGroup(
                                new GUIContent("Add test camera to scene", "For convenience and prototyping speed only, this camera is not production ready.  You'll see."), addCinemachineToScene);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            addMxMSearchManagerToScene = EditorGUILayout.BeginToggleGroup(
                                new GUIContent("Add MxMSearchManager to scene if not present", "Required for MxM 2.2.16+: Characters will not update at all without this in the scene"),
                                addMxMSearchManagerToScene);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            /*if (characterModel == null)
            {
                GUI.enabled = false;
            }*/
            switch (integrationType)
            {
                case CharacterIntegrationType.NewCharacterFromSimplePrefab:
                    if (characterModel == null)
                    {
                        GUI.enabled = false;
                    }
                    break;
                case CharacterIntegrationType.NewCharacterFromComplexSceneHierarchy:
                case CharacterIntegrationType.UpdateExistingCharacter:
                    if ((characterModel == null) || (wantedSceneModelParent == null) || (wantedSceneRootObject == null))
                    {
                        GUI.enabled = false;

                    }
                    break;
            }

            EditorGUILayout.EndScrollView();


            if (GUILayout.Button("Add to scene"))
            {
                LayerMask unsetMask = new LayerMask();
                if (groundLayers.value == unsetMask.value)
                {
                    if (!EditorUtility.DisplayDialog("Ground layers unset", "Your character's ground layers are set to 'Nothing' in the General Config section.  Are you sure you want to proceed?", "Yes, proceed.", "Cancel"))
                    {
                        return;
                    }
                }
                    
                rootObject = null;
                instantiatedModelObject = null;
                if (rootObject == null)
                {
#if ENABLE_INPUT_SYSTEM
                    bool isPlayerChar = (inputSchemeToUse == CharacterInputScheme.InputManager) || (inputSchemeToUse == CharacterInputScheme.InputSystem);
#else
                    bool isPlayerChar = (inputSchemeToUse == CharacterInputScheme.InputManager);
#endif
                    bool isUpdateExisting = false;

                    if (integrationType == CharacterIntegrationType.NewCharacterFromComplexSceneHierarchy)
                    {
                        rootObject = wantedSceneRootObject;
                        instantiatedModelObject = (GameObject)PrefabUtility.InstantiatePrefab(characterModel, activeScene);
                        instantiatedModelObject.transform.parent = wantedSceneModelParent.transform;
                    }
                    else if (integrationType == CharacterIntegrationType.UpdateExistingCharacter)
                    {
                        rootObject = wantedSceneRootObject;
                        instantiatedModelObject = wantedSceneModelParent;
                        isUpdateExisting = true;
                    }
                    else if ((integrationType == CharacterIntegrationType.NewCharacterFromSimplePrefab) && isPlayerChar)
                    {
                        rootObject = new GameObject("PlayerCharacter");
                        instantiatedModelObject = (GameObject)PrefabUtility.InstantiatePrefab(characterModel, activeScene);
                        instantiatedModelObject.transform.parent = rootObject.transform;
                    }
                    else
                    {
                        if (useCharacterControllerForNPC)
                        {
                            rootObject = new GameObject("NonPlayerCharacter");
                            instantiatedModelObject = (GameObject)PrefabUtility.InstantiatePrefab(characterModel, activeScene);
                            instantiatedModelObject.transform.parent = rootObject.transform;
                        }
                        else
                        {
                            // simple setup
                            rootObject = (GameObject)PrefabUtility.InstantiatePrefab(characterModel, activeScene);
                            rootObject.name = "NonPlayerCharacter";
                            instantiatedModelObject = rootObject;
                        }
                    }


                    if (!isUpdateExisting && (isPlayerChar || useCharacterControllerForNPC))
                    {
                        CharacterController charController = rootObject.GetComponent<CharacterController>();
                        if (charController == null)
                        {
                            charController = rootObject.AddComponent<CharacterController>();
                            MMCConfigWizardUtilities.SetupCharacterController(ref charController);
                        }
                    }

                    if (!grounderIntegration.SetupCharacter(rootObject, instantiatedModelObject))
                    {
                        Debug.LogError("Setting up Grounder component failed, please reimport Final IK and the FinalIK integration package and try again.");
                        return;
                    }

                    NGDispatchAnimationEvents ngdae = instantiatedModelObject.GetComponent<NGDispatchAnimationEvents>();
                    if (ngdae == null)
                    {
                        ngdae = instantiatedModelObject.AddComponent<NGDispatchAnimationEvents>();
                    }
                    if (addAudioSourceToRootObject)
                    {
                        AudioSource audioSource = rootObject.GetComponent<AudioSource>();
                        if (audioSource == null)
                        {
                            audioSource = rootObject.AddComponent<AudioSource>();
                        }
                        ngdae.audioSource = audioSource;
                    }

                    MxMAnimator mxma = instantiatedModelObject.GetComponent<MxMAnimator>();

                    if (mxma == null)
                    {
                        mxma = instantiatedModelObject.AddComponent<MxMAnimator>();
                    }
                    //mxma.PlaybackSpeed = 1;
                    mxma.PlaybackSpeedSmoothRate = 1;
                    mxma.UpdateInterval = 1f / 60f; //1f / 120f; // 83.3333f;
                    mxma.BlendTime = 0.3f;
                    mxma.EnableIdle = true;
                    mxma.AnimData = new MxMAnimData[] {
                        //AssetDatabase.LoadAssetAtPath<MxMAnimData>(AssetDatabase.GUIDToAssetPath("d967fd814513c5e4d8b65fc9d9217eab"))
                        AssetDatabase.LoadAssetAtPath<MxMAnimData>(AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("AnimData"))) //a21c31c0a5aabc548b5065c1477e3007
                            };

                    mxma.OverrideCalibration =
                        AssetDatabase.LoadAssetAtPath<CalibrationModule>(
                            AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("CalibrationModule"))); //"b74701d3c2c3f8e40857441fa1cd5975"));
                    mxma.AnimationRoot = rootObject.transform;
                    mxma.FavourTagMode = EFavourTagMethod.Stacking;

                    /*SerializedObject mxmaso = new SerializedObject(mxma);
                    mxmaso.Update();
                    SerializedProperty arar = mxmaso.FindProperty("m_animationRoot");
                    Debug.LogFormat("here it is {0}, {1}", arar.objectReferenceInstanceIDValue, arar.objectReferenceValue);*/


                    if (isPlayerChar || useCharacterControllerForNPC)
                    {
                        mxma.RootMotion = EMxMRootMotion.RootMotionApplicator;

                        NGCharacterControllerWrapper ngcc = rootObject.GetComponent<NGCharacterControllerWrapper>();
                        if (ngcc == null)
                        {
                            ngcc = rootObject.AddComponent<NGCharacterControllerWrapper>();
                        }
                        ngcc.ConfigureObject(true, 0.2f, 0.25f, true, groundLayers);

                        MxMRootMotionApplicator rma = instantiatedModelObject.GetComponent<MxMRootMotionApplicator>();
                        if (rma == null)
                        {
                            rma = instantiatedModelObject.AddComponent<MxMRootMotionApplicator>();
                        }
                        rma.EnableGravity = false;
                        rma.RotationOnly = false;
                        rma.ControllerWrapper = ngcc;
                    }
                    else
                    {
                        mxma.RootMotion = EMxMRootMotion.On;
                    }

                    mxma.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
                    mxma.DefaultBlendSpaceSmoothType = EBlendSpaceSmoothing.None;
                    mxma.SetFavourCurrentPose(true, 0.95f);
                    mxma.MinFootstepInterval = 0.2f;
                    mxma.AngularErrorWarpType = EAngularErrorWarp.On;
                    mxma.AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
                    mxma.AngularErrorWarpRate = 30f; //90f;
                    mxma.AngularErrorWarpThreshold = 1f; // 0.135f;
                    

                    if (striderIntegration.useStrider)
                    {
                        mxma.LongErrorWarpType = ELongitudinalErrorWarp.Stride;
                    }
                    else
                    {
                        // Set up longitude error warping
                        mxma.SetLongitudinalErrorWarping(ELongitudinalErrorWarp.Speed, new Vector2(0.5f, 1.5f));
                    }

                    MxMTrajectoryGenerator mxmt = instantiatedModelObject.GetComponent<MxMTrajectoryGenerator>();
                    if (mxmt == null)
                    {
                        mxmt = instantiatedModelObject.AddComponent<MxMTrajectoryGenerator>();
                    }

                    mxmt.ControlMode = ETrajectoryControlMode.UserInput;
                    /* this is now handled by the InputScheme logic automatically at runtime:
                    if (isPlayerChar)
                    {
                        Debug.Log("Setting mxmt control to user input");
                        mxmt.ControlMode = ETrajectoryControlMode.UserInput;
                    }
                    else
                    {
                        Debug.Log("Setting mxmt control to AI");
                        mxmt.ControlMode = ETrajectoryControlMode.AI_Basic;
                        SerializedObject mxmtso = new SerializedObject(mxmt);
                        mxmtso.Update();
                        SerializedProperty playerxform = mxmtso.FindProperty("m_controlMode");
                        playerxform.intValue = (int)ETrajectoryControlMode.AI_Basic;
                        mxmtso.ApplyModifiedProperties();
                    }*/

                    //mxmt.
                    mxmt.FaceDirectionOnIdle = false;
                    mxmt.MaxSpeed = 5f;
                    mxmt.PositionBias = 18;
                    mxmt.DirectionBias = 15;
                    mxmt.TrajectoryMode = ETrajectoryMoveMode.Normal;
                    mxmt.ScaleAdjustment = 1;
                    mxmt.SimulationSpeedScale = 1;
                    mxmt.RelativeCameraTransform = Camera.main.transform;
                    mxmt.InputProfile = AssetDatabase.LoadAssetAtPath<MxMInputProfile>(AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("InputProfile"))); //"3c07ab2af0077444ea1f548d15f76e33"));



                    if (!striderIntegration.SetupCharacter(rootObject, instantiatedModelObject))
                    {
                        Debug.LogError("Setting up StriderBipedMxM component failed, please reimport Strider, the MxM Strider Integration package, and the MMC Strider integration package and try again.");
                        return;
                    }

                    /*LocomotionSpeedRamp lsr = instantiatedModelObject.AddComponent<LocomotionSpeedRamp>();

                    SerializedObject lsrso = new SerializedObject(lsr);
                    lsrso.Update();
                    SerializedProperty lspddwn = lsrso.FindProperty("m_speedDowngradeTime");
                    SerializedProperty lplyxform = lsrso.FindProperty("m_playerTransform");
                    SerializedProperty lfvr = lsrso.FindProperty("m_favourMultiplier");
                    lspddwn.floatValue = 0.3f;
                    lplyxform.objectReferenceValue = rootObject.transform;
                    lfvr.floatValue = 0.6f;
                    lsrso.ApplyModifiedProperties();*/

                    NGCharacter ngchar = rootObject.GetComponent<NGCharacter>();
                    if (ngchar == null)
                    {
                        ngchar = rootObject.AddComponent<NGCharacter>();
                    }

                    MMCConfigWizardUtilities.SetupCharacterBaseStuff(ref ngchar, ref mxma, ref mxmt, /*ref lsr,*/ ref guidMgr);

                    GameObject cinemach = null;

                    if (overrideInputScheme != null)
                    {
                        ngchar.InputScheme = overrideInputScheme;
                    }
                    else
                    {
                        switch (inputSchemeToUse)
                        {
                            case CharacterInputScheme.InputManager:
                                NGInputScheme_InputManager tmpis = ScriptableObject.CreateInstance<NGInputScheme_InputManager>();
                                ngchar.InputScheme = tmpis;
                                ngchar.InputScheme.doEnvironmentAwareTrajectories = true;
                                if (addCinemachineToScene)
                                {
#if CINEMACHINE_2_OR_NEWER && !CINEMACHINE_3_OR_NEWER
                                    cinemach = FindOrMakeCinemachineFreeLookObject();
                                    CinemachineInputProvider ip = cinemach.GetComponent<CinemachineInputProvider>();
                                    if (ip != null)
                                    {
                                        Destroy(ip);
                                    }
#endif
                                }
                                break;
#if (ENABLE_INPUT_SYSTEM)
                            case CharacterInputScheme.InputSystem:
                                NGInputScheme_InputSystem tmpisys = ScriptableObject.CreateInstance<NGInputScheme_InputSystem>();
                                tmpisys.inputActionAsset =
                                        AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                                            AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("InputActionsDefault")));
                                ngchar.InputScheme = tmpisys;
                                if (addCinemachineToScene)
                                {
                                    SetupCinemachineInputProvider(rootObject, tmpisys.inputActionAsset);
                                }
                                ngchar.InputScheme.doEnvironmentAwareTrajectories = true;
                                break;
#endif
                            case CharacterInputScheme.NavMesh:
                                NGInputScheme_NavMesh tmpisnav = ScriptableObject.CreateInstance<NGInputScheme_NavMesh>();
                                ngchar.InputScheme = tmpisnav;
                                SetupNavMeshAgent(rootObject);
                                break;
                        }
                    }


                    if (overrideCharacterConfig != null)
                    {
                        ngchar.config = overrideCharacterConfig;
                    }
                    else
                    {
                        ngchar.config = AssetDatabase.LoadAssetAtPath<NGCharacterBaseConfig>(
                                            AssetDatabase.GUIDToAssetPath(GetGuidFromManagerKey("NGCharacterConfigBase")));
                    }

                    /*if (isPlayerChar)
                    {
                        NGInputScheme_InputManager tmpis = ScriptableObject.CreateInstance<NGInputScheme_InputManager>();
                        ngchar.InputScheme = tmpis;
                        ngchar.InputScheme.doEnvironmentAwareTrajectories = true;
                    }
                    else
                    {
                        NGInputScheme_NavMesh tmpis = ScriptableObject.CreateInstance<NGInputScheme_NavMesh>();
                        ngchar.InputScheme = tmpis;
                    }*/


                    foreach (MMCIntegrationBase integration in customIntegrationsAvailable)
                    {
                        if (integration.selected)
                        {
                            Debug.LogFormat("Setting up {0} integration on character.", integration.GetIntegrationName());
                            integration.SetupIntegration(rootObject, instantiatedModelObject, ngchar);
                        }
                    }


                    if (addCinemachineToScene)
                    {
                        SetupCinemachine(null, cinemach, rootObject, groundLayers);
                    }

                    if (addMxMSearchManagerToScene)
                    {
                        SetupSearchManager();
                    }
                }


            }
            GUI.enabled = true;

            /*if (GUILayout.Button("Read Events from FBX Prefab"))
            {
                ReadAnimationEventsFromFBXPrefab();
            }*/

            /*
            if (GUILayout.Button("Try layer thing"))
            {
                bool ignore = Physics.GetIgnoreLayerCollision(6, 10);
                Physics.IgnoreLayerCollision(6, 10, !ignore);
            }*/

            /*prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

            if (GUILayout.Button("Replace"))
            {
                var selection = Selection.gameObjects;

                for (var i = selection.Length - 1; i >= 0; --i)
                {
                    var selected = selection[i];
                    var prefabType = PrefabUtility.GetPrefabType(prefab);
                    GameObject newObject;

                    if (prefabType == PrefabType.Prefab)
                    {
                        newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    }
                    else
                    {
                        newObject = Instantiate(prefab);
                        newObject.name = prefab.name;
                    }

                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab");
                        break;
                    }

                    Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
                    newObject.transform.parent = selected.transform.parent;
                    newObject.transform.localPosition = selected.transform.localPosition;
                    newObject.transform.localRotation = selected.transform.localRotation;
                    newObject.transform.localScale = selected.transform.localScale;
                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }*/

            //EditorGUILayout.LabelField("Grounder Found: " + grounderFBBIKClassExists.ToString());
            //EditorGUILayout.LabelField("Created Scene Object: ");
        }

        private void SetupSearchManager()
        {
            // first check if there's already one in the scene:
            MxMSearchManager[] objs = FindObjectsOfType<MxMSearchManager>();

            if (objs.Length > 0)
            {
                Debug.Log("MxMSearchManager already present in scene, so not adding another.  This is totally fine and a good thing.");
                return;
            }

            string prefabGuid = GetGuidFromManagerKey("MxMSearchManagerPrefab");

            GameObject newObj = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(prefabGuid), typeof(GameObject));

            GameObject sceneObj = Instantiate(newObj);
            sceneObj.name = "MxMSearchManager";
        }

        private void SetupNavMeshAgent(GameObject rootObject)
        {
            NavMeshAgent agent = rootObject.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = rootObject.AddComponent<NavMeshAgent>();
            }
            // set agent settings
            agent.baseOffset = 0;
            agent.speed = 0;
            agent.angularSpeed = 60;
            agent.acceleration = 0;
            agent.stoppingDistance = 1;
            agent.autoBraking = true;
            agent.radius = 0.5f;
            agent.height = 2;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 50;
            agent.autoTraverseOffMeshLink = true;
            agent.autoRepath = true;
        }

        private void ReadAnimationEventsFromFBXPrefab()
        {
            FBXAnimationEventInfo infos = new FBXAnimationEventInfo();
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                Debug.LogFormat("{0}", assetPath);

                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);
                FBXAnimationEventInfo.FBXInfo theInfo = ReadAnimationEventsFromFBXPrefab(obj, ref importer);
                theInfo.fbxOriginalPath = assetPath;
                theInfo.fbxGuid = AssetDatabase.AssetPathToGUID(assetPath);
                infos.fbxPrefabs.Add(theInfo);
            }

            var savePath = EditorUtility.SaveFilePanelInProject("Save FBXAnimationEventInfo", "FBXAnimationEventInfo", "asset", "asset");

            if (string.IsNullOrEmpty(savePath))
            {
                // do nothing, they canceled the save dialog
            }
#if UNITY_2021_3_OR_NEWER
            else if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(savePath, AssetPathToGUIDOptions.OnlyExistingAssets)))
#else
            else if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(savePath)))
#endif
            {
                EditorUtility.DisplayDialog("File Exists!", "That file already exists, please delete it or pick a different save path.", "OK");
#if UNITY_2021_3_OR_NEWER
                Debug.LogFormat("Can't save to {0}, {1}", savePath, string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(savePath, AssetPathToGUIDOptions.OnlyExistingAssets)));
#else
                Debug.LogFormat("Can't save to {0}, {1}", savePath, string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(savePath)));
#endif
            }
            else 
            {
                Debug.LogFormat("Saving to {0}", savePath);
                AssetDatabase.CreateAsset(infos, savePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        public static FBXAnimationEventInfo.FBXInfo ReadAnimationEventsFromFBXPrefab(UnityEngine.Object prefab, ref ModelImporter importer)
        {
            //importer.animationType = ModelImporterAnimationType.Human;
            //importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            List<ModelImporterClipAnimation> theClips = new List<ModelImporterClipAnimation>();

            FBXAnimationEventInfo.FBXInfo info = new FBXAnimationEventInfo.FBXInfo();
            info.clips = new List<FBXAnimationEventInfo.StringAnimEvent>();

            ModelImporterClipAnimation[] importerAnimationsToUse = importer.clipAnimations;

            if (importer.clipAnimations.Length <= 0)
            {
                importerAnimationsToUse = importer.defaultClipAnimations;
            }

            foreach (ModelImporterClipAnimation clip in importerAnimationsToUse) //.defaultClipAnimations)
            {
                FBXAnimationEventInfo.StringAnimEvent events = new FBXAnimationEventInfo.StringAnimEvent();
                events.clipName = clip.name;

                events.keepOriginalOrientation = clip.keepOriginalOrientation;
                events.keepOriginalPositionXZ = clip.keepOriginalPositionXZ;
                events.keepOriginalPositionY = clip.keepOriginalPositionY;
                events.lockRootHeightY = clip.lockRootHeightY;
                events.lockRootPositionXZ = clip.lockRootPositionXZ;
                events.lockRootRotation = clip.lockRootRotation;

                //Debug.Log($"{clip.takeName}/{clip.name}");
                foreach (AnimationEvent evt in clip.events)
                {
                    Debug.Log($"EVENT[{clip.takeName}/{clip.name}] - {evt.functionName}");
                    events.clipEvents.Add(new FBXAnimationEventInfo.SerializableAnimationEvent(evt));
                }
                /*if (events.clipEvents.Count > 0)
                {*/
                    info.clips.Add(events);
                //}
            }

            return info;
        }

        private GameObject FindOrMakeCinemachineFreeLookObject(GameObject cinemach = null)
        {
#if CINEMACHINE_2_OR_NEWER && !CINEMACHINE_3_OR_NEWER
            if (cinemach == null)
            {
                // attempt to find the cinemachine object.  only select it if it's active/enabled.
                CinemachineFreeLook fl = GameObject.FindObjectOfType<CinemachineFreeLook>();

                GameObject go = null;

                if (fl != null)
                {
                    go = fl.gameObject;
                }

                if (go == null)
                {
                    CinemachineInputProvider ip = GameObject.FindObjectOfType<CinemachineInputProvider>();
                    if (ip != null)
                    {
                        go = ip.gameObject;
                    }
                }

                if (go == null)
                {
                    // make new object
                    cinemach = new GameObject("CinemachineFreeLookController");
                }
                else
                {
                    cinemach = go;
                }

            }
#endif
            return cinemach;
        }

#if (ENABLE_INPUT_SYSTEM)
        private void SetupCinemachineInputProvider(GameObject characterObject, InputActionAsset inputActionAsset)
        {
#if CINEMACHINE_2_OR_NEWER && !CINEMACHINE_3_OR_NEWER
            GameObject cinemach = FindOrMakeCinemachineFreeLookObject();
            CinemachineInputProvider ip = cinemach.GetComponent<CinemachineInputProvider>();

            if (ip == null)
            {
                ip = cinemach.AddComponent<CinemachineInputProvider>();
            }
            ip.XYAxis = InputActionReference.Create(inputActionAsset.actionMaps[0]["MouseLook"]);
            ip.enabled = true;
#endif
        }
#endif


        private void SetupCinemachine(GameObject maincamObject, GameObject cinemachineObject, GameObject characterObject, LayerMask collideAgainst)
        {
#if CINEMACHINE_2_OR_NEWER && !CINEMACHINE_3_OR_NEWER
            GameObject maincam = maincamObject;
            GameObject cinemach = cinemachineObject;

            if (maincam == null)
            {
                // find the camera
                maincam = GameObject.FindGameObjectWithTag("MainCamera");
                if (maincam == null)
                {
                    Debug.LogError("SetupCinemachine: Can't find MainCamera-tagged object in your scene.  The Scene needs a camera for this to work!");
                }
            }

            cinemach = FindOrMakeCinemachineFreeLookObject(cinemachineObject);

            CinemachineBrain brain = maincam.GetComponent<CinemachineBrain>();

            if (brain == null)
            {
                brain = maincam.AddComponent<CinemachineBrain>();

                brain.m_ShowCameraFrustum = true;
                brain.m_IgnoreTimeScale = false;
                brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
            }

            CinemachineFreeLook cine = cinemach.GetComponent<CinemachineFreeLook>();
            CinemachineCollider coll = cinemach.GetComponent<CinemachineCollider>();

            if (cine == null)
            {
                cine = cinemach.AddComponent<CinemachineFreeLook>();
            }

            if (coll == null)
            {
                coll = cinemach.AddComponent<CinemachineCollider>();
            }

            cine.Follow = characterObject.transform;
            cine.LookAt = characterObject.transform;
            cine.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.RoundRobin;
            cine.m_CommonLens = true;
            cine.m_Lens.NearClipPlane = 0.1f;
            cine.m_Lens.FarClipPlane = 5000f;
            cine.m_Lens.Dutch = 0;
            cine.m_Lens.FieldOfView = 40f;
            cine.m_YAxis.Value = 0.5f;
            cine.m_YAxis.m_MaxSpeed = 10f;
            cine.m_YAxis.m_AccelTime = 0.2f;
            cine.m_YAxis.m_DecelTime = 0.1f;
            cine.m_YAxis.m_InputAxisName = "Mouse Y";
            cine.m_YAxis.m_InputAxisValue = 0;
            cine.m_YAxis.m_InvertInput = true;
            
            cine.m_YAxisRecentering.m_enabled = false;
            cine.m_YAxisRecentering.m_WaitTime = 1;
            cine.m_YAxisRecentering.m_RecenteringTime = 2;

            cine.m_XAxis.Value = 0;
            cine.m_XAxis.m_MinValue = -180f;
            cine.m_XAxis.m_MaxValue = 180f;
            cine.m_XAxis.m_Wrap = true;
            cine.m_XAxis.m_MaxSpeed = 300;

            cine.m_XAxis.m_AccelTime = 0.1f;
            cine.m_XAxis.m_DecelTime = 0.1f;
            cine.m_XAxis.m_InputAxisName = "Mouse X";
            cine.m_XAxis.m_InputAxisValue = 0;
            cine.m_XAxis.m_InvertInput = false;
            cine.m_Heading.m_Definition = CinemachineOrbitalTransposer.Heading.HeadingDefinition.TargetForward;
            cine.m_Heading.m_Bias = 0;

            cine.m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;
            cine.m_SplineCurvature = 0.2f;

            CinemachineVirtualCamera topRig = cine.GetRig(0);
            CinemachineVirtualCamera midRig = cine.GetRig(1);
            CinemachineVirtualCamera btmRig = cine.GetRig(2);

            if (topRig != null)
            {
                CinemachineComposer comp = topRig.GetCinemachineComponent<CinemachineComposer>();
                PopulateComposerSettings(comp);
                cine.m_Orbits[0].m_Height = 3;
                cine.m_Orbits[0].m_Radius = 5f;
                CinemachineOrbitalTransposer transposer = topRig.GetCinemachineComponent<CinemachineOrbitalTransposer>();
                transposer.m_YDamping = 3f;
                transposer.m_XDamping = 3f;
            }
            if (midRig != null)
            {
                CinemachineComposer comp = midRig.GetCinemachineComponent<CinemachineComposer>();
                PopulateComposerSettings(comp);
                cine.m_Orbits[1].m_Height = 1.5f;
                cine.m_Orbits[1].m_Radius = 6;
                CinemachineOrbitalTransposer transposer = midRig.GetCinemachineComponent<CinemachineOrbitalTransposer>();
                transposer.m_YDamping = 3f;
                transposer.m_XDamping = 3f;

            }
            if (btmRig != null)
            {
                CinemachineComposer comp = btmRig.GetCinemachineComponent<CinemachineComposer>();
                PopulateComposerSettings(comp);
                cine.m_Orbits[2].m_Height = 1f;
                cine.m_Orbits[2].m_Radius = 5f;
                CinemachineOrbitalTransposer transposer = btmRig.GetCinemachineComponent<CinemachineOrbitalTransposer>();
                transposer.m_YDamping = 3f;
                transposer.m_XDamping = 3f;
            }

            coll.m_MinimumDistanceFromTarget = 0.1f;
            coll.m_AvoidObstacles = false;
            coll.m_DistanceLimit = 0;
            coll.m_MinimumOcclusionTime = 0;
            coll.m_CameraRadius = 0.1f;
            coll.m_Strategy = CinemachineCollider.ResolutionStrategy.PreserveCameraHeight;
            coll.m_MaximumEffort = 4;
            coll.m_SmoothingTime = 0;
            coll.m_Damping = 0;
            coll.m_DampingWhenOccluded = 0;
            coll.m_OptimalTargetDistance = 0;
#else
            Debug.LogWarning("MMCConfigWizard: Cinemachine auto-setup requires Cinemachine 2.x. Please configure your Cinemachine 3 camera manually.");
#endif
        }

        private void PopulateComposerSettings(
#if CINEMACHINE_2_OR_NEWER && !CINEMACHINE_3_OR_NEWER
            CinemachineComposer comp
#else
            object comp
#endif
            )
        {
#if CINEMACHINE_2_OR_NEWER && !CINEMACHINE_3_OR_NEWER
            comp.m_TrackedObjectOffset = new Vector3(0, 1.5f, 0);
            comp.m_LookaheadTime = 0;
            comp.m_LookaheadSmoothing = 10;
            comp.m_LookaheadIgnoreY = false;
            comp.m_HorizontalDamping = 0.5f;
            comp.m_VerticalDamping = 0.5f;
            comp.m_ScreenX = 0.5f;
            comp.m_ScreenY = 0.5f;
            comp.m_DeadZoneWidth = 0;
            comp.m_DeadZoneHeight = 0;
            comp.m_SoftZoneWidth = 0.8f;
            comp.m_SoftZoneHeight = 0.8f;
            comp.m_BiasX = 0;
            comp.m_BiasY = 0;
            comp.m_CenterOnActivate = true;
#endif
        }

        private bool StriderAvailable()
        {
            foreach (InfoChecker depInfo in dependencyInfos)
            {
                if (depInfo.info.integrationName.ToLower().StartsWith("strider 1")) {
                    return depInfo.IsDependencyMet();
                }
            }
            return false;
        }
        private void MakeGeneralConfigGUI()
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;

            GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(helpIcon, GUILayout.Width(helpIcon.image.width*2)))
            {
                Help.BrowseURL("https://threepeatgames.com/mmc_locomotion/configuration");
            }
            integrationType = (CharacterIntegrationType)EditorGUILayout.EnumPopup(new GUIContent("Integration Type"), integrationType);
            EditorGUILayout.EndHorizontal();
            switch (integrationType)
            {
                case CharacterIntegrationType.NewCharacterFromSimplePrefab:
                    characterModel = (GameObject)EditorGUILayout.ObjectField("Character Prefab", characterModel, typeof(GameObject), false);
                    break;
                case CharacterIntegrationType.NewCharacterFromComplexSceneHierarchy:
                case CharacterIntegrationType.UpdateExistingCharacter:
                    characterModel = (GameObject)EditorGUILayout.ObjectField("Model Prefab", characterModel, typeof(GameObject), false);
                    wantedSceneRootObject = (GameObject)EditorGUILayout.ObjectField("Scene Root Object", wantedSceneRootObject, typeof(GameObject), true);
                    if ((wantedSceneRootObject != null) && (wantedSceneModelParent == null))
                    {
                        wantedSceneModelParent = wantedSceneRootObject.GetComponentInChildren<Animator>().gameObject;
                    }

                    wantedSceneModelParent = (GameObject)EditorGUILayout.ObjectField(
//#if MMLC_EXPERIMENTAL_FEATURE_UPDATE_EXISTING_CHARACTER
                            (integrationType == CharacterIntegrationType.UpdateExistingCharacter) ?
                                "Scene Model Object" : "Scene Model Parent", 
/*#else
                            "Scene Model Parent",
#endif*/
                wantedSceneModelParent, 
                            typeof(GameObject), 
                            true);
                    break;

            }
            EditorGUILayout.Space();
#if (ENABLE_INPUT_SYSTEM)
            inputSchemeToUse = (CharacterInputScheme)EditorGUILayout.EnumPopup(new GUIContent("Character Input Scheme"), inputSchemeToUse);
#else
            inputSchemeToUse = (CharacterInputScheme)EditorGUILayout.EnumPopup(new GUIContent("Character Input Scheme*"), inputSchemeToUse);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("* - Input System not installed/enabled.  To use Input System, Please install it from Package Manager -> Unity Registry.");
#endif

            EditorGUILayout.Space();

            LayerMask tempMask = EditorGUILayout.MaskField(new GUIContent("Ground Layers", "Which layers should be treated as ground by the character locomotion and collision system"), InternalEditorUtility.LayerMaskToConcatenatedLayersMask(groundLayers), InternalEditorUtility.layers);
            groundLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            EditorGUILayout.Space();

            //EditorStyles.label.wordWrap = true;
            //EditorStyles.toggle.wordWrap = true;
            //EditorStyles.toggleGroup.wordWrap = true;
            EditorStyles.boldLabel.wordWrap = false;

            if (inputSchemeToUse == CharacterInputScheme.NavMesh)
            {
                useCharacterControllerForNPC = EditorGUILayout.BeginToggleGroup(
                    new GUIContent("NPC Has Full CharacterController rig", "If true, NPC will have a CharacterController component and all accompanying configuration.  If false, it will be a root-motion-based NavMeshAgent with minimum components.  Suggestion: Leave this true unless you really know what you're doing."),
                    useCharacterControllerForNPC);
                EditorGUILayout.EndToggleGroup();
            }

            EditorGUIUtility.labelWidth = labelWidth;
            /*
            useDecoupledController = EditorGUILayout.BeginToggleGroup(
                    new GUIContent("Use decoupled controller" , "See documentation!  Only recommended if increased responsiveness " +
                                   "is needed (at the expense of animation/movement accuracy)).  If enabled, character will be allowed to respond to player input faster than the animations actually support.  This will give increased control responsiveness at the cost of animation precision.  Meaning:  the more decoupled the controller, the more arcady/responsive the controls, but it will be less accurate and more foot sliding and unrealistic movement will occur."), useDecoupledController);
            EditorGUILayout.EndToggleGroup();*/
            //useDecoupledController = false;

            /*useMxMFootIK = EditorGUILayout.BeginToggleGroup(
                new GUIContent("Use MxM Foot IK (see tooltip)", "Enabled by default.  If your character's feet are sliding on the ground or aren't behaving properly with Final IK grounder, toggle this value and re-setup your character."), useMxMFootIK);
            EditorGUILayout.EndToggleGroup();*/

            //doSetupGrounder = EditorGUILayout.BeginToggleGroup("Setup Final IK Grounder on Character", doSetupGrounder);
        }

        private void MakeOptionalConfigGUI()
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;

            overrideInputScheme = 
                    (NGInputSchemeBase)EditorGUILayout.ObjectField(
                            new GUIContent("InputScheme Override", "Optional custom InputScheme"), overrideInputScheme, 
                            typeof(NGInputSchemeBase), false);

            overrideCharacterConfig = 
                    (NGCharacterBaseConfig)EditorGUILayout.ObjectField(
                            new GUIContent("Character Config Override", "Optional custom Character Config"), overrideCharacterConfig, 
                            typeof(NGCharacterBaseConfig), false);

            EditorGUILayout.Space();

            addAudioSourceToRootObject = EditorGUILayout.BeginToggleGroup(
                                new GUIContent("Add AudioSource to Character object", "Add an AudioSource component to the root object"), addAudioSourceToRootObject);
            EditorGUILayout.EndToggleGroup();

            //showHelp_Strider = ThreepeatEditorGUIUtilities.LabelWithHelp.LabelWithHelpField    

            if (!striderIntegration.IsPlaceholder() && StriderAvailable())
            {
                striderIntegration.MakeGUI();
            }
            else
            {
                striderIntegration.useStrider = false;
                GUI.enabled = false;
                striderIntegration.MakeGUI();
                GUI.enabled = true;
            }

            GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");

            // Go through non-major integrations first (major integrations handled outside of this method.

            foreach (MMCIntegrationBase integration in customIntegrationsAvailable)
            {
                if (integration.IsMajorIntegration())
                {
                    continue;
                }

                if (integration.GetHelpLink() != null)
                {
                    if (GUILayout.Button(helpIcon, GUILayout.Width(helpIcon.image.width * 2)))
                    {
                        Help.BrowseURL(integration.GetHelpLink());
                    }
                }
                bool wasSelected = integration.selected;
                integration.selected = EditorGUILayout.BeginToggleGroup(new GUIContent(integration.GetIntegrationName(), integration.GetDescription()), integration.selected);

                if (!wasSelected && integration.selected)
                {
                    integration.OnIntegrationEnable();
                }

                if (integration.selected)
                {
                    integration.MakeGUI();
                }
                EditorGUILayout.EndToggleGroup();
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
        private void SetupStyles()
        {
            Texture2D blackTex = MakeTex(32, 32, new Color(0.3f, 0.33f, 0.3f));
            SectionHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                margin = new RectOffset(0, 0, 4, 4),
                padding = new RectOffset(20, 8, 4, 4),
                fontSize = 13,
                fontStyle = FontStyle.Bold,
            };
            SectionHeaderStyle.normal.background = blackTex;
            SectionHeaderStyle.normal.textColor = Color.white;
            SectionHeaderStyle.border.top = 4;
            SectionHeaderStyle.border.bottom = 4;

            SectionHeaderStyleThin = new GUIStyle(EditorStyles.foldoutHeader)
            {
                margin = new RectOffset(0, 0, 4, 4),
                padding = new RectOffset(20, 8, 4, 4),
                fontSize = 13,
                fontStyle = FontStyle.Bold,
            };
            SectionHeaderStyleThin.normal.background = blackTex;
            SectionHeaderStyleThin.normal.textColor = Color.white;
            SectionHeaderStyleThin.border.top = 4;
            SectionHeaderStyleThin.border.bottom = 4;

        }

        public void SetIntegrationType(CharacterIntegrationType newType)
        {
            integrationType = newType;
        }

        public void SetCharacterInputScheme(CharacterInputScheme newScheme)
        {
            inputSchemeToUse = newScheme;
        }
    }
}