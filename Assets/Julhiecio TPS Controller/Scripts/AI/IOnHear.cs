using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// Defines a property for objects that can listen sounds. Useful for AIs that have hear sensor.
    /// </summary>
    public interface IOnHear
    {
        /// <summary>
        /// Called when hear something.
        /// Pass the position of the sound source as <see cref="Vector3"/> and 
        /// the source (if have) as <see cref="GameObject"/>.
        /// </summary>
        event UnityAction<Vector3, GameObject> OnHear;
    }
}