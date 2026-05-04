using System;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    public readonly struct InteractiveTransitionData
    {
        public static InteractiveTransitionData None => default;
        
        [field: NonSerialized] public AnimationClip ExitAnimation { get; }

        public float ExitAnimationLength => this.ExitAnimation != null
            ? this.ExitAnimation.length
            : 0f;

        public InteractiveTransitionData(AnimationClip exitAnimation)
        {
            this.ExitAnimation = exitAnimation;
        }
    }
}