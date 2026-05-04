using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class RunTraverseSequence : TRun<TraverseSequence>
    {
        private const int PREWARM_COUNTER = 3;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private TraverseSequence m_Sequence = new TraverseSequence();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override TraverseSequence Value => this.m_Sequence;
        
        protected override GameObject Template
        {
            get
            {
                if (this.m_Template == null) this.m_Template = CreateTemplate(this.Value);
                return this.m_Template;
            }
        }

        // PUBLIC GETTERS: ------------------------------------------------------------------------

        public T GetTrack<T>() where T : ITrack
        {
            return this.m_Sequence.GetTrack<T>();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async Task Run(
            string name,
            TimeMode time, 
            float speed, 
            float duration,
            AnimationClip animation,
            ICancellable token, 
            Args args)
        {
            GameObject template = this.Template;
            RunnerConfig config = new RunnerConfig
            {
                Name = name,
                Cancellable = token
            };
            
            RunnerTraverseSequence runner = RunnerTraverseSequence.Pick<RunnerTraverseSequence>(
                template,
                config,
                PREWARM_COUNTER
            );
            
            if (runner == null) return;
            
            await runner.Value.Run(time, speed, duration, animation, config.Cancellable, args);
            if (runner != null) RunnerTraverseSequence.Restore(runner);
        }

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        public static GameObject CreateTemplate(TraverseSequence value)
        {
            return RunnerTraverseSequence.CreateTemplate<RunnerTraverseSequence>(value);
        }
    }
}