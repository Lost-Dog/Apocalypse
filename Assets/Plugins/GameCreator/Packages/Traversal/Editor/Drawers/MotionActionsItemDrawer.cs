using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(MotionActionsItem))]
    public class MotionActionsItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(property.FindPropertyRelative("m_Id")));
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(property.FindPropertyRelative("m_Instructions")));
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(property.FindPropertyRelative("m_Exits")));
            
            return root;
        }
    }
}