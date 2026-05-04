using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public struct NGFootstepEvent
    {
        public FootstepType type;
        public int footIndex;
        public float timeSinceLastSameFootEvent;
        public string customTypeString;

        public float duration;

        public bool IsRight()
        {
            return footIndex == (int)FootIndex.Right;
        }

        public bool IsLeft()
        {
            return footIndex == (int)FootIndex.Left;
        }

        public NGFootstepEvent(FootstepType type, int footIndex, float timeSinceLastSameFootEvent, float duration = 0f, string customTypeStr = null)
        {
            this.type = type;
            this.footIndex = footIndex;
            this.timeSinceLastSameFootEvent = timeSinceLastSameFootEvent;
            this.duration = duration;
            this.customTypeString = customTypeStr;
        }

        public override string ToString()
        {
            string footString = footIndex.ToString();
            switch (footIndex)
            {
                case (int)FootIndex.Left:
                case (int)FootIndex.Right:
                    footString = ((FootIndex)footIndex).ToString();
                    break;
            }

            string outstring = string.Format("NGFootstepEvent[{0}, {1}] tsl {2:0.00}, dur {3:0.000}", footString, type, timeSinceLastSameFootEvent, duration);

            if (customTypeString.Length > 0)
            {
                outstring += ", " + customTypeString;
            }

            return outstring;
        }

        public enum FootstepType
        {
            Step,
            Scuff,
            Slide,
            Custom
        }

        public enum FootIndex
        {
            Right = 1,
            Left = 2
        }
    }
}