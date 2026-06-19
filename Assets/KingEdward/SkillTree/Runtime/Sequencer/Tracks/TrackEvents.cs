using System;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Track that triggers events at specific keyframes
    /// </summary>
    [Serializable]
    public class TrackEvents : ISkillTrack
    {
        [SerializeField] private List<ClipEvent> m_Clips = new List<ClipEvent>();
        
        public string TrackName => "Events";
        public float TrackHeight => 30f;
        
        public List<ClipEvent> Clips => m_Clips;
        
        public void Execute(float normalizedTime, Args args)
        {
            foreach (var clip in m_Clips)
            {
                clip.Update(normalizedTime, args);
            }
        }
        
        public void Reset()
        {
            foreach (var clip in m_Clips)
            {
                clip.Reset();
            }
        }
        
        public void AddClip(ClipEvent clip)
        {
            m_Clips.Add(clip);
        }
        
        public void RemoveClip(ClipEvent clip)
        {
            m_Clips.Remove(clip);
        }
    }
}
