using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using GameCreator.Editor.Common;

namespace KingEdward.SkillTree.Editor
{
    [CustomEditor(typeof(SkillTreeComponent))]
    public class SkillTreeComponentEditor : UnityEditor.Editor
    {

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            // Runtime Info (only in play mode)
            if (Application.isPlaying)
            {
                SkillTreeComponent component = (SkillTreeComponent)target;
                
                var runtimeBox = new Box();
                runtimeBox.style.backgroundColor = new Color(0.2f, 0.3f, 0.4f, 0.3f);
                runtimeBox.style.paddingTop = 5;
                runtimeBox.style.paddingBottom = 5;
                runtimeBox.style.paddingLeft = 5;
                runtimeBox.style.paddingRight = 5;
                runtimeBox.style.marginBottom = 10;
                
                int unlockedCount = 0;
                int totalSkills = 0;
                
                if (component.skillTree != null && component.skillTree.allSkills != null)
                {
                    totalSkills = component.skillTree.allSkills.Count;
                    foreach (var skill in component.skillTree.allSkills)
                    {
                        if (component.IsUnlocked(skill))
                        {
                            unlockedCount++;
                        }
                    }
                }
                
                runtimeBox.Add(new Label($"Skills Unlocked: {unlockedCount} / {totalSkills}"));
                
                root.Add(runtimeBox);
            }
            
            // Skill Tree Data
            root.Add(new PropertyField(serializedObject.FindProperty("m_SkillTree")));
            
            root.Add(new SpaceSmall());
            
            // Skill Points System
            root.Add(new LabelTitle("Skill Points System"));
            root.Add(new PropertyField(serializedObject.FindProperty("m_CurrentSkillPoints")));
            root.Add(new PropertyField(serializedObject.FindProperty("m_MaxSkillPoints")));
            
            root.Add(new SpaceSmall());
            
            // Refund Settings
            root.Add(new LabelTitle("Refund Settings"));
            root.Add(new PropertyField(serializedObject.FindProperty("m_AllowCascadeRefund")));
            
            root.Add(new SpaceSmall());
            
            // Debug Settings
            root.Add(new LabelTitle("Debug Settings"));
            root.Add(new PropertyField(serializedObject.FindProperty("enableDebugLogs")));
            
            return root;
        }
    }
}
