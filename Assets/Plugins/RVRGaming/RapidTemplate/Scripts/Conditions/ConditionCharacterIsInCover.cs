using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using RT;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is In Cover")]
    [Description("Returns true if the Character is in cover")]

    [Category("Cover/Is In Cover")]

    [Keywords("Cover", "Combat", "Character")]

    [Image(typeof(IconDiamondSolid), ColorTheme.Type.Yellow)]

    [Serializable]
    public class ConditionCharacterIsInCover : TConditionCharacter
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is in cover {this.m_Character}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;

            CoverController coverController = character.GetComponent<CoverController>();
            return coverController != null && coverController.IsInCover();
        }
    }
}
