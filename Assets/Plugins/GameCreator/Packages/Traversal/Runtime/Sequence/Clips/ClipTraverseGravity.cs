using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class ClipTraverseGravity : Clip
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
    
        [Range(0f, 1f)]
        [SerializeField] private float m_Gravity = 1f;
        
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClipTraverseGravity() : this(0f, DEFAULT_TIME)
        { }

        public ClipTraverseGravity(float time) : base(time, 0f)
        { }
        
        public ClipTraverseGravity(float time, float duration) : base(time, duration)
        { }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override void OnStart(ITrack track, Args args)
        {
            base.OnStart(track, args);
            
            Traverse traverse = args.Self.Get<Traverse>();
            Character character = args.Target.Get<Character>();
            
            if (traverse == null) return;
            if (character == null) return;
            
            traverse.RefreshCollisions(character, false);
        }

        protected override void OnComplete(ITrack track, Args args)
        {
            base.OnComplete(track, args);
            
            Traverse traverse = args.Self.Get<Traverse>();
            Character character = args.Target.Get<Character>();
            
            if (traverse == null) return;
            if (character == null) return;
            
            traverse.RefreshCollisions(character, true);
        }

        protected override void OnUpdate(ITrack track, Args args, float t)
        {
            base.OnUpdate(track, args, t);
            
            Character character = args.Target.Get<Character>();
            character.Driver.SetGravityInfluence(MotionBase.GRAVITY_KEY, this.m_Gravity);
        }
    }
}