using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(TraverseLink))]
    public class TraverseLinkEditor : UnityEditor.Editor
    {
        private const string USS_PATH = EditorPaths.PACKAGES + "Traversal/Editor/StyleSheets/Traverse";
        
        private const string ERR_CLIP = "A Traverse Link requires a Motion Link";

        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Root;
        
        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            this.m_Root = new VisualElement();
            
            StyleSheet[] styleSheets = StyleSheetUtils.Load(USS_PATH);
            foreach (StyleSheet styleSheet in styleSheets) this.m_Root.styleSheets.Add(styleSheet);
            
            SerializedProperty clip = this.serializedObject.FindProperty("m_Motion");
            SerializedProperty actions = this.serializedObject.FindProperty("m_Actions");
            SerializedProperty forceGrounded = this.serializedObject.FindProperty("m_ForceGrounded");
            SerializedProperty parentTo = this.serializedObject.FindProperty("m_ParentTo");
            
            ErrorMessage errorClip = new ErrorMessage(ERR_CLIP);
            PropertyField fieldClip = new PropertyField(clip);
            PropertyField fieldActions = new PropertyField(actions);
            PropertyField fieldForceGrounded = new PropertyField(forceGrounded);
            PropertyField fieldParentTo = new PropertyField(parentTo);
            
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(errorClip);
            this.m_Root.Add(fieldClip);
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(fieldActions);
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(fieldForceGrounded);
            this.m_Root.Add(fieldParentTo);

            fieldClip.RegisterValueChangeCallback(changeEvent =>
            {
                errorClip.style.display = changeEvent.changedProperty.objectReferenceValue != null
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            });

            errorClip.style.display = clip.objectReferenceValue != null
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            
            SerializedProperty type = this.serializedObject.FindProperty("m_Type");
            PropertyElement fieldType = new PropertyElement(type, type.displayName, false);

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(fieldType);
            
            SerializedProperty ignoreColliders = this.serializedObject.FindProperty("m_IgnoreColliders");
            ListView listIgnoreColliders = new ListView
            {
                bindingPath = ignoreColliders.propertyPath,
                selectionType = SelectionType.None,
                showBorder = true,
                reorderable = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                headerTitle = ignoreColliders.displayName,
                showAddRemoveFooter = true,
                bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign,
                reorderMode = ListViewReorderMode.Animated,
                makeNoneElement = null,
                allowAdd = true,
                allowRemove = true,
                makeItem = MakeItemObstacle,
                destroyItem = null,
            };
            
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new LabelTitle("Ignore Colliders"));
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(listIgnoreColliders);
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_ContinueTo")));
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("On Enter"));
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_OnEnter")));
            
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new LabelTitle("On Exit"));
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_OnExit")));
            
            return this.m_Root;
        }

        private static VisualElement MakeItemObstacle()
        {
            return new ObjectField
            {
                label = string.Empty,
                objectType = typeof(Collider),
                allowSceneObjects = true,
                style =
                {
                    marginRight = new Length(3, LengthUnit.Pixel)
                }
            };
        }
        
        // CREATION MENU: -------------------------------------------------------------------------
        
        [MenuItem("GameObject/Game Creator/Traversal/Traverse Link", false, 0)]
        public static void CreateElement(MenuCommand menuCommand)
        {
            GameObject instance = new GameObject("Traverse Link");
            instance.AddComponent<TraverseLink>();
            
            GameObjectUtility.SetParentAndAlign(instance, menuCommand?.context as GameObject);
            
            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}