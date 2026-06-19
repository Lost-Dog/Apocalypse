using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(MotionActionsList))]
    public class MotionActionsListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            MotionActionsTool actionsTool = new MotionActionsTool(property);
            return actionsTool;
        }
    }
}