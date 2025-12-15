using UnityEngine;
using UnityEditor;
using TMPro;

public class NotificationPanelSetupHelper : EditorWindow
{
    [MenuItem("Division Game/Setup/Configure Notification Panel")]
    public static void ShowWindow()
    {
        GetWindow<NotificationPanelSetupHelper>("Notification Setup");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Notification Panel Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This will configure the notification panel:\n" +
            "1. Add NotificationPanel component\n" +
            "2. Enable the panel (always visible parent)\n" +
            "3. Wire up text references\n" +
            "4. Configure to fade in/out instead of enable/disable", 
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Notification Panel", GUILayout.Height(40)))
        {
            SetupNotificationPanel();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Select Notification Panel"))
        {
            GameObject panel = GameObject.Find("HUD_Apocalypse_Comms_01");
            if (panel != null)
            {
                Selection.activeGameObject = panel;
                EditorGUIUtility.PingObject(panel);
            }
        }
        
        if (GUILayout.Button("Select NotificationManager"))
        {
            GameObject manager = GameObject.Find("NoficationManager");
            if (manager != null)
            {
                Selection.activeGameObject = manager;
                EditorGUIUtility.PingObject(manager);
            }
        }
    }
    
    private void SetupNotificationPanel()
    {
        GameObject panelGO = GameObject.Find("HUD_Apocalypse_Comms_01");
        
        if (panelGO == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'HUD_Apocalypse_Comms_01' in scene!", "OK");
            return;
        }
        
        panelGO.SetActive(true);
        
        NotificationPanel notifPanel = panelGO.GetComponent<NotificationPanel>();
        
        if (notifPanel == null)
        {
            notifPanel = panelGO.AddComponent<NotificationPanel>();
            Debug.Log("Added NotificationPanel component");
        }
        
        notifPanel.displayDuration = 5f;
        notifPanel.startDisabled = false;
        
        CanvasGroup canvasGroup = panelGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panelGO.AddComponent<CanvasGroup>();
            Debug.Log("Added CanvasGroup for fade effects");
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        Transform nameTransform = panelGO.transform.Find("Comms_Contents/Name/Label_Name");
        if (nameTransform != null)
        {
            TextMeshProUGUI textComponent = nameTransform.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                notifPanel.messageText = textComponent;
                Debug.Log("Connected message text: Label_Name");
            }
        }
        
        GameObject notificationManager = GameObject.Find("NoficationManager");
        if (notificationManager != null)
        {
            NotificationManager manager = notificationManager.GetComponent<NotificationManager>();
            if (manager != null)
            {
                manager.defaultNotificationPanel = notifPanel;
                EditorUtility.SetDirty(manager);
                Debug.Log("Connected to NotificationManager");
            }
        }
        
        EditorUtility.SetDirty(panelGO);
        EditorUtility.SetDirty(notifPanel);
        
        Debug.Log("Notification Panel setup complete!");
        EditorUtility.DisplayDialog(
            "Setup Complete", 
            "Notification Panel configured:\n\n" +
            "- Panel: Always enabled\n" +
            "- Component: NotificationPanel added\n" +
            "- Text: Connected\n" +
            "- Fade: Uses CanvasGroup alpha\n" +
            "- Duration: 5 seconds\n\n" +
            "Notifications will now show properly!", 
            "OK"
        );
        
        Selection.activeGameObject = panelGO;
    }
}
