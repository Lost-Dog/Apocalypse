using GameCreator.Editor.Common;
using GameCreator.Runtime.Characters.IK;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Characters
{
    [CustomPropertyDrawer(typeof(RigTwitching))]
    public class RigTwitchingDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty speed = property.FindPropertyRelative("m_Speed");
            SerializedProperty intensity = property.FindPropertyRelative("m_Intensity");
            
            SerializedProperty armsTwitch = property.FindPropertyRelative("m_ArmsTwitch");
            SerializedProperty handsTwitch = property.FindPropertyRelative("m_HandsTwitch");
            SerializedProperty fingersTwitch = property.FindPropertyRelative("m_FingersTwitch");
            
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(speed));
            root.Add(new PropertyField(intensity));
            
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(armsTwitch));
            root.Add(new PropertyField(handsTwitch));
            root.Add(new PropertyField(fingersTwitch));
            
            return root;
        }
    }
}