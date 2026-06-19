using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class ClipTraverseMotionWarpingRotation : ClipTraverseMotionWarpingBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Quaternion m_CharacterStartRotation;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClipTraverseMotionWarpingRotation() : this(0f, DEFAULT_TIME)
        { }

        public ClipTraverseMotionWarpingRotation(float time) : base(time, 0f)
        { }
        
        public ClipTraverseMotionWarpingRotation(float time, float duration) : base(time, duration)
        { }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override void OnStart(ITrack track, Args args)
        {
            base.OnStart(track, args);
            
            if (this.m_Character == null) return;
            if (this.m_TraverseLink == null) return;

            this.m_Character.CanUseRootMotionRotation = false;
            this.m_CharacterStartRotation = this.m_Character.transform.rotation;
        }

        protected override void OnComplete(ITrack track, Args args)
        {
            base.OnComplete(track, args);
            
            if (this.m_Character == null) return;
            this.m_Character.CanUseRootMotionRotation = true;
        }

        protected override void OnCancel(ITrack track, Args args)
        {
            base.OnCancel(track, args);
            
            if (this.m_Character == null) return;
            this.m_Character.CanUseRootMotionRotation = true;
        }

        protected override void OnUpdate(ITrack track, Args args, float t)
        {
            base.OnUpdate(track, args, t);

            if (this.m_Character == null) return;
            if (this.m_TraverseLink == null) return;
            
            Quaternion nextRotation = this.m_Mode switch
            {
                WarpMode.TowardsTarget => this.UpdateTowardsPoint(t),
                WarpMode.FromPointToPoint => this.UpdateFromPointToPoint(t),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            this.m_Character.Driver.SetRotation(nextRotation);
        }

        private Quaternion UpdateTowardsPoint(float t)
        {
            float ratio = Easing.GetEase(this.m_Easing, 0f, 1f, t);
            
            return Quaternion.Lerp(
                this.m_CharacterStartRotation,
                (this.m_TraverseLink.Transform.rotation * this.m_TraverseData.rotationB).normalized,
                ratio
            );
        }
        
        private Quaternion UpdateFromPointToPoint(float t)
        {
            if (t < this.m_TransitionIn)
            {
                float t1 = Mathf.InverseLerp(0f, this.m_TransitionIn, t);
                float ratio1 = Easing.GetEase(this.m_TransitionInEase, 0f, 1f, t1);
                
                return Quaternion.Lerp(
                    this.m_CharacterStartRotation,
                    (this.m_TraverseLink.Transform.rotation * this.m_TraverseData.rotationA).normalized,
                    ratio1
                );
            }
            
            float t2 = Mathf.InverseLerp(this.m_TransitionIn, 1f, t);
            float ratio2 = Easing.GetEase(this.m_Easing, 0f, 1f, t2);
            
            return Quaternion.Lerp(
                (this.m_TraverseLink.Transform.rotation * this.m_TraverseData.rotationA).normalized,
                (this.m_TraverseLink.Transform.rotation * this.m_TraverseData.rotationB).normalized,
                ratio2
            );
        }

        private static Vector3 Direction(Transform transform, float yaw)
        {
            return Quaternion.Euler(0f, yaw, 0f) * transform.forward;
        }
    }
}