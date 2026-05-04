using MxM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventIKManager : MonoBehaviour
    {
        public bool IKEnabled = true;
        private bool wasIKEnabled = false;

        public bool debugMode = false;

        [Header("Configuration")]
        protected Dictionary<string, EventDetails[]> animationClipEvents = new Dictionary<string, EventDetails[]>();

        private AnimationClip currentClip = null;
        private EventDetails[] currentClipEvents;

        [Header("Character-specific offsets")]
        public Vector3 leftHandIKOffsetLedge = new Vector3(0, -0.03f, -0.07f); //Vector3.zero;
        public Vector3 rightHandIKOffsetLedge = new Vector3(0, -0.03f, -0.07f); //Vector3.zero;
        public float leftFootIKOffsetWallZ = -0.2f;  //Vector3.zero;
        public float rightFootIKOffsetWallZ = -0.2f; // Vector3.zero;
        public float leftFootIKOffsetGroundY = 0.1f;
        public float rightFootIKOffsetGroundY = 0.1f;
        public float leftHandIKOffsetWallZ = -0.2f;
        public float rightHandIKOffsetWallZ = -0.2f;
        public float leftHandIKOffsetGroundY = 0f;
        public float rightHandIKOffsetGroundY = 0f;
        

        private Transform handLeft;
        private Transform handRight;
        private Transform footLeft;
        private Transform footRight;

        [Header("Play Mode Debugging Info")]
        public float IKWeight_RightHand = 0f;
        public float IKWeight_LeftHand = 0f;
        public float IKWeight_RightFoot = 0f;
        public float IKWeight_LeftFoot = 0f;



        [HideInInspector] public NGAbilityParkour parkour;
        protected MxMAnimator mxma;
        protected Animator animator;
        protected NGCharacter mmlcCharacter;

        public class EventDetails
        {
            public MMCAnimationAction act;

            public bool targetSet = false;
            public Vector3 targetPositionLH = Vector3.zero;
            public Vector3 targetPositionRH = Vector3.zero;
            public Vector3 targetPositionLF = Vector3.zero;
            public Vector3 targetPositionRF = Vector3.zero;

            //TODO: add Quaternion targetRotation and set/handling of it.
            public float fadeInStartNormalized = 0f;
            public float fadeInStopNormalized = 0f;
            public float fadeOutStartNormalized = 0f;
            public float fadeOutStopNormalized = 0f;
        }


        // Start is called before the first frame update
        void Start()
        {
            if (parkour == null)
            {
                parkour = transform.GetComponentInParent<NGAbilityParkour>();
            }
            if (parkour != null)
            {
                mmlcCharacter = parkour.GetComponent<NGCharacter>();
            }
            if (mxma == null)
            {
                mxma = GetComponent<MxMAnimator>();
            }
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

        }

        private bool haveZeroized = false;

        public void ClearEventDetails()
        {
            animationClipEvents.Clear();
        }

        public void AddClipEvents(AnimationClip currClip, MMCAnimationClipInfo clipInfo)
        {
            EventDetails[] tmpDetails = new EventDetails[clipInfo.actions.Length];
            //TODO: most of this needs to only be done once at startup.
            MMCAnimationAction act;
            for (int ii = 0; ii < clipInfo.actions.Length; ii++)
            {
                act = clipInfo.actions[ii];
                if (act.actionSubType == MMCAnimationAction.ActionSubType.None)
                {
                    continue;
                }
                tmpDetails[ii] = new EventDetails();
                tmpDetails[ii].act = act;
                tmpDetails[ii].fadeInStartNormalized = Mathf.Max(0f, act.timeStartNormalized - (act.timeFadeInGameTime / currClip.length));
                tmpDetails[ii].fadeInStopNormalized = act.timeStartNormalized;
                tmpDetails[ii].fadeOutStartNormalized = act.timeStopNormalized;
                tmpDetails[ii].fadeOutStopNormalized = Mathf.Min(1f, act.timeStopNormalized + (act.timeFadeOutGameTime / currClip.length));
            }

            if (!animationClipEvents.ContainsKey(currClip.name))
            {
                animationClipEvents.Add(clipInfo.clipName, tmpDetails);
            }
        }


        // return whether clip changed
        private bool CheckCurrentClip(AnimationClip currClip)
        {
            bool found = false;

            if (currClip == null)
            {
                if (currentClip != null)
                {
                    currentClip = null;
                    return true;
                }
                return false;
            }

            if ((currentClip == null) || !currClip.name.Equals(currentClip.name))
            {
                // clip changed
            }
            else
            {
                // clip is the same
                return false;
            }

            currentClip = currClip;

            EventDetails[] eventDetails;
            if (currClip != null)
            {
                found = animationClipEvents.TryGetValue(currClip.name, out eventDetails);
                if (found)
                {
                    currentClipEvents = eventDetails;
                    return true;
                }
            }

            currentClip = null;
            return true;

        }

        private void ZeroizeIK()
        {
            currentClip = null;
            currentClipEvents = null;
            haveZeroized = true;
            IKWeight_LeftFoot = IKWeight_LeftHand = IKWeight_RightFoot = IKWeight_RightHand = 0f;
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
        }

        private Vector3 RunHandLedge(Transform handPos, bool isLeft = false)
        {
            Vector3 rootToHand = handPos.position - parkour.transform.position;
            Vector3 proj = Vector3.Project(rootToHand, Vector3.Cross(Vector3.up, parkour.FrontEdgeNormal));

            float walkBackDistance = 1f;
            float sphereRadius = 0.15f;

            Vector3 sphereCastOrigin = parkour.FrontEdge + proj + Vector3.up * walkBackDistance;

            RaycastHit hit;
            Debug.DrawLine(sphereCastOrigin, sphereCastOrigin + Vector3.down * 3f * walkBackDistance, Color.white);
            Debug.DrawRay(parkour.FrontEdge, parkour.FrontEdgeNormal, Color.black);
            Debug.DrawLine(parkour.FrontEdge, parkour.FrontEdge + proj, Color.blue);

            if (Physics.SphereCast(
                    sphereCastOrigin,
                    sphereRadius,
                    Vector3.down,
                    out hit,
                    3f * walkBackDistance,
                    parkour.layerMask,
                    QueryTriggerInteraction.Ignore))
            {
                //Debug.Log("Hit!");

                Vector3 offsetSource = isLeft ? leftHandIKOffsetLedge : rightHandIKOffsetLedge;
                /*Vector3 offset =
                            new Vector3(
                                (-offsetSource.x * proj.normalized).magnitude,
                                offsetSource.y,
                                (-offsetSource.z * abilityParkour.FrontEdgeNormal.normalized).magnitude);*/

                Vector3 offset = parkour.transform.TransformVector(offsetSource);
                // hit!
                //Debug.LogFormat("Moving handPoint from {0} by {1}", hit.point.ToString("F2"), offset.ToString("F2"));
                return hit.point + offset;
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogFormat("Missed Ledge SphereCast({0})", isLeft ? "LH" : "RH");
                }
            }

            return parkour.FrontEdge;
        }

        private Vector3 RunIKWall(Transform handPos)
        {
            Vector3 rootToHand = handPos.position - parkour.transform.position;
            Vector3 proj = Vector3.Project(rootToHand, Vector3.Cross(Vector3.up, parkour.FrontEdgeNormal));

            float walkBackDistance = 1f;
            float sphereRadius = 0.15f;

            Vector3 sphereCastOrigin = handPos.position - parkour.transform.forward * walkBackDistance; //abilityParkour.FrontEdge + proj + Vector3.up * walkBackDistance;

            RaycastHit hit;
            Debug.DrawLine(sphereCastOrigin, sphereCastOrigin + parkour.transform.forward * -3f * walkBackDistance, Color.white);
            Debug.DrawRay(parkour.FrontEdge, parkour.FrontEdgeNormal, Color.black);
            Debug.DrawLine(parkour.FrontEdge, parkour.FrontEdge + proj, Color.blue);
            
            if (Physics.SphereCast(
                    sphereCastOrigin,
                    sphereRadius,
                    parkour.transform.forward,
                    out hit,
                    3f * walkBackDistance,
                    parkour.layerMask,
                    QueryTriggerInteraction.Ignore))
            {
                //Debug.Log("Hit!");

                //Debug.LogFormat("Moving handPoint from {0} by {1}", hit.point.ToString("F2"), offset.ToString("F2"));
                return hit.point;
            }
            else if (debugMode)
            {
                Debug.LogError("RunIKWall: Missed Ledge SphereCast!");
            }

            return parkour.FrontEdge;
        }


        private void GetTargetPosition(ref EventDetails details)
        {
            //float walkBackDistance = 0.15f;
            //float sphereRadius = 0.15f;

            Vector3 playerFwd = parkour.transform.forward;

            Vector3 sphereCastDirection = playerFwd;

            if (details.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_LEDGE)
            {
                //TODO: offset and sphere cast to find proper ledge hold for each hand
                switch (details.act.actionSubType)
                {
                    case MMCAnimationAction.ActionSubType.IK_HAND_LEFT:
                        //details.targetPositionLH = abilityParkour.FrontEdge;
                        details.targetPositionLH = RunHandLedge(handLeft, true);
                        break;
                    case MMCAnimationAction.ActionSubType.IK_HAND_RIGHT:
                        //details.targetPositionRH = abilityParkour.FrontEdge;
                        details.targetPositionRH = RunHandLedge(handRight);

                        break;
                    case MMCAnimationAction.ActionSubType.IK_HAND_BOTH:
                        details.targetPositionLH = parkour.FrontEdge;
                        details.targetPositionRH = parkour.FrontEdge;
                        break;
                }
                return;
            }
            else if (details.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_WALL)
            {
                // spherecast is good at player forward.

            }
            else if (details.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_GROUND_SURFACE)
            {
                sphereCastDirection = -parkour.transform.up;
            }

            if (details.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_GROUND_SURFACE)
            {
                details.targetPositionLH = new Vector3(handLeft.position.x, parkour.FrontEdge.y + leftHandIKOffsetGroundY, handLeft.position.z);
                details.targetPositionRH = new Vector3(handRight.position.x, parkour.FrontEdge.y + rightHandIKOffsetGroundY, handRight.position.z);

                details.targetPositionLF = new Vector3(footLeft.position.x, parkour.FrontEdge.y + leftFootIKOffsetGroundY, footLeft.position.z);
                details.targetPositionRF = new Vector3(footRight.position.x, parkour.FrontEdge.y + rightFootIKOffsetGroundY, footRight.position.z);
            }
            else
            {
                // wall
                details.targetPositionLH = RunIKWall(handLeft); //new Vector3(abilityParkour.FrontEdge.x, handLeft.position.y, abilityParkour.FrontEdge.z);
                details.targetPositionLF = new Vector3(details.targetPositionLH.x, handLeft.position.y, details.targetPositionLH.z) + parkour.transform.TransformVector(Vector3.forward * leftHandIKOffsetWallZ);
                details.targetPositionRH = RunIKWall(handRight); //new Vector3(abilityParkour.FrontEdge.x, handRight.position.y, abilityParkour.FrontEdge.z);
                details.targetPositionRH = new Vector3(details.targetPositionRH.x, handRight.position.y, details.targetPositionRH.z) + parkour.transform.TransformVector(Vector3.forward * rightHandIKOffsetWallZ);
                details.targetPositionLF = RunIKWall(footLeft);
                details.targetPositionLF = new Vector3(details.targetPositionLF.x, footLeft.position.y, details.targetPositionLF.z) + parkour.transform.TransformVector(Vector3.forward * leftFootIKOffsetWallZ);
                details.targetPositionRF = RunIKWall(footRight);
                details.targetPositionRF = new Vector3(details.targetPositionRF.x, footRight.position.y, details.targetPositionRF.z) + parkour.transform.TransformVector(Vector3.forward * rightFootIKOffsetWallZ);
            }
        }

        private void SetIKTargetPositions(EventDetails det)
        {
            Vector3 playerFwd = parkour.transform.forward;
            if ((det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_LEFT) ||
                (det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_BOTH))
            {

                animator.SetIKPosition(AvatarIKGoal.LeftHand, det.targetPositionLH); // + character.transform.TransformDirection(leftHandIKOffset));
            }

            if ((det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_RIGHT) ||
                (det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_BOTH))
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, det.targetPositionRH); // + character.transform.TransformDirection(rightHandIKOffset));
            }

            if ((det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_LEFT) ||
                (det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_BOTH))
            {

                animator.SetIKPosition(AvatarIKGoal.LeftFoot, det.targetPositionLF); // + character.transform.TransformDirection(leftHandIKOffset));
            }

            if ((det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_RIGHT) ||
                (det.act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_BOTH))
            {
                animator.SetIKPosition(AvatarIKGoal.RightFoot, det.targetPositionRF); // + character.transform.TransformDirection(rightHandIKOffset));
            }

        }

        private void SetIKWeights(MMCAnimationAction act, float t)
        {
            if ((act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_LEFT) ||
                (act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_BOTH))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, t);
                IKWeight_LeftHand = t;
            }

            if ((act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_RIGHT) ||
                (act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_BOTH))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, t);
                IKWeight_RightHand = t;
            }

            if ((act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_LEFT) ||
    (act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_BOTH))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, t);
                IKWeight_LeftFoot = t;
            }

            if ((act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_RIGHT) ||
                (act.actionSubType == MMCAnimationAction.ActionSubType.IK_FOOT_BOTH))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, t);
                IKWeight_RightFoot = t;
            }

        }

        private void OnAnimatorIK(int layerIndex)
        {
            //Debug.LogFormat("OnAnimatorIK called! {0}", layerIndex);
            if ((mmlcCharacter != null) && (mmlcCharacter.IKLayer >= 0) && (layerIndex != mmlcCharacter.IKLayer))
            {
                return;
            }
            // Handle overall IK enable/disable
            if (!IKEnabled)
            {
                if (wasIKEnabled)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                    wasIKEnabled = false;
                }
                return;
            }
            else if (!wasIKEnabled && (handLeft == null))
            {
                // enabling IK the first time, get Bone Transforms
                handLeft = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                handRight = animator.GetBoneTransform(HumanBodyBones.RightHand);
                footLeft = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                footRight = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            }
            wasIKEnabled = true;

            // end overall IK enable/disable handling

            if (!mxma.IsEventPlaying)
            {
                if (!haveZeroized)
                {
                    ZeroizeIK();
                }
                return;
            }

            float AnimationTime = mxma.ChosenPose.Time + mxma.TimeSinceMotionChosen;
            AnimationClip currClip = mxma.CurrentAnimData.Clips[mxma.ChosenPose.PrimaryClipId]; // .AnimId for event
            float normalizedAnimationTime = AnimationTime / currClip.length;

            haveZeroized = false;

            bool clipChanged = CheckCurrentClip(currClip);

            if (currentClip == null)
            {
                if (clipChanged)
                {
                    ZeroizeIK();
                }
                return;
            }

            if (clipChanged && debugMode)
            {
                Debug.LogFormat("Clip change: [ {0} -> {1} ]", currentClip?.name, currClip?.name);
            }

            for (int ii=0; ii < currentClipEvents.Length; ii++)
            {
                ref EventDetails det = ref currentClipEvents[ii];
                if ((normalizedAnimationTime >= det.fadeInStartNormalized) && (normalizedAnimationTime <= det.fadeInStopNormalized))
                {
                    // fade in
                    if (!det.targetSet || (det.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_GROUND_SURFACE))
                    {
                        //TODO: GetTargetPosition should be a Callback specified when NGAbilityParkour (or whichever IK-event-requestor) adds this event, so
                        // they can provide event-specific logic to get the appropriate collider volumes and target positions.
                        GetTargetPosition(ref det);
                        det.targetSet = true;
                        //SetIKTargetPositions(act, det);
                    }
                    //GetTargetPosition(act, ref det);
                    SetIKTargetPositions(det);
                    SetIKWeights(det.act, (normalizedAnimationTime - det.fadeInStartNormalized) / (det.fadeInStopNormalized - det.fadeInStartNormalized));
                }
                else if ((normalizedAnimationTime > det.fadeInStopNormalized) && (normalizedAnimationTime < det.fadeOutStartNormalized))
                {
                    /*if (wantBreak && (act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_RIGHT))
                    {
                        wantBreak = false;
                        Debug.Break();
                    }*/
                    if (det.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_GROUND_SURFACE) // != MMCAnimationAction.ActionSubType2.TARGET_TYPE_WALL) //
                    {
                        GetTargetPosition(ref det);
                    }
                    SetIKTargetPositions(det);
                    float mag = (det.targetPositionRH - parkour.FrontEdge).magnitude;
                    /*if (act.actionSubType == MMCAnimationAction.ActionSubType.IK_HAND_RIGHT) 
                    {
                        Debug.LogFormat("targetPositionRH = {0} dist to frontEdge {1}", 
                            det.targetPositionRH.ToString("F2"),
                            mag);
                        //Gizmos.DrawSphere(det.targetPositionRH, 0.15f);
                    }*/
                    // in it.
                    SetIKWeights(det.act, 1.0f);
                }
                else if ((normalizedAnimationTime >= det.fadeOutStartNormalized) && (normalizedAnimationTime <= det.fadeOutStopNormalized))
                {
                    if (det.act.actionSubType2 == MMCAnimationAction.ActionSubType2.TARGET_TYPE_GROUND_SURFACE)
                    {
                        GetTargetPosition(ref det);
                    }
                    SetIKTargetPositions(det);
                    SetIKWeights(det.act, 1.0f - ((normalizedAnimationTime - det.fadeOutStartNormalized) / (det.fadeOutStopNormalized - det.fadeOutStartNormalized)));
                }
                else if (det.targetSet && (normalizedAnimationTime > det.fadeOutStopNormalized))
                {
                    SetIKWeights(det.act, 0f);
                    det.targetSet = false;
                }
            }
        }

    } // class MMCEventIKManager
} // namespace Threepeat