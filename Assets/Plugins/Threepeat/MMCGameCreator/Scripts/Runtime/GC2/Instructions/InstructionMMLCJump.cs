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

    [Title("Jump  (MMLC)")]
    [Description("Executed MMLC Jump.")]

    [Category("MMLC/Jump (MMLC)")]

    [Parameter("Character", "The character to modify")]
    [Parameter("BigJump", "Whether to big-jump")]

    [Serializable]
    public class InstructionMMLCJump : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private bool m_BigJump = true;

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

            ngchar.DoJump(m_BigJump);

            return DefaultResult;
        }
    }
}