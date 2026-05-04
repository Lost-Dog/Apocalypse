using System;
using UnityEngine;
using UnityEngine.Events;
using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Clip that triggers a UnityEvent at a specific time
    /// </summary>
    [Serializable]
    public class ClipEvent : ISkillClip
    {
        [SerializeField] private string m_EventName = "Event";
        [SerializeField] private float m_Time = 0.5f;
        [SerializeField] private UnityEvent m_Event = new UnityEvent();
        
        private bool m_HasTriggered = false;
        
        public float TimeStart
        {
            get => m_Time;
            set => m_Time = Mathf.Clamp01(value);
        }
        
        public float TimeEnd
        {
            get => m_Time;
            set => m_Time = Mathf.Clamp01(value);
        }
        
        public string ClipName
        {
            get => m_EventName;
            set => m_EventName = value;
        }
        
        public UnityEvent Event => m_Event;
        
        public bool IsInRange(float normalizedTime)
        {
            return normalizedTime >= m_Time;
        }
        
        public void Update(float normalizedTime, Args args)
        {
            if (normalizedTime >= m_Time && !m_HasTriggered)
            {
                Execute(normalizedTime, args);
                m_HasTriggered = true;
            }
        }
        
        public void Execute(float normalizedTime, Args args)
        {
            m_Event?.Invoke();
        }
        
        public void OnEnter(Args args)
        {
            Execute(0f, args);
        }
        
        public void OnExit(Args args) { }
        
        public void Reset()
        {
            m_HasTriggered = false;
        }
    }
}
