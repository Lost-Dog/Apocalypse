using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("Pouch")]
    [Category("Shooter/Ammo/Pouch")]

    [Image(typeof(IconBullet), ColorTheme.Type.Green)]
    [Description("The total amount of ammo, except that in the Magazine, in the Character weapon")]
    
    [Serializable]
    public class GetDecimalInPouch : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponShooterCharacter.Create();
        
        public override double Get(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            IWeapon weapon = this.m_Weapon.Get(args);

            if (character == null) return 0;
            if (weapon == null) return 0;

            return character.Combat.RequestMunition(weapon) is ShooterMunition munition
                ? munition.Total - munition.InMagazine
                : 0;
        }

        public static PropertyGetDecimal Create => new PropertyGetDecimal(
            new GetDecimalInPouch()
        );

        public override string String => $"{this.m_Character}[{m_Weapon}] Pouch";
    }
}