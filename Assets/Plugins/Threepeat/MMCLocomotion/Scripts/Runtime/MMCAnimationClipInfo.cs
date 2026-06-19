using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [Serializable]
    public class MMCAnimationClipInfo
    {
        public MMCAnimationAction[] actions;

        public AnimationClip clip;

        public string clipName;

        private int clipNameHash = -1;

        public int ClipNameHash
        {
            get
            {
                if (clipNameHash == -1) {
                    clipNameHash = Animator.StringToHash(clipName);
                }
                return clipNameHash;
            }
        }

    } // class MMCAnimationClipInfo
}