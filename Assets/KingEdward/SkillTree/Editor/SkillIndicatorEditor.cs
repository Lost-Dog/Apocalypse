using UnityEngine;
using UnityEditor;

namespace KingEdward.SkillTree.Editor
{
    public static class SkillIndicatorEditor
    {
        [MenuItem("GameObject/KingEdward/Skill Tree/Create Skill Indicator", false, 10)]
        public static void CreateSkillIndicator()
        {
            GameObject root = new GameObject("SkillIndicatorController");
            GameObject indicatorObj = new GameObject("Indicator");
            indicatorObj.transform.SetParent(root.transform, false);

            var controller = root.AddComponent<SkillIndicatorController>();
            var indicator = indicatorObj.AddComponent<SkillIndicator>();

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("m_Indicator").objectReferenceValue = indicator;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = root;
            Undo.RegisterCreatedObjectUndo(root, "Create Skill Indicator");
        }
    }
}
