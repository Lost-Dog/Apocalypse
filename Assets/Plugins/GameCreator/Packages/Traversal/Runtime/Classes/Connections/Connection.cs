using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]    
    public class Connection
    {
        [SerializeField] private EnablerFloat m_MaxDistance = new EnablerFloat(false, 5f);
        [SerializeField] private Traverse m_Traverse;

        public float MaxDistance => this.m_MaxDistance.IsEnabled
            ? this.m_MaxDistance.Value
            : float.MaxValue;
        
        public Traverse Traverse => this.m_Traverse;

        public Connection()
        {
            
        }

        public Connection(bool hasMaxDistance, float maxDistance, Traverse traverse)
        {
            this.m_MaxDistance = new EnablerFloat(hasMaxDistance, maxDistance);
            this.m_Traverse = traverse;
        }
    }
}