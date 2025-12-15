using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class DivisionGameSetupTool : EditorWindow
{
    private Vector2 scrollPos;
    private bool showMissionSetup = true;
    private bool showManagerSetup = true;
    private bool showResourceSetup = true;
    private bool showValidation = true;
    
    [MenuItem("Division Game/Setup Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<DivisionGameSetupTool>("Division Setup");
        window.minSize = new Vector2(500, 600);
    }
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Division-Style Game Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool helps you configure all game systems correctly.", MessageType.Info);
        
        GUILayout.Space(10);
        
        DrawManagerSetup();
        DrawResourceSetup();
        DrawMissionSetup();
        DrawValidation();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawManagerSetup()
    {
        showManagerSetup = EditorGUILayout.Foldout(showManagerSetup, "1. Manager Setup", true);
        if (!showManagerSetup) return;
        
        EditorGUI.indentLevel++;
        
        GameObject gameSystemsObj = GameObject.Find("GameSystems");
        
        if (gameSystemsObj == null)
        {
            EditorGUILayout.HelpBox("GameSystems object not found in scene!", MessageType.Error);
            if (GUILayout.Button("Create GameSystems Object"))
            {
                CreateGameSystemsObject();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("GameSystems found: " + gameSystemsObj.name, MessageType.Info);
            
            GameManager gameManager = gameSystemsObj.GetComponent<GameManager>();
            MissionManager missionManager = gameSystemsObj.GetComponent<MissionManager>();
            FactionManager factionManager = gameSystemsObj.GetComponent<FactionManager>();
            ProgressionManager progressionManager = gameSystemsObj.GetComponent<ProgressionManager>();
            LootManager lootManager = gameSystemsObj.GetComponent<LootManager>();
            ChallengeManager challengeManager = gameSystemsObj.GetComponent<ChallengeManager>();
            SkillManager skillManager = gameSystemsObj.GetComponent<SkillManager>();
            
            if (gameManager == null)
            {
                EditorGUILayout.HelpBox("Missing: GameManager component", MessageType.Warning);
                if (GUILayout.Button("Add GameManager"))
                {
                    gameManager = gameSystemsObj.AddComponent<GameManager>();
                    EditorUtility.SetDirty(gameSystemsObj);
                }
            }
            else
            {
                EditorGUILayout.LabelField("✓ GameManager", EditorStyles.helpBox);
            }
            
            if (missionManager == null)
            {
                EditorGUILayout.HelpBox("Missing: MissionManager component", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("✓ MissionManager", EditorStyles.helpBox);
            }
            
            if (factionManager == null)
            {
                EditorGUILayout.HelpBox("Missing: FactionManager component", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("✓ FactionManager", EditorStyles.helpBox);
            }
            
            if (progressionManager == null)
            {
                EditorGUILayout.HelpBox("Missing: ProgressionManager component", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("✓ ProgressionManager", EditorStyles.helpBox);
            }
            
            if (lootManager == null)
            {
                EditorGUILayout.HelpBox("Missing: LootManager component", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("✓ LootManager", EditorStyles.helpBox);
            }
            
            if (challengeManager == null)
            {
                EditorGUILayout.HelpBox("Missing: ChallengeManager component", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("✓ ChallengeManager", EditorStyles.helpBox);
            }
            
            if (skillManager == null)
            {
                EditorGUILayout.HelpBox("Missing: SkillManager component", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("✓ SkillManager", EditorStyles.helpBox);
            }
            
            GUILayout.Space(10);
            
            if (gameManager != null && missionManager != null && factionManager != null && 
                progressionManager != null && lootManager != null && challengeManager != null && 
                skillManager != null)
            {
                if (GUILayout.Button("Auto-Wire All Managers"))
                {
                    AutoWireManagers(gameManager, missionManager, factionManager, progressionManager, 
                                   lootManager, challengeManager, skillManager);
                }
            }
            
            if (GUILayout.Button("Clean Up Duplicate/Empty GameObjects"))
            {
                CleanUpGameSystems();
            }
        }
        
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
    }
    
    private void DrawResourceSetup()
    {
        showResourceSetup = EditorGUILayout.Foldout(showResourceSetup, "2. Resources Folder Setup", true);
        if (!showResourceSetup) return;
        
        EditorGUI.indentLevel++;
        
        string resourcesPath = "Assets/Resources";
        string missionsResourcePath = "Assets/Resources/Missions";
        string challengesResourcePath = "Assets/Resources/Challenges";
        string skillsResourcePath = "Assets/Resources/Skills";
        
        bool resourcesExists = Directory.Exists(resourcesPath);
        bool missionsResourceExists = Directory.Exists(missionsResourcePath);
        bool challengesResourceExists = Directory.Exists(challengesResourcePath);
        bool skillsResourceExists = Directory.Exists(skillsResourcePath);
        
        if (!resourcesExists)
        {
            EditorGUILayout.HelpBox("Resources folder doesn't exist!", MessageType.Warning);
            if (GUILayout.Button("Create Resources Folder"))
            {
                Directory.CreateDirectory(resourcesPath);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            EditorGUILayout.LabelField("✓ Resources folder exists", EditorStyles.helpBox);
        }
        
        if (!missionsResourceExists && resourcesExists)
        {
            EditorGUILayout.HelpBox("Resources/Missions folder doesn't exist - MissionManager won't load missions!", MessageType.Error);
            if (GUILayout.Button("Create Resources/Missions Folder"))
            {
                Directory.CreateDirectory(missionsResourcePath);
                AssetDatabase.Refresh();
            }
        }
        else if (missionsResourceExists)
        {
            EditorGUILayout.LabelField("✓ Resources/Missions exists", EditorStyles.helpBox);
        }
        
        if (!challengesResourceExists && resourcesExists)
        {
            EditorGUILayout.HelpBox("Resources/Challenges folder doesn't exist!", MessageType.Warning);
            if (GUILayout.Button("Create Resources/Challenges Folder"))
            {
                Directory.CreateDirectory(challengesResourcePath);
                AssetDatabase.Refresh();
            }
        }
        else if (challengesResourceExists)
        {
            EditorGUILayout.LabelField("✓ Resources/Challenges exists", EditorStyles.helpBox);
        }
        
        if (!skillsResourceExists && resourcesExists)
        {
            EditorGUILayout.HelpBox("Resources/Skills folder doesn't exist!", MessageType.Warning);
            if (GUILayout.Button("Create Resources/Skills Folder"))
            {
                Directory.CreateDirectory(skillsResourcePath);
                AssetDatabase.Refresh();
            }
        }
        else if (skillsResourceExists)
        {
            EditorGUILayout.LabelField("✓ Resources/Skills exists", EditorStyles.helpBox);
        }
        
        GUILayout.Space(10);
        
        if (Directory.Exists("Assets/Missions"))
        {
            var missionAssets = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Missions" });
            EditorGUILayout.LabelField($"Found {missionAssets.Length} missions in Assets/Missions");
            
            if (missionAssets.Length > 0 && missionsResourceExists)
            {
                if (GUILayout.Button($"Copy All Missions to Resources/Missions ({missionAssets.Length} files)"))
                {
                    CopyMissionsToResources();
                }
            }
        }
        
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
    }
    
    private void DrawMissionSetup()
    {
        showMissionSetup = EditorGUILayout.Foldout(showMissionSetup, "3. Mission Assets", true);
        if (!showMissionSetup) return;
        
        EditorGUI.indentLevel++;
        
        if (!Directory.Exists("Assets/Missions"))
        {
            EditorGUILayout.HelpBox("Assets/Missions folder not found!", MessageType.Error);
            if (GUILayout.Button("Create Assets/Missions Folder"))
            {
                Directory.CreateDirectory("Assets/Missions");
                AssetDatabase.Refresh();
            }
        }
        else
        {
            var missionAssets = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Missions" });
            EditorGUILayout.LabelField($"Mission assets in Assets/Missions: {missionAssets.Length}", EditorStyles.helpBox);
            
            if (missionAssets.Length == 0)
            {
                EditorGUILayout.HelpBox("No mission assets found! Use 'Division Game -> Create All 30 Missions' to create them.", MessageType.Warning);
            }
            else if (missionAssets.Length < 30)
            {
                EditorGUILayout.HelpBox($"Only {missionAssets.Length}/30 missions found!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"✓ All {missionAssets.Length} missions created", EditorStyles.helpBox);
            }
        }
        
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
    }
    
    private void DrawValidation()
    {
        showValidation = EditorGUILayout.Foldout(showValidation, "4. Validation & Health Check", true);
        if (!showValidation) return;
        
        EditorGUI.indentLevel++;
        
        if (GUILayout.Button("Run Full System Check", GUILayout.Height(30)))
        {
            RunFullValidation();
        }
        
        EditorGUI.indentLevel--;
    }
    
    private void CreateGameSystemsObject()
    {
        GameObject gameSystemsObj = new GameObject("GameSystems");
        gameSystemsObj.AddComponent<GameManager>();
        gameSystemsObj.AddComponent<MissionManager>();
        gameSystemsObj.AddComponent<FactionManager>();
        gameSystemsObj.AddComponent<ProgressionManager>();
        gameSystemsObj.AddComponent<LootManager>();
        gameSystemsObj.AddComponent<ChallengeManager>();
        gameSystemsObj.AddComponent<SkillManager>();
        
        Selection.activeGameObject = gameSystemsObj;
        EditorUtility.SetDirty(gameSystemsObj);
        
        Debug.Log("Created GameSystems object with all managers!");
    }
    
    private void AutoWireManagers(GameManager gameManager, MissionManager missionManager, 
                                  FactionManager factionManager, ProgressionManager progressionManager,
                                  LootManager lootManager, ChallengeManager challengeManager, 
                                  SkillManager skillManager)
    {
        gameManager.missionManager = missionManager;
        gameManager.factionManager = factionManager;
        gameManager.progressionManager = progressionManager;
        gameManager.lootManager = lootManager;
        gameManager.challengeManager = challengeManager;
        gameManager.skillManager = skillManager;
        
        EditorUtility.SetDirty(gameManager);
        
        Debug.Log("Auto-wired all managers to GameManager!");
        EditorUtility.DisplayDialog("Success", "All managers have been wired to GameManager!", "OK");
    }
    
    private void CleanUpGameSystems()
    {
        GameObject gameSystemsObj = GameObject.Find("GameSystems");
        if (gameSystemsObj == null) return;
        
        int cleanedCount = 0;
        
        Transform[] children = gameSystemsObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == gameSystemsObj.transform) continue;
            
            if (child.GetComponents<Component>().Length <= 1)
            {
                DestroyImmediate(child.gameObject);
                cleanedCount++;
            }
        }
        
        Debug.Log($"Cleaned up {cleanedCount} empty/duplicate GameObjects from GameSystems!");
        EditorUtility.DisplayDialog("Cleanup Complete", $"Removed {cleanedCount} empty child objects!", "OK");
    }
    
    private void CopyMissionsToResources()
    {
        if (!Directory.Exists("Assets/Resources/Missions"))
        {
            Directory.CreateDirectory("Assets/Resources/Missions");
        }
        
        var missionGuids = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Missions" });
        int copiedCount = 0;
        
        foreach (var guid in missionGuids)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(sourcePath);
            string destPath = $"Assets/Resources/Missions/{fileName}";
            
            if (!File.Exists(destPath))
            {
                AssetDatabase.CopyAsset(sourcePath, destPath);
                copiedCount++;
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"Copied {copiedCount} missions to Resources/Missions/");
        EditorUtility.DisplayDialog("Copy Complete", $"Copied {copiedCount} mission files to Resources/Missions!", "OK");
    }
    
    private void RunFullValidation()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== Division Game System Validation ===\n");
        
        GameObject gameSystemsObj = GameObject.Find("GameSystems");
        if (gameSystemsObj == null)
        {
            report.AppendLine("❌ GameSystems object not found in scene!");
        }
        else
        {
            report.AppendLine("✓ GameSystems object exists");
            
            var gameManager = gameSystemsObj.GetComponent<GameManager>();
            var missionManager = gameSystemsObj.GetComponent<MissionManager>();
            var factionManager = gameSystemsObj.GetComponent<FactionManager>();
            var progressionManager = gameSystemsObj.GetComponent<ProgressionManager>();
            var lootManager = gameSystemsObj.GetComponent<LootManager>();
            var challengeManager = gameSystemsObj.GetComponent<ChallengeManager>();
            var skillManager = gameSystemsObj.GetComponent<SkillManager>();
            
            report.AppendLine(gameManager != null ? "✓ GameManager" : "❌ GameManager MISSING");
            report.AppendLine(missionManager != null ? "✓ MissionManager" : "❌ MissionManager MISSING");
            report.AppendLine(factionManager != null ? "✓ FactionManager" : "❌ FactionManager MISSING");
            report.AppendLine(progressionManager != null ? "✓ ProgressionManager" : "❌ ProgressionManager MISSING");
            report.AppendLine(lootManager != null ? "✓ LootManager" : "❌ LootManager MISSING");
            report.AppendLine(challengeManager != null ? "✓ ChallengeManager" : "❌ ChallengeManager MISSING");
            report.AppendLine(skillManager != null ? "✓ SkillManager" : "❌ SkillManager MISSING");
            
            report.AppendLine();
            
            if (gameManager != null)
            {
                report.AppendLine("GameManager References:");
                report.AppendLine(gameManager.missionManager != null ? "  ✓ missionManager wired" : "  ❌ missionManager NOT WIRED");
                report.AppendLine(gameManager.factionManager != null ? "  ✓ factionManager wired" : "  ❌ factionManager NOT WIRED");
                report.AppendLine(gameManager.progressionManager != null ? "  ✓ progressionManager wired" : "  ❌ progressionManager NOT WIRED");
                report.AppendLine(gameManager.lootManager != null ? "  ✓ lootManager wired" : "  ❌ lootManager NOT WIRED");
                report.AppendLine(gameManager.challengeManager != null ? "  ✓ challengeManager wired" : "  ❌ challengeManager NOT WIRED");
                report.AppendLine(gameManager.skillManager != null ? "  ✓ skillManager wired" : "  ❌ skillManager NOT WIRED");
            }
        }
        
        report.AppendLine();
        report.AppendLine("Resources Folders:");
        report.AppendLine(Directory.Exists("Assets/Resources") ? "✓ Assets/Resources" : "❌ Assets/Resources MISSING");
        report.AppendLine(Directory.Exists("Assets/Resources/Missions") ? "✓ Assets/Resources/Missions" : "❌ Assets/Resources/Missions MISSING");
        report.AppendLine(Directory.Exists("Assets/Resources/Challenges") ? "✓ Assets/Resources/Challenges" : "❌ Assets/Resources/Challenges MISSING");
        report.AppendLine(Directory.Exists("Assets/Resources/Skills") ? "✓ Assets/Resources/Skills" : "❌ Assets/Resources/Skills MISSING");
        
        report.AppendLine();
        
        var missionsInAssets = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Missions" });
        report.AppendLine($"Missions in Assets/Missions: {missionsInAssets.Length}");
        
        if (Directory.Exists("Assets/Resources/Missions"))
        {
            var missionsInResources = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Resources/Missions" });
            report.AppendLine($"Missions in Resources/Missions: {missionsInResources.Length}");
        }
        
        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("Validation Complete", "Check the Console for full report!", "OK");
    }
}
