using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;
using KingEdward;

namespace KingEdward.SkillTree
{
    [Icon(SkillTreePaths.SKILL_TREE_DATA)]
    [CreateAssetMenu(menuName = "KingEdward/Skill Tree/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        [Header("Skills")]
        [Tooltip("List of all skills in this skill tree")]
        public List<Skill> allSkills = new List<Skill>();
    }
}

