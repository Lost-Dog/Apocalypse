using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("On Skill Cooldown End")]
    [Category("KingEdward/Skill Tree/On Skill Cooldown End")]
    [Description("Triggered when a skill cooldown ends")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Keywords("Skill", "Tree", "Cooldown", "End", "Ready", "Event")]
    
    [Serializable]
    public class TriggerOnSkillCooldownEnd : GameCreator.Runtime.VisualScripting.Event
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
                skillTreeComponent.OnSkillCooldownChanged -= OnSkillCooldownChangedHandler;
                skillTreeComponent.OnSkillCooldownChanged += OnSkillCooldownChangedHandler;
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
                skillTreeComponent.OnSkillCooldownChanged -= OnSkillCooldownChangedHandler;
            }
        }
        
        private void OnSkillCooldownChangedHandler(Skill skill)
        {
            if (m_Skill == null || m_Skill == skill)
            {
                GameObject target = m_Target.Get(this.m_Trigger.gameObject);
                if (target == null) return;
                
                SkillTreeComponent skillTreeComponent = target.GetComponent<SkillTreeComponent>();
                if (skillTreeComponent != null)
                {
                    var skillInstance = skillTreeComponent.GetSkill(skill);
                    if (skillInstance != null && !skillInstance.isOnCooldown)
                    {
                        _ = this.m_Trigger.Execute(this.Self);
                    }
                }
            }
        }
    }
}
