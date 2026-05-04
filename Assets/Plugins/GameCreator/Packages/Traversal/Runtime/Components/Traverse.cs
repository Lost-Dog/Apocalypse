using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [DisallowMultipleComponent]
    
    [Serializable]
    public abstract class Traverse : MonoBehaviour
    {
        public const float MIN_DISTANCE_TRANSITION = 0.25f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private MotionActions m_Actions;
        
        [SerializeField] private bool m_ForceGrounded = true;
        [SerializeField] private Transform m_ParentTo = null;
        
        [SerializeField] private Collider[] m_IgnoreColliders = Array.Empty<Collider>();
        
        [SerializeField] private InstructionList m_OnEnter = new InstructionList();
        [SerializeField] private InstructionList m_OnExit = new InstructionList();
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private List<Character> m_CharactersUsing = new List<Character>();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool ForceGrounded => this.m_ForceGrounded;
        
        public bool IsOccupied => this.m_CharactersUsing.Count > 0;

        public int CharactersUsingCount => this.m_CharactersUsing.Count;
        
        public Transform Transform => this.transform;

        public abstract MotionBase Motion { get; }
        public MotionActions Actions => this.m_Actions;
        
        // EVENTS: --------------------------------------------------------------------------------

        public event Action EventCharacterEnter;
        public event Action EventCharacterExit;
        
        // PUBLIC STATIC METHODS: -----------------------------------------------------------------
        
        public static async Task ChangeTo(Traverse prevTraverse, Traverse nextTraverse, Character character, bool skipTransition)
        {
            Vector3 currentPosition = nextTraverse.Motion.CharacterPosition(character);
            
            switch (nextTraverse)
            {
                case TraverseLink traverseLink:
                    await traverseLink.Run(character);
                    break;
                
                case TraverseInteractive traverseInteractive:
                {
                    Vector3 nextStartPosition = nextTraverse.CalculateStartPosition(character);
                    if (Vector3.Distance(currentPosition, nextStartPosition) <= MIN_DISTANCE_TRANSITION)
                    {
                        skipTransition = true;
                    }
                    
                    AnimationClip exitAnimation = prevTraverse != null && !skipTransition 
                        ? prevTraverse.Motion.GetExitAnimation(
                            nextStartPosition - currentPosition,
                            character.transform.rotation
                        ) : null;
                    
                    InteractiveTransitionData transition = new InteractiveTransitionData(exitAnimation);
                    await traverseInteractive.Enter(character, transition);
                    break;
                }
            }
        }
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnDisable()
        {
            foreach (Character character in this.m_CharactersUsing)
            {
                if (character == null) continue;
                
                TraversalStance stance = character.Combat.RequestStance<TraversalStance>();
                stance.ForceCancel();
            }
        }

        // ACTION METHODS: ------------------------------------------------------------------------

        public void AttemptJump(Character character)
        {
            Args args = new Args(this.gameObject, character.gameObject);
            if (this.CanJump(args))
            {
                _ = this.m_Actions.AttemptJump(this, character);
            }
        }

        public abstract Vector3 CalculateStartPosition(Character character);

        public void AttemptAction(IdString actionId, Character character)
        {
            if (this.Actions != null)
            {
                this.Actions.AttemptAction(actionId, this, character);
            }
        }

        public void AttemptStateEnter(IdString stateId, Character character)
        {
            if (this.Actions != null)
            {
                this.Actions.AttemptStateEnter(stateId, this, character);
            }
        }
        
        public void AttemptStateExit(IdString stateId, Character character)
        {
            if (this.Actions != null)
            {
                this.Actions.AttemptStateExit(stateId, this, character);
            }
        }

        public bool CanCancel(Args args)
        {
            return this.Actions != null && this.Actions.CanCancel(args);
        }

        public bool CanJump(Args args)
        {
            return this.Actions != null && this.Actions.CanJump(args);
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void RefreshCollisions(Character character, bool ignore)
        {
            Collider characterCollider = character.Get<Collider>();
            if (characterCollider == null) return;

            foreach (Collider ignoreCollider in this.m_IgnoreColliders)
            {
                Physics.IgnoreCollision(characterCollider, ignoreCollider, ignore);
            }
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void OnEnter(Character character, Args args)
        {
            character.Driver.ForceGrounded(this.m_ForceGrounded);
            character.Busy.MakeLegsBusy();
            _ = this.m_OnEnter.Run(args);

            if (this.m_ParentTo != null)
            {
                character.Combat.RequestStance<TraversalStance>().Parent = character.transform.parent;
                character.transform.SetParent(this.m_ParentTo, true);
            }
            
            this.m_CharactersUsing.Add(character);
            
            for (int i = this.m_CharactersUsing.Count - 1; i >= 0; --i)
            {
                if (this.m_CharactersUsing[i] == null)
                {
                    this.m_CharactersUsing.RemoveAt(i);
                }
            }
            
            this.EventCharacterEnter?.Invoke();
        }

        protected void OnExit(Character character, Args args)
        {
            if (character != null)
            {
                character.Driver.ForceGrounded(false);
                character.Busy.RemoveLegsBusy();
                
                if (this.m_ParentTo != null)
                {
                    TraversalStance stance = character.Combat.RequestStance<TraversalStance>();
                    character.transform.SetParent(stance.Parent, true);
                    stance.Parent = null;
                }
            }
            
            _ = this.m_OnExit.Run(args);
            
            this.m_CharactersUsing.Remove(character);
            
            for (int i = this.m_CharactersUsing.Count - 1; i >= 0; --i)
            {
                if (this.m_CharactersUsing[i] == null)
                {
                    this.m_CharactersUsing.RemoveAt(i);
                }
            }
            
            this.EventCharacterExit?.Invoke();
        }
        
        // GIZMOS: --------------------------------------------------------------------------------

        protected void OnDrawGizmosSelected()
        {
            this.OnGizmos();
        }
        
        protected abstract void OnGizmos();
    }
}