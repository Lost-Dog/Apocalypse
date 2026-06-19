using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class TrackTraverseMotionWarpingRotation : TrackTraverseMotionWarpingBase
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeReference] private ClipTraverseMotionWarpingRotation[] m_Clips = 
        {
            new ClipTraverseMotionWarpingRotation()
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        public override IClip[] Clips => this.m_Clips;
        
        public override Color ColorConnectionMiddleNormal => ColorTheme.Get(ColorTheme.Type.Yellow);
        public override Color ColorConnectionMiddleSelect => ColorTheme.GetLighter(ColorTheme.Type.Yellow);

        public override float TransitionRange
        {
            get
            {
                if (this.m_Clips.Length != 1) return 0f;
                ClipTraverseMotionWarpingRotation clip = this.m_Clips[0];

                return clip.Mode == WarpMode.FromPointToPoint
                    ? clip.TransitionIn
                    : 0f;
            }
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TrackTraverseMotionWarpingRotation()
        { }

        public TrackTraverseMotionWarpingRotation(ClipTraverseMotionWarpingRotation clip)
        {
            this.m_Clips = new[] { clip };
        }
    }
}