using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Clip that defines three phases with individual speed multipliers
    /// </summary>
    [Serializable]
    public class ClipPhases : ISkillClip
    {
        [SerializeField] private float m_TimeStart = 0f;
        [SerializeField] private float m_TimeEnd = 1f;
        
        [SerializeField] [Range(0f, 1f)] private float m_AnticipationEnd = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float m_StrikeEnd = 0.7f;
        
        [SerializeField] [Range(0.1f, 5f)] private float m_AnticipationSpeed = 1f;
        [SerializeField] [Range(0.1f, 5f)] private float m_StrikeSpeed = 1f;
        [SerializeField] [Range(0.1f, 5f)] private float m_RecoverySpeed = 1f;
        
        public float TimeStart
        {
            get => m_TimeStart;
            set => m_TimeStart = Mathf.Clamp01(value);
        }
        
        public float TimeEnd
        {
            get => m_TimeEnd;
            set => m_TimeEnd = Mathf.Clamp01(value);
        }
        
        public float AnticipationEnd
        {
            get => m_AnticipationEnd;
            set => m_AnticipationEnd = Mathf.Clamp01(value);
        }
        
        public float StrikeEnd
        {
            get => m_StrikeEnd;
            set => m_StrikeEnd = Mathf.Clamp01(value);
        }
        
        public float AnticipationSpeed
        {
            get => m_AnticipationSpeed;
            set => m_AnticipationSpeed = Mathf.Clamp(value, 0.1f, 5f);
        }
        
        public float StrikeSpeed
        {
            get => m_StrikeSpeed;
            set => m_StrikeSpeed = Mathf.Clamp(value, 0.1f, 5f);
        }
        
        public float RecoverySpeed
        {
            get => m_RecoverySpeed;
            set => m_RecoverySpeed = Mathf.Clamp(value, 0.1f, 5f);
        }
        
        public string ClipName => "Phases";
        
        public bool IsInRange(float normalizedTime)
        {
            return normalizedTime >= m_TimeStart && normalizedTime <= m_TimeEnd;
        }
        
        public float GetSpeedAtTime(float normalizedTime)
        {
            if (!IsInRange(normalizedTime)) return 1f;
            
            // Normalize time within clip range
            float clipTime = (normalizedTime - m_TimeStart) / (m_TimeEnd - m_TimeStart);
            
            if (clipTime <= m_AnticipationEnd)
            {
                return m_AnticipationSpeed;
            }
            else if (clipTime <= m_StrikeEnd)
            {
                return m_StrikeSpeed;
            }
            else
            {
                return m_RecoverySpeed;
            }
        }
        
        public void Execute(float normalizedTime, Args args)
        {
            // Phases don't execute actions, they just modify speed
        }
        
        public void OnEnter(Args args) { }
        public void OnExit(Args args) { }
        public void Reset() { }
    }
}
