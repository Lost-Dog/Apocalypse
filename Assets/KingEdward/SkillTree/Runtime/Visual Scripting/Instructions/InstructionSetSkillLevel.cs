using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace KingEdward.SkillTree
{
    [Title("Set Skill Level")]
    [Description("Sets the level of a specific skill")]
    
    [Category("KingEdward/Skill Tree/Set Skill Level")]
    
    [Keywords("Skill", "Tree", "Level", "Set", "Upgrade")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Serializable]
    public class SetSkillLevel : Instruction
    {
        [SerializeField] private Skill m_Skill = null;
        [SerializeField] private PropertyGetInteger m_Level = new PropertyGetInteger(1);
        
        public override string Title => $"Set {m_Skill?.name ?? "(none)"} to Level {m_Level}";
        
        protected override Task Run(Args args)
        {
            if (m_Skill == null) return DefaultResult;
            
            int targetLevel = (int)m_Level.Get(args);
            targetLevel = Mathf.Clamp(targetLevel, 1, m_Skill.maxLevel);
            
            // Find SkillTreeComponent in the target
            SkillTreeComponent skillTreeComponent = args.Self.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent == null)
            {
                // Try to find it in the target game object
                GameObject target = args.Target;
                if (target != null)
                {
                    skillTreeComponent = target.GetComponent<SkillTreeComponent>();
                }
            }
            
            if (skillTreeComponent == null)
            {
                Debug.LogError($"[SkillTree] SetSkillLevel: Could not find SkillTreeComponent for {m_Skill.name}");
                return DefaultResult;
            }
            
            // Get the SkillInstance
            var skillInstance = skillTreeComponent.GetSkill(m_Skill);
            if (skillInstance == null)
            {
                Debug.LogError($"[SkillTree] SetSkillLevel: Could not get SkillInstance for {m_Skill.name} - skill may not be unlocked");
                return DefaultResult;
            }
            
            // Check if skill is unlocked
            if (!skillTreeComponent.IsUnlocked(m_Skill))
            {
                Debug.LogWarning($"[SkillTree] Cannot set level for {m_Skill.name} - skill is not unlocked");
                return DefaultResult;
            }
            
            // Set the level directly on the SkillInstance
            skillInstance.SetLevel(targetLevel);
            
            // Trigger UI refresh
            skillTreeComponent.TriggerRefreshAllSkills();
            
            return DefaultResult;
        }
    }
}
