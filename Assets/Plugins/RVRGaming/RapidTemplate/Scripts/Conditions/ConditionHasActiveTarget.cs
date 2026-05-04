using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Has Active Target")]
    [Description("Returns true if the character has an active combat target")]

    [Category("Characters/Combat/Has Active Target")]

    [Parameter("Character", "The character to check for an active combat target")]

    [Keywords("Combat", "Target", "Check")]

    [Image(typeof(IconBullsEye), ColorTheme.Type.Yellow)]
    [Serializable]
    public class ConditionHasActiveTarget : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Character {this.m_Character} has active target";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject characterObject = this.m_Character.Get(args);
            if (characterObject == null) return false;

            Character character = characterObject.GetComponent<Character>();
            return character != null && character.Combat.Targets.Primary != null;
        }
    }
}
