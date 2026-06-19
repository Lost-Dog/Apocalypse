using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Transitions
{
    [Title("Is Transitioning")]
    [Description("Returns true there is an active Transition")]

    [Category("Transitions/Is Transitioning")]
    
    [Keywords("Transition", "Transient")]
    
    [Image(typeof(IconGradient), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    [Serializable]
    public class ConditionTransitionsIsRunning : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary => "Is Transitioning";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return Transition.Current != null;
        }
    }
}
