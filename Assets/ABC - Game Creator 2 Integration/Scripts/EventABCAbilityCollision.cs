#if ABC_GC_2_Integration

using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ABCToolkit;

namespace GameCreator.Runtime.VisualScripting {

    public enum ABCCollisionReferenceType {
        AbilityName = 0,
        AbilityID = 1,
        Effect = 2,
        All = 3
    }

    [Title("ABC Collision Event")]
    [Description("A trigger for ABC ability collisions. Can be set to run GC events when an ability by name/ID collides, an effect is applied or any ability collides.")]

    [Category("ABC/Collision Event")]


    [Parameter("Collision Type", "The name or ID of the Ability to activate")]
    [Parameter("Collision Reference", "The type of event to check for (ability by name/ID, effect applied or any ability collision)")]
    [Parameter("Reference", "A string that represents the ability name, ID or effect. Depending on the reference type chosen")]
    [Image(typeof(IconCircleSolid), ColorTheme.Type.Green, typeof(IconDice))]

    [Keywords("Pass", "Through", "Touch", "Collision", "Collide", "ABC", "Ability", "Integration", "Event")]

    [Serializable]
    public class EventABCAbilityCollision : TEventPhysics
    {


        [SerializeField] private ABCCollisionReferenceType m_CollisionType = ABCCollisionReferenceType.AbilityID;
        [SerializeField] private string m_Reference;

        private void OnCollision(GameObject CollidedObj) {

            ABC_Projectile projABC = CollidedObj.GetComponentInChildren<ABC_Projectile>();

            if (projABC == null)
                return;

            switch (this.m_CollisionType) {
                case ABCCollisionReferenceType.All:
                    _ = this.m_Trigger.Execute(CollidedObj);
                    break;
                case ABCCollisionReferenceType.AbilityID:
                    if (projABC.ability.abilityID == int.Parse(this.m_Reference))
                        _ = this.m_Trigger.Execute(CollidedObj);
                    break;
                case ABCCollisionReferenceType.AbilityName:
                    if (projABC.ability.name == this.m_Reference)
                        _ = this.m_Trigger.Execute(CollidedObj);
                    break;
                case ABCCollisionReferenceType.Effect:

                    List<Effect> gcEffects = projABC.ability.effects.Where(e => e.effectName == "GCAbilityCollisionTrigger").ToList();

                    //If a GC ability collision trigger effect is found and it matches the effect property or we are not matching to an effect misc value then execute trigger
                    if (gcEffects.Count > 0 && (gcEffects.Where(e => e.miscellaneousProperty == this.m_Reference).Count() > 0 || this.m_Reference == ""))
                        _ = this.m_Trigger.Execute(CollidedObj);


                    break;
            }
        }

        protected override void OnCollisionEnter3D(Trigger trigger, Collision collision) {
            base.OnCollisionEnter3D(trigger, collision);

            if (!this.IsActive) return;
            if (!this.Match(collision.gameObject)) return;

            this.OnCollision(collision.gameObject);
        }

        protected override void OnCollisionEnter2D(Trigger trigger, Collision2D collision) {
            base.OnCollisionEnter2D(trigger, collision);

            if (!this.IsActive) return;
            if (!this.Match(collision.gameObject)) return;

            this.OnCollision(collision.gameObject);
        }

        protected override void OnTriggerEnter3D(Trigger trigger, Collider collider) {
            base.OnTriggerEnter3D(trigger, collider);

            if (!this.IsActive) return;
            if (!this.Match(collider.gameObject)) return;

            this.OnCollision(collider.gameObject);
        }

        protected override void OnTriggerEnter2D(Trigger trigger, Collider2D collider) {
            base.OnTriggerEnter2D(trigger, collider);

            if (!this.IsActive) return;
            if (!this.Match(collider.gameObject)) return;

            this.OnCollision(collider.gameObject);
        }
    }
}
#endif