using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    internal class HumanFK
    {
        private const float EPSILON = 0.001f;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private readonly SpringFloat m_RatioSpine = new SpringFloat(0f);
        [NonSerialized] private readonly SpringFloat m_RatioLowerChest = new SpringFloat(0f);
        [NonSerialized] private readonly SpringFloat m_RatioUpperChest = new SpringFloat(0f);
        [NonSerialized] private readonly SpringFloat m_RatioShoulders = new SpringFloat(0f);
        [NonSerialized] private readonly SpringFloat m_RatioNeck = new SpringFloat(0f);
        [NonSerialized] private readonly SpringFloat m_RatioHead = new SpringFloat(0f);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Update(THumanRotations humanBones, Args args, float decay, float deltaTime)
        {
            if (humanBones == null)
            {
                this.m_RatioSpine.Target = 0f;
                this.m_RatioLowerChest.Target = 0f;
                this.m_RatioUpperChest.Target = 0f;
                this.m_RatioShoulders.Target = 0f;
                this.m_RatioNeck.Target = 0f;
                this.m_RatioHead.Target = 0f;
            }
            else
            {
                this.m_RatioSpine.Target = humanBones.GetRatioSpine(args);
                this.m_RatioLowerChest.Target = humanBones.GetRatioLowerChest(args);
                this.m_RatioUpperChest.Target = humanBones.GetRatioUpperChest(args);
                this.m_RatioShoulders.Target = humanBones.GetRatioShoulders(args);
                this.m_RatioNeck.Target = humanBones.GetRatioNeck(args);
                this.m_RatioHead.Target = humanBones.GetRatioHead(args);
            }
            
            this.m_RatioSpine.Update(decay, deltaTime);
            this.m_RatioLowerChest.Update(decay, deltaTime);
            this.m_RatioUpperChest.Update(decay, deltaTime);
            this.m_RatioShoulders.Update(decay, deltaTime);
            this.m_RatioNeck.Update(decay, deltaTime);
            this.m_RatioHead.Update(decay, deltaTime);
        }
        
        public void RotateBody(Animator animator, Action<Transform, float> operation, float ratio, bool sideR, bool sideL)
        {
            Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            Transform lowerChest = animator.GetBoneTransform(HumanBodyBones.Chest);
            Transform upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Transform shoulderR = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform shoulderL = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            
            float ratioSpine = this.m_RatioSpine.Current * ratio;
            float ratioLowerChest = this.m_RatioLowerChest.Current * ratio;
            float ratioUpperChest = this.m_RatioUpperChest.Current * ratio;
            float ratioShoulders = this.m_RatioShoulders.Current * ratio;
            float ratioNeck = this.m_RatioNeck.Current * ratio;
            float ratioHead = this.m_RatioHead.Current * ratio;
            
            float ratioSideR = sideR ? 1f : -1f;
            float ratioSideL = sideL ? 1f : -1f;
            
            if (spine != null && ratioSpine > EPSILON) operation.Invoke(spine, ratioSpine);
            if (lowerChest != null && ratioLowerChest > EPSILON) operation.Invoke(lowerChest, ratioLowerChest);
            if (upperChest != null && ratioUpperChest > EPSILON) operation.Invoke(upperChest, ratioUpperChest);
            if (shoulderR != null && ratioShoulders > EPSILON) operation.Invoke(shoulderR, ratioShoulders * ratioSideR);
            if (shoulderL != null && ratioShoulders > EPSILON) operation.Invoke(shoulderL, ratioShoulders * ratioSideL);
            if (neck != null && ratioNeck > EPSILON) operation.Invoke(neck, ratioNeck);
            if (head != null && ratioHead > EPSILON) operation.Invoke(head, ratioHead);
        }
    }
}