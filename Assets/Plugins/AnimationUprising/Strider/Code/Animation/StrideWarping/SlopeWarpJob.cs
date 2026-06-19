// ============================================================================================
// File: SlopeWarJob.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-12-06: Created this file.
// 
//     Contains the StrideWarperJob struct for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief A burst compiled job struct used for fast processing of the StrideWarpJob with the 
    *  additional feature of slope warping.
    *  
    *  Note: Slope warping is an experimental future feature which is disabled in the meantime.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct SlopeWarpJob : IJob
    {
        [ReadOnly] public float SpeedWarp;
        [ReadOnly] public float CharPositionY;
        [ReadOnly] public float Offset;
        [ReadOnly] public float Slope;
        [ReadOnly] public float3 HipPosition;
        [ReadOnly] public float3 LeftThighPosition;
        [ReadOnly] public float3 RightThighPosition;
        [ReadOnly] public float3 LeftFootPosition;
        [ReadOnly] public float3 RightFootPosition;
        [ReadOnly] public float3 RootDelta;
        [WriteOnly] public NativeArray<float3> Results; //Index 0 - Hip Adjust, 1 - Left Foot Pos, 2 - Right Foot Pos

        //============================================================================================
        /**
        *  @brief Execution Kernel for the StrideWarpJob
        *         
        *********************************************************************************************/
        public void Execute()
        {
            float3 direction = math.normalize(RootDelta);
            quaternion centerOrient = quaternion.LookRotation(direction, new float3(0f, 1f, 0f));

            float3 offset = math.mul(centerOrient, new float3(0f, 0f, Offset));
            float3 centerPosition = new float3(HipPosition.x, CharPositionY, HipPosition.z) + offset;

            float4x4 centerMatrix = float4x4.TRS(centerPosition, centerOrient, new float3(1f));
            float4x4 centerMatrixInverse = math.inverse(centerMatrix);

            float4x4 centerMatrixSlope = float4x4.TRS(centerPosition, math.mul(centerOrient, quaternion.RotateX(Slope)), new float3(1f));
            //float4x4 centerMatrixSlopeInverse = math.inverse(centerMatrixSlope);


            //*****Left foot*****
            float thighToFootLength = math.length(LeftThighPosition - LeftFootPosition);
            float4 newLeftFootPosition4 = math.mul(centerMatrixInverse, new float4(LeftFootPosition, 1f));

            //rotate center matrix

            if (SpeedWarp < 1f)
                newLeftFootPosition4 *= new float4(1f, math.max(0.2f, SpeedWarp), SpeedWarp, 1f);
            else
                newLeftFootPosition4.z *= SpeedWarp;

            newLeftFootPosition4 = math.mul(centerMatrixSlope, newLeftFootPosition4);
            float3 newLeftFootPosition = new float3(newLeftFootPosition4.x, newLeftFootPosition4.y, newLeftFootPosition4.z);

            //Push foot back towards hips to ensure the legs are never hyper-extended
            float3 footToThighVector = newLeftFootPosition - LeftThighPosition;
            float curLength = math.length(footToThighVector);
            if (curLength > thighToFootLength)
                newLeftFootPosition -= math.normalize(footToThighVector) * (curLength - thighToFootLength);

            float leftHeightDelta = newLeftFootPosition.y - LeftFootPosition.y;

            //*****Right foot*****
            thighToFootLength = math.length(RightThighPosition - RightFootPosition);
            float4 newRightFootPosition4 = math.mul(centerMatrixInverse, new float4(RightFootPosition, 1f));

            if (SpeedWarp < 1f)
                newRightFootPosition4 *= new float4(1f, math.max(0.2f, SpeedWarp), SpeedWarp, 1f);
            else
                newRightFootPosition4.z *= SpeedWarp;

            newRightFootPosition4 = math.mul(centerMatrixSlope, newRightFootPosition4);
            float3 newRightFootPosition = new float3(newRightFootPosition4.x, newRightFootPosition4.y, newRightFootPosition4.z);

            //Push foot back towards hips to ensure the legs are never hyper-extended
            footToThighVector = newRightFootPosition - RightThighPosition;
            curLength = math.length(footToThighVector);
            if (curLength > thighToFootLength)
                newRightFootPosition -= math.normalize(footToThighVector) * (curLength - thighToFootLength);

            float rightHeightDelta = newRightFootPosition.y - RightFootPosition.y;

            //Calculate hip height adjustment
            if (rightHeightDelta > leftHeightDelta)
            {
                newLeftFootPosition.y += rightHeightDelta - leftHeightDelta;
                Results[0] = -rightHeightDelta;
            }
            else
            {
                newRightFootPosition.y += leftHeightDelta - rightHeightDelta;
                Results[0] = -leftHeightDelta;
            }

            Results[1] = newLeftFootPosition;
            Results[2] = newRightFootPosition;
        }

    }//End of struct job: SlopeWarpJob
}//End of namespace: AnimationUprising.Strider