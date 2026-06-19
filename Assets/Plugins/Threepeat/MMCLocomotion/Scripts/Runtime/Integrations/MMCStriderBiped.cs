using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Threepeat
{
    public class MMCStriderBiped : StriderBipedMxM
    {
        protected Coroutine smoothTransitionCoroutine;

        public float Weight
        {
            get
            {
                return p_weight;
            }
        }

        // a_fadeDuration is time (in seconds) it takes to get from 0 to 1.
        public void EnableSmoothByTime(float a_fadeDuration)
        {
            if (smoothTransitionCoroutine != null)
            {
                StopCoroutine(smoothTransitionCoroutine);
            }

            if (a_fadeDuration <= Mathf.Epsilon)
            {
                enabled = true;
                p_weight = 1f;
                if (p_playableOutput.IsOutputValid())
                {
                    p_playableOutput.SetWeight(1f);
                }
                return;
            }

            smoothTransitionCoroutine = StartCoroutine(EnableSmoothCoroutine(a_fadeDuration));
        }

        public void DisableSmoothByTime(float a_fadeDuration)
        {
            if (smoothTransitionCoroutine != null)
            {
                StopCoroutine(smoothTransitionCoroutine);
            }

            if (a_fadeDuration <= Mathf.Epsilon)
            {
                enabled = false;
                p_weight = 0f;
                if (p_playableOutput.IsOutputValid())
                {
                    p_playableOutput.SetWeight(0f);
                }
                return;
            }
            
            smoothTransitionCoroutine = StartCoroutine(DisableSmoothCoroutine(a_fadeDuration));
        }


        private IEnumerator EnableSmoothCoroutine(float a_fadeDuration, bool resetWeight = true)
        {
            enabled = true;
            if (resetWeight)
            {
                p_weight = 0f;
            }

            if (p_playableOutput.IsOutputValid())
            {
                p_playableOutput.SetWeight(1f);
            }

            while (true)
            {
                p_weight += Time.deltaTime / a_fadeDuration;

                if (p_weight >= 1f)
                {
                    p_weight = 1f;
                    break;
                }

                yield return null;
            }
        }

        private IEnumerator DisableSmoothCoroutine(float a_fadeDuration)
        {
            while (true)
            {
                p_weight -= Time.deltaTime / a_fadeDuration;

                if (p_weight <= 0f)
                {
                    p_weight = 0f;
                    break;
                }

                yield return null;
            }

            if (p_playableOutput.IsOutputValid())
            {
                p_playableOutput.SetWeight(0f);
            }

            enabled = false;
        }


    }
}