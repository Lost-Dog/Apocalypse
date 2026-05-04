using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is Moving Direction")]
    [Description("Returns true if the Character is currently in an active moving phase and in the specified direction")]

    [Category("Characters/Navigation/Is Moving Direction")]

    [Keywords("Translate", "Towards", "Destination", "Target", "Follow", "Walk", "Run")]

    [Image(typeof(IconCharacterRun), ColorTheme.Type.Yellow, typeof(OverlayArrowRight))]
    [Serializable]
    public class ConditionCharacterIsMovingDirection : TConditionCharacter
    {
        private const float MOVE_THRESHOLD = 0.1f;

        // Movement Direction Enum
        public enum MovementDirection
        {
            Forward,
            Backward
        }

        // Add dropdown for selecting the direction of movement
        [SerializeField] private MovementDirection direction = MovementDirection.Forward;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is Moving {this.m_Character} {this.direction}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;

            Vector3 moveDirection = character.Driver.WorldMoveDirection;

            if (moveDirection.magnitude <= MOVE_THRESHOLD) return false;

            // Determine if the character is moving forward or backward based on the selected direction
            float dotProduct = Vector3.Dot(character.transform.forward, moveDirection.normalized);

            if (this.direction == MovementDirection.Forward)
            {
                // Forward movement
                return dotProduct > 0;
            }
            else
            {
                // Backward movement
                return dotProduct < 0;
            }
        }
    }
}
