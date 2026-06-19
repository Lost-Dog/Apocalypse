// ============================================================================================
// File: EIKMethod.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-12-06: Created this file.
// 
//     Contains the EIKMethod enumeration for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief Enumeration for All supported IK methods
    *         
    *********************************************************************************************/
    public enum EIKMethod
    {
        UnityIK,            //Use Unity's internal IK system for IK solvers
        BuiltIn,            //Use the built in strider IK system which runs in late update
        BuiltIn_AnimJobs,   //**Experimental** Use the bulit in strider IK system which runs in the animation update
        AnimationRigging, //Not currently supported
        Custom              //Use your own custom IK system (e.g. FinalIK) to apply the IK targets from stride warping

    }//End of enum: EIKMethod
}//End of namespace: AnimationUprising.Strider
