using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Transitions
{
    [Title("On Transition Start")]
    [Image(typeof(IconGradient), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    
    [Category("Transitions/On Transition Start")]
    [Description("Executed when a Transition starts")]
    
    [Serializable]
    public class EventTransitionsOpen : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            Transient.EventOpen -= this.OnExecute;
            Transient.EventOpen += this.OnExecute;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            Transient.EventOpen -= this.OnExecute;
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