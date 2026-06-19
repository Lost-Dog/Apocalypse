using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Skill Tree Sequence - inherits from Game Creator's Sequence base
    /// </summary>
    [Serializable]
    public class SkillTreeSequence : Sequence
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private TimeMode m_TimeMode;
        [NonSerialized] private AnimationClip m_Animation;
        [NonSerialized] private Character m_Character;
        [NonSerialized] private Args m_Args;
        
        [NonSerialized] private float m_CastSpeed;
        [NonSerialized] private float m_ReleaseSpeed;
        [NonSerialized] private float m_RecoverySpeed;
        [NonSerialized] private float m_TotalDuration;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override float Duration => this.m_TotalDuration;
        public override TimeMode TimeMode => this.m_TimeMode;
        
        public TrackSkillTreePhases PhasesTrack => this.GetTrack<TrackSkillTreePhases>();
        public TrackSkillTreeClips ClipsTrack => this.GetTrack<TrackSkillTreeClips>();
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------
        
        public SkillTreeSequence() : base(new Track[]
        {
            new TrackSkillTreePhases(),
            new TrackSkillTreeClips()
        })
        { }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Cancel(Args args)
        {
            if (this.IsRunning)
            {
                this.DoCancel(args);
            }
        }
        
        public async Task Run(Args args, AnimationClip animationClip, Character character)
        {
            // Check if already running (should be handled by SkillTreeComponent now)
            if (this.IsRunning)
            {
                Debug.LogWarning($"[SkillTreeSequence] Sequence already running! This should be prevented by interrupt settings.");
                return;
            }
            
            this.m_TimeMode = character != null ? character.Time : new TimeMode(TimeMode.UpdateMode.GameTime);
            this.m_Animation = animationClip;
            this.m_Character = character;
            this.m_Args = args;
            
            TrackSkillTreePhases phasesTrack = this.PhasesTrack;
            float animLength = animationClip != null ? animationClip.length : 0f;
            
            // Pre-calculate speeds from phases track
            if (phasesTrack != null && phasesTrack.Clips.Length > 0 && phasesTrack.Clips[0] is ClipSkillTreePhases phases)
            {
                this.m_CastSpeed = phases.GetCastSpeed(args);
                this.m_ReleaseSpeed = phases.GetReleaseSpeed(args);
                this.m_RecoverySpeed = phases.GetRecoverySpeed(args);
                
                // Calculate total duration based on phase speeds
                float castEnd = phases.CastEnd;
                float releaseEnd = phases.ReleaseEnd;
                
                float castDuration = animLength * castEnd / this.m_CastSpeed;
                float releaseDuration = animLength * (releaseEnd - castEnd) / this.m_ReleaseSpeed;
                float recoveryDuration = animLength * (1f - releaseEnd) / this.m_RecoverySpeed;
                
                this.m_TotalDuration = castDuration + releaseDuration + recoveryDuration;
            }
            else
            {
                this.m_CastSpeed = 1f;
                this.m_ReleaseSpeed = 1f;
                this.m_RecoverySpeed = 1f;
                this.m_TotalDuration = animLength;
            }
            
            if (character != null && animationClip != null)
            {
                // Subscribe to phase changes to update animation speed
                this.EventBeforeUpdate += this.UpdatePhaseSpeed;
            }
            
            try
            {
                await this.DoRun(args);
            }
            finally
            {
                // Always unsubscribe, even if cancelled
                if (character != null && animationClip != null)
                {
                    this.EventBeforeUpdate -= this.UpdatePhaseSpeed;
                }
            }
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void UpdatePhaseSpeed()
        {
            if (this.m_Character == null || this.m_Animation == null) return;
            
            TrackSkillTreePhases phasesTrack = this.PhasesTrack;
            if (phasesTrack == null || phasesTrack.Clips.Length == 0) return;
            
            if (phasesTrack.Clips[0] is ClipSkillTreePhases phases)
            {
                float t = this.T;
                float castEnd = phases.CastEnd;
                float releaseEnd = phases.ReleaseEnd;
                
                // Determine current speed based on phase
                float currentSpeed = 1f;
                if (t <= castEnd)
                {
                    currentSpeed = this.m_CastSpeed;
                }
                else if (t <= releaseEnd)
                {
                    currentSpeed = this.m_ReleaseSpeed;
                }
                else
                {
                    currentSpeed = this.m_RecoverySpeed;
                }
                
                this.m_Character.Gestures.SetSpeed(this.m_Animation, currentSpeed);
            }
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override float GetDilated(float t)
        {
            TrackSkillTreePhases phasesTrack = this.PhasesTrack;
            if (phasesTrack == null || phasesTrack.Clips.Length == 0) return t;
            
            if (phasesTrack.Clips[0] is ClipSkillTreePhases phases)
            {
                // Get phase ratios
                float castEnd = phases.CastEnd;
                float releaseEnd = phases.ReleaseEnd;
                
                // Use pre-calculated speeds
                float castSpeed = this.m_CastSpeed;
                float releaseSpeed = this.m_ReleaseSpeed;
                float recoverySpeed = this.m_RecoverySpeed;
                
                // Calculate dilated ratios (how much time each phase takes in dilated time)
                float castDilatedRatio = castEnd > 0f && castSpeed > 0f ? castEnd / castSpeed : 0f;
                float releaseDuration = releaseEnd - castEnd;
                float releaseDilatedRatio = releaseDuration > 0f && releaseSpeed > 0f ? releaseDuration / releaseSpeed : 0f;
                float recoveryDuration = 1f - releaseEnd;
                float recoveryDilatedRatio = recoveryDuration > 0f && recoverySpeed > 0f ? recoveryDuration / recoverySpeed : 0f;
                
                float totalDilated = castDilatedRatio + releaseDilatedRatio + recoveryDilatedRatio;
                if (totalDilated <= 0f) return t;
                
                // Normalize dilated ratios
                castDilatedRatio /= totalDilated;
                releaseDilatedRatio /= totalDilated;
                recoveryDilatedRatio /= totalDilated;
                
                // Calculate dilated time based on current phase
                float dilated = 0f;
                
                // Cast phase
                if (t <= castEnd)
                {
                    if (castEnd > 0f)
                    {
                        float phaseProgress = t / castEnd;
                        dilated = phaseProgress * castDilatedRatio;
                    }
                }
                // Release phase
                else if (t <= releaseEnd)
                {
                    if (releaseDuration > 0f)
                    {
                        float phaseProgress = (t - castEnd) / releaseDuration;
                        dilated = castDilatedRatio + phaseProgress * releaseDilatedRatio;
                    }
                    else
                    {
                        dilated = castDilatedRatio;
                    }
                }
                // Recovery phase
                else
                {
                    if (recoveryDuration > 0f)
                    {
                        float phaseProgress = (t - releaseEnd) / recoveryDuration;
                        dilated = castDilatedRatio + releaseDilatedRatio + phaseProgress * recoveryDilatedRatio;
                    }
                    else
                    {
                        dilated = castDilatedRatio + releaseDilatedRatio;
                    }
                }
                
                return dilated;
            }
            
            return t;
        }
    }
}
