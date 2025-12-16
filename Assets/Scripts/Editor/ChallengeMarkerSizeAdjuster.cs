using UnityEngine;
using UnityEditor;

public class ChallengeMarkerSizeAdjuster : EditorWindow
{
    private float canvasScale = 0.01f;
    private Vector2 markerSize = new Vector2(100, 120);
    private float iconSize = 64f;
    private float distanceTextSize = 24f;
    private float individualMarkerScale = 1f;
    
    private GameObject canvasObject;
    private GameObject markerPrefab;
    
    [MenuItem("Division Game/Challenge System/Adjust Marker Size")]
    public static void ShowWindow()
    {
        ChallengeMarkerSizeAdjuster window = GetWindow<ChallengeMarkerSizeAdjuster>("Adjust Marker Size");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }
    
    private void OnEnable()
    {
        canvasObject = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        if (canvasObject != null)
        {
            RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                canvasScale = rectTransform.localScale.x;
                markerSize = rectTransform.sizeDelta;
            }
        }
        
        markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChallengeWorldMarker.prefab");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Marker Size Adjustment", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Adjust the size of challenge markers in world space.\n\n" +
            "Canvas Scale: Overall size of all markers\n" +
            "Marker Size: Base canvas dimensions\n" +
            "Icon/Text Size: Individual element sizes",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Canvas Scale (Primary Size Control)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This is the main control for marker size:\n" +
            "• 0.01 = Small (current default)\n" +
            "• 0.02 = Medium\n" +
            "• 0.03 = Large\n" +
            "• 0.04 = Very Large\n\n" +
            "Recommended: 0.02 - 0.03 for good visibility",
            MessageType.None);
        
        float newCanvasScale = EditorGUILayout.Slider("Canvas Scale", canvasScale, 0.005f, 0.1f);
        
        if (newCanvasScale != canvasScale)
        {
            canvasScale = newCanvasScale;
            ApplyCanvasScale();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Small (0.015)", GUILayout.Height(30)))
        {
            canvasScale = 0.015f;
            ApplyCanvasScale();
        }
        if (GUILayout.Button("Medium (0.025)", GUILayout.Height(30)))
        {
            canvasScale = 0.025f;
            ApplyCanvasScale();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Large (0.035)", GUILayout.Height(30)))
        {
            canvasScale = 0.035f;
            ApplyCanvasScale();
        }
        if (GUILayout.Button("Very Large (0.05)", GUILayout.Height(30)))
        {
            canvasScale = 0.05f;
            ApplyCanvasScale();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Individual Marker Scale", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Adjust the base scale of each marker independently from canvas scale.\n" +
            "This is set on the ChallengeWorldMarker prefab.\n\n" +
            "Useful for fine-tuning size without affecting canvas.",
            MessageType.None);
        
        individualMarkerScale = EditorGUILayout.Slider("Marker Base Scale", individualMarkerScale, 0.5f, 3f);
        
        if (GUILayout.Button("Apply Scale to Prefab", GUILayout.Height(30)))
        {
            ApplyIndividualMarkerScale();
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Advanced Settings (Optional)", EditorStyles.boldLabel);
        
        markerSize = EditorGUILayout.Vector2Field("Marker Base Size", markerSize);
        iconSize = EditorGUILayout.Slider("Icon Size", iconSize, 32f, 128f);
        distanceTextSize = EditorGUILayout.Slider("Distance Text Size", distanceTextSize, 16f, 48f);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Apply Advanced Settings to Prefab", GUILayout.Height(35)))
        {
            ApplyAdvancedSettings();
        }
        
        EditorGUILayout.Space(10);
        
        if (canvasObject != null)
        {
            EditorGUILayout.HelpBox($"✓ Canvas found: {canvasObject.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ WorldSpace_Challenges canvas not found in scene!", MessageType.Warning);
        }
        
        if (markerPrefab != null)
        {
            EditorGUILayout.HelpBox($"✓ Prefab found: {markerPrefab.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ ChallengeWorldMarker prefab not found!", MessageType.Warning);
        }
    }
    
    private void ApplyCanvasScale()
    {
        if (canvasObject == null)
        {
            canvasObject = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        }
        
        if (canvasObject == null)
        {
            EditorUtility.DisplayDialog("Error", "WorldSpace_Challenges canvas not found in scene!", "OK");
            return;
        }
        
        RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Undo.RecordObject(rectTransform, "Adjust Canvas Scale");
            rectTransform.localScale = new Vector3(canvasScale, canvasScale, canvasScale);
            EditorUtility.SetDirty(rectTransform);
            
            Debug.Log($"<color=green>✓ Canvas scale set to {canvasScale:F3}</color>");
        }
    }
    
    private void ApplyAdvancedSettings()
    {
        if (markerPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeWorldMarker prefab not found!", "OK");
            return;
        }
        
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(markerPrefab));
        
        if (prefabInstance != null)
        {
            RectTransform rootRect = prefabInstance.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.sizeDelta = markerSize;
            }
            
            Transform iconTransform = prefabInstance.transform.Find("Icon");
            if (iconTransform != null)
            {
                RectTransform iconRect = iconTransform.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    iconRect.sizeDelta = new Vector2(iconSize, iconSize);
                }
            }
            
            Transform distanceTransform = prefabInstance.transform.Find("Distance");
            if (distanceTransform != null)
            {
                TMPro.TextMeshProUGUI distanceText = distanceTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (distanceText != null)
                {
                    distanceText.fontSize = distanceTextSize;
                }
            }
            
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, AssetDatabase.GetAssetPath(markerPrefab));
            PrefabUtility.UnloadPrefabContents(prefabInstance);
            
            Debug.Log("<color=green>✓ Prefab updated with advanced settings!</color>");
            
            EditorUtility.DisplayDialog(
                "Settings Applied",
                $"Marker prefab updated!\n\n" +
                $"Marker Size: {markerSize}\n" +
                $"Icon Size: {iconSize}x{iconSize}\n" +
                $"Text Size: {distanceTextSize}\n\n" +
                "Existing markers will update on next spawn.",
                "OK");
        }
        
        if (canvasObject != null)
        {
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                Undo.RecordObject(canvasRect, "Adjust Canvas Base Size");
                canvasRect.sizeDelta = markerSize;
                EditorUtility.SetDirty(canvasRect);
            }
        }
    }
    
    private void ApplyIndividualMarkerScale()
    {
        if (markerPrefab == null)
        {
            markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChallengeWorldMarker.prefab");
        }
        
        if (markerPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeWorldMarker prefab not found!", "OK");
            return;
        }
        
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(markerPrefab));
        
        if (prefabInstance != null)
        {
            ChallengeWorldMarker marker = prefabInstance.GetComponent<ChallengeWorldMarker>();
            if (marker != null)
            {
                // Use reflection to set the baseScale field
                var field = typeof(ChallengeWorldMarker).GetField("baseScale", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(marker, individualMarkerScale);
                }
            }
            
            // Also update transform scale as fallback
            prefabInstance.transform.localScale = Vector3.one * individualMarkerScale;
            
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, AssetDatabase.GetAssetPath(markerPrefab));
            PrefabUtility.UnloadPrefabContents(prefabInstance);
            
            Debug.Log($"<color=green>✓ Individual marker scale set to {individualMarkerScale:F2}</color>");
            
            EditorUtility.DisplayDialog(
                "Scale Applied",
                $"Marker base scale set to {individualMarkerScale:F2}\n\n" +
                "Existing markers in scene won't update automatically.\n" +
                "New spawned markers will use this scale.",
                "OK");
        }
    }
}
