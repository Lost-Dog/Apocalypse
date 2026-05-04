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

    [Title("Fire Contextual Action (MMLC)")]
    [Description("Execute Contextual Action")]

    [Category("MMLC/Fire Contextual Action (MMLC)")]

    [Parameter("Character", "The character for which the action should be executed")]
    [Parameter("Action Name", "Action to fire (not case sensitive)")]

    [Serializable]
    public class InstructionMMLCContextualAction : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private string m_ActionName = "jump";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null)
            {
                Debug.LogError("selected character does not have a Character component.");
            }

            NGCharacter ngchar = character.GetComponent<NGCharacter>();

            if (ngchar == null)
            {
                return DefaultResult;
            }

            ngchar.FireContextualAction(m_ActionName);

            return DefaultResult;
        }
    }
}