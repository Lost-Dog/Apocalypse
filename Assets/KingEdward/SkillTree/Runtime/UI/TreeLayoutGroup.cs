using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Layout group that arranges UI elements in a hierarchical tree structure
    /// </summary>
    [AddComponentMenu("KingEdward/Skill Tree/Tree Layout Group")]
    public class TreeLayoutGroup : LayoutGroup
    {
        [Header("Tree Layout Settings")]
        [SerializeField] public float spacing = 20f;
        [SerializeField] public float levelSpacing = 50f;
        
        [Header("Connection Lines")]
        [SerializeField] public bool drawConnectionLines = false;
        [SerializeField] public Color lineColor = Color.white;
        [SerializeField] public float lineWidth = 3f;
        [SerializeField] public float lineRecess = 10f;
        [SerializeField] public bool taperedEnds = false;
        [SerializeField] public bool roundedEnds = true;
        [SerializeField] public LineCurveType curveType = LineCurveType.SingleCurve;
        [SerializeField] public float curveIntensity = 0.5f;
        [SerializeField] public int lineResolution = 20;
        
        private UILineRendererOptimized optimizedLineRenderer;
        private Transform linesContainer;
        
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }
        
        
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateTreeLayout();
        }
        
        public override void CalculateLayoutInputVertical()
        {
            CalculateTreeLayout();
        }
        
        public override void SetLayoutHorizontal()
        {
            // Positions applied in CalculateTreeLayout (called from CalculateLayoutInput*)
        }
        
        public override void SetLayoutVertical()
        {
            // Positions applied in CalculateTreeLayout (called from CalculateLayoutInput*)
        }
        
        private void CalculateTreeLayout()
        {
            if (rectChildren == null || rectChildren.Count == 0)
                return;
                
            // Only get DIRECT children, not recursive
            var children = new List<RectTransform>();
            for (int i = 0; i < rectChildren.Count; i++)
            {
                if (rectChildren[i] != null)
                {
                    children.Add(rectChildren[i]);
                }
            }
                
            if (children.Count == 0)
                return;
                
            
            // Group by skill prerequisites (roots = no prereqs, then dependents by level)
            var levelGroups = GroupBySkillPrerequisites(children);
            float currentY = padding.top;
            
            foreach (var level in levelGroups)
            {
                float levelWidth = CalculateLevelWidth(level);
                float startX = (rectTransform.rect.width - levelWidth) / 2f;
                float maxHeightInLevel = 0f;

                for (int i = 0; i < level.Count; i++)
                {
                    var child = level[i];
                    float w = child.rect.width;
                    float h = child.rect.height;
                    if (h > maxHeightInLevel) maxHeightInLevel = h;
                    float x = startX + (i * (w + spacing));
                    child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, w);
                    child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, currentY, h);
                }

                currentY += maxHeightInLevel + levelSpacing;
            }
            
            // Create connection lines after layout
            if (drawConnectionLines)
            {
                CreateConnectionLines(children);
            }
        }
        
        /// <summary>
        /// Groups children by skill prerequisites (roots = no prereqs, then dependents by level). Falls back to hierarchy if no SkillItemUI.
        /// </summary>
        private List<List<RectTransform>> GroupBySkillPrerequisites(List<RectTransform> children)
        {
            var levels = new List<List<RectTransform>>();
            var processed = new HashSet<RectTransform>();
            
            // Find skills with no prerequisites (level 0)
            var rootSkills = new List<RectTransform>();
            foreach (var child in children)
            {
                var skillItemUI = child.GetComponent<SkillItemUI>();
                if (skillItemUI != null && skillItemUI.skill != null)
                {
                    // Check if this skill has prerequisites
                    bool hasPrerequisites = skillItemUI.skill.prerequisites != null && 
                                           skillItemUI.skill.prerequisites.Count > 0;
                    
                    if (!hasPrerequisites)
                    {
                        rootSkills.Add(child);
                    }
                }
            }
            
            // If no skill components found, fall back to transform hierarchy
            if (rootSkills.Count == 0)
            {
                return GroupByHierarchy(children);
            }
            
            foreach (var rootSkill in rootSkills)
            {
                ProcessSkillHierarchy(rootSkill, 0, levels, processed, children);
            }
            
            return levels;
        }
        
        private List<List<RectTransform>> GroupByHierarchy(List<RectTransform> children)
        {
            var levels = new List<List<RectTransform>>();
            var processed = new HashSet<RectTransform>();
            var rootElements = children.Where(c => c.parent == rectTransform || !children.Contains(c.parent.GetComponent<RectTransform>())).ToList();
            foreach (var root in rootElements)
            {
                ProcessNodeHierarchy(root, 0, levels, processed, children);
            }
            return levels;
        }
        
        private void ProcessNodeHierarchy(RectTransform node, int level,
            List<List<RectTransform>> levels, HashSet<RectTransform> processed,
            List<RectTransform> allChildren)
        {
            if (processed.Contains(node)) return;
            while (levels.Count <= level) levels.Add(new List<RectTransform>());
            levels[level].Add(node);
            processed.Add(node);
            for (int i = 0; i < node.childCount; i++)
            {
                var child = node.GetChild(i).GetComponent<RectTransform>();
                if (child != null && allChildren.Contains(child))
                    ProcessNodeHierarchy(child, level + 1, levels, processed, allChildren);
            }
        }
        
        private void ProcessSkillHierarchy(RectTransform skillNode, int level, 
            List<List<RectTransform>> levels, HashSet<RectTransform> processed, 
            List<RectTransform> allChildren)
        {
            if (processed.Contains(skillNode)) return;
            
            // Ensure we have a list for this level
            while (levels.Count <= level)
            {
                levels.Add(new List<RectTransform>());
            }
            
            levels[level].Add(skillNode);
            processed.Add(skillNode);
            
            // Find skills that depend on this one
            var skillItemUI = skillNode.GetComponent<SkillItemUI>();
            if (skillItemUI != null && skillItemUI.skill != null)
            {
                foreach (var child in allChildren)
                {
                    var childSkillUI = child.GetComponent<SkillItemUI>();
                    if (childSkillUI != null && childSkillUI.skill != null)
                    {
                        // Check if this child skill has the current skill as prerequisite
                        bool isPrerequisite = childSkillUI.skill.prerequisites != null && 
                                            childSkillUI.skill.prerequisites.Any(prereq => prereq != null && prereq.skill == skillItemUI.skill);
                        
                        if (isPrerequisite)
                        {
                            ProcessSkillHierarchy(child, level + 1, levels, processed, allChildren);
                        }
                    }
                }
            }
        }
        
        private float CalculateLevelWidth(List<RectTransform> level)
        {
            if (level.Count == 0)
                return 0f;
                
            float totalWidth = 0f;
            foreach (var child in level)
            {
                totalWidth += child.rect.width;
            }
            
            totalWidth += (level.Count - 1) * spacing;
            return totalWidth;
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
        /// Create connection lines between skills based on prerequisites
        /// </summary>
        private void CreateConnectionLines(List<RectTransform> children)
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
            
            // Build skill map
            Dictionary<Skill, RectTransform> skillToRectMap = new Dictionary<Skill, RectTransform>();
            foreach (var child in children)
            {
                SkillItemUI skillUI = child.GetComponent<SkillItemUI>();
                if (skillUI != null && skillUI.skill != null)
                {
                    skillToRectMap[skillUI.skill] = child;
                }
            }
            
            // Create lines based on prerequisites
            foreach (var child in children)
            {
                SkillItemUI skillUI = child.GetComponent<SkillItemUI>();
                if (skillUI == null || skillUI.skill == null) continue;
                
                foreach (var prereq in skillUI.skill.prerequisites)
                {
                    if (prereq == null || prereq.skill == null) continue;
                    
                    RectTransform prereqRect;
                    if (skillToRectMap.TryGetValue(prereq.skill, out prereqRect))
                    {
                        optimizedLineRenderer.AddLine(
                            prereqRect, 
                            child, 
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