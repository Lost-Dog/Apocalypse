using UnityEngine;
using UnityEngine.Events;

namespace JU
{
    /// <summary>
    /// Defines a property that can be used by object that can have a specific target. Useful for enemies, police vehicles, monsters or any other thing
    /// that must have a specific target.
    /// </summary>
    public interface IOnSetTarget
    {
        /// <summary>
        /// Called on change the current target.
        /// </summary>
        event UnityAction<GameObject> OnSetTarget;
    }
}