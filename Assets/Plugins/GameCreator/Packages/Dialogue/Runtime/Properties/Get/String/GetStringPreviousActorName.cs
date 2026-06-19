using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [Title("Previous Actor Name")]
    [Category("Dialogue/Previous Actor Name")]
    
    [Image(typeof(IconBust), ColorTheme.Type.Yellow, typeof(OverlayDot))]
    [Description("Returns the name of the Previous Actor playing")]
    
    [Serializable]
    public class GetStringPreviousActorName : PropertyTypeGetString
    {
        public override string Get(Args args) => Dialogue.PreviousActor != null 
            ? Dialogue.PreviousActor.GetName(args) 
            : string.Empty;

        public override string String => "Previous Actor Name";
    }
}