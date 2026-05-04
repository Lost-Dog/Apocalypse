using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Runtime.Traversal
{
    [Title("On Traverse Interactive Exit")]
    [Category("Traversal/On Traverse Interactive Exit")]
    [Description("Executes when the specified Character exits a Traverse Interactive (or any)")]

    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.TextLight, typeof(OverlayArrowRight))]

    [Serializable]
    public class EventTraversalTraverseInteractiveExit : Event
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField] private CompareGameObjectOrAny m_TraverseInteractive = new CompareGameObjectOrAny(
            true,
            GetGameObjectInstance.Create()
        );

        [NonSerialized] private Character m_CharacterCache;
        [NonSerialized] private Args m_Args;
        
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);

            this.m_CharacterCache = this.m_Character.Get<Character>(this.Self);
            if (this.m_CharacterCache == null) return;

            this.m_Args = new Args(this.Self, this.m_CharacterCache.gameObject);
            this.m_CharacterCache.Combat.RequestStance<TraversalStance>().EventMotionExit += this.OnExit;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            
            if (this.m_CharacterCache == null) return;
            this.m_CharacterCache.Combat.RequestStance<TraversalStance>().EventMotionExit -= this.OnExit;
        }

        private void OnExit()
        {
            if (this.m_CharacterCache == null) return;
            
            if (this.m_TraverseInteractive.Match(this.m_CharacterCache.gameObject, this.m_Args))
            {
                _ = this.m_Trigger.Execute(this.m_Args);
            }
        }
    }
}