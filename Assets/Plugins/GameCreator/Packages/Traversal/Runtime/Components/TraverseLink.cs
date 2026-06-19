using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [DisallowMultipleComponent]
    
    [AddComponentMenu("Game Creator/Traversal/Traverse Link")]
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoTraverseLink.png")]
    
    [Serializable]
    public class TraverseLink : Traverse
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private MotionLink m_Motion;
        [SerializeReference] private TraverseLinkType m_Type = new TraverseLinkTypePointToPoint();

        [SerializeField] private Traverse m_ContinueTo;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override MotionBase Motion => this.m_Motion;
        
        public MotionLink MotionLink => this.m_Motion;
        
        public TraverseLinkType Type => this.m_Type;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async Task Run(Character character)
        {
            if (character == null) return;
            if (this.m_Motion == null) return;

            Args args = new Args(this.gameObject, character.gameObject);
            
            TraversalStance traversal = character.Combat.RequestStance<TraversalStance>();
            if (traversal.Traverse == this) return;
            
            TraversalToken token = await traversal.OnTraverseEnter(this);
            
            this.RefreshCollisions(character, true);
            this.OnEnter(character, args);
            
            bool canFollowThrough = await this.m_Motion.Run(character, args, token);
            
            if (character != null)
            {
                traversal.OnTraverseExit(this, token);
                this.RefreshCollisions(character, false);
            }
            
            this.OnExit(character, args);
            
            if (canFollowThrough && this.m_ContinueTo != null)
            {
                _ = ChangeTo(this, this.m_ContinueTo, character, false);
            }
        }
        
        public override Vector3 CalculateStartPosition(Character character)
        {
            return this.Transform.position;
        }

        // GIZMOS: --------------------------------------------------------------------------------

        protected override void OnGizmos()
        {
            this.m_Type?.OnDrawGizmos(this.Transform);
        }
    }
}