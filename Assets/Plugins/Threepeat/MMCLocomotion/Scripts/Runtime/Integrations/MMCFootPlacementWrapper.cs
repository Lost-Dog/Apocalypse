using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public interface MMCFootPlacementWrapper
    {
        public void EnableFootPlacement(float smoothTransitionTime = 0) { }

        public void DisableFootPlacement(float smoothTransitionTime = 0) { }
    }
}