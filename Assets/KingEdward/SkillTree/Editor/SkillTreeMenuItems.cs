using UnityEngine;
using UnityEditor;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Editor
{
    public static class SkillTreeMenuItems
    {
        
        [MenuItem("GameObject/KingEdward/Skill Tree/Add Skill Tree Component", false, 10)]
        public static void AddSkillTreeComponent(MenuCommand menuCommand)
        {
            GameObject go = menuCommand.context as GameObject;
            if (go != null)
            {
                Undo.AddComponent<SkillTreeComponent>(go);
            }
        }

        [MenuItem("GameObject/KingEdward/Skill Tree/Add Skill Tree Component", true)]
        public static bool ValidateAddSkillTreeComponent()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("Tools/KingEdward/Skill Tree/Documentation", priority = 100)]
        public static void OpenDocumentation()
        {
            string readmePath = "Assets/KingEdward/SkillTree/README.md";
            Object readme = AssetDatabase.LoadAssetAtPath<Object>(readmePath);
            if (readme != null)
            {
                Selection.activeObject = readme;
                EditorGUIUtility.PingObject(readme);
            }
            else
            {
                EditorUtility.DisplayDialog("Documentation", 
                    "README.md not found at:\n" + readmePath, 
                    "OK");
            }
        }
    }
}
