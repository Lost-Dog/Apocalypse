#if DREAMTECK_SPLINES
using Dreamteck.Splines;
using MxM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [CreateAssetMenu(fileName = "NGInputScheme_CinematicSpline", menuName = "Threepeat/Input Scheme/Spline - Cinematic")]
    public class NGInputScheme_SplineCinematic : NGInputSchemeBase
    {
        [Header("Spline Control Settings")]
        [Tooltip("How often (in seconds) to change (and check distance to) along Spline")]
        public float targetCheckInterval = 1.0f;
        [Tooltip("How far (in world units) to project along Spline for each target check interval")]
        public float targetCheckDistance = 2.0f;

        public float closeEnoughDistance = 0.5f;

        [Tooltip("Where to start the spline traversal.  Start of spline is 0, end of spline is 1")]
        public float startPointFraction = 0f;
        [Tooltip("Where to stop the spline traversal.  Start of spline is 0, end of spline is 1")]
        public float endPointFraction = 1f;
        public TraversalDirection traversalDirection = TraversalDirection.Forward;

        public enum TraversalDirection
        {
            Forward,
            Backward
        }


        public InputVectorLogic inputVectorLogic = InputVectorLogic.FractionalSpeedBasedOnDestinationDistance;

        //public GameObject virtualSteeringCamForStrafe = null;

        [Tooltip("Manual means you will set MxMTrajectoryGenerator StrafeDirection yourself")]
        public StrafeCamManipulationMode strafeCamManipulationMode = StrafeCamManipulationMode.WorldZPositive;
        //public bool haveCreatedVirtualStrafeCamObject = false;
        protected bool wasStrafing = false;
        public float strafeDirectionUpdateInterval = 0.25f;

        protected float strafeDirectionLastUpdateTime = 0f;

        [Header("Drive To Target Settings")]
        public float steerCorrectionThresholdDegrees = 15f;
        public float steerCorrectionRateDegPerSecond = 360f;

        public enum StrafeCamManipulationMode
        {
            WorldZPositive,
            Manual,
            FaceSplineNormal
        }

        public enum InputVectorLogic
        {
            MaxSpeed,
            FractionalSpeedBasedOnDestinationDistance,
            [InspectorName("Fractional Speed with Drive-to-Target during Strafe")]
            FracSpeedDriveToTarget
        }


        /*
        public TurnLogic turnLogic = TurnLogic.FullSpeedThroughTurns;

        public enum TurnLogic
        {
            FullSpeedThroughTurns,
            SlowForTurns //,
            //HardCuts
        };*/


        [Tooltip("Convenience variable between 0,1 to control input vector magnitude")]
        public float inputVectorMultiplier = 1f;

        public bool paused = false;

        // get-on-spline methods:  Teleport, Navmesh, 
        // offspline-entrypoint: NearestPointOnSpline, SplineStart

        protected float lastTargetCheckTime = 0f;
        protected Vector3 lastDestination = Vector3.zero;
        protected float lastSplineFraction = -1f;
        protected float currentDestinationSplineFraction = 0f;
        protected Vector3 currentDestination = Vector3.zero;
        protected Vector3 currentDestinationCharacterHeading= Vector3.zero;
        protected bool haveFiredTriggersSinceLastDestinationChange = false;
        //TODO: protected Vector3 currentDestinationPlayerFacing = Vector3.zero;

        [Tooltip("Convenience object so you can change agent's target from inspector.")]
        public SplineComputer wantedSpline = null;

        public void SetCurrentSplineFraction(float startPointFraction)
        {
            currentDestinationSplineFraction = Mathf.Clamp01(startPointFraction);
            RegenerateDestination();
        }


        protected SplineComputer currentSpline = null;

        public bool debugMode = false;
        public Transform debugCurrentDestinationTransformObject = null;

        public SplineComputer spline
        {
            get { return currentSpline; }

            set { wantedSpline = value; }
        }

        public override string GetSchemeName()
        {
            return "CinematicSpline";
        }

        public static float ComputeXZDistance(Vector3 displacementVector)
        {
            return Vector3.ProjectOnPlane(displacementVector, Vector3.up).magnitude;
        }

        protected override Vector3 GetInputVector()
        {
            float distToDestination = ComputeXZDistance(currentDestination - character.transform.position);

            if (paused)
            {
                return Vector3.zero;
            }
            else if (wantedSpline != null)
            {
                CheckTarget();
            }
            else if (currentDestinationSplineFraction >= endPointFraction)
            {
                if (!haveFiredTriggersSinceLastDestinationChange)
                {
                    spline.CheckTriggers(lastSplineFraction, endPointFraction);
                    haveFiredTriggersSinceLastDestinationChange = true;
                }
                wasStrafing = false;
                //mxmTrajectoryGenerator.RelativeCameraTransform = null;
                return Vector3.zero;
            }
            else if ((distToDestination <= closeEnoughDistance) || ((Time.time - lastTargetCheckTime) > targetCheckInterval))
            {
                //Debug.Log("Checking");
                CheckTarget();
            }

            if (spline == null)
            {
                return Vector3.zero;
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

            if (character.Strafing)
            {
                // start strafe
                if (strafeCamManipulationMode != StrafeCamManipulationMode.WorldZPositive)
                {
                    if (!wasStrafing)
                    {
                        /*if (virtualSteeringCamForStrafe == null)
                        {
                            virtualSteeringCamForStrafe = new GameObject("StrafeVirtualCam");
                            haveCreatedVirtualStrafeCamObject = true;
                        }
                        mxmTrajectoryGenerator.RelativeCameraTransform = virtualSteeringCamForStrafe.transform;
                        */
                    }
                    //vcamRelativeStrafeMode = true;
                }

                if ((strafeCamManipulationMode == StrafeCamManipulationMode.FaceSplineNormal) && 
                    (!wasStrafing || ((Time.time - strafeDirectionLastUpdateTime) > strafeDirectionUpdateInterval))) 
                {
                    UpdateStrafeCam();
                    strafeDirectionLastUpdateTime = Time.time;
                }

                wasStrafing = true;
            }
            else if (wasStrafing)
            {
                // just stopped strafing
                //mxmTrajectoryGenerator.RelativeCameraTransform = null;
                wasStrafing = false;
            }



            float dest = ComputeXZDistance(currentDestination - character.transform.position); //(currentDestination - character.transform.position).sqrMagnitude;

            if ((currentDestinationSplineFraction >= endPointFraction) && (dest < mxmTrajectoryGenerator.StoppingDistance))
            {
                return Vector3.zero;
            }

            // if dist to spline is low (we're on spline) and Angle(currFacing, desiredFacingAtDest) is high, lower trajectory mag.

            Vector3 steer = (currentDestination - character.transform.position);

            /*if (vcamRelativeStrafeMode)
            {
                steer = virtualSteeringCamForStrafe.transform.TransformVector(steer);
            }*/

            if (character.Strafing && (inputVectorLogic == InputVectorLogic.FracSpeedDriveToTarget))
            {
                steer = DriveSteerToTarget(steer);
            }

            float finalMultiplier = inputVectorMultiplier;

            if ((dest < targetCheckDistance) && (inputVectorLogic > InputVectorLogic.MaxSpeed))
            {
                finalMultiplier *= (dest / targetCheckDistance);
            }

            if (steer.sqrMagnitude > 1f)
            {
                return steer.normalized * finalMultiplier;
            }
            else if (steer.sqrMagnitude > 0.001f)
            {
                return steer.normalized * finalMultiplier;
            }

            //No input so just return zero
            return Vector3.zero;
        }

        // this is used by a) FaceSplineNormal strafe cam manipulation mode (automatically called) or
        // b) can be used in Manual strafe cam manipulation mode when called manually by user
        public void UpdateStrafeCam()
        {
            SplineSample sample = spline.Evaluate(currentDestinationSplineFraction);

            //virtualSteeringCamForStrafe.transform.position = character.transform.position - spline.transform.TransformDirection(sample.up) * 10f; // 10m behind character
            //virtualSteeringCamForStrafe.transform.LookAt(character.transform.position);
            mxmTrajectoryGenerator.StrafeDirection = sample.up; //spline.transform.TransformVector(sample.up) * 10f; //sample.up; //spline.transform.TransformDirection(sample.up) * 10f;
            //Debug.LogFormat("NormalVec: {0}", sample.up.ToString("F2"));
        }

        public Vector3 DriveSteerToTarget(Vector3 inSteer)
        {
            TrajectoryPoint[] goalPoints = mxmTrajectoryGenerator.GetCurrentGoal();
            //Debug.LogFormat("lastGoalPoint( {0} )", goalPoints[goalPoints.Length - 1].Position.ToString("F2"));
            Vector3 goalVec = goalPoints[goalPoints.Length - 1].Position.normalized * inSteer.magnitude;

            float ang = Vector3.Angle(goalVec, inSteer);
            //Debug.LogFormat("Steer/Goal Angle( {0} )", ang.ToString("F2"));
            if (ang > steerCorrectionThresholdDegrees)
            {
                float angAdj = ang + steerCorrectionRateDegPerSecond * Time.deltaTime;
                //Debug.LogFormat("Adjusting Goal by {0} degrees", angAdj);
                return Vector3.RotateTowards(goalVec, inSteer, angAdj * Mathf.Deg2Rad, 1f);
            }

            return inSteer;
        }

        private void CheckTarget()
        {
            bool isSet = true;
            if (wantedSpline != null)
            {
                currentSpline = wantedSpline;
                lastSplineFraction = -1f;
                currentDestinationSplineFraction = startPointFraction;
                wantedSpline = null;
                isSet = false;
                wasStrafing = false;
            }
            lastTargetCheckTime = Time.time;

            if (currentSpline == null)
            {
                lastDestination = currentDestination = character.transform.position;
                wasStrafing = false;
                return;
            }
            //GetCurrentTarget

            // are we within goodEnoughDistance?

            float distToDestination = ComputeXZDistance(currentDestination - character.transform.position);

            if (isSet && (distToDestination > closeEnoughDistance))
            {
                // destination remains
                return;
            }
            else
            {
                // let's move to next point
                spline.CheckTriggers(lastSplineFraction, currentDestinationSplineFraction);
                RegenerateDestination();
            }

            /*currTargetPos = currentDestination;

            float distTargetMovedSqd = (lastDestination - currTargetPos).sqrMagnitude;
            // we are setting the target
            if (distTargetMovedSqd > 1f)
            {
                //Debug.Log("Moved!");
                // target moved
                //agent.SetDestination(currTargetPos);
            }
            else
            {
                // target hasn't moved
            }
            //AdjustCharacterBehaviorBasedOnTarget(lastTargetVec, currTargetPos, distTargetMovedSqd);
            lastTargetVec = currTargetPos;
            if (distTargetMovedSqd > 1f)
            {
                //Debug.LogFormat("Target Moved: {0:F2}", distTargetMovedSqd);
                agent.SetDestination(lastTargetVec);
            }*/

        }

        private void RegenerateDestination()
        {
            float signedDistance = traversalDirection == TraversalDirection.Backward ? -targetCheckDistance : targetCheckDistance;
            float newSplineFraction = (float)spline.Travel(currentDestinationSplineFraction, signedDistance);
            lastDestination = currentDestination;
            currentDestination = spline.EvaluatePosition(newSplineFraction);
            SplineSample sample = spline.Evaluate(newSplineFraction);
            currentDestinationCharacterHeading = sample.forward;

            if (debugMode)
            {
                Debug.LogFormat("Setting new position at frac {0}", newSplineFraction);
            }
            if (debugCurrentDestinationTransformObject != null)
            {
                debugCurrentDestinationTransformObject.position = currentDestination;
            }
            lastSplineFraction = currentDestinationSplineFraction;
            currentDestinationSplineFraction = newSplineFraction; // this is wrong
            haveFiredTriggersSinceLastDestinationChange = false;
        }


        public override void Initialize(NGCharacter pCharacter, MxMTrajectoryGenerator pTrajGen)
        {
            base.Initialize(pCharacter, pTrajGen);
            
            // setup trajectory generator.  For now, we're leaving MxM thinking that user input is driving this 
            // character, but since we're providing the input vector (which comes from a Dreamteck Spline), MxM can just
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

        }

        public override void Deactivate()
        {
            //currentDestination = null;
            /*if (haveCreatedVirtualStrafeCamObject)
            {
                Destroy(virtualSteeringCamForStrafe);
                virtualSteeringCamForStrafe = null;
                haveCreatedVirtualStrafeCamObject = false;
            }*/
        }
    }
}
#endif