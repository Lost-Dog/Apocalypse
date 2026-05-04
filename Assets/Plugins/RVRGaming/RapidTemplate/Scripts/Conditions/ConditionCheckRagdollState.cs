using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Check if Ragdoll is Active")]
    [Description("Returns true if the character is in a ragdoll state")]

    [Category("Characters/Ragdoll/Check Ragdoll")]

    [Parameter("Character", "The character to check for an active combat target")]

    [Keywords("Characters", "Ragdoll", "Dead", "Kill", "Die")]
    [Image(typeof(IconSkeleton), ColorTheme.Type.Blue)]

    [Serializable]
    public class ConditionCheckRagdollState : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Character {this.m_Character} is in a ragdoll state";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            return character != null && character.Ragdoll.IsRagdoll;
        }
    }
}
