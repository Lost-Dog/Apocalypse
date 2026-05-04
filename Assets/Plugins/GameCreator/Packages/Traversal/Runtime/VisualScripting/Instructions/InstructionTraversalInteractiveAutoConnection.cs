using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Traverse Interactive Auto-Connection")]
    [Description("Automatically generates a list of Connections based on the specified filter")]

    [Category("Traversal/Connections/Traverse Interactive Auto-Connection")]

    [Keywords("Connect")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Yellow, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionTraversalInteractiveAutoConnection : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_TraverseInteractive = GetGameObjectInstance.Create();
        
        [SerializeField]
        private PropertyGetDecimal m_MaxDistance = new PropertyGetDecimal(-1f);
        
        [SerializeField]
        private ConditionList m_Filter = new ConditionList();
        
        public override string Title => $"Generate connections for {this.m_TraverseInteractive}";
        
        protected override Task Run(Args args)
        {
            TraverseInteractive traverseInteractive = this.m_TraverseInteractive.Get<TraverseInteractive>(args);
            if (traverseInteractive == null) return DefaultResult;

            float maxDistance = (float) this.m_MaxDistance.Get(args);
            
            Traverse[] candidates = Object.FindObjectsByType<Traverse>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            Args argsCandidate = new Args(traverseInteractive, traverseInteractive);
            
            foreach (Traverse candidate in candidates)
            {
                argsCandidate.ChangeTarget(candidate);
                if (this.m_Filter.Check(argsCandidate, CheckMode.And))
                {
                    Connection connection = new Connection(true, maxDistance, candidate);
                    traverseInteractive.Connections.Add(connection);
                }
            }
            
            return DefaultResult;
        }
    }
}
