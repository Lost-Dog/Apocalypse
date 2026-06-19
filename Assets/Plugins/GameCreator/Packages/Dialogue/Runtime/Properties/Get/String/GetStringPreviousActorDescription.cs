using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [Title("Previous Actor Description")]
    [Category("Dialogue/Previous Actor Description")]
    
    [Image(typeof(IconBust), ColorTheme.Type.Yellow, typeof(OverlayDot))]
    [Description("Returns the description of the Previous Actor playing")]
    
    [Serializable]
    public class GetStringPreviousActorDescription : PropertyTypeGetString
    {
        public override string Get(Args args) => Dialogue.PreviousActor != null 
            ? Dialogue.PreviousActor.GetDescription(args) 
            : string.Empty;

        public override string String => "Previous Actor Description";
    }
}