using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Threepeat
{
    public class Example_DefaultToWalk_KeyToRun_InputSystem : MonoBehaviour
    {
#if (ENABLE_INPUT_SYSTEM)

        public InputActionAsset inputActionAsset;
        public string inputActionMapActive = "Player";
        public string fireActionName = "holdToRun";

        private PlayerInput playerInput = null;
        private InputAction actionFire;

        private ContextualActionProcessor keyProcessorFire;
        private Dictionary<InputAction, ContextualActionProcessor> inputCaps = new Dictionary<InputAction, ContextualActionProcessor>();

        [Tooltip("If true, this action will be inserted ")]
        public bool overrideSprintKey = false;
        private NGCharacter character;

        private bool initialized = false;

        public void ConnectCAPToInput(ContextualActionProcessor cap, InputAction act)
        {
            inputCaps.Add(act, cap);
        }

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();
            if ((character == null) && (transform.parent != null))
            {
                character = transform.parent.GetComponent<NGCharacter>();
            }

            character.RegisterContextualAction(GetUniqueActionName(), CanPerformAction, PerformAction);
            character.movement.canRun = false;
            if (overrideSprintKey)
            {
                character.onInputSchemeChanged.AddListener(OnInputSchemeChanged);
            }

            ContextualActionTrigger[] fireTriggers =
            {
                new ContextualActionTrigger(GetUniqueActionName()),
                new ContextualActionTrigger(GetUniqueActionName(), ContextualActionTrigger.TriggeringCondition.InputReleased)
            };
            keyProcessorFire = new ContextualActionProcessor(character, fireTriggers);
        }

        private void OnDestroy()
        {
            character.onInputSchemeChanged.RemoveListener(OnInputSchemeChanged);
            character.UnregisterContextualAction(GetUniqueActionName());
        }

        private void OnInputSchemeChanged(bool initial)
        {
            if (overrideSprintKey)
            {
                InjectActionIntoSprintKey();
            }
            playerInput = character.GetComponent<PlayerInput>();
            if (playerInput != null) {
                actionFire = playerInput.actions[fireActionName];
                ConnectCAPToInput(keyProcessorFire, actionFire);
            }
            else
            {
                inputCaps.Clear();
            }
            initialized = true;
        }

        protected void CheckInputs()
        {
            foreach (var actMapping in inputCaps)
            {
                if (actMapping.Key.triggered)
                {
                    actMapping.Value.ProcessEvent_InputTrigger();
                }
                else if (actMapping.Key.WasReleasedThisFrame())
                {
                    actMapping.Value.ProcessEvent_InputRelease();
                }
            }
        }

        private void InjectActionIntoSprintKey()
        {
            if (!character.InputScheme.IsInputDriven())
            {
                // NPC, nothing to do here.
                return;
            }
            NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)character.InputScheme;

            if (scheme.keyProcessorSprintHold == null)
            {
                return;
            }

            List<ContextualActionTrigger> triggers = new List<ContextualActionTrigger>(scheme.keyProcessorSprintHold.Triggers);

            if (!NGCharacter.ActionNamesMatch(triggers[0].actionName, GetUniqueActionName()))
            {
                // parkour's not at top of this trigger, let's add it:
                triggers.Insert(0, new ContextualActionTrigger(GetUniqueActionName(), ContextualActionTrigger.TriggeringCondition.InputReleased));
                triggers.Insert(0, new ContextualActionTrigger(GetUniqueActionName()));
                scheme.keyProcessorSprintHold.Triggers = triggers.ToArray();
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (!initialized)
            {
                OnInputSchemeChanged(false);
            }

            CheckInputs();

            /*if (Input.GetKeyDown(fireActionKey) && CanPerformAction(ContextualActionTrigger.TriggeringCondition.InputTriggered))
            {
                PerformAction(ContextualActionTrigger.TriggeringCondition.InputTriggered);
            }
            else if (Input.GetKeyUp(fireActionKey))
            {
                // stop action
                PerformAction(ContextualActionTrigger.TriggeringCondition.InputReleased);
            }*/
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
            
            if (trigger == ContextualActionTrigger.TriggeringCondition.InputTriggered)
            {
                Debug.LogFormat("run started");
                character.movement.canRun = true;
            }
            else if (trigger == ContextualActionTrigger.TriggeringCondition.InputReleased)
            {
                Debug.LogFormat("run stopped");
                character.movement.canRun = false;
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
#endif
    }
}