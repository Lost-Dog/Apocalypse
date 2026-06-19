using UnityEditor;
using UnityEngine.UIElements;

namespace KingEdward.SkillTree.Editor
{
    [CustomPropertyDrawer(typeof(SkillTreeSequence))]
    public class SkillSequenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new SkillSequenceTool(property);
        }
    }
}
