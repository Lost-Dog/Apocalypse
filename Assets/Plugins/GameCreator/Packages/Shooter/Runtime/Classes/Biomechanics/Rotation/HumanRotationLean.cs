using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Serializable]
    public class HumanRotationLean : THumanRotations
    {
        [SerializeField] private PropertyGetDecimal m_Spine = new PropertyGetDecimal(20f);
        [SerializeField] private PropertyGetDecimal m_LowerChest = new PropertyGetDecimal(20f);
        [SerializeField] private PropertyGetDecimal m_UpperChest = new PropertyGetDecimal(0f);
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override float GetRatioSpine(Args args)
        {
            return (float) this.m_Spine.Get(args);
        }

        public override float GetRatioLowerChest(Args args)
        {
            return (float) this.m_LowerChest.Get(args);
        }

        public override float GetRatioUpperChest(Args args)
        {
            return (float) this.m_UpperChest.Get(args);
        }

        public override float GetRatioShoulders(Args args)
        {
            return 0f;
        }

        public override float GetRatioNeck(Args args)
        {
            return 0f;
        }

        public override float GetRatioHead(Args args)
        {
            return 0f;
        }
    }
}