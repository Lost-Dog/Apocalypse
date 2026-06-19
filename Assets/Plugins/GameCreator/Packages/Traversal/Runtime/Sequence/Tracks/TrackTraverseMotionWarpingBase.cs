using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public abstract class TrackTraverseMotionWarpingBase : Track
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public override TrackType TrackType => TrackType.Range;
        public override TrackAddType AllowAdd => TrackAddType.OnlyOne;
        public override TrackRemoveType AllowRemove => TrackRemoveType.Allow;

        public override Texture CustomClipIconNormal => new IconSequencerMotionWarp(this.ColorClipNormal).Texture;
        public override Texture CustomClipIconSelect => new IconSequencerMotionWarp(this.ColorClipSelect).Texture;

        public override Color ColorClipNormal => ColorTheme.Get(ColorTheme.Type.White);
        public override Color ColorClipSelect => ColorTheme.Get(ColorTheme.Type.White);
    }
}