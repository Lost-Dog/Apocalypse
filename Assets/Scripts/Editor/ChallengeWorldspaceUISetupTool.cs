using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class ChallengeWorldspaceUISetupTool : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Setup Worldspace UI Markers")]
    public static void ShowWindow()
    {
        ChallengeWorldspaceUISetupTool window = GetWindow<ChallengeWorldspaceUISetupTool>("Setup Worldspace UI Markers");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Worldspace UI Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will configure the WorldSpace_Objective UI elements to work with the challenge system.\n\n" +
            "It will add ChallengeWorldspaceUI components and wire up the references automatically.",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Setup All WorldSpace_Objective Markers", GUILayout.Height(40)))
        {
            SetupWorldspaceMarkers();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Configure ChallengeManager Reference", GUILayout.Height(40)))
        {
            ConfigureChallengeManager();
        }
    }
    
    private void SetupWorldspaceMarkers()
    {
        GameObject worldspaceContainer = GameObject.Find("UI/HUD/WorldSpace");
        
        if (worldspaceContainer == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find UI/HUD/WorldSpace in the scene!", "OK");
            return;
        }
        
        int setupCount = 0;
        
        foreach (Transform child in worldspaceContainer.transform)
        {
            if (child.name.StartsWith("WorldSpace_Objective_"))
            {
                ChallengeWorldspaceUI existingUI = child.GetComponent<ChallengeWorldspaceUI>();
                
                if (existingUI == null)
                {
                    existingUI = child.gameObject.AddComponent<ChallengeWorldspaceUI>();
                    Undo.RegisterCreatedObjectUndo(existingUI, "Add ChallengeWorldspaceUI");
                }
                
                SerializedObject so = new SerializedObject(existingUI);
                
                RectTransform markerRoot = child.GetComponent<RectTransform>();
                so.FindProperty("markerRoot").objectReferenceValue = markerRoot;
                
                Transform iconTransform = child.Find("Icon/ICON");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    so.FindProperty("iconImage").objectReferenceValue = iconImage;
                    
                    GameObject iconContainer = child.Find("Icon")?.gameObject;
                    so.FindProperty("iconContainer").objectReferenceValue = iconContainer;
                }
                
                Transform distanceTransform = child.Find("Distance/Label_ObjectiveDistance");
                if (distanceTransform != null)
                {
                    TextMeshProUGUI distanceText = distanceTransform.GetComponent<TextMeshProUGUI>();
                    so.FindProperty("distanceText").objectReferenceValue = distanceText;
                    
                    GameObject distanceContainer = child.Find("Distance")?.gameObject;
                    so.FindProperty("distanceContainer").objectReferenceValue = distanceContainer;
                }
                
                so.ApplyModifiedProperties();
                
                child.gameObject.SetActive(false);
                
                setupCount++;
            }
        }
        
        Debug.Log($"<color=green>✓ Set up {setupCount} WorldSpace_Objective markers with ChallengeWorldspaceUI components!</color>");
        
        EditorUtility.DisplayDialog(
            "Setup Complete",
            $"Configured {setupCount} WorldSpace_Objective markers.\n\n" +
            "Next, configure the ChallengeManager reference.",
            "OK");
    }
    
    private void ConfigureChallengeManager()
    {
        GameObject challengeManagerObj = GameObject.Find("GameSystems/ChallengeManager");
        
        if (challengeManagerObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find GameSystems/ChallengeManager in the scene!", "OK");
            return;
        }
        
        ChallengeManager manager = challengeManagerObj.GetComponent<ChallengeManager>();
        
        if (manager == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeManager component not found!", "OK");
            return;
        }
        
        GameObject worldspaceContainer = GameObject.Find("UI/HUD/WorldSpace");
        
        if (worldspaceContainer == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find UI/HUD/WorldSpace in the scene!", "OK");
            return;
        }
        
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("worldspaceUIContainer").objectReferenceValue = worldspaceContainer.transform;
        so.FindProperty("spawnWorldspaceUI").boolValue = true;
        so.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(manager);
        
        Debug.Log("<color=green>✓ ChallengeManager configured with WorldSpace UI container!</color>");
        
        EditorUtility.DisplayDialog(
            "Configuration Complete",
            "ChallengeManager has been configured to use the WorldSpace UI container.\n\n" +
            "The challenge system will now display UI overlays with icons and distance for active challenges!",
            "OK");
        
        Selection.activeGameObject = challengeManagerObj;
        EditorGUIUtility.PingObject(challengeManagerObj);
    }
}
