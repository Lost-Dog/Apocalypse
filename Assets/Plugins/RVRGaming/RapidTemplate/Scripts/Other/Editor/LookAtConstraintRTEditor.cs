using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using GameCreator.Editor.Common;
using UnityEngine.UIElements;

[CustomEditor(typeof(LookAtConstraintRT))]
public class MyScriptEditorLookAt : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        root.Add(new PropertyField(serializedObject.FindProperty("targetObject")));

        root.Add(new PropertyField(serializedObject.FindProperty("rotationSpeed")));

        root.Add(new PropertyField(serializedObject.FindProperty("freezeX")));
        root.Add(new PropertyField(serializedObject.FindProperty("freezeY")));
        root.Add(new PropertyField(serializedObject.FindProperty("freezeZ")));

        return root;
    }
}
