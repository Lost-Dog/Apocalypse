using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(TransitionAnimationsExit))]
    public class TransitionAnimationsExitDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            root.Add(new SpaceSmaller());
            root.Add(new LabelTitle("Exit Animations"));
            root.Add(new SpaceSmallest());
            root.Add(new PropertyField(property.FindPropertyRelative("m_Forward"), "To Forward"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Backward"), "To Backward"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Left"), "To Left"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Right"), "To Right"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Upward"), "To Upward"));
            root.Add(new PropertyField(property.FindPropertyRelative("m_Downward"), "To Downward"));
            
            return root;
        }
    }
}