using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace Threepeat
{
	[Title("Is MMLC Property Enabled")]
	[Description("Returns true if the Character's Property is enabled")]

	[Category("MMLC/Is MMLC Property Enabled")]
	[Parameter("Property", "which property to check")]
	[Parameter("Is Enabled", "if true, condition will pass if property is enabled, if false, condition will pass if it's not.")]
	[Serializable]
	public class ConditionMMLCProperty : TConditionCharacter
	{
		[SerializeField] private InstructionMMLCChangeProperty.PropertyName m_Property = InstructionMMLCChangeProperty.PropertyName.CanRun;

		[SerializeField] private bool m_IsEnabled = true;

		protected override bool Run(Args args)
		{
			Character character = this.m_Character.Get<Character>(args);
			NGCharacter ngchar = character?.GetComponent<NGCharacter>();
			if (ngchar == null)
			{
				return false;
			}

			switch (m_Property)
			{
				case InstructionMMLCChangeProperty.PropertyName.CanRun:
					return ngchar.canRun == m_IsEnabled;
				case InstructionMMLCChangeProperty.PropertyName.CanJump:
					return ngchar.canJump == m_IsEnabled;
			}
			return false;

		}
	}
}