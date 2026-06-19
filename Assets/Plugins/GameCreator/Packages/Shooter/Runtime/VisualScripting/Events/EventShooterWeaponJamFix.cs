using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("On Weapon Jam Fixed")]
    [Category("Shooter/On Weapon Jam Fixed")]
    [Description("Executed when the Character's weapon is fixed from a jam")]

    [Image(typeof(IconJam), ColorTheme.Type.Green)]
    
    [Keywords("Jam", "Malfunction", "Repair")]

    [Serializable]
    public class EventShooterWeaponJamFix : TEventCharacter
    {
        [NonSerialized] private Character m_CachedCharacter;
        
        // METHODS: -------------------------------------------------------------------------------
        
        protected override void WhenEnabled(Trigger trigger, Character character)
        {
            this.m_CachedCharacter = character;
            character.Combat.RequestStance<ShooterStance>().Jamming.EventFix += this.OnFix;
        }

        protected override void WhenDisabled(Trigger trigger, Character character)
        {
            this.m_CachedCharacter = character;
            character.Combat.RequestStance<ShooterStance>().Jamming.EventFix -= this.OnFix;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnFix(IWeapon weapon)
        {
            if (weapon is not ShooterWeapon) return;
            
            GameObject target = this.m_CachedCharacter.gameObject;
            _ = this.m_Trigger.Execute(target);
        }
    }
}