using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("On Weapon Jam")]
    [Category("Shooter/On Weapon Jam")]
    [Description("Executed when the Character's weapon jams")]

    [Image(typeof(IconJam), ColorTheme.Type.Red)]
    
    [Keywords("Jam", "Malfunction")]

    [Serializable]
    public class EventShooterWeaponJam : TEventCharacter
    {
        [NonSerialized] private Character m_CachedCharacter;
        
        // METHODS: -------------------------------------------------------------------------------
        
        protected override void WhenEnabled(Trigger trigger, Character character)
        {
            this.m_CachedCharacter = character;
            character.Combat.RequestStance<ShooterStance>().Jamming.EventJam += this.OnJam;
        }

        protected override void WhenDisabled(Trigger trigger, Character character)
        {
            this.m_CachedCharacter = character;
            character.Combat.RequestStance<ShooterStance>().Jamming.EventJam -= this.OnJam;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnJam(IWeapon weapon)
        {
            if (weapon is not ShooterWeapon) return;
            
            GameObject target = this.m_CachedCharacter.gameObject;
            _ = this.m_Trigger.Execute(target);
        }
    }
}