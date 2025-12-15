using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NotificationPanel))]
public class NotificationPanelEditor : Editor
{
    private string testMessage = "Test Notification Message";
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        NotificationPanel panel = (NotificationPanel)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            testMessage = EditorGUILayout.TextField("Test Message:", testMessage);
            
            if (GUILayout.Button("Show Test Notification"))
            {
                panel.ShowNotification(testMessage);
            }
            
            if (GUILayout.Button("Hide Notification"))
            {
                panel.HideNotification();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test notifications", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Setup Helper", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Find Message Text"))
        {
            Undo.RecordObject(panel, "Auto-Find Message Text");
            
            TMPro.TextMeshProUGUI[] texts = panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            if (texts.Length > 0)
            {
                SerializedObject so = new SerializedObject(panel);
                so.FindProperty("messageText").objectReferenceValue = texts[0];
                so.ApplyModifiedProperties();
                
                Debug.Log($"Found and assigned TextMeshProUGUI: {texts[0].name}");
            }
            else
            {
                Debug.LogWarning("No TextMeshProUGUI component found in children!");
            }
        }
        
        if (GUILayout.Button("Add Audio Source"))
        {
            if (panel.GetComponent<AudioSource>() == null)
            {
                Undo.AddComponent<AudioSource>(panel.gameObject);
                Debug.Log("Added AudioSource component");
            }
            else
            {
                Debug.Log("AudioSource already exists");
            }
        }
    }
}
