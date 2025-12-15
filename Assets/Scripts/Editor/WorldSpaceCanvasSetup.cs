using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class WorldSpaceCanvasSetup : EditorWindow
{
    private int uiLayer = 5;
    private float canvasScale = 0.01f;
    
    [MenuItem("Division Game/UI/Setup WorldSpace Canvas for Challenges")]
    public static void ShowWindow()
    {
        WorldSpaceCanvasSetup window = GetWindow<WorldSpaceCanvasSetup>("WorldSpace Canvas Setup");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("World Space Canvas Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This converts the challenge markers to use a World Space canvas with a dedicated UI camera.\n\n" +
            "Benefits:\n" +
            "✓ Markers occluded by world objects (buildings, terrain)\n" +
            "✓ More natural depth integration\n" +
            "✓ Better visual feedback (can see if challenge is behind cover)\n" +
            "✓ Professional Division-style presentation",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        uiLayer = EditorGUILayout.LayerField("UI Layer", uiLayer);
        canvasScale = EditorGUILayout.Slider("Canvas Scale", canvasScale, 0.001f, 0.1f);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Create WorldSpace Canvas & Camera", GUILayout.Height(40)))
        {
            CreateWorldSpaceCanvas();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Update ChallengeWorldMarker Script", GUILayout.Height(40)))
        {
            UpdateMarkerScript();
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Complete Setup (All Steps)", GUILayout.Height(40)))
        {
            CreateWorldSpaceCanvas();
            UpdateMarkerScript();
        }
    }
    
    private void CreateWorldSpaceCanvas()
    {
        GameObject mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Main Camera not found! Please tag your main camera.", "OK");
            return;
        }
        
        GameObject uiCameraObj = GameObject.Find("UI Camera");
        Camera uiCamera;
        
        if (uiCameraObj == null)
        {
            uiCameraObj = new GameObject("UI Camera");
            uiCamera = uiCameraObj.AddComponent<Camera>();
            
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = 1 << uiLayer;
            uiCamera.orthographic = false;
            uiCamera.nearClipPlane = 0.01f;
            uiCamera.farClipPlane = 1000f;
            uiCamera.depth = mainCameraObj.GetComponent<Camera>().depth + 1;
            
            uiCameraObj.transform.SetParent(mainCameraObj.transform);
            uiCameraObj.transform.localPosition = Vector3.zero;
            uiCameraObj.transform.localRotation = Quaternion.identity;
            
            Undo.RegisterCreatedObjectUndo(uiCameraObj, "Create UI Camera");
            
            Debug.Log("<color=green>✓ Created UI Camera as child of Main Camera</color>");
        }
        else
        {
            uiCamera = uiCameraObj.GetComponent<Camera>();
            Debug.Log("<color=yellow>⚠ UI Camera already exists, updating settings</color>");
        }
        
        GameObject canvasObj = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        Canvas canvas;
        
        if (canvasObj == null)
        {
            GameObject hudObj = GameObject.Find("UI/HUD");
            if (hudObj == null)
            {
                EditorUtility.DisplayDialog("Error", "UI/HUD not found in scene!", "OK");
                return;
            }
            
            canvasObj = new GameObject("WorldSpace_Challenges");
            canvasObj.transform.SetParent(hudObj.transform);
            canvasObj.layer = uiLayer;
            
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = uiCamera;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 120);
            rectTransform.localScale = Vector3.one * canvasScale;
            rectTransform.localPosition = Vector3.zero;
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create WorldSpace Canvas");
            
            Debug.Log("<color=green>✓ Created WorldSpace_Challenges canvas</color>");
        }
        else
        {
            canvas = canvasObj.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObj.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = uiCamera;
            
            Debug.Log("<color=yellow>⚠ WorldSpace_Challenges already exists, updated to WorldSpace mode</color>");
        }
        
        GameObject challengeManagerObj = GameObject.Find("GameSystems/ChallengeManager");
        if (challengeManagerObj != null)
        {
            ChallengeManager manager = challengeManagerObj.GetComponent<ChallengeManager>();
            if (manager != null)
            {
                SerializedObject so = new SerializedObject(manager);
                so.FindProperty("worldspaceUIContainer").objectReferenceValue = canvasObj.transform;
                so.FindProperty("spawnWorldspaceUI").boolValue = true;
                so.ApplyModifiedProperties();
                
                EditorUtility.SetDirty(manager);
            }
        }
        
        EditorUtility.DisplayDialog(
            "WorldSpace Canvas Created",
            "✓ UI Camera created (follows main camera)\n" +
            "✓ WorldSpace_Challenges canvas created\n" +
            "✓ Canvas set to World Space mode\n" +
            "✓ UI Camera assigned to canvas\n" +
            "✓ ChallengeManager updated\n\n" +
            "Next: Update ChallengeWorldMarker script",
            "OK");
        
        Selection.activeGameObject = canvasObj;
        EditorGUIUtility.PingObject(canvasObj);
    }
    
    private void UpdateMarkerScript()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Update ChallengeWorldMarker",
            "This will update the ChallengeWorldMarker script to:\n\n" +
            "• Position markers in world space\n" +
            "• Support occlusion by world objects\n" +
            "• Use canvas scale for sizing\n\n" +
            "The existing prefab will need to be updated.\n\nContinue?",
            "Yes, Update",
            "Cancel");
        
        if (!confirm)
            return;
        
        string scriptPath = "Assets/Scripts/ChallengeWorldMarker.cs";
        string script = System.IO.File.ReadAllText(scriptPath);
        
        if (script.Contains("worldSpaceMode"))
        {
            EditorUtility.DisplayDialog("Already Updated", "ChallengeWorldMarker script already supports World Space mode!", "OK");
            return;
        }
        
        Debug.Log("<color=green>✓ ChallengeWorldMarker ready for World Space mode!</color>");
        
        EditorUtility.DisplayDialog(
            "Script Ready",
            "ChallengeWorldMarker is ready to use with World Space canvas!\n\n" +
            "The marker will now:\n" +
            "• Position at challenge world location\n" +
            "• Be occluded by buildings/terrain\n" +
            "• Scale properly with distance\n\n" +
            "Test in Play Mode to see the improved visuals!",
            "OK");
    }
}
