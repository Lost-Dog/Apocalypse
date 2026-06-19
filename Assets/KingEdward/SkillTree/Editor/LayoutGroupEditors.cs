using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using KingEdward.SkillTree;
using System.Linq;

namespace KingEdward.Editor.SkillTree
{
    /// <summary>
    /// Editor customizado para TreeLayoutGroup
    /// </summary>
    [CustomEditor(typeof(TreeLayoutGroup))]
    public class TreeLayoutGroupEditor : UnityEditor.Editor
    {
        private SerializedProperty spacing;
        private SerializedProperty levelSpacing;
        private TreeLayoutGroup treeLayout;
        
        private void OnEnable()
        {
            treeLayout = (TreeLayoutGroup)target;
            spacing = serializedObject.FindProperty("spacing");
            levelSpacing = serializedObject.FindProperty("levelSpacing");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            
            // Tree Layout Settings (levels from skill prerequisites)
            EditorGUILayout.LabelField("Tree Layout Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spacing);
            EditorGUILayout.PropertyField(levelSpacing);
            
            EditorGUILayout.Space();
            
            // Connection Lines Settings
            EditorGUILayout.LabelField("Connection Lines", EditorStyles.boldLabel);
            SerializedProperty drawLines = serializedObject.FindProperty("drawConnectionLines");
            EditorGUILayout.PropertyField(drawLines);
            
            if (drawLines.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineWidth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineRecess"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("taperedEnds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("roundedEnds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveIntensity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineResolution"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Connection Lines button
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Update Layout & Lines"))
                {
                    // Force immediate layout calculation
                    Canvas.ForceUpdateCanvases();
                    treeLayout.CalculateLayoutInputHorizontal();
                    treeLayout.CalculateLayoutInputVertical();
                    treeLayout.SetLayoutHorizontal();
                    treeLayout.SetLayoutVertical();
                    
                    EditorUtility.SetDirty(treeLayout);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(treeLayout.gameObject.scene);
                }
            }
            
            EditorGUILayout.Space();
            
            // Draw Tree Structure Visualization
            if (Application.isPlaying)
            {
                DrawTreeStructure();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawTreeStructure()
        {
            EditorGUILayout.LabelField("Tree Structure", EditorStyles.boldLabel);
            
            var rectChildren = treeLayout.GetComponentsInChildren<RectTransform>().Where(rt => rt != treeLayout.GetComponent<RectTransform>()).ToList();
            
            if (rectChildren.Count == 0)
            {
                EditorGUILayout.HelpBox("No children found in tree layout.", MessageType.Info);
                return;
            }
            
            // Organizar nós por níveis
            var levels = OrganizeNodesByLevels(rectChildren);
            
            for (int i = 0; i < levels.Count; i++)
            {
                EditorGUILayout.LabelField($"Level {i}: {levels[i].Count} nodes", EditorStyles.miniLabel);
                
                EditorGUI.indentLevel++;
                foreach (var node in levels[i])
                {
                    EditorGUILayout.LabelField($"- {node.name}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
        }
        
        private System.Collections.Generic.List<System.Collections.Generic.List<RectTransform>> OrganizeNodesByLevels(System.Collections.Generic.List<RectTransform> rectChildren)
        {
            var levels = new System.Collections.Generic.List<System.Collections.Generic.List<RectTransform>>();
            var processed = new System.Collections.Generic.HashSet<RectTransform>();
            
            // Encontrar nós raiz
            var rootNodes = rectChildren.Where(child => 
                child.parent == treeLayout.transform || 
                !rectChildren.Contains(child.parent.GetComponent<RectTransform>())
            ).ToList();
            
            foreach (var root in rootNodes)
            {
                ProcessNodeRecursively(root, 0, levels, processed, rectChildren);
            }
            
            return levels;
        }
        
        private void ProcessNodeRecursively(RectTransform node, int level, 
            System.Collections.Generic.List<System.Collections.Generic.List<RectTransform>> levels, 
            System.Collections.Generic.HashSet<RectTransform> processed,
            System.Collections.Generic.List<RectTransform> rectChildren)
        {
            if (processed.Contains(node)) return;
            
            // Garantir que temos a lista para este nível
            while (levels.Count <= level)
            {
                levels.Add(new System.Collections.Generic.List<RectTransform>());
            }
            
            levels[level].Add(node);
            processed.Add(node);
            
            // Processar filhos
            for (int i = 0; i < node.childCount; i++)
            {
                var child = node.GetChild(i).GetComponent<RectTransform>();
                if (child != null && rectChildren.Contains(child))
                {
                    ProcessNodeRecursively(child, level + 1, levels, processed, rectChildren);
                }
            }
        }
    }
    
    /// <summary>
    /// Editor for RadialLayoutGroup — radial config, spacing, connection lines.
    /// </summary>
    [CustomEditor(typeof(RadialLayoutGroup))]
    public class RadialLayoutGroupEditor : UnityEditor.Editor
    {
        private SerializedProperty _radius, _startAngle, _endAngle, _useAutoRadius, _autoRadiusPadding;
        private RadialLayoutGroup _radial;

        private void OnEnable()
        {
            _radial = (RadialLayoutGroup)target;
            _radius = serializedObject.FindProperty("radius");
            _startAngle = serializedObject.FindProperty("startAngle");
            _endAngle = serializedObject.FindProperty("endAngle");
            _useAutoRadius = serializedObject.FindProperty("useAutoRadius");
            _autoRadiusPadding = serializedObject.FindProperty("autoRadiusPadding");
            }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Radial Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_startAngle);
            EditorGUILayout.PropertyField(_endAngle);
            EditorGUILayout.PropertyField(_useAutoRadius);
            EditorGUILayout.PropertyField(_autoRadiusPadding);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connection Lines", EditorStyles.boldLabel);
            SerializedProperty drawLines = serializedObject.FindProperty("drawConnectionLines");
            EditorGUILayout.PropertyField(drawLines);
            if (drawLines.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("connectToCenter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("centerOffset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineWidth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineRecess"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("taperedEnds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("roundedEnds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveIntensity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineResolution"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            if (!Application.isPlaying && GUILayout.Button("Update Layout & Lines"))
            {
                Canvas.ForceUpdateCanvases();
                _radial.CalculateLayoutInputHorizontal();
                _radial.CalculateLayoutInputVertical();
                _radial.SetLayoutHorizontal();
                _radial.SetLayoutVertical();
                EditorUtility.SetDirty(_radial);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_radial.gameObject.scene);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
