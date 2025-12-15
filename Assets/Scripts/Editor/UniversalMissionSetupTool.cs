using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class UniversalMissionSetupTool : EditorWindow
{
    private ChallengeData.ChallengeType selectedMissionType = ChallengeData.ChallengeType.SupplyDrop;
    private string missionName = "New Mission";
    private float zoneRadius = 30f;
    
    private GameObject enemyPrefab;
    private int enemyCount = 5;
    private GameObject civilianPrefab;
    private int civilianCount = 0;
    private GameObject bossPrefab;
    private bool includeBoss = false;
    private GameObject objectivePrefab;
    private int objectiveCount = 0;
    private GameObject lootBoxPrefab;
    private int lootBoxCount = 0;
    
    private bool createVisualMarker = true;
    private bool autoArrangeSpawns = true;
    private SpawnLayout spawnLayout = SpawnLayout.Circle;
    
    private enum SpawnLayout
    {
        Circle,
        Grid,
        Line,
        Random,
        Custom
    }
    
    private Vector2 scrollPosition;
    
    [MenuItem("Division Game/Challenge System/Universal Mission Setup")]
    public static void ShowWindow()
    {
        UniversalMissionSetupTool window = GetWindow<UniversalMissionSetupTool>("Mission Setup");
        window.minSize = new Vector2(400, 600);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Universal Mission Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Create any type of mission/challenge zone with custom spawn points.\n\n" +
            "Steps:\n" +
            "1. Select mission type\n" +
            "2. Configure spawn counts and prefabs\n" +
            "3. Choose spawn layout\n" +
            "4. Click Create Mission Zone\n" +
            "5. Position zone and customize spawn points",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        DrawMissionTypeSection();
        EditorGUILayout.Space(10);
        
        DrawSpawnConfigSection();
        EditorGUILayout.Space(10);
        
        DrawLayoutSection();
        EditorGUILayout.Space(10);
        
        DrawActionButtons();
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawMissionTypeSection()
    {
        EditorGUILayout.LabelField("Mission Type", EditorStyles.boldLabel);
        
        selectedMissionType = (ChallengeData.ChallengeType)EditorGUILayout.EnumPopup("Type", selectedMissionType);
        missionName = EditorGUILayout.TextField("Mission Name", missionName);
        zoneRadius = EditorGUILayout.Slider("Zone Radius", zoneRadius, 10f, 100f);
        
        EditorGUILayout.Space(5);
        ShowMissionTypeInfo();
    }

    private void ShowMissionTypeInfo()
    {
        string info = "";
        Color infoColor = Color.white;
        
        switch (selectedMissionType)
        {
            case ChallengeData.ChallengeType.SupplyDrop:
                info = "Enemies guarding loot. Kill enemies and collect supplies.";
                infoColor = new Color(1f, 0.8f, 0.3f);
                break;
            case ChallengeData.ChallengeType.CivilianRescue:
                info = "Save civilians from enemies. Protect them from harm.";
                infoColor = new Color(0.3f, 1f, 0.5f);
                break;
            case ChallengeData.ChallengeType.ControlPoint:
                info = "Capture and hold territory. Clear enemies and secure zone.";
                infoColor = new Color(0.5f, 0.7f, 1f);
                break;
            case ChallengeData.ChallengeType.HostageRescue:
                info = "Rescue hostages without casualties. Precision required.";
                infoColor = new Color(1f, 0.5f, 0.3f);
                break;
            case ChallengeData.ChallengeType.ExtractionDefense:
                info = "Defend extraction point against waves. Survive the assault.";
                infoColor = new Color(1f, 0.3f, 0.3f);
                break;
            case ChallengeData.ChallengeType.BossEncounter:
                info = "Elite enemy boss fight. High difficulty, high rewards.";
                infoColor = new Color(1f, 0f, 0.5f);
                break;
            case ChallengeData.ChallengeType.RivalAgent:
                info = "Rogue Division agent duel. Tactical 1v1 combat.";
                infoColor = new Color(0.8f, 0f, 0f);
                break;
        }
        
        GUI.backgroundColor = infoColor;
        EditorGUILayout.HelpBox(info, MessageType.None);
        GUI.backgroundColor = Color.white;
    }

    private void DrawSpawnConfigSection()
    {
        EditorGUILayout.LabelField("Spawn Configuration", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Enemies", EditorStyles.miniBoldLabel);
        enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
        enemyCount = EditorGUILayout.IntSlider("Enemy Count", enemyCount, 0, 30);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Boss", EditorStyles.miniBoldLabel);
        includeBoss = EditorGUILayout.Toggle("Include Boss", includeBoss);
        if (includeBoss)
        {
            EditorGUI.indentLevel++;
            bossPrefab = (GameObject)EditorGUILayout.ObjectField("Boss Prefab", bossPrefab, typeof(GameObject), false);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Civilians", EditorStyles.miniBoldLabel);
        civilianPrefab = (GameObject)EditorGUILayout.ObjectField("Civilian Prefab", civilianPrefab, typeof(GameObject), false);
        civilianCount = EditorGUILayout.IntSlider("Civilian Count", civilianCount, 0, 20);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Objectives & Loot", EditorStyles.miniBoldLabel);
        objectivePrefab = (GameObject)EditorGUILayout.ObjectField("Objective Prefab", objectivePrefab, typeof(GameObject), false);
        objectiveCount = EditorGUILayout.IntSlider("Objective Count", objectiveCount, 0, 10);
        
        lootBoxPrefab = (GameObject)EditorGUILayout.ObjectField("Loot Box Prefab", lootBoxPrefab, typeof(GameObject), false);
        lootBoxCount = EditorGUILayout.IntSlider("Loot Box Count", lootBoxCount, 0, 10);
    }

    private void DrawLayoutSection()
    {
        EditorGUILayout.LabelField("Spawn Layout", EditorStyles.boldLabel);
        
        autoArrangeSpawns = EditorGUILayout.Toggle("Auto Arrange Spawns", autoArrangeSpawns);
        
        if (autoArrangeSpawns)
        {
            EditorGUI.indentLevel++;
            spawnLayout = (SpawnLayout)EditorGUILayout.EnumPopup("Layout Type", spawnLayout);
            EditorGUI.indentLevel--;
            
            ShowLayoutPreview();
        }
        
        EditorGUILayout.Space(5);
        createVisualMarker = EditorGUILayout.Toggle("Create Visual Marker", createVisualMarker);
    }

    private void ShowLayoutPreview()
    {
        string preview = "";
        
        switch (spawnLayout)
        {
            case SpawnLayout.Circle:
                preview = "Spawns arranged in circle around zone center";
                break;
            case SpawnLayout.Grid:
                preview = "Spawns arranged in grid pattern";
                break;
            case SpawnLayout.Line:
                preview = "Spawns arranged in line formation";
                break;
            case SpawnLayout.Random:
                preview = "Spawns placed randomly within zone";
                break;
        }
        
        if (!string.IsNullOrEmpty(preview))
        {
            EditorGUILayout.HelpBox(preview, MessageType.None);
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.3f, 1f, 0.5f);
        if (GUILayout.Button("Create Mission Zone", GUILayout.Height(40)))
        {
            CreateMissionZone();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Add Spawn Points to Selected Zone", GUILayout.Height(30)))
        {
            AddSpawnPointsToSelectedZone();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Setup Selected GameObject as Mission Zone", GUILayout.Height(30)))
        {
            SetupSelectedAsMissionZone();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Supply Drop\n(Easy)"))
        {
            ApplySupplyDropPreset(5);
        }
        
        if (GUILayout.Button("Civilian Rescue\n(Medium)"))
        {
            ApplyCivilianRescuePreset();
        }
        
        if (GUILayout.Button("Boss Fight\n(Hard)"))
        {
            ApplyBossFightPreset();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void CreateMissionZone()
    {
        GameObject zoneParent = GameObject.Find("MissionZones");
        if (zoneParent == null)
        {
            zoneParent = new GameObject("MissionZones");
            Undo.RegisterCreatedObjectUndo(zoneParent, "Create Mission Zones Parent");
        }
        
        GameObject zoneObj = new GameObject(missionName);
        Undo.RegisterCreatedObjectUndo(zoneObj, "Create Mission Zone");
        zoneObj.transform.parent = zoneParent.transform;
        zoneObj.transform.position = Vector3.zero;
        zoneObj.SetActive(false);
        
        MissionZone zone = Undo.AddComponent<MissionZone>(zoneObj);
        zone.zoneName = missionName;
        zone.missionType = selectedMissionType;
        zone.zoneRadius = zoneRadius;
        zone.zoneColor = GetMissionTypeColor(selectedMissionType);
        
        if (createVisualMarker)
        {
            CreateZoneMarker(zoneObj);
        }
        
        if (autoArrangeSpawns)
        {
            CreateSpawnPoints(zoneObj, zone);
        }
        
        Selection.activeGameObject = zoneObj;
        EditorGUIUtility.PingObject(zoneObj);
        
        Debug.Log($"Created mission zone: {missionName} with {zone.spawnPoints.Count} spawn points");
        EditorUtility.DisplayDialog("Success", $"Mission zone '{missionName}' created!\n\nSpawn Points: {zone.spawnPoints.Count}\nType: {selectedMissionType}\n\nNote: Zone starts INACTIVE and will be activated by ChallengeManager.", "OK");
    }

    private void CreateZoneMarker(GameObject zoneObj)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "ZoneMarker";
        marker.transform.parent = zoneObj.transform;
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = new Vector3(zoneRadius * 2f, 0.1f, zoneRadius * 2f);
        
        DestroyImmediate(marker.GetComponent<Collider>());
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(GetMissionTypeColor(selectedMissionType).r, GetMissionTypeColor(selectedMissionType).g, GetMissionTypeColor(selectedMissionType).b, 0.3f);
        marker.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private void CreateSpawnPoints(GameObject zoneObj, MissionZone zone)
    {
        GameObject spawnParent = new GameObject("SpawnPoints");
        spawnParent.transform.parent = zoneObj.transform;
        spawnParent.transform.localPosition = Vector3.zero;
        
        List<Vector3> positions = GenerateSpawnPositions();
        int index = 0;
        
        if (enemyCount > 0 && enemyPrefab != null)
        {
            for (int i = 0; i < enemyCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(spawnParent, zone, $"Enemy_{i + 1:00}", positions[index], ChallengeData.SpawnableCategory.Enemy, enemyPrefab);
                index++;
            }
        }
        
        if (includeBoss && bossPrefab != null && index < positions.Count)
        {
            CreateSpawnPoint(spawnParent, zone, "Boss", zoneObj.transform.position, ChallengeData.SpawnableCategory.Boss, bossPrefab);
        }
        
        if (civilianCount > 0 && civilianPrefab != null)
        {
            for (int i = 0; i < civilianCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(spawnParent, zone, $"Civilian_{i + 1:00}", positions[index], ChallengeData.SpawnableCategory.Civilian, civilianPrefab);
                index++;
            }
        }
        
        if (objectiveCount > 0 && objectivePrefab != null)
        {
            for (int i = 0; i < objectiveCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(spawnParent, zone, $"Objective_{i + 1:00}", positions[index], ChallengeData.SpawnableCategory.Objective, objectivePrefab);
                index++;
            }
        }
        
        if (lootBoxCount > 0 && lootBoxPrefab != null)
        {
            for (int i = 0; i < lootBoxCount && index < positions.Count; i++)
            {
                CreateSpawnPoint(spawnParent, zone, $"LootBox_{i + 1:00}", positions[index], ChallengeData.SpawnableCategory.LootBox, lootBoxPrefab);
                index++;
            }
        }
    }

    private void CreateSpawnPoint(GameObject parent, MissionZone zone, string name, Vector3 position, ChallengeData.SpawnableCategory category, GameObject prefab)
    {
        GameObject spawnObj = new GameObject(name);
        spawnObj.transform.parent = parent.transform;
        spawnObj.transform.position = position;
        
        MissionZone.SpawnPoint spawnPoint = new MissionZone.SpawnPoint();
        spawnPoint.pointName = name;
        spawnPoint.transform = spawnObj.transform;
        spawnPoint.category = category;
        spawnPoint.prefabOverride = prefab;
        
        zone.spawnPoints.Add(spawnPoint);
    }

    private List<Vector3> GenerateSpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        int totalCount = enemyCount + civilianCount + objectiveCount + lootBoxCount;
        
        switch (spawnLayout)
        {
            case SpawnLayout.Circle:
                float angleStep = 360f / totalCount;
                for (int i = 0; i < totalCount; i++)
                {
                    float angle = angleStep * i * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * zoneRadius * 0.7f, 0f, Mathf.Sin(angle) * zoneRadius * 0.7f);
                    positions.Add(pos);
                }
                break;
                
            case SpawnLayout.Grid:
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
                float spacing = (zoneRadius * 1.5f) / gridSize;
                for (int i = 0; i < totalCount; i++)
                {
                    int x = i % gridSize;
                    int z = i / gridSize;
                    Vector3 pos = new Vector3((x - gridSize / 2f) * spacing, 0f, (z - gridSize / 2f) * spacing);
                    positions.Add(pos);
                }
                break;
                
            case SpawnLayout.Line:
                float lineSpacing = (zoneRadius * 1.5f) / totalCount;
                for (int i = 0; i < totalCount; i++)
                {
                    Vector3 pos = new Vector3((i - totalCount / 2f) * lineSpacing, 0f, 0f);
                    positions.Add(pos);
                }
                break;
                
            case SpawnLayout.Random:
                for (int i = 0; i < totalCount; i++)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * zoneRadius * 0.8f;
                    Vector3 pos = new Vector3(randomCircle.x, 0f, randomCircle.y);
                    positions.Add(pos);
                }
                break;
        }
        
        return positions;
    }

    private void AddSpawnPointsToSelectedZone()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Mission Zone first!", "OK");
            return;
        }
        
        MissionZone zone = Selection.activeGameObject.GetComponent<MissionZone>();
        if (zone == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object doesn't have a MissionZone component!", "OK");
            return;
        }
        
        CreateSpawnPoints(Selection.activeGameObject, zone);
        EditorUtility.SetDirty(zone);
        Debug.Log($"Added spawn points to {Selection.activeGameObject.name}");
    }

    private void SetupSelectedAsMissionZone()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject first!", "OK");
            return;
        }
        
        GameObject obj = Selection.activeGameObject;
        
        MissionZone zone = obj.GetComponent<MissionZone>();
        if (zone == null)
        {
            zone = Undo.AddComponent<MissionZone>(obj);
        }
        
        zone.zoneName = missionName;
        zone.missionType = selectedMissionType;
        zone.zoneRadius = zoneRadius;
        zone.zoneColor = GetMissionTypeColor(selectedMissionType);
        
        EditorUtility.SetDirty(zone);
        Debug.Log($"Setup {obj.name} as Mission Zone");
        EditorUtility.DisplayDialog("Success", $"{obj.name} is now a Mission Zone!", "OK");
    }

    private void ApplySupplyDropPreset(int difficulty)
    {
        selectedMissionType = ChallengeData.ChallengeType.SupplyDrop;
        missionName = "Supply Drop";
        enemyCount = difficulty;
        civilianCount = 0;
        includeBoss = false;
        objectiveCount = 0;
        lootBoxCount = 1;
        zoneRadius = 25f;
        spawnLayout = SpawnLayout.Circle;
    }

    private void ApplyCivilianRescuePreset()
    {
        selectedMissionType = ChallengeData.ChallengeType.CivilianRescue;
        missionName = "Civilian Rescue";
        enemyCount = 6;
        civilianCount = 4;
        includeBoss = false;
        objectiveCount = 0;
        lootBoxCount = 0;
        zoneRadius = 30f;
        spawnLayout = SpawnLayout.Random;
    }

    private void ApplyBossFightPreset()
    {
        selectedMissionType = ChallengeData.ChallengeType.BossEncounter;
        missionName = "Boss Fight";
        enemyCount = 8;
        civilianCount = 0;
        includeBoss = true;
        objectiveCount = 0;
        lootBoxCount = 1;
        zoneRadius = 35f;
        spawnLayout = SpawnLayout.Circle;
    }

    private Color GetMissionTypeColor(ChallengeData.ChallengeType type)
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
