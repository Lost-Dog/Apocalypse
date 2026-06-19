using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Layout Group que organiza elementos em formato radial/circular
    /// Similar ao Vertical/Horizontal Layout Group, mas para estruturas circulares
    /// </summary>
    [AddComponentMenu("KingEdward/Skill Tree/Radial Layout Group")]
    public class RadialLayoutGroup : LayoutGroup
    {
        [Header("Radial Configuration")]
        [SerializeField] public float radius = 100f;
        [SerializeField] public float startAngle = 0f;
        [SerializeField] public float endAngle = 360f;
        [SerializeField] private bool useAutoRadius = true;
        [SerializeField] private float autoRadiusPadding = 20f;
        
        [Header("Connection Lines")]
        [SerializeField] public bool drawConnectionLines = false;
        [SerializeField] public bool connectToCenter = true;
        [SerializeField] public Vector2 centerOffset = Vector2.zero;
        [SerializeField] public Color lineColor = Color.white;
        [SerializeField] public float lineWidth = 3f;
        [SerializeField] public float lineRecess = 10f;
        [SerializeField] public bool taperedEnds = false;
        [SerializeField] public bool roundedEnds = true;
        [SerializeField] public LineCurveType curveType = LineCurveType.Straight;
        [SerializeField] public float curveIntensity = 0.5f;
        [SerializeField] public int lineResolution = 20;
        
        private UILineRendererOptimized optimizedLineRenderer;
        private Transform linesContainer;
        private GameObject centerPoint;
        
        
        
        // Internal data
        private List<RadialNode> radialNodes = new List<RadialNode>();
        
        [System.Serializable]
        public class RadialNode
        {
            public RectTransform transform;
            public float angle;
            public float distance;
            public Vector2 targetPosition;

            public RadialNode(RectTransform rectTransform)
            {
                transform = rectTransform;
                angle = 0f;
                distance = 0f;
                targetPosition = Vector2.zero;
            }
        }
        
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateRadialLayout();
        }
        
        public override void CalculateLayoutInputVertical()
        {
            // Radial size is driven by horizontal; vertical input not used.
        }
        
        public override void SetLayoutHorizontal()
        {
            ApplyTargetPositions();
        }
        
        public override void SetLayoutVertical()
        {
            // Radial positions applied in SetLayoutHorizontal
        }
        
        private void CalculateRadialLayout()
        {
            BuildRadialNodes();
            CalculateNodePositions();
        }
        
        private void BuildRadialNodes()
        {
            radialNodes.Clear();
            
            // Only get DIRECT children, not recursive
            List<RectTransform> children = new List<RectTransform>();
            for (int i = 0; i < rectChildren.Count; i++)
            {
                if (rectChildren[i] != null)
                {
                    children.Add(rectChildren[i]);
                }
            }
            
            if (children.Count == 0) return;
            
            // Create radial nodes
            foreach (var child in children)
            {
                RadialNode node = new RadialNode(child);
                radialNodes.Add(node);
            }
            
            // Equal angle step between elements (range = Start/End Angle)
            CalculateAngles();
            
            // Calculate distances
            CalculateDistances();
        }
        
        private void CalculateAngles()
        {
            if (radialNodes.Count == 0) return;
            float totalAngle = endAngle - startAngle;
            if (totalAngle < 0) totalAngle += 360f;
            float angleStep = totalAngle / radialNodes.Count;
            for (int i = 0; i < radialNodes.Count; i++)
            {
                radialNodes[i].angle = startAngle + (i * angleStep);
            }
        }
        
        private void CalculateDistances()
        {
            if (useAutoRadius)
            {
                CalculateAutoRadius();
            }
            
            foreach (var node in radialNodes)
            {
                node.distance = radius;
            }
        }
        
        private void CalculateAutoRadius()
        {
            if (radialNodes.Count == 0) return;
            
            // Calculate the minimum radius needed to fit all elements
            float maxElementSize = 0f;
            
            foreach (var node in radialNodes)
            {
                if (node.transform != null)
                {
                    Vector2 elementSize = node.transform.sizeDelta;
                    float elementRadius = Mathf.Max(elementSize.x, elementSize.y) * 0.5f;
                    maxElementSize = Mathf.Max(maxElementSize, elementRadius);
                }
            }
            
            // Calculate radius based on container size with padding
            Rect containerRect = rectTransform.rect;
            float paddingLeft = padding.left;
            float paddingRight = padding.right;
            float paddingTop = padding.top;
            float paddingBottom = padding.bottom;
            
            float availableWidth = containerRect.width - paddingLeft - paddingRight;
            float availableHeight = containerRect.height - paddingTop - paddingBottom;
            float containerRadius = Mathf.Min(availableWidth, availableHeight) * 0.5f;
            
            // Use the smaller of the two requirements
            radius = Mathf.Min(containerRadius - autoRadiusPadding, maxElementSize + autoRadiusPadding);
        }
        
        private void CalculateNodePositions()
        {
            // Get center for positioning skills (no offset)
            Vector2 center = rectTransform.rect.center;
            
            // Apply padding offset
            center.x += (padding.right - padding.left) * 0.5f;
            center.y += (padding.top - padding.bottom) * 0.5f;
            
            foreach (var node in radialNodes)
            {
                // Calculate position based on angle and distance
                float radians = node.angle * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(
                    Mathf.Cos(radians) * node.distance,
                    Mathf.Sin(radians) * node.distance
                );
                
                node.targetPosition = center + offset;
            }
        }
        
        private Vector2 GetCenterPosition()
        {
            // Get center for lines (includes offset)
            Vector2 center = rectTransform.rect.center;
            
            // Apply padding offset
            center.x += (padding.right - padding.left) * 0.5f;
            center.y += (padding.top - padding.bottom) * 0.5f;
            
            // Apply center offset (only for lines)
            center += centerOffset;
            
            return center;
        }
        
        
        private void ApplyTargetPositions()
        {
            foreach (var node in radialNodes)
            {
                if (node.transform != null)
                {
                    // Apply position directly - centerOffset only affects the center point for lines
                    node.transform.anchoredPosition = node.targetPosition;
                }
            }
            
            // Create connection lines after layout
            if (drawConnectionLines)
            {
                CreateConnectionLines();
            }
        }
        
        
        
        // Public methods for external control
        public void AddNode(RectTransform nodeTransform, float customAngle = -1f)
        {
            if (nodeTransform == null) return;
            
            RadialNode newNode = new RadialNode(nodeTransform);
            if (customAngle >= 0f)
            {
                newNode.angle = customAngle;
            }
            
            radialNodes.Add(newNode);
            SetDirty();
        }
        
        public void RemoveNode(RectTransform nodeTransform)
        {
            RadialNode nodeToRemove = radialNodes.FirstOrDefault(n => n.transform == nodeTransform);
            if (nodeToRemove != null)
            {
                radialNodes.Remove(nodeToRemove);
                SetDirty();
            }
        }
        
        public void SetNodeAngle(RectTransform nodeTransform, float angle)
        {
            RadialNode node = radialNodes.FirstOrDefault(n => n.transform == nodeTransform);
            if (node != null)
            {
                node.angle = angle;
                SetDirty();
            }
        }
        
        public void SetNodeDistance(RectTransform nodeTransform, float distance)
        {
            RadialNode node = radialNodes.FirstOrDefault(n => n.transform == nodeTransform);
            if (node != null)
            {
                node.distance = distance;
                SetDirty();
            }
        }
        
        public void SetRadius(float newRadius)
        {
            radius = newRadius;
            useAutoRadius = false;
            SetDirty();
        }
        
        public void SetAngleRange(float start, float end)
        {
            startAngle = start;
            endAngle = end;
            SetDirty();
        }
        
        // Editor helper methods
        [ContextMenu("Rebuild Radial Layout")]
        public void RebuildRadialLayout()
        {
            BuildRadialNodes();
            CalculateNodePositions();
            SetDirty();
        }
        
        [ContextMenu("Auto Calculate Radius")]
        public void AutoCalculateRadius()
        {
            useAutoRadius = true;
            CalculateAutoRadius();
            SetDirty();
        }
        
        // Gizmos for editor visualization
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            
            Vector2 center = GetCenterPosition();
            Vector3 center3D = new Vector3(center.x, center.y, 0);
            
            // Draw radius circle
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center3D, radius);
            
            // Draw angle range
            Gizmos.color = Color.red;
            float startRad = startAngle * Mathf.Deg2Rad;
            float endRad = endAngle * Mathf.Deg2Rad;
            
            Vector3 startDir = new Vector3(Mathf.Cos(startRad), Mathf.Sin(startRad), 0) * radius;
            Vector3 endDir = new Vector3(Mathf.Cos(endRad), Mathf.Sin(endRad), 0) * radius;
            
            Gizmos.DrawLine(center3D, center3D + startDir);
            Gizmos.DrawLine(center3D, center3D + endDir);
        }
        
        // Force layout rebuild when properties change
        private new void SetDirty()
        {
            if (!IsActive()) return;
            
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        
        // Public method to force rebuild from code
        public void ForceRebuild()
        {
            SetDirty();
        }
        
        // Called when properties change in inspector
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
        #endif
        
        /// <summary>
        /// Create connection lines - either to center or between nodes
        /// </summary>
        private void CreateConnectionLines()
        {
            // Find or create lines container
            if (linesContainer == null)
            {
                linesContainer = transform.Find("ConnectionLines");
                if (linesContainer == null)
                {
                    GameObject containerGO = new GameObject("ConnectionLines");
                    linesContainer = containerGO.transform;
                    linesContainer.SetParent(transform, false);
                    linesContainer.SetAsFirstSibling();
                    
                    RectTransform containerRect = containerGO.AddComponent<RectTransform>();
                    containerRect.anchorMin = Vector2.zero;
                    containerRect.anchorMax = Vector2.one;
                    containerRect.sizeDelta = Vector2.zero;
                    containerRect.anchoredPosition = Vector2.zero;
                    
                    // Add LayoutElement to ignore this in layout calculations
                    LayoutElement layoutElement = containerGO.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;
                }
            }
            
            // Find or create optimized renderer
            if (optimizedLineRenderer == null)
            {
                optimizedLineRenderer = linesContainer.GetComponent<UILineRendererOptimized>();
                if (optimizedLineRenderer == null)
                {
                    optimizedLineRenderer = linesContainer.gameObject.AddComponent<UILineRendererOptimized>();
                }
            }
            
            // Clear existing lines
            optimizedLineRenderer.ClearLines();
            
            if (connectToCenter)
            {
                // Create center point if needed
                if (centerPoint == null)
                {
                    centerPoint = new GameObject("CenterPoint");
                    centerPoint.transform.SetParent(linesContainer, false);
                    
                    RectTransform centerRect = centerPoint.AddComponent<RectTransform>();
                    Vector2 center = GetCenterPosition();
                    centerRect.anchorMin = Vector2.zero;
                    centerRect.anchorMax = Vector2.zero;
                    centerRect.sizeDelta = Vector2.zero; // Zero size so recess works from exact center
                    centerRect.anchoredPosition = center;
                }
                else
                {
                    // Update center point position
                    RectTransform centerRect = centerPoint.GetComponent<RectTransform>();
                    Vector2 center = GetCenterPosition();
                    centerRect.anchoredPosition = center;
                }
                
                // Connect all nodes to center
                RectTransform centerRectTransform = centerPoint.GetComponent<RectTransform>();
                foreach (var node in radialNodes)
                {
                    if (node.transform != null)
                    {
                        optimizedLineRenderer.AddLine(
                            centerRectTransform,
                            node.transform,
                            lineColor,
                            lineWidth,
                            lineRecess,
                            taperedEnds,
                            roundedEnds,
                            curveType,
                            curveIntensity,
                            lineResolution
                        );
                    }
                }
            }
            
            // Refresh mesh
            optimizedLineRenderer.RefreshMesh();
        }
    }
}
