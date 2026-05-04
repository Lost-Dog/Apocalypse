using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace Threepeat
{
    [Title("Is MMLC Input Enabled")]
    [Description("Returns true if the Character's MMLC Input is enabled")]

    [Category("MMLC/Is MMLC Input Enabled")]
    [Parameter("Property", "which input to check")]
    [Parameter("Is Enabled", "if true, condition will pass if input is enabled, if false, condition will pass if it's not.")]
    [Serializable]
    public class ConditionMMLCInputEnabled : TConditionCharacter
    {
        [SerializeField] private InstructionMMLCChangeInputEnabled.PropertyName m_Property = InstructionMMLCChangeInputEnabled.PropertyName.SprintKey;

        [SerializeField] private bool m_IsEnabled = true;

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
			NGCharacter ngchar = character?.GetComponent<NGCharacter>();
			if (ngchar == null)
			{
				return false;
			}

			if (!ngchar.InputScheme.IsInputDriven())
			{
				// Not an input-driven input scheme, nothing to do.
				return true;
			}
			NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)ngchar.InputScheme;

			ContextualActionProcessor[] processors =
			{
				scheme.keyProcessorJumpParkour,
				scheme.keyProcessorSprintHold,
				scheme.keyProcessorCrouchToggle,
				scheme.keyProcessorStrafeToggle
			};

			switch (m_Property)
			{
				case InstructionMMLCChangeInputEnabled.PropertyName.AllKeyProcessors:
					bool retval = true;
					foreach (ContextualActionProcessor proc in processors)
					{
						retval = retval && (proc.enabled == m_IsEnabled);
					}
					return retval;
				default:
					return processors[(int)m_Property].enabled == m_IsEnabled;
			}

		}
	}
}