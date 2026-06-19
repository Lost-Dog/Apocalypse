using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Traverse to Connection")]
    [Description("Attempts to switch from a Character's current Traverse to another one in its Connection list")]

    [Category("Traversal/Connections/Try Traverse to Connection")]

    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Green, typeof(OverlayDot))]
    
    [Serializable]
    public class InstructionTraversalTryConnectTo : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField]
        private PropertyGetGameObject m_Connection = GetGameObjectInstance.Create();
        
        [SerializeField] private bool m_SkipTransition;
        
        public override string Title => $"Try Connect on {this.m_Character} to {this.m_Connection}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            TraversalStance stance = character.Combat.RequestStance<TraversalStance>();
            if (stance.Traverse == null) return DefaultResult;
            if (stance.InInteractiveTransition) return DefaultResult;
            
            TraverseInteractive traverseInteractive = stance.Traverse as TraverseInteractive;
            if (traverseInteractive == null) return DefaultResult;

            GameObject connectionParent = this.m_Connection.Get(args);
            if (connectionParent == null) return DefaultResult;

            Traverse[] candidates =connectionParent.GetComponentsInChildren<Traverse>();
            foreach (Traverse candidate in candidates)
            {
                if (traverseInteractive.IsCandidateConnection(character, candidate))
                {
                    _ = Traverse.ChangeTo(traverseInteractive, candidate, character, this.m_SkipTransition);
                    return DefaultResult;
                }
            }
            
            return DefaultResult;
        }
    }
}
