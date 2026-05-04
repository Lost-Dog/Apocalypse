using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using RT;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Enter Cover")]
    [Description("Instructs the Character to enter cover using the Cover Controller")]

    [Category("Cover/Enter Cover")]

    [Parameter("Character", "The Character game object that will enter cover")]

    [Keywords("Character", "Cover", "Hide", "Controller")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionCharacterEnterCover : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public override string Title => $"Make {this.m_Character} enter cover";

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
                        _ = coverController.TryEnterCover();
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
