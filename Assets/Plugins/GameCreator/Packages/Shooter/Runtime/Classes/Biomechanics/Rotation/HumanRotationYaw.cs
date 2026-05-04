using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Serializable]
    public class HumanRotationYaw : THumanRotations
    {
        // EXPOSED METHODS: -----------------------------------------------------------------------
        
        [SerializeField] private PropertyGetDecimal m_Spine = GetDecimalDecimal.Create(0.25f);
        [SerializeField] private PropertyGetDecimal m_LowerChest = GetDecimalDecimal.Create(0.25f);
        [SerializeField] private PropertyGetDecimal m_UpperChest = GetDecimalDecimal.Create(0.25f);
        [SerializeField] private PropertyGetDecimal m_Shoulders = GetDecimalDecimal.Create(0.1f);
        [SerializeField] private PropertyGetDecimal m_Neck = GetDecimalDecimal.Create(0.1f);
        [SerializeField] private PropertyGetDecimal m_Head = GetDecimalDecimal.Create(0.15f);
        
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
            return (float) this.m_Shoulders.Get(args);
        }

        public override float GetRatioNeck(Args args)
        {
            return (float) this.m_Neck.Get(args);
        }

        public override float GetRatioHead(Args args)
        {
            return (float) this.m_Head.Get(args);
        }
    }
}