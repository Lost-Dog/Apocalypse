using System;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public abstract class TransitionAnimationsBase
    {
        [SerializeField] protected AnimationClip m_Forward;
        [SerializeField] protected AnimationClip m_Backward;
        [SerializeField] protected AnimationClip m_Left;
        [SerializeField] protected AnimationClip m_Right;
        [SerializeField] protected AnimationClip m_Upward;
        [SerializeField] protected AnimationClip m_Downward;
        
        protected Vector3 WorldToLocalDirection(Vector3 worldDirection, Quaternion rotation)
        {
            return Quaternion.Inverse(rotation) * worldDirection;
        }
    }
}