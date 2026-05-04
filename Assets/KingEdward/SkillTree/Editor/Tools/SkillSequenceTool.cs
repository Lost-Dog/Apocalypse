using GameCreator.Editor.Common;
using GameCreator.Editor.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace KingEdward.SkillTree.Editor
{
    /// <summary>
    /// Skill Sequence Tool - inherits from Game Creator's SequenceTool
    /// This gives us the full timeline editor for FREE!
    /// </summary>
    public class SkillSequenceTool : SequenceTool
    {
        private AnimationClip m_AnimationClip;
        private GameObject m_Target;
        private SerializedProperty m_Property;
        
        public SkillSequenceTool(SerializedProperty property) : base(property)
        {
            m_Property = property;
            
            this.RegisterCallback<UnityEngine.UIElements.DetachFromPanelEvent>(_ =>
            {
                // Clean up
                EditorApplication.update -= CheckForAnimationChange;
                
                if (AnimationMode.InAnimationMode())
                {
                    AnimationMode.StopAnimationMode();
                }
            });
            
            this.PlaybackTool.EventChange += OnPlaybackChange;
            
            // Get animation clip from skill
            UpdateAnimationClip();
            
            // Get target from preview stage
            UpdateTarget();
            
            // Register for property changes to detect animation clip changes
            EditorApplication.update += CheckForAnimationChange;
        }
        
        private void OnPlaybackChange()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }
            
            // Always update animation clip before refreshing
            UpdateAnimationClip();
            UpdateTarget();
            RefreshPreview();
        }
        
        private void UpdateTarget()
        {
            if (SkillPreviewStage.InStage && SkillPreviewStage.Stage != null)
            {
                m_Target = SkillPreviewStage.Stage.Animator?.gameObject;
            }
        }
        
        private AnimationClip m_LastAnimationClip;
        
        private void CheckForAnimationChange()
        {
            if (m_Property == null || m_Property.serializedObject == null || m_Property.serializedObject.targetObject == null) 
            {
                EditorApplication.update -= CheckForAnimationChange;
                return;
            }
            
            try
            {
                m_Property.serializedObject.Update();
                
                if (m_Property.serializedObject.targetObject is Skill skill)
                {
                    AnimationClip currentClip = skill.AnimationClip;
                    
                    if (currentClip != m_LastAnimationClip)
                    {
                        m_LastAnimationClip = currentClip;
                        m_AnimationClip = currentClip;
                        
                        // Force refresh preview
                        if (AnimationMode.InAnimationMode())
                        {
                            RefreshPreview();
                        }
                    }
                }
            }
            catch
            {
                EditorApplication.update -= CheckForAnimationChange;
            }
        }
        
        private void UpdateAnimationClip()
        {
            if (m_Property != null && m_Property.serializedObject != null && m_Property.serializedObject.targetObject is Skill skill)
            {
                m_AnimationClip = skill.AnimationClip;
                m_LastAnimationClip = m_AnimationClip;
            }
        }
        
        protected override TrackTool CreateTrackTool(int trackIndex)
        {
            SerializedProperty track = this.Tracks?.GetArrayElementAtIndex(trackIndex);
            if (track == null)
            {
                UnityEngine.Debug.LogWarning($"Track at index {trackIndex} is null. You may need to recreate this Skill asset.");
                return null;
            }
            
            // Get the track type name from managed reference
            string typeName = track.managedReferenceFullTypename;
            
            if (string.IsNullOrEmpty(typeName))
            {
                UnityEngine.Debug.LogWarning($"Track type name is empty at index {trackIndex}. The Skill may have old serialized data. Create a new Skill asset.");
                return null;
            }
            
            if (typeName.Contains(nameof(TrackSkillTreePhases)))
            {
                return new TrackToolSkillTreePhases(this, trackIndex);
            }
            else if (typeName.Contains(nameof(TrackSkillTreeClips)))
            {
                return new TrackToolSkillTreeClips(this, trackIndex);
            }
            
            return base.CreateTrackTool(trackIndex);
        }
        
        public override bool ShowMetric0 => false;
        public override bool ShowMetric1 => true;
        public override bool RoundTimelineHead => true;
        
        private void RefreshPreview()
        {
            if (EditorApplication.isPlaying) return;
            if (!AnimationMode.InAnimationMode()) return;
            
            if (m_AnimationClip == null) return;
            if (m_Target == null)
            {
                // Try to get target from preview stage
                if (SkillPreviewStage.InStage && SkillPreviewStage.Stage != null)
                {
                    m_Target = SkillPreviewStage.Stage.Animator?.gameObject;
                }
                
                if (m_Target == null) return;
            }
            
            UnityEngine.Animator animator = m_Target.GetComponent<UnityEngine.Animator>();
            if (animator == null) return;
            
            // Check if should use root motion
            bool useRootMotion = false;
            if (m_Property.serializedObject.targetObject is Skill skill)
            {
                useRootMotion = skill.UseRootMotion;
            }
            
            AnimationMode.BeginSampling();
            
            if (useRootMotion)
            {
                // Sample with root motion
                AnimationMode.SampleAnimationClip(
                    animator.gameObject,
                    m_AnimationClip,
                    this.PlaybackTool.Value * m_AnimationClip.length
                );
            }
            else
            {
                // Sample without root motion (keep position/rotation)
                Vector3 originalPos = animator.transform.position;
                Quaternion originalRot = animator.transform.rotation;
                
                AnimationMode.SampleAnimationClip(
                    animator.gameObject,
                    m_AnimationClip,
                    this.PlaybackTool.Value * m_AnimationClip.length
                );
                
                animator.transform.position = originalPos;
                animator.transform.rotation = originalRot;
            }

            AnimationMode.EndSampling();
        }
    }
}
