using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    [CustomPropertyDrawer(typeof(ContextualActionTrigger))]

    public class ContextualActionTriggerInspector : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //ContextualActionTrigger trigger = as ContextualActionTrigger;

            if (property == null)
            {
                Debug.Log("property null!");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
     
            // Calculate rects here
            var line1Rect = new Rect(position.x, position.y + 5, position.width, EditorGUIUtility.singleLineHeight);
            var line3Rect = new Rect(position.x, position.y + 5 + 2 * (EditorGUIUtility.singleLineHeight + 2), position.width, EditorGUIUtility.singleLineHeight);
            var line2Rect = new Rect(position.x, position.y + 5 + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty sp_actionName = property.FindPropertyRelative("actionName");
            SerializedProperty sp_trigger = property.FindPropertyRelative("trigger");
            SerializedProperty sp_floatParam = property.FindPropertyRelative("floatParam");
            if (sp_actionName != null) {
                sp_trigger.enumValueIndex = (int)(ContextualActionTrigger.TriggeringCondition)EditorGUI.EnumPopup(line1Rect, new GUIContent("Trigger Condition"), (ContextualActionTrigger.TriggeringCondition)sp_trigger.enumValueIndex);
                ContextualActionTrigger.TriggeringCondition tc = (ContextualActionTrigger.TriggeringCondition)sp_trigger.enumValueIndex;
                switch (tc)
                {
                    case ContextualActionTrigger.TriggeringCondition.TriggerDisabled:
                        break;
                    case ContextualActionTrigger.TriggeringCondition.InputHeldDown:
                    case ContextualActionTrigger.TriggeringCondition.Other:
                        EditorGUI.PropertyField(line2Rect, sp_actionName, new GUIContent("Action Name"));
                        EditorGUI.PropertyField(line3Rect, sp_floatParam, new GUIContent("Float Param", "For Input Held Down, this is duration in seconds before action should trigger"));
                        break;
                    default:
                        EditorGUI.PropertyField(line2Rect, sp_actionName, new GUIContent("Action Name"));
                        break;
                }
            }
            else
            {
                EditorGUI.LabelField(line2Rect, "actionName null!");
            }
            //EditorGUI.PropertyField(floatValueRect, property.FindPropertyRelative("floatParam"), new GUIContent("Float Value"));

            EditorGUI.EndProperty();
        }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + 10; // Adjust height for three properties
    }
}
}