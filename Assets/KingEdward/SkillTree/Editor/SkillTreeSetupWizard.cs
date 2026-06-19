using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.IO;
using KingEdward.SkillTree;
using GameCreator.Runtime.Common;
using UnityEditor.SceneManagement;

namespace KingEdward.SkillTree.Editor
{
    public class SkillTreeSetupWizard : EditorWindow
    {
        private enum SetupStep
        {
            Welcome,
            PlayerSetup,
            SkillTreeCreation,
            UISetup,
            Complete
        }

        private SetupStep currentStep = SetupStep.Welcome;
        private GameObject playerObject;
        private string skillTreeName = "My Skill Tree";
        private string skillTreePath = "Assets/SkillTrees";
        private bool createExampleSkills = true;
        private bool setupUI = true;
        private Vector2 scrollPosition;
        
        // Skill Tree selection
        private bool useExistingSkillTree = false;
        private SkillTreeData existingSkillTree = null;
        
        // UI Prefab selection
        private string[] skillTreePrefabs = new string[0];
        private string[] hotbarPrefabs = new string[0];
        private int selectedSkillTreePrefab = 0;
        private int selectedHotbarPrefab = 0;
        private bool prefabsLoaded = false;

        [MenuItem("Tools/KingEdward/Skill Tree/Setup Wizard", false, 0)]
        public static void ShowWindow()
        {
            SkillTreeSetupWizard window = GetWindow<SkillTreeSetupWizard>("Skill Tree Setup");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }
        
        private void LoadPrefabLists()
        {
            try
            {
                string prefabPath = "Assets/KingEdward/SkillTree/Examples/SkillTreeUI";
                
                if (!AssetDatabase.IsValidFolder(prefabPath))
                {
                    Debug.LogError($"[Wizard] Folder not found: {prefabPath}");
                    skillTreePrefabs = new string[] { "Folder not found" };
                    hotbarPrefabs = new string[] { "Folder not found" };
                    return;
                }
                
                System.Collections.Generic.List<string> skillTreeList = new System.Collections.Generic.List<string>();
                System.Collections.Generic.List<string> hotbarList = new System.Collections.Generic.List<string>();
                
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabPath });
                
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    
                    if (prefab != null)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        
                        // Check for SkillTreeUI component
                        if (prefab.GetComponentInChildren<SkillTreeUI>() != null)
                        {
                            skillTreeList.Add(fileName);
                        }
                        
                        // Check for SkillHotbarUI component
                        if (prefab.GetComponentInChildren<SkillHotbarUI>() != null)
                        {
                            hotbarList.Add(fileName);
                        }
                    }
                }
                
                skillTreePrefabs = skillTreeList.Count > 0 ? skillTreeList.ToArray() : new string[] { "No SkillTreeUI prefabs found" };
                hotbarPrefabs = hotbarList.Count > 0 ? hotbarList.ToArray() : new string[] { "No SkillHotbarUI prefabs found" };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Wizard] Error loading prefabs: {ex.Message}");
                skillTreePrefabs = new string[] { "Error - check console" };
                hotbarPrefabs = new string[] { "Error - check console" };
            }
        }

        private void OnGUI()
        {
            // Header
            DrawHeader();

            EditorGUILayout.Space(10);

            // Progress bar
            DrawProgressBar();

            EditorGUILayout.Space(10);

            // Content area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (currentStep)
            {
                case SetupStep.Welcome:
                    DrawWelcomeStep();
                    break;
                case SetupStep.PlayerSetup:
                    DrawPlayerSetupStep();
                    break;
                case SetupStep.SkillTreeCreation:
                    DrawSkillTreeCreationStep();
                    break;
                case SetupStep.UISetup:
                    DrawUISetupStep();
                    break;
                case SetupStep.Complete:
                    DrawCompleteStep();
                    break;
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Navigation buttons
            DrawNavigationButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Skill Tree System Setup Wizard", EditorStyles.boldLabel);
            GUILayout.Label("Quick setup for your skill tree system", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawProgressBar()
        {
            EditorGUILayout.BeginHorizontal();
            
            string[] steps = { "Welcome", "Player", "Skills", "UI", "Done" };
            for (int i = 0; i < steps.Length; i++)
            {
                bool isActive = (int)currentStep == i;
                bool isComplete = (int)currentStep > i;
                
                GUI.color = isComplete ? Color.green : (isActive ? Color.yellow : Color.gray);
                
                if (GUILayout.Button(steps[i], GUILayout.Height(30)))
                {
                    // Allow jumping to previous steps
                    if (i < (int)currentStep)
                    {
                        currentStep = (SetupStep)i;
                    }
                }
                
                GUI.color = Color.white;
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawWelcomeStep()
        {
            EditorGUILayout.LabelField("Welcome to Skill Tree System!", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This wizard will help you set up the Skill Tree System in your project.\n\n" +
                "We'll guide you through:\n" +
                "• Setting up your Player object\n" +
                "• Creating your first skill tree\n" +
                "• Adding UI to your scene\n" +
                "• Creating example skills",
                MessageType.Info
            );

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("What you'll need:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("✓ Game Creator 2 installed", EditorStyles.label);
            EditorGUILayout.LabelField("✓ A Player GameObject in your scene", EditorStyles.label);
            EditorGUILayout.LabelField("✓ A Canvas for UI (optional)", EditorStyles.label);

            EditorGUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "This wizard is optional. You can always set up manually by following the documentation.",
                MessageType.Info
            );
        }

        private void DrawPlayerSetupStep()
        {
            EditorGUILayout.LabelField("Player Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Select your Player GameObject. We'll add the SkillTreeComponent to it.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            playerObject = (GameObject)EditorGUILayout.ObjectField(
                "Player Object",
                playerObject,
                typeof(GameObject),
                true
            );

            if (playerObject != null)
            {
                EditorGUILayout.Space();

                // Check if already has component
                SkillTreeComponent existingComponent = playerObject.GetComponent<SkillTreeComponent>();
                
                if (existingComponent != null)
                {
                    EditorGUILayout.HelpBox(
                        "✓ This GameObject already has a SkillTreeComponent!",
                        MessageType.Info
                    );
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "We'll add a SkillTreeComponent to this GameObject.",
                        MessageType.Info
                    );
                }

                // Show what will be added
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Components to add:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• SkillTreeComponent", EditorStyles.label);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Please select your Player GameObject to continue.",
                    MessageType.Warning
                );
            }
        }

        private void DrawSkillTreeCreationStep()
        {
            EditorGUILayout.LabelField("Skill Tree Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            useExistingSkillTree = EditorGUILayout.Toggle("Use Existing Skill Tree", useExistingSkillTree);
            
            EditorGUILayout.Space();

            if (useExistingSkillTree)
            {
                EditorGUILayout.HelpBox(
                    "Select an existing Skill Tree asset to use.",
                    MessageType.Info
                );
                
                existingSkillTree = (SkillTreeData)EditorGUILayout.ObjectField(
                    "Skill Tree Asset", 
                    existingSkillTree, 
                    typeof(SkillTreeData), 
                    false
                );
                
                if (existingSkillTree == null)
                {
                    EditorGUILayout.HelpBox(
                        "Please select a Skill Tree asset to continue.",
                        MessageType.Warning
                    );
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Let's create a new skill tree asset.",
                    MessageType.Info
                );

                EditorGUILayout.Space();

                skillTreeName = EditorGUILayout.TextField("Skill Tree Name", skillTreeName);
                
                EditorGUILayout.BeginHorizontal();
                skillTreePath = EditorGUILayout.TextField("Save Path", skillTreePath);
                if (GUILayout.Button("Browse", GUILayout.Width(70)))
                {
                    string path = EditorUtility.SaveFolderPanel("Select Folder", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        skillTreePath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                // Only show create example skills option if creating a new skill tree
                if (existingSkillTree == null)
                {
                    createExampleSkills = EditorGUILayout.Toggle("Create Example Skills", createExampleSkills);

                    if (createExampleSkills)
                    {
                        EditorGUILayout.HelpBox(
                            "We'll create 6 example skills:\n" +
                            "• 3 Basic skills (no prerequisites)\n" +
                            "• 2 Advanced skills (require basic skills)\n" +
                            "• 1 Ultimate skill (requires advanced skills)",
                            MessageType.Info
                        );
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Using existing skill tree. Example skills will not be created.",
                        MessageType.Info
                    );
                    createExampleSkills = false;
                }
            }
        }

        private void DrawUISetupStep()
        {
            EditorGUILayout.LabelField("UI Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            setupUI = EditorGUILayout.Toggle("Setup UI in Scene", setupUI);

            if (setupUI)
            {
                // Load prefabs only when needed (lazy loading)
                if (!prefabsLoaded)
                {
                    EditorGUILayout.HelpBox("Loading prefabs...", MessageType.Info);
                    if (GUILayout.Button("Load Prefabs"))
                    {
                        LoadPrefabLists();
                        prefabsLoaded = true;
                    }
                    return;
                }
                
                EditorGUILayout.Space();
                
                // Skill Tree UI Prefab Selection
                EditorGUILayout.LabelField("Skill Tree UI Prefab", EditorStyles.boldLabel);
                if (skillTreePrefabs != null && skillTreePrefabs.Length > 0 && !skillTreePrefabs[0].Contains("found"))
                {
                    selectedSkillTreePrefab = EditorGUILayout.Popup("Select Prefab", selectedSkillTreePrefab, skillTreePrefabs);
                }
                else
                {
                    EditorGUILayout.HelpBox("No SkillTreeUI prefabs found", MessageType.Warning);
                }
                
                EditorGUILayout.Space();
                
                // Hotbar Prefab Selection
                EditorGUILayout.LabelField("Hotbar UI Prefab", EditorStyles.boldLabel);
                if (hotbarPrefabs != null && hotbarPrefabs.Length > 0 && !hotbarPrefabs[0].Contains("found"))
                {
                    selectedHotbarPrefab = EditorGUILayout.Popup("Select Prefab", selectedHotbarPrefab, hotbarPrefabs);
                }
                else
                {
                    EditorGUILayout.HelpBox("No SkillHotbarUI prefabs found", MessageType.Warning);
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.HelpBox(
                    "We'll add the following to your scene:\n" +
                    "• Skill Tree UI Canvas\n" +
                    "• Skill Hotbar Canvas\n" +
                    "• Auto-link references to Player\n\n" +
                    "Note: Prefabs já são Canvas completos",
                    MessageType.Info
                );

                EditorGUILayout.Space();
                
                // Check for EventSystem
                UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem == null)
                {
                    EditorGUILayout.HelpBox(
                        "⚠ No EventSystem found. We'll create one for you.",
                        MessageType.Warning
                    );
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "You can add UI manually later using the prefabs.",
                    MessageType.Info
                );
            }
        }

        private void DrawCompleteStep()
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "✓ Your Skill Tree System is ready to use!",
                MessageType.Info
            );

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("What was created:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• SkillTreeComponent added to Player", EditorStyles.label);
            EditorGUILayout.LabelField($"• Skill Tree Data: {skillTreePath}/{skillTreeName}.asset", EditorStyles.label);
            
            if (createExampleSkills)
            {
                EditorGUILayout.LabelField("• 6 Example Skills", EditorStyles.label);
            }
            
            if (setupUI)
            {
                EditorGUILayout.LabelField("• Skill Tree UI in scene", EditorStyles.label);
                EditorGUILayout.LabelField("• Skill Hotbar in scene", EditorStyles.label);
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Next Steps:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("1. Customize your skills in the Project window", EditorStyles.label);
            EditorGUILayout.LabelField("2. Add Instructions to skill events (On Unlock, On Use, etc)", EditorStyles.label);
            EditorGUILayout.LabelField("3. Test in Play Mode!", EditorStyles.label);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Open Documentation", GUILayout.Height(30)))
            {
                Application.OpenURL("https://your-documentation-url.com");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Close Wizard", GUILayout.Height(30)))
            {
                Close();
            }
        }

        private void DrawNavigationButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = currentStep != SetupStep.Welcome;
            if (GUILayout.Button("← Back", GUILayout.Height(35)))
            {
                currentStep--;
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (currentStep == SetupStep.Complete)
            {
                if (GUILayout.Button("Finish", GUILayout.Height(35), GUILayout.Width(100)))
                {
                    Close();
                }
            }
            else
            {
                bool canProceed = CanProceedToNextStep();
                GUI.enabled = canProceed;
                
                string buttonText = currentStep == SetupStep.UISetup ? "Setup!" : "Next →";
                
                if (GUILayout.Button(buttonText, GUILayout.Height(35), GUILayout.Width(100)))
                {
                    if (currentStep == SetupStep.UISetup)
                    {
                        PerformSetup();
                    }
                    currentStep++;
                }
                
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool CanProceedToNextStep()
        {
            switch (currentStep)
            {
                case SetupStep.Welcome:
                    return true;
                case SetupStep.PlayerSetup:
                    return playerObject != null;
                case SetupStep.SkillTreeCreation:
                    return !string.IsNullOrEmpty(skillTreeName) && !string.IsNullOrEmpty(skillTreePath);
                case SetupStep.UISetup:
                    return true;
                default:
                    return false;
            }
        }

        private void PerformSetup()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Setting up Skill Tree", "Starting setup...", 0f);

                // Step 1: Add component to player
                SetupPlayer();
                EditorUtility.DisplayProgressBar("Setting up Skill Tree", "Player setup complete", 0.2f);

                // Step 2: Create skill tree asset
                SkillTreeData skillTreeData = CreateSkillTreeAsset();
                EditorUtility.DisplayProgressBar("Setting up Skill Tree", "Skill tree created", 0.4f);

                // Step 3: Create example skills (only if creating new tree)
                if (createExampleSkills && !useExistingSkillTree)
                {
                    CreateExampleSkills(skillTreeData);
                    EditorUtility.DisplayProgressBar("Setting up Skill Tree", "Example skills created", 0.6f);
                }

                // Step 4: Link skill tree to player
                LinkSkillTreeToPlayer(skillTreeData);
                EditorUtility.DisplayProgressBar("Setting up Skill Tree", "Linked to player", 0.8f);

                // Step 5: Setup UI
                if (setupUI)
                {
                    SetupUIInScene();
                    EditorUtility.DisplayProgressBar("Setting up Skill Tree", "UI setup complete", 1f);
                }

                EditorUtility.ClearProgressBar();
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Error", $"An error occurred during setup:\n{e.Message}", "OK");
                Debug.LogError($"[Skill Tree] Setup error: {e}");
            }
        }

        private void SetupPlayer()
        {
            if (playerObject == null) return;

            SkillTreeComponent component = playerObject.GetComponent<SkillTreeComponent>();
            if (component == null)
            {
                component = playerObject.AddComponent<SkillTreeComponent>();
            }
        }

        private SkillTreeData CreateSkillTreeAsset()
        {
            // If using existing skill tree, return it
            if (useExistingSkillTree && existingSkillTree != null)
            {
                return existingSkillTree;
            }
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(skillTreePath))
            {
                Directory.CreateDirectory(skillTreePath);
            }

            // Create skill tree data
            SkillTreeData skillTreeData = ScriptableObject.CreateInstance<SkillTreeData>();
            string assetPath = $"{skillTreePath}/{skillTreeName}.asset";
            AssetDatabase.CreateAsset(skillTreeData, assetPath);
            return skillTreeData;
        }

        private void CreateExampleSkills(SkillTreeData skillTreeData)
        {
            string skillsPath = $"{skillTreePath}/Skills";
            if (!Directory.Exists(skillsPath))
            {
                Directory.CreateDirectory(skillsPath);
            }

            // Create basic skills
            Skill basicSkill1 = CreateSkill("Basic Attack", "A simple attack skill", skillsPath, 1);
            Skill basicSkill2 = CreateSkill("Basic Defense", "Increases defense", skillsPath, 1);
            Skill basicSkill3 = CreateSkill("Basic Magic", "A basic spell", skillsPath, 1);

            // Create advanced skills with prerequisites
            Skill advancedSkill1 = CreateSkill("Power Strike", "A powerful attack", skillsPath, 2);
            advancedSkill1.prerequisites.Add(new SkillPrerequisite { skill = basicSkill1, requiredLevel = 1 });

            Skill advancedSkill2 = CreateSkill("Shield Wall", "Strong defense", skillsPath, 2);
            advancedSkill2.prerequisites.Add(new SkillPrerequisite { skill = basicSkill2, requiredLevel = 1 });

            // Create ultimate skill
            Skill ultimateSkill = CreateSkill("Ultimate Power", "The ultimate skill", skillsPath, 3);
            ultimateSkill.prerequisites.Add(new SkillPrerequisite { skill = advancedSkill1, requiredLevel = 1 });
            ultimateSkill.prerequisites.Add(new SkillPrerequisite { skill = advancedSkill2, requiredLevel = 1 });

            // Add all skills to tree
            skillTreeData.allSkills.Add(basicSkill1);
            skillTreeData.allSkills.Add(basicSkill2);
            skillTreeData.allSkills.Add(basicSkill3);
            skillTreeData.allSkills.Add(advancedSkill1);
            skillTreeData.allSkills.Add(advancedSkill2);
            skillTreeData.allSkills.Add(ultimateSkill);

            EditorUtility.SetDirty(skillTreeData);
            AssetDatabase.SaveAssets();
        }

        private Skill CreateSkill(string name, string description, string path, int cost)
        {
            Skill skill = ScriptableObject.CreateInstance<Skill>();
            
            // Use reflection to set private fields (since they use PropertyGet)
            // For now, just create the asset - user will configure in inspector
            
            string assetPath = $"{path}/{name.Replace(" ", "")}.asset";
            AssetDatabase.CreateAsset(skill, assetPath);
            
            return skill;
        }

        private void LinkSkillTreeToPlayer(SkillTreeData skillTreeData)
        {
            if (playerObject == null) return;

            SkillTreeComponent component = playerObject.GetComponent<SkillTreeComponent>();
            if (component != null)
            {
                component.SetSkillTree(skillTreeData);
                EditorUtility.SetDirty(component);
            }
        }

        private void SetupUIInScene()
        {
            // Create EventSystem if needed (Input System UI for gamepad, else Standalone)
            UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                Type inputSystemUI = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputSystemUI != null)
                    eventSystemGO.AddComponent(inputSystemUI);
                else
                    eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Load selected prefabs by searching for them
            GameObject skillTreePrefab = null;
            GameObject skillHotbarPrefab = null;
            
            if (skillTreePrefabs != null && skillTreePrefabs.Length > 0 && selectedSkillTreePrefab < skillTreePrefabs.Length)
            {
                string[] guids = AssetDatabase.FindAssets(skillTreePrefabs[selectedSkillTreePrefab] + " t:Prefab");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    skillTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
            
            if (hotbarPrefabs != null && hotbarPrefabs.Length > 0 && selectedHotbarPrefab < hotbarPrefabs.Length)
            {
                string[] guids = AssetDatabase.FindAssets(hotbarPrefabs[selectedHotbarPrefab] + " t:Prefab");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    skillHotbarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            GameObject skillTreeUI = null;
            GameObject skillHotbarUI = null;

            // Instantiate SkillTree prefab (prefab já é um Canvas completo)
            if (skillTreePrefab != null)
            {
                skillTreeUI = (GameObject)PrefabUtility.InstantiatePrefab(skillTreePrefab);
                skillTreeUI.name = "SkillTreeUI";
            }
            else
            {
                Debug.LogError($"[Skill Tree] Could not find selected SkillTree prefab");
            }

            // Instantiate SkillHotbar prefab (prefab já é um Canvas completo)
            if (skillHotbarPrefab != null)
            {
                skillHotbarUI = (GameObject)PrefabUtility.InstantiatePrefab(skillHotbarPrefab);
                skillHotbarUI.name = "SkillHotbarUI";
            }
            else
            {
                Debug.LogError($"[Skill Tree] Could not find selected SkillHotbar prefab");
            }

            // Auto-link references
            if (playerObject != null && skillTreeUI != null && skillHotbarUI != null)
            {
                AutoLinkReferences(skillTreeUI, skillHotbarUI);
            }

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string message = "UI Setup Complete!\n\n";
            
            if (skillTreeUI != null && skillHotbarUI != null)
            {
                message += "✓ SkillTreeUI Canvas added to scene\n";
                message += "✓ SkillHotbarUI Canvas added to scene\n";
                message += "✓ EventSystem created\n";
                message += "✓ References auto-linked\n\n";
                message += "Ready to use!";
            }
            else
            {
                message += "⚠ Could not find selected prefabs.\n\n";
                message += "Please check that prefabs exist in Assets/KingEdward/SkillTree/Prefabs";
            }
            
            EditorUtility.DisplayDialog("UI Setup", message, "OK");
        }
        
        private void AutoLinkReferences(GameObject skillTreeUI, GameObject skillHotbarUI)
        {
            if (playerObject == null)
            {
                Debug.LogWarning("[Wizard] Player object is null, cannot auto-link");
                return;
            }
            
            SkillTreeComponent skillTreeComponent = playerObject.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent == null)
            {
                Debug.LogWarning("[Wizard] No SkillTreeComponent on player, cannot auto-link");
                return;
            }
            
            SkillTreeUI treeUI = skillTreeUI != null ? skillTreeUI.GetComponent<SkillTreeUI>() : null;
            SkillHotbarUI hotbarUI = skillHotbarUI != null ? skillHotbarUI.GetComponent<SkillHotbarUI>() : null;
            
            if (treeUI == null && hotbarUI == null)
            {
                Debug.LogWarning("[Wizard] No UI components found, cannot auto-link");
                return;
            }
            
            // Link SkillTreeUI
            if (treeUI != null)
            {
                SerializedObject soTree = new SerializedObject(treeUI);
                SerializedProperty propTreeComponent = soTree.FindProperty("m_SkillTreeComponent");
                if (propTreeComponent != null)
                {
                    SerializedProperty fromProp = propTreeComponent.FindPropertyRelative("m_From");
                    if (fromProp != null)
                    {
                        fromProp.enumValueIndex = 0; // Self
                        soTree.ApplyModifiedProperties();
                    }
                }
            }
            
            // Link SkillHotbarUI
            if (hotbarUI != null)
            {
                SerializedObject soHotbar = new SerializedObject(hotbarUI);
                SerializedProperty propHotbarComponent = soHotbar.FindProperty("m_SkillTreeComponent");
                if (propHotbarComponent != null)
                {
                    SerializedProperty fromProp = propHotbarComponent.FindPropertyRelative("m_From");
                    if (fromProp != null)
                    {
                        fromProp.enumValueIndex = 0; // Self
                        soHotbar.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
}
