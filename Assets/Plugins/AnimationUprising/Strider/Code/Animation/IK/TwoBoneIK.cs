// ============================================================================================
// File: TwoBoneIk.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-01-22: Created this file.
// 
//     Contains the StrideWarperJob struct for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Experimental.Animations;
using UnityEngine.Animations;

namespace AnimationUprising.IK
{
    //============================================================================================
    /**
    *  @brief A static class containing a library of TwoBoneIK solver functions. 
    *  
    *  The method used by these solvers carried out in a single iteration. Functions exist for solving
    *  with and without an IK hint. Most functions are burst compilable. These solvers are un-constrained.
    *         
    *********************************************************************************************/
    public static class TwoBoneIK
    {
        //For all functions in this static class libary, use the key below to decern variable names
        // 'a' - Base Joint
        // 'b' - Mid Joint
        // 'c' - End Joint
        // 't' - IK Target
        // 'h' - IK Hint Direction
        // 'd' - IK Hint Axis
        // '_gr' - Global Rotation
        // '_gl' - Local Rotation
        // 'l_' - Bone length
        // 'eps' - epsilon / tolerance / error threshold
        // 'Tr' - Transform

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes POD and writes to referenced quaternion local
        *  rotations. This solver does not use an IK hint.
        *  
        *  @param [float3] a - base joint global position
        *  @param [float3] b - mid joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [quaternion] a_gr - base joint global rotation
        *  @param [quaternion] b_gr - mid joint globalrotation
        *  @param [ref quaternion] a_lr - reference to base joint local rotation
        *  @param [ref quaternion] b_lr - reference to mid joint local rotation
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(float3 a, float3 b, float3 c, float3 t,
            quaternion a_gr, quaternion b_gr,
            ref quaternion a_lr, ref quaternion b_lr, float eps = 0.01f)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

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

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes POD and writes to referenced quaternion local
        *  rotations. This solver takes an IK hint direction.
        *  
        *  @param [float3] a - base joint global position
        *  @param [float3] b - mid joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - hint direction
        *  @param [quaternion] a_gr - base joint global rotation
        *  @param [quaternion] b_gr - mid joint globalrotation
        *  @param [ref quaternion] a_lr - reference to base joint local rotation
        *  @param [ref quaternion] b_lr - reference to mid joint local rotation
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(float3 a, float3 b, float3 c, float3 t, float3 h,
            quaternion a_gr, quaternion b_gr, ref quaternion a_lr, ref quaternion b_lr, float eps = 0.01f)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            //Hint axis
            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes POD and writes to referenced quaternion local
        *  rotations. 
        *  
        *  This solver takes pre-calculated limb lengths to avoid additional magnitude calls. It does 
        *  not use an IK hint.
        *  
        *  @param [float3] a - base joint global position
        *  @param [float3] b - mid joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] l_ab - length from the base to mid joint
        *  @param [float] l_cb - length from the tip to mid joint
        *  @param [quaternion] a_gr - base joint global rotation
        *  @param [quaternion] b_gr - mid joint globalrotation
        *  @param [ref quaternion] a_lr - reference to base joint local rotation
        *  @param [ref quaternion] b_lr - reference to mid joint local rotation
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(float3 a, float3 b, float3 c, float3 t,
            float l_ab, float l_cb, quaternion a_gr, quaternion b_gr,
            ref quaternion a_lr, ref quaternion b_lr, float eps = 0.01f)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes POD and writes to referenced quaternion local
        *  rotations. 
        *  
        *  This solver takes pre-calculated limb lengths to avoid additional magnitude calls. It also uses
        *  a hint direction to stabalise solving.
        *  
        *  @param [float3] a - base joint global position
        *  @param [float3] b - mid joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - hint direction
        *  @param [float] l_ab - length from the base to mid joint
        *  @param [float] l_cb - length from the tip to mid joint
        *  @param [quaternion] a_gr - base joint global rotation
        *  @param [quaternion] b_gr - mid joint globalrotation
        *  @param [ref quaternion] a_lr - reference to base joint local rotation
        *  @param [ref quaternion] b_lr - reference to mid joint local rotation
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(float3 a, float3 b, float3 c, float3 t, float3 h,
            float l_ab, float l_cb, quaternion a_gr, quaternion b_gr,
            ref quaternion a_lr, ref quaternion b_lr, float eps = 0.01f)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            //Hint axis
            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes Unity Transforms for reading and writing limb 
        *  data. It does not use an IK hint
        *  
        *  This function is not burst compilable due to the Transforms
        *  
        *  @param [Transform] a_Tr - base joint transform
        *  @param [Transform] b_Tr - mid joint transform
        *  @param [Transform] c_Tr - tip joint transform
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(Transform a_Tr, Transform b_Tr, Transform c_Tr, float3 t,
            float eps = 0.01f)
        { 
            float3 c = c_Tr.position;

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.position;
            float3 b = b_Tr.position;

            quaternion a_gr = a_Tr.rotation;
            quaternion b_gr = b_Tr.rotation;
            quaternion a_lr = a_Tr.localRotation;
            quaternion b_lr = b_Tr.localRotation;

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

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.localRotation = math.mul(a_lr, math.mul(r0, r2));
            b_Tr.localRotation = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes Unity Transforms for reading and writing limb 
        *  data. It uses an IK hint for stability.
        *  
        *  This function is not burst compilable due to the Transforms
        *  
        *  @param [Transform] a_Tr - base joint transform
        *  @param [Transform] b_Tr - mid joint transform
        *  @param [Transform] c_Tr - tip joint transform
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - the hint direction
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(Transform a_Tr, Transform b_Tr, Transform c_Tr,
            ref float3 t, float3 h, float eps = 0.01f)
        { 
            float3 c = c_Tr.position;

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.position;
            float3 b = b_Tr.position;

            quaternion a_gr = a_Tr.rotation;
            quaternion b_gr = b_Tr.rotation;
            quaternion a_lr = a_Tr.localRotation;
            quaternion b_lr = b_Tr.localRotation;

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            //Hint axis
            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.localRotation = math.mul(a_lr, math.mul(r0, r2));
            b_Tr.localRotation = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes Unity Transforms for reading and writing limb 
        *  data. 
        *  
        *  It does not use an IK hint but takes in pre-computed limb lengths. This function is not burst 
        *  compilable due to the Transforms
        *  
        *  @param [Transform] a_Tr - base joint transform
        *  @param [Transform] b_Tr - mid joint transform
        *  @param [Transform] c_Tr - tip joint transform
        *  @param [float] l_ab - upper limb length
        *  @param [float] l_cb - lower limb length
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(Transform a_Tr, Transform b_Tr, Transform c_Tr, float3 t,
            float l_ab, float l_cb, float eps = 0.01f)
        {
            float3 c = c_Tr.position;

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.position;
            float3 b = b_Tr.position;

            quaternion a_gr = a_Tr.rotation;
            quaternion b_gr = b_Tr.rotation;
            quaternion a_lr = a_Tr.localRotation;
            quaternion b_lr = b_Tr.localRotation;

            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.localRotation = math.mul(a_lr, math.mul(r0, r2));
            b_Tr.localRotation = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which takes Unity Transforms for reading and writing limb 
        *  data. 
        *  
        *  This solver uses an IK hint and also takes in pre-computed limb lengths. This function is 
        *  not burst compilable due to the Transforms
        *  
        *  @param [Transform] a_Tr - base joint transform
        *  @param [Transform] b_Tr - mid joint transform
        *  @param [Transform] c_Tr - tip joint transform
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - the hint direction
        *  @param [float] l_ab - upper limb length
        *  @param [float] l_cb - lower limb length
        
        *  @param [float] eps - error threshold to improve stabliity
        *         
        *********************************************************************************************/
        public static void Solve(Transform a_Tr, Transform b_Tr, Transform c_Tr, float3 h,
            float3 t, float l_ab, float l_cb, float eps = 0.01f)
        {
            float3 c = c_Tr.position;
            
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.position;
            float3 b = b_Tr.position;

            quaternion a_gr = a_Tr.rotation;
            quaternion b_gr = b_Tr.rotation;
            quaternion a_lr = a_Tr.localRotation;
            quaternion b_lr = b_Tr.localRotation;

            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            //Hint axis
            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            a_Tr.localRotation = math.mul(a_lr, math.mul(r0, r2));
            b_Tr.localRotation = math.mul(b_lr, r1);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. Limb joint rotations can be
        *  read from transform stream handles.
        *  
        *  This solver does not use an IK hint. 
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [float3] a - base joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] eps - error threshold to improve stabliity
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, float3 a, float3 c, float3 t, float eps,
            TransformStreamHandle a_Tr, TransformStreamHandle b_Tr)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 b = b_Tr.GetPosition(a_stream);

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. Limb joint rotations can be
        *  read from transform stream handles.
        *  
        *  This solver uses an IK hint. 
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [float3] a - base joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - ik hint direction
        *  @param [float] eps - error threshold to improve stabliity
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, float3 a, float3 c, float3 t, float3 h, float eps,
            TransformStreamHandle a_Tr, TransformStreamHandle b_Tr)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 b = b_Tr.GetPosition(a_stream);

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            //Hint axis
            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. Limb joint rotations can be
        *  read from transform stream handles.
        *  
        *  This solver doesn not use an IK hint but does takes pre-computed limb lengths
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [float3] a - base joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] eps - error threshold to improve stabliity
        *  @param [float] l_ab - upper limb length
        *  @param [float] l_cb - lower limb length
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, float3 a, float3 c, float3 t, float eps,
            float l_ab, float l_cb, TransformStreamHandle a_Tr, TransformStreamHandle b_Tr)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 b = b_Tr.GetPosition(a_stream);
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. Limb joint rotations can be
        *  read from transform stream handles.
        *  
        *  This solver uses an IK hint and also takes pre-computed limb lengths
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [float3] a - base joint global position
        *  @param [float3] c - tip joint global position
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - ik hint direction
        *  @param [float] eps - error threshold to improve stabliity
        *  @param [float] l_ab - upper limb length
        *  @param [float] l_cb - lower limb length
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, float3 a, float3 c, float3 t, float3 h,
            float eps, float l_ab, float l_cb, TransformStreamHandle a_Tr, TransformStreamHandle b_Tr)
        {
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 b = b_Tr.GetPosition(a_stream);
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));
 
            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. All limb joint data can be read
        *  from transform stream handles
        *  
        *  This solver doesn't use an IK hint.
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *  @param [TransformSreamHandle] c_Tr - tip joint transform handle
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] eps - error threshold to improve stability
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, TransformStreamHandle a_Tr,
             TransformStreamHandle b_Tr, TransformStreamHandle c_Tr, float3 t, float eps)
        {
            float3 c = c_Tr.GetPosition(a_stream);

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.GetPosition(a_stream);
            float3 b = b_Tr.GetPosition(a_stream);

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. All limb joint data can be read
        *  from transform stream handles
        *  
        *  This solver uses an IK hint direction.
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *  @param [TransformSreamHandle] c_Tr - tip joint transform handle
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - ik hint direction
        *  @param [float] eps - error threshold to improve stability
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, TransformStreamHandle a_Tr, TransformStreamHandle b_Tr,
              TransformStreamHandle c_Tr, float3 t, float3 h, float eps)
        {
            float3 c = c_Tr.GetPosition(a_stream);

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.GetPosition(a_stream);
            float3 b = b_Tr.GetPosition(a_stream);
           
            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. All limb joint data can be read
        *  from transform stream handles
        *  
        *  This solver doesn't us an IK hint direction but it does take pre-computed limb lengths.
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *  @param [TransformSreamHandle] c_Tr - tip joint transform handle
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float] l_ab - upper limb length
        *  @param [float] l_cb - lower limb length
        *  @param [float] eps - error threshold to improve stability
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, TransformStreamHandle a_Tr,
             TransformStreamHandle b_Tr, TransformStreamHandle c_Tr, float3 t, float l_ab, float l_cb, float eps)
        {
            float3 c = c_Tr.GetPosition(a_stream);

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.GetPosition(a_stream);
            float3 b = b_Tr.GetPosition(a_stream);
            
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            float3 axis0 = math.normalize(math.cross(c - a, b - a));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Two bone IK solver function which is intended to be used by Animation jobs. It passes
        *  in an animation stream which can be read from and written to. All limb joint data can be read
        *  from transform stream handles
        *  
        *  This solver uses an IK hint direction and also pre-computed limb lengths.
        *  
        *  This function is burst compilable.
        *  
        *  @param [AnimationStream] a_stream - the current animation stream
        *  @param [TransformStreamHandle] a_Tr - base joint transform handle
        *  @param [TransformSreamHandle] b_Tr - mid joint transform handle
        *  @param [TransformSreamHandle] c_Tr - tip joint transform handle
        *  @param [float3] t - the ik target that the tip attempts to reach
        *  @param [float3] h - ik hint direction
        *  @param [float] l_ab - upper limb length
        *  @param [float] l_cb - lower limb length
        *  @param [float] eps - error threshold to improve stability
        *         
        *********************************************************************************************/
        public static void Solve(AnimationStream a_stream, float l_ab, float l_cb, TransformStreamHandle a_Tr,
             TransformStreamHandle b_Tr, TransformStreamHandle c_Tr, float3 h, float3 t, float eps)
        {
            float3 c = c_Tr.GetPosition(a_stream);

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = a_Tr.GetPosition(a_stream);
            float3 b = b_Tr.GetPosition(a_stream);
            
            float lat = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - lat * lat) / (-2f * l_ab * lat), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((lat * lat - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            quaternion a_gr = a_Tr.GetRotation(a_stream);
            quaternion b_gr = b_Tr.GetRotation(a_stream);

            float3 d = math.mul(b_gr, h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_gr), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_gr), axis1), ac_at_0);

            quaternion a_lr = a_Tr.GetLocalRotation(a_stream);
            quaternion b_lr = b_Tr.GetLocalRotation(a_stream);

            a_lr = math.mul(a_lr, math.mul(r0, r2));
            b_lr = math.mul(b_lr, r1);

            a_Tr.SetLocalRotation(a_stream, a_lr);
            b_Tr.SetLocalRotation(a_stream, b_lr);
        }

        //============================================================================================
        /**
        *  @brief Helper function for calculating the hint direction from it's position and the positions
        *  of the base and the tip
        *  
        *  @param [float3] a_basePos - global position of the base joint
        *  @param [float3] a_tipPos - global position of the tip pos
        *  @param [float3] a_hintPos - global position of the ik hint
        *  
        *  return float3 - the hint direction
        *         
        *********************************************************************************************/
        public static float3 HintPositionToDirection(float3 a_basePos, float3 a_tipPos, float3 a_hintPos)
        {
            return math.normalize(a_hintPos - (a_basePos + a_tipPos) / 2f);
        }

    }//End of static class: TwoBoneIK
}//End of namespace: AnimationUprising.IK
