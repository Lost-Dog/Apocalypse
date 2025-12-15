using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerXPDisplay))]
public class PlayerXPDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        PlayerXPDisplay display = (PlayerXPDisplay)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Set to Integer Display"))
        {
            Undo.RecordObject(display, "Set to Integer Display");
            SetPrivateField(display, "showAsPercentage", false);
            SetPrivateField(display, "showFraction", false);
            EditorUtility.SetDirty(display);
        }
        
        if (GUILayout.Button("Set to Fraction Display"))
        {
            Undo.RecordObject(display, "Set to Fraction Display");
            SetPrivateField(display, "showAsPercentage", false);
            SetPrivateField(display, "showFraction", true);
            EditorUtility.SetDirty(display);
        }
        
        if (GUILayout.Button("Set to Percentage Display"))
        {
            Undo.RecordObject(display, "Set to Percentage Display");
            SetPrivateField(display, "showAsPercentage", true);
            SetPrivateField(display, "showFraction", false);
            EditorUtility.SetDirty(display);
        }
    }
    
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
}
