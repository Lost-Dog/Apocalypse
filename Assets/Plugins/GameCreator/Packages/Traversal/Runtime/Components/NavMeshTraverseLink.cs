using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.AI;

namespace GameCreator.Runtime.Traversal
{
    [AddComponentMenu("Game Creator/Traversal/NavMesh Traverse Link")]
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoNavMeshTraverseLink.png")]
    
    public class NavMeshTraverseLink : MonoBehaviour, INavMeshTraverseLink
    {
        private const float MIN_DISTANCE = 0.01f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private TraverseLink m_TraverseLink;
        
        [SerializeField] private DriverNavmeshAgentType m_AgentTypeID = new DriverNavmeshAgentType();
        [SerializeField] private DriverNavmeshArea m_Area = new DriverNavmeshArea();
        [SerializeField] private float m_CostModifier;
        [SerializeField] private bool m_AutoUpdateLinks;

        [SerializeField] private Vector3 m_FromPosition;
        [SerializeField] private Vector3 m_ToPosition;
        [SerializeField] private float m_Width;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private NavMeshLinkInstance m_NavMeshLinkInstance;
        
        [NonSerialized] private Vector3 m_LastFromPosition;
        [NonSerialized] private Vector3 m_LastToPosition;
        [NonSerialized] private float m_LastWidth;
        
        // INITIALIZERS: --------------------------------------------------------------------------
        
        private void OnEnable()
        {
            this.AddLink();
        }

        private void OnDisable()
        {
            this.RemoveLink();
        }

        private void Update()
        {
            if (this.m_AutoUpdateLinks) return;
            
            Vector3 currentFromPosition = this.transform.TransformPoint(this.m_FromPosition);
            Vector3 currentToPosition = this.transform.TransformPoint(this.m_ToPosition);
            
            float distanceFrom = Vector3.Distance(this.m_LastFromPosition, currentFromPosition);
            float distanceTo = Vector3.Distance(this.m_LastToPosition, currentToPosition);
            bool sameWidth = Mathf.Approximately(this.m_Width, this.m_LastWidth);
            
            if (distanceFrom > MIN_DISTANCE || distanceTo > MIN_DISTANCE || !sameWidth)
            {
                this.UpdateLinks();
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public async Task Traverse(Character character, Action onFinish)
        {
            if (this.m_TraverseLink == null)
            {
                onFinish?.Invoke();
                return;
            }

            await this.m_TraverseLink.Run(character);
            onFinish?.Invoke();
        }

        public void UpdateLinks()
        {
            this.RemoveLink();
            this.AddLink();
        }

        public void Set(Vector3 positionA, Vector3 positionB, float width)
        {
            this.m_FromPosition = this.transform.InverseTransformPoint(positionA);
            this.m_ToPosition = this.transform.InverseTransformPoint(positionB);
            this.m_Width = width;
            
            if (this.m_AutoUpdateLinks) this.UpdateLinks();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RemoveLink()
        {
            NavMesh.RemoveLink(this.m_NavMeshLinkInstance);
        }
        
        private void AddLink()
        {
            TraverseLinkType traverseLinkType = this.m_TraverseLink?.Type;
            if (traverseLinkType == null) return;
            
            NavMeshLinkData navMeshLinkData = new NavMeshLinkData
            {
                startPosition = this.m_FromPosition,
                endPosition = this.m_ToPosition,
                costModifier = this.m_CostModifier,
                bidirectional = false,
                width = this.m_Width * this.m_TraverseLink.Transform.lossyScale.x,
                area = this.m_Area.Area,
                agentTypeID = this.m_AgentTypeID.AgentType
            };
            
            this.m_NavMeshLinkInstance = NavMesh.AddLink(
                navMeshLinkData,
                this.transform.position,
                this.transform.rotation
            );

            if (NavMesh.IsLinkValid(this.m_NavMeshLinkInstance))
            {
                NavMesh.SetLinkOwner(this.m_NavMeshLinkInstance, this);
                NavMesh.SetLinkActive(this.m_NavMeshLinkInstance, true);
            }

            this.m_LastFromPosition = this.transform.TransformPoint(this.m_FromPosition);
            this.m_LastToPosition = this.transform.TransformPoint(this.m_ToPosition);
            this.m_LastWidth = this.m_Width;
        }
        
        // GIZMOS: --------------------------------------------------------------------------------

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(this.m_FromPosition, this.m_ToPosition);
            
            if (this.m_Width > float.Epsilon)
            {
                Vector3 edgeA1 = this.m_FromPosition - Vector3.right * this.m_Width * 0.5f;
                Vector3 edgeA2 = this.m_FromPosition + Vector3.right * this.m_Width * 0.5f;
                Vector3 edgeB1 = this.m_ToPosition - Vector3.right * this.m_Width * 0.5f;
                Vector3 edgeB2 = this.m_ToPosition + Vector3.right * this.m_Width * 0.5f;
                
                Gizmos.DrawLine(edgeA1, edgeB1);
                Gizmos.DrawLine(edgeA2, edgeB2);
                Gizmos.DrawLine(edgeA1, edgeA2);
                Gizmos.DrawLine(edgeB1, edgeB2);   
            }
        }
    }
}