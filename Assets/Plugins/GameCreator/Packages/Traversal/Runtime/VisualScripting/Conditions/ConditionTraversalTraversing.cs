using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Is Character Traversing")]
    [Description("Returns true the chosen Character is currently traversing an obstacle")]

    [Category("Traversal/Is Character Traversing")]
    
    [Keywords("Obstacle", "Vault", "Jump", "Climb", "Cover")]

    [Image(typeof(IconTraverseLink), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionTraversalTraversing : TConditionCharacter
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private CompareGameObjectOrAny m_Traverse = new CompareGameObjectOrAny(
            true,
            GetGameObjectInstance.Create()
        );
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary => $"is {this.m_Character} traversing {this.m_Traverse}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;

            Traverse current = character.Combat.RequestStance<TraversalStance>().Traverse;
            return this.m_Traverse.Any
                ? current != null
                : current == this.m_Traverse.Get<Traverse>(args);
        }
    }
}
