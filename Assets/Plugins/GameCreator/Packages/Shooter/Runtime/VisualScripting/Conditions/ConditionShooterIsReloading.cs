using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("Is Reloading")]
    [Description("Checks if the Character is reloading a Shooter weapon")]

    [Category("Shooter/Reloading/Is Reloading")]
    
    [Parameter("Character", "The Character reference with a Weapon equipped")]

    [Keywords("Shooter", "Combat", "Reload", "Load")]
    [Image(typeof(IconReload), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionShooterIsReloading : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Character = GetGameObjectSelf.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary => $"{this.m_Character} is Reloading";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;
            
            return character.Combat
                .RequestStance<ShooterStance>()
                .Reloading
                .IsReloading;
        }
    }
}