using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Perception
{
    [Title("On Change Awareness Stage")]
    [Category("Perception/On Change Awareness Stage")]
    
    [Description("Executed when the Awareness value of a target changes")]

    [Image(typeof(IconAwareness), ColorTheme.Type.Blue, typeof(OverlayDot))]
    [Keywords("Perceive", "Alert", "Aware", "Suspicious", "Curious", "Detect")]
    
    [Serializable]
    public class EventPerceptionOnAwarenessStage : VisualScripting.Event
    {
        [Flags]
        private enum When
        {
            OnIncrease = 0x01,
            OnDecrease = 0x10
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectPerception.Create;

        [SerializeField] private CompareGameObjectOrAny m_Target = new CompareGameObjectOrAny();

        [SerializeField] private AwareMask m_Stage = AwareMask.Suspicious;
        [SerializeField] private When m_When = When.OnIncrease | When.OnDecrease;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private GameObject m_Source;
        [NonSerialized] private Args m_Args;

        [NonSerialized]
        private Dictionary<GameObject, AwareStage> m_PreviousStages =
            new Dictionary<GameObject, AwareStage>();
        
        // INITIALIZERS: --------------------------------------------------------------------------
        
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            
            Perception perception = this.m_Perception.Get<Perception>(trigger);
            if (perception == null) return;
            
            this.m_Source = perception.gameObject;
            this.m_Args = new Args(perception.gameObject);
            
            this.m_PreviousStages.Clear();

            perception.EventChangeAwarenessStage -= this.OnChangeAwarenessStage;
            perception.EventChangeAwarenessStage += this.OnChangeAwarenessStage;
        }
        
        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            
            if (ApplicationManager.IsExiting) return;

            Perception perception = this.m_Source != null ? this.m_Source.Get<Perception>() : null;
            if (perception == null) return;
            
            perception.EventChangeAwarenessStage -= this.OnChangeAwarenessStage;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChangeAwarenessStage(GameObject target, AwareStage stage)
        {
            Args args = new Args(this.m_Args.Self, target);
            if (!this.m_Target.Match(target, args)) return;
            
            AwareStage previousStage = this.m_PreviousStages.GetValueOrDefault(target, AwareStage.None);
            this.m_PreviousStages[target] = stage;
            
            if (!this.m_Stage.HasFlag((AwareMask) stage)) return;
            if (!this.m_When.HasFlag(When.OnIncrease) && previousStage < stage) return;
            if (!this.m_When.HasFlag(When.OnDecrease) && previousStage > stage) return;
            
            _ = this.m_Trigger.Execute(args);
        }
    }
}