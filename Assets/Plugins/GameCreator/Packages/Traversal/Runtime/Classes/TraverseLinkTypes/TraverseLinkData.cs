using System;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public struct TraverseLinkData
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] public Vector3 positionA;
        [SerializeField] public Vector3 positionB;
        
        [SerializeField] public Quaternion rotationA;
        [SerializeField] public Quaternion rotationB;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public TraverseLinkData(
            Vector3 positionA,
            Vector3 positionB,
            Quaternion rotationA,
            Quaternion rotationB)
        {
            this.positionA = positionA;
            this.positionB = positionB;
            
            this.rotationA = rotationA;
            this.rotationB = rotationB;
        }
    }
}