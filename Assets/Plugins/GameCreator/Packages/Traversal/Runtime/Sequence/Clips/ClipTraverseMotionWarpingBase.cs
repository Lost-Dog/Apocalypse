using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public abstract class ClipTraverseMotionWarpingBase : Clip
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] protected WarpMode m_Mode = WarpMode.TowardsTarget;
        
        [SerializeField] [Range(0f, 1f)] protected float m_TransitionIn = 0.25f;
        [SerializeField] protected Easing.Type m_TransitionInEase = Easing.Type.QuadInOut;
        
        [SerializeField] protected Easing.Type m_Easing = Easing.Type.QuadInOut;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] protected Character m_Character;
        [NonSerialized] protected TraverseLink m_TraverseLink;
        [NonSerialized] protected TraverseLinkData m_TraverseData;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public WarpMode Mode => this.m_Mode;
        
        public float TransitionIn => this.m_TransitionIn;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected ClipTraverseMotionWarpingBase() : this(0f, DEFAULT_TIME)
        { }

        protected ClipTraverseMotionWarpingBase(float time) : base(time, 0f)
        { }
        
        protected ClipTraverseMotionWarpingBase(float time, float duration) : base(time, duration)
        { }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override void OnStart(ITrack track, Args args)
        {
            base.OnStart(track, args);
            
            this.m_Character = args.Target.Get<Character>();
            this.m_TraverseLink = args.Self.Get<TraverseLink>();
            this.m_TraverseData = default;
            
            if (this.m_Character == null) return;
            if (this.m_TraverseLink == null) return;

            TraverseLinkType traverseLinkType = this.m_TraverseLink.Type;
            if (traverseLinkType == null) return;
            
            this.m_TraverseData = traverseLinkType.ToTraverseLinkData(
                this.m_Character,
                this.m_TraverseLink
            );
        }
    }
}