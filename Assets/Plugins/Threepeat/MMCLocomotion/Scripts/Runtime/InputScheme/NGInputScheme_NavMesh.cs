using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Threepeat
{

    [CreateAssetMenu(fileName = "NGInputScheme_NavMesh", menuName = "Threepeat/Input Scheme/NavMesh")]
    public class NGInputScheme_NavMesh : NGInputSchemeBase
    {
        [Header("NavMesh Control Settings")]
        [Tooltip("How often (in seconds) to change (and check distance to) NavMeshAgent's target and whether target position has changed")]
        public float targetCheckInterval = 1.0f;

        protected NavMeshAgent agent = null;
        protected float lastTargetCheckTime = 0f;
        protected Vector3 lastTargetVec = Vector3.zero;
        protected Vector3 lastTargetVecActual = Vector3.zero;
        protected Transform currentTargetTransform = null;

        public bool faceTarget = false;

        [Tooltip("Convenience object so you can change agent's target from inspector.")]
        public Transform wantedTargetTransform = null;

        [Tooltip("Maximum distance target can get from character before character will move to within MxMTrajectoryGenerator.StoppingDistance of target.  -1 means StoppingDistance is the goal distance and character will attempt to remain within that distance from target at all times.  Think of MxMTrajectoryGenerator.StoppingDistance as minDistanceToTarget and maxSeparationDistanceBeforeMoving as maxDistanceFromTarget")]
        public float maxSeparationDistanceBeforeMoving = -1;

        [Tooltip("Minimum distance target can get from character before character will move away ")]
        public float minSeparationDistanceBeforeMoving = -1;
        public Transform debugTargetObj = null;
        public Transform debugXForm = null;

        protected bool wasWithinRange = false;

        protected bool wasStrafingBeforeThisInputSchemeWasActivated = false;

       

        public Vector3 currentTargetVector
        {
            get
            {
                /*if (currentTargetTransform != null)
                {
                    return currentTargetTransform.position;
                }*/
                return agent.destination;
            }

            set
            {
                currentTargetTransform = null;
                
                if (debugTargetObj != null)
                {
                    debugTargetObj.position = value + GetOffset(value);
                }
                agent.SetDestination(value + GetOffset(value));
            }
        }

        public override void Deactivate()
        {
            currentTarget = null;
            agent.isStopped = true;
            agent.enabled = false;

            /*if (character.Strafing)
            {
                Debug.Log("still strafing");
            }*/

            if (faceTarget && !wasStrafingBeforeThisInputSchemeWasActivated)
            {
                character.Strafing = false;
            }
        }

        public Transform currentTarget
        {
            get => currentTargetTransform;

            set
            {
                SetTarget(value);
            }
        }

        public void SetTarget(Transform targetTransform)
        {
            currentTargetTransform = targetTransform;
            if (currentTargetTransform == null)
            {
                agent.isStopped = true;
            }
            else
            { 
                Vector3 offset = GetOffset(currentTargetTransform.position);
                if (debugTargetObj != null)
                {
                    debugTargetObj.position = currentTargetTransform.position + offset;
                }
                agent.SetDestination(currentTargetTransform.position+offset);
            }
            lastTargetCheckTime = 0f;
        }

        public Vector3 GetOffset(Vector3 targetPos)
        {
            if (minSeparationDistanceBeforeMoving > 0)
            {
                return (character.transform.position - targetPos).normalized * minSeparationDistanceBeforeMoving;
            }

            return Vector3.zero;
        }

        public override void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen)
        {
            base.Initialize(pCharacter, pTrajGen);

            agent = character.GetComponent<NavMeshAgent>();

            wasStrafingBeforeThisInputSchemeWasActivated = character.Strafing;
            
            if (agent == null)
            {
                //Debug.Log("Adding NavMeshAgent!");
                agent = character.gameObject.AddComponent<NavMeshAgent>();
                // set agent settings
                agent.baseOffset = 0;
                agent.speed = 0;
                agent.angularSpeed = 60;
                agent.acceleration = 0;
                agent.stoppingDistance = 1;
                agent.autoBraking = true;
                agent.radius = 0.5f;
                agent.height = 2;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.avoidancePriority = 50;
                agent.autoTraverseOffMeshLink = true;
                agent.autoRepath = true;
            }

            // setup trajectory generator.  For now, we're leaving MxM thinking that user input is driving this 
            // character, but since we're providing the input vector (which comes from NavMesh), MxM can just
            // stay abstracted away from that.
            TrajectoryGeneratorModule tgmod = ScriptableObject.CreateInstance<TrajectoryGeneratorModule>();
            tgmod.MaxSpeed = mxmTrajectoryGenerator.MaxSpeed;
            tgmod.PosBias = mxmTrajectoryGenerator.PositionBias;
            tgmod.DirBias = mxmTrajectoryGenerator.DirectionBias;
            tgmod.ControlMode = ETrajectoryControlMode.UserInput;
            tgmod.TrajectoryMode = ETrajectoryMoveMode.Normal;
            tgmod.FlattenTrajectory = false;
            tgmod.CustomInput = true;
            tgmod.FaceDirectionOnIdle = mxmTrajectoryGenerator.FaceDirectionOnIdle;
            tgmod.StoppingDistance = mxmTrajectoryGenerator.StoppingDistance;
            tgmod.ApplyRootSpeedToNavAgent = mxmTrajectoryGenerator.ApplyRootSpeedToNavAgent;
            tgmod.ResetDirectionOnNoInput = true;
            //tgmod.CamTransform = mxmTrajectoryGenerator.RelativeCameraTransform;
            tgmod.InputProfile = mxmTrajectoryGenerator.InputProfile;
            mxmTrajectoryGenerator.SetTrajectoryModule(tgmod);
            mxmTrajectoryGenerator.RelativeCameraTransform = null;
            
            agent.enabled = true;
            agent.isStopped = false;
        }

        protected override Vector3 GetInputVector()
        {
            if ((Time.time - lastTargetCheckTime) > targetCheckInterval)
            {
                //Debug.Log("Checking");
                CheckTarget();
            }

            /*Vector3 rawInput = new Vector3(Input.GetAxis(moveAxisHorizontal), 0f, Input.GetAxis(moveAxisVertical));

            if (doEnvironmentAwareTrajectories)
            {
                if (camTransform == null)
                {
                    Debug.LogError("Invalid camera transform reference in the player character's MxMTrajectoryGenerator component.");
                    return Vector3.zero;
                }


                float newScale = GetDesiredTrajectoryScale(camTransform, rawInput);

                return rawInput * newScale;
            }*/

            if (agent.destination == null)
            {
                //Debug.Log($"GIV -> zero : null destination");
                return Vector3.zero;
            }

            float destSqr = (agent.destination - character.transform.position).sqrMagnitude;

            float minDistSqr;


            /*if (minSeparationDistanceBeforeMoving > 0)
            {
                minDistSqr = minSeparationDistanceBeforeMoving * minSeparationDistanceBeforeMoving; 
            }
            else*/
            {
                 minDistSqr = mxmTrajectoryGenerator.StoppingDistance * mxmTrajectoryGenerator.StoppingDistance;
            }

            float maxDistSqr =
                   maxSeparationDistanceBeforeMoving > 0 ?
                        (maxSeparationDistanceBeforeMoving-minSeparationDistanceBeforeMoving) * 
                        (maxSeparationDistanceBeforeMoving-minSeparationDistanceBeforeMoving) :
                        minDistSqr;

            /*if ((minSeparationDistanceBeforeMoving > 0) && (destSqr < minDistSqr))
            {
                Vector3 tempSteer = character.transform.position - agent.destination;
                return tempSteer.normalized;
            }*/
            if (destSqr < mxmTrajectoryGenerator.StoppingDistance)
            {
                //Debug.Log($"GIV -> zero : within range");
                wasWithinRange = true;
                return Vector3.zero;
            }
            else if (wasWithinRange && (destSqr < maxDistSqr) && (destSqr >= minDistSqr))
            {
                //Debug.Log($"GIV -> zero : between max and stopping dist");
                return Vector3.zero;
            }

            wasWithinRange = false;

            Vector3 steer = (agent.steeringTarget - character.transform.position);

            if (steer.sqrMagnitude > 1f)
            {
                return steer.normalized;
            }
            else if (steer.sqrMagnitude > 0.001f)
            {
                
                return steer.normalized;
            }
            //Debug.Log($"GIV -> zero : no input");
            //No input so just return zero
            return Vector3.zero;
        }

        public override string GetSchemeName()
        {
            return "NavMesh";
        }

        private void CheckTarget()
        {
            if (wantedTargetTransform != null)
            {
                SetTarget(wantedTargetTransform);
                wantedTargetTransform = null;
            }
            lastTargetCheckTime = Time.time;
            Vector3 currTargetPos, currTargetPosActual;
            if (currentTargetTransform != null)
            {
                //Debug.Log($"ChkTarget -> setting to currTargetXForm + Offset: {GetOffset(currentTargetTransform.position)} ");
                currTargetPos = currentTargetTransform.position + GetOffset(currentTargetTransform.position);
                currTargetPosActual = currentTargetTransform.position;
                //Debug.Log($"    Post-dist: {(currTargetPos - currentTargetTransform.position).magnitude}");
            }
            else
            {
                //Debug.Log($"ChkTarget -> setting to agent dest");
                currTargetPos = agent.destination + GetOffset(agent.destination);
                currTargetPosActual = currentTargetTransform.position;
            }

            float distTargetMovedSqd = (lastTargetVec - currTargetPosActual).sqrMagnitude;
            // we are setting the target
            if (distTargetMovedSqd > 0.25f)
            {
                //Debug.Log("Moved!");
                // target moved
                //agent.SetDestination(currTargetPos + GetOffset(lastTargetVec));
            }
            else
            {
                // target hasn't moved
            }
            //AdjustCharacterBehaviorBasedOnTarget(lastTargetVec, currTargetPos, distTargetMovedSqd);
            lastTargetVec = currTargetPos;
            lastTargetVecActual = currTargetPosActual;
            //if (distTargetMovedSqd > 0.1f)
            {
                //Debug.LogFormat("Target Moved: {0:F2}", distTargetMovedSqd);
                if (debugTargetObj != null)
                {
                    debugTargetObj.position = currTargetPos; // lastTargetVec + GetOffset(lastTargetVec);
                }
                if (debugXForm != null)
                {
                    debugXForm.position = currentTargetTransform.position;
                }

                agent.SetDestination(lastTargetVec);
            }

            if (faceTarget)
            {
                if (!character.Strafing)
                {
                    character.Strafing = true;
                }
                //Debug.Log($"Strafe: {(currTargetPos - character.transform.position).normalized}");
                mxmTrajectoryGenerator.StrafeDirection = (currTargetPosActual - character.transform.position).normalized;

            }
            else if (!faceTarget && character.Strafing)
            {
                character.Strafing = false;
            }



        }

    }
}