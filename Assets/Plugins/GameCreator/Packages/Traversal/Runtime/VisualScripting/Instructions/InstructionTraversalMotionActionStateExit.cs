using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Motion Action State Exit")]
    [Description("Tells a character to exit the current Motion State (if any)")]
    
    [Category("Traversal/Actions/Try Motion Action State Exit")]
    
    [Keywords("Peek", "Aim", "Look")]
    [Image(typeof(IconTraverseAction), ColorTheme.Type.Blue, typeof(OverlayCross))]
    
    [Serializable]
    public class InstructionTraversalMotionActionStateExit : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public override string Title => $"Motion Action State exit on {this.m_Character}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            character.Combat.RequestStance<TraversalStance>().TryStateExit();
            return DefaultResult;
        }
    }
}
