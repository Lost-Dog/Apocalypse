// ============================================================================================
// File: StrideWarpPlayables.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-01-24: Created this file.
// 
//     Contains all stride warp animation payables for StriderPro in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine.Playables;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief Custom playable used to access the animation update just before the animation jobs
    *  are run. This is specifically used when strider is using BuiltIn_AnimJobs IK method.
    *         
    *********************************************************************************************/
    public class StrideWarpPlayable_AnimJobs : PlayableBehaviour
    {
        public StriderBiped StrideWarper { private get; set; } //A reference to the associated stride warper

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (StrideWarper.enabled)
                StrideWarper.UpdateBuiltIn_AnimJobs();
        }

    }//End of class: StrideWarpPlayable_AnimJobs

    //============================================================================================
    /**
    *  @brief Custom playable used to access the animation update just before the animation jobs
    *  are run. This is specifically used for the UnityRigging Animation Jobs IK method
    *         
    *********************************************************************************************/
    public class StrideWarpPlayable_UnityRigAnimJobs : PlayableBehaviour
    {
        public StriderBiped StrideWarper { private get; set; } //A reference to the associated stride warper

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (StrideWarper.enabled)
                StrideWarper.UpdateUnityRigging_AnimJobs();
        }

    }//End of class: StrideWarpPlayable_AnimJobs

    //============================================================================================
    /**
    *  @brief Custom playable used to access the animation update just before the animation jobs
    *  are run. This is specifically used to scale the root motion output from the animator.
    *         
    *********************************************************************************************/
    public class StrideWarpPlayable_RootMotion : PlayableBehaviour
    {
        public StriderBiped StrideWarper { private get; set; }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (StrideWarper.enabled)
                StrideWarper.UpdateRootMotionJob();
        }
    }



}//End of namespace: AnimationToolbox.Strider