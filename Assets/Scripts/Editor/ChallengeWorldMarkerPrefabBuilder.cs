using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class ChallengeWorldMarkerPrefabBuilder : EditorWindow
{
    private Sprite iconSprite;
    private Color iconColor = Color.yellow;
    
    [MenuItem("Division Game/Challenge System/Create UI WorldMarker Prefab")]
    public static void ShowWindow()
    {
        ChallengeWorldMarkerPrefabBuilder window = GetWindow<ChallengeWorldMarkerPrefabBuilder>("Create UI WorldMarker");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge World Marker Prefab Builder", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool creates a UI-based ChallengeWorldMarker prefab with:\n" +
            "• Screen-space icon image\n" +
            "• Distance TextMeshPro label\n" +
            "• Difficulty-based coloring\n" +
            "• Smooth fade in/out",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        iconSprite = (Sprite)EditorGUILayout.ObjectField("Icon Sprite", iconSprite, typeof(Sprite), false);
        iconColor = EditorGUILayout.ColorField("Default Icon Color", iconColor);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Create New UI WorldMarker Prefab", GUILayout.Height(40)))
        {
            CreateUIMarkerPrefab();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Update Existing Prefab to UI Version", GUILayout.Height(40)))
        {
            UpdateExistingPrefab();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Setup ChallengeManager References", GUILayout.Height(40)))
        {
            SetupChallengeManager();
        }
    }
    
    private void CreateUIMarkerPrefab()
    {
        GameObject markerRoot = new GameObject("ChallengeWorldMarker");
        
        RectTransform rectTransform = markerRoot.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 120);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        CanvasGroup canvasGroup = markerRoot.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(markerRoot.transform);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(64, 64);
        iconRect.anchoredPosition = new Vector2(0, 30);
        
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = iconColor;
        iconImage.raycastTarget = false;
        
        GameObject distanceObj = new GameObject("Distance");
        distanceObj.transform.SetParent(markerRoot.transform);
        RectTransform distanceRect = distanceObj.AddComponent<RectTransform>();
        distanceRect.sizeDelta = new Vector2(100, 30);
        distanceRect.anchoredPosition = new Vector2(0, -20);
        
        TextMeshProUGUI distanceText = distanceObj.AddComponent<TextMeshProUGUI>();
        distanceText.text = "000m";
        distanceText.fontSize = 24;
        distanceText.color = iconColor;
        distanceText.alignment = TextAlignmentOptions.Center;
        distanceText.raycastTarget = false;
        distanceText.fontStyle = FontStyles.Bold;
        
        ChallengeWorldMarker markerScript = markerRoot.AddComponent<ChallengeWorldMarker>();
        
        SerializedObject so = new SerializedObject(markerScript);
        so.FindProperty("markerRoot").objectReferenceValue = rectTransform;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("distanceText").objectReferenceValue = distanceText;
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.ApplyModifiedProperties();
        
        string prefabPath = "Assets/Prefabs/ChallengeWorldMarker_UI.prefab";
        
        PrefabUtility.SaveAsPrefabAsset(markerRoot, prefabPath);
        
        DestroyImmediate(markerRoot);
        
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        Debug.Log($"<color=green>✓ Created UI-based ChallengeWorldMarker prefab at {prefabPath}</color>");
        
        EditorUtility.DisplayDialog(
            "Prefab Created",
            $"UI-based ChallengeWorldMarker prefab created!\n\nPath: {prefabPath}\n\n" +
            "Next: Setup ChallengeManager to use this prefab.",
            "OK");
        
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
    
    private void UpdateExistingPrefab()
    {
        string prefabPath = "Assets/Prefabs/ChallengeWorldMarker.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (existingPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", $"Could not find prefab at {prefabPath}", "OK");
            return;
        }
        
        bool confirm = EditorUtility.DisplayDialog(
            "Update Existing Prefab",
            $"This will replace the 3D ChallengeWorldMarker prefab with a UI version.\n\n" +
            "The old prefab will be backed up to: {prefabPath}.backup\n\n" +
            "Continue?",
            "Yes, Update",
            "Cancel");
        
        if (!confirm)
            return;
        
        string backupPath = prefabPath + ".backup";
        AssetDatabase.CopyAsset(prefabPath, backupPath);
        
        CreateUIMarkerPrefab();
        
        string newPrefabPath = "Assets/Prefabs/ChallengeWorldMarker_UI.prefab";
        AssetDatabase.DeleteAsset(prefabPath);
        AssetDatabase.MoveAsset(newPrefabPath, prefabPath);
        
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>✓ Updated ChallengeWorldMarker prefab to UI version! Backup saved at {backupPath}</color>");
        
        EditorUtility.DisplayDialog(
            "Prefab Updated",
            $"ChallengeWorldMarker prefab updated to UI version!\n\n" +
            $"Backup saved at: {backupPath}",
            "OK");
    }
    
    private void SetupChallengeManager()
    {
        GameObject challengeManagerObj = GameObject.Find("GameSystems/ChallengeManager");
        
        if (challengeManagerObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find GameSystems/ChallengeManager in scene!", "OK");
            return;
        }
        
        ChallengeManager manager = challengeManagerObj.GetComponent<ChallengeManager>();
        
        if (manager == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeManager component not found!", "OK");
            return;
        }
        
        GameObject uiObj = GameObject.Find("UI/HUD/ScreenSpace");
        
        if (uiObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find UI/HUD/ScreenSpace in scene!", "OK");
            return;
        }
        
        GameObject markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChallengeWorldMarker.prefab");
        
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("worldMarkerPrefab").objectReferenceValue = markerPrefab;
        so.FindProperty("worldspaceUIContainer").objectReferenceValue = uiObj.transform;
        so.FindProperty("spawnWorldspaceUI").boolValue = true;
        so.FindProperty("spawnWorldMarkers").boolValue = true;
        so.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(manager);
        
        Debug.Log("<color=green>✓ ChallengeManager configured with UI-based WorldMarker!</color>");
        
        EditorUtility.DisplayDialog(
            "Setup Complete",
            "ChallengeManager configured!\n\n" +
            "• worldMarkerPrefab assigned\n" +
            "• worldspaceUIContainer set to ScreenSpace\n" +
            "• Spawn flags enabled\n\n" +
            "Ready to spawn UI-based challenge markers!",
            "OK");
        
        Selection.activeGameObject = challengeManagerObj;
        EditorGUIUtility.PingObject(challengeManagerObj);
    }
}
