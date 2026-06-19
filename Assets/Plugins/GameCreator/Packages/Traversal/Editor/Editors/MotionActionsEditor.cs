using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(MotionActions))]
    public class MotionActionsEditor : UnityEditor.Editor
    {
        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            StyleSheet[] styleSheets = StyleSheetUtils.Load();
            foreach (StyleSheet styleSheet in styleSheets) root.styleSheets.Add(styleSheet);

            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_CanCancel")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_CanJump")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_CustomJumpForce")));
            
            SerializedProperty dashOnJump = this.serializedObject.FindProperty("m_DashOnJump");
            SerializedProperty dashDirection = this.serializedObject.FindProperty("m_DashLocalDirection");
            SerializedProperty dashSpeed = this.serializedObject.FindProperty("m_DashSpeed");
            SerializedProperty dashDuration = this.serializedObject.FindProperty("m_DashDuration");
            
            PropertyField fieldDashOnJump = new PropertyField(dashOnJump);
            PropertyField fieldDashDirection = new PropertyField(dashDirection, "Local Direction");
            PropertyField fieldDashSpeed = new PropertyField(dashSpeed, "Speed");
            PropertyField fieldDashDuration = new PropertyField(dashDuration, "Duration");
            
            VisualElement dashContent = new VisualElement { style = { marginLeft = 10 } };
            
            root.Add(fieldDashOnJump);
            root.Add(dashContent);
            
            dashContent.Add(fieldDashDirection);
            dashContent.Add(fieldDashSpeed);
            dashContent.Add(fieldDashDuration);
            
            dashContent.style.display = dashOnJump.boolValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldDashOnJump.RegisterValueChangeCallback(changeEvent =>
            {
                dashContent.style.display = changeEvent.changedProperty.boolValue
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("Actions:"));
            root.Add(new SpaceSmallest());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ActionsList")));
            
            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("States:"));
            root.Add(new SpaceSmallest());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_StateList")));
            
            return root;
        }
    }
}