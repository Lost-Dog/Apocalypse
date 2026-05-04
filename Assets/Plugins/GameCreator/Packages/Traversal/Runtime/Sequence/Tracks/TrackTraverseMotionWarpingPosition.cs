using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class TrackTraverseMotionWarpingPosition : TrackTraverseMotionWarpingBase
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeReference] private ClipTraverseMotionWarpingPosition[] m_Clips = 
        {
            new ClipTraverseMotionWarpingPosition()
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        public override IClip[] Clips => this.m_Clips;
        
        public override Color ColorConnectionMiddleNormal => ColorTheme.Get(ColorTheme.Type.Purple);
        public override Color ColorConnectionMiddleSelect => ColorTheme.GetLighter(ColorTheme.Type.Purple);

        public override float TransitionRange
        {
            get
            {
                if (this.m_Clips.Length != 1) return 0f;
                ClipTraverseMotionWarpingPosition clip = this.m_Clips[0];
                
                return clip.Mode == WarpMode.FromPointToPoint
                    ? clip.TransitionIn
                    : 0f;
            }
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TrackTraverseMotionWarpingPosition()
        { }

        public TrackTraverseMotionWarpingPosition(ClipTraverseMotionWarpingPosition clip)
        {
            this.m_Clips = new[] { clip };
        }
    }
}