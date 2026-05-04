using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(ClipTraverseMotionWarpingPosition), true)]
    public class ClipTraverseMotionWarpingPositionDrawer : ClipTraverseMotionWarpingBaseDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = base.CreatePropertyGUI(property);
            
            SerializedProperty lift = property.FindPropertyRelative("m_Lift");
            SerializedProperty liftEase = property.FindPropertyRelative("m_LiftEase");
            
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(lift));
            root.Add(new PropertyField(liftEase));
            
            return root;
        }
    }
}