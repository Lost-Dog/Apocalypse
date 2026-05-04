using System;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Characters
{
    [Title("Footstep Detector")]
    [Image(typeof(IconFootprint), ColorTheme.Type.TextLight)]
    
    [Serializable]
    public abstract class FootstepDetectorBase : TPolymorphicItem<FootstepDetectorBase>
    {
        // METHODS: -------------------------------------------------------------------------------

        public abstract void OnEnable(Character character);
        public abstract void OnDisable(Character character);
        public abstract void OnUpdate(Character character);
        public abstract void OnGizmos(Character character);
    }
}