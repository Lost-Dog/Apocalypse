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

    [Title("Change MMLC Character Config")]
    [Description("Changes MMLC Character Configuration.")]

    [Category("MMLC/Change MMLC Character Config")]

    [Parameter("Character", "The character to modify")]
    [Parameter("New Character Config", "New character config")]


    [Serializable]
    public class InstructionMMLCChangeCharacterConfig : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private NGCharacterBaseConfig newCharacterConfig;

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

            if (newCharacterConfig == null)
            {
                // No config specified, nothing to do.
                return DefaultResult;
            }

            ngchar.SetConfig(newCharacterConfig);

            return DefaultResult;
        }
    }
}