using System;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Track that controls animation speed through different phases
    /// Anticipation -> Strike -> Recovery
    /// </summary>
    [Serializable]
    public class TrackPhases : ISkillTrack
    {
        [SerializeField] private List<ClipPhases> m_Clips = new List<ClipPhases>();
        
        public string TrackName => "Phases";
        public float TrackHeight => 60f;
        
        public List<ClipPhases> Clips => m_Clips;
        
        public TrackPhases()
        {
            // Create default phase clip
            m_Clips.Add(new ClipPhases
            {
                TimeStart = 0f,
                TimeEnd = 1f,
                AnticipationEnd = 0.3f,
                StrikeEnd = 0.7f,
                AnticipationSpeed = 1f,
                StrikeSpeed = 1f,
                RecoverySpeed = 1f
            });
        }
        
        public float GetSpeedAtTime(float normalizedTime)
        {
            foreach (var clip in m_Clips)
            {
                if (clip.IsInRange(normalizedTime))
                {
                    return clip.GetSpeedAtTime(normalizedTime);
                }
            }
            return 1f;
        }
        
        public void Execute(float normalizedTime, Args args)
        {
            foreach (var clip in m_Clips)
            {
                if (clip.IsInRange(normalizedTime))
                {
                    clip.Execute(normalizedTime, args);
                }
            }
        }
        
        public void Reset()
        {
            foreach (var clip in m_Clips)
            {
                clip.Reset();
            }
        }
        
        public void AddClip(ClipPhases clip)
        {
            m_Clips.Add(clip);
        }
        
        public void RemoveClip(ClipPhases clip)
        {
            m_Clips.Remove(clip);
        }
    }
}
