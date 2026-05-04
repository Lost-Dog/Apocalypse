using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Motion Jump")]
    [Description("Tells a Character to do a Motion Jump on its current Traverse object")]

    [Category("Traversal/Actions/Try Motion Jump")]
    
    [Keywords("Jump")]
    [Image(typeof(IconTraverseAction), ColorTheme.Type.Blue, typeof(OverlayArrowUp))]
    
    [Serializable]
    public class InstructionTraversalMotionActionJump : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public override string Title => $"Traverse Jump on {this.m_Character}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            character.Combat.RequestStance<TraversalStance>().TryJump();
            
            return DefaultResult;
        }
    }
}
