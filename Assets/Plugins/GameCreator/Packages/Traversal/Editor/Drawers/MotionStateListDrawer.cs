using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(MotionStateList))]
    public class MotionStateListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            MotionStatesTool statesTool = new MotionStatesTool(property);
            return statesTool;
        }
    }
}