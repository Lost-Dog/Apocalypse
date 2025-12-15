using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class ChallengeMinimapMarkerSetup : EditorWindow
{
    private Sprite iconSprite;
    
    [MenuItem("Division Game/Challenge System/Setup Minimap Markers")]
    public static void ShowWindow()
    {
        ChallengeMinimapMarkerSetup window = GetWindow<ChallengeMinimapMarkerSetup>("Minimap Marker Setup");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Minimap Marker Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Create a minimap marker prefab\n" +
            "2. Find and assign the minimap Icons container\n" +
            "3. Update ChallengeManager references\n\n" +
            "Make sure your minimap system is in the scene!",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        iconSprite = (Sprite)EditorGUILayout.ObjectField("Marker Icon", iconSprite, typeof(Sprite), false);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Create Minimap Marker Prefab", GUILayout.Height(40)))
        {
            CreateMinimapMarkerPrefab();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Setup ChallengeManager References", GUILayout.Height(40)))
        {
            SetupChallengeManager();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Complete Setup (All Steps)", GUILayout.Height(40)))
        {
            CreateMinimapMarkerPrefab();
            SetupChallengeManager();
        }
    }
    
    private void CreateMinimapMarkerPrefab()
    {
        string prefabPath = "Assets/Prefabs/ChallengeMinimapMarker.prefab";
        
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Prefab Exists",
                "ChallengeMinimapMarker prefab already exists. Overwrite?",
                "Yes, Overwrite",
                "Cancel");
            
            if (!overwrite)
                return;
        }
        
        GameObject markerRoot = new GameObject("ChallengeMinimapMarker");
        
        RectTransform rectTransform = markerRoot.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(20, 20);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        Image iconImage = markerRoot.AddComponent<Image>();
        if (iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }
        iconImage.color = Color.white;
        iconImage.raycastTarget = false;
        
        ChallengeMinimapMarker markerScript = markerRoot.AddComponent<ChallengeMinimapMarker>();
        
        SerializedObject so = new SerializedObject(markerScript);
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("iconRect").objectReferenceValue = rectTransform;
        so.FindProperty("iconSize").floatValue = 20f;
        so.FindProperty("defaultIcon").objectReferenceValue = iconSprite;
        so.ApplyModifiedProperties();
        
        Directory.CreateDirectory("Assets/Prefabs");
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(markerRoot, prefabPath);
        DestroyImmediate(markerRoot);
        
        Debug.Log($"<color=green>✓ Created minimap marker prefab at {prefabPath}</color>");
        
        EditorUtility.DisplayDialog(
            "Prefab Created",
            "Minimap marker prefab created successfully!\n\n" +
            "Prefab location: Assets/Prefabs/ChallengeMinimapMarker.prefab\n\n" +
            "Next: Setup ChallengeManager references",
            "OK");
        
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
    
    private void SetupChallengeManager()
    {
        GameObject minimapIconsObj = GameObject.Find("MiniMap [Circle] [Style 2]/Canvas/MiniMap UI/MiniMap/Map Image/Masked Area/Icons");
        
        if (minimapIconsObj == null)
        {
            EditorUtility.DisplayDialog(
                "Minimap Not Found",
                "Could not find minimap Icons container!\n\n" +
                "Expected path:\n" +
                "MiniMap [Circle] [Style 2]/Canvas/MiniMap UI/MiniMap/Map Image/Masked Area/Icons\n\n" +
                "Make sure your minimap is in the scene.",
                "OK");
            return;
        }
        
        GameObject challengeManagerObj = GameObject.Find("GameSystems/ChallengeManager");
        if (challengeManagerObj == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeManager not found in scene!", "OK");
            return;
        }
        
        ChallengeManager manager = challengeManagerObj.GetComponent<ChallengeManager>();
        if (manager == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeManager component not found!", "OK");
            return;
        }
        
        GameObject minimapMarkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChallengeMinimapMarker.prefab");
        
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("minimapMarkerPrefab").objectReferenceValue = minimapMarkerPrefab;
        so.FindProperty("minimapMarkerContainer").objectReferenceValue = minimapIconsObj.transform;
        so.FindProperty("spawnMinimapMarkers").boolValue = true;
        so.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(manager);
        
        Debug.Log("<color=green>✓ ChallengeManager configured with minimap references!</color>");
        
        EditorUtility.DisplayDialog(
            "Setup Complete",
            "ChallengeManager has been configured!\n\n" +
            "✓ Minimap marker prefab assigned\n" +
            "✓ Minimap container assigned\n" +
            "✓ Spawn minimap markers enabled\n\n" +
            "Challenge markers will now appear on the minimap!",
            "OK");
        
        Selection.activeGameObject = challengeManagerObj;
        EditorGUIUtility.PingObject(challengeManagerObj);
    }
}
