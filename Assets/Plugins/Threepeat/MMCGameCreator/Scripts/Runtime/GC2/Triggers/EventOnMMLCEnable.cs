using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace Threepeat
{
    [Title("On MMLC Enable")]

    [Category("MMLC/On MMLC Enable")]
    [Description("Executed every time MMLC is enabled/disabled per the parameter")]

    [Parameter("Trigger On", "Whether to trigger on Enable or Disable of MMLC")]

    [Serializable]
    public class EventOnMMLCEnable : TEventCharacter
    {
        private MMCGameCreator2 mmcgc;
        [SerializeField] private TriggerCriteria m_TriggerOn = TriggerCriteria.TriggerOnEnable;

        public enum TriggerCriteria
        {
            TriggerOnEnable,
            TriggerOnDisable
        }

        protected override void WhenDisabled(Trigger trigger, Character character)
        {
            if (mmcgc == null)
            {
                mmcgc = character.GetComponent<MMCGameCreator2>();
            }
            if (m_TriggerOn == TriggerCriteria.TriggerOnEnable) {
                mmcgc.OnMMLCEnable.RemoveListener(this.FireTrigger);
            }
            else
            {
                mmcgc.OnMMLCDisable.RemoveListener(this.FireTrigger);
            }
        }

        protected override void WhenEnabled(Trigger trigger, Character character)
        {
            if (mmcgc == null)
            {
                mmcgc = character.GetComponent<MMCGameCreator2>();
            }
            if (m_TriggerOn == TriggerCriteria.TriggerOnEnable)
            {
                mmcgc.OnMMLCEnable.AddListener(this.FireTrigger);
            }
            else
            {
                mmcgc.OnMMLCDisable.AddListener(this.FireTrigger);
            }
        }

        private void FireTrigger()
        {
            Character character = this.m_Character.Get<Character>(this.m_Trigger.gameObject);
            if (character != null) _ = this.m_Trigger.Execute(character.gameObject);
        }
    }
}