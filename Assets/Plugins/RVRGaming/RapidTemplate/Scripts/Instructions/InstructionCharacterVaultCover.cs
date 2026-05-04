using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using RT;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Vault Cover")]
    [Description("Instructs the Character to vault over cover using the Cover Controller")]

    [Category("Cover/Vault Cover")]

    [Parameter("Character", "The Character game object that will vault over cover")]

    [Keywords("Character", "Cover", "Vault", "Controller")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Green)]

    [Serializable]
    public class InstructionCharacterVaultCover : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public override string Title => $"Make {this.m_Character} vault cover";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character != null)
            {
                CoverController coverController = character.GetComponent<CoverController>();

                if (coverController != null)
                {
                    _ = coverController.TryVault();
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
