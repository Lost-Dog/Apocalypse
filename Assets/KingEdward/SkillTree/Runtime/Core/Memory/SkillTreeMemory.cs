using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace KingEdward.SkillTree
{

    [Image(typeof(IconSkillTreeComponent), ColorTheme.Type.Purple)]
    [Title("Skill Tree")]
    [Category("Skill Tree/Skill Tree")]
    [Description("Save and load unlocked skills and hotbar configuration")]
    
    [Serializable]
    public class MemorySkillTree : Memory
    {
        // Componente a ser salvo/carregado
        [SerializeField] private PropertyGetGameObject m_SkillTreeComponent = new PropertyGetGameObject();

        public override string Title => "Skill Tree";

        // Gerar Token para salvar 
        public override Token GetToken(GameObject target)
        {
            SkillTreeComponent skillTree = this.m_SkillTreeComponent.Get<SkillTreeComponent>(target);
            SkillHotbarUI hotbar = skillTree?.GetSkillHotbarUI();
            
            if (this.m_SkillTreeComponent == null)
            {
                Debug.LogError("[SkillTree] MemorySkillTree: m_SkillTreeComponent property is not configured");
                return null;
            }
            
            if (skillTree == null)
            {
                Debug.LogError("[SkillTree] MemorySkillTree: Could not find SkillTreeComponent in target");
                return null;
            }
            
            // Store the skillTree reference for later use
            var savedSkillTree = skillTree;
            
            // Create token with current state (even if no skills are unlocked)
            var token = new TokenSkillTree(savedSkillTree, hotbar);
            return token;
        }

        // Carregar dados salvos
        public override void OnRemember(GameObject target, Token token)
        {
            TokenSkillTree tokenSkillTree = token as TokenSkillTree;
            if (tokenSkillTree == null) 
            {
                Debug.LogWarning("[SkillTree] MemorySkillTree: No saved skill tree data found - starting with fresh state");
                return;
            }
            
            // Check if we have any saved skills to load
            if (tokenSkillTree.SkillIDs == null || tokenSkillTree.SkillIDs.Count == 0)
            {
                return;
            }
            
            SkillTreeComponent skillTree = this.m_SkillTreeComponent.Get<SkillTreeComponent>(target);
            if (skillTree == null) 
            {
                Debug.LogError("[SkillTree] MemorySkillTree: SkillTreeComponent not found while loading");
                return;
            }
            
            if (skillTree.skillTree == null)
            {
                Debug.LogError("[SkillTree] MemorySkillTree: skillTree.skillTree is null while loading");
                return;
            }
            
            if (skillTree.skillTree.allSkills == null || skillTree.skillTree.allSkills.Count == 0)
            {
                Debug.LogError("[SkillTree] MemorySkillTree: No skills found in skill tree");
                return;
            }
            
            SkillHotbarUI hotbar = skillTree?.GetSkillHotbarUI();
            
            // Clear existing skill instances
            skillTree.ClearUnlockedSkills();
            
            // Load skills from token
            if (tokenSkillTree.SkillIDs != null && skillTree.skillTree != null)
            {
                Args args = new Args(target);
                
                for (int i = 0; i < tokenSkillTree.SkillIDs.Count; i++)
                {
                    string guid = tokenSkillTree.SkillIDs[i];
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError("[SkillTree] MemorySkillTree: Null or empty GUID found in token");
                        continue;
                    }
                    
                    Skill skill = tokenSkillTree.FindSkillByGUID(guid, skillTree.skillTree);
                    if (skill != null)
                    {
                        // Only create skill instance if we have valid level data
                        if (i < tokenSkillTree.SkillLevels.Count)
                        {
                            int savedLevel = tokenSkillTree.SkillLevels[i];
                            if (savedLevel > 0 && savedLevel <= skill.maxLevel)
                            {
                                // Create skill instance only for unlocked skills
                                var skillInstance = SkillInstancePool.Get(skill);
                                skillInstance.SetLevel(savedLevel);
                                skillInstance.SetUnlocked(true);
                                
                                // Add to skill instances (using internal method)
                                skillTree.RestoreSkillInstance(skillInstance);
                                
                                // Reapply OnUnlock instructions if configured
                                if (skill.reapplyOnUnlockOnLoad && skill.onUnlock != null)
                                {
                                    _ = skill.onUnlock.Run(args);
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[SkillTree] Invalid level {savedLevel} for {skill.name}, skipping");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[SkillTree] No level data for {skill.name}, skipping");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[SkillTree] MemorySkillTree: Skill with GUID '{guid}' not found in current tree");
                    }
                }
            }
            
            // Configure hotbar if it exists
            if (hotbar != null && tokenSkillTree.HotbarSkillIDs != null && hotbar.slots != null)
            {
                // Reset all slots
                foreach (var slot in hotbar.slots)
                {
                    slot.skill = null;
                }
                
                // Configure slots in order
                int slotCount = Math.Min(tokenSkillTree.HotbarSkillIDs.Count, hotbar.slots.Count);
                
                for (int i = 0; i < slotCount; i++)
                {
                    string guid = tokenSkillTree.HotbarSkillIDs[i];
                    
                    if (!string.IsNullOrEmpty(guid))
                    {
                        Skill skill = tokenSkillTree.FindSkillByGUID(guid, skillTree.skillTree);
                        if (skill != null)
                        {
                            hotbar.slots[i].skill = skill;
                            
                            // Find existing skill instance (don't create new one during load)
                            SkillInstance skillInstance = skillTree.GetSkill(skill);
                            
                            if (skillInstance != null)
                            {
                                skillInstance.SetUnlocked(true);
                            }
                            else
                            {
                                Debug.LogWarning($"[SkillTree] Hotbar skill {skill.name} not found in skill instances - this shouldn't happen");
                            }
                        }
                    }
                }
                
                // Force refresh hotbar
                hotbar.ForceCompleteVisualRefresh();
            }
            
            // Restore Skill Points
            skillTree.SetSkillPoints(tokenSkillTree.SkillPoints);
            
            // Force refresh skill tree UI
            skillTree.TriggerRefreshAllSkills();
        }
    }
} 