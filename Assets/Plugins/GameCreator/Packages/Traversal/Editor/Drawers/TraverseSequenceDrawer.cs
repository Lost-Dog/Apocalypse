using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(RunTraverseSequence))]
    public class RunTraverseSequenceDrawer : PropertyDrawer
    {
        public const string NAME_SEQUENCE = "m_Sequence";
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty sequence = property.FindPropertyRelative(NAME_SEQUENCE);
            return new TraverseSequenceTool(sequence);
        }
    }
}