using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{

    public class Example_SwitchInputSchemeToInputSystemOnKey : MonoBehaviour
    {
        public KeyCode activationKey = KeyCode.E;

        [Tooltip("If left blank, the default InputManager scheme will be used.")]
        public NGInputScheme_InputSystem targetScheme;

        [Tooltip("If left null, controls will be left/right move on world X-axis and up/down moves on world Z-axis.")]
        public Transform camTransform;

        private NGCharacter character;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                if (targetScheme == null)
                {
                    Debug.LogError("InputScheme not set on Example_SwitchInputSchemeToInputSystemOnKey component.");
                    return;
                }
                if (character == null)
                {
                    character = GetComponent<NGCharacter>();
                }

                character.SetInputScheme(targetScheme, camTransform);
            }
        }
    }

}