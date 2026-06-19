using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Serializable]
    public class TrackSkillTreePhases : GameCreator.Runtime.VisualScripting.Track
    {
        [SerializeReference] private ClipSkillTreePhases[] m_Clips = new ClipSkillTreePhases[]
        {
            new ClipSkillTreePhases()
        };
        
        public override int TrackOrder => 0;
        public override TrackType TrackType => TrackType.Range;
        public override TrackAddType AllowAdd => TrackAddType.OnlyOne;
        public override TrackRemoveType AllowRemove => TrackRemoveType.Allow;
        
        public override GameCreator.Runtime.VisualScripting.IClip[] Clips => this.m_Clips;
        
        // Phase colors: Blue (Cast), Pink (Release), Purple (Recovery)
        public override Color ColorConnectionLeftNormal => ColorTheme.Get(ColorTheme.Type.Blue);
        public override Color ColorConnectionMiddleNormal => ColorTheme.Get(ColorTheme.Type.Pink);
        public override Color ColorConnectionRightNormal => ColorTheme.Get(ColorTheme.Type.Purple);
        
        public override bool IsConnectionLeftThin => true;
        public override bool IsConnectionRightThin => true;
        
        public override Color ColorClipNormal => ColorTheme.Get(ColorTheme.Type.White);
        public override Color ColorClipSelect => ColorTheme.Get(ColorTheme.Type.White);
        
        public override bool HasInspector => true;
    }
}
