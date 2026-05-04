using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [DefaultExecutionOrder(-100)]
    public class Example_OverrideStrafeToBeActiveOnHoldDown : MonoBehaviour
    {
        private NGCharacter character;
        private bool initialized = false;

        private const string StrafeOnHoldActionName = "STRAFE_ON_HOLD";

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            if ((character == null) && (transform.parent != null))
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }

            character.onInputSchemeChanged.AddListener(OnInputSchemeChanged);
            character.RegisterContextualAction(StrafeOnHoldActionName, CanPerformAction, PerformAction);
        }

        public bool CanPerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            return (character.currentState == NGCharacter.CharacterState.Locomotion) &&
                        character.controllerWrapper.IsGrounded && !character.mxmAnimator.IsEventPlaying;
        }

        public ContextualAction.Propagation PerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            // Insert code here to start doing whatever this action does.

            if (trigger == ContextualActionTrigger.TriggeringCondition.InputTriggered)
            {
                character.Strafing = true;
            }
            else if (trigger == ContextualActionTrigger.TriggeringCondition.InputReleased)
            {
                character.Strafing = false;
                return ContextualAction.Propagation.StopPropagationForAllSuccessiveEvents;
            }

            // Whether to allow event processing to continue to the next contextual action in the stack for
            // this trigger.  For example, if you wish the player character to both jump and put their hands 
            // in the air every time they press down the jump action key, you could put the "PutHandsInAir"
            // contextual action ahead of "Jump" in the ContextualActionProcessor for the given key, and have
            // the PutHandsInAir return ContextualAction.Propagation.ContinuePropagation to allow "Jump" to be fired.
            //
            // StopPropagation terminates all event processing for the given action (trigger, hold, or release)
            //
            // StopPropagationForAllSuccessiveEvents terminates all future events related to this specific event, so 
            // e.g. if the current triggering condition is Trigger, all hold and release event handling would be 
            // suppressed until the next time this action is triggered.
            return ContextualAction.Propagation.ContinuePropagation;
        }


        private void OnDestroy()
        {
            character.onInputSchemeChanged.RemoveListener(OnInputSchemeChanged);
            character.UnregisterContextualAction(StrafeOnHoldActionName);
        }

        private void OnInputSchemeChanged(bool initial)
        {
            if (!character.InputScheme.IsInputDriven())
            {
                // NPC, nothing to do here.
                return;
            }
            NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)character.InputScheme;

            if (scheme.keyProcessorStrafeToggle == null)
            {
                return;
            }

            List<ContextualActionTrigger> triggers = new List<ContextualActionTrigger>(scheme.keyProcessorStrafeToggle.Triggers);

            if (!NGCharacter.ActionNamesMatch(triggers[0].actionName, StrafeOnHoldActionName))
            {
                // parkour's not at top of this trigger, let's add it:
                triggers.Insert(0, new ContextualActionTrigger(StrafeOnHoldActionName, ContextualActionTrigger.TriggeringCondition.InputReleased));
                triggers.Insert(0, new ContextualActionTrigger(StrafeOnHoldActionName));
                scheme.keyProcessorStrafeToggle.Triggers = triggers.ToArray();
            }

            initialized = true;
        }

    } // class
} // namespace