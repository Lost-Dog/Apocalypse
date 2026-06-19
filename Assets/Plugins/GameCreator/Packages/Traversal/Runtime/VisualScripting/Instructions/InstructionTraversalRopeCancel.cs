using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Cancel Rope")]
    [Description("Cancels a Rope component being thrown")]

    [Category("Traversal/Rope/Cancel Rope")]

    [Keywords("Grapple", "Hook", "Reel", "Stop")]
    [Image(typeof(IconTraverseRope), ColorTheme.Type.Yellow, typeof(OverlayCross))]
    
    [Serializable]
    public class InstructionTraversalRopeCancel : Instruction
    {
        private enum CancelMode
        {
            Reel,
            Stop
        }
        
        [SerializeField]
        private PropertyGetGameObject m_Rope = GetGameObjectPlayer.Create();

        [SerializeField] private CancelMode m_CancelMode = CancelMode.Reel;
        
        public override string Title => $"{this.m_CancelMode} Rope {this.m_Rope}";
        
        protected override Task Run(Args args)
        {
            Rope rope = this.m_Rope.Get<Rope>(args);
            if (rope == null) return DefaultResult;

            switch (this.m_CancelMode)
            {
                case CancelMode.Reel:
                    rope.StopWithReel();
                    break;
                
                case CancelMode.Stop:
                    rope.Stop();
                    break;
                
                default: throw new ArgumentOutOfRangeException();
            }
            
            return DefaultResult;
        }
    }
}
