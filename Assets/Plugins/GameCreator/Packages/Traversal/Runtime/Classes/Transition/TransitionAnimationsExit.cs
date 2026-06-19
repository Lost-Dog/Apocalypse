using System;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class TransitionAnimationsExit : TransitionAnimationsBase
    {
        public AnimationClip Get(Vector3 moveDirection, Quaternion rotation)
        {
            Vector3 direction = this.WorldToLocalDirection(moveDirection, rotation).normalized;
            return direction.y switch
            {
                > +0.5f => this.m_Upward,
                < -0.5f => this.m_Downward,
                _ => direction.x switch
                {
                    > +0.5f => this.m_Right,
                    < -0.5f => this.m_Left,
                    _ => direction.z switch
                    {
                        > +0.5f => this.m_Forward,
                        < -0.5f => this.m_Backward,
                        _ => this.m_Forward
                    }
                }
            };
        }
    }
}