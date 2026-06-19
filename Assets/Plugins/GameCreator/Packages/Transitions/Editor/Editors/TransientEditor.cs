using GameCreator.Editor.Common;
using GameCreator.Runtime.Transitions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Transitions
{
    [CustomEditor(typeof(Transient))]
    public class TransientEditor : UnityEditor.Editor
    {
        // PAINT METHOD: --------------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement content = new VisualElement();

            SerializedProperty value = this.serializedObject.FindProperty("m_Value");
            SerializedProperty decimals = this.serializedObject.FindProperty("m_Decimals");
            SerializedProperty progress = this.serializedObject.FindProperty("m_Progress");
            
            SerializedProperty activeIfOpen = this.serializedObject.FindProperty("m_ActiveIfOpen");
            SerializedProperty activeIfReady = this.serializedObject.FindProperty("m_ActiveIfReady");
            
            content.Add(new PropertyField(value));
            content.Add(new PropertyField(decimals));
            
            content.Add(new SpaceSmall());
            content.Add(new PropertyField(progress));
            
            content.Add(new SpaceSmall());
            content.Add(new PropertyField(activeIfOpen));
            content.Add(new PropertyField(activeIfReady));

            return content;
        }
    }
}