using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Slide and Stick on Surface")]
    [Category("Slide and Stick on Surface")]
    
    [Description("Makes a character move towards an end position but always touching the surface")]
    [Image(typeof(IconTraverseLinkSlideOnSurface), ColorTheme.Type.Green)]
    
    [Keywords("Slide", "Wall", "Run", "Zip")]
    
    [Serializable]
    public class TraverseLinkTypeSlideOnSurface : TraverseLinkType
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Vector3 m_LocalOffset;
        
        [SerializeField] private float m_Width = 4f;
        [SerializeField] private float m_Length = 8f;
        
        [SerializeField] private Vector3 m_RotationA;
        [SerializeField] private Vector3 m_RotationB;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override Vector3 LocalPointA => this.m_LocalOffset + Vector3.forward * this.m_Length * 0.5f;
        
        public override Vector3 LocalPointB => this.m_LocalOffset + Vector3.back * this.m_Length * 0.5f;
        
        public float Width
        {
            get => this.m_Width;
            set => this.m_Width = value;
        }

        public float Length
        {
            get => this.m_Length;
            set => this.m_Length = value;
        }

        public Vector3 LocalOffset
        {
            get => this.m_LocalOffset;
            set => this.m_LocalOffset = value;
        }

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TraverseLinkTypeSlideOnSurface()
        { }

        public TraverseLinkTypeSlideOnSurface(float width, float length, Vector3 offset)
        {
            this.m_Width = width;
            this.m_Length = length;
            this.m_LocalOffset = offset;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink)
        {
            Vector3 startPosition = this.CalculateStartPosition(character, traverseLink);
            Vector3 localStartPosition = traverseLink.Transform.InverseTransformPoint(startPosition);

            return localStartPosition + this.m_LocalOffset;
        }

        public override Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink)
        {
            Vector3 startPosition = CalculateStartPosition(character, traverseLink);
            Vector3 localStartPosition = traverseLink.Transform.InverseTransformPoint(startPosition);
            
            localStartPosition = new Vector3(
                localStartPosition.x + this.m_LocalOffset.x,
                localStartPosition.y + this.m_LocalOffset.y,
                this.m_LocalOffset.z + this.m_Length * 0.5f
            );
            
            return localStartPosition;
        }

        public override Quaternion GetLocalRotationA(Character character, TraverseLink traverseLink)
        {
            return Quaternion.Euler(this.m_RotationA);
        }

        public override Quaternion GetLocalRotationB(Character character, TraverseLink traverseLink)
        {
            return Quaternion.Euler(this.m_RotationB);
        }

        public override void OnDrawGizmos(Transform transform)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 position = this.m_LocalOffset;
            Vector3 size = new Vector3(this.m_Width, 0f, this.m_Length);

            Gizmos.color = GIZMOS_COLOR_LIGHT;
            Gizmos.DrawCube(position, size);
            
            Gizmos.color = GIZMOS_COLOR_SOLID;
            Gizmos.DrawWireCube(position, size);
            
            Gizmos.color = GIZMOS_COLOR_SOLID;
            
            GizmosExtension.Triangle(
                position + Vector3.back * (this.m_Length * 0.5f + 0.35f),
                Quaternion.Euler(this.m_RotationA),
                0.15f
            );
            
            GizmosExtension.Triangle(
                position + Vector3.forward * (this.m_Length * 0.5f + 0.1f),
                Quaternion.Euler(this.m_RotationB),
                0.25f
            );
        }

        private Vector3 CalculateStartPosition(Character character, TraverseLink traverseLink)
        {
            if (traverseLink.MotionLink == null) return default;
            
            Vector3 characterPosition = traverseLink.MotionLink.CharacterPosition(character);
            Plane plane = new Plane(
                traverseLink.Transform.up,
                traverseLink.Transform.TransformPoint(this.m_LocalOffset)
            );
            
            Vector3 closestPosition = plane.ClosestPointOnPlane(characterPosition);

            float halfWidth = m_Width * 0.5f;
            float halfLength = m_Length * 0.5f;
            
            Vector3 localClosestPosition = traverseLink.Transform.InverseTransformPoint(closestPosition);
            
            localClosestPosition.x = this.m_LocalOffset.x + Mathf.Clamp(localClosestPosition.x, -halfWidth, halfWidth);
            localClosestPosition.y = this.m_LocalOffset.y;
            localClosestPosition.z = this.m_LocalOffset.z + Mathf.Clamp(localClosestPosition.z, -halfLength, halfLength);
            
            return traverseLink.Transform.TransformPoint(localClosestPosition);
        }
    }
}