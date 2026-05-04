using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomEditor(typeof(MotionLink))]
    public class MotionLinkEditor : UnityEditor.Editor
    {
        private const string ERR_ANIM = "An Traverse Clip requires an Animation Clip";
        private const string ERR_STATE = "An Traverse Clip requires an Animation State";

        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Root;
        
        private Button m_ButtonToggleStage;
        
        private Button m_ButtonCharacter;
        private ObjectField m_CharacterField;

        private TraverseSequenceTool m_SequenceTool;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            TraverseConfigurationStage.EventOpenStage -= this.RefreshTraverseState;
            TraverseConfigurationStage.EventOpenStage += this.RefreshTraverseState;
            
            TraverseConfigurationStage.EventCloseStage -= this.RefreshTraverseState;
            TraverseConfigurationStage.EventCloseStage += this.RefreshTraverseState;
        }

        private void OnDisable()
        {
            TraverseConfigurationStage.EventOpenStage -= this.RefreshTraverseState;
            TraverseConfigurationStage.EventCloseStage -= this.RefreshTraverseState;
        }

        [OnOpenAsset]
        public static bool OpenAnimationExecute(EntityId entityId, int line)
        {
            MotionLink motionLink = EditorUtility.EntityIdToObject(entityId) as MotionLink;
            if (motionLink == null) return false;

            if (motionLink.AnimationMode != MotionLink.Mode.AnimationClip)
            {
                return false;
            }

            if (TraverseConfigurationStage.InStage) StageUtility.GoToMainStage();
            Selection.activeObject = motionLink;
            
            string traverseClipPath = AssetDatabase.GetAssetPath(motionLink);
            TraverseConfigurationStage.EnterStage(traverseClipPath);
            
            return true;
        }

        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            this.m_Root = new VisualElement();
            
            StyleSheet[] styleSheets = StyleSheetUtils.Load();
            foreach (StyleSheet styleSheet in styleSheets) this.m_Root.styleSheets.Add(styleSheet);

            SerializedProperty canUse = this.serializedObject.FindProperty("m_CanUse");
            SerializedProperty anchor = this.serializedObject.FindProperty("m_Anchor");
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(canUse));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(anchor));
            
            SerializedProperty mode = this.serializedObject.FindProperty("m_Mode");
            PropertyField fieldMode = new PropertyField(mode);

            ContentBox modeAnimationClip = new ContentBox("Animation Clip", true);
            ContentBox modeAnimationState = new ContentBox("State", true);
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(fieldMode);
            this.m_Root.Add(new SpaceSmallest());
            this.m_Root.Add(modeAnimationClip);
            this.m_Root.Add(modeAnimationState);
            
            SerializedProperty animationClip = this.serializedObject.FindProperty("m_AnimationClip");
            SerializedProperty mask = this.serializedObject.FindProperty("m_Mask");
            
            ErrorMessage animationClipError = new ErrorMessage(ERR_ANIM);
            PropertyField fieldAnimationClip = new PropertyField(animationClip);
            PropertyField fieldMask = new PropertyField(mask);
            
            modeAnimationClip.Content.Add(animationClipError);
            modeAnimationClip.Content.Add(fieldAnimationClip);
            modeAnimationClip.Content.Add(fieldMask);
            
            SerializedProperty animationState = this.serializedObject.FindProperty("m_AnimationState");
            SerializedProperty layer = this.serializedObject.FindProperty("m_Layer");
            SerializedProperty transitionTime = this.serializedObject.FindProperty("m_TransitionTime");
            SerializedProperty transitionEase = this.serializedObject.FindProperty("m_TransitionEase");
            SerializedProperty movementSpeed = this.serializedObject.FindProperty("m_MovementSpeed");
            SerializedProperty movementEase = this.serializedObject.FindProperty("m_MovementEase");
            SerializedProperty lift = this.serializedObject.FindProperty("m_Lift");
            SerializedProperty liftEase = this.serializedObject.FindProperty("m_LiftEase");
            
            PropertyField fieldAnimationState = new PropertyField(animationState);
            ErrorMessage animationStateError = new ErrorMessage(ERR_STATE);
            modeAnimationState.Content.Add(animationStateError);
            modeAnimationState.Content.Add(fieldAnimationState);
            modeAnimationState.Content.Add(new PropertyField(layer));
            modeAnimationState.Content.Add(new SpaceSmaller());
            modeAnimationState.Content.Add(new PropertyField(transitionTime));
            modeAnimationState.Content.Add(new PropertyField(transitionEase));
            modeAnimationState.Content.Add(new SpaceSmaller());
            modeAnimationState.Content.Add(new PropertyField(movementSpeed));
            modeAnimationState.Content.Add(new PropertyField(movementEase));
            modeAnimationState.Content.Add(new SpaceSmaller());
            modeAnimationState.Content.Add(new PropertyField(lift));
            modeAnimationState.Content.Add(new PropertyField(liftEase));
            modeAnimationState.Content.Add(new SpaceSmaller());
            
            SerializedProperty gravity = this.serializedObject.FindProperty("m_Gravity");
            SerializedProperty transitionIn = this.serializedObject.FindProperty("m_TransitionIn");
            SerializedProperty transitionOut = this.serializedObject.FindProperty("m_TransitionOut");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(gravity));
            this.m_Root.Add(new PropertyField(transitionIn));
            this.m_Root.Add(new PropertyField(transitionOut));
            
            SerializedProperty applyMomentum = this.serializedObject.FindProperty("m_ApplyMomentum");
            SerializedProperty momentumDuration = this.serializedObject.FindProperty("m_MomentumDuration");
            SerializedProperty momentumTransition = this.serializedObject.FindProperty("m_MomentumTransition");
            
            PropertyField fieldApplyMomentum = new PropertyField(applyMomentum);
            PropertyField fieldMomentumDuration = new PropertyField(momentumDuration, "Duration");
            PropertyField fieldMomentumTransition = new PropertyField(momentumTransition, "Transition");
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(fieldApplyMomentum);
            this.m_Root.Add(fieldMomentumDuration);
            this.m_Root.Add(fieldMomentumTransition);
            
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

            VisualElement boxSequencer = new VisualElement();
            this.m_Root.Add(boxSequencer);
            
            this.m_ButtonToggleStage = new Button(this.ToggleTraverseMode)
            {
                style = { height = new Length(30f, LengthUnit.Pixel)}
            };

            boxSequencer.Add(new SpaceSmall());
            boxSequencer.Add(this.m_ButtonToggleStage);
            
            PadBox sequenceContent = new PadBox();
            
            boxSequencer.Add(new SpaceSmall());
            boxSequencer.Add(sequenceContent);
            
            this.m_CharacterField = new ObjectField(string.Empty)
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                }
            };

            this.m_ButtonCharacter = new Button(this.ChangeCharacter)
            {
                text = "Change Character",
                style =
                {
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                }
            };

            HorizontalBox changeCharacterContent = new HorizontalBox(
                HorizontalBox.FlexMode.FirstGrows,
                this.m_CharacterField,
                this.m_ButtonCharacter
            );
            
            sequenceContent.Add(changeCharacterContent);

            SerializedProperty animationSequence = this.serializedObject
                .FindProperty("m_AnimationSequence")
                .FindPropertyRelative(RunTraverseSequenceDrawer.NAME_SEQUENCE);

            this.m_SequenceTool = new TraverseSequenceTool(animationSequence)
            {
                AnimationClip = animationClip.objectReferenceValue as AnimationClip
            };

            sequenceContent.Add(new SpaceSmall());
            sequenceContent.Add(this.m_SequenceTool);
            
            fieldAnimationClip.RegisterValueChangeCallback(changeEvent =>
            {
                Object newValue = changeEvent.changedProperty.objectReferenceValue;
                animationClipError.style.display = newValue == null
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                
                this.m_SequenceTool.AnimationClip = newValue as AnimationClip;
            });
            
            animationClipError.style.display = animationClip.objectReferenceValue == null
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldAnimationState.RegisterValueChangeCallback(changeEvent =>
            {
                Object newValue = changeEvent.changedProperty.objectReferenceValue;
                animationStateError.style.display = newValue == null
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            animationStateError.style.display = animationState.objectReferenceValue == null
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            fieldMode.RegisterValueChangeCallback(changeEvent =>
            {
                if (changeEvent.changedProperty.enumValueIndex == 0)
                {
                    modeAnimationClip.style.display = DisplayStyle.Flex;
                    boxSequencer.style.display = DisplayStyle.Flex;
                    modeAnimationState.style.display = DisplayStyle.None;
                }
                else
                {
                    modeAnimationClip.style.display = DisplayStyle.None;
                    boxSequencer.style.display = DisplayStyle.None;
                    modeAnimationState.style.display = DisplayStyle.Flex;
                }
            });
            
            if (mode.enumValueIndex == 0)
            {
                modeAnimationClip.style.display = DisplayStyle.Flex;
                boxSequencer.style.display = DisplayStyle.Flex;
                modeAnimationState.style.display = DisplayStyle.None;
            }
            else
            {
                modeAnimationClip.style.display = DisplayStyle.None;
                boxSequencer.style.display = DisplayStyle.None;
                modeAnimationState.style.display = DisplayStyle.Flex;
            }

            SerializedProperty animationSpeed = this.serializedObject.FindProperty("m_AnimationSpeed");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(animationSpeed));
            
            SerializedProperty onStart = this.serializedObject.FindProperty("m_OnStart");
            SerializedProperty onFinish = this.serializedObject.FindProperty("m_OnFinish");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("On Start:"));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(onStart));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("On Finish:"));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(onFinish));

            this.RefreshTraverseState();
            
            return this.m_Root;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ChangeCharacter()
        {
            GameObject character = this.m_CharacterField.value as GameObject;
            TraverseConfigurationStage.ChangeCharacter(character);
            
            if (TraverseConfigurationStage.InStage)
            {
                this.m_SequenceTool.Target = TraverseConfigurationStage.Stage.Animator != null
                    ? TraverseConfigurationStage.Stage.Animator.gameObject
                    : null;
            }
        }

        private void ToggleTraverseMode()
        {
            if (TraverseConfigurationStage.InStage)
            {
                StageUtility.GoToMainStage();
                this.RefreshTraverseState();
                
                this.m_SequenceTool.DisablePreview();
                return;
            }

            MotionLink motionLink = this.target as MotionLink;
            if (motionLink == null) return;

            string path = AssetDatabase.GetAssetPath(motionLink);
            TraverseConfigurationStage.EnterStage(path);
            
            this.m_SequenceTool.Target = TraverseConfigurationStage.Stage.Animator != null
                ? TraverseConfigurationStage.Stage.Animator.gameObject
                : null; 
        }
        
        private void RefreshTraverseState()
        {
            if (this.m_ButtonToggleStage == null) return;
            
            bool isAnimationMode = TraverseConfigurationStage.InStage;
            this.m_ButtonToggleStage.text = isAnimationMode
                ? "Close Animation Mode" 
                : "Enter Animation Mode";

            Color borderColor = isAnimationMode
                ? ColorTheme.Get(ColorTheme.Type.Green)
                : ColorTheme.Get(ColorTheme.Type.Dark);
            
            this.m_ButtonToggleStage.style.borderTopColor = borderColor;
            this.m_ButtonToggleStage.style.borderBottomColor = borderColor;
            this.m_ButtonToggleStage.style.borderLeftColor = borderColor;
            this.m_ButtonToggleStage.style.borderRightColor = borderColor;

            this.m_ButtonToggleStage.style.color = isAnimationMode
                ? ColorTheme.Get(ColorTheme.Type.Green)
                : ColorTheme.Get(ColorTheme.Type.TextNormal);

            this.m_ButtonCharacter.SetEnabled(isAnimationMode);
            this.m_CharacterField.SetEnabled(isAnimationMode);
            this.m_SequenceTool.IsEnabled = isAnimationMode;
            
            if (isAnimationMode)
            {
                this.m_CharacterField.value = TraverseConfigurationStage.CharacterReference;
                this.m_SequenceTool.Target = TraverseConfigurationStage.Stage.Animator != null
                    ? TraverseConfigurationStage.Stage.Animator.gameObject
                    : null; 
            }
        }
    }
}