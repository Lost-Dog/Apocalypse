using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Point to Point")]
    [Category("Point to Point")]
    
    [Description("Makes a character move from a starting position to another one")]
    [Image(typeof(IconTraverseLinkPointToPoint), ColorTheme.Type.Green)]
    
    [Keywords("Vault", "Slide", "Mantle", "Pull")]
    
    [Serializable]
    public class TraverseLinkTypePointToPoint : TraverseLinkType
    {
        [SerializeField] private Vector3 m_LocalPositionA = Vector3.back * 2f;
        [SerializeField] private Vector3 m_LocalPositionB = Vector3.forward * 2f;

        [SerializeField] private Vector3 m_LocalRotationA;
        [SerializeField] private Vector3 m_LocalRotationB;
        
        [SerializeField] private float m_Width;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override Vector3 LocalPointA => this.m_LocalPositionA;

        public override Vector3 LocalPointB => this.m_LocalPositionB;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink)
        {
            Vector3 position = CalculateStartPosition(character, traverseLink); 
            return traverseLink.Transform.InverseTransformPoint(position);
        }

        public override Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink)
        {
            Vector3 position = CalculateEndPosition(character, traverseLink);
            return traverseLink.Transform.InverseTransformPoint(position);
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
            Vector3 pointA = transform.TransformPoint(this.m_LocalPositionA);
            Vector3 pointB = transform.TransformPoint(this.m_LocalPositionB);
            
            Gizmos.color = GIZMOS_COLOR_SOLID;
            Gizmos.DrawLine(pointA, pointB);
            
            Vector3 directionA = Quaternion.Euler(this.m_LocalRotationA) * transform.forward;
            Vector3 directionB = Quaternion.Euler(this.m_LocalRotationB) * transform.forward;
            
            Gizmos.DrawSphere(pointA, 0.05f);
            Gizmos.DrawSphere(pointB, 0.05f);
            
            GizmosExtension.Triangle(
                pointA - directionA.normalized * 0.25f + Vector3.up * 0.01f,
                directionA,
                0.1f
            );
            
            GizmosExtension.Triangle(
                pointB + directionB.normalized * 0.15f + Vector3.up * 0.01f,
                directionB,
                0.2f
            );

            if (this.m_Width > float.Epsilon)
            {
                Vector3 edgeA1 = transform.TransformPoint(this.m_LocalPositionA - Vector3.right * this.m_Width * 0.5f);
                Vector3 edgeA2 = transform.TransformPoint(this.m_LocalPositionA + Vector3.right * this.m_Width * 0.5f);
                Vector3 edgeB1 = transform.TransformPoint(this.m_LocalPositionB - Vector3.right * this.m_Width * 0.5f);
                Vector3 edgeB2 = transform.TransformPoint(this.m_LocalPositionB + Vector3.right * this.m_Width * 0.5f);
            
                Gizmos.color = GIZMOS_COLOR_SOLID;
                Gizmos.DrawLine(edgeA1, edgeB1);
                Gizmos.DrawLine(edgeA2, edgeB2);
                Gizmos.DrawLine(edgeA1, edgeA2);
                Gizmos.DrawLine(edgeB1, edgeB2);   
            }
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Vector3 CalculateStartPosition(Character character, TraverseLink traverseLink)
        {
            if (traverseLink.MotionLink == null) return default;
            Vector3 characterPosition = traverseLink.MotionLink.CharacterPosition(character);
            
            Vector3 a1 = traverseLink.Transform.TransformPoint(this.m_LocalPositionA - Vector3.right * (this.m_Width * 0.5f));
            Vector3 a2 = traverseLink.Transform.TransformPoint(this.m_LocalPositionA + Vector3.right * (this.m_Width * 0.5f));

            return characterPosition.OnSegment(a1, a2);
        }
        
        private Vector3 CalculateEndPosition(Character character, TraverseLink traverseLink)
        {
            if (traverseLink.MotionLink == null) return default;
            Vector3 characterPosition = traverseLink.MotionLink.CharacterPosition(character);
            
            Vector3 a1 = traverseLink.Transform.TransformPoint(this.m_LocalPositionA - Vector3.right * (this.m_Width * 0.5f));
            Vector3 a2 = traverseLink.Transform.TransformPoint(this.m_LocalPositionA + Vector3.right * (this.m_Width * 0.5f));

            Vector3 position = characterPosition.OnSegment(a1, a2);
            float t = Vector3Utils.InverseLerp(a1, a2, position);
            
            Vector3 b1 = traverseLink.Transform.TransformPoint(this.m_LocalPositionB - Vector3.right * (this.m_Width * 0.5f));
            Vector3 b2 = traverseLink.Transform.TransformPoint(this.m_LocalPositionB + Vector3.right * (this.m_Width * 0.5f));
            
            return Vector3.Lerp(b1, b2, t);
        }
    }
}