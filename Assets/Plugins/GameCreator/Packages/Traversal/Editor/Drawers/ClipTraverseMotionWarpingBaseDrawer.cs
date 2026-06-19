using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(ClipTraverseMotionWarpingBase), true)]
    public class ClipTraverseMotionWarpingBaseDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            SerializedProperty mode = property.FindPropertyRelative("m_Mode");
            SerializedProperty transitionIn = property.FindPropertyRelative("m_TransitionIn");
            SerializedProperty transitionInEase = property.FindPropertyRelative("m_TransitionInEase");
            SerializedProperty easing = property.FindPropertyRelative("m_Easing");
            
            PropertyField fieldMode = new PropertyField(mode);
            PropertyField fieldTransitionIn = new PropertyField(transitionIn);
            PropertyField fieldTransitionInEase = new PropertyField(transitionInEase);
            PropertyField fieldEasing = new PropertyField(easing);
            
            root.Add(fieldMode);
            root.Add(fieldTransitionIn);
            root.Add(fieldTransitionInEase);
            root.Add(new SpaceSmaller());
            root.Add(fieldEasing);

            fieldTransitionIn.style.display = mode.enumValueIndex == 1
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldTransitionInEase.style.display = mode.enumValueIndex == 1
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldMode.RegisterValueChangeCallback(changeEvent =>
            {
                fieldTransitionIn.style.display = changeEvent.changedProperty.enumValueIndex == 1
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            
                fieldTransitionInEase.style.display = changeEvent.changedProperty.enumValueIndex == 1
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            return root;
        }
    }
}