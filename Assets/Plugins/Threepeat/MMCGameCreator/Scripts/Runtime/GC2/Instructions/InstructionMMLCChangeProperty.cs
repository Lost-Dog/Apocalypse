using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using MxM;
using UnityEngine;

namespace Threepeat
{
    [Version(0, 1, 0)]

    [Title("Change MMLC Character Property")]
    [Description("Changes MMLC Character Properties.")]

    [Category("MMLC/Change MMLC Property")]

    [Parameter("Character", "The character to modify")]
    [Parameter("Property", "The property to modify")]
    [Parameter("Value To Set", "The target value")]

    [Serializable]
    public class InstructionMMLCChangeProperty : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyName m_Property = PropertyName.CanJump;
        [SerializeField] private bool m_ValueToSet = true;

        public enum PropertyName
        {
            CanRun,
            CanJump
        }

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null)
            {
                Debug.LogError("ChangeCharacterConfig: selected character does not have a Character component.");
            }

            NGCharacter ngchar = character.GetComponent<NGCharacter>();

            if (ngchar == null)
            {
                return DefaultResult;
            }

            switch (m_Property)
            {
                case PropertyName.CanRun:
                    ngchar.canRun = m_ValueToSet;
                    break;
                case PropertyName.CanJump:
                    ngchar.canJump = m_ValueToSet;
                    break;
            }


            return DefaultResult;
        }
    }
}