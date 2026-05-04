using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using RT;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Exit Cover")]
    [Description("Instructs the Character to exit cover using the Cover Controller")]

    [Category("Cover/Exit Cover")]

    [Parameter("Character", "The Character game object that will exit cover")]

    [Keywords("Character", "Cover", "Exit", "Controller")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Red)]

    [Serializable]
    public class InstructionCharacterExitCover : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public override string Title => $"Make {this.m_Character} exit cover";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character != null)
            {
                CoverController coverController = character.GetComponent<CoverController>();

                if (coverController != null)
                {
                    if (coverController.IsInCover())
                    {
                        _ = coverController.ExitCover();
                    }
                    else
                    {
                        Debug.LogWarning($"The character {character.gameObject.name} is not currently in cover.");
                    }
                }
                else
                {
                    Debug.LogWarning($"The character {character.gameObject.name} does not have a CoverController component.");
                }
            }

            return DefaultResult;
        }
    }
}
