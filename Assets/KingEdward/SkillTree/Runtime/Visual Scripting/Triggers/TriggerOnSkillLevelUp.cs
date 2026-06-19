using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("On Skill Level Up")]
    [Category("KingEdward/Skill Tree/On Skill Level Up")]
    [Description("Triggered when a skill levels up")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Blue)]
    
    [Keywords("Skill", "Tree", "Level", "Up", "Event")]
    
    [Serializable]
    public class TriggerOnSkillLevelUp : GameCreator.Runtime.VisualScripting.Event
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();
        [SerializeField] private Skill m_Skill = null;
        
        protected override void OnEnable(GameCreator.Runtime.VisualScripting.Trigger trigger)
        {
            base.OnEnable(trigger);
            
            GameObject target = m_Target.Get(trigger.gameObject);
            if (target == null) return;
            
            SkillTreeComponent skillTreeComponent = target.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent != null)
            {
                skillTreeComponent.OnSkillLevelUp -= OnSkillLevelUpHandler;
                skillTreeComponent.OnSkillLevelUp += OnSkillLevelUpHandler;
            }
        }

        protected override void OnDisable(GameCreator.Runtime.VisualScripting.Trigger trigger)
        {
            base.OnDisable(trigger);
            
            GameObject target = m_Target.Get(trigger.gameObject);
            if (target == null) return;
            
            SkillTreeComponent skillTreeComponent = target.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent != null)
            {
                skillTreeComponent.OnSkillLevelUp -= OnSkillLevelUpHandler;
            }
        }
        
        private void OnSkillLevelUpHandler(Skill skill, int newLevel)
        {
            if (m_Skill == null || m_Skill == skill)
            {
                _ = this.m_Trigger.Execute(this.Self);
            }
        }
    }
}
