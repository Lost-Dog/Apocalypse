using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Motion Action")]
    [Description("Tells a Character to do a Motion Action on its current Traverse object")]

    [Category("Traversal/Actions/Try Motion Action")]
    
    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseAction), ColorTheme.Type.Blue, typeof(OverlayBolt))]
    
    [Serializable]
    public class InstructionTraversalMotionActionRun : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetString m_Action = GetStringId.Create("my-action-id");

        public override string Title => $"Action {this.m_Action} on {this.m_Character}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            IdString actionId = new IdString(this.m_Action.Get(args));
            character.Combat.RequestStance<TraversalStance>().TryAction(actionId);
            
            return DefaultResult;
        }
    }
}
