using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threepeat
{
    [Serializable]
    public class MMCAnimationAction
    {

        public ActionType actionType = ActionType.BaseEvent;
        public ActionSubType actionSubType = ActionSubType.None;
        public ActionSubType2 actionSubType2 = ActionSubType2.None;

        public bool lockToStartingRotation = false;

        public float timeStartNormalized = -1f;
        public float timeStopNormalized = -1f;

        public float timeFadeInGameTime = 0.1f;
        public float timeFadeOutGameTime = 0.1f;

        public UnityEvent<MMCAnimationAction> onStart;
        public UnityEvent<MMCAnimationAction> onStop;


        // this will be autopopulated prior to MMLC calling onStart/onStop
        [HideInInspector]
        public int clipNameHash = -1;

        public enum ActionType
        {
            BaseEvent,      // just causes onStart and onStop to be fired at appropriate time, like
                            // an AnimationEvent.  This is useful if you want to avoid building a custom
                            // AnimationEvent handler and just want specific methods fired (e.g. for 
                            // VFX/SFX)
            IKEvent
        }

        public enum ActionSubType
        {
            None = 0,
            [InspectorName("IK - Left Hand")]
            IK_HAND_LEFT = 1,
            [InspectorName("IK - Right Hand")]
            IK_HAND_RIGHT = 2,
            [InspectorName("IK - Both Hands")]
            IK_HAND_BOTH = 3,
            [InspectorName("IK - Left Foot")]
            IK_FOOT_LEFT = 4,
            [InspectorName("IK - Right Foot")]
            IK_FOOT_RIGHT = 8,
            [InspectorName("IK - Both Feet")]
            IK_FOOT_BOTH = 12,
        }

        public enum ActionSubType2 
        { 
            None = 0,
            [InspectorName("Target - Wall")]
            TARGET_TYPE_WALL = 1,
            [InspectorName("Target - Ledge")]
            TARGET_TYPE_LEDGE = 2,
            [InspectorName("Target - Ground Surface")]
            TARGET_TYPE_GROUND_SURFACE = 4
        }

    } // class MMCAnimationAction

} // namespace Threepeat