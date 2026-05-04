using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Traverse Interactive remove Connection")]
    [Description("Removes a Traverse from a Traverse Interactive Connection list")]

    [Category("Traversal/Connections/Traverse Interactive remove Connection")]

    [Keywords("Connect")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Yellow, typeof(OverlayMinus))]
    
    [Serializable]
    public class InstructionTraversalInteractiveConnectionRemove : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_TraverseInteractive = GetGameObjectInstance.Create();

        [SerializeField]
        private PropertyGetGameObject m_Connection = GetGameObjectInstance.Create();
        
        public override string Title => $"Remove {this.m_Connection} from {this.m_TraverseInteractive}";
        
        protected override Task Run(Args args)
        {
            TraverseInteractive traverseInteractive = this.m_TraverseInteractive.Get<TraverseInteractive>(args);
            if (traverseInteractive == null) return DefaultResult;
            
            Traverse connectTo = this.m_Connection.Get<Traverse>(args);
            if (connectTo == null) return DefaultResult;
            
            for (int i = traverseInteractive.Connections.Count - 1; i >= 0; --i)
            {
                if (traverseInteractive.Connections[i].Traverse == connectTo)
                {
                    traverseInteractive.Connections.RemoveAt(i);
                }
            }
            
            return DefaultResult;
        }
    }
}
