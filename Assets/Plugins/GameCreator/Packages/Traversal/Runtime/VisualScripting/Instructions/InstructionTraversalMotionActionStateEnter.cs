using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Motion Action State Enter")]
    [Description("Tells a character to start a Motion Action State on its current Traverse object")]

    [Category("Traversal/Actions/Try Motion Action State Enter")]

    [Keywords("Peek", "Aim", "Look")]
    [Image(typeof(IconTraverseAction), ColorTheme.Type.Blue, typeof(OverlayTick))]
    
    [Serializable]
    public class InstructionTraversalMotionActionStateEnter : Instruction
    {
        [SerializeField] private PropertyGetString m_State = GetStringId.Create("my-state-id");
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        
        public override string Title => $"Motion Action State enter {this.m_State} on {this.m_Character}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            IdString stateId = new IdString(this.m_State.Get(args));
            character.Combat.RequestStance<TraversalStance>().TryStateEnter(stateId);
            
            return DefaultResult;
        }
    }
}
