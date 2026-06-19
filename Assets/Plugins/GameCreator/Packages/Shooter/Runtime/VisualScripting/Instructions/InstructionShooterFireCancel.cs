using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Version(0, 0, 1)]
    
    [Title("Cancel Fire Trigger")]
    [Description("Cancels a pulled trigger on a shooter weapon")]
    
    [Category("Shooter/Shooting/Cancel Fire Trigger")]
    [Image(typeof(IconTriggerPull), ColorTheme.Type.Yellow, typeof(OverlayCross))]
    
    [Parameter("Character", "The Character reference with a Weapon equipped")]
    [Parameter("Weapon", "Optional field. The weapon to cancel if there are more than one")]
    
    [Keywords("Shooter", "Combat", "Shoot", "Cancel", "Stop", "Charge")]
    
    [Serializable]
    public class InstructionShooterFireCancel : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponNone.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override string Title => $"Cancel Fire {this.m_Weapon}";
 
        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
 
            ShooterWeapon weapon = this.m_Weapon.Get(args) as ShooterWeapon;
            ShooterStance stance = character.Combat.RequestStance<ShooterStance>();
 
            stance.CancelTrigger(weapon); 
            return DefaultResult;
        }
    }
}