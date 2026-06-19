// ============================================================================================
// File: StrideWarpJob.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-12-06: Created this file.
// 
//     Contains the StrideWarperJob struct for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2019 Kenneth Claassen. All rights reserved.
// ============================================================================================
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief A burst compiled and multithreaded job which calculates stride warping and returns 
    *  the results
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct StrideWarpJob : IJob
    {
        [ReadOnly] public float SpeedWarp;
        [ReadOnly] public float CharPositionY;
        [ReadOnly] public float Offset;
        [ReadOnly] public float StrideSmoothing;
        [ReadOnly] public float3 HipPosition;
        [ReadOnly] public float3 LeftThighPosition;
        [ReadOnly] public float3 RightThighPosition;
        [ReadOnly] public float3 LeftFootPosition;
        [ReadOnly] public float3 RightFootPosition;
        [ReadOnly] public float3 WarpDir;

        public NativeArray<float3> Results; //Index 0 - Hip Adjust, 1 - Left Foot Pos, 2 - Right Foot Pos

        //============================================================================================
        /**
        *  @brief Execution Kernel for the StrideWarpJob
        *         
        *********************************************************************************************/
        public void Execute()
        {
            //float4x4 centerMatrix = StrideWarp.CalculateCenterMatrix(HipPosition, WarpDir, Offset, CharPositionY);

            //Find the center matrix
            quaternion centerOrient = quaternion.LookRotation(math.normalize(WarpDir), new float3(0f, 1f, 0f));

            float3 offset = math.mul(centerOrient, new float3(0f, 0f, Offset));
            offset = StrideWarp.MoveTowards(offset, Results[3], StrideSmoothing); //Need the last offset and a smoothing value
            Results[3] = offset;

            float3 centerPosition = new float3(HipPosition.x, CharPositionY, HipPosition.z) + offset;

            float4x4 centerMatrix = float4x4.TRS(centerPosition, centerOrient, new float3(1f));
            float4x4 centerMatrixInverse = math.inverse(centerMatrix);

            //*****Left foot*****
            float3 newLeftFootPosition = StrideWarp.ProcessStride(SpeedWarp, LeftThighPosition, 
                LeftFootPosition, centerMatrix, centerMatrixInverse);
            float leftHeightDelta = newLeftFootPosition.y - LeftFootPosition.y;

            //*****Right foot*****
            float3 newRightFootPosition = StrideWarp.ProcessStride(SpeedWarp, RightThighPosition, 
                RightFootPosition, centerMatrix, centerMatrixInverse);
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
    }//End of job struct:  StrideWarpJob
}//End of namespace: AnimationUprising.Strider
