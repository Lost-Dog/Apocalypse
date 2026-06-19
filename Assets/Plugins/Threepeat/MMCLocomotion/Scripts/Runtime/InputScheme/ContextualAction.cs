using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{


    public class ContextualAction
    {
        public enum Propagation
        {
            // This will stop event propagation for the current event (e.g. Trigger or Release)
            StopPropagation,

            // This will allow current event propagation to continue
            // (whatever ContextualAction is next in the list will be checked/attempted)
            ContinuePropagation,

            // This will stop event propagation for the current event and all downstream events related to it:
            // e.g. if input is triggered and this propagation is returned, all HeldDown and Released events
            // will be suppressed.  The next time the input is triggered, all functions will behave normally.
            StopPropagationForAllSuccessiveEvents
        }

        // this is the key that will be used to reference this action from InputSchemes.
        public delegate string GetUniqueActionNameDelegate();
    
        /* returns whether action is able to be performed (e.g. if action is "OpenDoor", it confirms whether you're 
         * standing in front of a door and have the means to open it.)
         */
        public delegate bool CanPerformActionDelegate(ContextualActionTrigger.TriggeringCondition trigger);

        /* Returns whether to stop propagation of the event/action.  True means stop propagation and False means propagate to continue firing the next action in the list.
         * For any multi-action button where you only want to perform the first available action, return true.
         * 
         * return True example(most common):  If action list is [Open Door, Parkour, Jump] you'd want to 
         *           return true from each of these contextual actions, since you wouldn't want to open the 
         *           door and then jump or try and do parkour with the same keypress.
         * 
         * return False only if you have a single button that needs to execute multiple simultaneous actions
         * with a single triggering of that button.
         */
        public delegate Propagation PerformActionDelegate(ContextualActionTrigger.TriggeringCondition trigger);

        public string actionName = "";
        public CanPerformActionDelegate canPerformAction = null;
        public PerformActionDelegate performAction = null;

        public ContextualAction(string pActionName, CanPerformActionDelegate pCanPerformAction, PerformActionDelegate pPerformAction)
        {
            this.actionName = pActionName;
            this.canPerformAction = pCanPerformAction;
            this.performAction = pPerformAction;
        }
    }
}