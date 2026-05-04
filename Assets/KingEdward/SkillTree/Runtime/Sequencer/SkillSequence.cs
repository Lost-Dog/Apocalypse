using System;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Timeline-based sequencer for skill execution
    /// Contains multiple tracks with clips for phases, instructions, and events
    /// </summary>
    [Serializable]
    public class SkillSequence
    {
        // Game Creator's SequenceTool expects m_Tracks array
        [SerializeField] private ISkillTrack[] m_Tracks = new ISkillTrack[2];
        [SerializeField] private float m_Duration = 1f;
        
        // Serialized track storage (Unity can't serialize interfaces directly)
        [SerializeField] private TrackPhases m_TrackPhases;
        [SerializeField] private TrackInstructions m_TrackInstructions;
        
        public float Duration
        {
            get => m_Duration;
            set => m_Duration = Mathf.Max(0.1f, value);
        }
        
        public SkillSequence()
        {
            m_TrackPhases = new TrackPhases();
            m_TrackInstructions = new TrackInstructions();
            
            m_Tracks = new ISkillTrack[2];
            m_Tracks[0] = m_TrackPhases;
            m_Tracks[1] = m_TrackInstructions;
        }
        
        public T GetTrack<T>() where T : class, ISkillTrack
        {
            foreach (var track in m_Tracks)
            {
                if (track is T typedTrack)
                    return typedTrack;
            }
            return null;
        }
        
        public ISkillTrack[] GetAllTracks()
        {
            return m_Tracks;
        }
        
        /// <summary>
        /// Get the phase speed multiplier at a specific time
        /// </summary>
        public float GetSpeedAtTime(float normalizedTime)
        {
            TrackPhases phases = GetTrack<TrackPhases>();
            if (phases == null) return 1f;
            
            return phases.GetSpeedAtTime(normalizedTime);
        }
        
        /// <summary>
        /// Execute all clips at the current time
        /// </summary>
        public void Execute(float normalizedTime, Args args)
        {
            foreach (var track in m_Tracks)
            {
                track?.Execute(normalizedTime, args);
            }
        }
        
        /// <summary>
        /// Reset all tracks to initial state
        /// </summary>
        public void Reset()
        {
            foreach (var track in m_Tracks)
            {
                track?.Reset();
            }
        }
    }
}
