using UnityEngine;
using UnityEditor;
using System.Linq;

public class MissionZoneStateManager : EditorWindow
{
    private Vector2 scrollPosition;
    private MissionZone[] allZones;
    private bool showActiveOnly = false;
    private bool showInactiveOnly = false;
    
    [MenuItem("Division Game/Challenge System/Mission Zone State Manager")]
    public static void ShowWindow()
    {
        MissionZoneStateManager window = GetWindow<MissionZoneStateManager>("Zone State Manager");
        window.minSize = new Vector2(500, 400);
        window.RefreshZones();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Mission Zone State Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Manage activation state of all Mission Zones.\n\n" +
            "BEST PRACTICE: All zones should be INACTIVE at edit time.\n" +
            "ChallengeManager will activate them automatically during gameplay.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        DrawControlButtons();
        EditorGUILayout.Space(10);
        
        DrawFilterOptions();
        EditorGUILayout.Space(10);
        
        DrawZoneList();
    }
    
    private void DrawControlButtons()
    {
        EditorGUILayout.LabelField("Bulk Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Refresh List", GUILayout.Height(30)))
        {
            RefreshZones();
        }
        
        if (GUILayout.Button("Deactivate ALL Zones", GUILayout.Height(30)))
        {
            DeactivateAllZones();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Selection.gameObjects.Length > 0;
        if (GUILayout.Button($"Deactivate Selected ({Selection.gameObjects.Length})", GUILayout.Height(25)))
        {
            DeactivateSelected();
        }
        
        if (GUILayout.Button($"Activate Selected ({Selection.gameObjects.Length})", GUILayout.Height(25)))
        {
            ActivateSelected();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawFilterOptions()
    {
        EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        showActiveOnly = GUILayout.Toggle(showActiveOnly, "Show Active Only");
        showInactiveOnly = GUILayout.Toggle(showInactiveOnly, "Show Inactive Only");
        
        if (showActiveOnly && showInactiveOnly)
        {
            showActiveOnly = false;
            showInactiveOnly = false;
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawZoneList()
    {
        if (allZones == null || allZones.Length == 0)
        {
            EditorGUILayout.HelpBox("No Mission Zones found in scene.\n\nUse Mission Zone Configurator to create zones.", MessageType.Warning);
            return;
        }
        
        var filteredZones = allZones.AsEnumerable();
        
        if (showActiveOnly)
        {
            filteredZones = filteredZones.Where(z => z.gameObject.activeSelf);
        }
        else if (showInactiveOnly)
        {
            filteredZones = filteredZones.Where(z => !z.gameObject.activeSelf);
        }
        
        var zoneList = filteredZones.ToArray();
        
        EditorGUILayout.LabelField($"Mission Zones ({zoneList.Length} of {allZones.Length})", EditorStyles.boldLabel);
        
        int activeCount = allZones.Count(z => z.gameObject.activeSelf);
        int inactiveCount = allZones.Length - activeCount;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Active: {activeCount}", GUILayout.Width(100));
        EditorGUILayout.LabelField($"Inactive: {inactiveCount}", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (MissionZone zone in zoneList)
        {
            if (zone == null) continue;
            
            DrawZoneItem(zone);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawZoneItem(MissionZone zone)
    {
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = zone.gameObject.activeSelf ? new Color(1f, 0.7f, 0.7f) : new Color(0.7f, 1f, 0.7f);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = originalColor;
        
        EditorGUILayout.BeginHorizontal();
        
        string statusIcon = zone.gameObject.activeSelf ? "✅ ACTIVE" : "🔘 INACTIVE";
        EditorGUILayout.LabelField(statusIcon, GUILayout.Width(100));
        
        EditorGUILayout.LabelField(zone.zoneName, EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"Type: {zone.missionType}", GUILayout.Width(200));
        
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            Selection.activeGameObject = zone.gameObject;
            EditorGUIUtility.PingObject(zone.gameObject);
        }
        
        if (zone.gameObject.activeSelf)
        {
            if (GUILayout.Button("Deactivate", GUILayout.Width(80)))
            {
                Undo.RecordObject(zone.gameObject, "Deactivate Zone");
                zone.gameObject.SetActive(false);
                RefreshZones();
            }
        }
        else
        {
            if (GUILayout.Button("Activate", GUILayout.Width(80)))
            {
                Undo.RecordObject(zone.gameObject, "Activate Zone");
                zone.gameObject.SetActive(true);
                RefreshZones();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Spawn Points: {zone.spawnPoints.Count}", GUILayout.Width(150));
        
        if (zone.linkedChallengeData != null)
        {
            EditorGUILayout.LabelField($"Linked: {zone.linkedChallengeData.challengeName}");
        }
        else
        {
            EditorGUILayout.LabelField("No linked ChallengeData", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }
    
    private void RefreshZones()
    {
        allZones = FindObjectsByType<MissionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Repaint();
    }
    
    private void DeactivateAllZones()
    {
        if (allZones == null || allZones.Length == 0)
        {
            EditorUtility.DisplayDialog("No Zones Found", "No Mission Zones found in the scene.", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog(
            "Deactivate All Zones?",
            $"This will deactivate all {allZones.Length} Mission Zones.\n\n" +
            "The zones will be managed automatically by ChallengeManager during gameplay.\n\n" +
            "Continue?",
            "Yes, Deactivate All",
            "Cancel"))
        {
            return;
        }
        
        int count = 0;
        foreach (MissionZone zone in allZones)
        {
            if (zone != null && zone.gameObject.activeSelf)
            {
                Undo.RecordObject(zone.gameObject, "Deactivate All Zones");
                zone.gameObject.SetActive(false);
                count++;
            }
        }
        
        Debug.Log($"Deactivated {count} Mission Zones. They will now be managed by ChallengeManager.");
        RefreshZones();
    }
    
    private void DeactivateSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        int count = 0;
        
        foreach (GameObject obj in selected)
        {
            MissionZone zone = obj.GetComponent<MissionZone>();
            if (zone != null && obj.activeSelf)
            {
                Undo.RecordObject(obj, "Deactivate Selected Zones");
                obj.SetActive(false);
                count++;
            }
        }
        
        Debug.Log($"Deactivated {count} selected Mission Zones.");
        RefreshZones();
    }
    
    private void ActivateSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        int count = 0;
        
        foreach (GameObject obj in selected)
        {
            MissionZone zone = obj.GetComponent<MissionZone>();
            if (zone != null && !obj.activeSelf)
            {
                Undo.RecordObject(obj, "Activate Selected Zones");
                obj.SetActive(true);
                count++;
            }
        }
        
        Debug.Log($"Activated {count} selected Mission Zones.");
        RefreshZones();
    }
}
