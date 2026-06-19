#if ROOTMOTION_FINALIK
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCFootPlacementWrapper_FinalIK : MonoBehaviour, MMCFootPlacementWrapper
    {
        public GrounderFBBIK gik;

        // Start is called before the first frame update
        void Start()
        {
            if (gik == null)
            {
                gik = GetComponent<GrounderFBBIK>();
            }
        }

        public void EnableFootPlacement(float smoothTransitionTime = 0)
        {
            if (gik != null)
            {
                gik.weight = 1f;
            }
        }

        public void DisableFootPlacement(float smoothTransitionTime = 0)
        {
            if (gik != null)
            {
                gik.weight = 0f;
            }
        }

    }
}
#endif