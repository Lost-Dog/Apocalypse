// ============================================================================================
// File: TwoBoneIKJobs.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-01-22: Created this file.
// 
//     Contains the multithreaded Jobs portion of the 'AnimationUprising' TwoBoneIK library
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace AnimationUprising.IK
{
    //For all following jobs, use the key below to decern variable names
    // 'a' - Base Joint
    // 'b' - Mid Joint
    // 'c' - End Joint
    // 't' - IK Target
    // 'h' - IK Hint direction
    // 'd' - IK Hint Axis
    // '_gr' - Global Rotation
    // '_gl' - Local Rotation
    // 'l_' - Bone length
    // 'eps' - epsilon / tolerance / error threshold
    // 'Tr' - Transform

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job. The calculated local rotation of the base and mid limbs is
    *  stored in a native array ab_lr which can be read from once the job is complete.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKJob : IJob
    {
        [ReadOnly] public float3 a;
        [ReadOnly] public float3 b;
        [ReadOnly] public float3 c;
        [ReadOnly] public float3 t;
        [ReadOnly] public quaternion a_gr;
        [ReadOnly] public quaternion b_gr;
        [ReadOnly] public quaternion a_lr;
        [ReadOnly] public quaternion b_lr;
        [ReadOnly] public float eps;

        //Thigh (0) and lower leg (1) joints will have their final local rotation written into this array
        [WriteOnly] public NativeArray<quaternion> ab_lr; 

        void IJob.Execute()
        {
            if(math.distancesq(t, c) < 0.00001f)
            {
                ab_lr[0] = a_lr;
                ab_lr[1] = b_lr;

                return;
            }

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

            ab_lr[0] = math.mul(a_lr, math.mul(r0, r2));
            ab_lr[1] = math.mul(b_lr, r1);
        }
    }//End of struct job: TwoBoneIKJob

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job. The calculated local rotation of the base and mid limbs is
    *  stored in a native array ab_lr which can be read from once the job is complete.
    *  
    *  This job also takes a hint direction for IK stability.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKHintJob : IJob
    {
        [ReadOnly] public float3 a;
        [ReadOnly] public float3 b;
        [ReadOnly] public float3 c;
        [ReadOnly] public float3 t;
        [ReadOnly] public float3 h;
        [ReadOnly] public quaternion a_gr;
        [ReadOnly] public quaternion b_gr;
        [ReadOnly] public quaternion a_lr;
        [ReadOnly] public quaternion b_lr;
        [ReadOnly] public float eps;

        //Thigh (0) and lower leg (1) joints will have their final local rotation written into this array
        [WriteOnly] NativeArray<quaternion> ab_lr;

        void IJob.Execute()
        {
            if (math.distancesq(t, c) < 0.00001f)
            {
                ab_lr[0] = a_lr;
                ab_lr[1] = b_lr;

                return;
            }

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

            ab_lr[0] = math.mul(a_lr, math.mul(r0, r2));
            ab_lr[1] = math.mul(b_lr, r1);
        }
    }//End of struct job: TwoBoneIKHintJob

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job which calculates two bone IK chains in bulk. 
    *  The calculated local rotation of the base and mid limbs for each chain is stored in NativeArrays
    *  a_lrA and b_lrA which can be ready from once the job is complete.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKBulkJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> aA;
        [ReadOnly] public NativeArray<float3> bA;
        [ReadOnly] public NativeArray<float3> cA;
        [ReadOnly] public NativeArray<float3> tA;
        [ReadOnly] public NativeArray<quaternion> a_grA;
        [ReadOnly] public NativeArray<quaternion> b_grA;
        [ReadOnly] public float eps;

        public NativeArray<quaternion> a_lrA;
        public NativeArray<quaternion> b_lrA;

        public void Execute(int index)
        {
            float3 c = cA[index];
            float3 t = tA[index];

            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = aA[index];
            float3 b = bA[index];

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

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[index]), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_grA[index]), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[index]), axis1), ac_at_0);

            a_lrA[index] = math.mul(a_lrA[index], math.mul(r0, r2));
            b_lrA[index] = math.mul(b_lrA[index], r1);
        }
    }//End of struct job: TwoBoneIKBulkJob

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job which calculates two bone IK chains in bulk. 
    *  The calculated local rotation of the base and mid limbs for each chain is stored in NativeArrays
    *  a_lrA and b_lrA which can be ready from once the job is complete.
    *  
    *  This job also takes a hint direction for each IK chain for Ik stability.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKBulkHintJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> aA;
        [ReadOnly] public NativeArray<float3> bA;
        [ReadOnly] public NativeArray<float3> cA;
        [ReadOnly] public NativeArray<float3> tA;
        [ReadOnly] public NativeArray<float3> hA;
        [ReadOnly] public NativeArray<quaternion> a_grA;
        [ReadOnly] public NativeArray<quaternion> b_grA;
        [ReadOnly] public float eps;

        public NativeArray<quaternion> a_lrA;
        public NativeArray<quaternion> b_lrA;

        public void Execute(int index)
        {
            float3 c = cA[index];
            float3 t = tA[index];
            
            if (math.distancesq(t, c) < 0.00001f)
                return;

            float3 a = aA[index];
            float3 b = bA[index];
            float3 h = hA[index];

            float l_ab = math.length(b - a);
            float l_cb = math.length(b - c);
            float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

            float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
            float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
            float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

            float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
            float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

            //Hint axis
            float3 d = math.mul(b_grA[index], h);

            float3 axis0 = math.normalize(math.cross(c - a, d));
            float3 axis1 = math.normalize(math.cross(c - a, t - a));

            quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[index]), axis0), ac_ab_1 - ac_ab_0);
            quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_grA[index]), axis0), ba_bc_1 - ba_bc_0);
            quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[index]), axis1), ac_at_0);

            a_lrA[index] = math.mul(a_lrA[index], math.mul(r0, r2));
            b_lrA[index] = math.mul(b_lrA[index], r1);
        }
    }//End of struct job: TwoBoneIKBulkHintJob

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job which calculates two bone IK chains in bulk and in batches to 
    *  improve performance. The calculated local rotation of the base and mid limbs for each chain is
    *  stored in NativeArrays, a_lrA and b_lrA which can be ready from once the job is complete.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKBulkJobBatch : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float3> aA;
        [ReadOnly] public NativeArray<float3> bA;
        [ReadOnly] public NativeArray<float3> cA;
        [ReadOnly] public NativeArray<float3> tA;
        [ReadOnly] public NativeArray<quaternion> a_grA;
        [ReadOnly] public NativeArray<quaternion> b_grA;
        [ReadOnly] public float eps;

        public NativeArray<quaternion> a_lrA;
        public NativeArray<quaternion> b_lrA;

        public void Execute(int startIndex, int count)
        {
            for(int i = startIndex; i < count; ++i)
            {
                float3 c = cA[i];
                float3 t = tA[i];

                if (math.distancesq(t, c) < 0.00001f)
                    return;

                float3 a = aA[i];
                float3 b = bA[i];

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

                quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[i]), axis0), ac_ab_1 - ac_ab_0);
                quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_grA[i]), axis0), ba_bc_1 - ba_bc_0);
                quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[i]), axis1), ac_at_0);

                a_lrA[i] = math.mul(a_lrA[i], math.mul(r0, r2));
                b_lrA[i] = math.mul(b_lrA[i], r1);
            }
        }
    }//End of struct job: TwoBoneIKBulkJobBatch

    //============================================================================================
    /**
    *  @brief TwoBoneIK animation job which calculates two bone IK chains in bulk and in batch.
    *  The calculated local rotation of the base and mid limbs for each chain is stored in NativeArrays
    *  a_lrA and b_lrA which can be ready from once the job is complete.
    *  
    *  This job also takes a hint direction for each IK chain for Ik stability.
    *         
    *********************************************************************************************/
    [BurstCompile]
    public struct TwoBoneIKBulkHintJobBatch : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float3> aA;
        [ReadOnly] public NativeArray<float3> bA;
        [ReadOnly] public NativeArray<float3> cA;
        [ReadOnly] public NativeArray<float3> tA;
        [ReadOnly] public NativeArray<float3> hA;
        [ReadOnly] public NativeArray<quaternion> a_grA;
        [ReadOnly] public NativeArray<quaternion> b_grA;
        [ReadOnly] public float eps;

        public NativeArray<quaternion> a_lrA;
        public NativeArray<quaternion> b_lrA;

        public void Execute(int startIndex, int count)
        {
            for(int i = startIndex; i < count; ++i)
            {
                float3 c = cA[i];
                float3 t = tA[i];
               
                if (math.distancesq(t, c) < 0.00001f)
                    return;

                float3 a = aA[i];
                float3 b = bA[i];
                float3 h = hA[i];

                float l_ab = math.length(b - a);
                float l_cb = math.length(b - c);
                float l_at = math.clamp(math.length(t - a), eps, l_ab + l_cb - eps);

                float ac_ab_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(b - a)), -1f, 1f));
                float ba_bc_0 = math.acos(math.clamp(math.dot(math.normalize(a - b), math.normalize(c - b)), -1f, 1f));
                float ac_at_0 = math.acos(math.clamp(math.dot(math.normalize(c - a), math.normalize(t - a)), -1f, 1f));

                float ac_ab_1 = math.acos(math.clamp((l_cb * l_cb - l_ab * l_ab - l_at * l_at) / (-2f * l_ab * l_at), -1f, 1f));
                float ba_bc_1 = math.acos(math.clamp((l_at * l_at - l_ab * l_ab - l_cb * l_cb) / (-2f * l_ab * l_cb), -1f, 1f));

                //Hint axis
                float3 d = math.mul(b_grA[i], h);

                float3 axis0 = math.normalize(math.cross(c - a, d));
                float3 axis1 = math.normalize(math.cross(c - a, t - a));

                quaternion r0 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[i]), axis0), ac_ab_1 - ac_ab_0);
                quaternion r1 = quaternion.AxisAngle(math.mul(math.inverse(b_grA[i]), axis0), ba_bc_1 - ba_bc_0);
                quaternion r2 = quaternion.AxisAngle(math.mul(math.inverse(a_grA[i]), axis1), ac_at_0);

                a_lrA[i] = math.mul(a_lrA[i], math.mul(r0, r2));
                b_lrA[i] = math.mul(b_lrA[i], r1);
            }
        }
    }//End of struct job: TwoBoneIKBulkHintJobBatch
}//End of namespace: AnimationUprising.IK
