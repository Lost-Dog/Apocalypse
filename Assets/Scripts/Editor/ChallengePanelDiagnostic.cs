using UnityEngine;
using UnityEditor;

public class ChallengePanelDiagnostic : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Diagnose Challenge Panel")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChallengePanelDiagnostic>("Challenge Panel Diagnostic");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Challenge Panel Diagnostic", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(40)))
        {
            RunDiagnostic();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Enable Challenge Panel", GUILayout.Height(30)))
        {
            EnableChallengePanel();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fix All References", GUILayout.Height(30)))
        {
            FixAllReferences();
        }
    }

    private void RunDiagnostic()
    {
        Debug.Log("=== CHALLENGE PANEL DIAGNOSTIC ===");

        // 1. Find ChallengeNotificationUI
        ChallengeNotificationUI notificationUI = FindFirstObjectByType<ChallengeNotificationUI>();
        if (notificationUI == null)
        {
            Debug.LogError("❌ ChallengeNotificationUI component not found in scene!");
            EditorUtility.DisplayDialog("Error", 
                "ChallengeNotificationUI component not found!\n\n" +
                "The challenge panel needs this component to work.\n\n" +
                "Add it to your UI hierarchy.", "OK");
            return;
        }

        Debug.Log($"✅ ChallengeNotificationUI found on: {notificationUI.gameObject.name}");

        // 2. Check notification panel reference
        var notificationPanelField = typeof(ChallengeNotificationUI).GetField("notificationPanel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (notificationPanelField != null)
        {
            GameObject notificationPanel = notificationPanelField.GetValue(notificationUI) as GameObject;
            if (notificationPanel == null)
            {
                Debug.LogError("❌ notificationPanel is NULL in ChallengeNotificationUI!");
            }
            else
            {
                Debug.Log($"✅ Notification Panel: {notificationPanel.name}");
                Debug.Log($"   - Active: {notificationPanel.activeSelf}");
                Debug.Log($"   - Active in Hierarchy: {notificationPanel.activeInHierarchy}");
                
                if (!notificationPanel.activeInHierarchy)
                {
                    Debug.LogWarning("⚠️ Panel is inactive in hierarchy - check parent GameObjects!");
                }
            }
        }

        // 3. Check HUDManager reference
        HUDManager hudManager = FindFirstObjectByType<HUDManager>();
        if (hudManager == null)
        {
            Debug.LogError("❌ HUDManager not found!");
        }
        else
        {
            Debug.Log($"✅ HUDManager found");
            
            var challengeNotificationUIField = typeof(HUDManager).GetField("challengeNotificationUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (challengeNotificationUIField != null)
            {
                ChallengeNotificationUI hudReference = challengeNotificationUIField.GetValue(hudManager) as ChallengeNotificationUI;
                if (hudReference == null)
                {
                    Debug.LogError("❌ ChallengeNotificationUI not assigned in HUDManager!");
                }
                else if (hudReference != notificationUI)
                {
                    Debug.LogWarning("⚠️ HUDManager references a different ChallengeNotificationUI instance!");
                }
                else
                {
                    Debug.Log("✅ HUDManager correctly references ChallengeNotificationUI");
                }
            }
        }

        // 4. Check ChallengeManager
        ChallengeManager challengeManager = ChallengeManager.Instance;
        if (challengeManager == null)
        {
            Debug.LogWarning("⚠️ ChallengeManager.Instance is null (normal in Edit mode)");
        }
        else
        {
            Debug.Log("✅ ChallengeManager found");
        }

        // 5. Check for common UI elements
        var titleTextField = typeof(ChallengeNotificationUI).GetField("titleText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (titleTextField != null)
        {
            var titleText = titleTextField.GetValue(notificationUI);
            Debug.Log($"   - Title Text: {(titleText != null ? "Assigned" : "NULL ❌")}");
        }

        Debug.Log("=== DIAGNOSTIC COMPLETE ===");
    }

    private void EnableChallengePanel()
    {
        ChallengeNotificationUI notificationUI = FindFirstObjectByType<ChallengeNotificationUI>();
        if (notificationUI == null)
        {
            EditorUtility.DisplayDialog("Error", "ChallengeNotificationUI not found!", "OK");
            return;
        }

        var notificationPanelField = typeof(ChallengeNotificationUI).GetField("notificationPanel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (notificationPanelField != null)
        {
            GameObject notificationPanel = notificationPanelField.GetValue(notificationUI) as GameObject;
            if (notificationPanel == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Notification Panel reference is NULL!\n\n" +
                    "Assign it in the ChallengeNotificationUI Inspector.", "OK");
                Selection.activeGameObject = notificationUI.gameObject;
                return;
            }

            // Enable the panel and all parents
            Transform current = notificationPanel.transform;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    Debug.Log($"Enabling: {current.name}");
                    current.gameObject.SetActive(true);
                    EditorUtility.SetDirty(current.gameObject);
                }
                current = current.parent;
            }

            Debug.Log("✅ Challenge panel enabled!");
            EditorUtility.DisplayDialog("Success", 
                "Challenge panel has been enabled!\n\n" +
                "Save the scene to keep this change.", "OK");
        }
    }

    private void FixAllReferences()
    {
        Debug.Log("=== FIXING CHALLENGE PANEL REFERENCES ===");

        // Find components
        ChallengeNotificationUI notificationUI = FindFirstObjectByType<ChallengeNotificationUI>();
        HUDManager hudManager = FindFirstObjectByType<HUDManager>();

        if (notificationUI == null)
        {
            Debug.LogError("❌ ChallengeNotificationUI not found!");
            EditorUtility.DisplayDialog("Error", "ChallengeNotificationUI not found in scene!", "OK");
            return;
        }

        if (hudManager == null)
        {
            Debug.LogError("❌ HUDManager not found!");
            EditorUtility.DisplayDialog("Error", "HUDManager not found in scene!", "OK");
            return;
        }

        // Fix HUDManager reference
        var challengeNotificationUIField = typeof(HUDManager).GetField("challengeNotificationUI", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (challengeNotificationUIField != null)
        {
            challengeNotificationUIField.SetValue(hudManager, notificationUI);
            EditorUtility.SetDirty(hudManager.gameObject);
            Debug.Log("✅ Fixed HUDManager reference to ChallengeNotificationUI");
        }

        // Check and enable notification panel
        var notificationPanelField = typeof(ChallengeNotificationUI).GetField("notificationPanel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (notificationPanelField != null)
        {
            GameObject notificationPanel = notificationPanelField.GetValue(notificationUI) as GameObject;
            if (notificationPanel != null && !notificationPanel.activeInHierarchy)
            {
                notificationPanel.SetActive(true);
                EditorUtility.SetDirty(notificationPanel);
                Debug.Log("✅ Enabled notification panel");
            }
        }

        Debug.Log("=== FIX COMPLETE ===");
        EditorUtility.DisplayDialog("Success", 
            "All references fixed!\n\n" +
            "Don't forget to save the scene!", "OK");
    }
}
