using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("On Skill Points Changed")]
    [Category("KingEdward/Skill Tree/On Skill Points Change")]
    [Description("Triggered when skill points change")]
    
    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Purple)]
    
    [Keywords("Skill", "Tree", "Points", "Currency", "Event")]
    
    [Serializable]
    public class TriggerOnSkillPointsChanged : GameCreator.Runtime.VisualScripting.Event
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();
        
        protected override void OnEnable(GameCreator.Runtime.VisualScripting.Trigger trigger)
        {
            base.OnEnable(trigger);
            
            GameObject target = m_Target.Get(trigger.gameObject);
            if (target == null) return;
            
            SkillTreeComponent skillTreeComponent = target.GetComponent<SkillTreeComponent>();
            if (skillTreeComponent != null)
            {
                skillTreeComponent.OnSkillPointsChanged -= OnSkillPointsChangedHandler;
                skillTreeComponent.OnSkillPointsChanged += OnSkillPointsChangedHandler;
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
                skillTreeComponent.OnSkillPointsChanged -= OnSkillPointsChangedHandler;
            }
        }
        
        private void OnSkillPointsChangedHandler(int newAmount)
        {
            _ = this.m_Trigger.Execute(this.Self);
        }
    }
}
