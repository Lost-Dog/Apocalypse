using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BuildingSafeZoneBatchSetup : EditorWindow
{
    private bool addSafeZone = true;
    private bool addBuildingSafeZone = true;
    private bool expandSafeZone = true;
    private Vector3 safeZoneExpansion = new Vector3(2f, 2f, 2f);
    
    private float healthRestoreRate = 10f;
    private float staminaRestoreRate = 20f;
    private bool restoreHealth = true;
    private bool restoreStamina = true;
    private bool cureInfection = true;
    private bool normalizeTemperature = true;
    
    private List<GameObject> selectedBuildings = new List<GameObject>();
    
    [MenuItem("Division Game/Setup/Building Safe Zone Batch Setup")]
    public static void ShowWindow()
    {
        GetWindow<BuildingSafeZoneBatchSetup>("Building Safe Zone Batch Setup");
    }
    
    private void OnEnable()
    {
        RefreshSelection();
    }
    
    private void OnSelectionChange()
    {
        RefreshSelection();
        Repaint();
    }
    
    private void RefreshSelection()
    {
        selectedBuildings.Clear();
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<MeshRenderer>() != null || obj.GetComponent<MeshFilter>() != null)
            {
                selectedBuildings.Add(obj);
            }
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Building Safe Zone Batch Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Select building GameObjects in the Hierarchy, then use this tool to add SafeZone and BuildingSafeZone components in batch.",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField($"Selected Buildings: {selectedBuildings.Count}", EditorStyles.boldLabel);
        
        if (selectedBuildings.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (GameObject building in selectedBuildings)
            {
                EditorGUILayout.LabelField($"• {building.name}");
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No buildings selected. Select building GameObjects in the Hierarchy.", MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Components to Add", EditorStyles.boldLabel);
        addSafeZone = EditorGUILayout.Toggle("Add SafeZone", addSafeZone);
        addBuildingSafeZone = EditorGUILayout.Toggle("Add BuildingSafeZone", addBuildingSafeZone);
        
        if (addBuildingSafeZone)
        {
            EditorGUI.indentLevel++;
            expandSafeZone = EditorGUILayout.Toggle("Expand Safe Zone", expandSafeZone);
            if (expandSafeZone)
            {
                safeZoneExpansion = EditorGUILayout.Vector3Field("Expansion (meters)", safeZoneExpansion);
            }
            EditorGUI.indentLevel--;
        }
        
        if (addSafeZone)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("SafeZone Settings", EditorStyles.boldLabel);
            restoreHealth = EditorGUILayout.Toggle("Restore Health", restoreHealth);
            if (restoreHealth)
            {
                healthRestoreRate = EditorGUILayout.FloatField("  Health Rate/sec", healthRestoreRate);
            }
            
            restoreStamina = EditorGUILayout.Toggle("Restore Stamina", restoreStamina);
            if (restoreStamina)
            {
                staminaRestoreRate = EditorGUILayout.FloatField("  Stamina Rate/sec", staminaRestoreRate);
            }
            
            cureInfection = EditorGUILayout.Toggle("Cure Infection", cureInfection);
            normalizeTemperature = EditorGUILayout.Toggle("Normalize Temperature", normalizeTemperature);
        }
        
        EditorGUILayout.Space(15);
        
        GUI.enabled = selectedBuildings.Count > 0;
        
        if (GUILayout.Button("Apply to Selected Buildings", GUILayout.Height(40)))
        {
            ApplyToBuildings();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Find All Buildings in Scene"))
        {
            FindAllBuildings();
        }
    }
    
    private void ApplyToBuildings()
    {
        int processedCount = 0;
        
        foreach (GameObject building in selectedBuildings)
        {
            Undo.RecordObject(building, "Add Building Safe Zone");
            
            if (addSafeZone)
            {
                SafeZone safeZone = building.GetComponent<SafeZone>();
                if (safeZone == null)
                {
                    safeZone = Undo.AddComponent<SafeZone>(building);
                }
                
                safeZone.safeZoneName = building.name;
                safeZone.restoreHealth = restoreHealth;
                safeZone.restoreStamina = restoreStamina;
                safeZone.cureInfection = cureInfection;
                safeZone.normalizeTemperature = normalizeTemperature;
                safeZone.healthRestoreRate = healthRestoreRate;
                safeZone.staminaRestoreRate = staminaRestoreRate;
            }
            
            if (addBuildingSafeZone)
            {
                BuildingSafeZone buildingSafeZone = building.GetComponent<BuildingSafeZone>();
                if (buildingSafeZone == null)
                {
                    buildingSafeZone = Undo.AddComponent<BuildingSafeZone>(building);
                }
                
                buildingSafeZone.expandSafeZone = expandSafeZone;
                buildingSafeZone.safeZoneExpansion = safeZoneExpansion;
                buildingSafeZone.SetupBuildingSafeZone();
            }
            
            EditorUtility.SetDirty(building);
            processedCount++;
        }
        
        Debug.Log($"<color=green>Applied Building Safe Zone setup to {processedCount} building(s)!</color>");
        EditorUtility.DisplayDialog("Success", $"Applied setup to {processedCount} building(s)!", "OK");
    }
    
    private void FindAllBuildings()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        Selection.objects = System.Array.FindAll(allObjects, obj => 
            obj.name.ToLower().Contains("house") || 
            obj.name.ToLower().Contains("building") || 
            obj.name.ToLower().Contains("warehouse") ||
            obj.name.ToLower().Contains("bld_"));
        
        RefreshSelection();
        Debug.Log($"<color=cyan>Found and selected {selectedBuildings.Count} potential buildings</color>");
    }
}
