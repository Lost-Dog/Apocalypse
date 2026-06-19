using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace KingEdward.SkillTree.Editor
{
    /// <summary>
    /// Custom property drawer for string arrays that should display tags
    /// Use with [TagArray] attribute
    /// </summary>
    [CustomPropertyDrawer(typeof(TagArrayAttribute))]
    public class TagArrayDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [TagArray] with string arrays only.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Get all available tags
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            
            // Get current value
            string currentTag = property.stringValue;
            
            // Find current index
            int currentIndex = 0;
            List<string> tagList = new List<string> { "(None)" };
            tagList.AddRange(tags);
            
            if (!string.IsNullOrEmpty(currentTag))
            {
                currentIndex = System.Array.IndexOf(tags, currentTag) + 1;
                if (currentIndex <= 0) currentIndex = 0;
            }
            
            // Draw popup
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, tagList.ToArray());
            
            // Update value
            if (newIndex != currentIndex)
            {
                property.stringValue = newIndex == 0 ? "" : tags[newIndex - 1];
            }

            EditorGUI.EndProperty();
        }
    }
}
