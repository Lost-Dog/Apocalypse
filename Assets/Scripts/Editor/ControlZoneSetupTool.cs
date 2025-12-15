using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ControlZoneSetupTool : EditorWindow
{
    private int numberOfZones = 3;
    private float captureRadius = 15f;
    private float captureTime = 10f;
    private int enemiesPerZone = 5;
    private GameObject enemyPrefab;
    private bool createVisualIndicators = true;
    private Material zoneMaterial;
    
    [MenuItem("Division Game/Challenge System/Setup Control Zones")]
    public static void ShowWindow()
    {
        GetWindow<ControlZoneSetupTool>("Control Zone Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Control Zone Setup Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool helps you create control zones for the Zone Control challenge.\n\n" +
            "Steps:\n" +
            "1. Configure settings below\n" +
            "2. Click 'Create Control Zones'\n" +
            "3. Position zones in your scene\n" +
            "4. Add enemy spawn points to each zone",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Zone Settings", EditorStyles.boldLabel);
        numberOfZones = EditorGUILayout.IntSlider("Number of Zones", numberOfZones, 1, 10);
        captureRadius = EditorGUILayout.Slider("Capture Radius", captureRadius, 5f, 50f);
        captureTime = EditorGUILayout.Slider("Capture Time (seconds)", captureTime, 5f, 30f);
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Enemy Settings", EditorStyles.boldLabel);
        enemiesPerZone = EditorGUILayout.IntSlider("Enemies Per Zone", enemiesPerZone, 0, 20);
        enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        createVisualIndicators = EditorGUILayout.Toggle("Create Visual Indicators", createVisualIndicators);
        zoneMaterial = (Material)EditorGUILayout.ObjectField("Zone Material", zoneMaterial, typeof(Material), false);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create Control Zones", GUILayout.Height(40)))
        {
            CreateControlZones();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add Spawn Points to Selected Zone", GUILayout.Height(30)))
        {
            AddSpawnPointsToSelected();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Selected as Control Zone", GUILayout.Height(30)))
        {
            SetupSelectedAsControlZone();
        }
    }

    private void CreateControlZones()
    {
        GameObject controlZonesParent = GameObject.Find("ControlZones");
        if (controlZonesParent == null)
        {
            controlZonesParent = new GameObject("ControlZones");
            Undo.RegisterCreatedObjectUndo(controlZonesParent, "Create Control Zones Parent");
        }
        
        for (int i = 0; i < numberOfZones; i++)
        {
            GameObject zoneObj = new GameObject($"ControlZone_{i + 1:00}");
            Undo.RegisterCreatedObjectUndo(zoneObj, "Create Control Zone");
            
            zoneObj.transform.parent = controlZonesParent.transform;
            zoneObj.transform.position = Vector3.zero + Vector3.right * i * (captureRadius * 3f);
            
            ControlZone zone = Undo.AddComponent<ControlZone>(zoneObj);
            zone.zoneName = $"Control Point {i + 1}";
            zone.captureRadius = captureRadius;
            zone.captureTime = captureTime;
            zone.enemyCount = enemiesPerZone;
            zone.enemyPrefab = enemyPrefab;
            
            SphereCollider trigger = Undo.AddComponent<SphereCollider>(zoneObj);
            trigger.isTrigger = true;
            trigger.radius = captureRadius;
            
            if (createVisualIndicators)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                indicator.name = "ZoneIndicator";
                indicator.transform.parent = zoneObj.transform;
                indicator.transform.localPosition = Vector3.zero;
                indicator.transform.localScale = new Vector3(captureRadius * 2f, 0.1f, captureRadius * 2f);
                
                DestroyImmediate(indicator.GetComponent<Collider>());
                
                MeshRenderer renderer = indicator.GetComponent<MeshRenderer>();
                zone.zoneRenderer = renderer;
                zone.visualIndicator = indicator;
                
                if (zoneMaterial != null)
                {
                    renderer.sharedMaterial = zoneMaterial;
                }
            }
            
            CreateSpawnPoints(zoneObj, zone);
        }
        
        Debug.Log($"Created {numberOfZones} control zones");
        EditorUtility.DisplayDialog("Success", $"Created {numberOfZones} control zones!\n\nPosition them in your scene and configure enemy spawn points.", "OK");
    }

    private void CreateSpawnPoints(GameObject zoneObj, ControlZone zone)
    {
        if (enemiesPerZone <= 0)
            return;
        
        GameObject spawnParent = new GameObject("EnemySpawnPoints");
        spawnParent.transform.parent = zoneObj.transform;
        spawnParent.transform.localPosition = Vector3.zero;
        
        List<Transform> spawnPoints = new List<Transform>();
        
        float angleStep = 360f / enemiesPerZone;
        float spawnDistance = captureRadius * 0.7f;
        
        for (int i = 0; i < enemiesPerZone; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i + 1:00}");
            spawnPoint.transform.parent = spawnParent.transform;
            
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * spawnDistance, 0f, Mathf.Sin(angle) * spawnDistance);
            spawnPoint.transform.localPosition = offset;
            spawnPoint.transform.LookAt(zoneObj.transform);
            
            spawnPoints.Add(spawnPoint.transform);
        }
        
        zone.enemySpawnPoints = spawnPoints.ToArray();
    }

    private void AddSpawnPointsToSelected()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Control Zone GameObject first!", "OK");
            return;
        }
        
        ControlZone zone = Selection.activeGameObject.GetComponent<ControlZone>();
        if (zone == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected GameObject doesn't have a ControlZone component!", "OK");
            return;
        }
        
        CreateSpawnPoints(Selection.activeGameObject, zone);
        Debug.Log($"Added {enemiesPerZone} spawn points to {Selection.activeGameObject.name}");
    }

    private void SetupSelectedAsControlZone()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject first!", "OK");
            return;
        }
        
        GameObject obj = Selection.activeGameObject;
        
        ControlZone zone = obj.GetComponent<ControlZone>();
        if (zone == null)
        {
            zone = Undo.AddComponent<ControlZone>(obj);
        }
        
        zone.zoneName = obj.name;
        zone.captureRadius = captureRadius;
        zone.captureTime = captureTime;
        zone.enemyCount = enemiesPerZone;
        zone.enemyPrefab = enemyPrefab;
        
        SphereCollider trigger = obj.GetComponent<SphereCollider>();
        if (trigger == null)
        {
            trigger = Undo.AddComponent<SphereCollider>(obj);
        }
        trigger.isTrigger = true;
        trigger.radius = captureRadius;
        
        Debug.Log($"Setup {obj.name} as Control Zone");
        EditorUtility.DisplayDialog("Success", $"{obj.name} is now a Control Zone!\n\nAdd enemy spawn points if needed.", "OK");
    }
}
