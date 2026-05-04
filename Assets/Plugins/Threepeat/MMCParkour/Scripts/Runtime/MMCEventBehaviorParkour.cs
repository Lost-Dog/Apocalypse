using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public abstract class MMCEventBehaviorParkour : MMCEventBehaviorBase
    {
        public NGAbilityParkour parkour;
        public NGParkourSettings config;
        public MMCEventCheckCache eventCheckCache;


        // lower value => higher priority 
        public override int GetDefaultPriority()
        {
            return 100;
        }

        public void SetReferences(NGCharacter pcharacter, MMCEventCheckCache pcache, NGAbilityParkour pparkour, NGParkourSettings pparkourSettings)
        {
            base.SetReferences(pcharacter, pcharacter != null ? pcharacter.controllerWrapper : null);
            this.eventCheckCache = pcache;
            this.parkour = pparkour;
            this.config = pparkourSettings;
        }

        public override void SetEventContacts(ref NGMxMEventDef eventToExecute)
        {
            int contactCount = Mathf.Max(eventToExecute.ContactCountToMatch, eventToExecute.ContactCountToWarp);
            float playerAngle = 0f;

            //if (eventToRun.RotationWarpType != EEventWarpType.None)
            {
                playerAngle = controllerWrapper.transform.rotation.eulerAngles.y;
                if (parkour.playerRotation == NGAbilityParkour.PlayerRotationMethod.UseFrontAndBackEdges)
                {
                    playerAngle = Quaternion.LookRotation(parkour.backEdge - parkour.frontEdge).eulerAngles.y;
                }
            }

            //Debug.LogFormat("CONTACT_COUNT = {0}, playerAngle = {1}", contactCount, playerAngle);

            Vector3 frontEdgeContact = parkour.frontEdge - parkour.playerFeetOffsetFromTransform;

            if (config.applyModelSpecificMultipliers)
            {
                Vector3 origin = parkour.playerFeetWorldSpace;

                frontEdgeContact =
                        new Vector3(
                                Mathf.LerpUnclamped(origin.x, parkour.frontEdge.x, config.gaitMultiplier),
                                Mathf.LerpUnclamped(origin.y, parkour.frontEdge.y /*+ playerFeetOffsetFromTransform.y*/, config.heightMultiplier),
                                Mathf.LerpUnclamped(origin.z, parkour.frontEdge.z, config.gaitMultiplier));
            }

            eventToExecute.ClearContacts();
            if (contactCount == 0)
            {
                return;
            }
            else if (contactCount == 1)
            {
                if (parkour.debugMode)
                {
                    Debug.LogFormat("eventType: {0}", eventToExecute.NGEventType.ToString());
                }
                if ((eventToExecute.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear) ||
                    (eventToExecute.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_mantle) ||
                    (eventToExecute.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear_reallyhigh) ||
                        (eventToExecute.NGEventType == NGMxMEventDef.NGAnimationEventType.parkour_mantle_reallyhigh))
                {
                    //Vector3 normalVec = frontEdge - frontEdgeNormal;
                    eventToExecute.AddEventContact(frontEdgeContact, Quaternion.LookRotation(-parkour.frontEdgeNormal).eulerAngles.y);
                }
                else
                {
                    //available-feature: I have running and walking animated drops (if running, either hop down to the lower level or drop into a slide to slide off platform to the ground)
                    eventToExecute.AddEventContact(frontEdgeContact, playerAngle);
                }
            }
            else if (contactCount == 2)
            {
                eventToExecute.AddEventContact(frontEdgeContact, playerAngle);
                eventToExecute.AddEventContact(parkour.GetLandingSpot(eventToExecute), playerAngle);
            }
            else if (contactCount == 3)
            {
                eventToExecute.AddEventContact(frontEdgeContact, playerAngle);
                eventToExecute.AddEventContact(parkour.backEdge, playerAngle);
                eventToExecute.AddEventContact(parkour.GetLandingSpot(eventToExecute), playerAngle);
            }

        }

    }
}