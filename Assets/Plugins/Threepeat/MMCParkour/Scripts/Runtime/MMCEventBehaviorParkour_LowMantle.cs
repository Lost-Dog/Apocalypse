using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventBehaviorParkour_LowMantle : MMCEventBehaviorParkour_LowVault
    {
        public override bool CanPerformEvent()
        {
            return CanPerformLowVaultOrMantle(false);
        }

        /*public override void ExecuteEvent(ref NGMxMEventDef eventToExecute)
        {
            base.ExecuteEvent(ref eventToExecute);
        }*/

        public override string GetBehaviorUniqueName()
        {
            return "parkour_mantlelow";
        }

        public override int GetDefaultPriority()
        {
            return 20;
        }

        public override void SetEventContacts(ref NGMxMEventDef eventToExecute)
        {
            base.SetEventContacts(ref eventToExecute);
        }
    }
}