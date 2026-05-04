using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is Player's Active Target")]
    [Description("Returns true if the specified character is the player's current active target")]

    [Category("Characters/Combat/Is Player's Active Target")]

    [Parameter("Player", "The player to check for an active target")]
    [Parameter("Character", "The character to check if it's the player's active target")]

    [Keywords("Combat", "Target", "Player", "Check", "Active")]
    [Image(typeof(IconBullsEye), ColorTheme.Type.Red)]

    [Serializable]
    public class ConditionIsActiveTarget : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Player = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectInstance.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{this.m_Character} is Player's active target";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject playerObject = this.m_Player.Get(args);
            GameObject characterObject = this.m_Character.Get(args);

            if (playerObject == null || characterObject == null) return false;

            Character playerCharacter = playerObject.GetComponent<Character>();
            if (playerCharacter == null) return false;

            return playerCharacter.Combat.Targets.Primary == characterObject;
        }
    }
}
