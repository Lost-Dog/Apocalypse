using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [Title("Current Actor Description")]
    [Category("Dialogue/Current Actor Description")]
    
    [Image(typeof(IconBust), ColorTheme.Type.Yellow, typeof(OverlayDot))]
    [Description("Returns the description of the Current Actor playing")]
    
    [Serializable]
    public class GetStringCurrentActorDescription : PropertyTypeGetString
    {
        public override string Get(Args args) => Dialogue.CurrentActor != null 
            ? Dialogue.CurrentActor.GetDescription(args) 
            : string.Empty;

        public override string String => "Current Actor Description";
    }
}