using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public abstract class SimpleKeyProcessor : MonoBehaviour
    {
        public KeyCode keyCode;

        public NGCharacter character;

        private void Start()
        {
            if (character == null)
            {
                character = GetComponent<NGCharacter>();
            }

            if (character == null)
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                HandleKeyDown();
            }
        }

        public abstract void HandleKeyDown();
    }
}