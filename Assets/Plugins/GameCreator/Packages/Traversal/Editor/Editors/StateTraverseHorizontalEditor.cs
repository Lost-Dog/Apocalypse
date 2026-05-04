using GameCreator.Editor.Characters;
using GameCreator.Editor.Common;
using UnityEditor;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(StateTraverseHorizontal))]
    public class StateTraverseHorizontalEditor : StateOverrideAnimatorEditor
    {
        // PAINT METHODS: -------------------------------------------------------------------------

        protected override void CreateContent()
        {
            SerializedProperty idle = this.serializedObject.FindProperty("m_Idle");
            SerializedProperty moveF = this.serializedObject.FindProperty("m_MoveForward");
            SerializedProperty moveB = this.serializedObject.FindProperty("m_MoveBackward");
            SerializedProperty moveR = this.serializedObject.FindProperty("m_MoveRight");
            SerializedProperty moveL = this.serializedObject.FindProperty("m_MoveLeft");
            SerializedProperty moveFR = this.serializedObject.FindProperty("m_MoveForwardRight");
            SerializedProperty moveFL = this.serializedObject.FindProperty("m_MoveForwardLeft");
            SerializedProperty moveBR = this.serializedObject.FindProperty("m_MoveBackwardRight");
            SerializedProperty moveBL = this.serializedObject.FindProperty("m_MoveBackwardLeft");
            SerializedProperty edgeF = this.serializedObject.FindProperty("m_EdgeForward");
            SerializedProperty edgeB = this.serializedObject.FindProperty("m_EdgeBackward");
            SerializedProperty edgeR = this.serializedObject.FindProperty("m_EdgeRight");
            SerializedProperty edgeL = this.serializedObject.FindProperty("m_EdgeLeft");
            
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(idle));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(moveF));
            this.m_Root.Add(new PropertyField(moveB));
            this.m_Root.Add(new PropertyField(moveR));
            this.m_Root.Add(new PropertyField(moveL));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(moveFR));
            this.m_Root.Add(new PropertyField(moveFL));
            this.m_Root.Add(new PropertyField(moveBR));
            this.m_Root.Add(new PropertyField(moveBL));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(edgeF));
            this.m_Root.Add(new PropertyField(edgeB));
            this.m_Root.Add(new PropertyField(edgeR));
            this.m_Root.Add(new PropertyField(edgeL));
            this.m_Root.Add(new SpaceSmall());
        }

        // CREATE STATE: --------------------------------------------------------------------------

        [MenuItem("Assets/Create/Game Creator/Characters/Traverse State Horizontal", false, 0)]
        internal static void CreateFromMenuItem()
        {
            StateTraverseHorizontal state = CreateState<StateTraverseHorizontal>(
                "Traverse State Horizontal",
                RuntimePaths.PACKAGES + "Traversal/Runtime/Animator/TraversalOverrideXZ.overrideController"
            );
            
            state.name = "Traverse State Horizontal";
        }
    }
}