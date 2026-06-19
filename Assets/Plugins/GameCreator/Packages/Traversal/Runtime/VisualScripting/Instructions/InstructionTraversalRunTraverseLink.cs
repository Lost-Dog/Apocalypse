using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Traverse Link")]
    [Description("Makes a Character traverse a specific Traverse Link")]

    [Category("Traversal/Traverse Link")]

    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseLink), ColorTheme.Type.Green)]
    
    [Serializable]
    public class InstructionTraversalRunTraverseLink : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_TraverseLink = new PropertyGetGameObject();
        
        [SerializeField] private bool m_WaitUntilFinish = true;
        
        public override string Title
        {
            get
            {
                string wait = this.m_WaitUntilFinish ? " and wait" : string.Empty;
                return $"Traverse {this.m_TraverseLink} with {this.m_Character}{wait}";
            }
        }

        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;
            
            GameObject root = this.m_TraverseLink.Get(args);
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
