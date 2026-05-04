using System.Collections.Generic;
using UnityEngine;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Object pool for SkillInstance to reduce allocations
    /// </summary>
    public static class SkillInstancePool
    {
        private static readonly Stack<SkillInstance> s_Pool = new Stack<SkillInstance>(32);
        private static readonly HashSet<SkillInstance> s_Active = new HashSet<SkillInstance>();
        
        /// <summary>
        /// Get a SkillInstance from the pool or create a new one
        /// </summary>
        public static SkillInstance Get(Skill skillReference)
        {
            if (skillReference == null)
            {
                Debug.LogError("[SkillInstancePool] Cannot create instance for null skill");
                return null;
            }
            
            SkillInstance instance;
            
            if (s_Pool.Count > 0)
            {
                instance = s_Pool.Pop();
                instance.Reset(skillReference);
            }
            else
            {
                instance = new SkillInstance(skillReference);
            }
            
            s_Active.Add(instance);
            return instance;
        }
        
        /// <summary>
        /// Return a SkillInstance to the pool
        /// </summary>
        public static void Release(SkillInstance instance)
        {
            if (instance == null) return;
            
            if (!s_Active.Remove(instance))
            {
                Debug.LogWarning("[SkillInstancePool] Trying to release instance that wasn't from pool");
                return;
            }
            
            instance.ResetCooldown();
            s_Pool.Push(instance);
        }
        
        /// <summary>
        /// Clear the pool (useful for scene transitions)
        /// </summary>
        public static void Clear()
        {
            s_Pool.Clear();
            s_Active.Clear();
        }
        
        /// <summary>
        /// Get pool statistics
        /// </summary>
        public static (int pooled, int active) GetStats()
        {
            return (s_Pool.Count, s_Active.Count);
        }
    }
}
