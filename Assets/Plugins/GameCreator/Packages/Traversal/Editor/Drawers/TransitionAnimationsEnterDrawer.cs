using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(TransitionAnimationsEnter))]
    public class TransitionAnimationsEnterDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            root.Add(new SpaceSmaller());
            root.Add(new LabelTitle("Enter Animations"));
            root.Add(new SpaceSmallest());
            root.Add(new PropertyField(property.FindPropertyRelative("m_Forward"), "From Forward"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Backward"), "From Backward"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Left"), "From Left"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Right"), "From Right"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Upward"), "From Upward"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Downward"), "From Downward"));
            
            return root;
        }
    }
}