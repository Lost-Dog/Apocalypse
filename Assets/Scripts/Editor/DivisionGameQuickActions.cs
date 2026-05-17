using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        gameSystemsObj.AddComponent<ProgressionManager>();
        gameSystemsObj.AddComponent<LootManager>();
        gameSystemsObj.AddComponent<ChallengeManager>();
        gameSystemsObj.AddComponent<SkillManager>();
        
        GameManager gm = gameSystemsObj.GetComponent<GameManager>();
        gm.missionManager = gameSystemsObj.GetComponent<MissionManager>();
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
        challenge.guaranteedLootRarity = LootRarity.Uncommon;
        
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
        skill1.skillId = 100;
        skill1.skillName = "Combat Medic";
        skill1.description = "Increases healing effectiveness and grants passive health regeneration.";
        skill1.requiredLevel = 1;
        skill1.baseCost = 100;
        skill1.maxLevel = 5;
        skill1.costMultiplierPerLevel = 1.5f;
        skill1.skillType = SkillData.SkillType.HealthRegen;
        skill1.baseValue = 2f;
        skill1.valuePerLevel = 1f;
        AssetDatabase.CreateAsset(skill1, "Assets/Resources/Skills/Skill_CombatMedic.asset");
        
        var skill2 = ScriptableObject.CreateInstance<SkillData>();
        skill2.skillId = 101;
        skill2.skillName = "Marksman";
        skill2.description = "Increases weapon damage and critical hit chance.";
        skill2.requiredLevel = 3;
        skill2.baseCost = 150;
        skill2.maxLevel = 5;
        skill2.costMultiplierPerLevel = 1.5f;
        skill2.skillType = SkillData.SkillType.Damage;
        skill2.baseValue = 15f;
        skill2.valuePerLevel = 5f;
        AssetDatabase.CreateAsset(skill2, "Assets/Resources/Skills/Skill_Marksman.asset");
        
        var skill3 = ScriptableObject.CreateInstance<SkillData>();
        skill3.skillId = 102;
        skill3.skillName = "Tech Specialist";
        skill3.description = "Reduces skill cooldowns and increases gadget effectiveness.";
        skill3.requiredLevel = 3;
        skill3.baseCost = 150;
        skill3.maxLevel = 5;
        skill3.costMultiplierPerLevel = 1.5f;
        skill3.skillType = SkillData.SkillType.CooldownReduction;
        skill3.baseValue = 20f;
        skill3.valuePerLevel = 5f;
        AssetDatabase.CreateAsset(skill3, "Assets/Resources/Skills/Skill_TechSpecialist.asset");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Created 3 sample skills in Resources/Skills/");
        EditorUtility.DisplayDialog("Success", "Created 3 sample skills:\n• Combat Medic\n• Marksman\n• Tech Specialist", "OK");
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
    
    [MenuItem("Division Game/Quick Actions/Configure Mission SOs on Selected", true)]
    public static bool ConfigureMissionSOsValidate()
    {
        return Selection.activeGameObject != null;
    }

    /// <summary>
    /// Scans the selected GameObject and all its children for components that reference
    /// MissionData ScriptableObjects and populates those fields from Resources/Missions.
    /// Targets: MissionManager.allMissions, MissionOfferManager (skipped — runtime-driven).
    /// </summary>
    [MenuItem("Division Game/Quick Actions/Configure Mission SOs on Selected")]
    public static void ConfigureMissionSOs()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Select a GameObject first.", "OK");
            return;
        }

        // Load all MissionData assets from the canonical folder.
        string[] guids = AssetDatabase.FindAssets("t:MissionData", new[] { "Assets/Game/Resources/Missions" });
        if (guids.Length == 0)
        {
            // Fallback: search entire project.
            guids = AssetDatabase.FindAssets("t:MissionData");
        }

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Missions Found", "Could not find any MissionData assets in the project.", "OK");
            return;
        }

        MissionData[] allMissions = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<MissionData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(m => m != null)
            .OrderBy(m => m.missionName)
            .ToArray();

        int totalConfigured = 0;

        // Collect self + all descendants.
        Component[] allComponents = selected.GetComponentsInChildren<Component>(includeInactive: true);

        foreach (Component component in allComponents)
        {
            if (component == null) continue;

            SerializedObject so = new SerializedObject(component);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            bool modified = false;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = prop.propertyType != SerializedPropertyType.String;

                // Populate List<MissionData> fields.
                if (prop.isArray && prop.arrayElementType == "PPtr<$MissionData>")
                {
                    prop.ClearArray();
                    for (int i = 0; i < allMissions.Length; i++)
                    {
                        prop.InsertArrayElementAtIndex(i);
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = allMissions[i];
                    }

                    Debug.Log($"✓ {component.gameObject.name} / {component.GetType().Name}.{prop.name}: populated {allMissions.Length} missions.");
                    modified = true;
                    totalConfigured++;
                }

                // Populate single MissionData fields only when they are null.
                else if (prop.propertyType == SerializedPropertyType.ObjectReference
                         && prop.objectReferenceValue == null)
                {
                    System.Type fieldType = null;
                    try
                    {
                        var fieldInfo = component.GetType()
                            .GetField(prop.name,
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic);
                        if (fieldInfo != null) fieldType = fieldInfo.FieldType;
                    }
                    catch { /* ignore reflection errors */ }

                    if (fieldType == typeof(MissionData) && allMissions.Length > 0)
                    {
                        prop.objectReferenceValue = allMissions[0];
                        Debug.Log($"✓ {component.gameObject.name} / {component.GetType().Name}.{prop.name}: assigned '{allMissions[0].missionName}'.");
                        modified = true;
                        totalConfigured++;
                    }
                }
            }

            if (modified)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }
        }

        if (totalConfigured == 0)
        {
            EditorUtility.DisplayDialog(
                "Nothing to Configure",
                $"No MissionData fields were found on '{selected.name}' or its children.\n\nMake sure MissionManager or similar components are attached.",
                "OK"
            );
            return;
        }

        EditorSceneManager.MarkSceneDirty(selected.scene);
        AssetDatabase.SaveAssets();

        string summary = $"Configured {totalConfigured} field(s) across '{selected.name}' and its children using {allMissions.Length} MissionData assets.";
        Debug.Log($"[Configure Mission SOs] {summary}");
        EditorUtility.DisplayDialog("Done", summary, "OK");
    }

    /// <summary>
    /// Finds all SkillData assets in the project and assigns them to the SkillManager
    /// on the selected GameObject (or any SkillManager in the scene if none is selected).
    /// </summary>
    /// <summary>
    /// Rewrites the m_Script GUID in all .asset files under Assets/Game/Skillls/Skill Data/
    /// so they point to the current SkillData.cs. Run once after the script schema change.
    /// </summary>
    [MenuItem("Division Game/Quick Actions/Fix Skill Asset Script References")]
    public static void FixSkillAssetScriptReferences()
    {
        const string oldGuid = "fad4c2f21cc9cce45b925b3f6afdfdac";
        const string targetFolder = "Assets/Game/Skillls/Skill Data";

        // Derive the live GUID from the current SkillData.cs meta file.
        string scriptPath = AssetDatabase.FindAssets("t:MonoScript SkillData")
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .FirstOrDefault(p => System.IO.Path.GetFileNameWithoutExtension(p) == "SkillData");

        if (scriptPath == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not locate SkillData.cs in the project.", "OK");
            return;
        }

        string newGuid = AssetDatabase.AssetPathToGUID(scriptPath);
        if (string.IsNullOrEmpty(newGuid))
        {
            EditorUtility.DisplayDialog("Error", $"Could not resolve GUID for {scriptPath}.", "OK");
            return;
        }

        if (newGuid == oldGuid)
        {
            EditorUtility.DisplayDialog("Nothing to Do", "Script GUIDs already match — assets are already pointing to the correct SkillData.cs.", "OK");
            return;
        }

        string absoluteFolder = System.IO.Path.Combine(Application.dataPath, "../", targetFolder).Replace('\\', '/');
        string[] assetFiles = System.IO.Directory.GetFiles(absoluteFolder, "*.asset", System.IO.SearchOption.AllDirectories);

        if (assetFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No Assets Found", $"No .asset files found in {targetFolder}.", "OK");
            return;
        }

        int fixedCount = 0;
        int skippedCount = 0;

        foreach (string filePath in assetFiles)
        {
            string text = System.IO.File.ReadAllText(filePath);
            if (!text.Contains(oldGuid)) { skippedCount++; continue; }

            string patched = text.Replace(oldGuid, newGuid);
            System.IO.File.WriteAllText(filePath, patched);
            fixedCount++;
            Debug.Log($"✓ Patched: {System.IO.Path.GetFileName(filePath)}");
        }

        AssetDatabase.Refresh();

        string summary = $"Patched {fixedCount} file(s). Skipped {skippedCount} (GUID already correct).\n\nOld: {oldGuid}\nNew: {newGuid}";
        Debug.Log($"[Fix Skill Assets] {summary}");
        EditorUtility.DisplayDialog("Done", summary + "\n\nNow run 'Assign Skills to SkillManager'.", "OK");
    }

    [MenuItem("Division Game/Quick Actions/Assign Skills to SkillManager")]
    public static void AssignSkillsToSkillManager()
    {
        SkillManager skillManager = null;

        if (Selection.activeGameObject != null)
            skillManager = Selection.activeGameObject.GetComponentInChildren<SkillManager>(includeInactive: true);

        if (skillManager == null)
            skillManager = Object.FindFirstObjectByType<SkillManager>(FindObjectsInactive.Include);

        if (skillManager == null)
        {
            EditorUtility.DisplayDialog("No SkillManager", "Could not find a SkillManager in the scene.", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:SkillData");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Skills Found", "Could not find any SkillData assets in the project.", "OK");
            return;
        }

        SkillData[] skills = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<SkillData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(s => s != null)
            .OrderBy(s => s.name)
            .ToArray();

        SerializedObject so = new SerializedObject(skillManager);
        SerializedProperty allSkillsProp = so.FindProperty("allSkills");

        allSkillsProp.ClearArray();
        for (int i = 0; i < skills.Length; i++)
        {
            allSkillsProp.InsertArrayElementAtIndex(i);
            allSkillsProp.GetArrayElementAtIndex(i).objectReferenceValue = skills[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(skillManager);
        EditorSceneManager.MarkSceneDirty(skillManager.gameObject.scene);

        string summary = $"Assigned {skills.Length} SkillData assets to SkillManager on '{skillManager.gameObject.name}'.";
        Debug.Log($"[Assign Skills] {summary}");
        foreach (var skill in skills)
            Debug.Log($"  • {skill.name}");

        EditorUtility.DisplayDialog("Done", summary, "OK");
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
