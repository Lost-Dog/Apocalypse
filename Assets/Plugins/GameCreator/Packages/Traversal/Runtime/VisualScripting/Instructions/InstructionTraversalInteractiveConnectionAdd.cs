using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Traverse Interactive add Connection")]
    [Description("Adds a Traverse to a Traverse Interactive Connection list")]

    [Category("Traversal/Connections/Traverse Interactive add Connection")]

    [Keywords("Connect")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Yellow, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionTraversalInteractiveConnectionAdd : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_TraverseInteractive = GetGameObjectInstance.Create();

        [SerializeField]
        private PropertyGetGameObject m_Connection = GetGameObjectInstance.Create();
        
        [SerializeField]
        private PropertyGetDecimal m_MaxDistance = new PropertyGetDecimal(-1f);
        
        public override string Title => $"Connect {this.m_TraverseInteractive} to {this.m_Connection}";
        
        protected override Task Run(Args args)
        {
            TraverseInteractive traverseInteractive = this.m_TraverseInteractive.Get<TraverseInteractive>(args);
            if (traverseInteractive == null) return DefaultResult;
            
            Traverse connectTo = this.m_Connection.Get<Traverse>(args);
            if (connectTo == null) return DefaultResult;

            float maxDistance = (float) this.m_MaxDistance.Get(args);
            Connection connection = new Connection(maxDistance > 0f, maxDistance, connectTo);
            
            traverseInteractive.Connections.Add(connection);
            
            return DefaultResult;
        }
    }
}
