#if ABC_GC_2_Integration

using System;
using System.Linq;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using UnityEngine;
using ABCToolkit;

namespace GameCreator.Runtime.VisualScripting
{

    public enum ABCCondition {
        HasABCWeaponEquipped = 0,
        HasABCAbilitySetup = 1, 
        IsABCAbilityEnabled = 2, 
        IsABCWeaponEnabled = 3, 
        HasABCTag = 4
    }


    [Title("ABC Conditions")]
    [Description("Returns true if an ABC condition has been met, else false. For example if a weapon is enabled or the entity has an ability setup")]

    [Category("ABC/ABC Conditions")]

    [Parameter("Target", "The targeted game object to check the ABC condition on")]
    [Parameter("Condition", "The targeted game object to check the ABC condition on")]
    [Parameter("Reference", "The name or ID of the Ability/Weapon/ABC Tag to check condition for")]

    [Keywords("ABC", "Ability", "Integration", "Condition")]
    
    [Image(typeof(IconCircleSolid), ColorTheme.Type.Green, typeof(IconDice))]

    [Serializable]
    public class ConditionABC: Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private ABCCondition m_Condition = ABCCondition.HasABCWeaponEquipped;
        [SerializeField] private string m_Reference;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{this.m_GameObject} {this.m_Condition.ToString()}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);

            GameObject target = this.m_GameObject.Get(args);

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(target);

            if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return false;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

            //Switch case the condition
            switch (this.m_Condition) {
                case ABCCondition.HasABCWeaponEquipped:

                    if (int.TryParse(this.m_Reference, out int a)) {

                        if (entity.currentEquippedWeapon != null && entity.currentEquippedWeapon.weaponID == int.Parse(this.m_Reference))
                            return true;
                        else
                            return false;

                    } else {

                        if (entity.currentEquippedWeapon != null && entity.currentEquippedWeapon.weaponName == this.m_Reference)
                            return true;
                        else
                            return false;
                    }

                case ABCCondition.HasABCAbilitySetup:

                    if (int.TryParse(this.m_Reference, out int b)) {

                        if (entity.CurrentAbilities.Where(a => a.abilityID == int.Parse(this.m_Reference)).Count() > 0)
                            return true;
                        else
                            return false;

                    } else {

                        if (entity.CurrentAbilities.Where(a => a.name == this.m_Reference).Count() > 0)
                            return true;
                        else
                            return false;
                    }
                case ABCCondition.IsABCAbilityEnabled:

                    if (int.TryParse(this.m_Reference, out int c)) {

                        return entity.IsAbilityEnabled(int.Parse(this.m_Reference));

                    } else {

                        return entity.IsAbilityEnabled(this.m_Reference);

                    }

                case ABCCondition.IsABCWeaponEnabled:

                    if (int.TryParse(this.m_Reference, out int d)) {

                        return entity.IsWeaponEnabled(int.Parse(this.m_Reference));

                    } else {

                        return entity.IsWeaponEnabled(this.m_Reference);

                    }

                case ABCCondition.HasABCTag:
                        
                        return entity.HasABCTag(this.m_Reference);
                   
            }


            return false;

        }
    }
}
#endif