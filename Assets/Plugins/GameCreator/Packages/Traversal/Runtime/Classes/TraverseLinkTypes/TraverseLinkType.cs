using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Traverse Link Type")]
    
    [Serializable]
    public abstract class TraverseLinkType : ITraverseLinkType
    {
        protected static readonly Color GIZMOS_COLOR_SOLID = Color.green;
        protected static readonly Color GIZMOS_COLOR_LIGHT = new Color(0f, 1f, 0f, 0.1f);
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public abstract Vector3 LocalPointA { get; }
        public abstract Vector3 LocalPointB { get; }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public abstract Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink);
        public abstract Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink);

        public abstract Quaternion GetLocalRotationA(Character character, TraverseLink traverseLink);
        public abstract Quaternion GetLocalRotationB(Character character, TraverseLink traverseLink);

        public TraverseLinkData ToTraverseLinkData(Character character, TraverseLink traverseLink)
        {
            return new TraverseLinkData(
                this.GetLocalPositionA(character, traverseLink),
                this.GetLocalPositionB(character, traverseLink),
                this.GetLocalRotationA(character, traverseLink),
                this.GetLocalRotationB(character, traverseLink)
            );
        }
        
        public abstract void OnDrawGizmos(Transform transform);
    }
}