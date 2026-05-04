using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Try Traverse to Direction")]
    [Description("Attempts to switch from its current Interactive to another based on the direction in screen space")]

    [Category("Traversal/Connections/Try Traverse to Direction")]
    
    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    
    [Serializable]
    public class InstructionTraversalTryConnectDirection : Instruction
    {
        private enum Swizzle
        {
            XY,
            XZ
        }

        private enum NoDirection
        {
            PickNone,
            PickClosest,
            TryJump
        }

        private enum NoConnections
        {
            Nothing,
            TryJump
        }
        
        [SerializeField]
        private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField]
        private PropertyGetGameObject m_Camera = GetGameObjectCameraMain.Create;

        [SerializeField]
        private PropertyGetDirection m_Direction = GetDirectionCharactersLocalInput.Create;
        
        [SerializeField] private Swizzle m_Swizzle = Swizzle.XZ;
        
        [SerializeField] private NoDirection m_NoDirection = NoDirection.PickNone;
        [SerializeField] private NoConnections m_NoConnections = NoConnections.Nothing;
        
        [SerializeField] private bool m_SkipTransition;
        
        public override string Title => $"Try switch Traverse connection on {this.m_Character}";
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            Camera camera = this.m_Camera.Get<Camera>(args);
            if (camera == null) return DefaultResult;
            
            TraversalStance stance = character.Combat.RequestStance<TraversalStance>();
            if (stance.Traverse == null) return DefaultResult;
            if (stance.InInteractiveTransition) return DefaultResult;
            
            TraverseInteractive traverseInteractive = stance.Traverse as TraverseInteractive;
            if (traverseInteractive == null) return DefaultResult;
            
            Vector3 direction3D = this.m_Direction.Get(args);
            Vector2 direction2D = this.m_Swizzle switch
            {
                Swizzle.XY => new Vector2(direction3D.x, direction3D.y).normalized,
                Swizzle.XZ => new Vector2(direction3D.x, direction3D.z).normalized,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (direction2D == Vector2.zero)
            {
                if (this.m_NoDirection == NoDirection.PickNone)
                {
                    return DefaultResult;
                }
                
                if (this.m_NoDirection == NoDirection.TryJump)
                {
                    character.Combat.RequestStance<TraversalStance>().TryJump();
                    return DefaultResult;
                }
            }
            
            Traverse candidate = traverseInteractive.GetCandidateConnection(character, camera, direction2D);

            if (candidate == null)
            {
                switch (this.m_NoConnections)
                {
                    case NoConnections.Nothing:
                        return DefaultResult;
                    
                    case NoConnections.TryJump:
                        stance.TryJump();
                        return DefaultResult;
                    
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            
            _ = Traverse.ChangeTo(traverseInteractive, candidate, character, this.m_SkipTransition);
            
            return DefaultResult;
        }
    }
}
