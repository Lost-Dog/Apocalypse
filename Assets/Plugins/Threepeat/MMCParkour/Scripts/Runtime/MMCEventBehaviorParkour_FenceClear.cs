using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventBehaviorParkour_FenceClear : MMCEventBehaviorParkour_Mantle
    {
        public override bool CanPerformEvent()
        {
            return CanPerformMantleOrReallyHighMantle(false, false);
        }

        // Execute the event: It can be expected that this will be called directly after CanPerformEvent
        // such that all necessary objects (e.g. frontedge/backedge for parkour) are properly set and
        // unchanged from that execution.

        public override string GetBehaviorUniqueName()
        {
            return "parkour_vaultfenceclear";
        }

        /*public override void ExecuteEvent(ref NGMxMEventDef eventToExecute)
        {
            //base.ExecuteEvent(ref eventToExecute);

            FireSpecificParkour fsp = character.GetComponent<FireSpecificParkour>();

            Debug.LogFormat($"firing fenceclear with clip {fsp.animClip.name}");
            character.mxmAnimator.BeginEvent(eventToExecute, fsp.animClip);
        }*/

        public override int GetDefaultPriority()
        {
            return 10;
        }

    }
}