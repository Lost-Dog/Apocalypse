using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Serializable]
    public class TrackSkillTreeClips : GameCreator.Runtime.VisualScripting.Track
    {
        [SerializeReference] private ClipSkillTreeInstructions[] m_Clips = Array.Empty<ClipSkillTreeInstructions>();
        
        public override int TrackOrder => 1;
        public override TrackType TrackType => TrackType.Single;
        public override TrackAddType AllowAdd => TrackAddType.Allow;
        public override TrackRemoveType AllowRemove => TrackRemoveType.Allow;
        
        public override GameCreator.Runtime.VisualScripting.IClip[] Clips => this.m_Clips;
        
        public override Color ColorClipNormal => ColorTheme.Get(ColorTheme.Type.Green);
        public override Color ColorClipSelect => ColorTheme.Get(ColorTheme.Type.Green);
    }
}
