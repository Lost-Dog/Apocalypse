using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MissionZoneConfigurator : EditorWindow
{
    private List<ChallengeData> allChallenges = new List<ChallengeData>();
    private Dictionary<ChallengeData, bool> selectedChallenges = new Dictionary<ChallengeData, bool>();
    private Dictionary<ChallengeData, MissionZone> existingZones = new Dictionary<ChallengeData, MissionZone>();
    
    private Vector2 scrollPosition;
    private bool showWorldEvents = true;
    private bool showDaily = true;
    private bool showWeekly = true;
    
    private GameObject defaultEnemyPrefab;
    private GameObject defaultCivilianPrefab;
    private GameObject defaultBossPrefab;
    private GameObject defaultLootBoxPrefab;
    
    private bool autoLinkZonesToChallenges = true;
    
    [MenuItem("Division Game/Challenge System/Configure Existing Missions")]
    public static void ShowWindow()
    {
        MissionZoneConfigurator window = GetWindow<MissionZoneConfigurator>("Mission Configurator");
        window.minSize = new Vector2(500, 700);
        window.LoadAllChallenges();
    }

    private void LoadAllChallenges()
    {
        allChallenges.Clear();
        selectedChallenges.Clear();
        existingZones.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData", new[] { "Assets/Resources/Challenges" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChallengeData challenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
            
            if (challenge != null)
            {
                allChallenges.Add(challenge);
                selectedChallenges[challenge] = false;
            }
        }
        
        allChallenges = allChallenges.OrderBy(c => c.frequency).ThenBy(c => c.challengeName).ToList();
        
        FindExistingMissionZones();
        
        Debug.Log($"Loaded {allChallenges.Count} challenges");
    }

    private void FindExistingMissionZones()
    {
        MissionZone[] zones = FindObjectsByType<MissionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (MissionZone zone in zones)
        {
            if (zone.linkedChallengeData != null)
            {
                existingZones[zone.linkedChallengeData] = zone;
            }
        }
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawDefaultPrefabs();
        DrawChallengeList();
        DrawActionButtons();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("Configure Existing Missions", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Configure your existing ChallengeData assets with Mission Zones.\n\n" +
            "1. Set default prefabs below\n" +
            "2. Select challenges to configure\n" +
            "3. Click 'Create Mission Zones for Selected'\n" +
            "4. Position zones in your scene",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Refresh Challenge List", GUILayout.Height(25)))
        {
            LoadAllChallenges();
        }
        
        EditorGUILayout.Space(10);
    }

    private void DrawDefaultPrefabs()
    {
        EditorGUILayout.LabelField("Default Prefabs", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        defaultEnemyPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Enemy", 
            defaultEnemyPrefab, 
            typeof(GameObject), 
            false
        );
        
        defaultCivilianPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Civilian", 
            defaultCivilianPrefab, 
            typeof(GameObject), 
            false
        );
        
        defaultBossPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Boss", 
            defaultBossPrefab, 
            typeof(GameObject), 
            false
        );
        
        defaultLootBoxPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Loot Box", 
            defaultLootBoxPrefab, 
            typeof(GameObject), 
            false
        );
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        autoLinkZonesToChallenges = EditorGUILayout.Toggle("Auto-Link Zones to Challenges", autoLinkZonesToChallenges);
        EditorGUILayout.Space(10);
    }

    private void DrawChallengeList()
    {
        EditorGUILayout.LabelField("Challenges", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            foreach (var key in selectedChallenges.Keys.ToList())
            {
                selectedChallenges[key] = true;
            }
        }
        if (GUILayout.Button("Select None"))
        {
            foreach (var key in selectedChallenges.Keys.ToList())
            {
                selectedChallenges[key] = false;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        showWorldEvents = EditorGUILayout.ToggleLeft("World Events", showWorldEvents, GUILayout.Width(120));
        showDaily = EditorGUILayout.ToggleLeft("Daily", showDaily, GUILayout.Width(80));
        showWeekly = EditorGUILayout.ToggleLeft("Weekly", showWeekly, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(350));
        
        ChallengeData.ChallengeFrequency currentFrequency = ChallengeData.ChallengeFrequency.WorldEvent;
        bool showCurrentSection = true;
        
        foreach (ChallengeData challenge in allChallenges)
        {
            if (challenge.frequency != currentFrequency)
            {
                currentFrequency = challenge.frequency;
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(currentFrequency.ToString(), EditorStyles.boldLabel);
                
                showCurrentSection = (currentFrequency == ChallengeData.ChallengeFrequency.WorldEvent && showWorldEvents) ||
                                   (currentFrequency == ChallengeData.ChallengeFrequency.Daily && showDaily) ||
                                   (currentFrequency == ChallengeData.ChallengeFrequency.Weekly && showWeekly);
            }
            
            if (!showCurrentSection)
                continue;
            
            DrawChallengeRow(challenge);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawChallengeRow(ChallengeData challenge)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        selectedChallenges[challenge] = EditorGUILayout.Toggle(selectedChallenges[challenge], GUILayout.Width(20));
        
        Color typeColor = GetChallengeTypeColor(challenge.challengeType);
        GUI.backgroundColor = typeColor;
        GUILayout.Label(challenge.challengeType.ToString(), EditorStyles.miniButton, GUILayout.Width(120));
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.LabelField(challenge.challengeName, GUILayout.Width(200));
        
        Color difficultyColor = challenge.GetDifficultyColor();
        GUI.backgroundColor = difficultyColor;
        GUILayout.Label(challenge.difficulty.ToString(), EditorStyles.miniButton, GUILayout.Width(80));
        GUI.backgroundColor = Color.white;
        
        if (existingZones.ContainsKey(challenge))
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Zone Exists ✓", GUILayout.Width(100)))
            {
                Selection.activeGameObject = existingZones[challenge].gameObject;
                EditorGUIUtility.PingObject(existingZones[challenge].gameObject);
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = new Color(1f, 0.5f, 0f);
            GUILayout.Label("No Zone", EditorStyles.miniButton, GUILayout.Width(100));
            GUI.backgroundColor = Color.white;
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.Space(10);
        
        int selectedCount = selectedChallenges.Values.Count(v => v);
        
        EditorGUILayout.LabelField($"Selected: {selectedCount} challenges", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(5);
        
        GUI.enabled = selectedCount > 0;
        
        GUI.backgroundColor = new Color(0.3f, 1f, 0.5f);
        if (GUILayout.Button($"Create Mission Zones for Selected ({selectedCount})", GUILayout.Height(40)))
        {
            CreateMissionZonesForSelected();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Create All Missing Zones", GUILayout.Height(30)))
        {
            CreateAllMissingZones();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Select All\nSupply Drops"))
        {
            SelectByType(ChallengeData.ChallengeType.SupplyDrop);
        }
        
        if (GUILayout.Button("Select All\nCivilian Rescue"))
        {
            SelectByType(ChallengeData.ChallengeType.CivilianRescue);
        }
        
        if (GUILayout.Button("Select All\nBoss Fights"))
        {
            SelectByType(ChallengeData.ChallengeType.BossEncounter);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Select All\nControl Points"))
        {
            SelectByType(ChallengeData.ChallengeType.ControlPoint);
        }
        
        if (GUILayout.Button("Select All\nHostage Rescue"))
        {
            SelectByType(ChallengeData.ChallengeType.HostageRescue);
        }
        
        if (GUILayout.Button("Select All\nExtraction"))
        {
            SelectByType(ChallengeData.ChallengeType.ExtractionDefense);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void CreateMissionZonesForSelected()
    {
        List<ChallengeData> toCreate = allChallenges.Where(c => selectedChallenges[c] && !existingZones.ContainsKey(c)).ToList();
        
        if (toCreate.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "All selected challenges already have zones!", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog(
            "Create Mission Zones",
            $"Create Mission Zones for {toCreate.Count} challenges?\n\nZones will be created at world origin.\nYou'll need to position them in your scene.",
            "Create",
            "Cancel"))
        {
            return;
        }
        
        GameObject zoneParent = GameObject.Find("MissionZones");
        if (zoneParent == null)
        {
            zoneParent = new GameObject("MissionZones");
            Undo.RegisterCreatedObjectUndo(zoneParent, "Create Mission Zones Parent");
        }
        
        int created = 0;
        
        foreach (ChallengeData challenge in toCreate)
        {
            CreateMissionZoneForChallenge(challenge, zoneParent);
            created++;
        }
        
        LoadAllChallenges();
        
        EditorUtility.DisplayDialog(
            "Success",
            $"Created {created} Mission Zones!\n\nFind them in Hierarchy under 'MissionZones'\n\nNext steps:\n1. Position zones in your scene\n2. Assign prefabs to spawn points\n3. Test in Play Mode",
            "OK"
        );
        
        Selection.activeGameObject = zoneParent;
        EditorGUIUtility.PingObject(zoneParent);
    }

    private void CreateAllMissingZones()
    {
        List<ChallengeData> toCreate = allChallenges.Where(c => !existingZones.ContainsKey(c)).ToList();
        
        if (toCreate.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "All challenges already have zones!", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog(
            "Create All Missing Zones",
            $"Create Mission Zones for {toCreate.Count} challenges?\n\nThis will create zones for ALL challenges that don't have one yet.",
            "Create All",
            "Cancel"))
        {
            return;
        }
        
        GameObject zoneParent = GameObject.Find("MissionZones");
        if (zoneParent == null)
        {
            zoneParent = new GameObject("MissionZones");
            Undo.RegisterCreatedObjectUndo(zoneParent, "Create Mission Zones Parent");
        }
        
        int created = 0;
        
        foreach (ChallengeData challenge in toCreate)
        {
            CreateMissionZoneForChallenge(challenge, zoneParent);
            created++;
        }
        
        LoadAllChallenges();
        
        EditorUtility.DisplayDialog(
            "Success",
            $"Created {created} Mission Zones!\n\nAll challenges now have zones.",
            "OK"
        );
    }

    private void CreateMissionZoneForChallenge(ChallengeData challenge, GameObject parent)
    {
        GameObject zoneObj = new GameObject(challenge.challengeName.Replace(" ", "_"));
        Undo.RegisterCreatedObjectUndo(zoneObj, "Create Mission Zone");
        zoneObj.transform.parent = parent.transform;
        zoneObj.transform.position = Vector3.zero;
        zoneObj.SetActive(false);
        
        MissionZone zone = Undo.AddComponent<MissionZone>(zoneObj);
        zone.zoneName = challenge.challengeName;
        zone.missionType = challenge.challengeType;
        zone.zoneRadius = challenge.detectionRadius;
        zone.zoneColor = GetChallengeTypeColor(challenge.challengeType);
        
        if (autoLinkZonesToChallenges)
        {
            zone.linkedChallengeData = challenge;
        }
        
        CreateZoneMarker(zoneObj, zone.zoneRadius, zone.zoneColor);
        CreateSpawnPointsBasedOnType(zoneObj, zone, challenge);
        
        EditorUtility.SetDirty(zone);
    }

    private void CreateZoneMarker(GameObject zoneObj, float radius, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "ZoneMarker";
        marker.transform.parent = zoneObj.transform;
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = new Vector3(radius * 2f, 0.1f, radius * 2f);
        
        DestroyImmediate(marker.GetComponent<Collider>());
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(color.r, color.g, color.b, 0.3f);
        marker.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private void CreateSpawnPointsBasedOnType(GameObject zoneObj, MissionZone zone, ChallengeData challenge)
    {
        GameObject spawnParent = new GameObject("SpawnPoints");
        spawnParent.transform.parent = zoneObj.transform;
        spawnParent.transform.localPosition = Vector3.zero;
        
        int enemyCount = 0;
        int civilianCount = 0;
        bool hasBoss = false;
        int lootCount = 0;
        
        switch (challenge.challengeType)
        {
            case ChallengeData.ChallengeType.SupplyDrop:
                enemyCount = GetEnemyCountForDifficulty(challenge.difficulty);
                lootCount = challenge.difficulty == ChallengeData.ChallengeDifficulty.Easy ? 1 : 2;
                break;
                
            case ChallengeData.ChallengeType.CivilianRescue:
                enemyCount = GetEnemyCountForDifficulty(challenge.difficulty);
                civilianCount = challenge.difficulty == ChallengeData.ChallengeDifficulty.Easy ? 3 : 5;
                break;
                
            case ChallengeData.ChallengeType.ControlPoint:
                enemyCount = GetEnemyCountForDifficulty(challenge.difficulty) + 2;
                break;
                
            case ChallengeData.ChallengeType.HostageRescue:
                enemyCount = GetEnemyCountForDifficulty(challenge.difficulty) - 1;
                civilianCount = challenge.difficulty == ChallengeData.ChallengeDifficulty.Easy ? 2 : 3;
                break;
                
            case ChallengeData.ChallengeType.ExtractionDefense:
                enemyCount = GetEnemyCountForDifficulty(challenge.difficulty) * 2;
                break;
                
            case ChallengeData.ChallengeType.BossEncounter:
                hasBoss = true;
                enemyCount = GetEnemyCountForDifficulty(challenge.difficulty);
                lootCount = 1;
                break;
                
            case ChallengeData.ChallengeType.RivalAgent:
                hasBoss = true;
                enemyCount = challenge.difficulty == ChallengeData.ChallengeDifficulty.Extreme ? 3 : 1;
                break;
        }
        
        List<Vector3> positions = GenerateCirclePositions(enemyCount + civilianCount + lootCount, zone.zoneRadius * 0.7f);
        int index = 0;
        
        if (defaultEnemyPrefab != null)
        {
            for (int i = 0; i < enemyCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(
                    spawnParent, 
                    zone, 
                    $"Enemy_{i + 1:00}", 
                    positions[index], 
                    ChallengeData.SpawnableCategory.Enemy, 
                    defaultEnemyPrefab
                );
                index++;
            }
        }
        
        if (hasBoss && defaultBossPrefab != null)
        {
            CreateSpawnPoint(
                spawnParent, 
                zone, 
                "Boss", 
                Vector3.zero, 
                ChallengeData.SpawnableCategory.Boss, 
                defaultBossPrefab
            );
        }
        
        if (defaultCivilianPrefab != null)
        {
            for (int i = 0; i < civilianCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(
                    spawnParent, 
                    zone, 
                    $"Civilian_{i + 1:00}", 
                    positions[index], 
                    ChallengeData.SpawnableCategory.Civilian, 
                    defaultCivilianPrefab
                );
                index++;
            }
        }
        
        if (defaultLootBoxPrefab != null)
        {
            for (int i = 0; i < lootCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(
                    spawnParent, 
                    zone, 
                    $"LootBox_{i + 1:00}", 
                    positions[index], 
                    ChallengeData.SpawnableCategory.LootBox, 
                    defaultLootBoxPrefab
                );
                index++;
            }
        }
    }

    private void CreateSpawnPoint(GameObject parent, MissionZone zone, string name, Vector3 localPosition, ChallengeData.SpawnableCategory category, GameObject prefab)
    {
        GameObject spawnObj = new GameObject(name);
        spawnObj.transform.parent = parent.transform;
        spawnObj.transform.localPosition = localPosition;
        
        MissionZone.SpawnPoint spawnPoint = new MissionZone.SpawnPoint();
        spawnPoint.pointName = name;
        spawnPoint.transform = spawnObj.transform;
        spawnPoint.category = category;
        spawnPoint.prefabOverride = prefab;
        
        zone.spawnPoints.Add(spawnPoint);
    }

    private List<Vector3> GenerateCirclePositions(int count, float radius)
    {
        List<Vector3> positions = new List<Vector3>();
        
        if (count == 0)
            return positions;
        
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            positions.Add(pos);
        }
        
        return positions;
    }

    private int GetEnemyCountForDifficulty(ChallengeData.ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeData.ChallengeDifficulty.Easy: return 4;
            case ChallengeData.ChallengeDifficulty.Medium: return 6;
            case ChallengeData.ChallengeDifficulty.Hard: return 8;
            case ChallengeData.ChallengeDifficulty.Extreme: return 10;
            default: return 5;
        }
    }

    private void SelectByType(ChallengeData.ChallengeType type)
    {
        foreach (var challenge in allChallenges)
        {
            selectedChallenges[challenge] = (challenge.challengeType == type);
        }
    }

    private Color GetChallengeTypeColor(ChallengeData.ChallengeType type)
    {
        switch (type)
        {
            case ChallengeData.ChallengeType.SupplyDrop: return new Color(1f, 0.8f, 0.3f);
            case ChallengeData.ChallengeType.CivilianRescue: return new Color(0.3f, 1f, 0.5f);
            case ChallengeData.ChallengeType.ControlPoint: return new Color(0.5f, 0.7f, 1f);
            case ChallengeData.ChallengeType.HostageRescue: return new Color(1f, 0.5f, 0.3f);
            case ChallengeData.ChallengeType.ExtractionDefense: return new Color(1f, 0.3f, 0.3f);
            case ChallengeData.ChallengeType.BossEncounter: return new Color(1f, 0f, 0.5f);
            case ChallengeData.ChallengeType.RivalAgent: return new Color(0.8f, 0f, 0f);
            default: return Color.cyan;
        }
    }
}
