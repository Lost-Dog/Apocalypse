using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Transitions
{
    [Title("On Transition Ready")]
    [Image(typeof(IconGradient), ColorTheme.Type.Blue, typeof(OverlayDot))]
    
    [Category("Transitions/On Transition Ready")]
    [Description("Executed when a Transition is ready to transition out")]
    
    [Serializable]
    public class EventTransitionsReady : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            Transient.EventReady -= this.OnExecute;
            Transient.EventReady += this.OnExecute;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            Transient.EventReady -= this.OnExecute;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnExecute()
        {
            Transient transient = Transient.Current;
            if (transient == null) return;
            
            _ = this.m_Trigger.Execute(transient.gameObject);
        }
    }
}