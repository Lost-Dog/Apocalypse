using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(Rope))]
    public class RopeEditor : UnityEditor.Editor
    {
        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            StyleSheet[] styleSheets = StyleSheetUtils.Load();
            foreach (StyleSheet styleSheet in styleSheets) root.styleSheets.Add(styleSheet);

            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Resolution")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_SolverIterations")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Hook")));
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_HookRotation")));
            
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Config")));
            
            return root;
        }
    }
}