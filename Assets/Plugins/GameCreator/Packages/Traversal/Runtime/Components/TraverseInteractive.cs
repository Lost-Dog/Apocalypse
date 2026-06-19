using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [DisallowMultipleComponent]
    
    [AddComponentMenu("Game Creator/Traversal/Traverse Interactive")]
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoTraverseInteractive.png")]
    
    [Serializable]
    public class TraverseInteractive : Traverse
    {
        public enum CharacterRotationMode
        {
            None,
            Path,
            Fixed
        }
        
        public enum CharacterRotationIdle
        {
            DoNotChange,
            Right,
            Left,
            Along
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private MotionInteractive m_Motion;
        
        [SerializeField] private float m_PositionA = -2f;
        [SerializeField] private float m_PositionB = 2f;

        [SerializeField] private float m_Width;
        
        [SerializeField] private CharacterRotationMode m_RotationMode = CharacterRotationMode.Fixed;
        [SerializeField] private CharacterRotationIdle m_RotationIdle = CharacterRotationIdle.DoNotChange;
        [SerializeField] private Vector3 m_Rotation;

        [SerializeField] private bool m_ExitOnEdgeA;
        [SerializeField] private bool m_ExitOnEdgeB;
        
        [SerializeField] private Traverse m_ContinueA;
        [SerializeField] private Traverse m_ContinueB;
        
        [SerializeField] private List<Connection> m_Connections = new List<Connection>();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override MotionBase Motion => this.m_Motion;
        
        public MotionInteractive MotionInteractive => this.m_Motion;

        public CharacterRotationMode RotationMode => this.m_RotationMode;
        public CharacterRotationIdle RotationIdle => this.m_RotationIdle;
        
        public Vector3 RotationValue => this.m_Rotation;
        
        public float PositionA => this.m_PositionA;
        public float PositionB => this.m_PositionB;
        
        public float Width => this.m_Width;

        public bool ExitOnEdgeA => this.m_ExitOnEdgeA;
        public bool ExitOnEdgeB => this.m_ExitOnEdgeB;
        
        public Traverse ContinueA => this.m_ContinueA;
        public Traverse ContinueB => this.m_ContinueB;
        
        public List<Connection> Connections => this.m_Connections;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async Task Enter(Character character, InteractiveTransitionData transition)
        {
            if (character == null) return;
            if (this.m_Motion == null) return;
            
            TraversalStance traversal = character.Combat.RequestStance<TraversalStance>();
            if (traversal.Traverse == this) return;
            
            Args args = new Args(this.gameObject, character.gameObject);
            TraversalToken token = await traversal.OnTraverseEnter(this);
            
            this.RefreshCollisions(character, true);
            this.OnEnter(character, args);
            
            await this.MotionInteractive.Enter(this, character, args, transition, token);
            
            if (character != null)
            {
                traversal.OnTraverseExit(this, token);
                this.RefreshCollisions(character, false);
            }
            
            this.OnExit(character, args);
        }
        
        public override Vector3 CalculateStartPosition(Character character)
        {
            if (this.MotionInteractive == null) return default;
            
            Vector3 characterPosition = this.MotionInteractive.CharacterPosition(character);
            Plane plane = new Plane(this.Transform.up, this.transform.position);
            
            Vector3 closestPosition = plane.ClosestPointOnPlane(characterPosition);
            
            Vector3 localClosestPosition = this.Transform.InverseTransformPoint(closestPosition);
            localClosestPosition = ClampInBounds(localClosestPosition);
            
            return this.Transform.TransformPoint(localClosestPosition);
        }

        public bool IsCandidateConnection(Character character, Traverse traverse)
        {
            if (traverse == null) return false;
            Args args = new Args(this.gameObject, character.gameObject);
                
            foreach (Connection candidate in this.m_Connections)
            {
                if (candidate?.Traverse == null) continue;
                if (candidate.Traverse != traverse) continue;
                if (candidate.Traverse.Motion.CanUse(args))
                {
                    Vector3 candidatePosition = candidate.Traverse.CalculateStartPosition(character);
                    
                    float distance = Vector3.Distance(
                        character.transform.position,
                        candidatePosition
                    );
                    
                    if (distance <= candidate.MaxDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public Traverse GetCandidateConnection(Character character, Camera view, Vector2 direction)
        {
            Args args = new Args(this.gameObject, character.gameObject);
            
            if (direction == Vector2.zero)
            {
                Traverse traverseClosest = null;
                float minDistance = Mathf.Infinity;
                
                foreach (Connection candidate in this.m_Connections)
                {
                    if (candidate?.Traverse == null) continue;
                    if (!candidate.Traverse.Motion.CanUse(args)) continue;
                    
                    Vector3 candidatePosition = candidate.Traverse.CalculateStartPosition(character);
                    
                    float distance = Vector3.Distance(
                        character.transform.position,
                        candidatePosition
                    );
                    
                    if (distance < minDistance && distance <= candidate.MaxDistance)
                    {
                        traverseClosest = candidate.Traverse;
                        minDistance = distance;
                    }
                }

                return traverseClosest;
            }
            
            Traverse traverseChosen = null;
            float maxSimilitude = Mathf.NegativeInfinity;
            
            foreach (Connection candidate in this.m_Connections)
            {
                if (candidate?.Traverse == null) continue;
                if (!candidate.Traverse.Motion.CanUse(args)) continue;
                
                Vector3 characterRelativePosition = candidate.Traverse.Motion.CharacterPosition(character);
                Vector2 characterScreenSpacePosition = view.WorldToScreenPoint(characterRelativePosition);
                
                Vector3 candidatePosition = candidate.Traverse.CalculateStartPosition(character);
                Vector2 candidateScreenSpacePosition = view.WorldToScreenPoint(candidatePosition);
                
                Vector2 candidateScreenSpaceDirection = (candidateScreenSpacePosition - characterScreenSpacePosition).normalized;
                float directionSimilitude = Vector2.Dot(direction, candidateScreenSpaceDirection);

                if (directionSimilitude < -0.01f) continue;
                if (directionSimilitude < maxSimilitude) continue;
                
                float distance3D = Vector3.Distance(
                    characterRelativePosition,
                    candidatePosition
                );
                
                if (distance3D <= candidate.MaxDistance)
                {
                    traverseChosen = candidate.Traverse;
                    maxSimilitude = directionSimilitude;
                    Debug.DrawLine(characterRelativePosition, candidatePosition, Color.green, 1f);
                }
                else
                {
                    Debug.DrawLine(characterRelativePosition, candidatePosition, Color.red, 1f);
                }
            }

            return traverseChosen;
        }
        
        // INTERNAL METHODS: ----------------------------------------------------------------------

        internal Vector3 ClampInBounds(Vector3 localPosition)
        {
            float halfWidth = this.m_Width * 0.5f;
            
            localPosition.x = Mathf.Clamp(localPosition.x, -halfWidth, halfWidth);
            localPosition.y = 0f;
            localPosition.z = Mathf.Clamp(localPosition.z, this.m_PositionA, this.m_PositionB);

            return localPosition;
        }

        internal bool PushingOutA(Vector3 currentLocalPosition, Vector3 direction)
        {
            if (Mathf.Approximately(currentLocalPosition.z, this.m_PositionA))
            {
                return direction.z < -0.1f;
            }

            return false;
        }
        
        internal bool PushingOutB(Vector3 currentLocalPosition, Vector3 direction)
        {
            if (Mathf.Approximately(currentLocalPosition.z, this.m_PositionB))
            {
                return direction.z > 0.1f;
            }

            return false;
        }
        
        // GIZMO METHODS: -------------------------------------------------------------------------

        protected override void OnGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Vector3 pointA = Vector3.forward * this.m_PositionA;
            Vector3 pointB = Vector3.forward * this.m_PositionB;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA, pointB);
            
            Gizmos.DrawSphere(pointA, 0.05f);
            Gizmos.DrawSphere(pointB, 0.05f);

            if (this.m_RotationMode == CharacterRotationMode.Fixed)
            {
                Vector3 point = Vector3.Lerp(pointA, pointB, 0.5f);
                GizmosExtension.Triangle(point, Quaternion.Euler(this.m_Rotation), 0.2f);
            }

            if (this.m_Width > float.Epsilon)
            {
                Vector3 edgeA1 = Vector3.forward * this.m_PositionA - Vector3.right * this.m_Width * 0.5f;
                Vector3 edgeA2 = Vector3.forward * this.m_PositionA + Vector3.right * this.m_Width * 0.5f;
                Vector3 edgeB1 = Vector3.forward * this.m_PositionB - Vector3.right * this.m_Width * 0.5f;
                Vector3 edgeB2 = Vector3.forward * this.m_PositionB + Vector3.right * this.m_Width * 0.5f;
            
                Gizmos.color = Color.green;
                Gizmos.DrawLine(edgeA1, edgeB1);
                Gizmos.DrawLine(edgeA2, edgeB2);
                Gizmos.DrawLine(edgeA1, edgeA2);
                Gizmos.DrawLine(edgeB1, edgeB2);
            }

            foreach (Connection connection in this.Connections)
            {
                if (connection?.Traverse == null) continue;
                Debug.DrawLine(this.transform.position, connection.Traverse.transform.position, Color.yellow);
            }
        }
    }
}