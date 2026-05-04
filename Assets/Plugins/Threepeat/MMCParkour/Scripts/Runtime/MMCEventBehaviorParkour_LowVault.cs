using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventBehaviorParkour_LowVault : MMCEventBehaviorParkour
    {
        public override bool CanPerformEvent()
        {
            return CanPerformLowVaultOrMantle(true);
        }

        protected bool CanPerformLowVaultOrMantle(bool isVault = true)
        {
            if (!controllerWrapper.IsGrounded)
            {
                if (parkour.debugMode)
                {
                    Debug.Log("ParkourCheck early-out, because not grounded.");
                }
                return false;
            }

            RaycastHit hitForward;
            float FORWARD_DISTANCE = parkour.GetForwardDistanceAndSetMovementState();

            //float maxDist = FORWARD_DISTANCE / 0.7071f; // ray is cast at 45 degree angle (MB-TODO: change to use cc max slope)
            Vector3 playerKneecaps = parkour.playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * config.kneeLevelObstacleCheckHeightFactor;

            bool kneeLevelForwardClear =
                //!Physics.Raycast(
                !eventCheckCache.SphereCast(
                        (int)MMCEventCheckCache.CollisionCastEnum.KneeLevelFwd,
                        playerKneecaps,
                        0.2f,
                        controllerWrapper.transform.forward,  //(charController.transform.forward + Vector3.up).normalized, 
                        out hitForward,
                        FORWARD_DISTANCE, //maxDist,
                        parkour.layerMask,
                        QueryTriggerInteraction.Ignore);

            //Debug.LogFormat("CHECKFORWARD: kneeLevelFwdClear {0}", kneeLevelForwardClear ? "CLEAR" : "BLOCKED");

            if (kneeLevelForwardClear)
            {
                return false;
            }

            // check for ramp:
            if (!parkour.CheckForObstacle_RampCheck(hitForward.distance))
            {
                if (parkour.debugMode)
                {
                    Debug.LogFormat("Parkour Check: Skipping (incline): distToKneeHit( {0} )", hitForward.distance);
                }
                return false;
            }
            else
            {
                if (parkour.debugMode)
                {
                    Debug.LogFormat("Parkour Check: Incline check OK: distToKneeHit( {0} )", hitForward.distance);
                }
            }

            // check vault vs. mantle
            float downRayMultiplier = 1.0f;
            Vector3 rayDirection = Vector3.forward;

            Vector3 downVecToFindTopOrigin = hitForward.point + controllerWrapper.transform.forward * 0.15f;
            downVecToFindTopOrigin.y = parkour.playerFeetWorldSpace.y + controllerWrapper.Height * downRayMultiplier * 1.5f;

            //TODO: do head-level forward spherecast to see if this is a gap that can be vaulted/mantled
            //TODO: if we end up mantling into a space we can't stand, should automatically crouch and adjust 
            // character controller height
            //bool headHit = false;

            RaycastHit hitForward2;

            Vector3 playerHead = parkour.playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * config.headLevelObstacleCheckHeightFactor;
            if (!eventCheckCache.SphereCast(//Physics.Raycast(
                (int)MMCEventCheckCache.CollisionCastEnum.HeadLevelFwd,
                playerHead,
                0.15f,
                controllerWrapper.transform.TransformDirection(rayDirection),
                out hitForward2,
                FORWARD_DISTANCE * 1.25f,
                parkour.layerMask,
                QueryTriggerInteraction.Ignore))
            {
                // this is a low vault/mantle for sure.
                downVecToFindTopOrigin.y = playerHead.y + 0.2f;
            }
            else
            {
                //headHit = true;
                return false;
            }

            RaycastHit hitDown;
            bool downRayHitSomething = false;
            if (eventCheckCache.SphereCast(
                    (int)MMCEventCheckCache.CollisionCastEnum.LowVaultMantleTopOriginDown,
                    downVecToFindTopOrigin,
                    0.15f,
                    Vector3.down,
                    out hitDown,
                    controllerWrapper.Height * downRayMultiplier * 0.8f,
                    parkour.layerMask,
                    QueryTriggerInteraction.Ignore))
            {
                downRayHitSomething = true;
                if (parkour.debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: Object hit by down ray {0} (prior hit object was {1})", hitDown.collider.name, parkour.parkourCurrentVaultObject == null ? null : parkour.parkourCurrentVaultObject.name);
                }
                float hitHeightAbovePlayerFeet = hitDown.point.y - parkour.playerFeetWorldSpace.y;

                if (hitHeightAbovePlayerFeet >= config.mantleReallyHighThreshold)
                {
                    return false;
                }

                parkour.parkourCurrentVaultObject = hitDown.collider.gameObject;
            }
            else
            {
                if (parkour.debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: DOWNHIT MISSED: mult( {0} )", downRayMultiplier);
                }
            }

            parkour.frontEdge = hitForward.point; // x and z values are final, still need y-val for top of the object
            parkour.frontEdgeNormal = hitForward.normal;
            Vector3 vecPlayerToCover = hitForward.point - playerKneecaps;
            bool success = parkour.PopulateFrontAndBackEdges(
                    hitForward,
                    downRayHitSomething ? hitDown : null,
                    vecPlayerToCover);

            float actualPlatformDepth = (parkour.frontEdge - parkour.backEdge).magnitude;
            float maxPlatformDepth = 1f;

            List<NGParkourSettings.ParkourCapability> potentialOvers = new List<NGParkourSettings.ParkourCapability>();

            bool canBeOver = false;
            float tmpPlatDepth;

            //TODO: add standing/running check and only use appropriate one

            //TODO: do this in Start and pre-compute all the max depths
            // figure-out over vs on:
            // 1. checking length and if length is > max for any of the over events in config then it's "on"
            foreach (NGParkourSettings.ParkourCapability cap in config.capabilities)
            {
                if (cap.eventType == NGMxMEventDef.NGAnimationEventType.parkour_vaultlow)
                {
                    tmpPlatDepth = parkour.GetPlatformDepthFromCapability(cap, parkour.movementState);
                    maxPlatformDepth = Mathf.Max(tmpPlatDepth, maxPlatformDepth);
                    if (tmpPlatDepth >= actualPlatformDepth)
                    {
                        potentialOvers.Add(cap);
                        canBeOver = true;
                    }
                }
            }

            // 2. if still possibly an over, cast ray
            if (canBeOver)
            {
                RaycastHit hitClearCheck;
                Vector3 ONE_FOOT_UP = Vector3.up * 0.3f;
                //TODO: cast ray 1ft above 
                Vector3 direction = parkour.backEdge - parkour.frontEdge;
                if (eventCheckCache.Raycast(
                            (int)MMCEventCheckCache.CollisionCastEnum.OneFootOverFwd,
                            parkour.frontEdge + ONE_FOOT_UP,
                            direction,
                            out hitClearCheck,
                            direction.magnitude,
                            parkour.layerMask,
                            QueryTriggerInteraction.Ignore))
                {
                    // ray 1 ft above platform from front to back hit something, so this can't be an over.
                    //Debug.Log("CANT-BE-OVER-REASON: 1ft-ray hit something");
                    canBeOver = false;
                }
            }/*
            else
            {
                Debug.LogFormat("CANT-BE-OVER-REASON: no potentials in config: actualPlatDepth( {0} ) frontEdge( {1} ), backEdge( {2} )",
                    actualPlatformDepth,
                    frontEdge,
                    backEdge);
            }*/

            // Get specific event

            /*if (canBeOver)
            {
                // This is actually an Over
                parkourType = properOverType;
                //Debug.LogFormat("PARKOURTYPE(Over): {0}", parkourType);
                eventToRun = GetEvent(parkourType, movementState);
            }
            else
            {
                // This is an On.
                parkourType = properOnType;
                //Debug.LogFormat("PARKOURTYPE(On): {0}", parkourType);
                eventToRun = GetEvent(parkourType, movementState);
            }*/

            // if canBeOver and isVault are true, good
            // if canBeOver and isVault are false, good
            // otherwise, no can do.
            return canBeOver == isVault;
        }

        // Execute the event: It can be expected that this will be called directly after CanPerformEvent
        // such that all necessary objects (e.g. frontedge/backedge for parkour) are properly set and
        // unchanged from that execution.
        /*public override void ExecuteEvent(ref NGMxMEventDef eventToExecute, AnimationClip forceClip = null)
        {
            base.ExecuteEvent(ref eventToExecute);
        }*/

        public override string GetBehaviorUniqueName()
        {
            return "parkour_vaultlow";
        }

        public override int GetDefaultPriority()
        {
            return 10;
        }

        public override void SetEventContacts(ref NGMxMEventDef eventToExecute)
        {
            base.SetEventContacts(ref eventToExecute);
        }
    }
}