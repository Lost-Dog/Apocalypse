#if ABC_GC_Integration
namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using GameCreator.Core;
    using ABCToolkit;


    [AddComponentMenu("")]
    public class IgniterABCAbilityCollision : Igniter
    {


        public enum AbilityReferenceType
        {
            AbilityName = 0,
            AbilityID = 1,
            Effect = 2,
            All = 3
        }

        public AbilityReferenceType abilityRefType = AbilityReferenceType.AbilityName;

        public int abilityID;
        public string abilityName;
        public string effectMiscValue;

#if UNITY_EDITOR
        public new static string NAME = "ABC Integration/On Collision With ABC Ability";
        public new static bool REQUIRES_COLLIDER = true;
#endif


        private void OnCollision(GameObject CollidedObj)
        {
            ABC_Projectile projABC = CollidedObj.GetComponentInChildren<ABC_Projectile>();

            if (projABC == null)
                return;

            switch (this.abilityRefType)
            {
                case AbilityReferenceType.All:
                    this.ExecuteTrigger(CollidedObj);
                    break;
                case AbilityReferenceType.AbilityID:
                    if (projABC.ability.abilityID == this.abilityID)
                        this.ExecuteTrigger(CollidedObj);
                    break;
                case AbilityReferenceType.AbilityName:
                    if (projABC.ability.name == this.abilityName)
                        this.ExecuteTrigger(CollidedObj);
                    break;
                case AbilityReferenceType.Effect:

                    List<Effect> gcEffects = projABC.ability.effects.Where(e => e.effectName == "GCAbilityCollisionTrigger").ToList();

                    //If a GC ability collision trigger effect is found and it matches the misc property or we are not matching to an effect misc value then execute trigger
                    if (gcEffects.Count > 0 && (gcEffects.Where(e => e.miscellaneousProperty == this.effectMiscValue).Count() > 0 || this.effectMiscValue == ""))
                        this.ExecuteTrigger(CollidedObj);


                    break;
            }
        }


        private void OnCollisionEnter(Collision c)
        {

            this.OnCollision(c.gameObject);

        }


        void OnTriggerEnter(Collider col)
        {

            this.OnCollision(col.gameObject);

        }


    }
}
#endif