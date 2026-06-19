using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Transitions
{
    [Title("On Transition Close")]
    [Image(typeof(IconGradient), ColorTheme.Type.Red, typeof(OverlayArrowLeft))]
    
    [Category("Transitions/On Transition Close")]
    [Description("Executed when a Transition starts closing")]
    
    [Serializable]
    public class EventTransitionClose : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            Transient.EventClose -= this.OnExecute;
            Transient.EventClose += this.OnExecute;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            Transient.EventClose -= this.OnExecute;
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