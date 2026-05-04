namespace KingEdward.SkillTree
{
    /// <summary>
    /// Gizmo/icon paths for Skill Tree components (self-contained so the Skill Tree assembly does not depend on Assembly-CSharp).
    /// </summary>
    public static class SkillTreePaths
    {
        private const string BASE = "Assets/KingEdward/SkillTree/Editor/Gizmos/";

        public const string SKILL = BASE + "GizmoSkill.png";
        public const string SKILL_TREE_DATA = BASE + "GizmoSkillTreeData.png";
        public const string SKILL_TREE_COMPONENT = BASE + "GizmoSkillTreeComponent.png";
        public const string SKILL_TREE_UI = BASE + "GizmoSkillTreeUI.png";
        public const string SKILL_ITEM_UI = BASE + "GizmoSkillItemUI.png";
        public const string SKILL_HOTBAR = BASE + "GizmoSkillHotbar.png";
        public const string PROJECTILE_BEHAVIOR = BASE + "GizmoProjectileBehavior.png";
        public const string CONFIRMATION_UI = BASE + "GizmoConfirmationUI.png";
        public const string SKILL_POINTS_UI = BASE + "GizmoSkillPointsUI.png";
        public const string SKILL_TOOLTIP = BASE + "GizmoSkillTooltip.png";
    }
}
