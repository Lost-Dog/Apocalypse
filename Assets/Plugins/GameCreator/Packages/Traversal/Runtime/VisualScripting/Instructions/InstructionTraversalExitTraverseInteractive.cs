using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Exit Traverse Interactive")]
    [Description("Makes a Character exit its current Traverse Interactive element")]

    [Category("Traversal/Exit Traverse Interactive")]

    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Green, typeof(OverlayCross))]
    
    [Serializable]
    public class InstructionTraversalExitTraverseInteractive : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        
        [SerializeField] private CompareGameObjectOrAny m_TraverseInteractive = new CompareGameObjectOrAny(
            true,
            GetGameObjectInstance.Create()
        );
        
        public override string Title
        {
            get
            {
                string interactive = this.m_TraverseInteractive.Any
                    ? "current Traverse Interactive"
                    : $"{this.m_TraverseInteractive}";
                
                return $"Exit {this.m_Character} {interactive}";
            }
        }

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            TraversalStance traversal = character.Combat.RequestStance<TraversalStance>();
            
            if (this.m_TraverseInteractive.Any)
            {
                if (traversal.Traverse is TraverseInteractive)
                {
                    traversal.ForceCancel();
                }
            }
            else
            {
                TraverseInteractive traverseInteractive = this.m_TraverseInteractive.Get<TraverseInteractive>(args);
                if (traverseInteractive != null && traverseInteractive == traversal.Traverse)
                {
                    traversal.ForceCancel();
                }
            }

            return DefaultResult;
        }
    }
}
