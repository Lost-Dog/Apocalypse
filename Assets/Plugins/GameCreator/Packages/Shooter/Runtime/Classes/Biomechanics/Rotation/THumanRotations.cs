using System;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Shooter
{
    [Serializable]
    public abstract class THumanRotations
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public abstract float GetRatioSpine(Args args);
        public abstract float GetRatioLowerChest(Args args);
        public abstract float GetRatioUpperChest(Args args);
        public abstract float GetRatioShoulders(Args args);
        public abstract float GetRatioNeck(Args args);
        public abstract float GetRatioHead(Args args);
    }
}