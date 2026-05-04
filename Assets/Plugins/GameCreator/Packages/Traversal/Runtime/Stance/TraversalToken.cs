using System;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Traversal
{
    public class TraversalToken : ICancellable
    {
        [field: NonSerialized] public bool IsCancelled { get; set; }
    }
}