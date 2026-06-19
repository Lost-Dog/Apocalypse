using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [AddComponentMenu("Game Creator/Traversal/Utils/Zipline")]
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoZipline.png")]
    
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    [RequireComponent(typeof(LineRenderer))]
    
    [Serializable]
    [ExecuteAlways]
    public class Zipline : MonoBehaviour
    {
        private const float EPSILON = 0.001f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private TraverseLink m_TraverseLink;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private LineRenderer m_LineRenderer;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [field: NonSerialized] public Vector3[] Points { get; private set; }
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_LineRenderer = this.GetComponent<LineRenderer>();

            if (!this.m_LineRenderer.useWorldSpace)
            {
                this.m_LineRenderer.useWorldSpace = true;
            }
            
            this.Points = new Vector3[2];
            if (this.UpdatePoints())
            {
                this.UpdateLineRenderer();
            }
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        private void Update()
        {
            if (this.m_LineRenderer == null) return;
            if (this.m_TraverseLink == null) return;
            
            if (this.UpdatePoints())
            {
                this.UpdateLineRenderer();
            }
        }

        private bool UpdatePoints()
        {
            if (this.m_TraverseLink == null) return false;
            
            Vector3 pointA = this.m_TraverseLink.transform.TransformPoint(this.m_TraverseLink.Type.LocalPointA);
            Vector3 pointB = this.m_TraverseLink.transform.TransformPoint(this.m_TraverseLink.Type.LocalPointB);

            if (this.Points.Length != 2)
            {
                this.Points = new Vector3[2];
            }
            else if (Vector3.Distance(this.Points[0], pointA) <= EPSILON && 
                     Vector3.Distance(this.Points[1], pointB) <= EPSILON)
            {
                return false;
            }
            
            this.Points[0] = pointA;
            this.Points[1] = pointB;
            
            return true;
        }
        
        private void UpdateLineRenderer()
        {
            this.m_LineRenderer.positionCount = 2;
            this.m_LineRenderer.SetPositions(this.Points);
        }
    }
}