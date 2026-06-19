#if HOAXGAMES_ISTEP
using HoaxGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCFootPlacementWrapper_iStep : MonoBehaviour, MMCFootPlacementWrapper
    {
        private Coroutine worker = null;
        public FootIK footIK;

        public float IKPlacementWeightWhenEnabled = 1.0f;
        public float BodyPlacementWeightWhenEnabled = 1.0f;

        private void Start()
        {
            if (footIK == null)
            {
                footIK = GetComponent<FootIK>();
            }
        }

        public void DisableFootPlacement(float smoothTransitionTime = 0)
        {
            if (worker != null)
            {
                StopCoroutine(worker);
            }
            if (footIK != null)
            {
                Debug.Log("Disabling iStep");
                //worker = StartCoroutine(DisableEnableWorker(false, smoothTransitionTime));
                footIK.enabled = false;
            }
        }

        public void EnableFootPlacement(float smoothTransitionTime = 0)
        {
            if (worker != null)
            {
                StopCoroutine(worker);
            }
            if (footIK != null)
            {
                Debug.Log("Enabling iStep");
                footIK.enabled = true;
                //worker = StartCoroutine(DisableEnableWorker(true, smoothTransitionTime));
            }
        }

        IEnumerator DisableEnableWorker(bool isEnable, float smoothTransitionTime)
        {
            float startIKPlacementWeight = isEnable ? 0 : IKPlacementWeightWhenEnabled;
            float startBodyPlacementWeight = isEnable ? 0 : BodyPlacementWeightWhenEnabled;

            float targetIKPW = isEnable ? IKPlacementWeightWhenEnabled : 0;
            float targetBodyPW = isEnable ? BodyPlacementWeightWhenEnabled : 0;

            float startTime = Time.time;
            float endTime = startTime + smoothTransitionTime;

            if (!isEnable)
            {
                footIK.enabled = true;
            }

            float fval, bval;
            float fraction;
            while (Time.time < endTime)
            {
                fraction = (Time.time - startTime) / smoothTransitionTime;
                fval = Mathf.Lerp(startIKPlacementWeight, targetIKPW, fraction);
                bval = Mathf.Lerp(startBodyPlacementWeight, targetBodyPW, fraction);
                footIK.setFootPlacementWeight(fval);
                footIK.setBodyPlacementWeight(bval);
                yield return null;
            }

            footIK.setFootPlacementWeight(targetIKPW);
            footIK.setBodyPlacementWeight(targetBodyPW);

            if (!isEnable)
            {
                footIK.enabled = false;
            }
            worker = null;
            yield return null;
        }


    }
}
#else
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    // Fallback wrapper to keep projects compiling when iStep is not installed.
    public class MMCFootPlacementWrapper_iStep : MonoBehaviour, MMCFootPlacementWrapper
    {
        public float IKPlacementWeightWhenEnabled = 1.0f;
        public float BodyPlacementWeightWhenEnabled = 1.0f;

        public void DisableFootPlacement(float smoothTransitionTime = 0)
        {
        }

        public void EnableFootPlacement(float smoothTransitionTime = 0)
        {
        }
    }
}
#endif