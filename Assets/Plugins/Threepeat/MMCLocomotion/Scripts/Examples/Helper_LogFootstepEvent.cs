using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Helper_LogFootstepEvent : MonoBehaviour
    {

        public void LogFootStepEvent(NGFootstepEvent evt)
        {
            Debug.LogFormat("{0:0.000},{1}: {2}", Time.time, Time.frameCount, evt.ToString());
        }
    }
}