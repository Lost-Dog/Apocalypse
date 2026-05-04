using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("Last Object Hit")]
    [Category("Shooter/Last Object Hit")]
    
    [Image(typeof(IconPistol), ColorTheme.Type.Yellow, typeof(OverlayFlame))]
    [Description("The last game object hit by the last Shooter Weapon that took a shot")]

    [Serializable]
    public class GetGameObjectShooterLastGameObjectHit : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args) => ShotData.LastHitObject;
        public override GameObject Get(GameObject gameObject) => ShotData.LastHitObject;

        public override string String => "Last Object Hit";
    }
}