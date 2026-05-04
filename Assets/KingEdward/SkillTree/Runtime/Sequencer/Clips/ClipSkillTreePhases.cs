using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Serializable]
    public class ClipSkillTreePhases : Clip
    {
        [SerializeField] [HideInInspector] private float m_CastEnd = 0.33f;
        [SerializeField] [HideInInspector] private float m_ReleaseEnd = 0.66f;
        
        [SerializeField] private PropertyGetDecimal m_CastSpeed = GetDecimalConstantOne.Create;
        [SerializeField] private PropertyGetDecimal m_ReleaseSpeed = GetDecimalConstantOne.Create;
        [SerializeField] private PropertyGetDecimal m_RecoverySpeed = GetDecimalConstantOne.Create;
        
        public float CastEnd => m_CastEnd;
        public float ReleaseEnd => m_ReleaseEnd;
        
        public float GetCastSpeed(Args args) => (float)m_CastSpeed.Get(args);
        public float GetReleaseSpeed(Args args) => (float)m_ReleaseSpeed.Get(args);
        public float GetRecoverySpeed(Args args) => (float)m_RecoverySpeed.Get(args);
        
        public ClipSkillTreePhases() : base(0f, 1f)
        { }
        
        public float GetSpeedAtTime(float normalizedTime, Args args)
        {
            if (normalizedTime <= m_CastEnd)
                return GetCastSpeed(args);
            else if (normalizedTime <= m_ReleaseEnd)
                return GetReleaseSpeed(args);
            else
                return GetRecoverySpeed(args);
        }
    }
}
