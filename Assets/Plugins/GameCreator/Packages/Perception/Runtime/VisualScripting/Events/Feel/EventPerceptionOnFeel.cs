using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Perception
{
    [Title("On Feel")]
    [Category("Perception/Feel/On Feel")]
    
    [Description("Executed every frame  that the Perception feels another tracked game object")]

    [Image(typeof(IconFeel), ColorTheme.Type.Green)]
    [Keywords("Track", "Touch", "Close", "Near")]
    [Example("This event can be called multiple times per frame as it can feel multiple objects at the same time")]
    
    [Serializable]
    public class EventPerceptionOnFeel : VisualScripting.Event
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectPerception.Create;

        [SerializeField]
        private CompareGameObjectOrAny m_Target = new CompareGameObjectOrAny(
            true,
            GetGameObjectPlayer.Create()
        );
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;
        [NonSerialized] private GameObject m_Source;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnAwake(Trigger trigger)
        {
            base.OnAwake(trigger);
            this.m_Args = new Args(this.Self);
        }

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            
            Perception perception = this.m_Perception.Get<Perception>(trigger);
            if (perception == null) return;
            
            SensorFeel sensorFeel = perception.GetSensor<SensorFeel>();
            if (sensorFeel == null) return;
            
            this.m_Source = perception.gameObject;
            this.m_Args.ChangeTarget(perception.gameObject);
            
            sensorFeel.EventFeel -= this.OnFeel;
            sensorFeel.EventFeel += this.OnFeel;
        }
        
        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            
            if (ApplicationManager.IsExiting) return;

            Perception perception = this.m_Source != null ? this.m_Source.Get<Perception>() : null;
            if (perception == null) return;
            
            SensorFeel sensorFeel = perception.GetSensor<SensorFeel>();
            if (sensorFeel == null) return;
            
            sensorFeel.EventFeel -= this.OnFeel;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnFeel(GameObject target)
        {
            if (!this.m_Target.Match(target, this.m_Args)) return;
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}