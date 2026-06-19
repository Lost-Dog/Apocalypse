using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Example_ForceWalkOnKey : MonoBehaviour
    {
        protected bool behaviorEnabled = false;

        public KeyCode activationKeyCode = KeyCode.K;

        public NGCharacter character;

        // Start is called before the first frame update
        void Start()
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

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(activationKeyCode))
            {
                if (character == null)
                {
                    Debug.LogError("NGCharacter component not found, can't do anything.");
                    return;
                }

                if (behaviorEnabled)
                {
                    // disabling forced walk
                    character.movement.canRun = true;
                }
                else
                {
                    // enabling forced walk
                    character.movement.canRun = false;
                }
                behaviorEnabled = !behaviorEnabled;
            }

        }
    }
}