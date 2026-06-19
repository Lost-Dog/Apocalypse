using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace Threepeat
{
    [Title("On MMLC Jump")]

    [Category("MMLC/On MMLC Jump")]
    [Description("Executed every time MMLC jump is performed per the parameter")]

    [Parameter("Trigger On", "Whether to trigger on Enable or Disable of MMLC")]

    [Serializable]
    public class EventOnMMLCJump : TEventCharacter
    {
        private MMCGameCreator2 mmcgc;
        [SerializeField] private TriggerOnCriteria m_TriggerOn = TriggerOnCriteria.AnyJump;

        public enum TriggerOnCriteria
        {
            RegularJump,
            BigJump,
            AnyJump
        }

        protected override void WhenDisabled(Trigger trigger, Character character)
        {
            if (mmcgc == null)
            {
                mmcgc = character.GetComponent<MMCGameCreator2>();
            }
            mmcgc.mmlcCharacter.movement.onJump.RemoveListener(this.FireTrigger);
            
        }

        protected override void WhenEnabled(Trigger trigger, Character character)
        {
            if (mmcgc == null)
            {
                mmcgc = character.GetComponent<MMCGameCreator2>();
            }
            mmcgc.mmlcCharacter.movement.onJump.AddListener(this.FireTrigger);
            
        }

        private void FireTrigger(bool bigJump)
        {
            if ((m_TriggerOn == TriggerOnCriteria.AnyJump) ||
                (!bigJump && (m_TriggerOn == TriggerOnCriteria.RegularJump)) ||
                (bigJump && (m_TriggerOn == TriggerOnCriteria.BigJump)))
            {
                Character character = this.m_Character.Get<Character>(this.m_Trigger.gameObject);
                if (character != null) _ = this.m_Trigger.Execute(character.gameObject);
            }
        }
    }
}