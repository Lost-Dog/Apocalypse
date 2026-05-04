using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(MotionStateItem))]
    public class MotionStateItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(property.FindPropertyRelative("m_Id")));
            root.Add(new PropertyField(property.FindPropertyRelative("m_AllowMovement")));
            
            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("On Enter"));
            root.Add(new SpaceSmallest());
            root.Add(new PropertyField(property.FindPropertyRelative("m_InstructionsOnEnter")));
            
            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("On Exit"));
            root.Add(new SpaceSmallest());
            root.Add(new PropertyField(property.FindPropertyRelative("m_InstructionsOnExit")));
            
            root.Bind(property.serializedObject);
            return root;
        }
    }
}