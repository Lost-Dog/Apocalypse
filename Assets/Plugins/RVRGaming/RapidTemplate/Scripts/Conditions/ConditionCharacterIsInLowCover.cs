using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using RT;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is In Low Cover")]
    [Description("Returns true if the Character is in low cover")]

    [Category("Cover/Is In Low Cover")]

    [Keywords("Cover", "Combat", "Character", "Low")]

    [Image(typeof(IconDiamondSolid), ColorTheme.Type.Yellow)]

    [Serializable]
    public class ConditionCharacterIsInLowCover : TConditionCharacter
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is in low cover {this.m_Character}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;

            CoverController coverController = character.GetComponent<CoverController>();
            if (coverController == null || !coverController.IsInCover()) return false;

            Cover currentCover = coverController.GetCurrentCover();
            return currentCover != null && currentCover.IsLowCover();
        }
    }
}