using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LootUIManager))]
public class LootUIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LootUIManager lootUI = (LootUIManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Setup Helper", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Audio Source"))
        {
            if (lootUI.GetComponent<AudioSource>() == null)
            {
                Undo.AddComponent<AudioSource>(lootUI.gameObject);
                
                AudioSource source = lootUI.GetComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                
                SerializedObject so = new SerializedObject(lootUI);
                so.FindProperty("audioSource").objectReferenceValue = source;
                so.ApplyModifiedProperties();
                
                Debug.Log("Added and configured AudioSource component");
            }
            else
            {
                Debug.Log("AudioSource already exists");
            }
        }
        
        if (GUILayout.Button("Enable Start Inactive"))
        {
            SerializedObject so = new SerializedObject(lootUI);
            so.FindProperty("startInactive").boolValue = true;
            so.ApplyModifiedProperties();
            
            Debug.Log("LootUIManager will now start inactive");
        }
        
        EditorGUILayout.Space();
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Panel Active:", lootUI.gameObject.activeSelf ? "Yes" : "No");
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see runtime information", MessageType.Info);
        }
    }
}
