using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [Obsolete("StrafeDirectionSetter is deprecated.  please use Example_StrafeTarget (which uses NGCharacter.SetStrafeTarget/ClearStrafeTarget) instead.")]
    public class StrafeDirectionSetter : MonoBehaviour
    {
        public NGCharacter character;

        public Transform strafeTarget;
        public KeyCode keyToggle = KeyCode.Q;

        public bool directionSetterActive = false;

        public Transform cameraTransform = null;

        CustomStrafeIVPP ivpp;

        public class CustomStrafeIVPP : NGInputVectorPostProcessorBase
        {
            public Transform camTransform;
            public override void PostProcessInputVector(NGInputSchemeBase inputScheme, ref Vector3 inputVectorLocalSpace)
            {
                // Get Cam-relative vector
                Vector3 forward = Vector3.ProjectOnPlane(camTransform.forward, Vector3.up);

                //Rotate our input vector relative to the camera
                inputVectorLocalSpace = Quaternion.FromToRotation(Vector3.forward, forward) * inputVectorLocalSpace;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            if (character == null)
            {
                character = GetComponent<NGCharacter>();
            }
            ivpp = CustomStrafeIVPP.CreateInstance<CustomStrafeIVPP>(); 
            ivpp.camTransform = character.mxmTrajectoryGenerator.RelativeCameraTransform;
        }

        bool firstTime = true;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(keyToggle))
            {
                if (directionSetterActive)
                {
                    // disable
                    character.mxmTrajectoryGenerator.StrafeDirection = Vector3.forward;
                    character.mxmTrajectoryGenerator.RelativeCameraTransform = cameraTransform;
                    ivpp.postProcessorActive = false;
                }
                else
                {
                    if (firstTime)
                    {
                        List<NGInputVectorPostProcessorBase> pps = new List<NGInputVectorPostProcessorBase>(character.InputScheme.postProcessors);
                        pps.Add(ivpp);
                        character.InputScheme.postProcessors = pps.ToArray();
                        firstTime = false;
                    }
                    ivpp.postProcessorActive = true;
                    cameraTransform = character.mxmTrajectoryGenerator.RelativeCameraTransform;
                    character.mxmTrajectoryGenerator.RelativeCameraTransform = null;
                }
                directionSetterActive = !directionSetterActive;
            }

            if (directionSetterActive && (strafeTarget != null))
            {
                character.mxmTrajectoryGenerator.StrafeDirection = GetStrafeDirection(transform.position, strafeTarget.position);
            }
        }

        private Vector3 GetStrafeDirection(Vector3 character, Vector3 target)
        {
            Vector3 vec = target - character;
            return new Vector3(vec.x, 0, vec.z);
        }
    }
}