using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Threepeat
{

    /* a while-holding-down behavior (like sprint or ADS):
    * 
    * public ContextualActionTrigger[] sprintHoldKey = 
    * {
    *      new ContextualActionTrigger("SprintStart"),
    *      new ContextualActionTrigger("SprintStop", ContextualActionTrigger.TriggeringCondition.InputReleased)
    * }
    * 
    * a toggle (like crouch or draw/holster weapon or enter/exit strafe):
    * 
    * public ContextualActionTrigger[] crouchToggleKey = 
    * {
    *      new ContextualActionTrigger("ToggleCrouch")
    * }
    * 
    * which could have an addition hold-down action (e.g. to go prone):
    * 
    * public ContextualActionTrigger[] crouchToggleKey = 
    * {
    *      new ContextualActionTrigger("ToggleCrouch"),
    *      new ContextualActionTrigger("GoProne", ContextualActionTrigger.TriggeringCondition.InputHeldDown, 0.5f)
    * }
    * 
    * Here you also have the choice to wait for button release to ToggleCrouch in order put 
    * priority on seeing if the player holds down the button long enough to go straight to prone:
    * 
    * public ContextualActionTrigger[] crouchToggleKey = 
    * {
    *      new ContextualActionTrigger("GoProne", ContextualActionTrigger.TriggeringCondition.InputHeldDown, 0.5f),
    *      new ContextualActionTrigger("ToggleCrouch", ContextualActionTrigger.TriggeringCondition.InputReleased)
    * }
    */


    [Serializable]
    public class ContextualActionTrigger //: UnityEngine.Object
    {
        [SerializeField] public string actionName;

        [SerializeField] public TriggeringCondition trigger;

        [SerializeField] public float floatParam = -1f;

        public ContextualActionTrigger(string actionName, TriggeringCondition trigger = TriggeringCondition.InputTriggered, float floatParam = -1f)
        {
            this.actionName = actionName;
            this.trigger = trigger;
            this.floatParam = floatParam;
        }


        //TODO: sort out how to handle stopping propagation of any further Triggering Conditions until after input is released...
        public enum TriggeringCondition
        {
            InputTriggered,             // startSprint, parkour, jump
            
            InputHeldDown,              // bigJump
                                        //InputDoubleTap,
            
            InputReleased,              // stopSprint

            Other,                       // Only useful when triggering an action from code.  All other condition types are
                                        // still available, but e.g. you want to capture when the action being performed
                                        // is a reaction vice an input-driven choice, this value will cover that case.
            
            TriggerDisabled             // put this at the top of the stack to disable a context action processor
        }


    }
}