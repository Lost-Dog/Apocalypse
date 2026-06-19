using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threepeat
{
    public class NGDispatchAnimationEvents : MonoBehaviour
    {
        public AudioSource audioSource;

        public UnityEvent<NGFootstepEvent> onRightFootstep = new UnityEvent<NGFootstepEvent>();
        public UnityEvent<NGFootstepEvent> onLeftFootstep = new UnityEvent<NGFootstepEvent>();
        public UnityEvent<NGFootstepEvent> onAnyFootstep = new UnityEvent<NGFootstepEvent>();

        public bool playRawAudioEvents = false;

        public const int RIGHT_FOOT = 1;
        public const int LEFT_FOOT = 2;

        public float minTimeBetweenSameFootEvents = 0.25f;

        public bool debugMode = false;

        public void Awake()
        {
            if ((audioSource == null) && debugMode)
            {
                Debug.LogWarningFormat("audioSource not set in DispatchAnimationEvents GameObject( {0} )", this.name);
            }
        }

        private float lastLeft = 0f;
        private float lastRight = 0f;
        public void NGFootstepEvent(AnimationEvent evt)
        {
            NGFootstepEvent step = new Threepeat.NGFootstepEvent
            {
                footIndex = evt.intParameter,
                type = Threepeat.NGFootstepEvent.FootstepType.Step,
                duration = evt.floatParameter,
                customTypeString = evt.stringParameter
            };

            if (evt.stringParameter.Length > 0)
            {
                string lower = evt.stringParameter.ToLower();
                if (lower.StartsWith("scu")) {
                    step.type = Threepeat.NGFootstepEvent.FootstepType.Scuff;
                }
                else if (lower.StartsWith("sli"))
                {
                    step.type = Threepeat.NGFootstepEvent.FootstepType.Slide;
                }
                else if (lower.StartsWith("cus"))
                {
                    step.type = Threepeat.NGFootstepEvent.FootstepType.Custom;
                }
            }

            int whichFoot = evt.intParameter;

            //Debug.LogFormat("received FootstepEvent event: {0}", whichFoot == LEFT_FOOT ? "left" : "right");
            if (whichFoot == LEFT_FOOT)
            {
                step.timeSinceLastSameFootEvent = Time.time - lastLeft;
                if (step.timeSinceLastSameFootEvent > minTimeBetweenSameFootEvents)
                {
                    if (onLeftFootstep != null)
                    {
                        onLeftFootstep.Invoke(step);
                    }
                    if (onAnyFootstep != null)
                    {
                        onAnyFootstep.Invoke(step);
                    }
                    lastLeft = Time.time;
                    //Debug.LogFormat("FOOT STEP - {0}", whichFoot == LEFT_FOOT ? "left" : "right");
                }
            }
            else if ((whichFoot == RIGHT_FOOT) && (onRightFootstep != null))
            {
                step.timeSinceLastSameFootEvent = Time.time - lastRight;
                if (step.timeSinceLastSameFootEvent > minTimeBetweenSameFootEvents)
                {
                    if (onRightFootstep != null)
                    {
                        onRightFootstep.Invoke(step);
                    }
                    if (onAnyFootstep != null)
                    {
                        onAnyFootstep.Invoke(step);
                    }

                    lastRight = Time.time;
                    //Debug.LogFormat("FOOT STEP - {0}", whichFoot == LEFT_FOOT ? "left" : "right");
                }
            }
        }

        public void NGRawAudioEvent(AnimationEvent evt)
        {
            if (playRawAudioEvents && (audioSource != null))
            {
                audioSource.PlayOneShot((AudioClip)evt.objectReferenceParameter, 
                        (evt.floatParameter > 0) ? Mathf.Min(1.0f, evt.floatParameter) : 1.0f );
            }
        }
    }
}