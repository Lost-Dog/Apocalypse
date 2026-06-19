// ============================================================================================
// File: TwoBoneIKAnimJobs.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-01-22: Created this file.
// 
//     Contains the AnimationJob portion of the 'AnimationUprising' TwoBoneIK library
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine.Experimental.Animations;
using UnityEngine.Animations;

namespace AnimationUprising.IK
{
    //For all following jobs, use the key below to decern variable names
    // 'a' - Base Joint
    // 'b' - Mid Joint
    // 'c' - End Joint
    // 't' - IK Target
    // '_gr' - Global Rotation
    // '_gl' - Local Rotation
    // 'l_' - Bone length
    // 'eps' - epsilon / tolerance / error threshold
    // 'Tr' - Transform

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job which performs and applies two bone IK to TransformStreamHandles
    *  in the animation update. IKTarget position is set directly
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKAnimJob : IAnimationJob
    {
        [ReadOnly] public TransformStreamHandle c_Tr;
        [ReadOnly] public float3 t;
        [ReadOnly] public float eps;

        public TransformStreamHandle a_Tr;
        public TransformStreamHandle b_Tr;

        public void ProcessAnimation(AnimationStream stream)
        {
            float3 c = c_Tr.GetPosition(stream);

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.GetPosition(stream);
            float3 b = b_Tr.GetPosition(stream);
            
            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion a_gr = a_Tr.GetRotation(stream);
            quaternion b_gr = b_Tr.GetRotation(stream);

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.SetLocalRotation(stream, math.mul(a_Tr.GetLocalRotation(stream), math.mul(r0, r2)));
            b_Tr.SetLocalRotation(stream, math.mul(b_Tr.GetLocalRotation(stream), r1));
        }

        public void ProcessRootMotion(AnimationStream stream) { }

    } //End of struct Job: TwoBoneIKAnimJob

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job, with hint, which performs and applies two bone IK to 
    *  TransformStreamHandles in the animation update. IKTarget position and hint is set directly
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKHintAnimJob : IAnimationJob
    {
        [ReadOnly] public TransformStreamHandle c_Tr;
        [ReadOnly] public float3 t;
        [ReadOnly] public float3 h;
        [ReadOnly] public float eps;

        public TransformStreamHandle a_Tr;
        public TransformStreamHandle b_Tr;

        public void ProcessAnimation(AnimationStream stream)
        {
            float3 a = a_Tr.GetPosition(stream);
            float3 b = b_Tr.GetPosition(stream);
            float3 c = c_Tr.GetPosition(stream);

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            quaternion a_gr = a_Tr.GetRotation(stream);
            quaternion b_gr = b_Tr.GetRotation(stream);

            //Hint axis
            float3 d = math.mul(b_gr, math.normalize(h - (a + c) / 2f));

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.SetLocalRotation(stream, math.mul(a_Tr.GetLocalRotation(stream), math.mul(r0, r2)));
            b_Tr.SetLocalRotation(stream, math.mul(b_Tr.GetLocalRotation(stream), r1));
        }

        public void ProcessRootMotion(AnimationStream stream) { }

    }//End of struct Job: TwoBoneIKHintAnimJob

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job which performs and applies two bone IK to TransformStreamHandles
    *  in the animation update. IKTarget position and hint positions are taken from 
    *  TransformStreamHandles as well.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKAnimJob_Rig : IAnimationJob
    {
        [ReadOnly] public TransformStreamHandle c_Tr;
        [ReadOnly] public TransformStreamHandle t_Tr;
        [ReadOnly] public float eps;

        public TransformStreamHandle a_Tr;
        public TransformStreamHandle b_Tr;

        public void ProcessAnimation(AnimationStream stream)
        {
            float3 c = c_Tr.GetPosition(stream);
            float3 t = t_Tr.GetPosition(stream);

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.GetPosition(stream);
            float3 b = b_Tr.GetPosition(stream);
            
            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion a_gr = a_Tr.GetRotation(stream);
            quaternion b_gr = b_Tr.GetRotation(stream);

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.SetLocalRotation(stream, math.mul(a_Tr.GetLocalRotation(stream), math.mul(r0, r2)));
            b_Tr.SetLocalRotation(stream, math.mul(b_Tr.GetLocalRotation(stream), r1));
        }

        public void ProcessRootMotion(AnimationStream stream) { }

    }//End of struct Job: TwoBoneIKAnimJob_Rig

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job, with hint, which performs and applies two bone IK to 
    *  TransformStreamHandles in the animation update. IKTarget position and hint positions are taken
    *  from TransformStreamHandles as well.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKHintAnimJob_Rig : IAnimationJob
    {
        [ReadOnly] public TransformStreamHandle c_Tr;
        [ReadOnly] public TransformStreamHandle t_Tr;
        [ReadOnly] public TransformStreamHandle h_Tr;
        [ReadOnly] public float eps;

        public TransformStreamHandle a_Tr;
        public TransformStreamHandle b_Tr;

        public void ProcessAnimation(AnimationStream stream)
        {
            float3 a = a_Tr.GetPosition(stream);
            float3 b = b_Tr.GetPosition(stream);
            float3 c = c_Tr.GetPosition(stream);
            float3 t = t_Tr.GetPosition(stream);
            float3 h = h_Tr.GetPosition(stream);

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            quaternion a_gr = a_Tr.GetRotation(stream);
            quaternion b_gr = b_Tr.GetRotation(stream);

            //Hint axis
            float3 d = math.mul(b_gr, math.normalize(h - (a + c) / 2f));

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.SetLocalRotation(stream, math.mul(a_Tr.GetLocalRotation(stream), math.mul(r0, r2)));
            b_Tr.SetLocalRotation(stream, math.mul(b_Tr.GetLocalRotation(stream), r1));
        }

        public void ProcessRootMotion(AnimationStream stream) { }

    }//End of struct Job: TwoBoneIKHintAnimJob
}//End of namespace: AnimationUprising.IK
