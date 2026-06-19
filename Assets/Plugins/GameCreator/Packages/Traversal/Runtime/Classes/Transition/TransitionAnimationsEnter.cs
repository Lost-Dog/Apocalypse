using System;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class TransitionAnimationsEnter : TransitionAnimationsBase
    {
        public AnimationClip Get(Vector3 moveDirection, Quaternion rotation)
        {
            Vector3 direction = this.WorldToLocalDirection(moveDirection, rotation).normalized;
            return direction.y switch
            {
                > +0.5f => this.m_Downward,
                < -0.5f => this.m_Upward,
                _ => direction.x switch
                {
                    > +0.5f => this.m_Left,
                    < -0.5f => this.m_Right,
                    _ => direction.z switch
                    {
                        > +0.5f => this.m_Backward,
                        < -0.5f => this.m_Forward,
                        _ => this.m_Forward
                    }
                }
            };
        }
    }
}