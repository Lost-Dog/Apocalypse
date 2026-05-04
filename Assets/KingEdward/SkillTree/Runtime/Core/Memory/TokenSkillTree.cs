using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KingEdward.SkillTree
{
    [Serializable]
    public class TokenSkillTree : Token
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private List<string> m_SkillIDs = new List<string>();
        [SerializeField] private List<int> m_SkillLevels = new List<int>();
        [SerializeField] private List<string> m_HotbarSkillIDs = new List<string>();
        [SerializeField] private int m_SkillPoints = 0;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public List<string> SkillIDs => this.m_SkillIDs;
        public List<int> SkillLevels => this.m_SkillLevels;
        public List<string> HotbarSkillIDs => this.m_HotbarSkillIDs;
        public int SkillPoints => this.m_SkillPoints;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TokenSkillTree(SkillTreeComponent skillTree, SkillHotbarUI hotbar)
        {
            this.m_SkillIDs = new List<string>();
            this.m_SkillLevels = new List<int>();
            this.m_HotbarSkillIDs = new List<string>();
            this.m_SkillPoints = skillTree != null ? skillTree.CurrentSkillPoints : 0;
            
            if (skillTree == null) 
            {
                Debug.LogError("[SkillTree] TokenSkillTree: skillTree component is null");
                return;
            }
            
            if (skillTree.skillTree == null)
            {
                Debug.LogError("[SkillTree] TokenSkillTree: skillTree asset reference is null");
                return;
            }
            
            var skillInstances = skillTree.GetAllSkillInstances();
            if (skillInstances == null)
            {
                Debug.LogError("[SkillTree] TokenSkillTree: skillInstances is null");
                return;
            }
            
            // Store skill instances using GUID and their levels
            foreach (SkillInstance skillInstance in skillInstances)
            {
                if (skillInstance != null && skillInstance.skillReference != null)
                {
                    string skillId = GetSkillGUID(skillInstance.skillReference);
                    if (!string.IsNullOrEmpty(skillId))
                    {
                        this.m_SkillIDs.Add(skillId);
                        this.m_SkillLevels.Add(skillInstance.currentLevel);
                    }
                }
            }
            
            // Store hotbar configuration
            if (hotbar != null && hotbar.slots != null)
            {
                this.m_HotbarSkillIDs = new List<string>(new string[hotbar.slots.Count]);
                
                for (int i = 0; i < hotbar.slots.Count; i++)
                {
                    if (hotbar.slots[i].skill != null)
                    {
                        string skillId = GetSkillGUID(hotbar.slots[i].skill);
                        this.m_HotbarSkillIDs[i] = skillId;
                    }
                    else
                    {
                        this.m_HotbarSkillIDs[i] = "";
                    }
                }
            }
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetSkillGUID(Skill skill)
        {
            if (skill == null) return "";
            return skill.UniqueID;
        }

        public Skill FindSkillByGUID(string guid, SkillTreeData skillTree)
        {
            if (string.IsNullOrEmpty(guid) || skillTree == null || skillTree.allSkills == null)
                return null;
            
            foreach (var skill in skillTree.allSkills)
            {
                if (skill != null && skill.UniqueID == guid)
                    return skill;
            }
            
            return null;
        }
    }
}
