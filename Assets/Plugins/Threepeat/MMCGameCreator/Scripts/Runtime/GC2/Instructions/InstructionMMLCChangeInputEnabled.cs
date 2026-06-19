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

    [Title("MMLC Change Input Enabled")]
    [Description("Changes whether MMLC Input(s) are enabled.")]

    [Category("MMLC/Change Input Enabled (MMLC)")]

    [Parameter("Character", "The character to modify")]
    [Parameter("Property", "The property to modify")]
    [Parameter("Do Enable", "Whether the input(s) specified in are to be enabled")]

    [Serializable]
    public class InstructionMMLCChangeInputEnabled : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyName m_Property = PropertyName.SprintKey;
        [SerializeField] private bool m_DoEnable = true;

        public enum PropertyName
        {
            JumpParkourKey = 0,
            SprintKey,
            CrouchKey,
            StrafeKey,
            AllKeyProcessors
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

            if (!ngchar.InputScheme.IsInputDriven())
            {
                // Not an input-driven input scheme, nothing to do.
                return DefaultResult;
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
                case PropertyName.AllKeyProcessors:
                    foreach (ContextualActionProcessor proc in processors)
                    {
                        proc.enabled = m_DoEnable;
                    }
                    break;
                default:
                    processors[(int)m_Property].enabled = m_DoEnable;
                    break;
            }


            return DefaultResult;
        }
    }
}