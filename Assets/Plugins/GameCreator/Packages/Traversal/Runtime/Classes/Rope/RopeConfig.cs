using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public struct RopeConfig
    {
        public static readonly RopeConfig Default = new RopeConfig(
            1f,
            Easing.Type.QuadIn,
            1f,
            0.0f,
            2f,
            5f,
            1f
        );
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] public float throwHeight;
        [SerializeField] public Easing.Type throwHeightEasing;
        
        [SerializeField] public float looseTensionFactor;
        [SerializeField] public float tightTensionFactor;
        
        [SerializeField] public float reelChaosX;
        [SerializeField] public float reelChaosY;
        [SerializeField] public float reelChaosMagnitude;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public RopeConfig(
            float throwHeight,
            Easing.Type throwHeightEasing,
            float looseTensionFactor,
            float tightTensionFactor,
            float reelChaosX,
            float reelChaosY,
            float reelChaosMagnitude)
        {
            this.looseTensionFactor = looseTensionFactor;
            this.tightTensionFactor = tightTensionFactor;
            this.throwHeightEasing = throwHeightEasing;
            this.throwHeight = throwHeight;
            this.reelChaosX = reelChaosX;
            this.reelChaosY = reelChaosY;
            this.reelChaosMagnitude = reelChaosMagnitude;
        }
    }
}