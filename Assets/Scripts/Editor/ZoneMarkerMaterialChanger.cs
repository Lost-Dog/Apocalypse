using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ZoneMarkerMaterialChanger : EditorWindow
{
    private Material newMaterial;
    private bool applyToChildren = true;
    private bool onlyApplyToRenderers = true;
    
    private List<GameObject> foundZoneMarkers = new List<GameObject>();
    private Vector2 scrollPosition;
    
    [MenuItem("Division Game/Zone Tools/Change Zone Marker Materials")]
    public static void ShowWindow()
    {
        ZoneMarkerMaterialChanger window = GetWindow<ZoneMarkerMaterialChanger>("Zone Marker Materials");
        window.minSize = new Vector2(450, 500);
    }
    
    private void OnEnable()
    {
        FindZoneMarkers();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Zone Marker Material Changer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Change materials on all ZoneMarker objects in the scene.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // Material Selection
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Material Settings", EditorStyles.boldLabel);
        
        newMaterial = (Material)EditorGUILayout.ObjectField("New Material", newMaterial, typeof(Material), false);
        
        EditorGUILayout.Space(5);
        applyToChildren = EditorGUILayout.Toggle("Apply to Children", applyToChildren);
        onlyApplyToRenderers = EditorGUILayout.Toggle("Only MeshRenderers", onlyApplyToRenderers);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Zone Markers List
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Zone Markers Found: {foundZoneMarkers.Count}", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Refresh List"))
        {
            FindZoneMarkers();
        }
        
        EditorGUILayout.Space(5);
        
        // Scrollable list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        
        foreach (GameObject marker in foundZoneMarkers)
        {
            if (marker != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(marker, typeof(GameObject), true);
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = marker;
                    EditorGUIUtility.PingObject(marker);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Action Buttons
        GUI.enabled = newMaterial != null && foundZoneMarkers.Count > 0;
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Apply Material to All", GUILayout.Height(40)))
        {
            ApplyMaterialToAll();
        }
        
        EditorGUILayout.EndHorizontal();
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // Individual Selection
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Individual Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Apply to Selected ZoneMarker"))
        {
            ApplyMaterialToSelected();
        }
        
        if (GUILayout.Button("Select All ZoneMarkers"))
        {
            SelectAllZoneMarkers();
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        if (newMaterial == null)
        {
            EditorGUILayout.HelpBox("⚠️ Select a material to apply!", MessageType.Warning);
        }
        
        if (foundZoneMarkers.Count == 0)
        {
            EditorGUILayout.HelpBox("No ZoneMarker objects found in scene. Make sure objects are named 'ZoneMarker'.", MessageType.Warning);
        }
    }
    
    private void FindZoneMarkers()
    {
        foundZoneMarkers.Clear();
        
        // Find all GameObjects in scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "ZoneMarker")
            {
                foundZoneMarkers.Add(obj);
            }
        }
        
        foundZoneMarkers.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        
        Repaint();
    }
    
    private void ApplyMaterialToAll()
    {
        if (newMaterial == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a material first!", "OK");
            return;
        }
        
        int changedCount = 0;
        
        Undo.RecordObjects(foundZoneMarkers.ToArray(), "Change Zone Marker Materials");
        
        foreach (GameObject marker in foundZoneMarkers)
        {
            if (marker != null)
            {
                changedCount += ApplyMaterialToObject(marker);
            }
        }
        
        EditorUtility.DisplayDialog("Success", 
            $"Applied material to {changedCount} renderers across {foundZoneMarkers.Count} zone markers!", 
            "OK");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    
    private void ApplyMaterialToSelected()
    {
        if (newMaterial == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a material first!", "OK");
            return;
        }
        
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a ZoneMarker in the hierarchy first!", "OK");
            return;
        }
        
        if (selected.name != "ZoneMarker")
        {
            if (!EditorUtility.DisplayDialog("Warning", 
                $"Selected object '{selected.name}' is not named 'ZoneMarker'. Apply material anyway?", 
                "Yes", "No"))
            {
                return;
            }
        }
        
        Undo.RecordObject(selected, "Change Zone Marker Material");
        
        int changedCount = ApplyMaterialToObject(selected);
        
        EditorUtility.DisplayDialog("Success", 
            $"Applied material to {changedCount} renderer(s) on {selected.name}!", 
            "OK");
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    
    private int ApplyMaterialToObject(GameObject obj)
    {
        int changedCount = 0;
        
        if (onlyApplyToRenderers)
        {
            if (applyToChildren)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Undo.RecordObject(renderer, "Change Material");
                    renderer.sharedMaterial = newMaterial;
                    changedCount++;
                }
            }
            else
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Undo.RecordObject(renderer, "Change Material");
                    renderer.sharedMaterial = newMaterial;
                    changedCount++;
                }
            }
        }
        else
        {
            // Apply to all MeshFilters (for cases where renderer might be separate)
            if (applyToChildren)
            {
                MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshRenderers)
                {
                    Undo.RecordObject(mr, "Change Material");
                    mr.sharedMaterial = newMaterial;
                    changedCount++;
                }
            }
            else
            {
                MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Undo.RecordObject(mr, "Change Material");
                    mr.sharedMaterial = newMaterial;
                    changedCount++;
                }
            }
        }
        
        return changedCount;
    }
    
    private void SelectAllZoneMarkers()
    {
        if (foundZoneMarkers.Count > 0)
        {
            Selection.objects = foundZoneMarkers.ToArray();
            EditorUtility.DisplayDialog("Selected", 
                $"Selected {foundZoneMarkers.Count} ZoneMarker objects!", 
                "OK");
        }
    }
}
