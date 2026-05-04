using MxM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public abstract class NGInputSchemeBase : ScriptableObject
    {
        [Header("General Settings")]
        public bool doEnvironmentAwareTrajectories = false;
        public float envAwareMinimumDistToObstacle = 0.5f;

        public LayerMask envAwareIgnoreLayers;

        [Tooltip("Min Time between each call GetInputVector in seconds.  So 0.1 would mean GetInputVector will be called ~10 times per second.  The higher this number, the less responsive the input vector will be, but also the less processing intensive the character will be.")]
        public float inputVectorChangeInterval = 0.05f;
        private float timeLastGetVectorCall = 0f;
        private Vector3 cachedVector = Vector3.zero;

        public NGCharacter character = null;
        public MxMTrajectoryGenerator mxmTrajectoryGenerator = null;

        [Header("Post-Processors")]
        public NGInputVectorPostProcessorBase[] postProcessors = null;


        public virtual void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen)
        {
            character = pCharacter;
            mxmTrajectoryGenerator = pTrajGen;
            for (int ii=0; ii < postProcessors.Length; ii++)
            {
                postProcessors[ii] = Instantiate(postProcessors[ii]);
            }
            //Debug.LogFormat("Firing up instance: {0}", this.GetInstanceID());
        }

        public abstract string GetSchemeName();


        protected abstract Vector3 GetInputVector();

        public virtual void Deactivate()
        {
        }

        // Returns whether this Input Scheme is Player-input driven
        public virtual bool IsInputDriven()
        {
            return false;
        }

        public Vector3 GetRawWorldSpaceInputVectorNoCache()
        {
            Vector3 rawInput = GetInputVector();


            if (mxmTrajectoryGenerator.RelativeCameraTransform == null)
            {
                PostProcessInputVector(ref rawInput);
                return rawInput;
            }
            
            Vector3 forward = Vector3.ProjectOnPlane(mxmTrajectoryGenerator.RelativeCameraTransform.forward, Vector3.up);

            PostProcessInputVector(ref rawInput);

            //Rotate our input vector relative to the camera
            rawInput = Quaternion.FromToRotation(Vector3.forward, forward) * rawInput;
            return rawInput;
        }

        private void PostProcessInputVector(ref Vector3 rawInput)
        {
            if (postProcessors == null)
            {
                return;
            }

            foreach (NGInputVectorPostProcessorBase pp in postProcessors)
            {
                if (pp.wasPostProcessorActive != pp.postProcessorActive)
                {
                    if (pp.postProcessorActive)
                    {
                        pp.Activate(this);
                        pp.wasPostProcessorActive = true;
                    }
                    else
                    {
                        pp.Deactivate(this);
                        pp.wasPostProcessorActive = false;
                    }
                }
                if (pp.postProcessorActive)
                {
                    pp.PostProcessInputVector(this, ref rawInput);
                }
            }
        }

        public Vector3 GetUpdatedInputVector()
        {
            if ((Time.time - timeLastGetVectorCall) >= inputVectorChangeInterval)
            {
                cachedVector = GetInputVector();
                timeLastGetVectorCall = Time.time;
            }
            Vector3 postProcVector = cachedVector;

            PostProcessInputVector(ref postProcVector);

            /*if (this.GetType().Name.Contains("Input"))
            {
                Debug.Log($"GetUpdatedInputVector now raw( {cachedVector.x}, {cachedVector.y}, {cachedVector.z} )");
            }*/
            return postProcVector; //cachedVector;
        }


        public bool AreInSameHierarchy(GameObject obj1, GameObject obj2)
        {
            Transform topParent1 = GetTopmostParent(obj1.transform);
            Transform topParent2 = GetTopmostParent(obj2.transform);

            return topParent1 == topParent2;
        }

        private Transform GetTopmostParent(Transform transform)
        {
            while (transform.parent != null)
            {
                transform = transform.parent;
            }
            return transform;
        }

        private float ENV_AWARE_RAYCAST_HIT_CHECK_INTERVAL = 0.25f;
        private float lastEnvAwareRaycastCheckTime = -1f;
        private RaycastHit[] cachedHits;

        protected float GetDesiredTrajectoryScale(Transform m_camTransform, Vector3 inputVector)
        {
            Vector3 forward = Vector3.ProjectOnPlane(m_camTransform.forward, Vector3.up);

            //Rotate our input vector relative to the camera
            Vector3 LinearInputVector = Quaternion.FromToRotation(Vector3.forward, forward) * inputVector;

            // Check for Obstacle
            float maxDist = 4.5f / 0.7071f; // ray is cast at 45 degree angle (MB-TODO: change to use cc max slope)
            float minDist = envAwareMinimumDistToObstacle; // how close you should get to walls... this is really character radius.

            float scale = 1.0f;

            Vector3 originPoint = character.transform.position + (Vector3.up * 0.3f);
            //if (Physics.Raycast(character.transform.position + Vector3.up * 0.3f, (LinearInputVector + Vector3.up).normalized, out hit, maxDist, -1, QueryTriggerInteraction.Ignore))
            if (Time.time > (lastEnvAwareRaycastCheckTime + ENV_AWARE_RAYCAST_HIT_CHECK_INTERVAL))
            {
                lastEnvAwareRaycastCheckTime = Time.time;
                cachedHits = Physics.RaycastAll(originPoint, (LinearInputVector + Vector3.up).normalized, maxDist, ~envAwareIgnoreLayers, QueryTriggerInteraction.Ignore);
            }
            

            if (cachedHits.Length > 0)
            {
                int validHitIndex = -1;
                // find first hit that doesn't share parentage with this character

                for (int ii = cachedHits.Length-1; ii >= 0; ii--)
                {
                    if (!AreInSameHierarchy(cachedHits[ii].collider.gameObject, character.gameObject))
                    {
                        validHitIndex = ii;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }


                if (validHitIndex >= 0)
                {
                    scale = Mathf.Clamp01(((cachedHits[validHitIndex].point-originPoint).magnitude - minDist) / (maxDist - minDist));
                }
            }

            //Return our desired velocity by multiplying our input by our max speed
            return scale;
        }

    }
}