// ============================================================================================
// File: ICustomStrideWarpApplicator.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-01-07: Created this file.
// 
//     Contains the ICustomStrideWarpApplicator interface for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using Unity.Mathematics;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief Interface for custom IK setups for use with the animation stride warper
    *  
    *  Note: Custom IK setup means any IK setup that isn't MecanimIK or Unity Animation Rigging.
    *  FinalIK has an integration downloadable from the asset discord channel.
    *         
    *********************************************************************************************/
    public interface ICustomStrideWarpApplicator
    {
        void StrideWarp(float a_hipAdjustY, float3 a_leftFootPos, float3 a_rightFootPos);

    }//End of interface: ICustomStrideWarpApplicator
}//End of namespace: AnimationUprising.Strider
