using GameCreator.Runtime.Characters;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    public abstract class StateTraverseBase : StateOverrideAnimator
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private AnimationClip m_Idle;
        
        [SerializeField] private AnimationClip m_MoveForward;
        [SerializeField] private AnimationClip m_MoveBackward;
        [SerializeField] private AnimationClip m_MoveRight;
        [SerializeField] private AnimationClip m_MoveLeft;
        [SerializeField] private AnimationClip m_MoveForwardRight;
        [SerializeField] private AnimationClip m_MoveForwardLeft;
        [SerializeField] private AnimationClip m_MoveBackwardRight;
        [SerializeField] private AnimationClip m_MoveBackwardLeft;
        
        [SerializeField] private AnimationClip m_EdgeForward;
        [SerializeField] private AnimationClip m_EdgeBackward;
        [SerializeField] private AnimationClip m_EdgeRight;
        [SerializeField] private AnimationClip m_EdgeLeft;

        // SERIALIZATION CALLBACKS: ---------------------------------------------------------------
        
        private const string IDLE    = "Traverse@Idle";
        private const string MOVE_F  = "Traverse@Move_F";
        private const string MOVE_B  = "Traverse@Move_B";
        private const string MOVE_R  = "Traverse@Move_R";
        private const string MOVE_L  = "Traverse@Move_L";
        private const string MOVE_FR  = "Traverse@Move_FR";
        private const string MOVE_FL  = "Traverse@Move_FL";
        private const string MOVE_BR  = "Traverse@Move_BR";
        private const string MOVE_BL  = "Traverse@Move_BL";
        private const string INTENT_F  = "Traverse@Intent_F";
        private const string INTENT_B  = "Traverse@Intent_B";
        private const string INTENT_R  = "Traverse@Intent_R";
        private const string INTENT_L  = "Traverse@Intent_L";

        protected sealed override void BeforeSerialize()
        {
            if (this.m_Controller == null) return;
            
            this.m_Controller[IDLE] = this.m_Idle;
            this.m_Controller[MOVE_F] = this.m_MoveForward;
            this.m_Controller[MOVE_B] = this.m_MoveBackward;
            this.m_Controller[MOVE_R] = this.m_MoveRight;
            this.m_Controller[MOVE_L] = this.m_MoveLeft;
            this.m_Controller[MOVE_FR] = this.m_MoveForwardRight;
            this.m_Controller[MOVE_FL] = this.m_MoveForwardLeft;
            this.m_Controller[MOVE_BR] = this.m_MoveBackwardRight;
            this.m_Controller[MOVE_BL] = this.m_MoveBackwardLeft;
            this.m_Controller[INTENT_F] = this.m_EdgeForward;
            this.m_Controller[INTENT_B] = this.m_EdgeBackward;
            this.m_Controller[INTENT_R] = this.m_EdgeRight;
            this.m_Controller[INTENT_L] = this.m_EdgeLeft;
        }

        protected sealed override void AfterSerialize()
        { }
    }
}