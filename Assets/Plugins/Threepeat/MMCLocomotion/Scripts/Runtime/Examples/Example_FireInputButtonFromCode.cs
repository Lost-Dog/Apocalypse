using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [DefaultExecutionOrder(-100)]
    public class Example_FireInputButtonFromCode : MonoBehaviour
    {
        private NGCharacter character;
        private bool initialized = false;

        protected ContextualActionProcessor jumpParkourButton = null;

        public void FireJumpParkour()
        {
            if (jumpParkourButton != null)
            {
                jumpParkourButton.ProcessEvent_InputTrigger();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            if ((character == null) && (transform.parent != null))
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }

            character.onInputSchemeChanged.AddListener(OnInputSchemeChanged);
        }

        private void OnDestroy()
        {
            character.onInputSchemeChanged.RemoveListener(OnInputSchemeChanged);
        }

        private void OnInputSchemeChanged(bool initial)
        {
            if (!character.InputScheme.IsInputDriven())
            {
                // NPC, nothing to do here.
                return;
            }
            jumpParkourButton = null;

            NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)character.InputScheme;

            if (scheme.keyProcessorJumpParkour == null)
            {
                return;
            }

            jumpParkourButton = scheme.keyProcessorJumpParkour;

            initialized = true;
        }

    } // class
} // namespace