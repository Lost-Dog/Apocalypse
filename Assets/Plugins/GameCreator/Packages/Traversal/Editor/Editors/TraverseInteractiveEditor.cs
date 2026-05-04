using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(TraverseInteractive))]
    public class TraverseInteractiveEditor : UnityEditor.Editor
    {
        private const string USS_PATH = EditorPaths.PACKAGES + "Traversal/Editor/StyleSheets/Traverse";
        
        private const string ERR_CLIP = "A Traverse Interactive requires a Motion Interactive";

        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Root;
        
        public float MaxDistance
        {
            get => EditorPrefs.GetFloat("gc:traversal:connections-max-distance", 5f);
            set => EditorPrefs.SetFloat("gc:traversal:connections-max-distance", value);
        }
        
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
            
            SerializedProperty positionA = this.serializedObject.FindProperty("m_PositionA");
            SerializedProperty positionB = this.serializedObject.FindProperty("m_PositionB");
            SerializedProperty width = this.serializedObject.FindProperty("m_Width");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(positionA));
            this.m_Root.Add(new PropertyField(positionB));
            this.m_Root.Add(new PropertyField(width));
            
            SerializedProperty rotationMode = this.serializedObject.FindProperty("m_RotationMode");
            SerializedProperty rotationIdle = this.serializedObject.FindProperty("m_RotationIdle");
            SerializedProperty rotation = this.serializedObject.FindProperty("m_Rotation");
            
            PropertyField fieldRotationMode = new PropertyField(rotationMode);
            PropertyField fieldRotationIdle = new PropertyField(rotationIdle);
            PropertyField fieldRotation = new PropertyField(rotation);
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(fieldRotationMode);
            this.m_Root.Add(fieldRotationIdle);
            this.m_Root.Add(fieldRotation);
            
            fieldRotationMode.RegisterValueChangeCallback(changeEvent =>
            {
                fieldRotationIdle.style.display = changeEvent.changedProperty.enumValueIndex == 1
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                
                fieldRotation.style.display = changeEvent.changedProperty.enumValueIndex == 2
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            fieldRotationIdle.style.display = rotationMode.enumValueIndex == 1
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldRotation.style.display = rotationMode.enumValueIndex == 2
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
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
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_ExitOnEdgeA")));
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_ExitOnEdgeB")));
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_ContinueA")));
            this.m_Root.Add(new PropertyField(this.serializedObject.FindProperty("m_ContinueB")));

            SerializedProperty connections = this.serializedObject.FindProperty("m_Connections");
            ListView listConnections = new ListView
            {
                bindingPath = connections.propertyPath,
                selectionType = SelectionType.None,
                showBorder = true,
                reorderable = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                showFoldoutHeader = false,
                headerTitle = connections.displayName,
                showAddRemoveFooter = true,
                bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign,
                reorderMode = ListViewReorderMode.Animated,
                makeNoneElement = null,
                allowAdd = true,
                allowRemove = true,
                destroyItem = null
            };
            
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new LabelTitle("Connections"));
            this.m_Root.Add(new SpaceSmallest());
            
            VisualElement connectionsButtons = new VisualElement { name = "ConnectionsButtons" };
            FloatField connectionDistance = new FloatField(string.Empty) { value = this.MaxDistance };
            connectionDistance.RegisterValueChangedCallback(changeEvent =>
            {
                this.MaxDistance = changeEvent.newValue;
            });
            
            connectionsButtons.Add(connectionDistance);
            connectionsButtons.Add(new Button(this.CollectConnections)
            {
                text = "Update Connections"
            });
            
            this.m_Root.Add(connectionsButtons);
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(listConnections);
            
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

        private void CollectConnections()
        {
            Traverse[] instances = FindObjectsByType<Traverse>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            Transform transform = ((TraverseInteractive) this.target).transform;
            
            SerializedProperty connections = this.serializedObject.FindProperty("m_Connections");
            connections.ClearArray();
            foreach (Traverse instance in instances)
            {
                if (instance == this.target) continue;
                
                float distance = Vector3.Distance(instance.transform.position, transform.position); 
                if (distance > this.MaxDistance) continue;
                
                connections.InsertArrayElementAtIndex(0);
                SerializedProperty connection = connections.GetArrayElementAtIndex(0);
                connection.FindPropertyRelative("m_Traverse").objectReferenceValue = instance;
                connection.FindPropertyRelative("m_MaxDistance.m_IsEnabled").boolValue = true;
                connection.FindPropertyRelative("m_MaxDistance.m_Value").floatValue = this.MaxDistance;
            }

            this.serializedObject.ApplyModifiedProperties();
            this.serializedObject.Update();
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
        
        [MenuItem("GameObject/Game Creator/Traversal/Traverse Interactive", false, 0)]
        public static void CreateElement(MenuCommand menuCommand)
        {
            GameObject instance = new GameObject("Traverse Interactive");
            instance.AddComponent<TraverseInteractive>();
            
            GameObjectUtility.SetParentAndAlign(instance, menuCommand?.context as GameObject);
            
            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}