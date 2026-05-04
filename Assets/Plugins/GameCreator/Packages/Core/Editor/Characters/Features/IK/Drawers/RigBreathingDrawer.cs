using GameCreator.Editor.Common;
using GameCreator.Runtime.Characters.IK;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Characters
{
    [CustomPropertyDrawer(typeof(RigBreathing))]
    public class RigBreathingDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty exertion = property.FindPropertyRelative("m_Exertion");
            SerializedProperty rate = property.FindPropertyRelative("m_Rate");
            
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(exertion));
            root.Add(new PropertyField(rate));

            return root;
        }
    }
}