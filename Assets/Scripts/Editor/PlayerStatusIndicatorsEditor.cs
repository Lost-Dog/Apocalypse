using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerStatusIndicators))]
public class PlayerStatusIndicatorsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        PlayerStatusIndicators indicators = (PlayerStatusIndicators)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Setup Helper", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Audio Source"))
        {
            if (indicators.GetComponent<AudioSource>() == null)
            {
                Undo.AddComponent<AudioSource>(indicators.gameObject);
                
                AudioSource source = indicators.GetComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                
                SerializedObject so = new SerializedObject(indicators);
                so.FindProperty("audioSource").objectReferenceValue = source;
                so.ApplyModifiedProperties();
                
                Debug.Log("Added and configured AudioSource component");
            }
            else
            {
                Debug.Log("AudioSource already exists");
            }
        }
        
        if (GUILayout.Button("Enable Panel Behavior"))
        {
            SerializedObject so = new SerializedObject(indicators);
            so.FindProperty("startDisabled").boolValue = true;
            so.FindProperty("autoHideWhenNoWarnings").boolValue = true;
            so.ApplyModifiedProperties();
            
            Debug.Log("Panel behavior configured: Starts disabled, auto-hides when no warnings");
        }
        
        EditorGUILayout.Space();
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            
            bool hasWarnings = indicators.HasActiveWarnings();
            EditorGUILayout.LabelField("Has Active Warnings:", hasWarnings ? "Yes" : "No");
            EditorGUILayout.LabelField("Panel Active:", indicators.gameObject.activeSelf ? "Yes" : "No");
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see runtime information", MessageType.Info);
        }
    }
}
