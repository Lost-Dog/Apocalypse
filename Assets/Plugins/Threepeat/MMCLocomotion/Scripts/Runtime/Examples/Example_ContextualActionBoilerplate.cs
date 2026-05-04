using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threepeat
{
    public class Example_ContextualActionBoilerplate : MonoBehaviour
    {
        public KeyCode fireActionKey = KeyCode.Q;

        [Tooltip("If true, this action will be inserted ")]
        public bool alsoInsertThisActionIntoJumpActionKey = false;
        private NGCharacter character;

        private bool initialized = false;

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            if ((character == null) && (transform.parent != null))
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }

            character.RegisterContextualAction(GetUniqueActionName(), CanPerformAction, PerformAction);
            if (alsoInsertThisActionIntoJumpActionKey)
            {
                character.onInputSchemeChanged.AddListener(OnInputSchemeChanged);
            }
        }

        private void OnDestroy()
        {
            character.onInputSchemeChanged.RemoveListener(OnInputSchemeChanged);
            character.UnregisterContextualAction(GetUniqueActionName());
        }

        private void OnInputSchemeChanged(bool initial)
        {
            InjectActionIntoJumpKey();
            initialized = true;
        }

        private void InjectActionIntoJumpKey()
        {
            if (!character.InputScheme.IsInputDriven())
            {
                // NPC, nothing to do here.
                return;
            }
            NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)character.InputScheme;

            if (scheme.keyProcessorJumpParkour == null)
            {
                return;
            }

            List<ContextualActionTrigger> triggers = new List<ContextualActionTrigger>(scheme.keyProcessorJumpParkour.Triggers);

            if (!NGCharacter.ActionNamesMatch(triggers[0].actionName, GetUniqueActionName()))
            {
                // parkour's not at top of this trigger, let's add it:
                triggers.Insert(0, new ContextualActionTrigger(GetUniqueActionName()));
                scheme.keyProcessorJumpParkour.Triggers = triggers.ToArray();
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (!initialized)
            {
                OnInputSchemeChanged(false);
            }

            if (Input.GetKeyDown(fireActionKey) && CanPerformAction(ContextualActionTrigger.TriggeringCondition.InputTriggered))
            {
                PerformAction(ContextualActionTrigger.TriggeringCondition.InputTriggered);
            }
        }

        public string GetUniqueActionName()
        {
            // Use any uniquely descriptive string to describe this action.  
            return this.GetType().Name;
        }

        public bool CanPerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            // Return whether this action can be performed (e.g. if the action requires the character
            // to be grounded, you would:
            // return character.controllerWrapper.IsGrounded;
            return true;
        }

        public ContextualAction.Propagation PerformAction(ContextualActionTrigger.TriggeringCondition trigger)
        {
            // Insert code here to start doing whatever this action does.
            Debug.LogFormat("action executed: {0}", this.GetType().Name);

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
    }
}