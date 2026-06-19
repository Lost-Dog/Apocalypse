using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class TrackTraverseGravity : Track
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeReference] private ClipTraverseGravity[] m_Clips = 
        {
            new ClipTraverseGravity()
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        public override IClip[] Clips => this.m_Clips;
        
        public override TrackType TrackType => TrackType.Range;
        public override TrackAddType AllowAdd => TrackAddType.OnlyOne;
        public override TrackRemoveType AllowRemove => TrackRemoveType.Allow;

        public override Color ColorClipNormal => ColorTheme.Get(ColorTheme.Type.White);
        public override Color ColorClipSelect => ColorTheme.Get(ColorTheme.Type.White);
        
        public override Color ColorConnectionMiddleNormal => ColorTheme.Get(ColorTheme.Type.Green);
        public override Color ColorConnectionMiddleSelect => ColorTheme.GetLighter(ColorTheme.Type.Green);

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TrackTraverseGravity()
        { }

        public TrackTraverseGravity(ClipTraverseGravity clip)
        {
            this.m_Clips = new[] { clip };
        }
    }
}