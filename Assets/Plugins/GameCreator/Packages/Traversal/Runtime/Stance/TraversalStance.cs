using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    public class TraversalStance : TStance
    {
        public static readonly int ID = "Traversal".GetHashCode();
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private TraversalToken m_CurrentToken;
        [NonSerialized] private IdString m_CurrentStateId;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Id => ID;
        
        [field: NonSerialized] public override Character Character { get; set; }
        [field: NonSerialized] public Traverse Traverse { get; private set; }
        
        [field: NonSerialized] internal Transform Parent { get; set; }
        [field: NonSerialized] internal Vector3 RelativePosition { get; set; }
        
        [field: NonSerialized] internal bool InInteractiveTransition { get; set; }
        [field: NonSerialized] internal bool AllowMovement { get; set; }
        
        // EVENTS: --------------------------------------------------------------------------------

        public event Action EventMotionEnter;
        public event Action EventMotionExit;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TraversalStance()
        { }

        // STANCE METHODS: ------------------------------------------------------------------------
        
        public override void OnEnable(Character character)
        {
            if (this.IsEnabled) return;
            base.OnEnable(character);
            
            this.Character = character;
        }

        public override void OnUpdate()
        { }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void TryCancel(Args args)
        {
            if (this.Traverse == null) return;
            if (this.m_CurrentToken == null) return;
            
            if (this.Traverse.CanCancel(args))
            {
                this.ForceCancel();
            }
        }

        public bool ForceCancel()
        {
            if (this.Traverse == null) return false;
            if (this.m_CurrentToken == null) return false;
            if (this.m_CurrentToken.IsCancelled) return false;
            
            float transitionOut = this.Traverse.Motion != null
                ? this.Traverse.Motion.TransitionOut
                : 0f;
            
            this.m_CurrentToken.IsCancelled = true;
            this.Character.Gestures.Stop(0f, transitionOut);
            return true;
        }

        public void TryJump()
        {
            if (this.Traverse == null) return;
            if (this.m_CurrentToken == null) return;
            if (this.InInteractiveTransition) return;
            
            this.Traverse.AttemptJump(this.Character);
        }
        
        public void TryAction(IdString actionId)
        {
            if (this.Traverse == null) return;
            if (this.m_CurrentToken == null) return;
            if (this.InInteractiveTransition) return;
            
            this.Traverse.AttemptAction(actionId, this.Character);
        }

        public void TryStateEnter(IdString stateId)
        {
            if (this.Traverse == null) return;
            if (this.m_CurrentToken == null) return;
            if (this.InInteractiveTransition) return;
            
            this.TryStateExit();
            
            this.m_CurrentStateId = stateId;
            this.Traverse.AttemptStateEnter(stateId, this.Character);
        }
        
        public void TryStateExit()
        {
            if (this.Traverse == null) return;
            if (this.m_CurrentToken == null) return;
            if (this.m_CurrentStateId == IdString.EMPTY) return;
            
            this.Traverse.AttemptStateExit(this.m_CurrentStateId, this.Character);
            this.m_CurrentStateId = IdString.EMPTY;
        }
        
        // INTERNAL METHODS: ----------------------------------------------------------------------

        internal async Task<TraversalToken> OnTraverseEnter(Traverse traverse)
        {
            if (this.ForceCancel())
            {
                await Task.Yield();
            }
            
            this.Traverse = traverse;
            this.AllowMovement = true;
            
            this.m_CurrentToken = new TraversalToken();
            this.m_CurrentStateId = IdString.EMPTY;
            
            this.EventMotionEnter?.Invoke();
            
            return this.m_CurrentToken;
        }

        internal void OnTraverseExit(Traverse traverse, TraversalToken token)
        {
            this.EventMotionExit?.Invoke();
            this.TryStateExit();

            if (traverse == this.Traverse) this.Traverse = null;
            if (this.m_CurrentToken == token) this.m_CurrentToken = null;
        }
    }
}