using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ChallengeSystemFixer : EditorWindow
{
    [MenuItem("Division Game/Fix Challenge Markers")]
    public static void FixChallengeMarkers()
    {
        Debug.Log("<color=cyan>===== Starting Challenge Marker System Fix =====</color>");
        GameObject gameSystemsObj = GameObject.Find("GameSystems");
        if (gameSystemsObj == null)
        {
            Debug.LogError("Cannot find 'GameSystems' GameObject in the scene!");
            return;
        }

        Transform challengeSpawner = gameSystemsObj.transform.Find("ChallengeSpawner");
        if (challengeSpawner == null)
        {
            GameObject spawnerObj = new GameObject("ChallengeSpawner");
            spawnerObj.transform.SetParent(gameSystemsObj.transform);
            spawnerObj.AddComponent<ChallengeSpawner>();
            
            Debug.Log("<color=green>✓ Created ChallengeSpawner GameObject</color>");
            EditorUtility.SetDirty(spawnerObj);
        }
        else
        {
            ChallengeSpawner spawner = challengeSpawner.GetComponent<ChallengeSpawner>();
            if (spawner == null)
            {
                challengeSpawner.gameObject.AddComponent<ChallengeSpawner>();
                Debug.Log("<color=green>✓ Added ChallengeSpawner component</color>");
            }
            else
            {
                Debug.Log("<color=yellow>ChallengeSpawner already exists and is configured</color>");
            }
        }

        Transform uiHud = GameObject.Find("HUD/UI/HUD/ScreenSpace")?.transform;
        if (uiHud != null)
        {
            Transform challengeMarkersContainer = uiHud.Find("ChallengeMarkers");
            if (challengeMarkersContainer == null)
            {
                GameObject markerContainer = new GameObject("ChallengeMarkers");
                markerContainer.transform.SetParent(uiHud);
                markerContainer.layer = LayerMask.NameToLayer("UI");
                
                RectTransform rect = markerContainer.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                
                Debug.Log("<color=green>✓ Created ScreenSpace ChallengeMarkers container</color>");
                EditorUtility.SetDirty(markerContainer);
            }
            else
            {
                Debug.Log("<color=green>✓ ScreenSpace ChallengeMarkers container already exists</color>");
            }
        }

        Transform compassContent = GameObject.Find("HUD/UI/HUD/ScreenSpace/Top/HUD_Apocalypse_Compass_01/Content/Compass_Content")?.transform;
        if (compassContent != null)
        {
            Transform challengeCompassContainer = compassContent.Find("Challenge_Markers");
            if (challengeCompassContainer == null)
            {
                GameObject compassMarkers = new GameObject("Challenge_Markers");
                compassMarkers.transform.SetParent(compassContent);
                
                RectTransform rect = compassMarkers.AddComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(1000f, 100f);
                
                Debug.Log("<color=green>✓ Created Challenge_Markers compass container</color>");
                EditorUtility.SetDirty(compassMarkers);
            }
        }

        GameObject challengeManagerObj = GameObject.Find("GameSystems/ChallengeManager");
        if (challengeManagerObj != null)
        {
            ChallengeManager manager = challengeManagerObj.GetComponent<ChallengeManager>();
            if (manager != null)
            {
                SerializedObject so = new SerializedObject(manager);
                
                Transform screenSpaceContainer = GameObject.Find("HUD/UI/HUD/ScreenSpace/ChallengeMarkers")?.transform;
                if (screenSpaceContainer != null)
                {
                    so.FindProperty("worldspaceUIContainer").objectReferenceValue = screenSpaceContainer;
                    Debug.Log("<color=green>✓ Assigned worldspaceUIContainer to ScreenSpace/ChallengeMarkers</color>");
                }
                else
                {
                    Debug.LogWarning("ScreenSpace ChallengeMarkers container not found!");
                }
                
                Transform compassContainer = GameObject.Find("HUD/UI/HUD/ScreenSpace/Top/HUD_Apocalypse_Compass_01/Content/Compass_Content/Challenge_Markers")?.transform;
                if (compassContainer != null)
                {
                    so.FindProperty("compassMarkerContainer").objectReferenceValue = compassContainer;
                    Debug.Log("<color=green>✓ Assigned compassMarkerContainer to Challenge_Markers</color>");
                }
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(manager);
            }
        }

        VerifyScreenSpaceCanvasSetup();
        ConfigureMarkerPrefabForScreenSpace();

        Debug.Log("<color=cyan>===== Challenge Marker System Fix Complete =====</color>");
        Debug.Log("Challenge markers will now appear in Screen Space UI!");
        Debug.Log("Enter Play Mode to test - challenges spawn every 60 seconds by default.");
    }

    private static void ConfigureMarkerPrefabForScreenSpace()
    {
        string prefabPath = "Assets/Prefabs/ChallengeWorldMarker.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogWarning("ChallengeWorldMarker prefab not found!");
            return;
        }
        
        ChallengeWorldMarker marker = prefab.GetComponent<ChallengeWorldMarker>();
        if (marker == null)
        {
            Debug.LogWarning("ChallengeWorldMarker component not found on prefab!");
            return;
        }
        
        SerializedObject so = new SerializedObject(marker);
        
        so.FindProperty("worldSpaceMode").boolValue = false;
        so.FindProperty("scaleWithDistance").boolValue = false;
        so.FindProperty("edgePadding").floatValue = 100f;
        so.FindProperty("minVisibleDistance").floatValue = 10f;
        so.FindProperty("maxVisibleDistance").floatValue = 800f;
        so.FindProperty("baseScale").floatValue = 1f;
        
        so.ApplyModifiedProperties();
        
        RectTransform rect = prefab.GetComponent<RectTransform>();
        if (rect != null)
        {
            SerializedObject rectSo = new SerializedObject(rect);
            rectSo.FindProperty("m_LocalScale").vector3Value = Vector3.one;
            rectSo.FindProperty("m_SizeDelta").vector2Value = new Vector2(120f, 120f);
            rectSo.ApplyModifiedProperties();
        }
        
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=green>✓ ChallengeWorldMarker prefab configured for screen-space mode</color>");
    }

    private static void VerifyScreenSpaceCanvasSetup()
    {
        GameObject screenSpaceObj = GameObject.Find("HUD/UI/HUD/ScreenSpace");
        if (screenSpaceObj == null)
        {
            Debug.LogError("ScreenSpace UI not found! Cannot configure screen-space markers.");
            return;
        }

        Transform challengeMarkersContainer = screenSpaceObj.transform.Find("ChallengeMarkers");
        if (challengeMarkersContainer != null)
        {
            RectTransform rect = challengeMarkersContainer.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                
                EditorUtility.SetDirty(challengeMarkersContainer.gameObject);
                Debug.Log("<color=green>✓ Screen-space ChallengeMarkers container configured</color>");
            }
        }
        
        Canvas parentCanvas = screenSpaceObj.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Debug.Log($"<color=cyan>ℹ Parent canvas render mode: {parentCanvas.renderMode}</color>");
        }
    }

    private static void VerifyWorldSpaceCanvasSetup()
    {
        GameObject worldSpaceCanvas = GameObject.Find("HUD/WorldSpace_Challenges");
        if (worldSpaceCanvas == null)
        {
            Debug.LogWarning("WorldSpace_Challenges canvas not found! Creating it now...");
            
            GameObject hudRoot = GameObject.Find("HUD");
            if (hudRoot != null)
            {
                GameObject canvasObj = new GameObject("WorldSpace_Challenges");
                canvasObj.transform.SetParent(hudRoot.transform);
                
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    canvas.worldCamera = mainCam;
                    Debug.Log("<color=green>✓ Created WorldSpace_Challenges canvas with Main Camera assigned</color>");
                }
                
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                RectTransform rect = canvasObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(1920, 1080);
                rect.localScale = Vector3.one * 0.01f;
                
                canvasObj.layer = LayerMask.NameToLayer("UI");
                
                EditorUtility.SetDirty(canvasObj);
                
                Debug.Log("<color=green>✓ WorldSpace_Challenges canvas created and configured</color>");
            }
            else
            {
                Debug.LogError("HUD root GameObject not found! Cannot create WorldSpace_Challenges.");
            }
        }
        else
        {
            Canvas canvas = worldSpaceCanvas.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("WorldSpace_Challenges exists but has no Canvas component!");
                return;
            }
            
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                Debug.Log("<color=yellow>⚠ Changed WorldSpace_Challenges to WorldSpace render mode</color>");
            }
            
            if (canvas.worldCamera == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    canvas.worldCamera = mainCam;
                    Debug.Log("<color=yellow>⚠ Assigned Main Camera to WorldSpace_Challenges</color>");
                    EditorUtility.SetDirty(canvas);
                }
            }
            
            Debug.Log("<color=green>✓ WorldSpace_Challenges canvas verified</color>");
        }
    }
}
