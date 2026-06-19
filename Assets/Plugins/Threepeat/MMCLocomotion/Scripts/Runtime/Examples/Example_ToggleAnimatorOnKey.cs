using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Threepeat
{
    public class Example_ToggleAnimatorOnKey : MonoBehaviour
    {
        private Animator animator;
        private NGCharacter character;

        public AvatarMask optionalAvatarMask;

        public string wantedState = null;

        private bool animatorEnabled = false;

        public KeyCode activationKey = KeyCode.Q;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            character = transform.parent.GetComponent<NGCharacter>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                if (!animatorEnabled)
                {
                    if (wantedState.Length > 0)
                    {
                        animator.Play(wantedState);
                    }
                    character.anim.SetControllerMask(optionalAvatarMask);

                    character.anim.BlendToAnimator(0.25f, false);
                }
                else
                {
                    character.anim.BlendFromAnimator();
                }
                animatorEnabled = !animatorEnabled;
            }
        }
    }
}