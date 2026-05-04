using UnityEngine;
using UnityEditor;
using KingEdward.SkillTree;

namespace KingEdward.Editor.SkillTree
{
    [CustomPropertyDrawer(typeof(HotkeyConfig))]
    public class HotkeyConfigPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get properties
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty keyCodeProp = property.FindPropertyRelative("keyCode");
            SerializedProperty keyProp = property.FindPropertyRelative("key");
            SerializedProperty gamepadProp = property.FindPropertyRelative("gamepadButton");
            SerializedProperty mouseProp = property.FindPropertyRelative("mouseButton");
            SerializedProperty inputActionAssetProp = property.FindPropertyRelative("inputActionAsset");
            SerializedProperty inputActionNameProp = property.FindPropertyRelative("inputActionName");

            // Calculate rects
            Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            Rect contentRect2 = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, position.width, EditorGUIUtility.singleLineHeight);

            // Draw type dropdown
            EditorGUI.PropertyField(typeRect, typeProp, new GUIContent("Type"));

            // Draw relevant fields based on type
            HotkeyType hotkeyType = (HotkeyType)typeProp.enumValueIndex;
            
            switch (hotkeyType)
            {
                case HotkeyType.Keyboard:
                    EditorGUI.PropertyField(contentRect, keyCodeProp, new GUIContent("Key"));
                    break;
                    
                case HotkeyType.Mouse:
                    EditorGUI.PropertyField(contentRect, mouseProp, new GUIContent("Mouse Button"));
                    break;
                    
                case HotkeyType.Gamepad:
                    EditorGUI.PropertyField(contentRect, gamepadProp, new GUIContent("Gamepad Button"));
                    break;
                    
                case HotkeyType.InputSystem:
                    EditorGUI.PropertyField(contentRect, inputActionAssetProp, new GUIContent("Input Action Asset"));
                    EditorGUI.PropertyField(contentRect2, inputActionNameProp, new GUIContent("Action Name"));
                    break;
                    
                case HotkeyType.None:
                    EditorGUI.LabelField(contentRect, "No input configured");
                    break;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            HotkeyType hotkeyType = (HotkeyType)typeProp.enumValueIndex;
            
            // InputSystem needs 3 lines (Type + Asset + Name), others need 2 lines
            int lines = (hotkeyType == HotkeyType.InputSystem) ? 3 : 2;
            return EditorGUIUtility.singleLineHeight * lines + (lines - 1) * 2;
        }
    }
}
