using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Traverse Interactive clear Connections")]
    [Description("Clears all connections from a Traverse Interactive")]

    [Category("Traversal/Connections/Traverse Interactive clear Connections")]

    [Keywords("Connect")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Yellow, typeof(OverlayMinus))]
    
    [Serializable]
    public class InstructionTraversalInteractiveConnectionsClear : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_TraverseInteractive = GetGameObjectInstance.Create();
        
        public override string Title => $"Clear connections from {this.m_TraverseInteractive}";
        
        protected override Task Run(Args args)
        {
            TraverseInteractive traverseInteractive = this.m_TraverseInteractive.Get<TraverseInteractive>(args);
            if (traverseInteractive != null)
            {
                traverseInteractive.Connections.Clear();
            }
            
            return DefaultResult;
        }
    }
}
