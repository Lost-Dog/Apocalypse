using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ChallengeCompassSetupTool : EditorWindow
{
    private GameObject iconObjectivePrefab;
    private string prefabSavePath = "Assets/Prefabs/ChallengeCompassMarker.prefab";
    
    [MenuItem("Division Game/Challenge System/Setup Compass Markers")]
    public static void ShowWindow()
    {
        ChallengeCompassSetupTool window = GetWindow<ChallengeCompassSetupTool>("Challenge Compass Setup");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Compass Marker Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool will set up screenspace compass markers for challenges.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Current Setup Status", EditorStyles.boldLabel);
        
        GameObject challengeManager = GameObject.Find("GameSystems/ChallengeManager");
        if (challengeManager != null)
        {
            ChallengeManager cm = challengeManager.GetComponent<ChallengeManager>();
            if (cm != null)
            {
                EditorGUILayout.HelpBox("✓ ChallengeManager found", MessageType.Info);
                
                if (cm.compassMarkerPrefab != null)
                    EditorGUILayout.HelpBox($"✓ Compass Marker Prefab: {cm.compassMarkerPrefab.name}", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("✗ Compass Marker Prefab not assigned", MessageType.Warning);
                    
                if (cm.compassMarkerContainer != null)
                    EditorGUILayout.HelpBox($"✓ Compass Container: {cm.compassMarkerContainer.name}", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("✗ Compass Container not assigned", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✗ ChallengeManager not found in scene!", MessageType.Error);
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Setup Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("1. Add Script to ICON_Objective", GUILayout.Height(35)))
        {
            AddScriptToIconObjective();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("2. Create Compass Marker Prefab", GUILayout.Height(35)))
        {
            CreateCompassMarkerPrefab();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("3. Wire ChallengeManager References", GUILayout.Height(35)))
        {
            WireChallengeManager();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("⚡ Quick Setup (All Steps)", GUILayout.Height(40)))
        {
            QuickSetup();
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Manual Configuration", EditorStyles.boldLabel);
        
        prefabSavePath = EditorGUILayout.TextField("Prefab Save Path", prefabSavePath);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Steps:\n" +
            "1. Add ChallengeCompassMarker script to ICON_Objective in the scene\n" +
            "2. Create a prefab from ICON_Objective\n" +
            "3. Wire the prefab reference to ChallengeManager",
            MessageType.None);
    }
    
    private void AddScriptToIconObjective()
    {
        GameObject iconObj = GameObject.Find("UI/HUD/ScreenSpace/Top/HUD_Apocalypse_Compass_01/Content/Compass_Content/Mask/Icons/ICON_Objective");
        
        if (iconObj == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find ICON_Objective in the scene!\n\nExpected path:\nUI/HUD/ScreenSpace/Top/HUD_Apocalypse_Compass_01/Content/Compass_Content/Mask/Icons/ICON_Objective", 
                "OK");
            return;
        }
        
        ChallengeCompassMarker existingMarker = iconObj.GetComponent<ChallengeCompassMarker>();
        if (existingMarker != null)
        {
            Debug.Log("ICON_Objective already has ChallengeCompassMarker script.");
            EditorGUIUtility.PingObject(iconObj);
            Selection.activeGameObject = iconObj;
            return;
        }
        
        Undo.RecordObject(iconObj, "Add ChallengeCompassMarker Script");
        ChallengeCompassMarker marker = iconObj.AddComponent<ChallengeCompassMarker>();
        
        RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
        Image image = iconObj.GetComponent<Image>();
        
        SerializedObject so = new SerializedObject(marker);
        so.FindProperty("markerRectTransform").objectReferenceValue = rectTransform;
        so.FindProperty("markerImage").objectReferenceValue = image;
        so.FindProperty("compassWidth").floatValue = 1000f;
        so.FindProperty("edgePadding").floatValue = 50f;
        so.ApplyModifiedProperties();
        
        Debug.Log("<color=green>✓ Added ChallengeCompassMarker script to ICON_Objective!</color>");
        EditorGUIUtility.PingObject(iconObj);
        Selection.activeGameObject = iconObj;
        
        EditorUtility.DisplayDialog("Success", 
            "ChallengeCompassMarker script added to ICON_Objective!\n\nThe GameObject is now selected in the hierarchy.", 
            "OK");
    }
    
    private void CreateCompassMarkerPrefab()
    {
        GameObject iconObj = GameObject.Find("UI/HUD/ScreenSpace/Top/HUD_Apocalypse_Compass_01/Content/Compass_Content/Mask/Icons/ICON_Objective");
        
        if (iconObj == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find ICON_Objective in the scene!", 
                "OK");
            return;
        }
        
        ChallengeCompassMarker existingMarker = iconObj.GetComponent<ChallengeCompassMarker>();
        if (existingMarker == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "ICON_Objective doesn't have ChallengeCompassMarker script!\n\nRun step 1 first.", 
                "OK");
            return;
        }
        
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabSavePath);
        if (existingPrefab != null)
        {
            bool overwrite = EditorUtility.DisplayDialog("Prefab Exists", 
                $"A prefab already exists at:\n{prefabSavePath}\n\nDo you want to overwrite it?", 
                "Yes", "Cancel");
                
            if (!overwrite)
            {
                Debug.Log("Prefab creation cancelled.");
                return;
            }
        }
        
        GameObject newMarker = new GameObject("ChallengeCompassMarker");
        
        RectTransform rectTransform = newMarker.AddComponent<RectTransform>();
        rectTransform.sizeDelta = iconObj.GetComponent<RectTransform>().sizeDelta;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = newMarker.AddComponent<Image>();
        Image sourceImage = iconObj.GetComponent<Image>();
        if (sourceImage != null)
        {
            image.sprite = sourceImage.sprite;
            image.color = sourceImage.color;
            image.raycastTarget = false;
        }
        
        ChallengeCompassMarker marker = newMarker.AddComponent<ChallengeCompassMarker>();
        
        SerializedObject so = new SerializedObject(marker);
        so.FindProperty("markerRectTransform").objectReferenceValue = rectTransform;
        so.FindProperty("markerImage").objectReferenceValue = image;
        so.FindProperty("compassWidth").floatValue = 1000f;
        so.FindProperty("edgePadding").floatValue = 50f;
        so.ApplyModifiedProperties();
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(newMarker, prefabSavePath);
        
        DestroyImmediate(newMarker);
        
        if (prefab != null)
        {
            Debug.Log($"<color=green>✓ Created compass marker prefab at: {prefabSavePath}</color>");
            EditorGUIUtility.PingObject(prefab);
            
            EditorUtility.DisplayDialog("Success", 
                $"Compass marker prefab created!\n\nPath: {prefabSavePath}", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", 
                "Failed to create prefab!", 
                "OK");
        }
    }
    
    private void WireChallengeManager()
    {
        GameObject challengeManagerObj = GameObject.Find("GameSystems/ChallengeManager");
        
        if (challengeManagerObj == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find ChallengeManager in the scene!", 
                "OK");
            return;
        }
        
        ChallengeManager cm = challengeManagerObj.GetComponent<ChallengeManager>();
        if (cm == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "ChallengeManager component not found!", 
                "OK");
            return;
        }
        
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabSavePath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Compass marker prefab not found at:\n{prefabSavePath}\n\nRun step 2 first.", 
                "OK");
            return;
        }
        
        GameObject containerObj = GameObject.Find("UI/HUD/ScreenSpace/Top/HUD_Apocalypse_Compass_01/Content/Compass_Content/Mask/Icons");
        if (containerObj == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find compass Icons container in the scene!", 
                "OK");
            return;
        }
        
        Undo.RecordObject(cm, "Wire ChallengeManager Compass References");
        
        SerializedObject so = new SerializedObject(cm);
        so.FindProperty("compassMarkerPrefab").objectReferenceValue = prefab;
        so.FindProperty("compassMarkerContainer").objectReferenceValue = containerObj.transform;
        so.FindProperty("spawnCompassMarkers").boolValue = true;
        so.ApplyModifiedProperties();
        
        Debug.Log("<color=green>✓ ChallengeManager compass references wired!</color>");
        EditorGUIUtility.PingObject(challengeManagerObj);
        Selection.activeGameObject = challengeManagerObj;
        
        EditorUtility.DisplayDialog("Success", 
            "ChallengeManager compass references configured!\n\n" +
            "✓ Compass Marker Prefab assigned\n" +
            "✓ Compass Container assigned\n" +
            "✓ Spawn Compass Markers enabled", 
            "OK");
    }
    
    private void QuickSetup()
    {
        bool proceed = EditorUtility.DisplayDialog("Quick Setup", 
            "This will:\n" +
            "1. Add ChallengeCompassMarker to ICON_Objective\n" +
            "2. Create a prefab\n" +
            "3. Wire ChallengeManager references\n\n" +
            "Continue?", 
            "Yes", "Cancel");
            
        if (!proceed)
            return;
            
        AddScriptToIconObjective();
        CreateCompassMarkerPrefab();
        WireChallengeManager();
        
        Debug.Log("<color=green>✓✓✓ Challenge compass marker setup complete!</color>");
    }
}
