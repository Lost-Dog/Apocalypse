using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ChallengeMarkerScreenSpaceSetup : EditorWindow
{
    private GameObject mainCanvas;
    private GameObject markerContainer;
    private string containerPath = "UI/HUD";
    
    [MenuItem("Division Game/Challenge System/Setup Screen Space Markers")]
    public static void ShowWindow()
    {
        ChallengeMarkerScreenSpaceSetup window = GetWindow<ChallengeMarkerScreenSpaceSetup>("Screen Space Setup");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }
    
    private void OnEnable()
    {
        FindMainCanvas();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Screen Space Marker Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will set up challenge markers in your main Screen Space UI canvas.\n\n" +
            "Benefits:\n" +
            "• Always renders on top (no occlusion)\n" +
            "• Better performance\n" +
            "• Easier to manage\n" +
            "• No separate WorldSpace canvas needed",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Step 1: Find Main Canvas", EditorStyles.boldLabel);
        
        containerPath = EditorGUILayout.TextField("Canvas Path", containerPath);
        
        if (GUILayout.Button("Find Main Canvas", GUILayout.Height(30)))
        {
            FindMainCanvas();
        }
        
        if (mainCanvas != null)
        {
            Canvas canvas = mainCanvas.GetComponent<Canvas>();
            EditorGUILayout.HelpBox(
                $"✓ Canvas found: {mainCanvas.name}\n" +
                $"Render Mode: {canvas.renderMode}\n" +
                $"Layer: {LayerMask.LayerToName(mainCanvas.layer)}",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ Main canvas not found at path: " + containerPath, MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Step 2: Create Marker Container", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create 'ChallengeMarkers' Container", GUILayout.Height(35)))
        {
            CreateMarkerContainer();
        }
        
        if (markerContainer != null)
        {
            EditorGUILayout.HelpBox($"✓ Container ready: {markerContainer.name}", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Step 3: Configure ChallengeManager", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Configure ChallengeManager", GUILayout.Height(35)))
        {
            ConfigureChallengeManager();
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.HelpBox(
            "After setup:\n" +
            "1. ChallengeManager will spawn markers in screen space\n" +
            "2. Markers will automatically position themselves\n" +
            "3. No WorldSpace canvas needed\n" +
            "4. Better performance and reliability",
            MessageType.None);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Complete Setup (All Steps)", GUILayout.Height(40)))
        {
            CompleteSetup();
        }
    }
    
    private void FindMainCanvas()
    {
        mainCanvas = GameObject.Find(containerPath);
        
        if (mainCanvas == null)
        {
            // Try common paths
            string[] commonPaths = {
                "UI",
                "Canvas",
                "UI/HUD",
                "HUD",
                "UICanvas"
            };
            
            foreach (string path in commonPaths)
            {
                mainCanvas = GameObject.Find(path);
                if (mainCanvas != null)
                {
                    Canvas canvas = mainCanvas.GetComponent<Canvas>();
                    if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        containerPath = path;
                        Debug.Log($"<color=cyan>Found Screen Space canvas at: {path}</color>");
                        break;
                    }
                }
            }
        }
    }
    
    private void CreateMarkerContainer()
    {
        if (mainCanvas == null)
        {
            FindMainCanvas();
        }
        
        if (mainCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Main canvas not found! Please find it first.", "OK");
            return;
        }
        
        // Check if container already exists
        Transform existing = mainCanvas.transform.Find("ChallengeMarkers");
        if (existing != null)
        {
            markerContainer = existing.gameObject;
            Debug.Log("<color=yellow>Container already exists, using existing one</color>");
            EditorUtility.DisplayDialog("Already Exists", "ChallengeMarkers container already exists!", "OK");
            return;
        }
        
        // Create new container
        GameObject container = new GameObject("ChallengeMarkers");
        container.transform.SetParent(mainCanvas.transform, false);
        
        // Add RectTransform and configure to fill parent
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        markerContainer = container;
        
        Debug.Log($"<color=green>✓ Created ChallengeMarkers container in {mainCanvas.name}</color>");
        
        EditorUtility.DisplayDialog(
            "Container Created",
            $"ChallengeMarkers container created in {mainCanvas.name}!\n\n" +
            "This will hold all challenge world markers.",
            "OK");
    }
    
    private void ConfigureChallengeManager()
    {
        ChallengeManager challengeManager = FindObjectOfType<ChallengeManager>();
        
        if (challengeManager == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeManager not found in scene!", "OK");
            return;
        }
        
        if (markerContainer == null)
        {
            // Try to find existing container
            if (mainCanvas != null)
            {
                Transform existing = mainCanvas.transform.Find("ChallengeMarkers");
                if (existing != null)
                {
                    markerContainer = existing.gameObject;
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please create the marker container first!", "OK");
                    return;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please find the main canvas first!", "OK");
                return;
            }
        }
        
        Undo.RecordObject(challengeManager, "Configure ChallengeManager for Screen Space");
        
        challengeManager.worldspaceUIContainer = markerContainer.transform;
        challengeManager.spawnWorldMarkers = true;
        
        EditorUtility.SetDirty(challengeManager);
        
        Debug.Log("<color=green>✓ ChallengeManager configured for Screen Space markers</color>");
        
        EditorUtility.DisplayDialog(
            "Configured",
            "ChallengeManager is now set up to use Screen Space markers!\n\n" +
            "Worldspace UI Container: " + markerContainer.name + "\n\n" +
            "Test in Play Mode to see markers in your main HUD.",
            "OK");
    }
    
    private void CompleteSetup()
    {
        FindMainCanvas();
        
        if (mainCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find main canvas! Please set the path manually.", "OK");
            return;
        }
        
        CreateMarkerContainer();
        ConfigureChallengeManager();
        
        Debug.Log("<color=green>✓✓✓ Screen Space marker setup complete!</color>");
        
        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "Challenge markers are now configured for Screen Space rendering!\n\n" +
            "What this means:\n" +
            "• Markers will appear in your main HUD\n" +
            "• Always visible (never occluded)\n" +
            "• Better performance\n" +
            "• Easier to manage\n\n" +
            "Test in Play Mode to see the results!",
            "OK");
    }
}
