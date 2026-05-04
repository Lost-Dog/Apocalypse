using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using RT;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is At Edge")]
    [Description("Returns true if the Character is at the specified edge of the cover")]

    [Category("Cover/Is At Edge")]

    [Keywords("Cover", "Edge", "Character")]

    [Image(typeof(IconDiamondSolid), ColorTheme.Type.Red)]

    [Serializable]
    public class ConditionCharacterIsAtEdge : TConditionCharacter
    {
        public enum EdgeType { Left, Right }

        [SerializeField] private EdgeType edgeToCheck;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is at {edgeToCheck} edge {this.m_Character}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;

            CoverController coverController = character.GetComponent<CoverController>();
            if (coverController == null || !coverController.IsInCover()) return false;

            // Check if the player is at the specified edge
            switch (edgeToCheck)
            {
                case EdgeType.Left:
                    return coverController.IsAtLeftEdge();
                case EdgeType.Right:
                    return coverController.IsAtRightEdge();
                default:
                    return false;
            }
        }
    }
}
