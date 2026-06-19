// ============================================================================================
// File: StrideWarp.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-01-22: Created this file.
// 
//     Contains the StrideWarp static library functions for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;
using Unity.Mathematics;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief Static class library for stride warping calculations
    *         
    *********************************************************************************************/
    public static class StrideWarp
    {
        //============================================================================================
        /**
        *  @brief Calculates and returns the center matrix where the stride should be warped from
        *  
        *  @param [float3] a_hipPos - position of the hips
        *  @param [float3] a_rootDelta - the root motion delta
        *  @param [float] a_offset - the offset magnitude of the stride 
        *  @param [float] a_charPositionY - the character position on the y axis
        *  
        *  @return float4x4 - the center matrix for stride warping
        *         
        *********************************************************************************************/
        public static float4x4 CalculateCenterMatrix(float3 a_hipPos, float3 a_rootDelta, float a_offset, float a_charPositionY)
        {
            float3 direction = math.normalize(a_rootDelta);
            quaternion centerOrient = quaternion.LookRotation(direction, new float3(0f, 1f, 0f));

            float3 offset = math.mul(centerOrient, new float3(0f, 0f, a_offset));

            float3 centerPosition = new float3(a_hipPos.x, a_charPositionY, a_hipPos.z) + offset;

            return float4x4.TRS(centerPosition, centerOrient, new float3(1f));
        }

        //============================================================================================
        /**
        *  @brief Calculates and returns the desired foot location required to warp the stride of a 
        *  single leg. The intention is that this return position is an IKTarget for the leg

        *  @param [float] a_speedWarp - the amount to warp
        *  @param [float3] a_thighPos - the global position of the thigh
        *  @param [float3] a_footPos - the global position of the foot
        *  @param [float4x4] a_centerMatrix - the center matrix
        *  @param [float4x4] a_centerMatrixInv - the inverse of the ceter matrix
        *  
        *  @return float3 - The new target position for the foot (ik foot target)
        *         
        *********************************************************************************************/
        public static float3 ProcessStride(float a_speedWarp, float3 a_thighPos, float3 a_footPos,
             float4x4 a_centerMatrix, float4x4 a_centerMatrixInv)
        {
            //Warp stride outwards from the center matrix
            float thighToFootLength = math.length(a_thighPos - a_footPos);
            float4 newFootPosition4 = math.mul(a_centerMatrixInv, new float4(a_footPos, 1f));

            if (a_speedWarp < 1f)
                newFootPosition4 *= new float4(1f, math.max(0.2f, a_speedWarp), a_speedWarp, 1f);
            else
                newFootPosition4.z *= a_speedWarp;

            newFootPosition4 = math.mul(a_centerMatrix, newFootPosition4);
            float3 newFootPosition = new float3(newFootPosition4.x, newFootPosition4.y, newFootPosition4.z);

            //Push foot back towards hips to ensure legs are never hyper extended
            float3 footToThighVector = newFootPosition - a_thighPos;
            float curLength = math.length(footToThighVector);
            if (curLength > thighToFootLength)
                newFootPosition -= math.normalize(footToThighVector) * (curLength - thighToFootLength);

            return newFootPosition;
        }

        //============================================================================================
        /**
        *  @brief Calculates and returns the desired foot location required to warp the stride of a 
        *  single leg. The intention is that this return position is an IKTarget for the leg

        *  @param [float] a_speedWarp - the amount to warp
        *  @param [float3] a_thighPos - the global position of the thigh
        *  @param [float3] a_footPos - the global position of the foot
        *  @param [float] a_maxLimbExtenstion - the maximum percentage of limb length allowed to extend
        *  @param [float] a_limbLength - the length of the limb from base to mid to tip
        *  @param [float4x4] a_centerMatrix - the center matrix
        *  @param [float4x4] a_centerMatrixInv - the inverse of the ceter matrix
        *  
        *  @return float3 - The new target position for the foot (ik foot target)
        *         
        *********************************************************************************************/
        public static float3 ProcessStride(float a_speedWarp, float3 a_thighPos, float3 a_footPos,
             float a_maxLimbExtension, float a_limbLength, float4x4 a_centerMatrix, float4x4 a_centerMatrixInv)
        {
            //Warp stride outwards from the center matrix
            float thighToFootLength = math.length(a_thighPos - a_footPos);
            float a_maxLimbLength = thighToFootLength + (a_limbLength - thighToFootLength) * a_maxLimbExtension;
            math.clamp(a_maxLimbLength, 0f, a_maxLimbLength);

            float4 newFootPosition4 = math.mul(a_centerMatrixInv, new float4(a_footPos, 1f));

            if (a_speedWarp < 1f)
                newFootPosition4 *= new float4(1f, math.max(0.2f, a_speedWarp), a_speedWarp, 1f);
            else
                newFootPosition4.z *= a_speedWarp;

            newFootPosition4 = math.mul(a_centerMatrix, newFootPosition4);
            float3 newFootPosition = new float3(newFootPosition4.x, newFootPosition4.y, newFootPosition4.z);

            //Push foot back towards hips to ensure legs are never hyper extended
            float3 footToThighVector = newFootPosition - a_thighPos;
            float curLength = math.length(footToThighVector);

            if (curLength > a_maxLimbLength)
                newFootPosition -= math.normalize(footToThighVector) * (curLength - a_maxLimbLength);

            return newFootPosition;
        }

        //============================================================================================
        /**
        *  @brief Calculates the length of a limb from transform to transform
        *  
        *  @param [Transform] a_foot - the transform of the foot (or tip) that belongs to this limb
        *  @param [Transform] a_base - the transform of the thigh (or base) that belongs to this limb
        *  
        *  @return float - the cumulative length of the leg (i.e. length of all the bones from the 
        *  foot to the hip
        *         
        *********************************************************************************************/
        public static float CalculateLimbLength(Transform a_tip, Transform a_base)
        {
            if (a_tip == null)
                return 0f;

            Transform joint = a_tip;
            float legLength = 0f;
            bool baseFound = false;
            for (int i = 0; i < 10; ++i)
            {
                legLength += joint.localPosition.magnitude;

                joint = joint.parent;

                if (joint == a_base || joint == null)
                {
                    baseFound = true;
                    break;
                }
            }

            if (!baseFound)
            {
                return -1f;
            }

            return legLength;
        }

        public static float3 MoveTowards(float3 from, float3 to, float maxDelta)
        {
            float3 totalVector = to - from;
            float mag = math.length(totalVector);
            if(mag <= maxDelta || mag == 0f)
                return to;

            return from + totalVector / mag * maxDelta;
        }


    }//End of static class: StrideWarp
}//End of namespace: AnimationUprising.Strider