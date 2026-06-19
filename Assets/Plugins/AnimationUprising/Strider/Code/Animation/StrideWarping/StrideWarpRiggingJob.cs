// ============================================================================================
// File: StrideWarpAnimJob.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-01-22: Created this file.
// 
//     Contains the StrideWarpAnimJob struct for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Experimental.Animations;
using UnityEngine.Animations;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief A burst compiled and multi threaded animation job which calculates stride warping and
    *  applies the foot targets to a Unity Animation rig (Animation Rigging Package.
    *  
    *  Note: This to issues with 'Unity Animation Rigging' and 'AnimationJobs' this feature is 
    *  disabled until stability is achieved.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct StrideWarpRiggingJob : IAnimationJob
    {
        [ReadOnly] public float SpeedWarp;
        [ReadOnly] public float CharPositionY;
        [ReadOnly] public float Offset;
        [ReadOnly] public float StrideSmoothing;
        [ReadOnly] public float SizeCompensation;
        [ReadOnly] public float HipDamping;
        [ReadOnly] public float HipAdjustCutoff;
        [ReadOnly] public float3 WarpDir;

        public NativeArray<float3> LastOffset;

        //Transform handles
        public TransformStreamHandle Hips;
        public TransformStreamHandle LeftThighJoint;
        public TransformStreamHandle RightThighJoint;
        public TransformStreamHandle LeftFoot;
        public TransformStreamHandle RightFoot;
        public TransformStreamHandle LeftFootIK;
        public TransformStreamHandle RightFootIK;
               
        //============================================================================================
        /**
        *  @brief Processes the animation for this animation job, taking in an animation stream of 
        *  previous animation flow and applying stride warping it. This particular job applies the 
        *  foot targets, calculated by stride warping, to a specified Unity Animation Rig.
        *  
        *  @param [AnimationStream] stream - the animation stream passed to this animation job
        *         
        *********************************************************************************************/
        public void ProcessAnimation(AnimationStream stream)
        {
            float3 hipPosition = Hips.GetPosition(stream);
            //float4x4 centerMatrix = StrideWarp.CalculateCenterMatrix(hipPosition, WarpDir, Offset, CharPositionY);

            float3 direction = math.normalize(WarpDir);
            quaternion centerOrient = quaternion.LookRotation(direction, new float3(0f, 1f, 0f));

            float3 offset = math.mul(centerOrient, new float3(0f, 0f, Offset));
            offset = math.lerp(offset, LastOffset[0], StrideSmoothing); //Need the last offset and a smoothing value
            LastOffset[0] = offset;

            float3 centerPosition = new float3(hipPosition.x, CharPositionY, hipPosition.z) + offset;

            float4x4 centerMatrix = float4x4.TRS(centerPosition, centerOrient, new float3(1f));
            float4x4 centerMatrixInverse = math.inverse(centerMatrix);

            //*****Left foot*****
            float3 leftThighPos = LeftThighJoint.GetPosition(stream);
            float3 leftFootPos = LeftFoot.GetPosition(stream);
            float3 newLeftFootPos = StrideWarp.ProcessStride(SpeedWarp, leftThighPos,
                leftFootPos, centerMatrix, centerMatrixInverse);

            float leftHeightDelta = newLeftFootPos.y - leftFootPos.y;

            //*****Right foot*****
            //Right Foot
            float3 rightThighPos = RightThighJoint.GetPosition(stream);
            float3 rightFootPos = RightFoot.GetPosition(stream);
            float3 newRightFootPos = StrideWarp.ProcessStride(SpeedWarp, rightThighPos,
                rightFootPos, centerMatrix, centerMatrixInverse);

            float rightHeightDelta = newRightFootPos.y - rightFootPos.y;

            //Adjust Hips
            float hipShift;
            if (rightHeightDelta > leftHeightDelta)
            {
                newLeftFootPos.y += rightHeightDelta - leftHeightDelta;
                hipShift = -rightHeightDelta;
            }
            else
            {
                newRightFootPos.y += leftHeightDelta - rightHeightDelta;
                hipShift = -leftHeightDelta;
            }

            if (hipShift < 0f)
            {
                hipShift = math.clamp(hipShift * HipDamping, -HipAdjustCutoff, 0f);
            }

            float3 hipShiftVector = new float3(0f, hipShift, 0f);

            Hips.SetPosition(stream, hipPosition + hipShiftVector);

            LeftFootIK.SetPosition(stream, newLeftFootPos + hipShiftVector);
            RightFootIK.SetPosition(stream, newRightFootPos + hipShiftVector);
            LeftFootIK.SetRotation(stream, LeftFoot.GetRotation(stream));
            RightFootIK.SetRotation(stream, RightFoot.GetRotation(stream));
        }

        //============================================================================================
        /**
        *  @brief Processes the root motion for this animation job. This particular animation job does
        *  not modify root motion.
        *  
        *  @param [AnimationStream] stream - the animation stream passed to this animation job
        *         
        *********************************************************************************************/
        public void ProcessRootMotion(AnimationStream stream)
        {
            stream.velocity *= SpeedWarp;
        }

    }//End of struct job: StrideWarpRiggingJob
}//End of namespace: AnimationUprising.Strider