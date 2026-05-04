using UnityEngine;
using UnityEngine.UI;
using GameCreator.Runtime.Common;
using System.Collections.Generic;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Optional component for skill tree preview/confirmation (Souls-like style)
    /// Allows selecting multiple skills before confirming changes
    /// </summary>
    [Icon(SkillTreePaths.CONFIRMATION_UI)]
    [AddComponentMenu("KingEdward/Skill Tree/Skill Tree Confirmation UI")]
    public class SkillTreeConfirmationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject m_ConfirmationPanel;
        [SerializeField] private Text m_PendingSkillsText;
        [SerializeField] private Text m_TotalCostText;
        [SerializeField] private Text m_CurrentPointsText;
        [SerializeField] private Text m_RemainingPointsText;
        [SerializeField] private Button m_ConfirmButton;
        [SerializeField] private Button m_CancelButton;
        
        [Header("Settings")]
        [SerializeField] private ConfirmationMode m_Mode = ConfirmationMode.Immediate;
        [SerializeField] private bool m_AutoShowPanel = true;
        [SerializeField] private bool m_PreventDirectUnlock = true;
        [SerializeField] private string m_PendingSkillsLabel = "Pending Changes:";
        
        [Header("Color Settings")]
        [SerializeField] private Color m_CostColor = Color.red;
        [SerializeField] private Color m_RefundColor = Color.green;
        [SerializeField] private Color m_NeutralColor = Color.white;
        [SerializeField] private Color m_InsufficientPointsColor = Color.red;
        [SerializeField] private Color m_SufficientPointsColor = Color.white;
        [SerializeField] private Color m_GainPointsColor = Color.green;
        
        public enum ConfirmationMode
        {
            Immediate,  // Shows confirmation for each skill (1 at a time)
            Batch       // Stacks multiple skills (Souls-like)
        }
        
        public bool PreventDirectUnlock => m_PreventDirectUnlock;
        
        private SkillTreeComponent m_SkillTreeComponent;
        private List<PendingSkillChange> m_PendingChanges = new List<PendingSkillChange>();
        
        private class PendingSkillChange
        {
            public Skill skill;
            public ChangeType type;
            public int cost; // Positive for unlock/levelup, negative for refund
            
            public enum ChangeType
            {
                Unlock,
                Refund,
                LevelUp
            }
        }
        
        private void Awake()
        {
            if (m_ConfirmationPanel != null)
            {
                m_ConfirmationPanel.SetActive(false);
            }
            
            if (m_ConfirmButton != null)
            {
                m_ConfirmButton.onClick.AddListener(OnConfirmClicked);
            }
            
            if (m_CancelButton != null)
            {
                m_CancelButton.onClick.AddListener(OnCancelClicked);
            }
        }
        
        private void OnEnable()
        {
            SkillItemUI.EventSkillClicked += OnSkillItemClicked;
        }
        
        private void OnDisable()
        {
            SkillItemUI.EventSkillClicked -= OnSkillItemClicked;
            m_PendingChanges.Clear();
        }
        
        private void OnSkillItemClicked(SkillItemUI skillItem)
        {
            if (skillItem == null || skillItem.skill == null) return;
            
            SkillTreeComponent skillTree = skillItem.GetSkillTreeComponent();
            if (skillTree == null) return;
            
            if (m_SkillTreeComponent == null)
            {
                m_SkillTreeComponent = skillTree;
            }
            
            bool isUnlocked = skillTree.IsUnlocked(skillItem.skill);
            
            if (!isUnlocked && skillTree.CanUnlock(skillItem.skill))
            {
                AddPendingUnlock(skillItem.skill);
            }
        }
        
        /// <summary>
        /// Set the skill tree component reference
        /// </summary>
        public void SetSkillTreeComponent(SkillTreeComponent skillTree)
        {
            m_SkillTreeComponent = skillTree;
        }
        
        /// <summary>
        /// Add a skill unlock to pending changes
        /// </summary>
        public void AddPendingUnlock(Skill skill)
        {
            if (skill == null || m_SkillTreeComponent == null) return;
            
            if (m_Mode == ConfirmationMode.Immediate)
            {
                m_PendingChanges.Clear();
            }
            
            foreach (var change in m_PendingChanges)
            {
                if (change.skill == skill) return;
            }
            
            m_PendingChanges.Add(new PendingSkillChange
            {
                skill = skill,
                type = PendingSkillChange.ChangeType.Unlock,
                cost = skill.Cost
            });
            
            UpdateDisplay();
            
            if (m_AutoShowPanel && m_ConfirmationPanel != null && !m_ConfirmationPanel.activeSelf)
            {
                m_ConfirmationPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Add a skill refund to pending changes
        /// </summary>
        public void AddPendingRefund(Skill skill, int refundAmount)
        {
            if (skill == null || m_SkillTreeComponent == null) return;
            
            if (m_Mode == ConfirmationMode.Immediate)
            {
                m_PendingChanges.Clear();
            }
            
            foreach (var change in m_PendingChanges)
            {
                if (change.skill == skill) return;
            }
            
            bool isPrerequisite = m_SkillTreeComponent.IsPrerequisiteForUnlockedSkill(skill);
            bool allowCascade = m_SkillTreeComponent.AllowCascadeRefund;
            
            m_PendingChanges.Add(new PendingSkillChange
            {
                skill = skill,
                type = PendingSkillChange.ChangeType.Refund,
                cost = -refundAmount
            });
            
            if (isPrerequisite && allowCascade)
            {
                AddCascadeRefunds(skill);
            }
            
            UpdateDisplay();
            
            if (m_AutoShowPanel && m_ConfirmationPanel != null && !m_ConfirmationPanel.activeSelf)
            {
                m_ConfirmationPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Recursively add dependent skills to refund list
        /// </summary>
        private void AddCascadeRefunds(Skill skill)
        {
            if (skill == null || m_SkillTreeComponent == null) return;
            
            // Get all dependent skills
            var dependentSkills = GetDependentUnlockedSkills(skill);
            
            foreach (var dependentSkill in dependentSkills)
            {
                // Check if already in pending list
                bool alreadyPending = false;
                foreach (var change in m_PendingChanges)
                {
                    if (change.skill == dependentSkill)
                    {
                        alreadyPending = true;
                        break;
                    }
                }
                
                if (alreadyPending) continue;
                
                // Calculate refund amount for dependent skill
                SkillInstance skillInstance = m_SkillTreeComponent.GetSkill(dependentSkill);
                if (skillInstance != null)
                {
                    int refundAmount = dependentSkill.Cost; // Initial unlock cost
                    int levelsGained = skillInstance.currentLevel - 1;
                    refundAmount += levelsGained * dependentSkill.Cost;
                    
                    // Add to pending
                    m_PendingChanges.Add(new PendingSkillChange
                    {
                        skill = dependentSkill,
                        type = PendingSkillChange.ChangeType.Refund,
                        cost = -refundAmount
                    });
                    
                    // Recursively add dependents of this skill
                    AddCascadeRefunds(dependentSkill);
                }
            }
        }
        
        /// <summary>
        /// Get all unlocked skills that depend on the specified skill
        /// </summary>
        private List<Skill> GetDependentUnlockedSkills(Skill skill)
        {
            List<Skill> dependentSkills = new List<Skill>();
            
            if (skill == null || m_SkillTreeComponent == null || m_SkillTreeComponent.skillTree == null)
            {
                return dependentSkills;
            }
            
            foreach (var otherSkill in m_SkillTreeComponent.skillTree.allSkills)
            {
                if (otherSkill == null || otherSkill == skill || !m_SkillTreeComponent.IsUnlocked(otherSkill))
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
        
        /// <summary>
        /// Add a skill level up to pending changes
        /// </summary>
        public void AddPendingLevelUp(Skill skill, int cost)
        {
            if (skill == null || m_SkillTreeComponent == null) return;
            
            if (m_Mode == ConfirmationMode.Immediate)
            {
                m_PendingChanges.Clear();
            }
            
            foreach (var change in m_PendingChanges)
            {
                if (change.skill == skill) return;
            }
            
            m_PendingChanges.Add(new PendingSkillChange
            {
                skill = skill,
                type = PendingSkillChange.ChangeType.LevelUp,
                cost = cost
            });
            
            UpdateDisplay();
            
            if (m_AutoShowPanel && m_ConfirmationPanel != null && !m_ConfirmationPanel.activeSelf)
            {
                m_ConfirmationPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Remove a pending change
        /// </summary>
        public void RemovePendingChange(Skill skill)
        {
            m_PendingChanges.RemoveAll(c => c.skill == skill);
            UpdateDisplay();
            
            // Hide panel if no pending changes
            if (m_PendingChanges.Count == 0 && m_ConfirmationPanel != null)
            {
                m_ConfirmationPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Clear all pending changes
        /// </summary>
        public void ClearPendingChanges()
        {
            m_PendingChanges.Clear();
            UpdateDisplay();
            
            if (m_ConfirmationPanel != null)
            {
                m_ConfirmationPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Check if there are pending changes
        /// </summary>
        public bool HasPendingChanges()
        {
            return m_PendingChanges.Count > 0;
        }
        
        private void UpdateDisplay()
        {
            if (m_SkillTreeComponent == null) return;
            
            // Calculate total cost
            int totalCost = 0;
            foreach (var change in m_PendingChanges)
            {
                totalCost += change.cost;
            }
            
            // Update pending skills list
            if (m_PendingSkillsText != null)
            {
                string text = m_PendingSkillsLabel + "\n";
                foreach (var change in m_PendingChanges)
                {
                    string action = "";
                    switch (change.type)
                    {
                        case PendingSkillChange.ChangeType.Unlock:
                            action = "Unlock";
                            break;
                        case PendingSkillChange.ChangeType.LevelUp:
                            action = "Level Up";
                            break;
                        case PendingSkillChange.ChangeType.Refund:
                            action = "Refund";
                            break;
                    }
                    
                    string costStr = change.cost > 0 ? $"-{change.cost}" : $"+{-change.cost}";
                    text += $"• {action} {change.skill.SkillName} ({costStr})\n";
                }
                m_PendingSkillsText.text = text;
            }
            
            // Update cost display
            if (m_TotalCostText != null)
            {
                if (totalCost > 0)
                {
                    m_TotalCostText.text = $"Total Cost: {totalCost}";
                    m_TotalCostText.color = m_CostColor;
                }
                else if (totalCost < 0)
                {
                    m_TotalCostText.text = $"Total Refund: +{-totalCost}";
                    m_TotalCostText.color = m_RefundColor;
                }
                else
                {
                    m_TotalCostText.text = "Total Cost: 0";
                    m_TotalCostText.color = m_NeutralColor;
                }
            }
            
            // Update points display
            int currentPoints = m_SkillTreeComponent.CurrentSkillPoints;
            int remainingPoints = currentPoints - totalCost;
            
            if (m_CurrentPointsText != null)
            {
                m_CurrentPointsText.text = $"Current Points: {currentPoints}";
            }
            
            if (m_RemainingPointsText != null)
            {
                m_RemainingPointsText.text = $"After Changes: {remainingPoints}";
                
                if (remainingPoints < 0)
                {
                    m_RemainingPointsText.color = m_InsufficientPointsColor;
                }
                else if (remainingPoints > currentPoints)
                {
                    m_RemainingPointsText.color = m_GainPointsColor;
                }
                else
                {
                    m_RemainingPointsText.color = m_SufficientPointsColor;
                }
            }
            
            // Enable/disable confirm button based on points
            if (m_ConfirmButton != null)
            {
                m_ConfirmButton.interactable = remainingPoints >= 0;
            }
        }
        
        private void OnConfirmClicked()
        {
            if (m_SkillTreeComponent == null) return;
            
            // Apply all pending changes
            foreach (var change in m_PendingChanges)
            {
                if (change.type == PendingSkillChange.ChangeType.Unlock)
                {
                    m_SkillTreeComponent.UnlockSkill(change.skill);
                }
                else if (change.type == PendingSkillChange.ChangeType.Refund)
                {
                    m_SkillTreeComponent.RefundSkill(change.skill, refundAllLevels: true, forceCascade: false);
                }
                else if (change.type == PendingSkillChange.ChangeType.LevelUp)
                {
                    m_SkillTreeComponent.LevelUpSkill(change.skill);
                }
            }
            
            // Clear and close
            m_PendingChanges.Clear();
            if (m_ConfirmationPanel != null)
            {
                m_ConfirmationPanel.SetActive(false);
            }
        }
        
        private void OnCancelClicked()
        {
            m_PendingChanges.Clear();
            if (m_ConfirmationPanel != null)
            {
                m_ConfirmationPanel.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            if (m_ConfirmButton != null)
            {
                m_ConfirmButton.onClick.RemoveListener(OnConfirmClicked);
            }
            
            if (m_CancelButton != null)
            {
                m_CancelButton.onClick.RemoveListener(OnCancelClicked);
            }
        }
    }
}
