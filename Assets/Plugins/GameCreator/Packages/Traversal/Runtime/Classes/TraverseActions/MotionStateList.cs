using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class MotionStateList : TPolymorphicList<MotionStateItem>
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeReference] private MotionStateItem[] m_States = 
        {
            new MotionStateItem()
        };
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Dictionary<IdString, MotionStateItem> m_StateMap;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => this.m_States.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public bool Contains(IdString stateId)
        {
            #if UNITY_EDITOR
            
            foreach (MotionStateItem state in this.m_States)
            {
                if (state.Id != stateId) continue;
                return true;
            }
            
            return false;
            
            #else
            
            this.RequireMap();
            return this.m_StateMap.ContainsKey(stateId);

            #endif
        }

        public MotionStateItem Get(IdString stateId)
        {
            #if UNITY_EDITOR
            
            foreach (MotionStateItem state in this.m_States)
            {
                if (state.Id != stateId) continue;
                return state;
            }
            
            return null;
            
            #else

            this.RequireMap();
            return this.m_StateMap.GetValueOrDefault(stateId);

            #endif
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RequireMap()
        {
            if (this.m_StateMap != null) return;
            
            this.m_StateMap = new Dictionary<IdString, MotionStateItem>(this.m_States.Length);
            foreach (MotionStateItem state in this.m_States)
            {
                this.m_StateMap[state.Id] = state;
            }
        }
    }
}