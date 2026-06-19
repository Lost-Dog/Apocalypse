using GameCreator.Editor.Common;
using KingEdward.SkillTree;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KingEdward.SkillTree.Editor
{
    [CustomEditor(typeof(Skill))]
    public class SkillEditor : UnityEditor.Editor
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        private SkillTreeSequenceTool m_SequenceTool;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            SkillPreviewStage.EventOpenStage -= this.RefreshStageState;
            SkillPreviewStage.EventOpenStage += this.RefreshStageState;
            
            SkillPreviewStage.EventCloseStage -= this.RefreshStageState;
            SkillPreviewStage.EventCloseStage += this.RefreshStageState;
        }

        private void OnDisable()
        {
            SkillPreviewStage.EventOpenStage -= this.RefreshStageState;
            SkillPreviewStage.EventCloseStage -= this.RefreshStageState;
        }
        
        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            // ----- Header (Unique ID) -----
            VisualElement header = new VisualElement();
            header.AddToClassList("gc-space-smaller");
            root.Add(header);
            
            SerializedProperty uniqueID = serializedObject.FindProperty("m_UniqueID");
            PropertyField uniqueIDField = new PropertyField(uniqueID, "Unique ID");
            uniqueIDField.SetEnabled(false);
            header.Add(uniqueIDField);
            root.Add(new SpaceSmall());
            
            // ----- Basic Information -----
            SerializedProperty skillName = serializedObject.FindProperty("m_SkillName");
            SerializedProperty description = serializedObject.FindProperty("m_Description");
            SerializedProperty icon = serializedObject.FindProperty("m_Icon");
            SerializedProperty cost = serializedObject.FindProperty("m_Cost");
            
            var basicInfoBox = new ContentBox("Basic Information", true);
            basicInfoBox.Content.Add(new PropertyField(skillName));
            basicInfoBox.Content.Add(new PropertyField(description));
            basicInfoBox.Content.Add(new PropertyField(icon));
            basicInfoBox.Content.Add(new PropertyField(cost));
            root.Add(basicInfoBox);
            root.Add(new SpaceSmall());

            // ----- Cooldown & Stacks -----
            SerializedProperty isActiveSkill = serializedObject.FindProperty("m_IsActiveSkill");
            SerializedProperty cooldownDuration = serializedObject.FindProperty("m_CooldownDuration");
            SerializedProperty hasStacks = serializedObject.FindProperty("m_HasStacks");
            SerializedProperty stackUsesBeforeCooldown = serializedObject.FindProperty("m_StackUsesBeforeCooldown");
            SerializedProperty useStackTime = serializedObject.FindProperty("m_UseStackTime");
            SerializedProperty stackWindowDuration = serializedObject.FindProperty("m_StackWindowDuration");
            
            var cooldownBox = new ContentBox("Cooldown & Stacks", true);
            cooldownBox.Content.Add(new PropertyField(cooldownDuration));
            cooldownBox.Content.Add(new PropertyField(isActiveSkill));
            var hasStacksField = new PropertyField(hasStacks);
            var stackUsesField = new PropertyField(stackUsesBeforeCooldown);
            var useStackTimeField = new PropertyField(useStackTime);
            var stackWindowDurationField = new PropertyField(stackWindowDuration);
            cooldownBox.Content.Add(hasStacksField);
            cooldownBox.Content.Add(stackUsesField);
            cooldownBox.Content.Add(useStackTimeField);
            cooldownBox.Content.Add(stackWindowDurationField);

            void UpdateStackVisibility()
            {
                bool enabled = hasStacks.boolValue;
                stackUsesField.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
                useStackTimeField.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
                stackWindowDurationField.style.display = enabled && useStackTime.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateStackVisibility();
            hasStacksField.RegisterValueChangeCallback(_ => UpdateStackVisibility());
            useStackTimeField.RegisterValueChangeCallback(_ => UpdateStackVisibility());
            
            root.Add(cooldownBox);
            root.Add(new SpaceSmall());
            
            // ----- Level System -----
            SerializedProperty maxLevel = serializedObject.FindProperty("m_MaxLevel");
            SerializedProperty canLevelUp = serializedObject.FindProperty("m_CanLevelUp");
            
            var levelBox = new ContentBox("Level System", false);
            var canLevelUpField = new PropertyField(canLevelUp);
            var maxLevelField = new PropertyField(maxLevel);
            levelBox.Content.Add(canLevelUpField);
            levelBox.Content.Add(maxLevelField);
            void UpdateMaxLevelVisibility()
            {
                maxLevelField.style.display = canLevelUp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateMaxLevelVisibility();
            canLevelUpField.RegisterValueChangeCallback(_ => UpdateMaxLevelVisibility());
            
            root.Add(levelBox);
            root.Add(new SpaceSmall());
            
            // ----- Prerequisites -----
            SerializedProperty prerequisites = serializedObject.FindProperty("m_Prerequisites");
            var prereqBox = new ContentBox("Prerequisites", false);
            prereqBox.Content.Add(new PropertyField(prerequisites));
            root.Add(prereqBox);
            root.Add(new SpaceSmall());
            
            // ----- Events -----
            SerializedProperty onUnlock = serializedObject.FindProperty("m_OnUnlock");
            SerializedProperty onUse = serializedObject.FindProperty("m_OnUse");
            SerializedProperty onLevelUp = serializedObject.FindProperty("m_OnLevelUp");
            SerializedProperty executeBeforeChange = serializedObject.FindProperty("m_ExecuteBeforeChange");
            SerializedProperty reapplyOnUnlockOnLoad = serializedObject.FindProperty("m_ReapplyOnUnlockOnLoad");
            
            var eventsBox = new ContentBox("Events", true);
            eventsBox.Content.Add(new LabelTitle("On Unlock"));
            eventsBox.Content.Add(new PropertyField(onUnlock));
            eventsBox.Content.Add(new PropertyField(reapplyOnUnlockOnLoad));
            eventsBox.Content.Add(new SpaceSmall());
            eventsBox.Content.Add(new LabelTitle("On Use"));
            eventsBox.Content.Add(new PropertyField(onUse));
            eventsBox.Content.Add(new SpaceSmall());
            eventsBox.Content.Add(new LabelTitle("On Level Up"));
            eventsBox.Content.Add(new PropertyField(onLevelUp));
            eventsBox.Content.Add(new PropertyField(executeBeforeChange));
            root.Add(eventsBox);
            root.Add(new SpaceSmall());
            
            // ----- Sequencer -----
            SerializedProperty useSequencer = serializedObject.FindProperty("m_UseSequencer");
            SerializedProperty animationClip = serializedObject.FindProperty("m_AnimationClip");
            SerializedProperty avatarMask = serializedObject.FindProperty("m_AvatarMask");
            SerializedProperty useRootMotion = serializedObject.FindProperty("m_UseRootMotion");
            SerializedProperty sequencer = serializedObject.FindProperty("m_Sequencer");
            SerializedProperty canInterruptOthers = serializedObject.FindProperty("m_CanInterruptOthers");
            SerializedProperty canBeInterrupted = serializedObject.FindProperty("m_CanBeInterrupted");
            
            var sequencerBox = new ContentBox("Sequencer", false);
            var useSequencerField = new PropertyField(useSequencer);
            var animationClipField = new PropertyField(animationClip);
            var avatarMaskField = new PropertyField(avatarMask);
            var useRootMotionField = new PropertyField(useRootMotion);
            var interruptOthersField = new PropertyField(canInterruptOthers);
            var interruptedField = new PropertyField(canBeInterrupted);
            
            sequencerBox.Content.Add(useSequencerField);
            sequencerBox.Content.Add(animationClipField);
            sequencerBox.Content.Add(avatarMaskField);
            sequencerBox.Content.Add(useRootMotionField);
            sequencerBox.Content.Add(new SpaceSmall());
            var interruptLabel = new LabelTitle("Interrupt Behavior");
            sequencerBox.Content.Add(interruptLabel);
            sequencerBox.Content.Add(interruptOthersField);
            sequencerBox.Content.Add(interruptedField);
            sequencerBox.Content.Add(new SpaceSmall());
            
            this.m_SequenceTool = new SkillTreeSequenceTool(sequencer)
            {
                AnimationClip = animationClip.objectReferenceValue as AnimationClip
            };
            sequencerBox.Content.Add(this.m_SequenceTool);
            
            var previewButton = new Button(() =>
            {
                if (SkillPreviewStage.InStage)
                {
                    StageUtility.GoToMainStage();
                    this.m_SequenceTool.DisablePreview();
                }
                else
                {
                    SkillPreviewStage.OpenStage(target as Skill);
                    if (SkillPreviewStage.InStage && SkillPreviewStage.Stage.Animator != null)
                        this.m_SequenceTool.Target = SkillPreviewStage.Stage.Animator.gameObject;
                }
            })
            {
                text = "Open Preview Scene",
                style = { height = 25, marginTop = 5 }
            };
            sequencerBox.Content.Add(previewButton);
            
            animationClipField.RegisterValueChangeCallback(evt =>
            {
                this.m_SequenceTool.AnimationClip = evt.changedProperty.objectReferenceValue as AnimationClip;
            });
            
            void UpdateSequencerVisibility()
            {
                bool show = useSequencer.boolValue;
                animationClipField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                avatarMaskField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                useRootMotionField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                this.m_SequenceTool.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                previewButton.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                interruptLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                interruptOthersField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                interruptedField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateSequencerVisibility();
            useSequencerField.RegisterValueChangeCallback(_ => UpdateSequencerVisibility());
            
            root.Add(sequencerBox);
            root.Add(new SpaceSmall());
            
            // ----- Charge, Channel & Indicator -----
            SerializedProperty useChargeStateWithIndicator = serializedObject.FindProperty("m_UseChargeStateWithIndicator");
            SerializedProperty chargeState = serializedObject.FindProperty("m_ChargeState");
            SerializedProperty chargeStateLayer = serializedObject.FindProperty("m_ChargeStateLayer");
            SerializedProperty isChannelSkill = serializedObject.FindProperty("m_IsChannelSkill");
            SerializedProperty channelState = serializedObject.FindProperty("m_ChannelState");
            SerializedProperty channelStateLayer = serializedObject.FindProperty("m_ChannelStateLayer");
            SerializedProperty onChannelTick = serializedObject.FindProperty("m_OnChannelTick");
            SerializedProperty channelConditions = serializedObject.FindProperty("m_ChannelConditions");
            SerializedProperty indicatorConfig = serializedObject.FindProperty("m_IndicatorConfig");
            SerializedProperty channelStartMode = serializedObject.FindProperty("m_ChannelStartMode");
            SerializedProperty channelStartCustomNormalizedTime = serializedObject.FindProperty("m_ChannelStartCustomNormalizedTime");
            SerializedProperty stopGestureOnChannelStart = serializedObject.FindProperty("m_StopGestureOnChannelStart");
            SerializedProperty stopGestureDelay = serializedObject.FindProperty("m_StopGestureDelay");
            SerializedProperty stopGestureTransition = serializedObject.FindProperty("m_StopGestureTransition");

            // Charge & Indicator box
            var chargeBox = new ContentBox("Charge & Indicator", false);
            chargeBox.Content.Add(new LabelTitle("Charge"));
            var useChargeField = new PropertyField(useChargeStateWithIndicator);
            var chargeStateField = new PropertyField(chargeState);
            var chargeLayerField = new PropertyField(chargeStateLayer);
            chargeBox.Content.Add(useChargeField);
            chargeBox.Content.Add(chargeStateField);
            chargeBox.Content.Add(chargeLayerField);
            chargeBox.Content.Add(new SpaceSmall());
            chargeBox.Content.Add(new LabelTitle("Skill Indicator"));
            chargeBox.Content.Add(new PropertyField(indicatorConfig));

            // Channeling box
            var channelBox = new ContentBox("Channeling", false);
            var isChannelSkillField = new PropertyField(isChannelSkill);
            var channelStateField = new PropertyField(channelState);
            var channelLayerField = new PropertyField(channelStateLayer);
            var onChannelTickField = new PropertyField(onChannelTick);
            var channelConditionsField = new PropertyField(channelConditions);
            channelBox.Content.Add(isChannelSkillField);
            channelBox.Content.Add(channelStateField);
            channelBox.Content.Add(channelLayerField);
            
            if (channelStartMode != null)
            {
                channelBox.Content.Add(new SpaceSmall());
                channelBox.Content.Add(new LabelTitle("Channel Start Mode (with Sequencer)"));
                var channelStartModeField = new PropertyField(channelStartMode);
                channelBox.Content.Add(channelStartModeField);

                if (channelStartCustomNormalizedTime != null)
                {
                    var customTimeField = new PropertyField(channelStartCustomNormalizedTime);
                    channelBox.Content.Add(customTimeField);

                    void UpdateCustomTimeVisibility()
                    {
                        serializedObject.Update();
                        bool showCustom = channelStartMode.enumValueIndex == (int)Skill.ChannelStartMode.CustomNormalizedTime;
                        customTimeField.style.display = showCustom ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                    
                    UpdateCustomTimeVisibility();
                    channelStartModeField.RegisterValueChangeCallback(_ => UpdateCustomTimeVisibility());
                }

                if (stopGestureOnChannelStart != null && stopGestureDelay != null && stopGestureTransition != null)
                {
                    channelBox.Content.Add(new SpaceSmall());
                    channelBox.Content.Add(new LabelTitle("Stop Gesture on Channel Start"));

                    var stopGestureToggleField = new PropertyField(stopGestureOnChannelStart);
                    channelBox.Content.Add(stopGestureToggleField);

                    var stopGestureDelayFieldUI = new PropertyField(stopGestureDelay);
                    var stopGestureTransitionFieldUI = new PropertyField(stopGestureTransition);

                    channelBox.Content.Add(stopGestureDelayFieldUI);
                    channelBox.Content.Add(stopGestureTransitionFieldUI);

                    void UpdateStopGestureVisibility()
                    {
                        serializedObject.Update();
                        bool show = stopGestureOnChannelStart.boolValue;
                        stopGestureDelayFieldUI.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                        stopGestureTransitionFieldUI.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                    }

                    UpdateStopGestureVisibility();
                    stopGestureToggleField.RegisterValueChangeCallback(_ => UpdateStopGestureVisibility());
                }
            }

            channelBox.Content.Add(onChannelTickField);
            channelBox.Content.Add(new SpaceSmall());
            channelBox.Content.Add(channelConditionsField);

            void UpdateChargeVisibility()
            {
                bool show = useChargeStateWithIndicator.boolValue;
                chargeStateField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                chargeLayerField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }

            void UpdateChannelVisibility()
            {
                bool show = isChannelSkill.boolValue;
                channelStateField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                channelLayerField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                onChannelTickField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                channelConditionsField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateChargeVisibility();
            UpdateChannelVisibility();
            useChargeField.RegisterValueChangeCallback(_ => UpdateChargeVisibility());
            isChannelSkillField.RegisterValueChangeCallback(_ => UpdateChannelVisibility());
            
            root.Add(chargeBox);
            root.Add(new SpaceSmall());
            root.Add(channelBox);
            root.Add(new SpaceSmall());
            
            // ----- Conditions -----
            SerializedProperty canUnlock = serializedObject.FindProperty("m_CanUnlock");
            SerializedProperty canUse = serializedObject.FindProperty("m_CanUse");
            SerializedProperty canLevelUpConditions = serializedObject.FindProperty("m_CanLevelUpConditions");
            SerializedProperty specificLevelUp = serializedObject.FindProperty("m_SpecificLevelUp");
            
            var conditionsBox = new ContentBox("Conditions", true);
            conditionsBox.Content.Add(new LabelTitle("Unlock Conditions"));
            conditionsBox.Content.Add(new PropertyField(canUnlock));
            conditionsBox.Content.Add(new SpaceSmall());
            conditionsBox.Content.Add(new LabelTitle("Use Conditions"));
            conditionsBox.Content.Add(new PropertyField(canUse));
            conditionsBox.Content.Add(new SpaceSmall());
            conditionsBox.Content.Add(new LabelTitle("Level Up Conditions"));
            conditionsBox.Content.Add(new PropertyField(canLevelUpConditions));
            conditionsBox.Content.Add(new SpaceSmall());
            conditionsBox.Content.Add(new LabelTitle("Specific Level Up"));
            conditionsBox.Content.Add(new PropertyField(specificLevelUp));
            root.Add(conditionsBox);
            
            this.RefreshStageState();
            return root;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void RefreshStageState()
        {
            if (this.m_SequenceTool == null) return;
            
            bool isInStage = SkillPreviewStage.InStage;
            this.m_SequenceTool.IsEnabled = isInStage;
            
            if (isInStage)
            {
                this.m_SequenceTool.Target = SkillPreviewStage.Stage.Animator != null
                    ? SkillPreviewStage.Stage.Animator.gameObject
                    : null;
            }
        }
    }
}
