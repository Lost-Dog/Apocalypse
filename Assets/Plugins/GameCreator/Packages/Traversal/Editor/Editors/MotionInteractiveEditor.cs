using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(MotionInteractive))]
    public class MotionInteractiveEditor : UnityEditor.Editor
    {
        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            StyleSheet[] styleSheets = StyleSheetUtils.Load();
            foreach (StyleSheet styleSheet in styleSheets) root.styleSheets.Add(styleSheet);

            SerializedProperty canUse = this.serializedObject.FindProperty("m_CanUse");
            SerializedProperty anchor = this.serializedObject.FindProperty("m_Anchor");
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(canUse));
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(anchor));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_AnimationState")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Layer")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_AnimationSpeed")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_InputDirection")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_InputX")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_InputY")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_InputZ")));
            
            SerializedProperty applyMomentum = this.serializedObject.FindProperty("m_ApplyMomentum");
            SerializedProperty momentumDuration = this.serializedObject.FindProperty("m_MomentumDuration");
            SerializedProperty momentumTransition = this.serializedObject.FindProperty("m_MomentumTransition");
            
            PropertyField fieldApplyMomentum = new PropertyField(applyMomentum);
            PropertyField fieldMomentumDuration = new PropertyField(momentumDuration, "Duration");
            PropertyField fieldMomentumTransition = new PropertyField(momentumTransition, "Transition");
            
            root.Add(new SpaceSmall());
            root.Add(fieldApplyMomentum);
            root.Add(fieldMomentumDuration);
            root.Add(fieldMomentumTransition);
            
            fieldMomentumDuration.style.display = applyMomentum.boolValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldMomentumTransition.style.display = applyMomentum.boolValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldApplyMomentum.RegisterValueChangeCallback(changeEvent =>
            {
                fieldMomentumDuration.style.display = changeEvent.changedProperty.boolValue
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                
                fieldMomentumTransition.style.display = changeEvent.changedProperty.boolValue
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            SerializedProperty gravity = this.serializedObject.FindProperty("m_Gravity");
            SerializedProperty transitionIn = this.serializedObject.FindProperty("m_TransitionIn");
            SerializedProperty transitionOut = this.serializedObject.FindProperty("m_TransitionOut");

            root.Add(new SpaceSmall());
            root.Add(new PropertyField(gravity));
            root.Add(new PropertyField(transitionIn));
            root.Add(new PropertyField(transitionOut));
            
            SerializedProperty transitionEase = this.serializedObject.FindProperty("m_TransitionEase");
            SerializedProperty enterAnimations = this.serializedObject.FindProperty("m_EnterAnimations");
            SerializedProperty exitAnimations = this.serializedObject.FindProperty("m_ExitAnimations");
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(transitionEase));
            root.Add(new PropertyField(enterAnimations));
            root.Add(new PropertyField(exitAnimations));
            
            SerializedProperty onStart = this.serializedObject.FindProperty("m_OnStart");
            SerializedProperty onFinish = this.serializedObject.FindProperty("m_OnFinish");

            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("On Start:"));
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(onStart));
            root.Add(new SpaceSmall());
            root.Add(new LabelTitle("On Finish:"));
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(onFinish));
            
            return root;
        }
    }
}