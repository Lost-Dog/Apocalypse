using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat {
    public abstract class MMCEventBehaviorBase //: ScriptableObject
    {
        public NGCharacter character;
        public MxMGameplay.NGCharacterControllerWrapper controllerWrapper;

        public abstract string GetBehaviorUniqueName();

        // lower value => higher priority 
        public virtual int GetDefaultPriority()
        {
            return 100;
        }

        public void SetReferences(NGCharacter pcharacter, MxMGameplay.NGCharacterControllerWrapper pwrapper)
        {
            this.character = pcharacter;
            this.controllerWrapper = pwrapper;
        }

        public virtual bool CanPerformEvent()
        {
            return true;
        }

        // Execute the event: It can be expected that this will be called directly after CanPerformEvent
        // such that all necessary objects (e.g. frontedge/backedge) are properly set and unchanged from
        // that execution.
        public virtual void ExecuteEvent(ref NGMxMEventDef eventToExecute, AnimationClip forceClip = null)
        {
            if (forceClip != null)
            {
                character.mxmAnimator.BeginEvent(eventToExecute, forceClip);
                if (!character.mxmAnimator.IsEventPlaying)
                {
                    character.mxmAnimator.BeginEvent(eventToExecute);
                }
            }
            else
            {
                character.mxmAnimator.BeginEvent(eventToExecute);
            }
        }

        public virtual void SetEventContacts(ref NGMxMEventDef eventToExecute)
        {

        }

    }
}