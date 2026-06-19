using GameCreator.Editor.Common;
using GameCreator.Runtime.Toasts;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Toasts
{
    [CustomEditor(typeof(ToastUI))]
    public class ToastUIEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Text")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Icon")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Color")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Duration")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ActiveIfText")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ActiveIfIcon")));
            
            return root;
        }
    }
}