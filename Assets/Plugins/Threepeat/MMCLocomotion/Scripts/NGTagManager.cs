using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace Threepeat
{
    public class NGTagManager
    {
        protected MxMAnimator mxmAnimator;

        public bool initialized = false;
        public enum NGUserTag
        {
            DisableGravity = 0,
            DisableCollision,
            DisableFootPlacement,
            EnableHandIK,
            EnableFootIK,
            IgnoreRootMotion,
            IgnoreRootMotionX,
            IgnoreRootMotionY,
            IgnoreRootMotionZ,
            IgnoreRootRotation,
            ZeroizeGroundSpeed,
            ZeroizeSpeedY,
            MidairExitRegion,
            MaxCount
        }

        // Easy access to MxM User Tags e.g. mxmUserTags[NGUserTag.EnableHandIK]
        protected EUserTags[] mxmUserTags;

        public bool Query(NGUserTag tag)
        {
            if (!initialized)
            {
                Initialize(mxmAnimator);
            }
            return (mxmUserTags != null) && (mxmUserTags[(int)tag] != EUserTags.None) && mxmAnimator.QueryUserTags(mxmUserTags[(int)tag]);
        }

        public void Initialize(MxMAnimator mxma)
        {
            initialized = false;
            mxmUserTags = null;
            mxmAnimator = mxma;

            List<EUserTags> tags = new List<EUserTags>();

            MxMAnimData adata = mxmAnimator.CurrentAnimData;

            EUserTags tag;
            string tagName;

            bool missedOne = false;

            string missedString = "Tags not found in AnimData";

            /*foreach (string utag in adata.UserTagNames)
            {
                Debug.LogFormat("UserTag in AnimData: {0}", utag);
            }*/
            if ((adata == null) || (adata.UserTagNames == null))
            {
                return;
            }
            List<string> utagNames = new List<string>(adata.UserTagNames);
            for (int ii = 0; ii < (int)NGUserTag.MaxCount; ii++)
            {
                tagName = ((NGUserTag)ii).ToString();

                if (utagNames.Contains(tagName))
                {
                    tag = adata.UserTagFromName(tagName);
                }
                else
                {
                    tag = EUserTags.None;
                }

                if (tag == EUserTags.None)
                {
                    missedOne = true;
                    //Debug.LogWarningFormat("Couldn't find tag( {0} ) in MxMAnimData", tagName);
                    missedString += ":" + tagName;
                    tags.Add(EUserTags.None);
                    continue;
                }
                tags.Add(tag);
            }
            if (missedOne)
            {
                Debug.LogFormat("Note: {0}", missedString);
            }
            mxmUserTags = tags.ToArray();

            initialized = true;
        }
    }
}