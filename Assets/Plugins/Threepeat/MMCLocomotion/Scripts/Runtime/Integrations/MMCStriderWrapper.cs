using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat {
    public class MMCStriderWrapper
    {
        protected GameObject objContainingStrider;
        protected MMCStriderBiped strider;

        protected bool m_forceDisable = false;

        public void SetObjectContainingStrider(GameObject obj)
        {
            objContainingStrider = obj;

            if (obj != null)
            {
                strider = obj.GetComponent<MMCStriderBiped>();
            }
        }

        public bool Enabled
        {
            set
            {
                if (strider != null)
                {
                    strider.enabled = value;
                }
            }

            get => (strider != null) ? strider.enabled : false;
        }

        public float Weight
        {
            get => (strider != null) ? strider.Weight : 0f;
        }

        public void DisableSmooth(float fadeDuration)
        {
            if (strider != null)
            {
                strider.DisableSmoothByTime(fadeDuration);
            }   
        }

        public void EnableSmooth(float fadeDuration)
        {
            if (strider != null)
            {
                strider.EnableSmoothByTime(fadeDuration);
            }
        }
    }

}