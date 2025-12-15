using System;
using UnityEngine;

namespace JUTPS.InteractionSystem.Interactables
{
    /// <summary>
    /// The base for interactible objects.
    /// </summary>
    public class JUInteractable : MonoBehaviour
    {
        private Collider _collider;

        /// <summary>
        /// If true, the <see cref="JUInteractionSystem"/> can interact with this object.
        /// </summary>
        public bool InteractionEnabled;

        /// <summary>
        /// This object center, based on collider bounds if have.
        /// </summary>
        public Vector3 SelfCenter
        {
            get
            {
                if (!_collider)
                    return transform.position;

                return _collider.bounds.center;
            }
        }

        /// <summary>
        /// Create Instance.
        /// </summary>
        protected JUInteractable()
        {
        }

        protected virtual void Start()
        {
            _collider = GetComponent<Collider>();
        }

        /// <summary>
        /// Return true if the object can be interacted.
        /// </summary>
        public virtual bool CanInteract(JUInteractionSystem interactionSystem)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called if the object was interacted.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Interact()
        {
        }
    }
}