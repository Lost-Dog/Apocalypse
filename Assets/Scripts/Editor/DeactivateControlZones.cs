using UnityEngine;
using UnityEditor;

public class DeactivateControlZonesWindow : EditorWindow
{
    [MenuItem("Division Game/Spawn Management/Deactivate All Control Zones")]
    public static void ShowWindow()
    {
        DeactivateControlZonesWindow window = GetWindow<DeactivateControlZonesWindow>("Control Zones");
        window.minSize = new Vector2(400, 300);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Control Zone Management", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Control Zones should start INACTIVE and be activated by ChallengeManager when needed.\n\n" +
            "This prevents all control zones from spawning enemies at game start.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        ControlZone[] allZones = FindObjectsByType<ControlZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        int activeCount = 0;
        int inactiveCount = 0;
        
        foreach (ControlZone zone in allZones)
        {
            if (zone != null)
            {
                if (zone.gameObject.activeSelf)
                    activeCount++;
                else
                    inactiveCount++;
            }
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Current Status:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Control Zones: {allZones.Length}");
        EditorGUILayout.LabelField($"Active (spawning enemies): {activeCount}");
        EditorGUILayout.LabelField($"Inactive (managed): {inactiveCount}");
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        if (activeCount > 0)
        {
            EditorGUILayout.HelpBox($"⚠️ {activeCount} Control Zones are ACTIVE and will spawn enemies at start!", MessageType.Warning);
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Deactivate All Control Zones", GUILayout.Height(40)))
            {
                DeactivateAllControlZones();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✅ All Control Zones are inactive - they will only spawn when activated by ChallengeManager.", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Individual Zone Management:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (allZones.Length > 0)
        {
            EditorGUILayout.LabelField("Control Zones in Scene:", EditorStyles.miniLabel);
            
            foreach (ControlZone zone in allZones)
            {
                if (zone == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                
                bool isActive = zone.gameObject.activeSelf;
                string status = isActive ? "ACTIVE" : "Inactive";
                Color statusColor = isActive ? Color.red : Color.green;
                
                GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
                statusStyle.normal.textColor = statusColor;
                statusStyle.fontStyle = FontStyle.Bold;
                
                EditorGUILayout.LabelField($"{zone.zoneName}", GUILayout.Width(150));
                EditorGUILayout.LabelField(status, statusStyle, GUILayout.Width(80));
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = zone.gameObject;
                    EditorGUIUtility.PingObject(zone.gameObject);
                }
                
                if (isActive)
                {
                    if (GUILayout.Button("Deactivate", GUILayout.Width(80)))
                    {
                        Undo.RecordObject(zone.gameObject, "Deactivate Control Zone");
                        zone.gameObject.SetActive(false);
                        EditorUtility.SetDirty(zone.gameObject);
                    }
                }
                else
                {
                    if (GUILayout.Button("Activate", GUILayout.Width(80)))
                    {
                        Undo.RecordObject(zone.gameObject, "Activate Control Zone");
                        zone.gameObject.SetActive(true);
                        EditorUtility.SetDirty(zone.gameObject);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("No Control Zones found in scene.");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DeactivateAllControlZones()
    {
        ControlZone[] allZones = FindObjectsByType<ControlZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        int deactivatedCount = 0;
        
        foreach (ControlZone zone in allZones)
        {
            if (zone != null && zone.gameObject.activeSelf)
            {
                Undo.RecordObject(zone.gameObject, "Deactivate All Control Zones");
                zone.gameObject.SetActive(false);
                EditorUtility.SetDirty(zone.gameObject);
                deactivatedCount++;
            }
        }
        
        EditorUtility.DisplayDialog(
            "Control Zones Deactivated",
            $"Deactivated {deactivatedCount} Control Zones.\n\n" +
            "They will now only spawn enemies when activated by ChallengeManager.",
            "OK"
        );
        
        Debug.Log($"Deactivated {deactivatedCount} Control Zones - they are now manager-controlled");
        
        Repaint();
    }
}
