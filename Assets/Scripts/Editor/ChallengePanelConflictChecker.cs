using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChallengePanelConflictChecker : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ConflictIssue> conflicts = new List<ConflictIssue>();
    
    private class ConflictIssue
    {
        public string title;
        public string description;
        public GameObject affectedObject;
        public Component component;
        public bool isError;
        
        public ConflictIssue(string title, string desc, GameObject obj, Component comp = null, bool error = false)
        {
            this.title = title;
            this.description = desc;
            this.affectedObject = obj;
            this.component = comp;
            this.isError = error;
        }
    }
    
    [MenuItem("Division Game/Diagnostics/Check Challenge Panel Conflicts")]
    public static void ShowWindow()
    {
        ChallengePanelConflictChecker window = GetWindow<ChallengePanelConflictChecker>("Challenge Panel Conflicts");
        window.minSize = new Vector2(500, 600);
        window.RunDiagnostics();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Challenge Panel Conflict Checker", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Checks for scripts fighting to control challenge UI elements.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Run Diagnostics", GUILayout.Height(30)))
        {
            RunDiagnostics();
        }
        
        EditorGUILayout.Space(10);
        
        // Summary
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Scan Results", EditorStyles.boldLabel);
        
        int errors = conflicts.FindAll(c => c.isError).Count;
        int warnings = conflicts.Count - errors;
        
        EditorGUILayout.LabelField($"Errors: {errors}", errors > 0 ? EditorStyles.helpBox : EditorStyles.label);
        EditorGUILayout.LabelField($"Warnings: {warnings}", warnings > 0 ? EditorStyles.helpBox : EditorStyles.label);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Issues List
        if (conflicts.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (ConflictIssue issue in conflicts)
            {
                DrawIssue(issue);
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No conflicts detected! ✓", MessageType.Info);
        }
    }
    
    private void DrawIssue(ConflictIssue issue)
    {
        EditorGUILayout.BeginVertical("box");
        
        // Title
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = issue.isError ? Color.red : new Color(1f, 0.6f, 0f);
        EditorGUILayout.LabelField($"{(issue.isError ? "❌" : "⚠️")} {issue.title}", titleStyle);
        
        // Description
        EditorGUILayout.LabelField(issue.description, EditorStyles.wordWrappedLabel);
        
        // Object reference
        if (issue.affectedObject != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Object:", issue.affectedObject, typeof(GameObject), true);
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = issue.affectedObject;
                EditorGUIUtility.PingObject(issue.affectedObject);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        if (issue.component != null)
        {
            EditorGUILayout.ObjectField("Component:", issue.component, issue.component.GetType(), true);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void RunDiagnostics()
    {
        conflicts.Clear();
        
        // Find all ChallengeNotificationUI instances
        ChallengeNotificationUI[] challengeUIs = FindObjectsOfType<ChallengeNotificationUI>();
        
        if (challengeUIs.Length == 0)
        {
            conflicts.Add(new ConflictIssue(
                "No ChallengeNotificationUI Found",
                "No ChallengeNotificationUI component found in scene. The challenge panel won't display.",
                null, null, true
            ));
        }
        else if (challengeUIs.Length > 1)
        {
            conflicts.Add(new ConflictIssue(
                "Multiple ChallengeNotificationUI Components",
                $"Found {challengeUIs.Length} ChallengeNotificationUI components. Only one should exist.",
                null, null, true
            ));
            
            foreach (var ui in challengeUIs)
            {
                conflicts.Add(new ConflictIssue(
                    "Duplicate Instance",
                    "This is a duplicate ChallengeNotificationUI. Remove extras.",
                    ui.gameObject, ui, true
                ));
            }
        }
        
        // Check each ChallengeNotificationUI
        foreach (var challengeUI in challengeUIs)
        {
            GameObject panel = GetNotificationPanel(challengeUI);
            
            if (panel == null)
            {
                conflicts.Add(new ConflictIssue(
                    "Missing Notification Panel Reference",
                    "ChallengeNotificationUI has no notificationPanel assigned.",
                    challengeUI.gameObject, challengeUI, true
                ));
                continue;
            }
            
            // Check for conflicting NotificationPanel script
            NotificationPanel[] notifPanels = panel.GetComponents<NotificationPanel>();
            if (notifPanels.Length > 0)
            {
                foreach (var notifPanel in notifPanels)
                {
                    conflicts.Add(new ConflictIssue(
                        "Conflicting NotificationPanel Script",
                        "Found NotificationPanel script on challenge panel. This could interfere with ChallengeNotificationUI's control (auto-hide timer, etc.).",
                        panel, notifPanel, false
                    ));
                }
            }
            
            // Check for multiple scripts updating same UI
            MonoBehaviour[] allScripts = panel.GetComponents<MonoBehaviour>();
            int uiControllers = 0;
            foreach (var script in allScripts)
            {
                string typeName = script.GetType().Name;
                if (typeName.Contains("Notification") || typeName.Contains("Challenge") || typeName.Contains("Panel"))
                {
                    uiControllers++;
                }
            }
            
            if (uiControllers > 1)
            {
                conflicts.Add(new ConflictIssue(
                    "Multiple UI Controllers",
                    $"Found {uiControllers} scripts that might control this panel. Check for conflicts.",
                    panel, null, false
                ));
            }
            
            // Check CanvasGroup alpha control
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Check if anything else modifies CanvasGroup
                Animator animator = panel.GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    conflicts.Add(new ConflictIssue(
                        "Animator on Panel",
                        "Panel has an Animator. Make sure it doesn't conflict with ChallengeNotificationUI's fade control.",
                        panel, animator, false
                    ));
                }
            }
            
            // Check for Update() methods that might interfere
            CheckForUpdateConflicts(panel, challengeUI);
        }
        
        // Check HUDManager setup
        GameObject hudManagerObj = GameObject.Find("HUDManager");
        if (hudManagerObj == null)
        {
            conflicts.Add(new ConflictIssue(
                "HUDManager Not Found",
                "HUDManager object not found. ChallengeNotificationUI won't be initialized.",
                null, null, true
            ));
        }
        
        Repaint();
    }
    
    private GameObject GetNotificationPanel(ChallengeNotificationUI challengeUI)
    {
        SerializedObject so = new SerializedObject(challengeUI);
        SerializedProperty panelProp = so.FindProperty("notificationPanel");
        
        if (panelProp != null && panelProp.objectReferenceValue != null)
        {
            return panelProp.objectReferenceValue as GameObject;
        }
        
        return null;
    }
    
    private void CheckForUpdateConflicts(GameObject panel, ChallengeNotificationUI challengeUI)
    {
        MonoBehaviour[] scripts = panel.GetComponentsInParent<MonoBehaviour>();
        
        foreach (var script in scripts)
        {
            if (script == challengeUI) continue;
            
            System.Type type = script.GetType();
            var updateMethod = type.GetMethod("Update", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance);
            
            if (updateMethod != null && updateMethod.DeclaringType == type)
            {
                conflicts.Add(new ConflictIssue(
                    "Script with Update() Method",
                    $"{type.Name} has Update() method that could interfere with challenge panel updates.",
                    panel, script, false
                ));
            }
        }
    }
}
