using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Traverse Link (Bi-Direction)")]
    [Description("Makes a Character traverse one of two Traverse Link based on their direction")]

    [Category("Traversal/Traverse Link (Bi-Direction)")]

    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseLink), ColorTheme.Type.Green)]
    
    [Serializable]
    public class InstructionTraversalRunTraverseLinkBiDirection : Instruction
    {
        private enum Direction
        {
            X,
            Y,
            Z
        }
        
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_Reference = new PropertyGetGameObject();
        [SerializeField] private Direction m_Direction = Direction.Z;
        
        [SerializeField] private PropertyGetGameObject m_TraverseLink1 = new PropertyGetGameObject();
        [SerializeField] private PropertyGetGameObject m_TraverseLink2 = new PropertyGetGameObject();
        
        [SerializeField] private bool m_WaitUntilFinish = true;
        
        public override string Title
        {
            get
            {
                string wait = this.m_WaitUntilFinish ? " and wait" : string.Empty;
                return $"Traverse {this.m_TraverseLink1} / {this.m_TraverseLink2} with {this.m_Character}{wait}";
            }
        }

        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;
            
            Transform reference = this.m_Reference.Get<Transform>(args);
            if (reference == null) return;

            float direction = Vector3.Dot(
                character.transform.forward,
                this.m_Direction switch
                {
                    Direction.X => reference.right,
                    Direction.Y => reference.up,
                    Direction.Z => reference.forward,
                    _ => throw new ArgumentOutOfRangeException()
                }
            );
            
            GameObject root = direction >= 0f
                ? this.m_TraverseLink1.Get(args)
                : this.m_TraverseLink2.Get(args);
            
            if (root == null) return;
            
            TraverseLink[] traverseList = root.GetComponentsInChildren<TraverseLink>();
            Args argsLink = new Args(character, character);
            
            foreach (TraverseLink link in traverseList)
            {
                if (link == null) continue;
                if (!link.isActiveAndEnabled) continue;
                
                argsLink.ChangeSelf(link.gameObject);
                if (!link.Motion.CanUse(argsLink)) continue;
                
                Task task = link.Run(character);
                if (this.m_WaitUntilFinish) await task;
                
                return;
            }
        }
    }
}
