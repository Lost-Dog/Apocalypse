using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

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
        Type controlZoneType = ResolveControlZoneType();
        if (controlZoneType == null)
        {
            EditorUtility.DisplayDialog("Missing Type", "ControlZone type was not found. Recreate or migrate the zone runtime before using this setup tool.", "OK");
            return;
        }

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
            
            Component zone = Undo.AddComponent(zoneObj, controlZoneType);
            ApplyZoneDefaults(zone, $"Control Point {i + 1}");
            
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
                SetObjectReference(zone, "zoneRenderer", renderer);
                SetObjectReference(zone, "visualIndicator", indicator);
                
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

    private void CreateSpawnPoints(GameObject zoneObj, Component zone)
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
        
        SetTransformArray(zone, "enemySpawnPoints", spawnPoints);
    }

    private void AddSpawnPointsToSelected()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Control Zone GameObject first!", "OK");
            return;
        }
        
        Component zone = GetControlZoneComponent(Selection.activeGameObject);
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

        Component zone = GetControlZoneComponent(obj);
        if (zone == null)
        {
            Type controlZoneType = ResolveControlZoneType();
            if (controlZoneType == null)
            {
                EditorUtility.DisplayDialog("Missing Type", "ControlZone type was not found. Recreate or migrate the zone runtime before using this setup tool.", "OK");
                return;
            }

            zone = Undo.AddComponent(obj, controlZoneType);
        }

        ApplyZoneDefaults(zone, obj.name);
        
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

    private Component GetControlZoneComponent(GameObject obj)
    {
        if (obj == null) return null;
        Type controlZoneType = ResolveControlZoneType();
        return controlZoneType != null ? obj.GetComponent(controlZoneType) : null;
    }

    private static Type ResolveControlZoneType()
    {
        Type direct = Type.GetType("ControlZone, Assembly-CSharp");
        if (direct != null) return direct;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type found = assemblies[i].GetType("ControlZone");
            if (found != null) return found;
        }

        return null;
    }

    private void ApplyZoneDefaults(Component zone, string zoneName)
    {
        if (zone == null) return;

        SerializedObject so = new SerializedObject(zone);
        SetString(so, "zoneName", zoneName);
        SetFloat(so, "captureRadius", captureRadius);
        SetFloat(so, "captureTime", captureTime);
        SetInt(so, "enemyCount", enemiesPerZone);
        SetObject(so, "enemyPrefab", enemyPrefab);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(zone);
    }

    private static void SetObjectReference(Component zone, string propertyName, UnityEngine.Object value)
    {
        if (zone == null) return;
        SerializedObject so = new SerializedObject(zone);
        SetObject(so, propertyName, value);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetTransformArray(Component zone, string propertyName, List<Transform> values)
    {
        if (zone == null) return;
        SerializedObject so = new SerializedObject(zone);
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null && property.isArray)
        {
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(SerializedObject so, string propertyName, string value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null && property.propertyType == SerializedPropertyType.String)
        {
            property.stringValue = value;
        }
    }

    private static void SetFloat(SerializedObject so, string propertyName, float value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null && property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = value;
        }
    }

    private static void SetInt(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null && property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = value;
        }
    }

    private static void SetObject(SerializedObject so, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
        {
            property.objectReferenceValue = value;
        }
    }
}
