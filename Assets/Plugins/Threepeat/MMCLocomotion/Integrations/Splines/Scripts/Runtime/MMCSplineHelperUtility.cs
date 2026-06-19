#if DREAMTECK_SPLINES
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    // This is a component that can be placed on the same object as NGCharacter to
    // provide a set of convenient helper functions for Spline interaction
    public class MMCSplineHelperUtility : MonoBehaviour
    {
        NGInputScheme_SplineCinematic inputScheme = null;
        [Tooltip("You can leave this null if this component is on same object as NGCharacter component")]
        public NGCharacter character = null;

        // Start is called before the first frame update
        void Start()
        {
            if (character == null)
            {
                character = GetComponent<NGCharacter>();
            }
        }

        public void PauseSplineTraversal()
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            inputScheme.paused = true;
        }

        public void UnpauseSplineTraversal()
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            inputScheme.paused = false;
        }

        public void SprintStart()
        {
            character.Sprinting = true;
        }

        public void SprintStop()
        {
            character.Sprinting = false;
        }
        public void StrafeStart()
        {
            character.Strafing = true;
        }

        public void StrafeStop()
        {
            character.Strafing = false;
        }

        public void ForceWalk(bool doForceWalk)
        {
            character.canRun = !doForceWalk;
        }

        public void RestartSpline()
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            inputScheme.SetCurrentSplineFraction(inputScheme.startPointFraction);
        }

        public void SetCurrentSplineFraction(float frac)
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            inputScheme.SetCurrentSplineFraction(frac);

        }

        public void StandInPlaceSecondsGameTime(float numSeconds)
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            StartCoroutine(PauseWorker(numSeconds));
        }

        public void PostProc_StandInPlaceSecondsGameTime(float numSeconds)
        {
            StartCoroutine(PausePostProcWorker(numSeconds));
        }


        public void FireContextualAction(string actionName)
        {
            character.FireContextualAction(actionName);
        }

        public void InputVectorRampDownAndUp(float rampTime)
        {
            InputVectorRampDownAndUp(rampTime, rampTime, 0f);
        }

        public void InputVectorRampDownAndUp(float downTime, float upTime, float downVal)
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            StartCoroutine(InputVectorRampDownAndUpWorker(downTime, upTime, downVal));
        }

        public void FollowNewSpline(SplineComputer sc)
        {
            if (!CheckAndSetScheme())
            {
                return;
            }

            inputScheme.wantedSpline = sc;
        }

        public float tempMultiplier = 0f;

        public void SetTempInputVectorMultiplier(float pTempSpeed)
        {
            tempMultiplier = pTempSpeed;
        }

        public void ChangeToTempInputVectorMultipler(float duration)
        {
            StartCoroutine(ChangeToTempMultiplierWorker(duration));
        }

        private IEnumerator ChangeToTempMultiplierWorker(float duration)
        {
            float inputVectorMultiplier = inputScheme.inputVectorMultiplier;
            float endTime = Time.time + duration;

            inputScheme.inputVectorMultiplier = tempMultiplier;

            while (Time.time < endTime)
            {
                yield return null;
            }

            inputScheme.inputVectorMultiplier = inputVectorMultiplier;
            yield return null;
        }

        private IEnumerator InputVectorRampDownAndUpWorker(float downTime, float upTime, float downVal)
        {
            float startTime = Time.time;

            float startVal = inputScheme.inputVectorMultiplier;
            float origVal = startVal;

            // Down phase
            while (Time.time <= (startTime+downTime))
            {
                inputScheme.inputVectorMultiplier = Mathf.Lerp(startVal, downVal, (Time.time - startTime) / downTime);
                yield return null;
            }
            // Up phase
            startTime = Time.time;
            startVal = downVal;

            while (Time.time <= (startTime + upTime))
            {
                inputScheme.inputVectorMultiplier = Mathf.Lerp(startVal, origVal, (Time.time - startTime) / upTime);
                yield return null;
            }

            inputScheme.inputVectorMultiplier = 1f;
            yield return null;
        }

        private IEnumerator PausePostProcWorker(float numSeconds)
        {
            foreach (NGInputVectorPostProcessorBase pp in character.InputScheme.postProcessors)
            {
                if (pp.GetType() == typeof(NGInputVectorPostProcessor_SplinePlayer))
                {
                    ((NGInputVectorPostProcessor_SplinePlayer)pp).paused = true;
                }
            }
                
                        yield return new WaitForSeconds(numSeconds);

            foreach (NGInputVectorPostProcessorBase pp in character.InputScheme.postProcessors)
            {
                if (pp.GetType() == typeof(NGInputVectorPostProcessor_SplinePlayer))
                {
                    ((NGInputVectorPostProcessor_SplinePlayer)pp).paused = false;
                }
            }

            yield return null;
        }

        private IEnumerator PauseWorker(float numSeconds)
        {
            inputScheme.paused = true;

            yield return new WaitForSeconds(numSeconds);

            inputScheme.paused = false;

            yield return null;
        }

        protected bool CheckAndSetScheme()
        {
            inputScheme = character.InputScheme as NGInputScheme_SplineCinematic;
            return inputScheme != null;
        }

    }
}
#endif