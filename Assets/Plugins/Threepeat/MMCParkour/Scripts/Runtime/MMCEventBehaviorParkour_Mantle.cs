using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventBehaviorParkour_Mantle : MMCEventBehaviorParkour
    {
        public override bool CanPerformEvent()
        {
            //eventCheckCache.Clear();
            return CanPerformMantleOrReallyHighMantle(false, true);
        }

        protected bool CanPerformMantleOrReallyHighMantle(bool isReallyHigh = false, bool isMantle = true)
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

            // for regular mantle, just need (in order of complexity to allow most efficient early-out):

            // 1. vertical surface to place brace feet (waist/chest high)
            // 2. the ledge
            // 3. a clear path to it and on it.
            // 4. landing spot on ledge


            // for really high mantle:
            // same, except may be multiple brace spot checks from waist to head high.


            // CHECK #1 - vertical surface to place feet
            Vector3 chestHeight = parkour.playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * 0.7f;
            Vector3 playerOriginRef = chestHeight;

            bool chestLevelForwardClear =
                    //!Physics.Raycast(
                    !eventCheckCache.SphereCast(
                            (int)MMCEventCheckCache.CollisionCastEnum.ChestLevelFwd,
                            chestHeight,
                            0.1f,
                            controllerWrapper.transform.forward, //TransformDirection(Vector3.forward),
                            out hitForward,
                            FORWARD_DISTANCE,
                            parkour.layerMask,
                            QueryTriggerInteraction.Ignore);

            if (chestLevelForwardClear)
            {
                return false;
            }

            //  CHECK #1 A - for really high mantle
            Vector3 playerHead = parkour.playerFeetWorldSpace + Vector3.up * controllerWrapper.Height * config.headLevelObstacleCheckHeightFactor;


            if (isReallyHigh)
            {
                if (!eventCheckCache.SphereCast(//Physics.Raycast(
                    (int)MMCEventCheckCache.CollisionCastEnum.HeadLevelFwd,
                    playerHead,
                    0.15f,
                    controllerWrapper.transform.forward,
                    out hitForward,
                    FORWARD_DISTANCE,
                    parkour.layerMask,
                    QueryTriggerInteraction.Ignore))
                {
                    // missing head-height brace surface
                    return false;
                }
                playerOriginRef = playerHead;
            }

            // CHECK #2. the ledge
            float heightMultiplier = 0.6f;
            float downRayMultiplier = config.mantleCheckCharacterHeightFactor;
            float distToObject = hitForward.distance;

            Vector3 downVecToFindTopOrigin = hitForward.point + controllerWrapper.transform.forward * 0.15f;
            downVecToFindTopOrigin.y = parkour.playerFeetWorldSpace.y + controllerWrapper.Height * downRayMultiplier * 1.5f;

            RaycastHit hitDown;
            bool downRayHitSomething = false;
            if (eventCheckCache.SphereCast(
                    (int)MMCEventCheckCache.CollisionCastEnum.HighVaultMantleTopOriginDown,
                    downVecToFindTopOrigin,
                    0.15f,
                    Vector3.down,
                    out hitDown,
                    (controllerWrapper.Height * downRayMultiplier * 1.5f) - 0.2f, //controllerWrapper.Height * downRayMultiplier * 0.8f,
                    parkour.layerMask,
                    QueryTriggerInteraction.Ignore))
            {
                downRayHitSomething = true;
                if (parkour.debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: Object hit by down ray {0} (prior hit object was {1})", hitDown.collider.name, parkour.parkourCurrentVaultObject == null ? null : parkour.parkourCurrentVaultObject.name);
                }
                if ((parkour.parkourCurrentVaultObject != null && parkour.parkourCurrentVaultObject.GetInstanceID() != hitDown.collider.gameObject.GetInstanceID()))
                {
                    // need to make sure backEdge gets set properly
                }
                float hitHeightAbovePlayerFeet = hitDown.point.y - parkour.playerFeetWorldSpace.y;

                if (hitHeightAbovePlayerFeet >= config.mantleReallyHighThreshold)
                {
                    /*if (parkour.debugMode)
                    {
                        Debug.Log("Really High Mantle");
                    }*/
                    //parkour.parkourCurrentVaultType = "reallyHighMantle";
                    if (!isReallyHigh)
                    {
                        return false;
                    }
                }

                parkour.parkourCurrentVaultObject = hitDown.collider.gameObject;
            }
            else
            {
                if (parkour.debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: DOWNHIT MISSED: mult( {0} )", downRayMultiplier);
                }

                // MAX
                return false;
            }

            //TODO: find actual front and back by using ray a foot below the hitDown.y
            parkour.frontEdge = hitForward.point; // x and z values are final, still need y-val for top of the object
            parkour.frontEdgeNormal = hitForward.normal;
            Vector3 vecPlayerToCover = hitForward.point - playerOriginRef;
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
                if (cap.eventType == 
                            (isReallyHigh ? 
                                    NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear_reallyhigh :
                                    NGMxMEventDef.NGAnimationEventType.parkour_vaultfenceclear))
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
                            (int)MMCEventCheckCache.CollisionCastEnum.OneFootOverFwdHigh,
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
                    //TODO: need to check distance to hit to see if mantling is even possible.

                    if (!isMantle)
                    {
                        return false;
                    }
                }

                // cast second one down past the edge to see if it's actually a vault

                
                if (eventCheckCache.SphereCast(
                    (int)MMCEventCheckCache.CollisionCastEnum.HighVaultMantleTopDownDeepCheck,
                    downVecToFindTopOrigin + controllerWrapper.transform.forward * (direction.magnitude + 0.2f),
                    0.15f,
                    Vector3.down,
                    out hitDown,
                    controllerWrapper.Height * downRayMultiplier * 0.8f,
                    parkour.layerMask,
                    QueryTriggerInteraction.Ignore))
                {
                    canBeOver = false;
                    if (!isMantle)
                    {
                        return false;
                    }
                }
            }

            // CHECK #3. a clear path to it and on it.
            RaycastHit hitForward2;

            //This was failing on really high mantles and fence clears: Vector3 testvec = downVecToFindTopOrigin - playerHead;
            Vector3 testvec = Vector3.up*(downVecToFindTopOrigin.y - playerHead.y);
            Vector3 oneFootBackFromObstacle =
                    new Vector3(
                        hitForward.point.x,
                        playerHead.y,
                        hitForward.point.z
                        ) - controllerWrapper.transform.forward*0.3f;

            if (eventCheckCache.SphereCast(//Physics.Raycast(
                (int)MMCEventCheckCache.CollisionCastEnum.CeilingCollisionCheck,
                oneFootBackFromObstacle,
                0.15f,
                testvec,
                out hitForward2,
                testvec.magnitude,
                parkour.layerMask,
                QueryTriggerInteraction.Ignore))
            {
                if (parkour.debugMode)
                {
                    Debug.LogFormat("PARKOUR_DEBUG: ceiling check failed");
                }
                return false;
            }

            /*
            if (canBeOver && !isMantle)
            {
                return true;
            }
            else if (!canBeOver && )*/

            // CHECK #4. landing spot on ledge: Vault vs. Mantle check

            return canBeOver != isMantle;
        }

        // Execute the event: It can be expected that this will be called directly after CanPerformEvent
        // such that all necessary objects (e.g. frontedge/backedge for parkour) are properly set and
        // unchanged from that execution.
 
        public override string GetBehaviorUniqueName()
        {
            return "parkour_mantle";
        }

        public override int GetDefaultPriority()
        {
            return 10;
        }

    }
}