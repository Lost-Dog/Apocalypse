using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventBehaviorParkour_FenceClearReallyHigh : MMCEventBehaviorParkour_Mantle
    {
        public override bool CanPerformEvent()
        {
            return CanPerformMantleOrReallyHighMantle(true, false);
        }

        // Execute the event: It can be expected that this will be called directly after CanPerformEvent
        // such that all necessary objects (e.g. frontedge/backedge for parkour) are properly set and
        // unchanged from that execution.

        public override string GetBehaviorUniqueName()
        {
            return "parkour_vaultfenceclear_reallyhigh";
        }

        public override int GetDefaultPriority()
        {
            return 10;
        }

    }
}