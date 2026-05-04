using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;
using System.Threading.Tasks;
using System;
using KingEdward;

namespace KingEdward.SkillTree
{
    [Icon(SkillTreePaths.SKILL_TREE_COMPONENT)]
    [AddComponentMenu("KingEdward/Skill Tree/Skill Tree Component")]
    public class SkillTreeComponent : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false;
    
    // Track currently executing skill
    private Skill m_CurrentlyExecutingSkill = null;
    
    private void DebugLog(string message)
    {
        #if UNITY_EDITOR
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillTree] {message}");
        }
        #endif
    }

    private void Awake()
    {
        if (m_SkillInstances == null)
        {
            m_SkillInstances = new List<SkillInstance>();
        }
    }
    
    private void OnDestroy()
    {
        // Return all instances to pool
        foreach (var instance in m_SkillInstances)
        {
            if (instance != null)
            {
                SkillInstancePool.Release(instance);
            }
        }
        m_SkillInstances.Clear();
    }

    // Events
    public event Action<Skill> OnSkillUnlocked;
    public event Action<Skill> OnSkillCooldownChanged;
    public event Action<Skill, int> OnSkillLevelUp;
    public event Action<Skill> OnSkillUsed;
    public event Action<int> OnSkillPointsChanged;
    public event Action ForceRefreshAllSkills;

    [SerializeField] private SkillTreeData m_SkillTree;
    private List<SkillInstance> m_SkillInstances = new List<SkillInstance>();
    
    [Header("References")]
    [HideInInspector] [SerializeField] private PropertyGetGameObject m_SkillHotbarUI = GetGameObjectSelf.Create();
    private List<SkillHotbarUI> m_RegisteredHotbars = new List<SkillHotbarUI>();
    
    [Header("Skill Points System")]
    [SerializeField] private int m_CurrentSkillPoints = 10;
    [SerializeField] private int m_MaxSkillPoints = 999;
    
    [Header("Refund Settings")]
    [SerializeField] private bool m_AllowCascadeRefund = false;
    [Tooltip("Se true, permite fazer refund de skills que são prerequisitos, fazendo refund automático das skills dependentes. Se false, bloqueia o refund.")]
    public bool AllowCascadeRefund => m_AllowCascadeRefund;
    
    // Channel context: which hotbar/slot triggered the current skill (for channelled skills)
    private SkillHotbarUI m_ChannelInputHotbar;
    private int m_ChannelInputSlotIndex = -1;
    
    // Public read-only properties
    public SkillTreeData skillTree => m_SkillTree;
    public int CurrentSkillPoints => m_CurrentSkillPoints;
    public int MaxSkillPoints => m_MaxSkillPoints;
    public bool HasEnoughSkillPoints(int cost) => m_CurrentSkillPoints >= cost;
    
    /// <summary>
    /// Get SkillHotbarUI component 
    /// </summary>
    public SkillHotbarUI GetSkillHotbarUI()
    {
        // First try registered hotbars
        if (m_RegisteredHotbars.Count > 0)
        {
            return m_RegisteredHotbars[0];
        }
        
        // Fallback to property reference
        return m_SkillHotbarUI?.Get<SkillHotbarUI>(Args.EMPTY);
    }
    
    /// <summary>
    /// Register a hotbar with this skill tree
    /// </summary>
    public void RegisterHotbar(SkillHotbarUI hotbar)
    {
        if (hotbar != null && !m_RegisteredHotbars.Contains(hotbar))
        {
            m_RegisteredHotbars.Add(hotbar);
        }
    }
    
    /// <summary>
    /// Unregister a hotbar from this skill tree
    /// </summary>
    public void UnregisterHotbar(SkillHotbarUI hotbar)
    {
        if (hotbar != null)
        {
            m_RegisteredHotbars.Remove(hotbar);
        }
    }
    
    /// <summary>
    /// Get all skill instances (read-only access)
    /// </summary>
    public IEnumerable<SkillInstance> GetAllSkillInstances()
    {
        return m_SkillInstances;
    }
    
    /// <summary>
    /// Get count of unlocked skills
    /// </summary>
    public int GetUnlockedSkillCount()
    {
        int count = 0;
        foreach (var instance in m_SkillInstances)
        {
            if (instance != null && instance.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Restore a skill instance (used by save/load system)
    /// </summary>
    public void RestoreSkillInstance(SkillInstance instance)
    {
        if (instance == null) return;
        
        foreach (var existing in m_SkillInstances)
        {
            if (existing.skillReference == instance.skillReference) return;
        }
        
        m_SkillInstances.Add(instance);
    }
    
    /// <summary>
    /// Set the skill tree data
    /// </summary>
    public void SetSkillTree(SkillTreeData skillTreeData)
    {
        m_SkillTree = skillTreeData;
    }
    
    /// <summary>
    /// Add skill points
    /// </summary>
    public bool AddSkillPoints(int amount)
    {
        if (amount <= 0) return false;
        
        int newAmount = m_CurrentSkillPoints + amount;
        if (newAmount > m_MaxSkillPoints)
        {
            newAmount = m_MaxSkillPoints;
        }
        
        if (newAmount != m_CurrentSkillPoints)
        {
            SetSkillPoints(newAmount);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Set skill points directly
    /// </summary>
    public void SetSkillPoints(int amount)
    {
        int oldAmount = m_CurrentSkillPoints;
        m_CurrentSkillPoints = Mathf.Clamp(amount, 0, m_MaxSkillPoints);
        if (oldAmount != m_CurrentSkillPoints)
        {
            OnSkillPointsChanged?.Invoke(m_CurrentSkillPoints);
        }
    }
    
    /// <summary>
    /// Reset skill points to zero
    /// </summary>
    public void ResetSkillPoints()
    {
        SetSkillPoints(0);
    }

    /// <summary>
    /// Check if a skill can be unlocked (prerequisites met)
    /// </summary>
    public bool CanUnlock(Skill skill)
    {
        if (skill == null)
        {
            return false;
        }
        
        // If already unlocked, can't unlock again
        if (IsUnlocked(skill))
        {
            return false;
        }
        
        // Check skill's unlock conditions
        Args args = new Args(this.gameObject);
        if (!skill.CanUnlock(args))
        {
            DebugLog($"{skill.name} unlock conditions not met");
            return false;
        }
        
        // Check cost
        if (skill.Cost > 0)
        {
            if (!HasEnoughSkillPoints(skill.Cost))
            {
                DebugLog($"{skill.name} costs {skill.Cost} but only has {CurrentSkillPoints} skill points");
                return false;
            }
        }
        
        // Check prerequisites
        if (skill.prerequisites != null && skill.prerequisites.Count > 0)
        {
            DebugLog($"Checking {skill.prerequisites.Count} prerequisites for {skill.name}");
            
            foreach (var prereq in skill.prerequisites)
            {
                if (prereq == null)
                {
                    continue;
                }
                
                bool isMet = prereq.IsMet(this);
                DebugLog($"- {prereq.GetStatusText(this)}");
                
                if (!isMet)
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// Unlock a skill (async version)
    /// </summary>
    public async Task<bool> UnlockSkillAsync(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillTree] Cannot unlock null skill");
            return false;
        }

        // Check if already unlocked first
        if (IsUnlocked(skill))
        {
            DebugLog($"{skill.name} is already unlocked");
            return false;
        }

        if (!CanUnlock(skill))
        {
            DebugLog($"Cannot unlock {skill.name} - conditions not met");
            return false;
        }

        try
        {
            // Calculate actual cost using Property 
            Args costArgs = new Args(this.gameObject);
            int actualCost = (int)skill.GetCost(costArgs);
            
            // Deduct skill points if necessary
            if (actualCost > 0)
            {
                SetSkillPoints(m_CurrentSkillPoints - actualCost);
                DebugLog($"Spent {actualCost} skill points to unlock {skill.name}. Remaining: {CurrentSkillPoints}");
            }
            
            // Find existing skill instance or create new one from pool
            SkillInstance skillInstance = GetOrCreateSkill(skill);
            
            skillInstance.Unlock();
            skillInstance.AddPointsSpent(actualCost);
            DebugLog($"Skill unlocked: {skill.name}. Total instances: {m_SkillInstances.Count}");
            
            // Trigger events
            OnSkillUnlocked?.Invoke(skill);
            
            // Execute unlock instructions
            if (skill.onUnlock != null)
            {
                await skill.onUnlock.Run(new Args(this.gameObject));
            }

            // Force UI refresh
            TriggerRefreshAllSkills();
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SkillTree] Error unlocking skill {skill.name}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Unlock a skill (synchronous wrapper)
    /// </summary>
    public void UnlockSkill(Skill skill)
    {
        _ = UnlockSkillAsync(skill);
    }

    /// <summary>
    /// Clear all unlocked skills
    /// </summary>
    public void ClearUnlockedSkills()
    {
        foreach (var instance in m_SkillInstances)
        {
            if (instance != null)
            {
                SkillInstancePool.Release(instance);
            }
        }
        
        m_SkillInstances.Clear();
        TriggerRefreshAllSkills();
    }
    
    /// <summary>
    /// Force complete reset - remove ALL instances and reset skill points
    /// </summary>
    [ContextMenu("Force Complete Reset")]
    public void ForceCompleteReset()
    {
        ClearUnlockedSkills();
        ResetSkillPoints();
    }

    /// <summary>
    /// Check if a skill is unlocked
    /// </summary>
    public bool IsUnlocked(Skill skill)
    {
        if (skill == null) return false;
        
        // Find existing skill instance WITHOUT creating a new one
        foreach (var skillInstance in m_SkillInstances)
        {
            if (skillInstance.skillReference == skill)
            {
                return skillInstance.isUnlocked;
            }
        }
        
        // No skill instance found = not unlocked
        return false;
    }

    /// <summary>
    /// Trigger UI refresh
    /// </summary>
    public void TriggerRefreshAllSkills()
    {
        ForceRefreshAllSkills?.Invoke();
    }
    
    /// <summary>
    /// Remove skill from all hotbars in the scene
    /// </summary>
    private void RemoveSkillFromHotbar(Skill skill)
    {
        // Use registered hotbars first
        foreach (var hotbar in m_RegisteredHotbars)
        {
            if (hotbar != null)
            {
                hotbar.RemoveSkillFromAllSlots(skill);
            }
        }
        
        // Fallback: find all hotbars in scene
        if (m_RegisteredHotbars.Count == 0)
        {
            SkillHotbarUI[] hotbars = FindObjectsByType<SkillHotbarUI>(FindObjectsSortMode.None);
            foreach (var hotbar in hotbars)
            {
                hotbar.RemoveSkillFromAllSlots(skill);
            }
        }
    }

    /// <summary>
    /// Use a skill (async version)
    /// </summary>
    public async Task<bool> UseSkillAsync(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillTree] Cannot use null skill");
            return false;
        }
        
        if (!IsUnlocked(skill))
        {
            DebugLog($"Cannot use {skill.name} - not unlocked");
            return false;
        }

        SkillInstance skillInstance = GetSkill(skill);
        if (skillInstance == null)
        {
            Debug.LogWarning($"[SkillTree] No instance found for {skill.name}");
            return false;
        }

        if (skillInstance.isOnCooldown)
        {
            DebugLog($"Cannot use {skill.name} - on cooldown");
            return false;
        }

        // Check use conditions
        Args conditionArgs = new Args(this.gameObject);
        if (!skill.CanUse(conditionArgs))
        {
            DebugLog($"Cannot use {skill.name} - use conditions not met");
            return false;
        }

        // Check if another skill is currently executing
        if (m_CurrentlyExecutingSkill != null)
        {
            // Check if current skill can be interrupted
            if (!m_CurrentlyExecutingSkill.CanBeInterrupted)
            {
                DebugLog($"Cannot use {skill.name} - {m_CurrentlyExecutingSkill.name} cannot be interrupted");
                return false;
            }
            
            // Check if new skill can interrupt
            if (!skill.CanInterruptOthers)
            {
                DebugLog($"Cannot use {skill.name} - cannot interrupt {m_CurrentlyExecutingSkill.name}");
                return false;
            }
            
            // Cancel current skill's sequencer if it has one
            if (m_CurrentlyExecutingSkill.UseSequencer && m_CurrentlyExecutingSkill.Sequencer != null)
            {
                Args cancelArgs = new Args(this.gameObject);
                m_CurrentlyExecutingSkill.Sequencer.Cancel(cancelArgs);
                DebugLog($"Interrupted {m_CurrentlyExecutingSkill.name} with {skill.name}");
            }
        }

        try
        {
            // Mark this skill as currently executing
            m_CurrentlyExecutingSkill = skill;
            
            // Trigger OnSkillUsed event
            OnSkillUsed?.Invoke(skill);
            
            // Execute skill (handles animation + sequencer + onUse instructions, or channel loop)
            Args args = new Args(this.gameObject);
            await skill.ExecuteAsync(args);
            
            // Decide whether to start cooldown now based on stack/window configuration
            if (skillInstance.ShouldStartCooldownOnUse())
            {
                skillInstance.StartCooldown(this);
                OnSkillCooldownChanged?.Invoke(skill);
            }
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SkillTree] Error using skill {skill.name}: {ex.Message}");
            return false;
        }
        finally
        {
            // Clear currently executing skill
            if (m_CurrentlyExecutingSkill == skill)
            {
                m_CurrentlyExecutingSkill = null;
                // Clear channel context after the skill finishes
                m_ChannelInputHotbar = null;
                m_ChannelInputSlotIndex = -1;
            }
        }
    }
    
    /// <summary>
    /// Use a skill (synchronous wrapper)
    /// </summary>
    public void UseSkill(Skill skill)
    {
        _ = UseSkillAsync(skill);
    }

    // AIM / INDICATOR (hold to aim, release to cast)
    private Skill m_SkillInAimMode;

    /// <summary>
    /// Whether currently in aim mode (holding key, indicator visible).
    /// </summary>
    public bool IsAiming => m_SkillInAimMode != null;

    /// <summary>
    /// Start aim mode: show indicator while holding. Call from input on key down.
    /// </summary>
    public void BeginAim(Skill skill)
    {
        if (skill == null) return;
        var config = skill.IndicatorConfig;
        if (config == null || !config.HasIndicator) return;
        if (m_SkillInAimMode != null) CancelAim();
        m_SkillInAimMode = skill;
        SkillIndicatorController.Instance?.ShowForSkill(skill);
    }

    /// <summary>
    /// End aim and cast the skill. Call from input on key up.
    /// </summary>
    public void EndAimAndCast()
    {
        if (m_SkillInAimMode == null) return;
        Skill skill = m_SkillInAimMode;
        m_SkillInAimMode = null;
        SkillIndicatorController.Instance?.Hide();
        UseSkill(skill);
    }

    /// <summary>
    /// Cancel aim without casting.
    /// </summary>
    public void CancelAim()
    {
        m_SkillInAimMode = null;
        SkillIndicatorController.Instance?.Hide();
    }

    /// <summary>
    /// Reset all cooldowns
    /// </summary>
    public void ResetAllCooldowns()
    {
        foreach (var skillInstance in m_SkillInstances)
        {
            if (skillInstance != null && skillInstance.skillReference != null)
            {
                skillInstance.ResetCooldown();
                OnSkillCooldownChanged?.Invoke(skillInstance.skillReference);
            }
        }
    }

    /// <summary>
    /// Called by hotbars before using a skill so channelled skills can query which slot/input is being held.
    /// </summary>
    public void SetChannelInputContext(SkillHotbarUI hotbar, int slotIndex)
    {
        m_ChannelInputHotbar = hotbar;
        m_ChannelInputSlotIndex = slotIndex;
    }

    /// <summary>
    /// Returns true while the input associated with the current channel context is being held.
    /// </summary>
    public bool IsChannelInputActive()
    {
        if (m_ChannelInputHotbar == null || m_ChannelInputSlotIndex < 0) return false;
        return m_ChannelInputHotbar.IsSlotInputHeld(m_ChannelInputSlotIndex);
    }

    /// <summary>
    /// Get a specific skill instance (returns null if not found)
    /// </summary>
    public SkillInstance GetSkill(Skill skillReference)
    {
        if (skillReference == null) return null;
        
        // Search for the skill in the instances list
        foreach (var skillInstance in m_SkillInstances)
        {
            if (skillInstance.skillReference == skillReference)
            {
                return skillInstance;
            }
        }
        
        // Not found = not unlocked
        return null;
    }
    
    /// <summary>
    /// Check if a skill is a prerequisite for any unlocked skill
    /// </summary>
    public bool IsPrerequisiteForUnlockedSkill(Skill skill)
    {
        if (skill == null || m_SkillTree == null || m_SkillTree.allSkills == null)
        {
            return false;
        }
        
        // Check all skills in the skill tree
        foreach (var otherSkill in m_SkillTree.allSkills)
        {
            if (otherSkill == null || otherSkill == skill || !IsUnlocked(otherSkill))
            {
                continue;
            }
            
            // Check if this skill is in the prerequisites
            if (otherSkill.prerequisites != null)
            {
                foreach (var prereq in otherSkill.prerequisites)
                {
                    if (prereq != null && prereq.skill == skill)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Get or create a skill instance using Object Pool
    /// </summary>
    private SkillInstance GetOrCreateSkill(Skill skillReference)
    {
        if (skillReference == null)
        {
            Debug.LogError("[SkillTree] Cannot create instance for null skill");
            return null;
        }
        
        // Search for existing instance
        foreach (var skillInstance in m_SkillInstances)
        {
            if (skillInstance.skillReference == skillReference)
            {
                return skillInstance;
            }
        }
        
        // Get from pool
        var newInstance = SkillInstancePool.Get(skillReference);
        m_SkillInstances.Add(newInstance);
        DebugLog($"GetOrCreateSkill - Got instance from pool for {skillReference.name}");
        return newInstance;
    }

    /// <summary>
    /// Level up a skill
    /// </summary>
    public bool LevelUpSkill(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillTree] Cannot level up null skill");
            return false;
        }
        
        if (!IsUnlocked(skill))
        {
            DebugLog($"Cannot level up {skill.name} - not unlocked");
            return false;
        }

        SkillInstance skillInstance = GetSkill(skill);
        if (skillInstance == null)
        {
            Debug.LogWarning($"[SkillTree] No instance found for {skill.name}");
            return false;
        }
        
        if (!skillInstance.CanLevelUp)
        {
            DebugLog($"Cannot level up {skill.name} - max level reached or insufficient points");
            return false;
        }
        
        // Check if has enough skill points
        int levelUpCost = skill.Cost; 
        if (!HasEnoughSkillPoints(levelUpCost))
        {
            DebugLog($"Cannot level up {skill.name} - not enough skill points (need {levelUpCost}, have {CurrentSkillPoints})");
            return false;
        }

        try
        {
            Args costArgs = new Args(this.gameObject);
            int actualCost = (int)skill.GetCost(costArgs);
            
            // Deduct skill points
            SetSkillPoints(m_CurrentSkillPoints - actualCost);
            DebugLog($"Spent {actualCost} skill points to level up {skill.name}. Remaining: {CurrentSkillPoints}");
            
            Args args = new Args(this.gameObject);
            bool success = skillInstance.LevelUp(args);
            
            if (success)
            {
                skillInstance.AddPointsSpent(actualCost);
                OnSkillLevelUp?.Invoke(skill, skillInstance.currentLevel);
                TriggerRefreshAllSkills();
                DebugLog($"Leveled up {skill.name} to level {skillInstance.currentLevel}");
            }
            else
            {
                // Refund points if level up failed
                SetSkillPoints(m_CurrentSkillPoints + actualCost);
                DebugLog($"Level up failed, refunded {actualCost} skill points");
            }
            
            return success;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SkillTree] Error leveling up skill {skill.name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Refund a skill, returning skill points and resetting it
    /// </summary>
    /// <param name="skill">The skill to refund</param>
    /// <param name="refundAllLevels">If true, refunds all levels. If false, only refunds unlock cost</param>
    /// <param name="forceCascade">If true, forces cascade refund even if AllowCascadeRefund is false (used internally for recursion)</param>
    public bool RefundSkill(Skill skill, bool refundAllLevels = true, bool forceCascade = false)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillTree] Cannot refund null skill");
            return false;
        }
        
        if (!IsUnlocked(skill))
        {
            DebugLog($"Cannot refund {skill.name} - not unlocked");
            return false;
        }

        SkillInstance skillInstance = GetSkill(skill);
        if (skillInstance == null)
        {
            Debug.LogWarning($"[SkillTree] No instance found for {skill.name}");
            return false;
        }
        
        // Check if this skill is a prerequisite for any unlocked skill
        bool isPrerequisite = IsPrerequisiteForUnlockedSkill(skill);
        
        if (isPrerequisite)
        {
            // Se não permite cascade refund E não é uma chamada forçada (recursiva), bloqueia
            if (!m_AllowCascadeRefund && !forceCascade)
            {
                Debug.LogWarning($"[SkillTree] Cannot refund {skill.name} - it is a prerequisite for other unlocked skills. Enable 'Allow Cascade Refund' to refund with dependencies.");
                return false;
            }
            
            // Se permite cascade refund, faz refund das dependências primeiro
            if (m_AllowCascadeRefund || forceCascade)
            {
                DebugLog($"Cascade refund for {skill.name} - refunding dependent skills first");
                
                // Get all dependent skills and refund them first
                List<Skill> dependentSkills = GetDependentUnlockedSkills(skill);
                
                foreach (var dependentSkill in dependentSkills)
                {
                    DebugLog($"Cascade refunding dependent skill: {dependentSkill.name}");
                    RefundSkill(dependentSkill, refundAllLevels, forceCascade: true); // Recursive cascade
                }
            }
        }

        try
        {
            // Calculate refund amount using tracked total points spent
            int refundAmount = refundAllLevels ? skillInstance.totalPointsSpent : skill.Cost;
            
            // Return skill points
            if (refundAmount > 0)
            {
                AddSkillPoints(refundAmount);
                DebugLog($"Refunded {refundAmount} skill points from {skill.name}");
            }
            
            // Remove skill instance
            m_SkillInstances.Remove(skillInstance);
            SkillInstancePool.Release(skillInstance);
            
            // Remove from hotbar if present
            RemoveSkillFromHotbar(skill);
            
            // Trigger UI refresh
            TriggerRefreshAllSkills();
            
            DebugLog($"Refunded skill: {skill.name}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SkillTree] Error refunding skill {skill.name}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get all unlocked skills that depend on the specified skill as a prerequisite
    /// </summary>
    private List<Skill> GetDependentUnlockedSkills(Skill skill)
    {
        List<Skill> dependentSkills = new List<Skill>();
        
        if (skill == null || m_SkillTree == null || m_SkillTree.allSkills == null)
        {
            return dependentSkills;
        }
        
        foreach (var otherSkill in m_SkillTree.allSkills)
        {
            if (otherSkill == null || otherSkill == skill || !IsUnlocked(otherSkill))
            {
                continue;
            }
            
            // Check if this skill is in the prerequisites
            if (otherSkill.prerequisites != null)
            {
                foreach (var prereq in otherSkill.prerequisites)
                {
                    if (prereq != null && prereq.skill == skill)
                    {
                        dependentSkills.Add(otherSkill);
                        break;
                    }
                }
            }
        }
        
        return dependentSkills;
    }


} 
}
