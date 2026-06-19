using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [Title("Current Actor Name")]
    [Category("Dialogue/Current Actor Name")]
    
    [Image(typeof(IconBust), ColorTheme.Type.Yellow, typeof(OverlayDot))]
    [Description("Returns the name of the Current Actor playing")]
    
    [Serializable]
    public class GetStringCurrentActorName : PropertyTypeGetString
    {
        public override string Get(Args args) => Dialogue.CurrentActor != null 
            ? Dialogue.CurrentActor.GetName(args) 
            : string.Empty;

        public override string String => "Current Actor Name";
    }
}