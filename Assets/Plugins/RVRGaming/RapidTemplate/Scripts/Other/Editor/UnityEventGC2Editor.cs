using GameCreator.Editor.Common;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(UnityEventGC2))]
public class RVRScriptEditor : Editor
{
    Texture2D headerImage;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        root.Add(new SpaceSmallest());
        root.Add(new LabelTitle("Execute"));
        root.Add(new PropertyField(serializedObject.FindProperty("onRun")));
        root.Add(new SpaceSmallest());

        return root;
    }
}