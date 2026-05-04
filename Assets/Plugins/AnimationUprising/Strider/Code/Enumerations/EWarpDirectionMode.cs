// ============================================================================================
// File: EWarpDirectionMode.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-12-06: Created this file.
// 
//     Contains the EWarpDirectionMode enumeration for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
namespace AnimationUprising.Strider
{ 
    //============================================================================================
    /**
    *  @brief Enumeration for all warp direction modes in StriderPro
    *         
    *********************************************************************************************/
    public enum EWarpDirectionMode
    {
        RootVelocity,       //Calculate warp direction from animator root velocity
        ActualVelocity,     //Calculate warp direction from actual transform velocity
        CharacterFacing,    //Warp direction is taken as the character facing direction
        Manual              //Allows the user to manually set warp direction.

    }//End of enum: EWarpDirectionMode
}//End of namespace: AnimationUprising.Strider
