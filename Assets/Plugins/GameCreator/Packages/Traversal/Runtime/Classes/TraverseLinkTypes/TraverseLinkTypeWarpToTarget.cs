using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Warp to Target")]
    [Category("Warp to Target")]
    
    [Description("Makes a character move from its current position towards the projected end position")]
    [Image(typeof(IconTraverseLinkWarpToTarget), ColorTheme.Type.Green)]
    
    [Keywords("Slide", "Wall", "Run", "Zip")]
    
    [Serializable]
    public class TraverseLinkTypeWarpToTarget : TraverseLinkType
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Vector3 m_LocalOffset;
        
        [SerializeField] private float m_Width = 4f;
        
        [SerializeField] private EnablerVector3 m_Rotation = new EnablerVector3(true, Vector3.zero);

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override Vector3 LocalPointA => this.m_LocalOffset;
        
        public override Vector3 LocalPointB => this.m_LocalOffset;
        
        public float Width
        {
            get => this.m_Width;
            set => this.m_Width = value;
        }

        public Vector3 LocalOffset
        {
            get => this.m_LocalOffset;
            set => this.m_LocalOffset = value;
        }

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TraverseLinkTypeWarpToTarget()
        { }

        public TraverseLinkTypeWarpToTarget(float width, Vector3 offset)
        {
            this.m_Width = width;
            this.m_LocalOffset = offset;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink)
        {
            Vector3 startPosition = this.CalculateStartPosition(character, traverseLink);
            return traverseLink.Transform.InverseTransformPoint(startPosition);
        }

        public override Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink)
        {
            Vector3 startPosition = CalculateStartPosition(character, traverseLink);

            Vector3 offset1 = this.m_LocalOffset + Vector3.right * (this.m_Width * 0.5f);
            Vector3 offset2 = this.m_LocalOffset + Vector3.left * (this.m_Width * 0.5f);
            
            Vector3 endPosition1 = traverseLink.transform.TransformPoint(offset1);
            Vector3 endPosition2 = traverseLink.transform.TransformPoint(offset2);
            
            Vector3 endPosition = startPosition.OnSegment(endPosition1, endPosition2);
            return traverseLink.Transform.InverseTransformPoint(endPosition);
        }

        public override Quaternion GetLocalRotationA(Character character, TraverseLink traverseLink)
        {
            return Quaternion.identity;
        }

        public override Quaternion GetLocalRotationB(Character character, TraverseLink traverseLink)
        {
            if (this.m_Rotation.IsEnabled)
            {
                return Quaternion.Euler(this.m_Rotation.Value);
            }
            
            Vector3 direction = traverseLink.transform.position - character.transform.position;
            direction.y = 0f;
            
            Vector3 localDirection = traverseLink.transform.InverseTransformDirection(direction);
            return Quaternion.LookRotation(localDirection);
        }

        public override void OnDrawGizmos(Transform transform)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 position = this.m_LocalOffset;
            
            Vector3 position1 = position + Vector3.right * (this.m_Width * 0.5f);
            Vector3 position2 = position - Vector3.right * (this.m_Width * 0.5f);

            Gizmos.color = GIZMOS_COLOR_SOLID;
            Gizmos.DrawLine(position1, position2);

            if (this.m_Rotation.IsEnabled)
            {
                GizmosExtension.Triangle(
                    position + Vector3.back * 0.35f,
                    Quaternion.Euler(this.m_Rotation.Value),
                    0.15f
                );
            }
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
            
            Vector3 localClosestPosition = traverseLink.Transform.InverseTransformPoint(closestPosition);
            
            localClosestPosition.x = this.m_LocalOffset.x + Mathf.Clamp(localClosestPosition.x, -halfWidth, halfWidth);
            localClosestPosition.y = this.m_LocalOffset.y;
            localClosestPosition.z = this.m_LocalOffset.z;
            
            return traverseLink.Transform.TransformPoint(localClosestPosition);
        }
    }
}