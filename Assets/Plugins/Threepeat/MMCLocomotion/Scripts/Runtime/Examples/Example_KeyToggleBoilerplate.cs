using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Example_KeyToggleBoilerplate : MonoBehaviour
    {
        private bool functionEnabled = false;
        public KeyCode activationKey = KeyCode.Q;
        private NGCharacter character;

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            if ((character == null) && (transform.parent != null))
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                if (!functionEnabled)
                {
                    Debug.LogFormat("Enabling function {0}", this.GetType().Name);
                    ActivateFunction();
                }
                else
                {
                    DeactivateFunction();
                }
                functionEnabled = !functionEnabled;
            }
        }

        private void DeactivateFunction()
        {
            // Insert code here to stop doing whatever this toggled function does.
        }

        private void ActivateFunction()
        {
            // Insert code here to start doing whatever this toggled function does.
        }
    }
}