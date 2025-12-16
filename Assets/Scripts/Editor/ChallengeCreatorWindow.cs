using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChallengeCreatorWindow : EditorWindow
{
    private string challengeName = "New Challenge";
    private string description = "Challenge description...";
    private ChallengeData.ChallengeType challengeType = ChallengeData.ChallengeType.SupplyDrop;
    private ChallengeData.ChallengeFrequency frequency = ChallengeData.ChallengeFrequency.WorldEvent;
    private ChallengeData.ChallengeDifficulty difficulty = ChallengeData.ChallengeDifficulty.Medium;
    
    private float timeLimit = 300f;
    private float detectionRadius = 50f;
    private int recommendedLevel = 1;
    private int requiredLevel = 1;
    
    private int xpReward = 200;
    private int currencyReward = 100;
    private LootManager.Rarity lootRarity = LootManager.Rarity.Uncommon;
    private int lootCount = 1;
    
    private List<SpawnItemTemplate> spawnTemplates = new List<SpawnItemTemplate>();
    private Vector2 scrollPosition;
    private Vector2 spawnScrollPosition;
    
    private bool showBasicSettings = true;
    private bool showRewards = true;
    private bool showSpawnItems = true;
    
    [MenuItem("Division Game/Challenge System/Create New Challenge")]
    public static void ShowWindow()
    {
        ChallengeCreatorWindow window = GetWindow<ChallengeCreatorWindow>("Challenge Creator");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }
    
    private void OnEnable()
    {
        if (spawnTemplates.Count == 0)
        {
            AddDefaultSpawnTemplate();
        }
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Creator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create a new challenge with flexible spawn configuration.", MessageType.Info);
        EditorGUILayout.Space(10);
        
        showBasicSettings = EditorGUILayout.Foldout(showBasicSettings, "Basic Settings", true);
        if (showBasicSettings)
        {
            EditorGUI.indentLevel++;
            DrawBasicSettings();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        showRewards = EditorGUILayout.Foldout(showRewards, "Rewards", true);
        if (showRewards)
        {
            EditorGUI.indentLevel++;
            DrawRewards();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        showSpawnItems = EditorGUILayout.Foldout(showSpawnItems, "Spawn Items Configuration", true);
        if (showSpawnItems)
        {
            EditorGUI.indentLevel++;
            DrawSpawnItems();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Add Preset Configuration", GUILayout.Height(35), GUILayout.Width(200)))
        {
            ShowPresetMenu();
        }
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create Challenge", GUILayout.Height(35), GUILayout.Width(150)))
        {
            CreateChallenge();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawBasicSettings()
    {
        challengeName = EditorGUILayout.TextField("Challenge Name", challengeName);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
        
        challengeType = (ChallengeData.ChallengeType)EditorGUILayout.EnumPopup("Challenge Type", challengeType);
        frequency = (ChallengeData.ChallengeFrequency)EditorGUILayout.EnumPopup("Frequency", frequency);
        difficulty = (ChallengeData.ChallengeDifficulty)EditorGUILayout.EnumPopup("Difficulty", difficulty);
        
        EditorGUILayout.Space(5);
        
        timeLimit = EditorGUILayout.FloatField("Time Limit (seconds)", timeLimit);
        detectionRadius = EditorGUILayout.FloatField("Detection Radius", detectionRadius);
        recommendedLevel = EditorGUILayout.IntField("Recommended Level", recommendedLevel);
        requiredLevel = EditorGUILayout.IntField("Required Level", requiredLevel);
    }
    
    private void DrawRewards()
    {
        xpReward = EditorGUILayout.IntField("XP Reward", xpReward);
        currencyReward = EditorGUILayout.IntField("Currency Reward", currencyReward);
        lootRarity = (LootManager.Rarity)EditorGUILayout.EnumPopup("Guaranteed Loot Rarity", lootRarity);
        lootCount = EditorGUILayout.IntField("Guaranteed Loot Count", lootCount);
    }
    
    private void DrawSpawnItems()
    {
        EditorGUILayout.Space(5);
        
        spawnScrollPosition = EditorGUILayout.BeginScrollView(spawnScrollPosition, GUILayout.Height(300));
        
        for (int i = 0; i < spawnTemplates.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            spawnTemplates[i].foldout = EditorGUILayout.Foldout(spawnTemplates[i].foldout, 
                $"[{i}] {spawnTemplates[i].itemName} ({spawnTemplates[i].category})", true);
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(18)))
            {
                spawnTemplates.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            if (spawnTemplates[i].foldout)
            {
                EditorGUI.indentLevel++;
                DrawSpawnTemplate(spawnTemplates[i]);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Spawn Item", GUILayout.Height(25)))
        {
            AddDefaultSpawnTemplate();
        }
        
        if (GUILayout.Button("Clear All", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear All Spawn Items?", 
                "Are you sure you want to remove all spawn items?", "Yes", "Cancel"))
            {
                spawnTemplates.Clear();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSpawnTemplate(SpawnItemTemplate template)
    {
        template.itemName = EditorGUILayout.TextField("Item Name", template.itemName);
        template.category = (ChallengeData.SpawnableCategory)EditorGUILayout.EnumPopup("Category", template.category);
        template.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", template.prefab, typeof(GameObject), false);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Count Range", GUILayout.Width(150));
        template.minCount = EditorGUILayout.IntField(template.minCount, GUILayout.Width(50));
        EditorGUILayout.LabelField("to", GUILayout.Width(20));
        template.maxCount = EditorGUILayout.IntField(template.maxCount, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        
        template.spawnLocation = (ChallengeData.SpawnLocationType)EditorGUILayout.EnumPopup("Spawn Location", template.spawnLocation);
        template.spawnRadius = EditorGUILayout.FloatField("Spawn Radius", template.spawnRadius);
        
        if (template.spawnLocation == ChallengeData.SpawnLocationType.AtCenter)
        {
            template.offset = EditorGUILayout.Vector3Field("Offset", template.offset);
        }
        
        template.requireNavMesh = EditorGUILayout.Toggle("Require NavMesh", template.requireNavMesh);
        template.randomRotation = EditorGUILayout.Toggle("Random Rotation", template.randomRotation);
        
        if (!template.randomRotation)
        {
            template.fixedRotation = EditorGUILayout.Vector3Field("Fixed Rotation", template.fixedRotation);
        }
        
        template.priority = EditorGUILayout.IntSlider("Priority", template.priority, 0, 10);
        template.required = EditorGUILayout.Toggle("Required", template.required);
        template.usePoolMode = EditorGUILayout.Toggle("Use Pool Mode", template.usePoolMode);
    }
    
    private void AddDefaultSpawnTemplate()
    {
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "New Spawn Item",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 15f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = true
        });
    }
    
    private void ShowPresetMenu()
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Enemy Pack (3-5 enemies)"), false, () => AddPreset_EnemyPack());
        menu.AddItem(new GUIContent("Boss Encounter"), false, () => AddPreset_BossEncounter());
        menu.AddItem(new GUIContent("Civilian Rescue"), false, () => AddPreset_CivilianRescue());
        menu.AddItem(new GUIContent("Supply Drop Defense"), false, () => AddPreset_SupplyDrop());
        menu.AddItem(new GUIContent("Control Point"), false, () => AddPreset_ControlPoint());
        menu.AddItem(new GUIContent("Mixed Combat Zone"), false, () => AddPreset_MixedCombat());
        
        menu.ShowAsContext();
    }
    
    private void AddPreset_EnemyPack()
    {
        spawnTemplates.Clear();
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Enemy Squad",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 15f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = true
        });
    }
    
    private void AddPreset_BossEncounter()
    {
        spawnTemplates.Clear();
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Boss Enemy",
            category = ChallengeData.SpawnableCategory.Boss,
            minCount = 1,
            maxCount = 1,
            spawnLocation = ChallengeData.SpawnLocationType.AtCenter,
            spawnRadius = 0f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 10,
            required = true,
            foldout = true
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Minion Enemies",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.RandomOnEdge,
            spawnRadius = 12f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = false
        });
    }
    
    private void AddPreset_CivilianRescue()
    {
        spawnTemplates.Clear();
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Civilians to Rescue",
            category = ChallengeData.SpawnableCategory.Civilian,
            minCount = 3,
            maxCount = 4,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 8f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 8,
            required = true,
            foldout = true
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Enemy Guards",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 5,
            maxCount = 8,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 15f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = false
        });
    }
    
    private void AddPreset_SupplyDrop()
    {
        spawnTemplates.Clear();
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Supply Drop Objective",
            category = ChallengeData.SpawnableCategory.Objective,
            minCount = 1,
            maxCount = 1,
            spawnLocation = ChallengeData.SpawnLocationType.AtCenter,
            spawnRadius = 0f,
            requireNavMesh = false,
            randomRotation = false,
            priority = 10,
            required = true,
            foldout = true
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Elite Guards",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 8,
            maxCount = 12,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 18f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = false
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Loot Crates",
            category = ChallengeData.SpawnableCategory.LootBox,
            minCount = 2,
            maxCount = 3,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 10f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 1,
            foldout = false
        });
    }
    
    private void AddPreset_ControlPoint()
    {
        spawnTemplates.Clear();
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Control Terminal",
            category = ChallengeData.SpawnableCategory.Objective,
            minCount = 1,
            maxCount = 1,
            spawnLocation = ChallengeData.SpawnLocationType.AtCenter,
            spawnRadius = 0f,
            requireNavMesh = false,
            randomRotation = false,
            priority = 10,
            required = true,
            foldout = true
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Defensive Positions",
            category = ChallengeData.SpawnableCategory.Cover,
            minCount = 4,
            maxCount = 6,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 8f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 7,
            foldout = false
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Defenders",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 10,
            maxCount = 15,
            spawnLocation = ChallengeData.SpawnLocationType.AroundPerimeter,
            spawnRadius = 20f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = false
        });
    }
    
    private void AddPreset_MixedCombat()
    {
        spawnTemplates.Clear();
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Main Enemies",
            category = ChallengeData.SpawnableCategory.Enemy,
            minCount = 6,
            maxCount = 8,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 15f,
            requireNavMesh = true,
            randomRotation = true,
            priority = 5,
            foldout = true
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Cover Objects",
            category = ChallengeData.SpawnableCategory.Cover,
            minCount = 3,
            maxCount = 5,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 12f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 2,
            foldout = false
        });
        
        spawnTemplates.Add(new SpawnItemTemplate
        {
            itemName = "Loot Boxes",
            category = ChallengeData.SpawnableCategory.LootBox,
            minCount = 2,
            maxCount = 3,
            spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius,
            spawnRadius = 10f,
            requireNavMesh = false,
            randomRotation = true,
            priority = 1,
            foldout = false
        });
    }
    
    private void CreateChallenge()
    {
        if (string.IsNullOrEmpty(challengeName))
        {
            EditorUtility.DisplayDialog("Error", "Challenge name cannot be empty!", "OK");
            return;
        }
        
        string folderPath = "Assets/Resources/Challenges";
        
        switch (frequency)
        {
            case ChallengeData.ChallengeFrequency.WorldEvent:
                folderPath += "/WorldEvents";
                break;
            case ChallengeData.ChallengeFrequency.Daily:
                folderPath += "/Daily";
                break;
            case ChallengeData.ChallengeFrequency.Weekly:
                folderPath += "/Weekly";
                break;
        }
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            CreateChallengesFolders();
        }
        
        string safeFileName = challengeName.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
        string assetPath = $"{folderPath}/{safeFileName}.asset";
        
        if (AssetDatabase.LoadAssetAtPath<ChallengeData>(assetPath) != null)
        {
            if (!EditorUtility.DisplayDialog("Asset Exists", 
                $"A challenge already exists at:\n{assetPath}\n\nOverwrite?", "Yes", "Cancel"))
            {
                return;
            }
        }
        
        ChallengeData challenge = ScriptableObject.CreateInstance<ChallengeData>();
        
        challenge.challengeName = challengeName;
        challenge.description = description;
        challenge.challengeType = challengeType;
        challenge.frequency = frequency;
        challenge.difficulty = difficulty;
        
        challenge.timeLimit = timeLimit;
        challenge.detectionRadius = detectionRadius;
        challenge.recommendedLevel = recommendedLevel;
        challenge.requiredPlayerLevel = requiredLevel;
        
        challenge.baseXPReward = xpReward;
        challenge.baseCurrencyReward = currencyReward;
        challenge.guaranteedLootRarity = lootRarity;
        challenge.guaranteedLootCount = lootCount;
        
        foreach (var template in spawnTemplates)
        {
            challenge.spawnItems.Add(new ChallengeData.SpawnableItem
            {
                itemName = template.itemName,
                category = template.category,
                prefab = template.prefab,
                minCount = template.minCount,
                maxCount = template.maxCount,
                spawnLocation = template.spawnLocation,
                spawnRadius = template.spawnRadius,
                offset = template.offset,
                requireNavMesh = template.requireNavMesh,
                randomRotation = template.randomRotation,
                fixedRotation = template.fixedRotation,
                priority = template.priority,
                required = template.required,
                usePoolMode = template.usePoolMode
            });
        }
        
        challenge.difficultyColor = challenge.GetDifficultyColor();
        
        AssetDatabase.CreateAsset(challenge, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Selection.activeObject = challenge;
        EditorGUIUtility.PingObject(challenge);
        
        Debug.Log($"✓ Challenge created: {assetPath}");
        EditorUtility.DisplayDialog("Success", 
            $"Challenge created successfully!\n\n{assetPath}\n\nSpawn Items: {spawnTemplates.Count}", "OK");
    }
    
    private void CreateChallengesFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges"))
            AssetDatabase.CreateFolder("Assets/Resources", "Challenges");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges/WorldEvents"))
            AssetDatabase.CreateFolder("Assets/Resources/Challenges", "WorldEvents");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges/Daily"))
            AssetDatabase.CreateFolder("Assets/Resources/Challenges", "Daily");
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Challenges/Weekly"))
            AssetDatabase.CreateFolder("Assets/Resources/Challenges", "Weekly");
    }
    
    [System.Serializable]
    private class SpawnItemTemplate
    {
        public string itemName = "Spawn Item";
        public ChallengeData.SpawnableCategory category = ChallengeData.SpawnableCategory.Enemy;
        public GameObject prefab;
        public int minCount = 1;
        public int maxCount = 1;
        public ChallengeData.SpawnLocationType spawnLocation = ChallengeData.SpawnLocationType.RandomInRadius;
        public float spawnRadius = 10f;
        public Vector3 offset = Vector3.zero;
        public bool requireNavMesh = true;
        public bool randomRotation = true;
        public Vector3 fixedRotation = Vector3.zero;
        public int priority = 5;
        public bool required = false;
        public bool usePoolMode = false;
        public bool foldout = true;
    }
}
