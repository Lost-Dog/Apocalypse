using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public abstract class MotionBase : ScriptableObject
    {
        public const int MOVE_DIRECTION_KEY = 9;
        
        public const int GRAVITY_KEY = 9;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private RunConditionsList m_CanUse = new RunConditionsList();

        [SerializeField] protected Anchor m_Anchor = Anchor.Feet;
        
        [Range(0f, 1f)]
        [SerializeField] private float m_Gravity;
        [SerializeField] private float m_TransitionIn = 0.1f;
        [SerializeField] private float m_TransitionOut = 0.25f;
        
        [SerializeField] private bool m_ApplyMomentum;
        [SerializeField] private float m_MomentumDuration = 0.1f;
        [SerializeField] private float m_MomentumTransition = 0.25f;
        
        [SerializeField] protected RunInstructionsList m_OnStart = new RunInstructionsList();
        [SerializeField] protected RunInstructionsList m_OnFinish = new RunInstructionsList();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public float Gravity => this.m_Gravity;
        public float TransitionIn => this.m_TransitionIn;
        public float TransitionOut => this.m_TransitionOut;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool CanUse(Args args) => this.m_CanUse.Check(args);

        public abstract AnimationClip GetExitAnimation(Vector3 direction, Quaternion rotation);
        
        public Vector3 CharacterPosition(Character character)
        {
            Vector3 offset = Vector3.up * character.Driver.SkinWidth;
            return this.m_Anchor switch
            {
                Anchor.Crown => character.Crown - offset,
                Anchor.Center => character.transform.position - offset,
                Anchor.Feet => character.Feet - offset,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void ApplyMomentum(Character character)
        {
            if (this.m_ApplyMomentum)
            {
                character.Motion.SetMotionTransient(
                    character.transform.forward,
                    character.Driver.WorldMoveDirection.magnitude,
                    this.m_MomentumDuration,
                    this.m_MomentumTransition
                );
            }
        }
    }
}