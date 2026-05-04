using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class MotionActionsList : TPolymorphicList<MotionActionsItem>
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeReference] private MotionActionsItem[] m_Actions = 
        {
            new MotionActionsItem()
        };
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Dictionary<IdString, MotionActionsItem> m_ActionsMap;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => this.m_Actions.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public bool Contains(IdString actionId)
        {
            #if UNITY_EDITOR
            
            foreach (MotionActionsItem action in this.m_Actions)
            {
                if (action.Id != actionId) continue;
                return true;
            }
            
            return false;
            
            #else
            
            this.RequireMap();
            return this.m_ActionsMap.ContainsKey(actionId);

            #endif
        }

        public MotionActionsItem Get(IdString actionId)
        {
            #if UNITY_EDITOR
            
            foreach (MotionActionsItem action in this.m_Actions)
            {
                if (action.Id != actionId) continue;
                return action;
            }
            
            return null;
            
            #else

            this.RequireMap();
            return this.m_ActionsMap.GetValueOrDefault(actionId);

            #endif
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RequireMap()
        {
            if (this.m_ActionsMap != null) return;
            
            this.m_ActionsMap = new Dictionary<IdString, MotionActionsItem>(this.m_Actions.Length);
            foreach (MotionActionsItem action in this.m_Actions)
            {
                this.m_ActionsMap[action.Id] = action;
            }
        }
    }
}