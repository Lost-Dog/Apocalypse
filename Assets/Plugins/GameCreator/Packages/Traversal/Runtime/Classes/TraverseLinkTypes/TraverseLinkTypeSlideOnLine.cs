using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Slide and Stick on Line")]
    [Category("Slide and Stick on Line")]
    
    [Description("Makes a character move towards an end position but always touching the line")]
    [Image(typeof(IconTraverseLinkSlideOnSurface), ColorTheme.Type.Green)]
    
    [Keywords("Slide", "Wall", "Run", "Zip")]
    
    [Serializable]
    public class TraverseLinkTypeSlideOnLine : TraverseLinkType
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Vector3 m_LocalPositionA = Vector3.zero;
        [SerializeField] private Vector3 m_LocalPositionB = Vector3.forward * 10f;
        
        [SerializeField] private Vector3 m_LocalRotationA;
        [SerializeField] private Vector3 m_LocalRotationB;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override Vector3 LocalPointA => this.m_LocalPositionA;
        
        public override Vector3 LocalPointB => this.m_LocalPositionB;
        
        public Vector3 LocalPositionA
        {
            get => this.m_LocalPositionA;
            set => this.m_LocalPositionA = value;
        }

        public Vector3 LocalPositionB
        {
            get => this.m_LocalPositionB;
            set => this.m_LocalPositionB = value;
        }

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TraverseLinkTypeSlideOnLine()
        { }

        public TraverseLinkTypeSlideOnLine(Vector3 localPositionA, Vector3 localPositionB)
        {
            this.m_LocalPositionA = localPositionA;
            this.m_LocalPositionB = localPositionB;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink)
        {
            Vector3 startPosition = this.CalculateStartPosition(character, traverseLink);
            Vector3 localStartPosition = traverseLink.Transform.InverseTransformPoint(startPosition);

            return localStartPosition;
        }

        public override Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink)
        {
            return this.m_LocalPositionB;
        }

        public override Quaternion GetLocalRotationA(Character character, TraverseLink traverseLink)
        {
            return Quaternion.Euler(this.m_LocalRotationA);
        }

        public override Quaternion GetLocalRotationB(Character character, TraverseLink traverseLink)
        {
            return Quaternion.Euler(this.m_LocalRotationB);
        }

        public override void OnDrawGizmos(Transform transform)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = GIZMOS_COLOR_SOLID;
            Gizmos.DrawLine(this.m_LocalPositionA, this.m_LocalPositionB);
            
            GizmosExtension.Triangle(
                this.m_LocalPositionA,
                Quaternion.Euler(this.m_LocalRotationA),
                0.15f
            );
            
            GizmosExtension.Triangle(
                this.m_LocalPositionB,
                Quaternion.Euler(this.m_LocalRotationB),
                0.25f
            );
        }

        private Vector3 CalculateStartPosition(Character character, TraverseLink traverseLink)
        {
            if (traverseLink.MotionLink == null) return default;
            
            return traverseLink.MotionLink.CharacterPosition(character).OnSegment(
                traverseLink.Transform.TransformPoint(this.m_LocalPositionA),
                traverseLink.Transform.TransformPoint(this.m_LocalPositionB)
            );
        }
    }
}