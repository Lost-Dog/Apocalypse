using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Cancel Traverse")]
    [Description("Tries to cancel a Character currently traversing an Obstacle")]

    [Category("Traversal/Actions/Try Cancel Traverse")]

    [Keywords("Stop", "Exit")]
    [Image(typeof(IconTraverseAction), ColorTheme.Type.Blue, typeof(OverlayCross))]
    
    [Serializable]
    public class InstructionTraversalTryCancelTraverse : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public override string Title => $"Try Cancel Traverse of {this.m_Character}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            character.Combat.RequestStance<TraversalStance>().TryCancel(args);
            return DefaultResult;
        }
    }
}
