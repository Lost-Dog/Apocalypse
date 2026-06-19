using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{

    public class Example_SwitchInputSchemeToInputManagerOnKey : MonoBehaviour
    {
        public KeyCode activationKey = KeyCode.P;

        [Tooltip("If left blank, the default InputManager scheme will be used.")]
        public NGInputScheme_InputManager targetScheme;

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
                if (character == null)
                {
                    character = GetComponent<NGCharacter>();
                }

                character.SetInputScheme(targetScheme != null ? targetScheme : ScriptableObject.CreateInstance<NGInputScheme_InputManager>(), camTransform);
            }
        }
    }

}