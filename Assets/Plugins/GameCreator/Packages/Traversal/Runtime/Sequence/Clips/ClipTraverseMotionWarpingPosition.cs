using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class ClipTraverseMotionWarpingPosition : ClipTraverseMotionWarpingBase
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private float m_Lift;
        [SerializeField] private Easing.Type m_LiftEase = Easing.Type.Linear;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Vector3 m_CharacterStartPosition;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClipTraverseMotionWarpingPosition() : this(0f, DEFAULT_TIME)
        { }

        public ClipTraverseMotionWarpingPosition(float time) : base(time, 0f)
        { }
        
        public ClipTraverseMotionWarpingPosition(float time, float duration) : base(time, duration)
        { }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override void OnStart(ITrack track, Args args)
        {
            base.OnStart(track, args);
            
            if (this.m_Character == null) return;
            if (this.m_TraverseLink == null) return;

            this.m_Character.CanUseRootMotionPosition = false;
            this.m_CharacterStartPosition = this.m_TraverseLink.MotionLink.CharacterPosition(this.m_Character);
        }

        protected override void OnComplete(ITrack track, Args args)
        {
            base.OnComplete(track, args);
            
            if (this.m_Character == null) return;
            this.m_Character.CanUseRootMotionPosition = true;
        }

        protected override void OnCancel(ITrack track, Args args)
        {
            base.OnCancel(track, args);
            
            if (this.m_Character == null) return;
            this.m_Character.CanUseRootMotionPosition = true;
        }

        protected override void OnUpdate(ITrack track, Args args, float t)
        {
            base.OnUpdate(track, args, t);

            if (this.m_Character == null) return;
            if (this.m_TraverseLink == null) return;
            
            Vector3 nextPosition = this.m_Mode switch
            {
                WarpMode.TowardsTarget => this.UpdateTowardsPoint(t),
                WarpMode.FromPointToPoint => this.UpdateFromPointToPoint(t),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            Vector3 currentPosition = this.m_TraverseLink.MotionLink.CharacterPosition(this.m_Character);
            this.m_Character.Driver.AddPosition(nextPosition - currentPosition);
        }

        private Vector3 UpdateTowardsPoint(float t)
        {
            float ratioLift = Easing.GetEase(this.m_LiftEase, 0f, 1f, t);
            Vector3 normalLift = new Vector3(0f, -4f * ratioLift * ratioLift + 4f * ratioLift, 0f);
            
            float ratio = Easing.GetEase(this.m_Easing, 0f, 1f, t);
            Vector3 endPosition = this.m_TraverseData.positionB;
            return Vector3.Lerp(
                this.m_CharacterStartPosition,
                this.m_TraverseLink.Transform.TransformPoint(endPosition),
                ratio
            ) + normalLift * this.m_Lift;
        }
        
        private Vector3 UpdateFromPointToPoint(float t)
        {
            Vector3 startPosition = this.m_TraverseData.positionA;
            Vector3 endPosition = this.m_TraverseData.positionB;
            
            if (t < this.m_TransitionIn)
            {
                float t1 = Mathf.InverseLerp(0f, this.m_TransitionIn, t);
                float ratio1 = Easing.GetEase(this.m_TransitionInEase, 0f, 1f, t1);
                
                return Vector3.Lerp(
                    this.m_CharacterStartPosition,
                    this.m_TraverseLink.Transform.TransformPoint(startPosition),
                    ratio1
                );
            }
            
            float t2 = Mathf.InverseLerp(this.m_TransitionIn, 1f, t);
            
            float ratioLift = Easing.GetEase(this.m_LiftEase, 0f, 1f, t2);
            Vector3 normalLift = new Vector3(0f, -4f * ratioLift * ratioLift + 4f * ratioLift, 0f);
            
            float ratio2 = Easing.GetEase(this.m_Easing, 0f, 1f, t2);
            return Vector3.Lerp(
                this.m_TraverseLink.Transform.TransformPoint(startPosition),
                this.m_TraverseLink.Transform.TransformPoint(endPosition),
                ratio2
            ) + normalLift * this.m_Lift;
        }
    }
}