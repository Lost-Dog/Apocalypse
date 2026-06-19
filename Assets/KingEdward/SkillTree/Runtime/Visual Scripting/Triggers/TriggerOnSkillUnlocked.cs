using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.VisualScripting
{
    [Title("On Skill Unlocked")]
    [Category("KingEdward/Skill Tree/On Skill Unlocked")]
    [Description("Triggered when a skill is unlocked")]
    
    [Image(typeof(IconSkill), ColorTheme.Type.Green)]
    
    [Keywords("Skill", "Tree", "Unlock", "Event")]
    
    [Serializable]
    public class TriggerOnSkillUnlocked : GameCreator.Runtime.VisualScripting.Event
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
                skillTreeComponent.OnSkillUnlocked -= OnSkillUnlockedHandler;
                skillTreeComponent.OnSkillUnlocked += OnSkillUnlockedHandler;
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
                skillTreeComponent.OnSkillUnlocked -= OnSkillUnlockedHandler;
            }
        }
        
        private void OnSkillUnlockedHandler(Skill skill)
        {
            if (m_Skill == null || m_Skill == skill)
            {
                _ = this.m_Trigger.Execute(this.Self);
            }
        }
    }
}
