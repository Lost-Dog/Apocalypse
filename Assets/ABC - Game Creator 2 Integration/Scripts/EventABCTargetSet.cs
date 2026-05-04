#if ABC_GC_2_Integration

using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ABCToolkit;

namespace GameCreator.Runtime.VisualScripting {

    public enum ABCTargetSetType {
        TargetSet  = 0,
        SoftTargetSet = 1, 
        TargetRemoved = 2, 
        SoftTargetRemoved = 3
    }

    [Title("ABC On Target Set Event")]
    [Description("Executed when an ABC target/soft target is set/unset")]

    [Category("ABC/On Target Set")]


    [Parameter("Target Set Type", "The type of ABC target event: Target or Soft Target")]
    [Image(typeof(IconCircleSolid), ColorTheme.Type.Green, typeof(IconDice))]

    [Keywords("Soft Target", "Target", "ABC", "Ability", "Integration", "Event")]

    [Serializable]
    public class EventABCTargetSet : TEventCharacter {


        [SerializeField] private ABCTargetSetType m_TargetSetType = ABCTargetSetType.TargetSet;

        protected override void WhenEnabled(Trigger trigger, GameCreator.Runtime.Characters.Character character) {

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(character.gameObject);

            if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

            ABC_Controller ABCEventsCont = entity.GetABCControllerObject().GetComponentInChildren<ABC_Controller>();

            if (ABCEventsCont != null) {
                ABCEventsCont.onTargetSet += this.OnABCTargetSet;
                ABCEventsCont.onSoftTargetSet += this.OnABCTargetSet;
            }


        }

        protected override void WhenDisabled(Trigger trigger, GameCreator.Runtime.Characters.Character character) {

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(character.gameObject);

            if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

            ABC_Controller ABCEventsCont = entity.GetABCControllerObject().GetComponentInChildren<ABC_Controller>();

            if (ABCEventsCont != null && (this.m_TargetSetType == ABCTargetSetType.TargetSet || this.m_TargetSetType == ABCTargetSetType.TargetRemoved)) {
                ABCEventsCont.onTargetSet -= this.OnABCTargetSet;
            }

            if (ABCEventsCont != null && (this.m_TargetSetType == ABCTargetSetType.SoftTargetSet || this.m_TargetSetType == ABCTargetSetType.SoftTargetRemoved)) {
                ABCEventsCont.onSoftTargetSet -= this.OnABCTargetSet;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnABCTargetSet(GameObject TargetObject) {

            if (TargetObject == null && (this.m_TargetSetType == ABCTargetSetType.TargetSet || this.m_TargetSetType == ABCTargetSetType.SoftTargetSet))
                return;


            if (TargetObject != null && (this.m_TargetSetType == ABCTargetSetType.TargetRemoved || this.m_TargetSetType == ABCTargetSetType.SoftTargetRemoved))
                return;

            this.m_Trigger.Execute(TargetObject);
        }
    }
}
#endif