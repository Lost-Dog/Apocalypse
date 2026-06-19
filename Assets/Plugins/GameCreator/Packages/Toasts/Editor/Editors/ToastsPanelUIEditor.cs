using GameCreator.Editor.Common;
using GameCreator.Runtime.Toasts;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Toasts
{
    [CustomEditor(typeof(ToastsPanelUI))]
    public class ToastsPanelUIEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_TimeMode")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_PanelId")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Content")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ToastPrefab")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Duration")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_MaxToasts")));
            
            return root;
        }
    }
}