#if DREAMTECK_SPLINES
using Dreamteck.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [CreateAssetMenu(fileName = "NGInputVectorPostProc_SplinePlayer", menuName = "Threepeat/Input PostProcessor/IVPP_SplinePlayer")]
    public class NGInputVectorPostProcessor_SplinePlayer : NGInputVectorPostProcessorBase
    {
        [Header("Spline Control Settings")]
        [Tooltip("How often (in seconds) to change (and check distance to) along Spline")]
        public float splineCheckInterval = 1.0f;

        [Tooltip("How far (in world units) to project along Spline for each target check interval")]
        public float lookAheadDistance = 2.0f;

        [Tooltip("How far away from the spline the player can get")]
        public float closeEnoughDistance = 0.5f;

        [Tooltip("How far off the closest spline point's direction the player can steer")]
        public float maxAngleTolerance = 45f;

        public JoinSplineStrategy joinSplineStrategy = JoinSplineStrategy.GotoStart;

        [Tooltip("Where to start the spline traversal.  Start of spline is 0, end of spline is 1")]
        public float startPointFraction = 0f;
        [Tooltip("Where to stop the spline traversal.  Start of spline is 0, end of spline is 1")]
        public float endPointFraction = 1f;

        [Header("Drive To Target Settings")]
        public float steerCorrectionThresholdDegrees = 15f;
        public float steerCorrectionRateDegPerSecond = 360f;
        public bool slowDownWhenInsideLookAhead = false;

        public NGInputScheme_SplineCinematic.InputVectorLogic inputVectorLogic = NGInputScheme_SplineCinematic.InputVectorLogic.FractionalSpeedBasedOnDestinationDistance;

        [Tooltip("Convenience variable between 0,1 to control input vector magnitude")]
        public float inputVectorMultiplier = 1f;

        public bool paused = false;

        public enum JoinSplineStrategy
        {
            GotoStart,
            GotoNearestPoint
        }

        public AllowableTraversalDirection allowableTraversalDirection = AllowableTraversalDirection.Forward;

        public InputType inputType = InputType.UpForwardDownBackward;

        public enum InputType
        {
            UpForwardDownBackward,
            AngleTolerance          // TODO
        }

        public enum AllowableTraversalDirection
        {
            Forward,
            Backward,
            Both
        }

        public enum TraversalDirection
        {
            Forward,
            Backward
        }

        public SplineComputer spline
        {
            get { return currentSpline; }

            set { wantedSpline = value; }
        }

        [Tooltip("Convenience object so you can change agent's target from inspector.")]
        public SplineComputer wantedSpline = null;

        public bool debugMode = false;

        protected SplineComputer currentSpline = null;

        protected float lastTargetCheckTime = -10f;
        protected Vector3 lastDestination = Vector3.zero;
        protected float lastSplineFraction = -1f;
        protected TraversalDirection lastDirection = TraversalDirection.Forward;

        protected Vector3 currentDestination = Vector3.zero;
        protected Vector3 currentDestinationCharacterHeading = Vector3.zero;
        protected TraversalDirection currentDirection = TraversalDirection.Forward;

        protected float playerToSplineDistance = 0;
        protected Vector3 playerToSplineVector = Vector3.zero;
        protected float currentDestinationSplineFraction = -1f;
        protected bool wasCloseEnoughLastTime = false;
        protected bool haveFiredTriggersSinceLastDestinationChange = false;
        protected Transform camTransform = null;

        public void UnsetSpline()
        {
            currentSpline = null;
        }

        public override void Activate(NGInputSchemeBase inputScheme)
        {
            camTransform = inputScheme.mxmTrajectoryGenerator.RelativeCameraTransform;
            inputScheme.mxmTrajectoryGenerator.RelativeCameraTransform = null;
            if (currentDestinationSplineFraction < 0)
            {
                if (currentDirection == TraversalDirection.Forward)
                {
                    currentDestinationSplineFraction = startPointFraction;
                }
                else
                {
                    currentDestinationSplineFraction = endPointFraction;
                }
            }
        }
        public override void Deactivate(NGInputSchemeBase inputScheme)
        {
            inputScheme.mxmTrajectoryGenerator.RelativeCameraTransform = camTransform;
        }

        public enum WantedDirection
        {
            Forward,
            Backward,
            None
        }

        public void SetCurrentSplineFraction(float startPointFraction, WantedDirection wantedDir)
        {
            currentDestinationSplineFraction = Mathf.Clamp01(startPointFraction);
            RegenerateDestination(wantedDir);
        }

        public override void PostProcessInputVector(NGInputSchemeBase inputScheme, ref Vector3 inputVectorLocalSpace)
        {
            float distToDestination = NGInputScheme_SplineCinematic.ComputeXZDistance(currentDestination - inputScheme.character.transform.position);

            bool inputUp = inputVectorLocalSpace.z > 0.1f;
            bool inputDown = inputVectorLocalSpace.z < -0.1f;


            WantedDirection wantedDir = WantedDirection.None;

            if (inputUp && ((allowableTraversalDirection == AllowableTraversalDirection.Forward) || (allowableTraversalDirection == AllowableTraversalDirection.Both))) 
            {
                wantedDir = WantedDirection.Forward;
            }
            else if (inputDown && ((allowableTraversalDirection == AllowableTraversalDirection.Backward) || (allowableTraversalDirection == AllowableTraversalDirection.Both)))
            {
                wantedDir = WantedDirection.Backward;
            }


            if (paused)
            {
                if (debugMode) Debug.Log($"PPIV - {inputVectorLocalSpace.ToString()} -> zero (paused)");
                inputVectorLocalSpace = Vector3.zero;
                return;
            }
            else if (wantedSpline != null)
            {
                CheckTarget(inputScheme, wantedDir);
            }
            else if (((wantedDir == WantedDirection.Forward) && (currentDestinationSplineFraction >= endPointFraction)) ||
                     ((wantedDir == WantedDirection.Backward) && (currentDestinationSplineFraction <= startPointFraction)))
            {
                if (!haveFiredTriggersSinceLastDestinationChange)
                {
                    spline.CheckTriggers(lastSplineFraction, (wantedDir == WantedDirection.Forward) ? endPointFraction : startPointFraction);
                    haveFiredTriggersSinceLastDestinationChange = true;
                }
                if (debugMode) Debug.Log($"PPIV - {inputVectorLocalSpace.ToString()} -> zero (past end)");
                inputVectorLocalSpace = Vector3.zero;
                //mxmTrajectoryGenerator.RelativeCameraTransform = null;
                //return Vector3.zero;
                return;
            }
            else if ((distToDestination <= closeEnoughDistance) || ((Time.time - lastTargetCheckTime) > splineCheckInterval))
            {
                //Debug.Log("Checking");
                CheckTarget(inputScheme, wantedDir);
            }

            if (spline == null)
            {
                //return Vector3.zero;
                if (debugMode) Debug.Log("PPIV - no spline");
                return;
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

            /*if (character.isStrafing)
            {
                // start strafe
                if (strafeCamManipulationMode != StrafeCamManipulationMode.WorldZPositive)
                {
                    if (!wasStrafing)
                    {
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
            }*/



            float dest = NGInputScheme_SplineCinematic.ComputeXZDistance(currentDestination - inputScheme.character.transform.position); //(currentDestination - character.transform.position).sqrMagnitude;

            if (((wantedDir == WantedDirection.Forward) && (currentDestinationSplineFraction >= endPointFraction)) ||
                     ((wantedDir == WantedDirection.Backward) && (currentDestinationSplineFraction <= startPointFraction)))
            {
                if (debugMode) Debug.Log($"PPIV - {inputVectorLocalSpace.ToString()} -> zero (past end 2)");
                inputVectorLocalSpace = Vector3.zero;
                return; // Vector3.zero;
            }

            switch (wantedDir)
            {
                case WantedDirection.Forward:
                    currentDirection = TraversalDirection.Forward;
                    break;
                case WantedDirection.Backward:
                    currentDirection = TraversalDirection.Backward;
                    break;
                default:
                    break;
            }
            

            // if dist to spline is low (we're on spline) and Angle(currFacing, desiredFacingAtDest) is high, lower trajectory mag.

            Vector3 steer = (currentDestination - inputScheme.character.transform.position);

            /*if (vcamRelativeStrafeMode)
            {
                steer = virtualSteeringCamForStrafe.transform.TransformVector(steer);
            }*/

            /*if (character.isStrafing && (inputVectorLogic == InputVectorLogic.FracSpeedDriveToTarget))
            {
                steer = DriveSteerToTarget(steer);
            }*/

            float turnSpeedMultiplier = 1f;

            float finalMultiplier = inputVectorMultiplier * inputVectorLocalSpace.magnitude;

            if (slowDownWhenInsideLookAhead && (dest < lookAheadDistance)) // && (inputVectorLogic > InputVectorLogic.MaxSpeed))
            {
                finalMultiplier *= (dest / lookAheadDistance);
            }

            if (steer.sqrMagnitude > 0.001f)
            {
                if (debugMode) Debug.Log($"PPIV - {inputVectorLocalSpace.ToString()} -> {(steer.normalized * finalMultiplier).ToString()}");
                inputVectorLocalSpace = steer.normalized * finalMultiplier;
                return;
            }

            if (debugMode) Debug.Log($"PPIV - {inputVectorLocalSpace.ToString()} -> zero (no input)");
            //No input so just return zero
            //return Vector3.zero;
            inputVectorLocalSpace = Vector3.zero;
            return;


            /*
            // handle spline changes
            if (wantedSpline != null)
            {
                currentSpline = wantedSpline;
                lastTargetCheckTime = -splineCheckInterval;
            }
            if (currentSpline == null)
            {
                return;
            }

            // cache new spline point on interval
            if (Time.time >= (lastTargetCheckTime + splineCheckInterval))
            {
                lastTargetCheckTime = Time.time;
                SplineSample sample = new SplineSample();
                currentSpline.Project(inputScheme.character.transform.position, ref sample);
                currentDestination = sample.position;
                currentDestinationCharacterHeading = sample.forward;
                playerToSplineVector = currentDestination - inputScheme.character.transform.position;
                playerToSplineDistance = playerToSplineVector.magnitude;

                // Fire events as appropriate
                if (currentDestinationSplineFraction >= 0)
                {
                    currentSpline.CheckTriggers(currentDestinationSplineFraction, sample.percent);
                }
                currentDestinationSplineFraction = (float)sample.percent;
            }

            // post-process vector based on settings
            float inputMag = inputVectorLocalSpace.magnitude;

            Vector3 inputVectorWorldSpace = inputVectorLocalSpace;
            Vector3 forward = Vector3.forward;
            if (inputScheme.mxmTrajectoryGenerator.RelativeCameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(inputScheme.mxmTrajectoryGenerator.RelativeCameraTransform.forward, Vector3.up);
                inputVectorWorldSpace = Quaternion.FromToRotation(Vector3.forward, forward) * inputVectorLocalSpace;
            }

            // check if close enough, if not just drive player to spline based on input vector mag
            if (playerToSplineDistance > closeEnoughDistance)
            {
                if (inputScheme.mxmTrajectoryGenerator.RelativeCameraTransform != null)
                {
                    inputVectorLocalSpace = playerToSplineVector.normalized * inputMag;
                }
                else
                {
                    inputVectorLocalSpace = Quaternion.FromToRotation(Vector3.forward, forward) * playerToSplineVector.normalized * inputMag;
                }

                wasCloseEnoughLastTime = false;
                return;
            }

            
            if (maxAngleTolerance >= 0)
            {
                //TODO: handle directionality, this only works for forward
                // check angle
                if (Vector3.Angle(inputVectorLocalSpace, currentDestinationCharacterHeading) > maxAngleTolerance)
                {
                    // angle is above tolerance, correct
                    inputVectorLocalSpace = Vector3.RotateTowards(inputVectorLocalSpace, currentDestinationCharacterHeading, Mathf.PI*2, 0);
                    return;
                }
            }

            // check if current vector would make you not close enough after applying and adjust angle as needed

            */
            
        }

        private void CheckTarget(NGInputSchemeBase inputScheme, WantedDirection wantedDir)
        {
            bool isSet = true;
            if (wantedSpline != null)
            {
                currentSpline = wantedSpline;
                lastSplineFraction = -1f;
                if (CanTraverse(AllowableTraversalDirection.Forward) && 
                        ((wantedDir == WantedDirection.Forward) || (wantedDir == WantedDirection.Backward)))
                {
                    currentDestinationSplineFraction = startPointFraction;
                    currentDirection = TraversalDirection.Forward;
                }
                else if (CanTraverse(AllowableTraversalDirection.Backward) && (wantedDir == WantedDirection.Backward))
                {
                    currentDestinationSplineFraction = endPointFraction;
                    currentDirection = TraversalDirection.Backward;
                }
                wantedSpline = null;
                isSet = false;
            }
            lastTargetCheckTime = Time.time;

            if (currentSpline == null)
            {
                lastDestination = currentDestination = inputScheme.character.transform.position;
                return;
            }
            //GetCurrentTarget

            // are we within goodEnoughDistance?

            if (isSet && IsDirectionChange(currentDirection, wantedDir))
            {
                lastDestination = inputScheme.character.transform.position;
                RegenerateDestination(wantedDir);
            }

            float distToDestination = NGInputScheme_SplineCinematic.ComputeXZDistance(currentDestination - inputScheme.character.transform.position);

            if (isSet && (distToDestination > closeEnoughDistance))
            {
                // destination remains
                return;
            }
            else
            {
                // let's move to next point
                spline.CheckTriggers(lastSplineFraction, currentDestinationSplineFraction);
                RegenerateDestination(wantedDir);
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

        private bool IsDirectionChange(TraversalDirection currentDirection, WantedDirection wantedDir)
        {
            if ((currentDirection == TraversalDirection.Forward) && 
                ((wantedDir == WantedDirection.Forward) || (wantedDir == WantedDirection.None)))
            {
                return false;
            }
            if ((currentDirection == TraversalDirection.Backward) &&
                ((wantedDir == WantedDirection.Backward) || (wantedDir == WantedDirection.None)))
            {
                return false;
            }
            return true;
        }


        private bool CanTraverse(AllowableTraversalDirection direction)
        {
            return ((allowableTraversalDirection == direction) || (allowableTraversalDirection == AllowableTraversalDirection.Both));
        }

        public TraversalDirection ConvertWantedDirToSplineDir(WantedDirection wdir)
        {
            if (wdir == WantedDirection.Forward) 
            {
                return TraversalDirection.Forward;
            }
            else if (wdir == WantedDirection.Backward)
            {
                return TraversalDirection.Backward;
            }
            return currentDirection;
        }
 
        private void RegenerateDestination(WantedDirection wantedDir)
        {
            //TODO: make this support forward and backward traversal
            if (debugMode) Debug.Log($"RD - calling Travel splineFrac {currentDestinationSplineFraction}, la {lookAheadDistance}");
            TraversalDirection travelDirection = ConvertWantedDirToSplineDir(wantedDir);
            float signedDistance = travelDirection == TraversalDirection.Backward ? -lookAheadDistance : lookAheadDistance;
            float newSplineFraction = (float)spline.Travel(currentDestinationSplineFraction, signedDistance);
            lastDestination = currentDestination;
            currentDestination = spline.EvaluatePosition(newSplineFraction);
            SplineSample sample = spline.Evaluate(newSplineFraction);
            currentDestinationCharacterHeading = sample.forward;

            //MBCURRENT

            /*if (debugMode)
            {
                Debug.LogFormat("Setting new position at frac {0}", newSplineFraction);
            }
            if (debugCurrentDestinationTransformObject != null)
            {
                debugCurrentDestinationTransformObject.position = currentDestination;
            }*/
            lastSplineFraction = currentDestinationSplineFraction;
            currentDestinationSplineFraction = newSplineFraction; // this is wrong
            haveFiredTriggersSinceLastDestinationChange = false;
        }

    }
}
#endif