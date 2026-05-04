using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(NavMeshTraverseLink))]
    public class NavMeshTraverseLinkEditor : UnityEditor.Editor
    {
        private const string ERROR = "Traverse Link component is required";
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            SerializedProperty traverseLink = this.serializedObject.FindProperty("m_TraverseLink");
            
            PropertyField fieldTraverseLink = new PropertyField(traverseLink);
            ErrorMessage fieldError = new ErrorMessage(ERROR);
            
            fieldTraverseLink.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
            
            root.Add(new SpaceSmaller());
            root.Add(fieldError);
            root.Add(fieldTraverseLink);
            
            fieldError.style.display = traverseLink.objectReferenceValue == null 
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldTraverseLink.RegisterValueChangeCallback(changeEvent =>
            {
                fieldError.style.display = changeEvent.changedProperty.objectReferenceValue == null
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_AgentTypeID")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Area")));
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_CostModifier")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_AutoUpdateLinks")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_FromPosition")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ToPosition")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Width")));
            
            return root;
        }
    }
}