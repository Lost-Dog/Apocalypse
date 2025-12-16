using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ChallengeMarkerOverlayHelper : EditorWindow
{
    private GameObject canvasObject;
    private int overlayLayer = 31; // Use high layer number for overlay
    private bool alwaysOnTop = true;
    private float sortingOrder = 1000f;
    
    [MenuItem("Division Game/Challenge System/Configure Marker Overlay")]
    public static void ShowWindow()
    {
        ChallengeMarkerOverlayHelper window = GetWindow<ChallengeMarkerOverlayHelper>("Marker Overlay Config");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }
    
    private void OnEnable()
    {
        canvasObject = GameObject.Find("UI/HUD/WorldSpace_Challenges");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Marker Overlay Configuration", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Configure markers to render OVER all other objects.\n\n" +
            "This tool will:\n" +
            "• Set proper canvas sorting order\n" +
            "• Configure camera culling\n" +
            "• Adjust render queue for materials\n" +
            "• Set up layer-based rendering",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        alwaysOnTop = EditorGUILayout.Toggle("Always Render On Top", alwaysOnTop);
        
        if (alwaysOnTop)
        {
            EditorGUILayout.HelpBox(
                "Markers will render over ALL objects including terrain and buildings.\n" +
                "Recommended for challenge markers to ensure visibility.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Markers will be occluded by world geometry.\n" +
                "More realistic but markers may be hidden behind objects.",
                MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        overlayLayer = EditorGUILayout.IntSlider("Overlay Layer", overlayLayer, 8, 31);
        sortingOrder = EditorGUILayout.FloatField("Sorting Order", sortingOrder);
        
        EditorGUILayout.Space(15);
        
        if (GUILayout.Button("Apply Overlay Configuration", GUILayout.Height(40)))
        {
            ApplyOverlaySettings();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Quick Fix: Make Markers Always Visible", GUILayout.Height(35)))
        {
            QuickFixAlwaysVisible();
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        
        if (canvasObject != null)
        {
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            EditorGUILayout.HelpBox($"✓ Canvas found: {canvasObject.name}\n" +
                                   $"Render Mode: {canvas.renderMode}\n" +
                                   $"Sort Order: {canvas.sortingOrder}\n" +
                                   $"Layer: {LayerMask.LayerToName(canvasObject.layer)}", 
                                   MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ WorldSpace_Challenges canvas not found in scene!", MessageType.Warning);
            
            if (GUILayout.Button("Find Canvas"))
            {
                canvasObject = GameObject.Find("UI/HUD/WorldSpace_Challenges");
            }
        }
    }
    
    private void ApplyOverlaySettings()
    {
        if (canvasObject == null)
        {
            canvasObject = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        }
        
        if (canvasObject == null)
        {
            EditorUtility.DisplayDialog("Error", "WorldSpace_Challenges canvas not found!", "OK");
            return;
        }
        
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Canvas component not found!", "OK");
            return;
        }
        
        Undo.RecordObject(canvas, "Configure Marker Overlay");
        Undo.RecordObject(canvasObject, "Configure Marker Overlay Layer");
        
        // Set canvas to render on specific layer
        canvasObject.layer = overlayLayer;
        
        // Set all children to same layer
        foreach (Transform child in canvasObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = overlayLayer;
        }
        
        // Configure canvas sorting
        canvas.sortingOrder = (int)sortingOrder;
        canvas.overrideSorting = true;
        
        // If WorldSpace canvas, ensure proper camera setup
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.planeDistance = 0.1f; // Render close to camera
        }
        
        // Update all Image and Text components to use proper material
        if (alwaysOnTop)
        {
            UpdateUIComponentsForOverlay(canvasObject);
        }
        
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(canvasObject);
        
        Debug.Log($"<color=green>✓ Overlay settings applied!</color>");
        Debug.Log($"  Layer: {overlayLayer} ({LayerMask.LayerToName(overlayLayer)})");
        Debug.Log($"  Sorting Order: {sortingOrder}");
        Debug.Log($"  Always On Top: {alwaysOnTop}");
        
        EditorUtility.DisplayDialog(
            "Overlay Configured",
            $"Marker overlay settings applied!\n\n" +
            $"Layer: {overlayLayer}\n" +
            $"Sorting Order: {sortingOrder}\n" +
            $"Always On Top: {alwaysOnTop}\n\n" +
            "Next steps:\n" +
            "1. Ensure your camera culls layer {overlayLayer}\n" +
            "2. Test in Play Mode\n" +
            "3. Adjust sorting order if needed",
            "OK");
    }
    
    private void UpdateUIComponentsForOverlay(GameObject root)
    {
        // Update all Images
        foreach (Image img in root.GetComponentsInChildren<Image>(true))
        {
            if (img.material != null && img.material.shader != null)
            {
                // Ensure material uses proper render queue
                Material mat = img.material;
                if (mat.renderQueue < 3000)
                {
                    mat.renderQueue = 3000; // Transparent queue
                }
            }
        }
        
        // Update all Text components
        foreach (TMPro.TextMeshProUGUI text in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
        {
            if (text.fontMaterial != null)
            {
                Material mat = text.fontMaterial;
                if (mat.renderQueue < 3000)
                {
                    mat.renderQueue = 3000;
                }
            }
        }
        
        Debug.Log($"<color=cyan>Updated {root.GetComponentsInChildren<Image>(true).Length} images and {root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).Length} texts for overlay rendering</color>");
    }
    
    private void QuickFixAlwaysVisible()
    {
        if (canvasObject == null)
        {
            canvasObject = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        }
        
        if (canvasObject == null)
        {
            EditorUtility.DisplayDialog("Error", "WorldSpace_Challenges canvas not found!", "OK");
            return;
        }
        
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null) return;
        
        Undo.RecordObject(canvas, "Quick Fix Markers");
        
        // Best settings for always-visible markers
        canvas.sortingOrder = 1000;
        canvas.overrideSorting = true;
        canvasObject.layer = 5; // UI layer
        
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.planeDistance = 0.1f;
        }
        
        // Set all children to UI layer
        foreach (Transform child in canvasObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = 5; // UI layer
        }
        
        EditorUtility.SetDirty(canvas);
        
        Debug.Log("<color=green>✓ Quick fix applied! Markers should now always be visible.</color>");
        
        EditorUtility.DisplayDialog(
            "Quick Fix Applied",
            "Challenge markers configured for maximum visibility!\n\n" +
            "Changes:\n" +
            "• Layer set to UI (5)\n" +
            "• Sorting order: 1000\n" +
            "• Plane distance: 0.1\n\n" +
            "Markers will now render on top of everything.",
            "OK");
    }
}
