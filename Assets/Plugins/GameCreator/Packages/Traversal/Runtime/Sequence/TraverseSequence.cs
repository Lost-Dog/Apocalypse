using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class TraverseSequence : Sequence
    {
        public const int TRACK_MOTION_WARPING_POSITION = 0;
        public const int TRACK_MOTION_WARPING_ROTATION = 1;
        public const int TRACK_GRAVITY = 2;
        public const int TRACK_INSTRUCTIONS = 3;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private TimeMode m_TimeMode;
        [NonSerialized] private ICancellable m_Cancellable;

        [NonSerialized] private float m_Duration;
        [NonSerialized] private AnimationClip m_Animation;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override TimeMode TimeMode => this.m_TimeMode;

        public override float Duration => this.m_Duration;

        protected override ICancellable CancellationToken => this.m_Cancellable;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TraverseSequence() : base(new Track[]
        {
            new TrackTraverseMotionWarpingPosition(),
            new TrackTraverseMotionWarpingRotation(),
            new TrackTraverseGravity(),
            new TrackDefault()
        }) { }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async Task Run(
            TimeMode mode, 
            float speed, 
            float duration,
            AnimationClip animation,
            ICancellable cancellable,
            Args args)
        {
            this.m_TimeMode = mode;
            this.m_Cancellable = cancellable;
            this.m_Animation = animation;
            
            this.Speed = speed;
            this.m_Duration = duration;
            await this.DoRun(args);
        }

        public void Cancel(Args args)
        {
            this.DoCancel(args);
        }
    }
}