using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Optimized line renderer using a single mesh for all lines
    /// Much better performance than multiple GameObjects
    /// Features anti-aliasing for crisp vector-like quality
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRendererOptimized : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            // Use default UI material which supports transparency for anti-aliasing
            material = defaultMaterial;
        }
        [System.Serializable]
        public class LineData
        {
            public RectTransform start;
            public RectTransform end;
            public Color color = Color.white;
            public float width = 3f;
            public float recess = 0.8f;
            public bool tapered = false;
            public bool roundedEnds = false;
            public LineCurveType curveType = LineCurveType.Straight;
            public float curveIntensity = 0.3f;
            public int resolution = 5;
            
            public LineData(RectTransform start, RectTransform end, Color color, float width, float recess, 
                          bool tapered, bool roundedEnds, LineCurveType curveType, float curveIntensity, int resolution)
            {
                this.start = start;
                this.end = end;
                this.color = color;
                this.width = width;
                this.recess = recess;
                this.tapered = tapered;
                this.roundedEnds = roundedEnds;
                this.curveType = curveType;
                this.curveIntensity = curveIntensity;
                this.resolution = resolution;
            }
        }
        
        [SerializeField]
        private List<LineData> lines = new List<LineData>();
        
        [SerializeField]
        private bool updateEveryFrame = false;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            
            if (lines == null || lines.Count == 0)
                return;
            
            RectTransform parentRect = transform as RectTransform;
            if (parentRect == null)
                return;
            
            foreach (var line in lines)
            {
                if (line.start == null || line.end == null)
                    continue;
                
                DrawLine(vh, line, parentRect);
            }
        }
        
        private void DrawLine(VertexHelper vh, LineData line, RectTransform parentRect)
        {
            // Convert world positions to local space
            Vector2 startPos = parentRect.InverseTransformPoint(line.start.position);
            Vector2 endPos = parentRect.InverseTransformPoint(line.end.position);
            
            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            
            if (distance < 0.01f)
                return;
            
            // Apply recess (absolute pixel value)
            if (line.recess > 0f)
            {
                Vector2 normalizedDirection = direction.normalized;
                startPos += normalizedDirection * line.recess;
                endPos -= normalizedDirection * line.recess;
                
                direction = endPos - startPos;
                distance = direction.magnitude;
            }
            
            // Store original positions for rounded ends
            Vector2 originalStart = startPos;
            Vector2 originalEnd = endPos;
            
            // Draw based on curve type
            switch (line.curveType)
            {
                case LineCurveType.Straight:
                    DrawStraightLine(vh, startPos, endPos, line);
                    break;
                    
                case LineCurveType.SingleCurve:
                    DrawCurvedLine(vh, startPos, endPos, line);
                    break;
                    
                case LineCurveType.SCurve:
                    DrawSCurveLine(vh, startPos, endPos, line);
                    break;
            }
            
            // Add rounded ends if requested
            if (line.roundedEnds)
            {
                DrawRoundedEnd(vh, originalStart, line.width, line.color);
                DrawRoundedEnd(vh, originalEnd, line.width, line.color);
            }
        }
        
        private void DrawStraightLine(VertexHelper vh, Vector2 start, Vector2 end, LineData line)
        {
            Vector2 direction = end - start;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
            
            if (line.tapered)
            {
                DrawTaperedLine(vh, start, end, perpendicular, line.width, line.color);
            }
            else
            {
                DrawSimpleLine(vh, start, end, perpendicular, line.width, line.color);
            }
        }
        
        private void DrawCurvedLine(VertexHelper vh, Vector2 start, Vector2 end, LineData line)
        {
            Vector2 direction = end - start;
            float distance = direction.magnitude;
            
            // Calculate curve control point
            Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
            Vector2 curveOffset = perpendicular * (distance * line.curveIntensity * 0.5f);
            Vector2 controlPoint = (start + end) * 0.5f + curveOffset;
            
            // Generate curve points
            List<Vector2> curvePoints = new List<Vector2>();
            for (int i = 0; i <= line.resolution; i++)
            {
                float t = (float)i / line.resolution;
                Vector2 point = BezierQuadratic(start, controlPoint, end, t);
                curvePoints.Add(point);
            }
            
            // Draw segments
            DrawCurveSegments(vh, curvePoints, line);
        }
        
        private void DrawSCurveLine(VertexHelper vh, Vector2 start, Vector2 end, LineData line)
        {
            Vector2 direction = end - start;
            float distance = direction.magnitude;
            
            // Calculate S-curve control points with more pronounced curves
            Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
            Vector2 curveOffset = perpendicular * (distance * line.curveIntensity * 0.6f); // Increased from 0.3f to 0.6f
            
            Vector2 controlPoint1 = start + direction * 0.33f + curveOffset;
            Vector2 controlPoint2 = start + direction * 0.67f - curveOffset;
            
            // Generate S-curve points
            List<Vector2> curvePoints = new List<Vector2>();
            for (int i = 0; i <= line.resolution; i++)
            {
                float t = (float)i / line.resolution;
                Vector2 point = BezierCubic(start, controlPoint1, controlPoint2, end, t);
                curvePoints.Add(point);
            }
            
            // Draw segments
            DrawCurveSegments(vh, curvePoints, line);
        }
        
        private void DrawCurveSegments(VertexHelper vh, List<Vector2> points, LineData line)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 segStart = points[i];
                Vector2 segEnd = points[i + 1];
                Vector2 direction = segEnd - segStart;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
                
                float width = line.width;
                
                if (line.tapered)
                {
                    // Apply taper based on position along curve
                    float t = (float)i / (points.Count - 1);
                    float taperFactor = Mathf.Sin(t * Mathf.PI);
                    width = line.width * taperFactor;
                }
                
                DrawSimpleLine(vh, segStart, segEnd, perpendicular, width, line.color);
            }
        }
        
        private void DrawRoundedEnd(VertexHelper vh, Vector2 center, float diameter, Color color)
        {
            int segments = 12; // More segments for smoother circle
            float radius = diameter * 0.5f;
            float aaWidth = 1f; // Anti-aliasing edge
            
            Color transparentColor = new Color(color.r, color.g, color.b, 0f);
            
            int centerIndex = vh.currentVertCount;
            
            // Center vertex
            vh.AddVert(center, color, Vector2.zero);
            
            // Inner circle (full opacity)
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(center + offset, color, Vector2.zero);
            }
            
            // Outer circle for anti-aliasing (transparent)
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius + aaWidth);
                vh.AddVert(center + offset, transparentColor, Vector2.zero);
            }
            
            // Create triangles for inner circle
            for (int i = 0; i < segments; i++)
            {
                vh.AddTriangle(centerIndex, centerIndex + 1 + i, centerIndex + 1 + i + 1);
            }
            
            // Create triangles for anti-aliasing edge
            int innerStart = centerIndex + 1;
            int outerStart = centerIndex + 1 + segments + 1;
            
            for (int i = 0; i < segments; i++)
            {
                vh.AddTriangle(innerStart + i, innerStart + i + 1, outerStart + i);
                vh.AddTriangle(innerStart + i + 1, outerStart + i + 1, outerStart + i);
            }
        }
        
        private Vector2 BezierQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            
            Vector2 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;
            
            return p;
        }
        
        private Vector2 BezierCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            
            Vector2 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;
            
            return p;
        }
        
        private void DrawSimpleLine(VertexHelper vh, Vector2 start, Vector2 end, Vector2 perpendicular, float width, Color color)
        {
            float halfWidth = width * 0.5f;
            float aaWidth = 1f; // Anti-aliasing edge width in pixels
            
            int startIndex = vh.currentVertCount;
            
            // Create quad with anti-aliasing edges
            Color transparentColor = new Color(color.r, color.g, color.b, 0f);
            
            // Inner vertices (full opacity)
            vh.AddVert(start - perpendicular * halfWidth, color, Vector2.zero);
            vh.AddVert(start + perpendicular * halfWidth, color, Vector2.zero);
            vh.AddVert(end + perpendicular * halfWidth, color, Vector2.zero);
            vh.AddVert(end - perpendicular * halfWidth, color, Vector2.zero);
            
            // Outer vertices for anti-aliasing (transparent)
            vh.AddVert(start - perpendicular * (halfWidth + aaWidth), transparentColor, Vector2.zero);
            vh.AddVert(start + perpendicular * (halfWidth + aaWidth), transparentColor, Vector2.zero);
            vh.AddVert(end + perpendicular * (halfWidth + aaWidth), transparentColor, Vector2.zero);
            vh.AddVert(end - perpendicular * (halfWidth + aaWidth), transparentColor, Vector2.zero);
            
            // Main quad triangles
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
            
            // Anti-aliasing edge triangles (bottom edge)
            vh.AddTriangle(startIndex, startIndex + 3, startIndex + 4);
            vh.AddTriangle(startIndex + 3, startIndex + 7, startIndex + 4);
            
            // Anti-aliasing edge triangles (top edge)
            vh.AddTriangle(startIndex + 1, startIndex + 5, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 5, startIndex + 6);
        }
        
        private void DrawTaperedLine(VertexHelper vh, Vector2 start, Vector2 end, Vector2 perpendicular, float maxWidth, Color color)
        {
            int segments = 3;
            
            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;
                
                Vector2 segStart = Vector2.Lerp(start, end, t1);
                Vector2 segEnd = Vector2.Lerp(start, end, t2);
                
                // Taper factor using sine wave for smooth transition
                float taperFactor1 = Mathf.Sin(t1 * Mathf.PI);
                float taperFactor2 = Mathf.Sin(t2 * Mathf.PI);
                
                float width1 = maxWidth * taperFactor1 * 0.5f;
                float width2 = maxWidth * taperFactor2 * 0.5f;
                
                int startIndex = vh.currentVertCount;
                
                // Create trapezoid vertices
                vh.AddVert(segStart - perpendicular * width1, color, Vector2.zero);
                vh.AddVert(segStart + perpendicular * width1, color, Vector2.zero);
                vh.AddVert(segEnd + perpendicular * width2, color, Vector2.zero);
                vh.AddVert(segEnd - perpendicular * width2, color, Vector2.zero);
                
                // Create triangles
                vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
            }
        }
        
        /// <summary>
        /// Add a line to be rendered
        /// </summary>
        public void AddLine(RectTransform start, RectTransform end, Color color, float width, float recess, 
                          bool tapered, bool roundedEnds, LineCurveType curveType, float curveIntensity, int resolution)
        {
            lines.Add(new LineData(start, end, color, width, recess, tapered, roundedEnds, curveType, curveIntensity, resolution));
            SetVerticesDirty();
        }
        
        /// <summary>
        /// Clear all lines
        /// </summary>
        public void ClearLines()
        {
            lines.Clear();
            SetVerticesDirty();
        }
        
        /// <summary>
        /// Update line color
        /// </summary>
        public void UpdateLineColor(int index, Color newColor)
        {
            if (index >= 0 && index < lines.Count)
            {
                lines[index].color = newColor;
                SetVerticesDirty();
            }
        }
        
        /// <summary>
        /// Force refresh the mesh
        /// </summary>
        public void RefreshMesh()
        {
            SetVerticesDirty();
        }
        
        private void Update()
        {
            if (updateEveryFrame)
            {
                SetVerticesDirty();
            }
        }
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
        #endif
    }
}
