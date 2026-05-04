using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Awareness))]
public class AwarenessEditor : Editor
{
    SerializedProperty alertRangeProp;

    private void OnEnable()
    {
        alertRangeProp = serializedObject.FindProperty("alertRange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(alertRangeProp, new GUIContent("Alert Range"));
        serializedObject.ApplyModifiedProperties();
    }
}
