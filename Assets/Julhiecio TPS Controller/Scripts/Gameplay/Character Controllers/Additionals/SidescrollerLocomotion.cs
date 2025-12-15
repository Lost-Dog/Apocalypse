using JUTPS.JUInputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUTPS.ActionScripts
{

    [AddComponentMenu("JU TPS/Third Person System/Additionals/Sidescroller Locomotion")]
    public class SidescrollerLocomotion : JUTPSActions.JUTPSAction
    {
        public bool BlockHorizontalLocomotion = true;
        public bool UseVerticalInputToCrouch = true;
        public bool BlockZPosition = true;

        private float startZPosition;
        private void Start()
        {
            //startZPosition = transform.position.z;
        }
        private void Update()
        {
            if (BlockHorizontalLocomotion)
            {
                TPSCharacter.BlockVerticalInput = true;
            }

            if (BlockZPosition)
            {
                Vector3 velocity = rb.linearVelocity; velocity.z = 0;
                rb.linearVelocity = velocity;

                transform.position = new Vector3(transform.position.x, transform.position.y, startZPosition);
            }

            if (UseVerticalInputToCrouch == false) return;

            if (!TPSCharacter.Inputs)
                return;

            Vector2 moveAxis = TPSCharacter.Inputs.MoveAxis;

            //Crouch
            if (moveAxis.y < -0.2f && TPSCharacter.IsCrouched == false)
            {
                TPSCharacter.IsCrouched = true;
            }
            if (moveAxis.y > 0.2f && TPSCharacter.IsCrouched == true)
            {
                TPSCharacter.IsCrouched = false;
            }
        }
    }

}