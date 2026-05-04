using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Clip that executes an InstructionList when entered
    /// </summary>
    [Serializable]
    public class ClipInstructions : ISkillClip
    {
        [SerializeField] private string m_ClipName = "Instructions";
        [SerializeField] private float m_TimeStart = 0f;
        [SerializeField] private float m_TimeEnd = 0.1f;
        [SerializeField] private RunInstructionsList m_Instructions = new RunInstructionsList();
        [SerializeField] private bool m_ExecuteOnEnter = true;
        [SerializeField] private bool m_ExecuteOnExit = false;
        
        private bool m_HasEntered = false;
        private bool m_HasExited = false;
        
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
        
        public string ClipName
        {
            get => m_ClipName;
            set => m_ClipName = value;
        }
        
        public RunInstructionsList Instructions => m_Instructions;
        
        public bool ExecuteOnEnter
        {
            get => m_ExecuteOnEnter;
            set => m_ExecuteOnEnter = value;
        }
        
        public bool ExecuteOnExit
        {
            get => m_ExecuteOnExit;
            set => m_ExecuteOnExit = value;
        }
        
        public bool IsInRange(float normalizedTime)
        {
            return normalizedTime >= m_TimeStart && normalizedTime <= m_TimeEnd;
        }
        
        public void Update(float normalizedTime, Args args)
        {
            bool inRange = IsInRange(normalizedTime);
            
            // Check for enter
            if (inRange && !m_HasEntered)
            {
                OnEnter(args);
                m_HasEntered = true;
                m_HasExited = false;
            }
            // Check for exit
            else if (!inRange && m_HasEntered && !m_HasExited)
            {
                OnExit(args);
                m_HasExited = true;
            }
        }
        
        public void Execute(float normalizedTime, Args args)
        {
            if (IsInRange(normalizedTime))
            {
                _ = m_Instructions.Run(args);
            }
        }
        
        public void OnEnter(Args args)
        {
            if (m_ExecuteOnEnter)
            {
                _ = m_Instructions.Run(args);
            }
        }
        
        public void OnExit(Args args)
        {
            if (m_ExecuteOnExit)
            {
                _ = m_Instructions.Run(args);
            }
        }
        
        public void Reset()
        {
            m_HasEntered = false;
            m_HasExited = false;
        }
    }
}
