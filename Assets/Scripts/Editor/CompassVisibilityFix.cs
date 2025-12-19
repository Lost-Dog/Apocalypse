using UnityEngine;
using UnityEditor;

public class CompassVisibilityFix : EditorWindow
{
    [MenuItem("Division Game/UI/Fix Compass Visibility")]
    public static void ShowWindow()
    {
        GetWindow<CompassVisibilityFix>("Compass Fix");
    }

    private void OnGUI()
    {
        GUILayout.Label("Compass Visibility Diagnostic", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(30)))
        {
            RunDiagnostic();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Enable Compass Now", GUILayout.Height(30)))
        {
            EnableCompass();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fix Camera Reference", GUILayout.Height(30)))
        {
            FixCameraReference();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fix Compass Element Reference", GUILayout.Height(30)))
        {
            FixCompassElementReference();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fix All Compass Issues", GUILayout.Height(40)))
        {
            FixAllIssues();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("If compass still doesn't move in Play Mode, add CompassDebugger component to see live status in Console.", MessageType.Info);

        if (GUILayout.Button("Add Runtime Debugger Component", GUILayout.Height(30)))
        {
            AddDebugger();
        }
    }

    private void RunDiagnostic()
    {
        Debug.Log("=== COMPASS DIAGNOSTIC ===");

        // Find compass GameObject
        GameObject compass = GameObject.Find("HUD_Apocalypse_Compass_01");
        if (compass == null)
        {
            Debug.LogError("❌ Compass GameObject 'HUD_Apocalypse_Compass_01' not found in scene!");
            return;
        }

        Debug.Log($"✅ Compass GameObject found: {compass.name}");
        Debug.Log($"   - Active in hierarchy: {compass.activeInHierarchy}");
        Debug.Log($"   - Active self: {compass.activeSelf}");
        Debug.Log($"   - Path: {GetGameObjectPath(compass)}");

        // Check Compass script
        Compass compassScript = compass.GetComponent<Compass>();
        if (compassScript == null)
        {
            Debug.LogError("❌ Compass script component not found!");
        }
        else
        {
            Debug.Log($"✅ Compass script found");
            Debug.Log($"   - Enabled: {compassScript.enabled}");
            Debug.Log($"   - View Direction: {(compassScript.viewDirection != null ? compassScript.viewDirection.name : "NULL ❌")}");
            Debug.Log($"   - Compass Element: {(compassScript.compassElement != null ? compassScript.compassElement.name : "NULL ❌")}");
            Debug.Log($"   - Compass Size: {compassScript.compassSize}");

            if (compassScript.viewDirection == null)
            {
                Debug.LogError("⚠️ View Direction (camera) reference is missing! Compass won't move!");
            }
            
            if (compassScript.compassElement == null)
            {
                Debug.LogError("⚠️ Compass Element reference is missing! Compass won't move!");
            }
            
            if (compassScript.compassSize == 0)
            {
                Debug.LogError("⚠️ Compass Size is 0! Compass movement will be invisible!");
            }
        }

        // Check HUDManager settings
        HUDManager hudManager = FindFirstObjectByType<HUDManager>();
        if (hudManager == null)
        {
            Debug.LogError("❌ HUDManager not found!");
        }
        else
        {
            Debug.Log($"✅ HUDManager found");
            
            // Use reflection to check private fields
            var compassPanelField = typeof(HUDManager).GetField("compassPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var showCompassField = typeof(HUDManager).GetField("showCompassOnStart", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (compassPanelField != null)
            {
                GameObject compassPanel = compassPanelField.GetValue(hudManager) as GameObject;
                Debug.Log($"   - Compass Panel Reference: {(compassPanel != null ? compassPanel.name : "NULL")}");
            }

            if (showCompassField != null)
            {
                bool showOnStart = (bool)showCompassField.GetValue(hudManager);
                Debug.Log($"   - Show Compass On Start: {showOnStart}");
            }
        }

        Debug.Log("=== END DIAGNOSTIC ===");
    }

    private void EnableCompass()
    {
        GameObject compass = GameObject.Find("HUD_Apocalypse_Compass_01");
        if (compass == null)
        {
            Debug.LogError("❌ Compass GameObject not found!");
            EditorUtility.DisplayDialog("Error", "Compass GameObject 'HUD_Apocalypse_Compass_01' not found in scene!", "OK");
            return;
        }

        if (!compass.activeSelf)
        {
            compass.SetActive(true);
            EditorUtility.SetDirty(compass);
            Debug.Log("✅ Compass enabled!");
            EditorUtility.DisplayDialog("Success", "Compass has been enabled! Save the scene to keep this change.", "OK");
        }
        else
        {
            Debug.Log("ℹ️ Compass is already enabled");
            EditorUtility.DisplayDialog("Info", "Compass is already enabled. If it's not visible, check:\n\n1. Camera reference in Compass script\n2. Parent GameObjects are active\n3. Canvas is rendering properly", "OK");
        }
    }

    private void FixCameraReference()
    {
        GameObject compass = GameObject.Find("HUD_Apocalypse_Compass_01");
        if (compass == null)
        {
            Debug.LogError("❌ Compass GameObject not found!");
            return;
        }

        Compass compassScript = compass.GetComponent<Compass>();
        if (compassScript == null)
        {
            Debug.LogError("❌ Compass script not found!");
            return;
        }

        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        if (mainCamera == null)
        {
            Debug.LogError("❌ No camera found in scene!");
            EditorUtility.DisplayDialog("Error", "No camera found in the scene!", "OK");
            return;
        }

        // Assign camera transform to viewDirection
        compassScript.viewDirection = mainCamera.transform;
        EditorUtility.SetDirty(compass);

        Debug.Log($"✅ Camera reference fixed! Assigned {mainCamera.name} to Compass.viewDirection");
        EditorUtility.DisplayDialog("Success", $"Camera '{mainCamera.name}' has been assigned to the Compass!\n\nSave the scene to keep this change.", "OK");
    }

    private void FixCompassElementReference()
    {
        GameObject compass = GameObject.Find("HUD_Apocalypse_Compass_01");
        if (compass == null)
        {
            Debug.LogError("❌ Compass GameObject not found!");
            return;
        }

        Compass compassScript = compass.GetComponent<Compass>();
        if (compassScript == null)
        {
            Debug.LogError("❌ Compass script not found!");
            return;
        }

        // Try to find the compass element - usually named "CompassPoints" or similar
        string[] possibleNames = new string[] 
        { 
            "CompassPoints", 
            "Compass_Content", 
            "Content", 
            "CompassElement",
            "Compass Points"
        };

        RectTransform foundElement = null;

        // First, search in children
        foreach (string name in possibleNames)
        {
            Transform child = compass.transform.Find(name);
            if (child == null)
            {
                // Try recursive search
                child = FindChildRecursive(compass.transform, name);
            }

            if (child != null)
            {
                foundElement = child.GetComponent<RectTransform>();
                if (foundElement != null)
                {
                    Debug.Log($"Found compass element: {name}");
                    break;
                }
            }
        }

        // If not found by name, try to find any RectTransform child that looks like compass content
        if (foundElement == null)
        {
            // Look for a child with "Content" or "Mask" in hierarchy
            Transform mask = compass.transform.Find("Content/Mask");
            if (mask != null)
            {
                foreach (Transform child in mask)
                {
                    if (child.name.Contains("Compass") && !child.name.Contains("Icon"))
                    {
                        foundElement = child.GetComponent<RectTransform>();
                        if (foundElement != null)
                        {
                            Debug.Log($"Found compass element in mask: {child.name}");
                            break;
                        }
                    }
                }
            }
        }

        if (foundElement == null)
        {
            Debug.LogError("❌ Could not find compass element! Tried: " + string.Join(", ", possibleNames));
            EditorUtility.DisplayDialog("Error", 
                "Could not automatically find the compass element RectTransform.\n\n" +
                "Please manually assign it in the Inspector:\n" +
                "1. Select HUD_Apocalypse_Compass_01\n" +
                "2. Find the Compass component\n" +
                "3. Assign the 'CompassPoints' child to 'Compass Element'", "OK");
            return;
        }

        // Assign the found element
        compassScript.compassElement = foundElement;
        EditorUtility.SetDirty(compass);

        Debug.Log($"✅ Compass element reference fixed! Assigned '{foundElement.name}' to Compass.compassElement");
        EditorUtility.DisplayDialog("Success", 
            $"Compass element '{foundElement.name}' has been assigned!\n\nSave the scene to keep this change.", "OK");
    }

    private void FixAllIssues()
    {
        Debug.Log("=== FIXING ALL COMPASS ISSUES ===");

        bool success = true;

        // 1. Enable compass
        GameObject compass = GameObject.Find("HUD_Apocalypse_Compass_01");
        if (compass != null && !compass.activeSelf)
        {
            compass.SetActive(true);
            EditorUtility.SetDirty(compass);
            Debug.Log("✅ Enabled compass GameObject");
        }

        // 2. Fix camera reference
        if (compass != null)
        {
            Compass compassScript = compass.GetComponent<Compass>();
            if (compassScript != null)
            {
                if (compassScript.viewDirection == null)
                {
                    Camera mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
                    if (mainCamera != null)
                    {
                        compassScript.viewDirection = mainCamera.transform;
                        EditorUtility.SetDirty(compass);
                        Debug.Log($"✅ Fixed camera reference: {mainCamera.name}");
                    }
                    else
                    {
                        Debug.LogError("❌ No camera found in scene!");
                        success = false;
                    }
                }

                // 3. Fix compass element if needed
                if (compassScript.compassElement == null)
                {
                    FixCompassElementReference();
                }

                // 4. Ensure compassSize is valid
                if (compassScript.compassSize == 0)
                {
                    compassScript.compassSize = 2300f;
                    EditorUtility.SetDirty(compass);
                    Debug.Log("✅ Set compassSize to default 2300");
                }
            }
        }
        else
        {
            Debug.LogError("❌ Compass GameObject not found!");
            success = false;
        }

        Debug.Log("=== FIX COMPLETE ===");

        if (success)
        {
            EditorUtility.DisplayDialog("Success", 
                "All compass issues have been fixed!\n\n" +
                "The compass should now work correctly.\n\n" +
                "Don't forget to SAVE THE SCENE!", "OK");
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    private void AddDebugger()
    {
        GameObject compass = GameObject.Find("HUD_Apocalypse_Compass_01");
        if (compass == null)
        {
            EditorUtility.DisplayDialog("Error", "Compass GameObject 'HUD_Apocalypse_Compass_01' not found!", "OK");
            return;
        }

        CompassDebugger debugger = compass.GetComponent<CompassDebugger>();
        if (debugger != null)
        {
            Debug.Log("CompassDebugger already exists on compass");
            EditorUtility.DisplayDialog("Info", "CompassDebugger component already exists.\n\nEnter Play Mode and check the Console for live compass status.", "OK");
            return;
        }

        compass.AddComponent<CompassDebugger>();
        EditorUtility.SetDirty(compass);
        
        Debug.Log("✅ Added CompassDebugger component");
        EditorUtility.DisplayDialog("Success", 
            "CompassDebugger component added!\n\n" +
            "Enter Play Mode and watch the Console.\n" +
            "It will show compass status every 2 seconds and indicate if it's moving.\n\n" +
            "Don't forget to save the scene!", "OK");
    }
}
