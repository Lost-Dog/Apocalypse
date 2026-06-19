using System;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Track that executes InstructionLists at specific times
    /// </summary>
    [Serializable]
    public class TrackInstructions : ISkillTrack
    {
        [SerializeField] private List<ClipInstructions> m_Clips = new List<ClipInstructions>();
        
        public string TrackName => "Instructions";
        public float TrackHeight => 40f;
        
        public List<ClipInstructions> Clips => m_Clips;
        
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
        
        public void AddClip(ClipInstructions clip)
        {
            m_Clips.Add(clip);
        }
        
        public void RemoveClip(ClipInstructions clip)
        {
            m_Clips.Remove(clip);
        }
    }
}
