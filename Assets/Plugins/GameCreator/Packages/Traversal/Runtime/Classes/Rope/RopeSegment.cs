using System;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    internal struct RopeSegment
    {
        [field: NonSerialized] public Vector3 PreviousPosition { get; set; }
        [field: NonSerialized] public Vector3 CurrentPosition { get; set; }
        
        public RopeSegment(Vector3 position)
        {
            this.PreviousPosition = position;
            this.CurrentPosition = position;
        }

        public RopeSegment(Vector3 previousPosition, Vector3 currentPosition)
        {
            this.PreviousPosition = previousPosition;
            this.CurrentPosition = currentPosition;
        }
    }
}