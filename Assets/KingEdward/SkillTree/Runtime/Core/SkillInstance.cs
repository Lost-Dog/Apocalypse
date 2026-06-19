using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;
using System;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Represents an instance of a skill with its current level and state
    /// </summary>
    [System.Serializable]
    public class SkillInstance
    {
        [SerializeField] private Skill m_SkillReference;
        [SerializeField] private int m_CurrentLevel = 1;
        [SerializeField] private bool m_IsUnlocked = false;
        [SerializeField] private int m_TotalPointsSpent = 0;
        
        [System.NonSerialized] private float m_CooldownRemaining = 0f;
        [System.NonSerialized] private bool m_IsOnCooldown = false;
        [System.NonSerialized] private float m_CooldownEndTime = 0f;
        [System.NonSerialized] private Coroutine m_ActiveCooldownCoroutine;
        [System.NonSerialized] private MonoBehaviour m_CoroutineRunner;
        
        public event Action<SkillInstance> OnCooldownChanged;
        
        // Public read-only properties
        public Skill skillReference => m_SkillReference;
        public int currentLevel => m_CurrentLevel;
        public bool isUnlocked => m_IsUnlocked;
        public int totalPointsSpent => m_TotalPointsSpent;
        public float cooldownRemaining => m_CooldownRemaining;
        public bool isOnCooldown => m_IsOnCooldown;
        public float cooldownEndTime => m_CooldownEndTime;
        
        [System.NonSerialized] private int m_UsesInWindow = 0;
        [System.NonSerialized] private float m_FirstUseTimeInWindow = 0f;
        
        public SkillInstance(Skill skillRef)
        {
            if (skillRef == null)
            {
                throw new System.ArgumentNullException(nameof(skillRef), "Skill reference cannot be null");
            }
            
            m_SkillReference = skillRef;
            m_CurrentLevel = 1;
            m_IsUnlocked = false;
            m_TotalPointsSpent = 0;
        }
        
        /// <summary>
        /// Reset this instance for reuse (Object Pool)
        /// </summary>
        internal void Reset(Skill skillRef)
        {
            if (skillRef == null)
            {
                throw new System.ArgumentNullException(nameof(skillRef), "Skill reference cannot be null");
            }
            
            m_SkillReference = skillRef;
            m_CurrentLevel = 1;
            m_IsUnlocked = false;
            m_TotalPointsSpent = 0;
            ResetCooldown();
        }
        
        /// <summary>
        /// Unlock this skill instance
        /// </summary>
        internal void Unlock()
        {
            m_IsUnlocked = true;
        }
        
        /// <summary>
        /// Set the level directly (internal use only)
        /// </summary>
        internal void SetLevel(int level)
        {
            m_CurrentLevel = Mathf.Clamp(level, 1, m_SkillReference.maxLevel);
        }
        
        /// <summary>
        /// Set unlocked state (internal use only)
        /// </summary>
        internal void SetUnlocked(bool unlocked)
        {
            m_IsUnlocked = unlocked;
        }
        
        /// <summary>
        /// Add points to total spent (internal use only)
        /// </summary>
        internal void AddPointsSpent(int points)
        {
            m_TotalPointsSpent += points;
        }
        
        /// <summary>
        /// Reset total points spent (internal use only)
        /// </summary>
        internal void ResetPointsSpent()
        {
            m_TotalPointsSpent = 0;
        }
        
        /// <summary>
        /// Check if this skill can level up
        /// </summary>
        public bool CanLevelUp => m_SkillReference.canLevelUp && !IsMaxLevel;
        
        /// <summary>
        /// Check if this skill is at max level
        /// </summary>
        public bool IsMaxLevel => m_CurrentLevel >= m_SkillReference.maxLevel;
        
        /// <summary>
        /// Get the cooldown duration for this skill
        /// </summary>
        public float CooldownDuration => m_SkillReference.CooldownDuration;
        
        /// <summary>
        /// Max number of uses allowed before cooldown when using stacks. Returns 0 if stacks are disabled.
        /// </summary>
        public int MaxStackUses
        {
            get
            {
                if (m_SkillReference == null || !m_SkillReference.HasStacks) return 0;
                return Mathf.Max(1, m_SkillReference.StackUsesBeforeCooldown);
            }
        }
        
        /// <summary>
        /// Remaining uses before cooldown starts in the current window. Returns 0 if stacks are disabled.
        /// </summary>
        public int RemainingStackUses
        {
            get
            {
                if (m_SkillReference == null || !m_SkillReference.HasStacks) return 0;
                int max = Mathf.Max(1, m_SkillReference.StackUsesBeforeCooldown);
                return Mathf.Clamp(max - m_UsesInWindow, 0, max);
            }
        }
        
        /// <summary>
        /// Get the cost for this skill
        /// </summary>
        public int Cost => m_SkillReference.Cost;
        
        /// <summary>
        /// Check if this skill can be used
        /// </summary>
        public bool CanUse(Args args)
        {
            return !m_IsOnCooldown && m_SkillReference.CanUse(args);
        }
        
        /// <summary>
        /// Check if this skill can be unlocked
        /// </summary>
        public bool CanUnlock(Args args)
        {
            return m_SkillReference.CanUnlock(args);
        }
        
        /// <summary>
        /// Level up this skill instance
        /// </summary>
        public bool LevelUp(Args args = null)
        {
            if (!CanLevelUp) return false;
            
            // Check conditions for the next level
            if (args != null && !CheckLevelUpConditionsForNextLevel(args))
            {
                return false;
            }
            
            int oldLevel = m_CurrentLevel;
            m_CurrentLevel++;
            
            // Execute level up instructions
            if (args != null)
            {
                // Execute general instructions
                if (m_SkillReference.executeBeforeChange)
                {
                    m_SkillReference.onLevelUp?.Run(args);
                }
                
                // Execute specific instructions
                var specificCondition = m_SkillReference.GetSpecificConditionForLevel(m_CurrentLevel);
                if (specificCondition != null)
                {
                    if (m_SkillReference.executeBeforeChange)
                    {
                        specificCondition.onLevelUp?.Run(args);
                    }
                }
                
                // If not executing before, execute after level change
                if (!m_SkillReference.executeBeforeChange)
                {
                    m_SkillReference.onLevelUp?.Run(args);
                    
                    if (specificCondition != null)
                    {
                        specificCondition.onLevelUp?.Run(args);
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Check level up conditions for the next level using instance data
        /// </summary>
        private bool CheckLevelUpConditionsForNextLevel(Args args)
        {
            return m_SkillReference.CheckLevelUpConditions(m_CurrentLevel, args);
        }
        
        /// <summary>
        /// Start cooldown for this skill
        /// </summary>
        public void StartCooldown(MonoBehaviour runner)
        {
            float duration = CooldownDuration;
            if (duration <= 0f) return;
            
            // Stop existing cooldown if any
            if (m_IsOnCooldown && m_CoroutineRunner != null && m_ActiveCooldownCoroutine != null)
            {
                try
                {
                    m_CoroutineRunner.StopCoroutine(m_ActiveCooldownCoroutine);
                    m_ActiveCooldownCoroutine = null;
                }
                catch (System.Exception)
                {
                    // Ignore errors from stopping coroutines
                }
            }
            
            m_CooldownEndTime = Time.time + duration;
            m_IsOnCooldown = true;
            m_CooldownRemaining = duration;
            
            OnCooldownChanged?.Invoke(this);
            
            bool runnerIsUsable = (runner != null && runner.gameObject != null && runner.gameObject.activeInHierarchy);
            
            if (runnerIsUsable)
            {
                m_CoroutineRunner = runner;
                try
                {
                    m_ActiveCooldownCoroutine = m_CoroutineRunner.StartCoroutine(CooldownRoutine());
                }
                catch (System.Exception)
                {
                    // Ignore errors from starting coroutines
                }
            }
        }
        
        /// <summary>
        /// Reset cooldown for this skill
        /// </summary>
        public void ResetCooldown()
        {
            m_IsOnCooldown = false;
            m_CooldownRemaining = 0f;
            m_CooldownEndTime = 0f;
            m_UsesInWindow = 0;
            m_FirstUseTimeInWindow = 0f;
            
            if (m_CoroutineRunner != null && m_ActiveCooldownCoroutine != null)
            {
                try
                {
                    m_CoroutineRunner.StopCoroutine(m_ActiveCooldownCoroutine);
                }
                catch (System.Exception)
                {
                    // Ignore errors
                }
            }
            
            m_ActiveCooldownCoroutine = null;
            m_CoroutineRunner = null;
            
            OnCooldownChanged?.Invoke(this);
        }

        /// <summary>
        /// Register a successful use and decide whether cooldown should start now,
        /// based on the skill's stack/window configuration.
        /// </summary>
        public bool ShouldStartCooldownOnUse()
        {
            if (m_SkillReference == null) return true;

            // If not using stacks, cooldown starts immediately.
            if (!m_SkillReference.HasStacks)
            {
                m_UsesInWindow = 0;
                m_FirstUseTimeInWindow = 0f;
                return true;
            }

            float now = Time.time;

            // First use in current window
            if (m_UsesInWindow == 0)
            {
                m_FirstUseTimeInWindow = now;
            }
            m_UsesInWindow++;

            // Has stacks = always have a use count; cooldown when we reach it
            bool countReached = m_UsesInWindow >= Mathf.Max(1, m_SkillReference.StackUsesBeforeCooldown);

            bool timeReached = m_SkillReference.UseStackTime &&
                               m_SkillReference.StackWindowDuration > 0f &&
                               (now - m_FirstUseTimeInWindow) >= m_SkillReference.StackWindowDuration;

            if (countReached || timeReached)
            {
                // Reset window for next cycle
                m_UsesInWindow = 0;
                m_FirstUseTimeInWindow = 0f;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Update cooldown state (for cases where coroutine isn't running)
        /// </summary>
        public void UpdateCooldown()
        {
            if (!m_IsOnCooldown) return;
            if (m_ActiveCooldownCoroutine != null) return; // Coroutine is handling it
            
            float currentTime = Time.time;
            m_CooldownRemaining = Mathf.Max(0, m_CooldownEndTime - currentTime);
            
            if (m_CooldownRemaining <= 0)
            {
                m_IsOnCooldown = false;
                m_CooldownRemaining = 0;
                m_CooldownEndTime = 0;
                OnCooldownChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Get cooldown progress (0 = just started, 1 = complete)
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                float duration = CooldownDuration;
                if (duration <= 0) return 1f;
                float progress = (duration - cooldownRemaining) / duration;
                return Mathf.Clamp01(progress);
            }
        }
        
        private System.Collections.IEnumerator CooldownRoutine()
        {
            float cooldownDuration = CooldownDuration;
            m_CooldownEndTime = Time.time + cooldownDuration;
            
            while (m_CooldownRemaining > 0)
            {
                yield return null;
                m_CooldownRemaining = Mathf.Max(0, m_CooldownEndTime - Time.time);
                OnCooldownChanged?.Invoke(this);
            }
            
            m_IsOnCooldown = false;
            m_CooldownRemaining = 0f;
            m_ActiveCooldownCoroutine = null;
            OnCooldownChanged?.Invoke(this);
        }
    }
}




