using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class ContextualActionProcessor
    {
        public bool enabled = true;

        public ContextualActionProcessor(NGCharacter pCharacter, ContextualActionTrigger[] pTriggers)
        {
            character = pCharacter;
            Triggers = pTriggers;
        }

        protected ContextualActionProcessor()
        {
        }

        public ActionTriggerState state = new ActionTriggerState();

        protected NGCharacter character;
        
        protected bool haveHoldDownConditionInList = false;
        protected int holdDownIndex = -1;
        protected float holdDownTimeThreshold = 0f;
        protected Coroutine currentHoldAction = null;

        protected int firstReleaseConditionIndex = -1;
        
        protected ContextualActionTrigger[] triggers = { };
        protected ContextualAction[] cachedActions = null;


        // ContextualActionTrigger lists must be in descending order, first by chronological event, then execution
        // order within each event.  All event types are optional (e.g. a button can be mapped to just release behavior)
        //
        //  This means:
        //
        //          all zero or more InputTriggered events go first,
        //          then the optional single InputHeldDown event,
        //          then all zero or more InputReleased events.
        //
        public ContextualActionTrigger[] Triggers
        {
            get
            {
                return triggers;
            }

            set
            {
                triggers = value;
                haveHoldDownConditionInList = false;
                firstReleaseConditionIndex = -1;
                cachedActions = null;

                int ii = 0;
                foreach (ContextualActionTrigger trig in triggers)
                {
                    if (trig.trigger == ContextualActionTrigger.TriggeringCondition.TriggerDisabled)
                    {
                        this.enabled = false;
                        return;
                    }
                    
                    if (trig.trigger == ContextualActionTrigger.TriggeringCondition.InputHeldDown)
                    {
                        haveHoldDownConditionInList = true;
                        holdDownTimeThreshold = trig.floatParam;
                        holdDownIndex = ii;
                    }
                    else if ((trig.trigger == ContextualActionTrigger.TriggeringCondition.InputReleased) &&
                            (firstReleaseConditionIndex < 0))
                    {
                        firstReleaseConditionIndex = ii;
                    }
                    ii++;
                }

                this.enabled = true;
            }
        }

        public class ActionTriggerState
        {
            public bool keyIsDown = false;
            public bool ignoreKeyUp = false;
            public float timeDown = 0f;

            public void ResetOnTrigger()
            {
                keyIsDown = true;
                ignoreKeyUp = false;
                timeDown = Time.time;
            }
        }


        public void ProcessEvent_InputTrigger()
        {
            if (!enabled)
            {
                if (currentHoldAction != null)
                {
                    character.StopCoroutine(currentHoldAction);
                    currentHoldAction = null;
                }
                return;
            }
            state.ResetOnTrigger();
#if MMC_DEBUG_INPUTS
            Debug.Log("InputTrigger");
#endif
            if (cachedActions == null)
            {
                cachedActions = new ContextualAction[triggers.Length];
                for (int ii=0; ii < triggers.Length; ii++)
                {
                    cachedActions[ii] = character.GetContextualAction(triggers[ii].actionName);
                    if (cachedActions[ii] == null)
                    {
                        Debug.LogErrorFormat("character[{0}] Can't find ContextualAction[{1}]", character.name, triggers[ii].actionName);
                    }
                }
            }
            
            if (currentHoldAction != null)
            {
                character.StopCoroutine(currentHoldAction);
                currentHoldAction = null;
            }

            ContextualAction.Propagation propagation = ContextualAction.Propagation.ContinuePropagation;

            state.keyIsDown = true;

            // attempt to execute Triggered events
            for (int ii=0; ii < triggers.Length; ii++)
            {
                if (triggers[ii].trigger != ContextualActionTrigger.TriggeringCondition.InputTriggered)
                {
                    break;
                }
#if MMC_DEBUG_INPUTS
                Debug.LogFormat("-----Triggered: checking [{0}]", triggers[ii].actionName);
#endif

                if ((cachedActions[ii] != null) && cachedActions[ii].canPerformAction(ContextualActionTrigger.TriggeringCondition.InputTriggered))
                {
#if MMC_DEBUG_INPUTS
                    Debug.LogFormat("     Triggered: performing [{0}]", triggers[ii].actionName);
#endif
                    propagation = cachedActions[ii].performAction(ContextualActionTrigger.TriggeringCondition.InputTriggered);
                }
#if MMC_DEBUG_INPUTS
                else
                {
                    Debug.LogFormat("     Triggered: can't perform [{0}]", triggers[ii].actionName);
                }
#endif
                if (propagation != ContextualAction.Propagation.ContinuePropagation)
                {
                    break;
                }
            }

            if (propagation == ContextualAction.Propagation.StopPropagationForAllSuccessiveEvents)
            {
                state.ignoreKeyUp = true;
            }
            else if (haveHoldDownConditionInList)
            {
                // if propagation isn't stopped for all follow ons...
                currentHoldAction = character.StartCoroutine(HoldDownTimer(holdDownTimeThreshold));
            }
           
        }

        public void ProcessEvent_InputRelease()
        {
#if MMC_DEBUG_INPUTS
            if (!state.ignoreKeyUp)
            {
                Debug.LogFormat("InputRelease( {0} )", state.ignoreKeyUp);
            }
#endif

            if (!enabled)
            {
                if (currentHoldAction != null)
                {
                    character.StopCoroutine(currentHoldAction);
                    currentHoldAction = null;
                }
                return;
            }


            ContextualAction.Propagation propagation = ContextualAction.Propagation.ContinuePropagation;
            if (currentHoldAction != null)
            {
                character.StopCoroutine(currentHoldAction);
            }

            if (state.ignoreKeyUp || (firstReleaseConditionIndex < 0))
            {
                return;
            }

            for (int ii = firstReleaseConditionIndex; ii < triggers.Length; ii++)
            {
                if (triggers[ii].trigger != ContextualActionTrigger.TriggeringCondition.InputReleased)
                {
                    break;
                }

                if ((cachedActions != null) && (cachedActions[ii] != null) && cachedActions[ii].canPerformAction(ContextualActionTrigger.TriggeringCondition.InputReleased))
                {
#if MMC_DEBUG_INPUTS
                    Debug.LogFormat("     Released: performing [{0}]", triggers[ii].actionName);
#endif
                    propagation = cachedActions[ii].performAction(ContextualActionTrigger.TriggeringCondition.InputReleased);
                }
#if MMC_DEBUG_INPUTS
                else
                {
                    Debug.LogFormat("     Released: can't perform [{0}]", triggers[ii].actionName);
                }
#endif

                if (propagation != ContextualAction.Propagation.ContinuePropagation)
                {
                    break;
                }
            }


        }

        IEnumerator HoldDownTimer(float timeThresh)
        {
#if MMC_DEBUG_INPUTS
            if (!state.ignoreKeyUp)
            {
                Debug.Log("InputHoldDown");
            }
#endif
            ContextualAction.Propagation propagation = ContextualAction.Propagation.ContinuePropagation;

            yield return new WaitForSeconds(holdDownTimeThreshold);
            if (enabled && !state.ignoreKeyUp && (cachedActions[holdDownIndex] != null))
            {
                if (cachedActions[holdDownIndex].canPerformAction(ContextualActionTrigger.TriggeringCondition.InputHeldDown))
                {
#if MMC_DEBUG_INPUTS
                    Debug.LogFormat("     Held: performing [{0}]", cachedActions[holdDownIndex].actionName);
#endif
                    propagation = cachedActions[holdDownIndex].performAction(ContextualActionTrigger.TriggeringCondition.InputHeldDown);
                }
#if MMC_DEBUG_INPUTS
                else
                {
                    Debug.LogFormat("     Held: can't perform [{0}]", cachedActions[holdDownIndex].actionName);
                }
#endif

                if (propagation == ContextualAction.Propagation.StopPropagationForAllSuccessiveEvents)
                {
                    state.ignoreKeyUp = true;
                }
            }
            currentHoldAction = null;
            yield return 0;
        }

    }
}