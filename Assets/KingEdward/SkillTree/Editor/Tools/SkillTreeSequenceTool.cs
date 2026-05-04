using GameCreator.Editor.Common;
using GameCreator.Editor.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KingEdward.SkillTree.Editor
{
    public class SkillTreeSequenceTool : SequenceTool
    {
        // MEMBERS: -------------------------------------------------------------------------------

        private GameObject m_Target;
        private AnimationClip m_AnimationClip;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override bool ShowMetric0 => false;
        public override bool ShowMetric1 => true;

        public override bool RoundTimelineHead => true;

        public AnimationClip AnimationClip
        {
            get => m_AnimationClip;
            set
            {
                m_AnimationClip = value;
                RefreshPreview();
                PlaybackTool.MaxFrame = GetFrames();
            }
        }

        public GameObject Target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                RefreshPreview();
            }
        }

        // CONSTRUCTOR: ---------------------------------------------------------------------------
        
        public SkillTreeSequenceTool(SerializedProperty property) : base(property)
        {
            this.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                try
                {
                    if (AnimationMode.InAnimationMode())
                    {
                        AnimationMode.StopAnimationMode();
                    }
                }
                catch (System.Exception)
                {
                    // Ignore errors when SerializedObject is disposed
                }
            });
            
            this.PlaybackTool.EventChange += () =>
            {
                if (!AnimationMode.InAnimationMode())
                {
                    AnimationMode.StartAnimationMode();
                }
                
                this.RefreshPreview();
            };

            this.PlaybackTool.MaxFrame = this.GetFrames();
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DisablePreview()
        {
            if (!AnimationMode.InAnimationMode()) return;
            AnimationMode.StopAnimationMode();
        }
        
        // OVERRIDE METHODS: ----------------------------------------------------------------------

        protected override TrackTool CreateTrackTool(int trackIndex)
        {
            SerializedProperty track = this.Tracks.GetArrayElementAtIndex(trackIndex);
            if (track == null) return null;

            if (track.managedReferenceValue.GetType() == typeof(TrackSkillTreePhases))
            {
                return new TrackToolSkillTreePhases(this, trackIndex);
            }
            
            if (track.managedReferenceValue.GetType() == typeof(TrackSkillTreeClips))
            {
                return new TrackToolSkillTreeClips(this, trackIndex);
            }

            return new TrackToolDefault(this, trackIndex);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RefreshPreview()
        {
            if (EditorApplication.isPlaying) return;
            if (!AnimationMode.InAnimationMode()) return;
            
            if (this.m_AnimationClip == null) return;
            if (this.m_Target == null) return;
            
            Animator animator = this.m_Target.GetComponentInChildren<Animator>();
            if (animator == null) return;
            
            AnimationMode.BeginSampling();
            
            AnimationMode.SampleAnimationClip(
                animator.gameObject,
                this.m_AnimationClip,
                this.PlaybackTool.Value * this.m_AnimationClip.length
            );

            AnimationMode.EndSampling();
        }

        private int GetFrames()
        {
            return this.m_AnimationClip != null
                ? Mathf.FloorToInt(this.m_AnimationClip.length * 30)
                : 0;
        }
    }
}
