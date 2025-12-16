using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class DivisionGameQuickActions : EditorWindow
{
    [MenuItem("Division Game/Quick Actions/Add GameManager to Scene")]
    public static void AddGameManagerToScene()
    {
        GameObject existing = GameObject.Find("GameSystems");
        if (existing != null)
        {
            Selection.activeGameObject = existing;
            EditorUtility.DisplayDialog("Already Exists", "GameSystems already exists in the scene!", "OK");
            return;
        }
        
        GameObject gameSystemsObj = new GameObject("GameSystems");
        gameSystemsObj.AddComponent<GameManager>();
        gameSystemsObj.AddComponent<MissionManager>();
        gameSystemsObj.AddComponent<FactionManager>();
        gameSystemsObj.AddComponent<ProgressionManager>();
        gameSystemsObj.AddComponent<LootManager>();
        gameSystemsObj.AddComponent<ChallengeManager>();
        gameSystemsObj.AddComponent<SkillManager>();
        
        GameManager gm = gameSystemsObj.GetComponent<GameManager>();
        gm.missionManager = gameSystemsObj.GetComponent<MissionManager>();
        gm.factionManager = gameSystemsObj.GetComponent<FactionManager>();
        gm.progressionManager = gameSystemsObj.GetComponent<ProgressionManager>();
        gm.lootManager = gameSystemsObj.GetComponent<LootManager>();
        gm.challengeManager = gameSystemsObj.GetComponent<ChallengeManager>();
        gm.skillManager = gameSystemsObj.GetComponent<SkillManager>();
        
        Selection.activeGameObject = gameSystemsObj;
        EditorUtility.SetDirty(gameSystemsObj);
        
        Debug.Log("Created and configured GameSystems with all managers!");
        EditorUtility.DisplayDialog("Success", "GameSystems object created with all managers configured!", "OK");
    }
    
    [MenuItem("Division Game/Quick Actions/Setup Resources Folders")]
    public static void SetupResourcesFolders()
    {
        string[] folders = new string[]
        {
            "Assets/Resources",
            "Assets/Resources/Missions",
            "Assets/Resources/Challenges",
            "Assets/Resources/Skills"
        };
        
        int createdCount = 0;
        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                createdCount++;
                Debug.Log($"Created folder: {folder}");
            }
        }
        
        AssetDatabase.Refresh();
        
        if (createdCount > 0)
        {
            EditorUtility.DisplayDialog("Success", $"Created {createdCount} Resource folders!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "All Resource folders already exist!", "OK");
        }
    }
    
    [MenuItem("Division Game/Quick Actions/Copy Missions to Resources")]
    public static void CopyMissionsToResources()
    {
        if (!Directory.Exists("Assets/Resources/Missions"))
        {
            Directory.CreateDirectory("Assets/Resources/Missions");
        }
        
        var missionGuids = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Missions" });
        
        if (missionGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Missions", "No MissionData assets found in Assets/Missions!", "OK");
            return;
        }
        
        int copiedCount = 0;
        int skippedCount = 0;
        
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
            else
            {
                skippedCount++;
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"Copied {copiedCount} missions to Resources/Missions/ (Skipped {skippedCount} existing)");
        EditorUtility.DisplayDialog("Copy Complete", $"Copied {copiedCount} mission files!\nSkipped {skippedCount} existing files.", "OK");
    }
    
    [MenuItem("Division Game/Quick Actions/Create Sample Challenge")]
    public static void CreateSampleChallenge()
    {
        if (!Directory.Exists("Assets/Resources/Challenges"))
        {
            Directory.CreateDirectory("Assets/Resources/Challenges");
        }
        
        var challenge = ScriptableObject.CreateInstance<ChallengeData>();
        challenge.challengeName = "Supply Drop Defense";
        challenge.description = "A supply drop has landed. Defend it from waves of rogues!";
        challenge.challengeType = ChallengeData.ChallengeType.SupplyDrop;
        challenge.recommendedLevel = 3;
        challenge.timeLimit = 300f;
        challenge.baseXPReward = 300;
        challenge.baseCurrencyReward = 150;
        challenge.guaranteedLootRarity = LootManager.Rarity.Uncommon;
        
        challenge.spawnItems.Add(new ChallengeData.SpawnableItem
        {
            itemName = "Enemy Guards",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 12,
            maxCount = 15,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 15f,
            requireNavMesh = true,
            priority = 5
        });
        
        AssetDatabase.CreateAsset(challenge, "Assets/Resources/Challenges/Challenge_SupplyDrop.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Selection.activeObject = challenge;
        Debug.Log("Created sample challenge: Supply Drop Defense");
        EditorUtility.DisplayDialog("Success", "Created sample Challenge in Resources/Challenges!", "OK");
    }
    
    [MenuItem("Division Game/Quick Actions/Create Sample Skills")]
    public static void CreateSampleSkills()
    {
        if (!Directory.Exists("Assets/Resources/Skills"))
        {
            Directory.CreateDirectory("Assets/Resources/Skills");
        }
        
        var skill1 = ScriptableObject.CreateInstance<SkillData>();
        skill1.skillName = "Combat Medic";
        skill1.description = "Increases healing effectiveness and grants passive health regeneration.";
        skill1.specialization = "Medical";
        skill1.statBonuses.Add(new StatBonus { statName = "HealingPower", bonusValue = 25f, isPercentage = true });
        skill1.statBonuses.Add(new StatBonus { statName = "HealthRegen", bonusValue = 2f, isPercentage = false });
        AssetDatabase.CreateAsset(skill1, "Assets/Resources/Skills/Skill_CombatMedic.asset");
        
        var skill2 = ScriptableObject.CreateInstance<SkillData>();
        skill2.skillName = "Marksman";
        skill2.description = "Increases weapon damage and critical hit chance.";
        skill2.specialization = "Combat";
        skill2.statBonuses.Add(new StatBonus { statName = "WeaponDamage", bonusValue = 15f, isPercentage = true });
        skill2.statBonuses.Add(new StatBonus { statName = "CritChance", bonusValue = 10f, isPercentage = true });
        AssetDatabase.CreateAsset(skill2, "Assets/Resources/Skills/Skill_Marksman.asset");
        
        var skill3 = ScriptableObject.CreateInstance<SkillData>();
        skill3.skillName = "Tech Specialist";
        skill3.description = "Reduces skill cooldowns and increases gadget effectiveness.";
        skill3.specialization = "Tech";
        skill3.statBonuses.Add(new StatBonus { statName = "CooldownReduction", bonusValue = 20f, isPercentage = true });
        skill3.statBonuses.Add(new StatBonus { statName = "GadgetDamage", bonusValue = 30f, isPercentage = true });
        AssetDatabase.CreateAsset(skill3, "Assets/Resources/Skills/Skill_TechSpecialist.asset");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Created 3 sample skills in Resources/Skills/");
        EditorUtility.DisplayDialog("Success", "Created 3 sample skills:\n• Combat Medic\n• Marksman\n• Tech Specialist", "OK");
    }
    
    [MenuItem("Division Game/Quick Actions/Add FactionMember to Selected")]
    public static void AddFactionMemberToSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects first!", "OK");
            return;
        }
        
        int addedCount = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<FactionMember>() == null)
            {
                obj.AddComponent<FactionMember>();
                EditorUtility.SetDirty(obj);
                addedCount++;
            }
        }
        
        Debug.Log($"Added FactionMember to {addedCount} GameObjects");
        EditorUtility.DisplayDialog("Success", $"Added FactionMember component to {addedCount} GameObjects!", "OK");
    }
    
    [MenuItem("Division Game/Quick Actions/List All Missions")]
    public static void ListAllMissions()
    {
        var missionGuids = AssetDatabase.FindAssets("t:MissionData");
        
        if (missionGuids.Length == 0)
        {
            Debug.Log("No missions found in project!");
            return;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Found {missionGuids.Length} Missions ===\n");
        
        foreach (var guid in missionGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MissionData mission = AssetDatabase.LoadAssetAtPath<MissionData>(path);
            
            if (mission != null)
            {
                sb.AppendLine($"[Lv{mission.levelRequirement}] {mission.missionName}");
                sb.AppendLine($"  Path: {path}");
                sb.AppendLine($"  Main: {mission.isMainStory}, Boss: {mission.isBossMission}");
                sb.AppendLine($"  Rewards: {mission.xpReward} XP, {mission.currencyReward} Credits");
                sb.AppendLine();
            }
        }
        
        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Mission List", $"Found {missionGuids.Length} missions. Check Console for details.", "OK");
    }
    
    [MenuItem("Division Game/Documentation/Open Quick Start Guide")]
    public static void OpenQuickStartGuide()
    {
        string pagePath = "/Pages/Division Game - Quick Start Guide.md";
        
        if (File.Exists(Application.dataPath + "/.." + pagePath))
        {
            Debug.Log($"Opening: {pagePath}");
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "Quick Start Guide page not found!", "OK");
        }
    }
}
